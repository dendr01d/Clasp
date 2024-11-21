using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.HeapMachine
{
    internal abstract record Instruction(string OpName, string? DebugInfo) { }

    #region Instruction Types

    internal sealed record PushConst(Expression ConstantValue, Instruction Next, string? DebugInfo = null)
        : Instruction("PUSH_CONST", DebugInfo)
    { }

    internal sealed record LexGet(Symbol VarName, Instruction Next, string? DebugInfo = null)
        : Instruction("LEX_GET", DebugInfo)
    { }

    internal sealed record LexSet(Symbol VarName, Instruction Next, string? DebugInfo = null)
        : Instruction("LEX_SET", DebugInfo)
    { }

    internal sealed record GlobGet(Symbol VarName, Instruction Next, string? DebugInfo = null)
        : Instruction("GLOB_GET", DebugInfo)
    { }

    internal sealed record GlobSet(Symbol VarName, Instruction Next, string? DebugInfo = null)
        : Instruction("GLOB_SET", DebugInfo)
    { }

    internal sealed record Branch(Instruction Then, Instruction Else, string? DebugInfo = null)
        : Instruction("BRANCH", DebugInfo)
    { }

    internal sealed record Conti(Instruction Next, string? DebugInfo = null)
        : Instruction("CONTI", DebugInfo)
    { }

    internal sealed record Nuate(Expression Continuation, Symbol ReturnVar, string? DebugInfo = null)
        : Instruction("NUATE", DebugInfo)
    { }

    internal sealed record CallFrame(Instruction Next, Instruction Return, string? DebugInfo = null)
        : Instruction("FRAME", DebugInfo)
    { }

    internal sealed record AccArg(Instruction Next, string? DebugInfo = null)
        : Instruction("ARG", DebugInfo)
    { }

    internal sealed record Apply(string? DebugInfo = null)
        : Instruction("APPLY", DebugInfo)
    { }

    internal sealed record Return(string? DebugInfo = null)
        : Instruction("RETURN", DebugInfo)
    { }

    internal sealed record Halt(string? DebugInfo = null)
        : Instruction("HALT", DebugInfo)
    { }


    #endregion
}
