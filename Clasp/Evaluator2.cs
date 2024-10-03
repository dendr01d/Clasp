using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{


    internal class Evaluator2
    {
        private Expression Exp = Undefined.Instance;
        private Expression Val = Undefined.Instance;
        private Expression Unev = Undefined.Instance;
        private Expression Argl = Undefined.Instance;
        private Expression Proc = Undefined.Instance;

        private Frame Env;

        private Stack<Expression> ExprStack;
        private Stack<Frame> EnvStack;
        private Stack<Label> OpStack;

        private Evaluator2(Expression expr, Frame env)
        {
            Exp = expr;
            Env = env;

            ExprStack = new Stack<Expression>();
            EnvStack = new Stack<Frame>();
            OpStack = new Stack<Label>();

            OpStack.Push(Label.Dispatch_Eval);
        }


        public enum Label
        {
            Dispatch_Eval,
            Eval_Self, Eval_Variable,
            
            Eval_Operator, Eval_Operator_Continue,
            Eval_Operand_Loop, Eval_Operand_Acc, Eval_Operand_Acc_Last,

            Dispatch_Apply,
            Apply_Primitive, Apply_Compound, Apply_Macro,

            Dispatch_Special,

            Eval_Quote, Eval_Lambda,
            Eval_Quasiquote, Eval_Syntax,
            Eval_Error,

            Eval_If, Eval_If_Decide,

            Eval_Begin,
            Eval_Sequence, Eval_Sequence_Continue, Eval_Sequence_End,

            Apply_Eval, Apply_Apply,

            Apply_Mutation,
            Mutate_Continue,

            Apply_Define, Define_Do,
            Apply_Set, Set_Do,
            Apply_DefineSyntax, DefineSyntax_Do,
            Set_Car, Set_Car_Do,
            Set_Cdr, Set_Cdr_Do,

            //then there's the quasiquote stuff as well

            NA
        }

        public static Expression Evaluate(Expression expr, Frame env, bool print = false)
        {
            Evaluator2 machine = new Evaluator2(expr, env);
            machine.Execute();
            return machine.Val;
        }

        private void Execute()
        {
            try
            {
                uint stepNum = 0;

                while (OpStack.Any())
                {
                    Label op = OpStack.Pop();

                    switch (op)
                    {
                        case Label.Dispatch_Eval:
                            OpStack.Push(Exp switch
                            {
                                Symbol => Label.Eval_Variable,
                                Error => Label.Eval_Error,
                                Atom => Label.Eval_Self,
                                _ => Label.Eval_Operator
                            });
                            break;

                        case Label.Eval_Self:
                            Val = Exp;
                            break;

                        case Label.Eval_Variable:
                            Val = Env.LookUp(Exp.Expect<Symbol>()); //what if I had a symbol register instead?
                            break;

                        #region Basic Special Forms

                        case Label.Eval_Quote:
                            Val = Exp.Cadr;
                            break;

                        case Label.Eval_Lambda:
                            if (Exp.Cddr.IsNil) throw new UncategorizedException("Null lambda body");
                            Val = new CompoundProcedure(Exp.Cadr, Exp.Cddr, Env);
                            break;

                        case Label.Eval_Error:
                            Val = Exp.Cadr is Charstring msg
                                ? new Error(msg.Value)
                                : new Error(Exp.Cdr);
                            break;


                        case Label.Apply_Eval:
                            Exp = Exp.Cadr;
                            OpStack.Push(Label.Dispatch_Eval);
                            break;

                        case Label.Apply_Apply:
                            Exp = Pair.Cons(Exp.Cadr, Exp.Cddr);
                            OpStack.Push(Label.Dispatch_Eval);
                            break;

                        #endregion

                        #region List-As-Proc-Application Form

                        case Label.Eval_Operator:
                            ExprStack.Push(Exp);
                            Exp = Exp.Car;
                            EnvStack.Push(Env);
                            OpStack.Push(Label.Eval_Operator_Continue);
                            OpStack.Push(Label.Dispatch_Eval);
                            break;

                        case Label.Eval_Operator_Continue:
                            Env = EnvStack.Pop();
                            Exp = ExprStack.Pop();

                            Unev = Exp.Cdr;
                            Proc = Val.Expect<Procedure>();

                            ExprStack.Push(Proc);
                            if (!Unev.IsNil && Proc.Expect<Procedure>().ApplicativeOrder)
                            {
                                Argl = Expression.Nil; //empty list
                                OpStack.Push(Label.Eval_Operand_Loop);
                                ExprStack.Push(Proc);
                            }
                            else
                            {
                                OpStack.Push(Label.Dispatch_Apply);
                            }
                            break;

                        case Label.Eval_Operand_Loop:
                            ExprStack.Push(Argl);
                            Exp = Unev.IsAtom
                                ? Unev
                                : Unev.Car;
                            if (Unev.IsAtom || Unev.Cdr.IsNil) //dotted tail
                            {
                                OpStack.Push(Label.Eval_Operand_Acc_Last);
                            }
                            else
                            {
                                EnvStack.Push(Env);
                                ExprStack.Push(Unev);
                                OpStack.Push(Label.Eval_Operand_Acc);
                            }
                            break;

                        case Label.Eval_Operand_Acc:
                            Unev = ExprStack.Pop();
                            Argl = ExprStack.Pop();
                            Env = EnvStack.Pop();

                            Argl = Pair.Append(Argl, Pair.List(Val));
                            Unev = Unev.Cdr;

                            OpStack.Push(Label.Eval_Operand_Loop);
                            break;

                        case Label.Eval_Operand_Acc_Last:
                            Argl = ExprStack.Pop();

                            Argl = Pair.Append(Argl, Pair.List(Val));

                            OpStack.Push(Label.Dispatch_Apply);
                            break;

                        #endregion

                        #region Procedural Application

                        case Label.Dispatch_Apply:
                            Proc = ExprStack.Pop();
                            OpStack.Push(Proc.Expect<Procedure>() switch
                            {
                                PrimitiveProcedure => Label.Apply_Primitive,
                                CompoundProcedure => Label.Apply_Compound,
                                Macro => Label.Apply_Macro,
                                SpecialForm => Label.Dispatch_Special,
                                _ => throw new UncategorizedException("Unknown procedure type: " + Proc.ToString())
                            });
                            break;


                        case Label.Apply_Primitive:
                            Val = Proc.Expect<PrimitiveProcedure>().Apply(Argl);
                            break;

                        case Label.Apply_Compound:
                            CompoundProcedure cProc = Proc.Expect<CompoundProcedure>();
                            Env = cProc.Closure.Enclose();
                            Unev = cProc.Parameters;
                            while (Unev.IsPair && Argl.IsPair) //pair off args
                            {
                                Env.BindNew(Unev.Car.Expect<Symbol>(), Argl.Car);
                                Unev = Unev.Cdr;
                                Argl = Argl.Cdr;
                            }

                            if (Unev is Symbol) // iff last parameter is dotted
                            {
                                Env.BindNew(Unev.Expect<Symbol>(), Argl);
                            }
                            else
                            {
                                if (!Argl.IsNil) throw new ArityConflictException(cProc, Argl);
                                if (!Unev.IsNil) throw new ArityConflictException(cProc);
                            }
                            Unev = cProc.Body;
                            OpStack.Push(Label.Eval_Sequence);
                            break;


                        //case Label.Apply_Macro:
                        //    Macro mProc = Proc.Expect<Macro>();
                        //    if (mProc.TryTransform(Exp, Env, out Expression result))
                        //    {
                        //        Exp = result;
                        //        OpStack.Push(Label.Dispatch_Eval);
                        //    }
                        //    else
                        //    {
                        //        throw new Exception("Failed to transform macro expression");
                        //    }
                        //    break;


                        case Label.Dispatch_Special:
                            OpStack.Push(Proc.Expect<SpecialForm>().OpCode);
                            break;

                        #endregion

                        #region Conditional Evaluation

                        case Label.Eval_If:
                            ExprStack.Push(Exp);
                            EnvStack.Push(Env);
                            Exp = Exp.Cadr;

                            OpStack.Push(Label.Eval_If_Decide);
                            OpStack.Push(Label.Dispatch_Eval);
                            break;

                        case Label.Eval_If_Decide:
                            Env = EnvStack.Pop();
                            Exp = ExprStack.Pop();

                            Exp = Val.IsTrue
                                ? Exp.Caddr
                                : Exp.Cadddr;
                            OpStack.Push(Label.Dispatch_Eval);
                            break;

                        #endregion

                        #region Sequential Evaluation

                        case Label.Eval_Begin:
                            Unev = Exp.Cdr;
                            OpStack.Push(Label.Eval_Sequence);
                            break;

                        case Label.Eval_Sequence:
                            if (Unev.IsAtom) throw new UncategorizedException("Expected pair type in sequential evaluation");
                            Exp = Unev.Car;
                            if (Unev.Cdr.IsNil)
                            {
                                OpStack.Push(Label.Eval_Sequence_End);
                            }
                            else
                            {
                                EnvStack.Push(Env);
                                ExprStack.Push(Unev);

                                OpStack.Push(Label.Eval_Sequence_Continue);
                                OpStack.Push(Label.Dispatch_Eval);
                            }
                            break;

                        case Label.Eval_Sequence_Continue:
                            Unev = ExprStack.Pop();
                            Env = EnvStack.Pop();

                            Unev = Unev.Cdr;
                            OpStack.Push(Label.Eval_Sequence);
                            break;

                        case Label.Eval_Sequence_End:
                            OpStack.Push(Label.Dispatch_Eval);
                            break;

                        #endregion

                        #region Generic Mutation Form Handling

                        case Label.Apply_Mutation:
                            ExprStack.Push(Exp.Cadr);
                            EnvStack.Push(Env);
                            Exp = Exp.Caddr;
                            OpStack.Push(Label.Mutate_Continue);
                            OpStack.Push(Label.Dispatch_Eval);
                            break;

                        case Label.Mutate_Continue:
                            Env = EnvStack.Pop();
                            Exp = ExprStack.Pop();
                            break;

                        #endregion

                        #region Environmental Mutation

                        case Label.Apply_Define:
                            if (!Exp.Cadr.IsAtom) //rewrite implicit lambda definition
                            {
                                Exp = Pair.List(
                                    Exp.Car,
                                    Exp.Cadr.Car,
                                    Pair.List(
                                        Symbol.Lambda,
                                        Exp.Cadr.Cdr,
                                        Exp.Caddr));
                            }
                            OpStack.Push(Label.Define_Do);
                            OpStack.Push(Label.Apply_Mutation);
                            break;

                        case Label.Define_Do:
                            Env.BindNew(Exp.Expect<Symbol>(), Val);
                            Val = Symbol.Ok;
                            break;

                        case Label.Apply_Set:
                            OpStack.Push(Label.Set_Do);
                            OpStack.Push(Label.Apply_Mutation);
                            break;

                        case Label.Set_Do:
                            Env.RebindExisting(Exp.Expect<Symbol>(), Val);
                            Val = Symbol.Ok;
                            break;

                        case Label.Apply_DefineSyntax:
                            throw new NotImplementedException();
                            break;

                        case Label.DefineSyntax_Do:
                            Env.BindNew(Exp.Expect<Symbol>(), Val.Expect<Macro>());
                            Val = Symbol.Ok;
                            break;

                        #endregion

                        #region Structural Mutation

                        case Label.Set_Car:
                            OpStack.Push(Label.Set_Car_Do);
                            OpStack.Push(Label.Apply_Mutation);
                            break;

                        case Label.Set_Car_Do:
                            Exp.Expect<Pair>().SetCar(Val);
                            Val = Symbol.Ok;
                            break;

                        case Label.Set_Cdr:
                            OpStack.Push(Label.Set_Cdr_Do);
                            OpStack.Push(Label.Apply_Mutation);
                            break;

                        case Label.Set_Cdr_Do:
                            Exp.Expect<Pair>().SetCdr(Val);
                            Val = Symbol.Ok;
                            break;

                        #endregion

                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Val = new Error(ex);
            }
        }

    }
}
