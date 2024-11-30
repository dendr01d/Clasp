using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.AST
{
    /// <summary>
    /// Represents a complex value built out of other nested values.
    /// </summary>
    internal abstract class Product : Fixed { }

    internal abstract class ConsCell : Product
    {
        public Fixed Car { get; private set; }
        public Fixed Cdr { get; private set; }

        protected ConsCell(Fixed car, Fixed cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public void SetCar(Fixed value) => Car = value;
        public void SetCdr(Fixed value) => Cdr = value;
    }

    internal sealed class Pair : ConsCell
    {
        public Pair(Fixed car, Fixed cdr) : base(car, cdr) { }
        public Pair(Fixed first, Fixed second, params Fixed[] rest) : base(
            first,
            rest.Length == 0
                ? second
                : new Pair(second, rest[0], rest[1..]))
        { }

        public override string ToString() => string.Format("PAIR({0}{1})", Car, PrintCdr(Cdr));
        private static string PrintCdr(Fixed e)
        {
            return e is Pair p
                ? string.Format("; {0}{1}", p.Car, PrintCdr(p.Cdr))
                : e.ToString();
        }
    }

    internal sealed class List : ConsCell, IEnumerable<Fixed>
    {
        public List(Fixed first, params Fixed[] rest) : base(
            first,
            rest.Length == 0
                ? Nil.Value
                : new List(rest[0], rest[1..]))
        { }

        public override string ToString() => string.Format("LIST({0}{1})", Car, PrintCdr(Cdr));
        private static string PrintCdr(Fixed e)
        {
            return e is List l
                ? string.Format(", {0}{1}", l.Car, PrintCdr(l.Cdr))
                : string.Empty; //must be Nil
        }

        private IEnumerable<Fixed> Enumerate()
        {
            List? target = this;

            while (target is not null)
            {
                yield return target.Car;
                target = target.Cdr is List l ? l : null;
            }

            yield break;
        }
        public IEnumerator<Fixed> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();
    }

    internal sealed class Vector : Product, IEnumerable<Fixed>
    {
        public readonly Fixed[] Values;
        public Vector(params Fixed[] values) => Values = values;

        public override string ToString() => string.Format("VECTOR({0})", string.Format(", ", Values.ToArray<object>()));

        public IEnumerator<Fixed> GetEnumerator() => ((IEnumerable<Fixed>)Values).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();
    }
}
