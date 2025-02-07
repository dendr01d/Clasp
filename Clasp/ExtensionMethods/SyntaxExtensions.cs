using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms.Syntax;

using Clasp.Data.Terms;

namespace Clasp.ExtensionMethods
{
    internal static class SyntaxExtensions
    {
        /// <summary>
        /// If <paramref name="input"/> is a not-<see langword="null"/> <see cref="SyntaxPair"/>,
        /// pull it apart into its constituent pieces.
        /// </summary>
        /// <typeparam name="T">The expected type of <paramref name="value"/>, i.e. <see cref="SyntaxPair.Car"/>.</typeparam>
        /// <param name="input">The syntax (presumably a <see cref="SyntaxPair"/>) to be destructed.</param>
        /// <param name="pair"><paramref name="input"/> as a <see cref="SyntaxPair"/>, if it truly is one.</param>
        /// <param name="value">The presumed <see cref="SyntaxPair.Car"/> value of <paramref name="input"/>.</param>
        /// <returns>The <see cref="SyntaxPair.Cdr"/> of <paramref name="input"/>, to be further destructed.</returns>
        public static Syntax? Destruct<T>(this Syntax? input,
            [NotNullWhen(true)] out SyntaxPair? pair,
            [NotNullWhen(true)] out T? value)
            where T : Syntax
        {
            if (input is SyntaxPair outPair
                && outPair.Car is T outValue)
            {
                pair = outPair;
                value = outValue;
                return outPair.Cdr;
            }
            pair = null;
            value = null;
            return null;
        }

        /// <summary>
        /// Assert that <paramref name="input"/> wraps Nil.
        /// </summary>
        public static bool Conclude(this Syntax? input,
            [NotNullWhen(true)] out Syntax? terminator)
        {
            terminator = input?.Expose() is Nil
                ? input
                : null;
            return terminator is not null;
        }

        /// <summary>
        /// Construct a new <see cref="SyntaxPair"/> from the provided <paramref name="car"/>
        /// and <paramref name="cdr"/> copying the lexical context of <paramref name="ctx"/>.
        /// </summary>
        public static SyntaxPair Construct(this Syntax cdr, SyntaxPair ctx, Syntax car)
        {
            return new SyntaxPair(ctx, car, cdr);
        }



        public static bool TryExposeOneArg(this Syntax stx,
            [NotNullWhen(true)] out Syntax? arg1)
        {
            if (stx is SyntaxPair stp)
            {
                arg1 = stp.Car;
                return stp.Cdr.Expose() is Nil;
            }

            arg1 = null;
            return false;
        }

        public static bool TryExposeTwoArgs(this Syntax stx,
            [NotNullWhen(true)] out Syntax? arg1,
            [NotNullWhen(true)] out Syntax? arg2)
        {
            if (stx is SyntaxPair stp)
            {
                arg1 = stp.Car;
                return stp.Cdr.TryExposeOneArg(out arg2);
            }

            arg1 = null;
            arg2 = null;
            return false;
        }

        public static bool TryExposeThreeArgs(this Syntax stx,
            [NotNullWhen(true)] out Syntax? arg1,
            [NotNullWhen(true)] out Syntax? arg2,
            [NotNullWhen(true)] out Syntax? arg3)
        {
            if (stx is SyntaxPair stp)
            {
                arg1 = stp.Car;
                return stp.Cdr.TryExposeTwoArgs(out arg2, out arg3);
            }

            arg1 = null;
            arg2 = null;
            arg3 = null;
            return false;
        }



    }
}
