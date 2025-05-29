using System.Collections;

using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed class StxPair : SyntaxBase, ICons<ISyntax>
    {
        public ISyntax Car { get; private set; }
        public ISyntax Cdr { get; private set; }

        public override bool IsAtom => false;
        public override bool IsNil => false;

        public StxPair(ISyntax car, ISyntax cdr, SourceRef? source = null) : base(source)
        {
            Car = car;
            Cdr = cdr;
        }

        public override string ToString() => IConsExtensions.ToString(this);
        public override void Print(TextWriter writer, int indent) => IConsExtensions.Print(this, writer, indent);

        public IEnumerator<ISyntax> GetEnumerator() => this.Enumerate();
        IEnumerator IEnumerable.GetEnumerator() => this.Enumerate();
    }
}
