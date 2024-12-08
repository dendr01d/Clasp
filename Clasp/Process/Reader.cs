using Clasp.Data.AbstractSyntax;
using Clasp.Data.Text;
using Clasp.Data.Terms;

namespace Clasp.Process
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

            if (tokens.Any())
            {
                return ReadSyntax(new Stack<Token>(tokens.Reverse()));
            }
            else
            {
                throw new ReaderException("Token stream is empty and cannot be read.");
            }

        }

        #region Parentheses-Checking
        private static void CheckParentheses(IEnumerable<Token> tokens)
        {
            int parenCheck = CountOpenParens(tokens).CompareTo(CountCloseParens(tokens));

            if (parenCheck != 0)
            {
                bool extraCloseParens = parenCheck < 0;

                Token? extraParen = LocateExtraParen(extraCloseParens, tokens);

                if (extraParen is null)
                {
                    throw new ReaderException(string.Format(
                        "The reader counted an extra {0} parenthesis, but was unable to determine where it is.",
                        extraCloseParens ? "closing" : "opening"));
                }
                else
                {
                    throw new ReaderException.UnmatchedParenthesis(extraParen);
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
            IEnumerable<Token> tokenStream = extraCloseParen ? input : input.Reverse();
            int parenInc = extraCloseParen ? 1 : -1;

            int parenCounter = 0;

            foreach (Token token in tokenStream)
            {
                if (token.TType == TokenType.OpenListParen
                    || token.TType == TokenType.OpenVecParen)
                {
                    parenCounter += parenInc;
                }
                else if (token.TType == TokenType.ClosingParen)
                {
                    parenCounter -= parenInc;
                }

                if (parenCounter < 0)
                {
                    return token;
                }
            }

            return null;
        }
        #endregion

        private static Syntax ReadSyntax(Stack<Token> tokens)
        {
            // Use this later to extract metadata about the syntax
            Token current = tokens.Pop();

            // Ignore whitespace
            while (current.TType == TokenType.Whitespace)
            {
                current = tokens.Pop();
            }

            // The reader must produce a syntax object.
            // As a syntax object may only encapsulate a list, a symbol, or some other atom...
            // Then all valid syntax must itself belong to one of these categories

            Term nextValue = current.TType switch
            {
                TokenType.ClosingParen => throw new ReaderException.UnexpectedToken(current),
                TokenType.DotOperator => throw new ReaderException.UnexpectedToken(current),

                TokenType.OpenListParen => ReadList(tokens, current),
                TokenType.OpenVecParen => ReadVector(tokens, current),

                TokenType.Quote => ConsList.ProperList(Symbol.Quote, ReadSyntax(tokens)),
                TokenType.Quasiquote => ConsList.ProperList(Symbol.Quasiquote, ReadSyntax(tokens)),
                TokenType.Unquote => ConsList.ProperList(Symbol.Unquote, ReadSyntax(tokens)),
                TokenType.UnquoteSplice => ConsList.ProperList(Symbol.UnquoteSplicing, ReadSyntax(tokens)),

                TokenType.Syntax => ConsList.ProperList(Symbol.Syntax, ReadSyntax(tokens)),
                TokenType.QuasiSyntax => ConsList.ProperList(Symbol.Quasisyntax, ReadSyntax(tokens)),
                TokenType.Unsyntax => ConsList.ProperList(Symbol.Unsyntax, ReadSyntax(tokens)),
                TokenType.UnsyntaxSplice => ConsList.ProperList(Symbol.UnsyntaxSplicing, ReadSyntax(tokens)),

                TokenType.Symbol => Symbol.Intern(current.Text),
                TokenType.Character => Character.Intern(current),
                TokenType.String => new CharString(current.Text),
                TokenType.Boolean => current.Text == Data.Terms.Boolean.True.ToString()
                    ? Data.Terms.Boolean.True
                    : Data.Terms.Boolean.False,
                TokenType.DecInteger => new Integer(long.Parse(current.Text)),
                TokenType.DecReal => new Real(double.Parse(current.Text)),

                TokenType.Malformed => throw new ReaderException.UnexpectedToken(current),

                _ => throw new ReaderException.UnhandledToken(current)
            };

            Syntax wrappedValue = Syntax.Wrap(nextValue, current);

            return wrappedValue;
        }

        // See here https://docs.racket-lang.org/reference/reader.html#%28part._parse-pair%29
        // For a peculiarity in how lists are read in the case of dotted terminators
        // Not sure that I care to implement that here?

        private static Syntax ReadList(Stack<Token> tokens, Token source)
        {
            Tuple<Syntax[], bool> elements = ReadSeries(tokens);

            if (elements.Item1.Length == 0)
            {
                return Syntax.Wrap(Nil.Value, source);
            }
            else if (elements.Item2) // improper list
            {
                ConsList cl = ConsList.ConstructDirect(elements.Item1);
                return Syntax.Wrap(cl, source);
            }
            else
            {
                Term cl = ConsList.ProperList(elements.Item1);
                return Syntax.Wrap(cl, source);
            }
        }

        private static Syntax ReadVector(Stack<Token> tokens, Token source)
        {
            Token vecBegin = tokens.Peek();
            Tuple<Syntax[], bool> elements = ReadSeries(tokens);

            if (elements.Item2)
            {
                throw new ReaderException(
                    "Unexpected {0} token in vector beginning on line {2}, column {3}.",
                    TokenType.DotOperator,
                    vecBegin.Location.LineNumber,
                    vecBegin.Location.Column);
            }
            else
            {
                Vector vec = new Vector(elements.Item1);
                return Syntax.Wrap(vec, source);
            }
        }

        /// <summary>
        /// Consumes and reads tokens until a closing paren is encountered.
        /// Returns a tuple with the contents and a bool indicating whether the series was dotted at the end.
        /// </summary>
        private static Tuple<Syntax[], bool> ReadSeries(Stack<Token> tokens)
        {
            if (tokens.Peek().TType == TokenType.DotOperator)
            {
                throw new ReaderException.UnexpectedToken(tokens.Peek());
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
                throw new ReaderException.ExpectedToken(TokenType.ClosingParen, tokens.Peek(), previous);
            }

            tokens.Pop(); //remove closing paren

            return new Tuple<Syntax[], bool>(output.ToArray(), dotted);
        }

    }
}
