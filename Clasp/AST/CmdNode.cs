using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.AST
{
    /// <summary>
    /// Represents operations that cause side-effects by mutating either objects or the environment. These
    /// always result in an output value of #undefined
    /// </summary>
    internal abstract class CmdNode : AstNode, IEffective { }

    /// <summary>
    /// Declares a NEW binding in the local environment, binding <see cref="Key"/> to <see cref="Value"/>.
    /// It's a runtime-error to evaluate a <see cref="BindFixed"/> in an environment where <see cref="Key"/> is already locally bound.
    /// </summary>
    internal sealed class BindFixed : CmdNode
    {
        public readonly Var Key;
        public readonly GenNode Value;
        public BindFixed(Var key, GenNode value)
        {
            Key = key;
            Value = value;
        }
        public override string ToString() => string.Format("BIND-FIXED({0}, {1})", Key, Value);
    }

    /// <summary>
    /// Declares a NEW binding in the local environment, binding <see cref="Key"/> to <see cref="Transformer"/>.
    /// It's a runtime-error to evaluate a <see cref="BindSyntax"/> in an environment where <see cref="Key"/> is already locally bound.
    /// </summary>
    internal sealed class BindSyntax : CmdNode
    {
        public readonly Var Key;
        public readonly GenNode Transformer;
        public BindSyntax(Var key, GenNode transformer)
        {
            Key = key;
            Transformer = transformer;
        }
        public override string ToString() => string.Format("BIND-SYNTAX({0}, {1})", Key, Transformer);
    }

    /// <summary>
    /// Mutates an EXISTING binding in the local environment, binding <see cref="Key"/> to <see cref="Transformer"/>.
    /// It's a runtime-error to evaluate a <see cref="SetFixed"/> in an environment where <see cref="Key"/> is NOT locally bound.
    /// </summary>
    internal sealed class SetFixed : CmdNode
    {
        public readonly Var Key;
        public readonly GenNode Value;
        public SetFixed(Var key, GenNode value)
        {
            Key = key;
            Value = value;
        }
        public override string ToString() => string.Format("SET-FIXED({0}, {1})", Key, Value);
    }
}
