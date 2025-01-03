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

    internal sealed class RememberCurrentEnv() : Instruction { public override string ToString() => "MEM-ENV()"; }
    internal sealed class RecallPreviousEnv() : Instruction { public override string ToString() => "POP-ENV()"; }

    internal sealed class RememberCurrentArgs() : Instruction { public override string ToString() => "MEM-ARGS()"; }
    internal sealed class RecallPreviousArgs() : Instruction { public override string ToString() => "POP-ARGS()"; }

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


    /// <summary>
    /// Dispatch the correct instruction depending on if the current value is a primitive or compound procedure.
    /// </summary>
    internal sealed class DispatchOnProcedure : Instruction
    {
        public readonly AstNode[] Args;
        public DispatchOnProcedure(params AstNode[] args) => Args = args;
        public override string ToString() => string.Format("DISP-PROC({0})", Args);
    }

    internal sealed class AccumulateArgument : Instruction
    {
        public AccumulateArgument() { }
        public override string ToString() => "ACC()";
    }

    internal sealed class EnqueuePrimitiveProcedure : Instruction
    {
        public readonly Terms.PrimProc Op;
        public readonly AstNode[] Args;

        public EnqueuePrimitiveProcedure(Terms.PrimProc op, params AstNode[] args)
        {
            Op = op;
            Args = args;
        }
        public override string ToString() => string.Format("PUSH-PRIM({0}, {1})", Op, Args);
    }

    internal sealed class EnqueueCompoundProcedure : Instruction
    {
        public readonly Terms.CompProc Op;
        public readonly AstNode[] Args;

        public EnqueueCompoundProcedure(Terms.CompProc op, params AstNode[] args)
        {
            Op = op;
            Args = args;
        }

        public override string ToString() => string.Format("PUSH-COMP({0}, {1})", Op, Args);
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
}
