using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.AST
{
    /// <summary>
    /// Represents operations that, when evaluated by their own specific semantics, will return a Fixed value.
    /// </summary>
    internal abstract class GenNode : AstNode { }

    /// <summary>
    /// Represents a Name (string) bound to a value in the environment. Evaluation results in dereferencing said value.
    /// </summary>
    internal sealed class Var : GenNode
    {
        public readonly string Name;
        public Var(string name) => Name = name;
        public override string ToString() => string.Format("VAR({0})", Name);
    }

    /// <summary>
    /// Represents a runtime construction operation that assembles a <see cref="CompProc"/>.
    /// </summary>
    internal sealed class Fun : GenNode
    {
        public readonly Symbol[] Parameters;
        public readonly AstNode[] Body;
        public Fun(Symbol[] parameters, AstNode[] body)
        {
            Parameters = parameters;
            Body = body;
        }
        public override string ToString() => string.Format("FUN({0} : {1})",
            string.Join(", ", Parameters.ToArray<object>()),
            string.Join(", ", Body.ToArray<object>()));
    }

    /// <summary>
    /// Represents a literal object that evaluates to itself.
    /// </summary>
    internal abstract class Fixed : GenNode { }
}
