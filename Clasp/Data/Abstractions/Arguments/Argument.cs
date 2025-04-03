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
        public readonly AbstractProgram Next;

        public Argument(AbstractProgram value, AbstractProgram next)
        {
            Value = value;
            Next = next;
        }

        public override string Express()
        {
            return Value.Express()
                + (Next is AbstractArgument aa
                ? aa.Express()
                : $" . {Next.Express()}");
        }
    }
}
