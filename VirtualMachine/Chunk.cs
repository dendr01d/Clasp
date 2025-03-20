using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMachine.Terms;

namespace VirtualMachine
{
    internal struct Chunk
    {
        public readonly List<byte> Code;
        public readonly List<byte> ConstantPool;

        public Chunk()
        {
            Code = new List<byte>();
            ConstantPool = new List<byte>();
        }

        public void WriteToEnd(byte b) => Code.Add(b);
        public void WriteToEnd(params byte[] bytes) => Code.AddRange(bytes);

        public int WriteConstant<T>(T value)
            where T : ITerm
        {
            int index = ConstantPool.Count;
            ConstantPool.AddRange(value.GetBytes());
            return index;
        }
    }
}
