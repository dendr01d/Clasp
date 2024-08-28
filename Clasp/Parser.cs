namespace Clasp
{
    internal static class Parser
    {
        public static IEnumerable<Expression> ParseText(string input)
        {
            return Parse(Lexer.Lex(input));
        }

        public static IEnumerable<Expression> ParseFile(string path)
        {
            return ParseText(File.ReadAllText(path));
        }

        public static IEnumerable<Stack<Token>> SegmentTokens(IEnumerable<Token> tokens)
        {
            using (var iter = tokens.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    if (iter.Current.TType != TokenType.LeftParen)
                    {
                        throw new ParsingException("Expected '(' at beginning of list.", iter.Current);
                    }

                    List<Token> segment = new List<Token>() { iter.Current };
                    int parenDepth = 1;

                    while (parenDepth > 0 && iter.MoveNext())
                    {
                        segment.Add(iter.Current);
                        if (iter.Current.TType == TokenType.LeftParen)
                        {
                            ++parenDepth;
                        }
                        else if (iter.Current.TType == TokenType.RightParen)
                        {
                            --parenDepth;
                        }
                    }

                    yield return new Stack<Token>(segment.AsEnumerable().Reverse());
                }
            }
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
                foreach(Stack<Token> segment in SegmentTokens(tokens))
                {
                    yield return ParseTokens(segment);
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
                    TokenType.VecParen => ParseVector(tokens),
                    TokenType.LeftParen => ParseList(tokens),
                    TokenType.RightParen => throw new ParsingException("Unexpected ')'", current),

                    TokenType.DotMarker => throw new ParsingException("Unexpected '.'", current),

                    TokenType.QuoteMarker => Pair.Cons(Symbol.Quote, ParseTokens(tokens)),
                    TokenType.QuasiquoteMarker => Pair.Cons(Symbol.Quasiquote, ParseTokens(tokens)),
                    TokenType.UnquoteMarker => Pair.Cons(Symbol.Unquote, ParseTokens(tokens)),
                    TokenType.UnquoteSplicingMarker => Pair.Cons(Symbol.UnquoteSplicing, ParseTokens(tokens)),
                    TokenType.Ellipsis => Symbol.Ellipsis,

                    TokenType.Symbol => Symbol.Ize(current.Text),
                    TokenType.Number => new SimpleNum(decimal.Parse(current.Text)),
                    TokenType.Boolean => Boolean.Judge(current.Text == Boolean.True.ToString()),

                    TokenType.Error => Expression.Error,

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
            bool specialTerminator = false;

            while (tokens.Peek().TType != TokenType.RightParen
                && tokens.Peek().TType != TokenType.DotMarker)
            {
                exprs.Add(ParseTokens(tokens));
            }

            if (tokens.Peek().TType != TokenType.RightParen)
            {
                specialTerminator = true;

                if (tokens.Peek().TType == TokenType.DotMarker)
                {
                    tokens.Pop(); //remove dot marker
                    exprs.Add(ParseTokens(tokens)); //grab the rest
                }

                if (tokens.Peek().TType != TokenType.RightParen)
                {
                    throw new ParsingException("Expected ')' after list-terminating structure", tokens.Peek());
                }
            }

            tokens.Pop(); //remove right paren

            return specialTerminator
                ? Pair.MakeImproperList(exprs.ToArray())
                : Pair.MakeList(exprs.ToArray());
        }

        private static Expression ParseVector(Stack<Token> tokens)
        {
            List<Expression> newList = new();

            while (tokens.Peek().TType != TokenType.RightParen)
            {
                newList.Add(ParseTokens(tokens));
            }

            tokens.Pop(); //remove right paren

            return Vector.MkVector(newList.ToArray());
        }

    }
}
