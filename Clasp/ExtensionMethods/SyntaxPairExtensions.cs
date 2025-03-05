using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms.ProductValues;

using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.ExtensionMethods
{
    internal static class SyntaxPairExtensions
    {
        #region Match Leading terms of Syntax-List

        public static bool TryMatchLeading<T>(this SyntaxPair stp,
            [NotNullWhen(true)] out T? car,
            out Term cdr)
            where T : Syntax
        {
            return stp.Expose().TryMatchLeading(out car, out cdr);
        }

        public static bool TryMatchLeading<T1, T2>(this SyntaxPair stp,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr,
            [NotNullWhen(true)] out Term? cddr)
            where T1 : Syntax
            where T2 : Syntax
        {
            car = null;
            cadr = null;
            cddr = null;

            return stp.Expose().TryMatchOnly(out car, out SyntaxPair? stp2)
                && stp2.Expose().TryMatchLeading(out cadr, out cddr);
        }

        public static bool TryMatchLeading<T1, T2, T3>(this SyntaxPair stp,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr,
            [NotNullWhen(true)] out T3? caddr,
            [NotNullWhen(true)] out Term? cdddr)
            where T1 : Syntax
            where T2 : Syntax
            where T3 : Syntax
        {

            car = null;
            cadr = null;
            caddr = null;
            cdddr = null;

            return stp.Expose().TryMatchOnly(out car, out SyntaxPair? stp2)
                && stp2.Expose().TryMatchOnly(out cadr, out SyntaxPair? stp3)
                && stp3.Expose().TryMatchLeading(out caddr, out cdddr);
        }

        #endregion

        #region Match Entirety of Syntax-List

        public static bool TryMatchOnly<T>(this SyntaxPair stp,
            [NotNullWhen(true)] out T? car)
            where T : Syntax
        {
            return TryMatchLeading(stp, out car, out Term? cdr)
                && cdr == Datum.NilDatum;
        }

        public static bool TryMatchOnly<T1, T2>(this SyntaxPair stp,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr)
            where T1 : Syntax
            where T2 : Syntax
        {
            return TryMatchLeading(stp, out car, out cadr, out Term? cddr)
                && cddr == Datum.NilDatum;
        }

        public static bool TryMatchOnly<T1, T2, T3>(this SyntaxPair stp,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr,
            [NotNullWhen(true)] out T3? caddr)
            where T1 : Syntax
            where T2 : Syntax
            where T3 : Syntax
        {
            return TryMatchLeading(stp, out car, out cadr, out caddr, out Term? cdddr)
                && cdddr == Datum.NilDatum;
        }

        #endregion

    }
}
