using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clasp
{
    internal abstract class Procedure : Atom
    {
        /// <summary>
        /// Indicates whether the procedure's arguments are evaluated BEFORE the application of the procedure itself
        /// </summary>
        public abstract bool ApplicativeOrder { get; }
    }

    internal class SpecialForm : Procedure
    {
        public readonly string Name;
        public readonly Action<Machine> InstructionPtr;
        public override bool ApplicativeOrder => false;

        public SpecialForm(string keyword, Action<Machine> ptr)
        {
            Name = keyword;
            InstructionPtr = ptr;
        }

        public override string ToPrinted() => $"sp_{Name}";
        public override string ToSerialized() => Name;
    }

    internal class CompoundProcedure : Procedure
    {
        public readonly Expression Parameters;
        public readonly Expression Body; //NOT Pair bc null bodies are allowed
        public readonly Environment Closure;
        public override bool ApplicativeOrder => true;

        public CompoundProcedure(Expression parameters, Expression body, Environment outerEnv)
        {
            Parameters = parameters;
            Body = body;
            Closure = outerEnv.Enclose();
        }

        public override string ToPrinted()
        {
            return Pair.Append(Pair.MakeList(Symbol.Lambda, Parameters), Body)
                .Expect<Pair>()
                .Format('<', '>');
        }
        public override string ToSerialized() => Pair.Cons(Symbol.Lambda, Pair.Cons(Parameters, Body)).ToSerialized();
    }

    internal class PrimitiveProcedure : Procedure
    {
        private readonly string _name;
        private readonly Func<Expression, Expression> _operation;
        public override bool ApplicativeOrder => true;

        private PrimitiveProcedure(string name, Func<Expression, Expression> op)
        {
            _name = name;
            _operation = op;
        }

        public Expression Apply(Expression args)
        {
            return _operation(args);
        }

        public override string ToPrinted() => $"<{_name}>";
        public override string ToSerialized() => _name;

        #region Native Operations

        public static Dictionary<string, PrimitiveProcedure> NativeOps = new();

        #region Definition Shorthand
        private static void Define(string name, Func<Expression, Expression> op)
            => NativeOps.Add(name, new PrimitiveProcedure(name, op));

        private static void DefinePrim<T1, T2>(string name, Func<T1, T2> op)
            where T1 : Expression
            where T2 : Expression
        {
            NativeOps.Add(name, new PrimitiveProcedure(name, p => op(p.Car.Expect<T1>())));
        }

        private static void DefinePrim<T1, T2, T3>(string name, Func<T1, T2, T3> op)
            where T1 : Expression
            where T2 : Expression
            where T3 : Expression
        {
            NativeOps.Add(name, new PrimitiveProcedure(name, p => op(p.Car.Expect<T1>(), p.Cadr.Expect<T2>())));
        }

        private static void DefineVariadic<T1, T2>(string name, Func<T1, T2, T1> op, T1 baseCase)
            where T1 : Expression
            where T2 : Expression
        {
            NativeOps.Add(name, new PrimitiveProcedure(name, p => Pair.Fold(op, baseCase, p)));
        }

        private static void DefinePred(string name, Func<Expression, bool> op)
        {
            NativeOps.Add(name, new PrimitiveProcedure(name, p => Boolean.Judge(op(p.Car))));
        }

        #endregion

        static PrimitiveProcedure()
        {
            Define("gensym", p => new GenSym());

            //special form list ops
            Define("car", p => p.Car.Car);
            Define("cdr", p => p.Car.Cdr);
            Define("cons", p => Pair.Cons(p.Car, p.Cadr));

            Define("set-car", p => p.Car.SetCar(p.Cadr));
            Define("set-cdr", p => p.Car.SetCdr(p.Cadr));

            //arithmetic ops
            DefineVariadic<SimpleNum, SimpleNum>("+", SimpleNum.Add, SimpleNum.Zero);
            Define("-", p => p.Cdr.IsNil
                ? SimpleNum.Negate(p.Car.Expect<SimpleNum>())
                : SimpleNum.Subtract(p.Car.Expect<SimpleNum>(), Pair.Fold<SimpleNum, SimpleNum>(SimpleNum.Add, SimpleNum.Zero, p.Cdr).Expect<SimpleNum>())
                );
                //: SimpleNum.Subtract(p.Car.Expect<SimpleNum>(), p.Cadr.Expect<SimpleNum>()));
            DefineVariadic<SimpleNum, SimpleNum>("*", SimpleNum.Multiply, SimpleNum.One);

            DefinePrim<SimpleNum, SimpleNum, SimpleNum>("quotient", SimpleNum.Quotient);
            DefinePrim<SimpleNum, SimpleNum, SimpleNum>("div", SimpleNum.IntDiv);
            DefinePrim<SimpleNum, SimpleNum, SimpleNum>("modulo", SimpleNum.Modulo);
            DefinePrim<SimpleNum, SimpleNum, SimpleNum>("remainder", SimpleNum.Remainder);
            DefinePrim<SimpleNum, SimpleNum, SimpleNum>("expt", SimpleNum.Exponent);
            DefinePrim<SimpleNum, SimpleNum>("abs", SimpleNum.Abs);

            //ordering/comparison
            DefinePrim<SimpleNum, SimpleNum, Boolean>("<",  SimpleNum.LessThan);
            DefinePrim<SimpleNum, SimpleNum, Boolean>("<=", SimpleNum.Leq);
            DefinePrim<SimpleNum, SimpleNum, Boolean>(">=", SimpleNum.Geq);
            DefinePrim<SimpleNum, SimpleNum, Boolean>(">",  SimpleNum.GreatherThan);

            //object equivalence
            Define("eq?", p => Boolean.Judge(Pred_Eq(p.Car, p.Cadr)));
            Define("eqv?", p => Boolean.Judge(Pred_Eqv(p.Car, p.Cadr)));
            Define("equal?", p => Boolean.Judge(Pred_Equal(p.Car, p.Cadr)));

            //type predicates
            DefinePred("atom?", x => x.IsAtom);
            DefinePred("null?", x => x.IsNil);
            DefinePred("list?", x => x.IsList);
            DefinePred("pair?", x => x is Pair);
            DefinePred("symbol?", x => x is Symbol);
            DefinePred("procedure?", x => x is Procedure);
            DefinePred("boolean?", x => x is Boolean);
            DefinePred("number?", x => x is SimpleNum);
            DefinePred("vector?", x => x is Vector);

            //type conversion
            DefinePrim<Charstring, SimpleNum>("string->number", s => new SimpleNum(decimal.Parse(s.Value)));
            DefinePrim<SimpleNum, Charstring>("number->string", n => new Charstring(n.Value.ToString()));
        }

        #endregion
    }

    internal class Macro : Procedure
    {
        public readonly Expression LiteralSymbols;
        public readonly Pair Transformers;
        public readonly Environment Closure;
        public override bool ApplicativeOrder => false;

        public Macro(Expression literals, Pair transformers, Environment closure)
        {
            LiteralSymbols = literals;
            Transformers = transformers;
            Closure = closure;
        }

        public bool TryTransform(Expression input, Environment local, out Expression output)
        {
            output = Undefined.Instance;

            foreach(SyntaxRule sr in Rules)
            {
                if (sr.TryTransform(input, LiteralSymbols, Closure, local, out output))
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToPrinted()
        {
            return Pair.Append(Pair.MakeList(
                Symbol.Macro, LiteralSymbols), Transformers)
                .Expect<Pair>()
                .Format('{', '}');
        }
        public override string ToSerialized() => throw new NotImplementedException(); //idk lol

    }
}
