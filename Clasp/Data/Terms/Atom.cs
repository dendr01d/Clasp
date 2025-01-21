using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Terms
{
    /// <summary>
    /// Represents an irreducible value. 
    /// </summary>
    internal abstract class Atom : Term { }


    internal sealed class Nil : Atom
    {
        public static readonly Nil Value = new Nil();
        private Nil() { }
        public override string ToString() => "'()";
        protected override string FormatType() => "nil";
    }

    internal sealed class Undefined : Atom
    {
        public static readonly Undefined Value = new Undefined();
        private Undefined() { }
        public override string ToString() => "#undefined";
        protected override string FormatType() => "undefined";
    }
}
