using System.Collections.Generic;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Data.Terms.SyntaxValues
{
    /// <summary>
    /// A syntactic value representing an arbitrary Clasp object.
    /// </summary>
    internal class Datum : Syntax
    {
        private readonly Term _datum;
        public override Term Expose() => _datum;

        public static Datum NullSyntax() => new Datum(Nil.Value, SourceCode.StaticSource);

        public Datum(Term t, SourceCode loc) : base(loc) => _datum = t;

        public override void AddScope(int phase, params Scope[] scopes) { }
        public override void FlipScope(int phase, params Scope[] scopes) { }
        public override void RemoveScope(int phase, params Scope[] scopes) { }
        public override Syntax StripScopes(int inclusivePhaseThreshold) => this;
        public override ScopeSet GetScopeSet() => ScopeSet.Empty;

        protected override string FormatType() => string.Format("StxDatum<{0}, {1}>", _datum.TypeName);
    }
}
