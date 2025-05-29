using System.Collections;
using System.Collections.Immutable;

using ClaspCompiler.IntermediateStackLang.Abstract;

namespace ClaspCompiler.IntermediateStackLang
{
    internal class Block : IPrintable
    {
        public readonly List<IStackInstr> Instructions;
        public Block(IEnumerable<IStackInstr> instrs)
        {
            Instructions = instrs.ToList();
        }

        public bool CanBreak => true;
        public override string ToString() => $"({string.Join(' ', Instructions)})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            if (Instructions.Any())
            {
                writer.Write(Instructions.First(), indent);

                for (int i = 1; i < Instructions.Count; ++i)
                {
                    writer.WriteLineIndent(indent);
                    writer.Write(Instructions[i], indent);
                }

                writer.WriteLineIndent(indent);
            }

            writer.Write(')');
        }

    }
}
