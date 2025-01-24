using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Data.Terms
{
    internal sealed class Syntax : Term, ISourceTraceable
    {
        public SourceLocation Location { get; private set; }

        private readonly Dictionary<int, ScopeTokenSet> _phasedScopeSets;
        //private readonly HashSet<string> _properties;

        private Lazy<Term> _lazyWrapped;
        //private Term _wrapped => _lazyWrapped.Value;
        public Term Expose() => _lazyWrapped.Value;

        //TODO: Review the semantics of wrapping. DO scopes get merged, or replaced?
        //what happens if you try to wrap syntax itself?

        private Syntax(Term term, SourceLocation loc, Syntax? copy)
        {
            _lazyWrapped = new Lazy<Term>(term);
            Location = loc;

            _phasedScopeSets = new Dictionary<int, ScopeTokenSet>(copy?._phasedScopeSets ?? []);
            //_properties = new HashSet<string>(copy?._properties ?? []);
        }

        // Too complicated to deal with lazy initialization rn
        //private Syntax(Func<Term> thunk, SourceLocation loc, Syntax? copy)
        //{
        //    _lazyWrapped = new Lazy<Term>(thunk);
        //    Location = loc;

        //    MultiScope = copy?.MultiScope ?? new MultiScope();
        //    Properties = copy?.Properties ?? new HashSet<string>();
        //}

        #region Static Constructors

        private static Syntax TrueWrap(Term term, SourceLocation loc, Syntax? copy)
        {
            return term switch
            {
                Syntax s => s,
                ConsList cl => new Syntax(ConsList.Cons(TrueWrap(cl.Car, loc, copy), TrueWrap(cl.Cdr, loc, copy)), loc, copy),
                _ => new Syntax(term, loc, copy)
            };
        }

        /// <summary>
        /// Create a syntax with blank scope derived from the given location.
        /// </summary>
        public static Syntax Wrap(Term term, SourceLocation location) => TrueWrap(term, location, null);

        /// <summary>
        /// Create a syntax with blank scope corresponding to the location of the given token.
        /// </summary>
        public static Syntax Wrap(Term term, Token token) => TrueWrap(term, token.Location, null);

        /// <summary>
        /// Create a syntax with the same scope and location as an existing syntax.
        /// </summary>
        public static Syntax Wrap(Term term, Syntax existingSyntax)
        {
            return TrueWrap(term, existingSyntax.Location.Derivation(), existingSyntax);
        }

        /// <summary>
        /// Create a syntax wrapped around the cons of <paramref name="car"/> and <paramref name="cdr"/>.
        /// </summary>
        public static Syntax Wrap(Term car, Term cdr, Syntax existingSyntax)
        {
            return Wrap(ConsList.Cons(car, cdr), existingSyntax);
        }

        #endregion

        // ---

        //public bool HasProperty(string propName) => _properties.Contains(propName);
        //public bool AddProperty(string propName) => _properties.Add(propName);

        // ---

        /// <summary>
        /// Retrieve a live reference to the scope set at the given phase
        /// </summary>
        public ScopeTokenSet GetScopeSet(int phase)
        {
            if (!_phasedScopeSets.ContainsKey(phase))
            {
                _phasedScopeSets[phase] = new ScopeTokenSet();
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

        #region Term Overrides

        public override string ToString()
        {
            return TryExposeList(out ConsList? list)
                ? string.Format("#'({0})", string.Join(", ", EnumerateAndPrint(this)))
                : string.Format("#'{0}", Expose());
        }

        private static IEnumerable<string> EnumerateAndPrint(Syntax stx)
        {
            Term current = stx;

            while (current is Syntax stxCurrent
                && stxCurrent.TryExposeList(out Term? car, out Term? cdr))
            {
                yield return car.ToString();
                current = cdr;
            }

            if (current is not Syntax terminatorStx
                || terminatorStx.Expose() is not Nil)
            {
                yield return ".";
                yield return current.ToString();
            }
        }

        protected override string FormatType() => string.Format("Syntax<{0}>", Expose().TypeName);

        #endregion


        #region Exposition

        public bool TryExposeList<T1, T2>(
            [NotNullWhen(true)] out ConsList? cons,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cdr)
            where T1 : Term
            where T2 : Term
        {
            if (Expose() is ConsList list
                && list.Car is T1 listCar
                && list.Cdr is T2 listCdr)
            {
                cons = list;
                car = listCar;
                cdr = listCdr;
                return true;
            }

            cons = null;
            car = null;
            cdr = null;
            return false;
        }

        public bool TryExposeList([NotNullWhen(true)] out ConsList? cons)
            => TryExposeList(out cons, out Syntax? _, out Syntax? _);

        public bool TryExposeList<T1, T2>([NotNullWhen(true)] out T1? car, [NotNullWhen(true)] out T2? cdr)
            where T1 : Term
            where T2 : Term
            => TryExposeList(out ConsList? _, out car, out cdr);

        // ---

        public bool TryExposeIdentifier(
            [NotNullWhen(true)] out Symbol? sym,
            [NotNullWhen(true)] out string? name)
        {
            if (Expose() is Symbol s)
            {
                sym = s;
                name = s.Name;
                return true;
            }

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
