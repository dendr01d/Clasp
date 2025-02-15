using Clasp.Data.AbstractSyntax;
using Clasp.Data.Text;
using Clasp.Data.Terms;
using System.Collections.Generic;
using System.Linq;
using System;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Terms.ProductValues;
using Clasp.ExtensionMethods;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

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

            return current.TType switch
            {
                TokenType.ClosingParen => throw new ReaderException.UnexpectedToken(current),
                TokenType.DotOperator => throw new ReaderException.UnexpectedToken(current),

                TokenType.OpenListParen => ReadList(current, tokens),
                TokenType.OpenVecParen => ReadVector(current, tokens),

                TokenType.Quote => NativelyExpandSyntax(current, Symbol.Quote, tokens),
                TokenType.Quasiquote => NativelyExpandSyntax(current, Symbol.Quasiquote, tokens),
                TokenType.Unquote => NativelyExpandSyntax(current, Symbol.Unquote, tokens),
                TokenType.UnquoteSplice => NativelyExpandSyntax(current, Symbol.UnquoteSplicing, tokens),

                TokenType.Syntax => NativelyExpandSyntax(current, Symbol.Syntax, tokens),
                TokenType.QuasiSyntax => NativelyExpandSyntax(current, Symbol.Quasisyntax, tokens),
                TokenType.Unsyntax => NativelyExpandSyntax(current, Symbol.Unsyntax, tokens),
                TokenType.UnsyntaxSplice => NativelyExpandSyntax(current, Symbol.UnsyntaxSplicing, tokens),

                TokenType.Symbol => new Identifier(current),
                TokenType.Character => new Datum(Character.Intern(current), current),
                TokenType.String => new Datum(new CharString(current.Text), current),
                TokenType.Boolean => ReadBoolean(current),

                TokenType.BinInteger => ReadInteger(current, 2),
                TokenType.OctInteger => ReadInteger(current, 8),
                TokenType.HexInteger => ReadInteger(current, 16),
                TokenType.DecInteger => ReadInteger(current, 10),
                TokenType.DecReal => ReadReal(current),

                TokenType.Malformed => throw new ReaderException.UnexpectedToken(current),

                _ => throw new ReaderException.UnhandledToken(current)
            };
        }

        private static Datum ReadBoolean(Token current)
        {
            Data.Terms.Boolean value = current.Text == Data.Terms.Boolean.True.ToString()
                ? Data.Terms.Boolean.True
                : Data.Terms.Boolean.False;

            return new Datum(value, current);
        }

        private static Datum ReadInteger(Token current, int baseSystem)
        {
            string num = current.Text[0] == '#'
                ? new string(current.Text.AsSpan()[2..])
                : current.Text;

            return new Datum(new Integer(Convert.ToInt64(num, baseSystem)), current);
        }

        private static Datum ReadReal(Token current)
        {
            string num = current.Text[0] == '#'
                ? new string(current.Text.AsSpan()[2..])
                : current.Text;

            return new Datum(new Real(double.Parse(num)), current);
        }

        private static Syntax NativelyExpandSyntax(Token opToken, Symbol opSym, Stack<Token> tokens)
        {
            Token subListToken = tokens.Peek();
            Syntax arg = ReadSyntax(tokens);

            SourceCode loc = SynthesizeSourceStructure(opToken, arg);

            return Datum.Implicit(Nil.Value)
                .Cons(arg, new LexInfo(subListToken.Location))
                .Cons(Syntax.FromDatum(opSym, opToken), new LexInfo(loc));
        }

        private static Syntax ReadVector(Token lead, Stack<Token> tokens)
        {
            List<Syntax> contents = new List<Syntax>();

            while (tokens.Peek().TType != TokenType.ClosingParen)
            {
                Syntax nextTerm = ReadSyntax(tokens);
                contents.Add(nextTerm);
            }

            Token close = tokens.Pop(); // remove closing paren

            SourceCode loc = SynthesizeSourceStructure(lead, close);

            return new Datum(new Vector(contents.ToArray()), new LexInfo(loc));
        }

        // See here https://docs.racket-lang.org/reference/reader.html#%28part._parse-pair%29
        // For a peculiarity in how lists are read in the case of dotted terminators
        // Not sure that I care to implement that here?

        private static Syntax ReadList(Token lead, Stack<Token> tokens)
        {
            if (tokens.Peek().TType == TokenType.DotOperator)
            {
                throw new ReaderException.UnexpectedToken(tokens.Peek());
            }
            else if (tokens.Peek().TType == TokenType.ClosingParen)
            {
                Token close = tokens.Pop(); // remove closing paren
                SourceCode loc = SynthesizeSourceStructure(lead, close);
                return new Datum(Nil.Value, new LexInfo(loc));
            }

            Syntax car = ReadSyntax(tokens);

            if (tokens.Peek().TType == TokenType.ClosingParen)
            {
                Token close = tokens.Pop(); // remove closing paren
                SourceCode loc = SynthesizeSourceStructure(lead, close);
                return new SyntaxPair(car, Datum.Implicit(Nil.Value), new LexInfo(loc));
            }
            else if (tokens.Peek().TType == TokenType.DotOperator)
            {
                Token dotOp = tokens.Pop(); // remove dot operator
                Syntax cdr = ReadSyntax(tokens);

                if (tokens.Peek().TType != TokenType.ClosingParen)
                {
                    throw new ReaderException.ExpectedListEnd(tokens.Peek(), dotOp);
                }

                Token close = tokens.Pop(); // remove closing paren
                SourceCode loc = SynthesizeSourceStructure(lead, close);
                return new SyntaxPair(car, cdr, new LexInfo(loc));
            }
            else
            {
                Syntax cdr = ReadList(tokens.Peek(), tokens);
                SourceCode loc = SynthesizeSourceStructure(lead, cdr);
                return new SyntaxPair(car, cdr, new LexInfo(loc));
            }
        }

        private static SourceCode SynthesizeSourceStructure(ISourceTraceable first, ISourceTraceable rest)
        {
            return new SourceCode(
                first.Location.Source,
                first.Location.LineNumber,
                first.Location.Column,
                first.Location.StartingPosition,
                rest.Location.StartingPosition + rest.Location.Length - first.Location.StartingPosition,
                first.Location.SourceText,
                true);
        }

    }
}
