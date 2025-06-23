using ClaspCompiler.SchemeSyntax.Abstract;

namespace ClaspCompiler.SchemeSyntax
{
    /// <summary>
    /// A syntactic program.
    /// </summary>
    internal sealed class Prog_Stx : IPrintable
    {
        public readonly ScopeSetMap LexicalScopes;
        public readonly ISyntax[] TopLevelForms;

        public Prog_Stx(ScopeSetMap lexScope, ISyntax[] topLevelForms)
        {
            LexicalScopes = lexScope;
            TopLevelForms = topLevelForms;
        }

        public bool BreaksLine => TopLevelForms.Length > 1 || TopLevelForms[0].BreaksLine;
        public string AsString => $"(program {LexicalScopes} {TopLevelForms})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            writer.WriteLineIndent("program", indent);
            writer.Write(LexicalScopes, indent);
            writer.WriteLineIndent(indent);
            writer.WriteLineByLine(TopLevelForms, indent);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
