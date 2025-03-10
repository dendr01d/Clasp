using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Clasp.Exceptions;

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

    internal abstract class ComplexNumeric : Number
    {
        public abstract RealNumeric RealPart { get; }
        public abstract RealNumeric ImaginaryPart { get; }

        public override bool IsExact => RealPart.IsExact && ImaginaryPart.IsExact;
        public override Number Downcast() => ImaginaryPart.AsFloatingPoint == 0 ? RealPart.Downcast() : this;

        public override string ToString() => string.Format("{0}{1}{2}¡", RealPart, ImaginaryPart.Sign, ImaginaryPart);
        protected override string FormatType() => "Complex";
    }

    internal abstract class RealNumeric : ComplexNumeric
    {
        public abstract double AsFloatingPoint { get; }
        public abstract bool IsNegative { get; }

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

    internal abstract class RationalNumeric : RealNumeric
    {
        public abstract IntegralNumeric Numerator { get; }
        public abstract IntegralNumeric Denominator { get; }

        public override bool IsExact => RealPart.IsExact && ImaginaryPart.IsExact;
        public override Number Downcast() => Denominator.AsInteger == One.AsInteger ? Numerator.Downcast() : this;

        public override string ToString() => string.Format("{0}/{1}", Numerator, Denominator);
        protected override string FormatType() => "Rational";
    }

    internal abstract class IntegralNumeric : RationalNumeric
    {
        public abstract long AsInteger { get; }

        public override Number Downcast() => this;

        public override string ToString() => AsInteger.ToString();
        protected override string FormatType() => "Integer";
    }

    #endregion

    #region Concrete Instances

    internal sealed class Complex : ComplexNumeric
    {
        private readonly RealNumeric _realPart;
        private readonly RealNumeric _imagPart;

        public override RealNumeric RealPart => _realPart;
        public override RealNumeric ImaginaryPart => _imagPart;

        public Complex(RealNumeric realPart, RealNumeric imaginaryPart)
        {
            _realPart = realPart;
            _imagPart = imaginaryPart;
        }

        public Complex(double realPart, double imaginaryPart, bool exact = true)
            : this(new Real(realPart, exact), new Real(imaginaryPart, exact))
        { }
    }

    internal sealed class Real : RealNumeric
    {
        private readonly double _value;
        private readonly bool _isExact;

        public override bool IsExact => _isExact;
        public override RealNumeric RealPart => this;
        public override RealNumeric ImaginaryPart => Zero;
        public override double AsFloatingPoint => _value;
        public override bool IsNegative => _value < 0;

        public Real(double d, bool exact = true)
        {
            _value = d;
            _isExact = exact;
        }
    }

    internal sealed class Rational : RationalNumeric
    {
        private readonly IntegralNumeric _numer;
        private readonly IntegralNumeric _denom;
        private readonly Lazy<double> _asDouble;

        public override RealNumeric RealPart => this;
        public override RealNumeric ImaginaryPart => Zero;
        public override double AsFloatingPoint => _asDouble.Value;
        public override bool IsNegative => _numer.IsNegative != _denom.IsNegative;
        public override IntegralNumeric Numerator => _numer;
        public override IntegralNumeric Denominator => _denom;

        private Rational(IntegralNumeric numerator, IntegralNumeric denominator)
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

        public static Rational Reduce(IntegralNumeric numerator, IntegralNumeric denominator)
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

    internal sealed class Integer : IntegralNumeric
    {
        private readonly long _value;
        private readonly bool _isExact;

        public override bool IsExact => _isExact;
        public override RealNumeric RealPart => this;
        public override RealNumeric ImaginaryPart => Zero;
        public override double AsFloatingPoint => _value;
        public override bool IsNegative => _value < 0;
        public override IntegralNumeric Numerator => this;
        public override IntegralNumeric Denominator => One;
        public override long AsInteger => _value;

        public Integer(long l, bool exact = true)
        {
            _value = l;
            _isExact = exact;
        }
    }

    #endregion
}
