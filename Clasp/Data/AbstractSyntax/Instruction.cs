using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Net.Mime.MediaTypeNames;

namespace Clasp.Data.AbstractSyntax
{
    internal abstract class Instruction
    {
        public abstract override string ToString();
    }

    #region Binding Operations

    /// <summary>
    /// Bind the current value to the given name in the current environment.
    /// </summary>
    internal sealed class BindFresh : Instruction
    {
        public string VarName { get; private init; }
        public BindFresh(string name) => VarName = name;
        public override string ToString() => string.Format("*DEF({0}, [])", VarName);
    }

    /// <summary>
    /// Rebind the current value to the given name in the current environment.
    /// </summary>
    internal sealed class RebindExisting : Instruction
    {
        public string VarName { get; private init; }
        public RebindExisting(string name) => VarName = name;
        public override string ToString() => string.Format("*SET({0}, [])", VarName);
    }

    #endregion

    #region Scope Management

    //internal sealed class RememberCurrentEnv : Instruction
    //{
    //    private RememberCurrentEnv() { }

    //    public static RememberCurrentEnv Instance = new RememberCurrentEnv();
    //    public override string ToString() => "MEM-ENV()";
    //}
    //internal sealed class RecallPreviousEnv : Instruction
    //{
    //    private RecallPreviousEnv() { }

    //    public static readonly RecallPreviousEnv Instance = new RecallPreviousEnv();
    //    public override string ToString() => "POP-ENV()";
    //}

    internal sealed class SetCurrentEnv : Instruction
    {
        public readonly Binding.Environment NewEnv;

        public SetCurrentEnv(Binding.Environment newEnv)
        {
            NewEnv = newEnv;
        }
        public override string ToString() => "SET-ENV()";
    }

    #endregion

    #region Branching

    /// <summary>
    /// Conditionally evaluate next either the consequent or alternate depending on the truthiness of the current value
    /// </summary>
    internal sealed class DispatchOnCondition : Instruction
    {
        public readonly AstNode Consequent;
        public readonly AstNode Alternate;
        public DispatchOnCondition(AstNode consequent, AstNode alternate)
        {
            Consequent = consequent;
            Alternate = alternate;
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

    internal sealed class DispatchOnOperator : Instruction
    {
        public readonly AstNode[] Arguments;

        public DispatchOnOperator(AstNode[] arguments)
        {
            Arguments = arguments;
        }

        public override string ToString() => string.Format("*APPL([]; {0}", string.Join(", ", Arguments.ToArray<object>()));
    }

    internal sealed class RollupVarArgs : Instruction
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

    internal sealed class InvokePrimitiveProcedure : Instruction
    {
        public readonly Terms.PrimitiveProcedure Op;
        public InvokePrimitiveProcedure(Terms.PrimitiveProcedure op) => Op = op;
        public override string ToString() => string.Format("*APPL({0}; [])", Op);
    }

    #endregion
}
