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
        public override string ToString() => string.Format("BIND({0})", VarName);
    }

    /// <summary>
    /// Rebind the current value to the given name in the current environment.
    /// </summary>
    internal sealed class RebindExisting : Instruction
    {
        public string VarName { get; private init; }
        public RebindExisting(string name) => VarName = name;
        public override string ToString() => string.Format("REBIND({0})", VarName);
    }

    #endregion

    #region Scope Management

    internal sealed class RememberCurrentEnv : Instruction
    {
        private RememberCurrentEnv() { }

        public static RememberCurrentEnv Instance = new RememberCurrentEnv();
        public override string ToString() => "MEM-ENV()";
    }
    internal sealed class RecallPreviousEnv : Instruction
    {
        private RecallPreviousEnv() { }

        public static readonly RecallPreviousEnv Instance = new RecallPreviousEnv();
        public override string ToString() => "POP-ENV()";
    }

    internal sealed class ReplaceCurrentEnv : Instruction
    {
        public readonly Binding.Environment NewEnv;

        public ReplaceCurrentEnv(Binding.Environment newEnv)
        {
            NewEnv = newEnv;
        }
        public override string ToString() => "NEW-ENV()";
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
        public override string ToString() => string.Format("DISP-COND({0}, {1})", Consequent, Alternate);
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


    internal sealed class AccumulateProcOp : Instruction
    {
        public readonly List<AstNode> UnevaluatedArgs;

        public AccumulateProcOp(IEnumerable<AstNode> unevaluatedArgs)
        {
            UnevaluatedArgs = unevaluatedArgs.ToList();
        }

        public override string ToString() => string.Format("ARGS({0})",
            string.Join(", ", UnevaluatedArgs));
    }

    internal sealed class AccumulateProcArgs : Instruction
    {
        public readonly Terms.Procedure Operator;
        public readonly List<Terms.Term> EvaluatedArgs;
        public readonly Stack<AstNode> UnevaluatedArgs;

        public AccumulateProcArgs(Terms.Procedure op, IEnumerable<AstNode> unevaluatedArgs)
        {
            Operator = op;
            UnevaluatedArgs = new Stack<AstNode>(unevaluatedArgs);
            EvaluatedArgs = new List<Terms.Term>();
        }

        public override string ToString() => string.Format("CALL({0}; {1}; {2})",
            Operator,
            string.Join(", ", EvaluatedArgs),
            string.Join(", ", UnevaluatedArgs));
    }

    internal sealed class InvokePrimitiveProcedure : Instruction
    {
        public readonly Terms.PrimitiveProcedure Op;
        public readonly List<Terms.Term> Args;

        public InvokePrimitiveProcedure(Terms.PrimitiveProcedure op, IEnumerable<Terms.Term> args)
        {
            Op = op;
            Args = args.ToList();
        }
        public override string ToString() => string.Format("CALL-PRIM({0})", Op);
    }

    internal sealed class InvokeCompoundProcedure : Instruction
    {
        public readonly Terms.CompoundProcedure Op;
        public readonly List<Terms.Term> Args;

        public InvokeCompoundProcedure(Terms.CompoundProcedure op, IEnumerable<Terms.Term> args)
        {
            Op = op;
            Args = args.ToList();
        }
        public override string ToString() => string.Format("CALL-COMP({0})", Op);
    }

    #endregion
}
