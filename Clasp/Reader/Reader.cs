
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
        public static Stx Read(IEnumerable<Token> tokens)
        {
            // First, do a quick check to make sure the parentheses all match up
            CheckParentheses(tokens);

            IEnumerator<Token> iter = tokens.GetEnumerator();

            if (iter.UseAsStack())
            {
                return ReadSyntax(iter);
            }
            else
            {
                throw new ReaderException("Token stream is empty.");
            }

        }

        #region Parentheses-Checking
        private static void CheckParentheses(IEnumerable<Lexer.Token> tokens)
        {
            int parenCheck = CountOpenParens(tokens).CompareTo(CountCloseParens(tokens));

            if (parenCheck != 0)
            {
                bool extraCloseParens = parenCheck > 0;

                Lexer.Token? nearestToken = LocateExtraParen(extraCloseParens, tokens);
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
        private static int CountOpenParens(IEnumerable<Lexer.Token> tokens)
        {
            return tokens.Where(x => x.TType == Lexer.TokenType.OpenListParen || x.TType == Lexer.TokenType.OpenVecParen).Count();
        }
        private static int CountCloseParens(IEnumerable<Lexer.Token> tokens)
        {
            return tokens.Where(x => x.TType == Lexer.TokenType.ClosingParen).Count();
        }
        private static Lexer.Token? LocateExtraParen(bool extraCloseParen, IEnumerable<Lexer.Token> input)
        {
            IEnumerable<Lexer.Token> tokenStream = (extraCloseParen ? input : input.Reverse());
            int parenInc = extraCloseParen ? 1 : -1;

            int parenCounter = 0;

            foreach(Lexer.Token token in tokenStream)
            {
                if (parenCounter < 0)
                {
                    return token;
                }
                else if (token.TType == Lexer.TokenType.OpenListParen
                    || token.TType == Lexer.TokenType.OpenVecParen)
                {
                    parenCounter += parenInc;
                }
                else if (token.TType == Lexer.TokenType.ClosingParen)
                {
                    parenCounter -= parenInc;
                }
            }

            return null;
        }
        #endregion

        private static Stx ReadSyntax(IEnumerator<Token> tokens)
        {

            Token current = tokens.Pop();

            // The reader must produce a syntax object.
            // As a syntax object may only encapsulate a list, a symbol, or some other atom...
            // Then all valid syntax must itself belong to one of these categories

            Val stxExpr = current.TType switch
            {
                TokenType.OpenListParen => new Stx(ReadList(tokens), new Ctx(), current.SourceLine, current.SourceIndex),
                TokenType.OpenVecParen => ReadVector(tokens),
                TokenType.Identifier => new Id(new Sym(current.Text), new Ctx()),
                
            }

            // A valid syntactic expression is either an atom or a list
        }

        private static List ReadShorthandOp(string op, IEnumerator<Token> tokens)
        {
            return new List([new Sym(op), ReadSyntax(tokens)]);
        }

        // See here https://docs.racket-lang.org/reference/reader.html#%28part._parse-pair%29
        // For a peculiarity in how lists are read in the case of dotted terminators
        // For now though I'm just explicitly tracking dotted status with a bool

        private static List ReadList(IEnumerator<Token> tokens)
        {
            Tuple<Stx[], bool> elements = ReadElements(tokens);
            return new List(elements.Item1, !elements.Item2);
        }

        private static List ReadVector(IEnumerator<Token> tokens)
        {
            Token vecBegin = tokens.Peek();
            Tuple<Stx[], bool> elements = ReadElements(tokens);

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
                return new List([new Prim(Primitive.VECTOR), .. elements.Item1]);
            }
        }

        private static Tuple<Stx[], bool> ReadElements(IEnumerator<Token> tokens)
        {
            if (tokens.Peek().TType == TokenType.DotOperator)
            {
                throw ReaderException.UnexpectedToken(tokens.Peek());
            }

            List<Stx> output = new List<Stx>();
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

            return new Tuple<Stx[], bool>(output.ToArray(), dotted);
        }

    }
}
