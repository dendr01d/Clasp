using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSyntax.Abstract
{
    /// <summary>
    /// A representation of scheme source code in the form of homoiconic data augmented with contextual metadata
    /// </summary>
    internal interface ISyntax : ISchemeExp
    {
        public ImmutableHashSet<uint> SurroundingScope { get; }
        public SourceRef Source { get; }

        public ISchemeExp Expose();
        public ISyntax AddScopes(IEnumerable<uint> scopeTokens);
        public ISyntax RemoveScopes(IEnumerable<uint> scopeTokens);
        public ISyntax FlipScopes(IEnumerable<uint> scopeTokens);
        public ISyntax ClearScopes();
    }

    internal static class ISyntaxExtensions
    {
        private static bool TryCast<T>(ISyntax stx, out T? typedStx)
            where T : class, ISyntax
        {
            if (stx is T casted)
            {
                typedStx = casted;
                return true;
            }
            else
            {
                typedStx = null;
                return false;
            }
        }

        //public static bool TryGetCar<T>(this ISyntax stx, out T? car)
        //    where T : class, ISyntax
        //{
        //    car = null;

        //    return stx is StxPair stp
        //        && TryCast(stp.Car, out car);
        //}

        //public static bool TryGetCdr<T>(this ISyntax stx, out T? cdr)
        //    where T : class, ISyntax
        //{
        //    cdr = null;

        //    return stx is StxPair stp
        //        && TryCast(stp.Cdr, out cdr);
        //}

        public static bool TryDestruct<A, B>(this ISyntax stx,
            [NotNullWhen(true)] out A? car,
            [NotNullWhen(true)] out B? cdr)
            where A : class, ISyntax
            where B : class, ISyntax
        {
            car = null;
            cdr = null;

            return stx is StxPair stp
                && TryCast(stp.Car, out car)
                && TryCast(stp.Cdr, out cdr);
        }
    }
}
