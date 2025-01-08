using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Data.Terms;

namespace Clasp.Data.AbstractSyntax
{
    internal abstract class EvFrame
    {
        public readonly Binding.Environment EvaluationEnv;
        public abstract void RunOnMachine(Stack<EvFrame> continuation, ref Term returnValue);

        protected EvFrame(Binding.Environment evalEnv)
        {
            EvaluationEnv = evalEnv;
        }

        public abstract override string ToString();
    }

    #region Binding Operations

    /// <summary>
    /// Bind the return value to the given name in the current environment.
    /// </summary>
    internal sealed class BindFresh : EvFrame
    {
        public string VarName { get; private init; }
        public BindFresh(Binding.Environment evalEnv, string key) : base(evalEnv)
        {
            VarName = key;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Term returnValue)
        {
            if (!EvaluationEnv.TryGetValue(VarName, out Term? def) || def is Undefined)
            {
                EvaluationEnv[VarName] = returnValue;
                returnValue = Undefined.Value;
            }
            else
            {
                throw new ClaspException.Uncategorized("Tried to re-define existing binding of variable '{0}'.", VarName);
            }
        }
        public override string ToString() => string.Format("*DEF({0}, [])", VarName);
    }

    /// <summary>
    /// Rebind the return value to the given name in the current environment.
    /// </summary>
    internal sealed class RebindExisting : EvFrame
    {
        public string VarName { get; private init; }
        public RebindExisting(Binding.Environment evalEnv, string key) : base(evalEnv)
        {
            VarName = key;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Term returnValue)
        {
            if (EvaluationEnv.ContainsKey(VarName))
            {
                EvaluationEnv[VarName] = returnValue;
                returnValue = Undefined.Value;
            }
            else
            { throw new ClaspException.Uncategorized("Tried to mutate non-existent binding of variable '{0}'.", VarName); }
        }
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
        public DispatchOnCondition(Binding.Environment evalEnv, AstNode consequent, AstNode alternate) : base(evalEnv)
        {
            Consequent = consequent;
            Alternate = alternate;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Term returnValue)
        {
            if (returnValue == Terms.Boolean.False)
            {
                continuation.Push(Alternate);
            }
            else
            {
                continuation.Push(Consequent);
            }
        }
        public override string ToString() => string.Format("*BRANCH([], {0}, {1})", Consequent, Alternate);
    }

    #endregion

    #region Function Application

    // - Evaluate the operator
    // - Branch based on whether the evaluated operator is primitive or compound:

    // - If Primitive:
    //      - Evaluate the arguments one by one, accumulating them into a list
    //      - Determine the desired arity of the primitive operation, then call the correct function on the items in the list

    // - If Compound:
    //      - Replace the current environment with (do not descend into) the closure of the compound procedure
    //      - Evaluate (Define) the arguments one by one, binding them according to the formal parameter name
    //      - The informal parameters may(?) already exist as undefined placeholders within the closure? Unsure...
    //      - Evaluate the sequential body
    //      - (No need to ascend out of the closure bc it's in tail position)

    internal sealed class DispatchOnOperator : EvFrame
    {
        public readonly AstNode[] Arguments;

        public DispatchOnOperator(Binding.Environment evalEnv, AstNode[] arguments) : base(evalEnv)
        {
            Arguments = arguments;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Term returnValue)
        {
            if (returnValue is MacroProcedure macro)
            {
                throw new ClaspException.Uncategorized("Cannot evaluate macro-procedure as normal application: {0}", macro);
            }
            else if (returnValue is not Procedure proc)
            {
                throw new ClaspException.Uncategorized("Tried to apply non-procedure: {0}", returnValue);
            }
            else if (Arguments.Length < proc.Arity)
            {
                throw new ClaspException.Uncategorized("Too few arguments provided for procedure: {0}", proc);
            }
            else if (Arguments.Length > proc.Arity && !proc.IsVariadic)
            {
                throw new ClaspException.Uncategorized("Too many arguments provided for fixed-arity procedure: {0}", proc);
            }
            else if (proc is CompoundProcedure cp)
            {
                Binding.Environment closure = new EnvFrame(cp.CapturedEnv);

                continuation.Push(cp.Body);

                int i = 0;
                for (; i < cp.Parameters.Length; ++i)
                {
                    continuation.Push(new BindingDefinition(closure, cp.Parameters[i], Arguments[i]));
                }

                if (cp.FinalParameter is not null)
                {

                    continuation.Push(new BindFresh(closure, cp.FinalParameter));

                    if (Arguments.Length >= i)
                    {
                        continuation.Push(new RollupVarArgs(Arguments[i..]));
                    }
                    else
                    {
                        continuation.Push(new Quotation(closure, Nil.Value));
                    }
                }

                // handle internal definitions here...?
            }
            else if (proc is PrimitiveProcedure pp)
            {
                continuation.Push(new InvokePrimitiveProcedure(pp));
                continuation.Push(new RollupVarArgs(Arguments));
            }
            else
            {
                throw new ClaspException.Uncategorized("Tried to apply procedure of unknown type: {0}", proc);
            }
        }
        public override string ToString() => string.Format("*APPL([]; {0}", string.Join(", ", Arguments.ToArray<object>()));
    }

    internal sealed class RollupVarArgs : EvFrame
    {
        public readonly Stack<AstNode> UnevaluatedArgs;
        public readonly List<Terms.Term> EvaluatedArgs;

        public bool RollupStarted => EvaluatedArgs.Count > 0;
        public bool RollupFinished => UnevaluatedArgs.Count == 0;

        public RollupVarArgs(AstNode[] args)
        {
            UnevaluatedArgs = new Stack<AstNode>(args.Reverse());
            EvaluatedArgs = new List<Terms.Term>();
        }

        public override string ToString()
        {
            return string.Format("VAR-ARGS({0}{1}{2}{3}{4})",
                string.Join(", ", EvaluatedArgs),
                EvaluatedArgs.Count > 0 ? ", " : string.Empty,
                "[]",
                UnevaluatedArgs.Count > 0 ? ", " : string.Empty,
                string.Join(", ", UnevaluatedArgs));
        }
    }

    internal sealed class InvokePrimitiveProcedure : EvFrame
    {
        public readonly Terms.PrimitiveProcedure Op;
        public InvokePrimitiveProcedure(Terms.PrimitiveProcedure op) => Op = op;
        public override string ToString() => string.Format("*APPL({0}; [])", Op);
    }

    #endregion
}
