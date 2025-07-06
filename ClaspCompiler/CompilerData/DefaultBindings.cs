using System.Diagnostics.CodeAnalysis;
using ClaspCompiler.LexicalScope;
using ClaspCompiler.SchemeData;

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
            PrimitiveOperator.Initialize();
            SpecialKeyword.Initialize();
        }

        public static void AddSpecial(SpecialKeyword kw) => _bindings[kw.Symbol] = BindingType.Special;
        public static void AddPrimitive(PrimitiveOperator op)
        {
            _bindings[op.Symbol] = BindingType.Primitive;
            _primLookup[op.Symbol] = op;
        }

        public static bool TryLookupPrimitive(Symbol sym, [NotNullWhen(true)] out PrimitiveOperator? op)
        {
            return _primLookup.TryGetValue(sym, out op);
        }
    }
}