using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Static;
using Clasp.Data.Terms;

namespace Clasp.Data.Abstractions.SpecialForms
{
    /// <summary>
    /// Represents a process by which an <see cref="AbstractProgram"/> series
    /// is executed one-by-one.
    /// </summary>
    class Sequence : AbstractSpecialForm
    {
        public readonly AbstractProgram[] Sequents;

        public Sequence(params AbstractProgram[] sequents) => Sequents = sequents;

        public override string ToString()
        {
            return $"(begin {string.Join(' ', Sequents.Select(x => x.Express()))})";
        }
        public override ITerm Express()
        {
            return Cons.List(Symbols.S_Begin, Sequents.Select(x => x.Express()).Append(new Nil()).ToArray());
        }
    }
}
