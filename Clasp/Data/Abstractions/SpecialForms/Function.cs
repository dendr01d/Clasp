using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Abstractions.Applications;
using Clasp.Data.Abstractions.Variables;
using Clasp.Data.Static;
using Clasp.Data.Terms;

namespace Clasp.Data.Abstractions.SpecialForms
{
    /// <summary>
    /// Represents the construction of a new <see cref="AbstractProgram"/> such that it can be
    /// applied by a <see cref="CompoundApplication"/>.
    /// </summary>
    class Function : AbstractSpecialForm
    {
        public readonly AbstractVariable[] FormalVariables;
        public readonly AbstractVariable[] CapturedVariables;
        public readonly AbstractVariable[] TempVariables;

        public readonly AbstractProgram Body;

        public Function(AbstractVariable[] formals, AbstractVariable[] captured, AbstractVariable[] temps, AbstractProgram body)
        {
            FormalVariables = formals;
            CapturedVariables = captured;
            TempVariables = temps;
            Body = body;
        }

        public override string ToString()
        {
            return $"({string.Join(", ", FormalVariables.Concat(CapturedVariables))}) => {Body}";
        }

        public override ITerm Express() => Symbols.S_Lambda;
    }
}
