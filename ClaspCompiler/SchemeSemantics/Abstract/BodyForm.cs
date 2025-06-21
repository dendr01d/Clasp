namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal sealed record BodyForm(ISemTop[] Sequents, ISemExp TailValue) : IPrintable
    {
        public bool BreaksLine => Sequents.Length > 0;
        public string AsString => $"{string.Concat(Sequents.Select(x => $"{x} "))}{TailValue}";
        public void Print(TextWriter writer, int indent) => writer.WriteLineByLine([.. Sequents, TailValue], indent);
        public sealed override string ToString() => AsString;
    }
}
