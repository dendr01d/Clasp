using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record FormalParameters(ISemVar Parameter, ISemParameters? Next) : ISemParameters
    {
        public bool BreaksLine => false;
        public string AsString => Parameter.ToString() + (Next is null ? string.Empty : $" {Next}");
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;

    }
}
