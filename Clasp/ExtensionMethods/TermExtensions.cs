using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.ExtensionMethods
{
    internal static class TermExtensions
    {
        /// <summary>
        /// Attempt to deconstruct <paramref name="t"/> as a Cons-list
        /// structured as (<typeparamref name="T1"/> . <typeparamref name="T2"/>)
        /// </summary>
        public static bool TryUnpair<T1, T2>(this Term t,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cdr)
            where T1 : Term
            where T2 : Term
        {
            Cons? cns = (t as SyntaxPair)?.Expose() ?? (t as Cons);
            car = cns?.Car as T1;

            cdr = cns?.Cdr as T2;

            return (car is not null && cdr is not null);
        }

        /// <summary>
        /// Attempt to deconstruct <paramref name="t"/> as a Cons-list
        /// structured as (<typeparamref name="T1"/> <typeparamref name="T2"/> . <typeparamref name="T3"/>)
        /// </summary>
        public static bool TryUnpair<T1, T2, T3>(this Term t,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr,
            [NotNullWhen(true)] out T3? cddr)
            where T1 : Term
            where T2 : Term
            where T3 : Term
        {
            Cons? cns = (t as SyntaxPair)?.Expose() ?? (t as Cons);
            car = cns?.Car as T1;

            Cons? cdr = (cns?.Cdr as SyntaxPair)?.Expose() ?? (cns?.Cdr as Cons);
            cadr = cdr?.Car as T2;

            cddr = cdr?.Cdr as T3;

            return (car is not null && cadr is not null && cddr is not null);
        }

        /// <summary>
        /// Attempt to deconstruct <paramref name="t"/> as a Cons-list
        /// structured as (<typeparamref name="T1"/> <typeparamref name="T2"/> <typeparamref name="T3"/> . <typeparamref name="T4"/>)
        /// </summary>
        public static bool TryUnpair<T1, T2, T3, T4>(this Term t,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr,
            [NotNullWhen(true)] out T3? caddr,
            [NotNullWhen(true)] out T4? cdddr)
            where T1 : Term
            where T2 : Term
            where T3 : Term
            where T4 : Term
        {
            Cons? cns = (t as SyntaxPair)?.Expose() ?? (t as Cons);
            car = cns?.Car as T1;

            Cons? cdr = (cns?.Cdr as SyntaxPair)?.Expose() ?? (cns?.Cdr as Cons);
            cadr = cdr?.Car as T2;

            Cons? cddr = (cdr?.Cdr as SyntaxPair)?.Expose() ?? (cdr?.Cdr as Cons);
            caddr = cddr?.Car as T3;

            cdddr = cddr?.Cdr as T4;

            return (car is not null && cadr is not null && caddr is not null && cdddr is not null);
        }

        // ---
        
        /// <summary>
        /// Attempt to deconstruct <paramref name="t"/> as a Cons-list
        /// structured as (<typeparamref name="T1"/>)
        /// </summary>
        public static bool TryDelist<T1>(this Term t,
            [NotNullWhen(true)] out T1? car)
            where T1 : Term
        {
            return t.TryUnpair(out car, out Term? cdr) && Nil.Is(cdr);
        }

        /// <summary>
        /// Attempt to deconstruct <paramref name="t"/> as a Cons-list
        /// structured as (<typeparamref name="T1"/> <typeparamref name="T2"/>)
        /// </summary>
        public static bool TryDelist<T1, T2>(this Term t,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr)
            where T1 : Term
            where T2 : Term
        {
            return t.TryUnpair(out car, out cadr, out Term? cddr) && Nil.Is(cddr);
        }

        /// <summary>
        /// Attempt to deconstruct <paramref name="t"/> as a Cons-list
        /// structured as (<typeparamref name="T1"/> <typeparamref name="T2"/> <typeparamref name="T3"/>)
        /// </summary>
        public static bool TryDelist<T1, T2, T3>(this Term t,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cadr,
            [NotNullWhen(true)] out T3? caddr)
            where T1 : Term
            where T2 : Term
            where T3 : Term
        {
            return t.TryUnpair(out car, out cadr, out caddr, out Term? cdddr) && Nil.Is(cdddr);
        }

    }
}
