using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class If : ISemExp
    {
        public ISemExp Condition { get; init; }
        public ISemExp Consequent { get; init; }
        public ISemExp Alternative { get; init; }
        public MetaData MetaData { get; init; }

        public If(ISemExp cond, ISemExp consq, ISemExp alt, MetaData? meta = null)
        {
            Condition = cond;
            Consequent = consq;
            Alternative = alt;
            MetaData = meta ?? new();
        }

        public bool BreaksLine => true;
        public string AsString => $"(if {Condition} {Consequent} {Alternative})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("if", [Condition, Consequent, Alternative], indent);
        public sealed override string ToString() => AsString;
    }
}
