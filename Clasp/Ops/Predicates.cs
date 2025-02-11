using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class Predicates
    {
        public static Term IsType<T>(Term t) where T : Term => t is T;

        #region Numeric
        public static Term IsExact(Number n) => n.IsExact;
        public static Term IsInexact(Number n) => !n.IsExact;

        public static Term IsZero(Number n) => Math.Equivalent(n, Number.Zero);
        public static Term IsPositive(RealNumber r) => !r.IsNegative;
        public static Term IsNegative(RealNumber r) => r.IsNegative;

        // https://groups.csail.mit.edu/mac/ftpdir/scheme-7.4/doc-html/scheme_5.html

        #endregion
    }
}
