namespace ClaspCompiler.Tokens
{
    internal enum TokenType
    {
        LeftParen, RightParen,
        LeftBracket, RightBracket,

        Integer, Symbol,
        Malformed,

        NewLine, Whitespace
    }
}
