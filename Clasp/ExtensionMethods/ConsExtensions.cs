using System;
using System.Diagnostics.CodeAnalysis;

using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.ExtensionMethods
{
    internal static class ConsExtensions
    {
        #region Match Leading terms of List

        public static bool TryMatchLeading<T>(this Cons p,
            [NotNullWhen(true)] out T? car,
            out Term cdr)
            where T : Term
        {
            cdr = p.Cdr;

            if (p.Car is T outCar)
            {
                car = outCar;
                return true;
            }

            car = null;
            return false;
        }

        public static bool TryMatchLeading<T1, T2>(this Cons p,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr,
            [NotNullWhen(true)] out Term? cddr)
            where T1 : Term
            where T2 : Term
        {
            if (p.Car is T1 outCar
                && p.Cdr is Pair p2
                && p2.Car is T2 outCadr)
            {
                car = outCar;
                cadr = outCadr;
                cddr = p2.Cdr;
                return true;
            }

            car = null;
            cadr = null;
            cddr = null;
            return false;
        }

        public static bool TryMatchLeading<T1, T2, T3>(this Cons p,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr,
            [NotNullWhen(true)] out T3? caddr,
            [NotNullWhen(true)] out Term? cdddr)
            where T1 : Term
            where T2 : Term
            where T3 : Term
        {
            if (p.Car is T1 outCar
                && p.Cdr is Pair p2
                && p2.Car is T2 outCadr
                && p2.Cdr is Pair p3
                && p3.Car is T3 outCaddr)
            {
                car = outCar;
                cadr = outCadr;
                caddr = outCaddr;
                cdddr = p3.Cdr;
                return true;
            }

            car = null;
            cadr = null;
            caddr = null;
            cdddr = null;
            return false;
        }

        #endregion

        #region Match Entirety of List

        public static bool TryMatchOnly<T>(this Cons p,
            [NotNullWhen(true)] out T? car)
            where T : Term
        {
            return TryMatchLeading(p, out car, out Term? cdr)
                && cdr is Nil;
        }

        public static bool TryMatchOnly<T1, T2>(this Cons p,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr)
            where T1 : Term
            where T2 : Term
        {
            return TryMatchLeading(p, out car, out cadr, out Term? cddr)
                && cddr is Nil;
        }

        public static bool TryMatchOnly<T1, T2, T3>(this Cons p,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr,
            [NotNullWhen(true)] out T3? caddr)
            where T1 : Term
            where T2 : Term
            where T3 : Term
        {
            return TryMatchLeading(p, out car, out cadr, out caddr, out Term? cdddr)
                && cdddr is Nil;
        }

        #endregion
    }
}
