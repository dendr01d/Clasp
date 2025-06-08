using ClaspCompiler.SchemeSyntax.Abstract;

namespace ClaspCompiler.SchemeSyntax
{
    /// <summary>
    /// A syntactic program.
    /// </summary>
    internal sealed class Prog_Stx : IPrintable
    {
        public readonly ScopeSetMap LexicalScopes;
        public readonly ISyntax Body;

        public Prog_Stx(ScopeSetMap lexScope, ISyntax body)
        {
            LexicalScopes = lexScope;
            Body = body;
        }

        public bool BreaksLine => Body.BreaksLine;
        public string AsString => $"(program {LexicalScopes} {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            writer.WriteLineIndent("program", indent);
            writer.Write(LexicalScopes, indent);
            writer.WriteLineIndent(indent);
            writer.Write(Body.ToString(), indent);
            writer.Write(')');
        }
        public sealed override string ToString() => AsString;
    }
}
