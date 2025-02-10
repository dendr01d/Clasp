using Clasp.Binding.Environments;
using Clasp.Data;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Product;

namespace Clasp.Binding
{
    internal static class StandardEnv
    {
        public static SuperEnvironment CreateNew()
        {
            SuperEnvironment output = new SuperEnvironment();

            foreach(Symbol kw in CoreKeywords)
            {
                output.DefineCoreForm(kw);
            }

            foreach(PrimitiveProcedure pp in PrimProcs)
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
            new("+", Ops.Math.Add, true, 0, 1, 2),
            new("-", Ops.Math.Subtract, false, 0, 1, 2),
            new("*", Ops.Math.Multiply, true, 0, 1, 2),
            new("quotient", Ops.Math.Divide, false, 0, 1, 2),

            new("eq", Ops.Predicates.Eq, false, 2),
            new("eqv", Ops.Predicates.Eqv, false, 2),
            new("equal", Ops.Predicates.Equal, false, 2),

            new("symbol?", Ops.Predicates.IsType<Symbol>, false, 1),
            new("null?", Ops.Predicates.IsType<Nil>, false, 1),
            new("character?", Ops.Predicates.IsType<Character>, false, 1),
            new("string?", Ops.Predicates.IsType<CharString>, false, 1),
            new("integer?", Ops.Predicates.IsType<Integer>, false, 1),
            new("real?", Ops.Predicates.IsType<Real>, false, 1),
            new("vector?", Ops.Predicates.IsType<Vector>, false, 1),
            new("pair?", Ops.Predicates.IsType<ConsList>, false, 1),

            new("string->symbol", Ops.Symbols.StringToSymbol, false, 1),
            new("symbol->string", Ops.Symbols.SymbolToString, false, 1),

            new("char=", Ops.Characters.CharEq, false, 2),
            new("char<", Ops.Characters.CharLT, false, 2),
            new("char>", Ops.Characters.CharGT, false, 2),
            new("char<=", Ops.Characters.CharLTE, false, 2),
            new("char>=", Ops.Characters.CharGTE, false, 2),

            new("char->integer", Ops.Characters.CharacterToInteger, false, 1),
            new("integer->char", Ops.Characters.IntegerToCharacter, false, 1)

        };

    }
}
