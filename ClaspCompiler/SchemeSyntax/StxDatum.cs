using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed class StxDatum : SyntaxBase
    {
        public readonly IAtom Value;

        public override bool IsAtom => true;
        public override bool IsNil => Value.IsNil;

        public StxDatum(IAtom value, SourceRef? source = null) : base(source)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
        public override void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
