namespace ClaspLib
{
    public interface IExpression
    {
        public bool IsAtomic { get; }
        public bool IsTrue { get; }
        public bool IsFalse { get; }

        public bool Pred_Eq(IExpression other);
        public bool Pred_Eqv(IExpression other);
        public bool Pred_Equal(IExpression other);

        public string Write();
        public string Display();
    }

    public interface IAtomic : IExpression
    {

    }

    public interface IEmpty : IAtomic
    {
        public IEmpty Nil { get; }
    }

    public interface ISymbol : IEmpty
    {

    }

    public interface ILiteral<T> : IAtomic
        where T : struct
    {
        public T Value { get; }
    }

    public interface IBoolean : ILiteral<bool> { }

    public interface ICharacter : ILiteral<char>
    {
        public bool CharEq(INumber num);
        public bool CharLt(INumber num);
        public bool CharGt(INumber num);
        public bool CharLeq(INumber num);
        public bool CharGeq(INumber num);
    }

    public interface INumber: IAtomic
    {
        public bool IsExact { get; }

        public INumber Add(INumber num);
        public INumber Subtract(INumber num);
        public INumber Multiply(INumber num);
        public INumber Divide(INumber num);

        public INumber Quotient(INumber num);
        public INumber Modulo(INumber num);
        public INumber Remainder(INumber num);

        public INumber Exponent(INumber num);

        public bool NumEq(INumber num);
        public bool NumLt(INumber num);
        public bool NumGt(INumber num);
        public bool NumLeq(INumber num);
        public bool NumGeq(INumber num);

        public INumber Abs();
        public INumber Truncate();
        public INumber Ceiling();
        public INumber Floor();
        public INumber Round();
    }

    public interface IComplexNumber : INumber
    {
        public INumber RealPart { get; }
        public INumber ImaginaryPart { get; }
    }

    public interface IRealNumber : IComplexNumber, ILiteral<double>
    {
        public double RealValue { get; }
    }

    public interface IRationalNumber : IRealNumber
    {
        public INumber Numerator { get; }
        public INumber Denominator { get; }
    }

    public interface IIntegralNumber : IRationalNumber, ILiteral<int>
    {
        public int IntegralValue { get; }
    }

    public interface IProcedure : IExpression
    {

    }

    public interface IPrimitiveProcedure : IExpression
    {

    }

    public interface ICompoundProcedure : IExpression
    {

    }

    public interface IConstruction : IExpression, IEnumerable<IExpression>
    {
        public IEnumerable<IExpression> Enumerate();
    }

    public interface IPair : IConstruction
    {
        public IExpression Car { get; }
        public IExpression Cdr { get; }

        public void SetCar(IExpression value);
        public void SetCdr(IExpression value);
    }

    public interface IVector : IConstruction
    {
        public int Length { get; } //?

        public IExpression Index(int i);

        public void SetIndex(int i, IExpression value);
    }

    public interface IString : IVector, IEnumerable<ICharacter>
    {

    }
}
