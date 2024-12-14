using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clasp.Data.ConcreteSyntax;
using Clasp.Data.Terms;
using Clasp.Interfaces;

namespace Clasp.Data.AbstractSyntax
{
    /// <summary>
    /// Represents operations that, when evaluated by their own specific semantics, will return a Fixed value.
    /// </summary>
    internal abstract class Generative : AstNode { }

    /// <summary>
    /// Represents a Name (string) bound to a value in the environment. Evaluation results in dereferencing said value.
    /// </summary>
    internal sealed class Variable : Generative, IBindable
    {
        public string Name { get; private init; }
        public Variable(string name) => Name = name;
        public override string ToString() => string.Format("VAR({0})", Name);
    }

    /// <summary>
    /// Represents a runtime construction operation that assembles a <see cref="CompProc"/>.
    /// </summary>
    /// <remarks>This object is the lambda invocation itself, NOT the resulting <see cref="CompProc"/> it constructs.</remarks>
    internal sealed class Functional : Generative
    {
        public readonly Variable[] Formals;
        public readonly Variable[] Informals;
        public readonly Variable[] Incidentals;
        public readonly Sequence Body;
        public Functional(Variable[] parameters, Sequence body)
        {
            Formals = parameters;
            Body = body;
        }
        public override string ToString() => string.Format("FUN({0}; {1})",
            string.Join(", ", Formals.ToArray<object>()),
            string.Join(", ", Body));
    }

    internal sealed class Macro : Generative
    {
        public readonly Variable Formal;
        public readonly Variable[] Informals;
        public readonly Sequence Body;

        public Macro(Variable parameter, Sequence body)
        {
            Formal = parameter;
            Body = body;
        }
        public override string ToString() => string.Format("MACRO({0}; {1})",
            Formal,
            string.Join(", ", Body));
    }

    /// <summary>
    /// Represents a literal object that evaluates to itself.
    /// </summary>
    internal class Fixed : Generative
    {
        public readonly Term Value;
        public Fixed(Term value) => Value = value;
        public override string ToString() => string.Format("LIT({0})", Value);

        public static implicit operator Term(Fixed f) => f.Value;
    }
}
