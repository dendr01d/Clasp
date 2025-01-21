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
    internal abstract class Syntax : Term, ISourceTraceable
    {
        public SourceLocation Location { get; private set; }

        private readonly Dictionary<int, HashSet<uint>> _phasedScopeSets;
        private readonly HashSet<string> _properties;

        public abstract Term Expose { get; }

        public static Dictionary<int, HashSet<uint>> BlankScope => new Dictionary<int, HashSet<uint>> ();

        //TODO: Review the semantics of wrapping. DO scopes get merged, or replaced?
        //what happens if you try to wrap syntax itself?

        protected Syntax(SourceLocation location, Dictionary<int, HashSet<uint>> scopeSets)
        {
            Location = location;

            _phasedScopeSets = scopeSets.ToDictionary(x => x.Key, x => new HashSet<uint>(x.Value));
            _properties = new HashSet<string>();
        }

        #region Static Construction

        private static Syntax Wrap<T>(T term, SourceLocation location, Dictionary<int, HashSet<uint>> scopeSet)
            where T : Term
        {
            return term switch
            {
                Syntax stx => stx,
                ConsList cl => new Syntax<ConsList>(
                    ConsList.Cons(Wrap(cl.Car, location.Derivation()), Wrap(cl.Cdr, location.Derivation())),
                    location,
                    scopeSet),
                _ => new Syntax<T>(term, location, scopeSet)
            };
        }

        /// <summary>
        /// Create a syntax with blank scope derived from the given location.
        /// </summary>
        public static Syntax Wrap(Term term, SourceLocation location) => Wrap(term, location, BlankScope);

        /// <summary>
        /// Create a syntax with blank scope corresponding to the location of the given token.
        /// </summary>
        public static Syntax Wrap(Term term, Token token) => Wrap(term, token.Location);

        /// <summary>
        /// Create a syntax with the same scope and location as an existing syntax.
        /// </summary>
        public static Syntax Wrap(Term term, Syntax existingSyntax) => Wrap(term, existingSyntax.Location, existingSyntax._phasedScopeSets);

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
                return ToDatum(stx.Expose);
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

        //public bool TryExposeList(
        //    [NotNullWhen(true)] out ConsList? cons,
        //    [NotNullWhen(true)] out Term? car,
        //    [NotNullWhen(true)] out Term? cdr)
        //{
        //    if (_wrappedValue is ConsList cl)
        //    {
        //        cons = cl;
        //        car = Wrap(cl.Car, this);
        //        cdr = Wrap(cl.Cdr, this);
        //        return true;
        //    }

        //    cons = null;
        //    car = null;
        //    cdr = null;
        //    return false;
        //}

        //public bool TryExposeIdentifier(
        //    [NotNullWhen(true)] out Symbol? sym,
        //    [NotNullWhen(true)] out string? name)
        //{
        //    if (_wrappedValue is Symbol s)
        //    {
        //        sym = s;
        //        name = s.Name;
        //        return true;
        //    }

        //    sym = null;
        //    name = null;
        //    return false;
        //}

        //public bool TryExposeVector(
        //    [NotNullWhen(true)] out Vector? vec,
        //    [NotNullWhen(true)] out Term[]? values)
        //{
        //    if (_wrappedValue is Vector v)
        //    {
        //        vec = v;
        //        values = v.Values;
        //        return true;
        //    }

        //    vec = null;
        //    values = null;
        //    return false;
        //}
    }

    internal sealed class Syntax<T> : Syntax
        where T : Term
    {
        public override T Expose { get; }

        public Syntax(T term, SourceLocation location, Dictionary<int, HashSet<uint>> scopeSets)
            : base(location, scopeSets)
        {
            Expose = term;
        }

        public override string ToString() => this switch
        {
            Syntax<ConsList> cl => string.Format("#'({0})", string.Join(' ', EnumerateAndPrint(cl))),
            Syntax<Symbol> sym => string.Format("#'{0}", sym.Expose),
            _ => Expose.ToString()
        };

        private static IEnumerable<string> EnumerateAndPrint(Syntax stx)
        {
            Term current = stx;

            while (current is Syntax<ConsList> currentStx)
            {
                yield return currentStx.Expose.Car.ToString();

                current = currentStx.Expose.Cdr;
            }

            if (current is not Syntax terminatorStx
                || terminatorStx.Expose is not Nil)
            {
                yield return ".";
                yield return current.ToString();
            }
        }

        protected override string FormatType() => string.Format("Syntax<{0}>", Expose.TypeName);
    }
}
