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
        public readonly Scope ImplicitScope;

        public static string ClaspSourceDir = string.Empty;

        #region Instance Data
        private StaticEnv()
        {
            ImplicitScope = new Scope(SourceCode.StaticSource);

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
        #endregion

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
            new NativeProcedure("cons", new NativeBinary<Term, Term>(ConsOps.Cons)),
            new NativeProcedure("car", new NativeUnary<Cons>(ConsOps.Car)),
            new NativeProcedure("cdr", new NativeUnary<Cons>(ConsOps.Cdr)),
            new NativeProcedure("set-car", new NativeBinary<Cons<Term, Term>, Term>(ConsOps.SetCar)),
            new NativeProcedure("set-cdr", new NativeBinary<Cons<Term, Term>, Term>(ConsOps.SetCdr)),

            // Value Equality
            new NativeProcedure("eq", new NativeBinary<Term, Term>(EqualityOps.Eq)),
            new NativeProcedure("eqv", new NativeBinary<Term, Term>(EqualityOps.Eqv)),
            new NativeProcedure("equal", new NativeBinary<Term, Term>(EqualityOps.Equal)),

            // Type Predicates
            new NativeProcedure("pair?", new NativeUnary<Term>(PredicateOps.IsType<Cons>)),
            new NativeProcedure("null?", new NativeUnary<Term>(PredicateOps.IsType<Nil>)),

            new NativeProcedure("symbol?", new NativeUnary<Term>(PredicateOps.IsType<Symbol>)),
            new NativeProcedure("character?", new NativeUnary<Term>(PredicateOps.IsType<Character>)),
            new NativeProcedure("string?", new NativeUnary<Term>(PredicateOps.IsType<CharString>)),
            new NativeProcedure("vector?", new NativeUnary<Term>(PredicateOps.IsType<Vector>)),
            new NativeProcedure("boolean?", new NativeUnary<Term>(PredicateOps.IsType<Boolean>)),

            new NativeProcedure("number?", new NativeUnary<Term>(PredicateOps.IsType<Number>)),
            new NativeProcedure("complex?", new NativeUnary<Term>(PredicateOps.IsType<ComplexNumeric>)),
            new NativeProcedure("real?", new NativeUnary<Term>(PredicateOps.IsType<RealNumeric>)),
            new NativeProcedure("rational?", new NativeUnary<Term>(PredicateOps.IsType<RationalNumeric>)),
            new NativeProcedure("integer?", new NativeUnary<Term>(PredicateOps.IsType<IntegralNumeric>)),

            new NativeProcedure("syntax?", new NativeUnary<Term>(PredicateOps.IsType<Syntax>)),
            new NativeProcedure("identifier?", new NativeUnary<Term>(PredicateOps.IsType<Identifier>)),

            // Symbol Ops
            new NativeProcedure("symbol->string", new NativeUnary<Symbol>(SymbolOps.SymbolToString)),
            new NativeProcedure("string->symbol", new NativeUnary<CharString>(SymbolOps.StringToSymbol)),

            // Character Ops
            new NativeProcedure("char=", new NativeBinary<Character, Character>(CharacterOps.CharEq)),
            new NativeProcedure("char<", new NativeBinary<Character, Character>(CharacterOps.CharLT)),
            new NativeProcedure("char<=", new NativeBinary<Character, Character>(CharacterOps.CharLTE)),
            new NativeProcedure("char>", new NativeBinary<Character, Character>(CharacterOps.CharGT)),
            new NativeProcedure("char>=", new NativeBinary<Character, Character>(CharacterOps.CharGTE)),

            new NativeProcedure("char->integer", new NativeUnary<Character>(CharacterOps.CharacterToInteger)),
            new NativeProcedure("integer->char", new NativeUnary<IntegralNumeric>(CharacterOps.IntegerToCharacter)),

            // Syntax Ops
            new NativeProcedure("syntax-source", new NativeUnary<Syntax>(SyntaxOps.SyntaxSource)),
            new NativeProcedure("syntax-line", new NativeUnary<Syntax>(SyntaxOps.SyntaxLine)),
            new NativeProcedure("syntax-column", new NativeUnary<Syntax>(SyntaxOps.SyntaxColumn)),
            new NativeProcedure("syntax-position", new NativeUnary<Syntax>(SyntaxOps.SyntaxPosition)),
            new NativeProcedure("syntax-span", new NativeUnary<Syntax>(SyntaxOps.SyntaxSpan)),
            new NativeProcedure("syntax-original", new NativeUnary<Syntax>(SyntaxOps.SyntaxOriginal)),

            new NativeProcedure("syntax-e", new NativeUnary<Syntax>(SyntaxOps.SyntaxE)),
            new NativeProcedure("syntax->list", new NativeUnary<Syntax>(SyntaxOps.SyntaxToList)),
            new NativeProcedure("syntax->datum", new NativeUnary<Syntax>(SyntaxOps.SyntaxToDatum)),
            new NativeProcedure("datum->syntax", new NativeBinary<Syntax, Term>(SyntaxOps.DatumToSyntax)),


            //Arithmetic
            new NativeProcedure("+")
            {
                new NativeBinary<Number, Number>(MathOps.Add),
                new NativeVariadic<Number>(MathOps.AddVar)
            },
            new NativeProcedure("-")
            {
                new NativeUnary<Number>(MathOps.Negate),
                new NativeBinary<Number, Number>(MathOps.Subtract),
                new NativeVariadic<Number>(MathOps.SubtractVar)
            },
            new NativeProcedure("*")
            {
                new NativeBinary<Number, Number>(MathOps.Multiply),
                new NativeVariadic<Number>(MathOps.MultiplyVar)
            },
            new NativeProcedure("/")
            {
                new NativeUnary<Number>(MathOps.Invert),
                new NativeBinary<Number, Number>(MathOps.Divide),
                new NativeVariadic<Number>(MathOps.DivideVar)
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
