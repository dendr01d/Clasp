using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Body(ISemDef[] Definitions, ISemCmd[] Commands, ISemExp Value, uint AstId) : ISemBody
    {
        private IEnumerable<IPrintable> GetTerms() => ((IPrintable[])Definitions).Concat(Commands).Append(Value);
        public bool BreaksLine => Definitions.Length > 0 || Value.BreaksLine;
        public string AsString => string.Join(' ', GetTerms());
        public void Print(TextWriter writer, int indent) => writer.WriteLineByLine(GetTerms(), indent);
        public sealed override string ToString() => AsString;
    }
}
