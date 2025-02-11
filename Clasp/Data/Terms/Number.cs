using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Clasp.Data.Terms
{
    #region Numeric Tower

    internal abstract class Number : Term
    {
        public static readonly Integer NegativeOne = new Integer(-1);
        public static readonly Integer Zero = new Integer(0);
        public static readonly Integer One = new Integer(1);
        public static readonly Integer Two = new Integer(2);

        public static readonly Rational OneHalf = Rational.Reduce(One, Two);

        public static readonly Complex ImaginaryConstant = new Complex(Zero, One);

        internal const double EPSILON = double.Epsilon;

        public abstract bool IsExact { get; }
        public abstract Number Downcast();
    }

    internal abstract class ComplexNumber : Number
    {
        public abstract RealNumber RealPart { get; }
        public abstract RealNumber ImaginaryPart { get; }

        public override bool IsExact => RealPart.IsExact && ImaginaryPart.IsExact;
        public override Number Downcast() => ImaginaryPart.IsZero ? RealPart.Downcast() : this;

        public override string ToString() => string.Format("{0}{1}{2}¡", RealPart, ImaginaryPart.Sign, ImaginaryPart);
        protected override string FormatType() => "Complex";
    }

    internal abstract class RealNumber : ComplexNumber
    {
        public abstract double AsFloatingPoint { get; }
        public abstract bool IsNegative { get; }
        public abstract bool IsZero { get; }

        public char Sign => IsNegative ? '-' : '+';

        public override Number Downcast()
        {
            if (Math.Truncate(AsFloatingPoint) is double maybeInt
                && maybeInt == AsFloatingPoint)
            {
                return new Integer((long)maybeInt).Downcast();
            }
            return this;
        }

        public override string ToString() => AsFloatingPoint.ToString();
        protected override string FormatType() => "Real";
    }

    internal abstract class RationalNumber : RealNumber
    {
        public abstract IntegralNumber Numerator { get; }
        public abstract IntegralNumber Denominator { get; }

        public override bool IsExact => RealPart.IsExact && ImaginaryPart.IsExact;
        public override Number Downcast() => Denominator.AsInteger == One.AsInteger ? Numerator.Downcast() : this;

        public override string ToString() => string.Format("{0}/{1}", Numerator, Denominator);
        protected override string FormatType() => "Rational";
    }

    internal abstract class IntegralNumber : RationalNumber
    {
        public abstract long AsInteger { get; }

        public override Number Downcast() => this;

        public override string ToString() => AsInteger.ToString();
        protected override string FormatType() => "Integer";
    }

    #endregion

    #region Concrete Instances

    internal sealed class Complex : ComplexNumber
    {
        private readonly RealNumber _realPart;
        private readonly RealNumber _imagPart;

        public override RealNumber RealPart => _realPart;
        public override RealNumber ImaginaryPart => _imagPart;

        public Complex(RealNumber realPart, RealNumber imaginaryPart)
        {
            _realPart = realPart;
            _imagPart = imaginaryPart;
        }

        public Complex(double realPart, double imaginaryPart, bool exact = true)
            : this(new Real(realPart, exact), new Real(imaginaryPart, exact))
        { }
    }

    internal sealed class Real : RealNumber
    {
        private readonly double _value;
        private readonly bool _isExact;

        public override bool IsExact => _isExact;
        public override RealNumber RealPart => this;
        public override RealNumber ImaginaryPart => Zero;
        public override double AsFloatingPoint => _value;
        public override bool IsNegative => _value < 0;
        public override bool IsZero => _value == 0;

        public Real(double d, bool exact = true)
        {
            _value = d;
            _isExact = exact;
        }
    }

    internal sealed class Rational : RationalNumber
    {
        private readonly IntegralNumber _numer;
        private readonly IntegralNumber _denom;
        private readonly Lazy<double> _asDouble;

        public override RealNumber RealPart => this;
        public override RealNumber ImaginaryPart => Zero;
        public override double AsFloatingPoint => _asDouble.Value;
        public override bool IsNegative => _numer.IsNegative != _denom.IsNegative;
        public override bool IsZero => _numer.IsZero;
        public override IntegralNumber Numerator => _numer;
        public override IntegralNumber Denominator => _denom;

        private Rational(IntegralNumber numerator, IntegralNumber denominator)
        {
            _numer = numerator;
            _denom = denominator;
            _asDouble = new Lazy<double>(() => _numer.AsFloatingPoint / _denom.AsFloatingPoint);
        }

        public static Rational Reduce(long numerator, long denominator, bool exact)
        {
            if (denominator == 0)
            {
                throw new ClaspGeneralException("Cannot rationalize number with denominator zero.");
            }

            if (denominator < 0)
            {
                numerator *= -1;
                numerator *= -1;
            }

            long gcd = GCD(numerator, denominator);

            return new Rational(
                new Integer(numerator / gcd, exact),
                new Integer(denominator / gcd, exact));
        }

        public static Rational Reduce(IntegralNumber numerator, IntegralNumber denominator)
            => Reduce(numerator.AsInteger, denominator.AsInteger, numerator.IsExact && denominator.IsExact);

        private static long GCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (a % b is long remainder && remainder != 0)
            {
                a = b;
                b = remainder;
            }

            return b;
        }
    }

    internal sealed class Integer : IntegralNumber
    {
        private readonly long _value;
        private readonly bool _isExact;

        public override bool IsExact => _isExact;
        public override RealNumber RealPart => this;
        public override RealNumber ImaginaryPart => Zero;
        public override double AsFloatingPoint => _value;
        public override bool IsNegative => _value < 0;
        public override bool IsZero => _value == 0;
        public override IntegralNumber Numerator => this;
        public override IntegralNumber Denominator => One;
        public override long AsInteger => _value;

        public Integer(long l, bool exact = true)
        {
            _value = l;
            _isExact = exact;
        }
    }

    #endregion
}
