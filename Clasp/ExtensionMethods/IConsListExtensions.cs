using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Interfaces;

namespace Clasp.ExtensionMethods
{
    internal static class IConsListExtensions
    {
        /// <summary>
        /// Recurse through the structure of <paramref name="consList"/> to determine
        /// if it's a proper list
        /// </summary>
        public static bool IsProper<T>(this IConsList<T> consList)
            where T : class
        {
            return consList.IsList && consList.CdrList.IsProper();
        }

        /// <summary>
        /// Recurse through the structure of <paramref name="consList"/> to determine
        /// if it's a dotted list
        /// </summary>
        public static bool IsImproper<T>(this IConsList<T> consList)
            where T : class
        {
            return consList.IsDotted || consList.CdrList.IsImproper();
        }

        /// <summary>
        /// Enumerate a nested sequence of <see cref="IConsList{T}"/> nodes.
        /// </summary>
        public static IEnumerable<IConsList<T>> EnumerateNodes<T>(this IConsList<T> consList)
            where T : class
        {
            IConsList<T>? next = consList;
            while (next is not null)
            {
                yield return consList;
                next = next.CdrList;
            }
            yield break;
        }

        /// <summary>
        /// Enumerate all the elements of <paramref name="consList"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsProper{T}(IConsList{T})"/>, the final element will be <see langword="null"/>.
        /// </remarks>
        public static IEnumerable<T> EnumerateElements<T>(this IConsList<T> consList)
            where T : class
        {
            yield return consList.Car;

            while (consList.CdrList is not null)
            {
                consList = consList.CdrList;
                yield return consList.Car;
            }

            yield return consList.CdrValue; // T if dotted, else null if nil-terminated
            yield break;
        }
    }
}
