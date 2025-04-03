using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Abstractions.Arguments;
using Clasp.Data.Abstractions.SpecialForms;

namespace Clasp.Data.Abstractions.Applications
{
    /// <summary>
    /// Represents the procedural execution of a <see cref="Function"/> or <see cref="PrimitiveProcedure"/>.
    /// </summary>
    internal abstract class AbstractApplication : AbstractProgram
    {
        public readonly AbstractArgument Arguments;

        protected AbstractApplication(AbstractArgument arguments) => Arguments = arguments;
    }
}
