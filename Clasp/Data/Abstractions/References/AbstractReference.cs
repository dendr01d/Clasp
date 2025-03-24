using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Abstractions;
using Clasp.Data.Abstractions.Variables;

namespace Clasp.Data.Abstractions.References
{
    /// <summary>
    /// Represents an indirect reference to an <see cref="AbstractObject"/>.
    /// </summary>
    internal abstract class AbstractReference : AbstractProgram
    {
        public abstract AbstractVariable Variable { get; }
    }
}
