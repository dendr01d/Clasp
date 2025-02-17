using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Ops;
using Clasp.Ops.Functions;

namespace Clasp.Binding.Environments
{
    internal static class StandardEnv
    {
        public static readonly Scope StaticScope = new Scope(SourceCode.StaticSource);

        static StandardEnv()
        {
            foreach (Symbol kw in CoreKeywords)
            {
                StaticScope.AddStaticBinding(kw.Name, BindingType.Special);
            }

            foreach (PrimitiveProcedure pp in PrimProcs)
            {
                StaticScope.AddStaticBinding(pp.OpSymbol.Name, BindingType.Primitive);
            }
        }

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
            Implicit.Sp_Top,
            Implicit.Sp_Var,

            Symbol.Quote,
            Implicit.Sp_Datum,

            Symbol.QuoteSyntax,

            Symbol.Apply,
            Implicit.Sp_Apply,

            Implicit.Par_Def,
            Symbol.Define,
            Symbol.DefineSyntax,
            Symbol.Set,

            Symbol.Lambda,
            Implicit.Sp_Lambda,

            Symbol.If,
            Symbol.Begin,
            Implicit.Sp_Begin,

            Symbol.Module,
            Symbol.BeginForSyntax,
            Symbol.ImportForSyntax,
            Symbol.Import,
            Symbol.Export
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

            new NativeProcedure("syntax?", new NativeUnary<Term>(Predicates.IsType<Syntax>)),
            new NativeProcedure("identifier?", new NativeUnary<Term>(Predicates.IsType<Identifier>)),

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

            // Syntax Ops
            new NativeProcedure("syntax-source", new NativeUnary<Syntax>(Syntaxes.SyntaxSource)),
            new NativeProcedure("syntax-line", new NativeUnary<Syntax>(Syntaxes.SyntaxLine)),
            new NativeProcedure("syntax-column", new NativeUnary<Syntax>(Syntaxes.SyntaxColumn)),
            new NativeProcedure("syntax-position", new NativeUnary<Syntax>(Syntaxes.SyntaxPosition)),
            new NativeProcedure("syntax-span", new NativeUnary<Syntax>(Syntaxes.SyntaxSpan)),
            new NativeProcedure("syntax-original", new NativeUnary<Syntax>(Syntaxes.SyntaxOriginal)),

            new NativeProcedure("syntax-e", new NativeUnary<Syntax>(Syntaxes.SyntaxE)),
            new NativeProcedure("syntax->list", new NativeUnary<Syntax>(Syntaxes.SyntaxToList)),
            new NativeProcedure("syntax->datum", new NativeUnary<Syntax>(Syntaxes.SyntaxToDatum)),
            new NativeProcedure("datum->syntax", new NativeBinary<Syntax, Term>(Syntaxes.DatumToSyntax)),


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
