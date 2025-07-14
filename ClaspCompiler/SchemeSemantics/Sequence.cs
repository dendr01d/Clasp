using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Sequence(Body Body, SourceRef Source) : ISemExp
    {
        public bool BreaksLine => Body.BreaksLine;
        public string AsString => $"(begin {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(begin ", ref indent);
            writer.Write(Body, indent);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}