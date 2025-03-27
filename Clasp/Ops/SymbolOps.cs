using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class SymbolOps
    {
        public static Term SymbolToString(Symbol sym) => new RefString(sym.Name);

        public static Term StringToSymbol(RefString cs) => Symbol.Intern(cs.Value);

    }
}
