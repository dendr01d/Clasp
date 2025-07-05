using ClaspCompiler.LexicalScope;
using ClaspCompiler.SchemeData;

namespace ClaspCompiler.CompilerData
{
    internal static class DefaultBindings
    {
        private static readonly Dictionary<Symbol, BindingType> _bindings;
        public static IReadOnlyDictionary<Symbol, BindingType> Bindings => _bindings;

        static DefaultBindings()
        {
            _bindings = [];
            PrimitiveOperator.Initialize();
            SpecialKeyword.Initialize();
        }

        public static void AddSpecial(Symbol sym) => _bindings[sym] = BindingType.Special;
        public static void AddPrimitive(Symbol sym) => _bindings[sym] = BindingType.Primitive;
    }
}
