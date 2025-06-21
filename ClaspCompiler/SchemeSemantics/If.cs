using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record If(ISemExp Condition, ISemExp Consequent, ISemExp Alternative) : ISemExp
    {
        public bool BreaksLine => true;
        public string AsString => $"(if {Condition} {Consequent} {Alternative})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("if", [Condition, Consequent, Alternative], indent);
        public sealed override string ToString() => AsString;
    }
}
