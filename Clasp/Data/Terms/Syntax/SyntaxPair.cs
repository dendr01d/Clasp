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
using System.Runtime.ConstrainedExecution;

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

        public SyntaxPair(Syntax car, Syntax cdr, LexInfo ctx) : base(ctx)
        {
            Car = car;
            Cdr = cdr;
            _lazyCons = null;
        }

        public SyntaxPair(Syntax car, Syntax cdr, Syntax copy)
            : this(car, cdr, copy.LexContext)
        { }

        public SyntaxPair(Term car, Term cdr, LexInfo ctx)
            : this(FromDatum(car, ctx), FromDatum(cdr, ctx), ctx)
        { }

        protected override SyntaxPair DeepCopy()
        {
            return new SyntaxPair(FromSyntax(Car), FromSyntax(Cdr), LexContext);
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
