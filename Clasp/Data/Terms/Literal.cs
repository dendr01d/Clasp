using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Text;

namespace Clasp.Data.Terms
{
    internal abstract class Literal<T> : Atom
    {
        public readonly T Value;
        protected Literal(T value) => Value = value;
        public override string ToString() => Value?.ToString() ?? "#?";
    }
}
