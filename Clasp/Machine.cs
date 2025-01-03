
using System.Collections.Generic;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;

namespace Clasp
{
    internal class Machine
    {
        private Term _returnValue;
        private Environment _currentEnv;
        private List<Term> _accArgs;
        private Proc _currentProc;

        private Stack<Instruction> _continuation;
        private Stack<Environment> _envChain;
        private Stack<List<Term>> _pendingArgs;
        private Stack<Proc> _pendingProc;

        private int _instructionCounter;

        public bool ComputationComplete => _continuation.Count == 0;

        private Machine(AstNode evaluatee, Environment env)
        {
            _returnValue = Undefined.Value;
            _currentEnv = env;
            _accArgs = new List<Term>();

            _continuation = new Stack<Instruction>();
            _envChain = new Stack<Environment>();
            _pendingArgs = new Stack<List<Term>>();

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

        #region Instruction Dispatch

        private void DispatchOnInstruction(Instruction instr)
        {
            switch (instr)
            {
                case BindingDefinition bd:
                    _continuation.Push(new BindFresh(bd.VarName));
                    _continuation.Push(bd.BoundValue);
                    break;

                case BindingMutation bm:
                    _continuation.Push(new RebindExisting(bm.VarName));
                    _continuation.Push(bm.BoundValue);
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
                    _continuation.Push(new DispatchOnProcedure(fa.Args));
                    _continuation.Push(fa.OperatorExpression);
                    break;

                case FunctionCreation fc:
                    {
                        CompProc newCompoundProc = new CompProc(fc.Formals, new Environment(_currentEnv), fc.Body);
                        foreach (string informalParam in fc.Informals)
                        {
                            newCompoundProc.Closure.Add(informalParam, Undefined.Value);
                        }

                        _returnValue = newCompoundProc;
                    }
                    break;

                case ConditionalForm cf:
                    _continuation.Push(new DispatchOnCondition(cf.Consequent, cf.Alternate));
                    _continuation.Push(new RecallPreviousArgs());
                    _continuation.Push(new RecallPreviousEnv());
                    _continuation.Push(cf.Test);
                    _continuation.Push(new RememberCurrentEnv());
                    _continuation.Push(new RememberCurrentArgs());
                    break;

                case SequentialForm sf:
                    _continuation.Push(sf.Sequence[^1]); //last term in tail-position

                    foreach(AstNode node in sf.Sequence[..^1])
                    {
                        _continuation.Push(new RecallPreviousArgs());
                        _continuation.Push(new RecallPreviousEnv());
                        _continuation.Push(node);
                        _continuation.Push(new RememberCurrentEnv());
                        _continuation.Push(new RememberCurrentArgs());
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

                case RememberCurrentArgs:
                    _pendingArgs.Push(_accArgs);
                    break;

                case RecallPreviousArgs:
                    {
                        if (_pendingArgs.TryPop(out List<Term>? prevArgs))
                        {
                            _accArgs = prevArgs;
                        }
                        else
                        {
                            throw new ClaspException.Uncategorized("Tried to pop from pending args, but it was empty.");
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


                default:

                    break;
            }
        }

        #endregion

        #region Printing

        private const string STACK_SEP = "≤";
        private const string EMPTY_PTR = "ε";

        private string FormatRegister(Expression? x) => x?.ToString() ?? EMPTY_PTR;
        private string FormatFunctor(Action<Machine>? ptr) => ptr?.Method.Name ?? EMPTY_PTR;

        public string GoingTo => FormatFunctor(_goto);
        public string ContinueTo => FormatFunctor(_continue);

        public void Print(TextWriter tw)
        {
            tw.WriteLine($" Exp: {FormatRegister(_exp)}");
            tw.WriteLine($" Val: {FormatRegister(_val)}");
            tw.WriteLine($"Proc: {FormatRegister(_proc)}");
            tw.WriteLine($"Unev: {FormatRegister(_unev)}");
            tw.WriteLine($"Argl: {FormatRegister(_argl)}");
            if (_expStack.Any())
            {
                tw.WriteLine();
                tw.WriteLine("Term Stack:");
                foreach (Expression expr in _expStack)
                {
                    tw.WriteLine($"{STACK_SEP} {expr}");
                }
            }
            tw.WriteLine();
            tw.WriteLine($"Env w/ {_env.CountBindings()} defs");
            tw.WriteLine($"{_envStack.Count} stacked environments");
            tw.WriteLine();
            tw.WriteLine($"GoTo -> {FormatFunctor(_goto)}");
            tw.WriteLine($"Cont -> {FormatFunctor(_continue)}");
            if (_ptrStack.Any())
            {
                tw.WriteLine();
                tw.WriteLine("Ptr Stack:");
                foreach (Action<Machine>? ptr in _ptrStack)
                {
                    tw.WriteLine($"{STACK_SEP} {FormatFunctor(ptr)}");
                }
            }
        }

        #endregion

        #region Helpers

        public void GoTo_Continue() => Assign_GoTo(Continue);

        public void AppendArgl(Expression newArg)
        {
            Assign_Argl(Pair.Append(Argl, Pair.List(newArg)));
        }

        public void NextArgl() => Assign_Argl(Argl.Cdr);
        public void NextUnev() => Assign_Unev(Unev.Cdr);

        #endregion


    }
}
