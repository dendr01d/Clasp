using ClaspCompiler.SchemeSyntax.Abstract;

namespace ClaspCompiler.SchemeSyntax
{
    /// <summary>
    /// A syntactic program.
    /// </summary>
    internal class ProgS1
    {
        //public readonly Dictionary<uint, ScopeMap> PhasedScopes;
        public ISyntax Body { get; private set; }

        public ProgS1(ISyntax body)
        {
            //PhasedScopes = [];
            Body = body;
        }

        public bool CanBreak => true;
        public override string ToString() => $"(program () {Body})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(program ", ref indent);
            writer.WriteLineIndent("(...)", indent);
            writer.Write(Body, indent);
            writer.Write(')');
        }
    }
}
