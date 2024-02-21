namespace Clasp
{
    internal static class Lexer
    {
        public static IEnumerable<Token> Lex(string input)
        {
            int lParens = input.Count(x => x == '(');
            int rParens = input.Count(x => x == ')');

            if (lParens != rParens)
            {
                throw new LexingException($"Missing one or more {(lParens < rParens ? "L-parens" : "R-parens")}");
            }

            string spaced = input
                .Replace("(", " ( ")
                .Replace(")", " ) ")
                .Replace("'", " ' ");

            IEnumerable<string> pieces = spaced.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return pieces.Select(x => Token.Tokenize(x));
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Text}")]
    internal class Token
    {
        public readonly string Text;
        public readonly TokenType TType;

        protected Token(string s, TokenType t)
        {
            Text = s;
            TType = t;
        }

        public static Token Tokenize(string s)
        {
            if (s[0] == '(')
            {
                return new Token("(", TokenType.LeftParen);
            }
            else if (s[0] == ')')
            {
                return new Token(")", TokenType.RightParen);
            }
            else if (s[0] == '\'')
            {
                return new Token("'", TokenType.QuoteMarker);
            }
            else if (s[0] == '.')
            {
                return new Token(".", TokenType.DotMarker);
            }
            else if (double.TryParse(s, out double result))
            {
                return new Token(result.ToString(), TokenType.Number);
            }
            else
            {
                return new Token(s, TokenType.Symbol);
            }
        }

        public override string ToString() => $"{{{Text}}}";
    }

    internal enum TokenType
    {
        LeftParen, RightParen,
        Symbol, Number,
        QuoteMarker, DotMarker
    }
}
