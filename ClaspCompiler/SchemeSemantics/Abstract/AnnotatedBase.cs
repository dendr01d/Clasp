using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal abstract record AnnotatedBase<T>(T Expression, SchemeType Type)
        : ISemAnnotated
        where T : ISemExp
    {
        public abstract IVisibleTypePredicate? VisiblePredicate { get; }
        public SourceRef Source => Expression.Source;

        public bool BreaksLine => Expression.BreaksLine;
        public string AsString => Expression.AsString;
        public void Print(TextWriter writer, int indent) => Expression.Print(writer, indent);
        public sealed override string ToString() => AsString;
    }
}