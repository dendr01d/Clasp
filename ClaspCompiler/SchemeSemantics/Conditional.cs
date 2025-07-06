using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Conditional(ISemExp Condition, ISemExp Consequent, ISemExp Alternative, SourceRef Source) : ISemExp
    {
        public bool BreaksLine => true;
        public string AsString => $"({SpecialKeyword.If.Name} {Condition} {Consequent} {Alternative})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(SpecialKeyword.If.Name, [Condition, Consequent, Alternative], indent);
        public sealed override string ToString() => AsString;
    }
}
