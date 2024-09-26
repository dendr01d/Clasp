//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Clasp
//{
//    internal class RegisterMachine
//    {
//        private Expression Exp = Undefined.Instance;
//        private Expression Val = Undefined.Instance;
//        private Expression Unev = Undefined.Instance;
//        private Expression Argl = Undefined.Instance;
//        private Expression Proc = Undefined.Instance;

//        private Frame Env;

//        private Stack<Expression> ExprStack;
//        private Stack<Frame> EnvStack;
//        private Stack<Label> OpStack;

//        private RegisterMachine(Expression expr, Frame env)
//        {
//            Exp = expr;
//            Env = env;

//            ExprStack = new Stack<Expression>();
//            EnvStack = new Stack<Frame>();
//            OpStack = new Stack<Label>();

//            OpStack.Push(Label.Dispatch_Eval);
//        }


//        /// <summary>
//        /// Functionally these act as op-codes for the machine
//        /// </summary>
//        public enum Label
//        {
//            Dispatch_Eval,

//            Eval_Self, Eval_Variable, Eval_Quoted,
//            Eval_Lambda, Eval_SyntaxRules,
//            Eval_Quasiquoted,

//            Eval_Application_Form, Eval_Application_Did_Op,
//            Eval_Operand_Loop, Eval_Operand_Accumulate, Eval_Operand_Accumulate_Last,

//            Dispatch_Apply,
//            Apply_Primitive, Apply_Compound, Apply_Special, Apply_Macro,

//            Eval_Begin, Eval_Sequence, Eval_Sequence_Continue, Eval_Sequence_End,

//            Eval_If, Eval_If_Decide,

//            Eval_Define, Eval_Define_Do,
//            Eval_Set, Eval_Set_Do,
//            Eval_DefineSyntax, Eval_DefineSyntax_Do,

//            Eval_SetCar, Eval_SetCar_Do,
//            Eval_SetCdr, Eval_SetCdr_Do,

//            Dispatch_Expand,
//            Expand_Nested, Expand_Did_Car, Expand_Did_Cdr,
//            Dispatch_ExpandList,
//            ExpandList_Continue, ExpandList_Spliced, ExpandList_Spliced_Continue,


//            Eval_Error,
//            NA
//        }

//        private void Execute()
//        {
//            try
//            {
//                uint stepNum = 0;

//                while (OpStack.Any())
//                {
//                    Label op = OpStack.Pop();

//                    switch (op)
//                    {
//                        case Label.Dispatch_Eval:
//                            OpStack.Push(Exp switch
//                            {
//                                Error => Label.Eval_Error,
//                                Symbol => Label.Eval_Variable,
//                                Atom => Label.Eval_Self,
//                                _ => Label.Eval_Application_Form
//                            });
//                            break;

//                        case Label.Eval_Error:
//                            Val = Exp;
//                            OpStack.Clear();
//                            break;

//                        case Label.Eval_Self:
//                            Val = Exp;
//                            break;

//                        case Label.Eval_Variable:
//                            Val = EnvStack.Peek().LookUp(Exp.Expect<Symbol>());
//                            break;

//                        #region List-As-Procedure-Application

//                        case Label.Eval_Application_Form:
//                            ExprStack.Push(Exp);
//                            Exp = Exp.Car;
//                            OpStack.Push(Label.Eval_Application_Did_Op);
//                            OpStack.Push(Label.Dispatch_Eval);
//                            EnvStack.Push(Env);
//                            break;

//                        case Label.Eval_Application_Did_Op:
//                            EnvStack.Pop();
//                            Exp = ExprStack.Pop();
//                            Proc = Val.AsProcedure();
//                            OpStack.Push(Label.Dispatch_Apply);
//                            if (Proc.ApplicativeOrder)
//                            {
//                                Argl = Empty.Instance;
//                                Unev = Exp.Cdr;
//                                ExprStack.Push(Proc);
//                                OpStack.Push(Label.Eval_Operand_Loop);
//                            }
//                            break;

//                        case Label.Eval_Operand_Loop:
//                            ExprStack.Push(Argl);
//                            Exp = Unev.IsAtom //atomic tail of dotted list
//                                ? Unev
//                                : Unev.Car;
//                            if (Unev.IsAtom || Unev.Car.IsNil)
//                            {
//                                OpStack.Push(Label.Eval_Operand_Accumulate_Last);
//                            }
//                            else
//                            {
//                                EnvStack.Push(Env);
//                                ExprStack.Push(Unev);

//                                OpStack.Push(Label.Eval_Operand_Accumulate);
//                            }
//                            OpStack.Push(Label.Dispatch_Eval);
//                            break;

//                        case Label.Eval_Operand_Accumulate:
//                            Unev = ExprStack.Pop();
//                            Env = EnvStack.Pop();

//                            Argl = ExprStack.Pop();
//                            Argl = Pair.Append(Argl, Pair.List(Val));
//                            Unev = Unev.Cdr;

//                            OpStack.Push(Label.Eval_Operand_Loop);
//                            break;

//                        case Label.Eval_Operand_Accumulate_Last:
//                            Argl = ExprStack.Pop();
//                            Argl = Unev.IsAtom //atomic tail of dotted list
//                                ? Pair.Append(Argl, Val)
//                                : Pair.Append(Argl, Pair.List(Val));

//                            Proc = ExprStack.Pop().AsProcedure();
//                            OpStack.Push(Label.Dispatch_Apply);
//                            break;

//                        #endregion

//                        #region Application of Procedure

//                        case Label.Dispatch_Apply:
//                            OpStack.Push(Exp switch
//                            {
//                                SpecialForm => throw new NotImplementedException(),
//                                PrimitiveProcedure => Label.Apply_Primitive,
//                                CompoundProcedure => Label.Apply_Compound,
//                                Macro => Label.Apply_Macro,
//                                _ => throw new NotImplementedException()
//                            });
//                            break;

//                        case Label.Apply_Primitive:
//                            Val = Proc.AsPrimitive().Apply(Argl);
//                            break;

//                        case Label.Apply_Compound:
//                            Env = Proc.AsCompound().Closure.Enclose();
//                            Unev = Proc.AsCompound().Parameters;
//                            while (Unev is Pair p)
//                            {
//                                if (Argl.IsNil) throw new UncategorizedException($"Procedure {Proc.ToPrinted()} expected additional arguments.");
//                                Env.BindNew(Unev.Car.AsSymbol(), Argl.Car);
//                                Unev = Unev.Cdr;
//                                Argl = Argl.Cdr;
//                            }
//                            if (Unev is Symbol sym) //from a dotted pair
//                            {
//                                Env.BindNew(sym, Argl);
//                            }
//                            else if (!Argl.IsNil)
//                            {
//                                throw new Exception($"Extraneous arguments {Argl.ToPrinted()} provided to procedure {Proc.ToPrinted()}.");
//                            }
//                            Unev = Proc.AsCompound().Body;
//                            OpStack.Push(Label.Eval_Sequence);
//                            break;

//                        case Label.Apply_Macro:
//                            throw new NotImplementedException();
//                            break;

//                        case Label.Apply_Special:
//                            throw new NotImplementedException();
//                            break;

//                        #endregion

//                        #region Sequential Evaluation

//                        case Label.Eval_Begin:
//                            Unev = Exp.Cdr;
//                            OpStack.Push(Label.Eval_Sequence);
//                            break;

//                        case Label.Eval_Sequence:
//                            Exp = Unev.Car;
//                            if (Unev.Cdr.IsNil)
//                            {
//                                OpStack.Push(Label.Eval_Sequence_End);
//                            }
//                            else
//                            {
//                                EnvStack.Push(Env);
//                                ExprStack.Push(Unev);

//                                OpStack.Push(Label.Eval_Sequence_Continue);
//                                OpStack.Push(Label.Dispatch_Eval);
//                            }
//                            break;

//                        case Label.Eval_Sequence_Continue:

//                            Unev = ExprStack.Pop();
//                            Env = EnvStack.Pop();

//                            Unev = Unev.Cdr;
//                            OpStack.Push(Label.Eval_Sequence);
//                            break;

//                        case Label.Eval_Sequence_End:
//                            OpStack.Push(Label.Dispatch_Eval);
//                            break;

//                        #endregion

//                        #region Basic Special Forms

//                        case Label.Eval_Quoted:
//                            Val = Exp.Cadr;
//                            break;

//                        case Label.Eval_Quasiquoted:
//                            Exp = Exp.Cadr;
//                            OpStack.Push(Label.Dispatch_Expand);
//                            break;

//                        case Label.Eval_Lambda:
//                            Val = new CompoundProcedure(
//                                Exp.Cadr,
//                                Exp.Cddr.Expect<Pair>(),
//                                Env);
//                            break;

//                        case Label.Eval_SyntaxRules:
//                            throw new NotImplementedException();
//                            break;

//                        #endregion

//                        #region Conditional Evaluation

//                        case Label.Eval_If:
//                            ExprStack.Push(Exp);
//                            Exp = Exp.Cadr;

//                            OpStack.Push(Label.Eval_If_Decide);
//                            OpStack.Push(Label.Dispatch_Eval);
//                            break;

//                        case Label.Eval_If_Decide:
//                            Env = EnvStack.Pop();
//                            Exp = ExprStack.Pop();

//                            Exp = Val.IsTrue
//                                ? Exp.Caddr
//                                : Exp.Cadddr;

//                            OpStack.Push(Label.Dispatch_Eval);
//                            break;

//                        #endregion

//                        #region Environment Alteration

//                        case Label.Eval_Define:
//                            if (!Exp.Cadr.IsAtom) //rewrite implicit lambda definition
//                            {
//                                Exp = Pair.List(
//                                    Exp.Car,
//                                    Exp.Cadr.Car,
//                                    Pair.List(
//                                        Symbol.Lambda,
//                                        Exp.Cadr.Cdr,
//                                        Exp.Caddr));
//                            }
//                            ExprStack.Push(Exp.Cadr.AsSymbol()));
//                            Exp = Exp.Caddr;
//                            EnvStack.Push(Env);
//                            OpStack.Push(Label.Eval_Define_Do);
//                            OpStack.Push(Label.Dispatch_Eval);
//                            break;

//                        case Label.Eval_Define_Do:
//                            Env = EnvStack.Pop();
//                            Unev = ExprStack.Pop();
//                            Env.BindNew(Unev.AsSymbol(), Val);
//                            Val = Symbol.Ok;
//                            break;

//                        case Label.Eval_Set:
//                            ExprStack.Push(Exp.Cadr.AsSymbol());
//                            Exp = Exp.Caddr;
//                            EnvStack.Push(Env);
//                            OpStack.Push(Label.Eval_Define_Do);
//                            OpStack.Push(Label.Dispatch_Eval);
//                            break;

//                        case Label.Eval_Set_Do:
//                            Env = EnvStack.Pop();
//                            Unev = ExprStack.Pop();
//                            Env.RebindExisting(Unev.AsSymbol(), Val);
//                            Val = Symbol.Ok;
//                            break;

//                        case Label.Eval_DefineSyntax:
//                            ExprStack.Push(Exp.Cadr.AsSymbol());
//                            Exp = Exp.Caddr;
//                            EnvStack.Push(Env);
//                            OpStack.Push(Label.Eval_Define_Do);
//                            throw new NotImplementedException(); //should I just always expect this to be syntax-rules?
//                            break;

//                        case Label.Eval_DefineSyntax_Do:

//                            break;

//                        #endregion

//                        #region Structural Alteration

//                        case Label.Eval_SetCar:
//                            ExprStack.Push(Exp);
//                            Exp = Exp.Caddr;
//                            OpStack.Push(Label.Eval_SetCar_Do);
//                            OpStack.Push(Label.Dispatch_Eval);
//                            break;

//                        case Label.Eval_SetCar_Do:
//                            Exp = ExprStack.Pop();
//                            Exp = Exp.Cadr;
//                            Exp.AsPair().SetCar(Val);
//                            Val = Symbol.Ok;
//                            break;

//                        case Label.Eval_SetCdr:
//                            ExprStack.Push(Exp);
//                            Exp = Exp.Caddr;
//                            OpStack.Push(Label.Eval_SetCar_Do);
//                            OpStack.Push(Label.Dispatch_Eval);
//                            break;

//                        case Label.Eval_SetCdr_Do:
//                            Exp = ExprStack.Pop();
//                            Exp = Exp.Cadr;
//                            Exp.AsPair().SetCdr(Val);
//                            Val = Symbol.Ok;
//                            break;

//                        #endregion

//                        #region Quasiquote Expansion

//                        case Label.Dispatch_Expand:
//                            if (Exp.IsAtom)
//                            {
//                                Val = Exp;
//                            }
//                            else if (Exp.IsTagged(Symbol.Quasiquote))
//                            {
//                                Exp = Exp.Cadr;
//                                OpStack.Push(Label.Dispatch_Expand);
//                                OpStack.Push(Label.Expand_Nested);
//                                OpStack.Push(Label.Dispatch_Expand);
//                            }
//                            else if (Exp.IsTagged(Symbol.Unquote))
//                            {
//                                Exp = Exp.Cadr;
//                            }
//                            else if (Exp.IsTagged(Symbol.UnquoteSplicing))
//                            {
//                                throw new UncategorizedException("Illegal operation lol");
//                            }
//                            else
//                            {
//                                ExprStack.Push(Exp);
//                                Exp = Exp.Car;
//                                OpStack.Push(Label.Expand_Did_Car);
//                                OpStack.Push(Label.Dispatch_ExpandList);
//                            }
//                            break;

//                        case Label.Expand_Nested:
//                            Exp = Val;
//                            OpStack.Push(Label.Dispatch_Expand);
//                            break;

//                        case Label.Expand_Did_Car:
//                            Exp = ExprStack.Pop();
//                            Exp = Exp.Cdr;
//                            ExprStack.Push(Val);
//                            OpStack.Push(Label.Expand_Did_Cdr);
//                            OpStack.Push(Label.Dispatch_Expand);
//                            break;

//                        case Label.Expand_Did_Cdr:
//                            Argl = ExprStack.Pop();
//                            Val = Pair.Append(Argl, Val);
//                            break;

//                        case Label.Dispatch_ExpandList:
//                            if (Exp.IsAtom)
//                            {
//                                Val = Pair.List(Exp);
//                            }
//                            else if (Exp.IsTagged(Symbol.Quasiquote))
//                            {
//                                Exp = Exp.Cadr;
//                                OpStack.Push(Label.Expand_Nested);
//                                OpStack.Push(Label.Dispatch_Expand);
//                            }
//                            else if (Exp.IsTagged(Symbol.Unquote))
//                            {
//                                Exp = Exp.Cadr;
//                                OpStack.Push(Label.ExpandList_Continue);
//                                OpStack.Push(Label.Dispatch_Eval);
//                            }
//                            else if (Exp.IsTagged(Symbol.UnquoteSplicing))
//                            {
//                                Unev = Exp.Cdr.AsPair();
//                                Argl = Empty.Instance;
//                                OpStack.Push(Label.ExpandList_Spliced);
//                            }
//                            else
//                            {
//                                ExprStack.Push(Exp);
//                                Exp = Exp.Car;
//                                OpStack.Push(Label.ExpandList_Continue);
//                                OpStack.Push(Label.Expand_Did_Car);
//                                OpStack.Push(Label.Dispatch_ExpandList);
//                            }
//                            break;

//                        case Label.ExpandList_Continue:
//                            Val = Pair.List(Val);
//                            break;

//                        case Label.ExpandList_Spliced:
//                            if (Unev.IsNil)
//                            {
//                                Val = Argl;
//                            }
//                            else
//                            {
//                                Exp = Unev.Car;
//                                Unev = Unev.Cdr;
//                                ExprStack.Push(Unev);
//                                ExprStack.Push(Argl);
//                                OpStack.Push(Label.ExpandList_Spliced_Continue);
//                                OpStack.Push(Label.Dispatch_Eval);
//                            }
//                            break;

//                        case Label.ExpandList_Spliced_Continue:
//                            Argl = ExprStack.Pop();
//                            Unev = ExprStack.Pop();
//                            Argl = Pair.Append(Argl, Val);
//                            OpStack.Push(Label.ExpandList_Spliced);
//                            break;

//                        #endregion

//                        #region Pattern-Matching

//                        //case Label.Eval_Match:
//                        //    Exp = Exp.Cadr;
//                        //    Unev = Exp.Cddr;
//                        //    ExprStack.Push(Unev);
//                        //    EnvStack.Push(Env);
//                        //    OpStack.Push(Label.Dispatch_Match);
//                        //    OpStack.Push(Label.Dispatch_Eval);
//                        //    break;

//                        //case Label.Eval_Match_Did_Input:
//                        //    Env = EnvStack.Pop();
//                        //    Unev = ExprStack.Pop();
//                        //    Exp = Val;
//                        //    OpStack.Push(Label.Eval_Match_Do);
//                        //    break;

//                        //case Label.Eval_Match_Do:
//                        //    if (Unev.Cdr.IsNil)
//                        //    {
//                        //        OpStack.Push(Label.Eval_Match_Final);
//                        //    }
//                        //    else
//                        //    {
//                        //        EnvStack.Push(Env);
//                        //        ExprStack.Push(Unev);

//                        //        OpStack.Push(Label.Eval_Match_Continue);
//                        //        OpStack.Push(Label.Dispatch_Match);
//                        //    }
//                        //    Val = Boolean.True;
//                        //    Unev = Unev.Caar; //grab the pattern to be matched against
//                        //    break;

//                        //case Label.Eval_Match_Continue:
//                        //    Unev = ExprStack.Pop();
//                        //    Env = EnvStack.Pop();

//                        //    if (Val.IsTrue)
//                        //    {
//                        //        ExprStack.Push()
//                        //    }

//                        //    Unev = Unev.Cdr;
//                        //    OpStack.Push(Label.Eval_Sequence);
//                        //    break;

//                        //case Label.Eval_Match_Final:

//                        //    Unev = ExprStack.Pop();
//                        //    if (Val.IsTrue) //match succeeded
//                        //    {
//                        //        Exp = Unev.Cadar; //template portion of current
//                        //        OpStack.Push(Label.Dispatch_Eval); //use the saturated environment
//                        //    }

//                        //    break;

//                        //case Label.Dispatch_Match:
//                        //    if (Unev.IsNil)
//                        //    {
//                        //        Val = Boolean.Judge(Exp.IsNil);
//                        //    }
//                        //    else if (Unev is Symbol sym)
//                        //    {
//                        //        //matches if pattern:
//                        //        //is a literal symbol & i/o are eq?
//                        //        //is an underscore
//                        //        //is bound within match, and input matches bound value
//                        //        //always matches otherwise
//                        //    }
//                        //    break;

//                        //case Label.Match_Empty:
//                        //    break;
//                        //case Label.Match_Identifier:
//                        //    break;
//                        //case Label.Match_Pair:
//                        //    break;
//                        //case Label.Match_Repeating:
//                        //    break;
//                        //case Label.Match_Datum:
//                        //    break;

//                        #endregion

//                        #region Syntactic Pattern-Building

//                        /*
//                        We assume that the current executing environment contains bindings for pattern variables
//                        Including any elliptically-defined repeating patterns

//                        Our first step will be to identify and treat any free-variables in the template expression
//                        This means recurring through the entire expression and acting on symbols

//                        How do we handle the requisite bindings?
//                        [Enclosing Env]  <-- need to insert new bindings here
//                        >
//                        [Pattern Vars] <-- don't want to touch this if I can help it
//                        >
//                        [Free Vars]
//                         */

//                        // (syntax (pVars ...) template-expr)
//                        //case Label.Eval_Syntax:
//                        //    Argl = Exp.Caddr;
//                        //    Exp = Exp.Cadddr;
//                        //    OpStack.Push(Label.Dispatch_Build);
//                        //    OpStack.Push(Label.Dispatch_AlphaConvert);
//                        //    break;

//                        //case Label.Dispatch_Build:
//                        //    if (Exp is Symbol tVar)
//                        //    {
//                        //        Val = Env.LookUp(tVar);
//                        //    }
//                        //    else if (Exp.IsPair)
//                        //    {
//                        //        ExprStack.Push(Exp.Cdr);
//                        //        Exp = Exp.Car;
//                        //        OpStack.Push(Label.Build_Did_Car);
//                        //        OpStack.Push(Label.Dispatch_Build);
//                        //    }
//                        //    else if (Exp.IsEllipticPair)

//                        //    break;

//                        //case Label.Build_Did_Car:
//                        //    Exp = ExprStack.Pop();
//                        //    ExprStack.Push(Val);
//                        //    OpStack.Push(Label.Build_Did_Cdr);
//                        //    OpStack.Push(Label.Dispatch_Build);
//                        //    break;

//                        //case Label.Build_Did_Cdr:
//                        //    Exp = ExprStack.Pop();
//                        //    Val = Pair.Cons(Exp, Val);
//                        //    break;


//                        //case Label.Build_Empty:
//                        //    break;
//                        //case Label.Build_Identifier:
//                        //    break;
//                        //case Label.Build_Pair:
//                        //    break;
//                        //case Label.Build_Repeating:
//                        //    break;
//                        //case Label.Build_Datum:
//                        //    break;


//                        //case Label.Dispatch_AlphaConvert:
//                        //    if (Exp is Symbol fVar && !Pair.Memq(fVar, Argl)) //symbol not introduced by match pattern
//                        //    {
//                        //        Environment encl = Env.Enclosing; //"executing" environment
//                        //        Environment orig = Proc.AsMacro().Closure;

//                        //        if (encl.HasBound(fVar) && orig.HasBound(fVar)) //bound in both envs
//                        //        {
//                        //            if (!Expression.Pred_Eq(encl.LookUp(fVar), orig.LookUp(fVar))) //bound to different things
//                        //            {
//                        //                Symbol mimic = GenSym.ShadowExisting(fVar, encl);
//                        //                encl.BindNew(mimic, orig.LookUp(fVar));
//                        //                Env.BindNew(fVar, mimic);
//                        //            }
//                        //            else //bound to same thing, so no replacement necessary
//                        //            {
//                        //                Env.BindNew(fVar, fVar);
//                        //            }
//                        //            //else they mean the same thing, so who cares
//                        //        }
//                        //        else if (orig.HasBound(fVar)) //bound in macro env, but not locally (unlikely, but still)
//                        //        {
//                        //            encl.BindNew(fVar, orig.LookUp(fVar));
//                        //            Env.BindNew(fVar, fVar);
//                        //        }
//                        //        else if (encl.HasBound(fVar)) //bound locally, not in macro env
//                        //        {
//                        //            Symbol mimic = GenSym.ShadowExisting(fVar, encl);
//                        //            Env.BindNew(fVar, mimic);
//                        //        }
//                        //        else
//                        //        {
//                        //            Env.BindNew(fVar, fVar);
//                        //        }
//                        //    }
//                        //    else if (Exp.IsPair)
//                        //    {
//                        //        ExprStack.Push(Exp.Cdr);
//                        //        Exp = Exp.Car;
//                        //        OpStack.Push(Label.AlphaConvert_Continue);
//                        //        OpStack.Push(Dispatch_AlphaConvert);
//                        //    }
//                        //    //else we don't care about it
//                        //    //maybe, if it's a vector...?
//                        //    break;

//                        //case Label.AlphaConvert_Continue: //i.e. did car of pair
//                        //    Exp = ExprStack.Pop();
//                        //    OpStack.Push(Label.Dispatch_AlphaConvert);
//                        //    break;




//                        #endregion

//                        default:
//                            break;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {

//            }
//        }
//    }
//}
