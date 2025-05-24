using System.Text.RegularExpressions;

using ClaspCompiler.Textual;
using ClaspCompiler.Tokens;

namespace ClaspCompiler.CompilerPasses
{
    internal static class Tokenize
    {
        private static readonly Dictionary<TokenType, string> _regexes = new()
        {
            { TokenType.NewLine   , @"$" },
            { TokenType.Whitespace, @"\s+" },

            { TokenType.LeftParen , @"\(" },
            { TokenType.RightParen, @"\)" },
            { TokenType.Integer   , @"-?[1-9]\d*" },
            { TokenType.Symbol    , @"(?:\+|\-|[a-zA-Z]+)" },
            { TokenType.Malformed , @".*" }

            /*
            { TokenType.Whitespace     , @"\s+" },
            { TokenType.Comment        , @"(?>\;.*$)" },
            
            { TokenType.Boolean        , @"(?>(#(?:[Tt][Rr][Uu][Ee]|[Ff][Aa][Ll][Ss][Ee]|[Tt]|[Ff])))" },
            
            { TokenType.BinInteger     , @"(?>(?:#[Bb])(?:\+)?(\-?[01]+))" },
            { TokenType.OctInteger     , @"(?>(?:#[Oo])(?:\+)?(\-?[0-7]+))" },
            { TokenType.HexInteger     , @"(?>(?:#[Xx])(?:\+)?(\-?[0-9a-fA-F]+))" },
            { TokenType.DecInteger     , @"(?>(?:#[Dd])?(?:\+)?(\-?[0-9]+))"), // radix flag is optional for decimals },
            { TokenType.DecReal        , @"(?>(?:#[Dd])?(?:\+)?(\-?[0-9]+\.[0-9]+))"), // ditto },
            
            { TokenType.Character      , @"(?>#\\(?:(?:[^\s\(\)\-]+)|.))"), // #\ followed by a single character, or alias with no spaces or parens },
            { TokenType.String         , @"(?>\""((?:[^\""|^\\]|\\""|\\\\)*)\"")"), // exclude double-quotes and backslashes unless escaped },
            
            { TokenType.Symbol         , @"(?>([a-zA-Z\!\$\%\&\*\/\:\<\=\>\?\^_\~][a-zA-Z0-9\!\$\%\&\*\/\:\<\=\>\?\^_\~\+\-\.\@]*)|\+|\-|\.\.\.)" },
            
            { TokenType.OpenListParen  , @"\(" },
            { TokenType.ClosingParen   , @"\)" },
            { TokenType.OpenVecParen   , @"\#\(" },
            
            { TokenType.Quote          , @"\'" },
            { TokenType.Quasiquote     , @"\`" },
            { TokenType.Unquote        , @"\," },
            { TokenType.UnquoteSplice  , @"\@\," },
            
            { TokenType.Syntax         , @"\#\'" },
            { TokenType.QuasiSyntax    , @"\#\`" },
            { TokenType.Unsyntax       , @"\#\," },
            { TokenType.UnsyntaxSplice , @"\#\@\," },
            
            { TokenType.DotOperator    , @"\." },
            { TokenType.Undefined      , @"\#undefined" },
            
            { TokenType.Malformed      , @".+$" },

            */
        };

        private static readonly string _productions = string.Join('|', _regexes.Select(FormatRule));
        private static readonly string _grammar = @"(" + _productions + ")";

        private static readonly Regex _masterRegex = new(_grammar);

        public static TokenStream Execute(string sourceName, string text)
        {
            return new TokenStream(ParseTokens(sourceName, text));
        }

        private static IEnumerable<Token> ParseTokens(string sourceName, string text)
        {
            IEnumerable<MatchCollection> matchesByLine = text
                .Split(Environment.NewLine)
                .Select(x => _masterRegex.Matches(x));

            int lineNumber = 1; // line numbers in text files start at one
            int columnNumber = 0;

            foreach (MatchCollection matchColl in matchesByLine)
            {
                foreach (Match match in matchColl)
                {
                    TokenType type = _regexes.Keys.First(x => match.Groups[x.ToString()].Success);

                    Group group = match.Groups[type.ToString()];

                    SourceRef source = new SourceRef(sourceName, text,
                        lineNumber, columnNumber, match.Index, match.Length);

                    if (type == TokenType.NewLine)
                    {
                        lineNumber++;
                        columnNumber = 0;
                    }
                    else
                    {
                        columnNumber += match.Length;
                    }

                    if (type != TokenType.Whitespace)
                    {
                        yield return new Token(type, source);
                    }
                }
            }
        }

        private static string FormatRule(KeyValuePair<TokenType, string> pair)
        {
            return $"(?<{pair.Key}>{pair.Value})";
        }

    }
}
