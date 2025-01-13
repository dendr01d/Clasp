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
    internal class Syntax : Term, ISourceTraceable
    {
        public SourceLocation Location { get; private set; }

        private readonly List<ScopeSet> _context;
        private readonly Term _wrappedTerm;

        private Syntax(Term term, SourceLocation source, IEnumerable<ScopeSet> lexInfo)
        {
            _wrappedTerm = term;

            Location = source;
            _context = lexInfo.ToList();
        }

        public static Syntax Wrap(Term term, SourceLocation source, IEnumerable<ScopeSet> lexInfo)
        {
            if (term is Syntax sw)
            {
                throw new InvalidOperationException("Can't wrap existing syntax wrapper.");
            }
            else
            {
                return new Syntax(term, source, lexInfo);
            }
        }

        public static Syntax Wrap(Term term, Syntax extantWrapper)
        {
            if (term is Syntax sw)
            {
                for (int i = 0; i < extantWrapper._context.Count; ++i)
                {
                    if (sw._context.Count <= i)
                    {
                        sw._context.Add(new ScopeSet(extantWrapper._context[i]));
                    }
                    else
                    {
                        sw._context[i].Add(extantWrapper._context[i]);
                    }
                }
                return sw;
            }
            else
            {
                return new Syntax(term, extantWrapper.Location, extantWrapper._context);
            }
        }

        public static Syntax Wrap(Term term, Token token) => Wrap(term, token.Location, Array.Empty<ScopeSet>());

        // ---

        public ScopeSet GetContext(int phase)
        {
            while (_context.Count <= phase)
            {
                _context.Add(new ScopeSet());
            }
            return _context[phase];
        }

        public void Paint(int phase, params uint[] scopeTokens)
        {
            while (_context.Count <= phase)
            {
                _context.Add(new ScopeSet());
            }
            _context[phase].Add(scopeTokens);
        }

        public void FlipScope(int phase, params uint[] scopeTokens)
        {
            while (_context.Count <= phase)
            {
                _context.Add(new ScopeSet());
            }
            _context[phase].Flip(scopeTokens);
        }

        public Term Strip()
        {
            if (_wrappedTerm is ConsList cl)
            {
                Term outCar = cl.Car is Syntax stxCar
                    ? stxCar.Strip()
                    : cl.Car;

                Term outCdr = cl.Cdr is Syntax stxCdr
                    ? stxCdr.Strip()
                    : cl.Cdr;

                if (cl.Car is Syntax || cl.Cdr is Syntax)
                {
                    return ConsList.Cons(
                        outCar == cl.Car ? cl.Car : outCar,
                        outCdr == cl.Cdr ? cl.Cdr : outCdr);
                }
                else
                {
                    return cl;
                }
            }
            //else if (WrappedTerm is Vector vec)
            //{

            //}
            else
            {
                return _wrappedTerm;
            }
        }

        public Term Expose()
        {
            if (_wrappedTerm is ConsList cl)
            {
                cl.SetCar(Wrap(cl.Car, this));
                cl.SetCdr(Wrap(cl.Cdr, this));
                return cl;
            }
            //else if (WrappedTerm is Vector vec)
            //{

            //}
            else
            {
                return _wrappedTerm;
            }
        }

        public bool TryExposeList(
            [NotNullWhen(true)] out Syntax? car,
            [NotNullWhen(true)] out Syntax? cdr)
        {
            if (Expose() is ConsList cl
                && cl.Car is Syntax stxCar
                && cl.Cdr is Syntax stxCdr)
            {
                car = stxCar;
                cdr = stxCdr;
                return true;
            }
            else
            {
                car = null;
                cdr = null;
                return false;
            }
        }

        public bool TryExposeIdentifier(
            [NotNullWhen(true)] out Symbol? sym,
            [NotNullWhen(true)] out string? name)
        {
            if (_wrappedTerm is Symbol s)
            {
                sym = s;
                name = s.Name;
                return true;
            }
            else
            {
                sym = null;
                name = null;
                return false;
            }
        }

        //public override string ToString() => string.Format("#'{0}", _wrappedTerm);

        public override string ToString() => Expose() switch
        {
            ConsList cl => string.Format("#'({0})", string.Join(' ', EnumerateAndPrint(this))),
            Symbol sym => string.Format("#'{0}", sym),
            _ => _wrappedTerm.ToString()
        };

        private static IEnumerable<string> EnumerateAndPrint(Syntax stx)
        {
            Term current = stx;

            while (current is Syntax currentStx
                && currentStx.Expose() is ConsList cl)
            {
                yield return cl.Car.ToString();

                current = cl.Cdr;
            }

            if (current is not Syntax terminatorStx
                || terminatorStx.Expose() is not Nil)
            {
                yield return ";";
                yield return current.ToString();
            }
        }
    }
}
