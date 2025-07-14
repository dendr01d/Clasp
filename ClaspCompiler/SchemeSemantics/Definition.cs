using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Definition(ISemVar Variable, ISemExp Value, SourceRef Source) : ISemForm
    {
        public bool BreaksLine => true;
        public string AsString => $"({SpecialKeyword.Define.Name} {Variable} {Value})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteApplication(SpecialKeyword.Define.Name, [Variable, Value], indent);
        }
        public sealed override string ToString() => AsString;
    }
}