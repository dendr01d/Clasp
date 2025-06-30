using System.Text.RegularExpressions;

using ClaspCompiler.Text;
using ClaspCompiler.Tokens;

namespace ClaspCompiler.CompilerPasses
{
    internal static class Tokenize
    {

        private const string PECULIAR_SYM = @"\+|\-|\.\.\.";

        private const string LETTER = @"[a-zA-Z]";
        private const string SPECIAL_INITIAL = @"\!|\$|\%|\&|\*|\/|\:|\<|\=|\>|\?|\^|_|\~";
        private const string INITIAL = $"{LETTER}|{SPECIAL_INITIAL}";

        private const string DIGIT = "[0-9]";
        private const string SPECIAL_SUBSEQUENT = @"\+|\-|\.|\@";

        private const string SUBSEQUENT = $"{INITIAL}|{DIGIT}|{SPECIAL_SUBSEQUENT}";

        private const string SYMBOL = $"(?:((?:{INITIAL})(?:{SUBSEQUENT})*)|{PECULIAR_SYM})";


        private static readonly Dictionary<TokenType, string> _regexes = new()
        {
            { TokenType.NewLine     , @"$" },
            { TokenType.Whitespace  , @"\s+" },
            { TokenType.Comment     , @"(?>\;.*$)" },

            { TokenType.LeftParen   , @"\(" },
            { TokenType.RightParen  , @"\)" },

            { TokenType.LeftBrack   , @"\[" },
            { TokenType.RightBrack  , @"\]" },

            { TokenType.OpenVec     , @"#\(" },

            { TokenType.DotOp    , @"\." },

            { TokenType.True        , @"#[Tt](?:[Rr][Uu][Ee])?" },
            { TokenType.False       , @"#[Ff](?:[Aa][Ll][Ss][Ee])?" },

            { TokenType.Quote            , @"\'" },
            { TokenType.Quasiquote       , @"\`" },
            { TokenType.Unquote          , @"\," },
            { TokenType.UnquoteSplice  , @"\@\," },

            { TokenType.Syntax         , @"\#\'" },
            { TokenType.Quasisyntax    , @"\#\`" },
            { TokenType.Unsyntax       , @"\#\," },
            { TokenType.UnsyntaxSplice , @"\#\@\," },

            { TokenType.Integer     , @"-?(0|[1-9]\d*)" },
            { TokenType.Symbol      , SYMBOL },

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

            Stack<Token> parenCounter = new();

            foreach (Match match in matches)
            {
                TokenType type = _regexes.Keys.First(x => match.Groups[x.ToString()].Success);
                Group group = match.Groups[type.ToString()];

                SourceRef src = new(new ReadFromFile(sourceName), line, col, match.Index, match.Length);

                if (type == TokenType.Malformed)
                {
                    throw new Exception($"Malformed token @ {src}: {match.Value}");
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

                if (type == TokenType.Whitespace || type == TokenType.Comment)
                {
                    continue;
                }

                Token nextToken = new Token(type, src, match.Value);

                if (type == TokenType.LeftParen
                    || type == TokenType.LeftBrack
                    || type == TokenType.OpenVec)
                {
                    parenCounter.Push(nextToken);
                }
                else if (type == TokenType.RightParen)
                {
                    if (parenCounter.Count == 0)
                    {
                        throw new Exception($"Extra {type} token @ {src}.");
                    }
                    else if (parenCounter.Peek().Type != TokenType.LeftParen
                        && parenCounter.Peek().Type != TokenType.OpenVec)
                    {
                        throw new Exception($"Parenthesis/Bracket mismatch @ {parenCounter.Peek().Source} vs {src}.");
                    }
                    else
                    {
                        parenCounter.Pop();
                    }
                }
                else if (type == TokenType.RightBrack)
                {
                    if (parenCounter.Count == 0)
                    {
                        throw new Exception($"Extra {type} token @ {src}.");
                    }
                    else if (parenCounter.Peek().Type != TokenType.LeftBrack)
                    {
                        throw new Exception($"Parenthesis/Bracket mismatch @ {parenCounter.Peek().Source} vs {src}.");
                    }
                    else
                    {
                        parenCounter.Pop();
                    }
                }

                yield return nextToken;
            }

            if (parenCounter.Count != 0)
            {
                throw new AggregateException(parenCounter.Select(x => new Exception($"Extra {x.Type} token @ {x.Source}.")));
            }

            yield return new Token(TokenType.EoF, new(new ReadFromFile(sourceName), line, col, text.Length, 0), "■");
        }

        private static string FormatRule(KeyValuePair<TokenType, string> pair)
        {
            return $"(?<{pair.Key}>{pair.Value})";
        }

    }
}
