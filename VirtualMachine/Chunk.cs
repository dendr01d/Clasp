using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMachine.Terms;

namespace VirtualMachine
{
    public ref struct Chunk
    {
        public readonly ReadOnlySpan<byte> Code;
        public readonly ReadOnlySpan<byte> Constants;

        public Chunk(byte[] code, byte[] constants)
        {
            Code = new ReadOnlySpan<byte>(code);
            Constants = new ReadOnlySpan<byte>(constants);
        }
    }
}
