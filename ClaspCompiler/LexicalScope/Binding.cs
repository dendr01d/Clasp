using ClaspCompiler.SchemeData;

namespace ClaspCompiler.LexicalScope
{
    internal enum BindingType { Special, Primitive, Variable, Module, Macro }

    internal sealed record Binding(Symbol UniqueName, BindingType BoundType) : IPrintable
    {
        public bool BreaksLine => false;
        public string AsString => $"({BoundType} . {UniqueName})";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}
