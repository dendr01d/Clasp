using System.Linq;

namespace Clasp.Data.Text
{
    public class Token : ISourceTraceable
    {
        public readonly string Text;
        public readonly TokenType TType;
        public Blob SourceText { get; private set; }
        public SourceCode Location { get; private set; }

        public string SurroundingLine => SourceText[Location.NormalizedLineNumber]; //lines of text are 1-indexed

        protected Token(string s, TokenType t, Blob text, SourceCode loc)
        {
            Text = s;
            TType = t;
            SourceText = text;
            Location = loc;
        }

        public static Token Tokenize(TokenType tType, string s, Blob text, SourceCode loc)
        {
            return new Token(s, tType, text, loc);
        }

        private static readonly TokenType[] _staticMarkers = new TokenType[]
        {
            TokenType.OpenListParen,
            TokenType.OpenVecParen,
            TokenType.ClosingParen,
            TokenType.Quote,
            TokenType.Quasiquote,
            TokenType.Unquote,
            TokenType.UnquoteSplice,
            TokenType.Syntax,
            TokenType.QuasiSyntax,
            TokenType.Unsyntax,
            TokenType.UnsyntaxSplice,
            TokenType.DotOperator,
            TokenType.Undefined,
        };

        public override string ToString()
        {
            if (_staticMarkers.Contains(TType))
            {
                return string.Format("({0})", TType);
            }
            else
            {
                return string.Format("({0}){1}", TType, Text);
            }
        }
    }
}
