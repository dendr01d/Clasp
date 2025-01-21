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

        private readonly Dictionary<int, HashSet<uint>> _phasedScopeSets;
        private readonly HashSet<string> _properties;

        /// <summary>Exposé</summary>
        public Term Exposee { get; }

        //public static Dictionary<int, HashSet<uint>> BlankScope => new Dictionary<int, HashSet<uint>> ();

        //TODO: Review the semantics of wrapping. DO scopes get merged, or replaced?
        //what happens if you try to wrap syntax itself?

        private Syntax(Term term, SourceLocation location, Dictionary<int, HashSet<uint>>? scopeSets)
        {
            Location = location;

            _phasedScopeSets = scopeSets?.ToDictionary(x => x.Key, x => new HashSet<uint>(x.Value))
                ?? new Dictionary<int, HashSet<uint>>();
            _properties = new HashSet<string>();

            Exposee = term;
        }

        #region Static Constructors

        /// <summary>
        /// Create a syntax with blank scope derived from the given location.
        /// </summary>
        public static Syntax Wrap(Term term, SourceLocation location) => new Syntax(term, location, null);

        /// <summary>
        /// Create a syntax with blank scope corresponding to the location of the given token.
        /// </summary>
        public static Syntax Wrap(Term term, Token token) => new Syntax(term, token.Location, null);

        /// <summary>
        /// Create a syntax with the same scope and location as an existing syntax.
        /// </summary>
        public static Syntax Wrap(Term term, Syntax existingSyntax) => new Syntax(term, existingSyntax.Location.Derivation(), existingSyntax._phasedScopeSets);

        public static Syntax Wrap(Term car, Term cdr, Syntax existingSyntax) => Wrap(ConsList.Cons(car, cdr), existingSyntax); //?

        #endregion

        // ---

        public bool GetProperty(string propName) => _properties.Contains(propName);
        public bool AddProperty(string propName) => _properties.Add(propName);

        public HashSet<uint> GetContext(int phase)
        {
            if (!_phasedScopeSets.TryGetValue(phase, out HashSet<uint>? scopeSet))
            {
                _phasedScopeSets[phase] = new HashSet<uint>();
            }
            return _phasedScopeSets[phase];
        }

        // ---

        private static Term EagerlyAdjustScope(Term term, int phase, uint[] scopeIds, Action<HashSet<uint>, uint[]> adjustment)
        {
            if (term is Syntax stx)
            {
                stx.GetContext(phase);
                adjustment(stx._phasedScopeSets[phase], scopeIds);
                return stx;
            }
            else if (term is ConsList cl)
            {
                Term car = PaintScope(cl, phase, scopeIds);
                Term cdr = PaintScope(cl, phase, scopeIds);
                return ConsList.Cons(car, cdr);
            }
            else
            {
                return term;
            }
        }

        public static Term PaintScope(Term term, int phase, params uint[] scopeIds)
            => EagerlyAdjustScope(term, phase, scopeIds, (scopeSet, ids) => scopeSet.UnionWith(ids));

        public static Term FlipScope(Term term, int phase, params uint[] scopeIds)
            => EagerlyAdjustScope(term, phase, scopeIds, (scopeSet, ids) => scopeSet.SymmetricExceptWith(ids));

        public static Term RemoveScope(Term term, int phase, params uint[] scopeIds)
            => EagerlyAdjustScope(term, phase, scopeIds, (scopeSet, ids) => scopeSet.RemoveWhere(x => ids.Contains(x)));

        // ---

        public static Term ToDatum(Term term)
        {
            if (term is Syntax stx)
            {
                return ToDatum(stx.Exposee);
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

        public override string ToString() => string.Format("#'{0}", Exposee is ConsList
            ? string.Format("({0})", string.Join(", ", EnumerateAndPrint(this)))
            : Exposee.ToString());

        private static IEnumerable<string> EnumerateAndPrint(Syntax stx)
        {
            Term current = stx;

            while (current is Syntax stxCurrent && stxCurrent.Exposee is ConsList cl)
            {
                yield return cl.Car.ToString();
                current = cl.Cdr;
            }

            if (current is not Syntax terminatorStx
                || terminatorStx.Exposee is not Nil)
            {
                yield return ".";
                yield return current.ToString();
            }
        }

        protected override string FormatType() => string.Format("Syntax<{0}>", Exposee.TypeName);

        #endregion


        #region Exposition

        public bool TryExposeList(
            [NotNullWhen(true)] out ConsList? cons)
        {
            if (Exposee is ConsList cl)
            {
                cons = cl;
                return true;
            }

            cons = null;
            return false;
        }

        public bool TryExposeList<T1, T2>(
            [NotNullWhen(true)] out ConsList? cons,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cdr)
            where T1 : Term
            where T2 : Term
        {
            if (TryExposeList(out cons)
                && cons.Car is T1 listCar
                && cons.Cdr is T2 listCdr)
            {
                car = listCar;
                cdr = listCdr;
                return true;
            }

            car = null;
            cdr = null;
            return false;
        }

        public bool TryExposeList<T1, T2>(
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cdr)
            where T1 : Term
            where T2 : Term
        {
            return TryExposeList(out ConsList? _, out car, out cdr);
        }

        // ---

        public bool TryExposeIdentifier(
            [NotNullWhen(true)] out Symbol? sym,
            [NotNullWhen(true)] out string? name)
        {
            if (Exposee is Symbol s)
            {
                sym = s;
                name = s.Name;
                return true;
            }

            sym = null;
            name = null;
            return false;
        }

        public bool TryExposeIdentifier(
            [NotNullWhen(true)] out Symbol? sym)
        {
            return TryExposeIdentifier(out sym, out string? _);
        }

        public bool TryExposeIdentifier(
            [NotNullWhen(true)] out string? name)
        {
            return TryExposeIdentifier(out Symbol? _, out name);
        }

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
