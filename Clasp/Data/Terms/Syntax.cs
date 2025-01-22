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

        public readonly MultiScope MultiScope;
        public readonly HashSet<string> Properties;

        public Term Wrapped { get; }

        //TODO: Review the semantics of wrapping. DO scopes get merged, or replaced?
        //what happens if you try to wrap syntax itself?

        private Syntax(Term term, SourceLocation loc, Syntax? copy)
        {
            Wrapped = term;
            Location = loc;

            MultiScope = copy?.MultiScope ?? new MultiScope(); // are they allowed to share?
            Properties = copy?.Properties ?? new HashSet<string>();
        }

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

        public bool GetProperty(string propName) => Properties.Contains(propName);
        public bool AddProperty(string propName) => Properties.Add(propName);

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

        public Term ToDatum() => ToDatum(Wrapped);
        private static Term ToDatum(Term term)
        {
            if (term is Syntax stx)
            {
                return ToDatum(stx.Wrapped);
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

        public override string ToString() => string.Format("#'{0}", Wrapped is ConsList
            ? string.Format("({0})", string.Join(", ", EnumerateAndPrint(this)))
            : Wrapped.ToString());

        private static IEnumerable<string> EnumerateAndPrint(Syntax stx)
        {
            Term current = stx;

            while (current is Syntax stxCurrent && stxCurrent.Wrapped is ConsList cl)
            {
                yield return cl.Car.ToString();
                current = cl.Cdr;
            }

            if (current is not Syntax terminatorStx
                || terminatorStx.Wrapped is not Nil)
            {
                yield return ".";
                yield return current.ToString();
            }
        }

        protected override string FormatType() => string.Format("Syntax<{0}>", Wrapped.TypeName);

        #endregion


        #region Exposition

        public bool TryExposeList(
            [NotNullWhen(true)] out ConsList? cons)
        {
            if (Wrapped is ConsList cl)
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
            if (Wrapped is Symbol s)
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
