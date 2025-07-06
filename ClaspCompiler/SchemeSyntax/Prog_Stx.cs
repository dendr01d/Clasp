using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSyntax.Abstract;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed record Prog_Stx(ISyntax TopLevelForms, SymbolFactory SymFactory) : IPrintable
    {
        public bool BreaksLine => TopLevelForms.BreaksLine;
        public string AsString => $"(program {TopLevelForms})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(program ", ref indent);
            writer.Write(TopLevelForms.AsString, indent);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}