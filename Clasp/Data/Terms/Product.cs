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

        public bool IsDotted { get; private set; }

        private ConsList(Term car, Term cdr)
        {
            Car = car;
            Cdr = cdr;
            UpdateDottedStatus();
        }

        public void SetCar(Term newCar) => Car = newCar;
        public void SetCdr(Term newCdr)
        {
            Cdr = newCdr;
            UpdateDottedStatus();
        }

        public static ConsList Cons(Term car, Term cdr) => new ConsList(car, cdr);

        private void UpdateDottedStatus()
        {
            IsDotted = Cdr is ConsList cl
                ? cl.IsDotted
                : Cdr is Nil;
        }

        // ---

        //TODO review these constructors and their usage...

        public static Term ProperList(params Term[] terms) => ProperList(terms.AsEnumerable());
        public static Term ProperList(IEnumerable<Term> terms)
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
                throw new ClaspGeneralException("At least two terms needed to construct a cons cell.");
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

        //public override string ToString() => string.Format("LIST({0}{1})", Car, PrintAsTail(Cdr));

        //private static string PrintAsTail(Term t) => t switch
        //{
        //    Nil => string.Empty,
        //    ConsList cl => string.Format(", {0}{1}", cl.Car, PrintAsTail(cl.Cdr)),
        //    _ => string.Format("; {0}", t)
        //};

        public override string ToString() => string.Format("({0})", string.Join(' ', EnumerateAndPrint(this)));

        private static IEnumerable<string> EnumerateAndPrint(Term ls)
        {
            Term current = ls;

            while (current is ConsList cl)
            {
                yield return cl.Car.ToString();
                current = cl.Cdr;
            }

            if (current is not Nil
                && !(current is Syntax stx && stx.Expose is Nil))
            {
                yield return ".";
                yield return current.ToString();
            }
        }


        #endregion
    }

    internal sealed class Vector : Product
    {
        public readonly Term[] Values;
        public Vector(params Term[] values) => Values = values;

        public override string ToString() => string.Format(
            "#({0})",
            string.Format(", ", Values.ToArray<object>()));
    }
}
