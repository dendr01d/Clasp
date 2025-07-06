using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Annotation(ISemExp Expression, SchemeType Type) : ISemExp
    {
        public SourceRef Source => Expression.Source;

        public bool BreaksLine => Expression.BreaksLine;
        public string AsString => Expression.AsString;
        public void Print(TextWriter writer, int indent) => Expression.Print(writer, indent);
        public sealed override string ToString() => AsString;
    }
}
