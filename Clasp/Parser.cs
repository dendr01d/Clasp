namespace Clasp
{
    internal static class Parser
    {
        public static Expression Parse(IEnumerable<Token> tokens)
        {
            Stack<Token> stack = new(tokens.Reverse());
            if (tokens.Any())
            {
                return ParseStack(stack, false);
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

        private static Expression ParseStack(Stack<Token> tokens, bool quasi)
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
                    TokenType.LeftParen => ParseList(tokens, false),
                    TokenType.RightParen => throw new ParsingException("Unexpected ')' in token stream"),
                    TokenType.Dot => throw new ParsingException("Unexpected '.' in token stream"),
                    TokenType.Quote => ExpandForm(new SPQuote(), ParseStack(tokens, false)),
                    TokenType.QuasiQuote => ParseQuasi(tokens),
                    TokenType.UnQuote => throw new ParsingException("Unexpected ',' in token stream"),
                    TokenType.Symbol => new Symbol(current.Text),
                    TokenType.Number => new Number(double.Parse(current.Text)),
                    _ => throw new ParsingException($"Unknown token type {current.TType}")
                };
            }
        }

        private static Expression ExpandForm(SpecialForm form, Expression args)
        {
            return new Pair(form, new Pair(args, Expression.Nil));
        }

        private static Expression ParseQuasi(Stack<Token> tokens)
        {
            if (tokens.Pop().TType != TokenType.LeftParen)
            {
                throw new Exception("Expected '(' following quasi-quote");
            }
            return ExpandForm(new SPQuasiQuote(), ParseList(tokens, true));
        }

        private static Expression ParseList(Stack<Token> tokens, bool quasi)
        {
            Stack<Expression> newList = new();
            bool dotted = false;

            while (tokens.Peek().TType != TokenType.RightParen && tokens.Peek().TType != TokenType.Dot)
            {
                if (tokens.Peek().TType == TokenType.UnQuote)
                {
                    if (!quasi)
                    {
                        throw new Exception("Unexpected ',' in token stream");
                    }
                    else
                    {
                        tokens.Pop(); //remove comma
                        //because it's unquoted, we turn the quasi context off for it
                        newList.Push(ExpandForm(new SPUnQuote(), ParseStack(tokens, false)));
                    }
                }
                else
                {
                    newList.Push(ParseStack(tokens, quasi));
                }
            }

            if (tokens.Peek().TType == TokenType.Dot)
            {
                dotted = true;
                tokens.Pop(); //remove dot marker
                newList.Push(ParseStack(tokens, quasi));

                if (tokens.Peek().TType != TokenType.RightParen)
                {
                    throw new ParsingException("Expected ')' following dotted pair");
                }
            }

            tokens.Pop(); //remove right paren

            IEnumerable<Expression> exprs = newList.Reverse();

            return dotted
                ? SList.Improper(exprs)
                : SList.Proper(exprs);

            //if (exprs.Length > 0 && exprs[0] is Symbol sym && SpecialForm.IsSpecialKeyword(sym))
            //{
            //    return SpecialForm.CreateForm(sym, SList.ConstructLinked(exprs[1..]));
            //}
            //else if (exprs.Length == 1 && exprs[0] is SList l && l.IsDotted)
            //{
            //    //this stack-method of building lists implicity assumes we're building a linked list
            //    //if it's ONLY a dotted pair though then it's just a single cons cell with no links
            //    return exprs[0];
            //}
            //else
            //{
            //    return SList.ConstructLinked(exprs);
            //}
        }

    }
}
