using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal class Math
    {
        #region Comparison

        public static Term Equivalent(Number n1, Number n2)
        {
            return (n1, n2) switch
            {
                (IntegralNumber z1, IntegralNumber z2) => z1.AsInteger == z2.AsInteger && z1.IsExact == z2.IsExact,
                (RationalNumber q1, RationalNumber q2) => Equivalent(q1.Numerator, q2.Numerator) && Equivalent(q1.Denominator, q2.Denominator),
                (RealNumber r1, RealNumber r2) => r1.AsFloatingPoint == r2.AsFloatingPoint && r1.IsExact == r2.IsExact,
                (ComplexNumber c1, ComplexNumber c2) => Equivalent(c1.RealPart, c2.RealPart) && Equivalent(c1.ImaginaryPart, c2.ImaginaryPart),
                _ => false
            };
        }

        public static Term LessThan(RealNumber r1, RealNumber r2) => r1.AsFloatingPoint < r2.AsFloatingPoint;
        public static Term LessThanOrEqual(RealNumber r1, RealNumber r2) => r1.AsFloatingPoint <= r2.AsFloatingPoint;

        public static Term GreaterThan(RealNumber r1, RealNumber r2) => r1.AsFloatingPoint > r2.AsFloatingPoint;
        public static Term GreaterThanOrEqual(RealNumber r1, RealNumber r2) => r1.AsFloatingPoint >= r2.AsFloatingPoint;

        #endregion

        #region Addition

        public static IntegralNumber AddIntegers(IntegralNumber z1, IntegralNumber z2)
            => new Integer(z1.AsInteger + z2.AsInteger, z1.IsExact && z2.IsExact);

        public static RationalNumber AddRationals(RationalNumber q1, RationalNumber q2)
            => Rational.Reduce(
                AddIntegers(
                    MultiplyIntegers(q1.Numerator, q2.Denominator),
                    MultiplyIntegers(q2.Numerator, q1.Denominator)),
                MultiplyIntegers(q1.Denominator, q2.Denominator));

        public static RealNumber AddReals(RealNumber r1, RealNumber r2)
            => new Real(r1.AsFloatingPoint + r2.AsFloatingPoint, r1.IsExact && r2.IsExact);

        public static ComplexNumber AddComplices(ComplexNumber c1, ComplexNumber c2)
            => new Complex(
                    AddReals(c1.RealPart, c2.RealPart),
                    AddReals(c1.ImaginaryPart, c2.ImaginaryPart));

        public static Number Add(Number n1, Number n2)
        {
            return (n1, n2) switch
            {
                (IntegralNumber z1, IntegralNumber z2) => AddIntegers(z1, z2).Downcast(),
                (RationalNumber q1, RationalNumber q2) => AddRationals(q1, q2).Downcast(),
                (RealNumber r1, RealNumber r2) => AddReals(r1, r2).Downcast(),
                (ComplexNumber c1, ComplexNumber c2) => AddComplices(c1, c2).Downcast(),
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

        public static IntegralNumber SubtractIntegers(IntegralNumber z1, IntegralNumber z2)
            => new Integer(z1.AsInteger - z2.AsInteger, z1.IsExact && z2.IsExact);

        public static RationalNumber SubtractRationals(RationalNumber q1, RationalNumber q2)
            => AddRationals(q1, MultiplyRationals(q2, Number.NegativeOne));

        public static RealNumber SubtractReals(RealNumber r1, RealNumber r2)
            => new Real(r1.AsFloatingPoint - r2.AsFloatingPoint, r1.IsExact && r2.IsExact);

        public static ComplexNumber SubtractComplices(ComplexNumber c1, ComplexNumber c2)
            => AddComplices(c1, MultiplyComplices(c2, Number.NegativeOne));

        public static Number Subtract(Number n1, Number n2)
        {
            return (n1, n2) switch
            {
                (IntegralNumber z1, IntegralNumber z2) => SubtractIntegers(z1, z2).Downcast(),
                (RationalNumber q1, RationalNumber q2) => SubtractRationals(q1, q2).Downcast(),
                (RealNumber r1, RealNumber r2) => SubtractReals(r1, r2).Downcast(),
                (ComplexNumber c1, ComplexNumber c2) => SubtractComplices(c1, c2).Downcast(),
                _ => throw new ProcessingException.UnknownNumericType(n1, n2)
            };
        }

        public static Number SubtractVar(params Number[] numbers)
            => Subtract(numbers[0], AddVar(numbers[1..]));

        #endregion

        #region Multiplication

        public static IntegralNumber MultiplyIntegers(IntegralNumber z1, IntegralNumber z2)
            => new Integer(z1.AsInteger * z2.AsInteger, z1.IsExact && z2.IsExact);

        public static RationalNumber MultiplyRationals(RationalNumber q1, RationalNumber q2)
            => Rational.Reduce(
                MultiplyIntegers(q1.Numerator, q2.Numerator),
                MultiplyIntegers(q1.Denominator, q2.Denominator));

        public static RealNumber MultiplyReals(RealNumber r1, RealNumber r2)
            => new Real(r1.AsFloatingPoint * r2.AsFloatingPoint, r1.IsExact && r2.IsExact);

        public static ComplexNumber MultiplyComplices(ComplexNumber c1, ComplexNumber c2)
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
                (IntegralNumber z1, IntegralNumber z2) => MultiplyIntegers(z1, z2).Downcast(),
                (RationalNumber q1, RationalNumber q2) => MultiplyRationals(q1, q2).Downcast(),
                (RealNumber r1, RealNumber r2) => MultiplyReals(r1, r2).Downcast(),
                (ComplexNumber c1, ComplexNumber c2) => MultiplyComplices(c1, c2).Downcast(),
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

        public static RationalNumber DivideIntegers(IntegralNumber z1, IntegralNumber z2)
            => Rational.Reduce(z1, z2);

        public static RationalNumber DivideRationals(RationalNumber q1, RationalNumber q2)
            => Rational.Reduce(
                MultiplyIntegers(q1.Numerator, q2.Denominator),
                MultiplyIntegers(q1.Denominator, q2.Numerator));

        public static RealNumber DivideReals(RealNumber r1, RealNumber r2)
            => new Real(r1.AsFloatingPoint / r2.AsFloatingPoint, r1.IsExact && r2.IsExact);

        public static ComplexNumber DivideComplices(ComplexNumber c1, ComplexNumber c2)
        {
            RealNumber denominator = AddReals(
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
                (IntegralNumber z1, IntegralNumber z2) => DivideIntegers(z1, z2),
                (RationalNumber q1, RationalNumber q2) => DivideRationals(q1, q2),
                (RealNumber r1, RealNumber r2) => DivideReals(r1, r2),
                (ComplexNumber c1, ComplexNumber c2) => DivideComplices(c1, c2),
                _ => throw new ProcessingException.UnknownNumericType(n1, n2)
            };
        }
        public static Number DivideVar(params Number[] numbers)
            => Subtract(numbers[0], MultiplyVar(numbers[1..]));

        #endregion

    }
}
