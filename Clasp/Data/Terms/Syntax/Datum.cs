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

        public Datum(Term t, LexInfo ctx) : base(ctx)
        {
            _datum = t;
        }

        public Datum(Term t, Syntax copy) : this(t, copy.LexContext) { }

        public static Datum Implicit(Term t) => new Datum(t, new LexInfo(SourceLocation.Innate));

        protected override Datum DeepCopy() => new Datum(_datum, this);

        public override string ToString() => string.Format("#'{0}", _datum);
        protected override string FormatType() => "stx";
    }
}
