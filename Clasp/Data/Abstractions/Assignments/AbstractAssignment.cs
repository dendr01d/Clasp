using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Assignments
{
    /// <summary>
    /// Represents the mutation of an <see cref="AbstractObject"/>.
    /// </summary>
    internal abstract class AbstractAssignment : AbstractProgram
    {
        public readonly IAbstractForm Form;

        protected AbstractAssignment(IAbstractForm form)
        {
            Form = form;
        }
    }
}
