using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Values
{
    /// <inheritdoc cref="byte"/>
    internal readonly struct Byte : IAbstractValue
    {
        public readonly byte Value;
        public int Size => sizeof(byte);
    }
}
