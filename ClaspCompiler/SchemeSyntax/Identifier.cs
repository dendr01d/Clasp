using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed class Identifier : SyntaxBase
    {
        public readonly Symbol FreeSymbol;
        public readonly Symbol BindingSymbol;

        public Identifier(Symbol value, SourceRef? source = null) : base(source)
        {
            FreeSymbol = value;
            BindingSymbol = value;
        }

        public Identifier(Identifier id, Symbol bindingName) : base(id.Source)
        {
            FreeSymbol = id.FreeSymbol;
            BindingSymbol = bindingName;
        }

        public override bool IsAtom => true;
        public override bool IsNil => false;

        public override bool Equals(object? obj) => obj is Identifier id
            && FreeSymbol.Equals(id.FreeSymbol);
        public override int GetHashCode() => FreeSymbol.GetHashCode();

        public override bool CanBreak => false;
        public override string ToString() => FreeSymbol.ToString();
        public override void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
