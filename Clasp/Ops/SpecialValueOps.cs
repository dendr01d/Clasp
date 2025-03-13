using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class SpecialValueOps
    {
        public static VoidTerm MakeVoid() => VoidTerm.Value;

        public static Undefined MakeUndefined() => Undefined.Value;

    }
}
