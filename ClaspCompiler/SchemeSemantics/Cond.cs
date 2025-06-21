using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Cond(BranchForm[] Branches, ISemExp? ElseValue) : ISemExp
    {
        public bool BreaksLine => true;
        public string AsString => string.Concat([
            "(cond",
            string.Join(' ', Branches.AsEnumerable()),
            ElseValue is null ? string.Empty : $" [else {ElseValue}]",
            ")"
        ]);
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(cond ", ref indent);

            writer.WriteLineByLine(Branches, indent + 1);

            if (ElseValue is not null)
            {
                writer.WriteLineIndent(indent);
                writer.WriteIndenting("[else ", ref indent);
                writer.Write(ElseValue, indent);
                writer.Write(']');
            }

            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
