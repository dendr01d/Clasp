using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal abstract class Number : Atom
    {
        public readonly bool Exact;

        protected Number(bool exact) => Exact = exact;

        public override string ToSerialized() => ToPrinted();



        #region Global Numeric Ops

        public abstract Number Add(Number n);
        public abstract Number Subtract(Number n);
        public abstract Number Multiply(Number n);
        public abstract Number Quotient(Number n);
        public abstract Number Remainer(Number n);
        public abstract Number Modulo(Number n);

        public abstract bool LessThan(Number n);
        public abstract bool NumEquals(Number n);

        public abstract Number Abs();
        public abstract Integer Floor();
        public abstract Integer Ceiling();
        public abstract Integer Truncate();

        #endregion

        #region Derived Ops

        public Number Min(Number n) => LessThan(n) ? this : n;
        public Number Max(Number n) => LessThan(n) ? n : this;

        #endregion

        #region Specialized Ops

        public static Integer GCD(Integer x, Integer y)
        {
            int a = int.Abs(x.Value);
            int b = int.Abs(y.Value);

            while (b != 0)
            {
                if (b > a)
                {
                    int temp = a;
                    a = b;
                    b = temp;
                }
                else
                {
                    a -= b;
                }
            }

            return a;
        }

        #endregion
    }

    #region Numerical Hierarchy

    internal abstract class ComplexNumber : Number
    {
        public ComplexNumber(bool exact) : base(exact) { }
        public abstract Complex AsComplex();
    }

    internal abstract class RealNumber : ComplexNumber
    {
        public RealNumber(bool exact) : base(exact) { }
        public abstract Real AsReal();
    }

    internal abstract class RationalNumber : RealNumber
    {
        public RationalNumber(bool exact) : base(exact) { }
        public abstract Rational AsRational();
    }

    internal abstract class IntegralNumber : RationalNumber
    {
        public IntegralNumber(bool exact) : base(exact) { }
        public abstract Integer AsInteger();
    }

    #endregion

    internal class Complex : ComplexNumber
    {
        public readonly Real RealPart;
        public readonly Real ImaginaryPart;

        public Complex (Real rp, Real ip) : base(rp.Exact && ip.Exact)
        {
            RealPart = rp;
            ImaginaryPart = ip;
        }

        public override Complex AsComplex() => this;

        public override string ToPrinted() => $"{RealPart} + {ImaginaryPart}i";
    }

    internal class Real : Number
    {
        public readonly double Value;

        public Real(double value, bool exact = true) : base(exact)
        {
            Value = value;
        }

        #region Conversions

        public static implicit operator Real(Rational r) => new Real((double)r.Numerator.Value / r.Denominator.Value, r.Exact);
        public static implicit operator Real(Integer i) => new Real(i.Value, i.Exact);

        public static implicit operator Real(double d) => new Real(d);
        public static implicit operator Real(int i) => new Real(i);

        #endregion

        public override string ToPrinted() => Value.ToString();
    }

    internal class Rational : Number
    {
        public readonly Integer Numerator;
        public readonly Integer Denominator;

        public Rational(Integer num, Integer denom, bool exact = true) : base(exact || (!exact && num.Exact && denom.Exact))
        {
            int gcd = GCD(num, denom);
            Numerator = num / gcd;
            Denominator = denom / gcd;
        }

        #region "Upcasting" Conversions

        public static implicit operator Rational(Integer i) => new Rational(i, 1);

        public static implicit operator Rational(double d)
        {
            const int DECA = 100000;
            return new Rational((int)Math.Truncate(d * DECA), DECA, false);
        }
        public static implicit operator Rational(int i) => new Rational(i, 1);

        #endregion

        public override string ToPrinted() => $"{Numerator}/{Denominator}";
    }

    internal class Integer : Number
    {
        public readonly int Value;

        public Integer (int value, bool exact = true) : base(exact)
        {
            Value = value;
        }

        #region Conversions

        public static implicit operator Integer(int i) => new Integer(i);
        public static implicit operator int(Integer i) => i.Value;

        #endregion

        public override string ToPrinted() => Value.ToString();
    }
}
