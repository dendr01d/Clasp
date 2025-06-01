using System.Collections.Immutable;

using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateLocLang.Abstract;

namespace ClaspCompiler.IntermediateLocLang
{
    internal class BinaryBlock : IPrintable
    {
        public readonly List<ImmutableHashSet<Var>> Liveness;
        public readonly BinaryInstruction[] Instructions;

        public BinaryBlock(IEnumerable<ImmutableHashSet<Var>> liveness, IEnumerable<BinaryInstruction> instrs)
        {
            Liveness = liveness.ToList();
            Instructions = instrs.ToArray();
        }

        public BinaryBlock(IEnumerable<BinaryInstruction> instrs)
            : this([], instrs)
        { }

        private static string LivenessToString(ImmutableHashSet<Var> liveMems)
        {
            return string.Format("; {{{0}}} ",
                string.Join(", ", liveMems.Select(x => x.ToString())));
        }

        public bool CanBreak => true;
        public override string ToString() => $"({string.Join(' ', Instructions.AsEnumerable())})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            if (Instructions.Any())
            {
                int padding = -25;
                string comment = Liveness.Count == 0
                    ? string.Empty
                    : LivenessToString(Liveness.Skip(1).First());
                writer.WriteWithComment(Instructions.First(), padding, comment);

                for (int i = 1; i < Instructions.Length; ++i)
                {
                    writer.WriteLineIndent(indent);
                    comment = Liveness.Count == 0
                        ? string.Empty
                        : LivenessToString(Liveness[i - 1]);
                    writer.WriteWithComment(Instructions[i], padding, comment);
                }

                writer.WriteLineIndent(indent);
            }

            writer.Write(')');
        }

    }
}
