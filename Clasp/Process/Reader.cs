using Clasp.Data.AbstractSyntax;
using Clasp.Data.Text;
using Clasp.Data.Terms;
using System.Collections.Generic;
using System.Linq;
using System;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.Syntax;
using Clasp.Data.Terms.Product;

namespace Clasp.Process
{
    internal static class Reader
    {
        /// <summary>
        /// Reads the given tokens into the syntactic representation of a program.
        /// The given sequence is assumed not to be empty.
        /// </summary>
        public static Syntax ReadTokens(IEnumerable<Token> tokens)
        {
            // First, do a quick check to make sure the parentheses all match up
            CheckParentheses(tokens);

            if (tokens.Any())
            {
                return ReadSyntax(new Stack<Token>(tokens.Reverse()));
            }
            else
            {
                throw new ReaderException.EmptyTokenStream();
            }

        }

        #region Parentheses-Checking
        private static void CheckParentheses(IEnumerable<Token> tokens)
        {
            int parenCheck = CountOpenParens(tokens).CompareTo(CountCloseParens(tokens));

            if (parenCheck != 0)
            {
                bool extraCloseParens = parenCheck < 0;

                Token? extraParen = LocateExtraParen(extraCloseParens, tokens);

                if (extraParen is null)
                {
                    throw new ReaderException.AmbiguousParenthesis(!extraCloseParens);
                }
                else
                {
                    throw new ReaderException.UnmatchedParenthesis(extraParen);
                }
            }
        }
        private static int CountOpenParens(IEnumerable<Token> tokens)
        {
            return tokens.Where(x => x.TType == TokenType.OpenListParen || x.TType == TokenType.OpenVecParen).Count();
        }
        private static int CountCloseParens(IEnumerable<Token> tokens)
        {
            return tokens.Where(x => x.TType == TokenType.ClosingParen).Count();
        }
        private static Token? LocateExtraParen(bool extraCloseParen, IEnumerable<Token> input)
        {
            IEnumerable<Token> tokenStream = extraCloseParen ? input : input.Reverse();
            int parenInc = extraCloseParen ? 1 : -1;

            int parenCounter = 0;

            foreach (Token token in tokenStream)
            {
                if (token.TType == TokenType.OpenListParen
                    || token.TType == TokenType.OpenVecParen)
                {
                    parenCounter += parenInc;
                }
                else if (token.TType == TokenType.ClosingParen)
                {
                    parenCounter -= parenInc;
                }

                if (parenCounter < 0)
                {
                    return token;
                }
            }

            return null;
        }
        #endregion

        private static Syntax ReadSyntax(Stack<Token> tokens)
        {
            // Use this later to extract metadata about the syntax
            Token current = tokens.Pop();

            // Ignore whitespace
            while (current.TType == TokenType.Whitespace)
            {
                current = tokens.Pop();
            }

            // The reader must produce a syntax object.
            // As a syntax object may only encapsulate a list, a symbol, or some other atom...
            // Then all valid syntax must itself belong to one of these categories

            Term nextValue = current.TType switch
            {
                TokenType.ClosingParen => throw new ReaderException.UnexpectedToken(current),
                TokenType.DotOperator => throw new ReaderException.UnexpectedToken(current),

                TokenType.OpenListParen => ReadList(tokens),
                TokenType.OpenVecParen => ReadVector(tokens),

                TokenType.Quote => NativelyExpandSyntax(current, Symbol.Quote, tokens),
                TokenType.Quasiquote => NativelyExpandSyntax(current, Symbol.Quasiquote, tokens),
                TokenType.Unquote => NativelyExpandSyntax(current, Symbol.Unquote, tokens),
                TokenType.UnquoteSplice => NativelyExpandSyntax(current, Symbol.UnquoteSplicing, tokens),

                TokenType.Syntax => NativelyExpandSyntax(current, Symbol.Syntax, tokens),
                TokenType.QuasiSyntax => NativelyExpandSyntax(current, Symbol.Quasisyntax, tokens),
                TokenType.Unsyntax => NativelyExpandSyntax(current, Symbol.Unsyntax, tokens),
                TokenType.UnsyntaxSplice => NativelyExpandSyntax(current, Symbol.UnsyntaxSplicing, tokens),

                TokenType.Symbol => Symbol.Intern(current.Text),
                TokenType.Character => Character.Intern(current),
                TokenType.String => new CharString(current.Text),
                TokenType.Boolean => current.Text == Data.Terms.Boolean.True.ToString()
                    ? Data.Terms.Boolean.True
                    : Data.Terms.Boolean.False,
                TokenType.DecInteger => new Integer(long.Parse(current.Text)),
                TokenType.DecReal => new Real(double.Parse(current.Text)),

                TokenType.Malformed => throw new ReaderException.UnexpectedToken(current),

                _ => throw new ReaderException.UnhandledToken(current)
            };

            SourceLocation loc = new SourceLocation(
                current.Location.Source,
                current.Location.LineNumber,
                current.Location.Column,
                current.Location.StartingPosition,
                tokens.Count > 0
                    ? tokens.Peek().Location.StartingPosition - current.Location.StartingPosition - 1
                    : current.Location.SourceText.Sum(x => x.Length) - current.Location.StartingPosition - 1,
                current.Location.SourceText,
                true);

            Syntax wrappedValue = Syntax.FromDatum(nextValue, current);

            return wrappedValue;
        }

        private static Term NativelyExpandSyntax(Token opToken, Symbol opSym, Stack<Token> tokens)
        {
            Token subListToken = tokens.Peek();
            Syntax arg = ReadSyntax(tokens);

            Syntax terminator = Syntax.FromDatum(Nil.Value, arg);

            return ConsList.Cons(
                Syntax.FromDatum(opSym, opToken),
                Syntax.FromDatum(ConsList.Cons(arg, terminator), subListToken));
        }

        private static Term ReadVector(Stack<Token> tokens)
        {
            List<Syntax> contents = new List<Syntax>();

            while (tokens.Peek().TType != TokenType.ClosingParen)
            {
                Syntax nextTerm = ReadSyntax(tokens);
                contents.Add(nextTerm);
            }

            tokens.Pop(); // remove closing paren

            return new Vector(contents.ToArray());
        }

        // See here https://docs.racket-lang.org/reference/reader.html#%28part._parse-pair%29
        // For a peculiarity in how lists are read in the case of dotted terminators
        // Not sure that I care to implement that here?

        private static Term ReadList(Stack<Token> tokens)
        {
            if (tokens.Peek().TType == TokenType.DotOperator)
            {
                throw new ReaderException.UnexpectedToken(tokens.Peek());
            }
            else if (tokens.Peek().TType == TokenType.ClosingParen)
            {
                tokens.Pop(); // remove closing paren
                return Nil.Value;
            }

            Syntax car = ReadSyntax(tokens);

            if (tokens.Peek().TType == TokenType.DotOperator)
            {
                tokens.Pop(); // remove dot operator
                Syntax cdr = ReadSyntax(tokens);

                return ConsList.Cons(car, cdr);
            }
            else
            {
                Token subListBeginning = tokens.Peek();
                Term subList = ReadList(tokens);
                Syntax cdr = Syntax.FromDatum(subList, subListBeginning);

                return ConsList.Cons(car, cdr);
            }
        }

        //private static Term ReadList(Stack<Token> tokens)
        //{
        //    if (tokens.Peek().TType == TokenType.DotOperator)
        //    {
        //        throw new ReaderException.UnexpectedToken(tokens.Peek());
        //    }

        //    Stack<Syntax> output = new Stack<Syntax>();
        //    Token previous = tokens.Peek();
        //    bool dotted = false;

        //    while (tokens.Peek().TType != TokenType.ClosingParen
        //        && tokens.Peek().TType != TokenType.DotOperator)
        //    {
        //        previous = tokens.Peek();
        //        output.Push(ReadSyntax(tokens));
        //    }

        //    if (tokens.Peek().TType == TokenType.DotOperator)
        //    {
        //        previous = tokens.Peek();
        //        dotted = true;
        //        tokens.Pop(); //remove dot
        //        output.Push(ReadSyntax(tokens)); //add the rest
        //    }

        //    if (tokens.Peek().TType != TokenType.ClosingParen)
        //    {
        //        throw new ReaderException.ExpectedToken(TokenType.ClosingParen, tokens.Peek(), previous);
        //    }

        //    tokens.Pop(); //remove closing paren

        //    if (output.Count == 0)
        //    {
        //        return Nil.Value;
        //    }
        //    else if (!dotted)
        //    {
        //        output.Push(Syntax.Wrap(Nil.Value, tokens.Peek()));
        //    }

        //    return ConstructSyntacticList(output);
        //}

        //private static Term ConstructSyntacticList(Stack<Syntax> stk)
        //{
        //    Term output = stk.Pop();

        //    while(stk.Count > 0)
        //    {
        //        Syntax car = stk.Pop();
        //        output = ConsList.Cons(car, output);

        //        if (stk.Count > 0)
        //        {
        //            output = Syntax.Wrap(output, car);
        //        }
        //    }

        //    return output;
        //}

    }
}
