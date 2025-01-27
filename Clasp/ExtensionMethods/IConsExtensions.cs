using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Interfaces;

namespace Clasp.ExtensionMethods
{
    internal static class IConsExtensions
    {

        public static IEnumerable<ICons<T1, T2>> EnumerateNodes<T1, T2>(this ICons<T1, T2> consList)
            where T1 : Term
            where T2 : Term
        {
            ICons<T1, T2>? next = consList;

            while(next is not null)
            {
                yield return next;
                next = next.Cdr as ICons<T1, T2>;
            }
            yield break;
        }

        public static IEnumerable<T1?> EnumerateElements<T1, T2>(this ICons<T1, T2> consList)
            where T1 : Term
            where T2 : Term
        {
            while(consList.Cdr is ICons<T1, T2> next)
            {
                yield return consList.Car;
                consList = next;
            }

            yield return consList.Cdr as T1;
            yield break;
        }



    }
}
