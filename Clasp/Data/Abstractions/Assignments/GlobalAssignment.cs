using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Abstractions.Variables;

namespace Clasp.Data.Abstractions.Assignments
{
    /// <summary>
    /// Represents the mutation of an <see cref="AbstractObject"/> defined at the "top level",
    /// i.e. outside the context of ANY <see cref="AbstractProgram"/>.
    /// </summary>
    internal sealed class GlobalAssignment : AbstractAssignment
    {
        public readonly GlobalVariable Variable;

        public GlobalAssignment(GlobalVariable var, IAbstractForm form) : base(form)
        {
            Variable = var;
        }
    }
}
