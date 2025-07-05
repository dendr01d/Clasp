namespace ClaspCompiler.CompilerData
{
    /// <summary>
    /// Represents a semantic variable -- a memory location in which data is stored.
    /// </summary>
    internal abstract record VarBase(string Name) : IPrintable
    {
        public bool BreaksLine => false;
        public string AsString => Name;
        public void Print(TextWriter writer, int indent) => writer.Write(Name);
        public sealed override string ToString() => Name;
    }
}