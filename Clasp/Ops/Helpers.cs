using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class Helpers
    {
        public static TOut Fold<TArg, TOut>(TOut seed, System.Func<TOut, TArg, TOut> fun, params TArg[] args)
            where TArg : Term
            where TOut : Term
        {
            TOut result = seed;

            for (int i = 0; i < args.Length; ++i)
            {
                result = fun(result, args[i]);
            }

            return result;
        }

        public static Boolean FoldComparison<TArg>(System.Func<TArg, TArg, bool> fun, params TArg[] args)
        {
            TArg prior = args[0];

            for (int i = 1; i < args.Length; ++i)
            {
                if (!fun(prior, args[i]))
                {
                    return Boolean.False;
                }

                prior = args[i];
            }

            return Boolean.True;
        }
    }
}
