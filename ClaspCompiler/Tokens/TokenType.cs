namespace ClaspCompiler.Tokens
{
    internal enum TokenType
    {
        LeftParen, RightParen,
        LeftBrack, RightBrack,
        OpenVec,
        DotOp,

        Integer, Symbol,
        True, False,

        Quote, Quasiquote,
        Unquote, UnquoteSplice,

        Syntax, Quasisyntax,
        Unsyntax, UnsyntaxSplice,

        Malformed,

        NewLine, Whitespace, Comment,

        EoF
    }
}
