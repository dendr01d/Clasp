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
                    string stepName = mx.GoingTo; //capture before the machine executes
                    mx.GoTo.Invoke(mx);
                    PrintStep(cout, pauseEachStep, stepName, mx);
                }
                catch (Exception ex)
                {
                    PrintError(cout, mx.GoingTo, mx, ex);
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
                        "define" => Eval_Define,
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

            mx.Assign_GoTo(nextStep);
        }

        #region Evaluate Terminal Value

        private static void Eval_Self(Machine mx)
        {
            mx.Assign_Val(mx.Exp);
            mx.GoTo_Continue();
        }

        private static void Eval_Variable(Machine mx)
        {
            mx.Assign_Val(mx.Env.LookUp(mx.Exp.Expect<Symbol>()));
            mx.GoTo_Continue();
        }

        private static void Eval_Quoted(Machine mx)
        {
            mx.Assign_Val(mx.Exp.Cadr);
            mx.GoTo_Continue();
        }

        private static void Eval_Lambda(Machine mx)
        {
            mx.Assign_Val(new CompoundProcedure(
                mx.Exp.Cadr.Expect<Pair>(),
                mx.Env,
                mx.Exp.Cddr));
            mx.GoTo_Continue();
        }

        #endregion

        #region Procedure Evaluation

        private static void Eval_Application(Machine mx)
        {
            mx.Save_Continue();
            mx.EnterNewScope();

            mx.Assign_Unev(mx.Exp.Cdr);
            mx.Save_Unev();

            mx.Assign_Exp(mx.Exp.Car);

            mx.Assign_GoTo(Eval_Dispatch);
            mx.Assign_Continue(Eval_Apply_Did_Op);
        }

        private static void Eval_Apply_Did_Op(Machine mx)
        {
            mx.Assign_Proc(mx.Val.Expect<Procedure>());

            mx.Restore_Unev();
            mx.LeaveScope();

            mx.Assign_Argl(Expression.Nil); //empty list

            if (mx.Unev.IsNil)
            {
                //no args, proceed to proc application
                mx.Assign_GoTo(Apply_Dispatch);
            }
            else
            {
                mx.Save_Proc();
                mx.Assign_GoTo(Eval_Apply_Operand_Loop);
            }
        }

        private static void Eval_Apply_Operand_Loop(Machine mx)
        {
            mx.Save_Argl();
            mx.Assign_Exp(mx.Unev.Car);

            if (mx.Unev.Cdr.IsNil)
            {
                //if last arg, skip unecessary processing steps
                mx.Assign_Continue(Eval_Apply_Accumulate_Last_Arg);
            }
            else
            {
                mx.EnterNewScope();
                mx.Save_Unev();

                mx.Assign_Continue(Eval_Apply_Accumulate_Arg);
            }

            //either way this arg needs evaluating
            mx.Assign_GoTo(Eval_Dispatch);
        }

        private static void Eval_Apply_Accumulate_Arg(Machine mx)
        {
            mx.Restore_Unev();
            mx.LeaveScope();
            mx.Restore_Argl();

            mx.Assign_Argl(Pair.Append(mx.Argl, mx.Val));
            mx.Assign_Unev(mx.Unev.Cdr);

            mx.Assign_GoTo(Eval_Apply_Operand_Loop);
        }

        private static void Eval_Apply_Accumulate_Last_Arg(Machine mx)
        {
            mx.Restore_Argl();
            mx.Assign_Argl(Pair.Append(mx.Argl, mx.Val));

            mx.Restore_Proc();

            mx.Assign_GoTo(Apply_Dispatch);
        }

        #endregion

        #region Primitive & Compound Procedure Application

        private static void Apply_Dispatch(Machine mx)
        {
            if (mx.Proc is PrimitiveProcedure)
            {
                mx.Assign_GoTo(Primitive_Apply);
            }
            else if (mx.Proc is CompoundProcedure)
            {
                mx.Assign_GoTo(Compound_Apply);
            }
            else
            {
                mx.Assign_GoTo(Err_Procedure);
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
            CompoundProcedure proc = mx.Proc.Expect<CompoundProcedure>();

            mx.ReplaceScope(proc.Closure.DefineMany(proc.Parameters, mx.Argl.Expect<Pair>()));
            mx.Assign_Unev(proc.Body);

            mx.Restore_Continue();
            mx.Assign_GoTo(Eval_Sequence);
        }

        #endregion

        #region Sequential Term Evaluation

        private static void Eval_Begin(Machine mx)
        {
            mx.Assign_Unev(mx.Exp.Cdr);
            mx.Save_Continue();
            mx.Assign_GoTo(Eval_Sequence);
        }

        private static void Eval_Sequence(Machine mx)
        {
            mx.Assign_Exp(mx.Unev.Car);

            if (mx.Unev.Cdr.IsNil)
            {
                mx.Restore_Continue();
                mx.Assign_GoTo(Eval_Dispatch);
            }
            else
            {
                mx.Save_Unev();
                mx.EnterNewScope();

                mx.Assign_Continue(Eval_Sequence_Continue);
                mx.Assign_GoTo(Eval_Dispatch);
            }
        }

        private static void Eval_Sequence_Continue(Machine mx)
        {
            mx.LeaveScope();
            mx.Restore_Unev();

            mx.Assign_Unev(mx.Unev.Cdr);
            mx.Assign_GoTo(Eval_Sequence);
        }

        #endregion

        #region If/Then/Else

        private static void Eval_If(Machine mx)
        {
            mx.Save_Exp();
            mx.EnterNewScope();
            mx.Save_Continue();

            mx.Assign_Continue(Eval_If_Decide);
            mx.Assign_Exp(mx.Exp.Cadr);
            mx.Assign_GoTo(Eval_Dispatch);
        }

        private static void Eval_If_Decide(Machine mx)
        {
            mx.Restore_Continue();
            mx.LeaveScope();
            mx.Restore_Exp();

            if (mx.Val.IsTrue)
            {
                mx.Assign_Exp(mx.Exp.Caddr);
            }
            else
            {
                mx.Assign_Exp(mx.Exp.Cadddr);
            }

            mx.Assign_GoTo(Eval_Dispatch);
        }

        #endregion


        #region Variable Assignment & Definition

        private static void Eval_Define(Machine mx)
        {
            if (!mx.Exp.Cadr.IsAtom)
            {
                //rewrite into a lambda
                mx.Assign_Exp(Pair.List(
                    mx.Exp.Car, //define
                    mx.Exp.Cadar, //name of function
                    Pair.List(
                        Symbol.Lambda,
                        mx.Exp.Cadr.Cadr,
                        mx.Exp.Caddr)));
            }

            mx.Assign_GoTo(Eval_Definition);
        }

        private static void Eval_Definition(Machine mx)
        {
            mx.Assign_Unev(mx.Exp.Cadr);
            mx.Save_Unev();

            mx.Assign_Exp(mx.Exp.Caddr);

            mx.EnterNewScope();

            mx.Save_Continue();
            mx.Assign_Continue(Eval_Definition_Do);
            mx.Assign_GoTo(Eval_Dispatch);
        }

        private static void Eval_Definition_Do(Machine mx)
        {
            mx.Restore_Continue();
            mx.LeaveScope();
            mx.Restore_Unev();

            mx.Env.Define(mx.Unev.Expect<Symbol>(), mx.Val);

            mx.Assign_Val(Symbol.Ok);
            mx.GoTo_Continue();
        }

        private static void Eval_Assignment(Machine mx)
        {
            mx.Assign_Unev(mx.Exp.Cadr);
            mx.Save_Unev();

            mx.Assign_Exp(mx.Exp.Caddr);

            mx.EnterNewScope();

            mx.Save_Continue();
            mx.Assign_Continue(Eval_Assignment_Do);
            mx.Assign_GoTo(Eval_Dispatch);
        }

        private static void Eval_Assignment_Do(Machine mx)
        {
            mx.Restore_Continue();
            mx.LeaveScope();
            mx.Restore_Unev();

            mx.Env.SetBang(mx.Unev.Expect<Symbol>(), mx.Val);

            mx.Assign_Val(Symbol.Ok);
            mx.GoTo_Continue();
        }

        #endregion

        #region Derived Forms

        private static void Eval_Cond(Machine mx)
        {
            // (cond (test1 result1 result2 ...) clause1 clause2 ...)
            // -->
            // (if test1 (begin result1 result2 ...) (cond clause1 clause2 ...))

            if (mx.Exp.Cdr.IsNil)
            {
                throw new Exception("Fell out of cond. oops!");
            }
            else if (mx.Exp.Cadar == Symbol.CondElse)
            {
                mx.Assign_Unev(mx.Exp.Cdr.Car.Cdr);
                mx.Assign_GoTo(Eval_Sequence);
            }
            else
            {
                mx.Assign_Exp(Pair.List(
                    Symbol.If,
                    mx.Exp.Cadar,
                    Pair.Cons(Symbol.Begin, mx.Exp.Cdr.Car.Cdr),
                    Pair.Cons(Symbol.Cond, mx.Exp.Cddr)));

                mx.Assign_GoTo(Eval_If);
            }
        }

        private static void Eval_Case(Machine mx)
        {
            //first we need to make sure that the key is evaluated

            // (case key ((ex1 ex2 ...) result1 result2 ...) clause1 clause2 ...)
            // -->
            // ((lambda (VAR) (case VAR ((ex1 ex2 ...) result1 result2 ...) clause1 clause2 ...) key)

            if (mx.Exp.Cddr.IsNil)
            {
                throw new Exception("Fell out of Case...");
            }
            else if (mx.Exp.Cdr.Cdr.Car.Car.Car == Symbol.CondElse)
            {
                mx.Assign_Unev(mx.Exp.Cdr.Cdr.Car.Cdr);
                mx.Assign_GoTo(Eval_Sequence);
            }
            else
            {
                Symbol newSym = new GenSym();

                mx.Assign_Exp(Pair.List(
                    Pair.List(
                        Symbol.Lambda,
                        Pair.List(newSym),
                        Pair.List(
                            Symbol.Case,
                            newSym,
                            mx.Exp.Cddr)),
                    mx.Exp.Cadr));

                mx.Assign_Continue(Eval_Case_Continue);
                mx.Assign_GoTo(Eval_Application);
            }
        }

        private static void Eval_Case_Continue(Machine mx)
        {
            // (case key ((ex1 ex2 ...) result1 result2 ...) clause1 clause2 ...)
            // -->
            // (if (eq? key (quote ex1)) (begin result1 result2...) (case key ((ex2 ...) result1 result2) clause1 clause2 ...)
            // -->
            // (case key clause1 clause2 ...)

            
        }

        private static void Eval_And(Machine mx)
        {
            // (and test1 test2 ...)
            // -->
            // (if test1 (and test2 ...) #f)

            if (mx.Exp.Cdr.IsNil)
            {
                mx.Assign_Exp(Boolean.True);
                mx.GoTo_Continue();
            }
            else
            {
                mx.Assign_Exp(Pair.List(
                    Symbol.If,
                    mx.Exp.Cadr,
                    Pair.Cons(Symbol.And, mx.Exp.Cddr),
                    mx.Exp.Cadr));
                mx.Assign_GoTo(Eval_If);
            }
        }

        private static void Eval_Or(Machine mx)
        {
            // (or test1 test2 ...)
            // -->
            // (if test1 #t (or test2 ...))

            if (mx.Exp.Cdr.IsNil)
            {
                mx.Assign_Exp(Boolean.False);
                mx.GoTo_Continue();
            }
            else
            {
                mx.Assign_Exp(Pair.List(
                    Symbol.If,
                    mx.Exp.Cadr,
                    mx.Exp.Cadr,
                    Pair.Cons(Symbol.Or, mx.Exp.Cddr)));
                mx.Assign_GoTo(Eval_If);
            }

        }

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
