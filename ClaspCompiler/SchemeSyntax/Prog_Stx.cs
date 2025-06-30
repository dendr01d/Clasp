using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSyntax.Abstract;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed record Prog_Stx(ISyntax Body, SymbolFactory SymFactory) : IPrintable
    {
        public bool BreaksLine => Body.BreaksLine;
        public string AsString => $"(program {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(program ", ref indent);
            writer.Write(Body.AsString, indent);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
