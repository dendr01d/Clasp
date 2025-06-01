using ClaspCompiler.CompilerData;

namespace ClaspCompiler.IntermediateStackLang
{
    internal class ProgStack0 : IPrintable
    {
        public readonly Dictionary<Label, UnaryBlock> LabeledBlocks;

        public ProgStack0(Dictionary<Label, UnaryBlock> labeledBlocks)
        {
            LabeledBlocks = labeledBlocks;
        }

        public bool CanBreak => true;
        public override string ToString()
        {
            return string.Format("(program () ({0}))",
                string.Join(' ', LabeledBlocks.Select(x => string.Format("({0} . {1})", x.Key, x.Value))));
        }

        public void Print(TextWriter writer, int indent)
        {
            writer.Write("(program ");

            writer.WriteLineIndent("()", indent);
            writer.WriteIndenting("  (", ref indent);

            foreach (var pair in LabeledBlocks)
            {
                writer.Write('(');
                writer.Write(pair.Key);
                writer.WriteLineIndent(" .", indent);
                writer.Write(pair.Value, indent);
                writer.Write(')');
            }

            writer.Write("))");
        }
    }
}
