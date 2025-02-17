using System;
using System.Collections.Generic;
using System.Linq;

namespace Clasp.Exceptions
{
    public abstract class ClaspException : Exception
    {
        protected ClaspException(string format, params object?[] args)
            : base(string.Format(format, args))
        { }

        protected ClaspException(Exception innerException, string format, params object?[] args)
            : base(string.Format(format, args), innerException)
        { }

        protected static string FormatListItems(IEnumerable<string> items)
        {
            return string.Concat(items.Select(x => string.Format("\n\t- {0}", x)));
        }
    }
}
