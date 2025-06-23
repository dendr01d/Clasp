namespace ClaspCompiler.SchemeTypes
{
    internal abstract record SchemeType : IPrintable, IEquatable<SchemeType>
    {
        public virtual bool BreaksLine => false;
        public abstract string AsString { get; }
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
