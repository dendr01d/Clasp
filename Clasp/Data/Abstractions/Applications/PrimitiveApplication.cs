using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Abstractions.Arguments;
using Clasp.Data.Abstractions.Values;
using Clasp.Data.Abstractions.Variables;

namespace Clasp.Data.Abstractions.Applications
{
    /// <summary>
    /// Represents the procedural execution of a <see cref="PrimitiveProcedure"/>.
    /// </summary>
    class PrimitiveApplication : AbstractApplication
    {
        public readonly AbstractVariable Operator;

        public PrimitiveApplication(AbstractVariable op, AbstractArgument arguments)
            : base(arguments)
        {
            Operator = op;
        }

        public override string Express()
        {
            return $"({Operator.Express()}{Arguments.Express()})";
        }
    }
}
