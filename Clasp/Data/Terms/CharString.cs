using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Terms
{
    internal sealed class CharString : Term
    {
        public readonly string Value;

        public CharString(string s) => Value = s;
        public override string ToString() => string.Format("\"{0}\"", Value);
        protected override string FormatType() => "String";
    }
}
