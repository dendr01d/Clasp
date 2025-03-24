using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMachine.Terms;

namespace VirtualMachine
{
    public class ChunkBuilder
    {
        public readonly List<byte> Code;
        public readonly List<byte> Constants;

        public ChunkBuilder()
        {
            Code = new List<byte>();
            Constants = new List<byte>();
        }

        public void AppendCode(params byte[] bytes) => Code.AddRange(bytes);

        public int AddConstant(params byte[] bytes)
        {
            int index = Constants.Count;
            Constants.AddRange(bytes);
            return index;
        }

        public Chunk Finalize()
        {
            return new Chunk(Code, Constants);
        }
    }
}
