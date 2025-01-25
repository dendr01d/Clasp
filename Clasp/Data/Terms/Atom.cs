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


    internal class Nil : Atom
    {
        public static readonly Nil Value = new Nil();
        protected Nil() { }
        public override string ToString() => "'()";
        protected override string FormatType() => "Nil";
    }

    //internal sealed class Maybe<T> : Nil
    //    where T : Term
    //{
    //    private T? _value;

    //    public Maybe(T value) => _value = value;
    //    public Maybe(Nil _) => _value = null;
    //}

    internal sealed class Undefined : Atom
    {
        public static readonly Undefined Value = new Undefined();
        private Undefined() { }
        public override string ToString() => "#undefined";
        protected override string FormatType() => "Undefined";
    }
}
