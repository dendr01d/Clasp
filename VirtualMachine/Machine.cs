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
        public static readonly int WordSize = sizeof(int);

        private MachineResult _result;
        private int _ip;
        private readonly Chunk _chunk;

        private List<int> _staticMemory = new List<int>();
        private List<int> _globalMemory = new List<int>();
        private List<int> _localMemory = new List<int>();

        private int _accumulator;

        private Machine(Chunk program)
        {
            _result = MachineResult.Undetermined;
            _ip = 0;
            _chunk = program;
        }

        //private MachineResult Run()
        //{
        //    while (_result == MachineResult.Undetermined)
        //    {
        //        OpCode instruction = (OpCode)_chunk.Code[_ip++];

        //        switch (instruction)
        //        {
        //            case (byte)OpCode.Op_Return:
        //                _result = MachineResult.OK;
        //                break;

        //            case OpCode.Load_Local_Acc:
        //                _localMemory.Slice(_chunk.Code[_ip++], _chunk.Code[_ip++]).CopyTo(_accumulator);
        //                break;

        //            case OpCode.Load_Local_Arg1:
        //                _localMemory.Slice(_chunk.Code[_ip++], _chunk.Code[_ip++]).CopyTo(_arg1);
        //                break;

        //            case OpCode.Load_Local_Arg2:
        //                _localMemory.Slice(_chunk.Code[_ip++], _chunk.Code[_ip++]).CopyTo(_arg2);
        //                break;

        //            case OpCode.Mutate_Local:
        //                _accumulator.CopyTo(_localMemory.Slice(_chunk.Code[_ip++], _chunk.Code[_ip++]));
        //                break;

        //            case OpCode.Push_Local:
        //                _localMemory.Push(_accumulator);
        //                break;

        //            #region Comparisons
        //            case OpCode.Eq:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    (_accumulator.SequenceEqual(_arg1)) ? 1 : 0);
        //                break;

        //            case OpCode.Neq:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    (_accumulator.SequenceEqual(_arg1)) ? 0 : 1);
        //                break;

        //            case OpCode.Lt:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    (_accumulator.SequenceCompareTo(_arg1) < 0) ? 1 : 0);
        //                break;

        //            case OpCode.Leq:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    (_accumulator.SequenceCompareTo(_arg1) <= 0) ? 1 : 0);
        //                break;

        //            case OpCode.Gt:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    (_accumulator.SequenceCompareTo(_arg1) > 0) ? 1 : 0);
        //                break;

        //            case OpCode.Geq:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    (_accumulator.SequenceCompareTo(_arg1) >= 0) ? 1 : 0);
        //                break;
        //            #endregion

        //            #region Integer Arithmetic
        //            case OpCode.IAdd:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadInt64LittleEndian(_accumulator)
        //                    + BinaryPrimitives.ReadInt64LittleEndian(_arg1));
        //                break;

        //            case OpCode.ISub:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadInt64LittleEndian(_accumulator)
        //                    - BinaryPrimitives.ReadInt64LittleEndian(_arg1));
        //                break;

        //            case OpCode.IMul:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadInt64LittleEndian(_accumulator)
        //                    * BinaryPrimitives.ReadInt64LittleEndian(_arg1));
        //                break;

        //            case OpCode.SDiv:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadInt64LittleEndian(_accumulator)
        //                    / BinaryPrimitives.ReadInt64LittleEndian(_arg1));
        //                break;

        //            case OpCode.UDiv:
        //                BinaryPrimitives.WriteUInt64LittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadUInt64LittleEndian(_accumulator)
        //                    / BinaryPrimitives.ReadUInt64LittleEndian(_arg1));
        //                break;

        //            case OpCode.SMod:
        //                BinaryPrimitives.WriteInt64LittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadInt64LittleEndian(_accumulator)
        //                    % BinaryPrimitives.ReadInt64LittleEndian(_arg1));
        //                break;

        //            case OpCode.UMod:
        //                BinaryPrimitives.WriteUInt64LittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadUInt64LittleEndian(_accumulator)
        //                    % BinaryPrimitives.ReadUInt64LittleEndian(_arg1));
        //                break;
        //            #endregion

        //            #region Floating-Point Arithmetic
        //            case OpCode.FAdd:
        //                BinaryPrimitives.WriteDoubleLittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadDoubleLittleEndian(_accumulator)
        //                    + BinaryPrimitives.ReadDoubleLittleEndian(_arg1));
        //                break;

        //            case OpCode.FSub:
        //                BinaryPrimitives.WriteDoubleLittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadDoubleLittleEndian(_accumulator)
        //                    - BinaryPrimitives.ReadDoubleLittleEndian(_arg1));
        //                break;

        //            case OpCode.FMul:
        //                BinaryPrimitives.WriteDoubleLittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadDoubleLittleEndian(_accumulator)
        //                    * BinaryPrimitives.ReadDoubleLittleEndian(_arg1));
        //                break;

        //            case OpCode.FDiv:
        //                BinaryPrimitives.WriteDoubleLittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadDoubleLittleEndian(_accumulator)
        //                    / BinaryPrimitives.ReadDoubleLittleEndian(_arg1));
        //                break;

        //            case OpCode.FMod:
        //                BinaryPrimitives.WriteDoubleLittleEndian(_accumulator,
        //                    BinaryPrimitives.ReadDoubleLittleEndian(_accumulator)
        //                    % BinaryPrimitives.ReadDoubleLittleEndian(_arg1));
        //                break;

        //            #endregion

        //            default:
        //                _result = MachineResult.BadOpCode;
        //                break;
        //        }
        //    }

        //    return _result;
        //}


        //public static MachineResult Run(Chunk program)
        //{
        //    Machine mx = new Machine(program);
        //    return mx.Run();
        //}
    }
}
