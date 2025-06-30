using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Definition(ISemVar Variable, ISemExp Value, uint AstId) : ISemDef
    {
        public bool BreaksLine => Value.BreaksLine;
        public string AsString => $"({SpecialKeyword.Define.Name} {Variable} {Value})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(SpecialKeyword.Define.Name, [Variable, Value], indent);
        public sealed override string ToString() => AsString;
    }
}
