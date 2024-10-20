using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{


    internal class Evaluator
    {
        public Expression Exp  { get; set; }= Undefined.Instance;
        public Expression Val  { get; set; }= Undefined.Instance;
        public Expression Unev { get; set; } = Undefined.Instance;
        public Expression Argl { get; set; } = Undefined.Instance;
        public Expression Proc { get; set; } = Undefined.Instance;

        private Environment Env;

        private Stack<Expression> ExprStack;
        private Stack<Environment> EnvStack;
        private Stack<Label> OpStack;

        private Evaluator(Expression expr, Environment env)
        {
            Exp = expr;
            Env = env;

            ExprStack = new Stack<Expression>();
            EnvStack = new Stack<Environment>();
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
            Eval_Quasiquote,
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

            Eval_SyntaxCase, Eval_SyntaxCase_Did_Input,
            Eval_SyntaxCase_Loop, Eval_SyntaxCase_Continue,

            Eval_SyntaxClause, Eval_SyntaxClause_Did_Match,
            Eval_SyntaxClause_Fender, Eval_SyntaxClause_Fender_Continue,
            Eval_SyntaxClause_Output, Eval_SyntaxClause_Did_Output,

            Apply_BasicMatch, Apply_BasicMatch_Do,

            Dispatch_Match,
            Match_Symbol,
            Match_Pair, Match_Pair_Continue,
            Match_Repeating, Match_Repeating_Continue,
            Match_Datum,

            Eval_Syntax,

            Dispatch_Build,
            Build_Symbol,
            Build_List, Build_List_Loop, Build_List_Acc, Build_List_Acc_Last,
            Build_Repeating, Build_Repeating_Continue,
            Build_Datum,

            Dispatch_Expand,
            Expand_Datum,
            Expand_Identifier,
            Expand_Operator, Expand_Operator_Continue,
            Expand_List, Expand_List_Continue, Expand_List_End,
            Expand_Macro, Expand_Macro_Continue,
            Expand_Lambda, Expand_Lambda_Parameters, Expand_Lambda_Continue, Expand_Lambda_End,
            Expand_Quote,
            Expand_Define
        }

        public static Expression Evaluate(Expression expr, Environment env, bool print = false, bool pause = false)
        {
            Evaluator machine = new Evaluator(expr, env);
            try
            {
                uint stepNum = 0;

                while (machine.OpStack.Any())
                {
                    ++stepNum;
                    Label op = machine.OpStack.Pop();

                    Expression prevExp = machine.Exp;
                    Expression prevVal = machine.Val;
                    Expression prevUnev = machine.Unev;
                    Expression prevArgl = machine.Argl;
                    Expression prevProc = machine.Proc;

                    int prevExprCount = machine.ExprStack.Count;
                    int prevEnvCount = machine.EnvStack.Count;
                    int prevOpCount = machine.OpStack.Count;

                    machine.Step(op);

                    if (print)
                    {
                        Console.WriteLine(ConsoleSeparator());
                        Console.WriteLine("Step {0}: {1}", stepNum, op.ToString());
                        Console.WriteLine();
                        Console.WriteLine(" Exp: {0}", OldVsNew(prevExp, machine.Exp));
                        Console.WriteLine(" Val: {0}", OldVsNew(prevVal, machine.Val));
                        Console.WriteLine("Unev: {0}", OldVsNew(prevUnev, machine.Unev));
                        Console.WriteLine("Argl: {0}", OldVsNew(prevArgl, machine.Argl));
                        Console.WriteLine("Proc: {0}", OldVsNew(prevProc, machine.Proc));
                        Console.WriteLine();
                        Console.WriteLine("Exp Stk: {0}", FormatStack(machine.ExprStack, prevExprCount));
                        Console.WriteLine("Env Stk: {0}", FormatStack(machine.EnvStack, prevEnvCount));
                        Console.WriteLine(" Op Stk: {0}", FormatStack(machine.OpStack, prevOpCount));
                        Console.WriteLine();
                    }

                    if (pause)
                    {
                        Console.ReadKey(true);
                    }
                }
            }
            catch (Exception ex)
            {
                return new Error(ex);
            }
            return machine.Val;
        }

        public static Expression EvalSilent(Expression expr, Environment env)
        {
            Evaluator machine = new Evaluator(expr, env);
            while(machine.OpStack.Any())
            {
                machine.Step(machine.OpStack.Pop());
            }
            return machine.Val;
        }

        private void Step(Label op)
        {
            switch (op)
            {
                case Label.Dispatch_Eval:
                    {
                        OpStack.Push(Exp switch
                        {
                            Symbol => Label.Eval_Variable,
                            Error => Label.Eval_Error,
                            Atom => Label.Eval_Self,
                            _ => Label.Eval_Operator
                        });
                    }
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

                case Label.Apply_BasicMatch:
                    Exp = Exp.Cdr;
                    Argl = Exp.Cddar;
                    Unev = Exp.Cdddr;
                    Exp = Pair.Cons(Exp.Cadr, Exp.Car);
                    Env = Env.Enclose();

                    OpStack.Push(Label.Apply_BasicMatch_Do);
                    OpStack.Push(Label.Dispatch_Match);
                    break;

                case Label.Apply_BasicMatch_Do:
                    if (Val.IsTrue)
                    {
                        OpStack.Push(Label.Eval_Sequence);
                    }
                    break;

                case Label.Eval_Syntax:
                    Exp = Exp.Cadr;
                    OpStack.Push(Label.Dispatch_Build);
                    break;

                #endregion

                #region List-As-Proc-Application Form

                case Label.Eval_Operator:
                    {
                        ExprStack.Push(Exp);
                        Exp = Exp.Car;
                        EnvStack.Push(Env);
                        OpStack.Push(Label.Eval_Operator_Continue);
                        OpStack.Push(Label.Dispatch_Eval);
                    }
                    break;

                case Label.Eval_Operator_Continue:
                    {
                        Env = EnvStack.Pop();
                        Exp = ExprStack.Pop();

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
                    }
                    break;

                case Label.Eval_Operand_Loop:
                    {
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

                        OpStack.Push(Label.Dispatch_Eval);
                    }
                    break;

                case Label.Eval_Operand_Acc:
                    {
                        Unev = ExprStack.Pop();
                        Argl = ExprStack.Pop();
                        Env = EnvStack.Pop();

                        Argl = Pair.AppendLast(Argl, Val);
                        Unev = Unev.Cdr;

                        OpStack.Push(Label.Eval_Operand_Loop);
                    }
                    break;

                case Label.Eval_Operand_Acc_Last:
                    {
                        Argl = ExprStack.Pop();
                        Argl = Pair.AppendLast(Argl, Val);
                        OpStack.Push(Label.Dispatch_Apply);
                    }
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
                    {
                        CompoundProcedure cProc = Proc.Expect<CompoundProcedure>();
                        Env = cProc.Closure; //direct access to the closure itself
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
                    }
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
                    {
                        ExprStack.Push(Exp);
                        EnvStack.Push(Env);
                        Exp = Exp.Cadr;

                        OpStack.Push(Label.Eval_If_Decide);
                        OpStack.Push(Label.Dispatch_Eval);
                    }
                    break;

                case Label.Eval_If_Decide:
                    {
                        Env = EnvStack.Pop();
                        Exp = ExprStack.Pop();

                        Exp = Val.IsTrue
                            ? Exp.Caddr
                            : Exp.Cadddr;
                        OpStack.Push(Label.Dispatch_Eval);
                    }
                    break;

                #endregion

                #region Sequential Evaluation

                case Label.Eval_Begin:
                    Unev = Exp.Cdr;
                    OpStack.Push(Label.Eval_Sequence);
                    break;

                case Label.Eval_Sequence:
                    {
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


                #region Syntactic Restructuring

                case Label.Eval_SyntaxCase:
                    {
                        Exp = Exp.Cdr; //remove operator
                        ExprStack.Push(Exp);
                        Exp = Exp.Cadr;

                        EnvStack.Push(Env);
                        OpStack.Push(Label.Eval_SyntaxCase_Did_Input);
                        OpStack.Push(Label.Dispatch_Eval);
                    }
                    break;

                case Label.Eval_SyntaxCase_Did_Input:
                    {
                        Env = EnvStack.Pop();
                        Exp = ExprStack.Pop();
                        Argl = Exp.Caddr; //literals
                        Unev = Exp.Cadddr; //clauses

                        if (Unev.IsNil) throw new UncategorizedException("No clauses in syntax-case expression");
                        Exp = Val; //evaluated input

                        OpStack.Push(Label.Eval_SyntaxCase_Loop);
                    }
                    break;

                case Label.Eval_SyntaxCase_Loop:
                    {
                        ExprStack.Push(Argl);
                        ExprStack.Push(Exp);

                        Exp = Pair.ListStar(Exp, Argl, Unev.Car); //format transformer input

                        EnvStack.Push(Env);
                        ExprStack.Push(Unev);
                        OpStack.Push(Label.Eval_SyntaxCase_Continue);
                        OpStack.Push(Label.Eval_SyntaxClause);
                    }
                    break;

                case Label.Eval_SyntaxCase_Continue:
                    {
                        Unev = ExprStack.Pop();
                        Env = EnvStack.Pop();
                        Exp = ExprStack.Pop();
                        Argl = ExprStack.Pop();

                        if (Val.IsPair && Val.Car.IsTrue)
                        {
                            Val = Val.Cadr;
                        }
                        else if (Unev.Cdr.IsNil)
                        {
                            throw new UncategorizedException("Ran past end of syntax clauses");
                        }
                        else
                        {
                            Unev = Unev.Cdr;
                            OpStack.Push(Label.Eval_SyntaxCase_Loop);
                        }
                    }
                    break;

                #endregion

                #region Syntax-Case Clause Evaluation

                case Label.Eval_SyntaxClause:
                    {
                        //sub env to hold pattern variable bindings locally
                        Env = new ExpansionFrame(Env);
                        EnvStack.Push(Env);
                        Argl = Exp.Cadr; //literals
                        Unev = Exp.Cddr; //fenders and output
                        Exp = Pair.Cons(Exp.Caddr, Exp.Car); // (pattern . input)

                        OpStack.Push(Label.Eval_SyntaxClause_Did_Match);
                        OpStack.Push(Label.Dispatch_Match);
                    }
                    break;

                case Label.Eval_SyntaxClause_Did_Match:
                    if (Val.IsTrue)
                    {
                        Environment subEnv = Env;
                        Env = EnvStack.Pop();
                        Env.Subsume(subEnv);
                        Exp = Unev;
                        OpStack.Push(Exp.Cdr.IsNil
                            ? Label.Eval_SyntaxClause_Output
                            : Label.Eval_SyntaxClause_Fender);
                    }
                    else
                    {
                        Val = Pair.List(Boolean.False);
                    }
                    break;

                case Label.Eval_SyntaxClause_Fender:
                    Exp = Unev.Car;
                    ExprStack.Push(Unev);
                    EnvStack.Push(Env);
                    OpStack.Push(Label.Eval_SyntaxClause_Fender_Continue);
                    OpStack.Push(Label.Dispatch_Eval);
                    break;

                case Label.Eval_SyntaxClause_Fender_Continue:
                    if (Val.IsTrue)
                    {
                        Env = EnvStack.Pop();
                        Unev = ExprStack.Pop();
                        Unev = Unev.Cdr;
                        OpStack.Push(Exp.Cdr.IsNil
                            ? Label.Eval_SyntaxClause_Output
                            : Label.Eval_SyntaxClause_Fender);
                    }
                    else
                    {
                        Val = Pair.List(Boolean.False);
                    }
                    break;

                case Label.Eval_SyntaxClause_Output:
                    Exp = Unev.Car;
                    OpStack.Push(Label.Eval_SyntaxClause_Did_Output);
                    OpStack.Push(Label.Dispatch_Eval);
                    break;

                case Label.Eval_SyntaxClause_Did_Output:
                    Val = Pair.Cons(Boolean.True, Val);
                    break;

                #endregion

                #region Syntactic Pattern-Matching

                case Label.Dispatch_Match:
                    OpStack.Push(Exp.Car switch
                    {
                        Symbol => Label.Match_Symbol,
                        Pair => Label.Match_Pair,
                        _ => Label.Match_Datum
                    });
                    break;

                case Label.Match_Symbol:
                    Symbol pSym = Exp.Car.Expect<Symbol>();
                    if (pSym.Pred_Eq(Symbol.Underscore))
                    {
                        Val = Boolean.True;
                    }
                    else if (Pair.Enumerate(Argl).Any(pSym.Pred_Eq))
                    {
                        Val = pSym.Pred_Eq(Exp.Cdr);
                    }
                    else if (Env.BindsLocally(pSym))
                    {
                        Val = Env.LookUp(pSym).Pred_Equal(Exp.Cdr);
                    }
                    else
                    {
                        Env.BindNew(pSym, Exp.Cdr);
                        Val = Boolean.True;
                    }
                    break;

                case Label.Match_Pair:
                    if (Exp.Car.IsEllipticTerm)
                    {
                        if (!Exp.Cddar.IsNil) throw new UncategorizedException("Patterns don't permit ellipses in non-tail position");
                        Exp = Pair.Cons(Exp.Caar, Exp.Cdr);
                        OpStack.Push(Label.Match_Repeating);
                    }
                    else
                    {
                        ExprStack.Push(Exp);
                        Exp = Pair.Cons(Exp.Caar, Exp.Cdar);
                        OpStack.Push(Label.Match_Pair_Continue);
                        OpStack.Push(Label.Dispatch_Match);
                    }
                    break;

                case Label.Match_Pair_Continue:
                    Exp = ExprStack.Pop();
                    if (Val.IsTrue)
                    {
                        Exp = Pair.Cons(Exp.Cdar, Exp.Cddr);
                        OpStack.Push(Label.Dispatch_Match);
                    }
                    break;

                case Label.Match_Repeating:
                    if (Exp.Cdr.IsNil)
                    {
                        Val = Boolean.True;
                    }
                    else if (Exp.Cdr.IsAtom)
                    {
                        throw new UncategorizedException("Repeating patterns can't capture dotted lists");
                    }
                    else
                    {
                        ExprStack.Push(Exp);
                        Exp = Pair.Cons(Exp.Car, Exp.Cadr);
                        EnvStack.Push(Env);
                        Env = Env.Enclose();
                        OpStack.Push(Label.Match_Repeating_Continue);
                        OpStack.Push(Label.Dispatch_Match);
                    }
                    break;

                case Label.Match_Repeating_Continue:
                    {
                        if (Val.IsTrue)
                        {
                            ExpansionFrame subEnv = Env as ExpansionFrame
                                ?? throw new UncategorizedException("Unexpected Standard Frame in expansion context");
                            Env = EnvStack.Pop();
                            Env.SubsumeRecurrent(subEnv);
                            Exp = ExprStack.Pop();
                            Exp = Pair.Cons(Exp.Car, Exp.Cddr);
                            OpStack.Push(Label.Match_Repeating);
                        }
                    }
                    break;

                case Label.Match_Datum:
                    Val = Exp.Car.Pred_Eq(Exp.Cdr);
                    break;

                #endregion

                #region Syntactic Template-Building

                case Label.Dispatch_Build:
                    OpStack.Push(Exp switch
                    {
                        Symbol => Label.Build_Symbol,
                        Pair => Label.Build_List,
                        _ => Label.Build_Datum
                    });
                    break;

                case Label.Build_Symbol:
                    Val = Env.LookUp(Exp.Expect<Symbol>());
                    break;

                case Label.Build_List:
                    if (Exp.IsTagged(Symbol.Ellipsis))
                    {
                        if (Exp.IsEllipticTerm && Exp.Cddr.IsNil)
                        {
                            Val = Symbol.Ellipsis;
                        }
                        else
                        {
                            throw new UncategorizedException("Unexpected ellipsis term");
                        }
                    }
                    else
                    {
                        Argl = Expression.Nil;
                        Unev = Exp;
                        OpStack.Push(Label.Build_List_Loop);
                    }
                    break;

                case Label.Build_List_Loop:
                    ExprStack.Push(Argl);
                    ExprStack.Push(Unev);
                    if (Unev.IsAtom)
                    {
                        Exp = Unev;
                        OpStack.Push(Label.Eval_Operand_Acc_Last);
                        OpStack.Push(Label.Dispatch_Build);
                    }
                    else if (Unev.IsEllipticTerm)
                    {
                        Exp = Unev.Car;

                        OpStack.Push(Unev.Cddr.IsNil
                            ? Label.Build_List_Acc_Last
                            : Label.Build_List_Acc);

                        Argl = Expression.Nil;
                        OpStack.Push(Label.Build_Repeating);
                    }
                    else
                    {
                        Exp = Unev.Car;

                        OpStack.Push(Unev.Cdr.IsNil
                            ? Label.Build_List_Acc_Last
                            : Label.Build_List_Acc);

                        OpStack.Push(Label.Dispatch_Build);
                    }
                    break;

                case Label.Build_List_Acc:
                    Unev = ExprStack.Pop();
                    Argl = ExprStack.Pop();
                    Argl = Unev.IsEllipticTerm
                        ? Pair.Append(Argl, Val)
                        : Pair.Append(Argl, Pair.List(Val));
                    Unev = Unev.IsEllipticTerm
                        ? Unev.Cddr
                        : Unev.Cdr;
                    OpStack.Push(Label.Build_List_Loop);
                    break;

                case Label.Build_List_Acc_Last:
                    Unev = ExprStack.Pop();
                    Argl = ExprStack.Pop();
                    Argl = (Unev.IsAtom || Unev.IsEllipticTerm)
                        ? Pair.Append(Argl, Val)
                        : Pair.Append(Argl, Pair.List(Val));
                    break;

                case Label.Build_Repeating:
                    ExprStack.Push(Argl);
                    ExprStack.Push(Exp);
                    EnvStack.Push(Env);
                    Env = Env.SplitRecurrent();
                    OpStack.Push(Label.Build_Repeating_Continue);
                    OpStack.Push(Label.Dispatch_Build);
                    break;

                case Label.Build_Repeating_Continue:
                    Env = EnvStack.Pop();
                    Exp = ExprStack.Pop();
                    Argl = ExprStack.Pop();
                    Argl = Pair.Append(Argl, Pair.List(Val));
                    if (Env.MoreRecurrent())
                    {
                        OpStack.Push(Label.Build_Repeating);
                    }
                    else
                    {
                        Val = Argl;
                    }
                    break;

                case Label.Build_Datum:
                    Val = Exp;
                    break;


                #endregion


                #region Term Expansion

                case Label.Dispatch_Expand:
                    {
                        OpStack.Push(Exp switch
                        {
                            Identifier => Label.Expand_Identifier,
                            Pair => Label.Expand_Operator,
                            _ => Label.Expand_Datum
                        });
                    }
                    break;

                case Label.Expand_Datum:
                    {
                        Val = Exp.Expose();
                    }
                    break;

                case Label.Expand_Identifier:
                    {
                        Binding bType = Env.LookupBindingType(Exp.Expect<Identifier>());

                        if (bType == Binding.Transformer
                            && Env.LookUp(Exp.Expect<Symbol>()).Expect<CompoundProcedure>().Parameters.IsNil)
                        {
                            OpStack.Push(Label.Dispatch_Expand);
                            OpStack.Push(Label.Expand_Macro);
                        }
                        else if (bType != Binding.Variable)
                        {
                            throw new UncategorizedException("Invalid syntax " + Exp.Serialize());
                        }
                    }
                    break;

                case Label.Expand_Operator:
                    {
                        Exp = Exp.Expose();
                        ExprStack.Push(Exp);
                        Exp = Exp.Car;

                        EnvStack.Push(Env);
                        OpStack.Push(Label.Expand_Operator_Continue);
                        OpStack.Push(Label.Dispatch_Expand);
                    }
                    break;

                case Label.Expand_Operator_Continue:
                    {
                        Exp = ExprStack.Pop();
                        Env = EnvStack.Pop();

                        if (Val is Identifier id)
                        {
                            Binding bType = Env.LookupBindingType(Exp.Expect<Identifier>());

                            if (bType == Binding.Transformer)
                            {
                                Proc = Env.LookUp(Exp.Expect<Identifier>()).Expect<CompoundProcedure>();
                                OpStack.Push(Label.Dispatch_Expand);
                                OpStack.Push(Label.Expand_Macro);
                            }
                            else if (bType != Binding.Variable)
                            {
                                OpStack.Push(bType switch
                                {
                                    Binding.SpecialQuote => Label.Expand_Quote,
                                    Binding.SpecialLambda => Label.Expand_Lambda,
                                    Binding.SpecialDefine => Label.Expand_Define,
                                    _ => throw new UncategorizedException("Invalid syntax " + Exp.Serialize());
                                });
                            }
                        }
                        else
                        {
                            Argl = Pair.List(Val);
                            Unev = Exp.Cdr;
                            OpStack.Push(Label.Expand_List);
                        }
                    }
                    break;

                case Label.Expand_List:
                    {
                        ExprStack.Push(Argl);
                        Exp = Unev.IsAtom
                            ? Unev
                            : Unev.Car;

                        if (Unev.IsAtom || Unev.Cdr.IsNil)
                        {
                            OpStack.Push(Label.Expand_List_End);
                        }
                        else
                        {
                            EnvStack.Push(Env);
                            ExprStack.Push(Unev);
                            OpStack.Push(Label.Expand_List_Continue);
                        }
                        OpStack.Push(Label.Dispatch_Expand);
                    }
                    break;

                case Label.Expand_List_Continue:
                    {
                        Unev = ExprStack.Pop();
                        Argl = ExprStack.Pop();
                        Env = EnvStack.Pop();

                        Argl = Pair.AppendLast(Argl, Val);
                        Unev = Unev.Cdr;

                        OpStack.Push(Label.Expand_List);
                    }
                    break;

                case Label.Expand_List_End:
                    {
                        Argl = ExprStack.Pop();
                        Argl = Pair.AppendStar(Argl, Val);
                        Val = Argl;
                    }
                    break;

                case Label.Expand_Macro:
                    {
                        Expression mk = GetFreshMark(); //function in the evaluator itself to spit out int literals?
                        ExprStack.Push(mk);

                        Exp = Exp.Mark(mk);
                        OpStack.Push(Label.Expand_Macro_Continue);

                        if (Exp.IsAtom) //we came from expanding an identifier by itself
                        {
                            Argl = Expression.Nil;
                            Proc = Env.LookUp(Exp.Expect<Symbol>()).Expect<CompoundProcedure>();
                        }
                        else //we came from evaluating a list with a macro-bound identifier in the op position
                        {
                            Argl = Pair.List(Exp);
                            Proc = Env.LookUp(Exp.Expose().Car.Expect<Symbol>()).Expect<CompoundProcedure>();
                        }
                        OpStack.Push(Label.Apply_Compound);
                    }
                    break;

                case Label.Expand_Macro_Continue:
                    {
                        Expression mk = ExprStack.Pop(); //I guess I could use a register for this?
                        Val = Val.Mark(mk);
                    }
                    break;

                case Label.Expand_Quote:
                    {
                        Val = Exp.Strip();
                    }
                    break;

                case Label.Expand_Lambda:
                    {
                        //start as if expanding a normal list
                        Argl = Pair.List(Exp.Car);
                        Unev = Exp.Expose().Cdr.Expose();

                        //look at the lambda's parameter list
                        Exp = Unev.Car;
                        Unev = Unev.Cdr;

                        if (Exp.IsNil)
                        {
                            //no params means no symbol substitution
                            //means no special body environment
                            //means no special expansion pathway
                            Argl = Pair.AppendLast(Argl, Exp);
                            OpStack.Push(Label.Expand_List);
                        }
                        else
                        {
                            EnvStack.Push(Env);
                            Env = new ExpansionFrame(Env);
                            
                            ExprStack.Push(Argl);
                            Argl = Expression.Nil;

                            OpStack.Push(Label.Expand_Lambda_Parameters);
                        }
                    }
                    break;

                case Label.Expand_Lambda_Parameters:
                    {
                        Expression param;

                        if (Exp.IsAtom)
                        {
                            param = Exp.Resolve();
                        }
                        else
                        {
                            Exp = Exp.Expose();
                            param = Exp.Car;
                        }

                        Identifier oldSym = Identifier.Wrap(param.Expect<Symbol>());
                        Symbol newSym = new GenSym(oldSym);



                        //SPECIFICALLY
                        //we need to make sure it's bound to the same type as the old symbol
                        //eg if the old symbol represented the special quote form
                        //then the new symbol also must
                        Env.BindType(newSym, Binding.Variable);




                        Unev = Unev.Subst(oldSym, newSym);

                        if (Exp.IsAtom)
                        {
                            Argl = Pair.Append(Argl, newSym);
                            OpStack.Push(Label.Expand_Lambda_Continue);
                        }
                        else
                        {
                            Argl = Pair.AppendLast(Argl, newSym);

                            if (Exp.Cdr.IsNil)
                            {
                                OpStack.Push(Label.Expand_Lambda_Continue);
                            }
                            else
                            {
                                Exp = Exp.Cdr;
                                OpStack.Push(Label.Expand_Lambda_Parameters);
                            }
                        }

                    }
                    break;

                case Label.Expand_Lambda_Continue:
                    {
                        //append the substituted parameter list to the list elements
                        Exp = Argl;
                        Argl = ExprStack.Pop();
                        Argl = Pair.AppendLast(Argl, Exp);

                        //now recurse through the body like a normal list
                        //but remember to go back and pop out of the body env later
                        OpStack.Push(Label.Expand_Lambda_End);
                        OpStack.Push(Label.Expand_List);
                    }
                    break;

                case Label.Expand_Lambda_End:
                    {
                        //dismiss the lambda's inner environment
                        Env = EnvStack.Pop();
                    }
                    break;

                case Label.Expand_Define:
                    {
                        //similar to expanding a lambda but with three differences:
                        // 1- we may have to implicitly rewrite it into a lambda definition
                        // 2- there's only one parameter that can be bound, and it must exist
                        // 3- there's no separate env for the body, do the subst inline
                    }
                    break;

                #endregion


                default:
                    break;
            }
        }

        private static string ConsoleSeparator() => new string('_', Console.WindowWidth - 1);

        private static string OldVsNew(Expression older, Expression newer)
        {
            if (older != newer)
            {
                return "\x1B[33m"
                    + older.Print()
                    + "  ->  "
                    + newer.Print()
                    + "\u001b[0m";
            }
            else
            {
                return newer.Print();
            }
        }

        private static string FormatStack<T>(Stack<T> stk, int oldCount)
        {
            string output = string.Join("  ->  ", stk);

            if (stk.Count != oldCount)
            {
                return "\x1B[33m"
                    + output
                    + "\u001b[0m";
            }
            else
            {
                return output;
            }
        }
    }
}
