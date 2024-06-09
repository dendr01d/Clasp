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

        public Expression AsExpression() => Pair.List(Symbol.Lambda, Parameters, Body);
        public override string ToString()
        {
            return AsExpression().ToString();
        }

        #region Derived Operations

        public static Dictionary<string, string> DerivedOps = new();

        private static void Define(string name, string op) => DerivedOps.Add(name, op);

        static CompoundProcedure()
        {
            Define("caar", "(lambda (x) (car (car x)))");
            Define("cadr", "(lambda (x) (car (cdr x)))");
            Define("cdar", "(lambda (x) (cdr (car x)))");
            Define("cddr", "(lambda (x) (cdr (cdr x)))");

            Define("caaar", "(lambda (x) (car (car (car x))))");
            Define("caadr", "(lambda (x) (car (car (cdr x))))");
            Define("cadar", "(lambda (x) (car (cdr (car x))))");
            Define("caddr", "(lambda (x) (car (cdr (cdr x))))");

            Define("cdaar", "(lambda (x) (cdr (car (car x))))");
            Define("cdadr", "(lambda (x) (cdr (car (cdr x))))");
            Define("cddar", "(lambda (x) (cdr (cdr (car x))))");
            Define("cdddr", "(lambda (x) (cdr (cdr (cdr x))))");


            Define("foldl", "(lambda (ls op t) (if (null? ls) t (foldl (cdr ls) op (op t (car ls)))))");
            Define("foldr", "(lambda (ls op t) (if (null? ls) t (op (car ls) (foldr (cdr ls) op t))))");
            Define("mapcar", "(lambda (ls op) (if (null? ls) '() (cons (op (car ls)) (mapcar (cdr ls) op))))");

            Define("cond", "(lambda (first . rest) (if (or (eq? (car first) 'else) (true? (car first))) (cadr first) (if (null? rest) #error ('cond . rest))))");

            Define("true?", "(lambda (x) (if x #t #f))");
            Define("not?", "(lambda (x) (if x #f #t))");

            Define("append", "(lambda (ls t) (if (null? ls) t (cons (car ls) (append (cdr ls) t))))");
        }

        #endregion
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

        public override string ToString() => $"<{_name}>";

        #region Native Operations

        public static Dictionary<string, PrimitiveProcedure> NativeOps = new();

        #region Definition Shorthand
        private static void Define(string name, Func<Pair, Expression> op)
            => NativeOps.Add(name, new PrimitiveProcedure(name, op));

        private static void Define(string name, Func<Expression, Expression, Expression> op)
            => Define(name, p => op(p.Car, p.Cadr));

        private static void Define(string name, Func<Expression, bool> op)
            => Define(name, p => Boolean.Judge(op(p.Car)));

        private static void Define(string name, Func<double, double, double> op)
            => Define(name, (a, b) => new Number(op(a.Expect<Number>().Value, b.Expect<Number>().Value)));

        private static void Define(string name, Func<bool, bool, bool> op)
            => Define(name, (a, b) => Boolean.Judge(op(a.IsTrue, b.IsTrue)));

        private static void Define(string name, Func<double, double, bool> op)
            => Define(name, p => Boolean.Judge(op(p.Car.Expect<Number>().Value, p.Cadr.Expect<Number>().Value)));

        #endregion

        static PrimitiveProcedure()
        {
            Define("+", (a, b) => a + b);
            Define("-", p => p.Cdr.IsNil
                ? new Number(p.Car.Expect<Number>().Value * -1)
                : new Number(p.Car.Expect<Number>().Value - p.Cadr.Expect<Number>().Value));
            Define("*", (a, b) => a * b);
            Define("/", (a, b) => a / b);
            Define("mod", (a, b) => a % b);

            Define("&&", (a, b) => a && b);
            Define("||", (a, b) => a || b);

            Define("<", (a, b) => a < b);
            Define("<=", (a, b) => a <= b);
            Define("==", (double a, double b) => Equals(a, b));
            Define(">=", (a, b) => a >= b);
            Define(">", (a, b) => a > b);

            Define("car", p => p.Caar);
            Define("cdr", p => p.Cdar);
            Define("cons", p => Pair.Cons(p.Car, p.Cadr));

            Define("eq?", (a, b) => Boolean.Judge(ReferenceEquals(a, b)));

            Define("atom?", x => x.IsAtom);
            Define("null?", x => x.IsNil);
            Define("pair?", x => x is Pair);
            Define("symbol?", x => x is Symbol);
            Define("procedure?", x => x is Procedure);

        }

        #endregion
    }

    internal class Macro : Procedure
    {
        public readonly Pair Transformers;
        public readonly Environment Closure;

        public Macro(Pair transformers, Environment closure)
        {
            Transformers = transformers;
            Closure = closure;
        }

        public override string ToString()
        {
            return $"{{macro {Transformers}}}";
        }
    }
}
