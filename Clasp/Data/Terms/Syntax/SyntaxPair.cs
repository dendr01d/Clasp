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
        private Pair? _lazyCons;
        public override Pair Expose()
        {
            if (_lazyCons is null)
            {
                _lazyCons = Pair.Cons(Car, Cdr);
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

        public override void AddScope(int phase, params uint[] scopeTokens)
        {
            base.AddScope(phase, scopeTokens);
            Car.AddScope(phase, scopeTokens);
            Cdr.AddScope(phase, scopeTokens);
        }
        public override void FlipScope(int phase, params uint[] scopeTokens)
        {
            base.FlipScope(phase, scopeTokens);
            Car.FlipScope(phase, scopeTokens);
            Cdr.FlipScope(phase, scopeTokens);
        }
        public override void RemoveScope(int phase, params uint[] scopeTokens)
        {
            base.RemoveScope(phase, scopeTokens);
            Car.RemoveScope(phase, scopeTokens);
            Cdr.RemoveScope(phase, scopeTokens);
        }

        public IEnumerator<Syntax?> GetEnumerator() => this.EnumerateElements().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.EnumerateElements().GetEnumerator();

        public override string ToString() => string.Format("#'({0}{1})", Car, PrintAsTail(Cdr));

        private static string PrintAsTail(Syntax stx)
        {
            if (stx.IsTerminator())
            {
                return string.Empty;
            }
            else if (stx is SyntaxPair stp)
            {
                return string.Format(" {0}{1}", stp.Car, PrintAsTail(stp.Cdr));
            }
            else
            {
                return string.Format(" . {0}", stx);
            }
        }
        protected override string FormatType() => string.Format("StxCons<{0}, {1}>", Car.TypeName, Cdr.TypeName);
        public override string ToSourceString() => Pair.Cons(Car.Expose(), Cdr.Expose()).ToString();
    }
}
