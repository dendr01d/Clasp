using ClaspCompiler.SchemeCore.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeCore
{
    internal sealed class Determination : ICoreExp
    {
        public ICoreExp Condition { get; init; }
        public ICoreExp Consequent { get; init; }
        public ICoreExp Alternative { get; init; }
        public SchemeType Type { get; init; }

        public Determination(ICoreExp cond, ICoreExp consq, ICoreExp alt, SchemeType type)
        {
            Condition = cond;
            Consequent = consq;
            Alternative = alt;
            Type = type;
        }

        public bool BreaksLine => true;
        public string AsString => $"(if {Condition} {Consequent} {Alternative})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("if", [Condition, Consequent, Alternative], indent);
        public sealed override string ToString() => AsString;
    }
}
