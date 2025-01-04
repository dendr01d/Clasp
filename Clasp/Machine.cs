
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
        private Environment _currentEnv;

        private readonly Stack<Instruction> _continuation;
        private readonly Stack<Environment> _envChain;

        private int _instructionCounter;

        public bool ComputationComplete => _continuation.Count == 0;

        private Machine(AstNode evaluatee, Environment env)
        {
            _returnValue = Undefined.Value;
            _currentEnv = env;

            _continuation = new Stack<Instruction>();
            _envChain = new Stack<Environment>();

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
                    _continuation.Push(RecallPreviousEnv.Instance);
                    _continuation.Push(bd.BoundValue);
                    _continuation.Push(RememberCurrentEnv.Instance);
                    break;

                case BindingMutation bm:
                    _continuation.Push(new RebindExisting(bm.VarName));
                    _continuation.Push(RecallPreviousEnv.Instance);
                    _continuation.Push(bm.BoundValue);
                    _continuation.Push(RememberCurrentEnv.Instance);
                    break;

                case VariableLookup vl:
                    _returnValue = _currentEnv[vl.VarName];
                    break;

                case ConstantValue cv:
                    _returnValue = cv.Value;
                    break;

                case Quotation quot:
                    _returnValue = quot.Value;
                    break;

                case FunctionApplication fa:
                    _continuation.Push(new AccumulateProcOp(fa.Args));
                    _continuation.Push(RecallPreviousEnv.Instance);
                    _continuation.Push(fa.Operator);
                    _continuation.Push(RememberCurrentEnv.Instance);
                    break;

                case FunctionCreation fc:
                    {
                        CompoundProcedure newProc = new(fc.Formals, new Environment(_currentEnv), fc.Body);
                        foreach (string informalParam in fc.Informals)
                        {
                            newProc.CapturedEnv.Add(informalParam, Undefined.Value);
                        }

                        _returnValue = newProc;
                    }
                    break;

                case ConditionalForm cf:
                    _continuation.Push(new DispatchOnCondition(cf.Consequent, cf.Alternate));
                    _continuation.Push(RecallPreviousEnv.Instance);
                    _continuation.Push(cf.Test);
                    _continuation.Push(RememberCurrentEnv.Instance);
                    break;

                case SequentialForm sf:
                    _continuation.Push(sf.Sequence[^1]); //last term in tail-position

                    foreach (AstNode node in sf.Sequence[..^1])
                    {
                        _continuation.Push(RecallPreviousEnv.Instance);
                        _continuation.Push(node);
                        _continuation.Push(RememberCurrentEnv.Instance);
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
                        {
                            throw new ClaspException.Uncategorized("Tried to re-define existing binding of variable '{0}'.", bf.VarName);
                        }
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
                        {
                            throw new ClaspException.Uncategorized("Tried to mutate non-existent binding of variable '{0}'.", re.VarName);
                        }
                    }
                    break;

                case RememberCurrentEnv:
                    _envChain.Push(_currentEnv);
                    break;

                case RecallPreviousEnv:
                    {
                        if (_envChain.TryPop(out Environment? prevEnv))
                        {
                            _currentEnv = prevEnv;
                        }
                        else
                        {
                            throw new ClaspException.Uncategorized("Tried to pop from env chain, but it was empty.");
                        }
                    }
                    break;


                case DispatchOnCondition doc:
                    if (_returnValue != Boolean.False)
                    {
                        _continuation.Push(doc.Consequent);
                    }
                    else
                    {
                        _continuation.Push(doc.Alternate);
                    }
                    break;

                case AccumulateProcOp apo:
                    {
                        if (_returnValue is Procedure proc)
                        {
                            AccumulateProcArgs rollup = new(proc, apo.UnevaluatedArgs);
                            _continuation.Push(rollup);

                            if (rollup.UnevaluatedArgs.Count > 0)
                            {
                                _continuation.Push(RecallPreviousEnv.Instance);
                                _continuation.Push(rollup.UnevaluatedArgs.Pop());
                                _continuation.Push(RememberCurrentEnv.Instance);
                            }
                        }
                        else
                        {
                            throw new ClaspException.Uncategorized("Tried to apply non-procedure: {0}", _returnValue);
                        }
                    }
                    break;

                case AccumulateProcArgs apa:
                    {
                        apa.EvaluatedArgs.Add(_returnValue);

                        if (apa.UnevaluatedArgs.Count > 0)
                        {
                            _continuation.Push(apa);
                            _continuation.Push(RecallPreviousEnv.Instance);
                            _continuation.Push(apa.UnevaluatedArgs.Pop());
                            _continuation.Push(RememberCurrentEnv.Instance);
                        }
                        else if (apa.Operator is PrimitiveProcedure primProc)
                        {
                            _continuation.Push(new InvokePrimitiveProcedure(primProc, apa.EvaluatedArgs));
                        }
                        else if (apa.Operator is CompoundProcedure compProc)
                        {
                            _continuation.Push(new InvokeCompoundProcedure(compProc, apa.EvaluatedArgs));
                        }
                        else
                        {
                            throw new ClaspException.Uncategorized("Finished accumulating args, but unknown op type: {0}", apa.Operator);
                        }
                    }
                    break;

                case InvokePrimitiveProcedure ipp:
                    //idk
                    //use the operator + arity to look up the appropriate function to call
                    //then... call it?
                    break;

                case InvokeCompoundProcedure icp:
                    {
                        _continuation.Push(icp.Op.Body);

                        int index = 0;
                        for (; index > icp.Op.Parameters.Length; ++index)
                        {
                            if (index > icp.Args.Count)
                            {
                                throw new ClaspException.Uncategorized("Too few arguments provided for compound proc: {0}", icp.Op);
                            }
                            else
                            {
                                _continuation.Push(new BindingDefinition(icp.Op.Parameters[index], icp.Args[index]));
                            }
                        }

                        if (icp.Op.FinalParameter is not null)
                        {
                            _continuation.Push(new BindFresh(icp.Op.FinalParameter));

                            if (index >= icp.Args.Count)
                            {
                                _continuation.Push(new ConstantValue(Nil.Value));
                            }
                            else
                            {
                                _continuation.Push(new Quotation(ConsList.ProperList(icp.Args[index..].ToArray())));
                            }
                        }
                        else if (icp.Args.Count > index)
                        {
                            throw new ClaspException.Uncategorized("Too many arguments provided for compound proc: {0}", icp.Op);
                        }

                        _continuation.Push(new ReplaceCurrentEnv(new Environment(icp.Op.CapturedEnv)));
                    }
                    break;


                default:

                    break;
            }
        }

        #endregion

        #region Printing

        private const string STACK_SEP = "≤";
        private const string EMPTY_PTR = "ε";

        public void Print(TextWriter tw)
        {
            foreach (Instruction instr in _continuation)
            {
                tw.WriteLine(instr);
            }


        }

        #endregion
    }
}
