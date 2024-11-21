using System.Text.RegularExpressions;

namespace Clasp.Lexer
{
    internal static class Lexer
    {
        //Derived from the lexical rules described at https://groups.csail.mit.edu/mac/ftpdir/scheme-reports/r5rs-html/r5rs_9.html#SEC73

        // Attempt to parse tokens using these regexes, in this order
        private static readonly string[] _regexes = new string[]
        {
            BuildRgx(TokenType.Comment        , @"(?>\;.*$)"),

            BuildRgx(TokenType.Identifier     , @"(?>([a-zA-Z\!\$\%\&\*\/\:\<\=\>\?\^\_\~][a-zA-Z\!\$\%\&\*\/\:\<\=\>\?\^\_\~0-9\+\-\.\@]*)|\+|\-|\.\.\.)"),
            BuildRgx(TokenType.Boolean        , @"(?>(#(?:[Tt][Rr][Uu][Ee]|[Ff][Aa][Ll][Ss][Ee]|[Tt]|[Ff])))"),

            BuildRgx(TokenType.DecReal        , @"(?>(?:#[Dd])?(?:\+)?(\-?[0-9]+\.[0-9]+))"),
            BuildRgx(TokenType.BinInteger     , @"(?>(?:#[Bb])(?:\+)?(\-?[01]+))"),
            BuildRgx(TokenType.OctInteger     , @"(?>(?:#[Oo])(?:\+)?(\-?[0-7]+))"),
            BuildRgx(TokenType.HexInteger     , @"(?>(?:#[Xx])(?:\+)?(\-?[0-9a-fA-F]+))"),
            BuildRgx(TokenType.DecInteger     , @"(?>(?:#[Dd])?(?:\+)?(\-?[0-9]+))"), //radix flag is optional for decimals

            BuildRgx(TokenType.Character      , @"(?>#\\(?:space|newline|tab|.))"),
            BuildRgx(TokenType.String         , @"(?>\""((?:[^\""|^\\]|\\""|\\\\)*)\"")"),

            BuildRgx(TokenType.OpenListParen  , @"\("),
            BuildRgx(TokenType.ClosingParen   , @"\)"),
            BuildRgx(TokenType.OpenVecParen   , @"\#\("),

            BuildRgx(TokenType.Quote          , @"\'"),
            BuildRgx(TokenType.Quasiquote     , @"\`"),
            BuildRgx(TokenType.Unquote        , @"\,"),
            BuildRgx(TokenType.UnquoteSplice  , @"\@\,"),

            BuildRgx(TokenType.Syntax         , @"\#\'"),
            BuildRgx(TokenType.QuasiSyntax    , @"\#\`"),
            BuildRgx(TokenType.Unsyntax       , @"\#\,"),
            BuildRgx(TokenType.UnsyntaxSplice , @"\#\@\,"),

            BuildRgx(TokenType.DotOperator    , @"\."),
            BuildRgx(TokenType.Undefined      , @"\#undefined"),

            BuildRgx(TokenType.Malformed      , @".*$"),
        };

        // Combines the enum name with the corresponding regex so we can tell which one actually matched
        private static string BuildRgx(TokenType tt, string pattern) => $"(?<{tt}>{pattern})";

        private static string _grammar => $"(?>{string.Join('|', _regexes)})";

        /// <summary>
        /// Split input text into a sequence of tokens using the Lexer's grammar rules. The text is
        /// first split into lines to accurately track the position of each token.
        /// </summary>
        public static IEnumerable<Token> Lex(string text)
        {
            return LexLines(text.Split(System.Environment.NewLine));
        }

        /// <summary>
        /// Read the text of the specified file line-by-line, and parse it into a sequence of tokens.
        /// </summary>
        public static IEnumerable<Token> LexFile(string path)
        {
            IEnumerable<string> sourceLines = File.ReadAllLines(path);
            return LexLines(sourceLines);
        }

        /// <summary>
        /// Parse a sequence of tokens out of the aggregate text formed from the provided lines of input.
        /// The line divisions are used to record the relative position of each token.
        /// </summary>
        /// <exception cref="LexingException"></exception>
        public static IEnumerable<Token> LexLines(IEnumerable<string> inputLines)
        {
            if (!inputLines.Any())
            {
                return Array.Empty<Token>();
            }

            List<Token> output = new List<Token>();
            List<LexingException> malformedInputs = new List<LexingException>();

            int lineNo = 1; //line numbers in text files are usually 1-indexed?

            foreach(string line in inputLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var lineMatches = Regex.Matches(line, _grammar);

                    foreach(Match match in lineMatches)
                    {
                        TokenType matchedType = ExtractMatchedTokenType(match);

                        if (matchedType == TokenType.Comment)
                        {
                            continue; //maybe I'll do something with this later?
                        }
                        else if (matchedType == TokenType.Malformed)
                        {
                            LexingException ex = new LexingException(string.Format(
                                "Malformed input on line {0} beginning at index {1}: {2}",
                                lineNo, match.Index + 1, match.Value));
                            malformedInputs.Add(ex);
                        }
                        else
                        {
                            output.Add(Token.Tokenize(matchedType, match.Value, lineNo, match.Index + 1));
                        }
                    }
                }

                ++lineNo;
            }

            if (malformedInputs.Any())
            {
                throw new AggregateException("Malformed input/s lexemes found in input.", malformedInputs);
            }

            return output;
        }

        private static TokenType ExtractMatchedTokenType(Match regexMatch)
        {
            // find the name of the first regex (in the _grammar) that successfully matched
            string name = regexMatch.Groups.Values.Skip(1).First(x => x.Success).Name;
            return Enum.TryParse(name, out TokenType matchedType)
                ? matchedType
                : TokenType.Malformed;
        }
    }


    internal enum TokenType
    {
        Comment,

        Identifier,
        Boolean,

        //Number,
        DecReal,
        BinInteger,
        OctInteger,
        DecInteger,
        HexInteger,

        Character,
        String,

        OpenListParen,
        OpenVecParen,
        ClosingParen,
        Quote,
        Quasiquote,
        Unquote,
        UnquoteSplice,

        Syntax,
        QuasiSyntax,
        Unsyntax,
        UnsyntaxSplice,

        DotOperator,
        Undefined,

        Malformed
    }
}
