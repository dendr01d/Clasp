using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Define(ISemVar Variable, ISemExp Value) : ISemDef
    {
        public bool BreaksLine => Value.BreaksLine;
        public string AsString => $"(define {Variable} {Value})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("define", [Variable, Value], indent);
        public sealed override string ToString() => AsString;
    }
}
