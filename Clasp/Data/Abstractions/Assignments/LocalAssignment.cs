using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Abstractions.References;

namespace Clasp.Data.Abstractions.Assignments
{
    /// <summary>
    /// Represents the mutation of an <see cref="AbstractObject"/> defined within the context
    /// of the enclosing <see cref="AbstractProgram"/>.
    /// </summary>
    internal sealed class LocalAssignment : AbstractAssignment
    {
        public readonly LocalReference Reference;

        public LocalAssignment(LocalReference reference, IAbstractForm form) : base(form)
        {
            Reference = reference;
        }
    }
}
