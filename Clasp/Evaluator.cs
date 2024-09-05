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

            PrintStep(cout, pauseEachStep, "Start", 0, mx);
            int step = 1;

            while(mx.GoTo is not null)
            {
                string stepName = mx.GoingTo; //capture before the machine executes
                mx.GoTo.Invoke(mx);
                PrintStep(cout, pauseEachStep, stepName, step++, mx);
            }

            PrintStep(cout, pauseEachStep, "Final Result", step, null);

            return mx.Val ?? Expression.Error;
        }

        private static void PrintStep(TextWriter? tw, bool pause, string stepName, int stepNum, Machine? mx)
        {
            if (tw is not null)
            {
                tw.WriteLine(new string('_', 60));
                tw.WriteLine($"Step #{stepNum} - '{stepName}'");
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

        public static readonly Dictionary<string, Action<Machine>> SpecialFormRouting = new()
        {
            { "eval", Eval_Eval },
            { "apply", Eval_Raw_Apply },

            { "quote", Eval_Quoted },
            { "quasiquote", Eval_Quasiquoted },
            { "unquote", Err_Illegal_Operation }, //this can't be right, surely?
            { "unquote-splicing", Err_Illegal_Operation }, //or this?

            { "set!", Eval_Assignment },
            { "define", Eval_Define },
            { "defmacro", Eval_DefMacro },

            { "if", Eval_If },
            { "lambda", Eval_Lambda },
            { "begin", Eval_Begin }
        };

        public static void Eval_Dispatch(Machine mx)
        {
            Action<Machine> nextStep = mx.Exp switch
            {
                Symbol => Eval_Variable,
                Atom => Eval_Self,
                _ => Eval_Application
            };

            mx.Assign_GoTo(nextStep);
        }

        private static void Eval_Eval(Machine mx)
        {
            mx.Assign_Exp(Pair.Cons(Symbol.Begin, mx.Exp.Cdr));
            mx.Assign_GoTo(Eval_Dispatch);
        }

        private static void Eval_Raw_Apply(Machine mx)
        {
            mx.Assign_Exp(Pair.Cons(mx.Exp.Cadr, mx.Exp.Cddr));
            mx.Assign_GoTo(Eval_Dispatch);
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

        #region Advanced Quotation

        private static void Eval_Quasiquoted(Machine mx)
        {
            mx.Assign_Exp(mx.Exp.Cadr);
            mx.Assign_GoTo(Expand_Dispatch);
        }

        private static void Expand_Dispatch(Machine mx)
        {
            if (mx.Exp.IsAtom)
            {
                mx.Assign_Val(mx.Exp);
                mx.GoTo_Continue();
            }
            else if (mx.Exp.Car == Symbol.Quasiquote)
            {
                mx.Assign_Exp(mx.Exp.Cadr);
                mx.Save_Continue();
                mx.Assign_Continue(Expand_Dispatch);
                mx.Save_Continue();
                mx.Assign_Continue(Expand_Nested);
                mx.Assign_GoTo(Expand_Dispatch);
            }
            else if (mx.Exp.Car == Symbol.Unquote)
            {
                mx.Assign_Exp(mx.Exp.Cadr);
                mx.Assign_GoTo(Eval_Dispatch);
            }
            else if (mx.Exp.Car == Symbol.UnquoteSplicing)
            {
                mx.Assign_GoTo(Err_Illegal_Operation);
            }
            else
            {
                mx.Save_Exp();
                mx.Assign_Exp(mx.Exp.Car);
                mx.Save_Continue();
                mx.Assign_Continue(Expand_Did_Car);
                mx.Assign_GoTo(Expand_List_Dispatch);
            }
        }

        private static void Expand_Nested(Machine mx)
        {
            mx.Restore_Continue();
            mx.Assign_Exp(mx.Val);
            mx.Assign_GoTo(Expand_Dispatch);
        }

        private static void Expand_Did_Car(Machine mx)
        {
            mx.Restore_Exp();
            mx.Assign_Exp(mx.Exp.Cdr);

            mx.Assign_Argl(mx.Val);

            mx.Save_Argl();
            mx.Assign_Continue(Expand_Did_Cdr);
            mx.Assign_GoTo(Expand_Dispatch);
        }

        private static void Expand_Did_Cdr(Machine mx)
        {
            mx.Restore_Argl();
            mx.Assign_Val(Pair.Append(mx.Argl, mx.Val));
            mx.Restore_Continue();
            mx.GoTo_Continue();
        }

        private static void Expand_List_Dispatch(Machine mx)
        {
            if (mx.Exp.IsAtom)
            {
                mx.Assign_Val(Pair.MakeList(mx.Exp));
                mx.GoTo_Continue();
            }
            else if (mx.Exp.Car == Symbol.Quasiquote)
            {
                mx.Assign_Exp(mx.Exp.Cadr);
                mx.Save_Continue();
                mx.Assign_Continue(Expand_Nested);
                mx.Assign_GoTo(Expand_Dispatch);
            }
            else if (mx.Exp.Car == Symbol.Unquote)
            {
                mx.Assign_Exp(mx.Exp.Cadr);
                mx.Save_Continue();
                mx.Assign_Continue(Expand_List_Result);
                mx.Assign_GoTo(Eval_Dispatch);
            }
            else if (mx.Exp.Car == Symbol.UnquoteSplicing)
            {
                mx.Assign_Unev(mx.Exp.Cdr.Expect<Pair>());
                mx.Assign_Argl(Expression.Nil);
                mx.Assign_GoTo(Expand_List_Spliced);
            }
            else
            {
                mx.Save_Exp();
                mx.Assign_Exp(mx.Exp.Car);
                mx.Save_Continue();
                mx.Assign_Continue(Expand_List_Result);
                mx.Save_Continue();
                mx.Assign_Continue(Expand_Did_Car);
                mx.Assign_GoTo(Expand_List_Dispatch);
            }
        }

        private static void Expand_List_Result(Machine mx)
        {
            mx.Assign_Val(Pair.MakeList(mx.Val));
            mx.Restore_Continue();
            mx.GoTo_Continue();
        }

        private static void Expand_List_Spliced(Machine mx)
        {
            if (mx.Unev.IsNil)
            {
                mx.Assign_Val(mx.Argl);
                mx.GoTo_Continue();
            }
            else
            {
                mx.Assign_Exp(mx.Unev.Car);
                mx.NextUnev();

                mx.Save_Unev();
                mx.Save_Argl();

                mx.Save_Continue();
                mx.Assign_Continue(Expand_List_Spliced_Continue);
                mx.Assign_GoTo(Eval_Dispatch);
            }
        }

        private static void Expand_List_Spliced_Continue(Machine mx)
        {
            mx.Restore_Continue();
            mx.Restore_Argl();
            mx.Restore_Unev();

            mx.Assign_Argl(Pair.Append(mx.Argl, mx.Val)); //NOT AppendLast
            mx.Assign_GoTo(Expand_List_Spliced);
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
                //if there are no args to evaluate, always skip ahead
                mx.Assign_GoTo(Apply_Dispatch);
            }
            else if (mx.Proc.ApplicativeOrder)
            {
                //if the args DO need to be evaluated, loop through them and do that
                mx.Save_Proc();
                mx.Assign_GoTo(Eval_Apply_Operand_Loop);
            }
            else
            {
                mx.Restore_Continue();

                //otherwise rebuild the original expression and skip ahead
                mx.Assign_Exp(Pair.Cons(mx.Exp, mx.Unev));
                mx.Assign_GoTo(Apply_Dispatch);
            }
        }

        private static void Eval_Apply_Operand_Loop(Machine mx)
        {
            mx.Save_Argl(); //memorize any eval'd args thus far

            //grab the next uneval'd arg
            //which may be the tail end of a dotted list
            if (mx.Unev.IsAtom)
            {
                mx.Assign_Exp(mx.Unev);
            }
            else
            {
                mx.Assign_Exp(mx.Unev.Car);
            }

            //check if there are more args
            if (mx.Unev.IsAtom || mx.Unev.Cdr.IsNil)
            {
                //tail call time
                mx.Assign_Continue(Eval_Apply_Accumulate_Last_Arg);
                mx.Assign_GoTo(Eval_Dispatch);
            }
            else
            {
                mx.EnterNewScope();
                mx.Save_Unev();

                mx.Assign_Continue(Eval_Apply_Accumulate_Arg);
                mx.Assign_GoTo(Eval_Dispatch);
            }
        }

        private static void Eval_Apply_Accumulate_Arg(Machine mx)
        {
            mx.Restore_Unev();
            mx.LeaveScope();
            mx.Restore_Argl();

            mx.AppendArgl(mx.Val);
            mx.Assign_Unev(mx.Unev.Cdr);

            mx.Assign_GoTo(Eval_Apply_Operand_Loop);
        }

        private static void Eval_Apply_Accumulate_Last_Arg(Machine mx)
        {
            mx.Restore_Argl();
            mx.AppendArgl(mx.Val);

            mx.Restore_Proc();

            mx.Assign_GoTo(Apply_Dispatch);
        }

        #endregion

        #region Procedure Application

        private static void Apply_Dispatch(Machine mx)
        {
            Action<Machine> gotoPtr = mx.Proc switch
            {
                SpecialForm sf => sf.InstructionPtr,
                PrimitiveProcedure => Primitive_Apply,
                CompoundProcedure => Compound_Apply,
                Macro => Macro_Apply,
                _ => Err_Procedure
            };

            mx.Assign_GoTo(gotoPtr);
        }

        private static void Primitive_Apply(Machine mx)
        {
            PrimitiveProcedure proc = mx.Proc.Expect<PrimitiveProcedure>();

            mx.Assign_Val(proc.Apply(mx.Argl.Expect<Pair>()));
            mx.Restore_Continue();
            mx.GoTo_Continue();
        }

        private static void Compound_Apply(Machine mx)
        {
            CompoundProcedure proc = mx.Proc.Expect<CompoundProcedure>();

            mx.ReplaceScope(proc.Closure);
            mx.Assign_Unev(proc.Body);

            if (mx.Argl.IsNil)
            {
                mx.Assign_GoTo(Eval_Sequence);
            }
            else
            {
                mx.Save_Unev();
                mx.Assign_Unev(proc.Parameters);
                mx.Assign_GoTo(Bind_Args);
            }
        }

        private static void Bind_Args(Machine mx)
        {
            if (mx.Unev.IsAtom)
            {
                mx.Env.BindNew(mx.Unev.Expect<Symbol>(), mx.Argl);
                mx.Assign_GoTo(Args_Bound);
            }
            else
            {
                mx.Env.BindNew(mx.Unev.Car.Expect<Symbol>(), mx.Argl.Car);
                mx.NextUnev();
                mx.NextArgl();

                if (mx.Unev.IsNil)
                {
                    mx.Assign_GoTo(Args_Bound);
                }
            }
        }

        private static void Args_Bound(Machine mx)
        {
            mx.Restore_Unev();
            mx.Assign_GoTo(Eval_Sequence);
        }

        private static void Macro_Apply(Machine mx)
        {
            Macro proc = mx.Proc.Expect<Macro>();

            mx.Assign_Argl(proc.LiteralSymbols);
            mx.Assign_Unev(proc.Transformers);

            mx.Assign_GoTo(Apply_Macro_Clauses);
        }

        private static void Apply_Macro_Clauses(Machine mx)
        {
            if (mx.Unev.Car is SyntaxRule sr)
            {
                if (sr.TryTransform(mx.Exp, mx.Argl,
                    mx.Proc.Expect<Macro>().Closure, mx.Env,
                    out Expression result))
                {
                    mx.Assign_Exp(result);
                    mx.Restore_Continue();
                    mx.Assign_GoTo(Eval_Dispatch);
                }
                else
                {
                    mx.NextUnev();
                }
            }
            else
            {
                mx.Assign_GoTo(Err_Unknown_Expression);
            }
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
                //mx.Restore_Continue();
                mx.Assign_GoTo(Eval_Sequence_End);
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

            mx.NextUnev();
            mx.Assign_GoTo(Eval_Sequence);
        }

        private static void Eval_Sequence_End(Machine mx)
        {
            mx.Restore_Continue();
            mx.Assign_GoTo(Eval_Dispatch);
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
                mx.Assign_Exp(Pair.MakeList(
                    mx.Exp.Car, //define
                    mx.Exp.Cadr.Car, //name of function
                    Pair.MakeImproperList(
                        Symbol.Lambda,
                        mx.Exp.Cadr.Cdr,
                        mx.Exp.Cddr)));
            }

            mx.Assign_GoTo(Eval_Definition);
        }

        private static void Eval_Definition(Machine mx)
        {
            mx.Assign_Unev(Pair.MakeList(mx.Exp.Cadr));
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

            mx.Env.BindNew(mx.Unev.Car.Expect<Symbol>(), mx.Val);

            mx.Assign_Val(Symbol.Ok);
            mx.GoTo_Continue();
        }

        //---

        private static void Eval_DefMacro(Machine mx)
        {
            //Symbol name = mx.Exp.Cadr.Expect<Symbol>();
            //mx.Env.BindNew(
            //    name,
            //    new Macro(
            //        name.Name,
            //        mx.Exp.Cddr.Expect<Pair>(),
            //        mx.Env));

            //mx.Assign_Val(Symbol.Ok);
            //mx.GoTo_Continue();

            mx.Assign_Exp(mx.Exp.Cdr);
            mx.Save_Exp();
            mx.Assign_Argl(Expression.Nil);
            mx.Assign_Unev(mx.Exp.Cddr.Expect<Pair>());

            mx.Assign_GoTo(Eval_Macro_Clauses);
        }

        private static void Eval_Macro_Clauses(Machine mx)
        {
            mx.Assign_Exp(mx.Unev.Car);
            mx.AppendArgl(
                new SyntaxRule(
                    mx.Exp.Car,
                    mx.Exp.Cadr));
            mx.NextUnev();

            if (mx.Unev.IsNil)
            {
                mx.Assign_GoTo(Eval_Macro_Binding);
            }
        }

        private static void Eval_Macro_Binding(Machine mx)
        {
            mx.Restore_Exp();

            Symbol sym = mx.Exp.Car.Expect<Symbol>();
            Macro m = new Macro(
                sym.Name,
                mx.Exp.Cadr,
                mx.Argl.Expect<Pair>(),
                mx.Env);

            mx.Env.BindNew(sym, m);

            mx.Assign_Val(Symbol.Ok);
            mx.GoTo_Continue();
        }

        //---

        private static void Eval_Assignment(Machine mx)
        {
            mx.Assign_Unev(Pair.MakeList(mx.Exp.Cadr));
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

            mx.Env.RebindExisting(mx.Unev.Car.Expect<Symbol>(), mx.Val);

            mx.Assign_Val(Symbol.Ok);
            mx.GoTo_Continue();
        }

        #endregion

        #region Pattern-Matching

        //private static void Eval_Match(Machine mx)
        //{
        //    mx.Save_Continue();

        //    mx.Assign_Argl(mx.Exp.Caddr); //pattern
        //    mx.Save_Argl();

        //    mx.Assign_Unev(mx.Exp.Cdddr); //body
        //    mx.Save_Unev();

        //    mx.Assign_Exp(mx.Exp.Cadr); //exp

        //    mx.Assign_GoTo(Eval_Dispatch);
        //    mx.Assign_Continue(Eval_Match_Continue);
        //}

        //private static void Eval_Match_Continue(Machine mx)
        //{
        //    mx.Restore_Unev();
        //    mx.Restore_Argl();

        //    mx.EnterNewScope();

        //    if (mx.Env.TryUnify(mx.Val, mx.Argl))
        //    {
        //        if (mx.Unev.IsNil)
        //        {
        //            mx.LeaveScope();
        //            mx.Restore_Continue();
        //            mx.Assign_Val(Boolean.True);
        //            mx.GoTo_Continue();
        //        }
        //        else
        //        {
        //            mx.Assign_GoTo(Eval_Sequence);
        //        }
        //    }
        //    else
        //    {
        //        mx.LeaveScope();
        //        mx.Restore_Continue();
        //        mx.Assign_Val(Boolean.False);
        //        mx.GoTo_Continue();
        //    }
        //}

        #endregion

        #region Error States

        private static void Err_Unknown_Expression(Machine mx)
        {
            mx.Assign_Val(Expression.Error);
            mx.Assign_GoTo(null);
        }

        private static void Err_Procedure(Machine mx)
        {
            mx.Assign_Val(Expression.Error);
            mx.Assign_GoTo(null);
        }

        private static void Err_Illegal_Operation(Machine mx)
        {
            mx.Assign_Val(Expression.Error);
            mx.Assign_GoTo(null);
        }

        #endregion
    }
}
