using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Data.Terms;
//using Clasp.Data.Terms.SyntaxValues;
using Clasp.Ops;
using Clasp.Ops.Functions;

namespace Clasp.Data.Static
{
    internal static class Primitives
    {
        public static readonly PrimitiveProcedure[] PrimitiveProcs;

        static Primitives()
        {
            // Need to build this in the static initializer to guarantee all the other arrays are built beforehand

            PrimitiveProcs = new List<PrimitiveProcedure[]>()
            {
                _listProcs,
                _equalityProcs,
                _typePredicateProcs,

                _symbolProcs,
                _characterProcs,

                //_syntaxProcs,

                _arithmeticProcs,

                _portProcs,

                _specialValueProcs
            }
            .SelectMany(x => x).ToArray();
        }

        private static readonly PrimitiveProcedure[] _listProcs =
        [
            new("cons", new BinaryFn<ITerm, ITerm>(ConsOps.Cons)),
            new("car", new UnaryFn<Cons>(ConsOps.Car)),
            new("cdr", new UnaryFn<Cons>(ConsOps.Cdr)),
            new("set-car", new BinaryFn<Cons, ITerm>(ConsOps.SetCar)),
            new("set-cdr", new BinaryFn<Cons, ITerm>(ConsOps.SetCdr)),
        ];

        private static readonly PrimitiveProcedure[] _equalityProcs =
        [
            new("eq", new BinaryFn<ITerm, ITerm>(EqualityOps.Eq)),
            new("eqv", new BinaryFn<ITerm, ITerm>(EqualityOps.Eqv)),
            new("equal", new BinaryFn<ITerm, ITerm>(EqualityOps.Equal)),
        ];

        private static readonly PrimitiveProcedure[] _typePredicateProcs =
        [
            new("pair?", new UnaryFn<ITerm>(PredicateOps.IsType<Cons>)),
            new("null?", new UnaryFn<ITerm>(PredicateOps.IsType<Nil>)),

            new("symbol?", new UnaryFn<ITerm>(PredicateOps.IsType<Symbol>)),
            new("character?", new UnaryFn<ITerm>(PredicateOps.IsType<Character>)),
            new("string?", new UnaryFn<ITerm>(PredicateOps.IsType<RefString>)),
            new("vector?", new UnaryFn<ITerm>(PredicateOps.IsType<Vector>)),
            new("boolean?", new UnaryFn<ITerm>(PredicateOps.IsType<Boole>)),

            //new("number?", new UnaryFn<ITerm>(PredicateOps.IsType<Number>)),
            new("flonum?", new UnaryFn<ITerm>(PredicateOps.IsType<FloNum>)),
            new("fixnum?", new UnaryFn<ITerm>(PredicateOps.IsType<FixNum>)),

            //new("syntax?", new UnaryFn<ITerm>(PredicateOps.IsType<Syntax>)),
            //new("identifier?", new UnaryFn<ITerm>(PredicateOps.IsType<Identifier>)),
        ];

        private static readonly PrimitiveProcedure[] _symbolProcs =
        [
            new("symbol->string", new UnaryFn<Symbol>(SymbolOps.SymbolToString)),
            new("string->symbol", new UnaryFn<RefString>(SymbolOps.StringToSymbol))
        ];

        private static readonly PrimitiveProcedure[] _characterProcs =
        [
            new("char=", new BinaryFn<Character, Character>(CharacterOps.CharEq)),
            new("char<", new BinaryFn<Character, Character>(CharacterOps.CharLT)),
            new("char<=", new BinaryFn<Character, Character>(CharacterOps.CharLTE)),
            new("char>", new BinaryFn<Character, Character>(CharacterOps.CharGT)),
            new("char>=", new BinaryFn<Character, Character>(CharacterOps.CharGTE)),

            new("char->integer", new UnaryFn<Character>(CharacterOps.CharacterToInteger)),
            new("integer->char", new UnaryFn<FixNum>(CharacterOps.IntegerToCharacter)),
        ];

        //private static readonly PrimitiveProcedure[] _syntaxProcs =
        //[
        //    new("syntax-source", new UnaryFn<Syntax>(SyntaxOps.SyntaxSource)),
        //    new("syntax-line", new UnaryFn<Syntax>(SyntaxOps.SyntaxLine)),
        //    new("syntax-column", new UnaryFn<Syntax>(SyntaxOps.SyntaxColumn)),
        //    new("syntax-position", new UnaryFn<Syntax>(SyntaxOps.SyntaxPosition)),
        //    new("syntax-span", new UnaryFn<Syntax>(SyntaxOps.SyntaxSpan)),
        //    new("syntax-original", new UnaryFn<Syntax>(SyntaxOps.SyntaxOriginal)),

        //    new("free-identifier=?", new BinaryMxFn<Identifier, Identifier>(SyntaxOps.FreeIdentifierEq)),
        //    new("bound-identifier=?", new BinaryFn<Identifier, Identifier>(SyntaxOps.BoundIdentifierEq)),

        //    new("syntax-e", new UnaryFn<Syntax>(SyntaxOps.SyntaxE)),
        //    //new("syntax->list", new UnaryFn<Syntax>(SyntaxOps.SyntaxToList)),
        //    //new("syntax->datum", new UnaryFn<Syntax>(SyntaxOps.SyntaxToDatum)),
        //    //new("datum->syntax", new BinaryFn<Syntax, ITerm>(SyntaxOps.DatumToSyntax)),
        //];

        private static readonly PrimitiveProcedure[] _arithmeticProcs =
        [
            new("+",
                new BinaryFn<Number, Number>(MathOps.Add),
                new VariadicFn<Number>(MathOps.AddVar)
            ),
            new("-",
                new UnaryFn<Number>(MathOps.Negate),
                new BinaryFn<Number, Number>(MathOps.Subtract),
                new VariadicFn<Number>(MathOps.SubtractVar)
            ),
            new("*",
                new BinaryFn<Number, Number>(MathOps.Multiply),
                new VariadicFn<Number>(MathOps.MultiplyVar)
            ),
            new("/",
                new UnaryFn<Number>(MathOps.Invert),
                new BinaryFn<Number, Number>(MathOps.Divide),
                new VariadicFn<Number>(MathOps.DivideVar)
            )
        ];

        private static readonly PrimitiveProcedure[] _portProcs =
        [
            new("open-console-out", new NullaryFn(PortOps.OpenConsoleOut)),
            new("open-console-in", new NullaryFn(PortOps.OpenConsoleIn)),

            new("port-read", new UnaryFn<PortReader>(PortOps.Read)),
            new("port-write", new BinaryFn<PortWriter, ITerm>(PortOps.Write))
        ];

        private static readonly PrimitiveProcedure[] _specialValueProcs =
        [
            new("void", new NullaryFn(SpecialValueOps.MakeVoid)),
            new("undefined", new NullaryFn(SpecialValueOps.MakeUndefined))
        ];
    }
}
