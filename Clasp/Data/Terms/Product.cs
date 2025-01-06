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


    internal sealed class ConsList : Product, IEnumerable<Term>
    {
        public Term Car { get; private set; }
        public Term Cdr { get; private set; }

        private ConsList(Term car, Term cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public void SetCar(Term newCar) => Car = newCar;
        public void SetCdr(Term newCdr) => Cdr = newCdr;

        public static ConsList Cons(Term car, Term cdr) => new ConsList(car, cdr);

        public static Term ProperList(params Term[] terms)
        {
            Term output = Nil.Value;

            foreach(Term t in terms.Reverse())
            {
                output = new ConsList(t, output);
            }

            return output;
        }

        public static ConsList ImproperList(Term first, Term second, params Term[] rest)
        {
            if (rest.Length == 0)
            {
                return new ConsList(first, second);
            }
            else
            {
                ConsList cdr = ImproperList(second, rest[0], rest[1..]);
                return new ConsList(first, cdr);
            }
        }

        public static ConsList ConstructDirect(IEnumerable<Term> terms)
        {
            Term[] realized = terms.ToArray();

            if (realized.Length < 2)
            {
                throw new ClaspException.Uncategorized("At least two terms needed to construct a cons cell.");
            }
            else
            {
                return ImproperList(realized[0], realized[1], realized[2..]);
            }
        }

        #region Enumeration

        private static IEnumerable<ConsList> EnumerateLinks(ConsList cl)
        {
            Term target = cl;

            while (target is ConsList current)
            {
                yield return current;
                target = current.Cdr;
            }

            yield break;
        }

        private static IEnumerable<Term> EnumerateValues(ConsList cl)
        {
            Term last = cl;

            foreach(ConsList cell in EnumerateLinks(cl))
            {
                yield return cell.Car;
                last = cell.Cdr;
            }

            yield return last;
        }

        public Term[] ToArray() => EnumerateValues(this).ToArray();

        public IEnumerator<Term> GetEnumerator() => EnumerateValues(this).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => EnumerateValues(this).GetEnumerator();

        #endregion

        #region Printing

        public override string ToString() => string.Format("LIST({0}{1})", Car, PrintAsTail(Cdr));

        private static string PrintAsTail(Term t) => t switch
        {
            Nil => string.Empty,
            ConsList cl => string.Format(", {0}{1}", cl.Car, PrintAsTail(cl.Cdr)),
            _ => string.Format("; {0}", t)
        };



        #endregion
    }

    //internal abstract class ConsCell : Product
    //{
    //    public Term Car { get; private set; }
    //    public Term Cdr { get; private set; }

    //    protected ConsCell(Term car, Term cdr)
    //    {
    //        Car = car;
    //        Cdr = cdr;
    //    }

    //    public void SetCar(Term value) => Car = value;
    //    public void SetCdr(Term value) => Cdr = value;
    //}

    //internal sealed class Pair : ConsCell
    //{
    //    public Pair(Term car, Term cdr) : base(car, cdr) { }
    //    public Pair(Term first, Term second, params Term[] rest) : base(
    //        first,
    //        rest.Length == 0
    //            ? second
    //            : new Pair(second, rest[0], rest[1..]))
    //    { }

    //    public override string ToString() => string.Format("PAIR({0}{1})", Car, PrintCdr(Cdr));
    //    private static string PrintCdr(Term e)
    //    {
    //        return e is Pair p
    //            ? string.Format("; {0}{1}", p.Car, PrintCdr(p.Cdr))
    //            : e.ToString();
    //    }
    //}

    //internal sealed class List : ConsCell, IEnumerable<Term>
    //{
    //    public List(Term first, params Term[] rest) : base(
    //        first,
    //        rest.Length == 0
    //            ? Nil.Value
    //            : new List(rest[0], rest[1..]))
    //    { }

    //    public override string ToString() => string.Format("LIST({0}{1})", Car, PrintCdr(Cdr));
    //    private static string PrintCdr(Term e)
    //    {
    //        return e is List l
    //            ? string.Format(", {0}{1}", l.Car, PrintCdr(l.Cdr))
    //            : string.Empty; //must be Nil
    //    }

    //    private IEnumerable<Term> Enumerate()
    //    {
    //        List? target = this;

    //        while (target is not null)
    //        {
    //            yield return target.Car;
    //            target = target.Cdr is List l ? l : null;
    //        }

    //        yield break;
    //    }
    //    public IEnumerator<Term> GetEnumerator() => Enumerate().GetEnumerator();
    //    IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();
    //}

    //internal sealed class Vector : Product, IList<Term>
    //{
    //    public readonly Term[] Values;
    //    public Vector(params Term[] values) => Values = values;

    //    public override string ToString() => string.Format("VECTOR({0})", string.Format(", ", Values.ToArray<object>()));

    //    public IEnumerator<Term> GetEnumerator() => ((IEnumerable<Term>)Values).GetEnumerator();
    //    IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();
    //}
}
