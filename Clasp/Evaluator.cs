using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal static class Evaluator
    {
        #region Interfacing

        public static Expression Evaluate(Expression expr, Environment env, TextWriter? cout = null, bool pauseEachStep = false)
        {
            Machine mx = new Machine(expr, env, Eval_Dispatch);

            PrintStep(cout, pauseEachStep, "Start", mx);

            while(mx.GoTo is not null)
            {
                try
                {
                    string stepName = mx.StepName; //capture before the machine executes
                    mx.GoTo.Invoke(mx);
                    PrintStep(cout, pauseEachStep, stepName, mx);
                }
                catch (Exception ex)
                {
                    PrintError(cout, mx.StepName, mx, ex);
                    return Expression.Error;
                }
            }

            PrintStep(cout, pauseEachStep, "Final Result", null);

            return mx.Val ?? Expression.Error;
        }

        private static void PrintStep(TextWriter? tw, bool pause, string stepName, Machine? mx)
        {
            if (tw is not null)
            {
                tw.WriteLine(new string('_', 60));
                tw.WriteLine($"Step '{stepName}'");
                tw.WriteLine();
                mx?.Print(tw);

                if (mx is not null && pause) Console.ReadKey(true);
            }
        }

        private static void PrintError(TextWriter? tw, string stepName, Machine mx, Exception ex)
        {
            if (tw is not null)
            {
                tw.WriteLine(new string('_', 60));
                tw.WriteLine($"Error in Step '{stepName}'");
                tw.WriteLine();
                mx.Print(tw);
                tw.WriteLine();
                tw.WriteLine(ex.Message);
                tw.WriteLine(ex.StackTrace);
            }
        }

        #endregion

        public static void Eval_Dispatch(Machine mx)
        {
            Action<Machine> nextStep = Err_Unknown_Expression;

            if (mx.Exp is Error)
            {
                throw new Exception("Tried to dispatch on #error value");
            }
            else if (mx.Exp is Symbol)
            {
                nextStep = Eval_Variable;
            }
            else if (mx.Exp is not Pair and not Empty)
            {
                nextStep = Eval_Self;
            }
            else
            {
                Expression firstTerm = mx.Exp.Car;

                if (firstTerm is Symbol sym)
                {
                    nextStep = sym.Name switch
                    {
                        "quote" => Eval_Quoted,
                        "set!" => Eval_Assignment,
                        "define" => Eval_Definition_Kind,
                        "if" => Eval_If,
                        "lambda" => Eval_Lambda,
                        "begin" => Eval_Begin,
                        //"cond" => Eval_Cond,
                        //"let" => Eval_Let,
                        "and" => Eval_And,
                        "or" => Eval_Or,
                        _ => Eval_Application
                    };
                }
                else if (firstTerm is Pair)
                {
                    nextStep = Eval_Application;
                }
            }

            mx.GoTo.Assign(nextStep);
        }

        #region Evaluate Terminal Value

        private static void Eval_Self(Machine mx)
        {
            mx.Val.Assign(mx.Exp);
            mx.GoTo.Assign(mx.Continue);
        }

        private static void Eval_Variable(Machine mx)
        {
            mx.Val.Assign(mx.Env.LookUp(mx.Exp.Expect<Symbol>()));
            mx.GoTo.Assign(mx.Continue);
        }

        private static void Eval_Quoted(Machine mx)
        {
            mx.Val.Assign(mx.Exp.Cadr);
            mx.GoTo.Assign(mx.Continue);
        }

        private static void Eval_Lambda(Machine mx)
        {
            //mx.Unev.Assign(mx.Exp.Cdr.Expect<Pair>());
            //mx.Exp.Assign(mx.Exp.Caddr);

            mx.Val.Assign(new CompoundProcedure(
                mx.Exp.Cadr.Expect<Pair>(),
                mx.Env,
                mx.Exp.Cddr));

            mx.GoTo.Assign(mx.Continue);
        }

        #endregion

        #region Procedure Evaluation

        private static void Eval_Application(Machine mx)
        {
            mx.Continue.Save();
            mx.Env.Save();

            mx.Unev.Assign(mx.Exp.Cdr);
            mx.Unev.Save();

            mx.Exp.Assign(mx.Exp.Car);

            mx.GoTo.Assign(Eval_Dispatch);
            mx.Continue.Assign(Eval_Apply_Did_Op);
        }

        private static void Eval_Apply_Did_Op(Machine mx)
        {
            mx.Proc.Assign(mx.Val.Expect<Procedure>());

            mx.Unev.Restore();
            mx.Env.Restore();

            mx.ArgL.Assign(Expression.Nil); //empty list

            mx.Test(mx.Unev.IsNil);
            if (mx.Branch)
            {
                mx.GoTo.Assign(Apply_Dispatch);
            }
            else
            {
                mx.Proc.Save();
                mx.GoTo.Assign(Eval_Apply_Operand_Loop);
            }
        }

        private static void Eval_Apply_Operand_Loop(Machine mx)
        {
            mx.ArgL.Save();
            mx.Exp.Assign(mx.Unev.Car);

            mx.Test(mx.Unev.Cdr.IsNil);
            if (mx.Branch)
            {
                mx.GoTo.Assign(Eval_Apply_Last_Arg);
            }
            else
            {
                mx.Env.Save();
                mx.Unev.Save();

                mx.Continue.Assign(Eval_Apply_Accumulate_Arg);
                mx.GoTo.Assign(Eval_Dispatch);
            }
        }

        private static void Eval_Apply_Accumulate_Arg(Machine mx)
        {
            mx.Unev.Restore();
            mx.Env.Restore();
            mx.ArgL.Restore();

            mx.ArgL.Assign(Pair.Append(mx.ArgL, mx.Val));
            mx.Unev.Assign(mx.Unev.Cdr);

            mx.GoTo.Assign(Eval_Apply_Operand_Loop);
        }

        private static void Eval_Apply_Last_Arg(Machine mx)
        {
            mx.Continue.Assign(Eval_Apply_Accumulate_Last_Arg);
            mx.GoTo.Assign(Eval_Dispatch);
        }

        private static void Eval_Apply_Accumulate_Last_Arg(Machine mx)
        {
            mx.ArgL.Restore();
            mx.ArgL.Assign(Pair.Append(mx.ArgL, mx.Val));

            mx.Proc.Restore();

            mx.GoTo.Assign(Apply_Dispatch);
        }

        #endregion

        #region Primitive & Compound Procedure Application

        private static void Apply_Dispatch(Machine mx)
        {
            mx.Test(mx.Proc is PrimitiveProcedure);
            if (mx.Branch)
            {
                mx.GoTo.Assign(Primitive_Apply);
            }
            else
            {
                mx.Test(mx.Proc is CompoundProcedure);
                if (mx.Branch)
                {
                    mx.GoTo.Assign(Compound_Apply);
                }
                else
                {
                    mx.GoTo.Assign(Err_Procedure);
                }
            }
        }

        private static void Primitive_Apply(Machine mx)
        {
            mx.Val.Assign(mx.Proc.Expect<PrimitiveProcedure>().Apply(mx.ArgL.Expect<Pair>()));
            mx.Continue.Restore();
            mx.GoTo.Assign(mx.Continue);
        }

        private static void Compound_Apply(Machine mx)
        {
            //using unev here as temp storage for input into DefineMany
            mx.Unev.Assign(mx.Proc.Expect<CompoundProcedure>().Parameters);

            mx.Env.Assign(mx.Proc.Expect<CompoundProcedure>().Closure);
            mx.Env.Assign(mx.Env.DefineMany(mx.Unev.Expect<Pair>(), mx.ArgL.Expect<Pair>()));

            mx.Unev.Assign(mx.Proc.Expect<CompoundProcedure>().Body);

            mx.Continue.Restore();
            mx.GoTo.Assign(Eval_Sequence);
        }

        #endregion

        #region Evaluate Terms Sequentially

        private static void Eval_Begin(Machine mx)
        {
            mx.Unev.Assign(mx.Exp.Cdr);
            mx.Continue.Save();
            mx.GoTo.Assign(Eval_Sequence);
        }

        private static void Eval_Sequence(Machine mx)
        {
            mx.Exp.Assign(mx.Unev.Car);

            mx.Test(mx.Unev.Cdr.IsNil);
            if (mx.Branch)
            {
                mx.GoTo.Assign(Eval_Sequence_End);
            }
            else
            {
                mx.Unev.Save();
                mx.Env.Save();

                mx.Continue.Assign(Eval_Sequence_Continue);
                mx.GoTo.Assign(Eval_Dispatch);
            }
        }

        private static void Eval_Sequence_Continue(Machine mx)
        {
            mx.Env.Restore();
            mx.Unev.Restore();

            mx.Unev.Assign(mx.Unev.Cdr);
            mx.GoTo.Assign(Eval_Sequence);
        }

        private static void Eval_Sequence_End(Machine mx)
        {
            mx.Continue.Restore();
            mx.GoTo.Assign(Eval_Dispatch);
        }

        #endregion

        #region If/Then/Else

        private static void Eval_If(Machine mx)
        {
            mx.Exp.Save();
            mx.Env.Save();
            mx.Continue.Save();

            mx.Continue.Assign(Eval_If_Decide);
            mx.Exp.Assign(mx.Exp.Car);
            mx.GoTo.Assign(Eval_Dispatch);
        }

        private static void Eval_If_Decide(Machine mx)
        {
            mx.Continue.Restore();
            mx.Env.Restore();
            mx.Exp.Restore();

            mx.Test(mx.Val.IsTrue);
            if (mx.Branch)
            {
                mx.GoTo.Assign(Eval_If_Consequent);
            }
            else
            {
                mx.GoTo.Assign(Eval_If_Alternative);
            }
        }

        private static void Eval_If_Consequent(Machine mx)
        {
            mx.Exp.Assign(mx.Exp.Caddr);
            mx.GoTo.Assign(Eval_Dispatch);
        }

        private static void Eval_If_Alternative(Machine mx)
        {
            mx.Exp.Assign(mx.Exp.Cadddr);
            mx.GoTo.Assign(Eval_Dispatch);
        }

        #endregion


        #region Variable Assignment & Definition

        private static void Eval_Assignment(Machine mx)
        {
            mx.Unev.Assign(mx.Exp.Cadr);
            mx.Unev.Save();

            mx.Exp.Assign(mx.Exp.Caddr);

            mx.Env.Save();

            mx.Continue.Save();
            mx.Continue.Assign(Eval_Assignment_Do);
            mx.GoTo.Assign(Eval_Dispatch);
        }

        private static void Eval_Assignment_Do(Machine mx)
        {
            mx.Continue.Restore();
            mx.Env.Restore();
            mx.Unev.Restore();

            mx.Env.SetBang(mx.Unev.Expect<Symbol>(), mx.Val);

            mx.Val.Assign(Symbol.Ok);
            mx.GoTo.Assign(mx.Continue);
        }

        private static void Eval_Definition_Kind(Machine mx)
        {
            mx.GoTo.Assign(Eval_Definition);

            mx.Test(!mx.Exp.Cadr.IsAtom);
            if (mx.Branch)
            {
                //re-write into a lambda definition
                mx.Exp.Assign(Pair.List(
                    mx.Exp.Car,
                    mx.Exp.Cadar,
                    Pair.List(
                        Symbol.Lambda,
                        mx.Exp.Cadr.Cdr,
                        mx.Exp.Caddr)));
            }
        }

        private static void Eval_Definition(Machine mx)
        {
            mx.Unev.Assign(mx.Exp.Cadr);
            mx.Unev.Save();

            mx.Exp.Assign(mx.Exp.Caddr);

            mx.Env.Save();

            mx.Continue.Save();
            mx.Continue.Assign(Eval_Definition_Do);
            mx.GoTo.Assign(Eval_Dispatch);
        }

        private static void Eval_Definition_Do(Machine mx)
        {
            mx.Continue.Restore();
            mx.Env.Restore();
            mx.Unev.Restore();

            mx.Env.Define(mx.Unev.Expect<Symbol>(), mx.Val);

            mx.Val.Assign(Symbol.Ok);
            mx.GoTo.Assign(mx.Continue);
        }

        #endregion


        #region Logical Connectives

        public static void Eval_And(Machine mx)
        {
            mx.Continue.Save();
            mx.Unev.Assign(mx.Exp.Cdr);
            mx.ArgL.Assign(Boolean.False); //"wrong" value
            mx.GoTo.Assign(Eval_Connective_Loop);
        }

        public static void Eval_Or(Machine mx)
        {
            mx.Continue.Save();
            mx.Unev.Assign(mx.Exp.Cdr);
            mx.ArgL.Assign(Boolean.True);
            mx.GoTo.Assign(Eval_Connective_Loop);
        }

        public static void Eval_Connective_Loop(Machine mx)
        {
            //if we've run out of args, return the "right" value
            mx.Test(mx.Unev.IsNil);
            if (mx.Branch)
            {
                mx.Val.Assign(Boolean.Not(mx.ArgL));
                mx.GoTo.Assign(Eval_Connective_End);
            }
            else
            {
                mx.Exp.Assign(mx.Unev.Car);
                mx.Unev.Assign(mx.Unev.Cdr);

                mx.ArgL.Save();
                mx.Env.Save();
                mx.Unev.Save();

                mx.GoTo.Assign(Eval_Dispatch);
                mx.Continue.Assign(Eval_Connective_Continue);
            }
        }

        private static void Eval_Connective_Continue(Machine mx)
        {
            mx.Unev.Restore();
            mx.Env.Restore();
            mx.ArgL.Restore();

            //if the most recent evaluation returned the "wrong" value
            //then short-circuit and return it
            mx.Test(mx.Val == mx.ArgL);
            if (mx.Branch)
            {
                mx.Val.Assign(mx.ArgL);
                mx.GoTo.Assign(Eval_Connective_End);
            }
            else
            {
                mx.GoTo.Assign(Eval_Connective_Loop);
            }
        }

        private static void Eval_Connective_End(Machine mx)
        {
            mx.Continue.Restore();
            mx.GoTo.Assign(mx.Continue);
        }

        #endregion


        #region Derived Forms

        private static void Eval_Cond(Machine mx)
        {
            //(cond (t1 r1) (t2 r2) (t3 r3) ...)
            // -->
            //(if t1 r1 (if t2 r2 (if t3 r3 ... Error) ...)))

            //pop off the "cond" operator
            mx.Exp.Assign(mx.Exp.Cdr);

            //if no more clauses, throw an error
            mx.Test(mx.Exp.IsNil);
            if (mx.Branch)
            {
                mx.GoTo.Assign(Err_Unknown_Expression);
                return;
            }

            //if "else" clause, cut straight to evaluating the consequent
            mx.Test(mx.Exp.Caar == Symbol.CondElse);
            if (mx.Branch)
            {
                mx.Exp.Assign(mx.Exp.Cadar);
                mx.GoTo.Assign(Eval_Sequence);
                return;
            }

            //otherwise rewrite first clause into if/else form
            //with alternative primed to route back to this subroutine
            mx.Exp.Assign(Pair.List(
                Symbol.If,
                mx.Exp.Caar,
                Pair.Cons(Symbol.Begin, mx.Exp.Cdar),
                Pair.Cons(Symbol.Cond, mx.Exp.Cddr)));
            mx.GoTo.Assign(Eval_If);
        }

        //private static void Eval_Case(Machine mx)
        //{
        //    //(case expr (t1 r1) (t2 r2) ...)
        //    // -->
        //    //(if (eq? expr t1) r1 (case expr (t2 r2) ...))

        //    //if no more cases, throw an error
        //    mx.Test(mx.Exp.Cddr.IsNil);
        //    if (mx.Branch)
        //    {
        //        mx.GoTo.Assign(Err_Unknown_Expression);
        //        return;
        //    }

        //    //if "else" clause, cut straight to evaluating the consequent
        //    mx.Test(mx.Exp.Caddr == Symbol.CondElse);
        //    if (mx.Branch)
        //    {
        //        mx.Exp.Assign(mx.Exp.Cddr.Cadr);
        //        mx.GoTo.Assign(Eval_Sequence);
        //        return;
        //    }

        //    //otherwise, like with cond, rewrite the first clause
        //    //then emplace a recurrence in the alternative
        //    mx.Exp.Assign(Pair.List(
        //        Symbol.If,
        //        Pair.List(Symbol.Eq, mx.Exp.Caddr, mx.Exp.Cddr.Car),
        //        Pair.Cons(Symbol.Begin, mx.Exp.Cddr.Cadr),
        //        Pair.ListStar(Symbol.Case, mx.Exp.Cadr, mx.Exp.Cdddr)));
        //}

        //Eval Let

        //separate the binding definitions
        //and rewrite into a lambda application

        //(let ((k1 d1) (k2 d2) (k3 d3) ...) body1 body2 body3 ...)
        // -->
        //((lambda (k1 k2 k3 ...) body1 body2 body3 ...) d1 d2 d3 ...)

        //private static void Eval_Let(Machine mx)
        //{
        //    //pop off the "let" operator and save the result
        //    mx.Exp.Assign(mx.Exp.Cdr);
        //    mx.Exp.Save();

        //    //consider only the list of binding definitions
        //    mx.Exp.Assign(mx.Exp.Car);

        //    //clear the unev and argl registers
        //    mx.Unev.Assign(Expression.Nil);
        //    mx.ArgL.Assign(Expression.Nil);

        //    //begin looping through the unbinding step
        //    mx.GoTo.Assign(Eval_Let_Continue);
        //}

        //private static void Eval_Let_Continue(Machine mx)
        //{
        //    //if there are no bindings left, assemble the final lambda
        //    mx.Test(mx.Exp.IsNil);
        //    if (mx.Branch)
        //    {
        //        mx.GoTo.Assign(Eval_Let_End);
        //    }
        //    else
        //    {
        //        //otherwise take the first binding and append the key to unev
        //        //and the definition to argl
        //        mx.Unev.Assign(Pair.Append(mx.Unev, mx.Exp.Caar));
        //        mx.ArgL.Assign(Pair.Append(mx.ArgL, mx.Exp.Cadar));

        //        //now that we've consumed the binding, pop it off
        //        mx.Exp.Assign(mx.Exp.Cdr);

        //        //GoTo is already routed to recur in this subroutine
        //    }

        //}

        //private static void Eval_Let_End(Machine mx)
        //{
        //    //restore the original expression, sans "let"
        //    mx.Exp.Restore();

        //    //build the lambda
        //    mx.Exp.Assign(Pair.ListStar(Symbol.Lambda, mx.Unev, mx.Exp.Cdr));
        //    //and prepare the application
        //    mx.Exp.Assign(Pair.Cons(mx.Exp, mx.ArgL));

        //    mx.GoTo.Assign(Eval_Application);
        //}

        #endregion

        #region Error States

        private static void Err_Unknown_Expression(Machine mx)
        {
            mx.Val.Assign(Expression.Error);
            mx.GoTo.Clear();
        }

        private static void Err_Procedure(Machine mx)
        {
            mx.Val.Assign(Expression.Error);
            mx.GoTo.Clear();
        }

        #endregion
    }
}
