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

        public static T Calculate<T>(Expression x, Expression y, Func<SimpleNum, SimpleNum, T> calc) => calc(x.Expect<SimpleNum>(), y.Expect<SimpleNum>());

        public static SimpleNum Add(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value + y.Value);
        public static SimpleNum Subtract(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value - y.Value);
        public static SimpleNum Multiply(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value * y.Value);
        public static SimpleNum Divide(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value / y.Value);
        public static SimpleNum Quotient(SimpleNum x, SimpleNum y) => new SimpleNum(Math.Truncate(x.Value / y.Value));
        public static SimpleNum Modulo(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value - y.Value * Math.Floor(x.Value / y.Value));
        public static SimpleNum Remainder(SimpleNum x, SimpleNum y) => new SimpleNum(x.Value - y.Value * Math.Truncate(x.Value / y.Value));
        public static SimpleNum Exponent(SimpleNum x, SimpleNum y) => new SimpleNum((decimal)Math.Pow((double)x.Value, (double)y.Value));

        public static bool NumEquals(SimpleNum x, SimpleNum y) => x.Value == y.Value;
        public static bool LessThan(SimpleNum x, SimpleNum y) => x.Value < y.Value;
        public static bool GreatherThan(SimpleNum x, SimpleNum y) => x.Value > y.Value;
        public static bool Leq(SimpleNum x, SimpleNum y) => x.Value <= y.Value;
        public static bool Geq(SimpleNum x, SimpleNum y) => x.Value >= y.Value;

        public static SimpleNum Negate(SimpleNum x) => new SimpleNum(x.Value * -1);

        public static SimpleNum Abs(SimpleNum x) => x.Value < 0 ? Negate(x) : x;
        public static SimpleNum Truncate(SimpleNum x) => new SimpleNum(Math.Truncate(x.Value));
        public static SimpleNum Ceiling(SimpleNum x) => new SimpleNum(Math.Ceiling(x.Value));
        public static SimpleNum Floor(SimpleNum x) => new SimpleNum(Math.Floor(x.Value));
        public static SimpleNum Round(SimpleNum x, SimpleNum digits) => new SimpleNum(Math.Round(x.Value, (int)Math.Truncate(digits.Value)));

        public override string Write() => Value.ToString();
    }

    //internal abstract class Number : Atom
    //{
    //    #region Some Important Numbers

    //    public IntegralNumber Zero => new IntegralNumber(0);
    //    public IntegralNumber One => new IntegralNumber(1);
    //    public IntegralNumber NegOne => new IntegralNumber(-1);
    //    public IntegralNumber Two => new IntegralNumber(2);

    //    public RationalNumber OneHalf => RationalNumber.Make(One, Two);

    //    public ComplexNumber ImagConst => ComplexNumber.MakeRect(Zero, One);

    //    #endregion


    //    public readonly bool IsExact;
    //    protected Number(bool exact) => IsExact = exact;

    //    public abstract Number RealPart { get; }
    //    public abstract Number ImaginaryPart { get; }
    //    public abstract Number Numerator { get; }
    //    public abstract Number Denominator { get; }
    //    public abstract bool LeadingMinus { get; }

    //    public abstract Number Sink();

    //    public abstract Number Abs();
    //    public abstract Number Magnitude();
    //    public abstract Number Angle();

    //    public abstract Number Negate();
    //    public abstract Number Invert();

    //    public abstract Number Floor();
    //    public abstract Number Ceiling();
    //    public abstract Number Truncate();
    //    public abstract Number Round();

    //    public abstract Number Exp();
    //    public abstract Number Log();
    //    public abstract Number Sin();
    //    public abstract Number Cos();
    //    public abstract Number Tan();
    //    public abstract Number ArcSin();
    //    public abstract Number ArcCos();
    //    public abstract Number ArcTan();
    //    public abstract Number ArcTan2(Number other);

    //    public abstract Number Add(Number other);
    //    public abstract Number Subtract(Number other);
    //    public abstract Number Multiply(Number other);
    //    public abstract Number DivideBy(Number other);

    //    public abstract Number Quotient(Number other);
    //    public abstract Number Modulo(Number other);
    //    public abstract Number Remainder(Number other);

    //    public abstract Number Exponent(Number other);

    //    public abstract bool NumEquals(Number other);
    //    public abstract bool NumLt(Number other);
    //    public bool NumGt(Number other) => !NumLeq(other);
    //    public bool NumLeq(Number other) => NumEquals(other) || NumLt(other);
    //    public bool NumGeq(Number other) => NumEquals(other) || !NumEquals(other);


    //    public Number Squared => Multiply(this);
    //    public Number Sqrt => Exponent(OneHalf);

    //    public static Number GCD(params Number[] nums)
    //    {

    //    }

    //    public static Number LCM(params Number[] nums)
    //    {

    //    }
    //}

    //internal abstract class ComplexMember : Number
    //{
    //    protected ComplexMember(bool isExact) : base(isExact) { }

    //    public override Number Negate() => Multiply(NegOne);
    //    public override Number Invert() => One.DivideBy(this);
    //}

    //internal class ComplexNumber : ComplexMember
    //{
    //    private readonly Number _realPart;
    //    private readonly Number _imaginaryPart;

    //    public override Number RealPart => _realPart;
    //    public override Number ImaginaryPart => _imaginaryPart;
    //    public override Number Numerator => this;
    //    public override Number Denominator => One;
    //    public override bool LeadingMinus => _realPart.LeadingMinus;

    //    protected ComplexNumber(Number a, Number b) : base(a.IsExact && b.IsExact)
    //    {
    //        // a + bi
    //        // a = c + di
    //        // b = e + fi
    //        // (c + di) + (e + fi)i
    //        // = c + di + ei + (-1 * f)
    //        // = (c - f) + (e + d)i

    //        _realPart = a.RealPart.Subtract(b.ImaginaryPart);
    //        _imaginaryPart = b.RealPart.Add(a.ImaginaryPart);
    //    }

    //    public static ComplexNumber MakeRect(Number realPart, Number imaginaryPart) => new ComplexNumber(realPart, imaginaryPart);
    //    public static ComplexNumber MakePolar(Number magnitude, Number angle)
    //    {
    //        return new ComplexNumber(
    //            magnitude.Abs().Multiply(angle.Cos()),
    //            magnitude.Abs().Multiply(angle.Sin()));
    //    }

    //    public override Number Sink()
    //    {
    //        Number imag = _imaginaryPart.Sink();

    //        return imag.NumEquals(Zero)
    //            ? _realPart.Sink()
    //            : new ComplexNumber(_realPart.Sink(), imag);
    //    }

    //    public override Number Abs() => Magnitude();
    //    public override Number Magnitude() => (RealPart.Squared.Multiply(ImaginaryPart.Squared)).Sqrt;
    //    public override Number Angle() => (ImaginaryPart.DivideBy(RealPart)).ArcTan();

    //    public override Number Floor() => MakeRect(RealPart.Floor(), ImaginaryPart.Floor());
    //    public override Number Ceiling() => MakeRect(RealPart.Ceiling(), ImaginaryPart.Ceiling());
    //    public override Number Truncate() => MakeRect(RealPart.Truncate(), ImaginaryPart.Truncate());
    //    public override Number Round() => MakeRect(RealPart.Round(), ImaginaryPart.Round());

    //    // e^(a + bi) = e^a * e^bi = e^a * (cos(b) + sin(b)i)
    //    public override Number Exp() => RealPart.Exp().Multiply(ComplexNumber.MakeRect(ImaginaryPart.Cos(), ImaginaryPart.Sin()));
    //    public override Number Log() => throw new NotImplementedException("This complex math not supported");
    //    public override Number Sin() => throw new NotImplementedException("This complex math not supported");
    //    public override Number Cos() => throw new NotImplementedException("This complex math not supported");
    //    public override Number Tan() => throw new NotImplementedException("This complex math not supported");
    //    public override Number ArcSin() => throw new NotImplementedException("This complex math not supported");
    //    public override Number ArcCos() => throw new NotImplementedException("This complex math not supported");
    //    public override Number ArcTan() => throw new NotImplementedException("This complex math not supported");
    //    public override Number ArcTan2(Number other) => throw new NotImplementedException("This complex math not supported");

    //    public override Number Add(Number other) => MakeRect(RealPart.Add(other.RealPart), ImaginaryPart.Add(other.ImaginaryPart));
    //    public override Number Subtract(Number other) => MakeRect(RealPart.Subtract(other.RealPart), ImaginaryPart.Subtract(other.ImaginaryPart));
    //    public override Number Multiply(Number other) => MakeRect(
    //        (RealPart.Multiply(other.RealPart)).Subtract(ImaginaryPart.Multiply(other.ImaginaryPart)),
    //        (RealPart.Multiply(other.ImaginaryPart).Add(ImaginaryPart.Multiply(other.RealPart))));
    //    public override Number DivideBy(Number other) => MakeRect(
    //        (RealPart.Multiply(other.RealPart).Add(ImaginaryPart.Multiply(other.ImaginaryPart))).DivideBy(other.RealPart.Squared.Add(other.ImaginaryPart.Squared)),
    //        (ImaginaryPart.Multiply(other.RealPart).Subtract(RealPart.Multiply(other.ImaginaryPart))).DivideBy(other.RealPart.Squared.Add(other.ImaginaryPart.Squared)));

    //    public override Number Quotient(Number other) => throw new NotImplementedException("This complex math not supported");
    //    public override Number Modulo(Number other) => throw new NotImplementedException("This complex math not supported");
    //    public override Number Remainder(Number other) => throw new NotImplementedException("This complex math not supported");

    //    public override Number Exponent(Number other) => throw new NotImplementedException("This complex math not supported");

    //    public override bool NumEquals(Number other) => RealPart.NumEquals(other.RealPart) && RealPart.NumEquals(other.ImaginaryPart);
    //    public override bool NumLt(Number other) => throw new InvalidNumericOperationException(this, "less than");
    //}

    //internal abstract class RealMember : ComplexMember
    //{
    //    protected RealMember(bool isExact) : base(isExact) { }

    //    public override Number Abs() => NumLeq(Zero) ? Negate() : this;
    //    public override Number Magnitude() => Abs();
    //    public override Number Angle() => Zero;

    //    public override Number Exp();
    //    public override Number Log();
    //    public override Number Sin();
    //    public override Number Cos();
    //    public override Number Tan();
    //    public override Number ArcSin();
    //    public override Number ArcCos();
    //    public override Number ArcTan();
    //    public override Number ArcTan2();

    //    public override Number Add(Number other);
    //    public override Number Subtract(Number other);
    //    public override Number Multiply(Number other);
    //    public override Number DivideBy(Number other);

    //    public override Number Quotient(Number other);
    //    public override Number Modulo(Number other);
    //    public override Number Remainder(Number other);

    //    public override Number Exponent(Number other);

    //    public override bool NumEquals(Number other) => _realPart.NumEquals(other.RealPart) && _imaginaryPart.NumEquals(other.ImaginaryPart);
    //    public override bool NumLt(Number other) => throw new InvalidNumericOperationException(this, "less than");
    //}

    //internal class RealNumber : RealMember
    //{
    //    public readonly double RealValue;

    //    public override Number RealPart => this;
    //    public override Number ImaginaryPart => Zero;
    //    public override Number Numerator => this;
    //    public override Number Denominator => One;

    //    #region Constructors

    //    public RealNumber(double value, bool exact = true) : base(exact) => RealValue = value;

    //    #endregion
    //}

    //internal abstract class RationalMember : RealMember
    //{
    //    protected RationalMember(bool isExact) : base(isExact) { }
    //}

    //internal class RationalNumber : RationalMember
    //{
    //    private readonly IntegralNumber _numerator;
    //    private readonly IntegralNumber _denominator;

    //    public override Number RealPart => this;
    //    public override Number ImaginaryPart => Zero;
    //    public override Number Numerator => _numerator;
    //    public override Number Denominator => _denominator;

    //    #region Constructors

    //    private RationalNumber(Number a, Number b) : base(a.IsExact && b.IsExact)
    //    {

    //    }

    //    public static RationalNumber Make(Number numerator, Number Denominator) => new RationalNumber(numerator, Denominator);

    //    #endregion

    //    public override Number Sink();

    //    public override Number Abs();
    //    public override Number Magnitude();
    //    public override Number Angle();

    //    public override Number Negate();
    //    public override Number Invert();

    //    public override Number Floor();
    //    public override Number Ceiling();
    //    public override Number Truncate();
    //    public override Number Round();

    //    public override Number Exp();
    //    public override Number Log();
    //    public override Number Sin();
    //    public override Number Cos();
    //    public override Number Tan();
    //    public override Number ArcSin();
    //    public override Number ArcCos();
    //    public override Number ArcTan();
    //    public override Number ArcTan2();

    //    public override Number Add(Number other);
    //    public override Number Subtract(Number other);
    //    public override Number Multiply(Number other);
    //    public override Number DivideBy(Number other);

    //    public override Number Quotient(Number other);
    //    public override Number Modulo(Number other);
    //    public override Number Remainder(Number other);

    //    public override Number Exponent(Number other);

    //    public override bool NumEquals(Number other);
    //    public override bool NumLt(Number other);
    //}

    //internal abstract class IntegralMember : RationalMember
    //{
    //    protected IntegralMember(bool isExact) : base(isExact) { }
    //}

    //internal class IntegralNumber : IntegralMember
    //{
    //    public readonly int DiscreteValue;

    //    public override Number RealPart => this;
    //    public override Number ImaginaryPart => Zero;
    //    public override Number Numerator => this;
    //    public override Number Denominator => One;

    //    #region Constructors

    //    public IntegralNumber(int value, bool exact = true) : base(exact) => DiscreteValue = value;

    //    #endregion

    //    public override Number Sink() => this;

    //    public override Number Abs() => DiscreteValue < 0 ? Multiply(NegOne) : this;
    //    public override Number Magnitude() => Abs();
    //    public override Number Angle() => Zero;

    //    public override Number Negate();
    //    public override Number Invert();

    //    public override Number Floor();
    //    public override Number Ceiling();
    //    public override Number Truncate();
    //    public override Number Round();

    //    public override Number Exp();
    //    public override Number Log();
    //    public override Number Sin();
    //    public override Number Cos();
    //    public override Number Tan();
    //    public override Number ArcSin();
    //    public override Number ArcCos();
    //    public override Number ArcTan();
    //    public override Number ArcTan2();

    //    public override Number Add(Number other);
    //    public override Number Subtract(Number other);
    //    public override Number Multiply(Number other);
    //    public override Number DivideBy(Number other);

    //    public override Number Quotient(Number other);
    //    public override Number Modulo(Number other);
    //    public override Number Remainder(Number other);

    //    public override Number Exponent(Number other);

    //    public override bool NumEquals(Number other);
    //    public override bool NumLt(Number other);
    //}


}
