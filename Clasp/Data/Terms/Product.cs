using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Terms
{
    /// <summary>
    /// Represents a complex value built out of other nested values.
    /// </summary>
    internal abstract class Product : Term { }

    internal abstract class ConsCell : Product
    {
        public Term Car { get; private set; }
        public Term Cdr { get; private set; }

        protected ConsCell(Term car, Term cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public void SetCar(Term value) => Car = value;
        public void SetCdr(Term value) => Cdr = value;
    }

    internal sealed class Pair : ConsCell
    {
        public Pair(Term car, Term cdr) : base(car, cdr) { }
        public Pair(Term first, Term second, params Term[] rest) : base(
            first,
            rest.Length == 0
                ? second
                : new Pair(second, rest[0], rest[1..]))
        { }

        public override string ToString() => string.Format("PAIR({0}{1})", Car, PrintCdr(Cdr));
        private static string PrintCdr(Term e)
        {
            return e is Pair p
                ? string.Format("; {0}{1}", p.Car, PrintCdr(p.Cdr))
                : e.ToString();
        }
    }

    internal sealed class List : ConsCell, IEnumerable<Term>
    {
        public List(Term first, params Term[] rest) : base(
            first,
            rest.Length == 0
                ? Nil.Value
                : new List(rest[0], rest[1..]))
        { }

        public override string ToString() => string.Format("LIST({0}{1})", Car, PrintCdr(Cdr));
        private static string PrintCdr(Term e)
        {
            return e is List l
                ? string.Format(", {0}{1}", l.Car, PrintCdr(l.Cdr))
                : string.Empty; //must be Nil
        }

        private IEnumerable<Term> Enumerate()
        {
            List? target = this;

            while (target is not null)
            {
                yield return target.Car;
                target = target.Cdr is List l ? l : null;
            }

            yield break;
        }
        public IEnumerator<Term> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();
    }

    internal sealed class Vector : Product, IEnumerable<Term>
    {
        public readonly Term[] Values;
        public Vector(params Term[] values) => Values = values;

        public override string ToString() => string.Format("VECTOR({0})", string.Format(", ", Values.ToArray<object>()));

        public IEnumerator<Term> GetEnumerator() => ((IEnumerable<Term>)Values).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();
    }
}
