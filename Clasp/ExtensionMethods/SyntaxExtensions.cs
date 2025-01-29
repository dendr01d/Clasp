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
