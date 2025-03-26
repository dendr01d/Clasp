using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Arguments
{
    internal sealed class Argument : AbstractArgument
    {
        public readonly AbstractProgram Value;
        public readonly AbstractArgument Next;

        public Argument(AbstractProgram value, AbstractArgument next)
        {
            Value = value;
            Next = next;
        }
    }
}
