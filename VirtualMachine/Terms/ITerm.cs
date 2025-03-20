using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Terms
{
    internal interface ITerm : IEquatable<ITerm>
    {
        public byte[] GetBytes();
    }
}
