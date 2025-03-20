using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine
{
    internal sealed class Machine
    {
        private MachineResult _result;
        private int _ip;
        private readonly ClosureBuilder _chunk;

        private Machine(ClosureBuilder program)
        {
            _ip = 0;
            _chunk = program;
        }

        private MachineResult Run()
        {
            while (_result == MachineResult.Undetermined)
            {
                OpCode instruction = (OpCode)_chunk.Code[_ip++];

                switch (instruction)
                {
                    case (byte)OpCode.Op_Return:
                        _result = MachineResult.OK;
                        break;

                    default:
                        _result = MachineResult.BadOpCode;
                        break;
                }
            }

            return _result;
        }
        

        public static MachineResult Run(ClosureBuilder program)
        {
            Machine mx = new Machine(program);
            return mx.Run();
        }
    }
}
