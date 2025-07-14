using System.Diagnostics.CodeAnalysis;
using ClaspCompiler.LexicalScope;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.CompilerData
{
    internal static class DefaultBindings
    {
        private static readonly Dictionary<Symbol, BindingType> _bindings;
        public static IReadOnlyDictionary<Symbol, BindingType> Bindings => _bindings;

        private static Dictionary<Symbol, PrimitiveOperator> _primLookup = [];

        static DefaultBindings()
        {
            _bindings = [];
            SpecialKeyword.Initialize();
            SchemeType.Initialize();
            InitializePrimitiveOperatorBindings();
        }

        public static void AddSpecial(SpecialKeyword kw) => _bindings[kw.Symbol] = BindingType.Special;

        public static bool TryLookupPrimitive(Symbol sym, [NotNullWhen(true)] out PrimitiveOperator? op)
        {
            return _primLookup.TryGetValue(sym, out op);
        }


        #region Primitive Procedures

        private static void AddPrim(string name, SchemeType type, bool hasSideEffect = false)
        {
            Symbol sym = SymbolFactory.InternGlobal(name);
            PrimitiveOperator prop = new(name, type, hasSideEffect);

            _bindings[sym] = BindingType.Primitive;
            _primLookup[sym] = prop;
        }

        public static void DeclarePredicate(string name, FunctionType type) => AddPrim(name, type, false);

        private static void InitializePrimitiveOperatorBindings()
        {
            AddPrim("read", new FunctionType(SchemeType.Integer, []), true);

            AddPrim("+", new FunctionType(SchemeType.Integer, SchemeType.ListOf(SchemeType.Integer)));
            AddPrim("-", new FunctionType(SchemeType.Integer, [SchemeType.Integer, SchemeType.Integer]));
            AddPrim("-", new FunctionType(SchemeType.Integer, [SchemeType.Integer]));

            AddPrim("eq?", new FunctionType(SchemeType.Boolean, [SchemeType.Any, SchemeType.Any]));
            AddPrim("eqv?", new FunctionType(SchemeType.Boolean, [SchemeType.Any, SchemeType.Any]));

            AddPrim("<", new FunctionType(SchemeType.Boolean, SchemeType.Integer, SchemeType.Integer));
            AddPrim("<=", new FunctionType(SchemeType.Boolean, SchemeType.Integer, SchemeType.Integer));
            AddPrim(">", new FunctionType(SchemeType.Boolean, SchemeType.Integer, SchemeType.Integer));
            AddPrim(">=", new FunctionType(SchemeType.Boolean, SchemeType.Integer, SchemeType.Integer));

            AddPrim("not", new FunctionType(SchemeType.Boolean, SchemeType.Any));

            AddPrim("stx-e", new FunctionType(SchemeType.Any, SchemeType.Syntax));
            AddPrim("mk-stx", new FunctionType(SchemeType.Syntax, SchemeType.Any, SchemeType.Syntax));

            AddPrim("void", new FunctionType(SchemeType.Void, []));

            AddPrim("cons", AllType.Construct(2, x => new FunctionType(new PairType(x[0], x[1]), x[0], x[1])));
            AddPrim("car", AllType.Construct(x => new FunctionType(x, new PairType(x, SchemeType.Any))));
            AddPrim("cdr", AllType.Construct(x => new FunctionType(x, new PairType(SchemeType.Any, x))));
        }

        #endregion
    }
}