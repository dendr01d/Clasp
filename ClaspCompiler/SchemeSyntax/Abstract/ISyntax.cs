using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax.Abstract
{
    /// <summary>
    /// A representation of scheme source code in the form of homoiconic data augmented with contextual metadata
    /// </summary>
    internal interface ISyntax : ISchemeExp
    {
        public SourceRef Source { get; }
        public ImmutableHashSet<uint> ScopeSet { get; }

        public ISyntax AddScopes(params uint[] ids);
        public ISyntax RemoveScopes(params uint[] ids);
        public ISyntax FlipScopes(params uint[] ids);
        public ISyntax ClearScopes();
    }

    internal static class ISyntaxExtensions
    {
        private static bool TryCast<T>(this ISyntax stx, [NotNullWhen(true)] out T? result)
            where T : class, ISyntax
        {
            if (stx is T casted)
            {
                result = casted;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryCastNil(this ISyntax stx)
        {
            return stx is StxDatum std
                && std.IsNil;
        }

        public static bool TryDestruct<A, B>(this ISyntax stx,
            [NotNullWhen(true)] out A? car,
            [NotNullWhen(true)] out B? cdr)
            where A : class, ISyntax
            where B : class, ISyntax
        {
            car = null;
            cdr = null;

            return stx.TryCast(out StxPair? stp)
                && stp.Car.TryCast(out car)
                && stp.Cdr.TryCast(out cdr);
        }

        public static bool TryDestructLast<T>(this ISyntax stx,
            [NotNullWhen(true)] out T? last)
            where T : class, ISyntax
        {
            return stx.TryDestruct(out last, out ISyntax? maybeTerminator)
                && maybeTerminator.TryCastNil();
        }

        public static StxPair Rebuild<A, B>(this ISyntax stx,
            Func<A, ISyntax> rebuildCar,
            Func<B, ISyntax> rebuildCdr)
            where A : class, ISyntax
            where B : class, ISyntax
        {
            if (stx is StxPair pair
                && pair.Car is A car
                && pair.Cdr is B cdr)
            {
                return new StxPair(pair.Source, pair.ScopeSet)
                {
                    Car = rebuildCar(car),
                    Cdr = rebuildCdr(cdr)
                };
            }
            else
            {
                string msg = string.Format("Failed to deconstruct {0} into {1} and {2}: {3}",
                    nameof(StxPair),
                    nameof(A),
                    nameof(B),
                    stx);
                throw new Exception(msg);
            }
        }

        public static StxPair RebuildLast<A>(this ISyntax stx,
            Func<A, ISyntax> rebuildCar)
            where A : class, ISyntax
        {
            if (stx is StxPair pair
                && pair.Car is A car
                && pair.Cdr is ISyntax cdr
                && cdr.IsNil)
            {
                return new StxPair(pair.Source, pair.ScopeSet)
                {
                    Car = rebuildCar(car),
                    Cdr = cdr
                };
            }
            else
            {
                string msg = string.Format("Failed to deconstruct {0} into {1} and nil-terminator: {2}",
                    nameof(StxPair),
                    nameof(A),
                    stx);
                throw new Exception(msg);
            }
        }
    }
}
