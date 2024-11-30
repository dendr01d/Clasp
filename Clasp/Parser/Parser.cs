using System;
using System.Reflection;

using Clasp.AST;

namespace Clasp
{
    internal static class Parser
    {
        public static AstNode ParseAST(params Syntax[] stx)
        {
            if (stx.Length > 1)
            {
                return ParseSequence(stx);
            }
            else
            {
                return stx[0] switch
                {
                    SyntaxId sid => ParseIdentifier(sid),
                    SyntaxAtom sat => sat.WrappedValue,
                    SyntaxProduct sp => ParseProduct(sp),
                    _ => throw new ParserException("Unknown syntax.", stx[0])
                };
            }
        }

        private static Var ParseIdentifier(SyntaxId id)
        {
            return new Var(id.WrappedValue.Name);
        }

        private static AstNode ParseProduct(SyntaxProduct prod)
        {
            if (prod.WrappedValue is Vector vec)
            {
                List<Fixed> contents = new List<Fixed>();
                int index = 0;

                foreach(Fixed value in vec.Values)
                {
                    if (value is Syntax stxValue)
                    {
                        AstNode parsed = ParseAST(stxValue);

                        if (parsed is Fixed parsedValue)
                        {
                            contents.Add(parsedValue);
                        }
                        else
                        {
                            throw new ParserException(
                                string.Format("Expected vector element {0} at index {1} to parse to fixed value.",
                                    value, index),
                                prod,
                                stxValue);
                        }
                    }
                    else
                    {
                        throw new ParserException(
                            string.Format("Vector element {0} at index {1} isn't syntax that can be parsed.",
                                value, index),
                            prod);
                    }

                    ++index;
                }
            }
            else if (prod.WrappedValue is ConsCell cell)
            {
                if (cell.Car is Syntax stxCar)
                {
                    if (cell.Cdr is Syntax stxCdr)
                    {
                        return ParseTopCell(stxCar, stxCdr);
                    }
                    else
                    {
                        throw new ParserException(
                            string.Format("ConsCell cdr {0} isn't syntax that can be parsed.", cell.Cdr),
                            prod);
                    }
                }
                else
                {
                    throw new ParserException(
                        string.Format("ConsCell car {0} isn't syntax that can be parsed.", cell.Car),
                        prod);
                }
            }

            throw new ParserException("Unknown syntax.", prod);
        }

        private static AstNode ParseTopCell(Syntax car, Syntax cdr)
        {
            AstNode parsedCar = ParseAST(car);

            if (parsedCar is CmdNode)
            {
                throw new ParserException("Leading term of new cons list isn't generative.", car);
            }

            //the only valid way to start a list is with a proc or otherwise not-fixed gennode
            //buuuut procs technically can't be represented syntactically
            //primitives are indirectly accessed by reference, as as compounds
            //so only non-fixed gennodes are allowed
        }

        private static Sequence ParseSequence(IEnumerable<Syntax> stx)
        {
            IEnumerable<AstNode> pieces = stx.Select(x => ParseAST(x));

            AstNode[] series = pieces.SkipLast(1).ToArray();
            
            if (pieces.Last() is GenNode final)
            {
                return new Sequence(final, series);
            }
            else
            {
                throw new ParserException(
                    string.Format("Failed to parse {0} -- final {1} must be a {2}.",
                        nameof(Sequence), nameof(AstNode), nameof(GenNode)),
                    stx[0],
                    stx[^1]);
            }
        }




        public static IEnumerable<Expression> ParseText(string input)
        {
            return Parse(Lexer.LexLines(new string[] { input }));
        }

        public static IEnumerable<Expression> ParseFile(string path)
        {
            IEnumerable<string> sourceLines = File.ReadAllLines(path);
            return Parse(Lexer.LexLines(sourceLines));
        }

        public static IEnumerable<Expression> Parse(IEnumerable<Token> tokens)
        {
            if (!tokens.Any())
            {
                yield return Expression.Nil;
                yield break;
            }
            else
            {
                foreach(IEnumerable<Token> segment in Lexer.SegmentTokens(tokens))
                {
                    Stack<Token> stack = new Stack<Token>(segment.Reverse());
                    yield return ParseTokens(stack);
                }
            }
        }

        public static Expression ParseTokens(Stack<Token> tokens)
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
                    //TokenType.VecParen => ParseVector(tokens),
                    TokenType.OpenParenLst => ParseList(tokens),
                    TokenType.ClosingParen => throw new ParsingException("Unexpected ')'", current),
                    TokenType.ListDot => throw new ParsingException("Unexpected '.'", current),

                    TokenType.QuoteMrk => Pair.List(Symbol.Quote, ParseTokens(tokens)),
                    TokenType.SyntaxMrk => Pair.List(Symbol.Syntax, ParseTokens(tokens)),
                    TokenType.QuasiquoteMrk => Pair.List(Symbol.Quasiquote, ParseTokens(tokens)),
                    TokenType.UnquoteMrk => Pair.List(Symbol.Unquote, ParseTokens(tokens)),
                    TokenType.UnquoteSpliceMrk => Pair.List(Symbol.UnquoteSplicing, ParseTokens(tokens)),
                    TokenType.Ellipsis => Symbol.Ellipsis,

                    TokenType.Symbol => Symbol.Ize(current.Text),
                    TokenType.Number => new SimpleNum(decimal.Parse(current.Text)),
                    TokenType.Boolean => (current.Text == Boolean.True.ToString()),
                    TokenType.Character => Character.FromToken(current),
                    TokenType.QuotedString => Charstring.FromToken(current),

                    TokenType.Error => new Error("Parsing error?"),

                    _ => throw new ParsingException($"Unknown token type {current.TType}", current)
                };
            }
        }

        private static Expression ParseList(Stack<Token> tokens)
        {
            if (tokens.Peek().TType == TokenType.ListDot)
            {
                throw new ParsingException("Expected Car of dotted pair", tokens.Peek());
            }

            List<Expression> exprs = new List<Expression>();
            bool dottedPair = false;

            while (tokens.Peek().TType != TokenType.ClosingParen
                && tokens.Peek().TType != TokenType.ListDot)
            {
                exprs.Add(ParseTokens(tokens));
            }

            if (tokens.Peek().TType == TokenType.ListDot)
            {
                dottedPair = true;

                tokens.Pop(); //remove dot marker
                exprs.Add(ParseTokens(tokens)); //grab the rest

                if (tokens.Peek().TType != TokenType.ClosingParen)
                {
                    throw new ParsingException("Expected ')' after dotted pair.", tokens.Peek());
                }
            }

            tokens.Pop(); //remove right paren

            return dottedPair
                ? Pair.ListStar(exprs[0], exprs[1..].ToArray())
                : Pair.List(exprs.ToArray());
        }

        //private static Expression ParseVector(Stack<Token> tokens)
        //{
        //    List<Expression> newList = new();

        //    while (tokens.Peek().TType != TokenType.RightParen)
        //    {
        //        newList.Add(ParseTokens(tokens));
        //    }

        //    tokens.Pop(); //remove right paren

        //    return Vector.MkVector(newList.ToArray());
        //}

    }
}
