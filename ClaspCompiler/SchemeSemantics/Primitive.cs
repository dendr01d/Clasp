using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Primitive(PrimitiveOperator Operator) : ISemLiteral
    {
        public SourceRef Source => SourceRef.DefaultSyntax;
        public SchemeType Type => Operator.Type;

        public bool BreaksLine => false;
        public string AsString => Operator.AsString;
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
