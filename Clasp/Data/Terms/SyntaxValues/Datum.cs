using System.Collections.Generic;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Text;

namespace Clasp.Data.Terms.SyntaxValues
{
    internal class Datum : Syntax
    {
        private readonly Term _datum;
        public override Term Expose() => _datum;

        public static Datum NilDatum => new Datum(Nil.Value, SourceCode.StaticSource);

        public Datum(Term t, SourceCode loc) : base(loc) => _datum = t;

        public override void AddScope(int phase, params Scope[] scopes) { }
        public override void FlipScope(int phase, params Scope[] scopes) { }
        public override void RemoveScope(int phase, params Scope[] scopes) { }
        public override Syntax StripScopes(int inclusivePhaseThreshold) => this;
        public override ScopeSet GetScopes() => ScopeSet.Empty;

        public override SyntaxPair ListPrepend(Syntax stx) => new SyntaxPair(stx, this, Location, new ScopeSet());

        protected override string FormatType() => string.Format("StxDatum<{0}, {1}>", _datum.TypeName);
    }
}
