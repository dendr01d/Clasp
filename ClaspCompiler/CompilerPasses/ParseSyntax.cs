using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.Text;
using ClaspCompiler.Tokens;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ParseSyntax
    {
        public static Prog_Stx Execute(TokenStream stream)
        {
            IEnumerator<Token> tokens = stream.GetEnumerator();
            tokens.MoveNext();

            SymbolFactory symGen = new();

            StxPair body = ParseOneOrMoreExpressions(tokens, symGen);

            body = new StxPair(body)
            {
                Car = SpecialKeyword.Begin.Identifier,
                Cdr = body
            };

            return new Prog_Stx(body, symGen);

        }

        private static StxPair ParseOneOrMoreExpressions(IEnumerator<Token> tokens, SymbolFactory symGen)
        {
            StxPair output;
            ISyntax nextExpression = ParseNextExpression(tokens, symGen);

            if (tokens.Current.Type != TokenType.EoF)
            {
                output = new StxPair(nextExpression)
                {
                    Car = nextExpression,
                    Cdr = ParseOneOrMoreExpressions(tokens, symGen)
                };
            }
            else
            {
                output = new StxPair(nextExpression)
                {
                    Car = nextExpression,
                    Cdr = new StxDatum(Nil.Instance, nextExpression.Source)
                };
            }

            return output;
        }

        private static ISyntax ParseNextExpression(IEnumerator<Token> tokens, SymbolFactory symGen)
        {
            Token nextToken = tokens.Current;
            tokens.MoveNext();

            return nextToken.Type switch
            {
                TokenType.EoF => throw new Exception("Unexpectedly reached end of input."),

                TokenType.RightParen or TokenType.RightBrack => throw new Exception($"Unexpected {nextToken.Type} token @ {nextToken.Source}."),

                TokenType.LeftParen => ParseList(tokens, symGen, nextToken.Source, false),
                TokenType.LeftBrack => ParseList(tokens, symGen, nextToken.Source, true),

                //TokenType.OpenVec => ParseVector(tokens, symGen, nextToken.Source),

                TokenType.Quote => ParseGlyphicAppl(SpecialKeyword.Quote.Symbol, nextToken.Source, tokens, symGen),
                TokenType.Quasiquote => ParseGlyphicAppl(SpecialKeyword.Quote.Symbol, nextToken.Source, tokens, symGen),
                TokenType.Unquote => ParseGlyphicAppl(SpecialKeyword.Quote.Symbol, nextToken.Source, tokens, symGen),
                TokenType.UnquoteSplice => ParseGlyphicAppl(SpecialKeyword.Quote.Symbol, nextToken.Source, tokens, symGen),

                TokenType.Integer => ParseInteger(nextToken),
                TokenType.Symbol => ParseSymbol(nextToken, symGen),
                TokenType.True => ParseBoolean(nextToken),
                TokenType.False => ParseBoolean(nextToken),

                _ => throw new Exception($"Can't parse {nextToken.Type} token: {nextToken}")
            };
        }

        private static ISyntax ParseList(IEnumerator<Token> tokens, SymbolFactory symGen, SourceRef listSource, bool bracketed)
        {
            if (tokens.Current.Type == TokenType.RightParen
                || tokens.Current.Type == TokenType.RightBrack)
            {
                if (tokens.Current.Type == TokenType.RightParen && bracketed)
                {
                    throw new Exception("Unexpected right parenthesis.");
                }

                if (tokens.Current.Type == TokenType.RightBrack && !bracketed)
                {
                    throw new Exception("Unexpected right bracket.");
                }

                tokens.MoveNext(); // pop off the closing paren/bracket
                return new StxDatum(Nil.Instance, listSource);
            }
            else if (tokens.Current.Type == TokenType.DotOp)
            {
                tokens.MoveNext(); // pop the dot

                ISyntax finale = ParseNextExpression(tokens, symGen);

                // assert that the list is closed immediately afterward
                if (ParseList(tokens, symGen, listSource, bracketed).IsNil)
                {
                    return finale;
                }
                else
                {
                    throw new Exception("Expected dotted item to terminate list.");
                }
            }
            else
            {
                ISyntax car = ParseNextExpression(tokens, symGen);
                ISyntax cdr = ParseList(tokens, symGen, listSource, bracketed);

                SourceRef mergedSource = listSource is null
                    ? car.Source.MergeWith(cdr.Source)
                    : listSource.MergeWith(cdr.Source);

                return new StxPair(mergedSource)
                {
                    Car = car,
                    Cdr = cdr
                };
            }
        }

        //private static StxPair ParseVector(IEnumerator<Token> tokens, SymbolFactory symGen, SourceRef listSource)
        //{
        //    ISyntax args = ParseList(tokens, symGen, listSource, false);
        //    return new StxPair(listSource)
        //    {
        //        Car = new Identifier(Operator.Vector.Symbol, listSource),
        //        Cdr = args
        //    };
        //}

        private static StxPair ParseGlyphicAppl(Symbol opSym, SourceRef src, IEnumerator<Token> tokens, SymbolFactory symGen)
        {
            Identifier keyword = new(opSym, src);
            ISyntax operand = ParseNextExpression(tokens, symGen);

            return new StxPair(src)
            {
                Car = keyword,
                Cdr = new StxPair(src)
                {
                    Car = operand,
                    Cdr = new StxDatum(Nil.Instance, src)
                }
            };
        }

        private static StxDatum ParseInteger(Token token)
        {
            return new StxDatum(new Integer(int.Parse(token.Text)), token.Source);
        }

        private static Identifier ParseSymbol(Token token, SymbolFactory symGen)
        {
            return new Identifier(symGen.Intern(token.Text), token.Source);
        }

        private static StxDatum ParseBoolean(Token token)
        {
            return new StxDatum(token.Type == TokenType.True ? SchemeData.Boolean.True : SchemeData.Boolean.False, token.Source);
        }
    }
}
