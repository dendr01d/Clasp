using Clasp.Binding.Environments;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Product;
using Clasp.Ops;

namespace Clasp.Binding
{
    internal static class StandardEnv
    {
        public static SuperEnvironment CreateNew()
        {
            SuperEnvironment output = new SuperEnvironment();

            foreach (Symbol kw in CoreKeywords)
            {
                output.DefineCoreForm(kw);
            }

            foreach (PrimitiveProcedure pp in PrimProcs)
            {
                output.DefineInitial(pp.OpSymbol.Name, pp);
            }

            return output;
        }

        private static readonly Symbol[] CoreKeywords = new Symbol[]
        {
            Implicit.SpTop,
            Implicit.SpVar,

            Symbol.Quote,
            Implicit.SpDatum,

            Symbol.QuoteSyntax,

            Symbol.Apply,
            Implicit.SpApply,

            Implicit.ParDef,
            Symbol.Define,
            Symbol.DefineSyntax,
            Symbol.Set,

            Symbol.Lambda,
            Implicit.SpLambda,

            Symbol.If,
            Symbol.Begin,
        };

        private static readonly PrimitiveProcedure[] PrimProcs = new PrimitiveProcedure[]
        {
            // List Ops
            new("cons", new BinaryOp<Term, Term>(Pairs.Cons)),
            new("car", new UnaryOp<Pair>(Pairs.Car)),
            new("cdr", new UnaryOp<Pair>(Pairs.Cdr)),
            new("set-car", new BinaryOp<Pair, Term>(Pairs.SetCar)),
            new("set-cdr", new BinaryOp<Pair, Term>(Pairs.SetCdr)),

            // Value Equality
            new("eq", new BinaryOp<Term, Term>(Equality.Eq)),
            new("eqv", new BinaryOp<Term, Term>(Equality.Eqv)),
            new("equal", new BinaryOp<Term, Term>(Equality.Equal)),

            // Type Predicates
            new("pair?", new UnaryOp<Term>(Predicates.IsType<Pair>)),
            new("null?", new UnaryOp<Term>(Predicates.IsType<Nil>)),

            new("symbol?", new UnaryOp<Term>(Predicates.IsType<Symbol>)),
            new("character?", new UnaryOp<Term>(Predicates.IsType<Character>)),
            new("string?", new UnaryOp<Term>(Predicates.IsType<CharString>)),
            new("vector?", new UnaryOp<Term>(Predicates.IsType<Vector>)),
            new("boolean?", new UnaryOp<Term>(Predicates.IsType<Boolean>)),

            new("number?", new UnaryOp<Term>(Predicates.IsType<Number>)),
            new("complex?", new UnaryOp<Term>(Predicates.IsType<ComplexNumber>)),
            new("real?", new UnaryOp<Term>(Predicates.IsType<RealNumber>)),
            new("rational?", new UnaryOp<Term>(Predicates.IsType<RationalNumber>)),
            new("integer?", new UnaryOp<Term>(Predicates.IsType<IntegralNumber>)),

            // Symbol Ops
            new("symbol->string", new UnaryOp<Symbol>(Symbols.SymbolToString)),
            new("string->symbol", new UnaryOp<CharString>(Symbols.StringToSymbol)),

            // Character Ops
            new("char=", new BinaryOp<Character, Character>(Characters.CharEq)),
            new("char<", new BinaryOp<Character, Character>(Characters.CharLT)),
            new("char<=", new BinaryOp<Character, Character>(Characters.CharLTE)),
            new("char>", new BinaryOp<Character, Character>(Characters.CharGT)),
            new("char>=", new BinaryOp<Character, Character>(Characters.CharGTE)),

            new("char->integer", new UnaryOp<Character>(Characters.CharacterToInteger)),
            new("integer->char", new UnaryOp<IntegralNumber>(Characters.IntegerToCharacter)),

            //Arithmetic
            new("+")
            {
                new BinaryOp<Number, Number>(Math.Add),
                new VarOp<Number>(Math.AddVar)
            },
            new("-")
            {
                new UnaryOp<Number>(Math.Negate),
                new BinaryOp<Number, Number>(Math.Subtract),
                new VarOp<Number>(Math.SubtractVar)
            },
            new("*")
            {
                new BinaryOp<Number, Number>(Math.Multiply),
                new VarOp<Number>(Math.MultiplyVar)
            },
            new("/")
            {
                new UnaryOp<Number>(Math.Invert),
                new BinaryOp<Number, Number>(Math.Divide),
                new VarOp<Number>(Math.DivideVar)
            }

        };

    }
}
