using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record FormalArguments(ISemExp Argument, FormalArguments? Next) : ISemSubForm
    {
        public bool BreaksLine => false;
        public string AsString => Argument.ToString() + (Next is null ? string.Empty : $" {Next}");
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
