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
        public override string ToSerialized() => Pair.MakeList(Symbol.Lambda, Parameters, Body).ToSerialized();
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

        private static void Define(string name, Func<int, int, int> op)
            => Define(name, (a, b) => new Number(op(a.Expect<Number>().Value, b.Expect<Number>().Value)));

        private static void Define(string name, Func<bool, bool, bool> op)
            => Define(name, (a, b) => Boolean.Judge(op(a.IsTrue, b.IsTrue)));

        private static void Define(string name, Func<double, double, bool> op)
            => Define(name, p => Boolean.Judge(op(p.Car.Expect<Number>().Value, p.Cadr.Expect<Number>().Value)));

        #endregion

        static PrimitiveProcedure()
        {
            //special form list ops
            Define("car", p => p.Caar);
            Define("cdr", p => p.Cadar);
            Define("cons", p => Pair.Cons(p.Car, p.Cadr));

            Define("set-car", p => p.Car.SetCar(p.Cadr));
            Define("set-cdr", p => p.Car.SetCdr(p.Cadr));

            //arithmetic ops
            Define("+", p => Pair.Fold((a, b) => new Number(a.Expect<Number>().Value + b.Expect<Number>().Value), Number.Zero, p));
            Define("-", p => p.Cdr.IsNil
                ? new Number(p.Car.Expect<Number>().Value * -1)
                : Pair.Fold((a, b) => new Number(p.Car.Expect<Number>().Value - p.Cadr.Expect<Number>().Value), Number.Zero, p));
            Define("*", p => Pair.Fold((a, b) => new Number(a.Expect<Number>().Value * b.Expect<Number>().Value), Number.One, p));
            Define("quotient", (a, b) => a / b);
            Define("modulo", (a, b) => a % b);

            //object equivalence
            Define("eq?", p => Pred_Eq(p.Car, p.Cadr));
            Define("eqv?", p => Pred_Eqv(p.Car, p.Cadr));
            Define("equal?", p => Pred_Equal(p.Car, p.Cadr));

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
            Define("number?", x => x is Number);
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
