using System.Text.RegularExpressions;

namespace Clasp
{
    internal static class Lexer
    {
        private static readonly string[] _regexes = new string[]
        {
            rgx(TokenType.Comment, @"(?>\;.*$)"),
            rgx(TokenType.QuotedString, @"(?>""(?:\\.|[^""\\])*"")"),
            rgx(TokenType.Character, @"(?>#\\(?:space|newline|tab|.))"),
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

        public static IEnumerable<Token> Lex(string text)
        {
            return LexLines(text.Split(System.Environment.NewLine));
        }

        public static IEnumerable<Token> LexLines(IEnumerable<string> inputLines)
        {
            if (!inputLines.Any())
            {
                return Array.Empty<Token>();
            }

            int lParens = inputLines.Select(y => y.Count(x => x == '(')).Sum();
            int rParens = inputLines.Select(y => y.Count(x => x == ')')).Sum();

            if (lParens != rParens)
            {
                Tuple<int, int> check = LocateExtraParen(lParens < rParens, inputLines);
                throw new LexingException($"Missing one or more {(lParens < rParens ? "L-parens" : "R-parens")} around block at row {check.Item1}, column {check.Item2}.");
            }

            IEnumerable<string> cleanLines = inputLines
                .Select(x => new string(x.TakeWhile(y => y != ';').ToArray()))
                .Select(x => x.Trim());

            List<Token> output = new List<Token>();

            int lineNo = 0;

            foreach(string line in cleanLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var newTokens = Regex.Matches(line, _grammar)
                        .Select(x => Token.Tokenize(getGroupName(x.Groups), x.Value, lineNo, x.Index + 1))
                        .Where(x => x.TType != TokenType.Comment);
                    output.AddRange(newTokens);
                }

                ++lineNo;
            }

            return output;
        }

        public static IEnumerable<IEnumerable<Token>> SegmentTokens(IEnumerable<Token> tokens)
        {
            Queue<Token> queue = new Queue<Token>(tokens);

            int parenLevel = 0;
            while (queue.Any())
            {
                List<Token> segment = new List<Token>()
                {
                    PullToken(ref queue, ref parenLevel)
                };

                while (parenLevel != 0)
                {
                    segment.Add(PullToken(ref queue, ref parenLevel));
                }

                yield return segment;
            }

            yield break;
        }

        private static Token PullToken(ref Queue<Token> queue, ref int parenLevel)
        {
            if (queue.Peek().TType == TokenType.LeftParen) ++parenLevel;
            else if (queue.Peek().TType == TokenType.RightParen) --parenLevel;

            return queue.Dequeue();
        }

        private static Tuple<int, int> LocateExtraParen(bool fromLeft, IEnumerable<string> input)
        {
            int parenCounter = 0;

            int row = 0;
            int col = 0;

            foreach(string line in (fromLeft ? input : input.Reverse()))
            {
                ++row;

                foreach(char c in (fromLeft ? line : line.Reverse()))
                {
                    if (parenCounter < 0)
                    {
                        return new Tuple<int, int>(
                            fromLeft ? row : input.Count() - row,
                            fromLeft ? col : line.Length - col);
                    }
                    else
                    {
                        ++col;

                        if (c == '(') parenCounter += (fromLeft ? 1 : -1);
                        if (c == ')') parenCounter += (fromLeft ? -1 : 1);
                    }
                }
            }

            return new Tuple<int, int>(0, 0);
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
        Symbol,
        QuoteMarker, DotMarker,
        QuasiquoteMarker, UnquoteMarker, UnquoteSplicingMarker,
        QuotedString, Number, Boolean, Character,
        Ellipsis,
        Comment, Error
    }
}
