using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class SymbolOps
    {
        public static Term SymbolToString(Symbol sym) => new CharString(sym.Name);

        public static Term StringToSymbol(CharString cs) => Symbol.Intern(cs.Value);

    }
}
