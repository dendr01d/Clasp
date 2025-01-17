using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Data.Terms
{
    internal sealed class Syntax : Term, ISourceTraceable
    {
        public SourceLocation Location { get; private set; }

        private readonly Term _wrappedValue;
        private readonly Dictionary<int, HashSet<uint>> _phasedScopeSets;
        private readonly HashSet<string> _properties;

        public static Dictionary<int, HashSet<uint>> BlankScope => new Dictionary<int, HashSet<uint>> ();

        //TODO: Review the semantics of wrapping. DO scopes get merged, or replaced?
        //what happens if you try to wrap syntax itself?


        private Syntax(Term term, SourceLocation location, Dictionary<int, HashSet<uint>> scopeSets)
        {
            Location = location;

            _wrappedValue = term;
            _phasedScopeSets = scopeSets.ToDictionary(x => x.Key, x => new HashSet<uint>(x.Value));
            _properties = new HashSet<string>();
        }

        /// <summary>
        /// Create a syntax with blank scope derived from the given location.
        /// </summary>
        public static Syntax Wrap(Term term, SourceLocation location)
        {
            return term switch
            {
                Syntax stx => stx,
                ConsList cl => new Syntax(
                    ConsList.Cons(Wrap(cl.Car, location.Derivation()), Wrap(cl.Cdr, location.Derivation())),
                    location,
                    BlankScope),
                _ => new Syntax(term, location, BlankScope)
            };
        }

        /// <summary>
        /// Create a syntax with blank scope corresponding to the location of the given token.
        /// </summary>
        public static Syntax Wrap(Term term, Token token) => Wrap(term, token.Location);

        /// <summary>
        /// Create a syntax with the same scope and location as an existing syntax.
        /// </summary>
        public static Syntax Wrap<T>(T term, Syntax existingSyntax)
            where T : Term
        {
            return term switch
            {
                Syntax stx => stx,
                ConsList cl => new Syntax(
                    ConsList.Cons(Wrap(cl.Car, existingSyntax), Wrap(cl.Cdr, existingSyntax)),
                    existingSyntax.Location.Derivation(),
                    existingSyntax._phasedScopeSets),
                _ => new Syntax(term, existingSyntax.Location, BlankScope)
            };
        }

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

        /// <summary>
        /// Unwraps this Syntax's datum while preserving the context of any nested Syntax.
        /// </summary>
        public Term ExposeTop()
        {
            return _wrappedValue;
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
                return ToDatum(stx.ExposeTop());
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

        public bool TryExposeList(
            [NotNullWhen(true)] out ConsList? cons,
            [NotNullWhen(true)] out Term? car,
            [NotNullWhen(true)] out Term? cdr)
        {
            if (_wrappedValue is ConsList cl)
            {
                cons = cl;
                car = Wrap(cl.Car, this);
                cdr = Wrap(cl.Cdr, this);
                return true;
            }

            cons = null;
            car = null;
            cdr = null;
            return false;
        }

        public bool TryExposeIdentifier(
            [NotNullWhen(true)] out Symbol? sym,
            [NotNullWhen(true)] out string? name)
        {
            if (_wrappedValue is Symbol s)
            {
                sym = s;
                name = s.Name;
                return true;
            }

            sym = null;
            name = null;
            return false;
        }

        public bool TryExposeVector(
            [NotNullWhen(true)] out Vector? vec,
            [NotNullWhen(true)] out Term[]? values)
        {
            if (_wrappedValue is Vector v)
            {
                vec = v;
                values = v.Values;
                return true;
            }

            vec = null;
            values = null;
            return false;
        }

        //public override string ToString() => string.Format("#'{0}", _wrappedTerm);

        public override string ToString() => _wrappedValue switch
        {
            ConsList cl => string.Format("#'({0})", string.Join(' ', EnumerateAndPrint(this))),
            Symbol sym => string.Format("#'{0}", sym),
            _ => _wrappedValue.ToString()
        };

        private static IEnumerable<string> EnumerateAndPrint(Syntax stx)
        {
            Term current = stx;

            while (current is Syntax currentStx
                && currentStx._wrappedValue is ConsList cl)
            {
                yield return cl.Car.ToString();

                current = cl.Cdr;
            }

            if (current is not Syntax terminatorStx
                || terminatorStx._wrappedValue is not Nil)
            {
                yield return ";";
                yield return current.ToString();
            }
        }
    }
}
