using System.Diagnostics;

namespace Clasp
{
    internal abstract record class Expression()
    {

        #region Class-Specific Methods
        public abstract bool IsAtom { get; }
        public abstract bool IsList { get; }
        protected abstract Recurrence Evaluate(Environment env);
        protected abstract string FormatString();
        #endregion
        public sealed override string ToString() => FormatString();


        public static readonly Symbol TrueValue = new("#t");
        public static readonly Symbol FalseValue = new("#f");
        public static readonly Empty Nil = new();

        #region Logical Checking

        public bool IsNil => ReferenceEquals(this, Nil);
        public bool IsFalse => ReferenceEquals(this, FalseValue) || IsNil;
        public bool IsTrue => !IsFalse;

        [DebuggerStepThrough]
        public Expression GetCar() => The<SList>().Car;

        [DebuggerStepThrough]
        public Expression GetCdr() => The<SList>().Cdr;

        [DebuggerStepThrough]
        public bool IsA<T>() where T : Expression => this is T;

        [DebuggerStepThrough]
        public T The<T>()
            where T : Expression
        {
            if (this is T expr)
            {
                return expr;
            }
            else
            {
                throw new ExpectedTypeException(typeof(T).Name, ToString());
            }
        }

        #endregion


        #region Default Conversions

        public static implicit operator Expression(double d) => new Number(d);
        public static implicit operator Expression(int i) => new Number(i);
        public static implicit operator Expression(char c) => new Character(c);
        public static implicit operator Expression(string s) => new Symbol(s);
        public static implicit operator Expression(bool b) => b ? TrueValue : FalseValue;

        #endregion


        #region Evaluation Helpers

        public Expression CallEval(Environment env)
        {
            Recurrence recur = Evaluate(env);

            while (recur.NextFunc is not null && recur.NextEnv is not null)
            {
                recur = recur.NextFunc(recur.Result, recur.NextEnv);
            }

            return recur.Result;
        }

        protected static Recurrence StdEval(Expression expr, Environment env)
        {
            return expr.Evaluate(env);
        }

        protected static Recurrence ContinueWith(Expression expr, Environment env, Continuation cont)
        {
            return new Recurrence(expr, env, cont);
        }

        protected static Recurrence FinishedEval(Expression expr)
        {
            return new Recurrence(expr, null, null);
        }

        #endregion
    }
}
