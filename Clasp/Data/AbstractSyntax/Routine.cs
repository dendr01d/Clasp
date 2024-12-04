using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.AbstractSyntax
{
    /// <summary>
    /// A "routine" node. Represents an operation composed of smaller operations carried out in a particular order.
    /// May cause side-effects by dint of the sub-components potentially causing side-effects.
    /// </summary>
    internal abstract class Routine : Generative { }

    /// <summary>
    /// Corresponds to an If-Then-Else conditional evaluation structure. Depending on the truthiness of the evaluated <see cref="Test"/>,
    /// either <see cref="Consequent"/> or <see cref="Alternate"/> will be evaluated in tail position.
    /// </summary>
    internal sealed class Branch : Routine
    {
        public readonly Generative Test;
        public readonly Generative Consequent;
        public readonly Generative Alternate;
        public Branch(Generative test, Generative consequent, Generative alternate)
        {
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }
        public override string ToString() => string.Format("BRANCH({0}, {1}, {2})", Test, Consequent, Alternate);
    }

    /// <summary>
    /// Represents a series of nodes to be evaluated in sequence (for the sake of their side-effects) followed by a final node
    /// that is evaluated in tail position.
    /// </summary>
    internal sealed class Sequence : Routine
    {
        public readonly AstNode[] Series;
        public readonly Generative Final;
        public Sequence(AstNode[] series, Generative final)
        {
            Series = series;
            Final = final;
        }
        public override string ToString() => string.Format("SEQUENCE({0}, {1})", string.Join(", ", Series.ToString(), Final));
    }

    /// <summary>
    /// Represents the application of procedures to a number of input arguments.
    /// </summary>
    internal sealed class Appl : Routine
    {
        public readonly Generative Op;
        public readonly Generative[] Args;
        public Appl(Generative op, params Generative[] args)
        {
            Op = op;
            Args = args;
        }
        public override string ToString() => string.Format(
            "APPL({0}, {1})",
            Op,
            string.Join(", ", Args.ToArray<object>()));
    }
}
