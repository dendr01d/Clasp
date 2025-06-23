using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.Tokens;
using ClaspCompiler.Textual;
using ClaspCompiler.CompilerData;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ParseSyntax
    {
        public static Prog_Stx Execute(TokenStream stream)
        {
            IEnumerator<Token> tokens = stream.GetEnumerator();
            tokens.MoveNext();

            ISyntax body = ParseOneOrMoreExpressions(tokens);

            return new Prog_Stx(new(), body);
        }

        private static ISyntax ParseOneOrMoreExpressions(IEnumerator<Token> tokens)
        {
            ISyntax parsed = ParseNextExpression(tokens);

            if (tokens.MoveNext())
            {
                ISyntax more = ParseOneOrMoreExpressions(tokens);

                parsed = new StxPair(parsed.Source)
                {
                    Car = parsed,
                    Cdr = more
                };
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

                TokenType.RightBrack => throw new Exception("Unexpected right bracket."),
                TokenType.LeftBrack => ParseList(tokens, true),

                TokenType.OpenVec => ParseVector(tokens, nextToken.Source),

                TokenType.Integer => ParseInteger(nextToken),
                TokenType.Symbol => ParseSymbol(nextToken),
                TokenType.True => ParseBoole(nextToken),
                TokenType.False => ParseBoole(nextToken),

                _ => throw new Exception($"Can't parse token of type {nextToken.Type}: {nextToken}")
            };
        }

        private static ISyntax ParseList(IEnumerator<Token> tokens, bool bracketed, SourceRef? listSrc = null)
        {
            if (tokens.Current.Type == TokenType.RightParen
                || tokens.Current.Type == TokenType.RightBrack)
            {
                if (tokens.Current.Type == TokenType.RightParen && bracketed)
                {
                    throw new Exception("Unexpected closing bracket in parenthetical list.");
                }

                if (tokens.Current.Type == TokenType.RightBrack && !bracketed)
                {
                    if (!bracketed) throw new Exception("Unexpected closing parenthesis in bracketed list.");
                }

                tokens.MoveNext(); //pop off closing paren/bracket
                return new StxDatum(tokens.Current.Source)
                {
                    Value = Nil.Instance
                };
            }
            else
            {
                ISyntax car = ParseNextExpression(tokens);
                ISyntax cdr = ParseList(tokens, bracketed);

                SourceRef mergedSrc = listSrc is null
                    ? car.Source.Merge(cdr.Source)
                    : listSrc.Merge(cdr.Source);

                return new StxPair(mergedSrc)
                {
                    Car = car,
                    Cdr = cdr
                };
            }
        }

        private static StxPair ParseVector(IEnumerator<Token> tokens, SourceRef listSource)
        {
            ISyntax contents = ParseList(tokens, false, listSource);

            if (contents.IsNil)
            {
                throw new Exception($"Vector form @ {listSource} has zero elements.");
            }
            else
            {
                return new StxPair(listSource)
                {
                    Car = new Identifier(Symbol.Intern(Keyword.VECTOR), null, listSource),
                    Cdr = contents
                };
            }
        }

        private static StxDatum ParseInteger(Token token)
        {
            return new StxDatum(new Integer(int.Parse(token.Text)), token.Source);
        }

        private static Identifier ParseSymbol(Token token)
        {
            return new Identifier(Symbol.Intern(token.Text), null, token.Source);
        }

        private static StxDatum ParseBoole(Token token)
        {
            return new StxDatum(token.Source)
            {
                Value = token.Type == TokenType.True ? Boole.True : Boole.False
            };
        }
    }
}
