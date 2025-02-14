using Clasp.Data.Metadata;
using Clasp.Data.Text;

namespace Clasp.Data.Terms.Syntax
{
    internal class Datum : Syntax
    {
        private readonly Term _datum;
        public override Term Expose() => _datum;

        public Datum(Term t, LexInfo ctx) : base(ctx) => _datum = t;

        public Datum(Term t, Syntax copy) : this(t, copy.LexContext) { }

        public Datum(Term t, Token source) : this(t, new LexInfo(source.Location)) { }

        public override string ToString() => string.Format("#'{0}", _datum);
        protected override string FormatType() => "Stx";
    }
}
