using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class If : ISemSpec
    {
        public SpecialKeyword Keyword => SpecialKeyword.If;
        public ISemExp Condition { get; init; }
        public ISemExp Consequent { get; init; }
        public ISemExp Alternative { get; init; }

        public If(ISemExp cond, ISemExp consq, ISemExp alt)
        {
            Condition = cond;
            Consequent = consq;
            Alternative = alt;
        }

        public bool BreaksLine => true;
        public string AsString => $"(if {Condition} {Consequent} {Alternative})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("if", [Condition, Consequent, Alternative], indent);
        public sealed override string ToString() => AsString;
    }
}
