using ClaspCompiler.Textual;

namespace ClaspCompiler.Tokens
{
    internal sealed class Token
    {
        public readonly TokenType Type;
        public readonly SourceRef Source; 

        public Token(TokenType tt, SourceRef source)
        {
            Type = tt;
            Source = source;
        }

        public override string ToString() => Source.GetSnippet().ToString();
    }
}
