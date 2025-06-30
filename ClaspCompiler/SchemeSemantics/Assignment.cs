using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Assignment(ISemVar Variable, ISemExp Value, uint AstId) : ISemCmd
    {
        public bool BreaksLine => Value.BreaksLine;
        public string AsString => $"({SpecialKeyword.SetBang.Name} {Variable} {Value})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(SpecialKeyword.SetBang.Name, [Variable, Value], indent);
        public sealed override string ToString() => AsString;
    }
}
