using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.ExtensionMethods;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.Product;
using Clasp.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Clasp.Data.Terms.Syntax
{
    internal sealed class SyntaxPair : Syntax, ICons<Syntax, Syntax>
    {
        private ConsList? _lazyCons;
        public override ConsList Expose()
        {
            if (_lazyCons is null)
            {
                _lazyCons = ConsList.Cons(Car, Cdr);
            }
            return _lazyCons;
        }

        public Syntax Car { get; private set; }
        public Syntax Cdr { get; private set; }
        public bool IsDotted => Cdr is not SyntaxPair;

        public SyntaxPair(Syntax car, Syntax cdr, SourceLocation loc, Syntax? copy = null)
            : base(loc, copy)
        {
            Car = car;
            Cdr = cdr;
            _lazyCons = null;
        }

        public SyntaxPair(Term car, Term cdr, SourceLocation loc, Syntax? copy = null)
            : this(
                  FromDatum(car, loc.Derivation()),
                  FromDatum(cdr, loc.Derivation()),
                  loc, copy)
        { }

        public SyntaxPair(ConsList cl, SourceLocation loc, Syntax? copy = null)
            : this(cl.Car, cl.Cdr, loc, copy)
        { }

        protected override SyntaxPair DeepCopy()
        {
            return new SyntaxPair(Syntax.FromSyntax(Car), Syntax.FromSyntax(Cdr), Location, this);
        }

        public void SetCar(Syntax newCar)
        {
            Car = newCar;
            _lazyCons = null;
        }

        public void SetCdr(Syntax newCdr)
        {
            Cdr = newCdr;
            _lazyCons = null;
        }

        public IEnumerator<Syntax?> GetEnumerator() => this.EnumerateElements().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.EnumerateElements().GetEnumerator();

        public override bool TryExposeList(
            [NotNullWhen(true)] out ConsList? cons,
            [NotNullWhen(true)] out Syntax? car,
            [NotNullWhen(true)] out Syntax? cdr)
        {
            cons = Expose();
            car = Car;
            cdr = Cdr;
            return true;
        }


        public override string ToString()
        {
            Syntax?[] terms = this.EnumerateElements().ToArray();

            return string.Format(
                "#'({0}{1})",
                string.Join(" ", terms[^2]),
                terms[^1]?.Expose() is null ? string.Empty : terms[^1]);
        }
        protected override string FormatType() => string.Format("StxCons<{0}, {1}>", Car.TypeName, Cdr.TypeName);
    }
}
