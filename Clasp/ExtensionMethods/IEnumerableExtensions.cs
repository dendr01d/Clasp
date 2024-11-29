using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.ExtensionMethods
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<Tuple<T, T>> PairwiseEnumerate<T>(this IEnumerable<T> collection)
        {
            if (collection.Count() < 2)
            {
                return Array.Empty<Tuple<T, T>>();
            }

            IEnumerable<T> lefts = collection.SkipLast(1);
            IEnumerable<T> rights = collection.Skip(1);

            return lefts.Zip(rights, (x, y) => new Tuple<T, T>(x, y));
        }

        public static IEnumerable<T2> PairwiseSelect<T1, T2>(this IEnumerable<T1> collection, Func<T1, T1, T2> selector)
        {
            return collection.PairwiseEnumerate().Select(x => selector(x.Item1, x.Item2));
        }

        public static bool AllTrue(this IEnumerable<bool> collection)
        {
            return collection.All(x => x);
        }

        public static bool AnyTrue(this IEnumerable<bool> collection)
        {
            return collection.Any(x => x);
        }
    }
}
