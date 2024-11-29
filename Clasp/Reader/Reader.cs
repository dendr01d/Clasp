
using Clasp.AST;
using Clasp.Lexer;
using Clasp.Primitives;

namespace Clasp.Reader
{
    internal static class Reader
    {
        /// <summary>
        /// Reads the given tokens into the syntactic representation of a program.
        /// The given sequence is assumed not to be empty.
        /// </summary>
        public static Syntax Read(IEnumerable<Token> tokens)
        {
            // First, do a quick check to make sure the parentheses all match up
            CheckParentheses(tokens);

            IEnumerator<Token> iter = tokens.GetEnumerator();

            if (iter.Any())
            {
                return ReadSyntax(iter);
            }
            else
            {
                throw new ReaderException("Token stream is empty.");
            }

        }

        #region Parentheses-Checking
        private static void CheckParentheses(IEnumerable<Token> tokens)
        {
            int parenCheck = CountOpenParens(tokens).CompareTo(CountCloseParens(tokens));

            if (parenCheck != 0)
            {
                bool extraCloseParens = parenCheck > 0;

                Token? nearestToken = LocateExtraParen(extraCloseParens, tokens);
                if (nearestToken is null)
                {
                    throw new ReaderException(string.Format(
                        "The reader found an extra {0} parenthesis, but was unable to determine where it is.",
                        extraCloseParens ? "closing" : "opening"));
                }
                else
                {
                    throw new ReaderException(string.Format(
                        "The reader found an {0} parenthesis {1} the token {2} on line {3}, index {4}.",
                        extraCloseParens ? "un-opened" : "un-closed",
                        extraCloseParens ? "before" : "after",
                        nearestToken,
                        nearestToken.SourceLine,
                        nearestToken.SourceIndex));
                }
            }
        }
        private static int CountOpenParens(IEnumerable<Token> tokens)
        {
            return tokens.Where(x => x.TType == TokenType.OpenListParen || x.TType == TokenType.OpenVecParen).Count();
        }
        private static int CountCloseParens(IEnumerable<Token> tokens)
        {
            return tokens.Where(x => x.TType == TokenType.ClosingParen).Count();
        }
        private static Token? LocateExtraParen(bool extraCloseParen, IEnumerable<Token> input)
        {
            IEnumerable<Token> tokenStream = (extraCloseParen ? input : input.Reverse());
            int parenInc = extraCloseParen ? 1 : -1;

            int parenCounter = 0;

            foreach (Token token in tokenStream)
            {
                if (parenCounter < 0)
                {
                    return token;
                }
                else if (token.TType == TokenType.OpenListParen
                    || token.TType == TokenType.OpenVecParen)
                {
                    parenCounter += parenInc;
                }
                else if (token.TType == TokenType.ClosingParen)
                {
                    parenCounter -= parenInc;
                }
            }

            return null;
        }
        #endregion

        private static Syntax ReadSyntax(IEnumerator<Token> tokens)
        {
            // Use this later to extract metadata about the syntax
            Token current = tokens.Pop();

            // The reader must produce a syntax object.
            // As a syntax object may only encapsulate a list, a symbol, or some other atom...
            // Then all valid syntax must itself belong to one of these categories

            Fixed nextValue = current.TType switch
            {
                TokenType.ClosingParen => throw ReaderException.UnexpectedToken(current),
                TokenType.DotOperator => throw ReaderException.UnexpectedToken(current),

                TokenType.OpenListParen => ReadList(tokens),
                TokenType.OpenVecParen => ReadVector(tokens),

                TokenType.Quote => new List(AST.Symbol.Quote, ReadSyntax(tokens)),
                TokenType.Quasiquote => new List(AST.Symbol.Quasiquote, ReadSyntax(tokens)),
                TokenType.Unquote => new List(AST.Symbol.Unquote, ReadSyntax(tokens)),
                TokenType.UnquoteSplice => new List(AST.Symbol.UnquoteSplicing, ReadSyntax(tokens)),

                TokenType.Syntax => new List(AST.Symbol.Syntax, ReadSyntax(tokens)),
                TokenType.QuasiSyntax => new List(AST.Symbol.Quasisyntax, ReadSyntax(tokens)),
                TokenType.Unsyntax => new List(AST.Symbol.Unsyntax, ReadSyntax(tokens)),
                TokenType.UnsyntaxSplice => new List(AST.Symbol.UnsyntaxSplicing, ReadSyntax(tokens)),

                TokenType.Symbol => AST.Symbol.Intern(current.Text),
                TokenType.Character => AST.Character.Intern(current),
                TokenType.String => new AST.CharString(current.Text),
                TokenType.Boolean => current.Text == AST.Boolean.True.ToString() ? AST.Boolean.True : AST.Boolean.False,
                TokenType.DecInteger => new AST.Integer(long.Parse(current.Text)),
                TokenType.DecReal => new AST.Real(double.Parse(current.Text)),

                TokenType.Malformed => throw ReaderException.UnexpectedToken(current),

                _ => throw ReaderException.UnhandledToken(current)
            };

            Syntax wrappedValue = Syntax.Wrap(nextValue, current.SourceLine, current.SourceIndex);

            return wrappedValue;
        }

        // See here https://docs.racket-lang.org/reference/reader.html#%28part._parse-pair%29
        // For a peculiarity in how lists are read in the case of dotted terminators
        // For now though I'm just explicitly tracking dotted status with a bool

        private static ConsCell ReadList(IEnumerator<Token> tokens)
        {
            Tuple<Syntax[], bool> elements = ReadSeries(tokens);
            
            if (elements.Item2) // improper list
            {
                return new AST.Pair(elements.Item1[0], elements.Item1[1], elements.Item1[2..]);
            }
            else
            {
                return new AST.List(elements.Item1[0], elements.Item1[1..])
            }
        }

        private static AST.Vector ReadVector(IEnumerator<Token> tokens)
        {
            Token vecBegin = tokens.Peek();
            Tuple<Syntax[], bool> elements = ReadSeries(tokens);

            if (elements.Item2)
            {
                throw new ReaderException(string.Format(
                    "Unexpected {0} token in vector starting on line {2}, index {3}.",
                    TokenType.DotOperator,
                    vecBegin.SourceLine,
                    vecBegin.SourceIndex));
            }
            else
            {
                return new AST.Vector(elements.Item1);
            }
        }

        /// <summary>
        /// Consumes and reads tokens until a closing paren is encountered.
        /// Returns a tuple with the contents and a bool indicating whether the series was dotted at the end.
        /// </summary>
        private static Tuple<Syntax[], bool> ReadSeries(IEnumerator<Token> tokens)
        {
            if (tokens.Peek().TType == TokenType.DotOperator)
            {
                throw ReaderException.UnexpectedToken(tokens.Peek());
            }

            List<Syntax> output = new List<Syntax>();
            Token previous = tokens.Peek();
            bool dotted = false;

            while (tokens.Peek().TType != TokenType.ClosingParen
                && tokens.Peek().TType != TokenType.DotOperator)
            {
                previous = tokens.Peek();
                output.Add(ReadSyntax(tokens));
            }

            if (tokens.Peek().TType == TokenType.DotOperator)
            {
                previous = tokens.Peek();
                dotted = true;
                tokens.Pop(); //remove dot
                output.Add(ReadSyntax(tokens)); //add the rest
            }

            if (tokens.Peek().TType != TokenType.ClosingParen)
            {
                throw ReaderException.ExpectedToken(TokenType.ClosingParen, previous);
            }

            tokens.Pop(); //remove closing paren

            return new Tuple<Syntax[], bool>(output.ToArray(), dotted);
        }

    }
}
