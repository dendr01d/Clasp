using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Values
{
    /// <summary>
    /// Represents a static, immutable value.
    /// </summary>
    internal interface IAbstractValue : IAbstractForm
    {
        /// <summary>
        /// The size (in bytes) of this value.
        /// </summary>
        public int Size { get; }
    }
}
