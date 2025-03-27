using System.Collections.Generic;

using Clasp.Data.Terms;

namespace Clasp.Data.Static
{
    internal static class InternedSymbols
    {
        private static readonly Dictionary<string, Symbol> _internment = [];

        public static bool TryGetSymbol(string name, out Symbol symbol)
        {
            return _internment.TryGetValue(name, out symbol);
        }

        public static bool Contains(string name) => _internment.ContainsKey(name);

        public static void Intern(Symbol sym)
        {
            _internment[sym.Name] = sym;
        }
    }
}
