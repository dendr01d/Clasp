using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal sealed class SimpleNum : Literal<decimal>
    {
        public SimpleNum(decimal val) : base(val) { }

        public static readonly SimpleNum Zero = new SimpleNum(0);
        public static readonly SimpleNum One = new SimpleNum(1);


        public static SimpleNum Add(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value + y.Value);
        public static SimpleNum Subtract(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value - y.Value);
        public static SimpleNum Multiply(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value * y.Value);
        public static SimpleNum Quotient(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value / y.Value);
        public static SimpleNum IntDiv(SimpleNum x, SimpleNum y) => new SimpleNum((int)Math.Truncate(x.Value) / (int)Math.Truncate(y.Value));
        public static SimpleNum Modulo(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value % y.Value);
        public static SimpleNum Exponent(SimpleNum x, SimpleNum y) => new SimpleNum((decimal)Math.Pow((double)x.Value, (double)y.Value));

        public static Boolean NumEquals(SimpleNum x, SimpleNum y) => Boolean.Judge(x.Value == y.Value);
        public static Boolean LessThan(SimpleNum x, SimpleNum y) => Boolean.Judge(x.Value < y.Value);
        public static Boolean GreatherThan(SimpleNum x, SimpleNum y) => Boolean.Judge(x.Value > y.Value);
        public static Boolean Leq(SimpleNum x, SimpleNum y) => Boolean.Judge(x.Value <= y.Value);
        public static Boolean Geq(SimpleNum x, SimpleNum y) => Boolean.Judge(x.Value >= y.Value);

        public static SimpleNum Negate(SimpleNum x) => new SimpleNum(x.Value * -1);

        public static SimpleNum Abs(SimpleNum x) => x.Value < 0 ? Negate(x) : x;
        public static SimpleNum Truncate(SimpleNum x) => new SimpleNum(Math.Truncate(x.Value));
        public static SimpleNum Ceiling(SimpleNum x) => new SimpleNum(Math.Ceiling(x.Value));
        public static SimpleNum Floor(SimpleNum x) => new SimpleNum(Math.Floor(x.Value));
        public static SimpleNum Round(SimpleNum x, SimpleNum digits) => new SimpleNum(Math.Round(x.Value, (int)Math.Truncate(digits.Value)));
    }
}
