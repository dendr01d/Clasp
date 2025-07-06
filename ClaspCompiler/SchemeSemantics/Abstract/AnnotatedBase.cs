using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal abstract record AnnotatedBase<T>(T Expression, SchemeType Type) : ISemExp
        where T : ISemExp
    {
        public SourceRef Source => Expression.Source;

        public bool BreaksLine => Expression.BreaksLine;
        public string AsString => Expression.AsString;
        public void Print(TextWriter writer, int indent) => Expression.Print(writer, indent);
        public sealed override string ToString() => AsString;
    }
}
