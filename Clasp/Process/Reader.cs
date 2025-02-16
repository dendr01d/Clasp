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
using Clasp.Interfaces;

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

                TokenType.Quote => NativelyExpand(current, Symbol.Quote, tokens),
                TokenType.Quasiquote => NativelyExpand(current, Symbol.Quasiquote, tokens),
                TokenType.Unquote => NativelyExpand(current, Symbol.Unquote, tokens),
                TokenType.UnquoteSplice => NativelyExpand(current, Symbol.UnquoteSplicing, tokens),

                TokenType.Syntax => NativelyExpand(current, Symbol.Syntax, tokens),
                TokenType.QuasiSyntax => NativelyExpand(current, Symbol.Quasisyntax, tokens),
                TokenType.Unsyntax => NativelyExpand(current, Symbol.Unsyntax, tokens),
                TokenType.UnsyntaxSplice => NativelyExpand(current, Symbol.UnsyntaxSplicing, tokens),

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

        private static SyntaxList NativelyExpand(Token opToken, Symbol opSym, Stack<Token> tokens)
        {
            Syntax arg = ReadSyntax(tokens);

            LexInfo info = SynthesizeLexicalSource(opToken, arg);

            Identifier op = new Identifier(opSym, arg.LexContext);

            return new SyntaxList(arg, info)
                .Push(op);
        }

        private static Datum ReadVector(Token lead, Stack<Token> tokens)
        {
            List<Syntax> contents = new List<Syntax>();

            while (tokens.Peek().TType != TokenType.ClosingParen)
            {
                Syntax nextTerm = ReadSyntax(tokens);
                contents.Add(nextTerm);
            }

            Token close = tokens.Pop(); // remove closing paren

            LexInfo info = SynthesizeLexicalSource(lead, close);

            return new Datum(new Vector(contents.ToArray()), info);
        }

        private static Syntax ReadList(Token lead, Stack<Token> tokens)
        {
            if (tokens.Peek().TType == TokenType.DotOperator)
            {
                throw new ReaderException.UnexpectedToken(tokens.Peek());
            }
            else if (tokens.Peek().TType == TokenType.ClosingParen)
            {
                Token close = tokens.Pop(); // remove closing paren
                LexInfo info = SynthesizeLexicalSource(lead, close);
                return new Datum(Nil.Value, info);
            }

            Token lastReadToken = tokens.Peek(); // stash the last token read for reporting purposes

            List<Syntax> contents = new List<Syntax>();
            bool dottedList = false;

            while(tokens.Peek().TType != TokenType.DotOperator
                && tokens.Peek().TType != TokenType.ClosingParen)
            {
                lastReadToken = tokens.Peek();
                Syntax next = ReadSyntax(tokens);
                contents.Add(next);
            }

            // check if there's a dotted element at the end
            if (tokens.Peek().TType == TokenType.DotOperator)
            {
                // must be an element before AND after the dot
                if (contents.Count == 0)
                {
                    throw new ReaderException.UnexpectedToken(tokens.Peek());
                }

                tokens.Pop(); // remove the dot operator
                dottedList = true;

                lastReadToken = tokens.Peek();
                Syntax next = ReadSyntax(tokens); // read the dotted item
                contents.Add(next);
            }

            // ensure that a closing paren finishes the list
            if (tokens.Peek().TType != TokenType.ClosingParen)
            {
                throw new ReaderException.ExpectedListEnd(tokens.Peek(), lastReadToken);
            }

            // pop off the closing paren while also synthesizing the aggregate lexical info
            LexInfo listContext = SynthesizeLexicalSource(lead, tokens.Pop());

            if (dottedList)
            {
                return SyntaxList.ImproperList(listContext, contents[0], contents[1], contents[2..].ToArray());
            }
            else
            {
                return SyntaxList.ProperList(listContext, contents[0], contents[1..].ToArray());
            }
        }

        private static LexInfo SynthesizeLexicalSource(ISourceTraceable first, ISourceTraceable rest)
        {
            SourceCode sc = new SourceCode(
                first.Location.Source,
                first.Location.LineNumber,
                first.Location.Column,
                first.Location.StartingPosition,
                rest.Location.StartingPosition + rest.Location.Length - first.Location.StartingPosition,
                first.Location.SourceText,
                true);

            return new LexInfo(sc);
        }

    }
}
