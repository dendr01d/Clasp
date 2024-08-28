using System.Text.RegularExpressions;

namespace Clasp
{
    internal static class Lexer
    {
        private static readonly string[] _regexes = new string[]
        {
            rgx(TokenType.Comment, @"(?>\;.*$)"),
            rgx(TokenType.VecParen, @"\#\("),
            rgx(TokenType.LeftParen, @"\("),
            rgx(TokenType.RightParen, @"\)"),
            rgx(TokenType.QuoteMarker, @"\'"),
            rgx(TokenType.QuasiquoteMarker, @"\`"),
            rgx(TokenType.UnquoteSplicingMarker, @"\,\@"),
            rgx(TokenType.UnquoteMarker, @"\,"),
            rgx(TokenType.Ellipsis, @"\.\.\."),
            rgx(TokenType.DotMarker, @"\."),
            rgx(TokenType.Number, @"\d+"),
            rgx(TokenType.Boolean, @"(?>\#t|\#f)"),
            rgx(TokenType.Symbol, @"(?>\+|\-|[^\s\(\)\+\-\.][^\s\(\)\.]*)"),
        };

        private static string rgx(TokenType tt, string pattern) => $"(?<{tt}>{pattern})";

        private static string _grammar => $"(?>{string.Join('|', _regexes)})";

        public static IEnumerable<Token> Lex(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Array.Empty<Token>();
            }

            int lParens = input.Count(x => x == '(');
            int rParens = input.Count(x => x == ')');

            if (lParens != rParens)
            {
                int check = LocateExtraParen(lParens < rParens, input);
                throw new LexingException($"Missing one or more {(lParens < rParens ? "L-parens" : "R-parens")} around block at position {check}");
            }

            IEnumerable<string> lines = input.Split(System.Environment.NewLine);
            int lineNo = 1;

            List<Token> output = new List<Token>();

            foreach(string line in lines)
            {
                var newTokens = Regex.Matches(line, _grammar)
                    .Select(x => Token.Tokenize(getGroupName(x.Groups), x.Value, lineNo, x.Index + 1))
                    .Where(x => x.TType != TokenType.Comment);
                output.AddRange(newTokens);
                lineNo++;
            }

            return output;
        }

        private static int LocateExtraParen(bool fromLeft, string input)
        {
            int parenCounter = 0;

            IEnumerator<char> text = (fromLeft ? input : input.Reverse()).GetEnumerator();

            int pos = 0;
            while (parenCounter >= 0 && text.MoveNext())
            {
                ++pos;
                if (text.Current == '(') parenCounter += (fromLeft ? 1 : -1);
                if (text.Current == ')') parenCounter += (fromLeft ? -1 : 1);
            }

            return (fromLeft ? pos : input.Length - pos);
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
        public readonly int SourceLine;

        protected Token(string s, TokenType t, int line, int index)
        {
            Text = s;
            TType = t;
            SourceIndex = index;
            SourceLine = line;
        }

        public static Token Tokenize(string patternName, string s, int line, int index)
        {
            return new Token(s, _reverseLookup[patternName], line, index);
        }

        public override string ToString() => $"[{Text}]";

        private static readonly Dictionary<string, TokenType> _reverseLookup =
            Enum.GetValues<TokenType>()
            .Cast<TokenType>()
            .ToDictionary(x => x.ToString(), x => x);
            
    }

    internal enum TokenType
    {
        LeftParen, RightParen, VecParen,
        Symbol, Number, Boolean,
        QuoteMarker, DotMarker,
        QuasiquoteMarker, UnquoteMarker, UnquoteSplicingMarker, Ellipsis,
        Comment, Error
    }
}
