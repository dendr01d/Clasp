using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class TypedExpression : ISemExp
    {
        public SchemeType Type { get; init; }
        public ISemExp Expression { get; init; }

        public TypedExpression(SchemeType type, ISemExp exp)
        {
            Type = type;
            Expression = exp;
        }

        public bool BreaksLine => false;
        public string AsString => $"({Type} {Expression})";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
