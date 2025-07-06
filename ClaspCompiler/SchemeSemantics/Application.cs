using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.Text;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Application(ISemExp Procedure, FormalArguments Arguments, SourceRef Source) : ISemExp
    {
        public bool BreaksLine => Arguments.BreaksLine;
        public string AsString => $"({SpecialKeyword.Apply.Name} {Procedure} {Arguments})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(Procedure, Arguments.Values, indent);
        public sealed override string ToString() => AsString;
    }
}
