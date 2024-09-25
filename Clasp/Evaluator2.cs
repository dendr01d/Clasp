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


        private enum Label
        {
            Dispatch_Eval,
            Eval_Self, Eval_Variable,
            
            Eval_Operator, Eval_Operator_Continue,
            Eval_Operand_Loop, Eval_Operand_Acc, Eval_Operand_Acc_Last,

            Dispatch_Apply,
            Apply_Primitive, Apply_Compound, Apply_Macro,

            Dispatch_Special,

            Eval_Quote, Eval_Lambda,
            Eval_If, Eval_If_Decide,
            Eval_Begin, Eval_Sequence, Eval_Sequence_Continue,

            Apply_Eval, Apply_Apply,

            Dispatch_Mutate,

            Apply_Define, Apply_Set,
            Set_Car, Set_Cdr,

            Define_Syntax, //loop this into the other mutations

            //then there's the quasiquote stuff as well

            Eval_Error
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


                        case Label.Eval_Quote:
                            Val = Exp.Cadr;
                            break;


                        case Label.Eval_Lambda:
                            Val = new CompoundProcedure(Exp.Cadr, Exp.Cddr, Env);
                            break;


                        case Label.Eval_Operator:
                            ExprStack.Push(Exp);
                            Exp = Exp.Car;
                            EnvStack.Push(Env);
                            OpStack.Push(Label.Eval_Operator_Continue);
                            OpStack.Push(Label.Dispatch_Eval);
                            break;

                        case Label.Eval_Operator_Continue:
                            Exp = ExprStack.Pop();
                            Env = EnvStack.Pop();

                            Argl = Expression.Nil; //empty list
                            Unev = Exp.Cdr;
                            Proc = Val.Expect<Procedure>();

                            ExprStack.Push(Proc);
                            if (!Unev.IsNil && Proc.Expect<Procedure>().ApplicativeOrder)
                            {
                                Unev = Exp.Cdr;
                                OpStack.Push(Label.Eval_Operand_Loop);
                            }
                            else
                            {
                                OpStack.Push(Label.Dispatch_Apply);
                            }
                            break;

                        case Label.Eval_Operand_Loop:
                            Exp = Unev.IsAtom
                                ? Unev
                                : Unev.Car;
                            ExprStack.Push(Argl);
                            if (Unev.IsAtom || Unev.Cdr.IsNil)
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

                            Argl = Pair.Append(Argl, Pair.MakeList(Val));
                            Unev = Unev.Cdr;

                            OpStack.Push(Label.Eval_Operand_Loop);
                            break;

                        case Label.Eval_Operand_Acc_Last:
                            Argl = ExprStack.Pop();

                            Argl = Pair.Append(Argl, Pair.MakeList(Val));

                            OpStack.Push(Label.Dispatch_Apply);
                            break;


                        case Label.Dispatch_Apply:
                            Proc = ExprStack.Pop();
                            OpStack.Push(Proc.Expect<Procedure>() switch
                            {
                                PrimitiveProcedure => Label.Apply_Primitive,
                                CompoundProcedure => Label.Apply_Compound,
                                Macro => Label.Apply_Macro,
                                SpecialForm => Label.Dispatch_Special,
                                _ => throw new Exception("Unknown procedure type: " + Proc.ToString())
                            });
                            break;


                        case Label.Apply_Primitive:
                            Val = Proc.Expect<PrimitiveProcedure>().Apply(Argl);
                            break;

                        case Label.Apply_Compound:
                            CompoundProcedure cProc = Proc.Expect<CompoundProcedure>();
                            Env = cProc.Closure.Enclose();
                            Unev = cProc.Parameters;
                            while (!Unev.IsNil && !Argl.IsNil)
                            {
                                Env.BindNew(Unev.Car.Expect<Symbol>(), Argl.Car);
                                Unev = Unev.Cdr;
                                Argl = Argl.Cdr;
                            }
                            if (!Argl.IsNil) throw new ArityConflictException(cProc, Argl);
                            if (!Unev.IsNil) throw new ArityConflictException(cProc);
                            Unev = cProc.Body;
                            OpStack.Push(Label.Eval_Sequence);
                            break;


                        case Label.Apply_Macro:
                            Macro mProc = Proc.Expect<Macro>();
                            if (mProc.TryTransform(Exp, Env, out Expression result))
                            {
                                Exp = result;
                                OpStack.Push(Label.Dispatch_Eval);
                            }
                            else
                            {
                                throw new Exception("Failed to transform macro expression");
                            }
                            break;


                        case Label.Dispatch_Special:
                            OpStack.Push(Proc.Expect<SpecialForm>().OpLabel);
                            break;

                        case Label.Eval_If:
                            break;
                        case Label.Eval_If_Decide:
                            break;

                        case Label.Eval_Begin:
                            break;
                        case Label.Eval_Sequence:
                            break;
                        case Label.Eval_Sequence_Continue:
                            break;

                        case Label.Apply_Eval:
                            break;
                        case Label.Apply_Apply:
                            break;

                        case Label.Dispatch_Mutate:
                            break;

                        case Label.Apply_Define:
                            break;

                        case Label.Apply_Set:
                            break;

                        case Label.Set_Car:
                            break;

                        case Label.Set_Cdr:
                            break;

                        case Label.Define_Syntax:
                            break;

                        case Label.Eval_Error:
                            break;
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
