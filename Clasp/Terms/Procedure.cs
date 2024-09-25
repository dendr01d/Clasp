using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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

    internal class CompoundProcedure : Procedure
    {
        public readonly Expression Parameters;
        public readonly Expression Body;
        public readonly Environment Closure;
        public override bool ApplicativeOrder => true;

        public CompoundProcedure(Expression parameters, Expression body, Environment outerEnv)
        {
            Parameters = parameters;
            Body = body;
            Closure = outerEnv;
        }

        public override Expression Deconstruct() => Pair.ListStar(Symbol.Lambda, Parameters.Deconstruct(), Body);
        public override string Serialize() => Deconstruct().Serialize();
        public override string Print() => string.Format("<λ{0}>", Parameters.Print());
    }

    internal class PrimitiveProcedure : Procedure
    {
        private readonly Symbol _referant;
        private readonly Func<Expression, Expression> _operation;
        public override bool ApplicativeOrder => true;

        private PrimitiveProcedure(Symbol referant, Func<Expression, Expression> op)
        {
            _referant = referant;
            _operation = op;
        }

        public void Manifest(Environment env, string name, Func<Expression, Expression> op)
        {
            Symbol sym = Symbol.Ize(name);
            env.BindNew(sym, new PrimitiveProcedure(sym, op));
        }

        public Expression Apply(Expression input) => _operation(input);

        public override Expression Deconstruct() => _referant;
        public override string Serialize() => _referant.Serialize();
        public override string Print() => string.Format("<{0}>", _referant);

        #region Native Operations

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
    
    internal class SpecialForm : Procedure
    {
        private readonly Symbol _referant;
        public readonly RegisterMachine.Label OpCode;
        public override bool ApplicativeOrder => false;

        private SpecialForm(Symbol referant, RegisterMachine.Label op)
        {
            _referant = referant;
            OpCode = op;
        }

        public void Manifest(Environment env, string name, RegisterMachine.Label op)
        {
            Symbol sym = Symbol.Ize(name);
            env.BindNew(sym, new SpecialForm(sym, op));
        }

        public override Expression Deconstruct() => _referant;
        public override string Serialize() => _referant.Serialize();
        public override string Print() => string.Format("<{0}>", _referant);
    }

    internal class Macro : Procedure
    {
        public readonly Expression LiteralSymbols;
        public readonly SyntaxRule[] Rules;
        public readonly Environment Closure;
        public override bool ApplicativeOrder => false;

        public Macro(Expression literals, Pair transformers, Environment closure)
        {
            LiteralSymbols = literals;
            Transformers = transformers;
            Closure = closure;
        }

        public override Expression Deconstruct() => _referant;
        public override string Serialize() => _referant.Serialize();
        public override string Print() => string.Format("<μ{0}>", _referant);
        //
        //public override string ToPrinted()
        //{
        //    return Pair.Append(Pair.List(
        //        Symbol.Macro, LiteralSymbols), Transformers)
        //        .Expect<Pair>()
        //        .Format('{', '}');
        //}
        //public override string ToSerialized() => throw new NotImplementedException(); //idk lol

    }
}
