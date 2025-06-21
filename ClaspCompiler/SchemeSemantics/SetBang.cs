using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record SetBang(ISemVar Variable, ISemExp NewValue) : ISemExp
    {
        public bool BreaksLine => NewValue.BreaksLine;
        public string AsString => $"(set! {Variable} {NewValue})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("set!", [Variable, NewValue], indent);
        public sealed override string ToString() => AsString;
    }
}
