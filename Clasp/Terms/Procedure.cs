using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal abstract class Procedure : Atom { }

    internal class CompoundProcedure : Procedure
    {
        public readonly Pair Parameters;
        public readonly Environment Closure;
        public readonly Expression Body;

        public CompoundProcedure(Pair parameters, Environment closure, Expression body)
        {
            Parameters = parameters;
            Closure = closure;
            Body = body;
        }

        public override string ToPrinted() => $"<lambda {Parameters} {Body}>";
        public override string ToSerialized() => Pair.Cons(Symbol.Lambda, Pair.Cons(Parameters, Body)).ToSerialized();
    }

    internal class PrimitiveProcedure : Procedure
    {
        private readonly string _name;

        private readonly Func<Pair, Expression> _operation;

        private PrimitiveProcedure(string name, Func<Pair, Expression> op)
        {
            _name = name;
            _operation = op;
        }

        public Expression Apply(Pair args)
        {
            return _operation(args);
        }

        public override string ToPrinted() => $"<{_name}>";
        public override string ToSerialized() => _name;

        #region Native Operations

        public static Dictionary<string, PrimitiveProcedure> NativeOps = new();

        #region Definition Shorthand
        private static void Define(string name, Func<Pair, Expression> op)
            => NativeOps.Add(name, new PrimitiveProcedure(name, op));

        private static void Define(string name, Func<Expression, Expression, Expression> op)
            => Define(name, p => op(p.Car, p.Cadr));

        private static void Define(string name, Func<Expression, bool> op)
            => Define(name, p => Boolean.Judge(op(p.Car)));

        private static void Define(string name, Func<bool, bool, bool> op)
            => Define(name, (a, b) => Boolean.Judge(op(a.IsTrue, b.IsTrue)));

        private static void Define(string name, Func<decimal, decimal, decimal> op)
            => Define(name, (a, b) => new PrimitiveNumber(op(a.Expect<PrimitiveNumber>().Value, b.Expect<PrimitiveNumber>().Value)));

        private static void Define(string name, Func<decimal, decimal, bool> op)
            => Define(name, p => Boolean.Judge(op(p.Car.Expect<PrimitiveNumber>().Value, p.Cadr.Expect<PrimitiveNumber>().Value)));

        #endregion

        static PrimitiveProcedure()
        {
            Define("gensym", p => new GenSym());

            //special form list ops
            Define("car", p => p.Caar);
            Define("cdr", p => p.Cadar);
            Define("cons", p => Pair.Cons(p.Car, p.Cadr));

            Define("set-car", p => p.Car.SetCar(p.Cadr));
            Define("set-cdr", p => p.Car.SetCdr(p.Cadr));

            //arithmetic ops
            Define("+", p => Pair.Fold((a, b) => new PrimitiveNumber(a.Expect<PrimitiveNumber>().Value + b.Expect<PrimitiveNumber>().Value), PrimitiveNumber.Zero, p));
            Define("-", p => p.Cdr.IsNil
                ? new PrimitiveNumber(p.Car.Expect<PrimitiveNumber>().Value * -1)
                : new PrimitiveNumber(p.Car.Expect<PrimitiveNumber>().Value - p.Cadr.Expect<PrimitiveNumber>().Value));
            Define("*", p => Pair.Fold((a, b) => new PrimitiveNumber(a.Expect<PrimitiveNumber>().Value * b.Expect<PrimitiveNumber>().Value), PrimitiveNumber.One, p));
            Define("quotient", (a, b) => a / b);
            Define("modulo", (a, b) => a % b);

            //object equivalence
            Define("eq?", p => p.Car.Eq(p.Cadr));
            Define("eqv?", p => p.Car.Eqv(p.Cadr));
            Define("equal?", p => p.Car.Equal(p.Cadr));

            //ordering/comparison
            Define("<", (a, b) => a < b);
            Define("<=", (a, b) => a <= b);
            Define(">=", (a, b) => a >= b);
            Define(">", (a, b) => a > b);

            //type predicates
            Define("atom?", x => x.IsAtom);
            Define("null?", x => x.IsNil);
            Define("pair?", x => x is Pair);
            Define("symbol?", x => x is Symbol);
            Define("procedure?", x => x is Procedure or SpecialFormRef);
            Define("vector?", x => x is Vector);
            Define("boolean?", x => x is Boolean);
            Define("number?", x => x is PrimitiveNumber);
        }

        #endregion
    }

    internal class Macro : Procedure
    {
        private readonly string _name;
        public readonly Pair Transformers;
        public readonly Environment Closure;

        public Macro(string name, Pair transformers, Environment closure)
        {
            _name = name;
            Transformers = transformers;
            Closure = closure;
        }

        public override string ToPrinted() => $"[macro '{_name}']";
        public override string ToSerialized() => throw new NotImplementedException(); //idk lol

    }
}
