using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Values
{
    /// <inheritdoc cref="double"/>
    internal readonly struct FloNum : IAbstractValue
    {
        public readonly double Value;
    }
}
