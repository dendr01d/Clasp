namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal sealed record BindingForm(ISemVar Variable, ISemExp Value) : IPrintable
    {
        public bool BreaksLine => Value.BreaksLine;
        public string AsString => $"[{Variable} {Value}]";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting($"[{Variable} ", ref indent);
            if (Value.BreaksLine)
            {
                writer.WriteLineIndent(indent);
            }
            else
            {
                writer.Write(' ');
            }
            writer.Write(Value, indent);
            writer.Write(']');
        }
        public sealed override string ToString() => AsString;
    }
}
