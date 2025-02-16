using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Ops;
using Clasp.Ops.Functions;
using Clasp.Process;

namespace Clasp.Binding.Environments
{
    internal static class StandardEnv
    {
        public static SuperEnvironment CreateNew(Processor processor)
        {
            SuperEnvironment output = new SuperEnvironment(processor);

            foreach (Symbol kw in CoreKeywords)
            {
                output.DefineStaticKeyword(kw);
            }

            foreach (PrimitiveProcedure pp in PrimProcs)
            {
                output.DefineStaticPrimitive(pp.OpSymbol, pp);
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
            new NativeProcedure("cons", new NativeBinary<Term, Term>(Conses.Cons)),
            new NativeProcedure("car", new NativeUnary<Cons>(Conses.Car)),
            new NativeProcedure("cdr", new NativeUnary<Cons>(Conses.Cdr)),
            new NativeProcedure("set-car", new NativeBinary<Cons<Term, Term>, Term>(Conses.SetCar)),
            new NativeProcedure("set-cdr", new NativeBinary<Cons<Term, Term>, Term>(Conses.SetCdr)),

            // Value Equality
            new NativeProcedure("eq", new NativeBinary<Term, Term>(Equality.Eq)),
            new NativeProcedure("eqv", new NativeBinary<Term, Term>(Equality.Eqv)),
            new NativeProcedure("equal", new NativeBinary<Term, Term>(Equality.Equal)),

            // Type Predicates
            new NativeProcedure("pair?", new NativeUnary<Term>(Predicates.IsType<Cons>)),
            new NativeProcedure("null?", new NativeUnary<Term>(Predicates.IsType<Nil>)),

            new NativeProcedure("symbol?", new NativeUnary<Term>(Predicates.IsType<Symbol>)),
            new NativeProcedure("character?", new NativeUnary<Term>(Predicates.IsType<Character>)),
            new NativeProcedure("string?", new NativeUnary<Term>(Predicates.IsType<CharString>)),
            new NativeProcedure("vector?", new NativeUnary<Term>(Predicates.IsType<Vector>)),
            new NativeProcedure("boolean?", new NativeUnary<Term>(Predicates.IsType<Boolean>)),

            new NativeProcedure("number?", new NativeUnary<Term>(Predicates.IsType<Number>)),
            new NativeProcedure("complex?", new NativeUnary<Term>(Predicates.IsType<ComplexNumeric>)),
            new NativeProcedure("real?", new NativeUnary<Term>(Predicates.IsType<RealNumeric>)),
            new NativeProcedure("rational?", new NativeUnary<Term>(Predicates.IsType<RationalNumeric>)),
            new NativeProcedure("integer?", new NativeUnary<Term>(Predicates.IsType<IntegralNumeric>)),

            // Symbol Ops
            new NativeProcedure("symbol->string", new NativeUnary<Symbol>(Symbols.SymbolToString)),
            new NativeProcedure("string->symbol", new NativeUnary<CharString>(Symbols.StringToSymbol)),

            // Character Ops
            new NativeProcedure("char=", new NativeBinary<Character, Character>(Characters.CharEq)),
            new NativeProcedure("char<", new NativeBinary<Character, Character>(Characters.CharLT)),
            new NativeProcedure("char<=", new NativeBinary<Character, Character>(Characters.CharLTE)),
            new NativeProcedure("char>", new NativeBinary<Character, Character>(Characters.CharGT)),
            new NativeProcedure("char>=", new NativeBinary<Character, Character>(Characters.CharGTE)),

            new NativeProcedure("char->integer", new NativeUnary<Character>(Characters.CharacterToInteger)),
            new NativeProcedure("integer->char", new NativeUnary<IntegralNumeric>(Characters.IntegerToCharacter)),

            //Arithmetic
            new NativeProcedure("+")
            {
                new NativeBinary<Number, Number>(Math.Add),
                new NativeVariadic<Number>(Math.AddVar)
            },
            new NativeProcedure("-")
            {
                new NativeUnary<Number>(Math.Negate),
                new NativeBinary<Number, Number>(Math.Subtract),
                new NativeVariadic<Number>(Math.SubtractVar)
            },
            new NativeProcedure("*")
            {
                new NativeBinary<Number, Number>(Math.Multiply),
                new NativeVariadic<Number>(Math.MultiplyVar)
            },
            new NativeProcedure("/")
            {
                new NativeUnary<Number>(Math.Invert),
                new NativeBinary<Number, Number>(Math.Divide),
                new NativeVariadic<Number>(Math.DivideVar)
            },

            //IO
            new SystemProcedure("display", new SystemVariadic<Term>(IO.Display)),
            //new SystemProcedure("import", new SystemUnary<CharString>(IO.Import))
        };

    }
}
