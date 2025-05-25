using ClaspCompiler.Common;

namespace ClaspCompiler.ANormalForms
{
    internal sealed class ProgC0 : IPrintable
    {
        public Var[] Locals { get; init; }
        public Dictionary<string, ITail> LabeledTails { get; init; }

        public ProgC0(Dictionary<string, ITail> labeledTails, params Var[] locals)
        {
            LabeledTails = labeledTails;
            Locals = locals;
        }

        public ProgC0(ITail entry, params Var[] locals)
            : this(new Dictionary<string, ITail>() { { "start", entry } }, locals)
        { }

        private string FormatLocals() => string.Format("({0})", string.Join(' ', Locals.Select(x => x.ToString())));

        public override string ToString()
        {
            return string.Format("(program {0} ({1}))",
                FormatLocals(),
                string.Join(' ', LabeledTails.Select(x => $"({x.Key} . {x.Value}")));
        }

        public void Print(TextWriter writer, int indent)
        {
            writer.Write("(program "); // no hanging indent
            writer.WriteLineIndent(FormatLocals(), indent);

            writer.WriteIndenting("  (", ref indent);

            foreach (var pair in LabeledTails)
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
