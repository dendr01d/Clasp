using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Clasp.Data.Text;

using static Clasp.LexerException;

namespace Clasp.Process
{
    internal static class Lexer
    {
        //Derived from the lexical rules described at https://groups.csail.mit.edu/mac/ftpdir/scheme-reports/r5rs-html/r5rs_9.html#SEC73

        // Attempt to parse tokens using these regexes, in this order
        private static readonly string[] _regexes = new string[]
        {
            BuildRgx(TokenType.Whitespace     , @"\s+"),
            BuildRgx(TokenType.Comment        , @"(?>\;.*$)"),

            BuildRgx(TokenType.Symbol         , @"(?>([a-zA-Z\!\$\%\&\*\/\:\<\=\>\?\^_\~][a-zA-Z0-9\!\$\%\&\*\/\:\<\=\>\?\^_\~\+\-\.\@]*)|\+|\-|\.\.\.)"),
            BuildRgx(TokenType.Boolean        , @"(?>(#(?:[Tt][Rr][Uu][Ee]|[Ff][Aa][Ll][Ss][Ee]|[Tt]|[Ff])))"),

            BuildRgx(TokenType.BinInteger     , @"(?>(?:#[Bb])(?:\+)?(\-?[01]+))"),
            BuildRgx(TokenType.OctInteger     , @"(?>(?:#[Oo])(?:\+)?(\-?[0-7]+))"),
            BuildRgx(TokenType.HexInteger     , @"(?>(?:#[Xx])(?:\+)?(\-?[0-9a-fA-F]+))"),
            BuildRgx(TokenType.DecInteger     , @"(?>(?:#[Dd])?(?:\+)?(\-?[0-9]+))"), //radix flag is optional for decimals
            BuildRgx(TokenType.DecReal        , @"(?>(?:#[Dd])?(?:\+)?(\-?[0-9]+\.[0-9]+))"), //ditto

            BuildRgx(TokenType.Character      , @"(?>#\\(?:(?:[^\s\(\)\-]+)|.))"),
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

            BuildRgx(TokenType.Malformed      , @".+$"),
        };

        // Combines the enum name with the corresponding regex so we can tell which one actually matched
        private static string BuildRgx(TokenType tt, string pattern) => $"(?<{tt}>{pattern})";

        private static readonly string _grammar = $"(?>{string.Join('|', _regexes)})";

        /// <summary>
        /// Read the text of the specified file line-by-line, and parse it into a sequence of tokens.
        /// </summary>
        public static IEnumerable<Token> LexFile(string path)
        {
            IEnumerable<string> sourceLines = File.ReadAllLines(path);
            return LexLines(path, sourceLines);
        }

        /// <summary>
        /// Split input text into a sequence of tokens using the Lexer's grammar rules. The text is
        /// first split into lines to accurately track the position of each token.
        /// </summary>
        public static IEnumerable<Token> LexText(string sourceName, string text)
        {
            return LexLines(sourceName, text.Split(Environment.NewLine));
        }

        /// <summary>
        /// Parse a sequence of tokens out of the aggregate text formed from the provided lines of input.
        /// The line divisions are used to record the relative position of each token.
        /// </summary>
        /// <exception cref="LexerException"></exception>
        public static IEnumerable<Token> LexLines(string sourceName, IEnumerable<string> inputLines)
        {
            if (!inputLines.Any())
            {
                return Array.Empty<Token>();
            }

            Blob sourceText = new Blob(sourceName, inputLines);
            List<Token> output = new List<Token>();
            List<LexerException> malformedInputs = new List<LexerException>();

            int lineNo = 1; //line numbers in source text are 1-indexed
            int charNo = 1; //ditto with character index

            foreach (string line in inputLines)
            {
                try
                {
                    IEnumerable<Token> lineOutput = LexSingleLine(sourceText, line, lineNo, charNo);
                }
                catch(AggregateException aggEx)
                {
                    foreach(LexerException inner in aggEx.InnerExceptions.OfType<LexerException>())
                    {
                        malformedInputs.Add(inner);
                    }
                }

                charNo += line.Length;
                ++lineNo;
            }

            if (malformedInputs.Any())
            {
                throw new AggregateException("Malformed lexemes found in input.", malformedInputs);
            }

            return output;
        }

        public static IEnumerable<Token> LexSingleLine(Blob sourceText, string text, int lineNo, int charNo)
        {
            List<Token> output = new List<Token>();
            List<LexerException> malformedInputs = new List<LexerException>();

            if (!string.IsNullOrWhiteSpace(text))
            {
                var lineMatches = Regex.Matches(text, _grammar);

                foreach (Match match in lineMatches)
                {
                    SourceCode locale = new SourceCode(
                        sourceText.Source, lineNo, match.Index, charNo + match.Index, match.Length, sourceText);

                    TokenType tokType = ExtractMatchedTokenType(match);

                    Token newToken = Token.Tokenize(tokType, match.Value, sourceText, locale);

                    if (newToken.TType == TokenType.Comment || newToken.TType == TokenType.Whitespace)
                    {
                        continue; //maybe I'll do something with this later?
                    }
                    else if (newToken.TType == TokenType.Malformed)
                    {
                        malformedInputs.Add(new LexerException.MalformedInput(newToken));
                    }
                    else
                    {
                        output.Add(newToken);
                    }
                }
            }

            if (malformedInputs.Any())
            {
                throw new AggregateException("Malformed lexemes found in input.", malformedInputs);
            }

            return output;
        }

        private static TokenType ExtractMatchedTokenType(Match regexMatch)
        {
            // find the name of the first regex (in the _grammar) that successfully matched
            string name = regexMatch.Groups.Values
                .Skip(regexMatch.Groups.Count - _regexes.Length)
                .First(x => x.Success).Name;
            return Enum.TryParse(name, out TokenType matchedType)
                ? matchedType
                : TokenType.Malformed;
        }
    }
}
