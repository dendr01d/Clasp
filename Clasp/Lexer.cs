using System.Text.RegularExpressions;

namespace Clasp
{
    internal static class Lexer
    {
        private static readonly string[] _regexes = new string[]
        {
            rgx(TokenType.LeftParen, @"\("),
            rgx(TokenType.RightParen, @"\)"),
            rgx(TokenType.QuoteMarker, @"\'"),
            rgx(TokenType.QuasiquoteMarker, @"\`"),
            rgx(TokenType.UnquoteMarker, @"\,"),
            rgx(TokenType.UnquoteSplicingMarker, @"\,\@"),
            rgx(TokenType.Ellipsis, @"\.\.\."),
            rgx(TokenType.DotMarker, @"\."),
            rgx(TokenType.Number, @"\d+"),
            rgx(TokenType.Boolean, @"(?>\#t|\#f)"),
            rgx(TokenType.Symbol, @"(?>\+|\-|[^\s\(\)\+\-\.][^\s\(\)\.]*)")
        };

        private static string rgx(TokenType tt, string pattern) => $"(?<{tt}>{pattern})";

        private static string _grammar => $"(?>{string.Join('|', _regexes)})";

        public static IEnumerable<Token> Lex(string input)
        {
            int lParens = input.Count(x => x == '(');
            int rParens = input.Count(x => x == ')');

            if (lParens != rParens)
            {
                throw new LexingException($"Missing one or more {(lParens < rParens ? "L-parens" : "R-parens")}");
            }

            return Regex.Matches(input, _grammar)
                .Select(x => Token.Tokenize(getGroupName(x.Groups), x.Value, x.Index));
        }

        private static string getGroupName(GroupCollection groups)
        {
            return groups.Values.Skip(1).First(x => x.Success).Name;
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Text}")]
    internal class Token
    {
        public readonly string Text;
        public readonly TokenType TType;
        public readonly int SourceIndex;

        protected Token(string s, TokenType t, int index)
        {
            Text = s;
            TType = t;
            SourceIndex = index;
        }

        public static Token Tokenize(string patternName, string s, int index)
        {
            return new Token(s, _reverseLookup[patternName], index);
        }

        public override string ToString() => $"{{{Text}}}";

        private static readonly Dictionary<string, TokenType> _reverseLookup =
            Enum.GetValues<TokenType>()
            .Cast<TokenType>()
            .ToDictionary(x => x.ToString(), x => x);
            
    }

    internal enum TokenType
    {
        LeftParen, RightParen,
        Symbol, Number, Boolean,
        QuoteMarker, DotMarker,
        QuasiquoteMarker, UnquoteMarker, UnquoteSplicingMarker, Ellipsis,
        Error
    }
}
