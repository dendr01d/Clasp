using System.Collections;
using System.Collections.Immutable;

using ClaspCompiler.IntermediateStackLang.Abstract;

namespace ClaspCompiler.IntermediateStackLang
{
    internal class UnaryBlock : IPrintable
    {
        public readonly List<UnaryInstruction> Instructions;
        public UnaryBlock(IEnumerable<UnaryInstruction> instrs)
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
