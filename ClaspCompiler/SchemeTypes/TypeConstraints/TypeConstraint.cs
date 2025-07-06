using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeTypes.TypeConstraints
{
    internal abstract record TypeConstraint(ISemAstNode Node) : IPrintable
    {
        public virtual bool BreaksLine => false;
        public abstract string AsString { get; }
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
