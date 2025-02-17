using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Exceptions;

namespace Clasp.Ops
{
    internal class Math
    {
        #region Comparison

        public static Boolean Equivalent(Number n1, Number n2)
        {
            return (n1, n2) switch
            {
                (IntegralNumeric z1, IntegralNumeric z2) => z1.AsInteger == z2.AsInteger && z1.IsExact == z2.IsExact,
                (RationalNumeric q1, RationalNumeric q2) => Equivalent(q1.Numerator, q2.Numerator) && Equivalent(q1.Denominator, q2.Denominator),
                (RealNumeric r1, RealNumeric r2) => r1.AsFloatingPoint == r2.AsFloatingPoint && r1.IsExact == r2.IsExact,
                (ComplexNumeric c1, ComplexNumeric c2) => Equivalent(c1.RealPart, c2.RealPart) && Equivalent(c1.ImaginaryPart, c2.ImaginaryPart),
                _ => false
            };
        }

        public static Boolean LessThan(RealNumeric r1, RealNumeric r2) => r1.AsFloatingPoint < r2.AsFloatingPoint;
        public static Boolean LessThanOrEqual(RealNumeric r1, RealNumeric r2) => r1.AsFloatingPoint <= r2.AsFloatingPoint;

        public static Boolean GreaterThan(RealNumeric r1, RealNumeric r2) => r1.AsFloatingPoint > r2.AsFloatingPoint;
        public static Boolean GreaterThanOrEqual(RealNumeric r1, RealNumeric r2) => r1.AsFloatingPoint >= r2.AsFloatingPoint;

        #endregion

        #region Predicates

        public static Boolean IsZero(ComplexNumeric c) => c.RealPart.AsFloatingPoint == 0 && c.ImaginaryPart.AsFloatingPoint == 0;
        public static Boolean IsPositive(RealNumeric r) => r.RealPart.AsFloatingPoint > 0;
        public static Boolean IsNegative(RealNumeric r) => r.RealPart.AsFloatingPoint < 0;
        public static Boolean IsOdd(Number n) => !Equivalent(Modulo(n, Number.Two), Number.Zero);
        public static Boolean IsEven(Number n) => Equivalent(Modulo(n, Number.Two), Number.Zero);

        #endregion

        #region Addition

        public static IntegralNumeric AddIntegers(IntegralNumeric z1, IntegralNumeric z2)
            => new Integer(z1.AsInteger + z2.AsInteger, z1.IsExact && z2.IsExact);

        public static RationalNumeric AddRationals(RationalNumeric q1, RationalNumeric q2)
            => Rational.Reduce(
                AddIntegers(
                    MultiplyIntegers(q1.Numerator, q2.Denominator),
                    MultiplyIntegers(q2.Numerator, q1.Denominator)),
                MultiplyIntegers(q1.Denominator, q2.Denominator));

        public static RealNumeric AddReals(RealNumeric r1, RealNumeric r2)
            => new Real(r1.AsFloatingPoint + r2.AsFloatingPoint, r1.IsExact && r2.IsExact);

        public static ComplexNumeric AddComplices(ComplexNumeric c1, ComplexNumeric c2)
            => new Complex(
                    AddReals(c1.RealPart, c2.RealPart),
                    AddReals(c1.ImaginaryPart, c2.ImaginaryPart));

        public static Number Add(Number n1, Number n2)
        {
            return (n1, n2) switch
            {
                (IntegralNumeric z1, IntegralNumeric z2) => AddIntegers(z1, z2).Downcast(),
                (RationalNumeric q1, RationalNumeric q2) => AddRationals(q1, q2).Downcast(),
                (RealNumeric r1, RealNumeric r2) => AddReals(r1, r2).Downcast(),
                (ComplexNumeric c1, ComplexNumeric c2) => AddComplices(c1, c2).Downcast(),
                _ => throw new ProcessingException.UnknownNumericType(n1, n2)
            };
        }

        public static Number AddVar(params Number[] numbers)
        {
            return Helpers.Fold<Number, Number>(Number.Zero, Add, numbers);
        }

        #endregion

        #region Subtraction

        public static Number Negate(Number n) => Multiply(n, Number.NegativeOne);

        public static IntegralNumeric SubtractIntegers(IntegralNumeric z1, IntegralNumeric z2)
            => new Integer(z1.AsInteger - z2.AsInteger, z1.IsExact && z2.IsExact);

        public static RationalNumeric SubtractRationals(RationalNumeric q1, RationalNumeric q2)
            => AddRationals(q1, MultiplyRationals(q2, Number.NegativeOne));

        public static RealNumeric SubtractReals(RealNumeric r1, RealNumeric r2)
            => new Real(r1.AsFloatingPoint - r2.AsFloatingPoint, r1.IsExact && r2.IsExact);

        public static ComplexNumeric SubtractComplices(ComplexNumeric c1, ComplexNumeric c2)
            => AddComplices(c1, MultiplyComplices(c2, Number.NegativeOne));

        public static Number Subtract(Number n1, Number n2)
        {
            return (n1, n2) switch
            {
                (IntegralNumeric z1, IntegralNumeric z2) => SubtractIntegers(z1, z2).Downcast(),
                (RationalNumeric q1, RationalNumeric q2) => SubtractRationals(q1, q2).Downcast(),
                (RealNumeric r1, RealNumeric r2) => SubtractReals(r1, r2).Downcast(),
                (ComplexNumeric c1, ComplexNumeric c2) => SubtractComplices(c1, c2).Downcast(),
                _ => throw new ProcessingException.UnknownNumericType(n1, n2)
            };
        }

        public static Number SubtractVar(params Number[] numbers)
            => Subtract(numbers[0], AddVar(numbers[1..]));

        #endregion

        #region Multiplication

        public static IntegralNumeric MultiplyIntegers(IntegralNumeric z1, IntegralNumeric z2)
            => new Integer(z1.AsInteger * z2.AsInteger, z1.IsExact && z2.IsExact);

        public static RationalNumeric MultiplyRationals(RationalNumeric q1, RationalNumeric q2)
            => Rational.Reduce(
                MultiplyIntegers(q1.Numerator, q2.Numerator),
                MultiplyIntegers(q1.Denominator, q2.Denominator));

        public static RealNumeric MultiplyReals(RealNumeric r1, RealNumeric r2)
            => new Real(r1.AsFloatingPoint * r2.AsFloatingPoint, r1.IsExact && r2.IsExact);

        public static ComplexNumeric MultiplyComplices(ComplexNumeric c1, ComplexNumeric c2)
            => new Complex(
                SubtractReals(
                    MultiplyReals(c1.RealPart, c2.RealPart),
                    MultiplyReals(c1.ImaginaryPart, c2.ImaginaryPart)),
                AddReals(
                    MultiplyReals(c1.RealPart, c2.ImaginaryPart),
                    MultiplyReals(c1.ImaginaryPart, c2.RealPart)));

        public static Number Multiply(Number n1, Number n2)
        {
            return (n1, n2) switch
            {
                (IntegralNumeric z1, IntegralNumeric z2) => MultiplyIntegers(z1, z2).Downcast(),
                (RationalNumeric q1, RationalNumeric q2) => MultiplyRationals(q1, q2).Downcast(),
                (RealNumeric r1, RealNumeric r2) => MultiplyReals(r1, r2).Downcast(),
                (ComplexNumeric c1, ComplexNumeric c2) => MultiplyComplices(c1, c2).Downcast(),
                _ => throw new ProcessingException.UnknownNumericType(n1, n2)
            };
        }

        public static Number MultiplyVar(params Number[] numbers)
        {
            return Helpers.Fold<Number, Number>(Number.One, Multiply, numbers);
        }

        #endregion

        #region Division

        public static Number Invert(Number n) => Divide(Number.One, n);

        public static RationalNumeric DivideIntegers(IntegralNumeric z1, IntegralNumeric z2)
            => Rational.Reduce(z1, z2);

        public static RationalNumeric DivideRationals(RationalNumeric q1, RationalNumeric q2)
            => Rational.Reduce(
                MultiplyIntegers(q1.Numerator, q2.Denominator),
                MultiplyIntegers(q1.Denominator, q2.Numerator));

        public static RealNumeric DivideReals(RealNumeric r1, RealNumeric r2)
            => new Real(r1.AsFloatingPoint / r2.AsFloatingPoint, r1.IsExact && r2.IsExact);

        public static ComplexNumeric DivideComplices(ComplexNumeric c1, ComplexNumeric c2)
        {
            RealNumeric denominator = AddReals(
                MultiplyReals(c2.RealPart, c2.RealPart),
                MultiplyReals(c2.ImaginaryPart, c2.ImaginaryPart));

            return new Complex(
                DivideReals(
                    AddReals(
                        MultiplyReals(c1.RealPart, c2.RealPart),
                        MultiplyReals(c1.ImaginaryPart, c2.ImaginaryPart)),
                    denominator),
                DivideReals(
                    SubtractReals(
                        MultiplyReals(c1.ImaginaryPart, c2.RealPart),
                        MultiplyReals(c1.RealPart, c2.ImaginaryPart)),
                    denominator));
        }

        public static Number Divide(Number n1, Number n2)
        {
            return (n1, n2) switch
            {
                (IntegralNumeric z1, IntegralNumeric z2) => DivideIntegers(z1, z2),
                (RationalNumeric q1, RationalNumeric q2) => DivideRationals(q1, q2),
                (RealNumeric r1, RealNumeric r2) => DivideReals(r1, r2),
                (ComplexNumeric c1, ComplexNumeric c2) => DivideComplices(c1, c2),
                _ => throw new ProcessingException.UnknownNumericType(n1, n2)
            };
        }
        public static Number DivideVar(params Number[] numbers)
            => Subtract(numbers[0], MultiplyVar(numbers[1..]));

        #endregion

        #region Truncation

        // It would be nice if I could lump these together more closely >:|

        public static Number Ceiling(Number n)
        {
            return n switch
            {
                (IntegralNumeric z) => z,
                //rationals get lumped in with reals
                (RealNumeric r) => new Real(System.Math.Ceiling(r.AsFloatingPoint)),
                (ComplexNumeric c) => new Complex(
                    new Real(System.Math.Ceiling(c.RealPart.AsFloatingPoint)),
                    new Real(System.Math.Ceiling(c.ImaginaryPart.AsFloatingPoint))),
                _ => throw new ProcessingException.UnknownNumericType(n)
            };
        }

        public static Number Floor(Number n)
        {
            return n switch
            {
                (IntegralNumeric z) => z,
                //rationals get lumped in with reals
                (RealNumeric r) => new Real(System.Math.Floor(r.AsFloatingPoint)),
                (ComplexNumeric c) => new Complex(
                    new Real(System.Math.Floor(c.RealPart.AsFloatingPoint)),
                    new Real(System.Math.Floor(c.ImaginaryPart.AsFloatingPoint))),
                _ => throw new ProcessingException.UnknownNumericType(n)
            };
        }

        public static Number Truncate(Number n)
        {
            return n switch
            {
                (IntegralNumeric z) => z,
                //rationals get lumped in with reals
                (RealNumeric r) => new Real(System.Math.Truncate(r.AsFloatingPoint)),
                (ComplexNumeric c) => new Complex(
                    new Real(System.Math.Truncate(c.RealPart.AsFloatingPoint)),
                    new Real(System.Math.Truncate(c.ImaginaryPart.AsFloatingPoint))),
                _ => throw new ProcessingException.UnknownNumericType(n)
            };
        }

        #endregion

        #region Composite Operations

        public static Number Modulo(Number n1, Number n2) => Subtract(n1, (Multiply(n2, (Floor(Divide(n1, n2))))));
        public static Number Remainder(Number n1, Number n2) => Subtract(n1, (Multiply(n2, (Truncate(Divide(n1, n2))))));

        public static Number AbsoluteValue(Number n)
        {
            return n switch
            {
                (RealNumeric r) => IsNegative(r) ? Multiply(Number.NegativeOne, r) : r,
                (ComplexNumeric c) => new Real(System.Math.Sqrt(
                    (c.RealPart.AsFloatingPoint * c.RealPart.AsFloatingPoint) +
                    (c.ImaginaryPart.AsFloatingPoint * c.ImaginaryPart.AsFloatingPoint)))
            };
        }

        #endregion

        #region Misc

        public static Number Exp(RealNumeric r) => new Real(System.Math.Exp(r.AsFloatingPoint));
        public static Number Log(RealNumeric r) => new Real(System.Math.Log(r.AsFloatingPoint));
        public static Number Sin(RealNumeric r) => new Real(System.Math.Sin(r.AsFloatingPoint));
        public static Number Cos(RealNumeric r) => new Real(System.Math.Cos(r.AsFloatingPoint));
        public static Number Tan(RealNumeric r) => new Real(System.Math.Tan(r.AsFloatingPoint));
        public static Number ASin(RealNumeric r) => new Real(System.Math.Asin(r.AsFloatingPoint));
        public static Number ACos(RealNumeric r) => new Real(System.Math.Acos(r.AsFloatingPoint));
        public static Number ATan(RealNumeric r) => new Real(System.Math.Atan(r.AsFloatingPoint));
        public static Number ATan(RealNumeric r1, RealNumeric r2) => new Real(System.Math.Atan2(r1.AsFloatingPoint, r2.AsFloatingPoint));

        public static Number Sqrt(RealNumeric r) => new Real(System.Math.Sqrt(r.AsFloatingPoint));

        #endregion

    }
}
