using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Terms
{
    internal sealed class Boolean : Term
    {
        public readonly bool Value;

        public static readonly Boolean True = new Boolean(true);
        public static readonly Boolean False = new Boolean(false);
        private Boolean(bool b) => Value = b;
        public override string ToString() => Value ? "#t" : "#f";
        protected override string FormatType() => "Bool";

        public static implicit operator Boolean(bool b) => b ? True : False;
        public static implicit operator bool(Boolean b) => b != False;
    }
}
