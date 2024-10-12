namespace Clasp
{
    internal static class Parser
    {
        public static IEnumerable<Expression> ParseText(string input)
        {
            return Parse(Lexer.LexLines(new string[] { input }));
        }

        public static IEnumerable<Expression> ParseFile(string path)
        {
            IEnumerable<string> sourceLines = File.ReadAllLines(path);
            return Parse(Lexer.LexLines(sourceLines));
        }

        public static IEnumerable<Expression> Parse(IEnumerable<Token> tokens)
        {
            if (!tokens.Any())
            {
                yield return Expression.Nil;
                yield break;
            }
            else
            {
                foreach(IEnumerable<Token> segment in Lexer.SegmentTokens(tokens))
                {
                    Stack<Token> stack = new Stack<Token>(segment.Reverse());
                    yield return ParseTokens(stack);
                }
            }
        }

        public static Expression ParseTokens(Stack<Token> tokens)
        {
            if (tokens.Count <= 0)
            {
                throw new ParsingException("Unexpected end of token stream", null);
            }
            else
            {
                Token current = tokens.Pop();

                return current.TType switch
                {
                    //TokenType.VecParen => ParseVector(tokens),
                    TokenType.LeftParen => ParseList(tokens),
                    TokenType.RightParen => throw new ParsingException("Unexpected ')'", current),
                    TokenType.DotMarker => throw new ParsingException("Unexpected '.'", current),

                    TokenType.QuoteMarker => Pair.List(Symbol.Quote, ParseTokens(tokens)),
                    TokenType.SyntaxMarker => Pair.List(Symbol.Syntax, ParseTokens(tokens)),
                    TokenType.QuasiquoteMarker => Pair.List(Symbol.Quasiquote, ParseTokens(tokens)),
                    TokenType.UnquoteMarker => Pair.List(Symbol.Unquote, ParseTokens(tokens)),
                    TokenType.UnquoteSplicingMarker => Pair.List(Symbol.UnquoteSplicing, ParseTokens(tokens)),
                    TokenType.Ellipsis => Symbol.Ellipsis,

                    TokenType.Symbol => Symbol.Ize(current.Text),
                    TokenType.Number => new SimpleNum(decimal.Parse(current.Text)),
                    TokenType.Boolean => (current.Text == Boolean.True.ToString()),
                    TokenType.Character => Character.FromToken(current),
                    TokenType.QuotedString => Charstring.FromToken(current),

                    TokenType.Error => new Error("Parsing error?"),

                    _ => throw new ParsingException($"Unknown token type {current.TType}", current)
                };
            }
        }

        private static Expression ParseList(Stack<Token> tokens)
        {
            if (tokens.Peek().TType == TokenType.DotMarker)
            {
                throw new ParsingException("Expected Car of dotted pair", tokens.Peek());
            }

            List<Expression> exprs = new List<Expression>();
            bool dottedPair = false;

            while (tokens.Peek().TType != TokenType.RightParen
                && tokens.Peek().TType != TokenType.DotMarker)
            {
                exprs.Add(ParseTokens(tokens));
            }

            if (tokens.Peek().TType == TokenType.DotMarker)
            {
                dottedPair = true;

                tokens.Pop(); //remove dot marker
                exprs.Add(ParseTokens(tokens)); //grab the rest

                if (tokens.Peek().TType != TokenType.RightParen)
                {
                    throw new ParsingException("Expected ')' after dotted pair.", tokens.Peek());
                }
            }

            tokens.Pop(); //remove right paren

            return dottedPair
                ? Pair.ListStar(exprs[0], exprs[1..].ToArray())
                : Pair.List(exprs.ToArray());
        }

        //private static Expression ParseVector(Stack<Token> tokens)
        //{
        //    List<Expression> newList = new();

        //    while (tokens.Peek().TType != TokenType.RightParen)
        //    {
        //        newList.Add(ParseTokens(tokens));
        //    }

        //    tokens.Pop(); //remove right paren

        //    return Vector.MkVector(newList.ToArray());
        //}

    }
}
