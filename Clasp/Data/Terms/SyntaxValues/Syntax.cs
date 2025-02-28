using System.Diagnostics;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Text;
using Clasp.ExtensionMethods;
using Clasp.Interfaces;

namespace Clasp.Data.Terms.SyntaxValues
{
    internal abstract class Syntax : Term, ISourceTraceable
    {
        public readonly LexInfo LexContext;
        public SourceCode Location => LexContext.Location;

        public abstract Term Expose();

        protected Syntax(LexInfo ctx)
        {
            LexContext = new LexInfo(ctx);
        }

        #region Static Constructors

        private static Syntax Wrap(Term term, LexInfo ctx)
        {
            return term switch
            {
                Syntax s => s,
                Cons cl => SyntaxList.Wrap(cl, ctx),
                Symbol sym => new Identifier(sym, ctx),
                _ => new Datum(term, ctx)
            };
        }

        /// <summary>
        /// Create a syntax with blank scope corresponding to the location of the given token.
        /// </summary>
        public static Syntax FromDatum(Term term, Token token) => Wrap(term, new LexInfo(token.Location));

        /// <summary>
        /// Create a syntax with the same scope and location as existing syntax.
        /// </summary>
        public static Syntax FromDatum(Term term, Syntax stx) => Wrap(term, stx.LexContext);

        /// <summary>
        /// Create a syntax with the provided lexical information.
        /// </summary>
        public static Syntax FromDatum(Term term, LexInfo ctx) => Wrap(term, ctx);

        #endregion

        public virtual void AddScope(int phase, params Scope[] scopes) => LexContext.AddScope(phase, scopes);
        public virtual void FlipScope(int phase, params Scope[] scopes) => LexContext.FlipScope(phase, scopes);
        public virtual void RemoveScope(int phase, params Scope[] scopes) => LexContext.RemoveScope(phase, scopes);

        /// <summary>
        /// Create a copy of this Syntax stripped of scope information at or above
        /// the level of <paramref name="inclusivePhaseThreshold"/>.
        /// </summary>
        public Syntax StripScopes(int inclusivePhaseThreshold)
        {
            LexInfo strippedContext = LexContext.RestrictPhaseUpTo(inclusivePhaseThreshold);
            return FromDatum(ToDatum(), strippedContext);
        }


        /// <summary>
        /// Strip all the syntactic info from this syntax's wrapped value. Recurs upon nested terms.
        /// </summary>
        public Term ToDatum() => ToDatum(Expose());
        private static Term ToDatum(Term term)
        {
            if (term is Syntax stx)
            {
                return ToDatum(stx.Expose());
            }
            if (term is Cons cl)
            {
                return Cons.Truct(ToDatum(cl.Car), ToDatum(cl.Cdr));
            }
            else if (term is Vector vec)
            {
                return new Vector(vec.Values.Select(ToDatum).ToArray());
            }
            else
            {
                return term;
            }
        }

        public override string ToString() => string.Format("#'{0}", Expose().ToString());

        public string ToSyntaxString()
        {
            return string.Format("#<syntax:{0}:{1}:{2} {3}>",
                LexContext.Location.Source,
                LexContext.Location.LineNumber,
                LexContext.Location.Column,
                ToString());
        }
    }
}
