using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.Tokens;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ParseSyntax
    {
        public static ProgS1 Execute(TokenStream stream)
        {
            IEnumerator<Token> tokens = stream.GetEnumerator();
            tokens.MoveNext();

            ISyntax body = ParseOneOrMoreExpressions(tokens);

            return new ProgS1(body);
        }

        private static ISyntax ParseOneOrMoreExpressions(IEnumerator<Token> tokens)
        {
            ISyntax parsed = ParseNextExpression(tokens);

            if (tokens.MoveNext())
            {
                ISyntax more = ParseOneOrMoreExpressions(tokens);
                parsed = new StxPair(parsed, more);
            }

            return parsed;
        }

        private static ISyntax ParseNextExpression(IEnumerator<Token> tokens)
        {
            Token nextToken = tokens.Current;
            tokens.MoveNext();

            return nextToken.Type switch
            {
                TokenType.RightParen => throw new Exception("Unexpected right parenthesis."),
                TokenType.LeftParen => ParseList(tokens, false),
                TokenType.RightBracket => throw new Exception("Unexpected right bracket."),
                TokenType.LeftBracket => ParseList(tokens, true),
                TokenType.Integer => ParseInteger(nextToken),
                TokenType.Symbol => ParseSymbol(nextToken),
                _ => throw new Exception($"Can't parse token of type {nextToken.Type}: {nextToken}")
            };
        }

        private static ISyntax ParseList(IEnumerator<Token> tokens, bool bracketed)
        {
            if (tokens.Current.Type == TokenType.RightParen)
            {
                if (bracketed) throw new Exception("Encountered closing bracket in paren list.");

                tokens.MoveNext(); //pop off right paren
                return new StxDatum(Nil.Instance);
            }
            else if (tokens.Current.Type == TokenType.RightBracket)
            {
                if (!bracketed) throw new Exception("Encountered closing paren in bracket list.");

                tokens.MoveNext(); // pop off right bracket
                return new StxDatum(Nil.Instance);
            }
            else
            {
                ISyntax car = ParseNextExpression(tokens);
                ISyntax cdr = ParseList(tokens, bracketed);
                return new StxPair(car, cdr);
            }
        }

        private static ISyntax ParseInteger(Token token)
        {
            return new StxDatum(new Integer(int.Parse(token.Source.GetSnippet())));
        }

        private static ISyntax ParseSymbol(Token token)
        {
            return new Identifier(new Symbol(token.Source.GetSnippet().ToString()));
        }
    }
}
