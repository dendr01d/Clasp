using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateVarLang.Abstract;

namespace ClaspCompiler.IntermediateVarLang
{
    internal class Block : IPrintable
    {
        public readonly List<HashSet<Var>> Liveness;
        public readonly ILocInstr[] Instructions;

        public Block(IEnumerable<HashSet<Var>> liveness, IEnumerable<ILocInstr> instrs)
        {
            Liveness = liveness.ToList();
            Instructions = instrs.ToArray();
        }

        private static string LivenessToString(HashSet<Var> liveMems)
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
                string comment = Liveness.Count == 0 ? string.Empty : LivenessToString(Liveness.Skip(1).First());
                writer.WriteWithComment(Instructions.First(), padding, comment);

                for (int i = 1; i < Instructions.Length; ++i)
                {
                    writer.WriteLineIndent(indent);
                    comment = Liveness.Count == 0 ? string.Empty : LivenessToString(Liveness[i - 1]);
                    writer.WriteWithComment(Instructions[i], padding, comment);
                }

                writer.WriteLineIndent(indent);
            }

            writer.Write(')');
        }

    }
}
