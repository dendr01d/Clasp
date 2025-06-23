using ClaspCompiler.SchemeData;

namespace ClaspCompiler.CompilerData
{
    internal static class DefaultBindings
    {
        private static readonly Dictionary<Symbol, Symbol> _bindings = [];

        static DefaultBindings()
        {
            foreach(string kw in SpecialKeyword.Keywords)
            {
                Symbol sym = SymbolFactory.InternGlobal(kw);
                _bindings[sym] = sym;
            }
        }

        public static Dictionary<Symbol, Symbol> Get()
        {
            return _bindings.ToDictionary();
        }

        public static void AddDefault(Symbol sym) => _bindings[sym] = sym;
    }
}
