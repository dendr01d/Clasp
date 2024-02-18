namespace Clasp
{
    internal static class Parser
    {
        public static Expression Parse(IEnumerable<Token> tokens)
        {
            Stack<Token> stack = new(tokens.Reverse());
            if (tokens.Any())
            {
                return ParseStack(ref stack);
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

        private static Expression ParseStack(ref Stack<Token> tokens)
        {
            if (tokens.Count <= 0)
            {
                throw new Exception("Parsing error: unexpected EOF");
            }
            else
            {
                Token current = tokens.Pop();

                return current.TType switch
                {
                    TokenType.LeftParen => ParseList(ref tokens),
                    TokenType.RightParen => throw new Exception("Parsing error: unexpected ')'"),
                    TokenType.DotMarker => throw new Exception("Parsing error: unexpected '.'"),
                    TokenType.QuoteMarker => SpecialForm.CreateForm("quote", [ParseStack(ref tokens)]),
                    TokenType.Symbol => new Symbol(current.Text),
                    TokenType.Number => new Number(double.Parse(current.Text)),
                    _ => throw new Exception($"Parsing error: unknown token type {current.TType}")
                };
            }
        }

        private static Expression ParseList(ref Stack<Token> tokens)
        {
            Stack<Expression> newList = new();

            while (tokens.Peek().TType != TokenType.RightParen && tokens.Peek().TType != TokenType.DotMarker)
            {
                newList.Push(ParseStack(ref tokens));
            }

            if (tokens.Peek().TType == TokenType.DotMarker)
            {
                tokens.Pop(); //remove dot marker
                newList.Push(Pair.Cons(newList.Pop(), ParseStack(ref tokens))); //combine last two items into pair

                if (tokens.Peek().TType != TokenType.RightParen)
                {
                    throw new Exception("Parsing error: expected ')' after dotted pair");
                }
            }

            tokens.Pop(); //remove right paren

            Expression[] exprs = newList.Reverse().ToArray();

            if (exprs.Length > 0 && exprs[0] is Symbol sym && SpecialForm.IsSpecialKeyword(sym))
            {
                return SpecialForm.CreateForm(sym, exprs[1..]);
            }
            else
            {
                return SList.ConstructLinked(exprs);
            }
        }

    }
}
