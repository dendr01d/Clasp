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
            uint step = 1;

            while(mx.GoTo is not null)
            {
                string stepName = mx.GoingTo; //capture before the machine executes

                mx.GoTo.Invoke(mx);
                PrintStep(cout, pauseEachStep, stepName, step++, mx);

                if (step == 10000)
                {
                    PrintStep(cout, true, stepName, step, mx);
                }

            }

            PrintStep(cout, pauseEachStep, "Final Result", step, null);

            return mx.Val ?? Expression.Error;
        }

        private static void PrintStep(TextWriter? tw, bool pause, string stepName, uint stepNum, Machine? mx)
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

        public static Expression SilentEval(Expression expr, Environment env)
        {
            Machine mx = new Machine(expr, env, Eval_Dispatch);

            while (mx.GoTo is not null)
            {
                mx.GoTo.Invoke(mx);
            }

            return mx.Val;
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

            { "set-car!", Eval_SetCar },
            { "set-cdr!", Eval_SetCdr },

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
            mx.Assign_Exp(mx.Exp.Cadr);
            mx.Assign_GoTo(Eval_Dispatch);
        }

        private static void Eval_Raw_Apply(Machine mx)
        {
            mx.Assign_Exp(Pair.Cons(
                mx.Exp.Cadr,
                mx.Exp.Caddr));
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
                mx.Exp.Cadr,
                mx.Exp.Cddr.Expect<Pair>(),
                mx.Env));
            mx.GoTo_Continue();
        }

        #endregion

        #region Imperative List Modification

        private static void Eval_SetCar(Machine mx)
        {
            mx.Save_Exp();
            mx.Assign_Exp(mx.Exp.Caddr);
            mx.Save_Continue();
            mx.Assign_Continue(Eval_SetCar_Do);
            mx.Assign_GoTo(Eval_Dispatch);
        }

        private static void Eval_SetCar_Do(Machine mx)
        {
            mx.Restore_Continue();
            mx.Restore_Exp();

            mx.Assign_Exp(mx.Exp.Cadr);
            mx.Exp.Expect<Pair>().SetCar(mx.Val);

            mx.Assign_Val(Symbol.Ok);
            mx.GoTo_Continue();
        }

        private static void Eval_SetCdr(Machine mx)
        {
            mx.Save_Exp();
            mx.Assign_Exp(mx.Exp.Caddr);
            mx.Save_Continue();
            mx.Assign_Continue(Eval_SetCdr_Do);
            mx.Assign_GoTo(Eval_Dispatch);
        }

        private static void Eval_SetCdr_Do(Machine mx)
        {
            mx.Restore_Continue();
            mx.Restore_Exp();

            mx.Assign_Exp(mx.Exp.Cadr);
            mx.Exp.Expect<Pair>().SetCdr(mx.Val);

            mx.Assign_Val(Symbol.Ok);
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

        #region Function Application-Position Term Evaluation

        private static void Eval_Application(Machine mx)
        {
            mx.Save_Continue();

            mx.Assign_Unev(mx.Exp.Cdr);
            mx.Assign_Exp(mx.Exp.Car);

            mx.Assign_GoTo(Eval_Dispatch);
            mx.Assign_Continue(Eval_Apply_Did_Op);

            mx.EnterNewScope();
            mx.Save_Unev();
        }

        private static void Eval_Apply_Did_Op(Machine mx)
        {
            mx.Restore_Unev();
            mx.LeaveScope();

            mx.Assign_Argl(Expression.Nil); //empty list

            mx.Assign_Proc(mx.Val.Expect<Procedure>());

            
            if (mx.Proc.ApplicativeOrder)
            {
                //if the args DO need to be evaluated, loop through them and do that
                mx.Save_Proc();
                mx.Assign_GoTo(Eval_Apply_Operand_Loop);
            }
            else
            {
                //mx.Restore_Continue();

                //otherwise rebuild the original expression and skip ahead
                mx.Restore_Continue();
                mx.Assign_Exp(Pair.Cons(mx.Exp, mx.Unev));
                mx.Assign_GoTo(Apply_Dispatch);
            }
        }

        #endregion

        #region Function Argument-Position Term Evaluation

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
            mx.NextUnev();

            mx.Assign_GoTo(Eval_Apply_Operand_Loop);
        }

        private static void Eval_Apply_Accumulate_Last_Arg(Machine mx)
        {
            mx.Restore_Argl();

            if (mx.Unev.IsAtom)
            {
                //if the args list was dotted, the append is less gentle
                mx.Assign_Argl(Pair.Append(mx.Argl, mx.Val));
            }
            else
            {
                mx.AppendArgl(mx.Val);
            }

            mx.Restore_Proc();
            mx.Assign_GoTo(Apply_Dispatch);
        }

        #endregion

        #region Procedure Application

        private static void Apply_Dispatch(Machine mx)
        {
            mx.Assign_GoTo(mx.Proc switch
            {
                SpecialForm sf => sf.InstructionPtr,
                PrimitiveProcedure => Primitive_Apply,
                CompoundProcedure => Compound_Apply,
                Macro => Macro_Apply,
                _ => Err_Procedure
            });
        }

        private static void Primitive_Apply(Machine mx)
        {
            PrimitiveProcedure proc = mx.Proc.Expect<PrimitiveProcedure>();

            if (!mx.Argl.IsList)
            {
                throw new UncategorizedException($"Expected List-type expression, but received '{mx.Argl}'.");
            }

            mx.Assign_Val(proc.Apply(mx.Argl));
            mx.Restore_Continue();
            mx.GoTo_Continue();
        }

        private static void Compound_Apply(Machine mx)
        {
            CompoundProcedure proc = mx.Proc.Expect<CompoundProcedure>();

            mx.ReplaceScope(proc.Closure.Enclose());
            mx.Assign_Unev(proc.Parameters);

            while (mx.Unev is Pair p)
            {
                if (mx.Argl.IsNil)
                {
                    throw new UncategorizedException($"Procedure {mx.Proc.ToPrinted()} expected additional arguments.");
                }

                mx.Env.BindNew(mx.Unev.Car.Expect<Symbol>(), mx.Argl.Car);
                mx.NextUnev();
                mx.NextArgl();
            }

            if (mx.Unev is Symbol sym)
            {
                mx.Env.BindNew(sym, mx.Argl); //even if it's nil
            }
            else if (!mx.Argl.IsNil)
            {
                throw new Exception($"Extraneous arguments {mx.Argl.ToPrinted()} provided to procedure {mx.Proc.ToPrinted()}.");
            }

            mx.Assign_Unev(proc.Body);
            mx.Assign_GoTo(Eval_Sequence);
        }

        private static void Macro_Apply(Machine mx)
        {
            Macro proc = mx.Proc.Expect<Macro>();
            mx.Assign_Unev(proc.Transformers);

            while (mx.Unev is Pair p)
            {
                if (p.Car is SyntaxRule sr
                    && sr.TryTransform(mx.Exp, proc.LiteralSymbols, proc.Closure, mx.Env, out Expression result))
                {
                    mx.Assign_Exp(result);
                    mx.Assign_GoTo(Eval_Dispatch);
                    break;
                }
                else
                {
                    mx.NextUnev();
                }
            }

            if (mx.Unev.IsNil)
            {
                throw new Exception($"No matching syntax found in transformation rules for {mx.Proc.ToPrinted()}.");
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
                    Pair.MakeList(
                        Symbol.Lambda,
                        mx.Exp.Cadr.Cdr,
                        mx.Exp.Caddr)));
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
            Symbol name = mx.Exp.Cadr.Expect<Symbol>();
            Expression literals = mx.Exp.Caddr;
            Pair clauses = mx.Exp.Cdddr.Expect<Pair>();

            Expression rules = Pair.MakeList(
                Pair.Enumerate(clauses)
                .Select(x => new SyntaxRule(x.Car, x.Cadr))
                .ToArray());

            Macro newMacro = new Macro(literals, rules.Expect<Pair>(), mx.Env);

            mx.Env.BindNew(name, newMacro);

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


    internal class Evaluation
    {

        private Label Goto;

        private enum Label
        {
            Dispatch_Eval,
            Eval_Self, Eval_Variable, Eval_Quoted, Eval_Lambda,
            Eval_Quasiquoted, Eval_SyntaxRules,

            Eval_Application, Eval_Application_Did_Op,
            Eval_Operand_Loop, Eval_Operand_Accumulate, Eval_Operand_Accumulate_Last,

            Dispatch_Apply,
            Apply_Primitive, Apply_Compound, Apply_Special, Apply_Macro,

            Eval_Begin, Eval_Sequence, Eval_Sequence_Continue, Eval_Sequence_End,

            Eval_If, Eval_If_Decide,

            Eval_Define, Eval_Define_Do,
            Eval_Set, Eval_Set_Do,
            Eval_DefineSyntax, Eval_DefineSyntax_Do,

            Eval_SetCar, Eval_SetCar_Do,
            Eval_SetCdr, Eval_SetCdr_Do,

            Dispatch_Expand,
            Expand_Nested, Expand_Did_Car, Expand_Did_Cdr,
            Dispatch_ExpandList,
            ExpandList_Continue, ExpandList_Spliced, ExpandList_Spliced_Continue,

            Dispatch_Match,
            Match_Empty, Match_Identifier, Match_Pair, Match_Repeating, Match_Datum,

            Dispatch_Build,
            Build_Empty, Build_Identifier, Build_Pair, Build_Repeating, Build_Datum,

            Eval_Error
        }

        public void Evaluate()
        {
            while (true)
            {
                switch (Goto)
                {
                    case Label.Dispatch_Eval:
                        break;
                    case Label.Eval_Self:
                        break;
                    case Label.Eval_Variable:
                        break;
                    case Label.Eval_Quoted:
                        break;
                    case Label.Eval_Lambda:
                        break;
                    case Label.Eval_Quasiquoted:
                        break;
                    case Label.Eval_SyntaxRules:
                        break;
                    case Label.Eval_Application:
                        break;
                    case Label.Eval_Application_Did_Op:
                        break;
                    case Label.Eval_Operand_Loop:
                        break;
                    case Label.Eval_Operand_Accumulate:
                        break;
                    case Label.Eval_Operand_Accumulate_Last:
                        break;
                    case Label.Dispatch_Apply:
                        break;
                    case Label.Apply_Primitive:
                        break;
                    case Label.Apply_Compound:
                        break;
                    case Label.Apply_Special:
                        break;
                    case Label.Apply_Macro:
                        break;
                    case Label.Eval_Begin:
                        break;
                    case Label.Eval_Sequence:
                        break;
                    case Label.Eval_Sequence_Continue:
                        break;
                    case Label.Eval_Sequence_End:
                        break;
                    case Label.Eval_If:
                        break;
                    case Label.Eval_If_Decide:
                        break;
                    case Label.Eval_Define:
                        break;
                    case Label.Eval_Define_Do:
                        break;
                    case Label.Eval_Set:
                        break;
                    case Label.Eval_Set_Do:
                        break;
                    case Label.Eval_DefineSyntax:
                        break;
                    case Label.Eval_DefineSyntax_Do:
                        break;
                    case Label.Eval_SetCar:
                        break;
                    case Label.Eval_SetCar_Do:
                        break;
                    case Label.Eval_SetCdr:
                        break;
                    case Label.Eval_SetCdr_Do:
                        break;
                    case Label.Dispatch_Expand:
                        break;
                    case Label.Expand_Nested:
                        break;
                    case Label.Expand_Did_Car:
                        break;
                    case Label.Expand_Did_Cdr:
                        break;
                    case Label.Dispatch_ExpandList:
                        break;
                    case Label.ExpandList_Continue:
                        break;
                    case Label.ExpandList_Spliced:
                        break;
                    case Label.ExpandList_Spliced_Continue:
                        break;
                    case Label.Dispatch_Match:
                        break;
                    case Label.Match_Empty:
                        break;
                    case Label.Match_Identifier:
                        break;
                    case Label.Match_Pair:
                        break;
                    case Label.Match_Repeating:
                        break;
                    case Label.Match_Datum:
                        break;
                    case Label.Dispatch_Build:
                        break;
                    case Label.Build_Empty:
                        break;
                    case Label.Build_Identifier:
                        break;
                    case Label.Build_Pair:
                        break;
                    case Label.Build_Repeating:
                        break;
                    case Label.Build_Datum:
                        break;
                    case Label.Eval_Error:
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
