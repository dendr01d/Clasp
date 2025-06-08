using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps.Abstract;

namespace ClaspCompiler.IntermediateCps
{
    internal sealed class Prog_Cps : IPrintable
    {
        public readonly string Info;
        public readonly Dictionary<Label, ITail> LabeledTails;

        public static readonly Label StartLabel = new Label("start00");

        public Prog_Cps(string info, Dictionary<Label, ITail> labeledTails)
        {
            Info = info;
            LabeledTails = labeledTails;
        }

        public Prog_Cps(string info, ITail start)
            : this(info, new Dictionary<Label, ITail>() { { StartLabel, start } })
        { }

        public bool BreaksLine => true;
        public string AsString => $"(program {Info} {string.Join(' ', LabeledTails.Select(x => $"({x.Key} . {x.Value})"))})";
        public void Print(TextWriter writer, int indent)
        {
            writer.Write("(program "); //no indentation

            writer.Write(Info);

            writer.WriteLineIndent(indent);
            writer.WriteIndenting("  (", ref indent);

            writer.WriteLineByLine(LabeledTails, WriteLabeledTail, indent);

            writer.Write("))");
        }
        public sealed override string ToString() => AsString;

        private static void WriteLabeledTail(TextWriter writer, KeyValuePair<Label, ITail> labeledTail, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            writer.WriteIndenting(labeledTail.Key, ref indent);
            writer.WriteIndenting(" . (", ref indent);

            writer.Write(labeledTail.Value, indent);
            writer.Write("))");
        }
    }
}
