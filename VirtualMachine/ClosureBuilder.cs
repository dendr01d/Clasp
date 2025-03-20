using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMachine.Terms;

namespace VirtualMachine
{
    internal struct ClosureBuilder
    {
        public readonly List<byte> Code;
        public readonly List<int> Const_Integers;
        public readonly List<string> Const_Strings;
        public readonly List<int> JumpTargets;

        public ClosureBuilder()
        {
            Code = new List<byte>();
            ConstantPool = new List<byte>();
        }


        public void AppendCode(params byte[] bytes) => Code.AddRange(bytes);

        public int AddConstant<T>(T value)
            where T : ITerm
        {
            int index = ConstantPool.Count;
            ConstantPool.AddRange(value.GetBytes());
            return index;
        }
    }
}
