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

        /// <summary>
        /// Wrap a <see cref="Term"/> with syntactic info corresponding to <see cref="SourceCode.StaticSource"/>
        /// and an empty scope set;
        /// </summary>
        public static Syntax WrapRaw(Term t, SourceCode? loc = null)
        {
            return t switch
            {
                Syntax stx => stx,
                Cons cns => new SyntaxPair(cns.Car, cns.Cdr, loc ?? SourceCode.StaticSource),
                Symbol sym => new Identifier(sym, loc ?? SourceCode.StaticSource),
                _ => new Datum(t, loc ?? SourceCode.StaticSource)
            };
        }

        /// <summary>
        /// Wrap a <see cref="Term"/> with syntactic info duplicating that of <paramref name="stx"/>.
        /// </summary>
        public static Syntax WrapWithRef(Term t, Syntax stx)
        {
            Syntax output = WrapRaw(t);

            // If the input term was already syntax, don't do anything to it.
            if (!ReferenceEquals(t, output))
            {
                output.Location = stx.Location;
                output.SetScopes(stx.GetScopeSet());
            }

            return output;
        }

        #region Scope Adjustment

        public abstract void AddScope(int phase, params Scope[] scopes);
        public abstract void FlipScope(int phase, params Scope[] scopes);
        public abstract void RemoveScope(int phase, params Scope[] scopes);

        /// <summary>
        /// Create a copy of this Syntax stripped of scope information at or above the level of <paramref name="inclusivePhaseThreshold"/>.
        /// </summary>
        public abstract Syntax StripScopes(int inclusivePhaseThreshold);

        public abstract ScopeSet GetScopeSet();
        private void SetScopes(ScopeSet original)
        {
            foreach(var phasedScopes in original.Enumerate())
            {
                AddScope(phasedScopes.Key, phasedScopes.Value);
            }
        }

        #endregion

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
