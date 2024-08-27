namespace Clasp
{
    internal static class Parser
    {
        public static Expression Parse(IEnumerable<Token> tokens)
        {
            Stack<Token> stack = new(tokens.Reverse());
            if (tokens.Any())
            {
                return ParseTokens(stack);
            }
            else
            {
                return Expression.Nil;
            }
        }

        public static Expression Parse(string input)
        {
            return Parse(Lexer.Lex(input));
        }

        public static Expression ParseFile(string path)
        {
            string raw = File.ReadAllText(path);
            return Parse($"({raw})");
        }

        private static Expression ParseTokens(Stack<Token> tokens)
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

                    //TokenType.QuoteMarker => Pair.List(Symbol.Quote, ParseTokens(tokens)),
                    //TokenType.QuasiquoteMarker => Pair.List(Symbol.Quasiquote, ParseTokens(tokens)),
                    //TokenType.UnquoteMarker => Pair.List(Symbol.Unquote, ParseTokens(tokens)),
                    //TokenType.UnquoteSplicingMarker => Pair.List(Symbol.UnquoteSplicing, ParseTokens(tokens)),
                    TokenType.QuoteMarker => new Quoted(ParseTokens(tokens)),
                    TokenType.QuasiquoteMarker => new Quasiquoted(ParseTokens(tokens)),
                    TokenType.UnquoteMarker => new Unquoted(ParseTokens(tokens)),
                    TokenType.UnquoteSplicingMarker => new UnquoteSpliced(ParseTokens(tokens)),
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
                && tokens.Peek().TType != TokenType.DotMarker
                && tokens.Peek().TType != TokenType.Ellipsis)
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
                else //it must be an ellipsis
                {
                    tokens.Pop();
                    //convert the last item in the list to an elliptic pattern
                    exprs[exprs.Count - 1] = new EllipticPattern(exprs[exprs.Count - 1]);
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
