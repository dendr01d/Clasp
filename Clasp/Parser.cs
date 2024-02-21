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
                throw new ParsingException("Unexpected end of token stream");
            }
            else
            {
                Token current = tokens.Pop();

                return current.TType switch
                {
                    TokenType.LeftParen => ParseList(ref tokens),
                    TokenType.RightParen => throw new ParsingException("Unexpected ')' in token stream"),
                    TokenType.DotMarker => throw new ParsingException("Unexpected '.' in token stream"),
                    TokenType.QuoteMarker => SpecialForm.CreateForm(new("quote"), SList.ConstructLinked(ParseStack(ref tokens))), //ensure lists aren't merged
                    TokenType.Symbol => new Symbol(current.Text),
                    TokenType.Number => new Number(double.Parse(current.Text)),
                    _ => throw new ParsingException($"Unknown token type {current.TType}")
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
                newList.Push(Pair.Cons(newList.Pop(), ParseStack(ref tokens)));

                if (tokens.Peek().TType != TokenType.RightParen)
                {
                    throw new ParsingException("Expected ')' following dotted pair");
                }
            }

            tokens.Pop(); //remove right paren

            Expression[] exprs = newList.Reverse().ToArray();

            if (exprs.Length > 0 && exprs[0] is Symbol sym && SpecialForm.IsSpecialKeyword(sym))
            {
                return SpecialForm.CreateForm(sym, SList.ConstructLinked(exprs[1..]));
            }
            else if (exprs.Length == 1 && exprs[0] is SList l && l.IsDotted)
            {
                //this stack-method of building lists implicity assumes we're building a linked list
                //if it's ONLY a dotted pair though then it's just a single cons cell with no links
                return exprs[0];
            }
            else
            {
                return SList.ConstructLinked(exprs);
            }
        }

    }
}
