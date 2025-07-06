using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record MultipleValues(ISemExp[] Values, SourceRef Source) : ISemAstNode
    {
        public SchemeType Type { get; init; } = AtomicType.Undefined;

        public bool BreaksLine => Values.Length > 3 || Values.Any(x => x.BreaksLine);
        public string AsString => $"({SpecialKeyword.Values}{string.Concat(Values.Select(x => $" {x}"))})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(SpecialKeyword.Values, Values, indent);
        public sealed override string ToString() => AsString;
    }
}
