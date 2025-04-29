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
            sb.AppendLine();

            int offset = 0;
            while (offset < chunk.Size)
            {
                offset = DisassembleInstruction(chunk, offset, sb);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.Append("== end ==");

            return sb.ToString();
        }

        private static int DisassembleInstruction(Chunk chunk, int offset, StringBuilder sb)
        {
            sb.Append($"{offset:0000} : ");

            OpCode instruction = (OpCode)chunk[offset];

            return instruction switch
            {
                OpCode.Jump => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(int))),
                OpCode.Jump_If => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(int))),
                OpCode.Jump_IfNot => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(int))),

                OpCode.Const_Boolean => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(bool))),
                OpCode.Const_Byte => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(byte))),
                OpCode.Const_Char => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(char))),
                OpCode.Const_FixNum => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(int))),
                OpCode.Const_FloNum => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(double))),
                OpCode.Const_Raw => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(ulong))),

                OpCode.TypeCast => Render1ArgInstruction(instruction, offset, sb, ReadBytes(chunk, offset, sizeof(byte))),

                _ => RenderSimpleInstruction(instruction, offset, sb)
            };
        }

        private static int _opCodeMaxLen = Enum.GetNames(typeof(OpCode)).Max(x => x.Length);
        private static string _opCodeFormat = $"{{0, {-1 * _opCodeMaxLen}}}";

        private static byte[] ReadBytes(Chunk chunk, int opOffset, int byteCount)
        {
            return chunk.ReadBytes(opOffset + 1, byteCount).ToArray().Reverse().ToArray();
        }

        private static void PrintOpCode(OpCode op, StringBuilder sb)
        {
            sb.Append(string.Format(_opCodeFormat, Enum.GetName(typeof(OpCode), op)));
        }

        private static int RenderSimpleInstruction(OpCode op, int offset, StringBuilder sb)
        {
            PrintOpCode(op, sb);
            sb.Append(" |");
            return offset + 1;
        }

        private static int Render1ArgInstruction(OpCode op, int offset, StringBuilder sb, byte[] arg)
        {
            RenderSimpleInstruction(op, offset, sb);
            sb.Append(' ');
            sb.Append(BitConverter.ToString(arg).Replace('-', ' '));

            return offset + 1 + arg.Length;
        }

    }
}
