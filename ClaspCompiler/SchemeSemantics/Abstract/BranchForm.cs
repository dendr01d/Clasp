namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal sealed record BranchForm(ISemExp Condition, ISemExp Consequent) : IPrintable
    {
        public bool BreaksLine => Condition.BreaksLine || Consequent.BreaksLine;
        public string AsString => $"[{Condition} {Consequent}]";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('[', ref indent);
            writer.WriteIndenting(Condition, ref indent);
            writer.WriteIndenting(' ', ref indent);
            if (Consequent.BreaksLine)
            {
                writer.WriteLineIndent(indent);
            }
            else
            {
                writer.Write(' ');
            }
            writer.Write(Consequent, indent);
            writer.Write(']');
        }
        public sealed override string ToString() => AsString;
    }
}
