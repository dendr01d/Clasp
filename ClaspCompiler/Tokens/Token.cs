using ClaspCompiler.Textual;

namespace ClaspCompiler.Tokens
{
    internal sealed record Token
    {
        public TokenType Type { get; init; }
        public SourceRef Source { get; init; }
        public string Text { get; init; }

        public Token(TokenType tt, SourceRef source, string text)
        {
            Type = tt;
            Source = source;
            Text = text;
        }

        public override string ToString() => Text;
    }
}
