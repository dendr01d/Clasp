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

        private byte[] _workspace = new byte[sizeof(double)];
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
                        _accumulator = new Span<byte>(new byte[1]);
                        _localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 1).CopyTo(_accumulator);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Store1:
                        _accumulator.CopyTo(_localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 1));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Pop1:
                        _accumulator = new Span<byte>(new byte[1]);
                        _localMemory.PopValues(1).CopyTo(_accumulator);
                        break;

                    case OpCode.Local_Load2:
                        _accumulator = new Span<byte>(new byte[2]);
                        _localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 2).CopyTo(_accumulator);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Store2:
                        _accumulator.CopyTo(_localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 2));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Pop2:
                        _accumulator = new Span<byte>(new byte[2]);
                        _localMemory.PopValues(2).CopyTo(_accumulator);
                        break;

                    case OpCode.Local_Load4:
                        _accumulator = new Span<byte>(new byte[4]);
                        _localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 4).CopyTo(_accumulator);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Store4:
                        _accumulator.CopyTo(_localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 4));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Pop4:
                        _localMemory.PopValues(4);
                        break;

                    case OpCode.Local_Load8:
                        _accumulator = new Span<byte>(new byte[8]);
                        _localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 8).CopyTo(_accumulator);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Store8:
                        _accumulator.CopyTo(_localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 8));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Pop8:
                        _accumulator = new Span<byte>(new byte[8]);
                        _localMemory.PopValues(8).CopyTo(_accumulator);
                        break;

                    case OpCode.Local_Push:
                        _localMemory.Push(_accumulator);
                        break;

                    case OpCode.Global_Load1:
                        _accumulator = new Span<byte>(new byte[1]);
                        _localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 1).CopyTo(_accumulator);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Store1:
                        _accumulator.CopyTo(_localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 1));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Pop1:
                        _accumulator = new Span<byte>(new byte[1]);
                        _localMemory.PopValues(1).CopyTo(_accumulator);
                        break;

                    case OpCode.Global_Load2:
                        _accumulator = new Span<byte>(new byte[2]);
                        _localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 2).CopyTo(_accumulator);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Store2:
                        _accumulator.CopyTo(_localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 2));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Pop2:
                        _accumulator = new Span<byte>(new byte[2]);
                        _localMemory.PopValues(2).CopyTo(_accumulator);
                        break;

                    case OpCode.Global_Load4:
                        _accumulator = new Span<byte>(new byte[4]);
                        _localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 4).CopyTo(_accumulator);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Store4:
                        _accumulator.CopyTo(_localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 4));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Pop4:
                        _localMemory.PopValues(4);
                        break;

                    case OpCode.Global_Load8:
                        _accumulator = new Span<byte>(new byte[8]);
                        _localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 8).CopyTo(_accumulator);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Store8:
                        _accumulator.CopyTo(_localMemory.Slice(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadCode(_ip, sizeof(int))), 8));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Pop8:
                        _accumulator = new Span<byte>(new byte[8]);
                        _localMemory.PopValues(8).CopyTo(_accumulator);
                        break;

                    case OpCode.Global_Push:
                        _localMemory.Push(_accumulator);
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
                        bool result = _localMemory.PeekValues(_accumulator.Length).SequenceEqual(_accumulator);
                        _accumulator = new Span<byte>(new byte[1]);
                        if (result) _accumulator[0] = 0x1;
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
                        {
                            int arg = BinaryPrimitives.ReadInt32LittleEndian(_localMemory.PopValues(sizeof(int)));
                            BinaryPrimitives.WriteInt32LittleEndian(_accumulator,
                                BinaryPrimitives.ReadInt32LittleEndian(_accumulator) + arg);
                        }
                        break;
                    case OpCode.Fix_Mul:
                        {
                            int arg = BinaryPrimitives.ReadInt32LittleEndian(_localMemory.PopValues(sizeof(int)));
                            BinaryPrimitives.WriteInt32LittleEndian(_accumulator,
                                BinaryPrimitives.ReadInt32LittleEndian(_accumulator) * arg);
                        }
                        break;
                    case OpCode.Fix_Sub:
                        {
                            int arg = BinaryPrimitives.ReadInt32LittleEndian(_localMemory.PopValues(sizeof(int)));
                            BinaryPrimitives.WriteInt32LittleEndian(_accumulator,
                                BinaryPrimitives.ReadInt32LittleEndian(_accumulator) - arg);
                        }
                        break;
                    case OpCode.Fix_Div:
                        {
                            int arg = BinaryPrimitives.ReadInt32LittleEndian(_localMemory.PopValues(sizeof(int)));
                            BinaryPrimitives.WriteInt32LittleEndian(_accumulator,
                                BinaryPrimitives.ReadInt32LittleEndian(_accumulator) / arg);
                        }
                        break;
                    case OpCode.Fix_Mod:
                        {
                            int arg = BinaryPrimitives.ReadInt32LittleEndian(_localMemory.PopValues(sizeof(int)));
                            BinaryPrimitives.WriteInt32LittleEndian(_accumulator,
                                BinaryPrimitives.ReadInt32LittleEndian(_accumulator) % arg);
                        }
                        break;

                    case OpCode.Flo_Add:
                        {
                            double arg = BinaryPrimitives.ReadDoubleLittleEndian(_localMemory.PopValues(sizeof(double)));
                            BinaryPrimitives.WriteDoubleLittleEndian(_accumulator,
                                BinaryPrimitives.ReadDoubleLittleEndian(_accumulator) + arg);
                        }
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
