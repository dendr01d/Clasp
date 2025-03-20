using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine
{
    internal static class Disassembler
    {
        public static string Disassemble(ClosureBuilder chunk, string name)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"== {name} ==");

            int offset = 0;
            while (offset < chunk.Code.Count)
            {
                offset = DisassembleInstruction(chunk, offset, sb);
                sb.AppendLine();
            }

            sb.Append("== end ==");

            return sb.ToString();
        }

        private static int DisassembleInstruction(ClosureBuilder chunk, int offset, StringBuilder sb)
        {
            sb.Append($"{offset:0000} ");

            OpCode instruction = (OpCode)chunk.Code[offset];

            switch(instruction)
            {
                case OpCode.Op_Return:
                    return RenderSimpleInstruction(Enum.GetName(typeof(OpCode), instruction), offset, sb);

                default:
                    sb.Append($"Unknown OpCode: {(int)instruction}");
                    return offset + 1;
            }
        }

        private static int RenderSimpleInstruction(string? name, int offset, StringBuilder sb)
        {
            sb.Append(name ?? "???");
            return offset + 1;
        }

    }
}
