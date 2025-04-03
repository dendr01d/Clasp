using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Variables
{
    /// <summary>
    /// Represents the set of abstract objects that correspond to semantic variables.
    /// Without semantic meaning of their own, they cannot constitute programs.
    /// </summary>
    internal abstract class AbstractVariable : AbstractObject
    {
        public readonly string SymbolicName;

        protected AbstractVariable(string symbolicName)
        {
            SymbolicName = symbolicName;
        }

        public override string Express() => SymbolicName;
    }
}
