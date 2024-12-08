using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clasp.Data.ConcreteSyntax;

namespace Clasp.Data.AbstractSyntax
{
    /// <summary>
    /// Represents operations that, when evaluated by their own specific semantics, will return a Fixed value.
    /// </summary>
    internal abstract class Generative : AstNode { }

    /// <summary>
    /// Represents a Name (string) bound to a value in the environment. Evaluation results in dereferencing said value.
    /// </summary>
    internal sealed class Var : Generative
    {
        public readonly string Name;
        public Var(string name) => Name = name;
        public override string ToString() => string.Format("VAR({0})", Name);
    }

    /// <summary>
    /// Represents a runtime construction operation that assembles a <see cref="CompProc"/>.
    /// </summary>
    /// <remarks>This object is the lambda invocation itself, NOT the resulting <see cref="CompProc"/> it constructs.</remarks>
    internal sealed class Fun : Generative
    {
        public readonly Var[] Parameters;
        public readonly Sequence Body;
        public Fun(Var[] parameters, Sequence body)
        {
            Parameters = parameters;
            Body = body;
        }
        public override string ToString() => string.Format("FUN({0}; {1})",
            string.Join(", ", Parameters.ToArray<object>()),
            string.Join(", ", Body));
    }

    /// <summary>
    /// Represents a literal object that evaluates to itself.
    /// </summary>
    internal sealed class Fixed : Generative
    {
        public readonly Terms.Term Value;
        public Fixed(Terms.Term value) => Value = value;
        public override string ToString() => string.Format("LIT({0})", Value);
    }
}
