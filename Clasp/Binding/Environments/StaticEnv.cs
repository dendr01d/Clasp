using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Exceptions;
using Clasp.Ops;
using Clasp.Ops.Functions;

namespace Clasp.Binding.Environments
{
    internal sealed class StaticEnv : ClaspEnvironment
    {
        private static readonly Dictionary<string, Term> _definitions = new Dictionary<string, Term>();

        public static readonly StaticEnv Instance = new StaticEnv();
        public static readonly Scope ImplicitScope = new Scope(SourceCode.StaticSource);

        public static string ClaspSourceDir = string.Empty;

        static StaticEnv()
        {
            foreach (Symbol sym in CoreKeywords)
            {
                ImplicitScope.AddStaticBinding(sym.Name, BindingType.Special);
                _definitions.Add(sym.Name, sym);
            }

            foreach (PrimitiveProcedure pp in PrimProcs)
            {
                ImplicitScope.AddStaticBinding(pp.OpSymbol.Name, BindingType.Primitive);
                _definitions.Add(pp.OpSymbol.Name, pp);
            }
        }

        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value)
        {
            if (_definitions.TryGetValue(key, out value))
            {
                return true;
            }
            // end of the line
            throw new ClaspGeneralException("Could not find definition of '{0}' in environment chain.", key);
        }

        private static readonly Symbol[] CoreKeywords = new Symbol[]
        {
            Symbols.Quote,
            Symbols.QuoteSyntax,

            Symbols.Apply,

            Symbols.Define,
            Symbols.Set,

            Symbols.Lambda,

            Symbols.If,

            Symbols.Begin,

            Symbols.Module,
            Symbols.Import,
            Symbols.Export,

            Symbols.DefineSyntax,
            Symbols.BeginForSyntax,
            Symbols.ImportForSyntax
        };

        private static readonly PrimitiveProcedure[] PrimProcs = new PrimitiveProcedure[]
        {
            // Process Ops


            // List Ops
            new("cons", new BinaryFn<Term, Term>(ConsOps.Cons)),
            new("car", new UnaryFn<Cons>(ConsOps.Car)),
            new("cdr", new UnaryFn<Cons>(ConsOps.Cdr)),
            new("set-car", new BinaryFn<Cons<Term, Term>, Term>(ConsOps.SetCar)),
            new("set-cdr", new BinaryFn<Cons<Term, Term>, Term>(ConsOps.SetCdr)),

            // Value Equality
            new("eq", new BinaryFn<Term, Term>(EqualityOps.Eq)),
            new("eqv", new BinaryFn<Term, Term>(EqualityOps.Eqv)),
            new("equal", new BinaryFn<Term, Term>(EqualityOps.Equal)),

            // Type Predicates
            new("pair?", new UnaryFn<Term>(PredicateOps.IsType<Cons>)),
            new("null?", new UnaryFn<Term>(PredicateOps.IsType<Nil>)),

            new("symbol?", new UnaryFn<Term>(PredicateOps.IsType<Symbol>)),
            new("character?", new UnaryFn<Term>(PredicateOps.IsType<Character>)),
            new("string?", new UnaryFn<Term>(PredicateOps.IsType<CharString>)),
            new("vector?", new UnaryFn<Term>(PredicateOps.IsType<Vector>)),
            new("boolean?", new UnaryFn<Term>(PredicateOps.IsType<Boolean>)),

            new("number?", new UnaryFn<Term>(PredicateOps.IsType<Number>)),
            new("complex?", new UnaryFn<Term>(PredicateOps.IsType<ComplexNumeric>)),
            new("real?", new UnaryFn<Term>(PredicateOps.IsType<RealNumeric>)),
            new("rational?", new UnaryFn<Term>(PredicateOps.IsType<RationalNumeric>)),
            new("integer?", new UnaryFn<Term>(PredicateOps.IsType<IntegralNumeric>)),

            new("syntax?", new UnaryFn<Term>(PredicateOps.IsType<Syntax>)),
            new("identifier?", new UnaryFn<Term>(PredicateOps.IsType<Identifier>)),

            // Symbol Ops
            new("symbol->string", new UnaryFn<Symbol>(SymbolOps.SymbolToString)),
            new("string->symbol", new UnaryFn<CharString>(SymbolOps.StringToSymbol)),

            // Character Ops
            new("char=", new BinaryFn<Character, Character>(CharacterOps.CharEq)),
            new("char<", new BinaryFn<Character, Character>(CharacterOps.CharLT)),
            new("char<=", new BinaryFn<Character, Character>(CharacterOps.CharLTE)),
            new("char>", new BinaryFn<Character, Character>(CharacterOps.CharGT)),
            new("char>=", new BinaryFn<Character, Character>(CharacterOps.CharGTE)),

            new("char->integer", new UnaryFn<Character>(CharacterOps.CharacterToInteger)),
            new("integer->char", new UnaryFn<IntegralNumeric>(CharacterOps.IntegerToCharacter)),

            // Syntax Ops
            new("syntax-source", new UnaryFn<Syntax>(SyntaxOps.SyntaxSource)),
            new("syntax-line", new UnaryFn<Syntax>(SyntaxOps.SyntaxLine)),
            new("syntax-column", new UnaryFn<Syntax>(SyntaxOps.SyntaxColumn)),
            new("syntax-position", new UnaryFn<Syntax>(SyntaxOps.SyntaxPosition)),
            new("syntax-span", new UnaryFn<Syntax>(SyntaxOps.SyntaxSpan)),
            new("syntax-original", new UnaryFn<Syntax>(SyntaxOps.SyntaxOriginal)),

            new("free-identifier=?", new BinaryMxFn<Identifier, Identifier>(SyntaxOps.FreeIdentifierEq)),
            new("bound-identifier=?", new BinaryFn<Identifier, Identifier>(SyntaxOps.BoundIdentifierEq)),

            new("syntax-e", new UnaryFn<Syntax>(SyntaxOps.SyntaxE)),
            new("syntax->list", new UnaryFn<Syntax>(SyntaxOps.SyntaxToList)),
            new("syntax->datum", new UnaryFn<Syntax>(SyntaxOps.SyntaxToDatum)),
            new("datum->syntax", new BinaryFn<Syntax, Term>(SyntaxOps.DatumToSyntax)),


            //Arithmetic
            new("+")
            {
                new BinaryFn<Number, Number>(MathOps.Add),
                new VariadicFn<Number>(MathOps.AddVar)
            },
            new("-")
            {
                new UnaryFn<Number>(MathOps.Negate),
                new BinaryFn<Number, Number>(MathOps.Subtract),
                new VariadicFn<Number>(MathOps.SubtractVar)
            },
            new("*")
            {
                new BinaryFn<Number, Number>(MathOps.Multiply),
                new VariadicFn<Number>(MathOps.MultiplyVar)
            },
            new("/")
            {
                new UnaryFn<Number>(MathOps.Invert),
                new BinaryFn<Number, Number>(MathOps.Divide),
                new VariadicFn<Number>(MathOps.DivideVar)
            }
        };

        #region Module Cache

        private static Dictionary<string, Module> _loadedModules = new Dictionary<string, Module>();

        public static bool TryGetModule(string moduleName, [NotNullWhen(true)] out Module? mdl)
        {
            return _loadedModules.TryGetValue(moduleName, out mdl);
        }

        public static void CacheModule(Module mdl)
        {
            if (_loadedModules.ContainsKey(mdl.Name))
            {
                throw new ClaspGeneralException("Cannot cache duplicate module '{0}'.", mdl.Name);
            }

            _loadedModules[mdl.Name] = mdl;
        }

        #endregion
    }
}
