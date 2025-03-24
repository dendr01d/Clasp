using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Abstractions.References;

namespace Clasp.Data.Abstractions.SpecialForms
{
    /// <summary>
    /// Represents the establishment of a lexical closure containing zero or more locally-bound variables
    /// that are initialized to the values of the provided arguments.
    /// </summary>
    class Block
    {
        public readonly AbstractReference[] Closure; //all the state implicitly captured from the lexical context

        public readonly LocalVariable[] LocalVariables;
        public readonly AbstractProgram[] Arguments;

    }
}
