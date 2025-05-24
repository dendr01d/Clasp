using ClaspCompiler.Data;
using ClaspCompiler.Textual;

namespace ClaspCompiler.Syntax
{
    internal sealed class StxDatum : SyntaxBase
    {
        public readonly ITerm Value;

        public override bool IsAtom => true;
        public override bool IsNil => Value.IsNil;

        public StxDatum(ITerm value, SourceRef? source = null) : base(source)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
        public override void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
