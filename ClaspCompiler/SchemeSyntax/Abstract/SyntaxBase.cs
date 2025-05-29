using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax.Abstract
{
    internal abstract class SyntaxBase : ISyntax
    {
        public readonly SourceRef? Source;
        //scope information as well. yikes

        public abstract bool IsAtom { get; }
        public abstract bool IsNil { get; }

        protected SyntaxBase(SourceRef? source) => Source = source;

        public abstract override string ToString();
        public abstract void Print(TextWriter writer, int indent);
    }
}
