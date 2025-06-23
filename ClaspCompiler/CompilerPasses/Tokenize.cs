using System.Text.RegularExpressions;

using ClaspCompiler.Textual;
using ClaspCompiler.Tokens;

namespace ClaspCompiler.CompilerPasses
{
    internal static class Tokenize
    {
        private static readonly Dictionary<TokenType, string> _regexes = new()
        {
            { TokenType.NewLine     , @"$" },
            { TokenType.Whitespace  , @"\s+" },

            { TokenType.LeftParen   , @"\(" },
            { TokenType.RightParen  , @"\)" },

            { TokenType.LeftBrack , @"\[" },
            { TokenType.RightBrack, @"\]" },

            { TokenType.OpenVec   , @"#\(" },

            { TokenType.True, @"#[Tt](?:[Rr][Uu][Ee])?" },
            { TokenType.False, @"#[Ff](?:[Aa][Ll][Ss][Ee])?" },

            { TokenType.Integer   , @"-?(0|[1-9]\d*)" },
            //{ TokenType.Symbol    , @"(?:\+|\-|[a-zA-Z][a-zA-Z0-9\-]*)" },
            { TokenType.Symbol    , @"(?>([a-zA-Z\!\$\%\&\*\/\:\<\=\>\?\^_\~][a-zA-Z0-9\!\$\%\&\*\/\:\<\=\>\?\^_\~\+\-\.\@]*)|\+|\-|\.\.\.)" },

            { TokenType.Malformed   , @".*" }

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

        private static readonly string _grammar = @"(" + string.Join('|', _regexes.Select(FormatRule)) + ")";
        private static readonly Regex _masterRegex = new(_grammar);

        public static TokenStream Execute(string sourceName, string text)
        {
            return new TokenStream(ParseTokens(sourceName, text));
        }

        private static IEnumerable<Token> ParseTokens(string sourceName, string text)
        {
            MatchCollection matches = _masterRegex.Matches(text);

            int line = 1; // line numbers in text files start at one
            int col = 0;

            foreach (Match match in matches)
            {
                TokenType type = _regexes.Keys.First(x => match.Groups[x.ToString()].Success);
                Group group = match.Groups[type.ToString()];

                SourceRef source = new SourceRef(sourceName, line, col, match.Index, match.Length);

                if (type == TokenType.Malformed)
                {
                    throw new Exception($"Malformed token @ {source}: {match.Value}");
                }

                if (type == TokenType.NewLine)
                {
                    line++;
                    col = 0;
                    continue;
                }
                else
                {
                    col += match.Length;
                }

                if (type != TokenType.Whitespace)
                {
                    yield return new Token(type, source, match.Value);
                }
            }

            SourceRef final = new(sourceName, line, col, text.Length, 0);

            yield return new Token(TokenType.EoF, final, "■");
        }

        private static string FormatRule(KeyValuePair<TokenType, string> pair)
        {
            return $"(?<{pair.Key}>{pair.Value})";
        }

    }
}
