using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Values
{
    internal readonly struct CString : IAbstractValue
    {
        public readonly string Value;

        public CString(string value)
        {
            Value = value;
        }
    }
}
