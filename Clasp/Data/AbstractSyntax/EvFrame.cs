using System.Collections.Generic;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.Terms;

namespace Clasp.Data.AbstractSyntax
{
    internal abstract class EvFrame
    {
        public abstract void RunOnMachine(
            Stack<EvFrame> continuation,
            ref Environment currentEnv,
            ref Term currentValue);

        public abstract EvFrame CopyContinuation();

        public abstract override string ToString();
    }

    #region Binding Operations

    /// <summary>
    /// Bind the return value to the given name in the current environment.
    /// </summary>
    internal sealed class BindFresh : EvFrame
    {
        public string VarName { get; private init; }
        public BindFresh(string key) : base()
        {
            VarName = key;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Environment currentEnv, ref Term currentValue)
        {
            if (!currentEnv.TryGetValue(VarName, out Term? def) || def is Undefined)
            {
                currentEnv[VarName] = currentValue;
                currentValue = Undefined.Value;
            }
            else
            {
                throw new ClaspException.Uncategorized("Tried to re-define existing binding of variable '{0}'.", VarName);
            }
        }
        public override EvFrame CopyContinuation() => new BindFresh(VarName);
        public override string ToString() => string.Format("*DEF({0}, [])", VarName);
    }

    /// <summary>
    /// Rebind the return value to the given name in the current environment.
    /// </summary>
    internal sealed class RebindExisting : EvFrame
    {
        public string VarName { get; private init; }
        public RebindExisting(string key) : base()
        {
            VarName = key;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Environment currentEnv, ref Term currentValue)
        {
            if (currentEnv.ContainsKey(VarName))
            {
                currentEnv[VarName] = currentValue;
                currentValue = Undefined.Value;
            }
            else
            {
                throw new ClaspException.Uncategorized("Tried to mutate non-existent binding of variable '{0}'.", VarName);
            }
        }
        public override EvFrame CopyContinuation() => new RebindExisting(VarName);
        public override string ToString() => string.Format("*SET({0}, [])", VarName);

    }

    #endregion

    #region Branching

    /// <summary>
    /// Conditionally evaluate next either the consequent or alternate depending on the truthiness of the current value
    /// </summary>
    internal sealed class DispatchOnCondition : EvFrame
    {
        public readonly AstNode Consequent;
        public readonly AstNode Alternate;
        public DispatchOnCondition(AstNode consequent, AstNode alternate) : base()
        {
            Consequent = consequent;
            Alternate = alternate;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Environment currentEnv, ref Term currentValue)

        {
            if (currentValue == Boolean.False)
            {
                continuation.Push(Alternate);
            }
            else
            {
                continuation.Push(Consequent);
            }
        }
        public override EvFrame CopyContinuation() => new DispatchOnCondition(Consequent, Alternate);
        public override string ToString() => string.Format("*BRANCH([], {0}, {1})", Consequent, Alternate);
    }

    #endregion

    #region Function Application

    internal sealed class RollUpArguments : EvFrame
    {
        private static readonly System.Random _rng = new System.Random();

        public readonly AstNode[] Arguments;

        public RollUpArguments(AstNode[] arguments) : base()
        {
            Arguments = arguments;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Environment currentEnv, ref Term currentValue)
        {
            if (currentValue is MacroProcedure macro)
            {
                throw new ClaspException.Uncategorized("Cannot evaluate macro-procedure as normal application: {0}", macro);
            }
            else if (currentValue is not Procedure proc)
            {
                throw new ClaspException.Uncategorized("Tried to apply non-procedure: {0}", currentValue);
            }
            else if (Arguments.Length < proc.Arity)
            {
                throw new ClaspException.Uncategorized("Too few arguments provided for procedure: {0}", proc);
            }
            else if (Arguments.Length > proc.Arity && !proc.IsVariadic)
            {
                throw new ClaspException.Uncategorized("Too many arguments provided for fixed-arity procedure: {0}", proc);
            }
            else
            {
                continuation.Push(new ApplyProcedure(proc));

                if (Arguments.Length > 0)
                {
                    EvFrame unrolled = new ConstValue(Nil.Value);

                    foreach(AstNode arg in Arguments.Reverse().Skip(1))
                    {
                        unrolled = unrolled = new ArgumentSplitter(arg, unrolled, RandomBool());
                    }

                    continuation.Push(unrolled);
                }
            }
        }
        public override EvFrame CopyContinuation() => new RollUpArguments(Arguments.ToArray());
        public override string ToString() => string.Format("*APPL([]; {0}", string.Join(", ", Arguments.ToArray<object>()));

        private static bool RandomBool() => _rng.Next(2) == 0;
    }

    internal sealed class ArgumentSplitter : EvFrame
    {
        public readonly EvFrame Head;
        public readonly EvFrame Tail;
        public readonly bool HeadFirst;

        public ArgumentSplitter(EvFrame head, EvFrame tail, bool headFirst) : base()
        {
            Head = head;
            Tail = tail;
            HeadFirst = headFirst;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Environment currentEnv, ref Term currentValue)
        {
            if (HeadFirst)
            {
                continuation.Push(new ArgumentSwitcher(Tail, false));
                continuation.Push(Head);
            }
            else
            {
                continuation.Push(new ArgumentSwitcher(Head, true));
                continuation.Push(Tail);
            }
        }
        public override EvFrame CopyContinuation() => new ArgumentSplitter(Head, Tail, HeadFirst);
        public override string ToString() => string.Format("ROLL-ARGS({0}{1}, {2}{3})",
            Head, HeadFirst ? "º" : string.Empty,
            Tail, HeadFirst ? string.Empty : "º");
    }

    internal sealed class ArgumentSwitcher : EvFrame
    {
        public readonly EvFrame RemainingTerm;
        public readonly bool RemainingIsHead;

        public ArgumentSwitcher(EvFrame remaining, bool remainingFirst)
        {
            RemainingTerm = remaining;
            RemainingIsHead = remainingFirst;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new ArgumentAccumulator(currentValue, !RemainingIsHead));
            continuation.Push(RemainingTerm);
        }
        public override EvFrame CopyContinuation() => new ArgumentSwitcher(RemainingTerm, RemainingIsHead);
        public override string ToString() => string.Format("SWITCH-ARGS({0}, {1})",
            RemainingIsHead ? RemainingTerm : "[]",
            RemainingIsHead ? "[]" : RemainingTerm);
    }

    internal sealed class ArgumentAccumulator : EvFrame
    {
        public readonly Term CompletedTerm;
        public readonly bool CompletedIsHead;

        public ArgumentAccumulator(Term completed, bool completedFirst)
        {
            CompletedTerm = completed;
            CompletedIsHead = completedFirst;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Environment currentEnv, ref Term currentValue)
        {
            if (CompletedIsHead)
            {
                currentValue = ConsList.Cons(CompletedTerm, currentValue);
            }
            else
            {
                currentValue = ConsList.Cons(currentValue, CompletedTerm);
            }
        }
        public override EvFrame CopyContinuation() => new ArgumentAccumulator(CompletedTerm, CompletedIsHead);
        public override string ToString() => string.Format("ACC-ARGS({0}, {1})",
            CompletedIsHead ? CompletedTerm : "[]",
            CompletedIsHead ? "[]" : CompletedTerm);
    }

    internal sealed class ApplyProcedure : EvFrame
    {
        public readonly Procedure Op;
        public ApplyProcedure(Procedure op) => Op = op;
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Environment currentEnv, ref Term currentValue)
        {
            Term args = currentValue;

            if (currentValue is not ConsList)
            {
                throw new ClaspException.Uncategorized("Expected current value to be args list: {0}", currentValue);
            }

            if (Op is CompoundProcedure cp)
            {
                Environment closure = new EnvFrame(cp.CapturedEnv);

                continuation.Push(cp.Body);
                continuation.Push(new ChangeCurrentEnvironment(closure));

                for (int i = 0; i < cp.Parameters.Length; ++i)
                {
                    continuation.Push(new ConstValue((args as ConsList)!.Car));
                    continuation.Push(new BindFresh(cp.Parameters[i]));
                    continuation.Push(new ChangeCurrentEnvironment(closure));

                    args = (args as ConsList)!.Cdr;
                }

                if (cp.FinalParameter is not null)
                {
                    continuation.Push(new ConstValue(args));
                    continuation.Push(new BindFresh(cp.FinalParameter));
                    continuation.Push(new ChangeCurrentEnvironment(closure));
                }
            }
        }
        public override EvFrame CopyContinuation() => new ApplyProcedure(Op);
        public override string ToString() => string.Format("*APPL({0}; [])", Op);
    }

    #endregion

    internal sealed class ChangeCurrentEnvironment : EvFrame
    {
        public readonly Environment NewEnvironment;
        public ChangeCurrentEnvironment(Environment newEnv)
        {
            NewEnvironment = newEnv;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Environment currentEnv, ref Term currentValue)
        {
            currentEnv = NewEnvironment;
        }
        public override EvFrame CopyContinuation() => new ChangeCurrentEnvironment(NewEnvironment);
        public override string ToString() => string.Format("MOD-ENV()");
    }
}
