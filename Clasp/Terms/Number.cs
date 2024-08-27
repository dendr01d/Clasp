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


    //    #region "Number Tower"

    //    internal interface INumeric
    //    {
    //        public bool ExactValue { get; }
    //    }

    //    internal interface IComplex : INumeric
    //    {
    //        public IReal RealPart { get; }
    //        public IReal ImaginaryPart { get; }
    //    }

    //    internal interface IReal : IComplex
    //    {
    //        public double RealValue { get; }
    //    }

    //    internal interface IRational : IReal
    //    {
    //        public IIntegral Numerator { get; }
    //        public IIntegral Denominator { get; }
    //    }

    //    internal interface IIntegral : IRational
    //    {
    //        public int IntegralValue { get; }
    //    }

    //    //internal interface INatural : IIntegral
    //    //{
    //    //    public uint NaturalValue { get; }
    //    //}

    //    #endregion

    //    internal abstract class Common : Atom
    //    {

    //        public override string ToSerialized() => ToPrinted();


    //        public static readonly INatural Zero = new Natural(0);
    //        public static readonly INatural One = new Natural(1);

    //        #region Global Numeric Ops

    //        public abstract Number Abs();
    //        public abstract Integer Floor();
    //        public abstract Integer Ceiling();
    //        public abstract Integer Truncate();

    //        #endregion

    //        #region Derived Ops

    //        //public Number Min(Number n) => LessThan(n) ? this : n;
    //        //public Number Max(Number n) => LessThan(n) ? n : this;

    //        #endregion

    //        #region Specialized Ops


    //        #endregion
    //    }

    //    internal abstract class Number : Common, INumeric
    //    {
    //        public bool ExactValue { get; protected set; }

    //        protected Number(bool exact) => ExactValue = exact;

    //    }

    //    internal class Complex : Common, IComplex
    //    {
    //        public bool ExactValue { get => RealPart.ExactValue && ImaginaryPart.ExactValue; }
    //        public IReal RealPart { get; private set; }
    //        public IReal ImaginaryPart { get; private set; }

    //        public Complex (IReal rp, IReal ip)
    //        {
    //            RealPart = rp;
    //            ImaginaryPart = ip;
    //        }

    //        public override string ToPrinted() => $"{RealPart} + {ImaginaryPart}i";
    //    }

    //    internal class Real : Common, IReal
    //    {
    //        public bool ExactValue { get; protected set; }
    //        public IReal RealPart { get => this; }
    //        public IReal ImaginaryPart { get => Zero; }
    //        public double RealValue { get; private set; }

    //        public Real(double value, bool exact = true)
    //        {
    //            ExactValue = exact;
    //            RealValue = value;
    //        }

    //        public override string ToPrinted() => RealValue.ToString();
    //    }

    //    internal class Rational : Common, IRational
    //    {
    //        public bool ExactValue { get => true; }
    //        public IReal RealPart { get => this; }
    //        public IReal ImaginaryPart { get => Zero; }
    //        public double RealValue { get => Numerator.RealValue / Denominator.RealValue; }
    //        public IIntegral Numerator { get; private set; }
    //        public IIntegral Denominator { get; private set; }

    //        public Rational(IIntegral num, IIntegral denom)
    //        {
    //            IIntegral gcd = GCD(num, denom);
    //            bool negate = (num.IntegralValue < 0) ^ (denom.IntegralValue < 0);

    //            Numerator = gcd.NumEqual(One)
    //                ? negate
    //                    ? new Integer(Math.Abs(num.IntegralValue))
    //                    : num
    //                : negate
    //                    ? new Integer(Math.Abs(num.IntegralValue / gcd.IntegralValue))
    //                    : new Integer(num.IntegralValue / gcd.IntegralValue);

    //            Denominator = gcd.NumEqual(One)
    //                ? negate
    //                    ? new Integer(-1 * Math.Abs(denom.IntegralValue))
    //                    : denom
    //                : negate
    //                    ? new Integer(-1 * Math.Abs(denom.IntegralValue / gcd.IntegralValue))
    //                    : new Integer(denom.IntegralValue / gcd.IntegralValue);
    //        }

    //        public override string ToPrinted() => $"{Numerator}/{Denominator}";
    //    }

    //    internal class Integer : Common, IIntegral
    //    {
    //        public bool ExactValue { get => true; }
    //        public IReal RealPart { get => this; }
    //        public IReal ImaginaryPart { get => Zero; }
    //        public double RealValue { get => IntegralValue; }
    //        public IIntegral Numerator { get => this; }
    //        public IIntegral Denominator { get => One; }
    //        public int IntegralValue { get; private set; }

    //        public Integer(int value) => IntegralValue = value;

    //        public override string ToPrinted() => IntegralValue.ToString();
    //    }

    //    //internal class Natural : Common, INatural
    //    //{
    //    //    public uint NaturalValue { get; private set; }

    //    //    public Natural(uint value) : base(true) => NaturalValue = value;
    //    //    public Natural(int value)
    //    //    {
    //    //        if (value < 0) throw new ArgumentOutOfRangeException($"Can't construct natural number from {value}");
    //    //        NaturalValue = (uint)value;
    //    //    }
    //    //}


    //    internal static class Arithmetic
    //    {
    //        #region Complex Numbers
    //        public static Complex Add(this Complex a, IComplex b) => new Complex(a.RealPart.Add(b.RealPart), a.ImaginaryPart.Add(b.ImaginaryPart));
    //        public static Complex Subtract(this Complex a, IComplex b) => new Complex(a.RealPart.Subtract(b.RealPart), a.ImaginaryPart.Subtract(b.ImaginaryPart));
    //        public static Complex Multiply(this IComplex a, IComplex b) => new Complex(
    //            a.RealPart.Multiply(b.RealPart).Subtract(a.ImaginaryPart.Multiply(b.ImaginaryPart)),
    //            a.RealPart.Multiply(b.ImaginaryPart).Add(a.ImaginaryPart.Multiply(b.RealPart)));
    //        public static Complex Quotient(this IComplex a, IComplex b) => new Complex(
    //            )
    //        #endregion


    //        #region Real Numbers
    //        public static Real Add(this IReal a, IReal b) => new Real(a.RealValue + b.RealValue);
    //        public static Real Subtract(this IReal a, IReal b) => new Real(a.RealValue - b.RealValue);
    //        public static Real Multiply(this IReal a, IReal b) => new Real(a.RealValue * b.RealValue);
    //        public static Real Quotient(this IReal a, IReal b) => new Real(a.RealValue / b.RealValue);
    //        public static Real Remainder(this IReal a, IReal b) => new Real(a.RealValue - double.Truncate(a.RealValue / b.RealValue) * b.RealValue);
    //        public static Real Modulo(this IReal a, IReal b) => new Real(a.RealValue % b.RealValue);
    //        #endregion


    //        #region Rational Numbers
    //        public static Rational Add(this IRational a, IRational b) => new Rational(a.Numerator.Add(b.Numerator), LCM(a.Denominator, b.Denominator));
    //        public static Rational Subtract(this IRational a, IRational b) => new Rational(a.Numerator.Subtract(b.Numerator), LCM(a.Denominator, b.Denominator));
    //        public static Rational Multiply(this IRational a, IRational b) => new Rational(a.Numerator.Multiply(b.Numerator), a.Denominator.Multiply(b.Denominator));
    //        public static Rational Quotient(this IRational a, IRational b) => new Rational(a.Numerator.Multiply(b.Denominator), a.Denominator.Multiply(b.Numerator));

    //        #endregion


    //        #region Integral Numbers
    //        public static Integer Add(this IIntegral a, IIntegral b) => new Integer(a.IntegralValue + b.IntegralValue);
    //        public static Integer Subtract(this IIntegral a, IIntegral b) => new Integer(a.IntegralValue - b.IntegralValue);
    //        public static Integer Multiply(this IIntegral a, IIntegral b) => new Integer(a.IntegralValue * b.IntegralValue);
    //        public static Rational Quotient(this IIntegral a, IIntegral b) => new Rational(a, b);
    //        public static Integer Remainder(this IIntegral a, IIntegral b) => Modulo(a, b);
    //        public static Integer Modulo(this IIntegral a, IIntegral b) => new Integer(a.IntegralValue % b.IntegralValue);

    //        public static bool LessThan(this IIntegral a, IIntegral b) => a.IntegralValue < b.IntegralValue;
    //        public static bool NumEqual(this IIntegral a, IIntegral b) => a.IntegralValue == b.IntegralValue;
    //        #endregion


    //        #region Other Ops

    //        private static int IntegralGCD(int a, int b)
    //        {
    //            while (b != 0)
    //            {
    //                if (b > 0)
    //                {
    //                    int temp = a;
    //                    a = b;
    //                    b = temp;
    //                }
    //                else
    //                {
    //                    a -= b;
    //                }
    //            }

    //            return a;
    //        }

    //        public static IIntegral GCD(IIntegral x, IIntegral y)
    //        {
    //            return new Integer(IntegralGCD(x.IntegralValue, y.IntegralValue));
    //        }

    //        public static IIntegral LCM(IIntegral x, IIntegral y)
    //        {
    //            return new Integer(x.IntegralValue * y.IntegralValue / IntegralGCD(x.IntegralValue, y.IntegralValue));
    //        }

    //        #endregion
    //}
}
