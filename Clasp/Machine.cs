
using System.Collections.Generic;
using System.IO;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;

namespace Clasp
{
    internal class Machine
    {
        private Term _returnValue;
        private EnvFrame _currentEnv;

        private readonly Stack<Instruction> _continuation;
        private readonly Stack<EnvFrame> _envChain;

        private int _instructionCounter;

        public bool ComputationComplete => _continuation.Count == 0;

        private Machine(AstNode evaluatee, EnvFrame env)
        {
            _returnValue = Undefined.Value;
            _currentEnv = env;

            _continuation = new Stack<Instruction>();
            _envChain = new Stack<EnvFrame>();

            _instructionCounter = 0;

            _continuation.Push(evaluatee);
        }

        public bool Step()
        {
            if (!ComputationComplete)
            {
                Instruction nextInstruction = _continuation.Pop();

                DispatchOnInstruction(nextInstruction);
            }

            ++_instructionCounter;

            return !ComputationComplete;
        }

        public Term Compute()
        {
            while (Step()) { }

            return _returnValue;
        }

        #region Instruction Dispatch

        private void DispatchOnInstruction(Instruction instr)
        {
            switch (instr)
            {
                case BindingDefinition bd:
                    _continuation.Push(new BindFresh(bd.VarName));
                    ResetCurrentEnv();
                    _continuation.Push(bd.BoundValue);
                    break;

                case BindingMutation bm:
                    _continuation.Push(new RebindExisting(bm.VarName));
                    ResetCurrentEnv();
                    _continuation.Push(bm.BoundValue);
                    break;

                case VariableLookup vl:
                    {
                        if (_currentEnv.TryGetValue(vl.VarName, out Term? boundValue))
                        { _returnValue = boundValue; }
                        else
                        { throw new ClaspException.Uncategorized("Failed to look up binding for variable '{0}'.", vl.VarName); }
                    }
                    break;

                case ConstantValue cv:
                    _returnValue = cv.Value;
                    break;

                case Quotation quot:
                    _returnValue = quot.Value;
                    break;

                case FunctionApplication fa:
                    _continuation.Push(new DispatchOnOperator(fa.Args));
                    ResetCurrentEnv();
                    _continuation.Push(fa.Operator);
                    break;

                case FunctionCreation fc:
                    {
                        CompoundProcedure newProc = new(fc.Formals, fc.DottedFormal, new EnvFrame(_currentEnv), fc.Body);
                        //foreach (string informalParam in fc.Informals)
                        //{
                        //    newProc.CapturedEnv.Add(informalParam, Undefined.Value);
                        //}

                        _returnValue = newProc;
                    }
                    break;

                case ConditionalForm cf:
                    _continuation.Push(new DispatchOnCondition(cf.Consequent, cf.Alternate));
                    ResetCurrentEnv();
                    _continuation.Push(cf.Test);
                    break;

                case SequentialForm sf:
                    _continuation.Push(sf.Sequence[^1]); //last term in tail-position

                    foreach (AstNode node in sf.Sequence[..^1])
                    {
                        ResetCurrentEnv();
                        _continuation.Push(node);
                    }
                    break;

                // ------------------

                case BindFresh bf:
                    {
                        if (!_currentEnv.TryGetValue(bf.VarName, out Term? def) || def is Undefined)
                        {
                            _currentEnv[bf.VarName] = _returnValue;
                            _returnValue = Undefined.Value;
                        }
                        else
                        { throw new ClaspException.Uncategorized("Tried to re-define existing binding of variable '{0}'.", bf.VarName); }
                    }
                    break;

                case RebindExisting re:
                    {
                        if (_currentEnv.ContainsKey(re.VarName))
                        {
                            _currentEnv[re.VarName] = _returnValue;
                            _returnValue = Undefined.Value;
                        }
                        else
                        { throw new ClaspException.Uncategorized("Tried to mutate non-existent binding of variable '{0}'.", re.VarName); }
                    }
                    break;

                case DispatchOnCondition doc:
                    if (_returnValue != Boolean.False)
                    { _continuation.Push(doc.Consequent); }
                    else
                    { _continuation.Push(doc.Alternate); }
                    break;

                case DispatchOnOperator doo:
                    {
                        if (_returnValue is MacroProcedure macro)
                        {
                            throw new ClaspException.Uncategorized("Cannot evaluate macro-procedure as normal application: {0}", macro);
                        }
                        else if (_returnValue is not Procedure proc)
                        {
                            throw new ClaspException.Uncategorized("Tried to apply non-procedure: {0}", _returnValue);
                        }
                        else if (doo.Arguments.Length < proc.Arity)
                        {
                            throw new ClaspException.Uncategorized("Too few arguments provided for procedure: {0}", proc);
                        }
                        else if (doo.Arguments.Length > proc.Arity && !proc.IsVariadic)
                        {
                            throw new ClaspException.Uncategorized("Too many arguments provided for fixed-arity procedure: {0}", proc);
                        }
                        else if (proc is CompoundProcedure cp)
                        {
                            HandleCompoundProcedure(cp, doo.Arguments);
                        }
                        else if (proc is PrimitiveProcedure pp)
                        {
                            HandlePrimitiveProcedure(pp, doo.Arguments);
                        }
                        else
                        {
                            throw new ClaspException.Uncategorized("Tried to apply procedure of unknown type: {0}", proc);
                        }
                    }
                    break;

                case RollupVarArgs rva:
                    if (!rva.RollupStarted)
                    {
                        _continuation.Push(rva);
                        _continuation.Push(rva.UnevaluatedArgs.Pop());
                    }
                    else if (!rva.RollupFinished)
                    {
                        rva.EvaluatedArgs.Add(_returnValue);
                        _continuation.Push(rva);
                        _continuation.Push(rva.UnevaluatedArgs.Pop());
                    }
                    else
                    {
                        _returnValue = ConsList.ProperList(rva.EvaluatedArgs.ToArray());
                    }
                    break;

                case InvokePrimitiveProcedure ipp:
                    //idk
                    //use the operator + arity to look up the appropriate function to call
                    //then... call it?
                    break;

                default:
                    throw new ClaspException.Uncategorized("Unknown instruction type: {0}", instr);
                    //break;
            }
        }

        #endregion

        #region Printing

        //private const string STACK_SEP = "≤";
        //private const string EMPTY_PTR = "ε";

        public void Print(TextWriter tw)
        {
            foreach (Instruction instr in _continuation)
            {
                tw.WriteLine(instr);
            }


        }

        #endregion

        #region Helpers

        private void ResetCurrentEnv() => _continuation.Push(new SetCurrentEnv(_currentEnv));

        private void HandleCompoundProcedure(CompoundProcedure cp, AstNode[] args)
        {
            Environment closure = new EnvFrame(cp.CapturedEnv);

            _continuation.Push(cp.Body);

            int i = 0;
            for (; i < cp.Parameters.Length; ++i)
            {
                _continuation.Push(new BindingDefinition(cp.Parameters[i], args[i]));
            }

            if (cp.FinalParameter is not null)
            {
                if (args.Length >= i)
                {
                    _continuation.Push(new BindingDefinition(cp.FinalParameter, new RollupVarArgs(args[i..])));
                }
                else
                {
                    _continuation.Push(new BindFresh(cp.FinalParameter));
                    _continuation.Push(new ConstantValue(Nil.Value));
                }
            }

            // handle internal definitions here...?

            _continuation.Push(new SetCurrentEnv(closure));
        }

        private void HandlePrimitiveProcedure(PrimitiveProcedure pp, AstNode[] args)
        {
            _continuation.Push(new InvokePrimitiveProcedure(pp));
            _continuation.Push(new RollupVarArgs(args));
        }

        #endregion
    }
}