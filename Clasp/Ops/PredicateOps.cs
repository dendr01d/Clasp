using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class PredicateOps
    {
        public static ITerm IsType<T>(ITerm t) where T : ITerm => new Boole(t is T);

        #region Numeric
        //public static ITerm IsExact(Number n) => n.IsExact;
        //public static ITerm IsInexact(Number n) => !n.IsExact;

        //public static ITerm IsZero(Number n) => MathOps.Equivalent(n, Number.Zero);
        //public static ITerm IsPositive(RealNumeric r) => !r.IsNegative;
        //public static ITerm IsNegative(RealNumeric r) => r.IsNegative;

        // https://groups.csail.mit.edu/mac/ftpdir/scheme-7.4/doc-html/scheme_5.html

        #endregion
    }
}
