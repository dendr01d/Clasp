using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.Product;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Data.Terms.Syntax
{
    internal abstract class Syntax : Term, ISourceTraceable
    {
        public readonly LexInfo LexContext;
        public SourceLocation Location => LexContext.Location;

        public abstract Term Expose();

        //TODO: Review the semantics of wrapping. DO scopes get merged, or replaced?
        //what happens if you try to wrap syntax itself?

        protected Syntax(LexInfo ctx)
        {
            LexContext = new LexInfo(ctx);
        }

        /// <summary>
        /// Perform a deep copy of this syntax and any of its syntactic sub-structure.
        /// Doesn't create copies of the normal data objects wrapped within.
        /// </summary>
        protected abstract Syntax DeepCopy();

        #region Static Constructors

        private static Syntax Wrap(Term term, SourceLocation loc)
        {
            LexInfo ctx = new LexInfo(loc);
            return Wrap(term, ctx);
        }

        private static Syntax Wrap(Term term, LexInfo ctx)
        {
            return term switch
            {
                Syntax s => s,
                Pair cl => new SyntaxPair(cl.Car, cl.Cdr, ctx),
                Symbol sym => new Identifier(sym, ctx),
                _ => new Datum(term, ctx)
            };
        }

        /// <summary>
        /// Create a syntax with blank scope derived from the given location.
        /// </summary>
        public static Syntax FromDatum(Term term, SourceLocation location) => Wrap(term, location);

        /// <summary>
        /// Create a syntax with blank scope corresponding to the location of the given token.
        /// </summary>
        public static Syntax FromDatum(Term term, Token token) => Wrap(term, token.Location);

        /// <summary>
        /// Create a syntax with the same scope and location as an existing syntax.
        /// </summary>
        public static Syntax FromDatum(Term term, Syntax stx) => Wrap(term, stx.LexContext);

        public static Syntax FromDatum(Term term, LexInfo ctx) => Wrap(term, ctx);


        public static Syntax FromSyntax(Syntax original) => original.DeepCopy();

        #endregion

        public virtual void AddScope(int phase, params uint[] scopeTokens) => LexContext.AddScope(phase, scopeTokens);
        public virtual void FlipScope(int phase, params uint[] scopeTokens) => LexContext.FlipScope(phase, scopeTokens);
        public virtual void RemoveScope(int phase, params uint[] scopeTokens) => LexContext.RemoveScope(phase, scopeTokens);

        /// <summary>
        /// Retrieve a live reference to the scope set at the given phase
        /// </summary>
        public IEnumerable<uint> GetScopeSet(int phase) => LexContext[phase];

        /// <summary>
        /// Strip all the syntactic info from this syntax's wrapped value. Recurs upon nested terms.
        /// </summary>
        /// <returns></returns>
        public Term ToDatum() => ToDatum(Expose());
        private static Term ToDatum(Term term)
        {
            if (term is Syntax stx)
            {
                return ToDatum(stx.Expose());
            }
            if (term is Pair cl)
            {
                return Pair.Cons(ToDatum(cl.Car), ToDatum(cl.Cdr));
            }
            else if (term is Vector vec)
            {
                return new Vector(vec.Values.Select(x => ToDatum(x)).ToArray());
            }
            else
            {
                return term;
            }
        }

        public abstract string ToSourceString();
    }
}
