using ClaspCompiler.Data;
using ClaspCompiler.Syntax;
using ClaspCompiler.Tokens;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ParseSyntax
    {
        public static ISyntax Execute(TokenStream stream)
        {
            IEnumerator<Token> tokens = stream.GetEnumerator();
            tokens.MoveNext();

            return ParseOneOrMoreExpressions(tokens);
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
                TokenType.LeftParen => ParseList(tokens),
                TokenType.Integer => ParseInteger(nextToken),
                TokenType.Symbol => ParseSymbol(nextToken),
                _ => throw new Exception($"Can't parse token of type {nextToken.Type}: {nextToken}")
            };
        }

        private static ISyntax ParseList(IEnumerator<Token> tokens)
        {
            if (tokens.Current.Type == TokenType.RightParen)
            {
                tokens.MoveNext(); //pop off right paren
                return new StxDatum(Nil.Instance);
            }
            else
            {
                ISyntax car = ParseNextExpression(tokens);
                ISyntax cdr = ParseList(tokens);
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
