using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Buffers.Binary;

namespace VirtualMachine
{
    internal ref struct Machine
    {
        private readonly Chunk _chunk;

        private MachineResult _result;
        private int _ip;
        private int _frame;

        private RegisterStack<byte> _globalMemory = new RegisterStack<byte>();
        private RegisterStack<byte> _localMemory = new RegisterStack<byte>();

        private byte[] _workspace = new byte[sizeof(Int64)];
        private Span<byte> _accumulator;

        private Machine(Chunk program)
        {
            _chunk = program;

            _result = MachineResult.Undetermined;
            _ip = 0;
            _frame = 0;

            _accumulator = new Span<byte>(_workspace);
        }

        private MachineResult Run()
        {
            while (_result == MachineResult.Undetermined)
            {
                OpCode instruction = _chunk.ReadOpCode(_ip++);

                switch (instruction)
                {
                    case (byte)OpCode.Op_Return:
                        _result = MachineResult.OK;
                        break;

                    case OpCode.Jump:
                        _ip = BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int)));
                        break;
                    case OpCode.Jump_If:
                        if (BinaryPrimitives.ReadUInt64LittleEndian(_accumulator) != 0)
                        {
                            _ip = BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int)));
                        }
                        else
                        {
                            _ip += sizeof(int);
                        }
                        break;
                    case OpCode.Jump_IfNot:
                        if (BinaryPrimitives.ReadUInt64LittleEndian(_accumulator) == 0)
                        {
                            _ip = BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int)));
                        }
                        else
                        {
                            _ip += sizeof(int);
                        }
                        break;

                    case OpCode.Local_Load1:
                        _accumulator = _localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 1);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Store1:
                        _accumulator.CopyTo(_localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 1));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Push1:
                        _localMemory.Push(_accumulator);
                        break;
                    case OpCode.Local_Pop1:
                        _localMemory.PopValues(1);
                        break;

                    case OpCode.Local_Load2:
                        break;
                    case OpCode.Local_Store2:
                        break;
                    case OpCode.Local_Push2:
                        break;
                    case OpCode.Local_Pop2:
                        break;
                    case OpCode.Local_Load4:
                        break;
                    case OpCode.Local_Store4:
                        break;
                    case OpCode.Local_Push4:
                        break;
                    case OpCode.Local_Pop4:
                        break;
                    case OpCode.Local_Load8:
                        break;
                    case OpCode.Local_Store8:
                        break;
                    case OpCode.Local_Push8:
                        break;
                    case OpCode.Local_Pop8:
                        break;
                    case OpCode.Global_Load1:
                        break;
                    case OpCode.Global_Store1:
                        break;
                    case OpCode.Global_Push1:
                        break;
                    case OpCode.Global_Pop1:
                        break;
                    case OpCode.Global_Load2:
                        break;
                    case OpCode.Global_Store2:
                        break;
                    case OpCode.Global_Push2:
                        break;
                    case OpCode.Global_Pop2:
                        break;
                    case OpCode.Global_Load4:
                        break;
                    case OpCode.Global_Store4:
                        break;
                    case OpCode.Global_Push4:
                        break;
                    case OpCode.Global_Pop4:
                        break;
                    case OpCode.Global_Load8:
                        break;
                    case OpCode.Global_Store8:
                        break;
                    case OpCode.Global_Push8:
                        break;
                    case OpCode.Global_Pop8:
                        break;
                    case OpCode.Cons:
                        break;
                    case OpCode.Car:
                        break;
                    case OpCode.Cdr:
                        break;
                    case OpCode.Set_Car:
                        break;
                    case OpCode.Set_Cdr:
                        break;
                    case OpCode.Eq:
                        break;
                    case OpCode.Neq:
                        break;
                    case OpCode.Lt:
                        break;
                    case OpCode.Leq:
                        break;
                    case OpCode.Gt:
                        break;
                    case OpCode.Geq:
                        break;
                    case OpCode.Fix_Add:
                        break;
                    case OpCode.Fix_Mul:
                        break;
                    case OpCode.Fix_Sub:
                        break;
                    case OpCode.Fix_Div:
                        break;
                    case OpCode.Fix_Mod:
                        break;
                    case OpCode.Flo_Add:
                        break;
                    case OpCode.Flo_Mul:
                        break;
                    case OpCode.Flo_Sub:
                        break;
                    case OpCode.Flo_Div:
                        break;
                    case OpCode.Flo_Mod:
                        break;
                    case OpCode.Shift_L:
                        break;
                    case OpCode.Shift_R:
                        break;
                    case OpCode.Bitwise_And:
                        break;
                    case OpCode.Bitwise_Or:
                        break;
                    case OpCode.Bitwise_Xor:
                        break;
                    default:
                        _result = MachineResult.BadOpCode;
                        break;
                }
            }

            return _result;
        }


        //public static MachineResult Run(Chunk program)
        //{
        //    Machine mx = new Machine(program);
        //    return mx.Run();
        //}
    }
}
