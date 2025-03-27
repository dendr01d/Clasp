using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class SymbolOps
    {
        public static ITerm SymbolToString(Symbol sym) => new RefString(sym.Name);

        public static ITerm StringToSymbol(RefString cs) => Symbol.Intern(cs.Value);

    }
}
