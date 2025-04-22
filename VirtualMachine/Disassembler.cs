using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine
{
    internal static class Disassembler
    {
        public static string Disassemble(Chunk chunk, string name)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"== {name} ==");

            int offset = 0;
            while (offset < chunk.Size)
            {
                offset = DisassembleInstruction(chunk, offset, sb);
                sb.AppendLine();
            }

            sb.Append("== end ==");

            return sb.ToString();
        }

        private static int DisassembleInstruction(Chunk chunk, int offset, StringBuilder sb)
        {
            sb.Append($"{offset:0000} ");

            OpCode instruction = (OpCode)chunk[offset];

            switch (instruction)
            {
                case OpCode.Op_Return:
                    return RenderSimpleInstruction(, offset, sb);
                case OpCode.Jump:
                    return RenderSimpleInstruction(Enum.GetName(typeof(OpCode), instruction), offset, sb);
                case OpCode.Jump_If:
                    break;
                case OpCode.Jump_IfNot:
                    break;
                case OpCode.Local_Load:
                    break;
                case OpCode.Local_Store:
                    break;
                case OpCode.Local_Push:
                    break;
                case OpCode.Local_Pop:
                    break;
                case OpCode.Local_Swap:
                    break;
                case OpCode.Global_Load:
                    break;
                case OpCode.Global_Store:
                    break;
                case OpCode.Global_Push:
                    break;
                case OpCode.Global_Pop:
                    break;
                case OpCode.Const_Boolean:
                    break;
                case OpCode.Const_Byte:
                    break;
                case OpCode.Const_Char:
                    break;
                case OpCode.Const_FixNum:
                    break;
                case OpCode.Const_FloNum:
                    break;
                case OpCode.Const_Raw:
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
                case OpCode.Raw_Add:
                    break;
                case OpCode.Raw_Mul:
                    break;
                case OpCode.Raw_Sub:
                    break;
                case OpCode.Raw_Div:
                    break;
                case OpCode.Raw_Mod:
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
                case OpCode.TypeCast:
                    break;
                case OpCode.Fix_From_Raw:
                    break;
                case OpCode.Fix_From_Flo:
                    break;
                case OpCode.Flo_From_Raw:
                    break;
                case OpCode.Flo_From_Fix:
                    break;
                case OpCode.Raw_From_Fix:
                    break;
                case OpCode.Raw_From_Flo:
                    break;
                case OpCode.Box:
                    break;
                case OpCode.Unbox:
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
                case OpCode.Vec_Make:
                    break;
                case OpCode.Vec_Emplace:
                    break;
                case OpCode.Vec_Retrieve:
                    break;
                case OpCode.Vec_Length:
                    break;
                case OpCode.Func_Make:
                    break;
                case OpCode.Func_Apply:
                    break;
                case OpCode.Port_Get_Console:
                    break;
                case OpCode.Port_Get_File:
                    break;
                case OpCode.Port_Open_Write:
                    break;
                case OpCode.Port_Open_Read:
                    break;

                    //default:
                    //    sb.Append($"Unknown OpCode: {(int)instruction}");
                    //    return offset + 1;
            }
        }

        private static int RenderSimpleInstruction(OpCode op, int offset, StringBuilder sb)
        {
            sb.Append(Enum.GetName(typeof(OpCode), op));
            return offset + 1;
        }

    }
}
