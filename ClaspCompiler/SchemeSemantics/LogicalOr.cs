using ClaspCompiler;
using ClaspCompiler.SchemeSemantics.Abstract;

internal sealed record LogicalOr(ISemExp[] Arguments) : ISemExp
{
    public bool BreaksLine => Arguments.Any(x => x.BreaksLine);
    public string AsString => $"(or {string.Join(' ', Arguments.AsEnumerable())})";
    public void Print(TextWriter writer, int indent) => writer.WriteApplication("or", Arguments, indent);
    public sealed override string ToString() => AsString;
}
