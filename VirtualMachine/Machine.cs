using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Buffers.Binary;
using VirtualMachine.Objects;

namespace VirtualMachine
{
    internal ref struct Machine
    {
        private readonly Chunk _chunk;

        private MachineResult _result;
        private int _ip;
        private int _frame;

        private RegisterStack _globalMemory = new();
        private RegisterStack _localMemory = new();

        private Term _accumulator;

        private Machine(Chunk program)
        {
            _chunk = program;

            _result = MachineResult.Undetermined;
            _ip = 0;
            _frame = 0;
        }

        private MachineResult Run()
        {
            while (_result == MachineResult.Undetermined)
            {
                OpCode instruction = (OpCode)_chunk.ReadByte(_ip++);

                switch (instruction)
                {
                    case (byte)OpCode.Op_Return:
                        _result = MachineResult.OK;
                        break;

                    case OpCode.Jump:
                        _ip = BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)));
                        break;
                    case OpCode.Jump_If:
                        if (_accumulator.IsTruthy)
                        {
                            _ip = BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)));
                        }
                        else
                        {
                            _ip += sizeof(int);
                        }
                        break;
                    case OpCode.Jump_IfNot:
                        if (_accumulator.IsFalsy)
                        {
                            _ip = BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)));
                        }
                        else
                        {
                            _ip += sizeof(int);
                        }
                        break;

                    case OpCode.Frame_Load:
                        _accumulator = _localMemory[_frame + BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)))];
                        _ip += sizeof(int);
                        break;
                    case OpCode.Frame_Store:
                        _localMemory[_frame + BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)))] = _accumulator;
                        _ip += sizeof(int);
                        break;
                    case OpCode.Frame_Swap:
                        {
                            Term temp = _accumulator;
                            int addr = _frame + BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)));
                            _ip += sizeof(int);
                            _accumulator = _localMemory[addr];
                            _localMemory[addr] = _accumulator;
                        }
                        break;

                    case OpCode.Local_Load:
                        _accumulator = _localMemory[BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)))];
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Store:
                        _localMemory[BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)))] = _accumulator;
                        _ip += sizeof(int);
                        break;
                    case OpCode.Local_Swap:
                        {
                            Term temp = _accumulator;
                            int addr = BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)));
                            _ip += sizeof(int);
                            _accumulator = _localMemory[addr];
                            _localMemory[addr] = _accumulator;
                        }
                        break;
                    case OpCode.Local_Push:
                        _localMemory.Push(_accumulator);
                        break;
                    case OpCode.Local_Pop:
                        _localMemory.Pop();
                        break;
                    case OpCode.Local_Shuffle:
                        {
                            Term temp = _accumulator;
                            _accumulator = _localMemory.Pop();
                            _localMemory.Push(temp);
                        }
                        break;

                    case OpCode.Global_Load:
                        _accumulator = _globalMemory[_frame + BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)))];
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Store:
                        _globalMemory[_frame + BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int)))] = _accumulator;
                        _ip += sizeof(int);
                        break;
                    case OpCode.Global_Push:
                        _globalMemory.Push(_accumulator);
                        break;
                    case OpCode.Global_Pop:
                        _globalMemory.Pop();
                        break;

                    case OpCode.Const_Boolean:
                        _accumulator = Term.Boolean(_chunk.ReadByte(_ip++) != 0);
                        break;
                    case OpCode.Const_Byte:
                        _accumulator = Term.Byte(_chunk.ReadByte(_ip++));
                        break;
                    case OpCode.Const_Char:
                        _accumulator = Term.Character((char)_chunk.ReadByte(_ip++));
                        break;
                    case OpCode.Const_FixNum:
                        _accumulator = Term.FixNum(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int))));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Const_FloNum:
                        _accumulator = Term.FloNum(BinaryPrimitives.ReadDoubleLittleEndian(_chunk.ReadBytes(_ip, sizeof(double))));
                        _ip += sizeof(double);
                        break;
                    //case OpCode.Const_Raw:
                    //    _accumulator = Term.RawNum(BinaryPrimitives.ReadUInt64LittleEndian(_chunk.ReadBytes(_ip, sizeof(ulong))));
                    //    _ip += sizeof(ulong);
                    //    break;

                    case OpCode.Eq:
                        _accumulator = Term.Boolean(_accumulator.Equals(_localMemory.Pop()));
                        break;
                    case OpCode.Neq:
                        _accumulator = Term.Boolean(!_accumulator.Equals(_localMemory.Pop()));
                        break;
                    case OpCode.Lt:
                        _accumulator = Term.Boolean(_localMemory.Pop().CompareTo(_accumulator) < 0);
                        break;
                    case OpCode.Leq:
                        _accumulator = Term.Boolean(_localMemory.Pop().CompareTo(_accumulator) <= 0);
                        break;
                    case OpCode.Gt:
                        _accumulator = Term.Boolean(_localMemory.Pop().CompareTo(_accumulator) > 0);
                        break;
                    case OpCode.Geq:
                        _accumulator = Term.Boolean(_localMemory.Pop().CompareTo(_accumulator) >= 0);
                        break;

                    case OpCode.Fix_Add:
                        _accumulator = Term.FixNum(_accumulator.AsFixNum + _localMemory.Pop().AsFixNum);
                        break;
                    case OpCode.Fix_Mul:
                        _accumulator = Term.FixNum(_accumulator.AsFixNum * _localMemory.Pop().AsFixNum);
                        break;
                    case OpCode.Fix_Sub:
                        _accumulator = Term.FixNum(_accumulator.AsFixNum - _localMemory.Pop().AsFixNum);
                        break;
                    case OpCode.Fix_Div:
                        _accumulator = Term.FixNum(_accumulator.AsFixNum / _localMemory.Pop().AsFixNum);
                        break;
                    case OpCode.Fix_Mod:
                        _accumulator = Term.FixNum(_accumulator.AsFixNum % _localMemory.Pop().AsFixNum);
                        break;

                    case OpCode.Flo_Add:
                        _accumulator = Term.FloNum(_accumulator.AsFloNum + _localMemory.Pop().AsFloNum);
                        break;
                    case OpCode.Flo_Mul:
                        _accumulator = Term.FloNum(_accumulator.AsFloNum * _localMemory.Pop().AsFloNum);
                        break;
                    case OpCode.Flo_Sub:
                        _accumulator = Term.FloNum(_accumulator.AsFloNum - _localMemory.Pop().AsFloNum);
                        break;
                    case OpCode.Flo_Div:
                        _accumulator = Term.FloNum(_accumulator.AsFloNum / _localMemory.Pop().AsFloNum);
                        break;
                    case OpCode.Flo_Mod:
                        _accumulator = Term.FloNum(_accumulator.AsFloNum % _localMemory.Pop().AsFloNum);
                        break;

                    case OpCode.Shift_L:
                        _accumulator = Term.ByteString(_accumulator.AsRawNum << BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int))), _accumulator.Tag);
                        _ip += sizeof(int);
                        break;
                    case OpCode.Shift_R:
                        _accumulator = Term.ByteString(_accumulator.AsRawNum >> BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int))), _accumulator.Tag);
                        _ip += sizeof(int);
                        break;

                    case OpCode.Bitwise_And:
                        _accumulator = Term.ByteString(_accumulator.AsRawNum & _localMemory.Pop().AsRawNum, _accumulator.Tag);
                        break;
                    case OpCode.Bitwise_Or:
                        _accumulator = Term.ByteString(_accumulator.AsRawNum | _localMemory.Pop().AsRawNum, _accumulator.Tag);
                        break;
                    case OpCode.Bitwise_Xor:
                        _accumulator = Term.ByteString(_accumulator.AsRawNum ^ _localMemory.Pop().AsRawNum, _accumulator.Tag);
                        break;

                    case OpCode.TypeCast:
                        _accumulator = Term.ByType(_accumulator.AsRawNum, _accumulator.AsObject, (TypeTag)_chunk.ReadByte(_ip++));
                        break;

                    case OpCode.Flo_To_Fix:
                        _accumulator = Term.FixNum(int.CreateTruncating(double.Truncate(_accumulator.AsFloNum)));
                        break;
                    case OpCode.Fix_To_Flo:
                        _accumulator = Term.FloNum(_accumulator.AsFixNum);
                        break;

                    case OpCode.Box:
                        _accumulator = Term.Box(_accumulator);
                        break;
                    case OpCode.Unbox:
                        if (_accumulator.Tag != TypeTag.Box) throw InvalidAccumulator(instruction, _accumulator);
                        _accumulator = _accumulator.AsBox.Contents;
                        break;

                    case OpCode.Cons:
                        _accumulator = Term.Cons(_localMemory.Pop(), _accumulator);
                        break;
                    case OpCode.Car:
                        if (_accumulator.Tag != TypeTag.Cons) throw InvalidAccumulator(instruction, _accumulator);
                        _accumulator = _accumulator.AsCons.Car;
                        break;
                    case OpCode.Cdr:
                        if (_accumulator.Tag != TypeTag.Cons) throw InvalidAccumulator(instruction, _accumulator);
                        _accumulator = _accumulator.AsCons.Cdr;
                        break;
                    case OpCode.Set_Car:
                        if (_accumulator.Tag != TypeTag.Cons) throw InvalidAccumulator(instruction, _accumulator);
                        _accumulator.AsCons.SetCar(_localMemory.Pop());
                        break;
                    case OpCode.Set_Cdr:
                        if (_accumulator.Tag != TypeTag.Cons) throw InvalidAccumulator(instruction, _accumulator);
                        _accumulator.AsCons.SetCdr(_localMemory.Pop());
                        break;

                    case OpCode.Vec_Make:
                        _accumulator = Term.Vector(BinaryPrimitives.ReadInt32LittleEndian(_chunk.ReadBytes(_ip, sizeof(int))));
                        _ip += sizeof(int);
                        break;
                    case OpCode.Vec_Emplace:
                        if (_accumulator.Tag != TypeTag.Vector) throw InvalidAccumulator(instruction, _accumulator);
                        if (_localMemory.Peek().Tag != TypeTag.FixNum) throw InvalidArg(instruction, _accumulator, _localMemory.Peek());
                        _accumulator.AsVector.Elements[_localMemory.Pop().AsFixNum] = _localMemory.Pop();
                        break;
                    case OpCode.Vec_Retrieve:
                        if (_accumulator.Tag != TypeTag.Vector) throw InvalidAccumulator(instruction, _accumulator);
                        if (_localMemory.Peek().Tag != TypeTag.FixNum) throw InvalidArg(instruction, _accumulator, _localMemory.Peek());
                        _accumulator = _accumulator.AsVector.Elements[_localMemory.Pop().AsFixNum];
                        break;
                    case OpCode.Vec_Length:
                        if (_accumulator.Tag != TypeTag.Vector) throw InvalidAccumulator(instruction, _accumulator);
                        _accumulator = Term.FixNum(_accumulator.AsVector.Elements.Length);
                        break;

                    default:
                        _result = MachineResult.BadOpCode;
                        break;
                }
            }

            return _result;
        }

        private static InvalidOperationException InvalidAccumulator(OpCode op, Term acc)
        {
            return new InvalidOperationException($"Invalid to perform operation {op} with accumulator {acc}.");
        }

        private static InvalidOperationException InvalidArg(OpCode op, Term acc, Term arg)
        {
            return new InvalidOperationException($"Invalid to perform operation {op} with accumulator {acc} and argument {arg}.");
        }

        public static (MachineResult, Term) Run(Chunk program)
        {
            Machine mx = new Machine(program);
            MachineResult result = mx.Run();
            Term finalTerm = mx._accumulator;

            return (result, finalTerm);
        }
    }
}
