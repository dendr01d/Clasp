using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms.Syntax;

using Clasp.Data.Terms;
using Clasp.Data.Metadata;

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
        public static bool TryDestruct<TCar, TCdr>(this Syntax? input,
            [NotNullWhen(true)] out TCar? car,
            [NotNullWhen(true)] out TCdr? cdr,
            [NotNullWhen(true)] out LexInfo? info)
            where TCar : Syntax
            where TCdr : Syntax
        {
            if (input is SyntaxPair stp
                && stp.Car is TCar outCar
                && stp.Cdr is TCdr outCdr)
            {
                car = outCar;
                cdr = outCdr;
                info = stp.LexContext;
                return true;
            }
            car = null;
            cdr = null;
            info = null;
            return false;
        }

        /// <summary>
        /// Assert that <paramref name="input"/> wraps Nil (the list-terminator)
        /// </summary>
        public static bool IsTerminator(this Syntax input)
        {
            return input.Expose() is Nil;
        }

        /// <summary>
        /// Construct a new <see cref="SyntaxPair"/> from the provided <paramref name="car"/>
        /// and <paramref name="cdr"/> copying the lexical context of <paramref name="ctx"/>.
        /// </summary>
        /// <remarks>
        /// Cons lists work like stacks, so we're actually PREPENDING <paramref name="car"/> onto <paramref name="cdr"/>.
        /// </remarks>
        public static SyntaxPair Cons(this Syntax cdr, Syntax car, LexInfo ctx)
        {
            return new SyntaxPair(car, cdr, ctx);
        }
    }
}
