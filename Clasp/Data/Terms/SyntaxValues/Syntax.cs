using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Data.Terms.SyntaxValues
{
    internal abstract class Syntax : Term, ISourceTraceable
    {
        public SourceCode Location { get; private set; }
        public abstract Term Expose();

        protected Syntax(SourceCode loc)
        {
            Location = loc;
        }

        public static Syntax FromTerm(Term t, SourceCode loc, ScopeSet? scopes = null)
        {
            return t switch
            {
                Syntax stx => stx,
                Cons cns => new SyntaxPair(cns.Car, cns.Cdr, loc, scopes ?? new ScopeSet()),
                Symbol sym => new Identifier(sym, loc, scopes ?? new ScopeSet()),
                _ => new Datum(t, loc)
            };
        }

        #region Scope Adjustment

        public abstract void AddScope(int phase, params Scope[] scopes);
        public abstract void FlipScope(int phase, params Scope[] scopes);
        public abstract void RemoveScope(int phase, params Scope[] scopes);
        /// <summary>
        /// Create a copy of this Syntax stripped of scope information at or above the level of <paramref name="inclusivePhaseThreshold"/>.
        /// </summary>
        public abstract Syntax StripScopes(int inclusivePhaseThreshold);
        public abstract ScopeSet GetScopes();

        #endregion

        public abstract SyntaxPair ListPrepend(Syntax stx);

        public override string ToString() => string.Format("#'{0}", Expose().ToString());
        public string ToSyntaxString()
        {
            return string.Format("#<syntax:{0}:{1}:{2} {3}>",
                Location.Source,
                Location.LineNumber,
                Location.Column,
                ToString());
        }
    }
}
