namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal abstract record TypeConstraint(uint SourceAstId) : IPrintable
    {
        public int StructuralDepth => 0;
        public virtual bool BreaksLine => false;
        public abstract string AsString { get; }
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
