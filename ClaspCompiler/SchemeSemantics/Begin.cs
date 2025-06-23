using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Begin(ISemExp[] Sequents) : ISemExp
    {
        public bool BreaksLine => true;
        public string AsString => $"(begin {string.Join(' ', Sequents.AsEnumerable())})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("begin", Sequents, indent);
        public sealed override string ToString() => AsString;
    }
}

