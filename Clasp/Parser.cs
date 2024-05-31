namespace Clasp
{
    internal static class Parser
    {
        public static Expression Parse(IEnumerable<Token> tokens)
        {
            Stack<Token> stack = new(tokens.Reverse());
            if (tokens.Any())
            {
                return ParseStack(stack);
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


        private static Expression ParseStack(Stack<Token> tokens)
        {
            if (tokens.Count <= 0)
            {
                throw new ParsingException("Unexpected end of token stream");
            }
            else
            {
                Token current = tokens.Pop();

                return current.TType switch
                {
                    TokenType.LeftParen => ParseList(tokens),
                    TokenType.RightParen => throw new ParsingException("Unexpected ')'"),

                    TokenType.DotMarker => throw new ParsingException("Unexpected '.'"),

                    TokenType.QuoteMarker => Pair.List(Symbol.Quote, ParseStack(tokens)),

                    TokenType.QuasiquoteMarker => Pair.List(Symbol.Quasiquote, ParseStack(tokens)),
                    TokenType.UnquoteMarker => Pair.List(Symbol.Unquote, ParseStack(tokens)),
                    TokenType.UnquoteSplicingMarker => Pair.List(Symbol.UnquoteSplicing, ParseStack(tokens)),
                    TokenType.Ellipsis => Pair.List(Symbol.Ellipsis, ParseStack(tokens)),

                    TokenType.Symbol => Symbol.Ize(current.Text),
                    TokenType.Number => new Number(double.Parse(current.Text)),
                    TokenType.Boolean => Boolean.Judge(current.Text == Boolean.True.ToString()),

                    TokenType.Error => Expression.Error,

                    _ => throw new ParsingException($"Unknown token type {current.TType}")
                };
            }
        }

        private static Expression ParseList(Stack<Token> tokens)
        {

            if (tokens.Peek().TType == TokenType.DotMarker)
            {
                throw new ParsingException("Expected Car arg of dotted pair");
            }

            Stack<Expression> newList = new();
            bool proper = true;

            while (tokens.Peek().TType != TokenType.RightParen && tokens.Peek().TType != TokenType.DotMarker)
            {
                newList.Push(ParseStack(tokens));
            }

            if (tokens.Peek().TType == TokenType.DotMarker)
            {
                proper = false;
                tokens.Pop(); //remove dot marker
                //newList.Push(Pair.Cons(newList.Pop(), ParseStack(ref tokens))); //combine last two items into pair
                newList.Push(ParseStack(tokens)); //grab the rest

                if (tokens.Peek().TType != TokenType.RightParen)
                {
                    throw new ParsingException("Expected ')' following dotted pair");
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
