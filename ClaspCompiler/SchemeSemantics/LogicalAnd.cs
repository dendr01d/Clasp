using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record LogicalAnd(ISemExp[] Arguments) : ISemExp
    {
        public bool BreaksLine => Arguments.Any(x => x.BreaksLine);
        public string AsString => $"(and {string.Join(' ', Arguments.AsEnumerable())})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("and", Arguments, indent);
        public sealed override string ToString() => AsString;
    }
}
