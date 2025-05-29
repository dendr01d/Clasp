using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed class Identifier : SyntaxBase
    {
        public readonly Symbol SymbolicName;
        public readonly Symbol BindingName;

        public Identifier(Symbol value, SourceRef? source = null) : base(source)
        {
            SymbolicName = value;
            BindingName = value;
        }

        public Identifier(Identifier id, Symbol bindingName) : base(id.Source)
        {
            SymbolicName = id.SymbolicName;
            BindingName = bindingName;
        }

        public override bool IsAtom => true;
        public override bool IsNil => false;

        public override bool Equals(object? obj) => obj is Identifier id && SymbolicName.Equals(id.SymbolicName);
        public override int GetHashCode() => SymbolicName.GetHashCode();
        public override void Print(TextWriter writer, int indent) => writer.Write(ToString());
        public override string ToString() => SymbolicName.ToString();
    }
}
