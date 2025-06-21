namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal abstract record AbstractQuote : ISemQQ
    {
        protected abstract string Prefix { get; }
        protected abstract string Keyword { get; }
        public abstract ISemQQ Value { get; }

        public bool BreaksLine => Value.BreaksLine;
        public string AsString => $"{Prefix}{Value}";
        public void Print(TextWriter writer, int indent)
        {
            if (!Value.BreaksLine)
            {
                writer.Write(AsString);
            }
            else
            {
                writer.WriteIndenting($"({Keyword} ", ref indent);
                writer.Write(Value, indent);
                writer.Write(')');
            }
        }
        public sealed override string ToString() => AsString;
    }
}
