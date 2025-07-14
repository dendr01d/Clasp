using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Application(ISemExp Procedure, ISemExp[] Arguments, SourceRef Source) : ISemExp
    {
        public bool BreaksLine => true;
        public string AsString => $"({SpecialKeyword.Apply.Name} {Procedure} {string.Join(' ', Arguments.AsEnumerable())})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(Procedure, Arguments, indent);
        public sealed override string ToString() => AsString;
    }
}