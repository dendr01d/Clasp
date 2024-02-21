using System.Diagnostics;

namespace Clasp
{
    internal abstract class Expression
    {
        protected Expression() { }

        public static readonly Symbol TrueValue = new("#t");
        public static readonly Symbol FalseValue = new("#f");
        public static readonly Empty Nil = new();

        #region Logical Type-Checking

        public abstract bool IsAtom { get; }
        public abstract bool IsList { get; }
        public bool IsFalse => ReferenceEquals(this, FalseValue) || IsNil;
        public bool IsTrue => !IsFalse;
        public bool IsNil => ReferenceEquals(this, Nil);

        public bool IsSymbol => this is Symbol;
        public bool IsProcedure => this is Procedure || (this is Symbol sym && SpecialForm.IsSpecialKeyword(sym));
        public bool IsNumber => this is Number;
        public bool IsPair => this is Pair;

        #endregion

        #region Runtime Type-Checking

        [DebuggerStepThrough]
        public Symbol AsSymbol() => ExpectDerived<Symbol>();
        [DebuggerStepThrough]
        public Procedure AsProc() => ExpectDerived<Procedure>();
        [DebuggerStepThrough]
        public Number AsNumber() => ExpectDerived<Number>();
        [DebuggerStepThrough]
        public SList AsList() => ExpectDerived<SList>();

        [DebuggerStepThrough]
        private T ExpectDerived<T>() where T : Expression
        {
            if (this is T derived)
            {
                return derived;
            }
            else
            {
                throw new ExpectedTypeException(typeof(T).Name, this.ToString());
            }
        }

        [DebuggerStepThrough]
        public virtual Expression Car() => ExpectDerived<SList>();
        [DebuggerStepThrough]
        public virtual Expression Cdr() => ExpectDerived<SList>();

        #endregion

        public abstract bool EqualsByValue(Expression? other);

        #region Default Conversions

        public static implicit operator Expression(double d) => new Number(d);
        public static implicit operator Expression(int i) => new Number(i);
        public static implicit operator Expression(char c) => new Character(c);
        public static implicit operator Expression(string s) => new Symbol(s);
        public static implicit operator Expression(bool b) => b ? TrueValue : FalseValue;

        #endregion

        public abstract Expression Evaluate(Environment env);

        public abstract override string ToString();

        public virtual string ToStringent() => ToString();
    }
}
