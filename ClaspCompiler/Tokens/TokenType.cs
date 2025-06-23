namespace ClaspCompiler.Tokens
{
    internal enum TokenType
    {
        LeftParen, RightParen,
        LeftBrack, RightBrack,
        OpenVec,

        Integer, Symbol,
        True, False,

        Malformed,

        NewLine, Whitespace, EoF
    }
}
