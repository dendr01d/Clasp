using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Metadata;

namespace Clasp.Data.Terms.Syntax
{
    internal class Datum : Syntax
    {
        private Term _datum;
        public override Term Expose() => _datum;

        public Datum(Term t, SourceLocation loc, Syntax? copy = null)
            : base(loc, copy)
        {
            _datum = t;
        }

        public Datum(Term t, Syntax copy) : this(t, copy.Location, copy) { }

        protected override Datum DeepCopy() => new Datum(_datum, Location, this);

        public override string ToString() => string.Format("#'{0}", _datum);
        protected override string FormatType() => "stx";
    }
}
