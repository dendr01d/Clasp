using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Abstractions.Arguments;
using Clasp.Data.Abstractions.References;
using Clasp.Data.Abstractions.Variables;

namespace Clasp.Data.Abstractions.SpecialForms
{
    /// <summary>
    /// Represents the establishment of a lexical closure containing zero or more locally-bound variables
    /// that are initialized to the values of the provided arguments.
    /// </summary>
    class FixLet : AbstractSpecialForm
    {
        public readonly AbstractVariable[] Variables;
        public readonly AbstractArgument Arguments;

        public readonly AbstractProgram Body;

    }
}
