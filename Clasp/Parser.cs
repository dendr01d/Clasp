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
                    TokenType.QuoteMarker => Pair.List(Symbol.New("quote"), ParseStack(ref tokens)),
                    TokenType.Symbol => Symbol.New(current.Text),
                    TokenType.Number => new Number(double.Parse(current.Text)),
                    TokenType.Boolean => Boolean.Not(current.Text == "#f"),
                    TokenType.Error => Expression.Error,
                    _ => throw new Exception($"Parsing error: unknown token type {current.TType}")
                };
            }
        }

        private static Expression ParseList(ref Stack<Token> tokens)
        {

            if (tokens.Peek().TType == TokenType.DotMarker)
            {
                throw new Exception("Parsing error: expected car expression of dotted pair");
            }

            Stack<Expression> newList = new();
            bool proper = true;

            while (tokens.Peek().TType != TokenType.RightParen && tokens.Peek().TType != TokenType.DotMarker)
            {
                newList.Push(ParseStack(ref tokens));
            }

            if (tokens.Peek().TType == TokenType.DotMarker)
            {
                proper = false;
                tokens.Pop(); //remove dot marker
                //newList.Push(Pair.Cons(newList.Pop(), ParseStack(ref tokens))); //combine last two items into pair
                newList.Push(ParseStack(ref tokens)); //grab the rest

                if (tokens.Peek().TType != TokenType.RightParen)
                {
                    throw new Exception("Parsing error: expected ')' after dotted pair");
                }
            }

            tokens.Pop(); //remove right paren

            Expression[] exprs = newList.Reverse().ToArray();

            return proper
                ? Pair.List(exprs)
                : Pair.ListStar(exprs);

            //if (exprs.Length > 0 && exprs[0] is Symbol sym && SpecialForm.IsSpecialKeyword(sym))
            //{
            //    return SpecialForm.CreateForm(sym, exprs[1..]);
            //}
            //else
            //{
            //    return Pair.ConstructLinked(exprs);
            //}
        }

    }
}
