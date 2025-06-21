using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Let(BindingForm[] Bindings, BodyForm Body) : ISemExp
    {
        public bool BreaksLine => true;
        public string AsString => $"(let {string.Join(' ', Bindings.AsEnumerable())} {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(let ", ref indent);

            writer.Write('(');
            writer.WriteLineByLine(Bindings, indent + 1);
            writer.Write(')');

            writer.WriteLineIndent(indent);
            writer.Write(Body, indent);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
