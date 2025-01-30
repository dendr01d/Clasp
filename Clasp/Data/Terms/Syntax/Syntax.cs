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
        public SourceLocation Location { get; private set; }

        private readonly Dictionary<int, HashSet<uint>> _phasedScopeSets;

        public abstract Term Expose();

        //TODO: Review the semantics of wrapping. DO scopes get merged, or replaced?
        //what happens if you try to wrap syntax itself?

        protected Syntax(SourceLocation loc, Syntax? copy)
        {
            Location = loc;
            _phasedScopeSets = new Dictionary<int, HashSet<uint>>(copy?._phasedScopeSets ?? []);
        }

        // Too complicated to deal with lazy initialization rn
        //private Syntax(Func<Term> thunk, SourceLocation loc, Syntax? copy)
        //{
        //    _lazyWrapped = new Lazy<Term>(thunk);
        //    Location = loc;

        //    MultiScope = copy?.MultiScope ?? new MultiScope();
        //    Properties = copy?.Properties ?? new HashSet<string>();
        //}

        /// <summary>
        /// Perform a deep copy of this syntax and any of its syntactic sub-structure.
        /// Doesn't create copies of the normal data objects wrapped within.
        /// </summary>
        protected abstract Syntax DeepCopy();

        #region Static Constructors

        private static Syntax FromDatum(Term term, SourceLocation loc, Syntax? copy)
        {
            return term switch
            {
                Syntax s => s,
                ConsList cl => new SyntaxPair(cl, loc, copy),
                Symbol sym => new Identifier(sym, loc, copy),
                _ => new Datum(term, loc, copy)
            };
        }

        /// <summary>
        /// Create a syntax with blank scope derived from the given location.
        /// </summary>
        public static Syntax FromDatum(Term term, SourceLocation location) => FromDatum(term, location, null);

        /// <summary>
        /// Create a syntax with blank scope corresponding to the location of the given token.
        /// </summary>
        public static Syntax FromDatum(Term term, Token token) => FromDatum(term, token.Location, null);

        /// <summary>
        /// Create a syntax with the same scope and location as an existing syntax.
        /// </summary>
        public static Syntax FromDatum(Term term, Syntax existingSyntax)
        {
            return FromDatum(term, existingSyntax.Location.Derivation(), existingSyntax);
        }

        public static Syntax FromSyntax(Syntax original) => original.DeepCopy();

        #endregion

        public virtual void AddScope(int phase, params uint[] scopeTokens) => GetScopeSet(phase).UnionWith(scopeTokens);
        public virtual void FlipScope(int phase, params uint[] scopeTokens) => GetScopeSet(phase).SymmetricExceptWith(scopeTokens);
        public virtual void RemoveScope(int phase, params uint[] scopeTokens) => GetScopeSet(phase).ExceptWith(scopeTokens);

        public Syntax StripFromPhase(int phase)
        {
            Syntax output = FromSyntax(this);

            IEnumerable<int> phasesToStrip = output._phasedScopeSets.Keys.Where(x => x >= phase);
            foreach(var pair in output._phasedScopeSets)
            {
                output._phasedScopeSets.Remove(pair.Key);
            }

            return output;
        }

        /// <summary>
        /// Retrieve a live reference to the scope set at the given phase
        /// </summary>
        public HashSet<uint> GetScopeSet(int phase)
        {
            if (!_phasedScopeSets.ContainsKey(phase))
            {
                _phasedScopeSets[phase] = new();
            }
            return _phasedScopeSets[phase];
        }

        /// <summary>
        /// Retrieve all the phases for which this syntax contains scope set information.
        /// </summary>
        public IEnumerable<int> GetLivePhases() => _phasedScopeSets.Keys;

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
            if (term is ConsList cl)
            {
                return ConsList.Cons(ToDatum(cl.Car), ToDatum(cl.Cdr));
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


        #region Exposition

        public virtual bool TryExposeList(
            [NotNullWhen(true)] out ConsList? cons,
            [NotNullWhen(true)] out Syntax? car,
            [NotNullWhen(true)] out Syntax? cdr)
        {
            cons = null;
            car = null;
            cdr = null;
            return false;
        }

        public bool TryExposeList([NotNullWhen(true)] out ConsList? cons)
            => TryExposeList(out cons, out Syntax? _, out Syntax? _);

        public bool TryExposeList([NotNullWhen(true)] out Syntax? car, [NotNullWhen(true)] out Syntax? cdr)
            => TryExposeList(out ConsList? _, out car, out cdr);

        // ---

        public virtual bool TryExposeIdentifier(
            [NotNullWhen(true)] out Symbol? sym,
            [NotNullWhen(true)] out string? name)
        {
            sym = null;
            name = null;
            return false;
        }

        public bool TryExposeIdentifier([NotNullWhen(true)] out Symbol? sym)
            => TryExposeIdentifier(out sym, out string? _);

        public bool TryExposeIdentifier([NotNullWhen(true)] out string? name)
            => TryExposeIdentifier(out Symbol? _, out name);

        //public bool TryExposeVector<T>(
        //    [NotNullWhen(true)] out Vector? vec,
        //    [NotNullWhen(true)] out T[]? values)
        //    where T : Term
        //{
        //    if (Exposee is Vector v)
        //    {
        //        vec = v;
        //        values = v.Values;
        //        return true;
        //    }

        //    vec = null;
        //    values = null;
        //    return false;
        //}

        #endregion
    }
}
