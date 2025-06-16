namespace ClaspCompiler.SchemeSemantics.Abstract.TypeConstraints
{
    internal abstract class TypeConstraint : IPrintable
    {
        public ISemExp SourceNode { get; init; }

        protected TypeConstraint(ISemExp src) => SourceNode = src;

        public bool BreaksLine => false;
        public abstract string AsString { get; }
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
