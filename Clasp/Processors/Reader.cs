using System;
using System.Collections.Generic;
using System.Linq;

using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Exceptions;
using Clasp.Interfaces;

namespace Clasp.Process
{
    internal static class Reader
    {
        ///// <summary>
        ///// Read a series of tokens into a series of syntactic terms, as elements of an implicit
        ///// <see cref="Keywords.BEGIN"/> form.
        ///// </summary>
        //public static Syntax ReadBeginForm(IEnumerable<Token> tokens)
        //{
        //    return ReadTokens(Symbols.S_Begin, tokens);
        //}

        ///// <summary>
        ///// Read a series of tokens into a series of syntactic terms, as elements of an
        ///// (implicit or otherwise) <see cref="Keywords.MODULE"/> form.
        ///// </summary>
        //public static Syntax ReadModuleForm(IEnumerable<Token> tokens, string moduleName)
        //{
        //    IEnumerable<Token> checkedTokens = tokens;

        //    if (tokens.First().TType == TokenType.ModuleFlag)
        //    {
        //        checkedTokens = checkedTokens.Skip(1); // remove the module flag

        //        Token moduleNameToken = Token.Tokenize(TokenType.Symbol, moduleName, tokens.First().SourceText, tokens.First().Location);

        //        checkedTokens = checkedTokens.Prepend(moduleNameToken);
        //    }

        //    return ReadTokens(Symbols.S_ImplicitModule, checkedTokens);
        //}

        /// <summary>
        /// Reads the given tokens into a Term, or a proper list of Terms if there are more than one.
        /// </summary>
        public static Term ReadTokenText(IEnumerable<Token> tokens)
        {
            return ReadTokens(tokens, out _);
        }

        /// <summary>
        /// Reads the given tokens into a syntax object, or a SyntaxPair-list thereof if there are more than one.
        /// </summary>
        public static Syntax ReadTokenSyntax(IEnumerable<Token> tokens)
        {
            Term t = ReadTokens(tokens, out SourceCode location);
            return Syntax.WrapRaw(t, location);
        }

        private static Term ReadTokens(IEnumerable<Token> tokens, out SourceCode location)
        {
            // First, do a quick check to make sure the parentheses all match up
            CheckParentheses(tokens);

            if (!tokens.Any())
            {
                throw new ReaderException.EmptyTokenStream();
            }

            Syntax[] forms = ReadMultipleSyntaxes(new Stack<Token>(tokens.Reverse())).ToArray();
            location = SynthesizeLexicalSource(forms[0], forms[^1]);

            return Cons.ProperList(forms);
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

        private static IEnumerable<Syntax> ReadMultipleSyntaxes(Stack<Token> tokens)
        {
            while (tokens.Peek().TType != TokenType.Terminator)
            {
                yield return ReadSyntax(tokens);
            }
        }

        private static Syntax ReadSyntax(Stack<Token> tokens)
        {
            if (tokens.Peek().TType == TokenType.Terminator)
            {
                throw new ReaderException.UnexpectedToken(tokens.Peek());
            }

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

                TokenType.Quote => NativelyExpand(current, Symbols.Quote, tokens),
                TokenType.Quasiquote => NativelyExpand(current, Symbols.Quasiquote, tokens),
                TokenType.Unquote => NativelyExpand(current, Symbols.Unquote, tokens),
                TokenType.UnquoteSplice => NativelyExpand(current, Symbols.UnquoteSplicing, tokens),

                TokenType.Syntax => NativelyExpand(current, Symbols.Syntax, tokens),
                TokenType.QuasiSyntax => NativelyExpand(current, Symbols.Quasisyntax, tokens),
                TokenType.Unsyntax => NativelyExpand(current, Symbols.Unsyntax, tokens),
                TokenType.UnsyntaxSplice => NativelyExpand(current, Symbols.UnsyntaxSplicing, tokens),

                TokenType.Symbol => new Identifier(current),
                TokenType.Character => new Datum(Character.Intern(current), current.Location),
                TokenType.String => ReadCharString(current),
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

            return new Datum(value, current.Location);
        }

        private static Datum ReadInteger(Token current, int baseSystem)
        {
            string num = current.Text[0] == '#'
                ? new string(current.Text.AsSpan()[2..])
                : current.Text;

            return new Datum(new Integer(Convert.ToInt64(num, baseSystem)), current.Location);
        }

        private static Datum ReadReal(Token current)
        {
            string num = current.Text[0] == '#'
                ? new string(current.Text.AsSpan()[2..])
                : current.Text;

            return new Datum(new Real(double.Parse(num)), current.Location);
        }

        private static Datum ReadCharString(Token current)
        {
            string sansQuotes = current.Text.Trim('\"');
            return new Datum(new RefString(sansQuotes), current.Location);
        }

        private static SyntaxPair NativelyExpand(Token opToken, Symbol opSym, Stack<Token> tokens)
        {
            Syntax arg = ReadSyntax(tokens);

            SourceCode info = SynthesizeLexicalSource(opToken, arg);

            Identifier op = new Identifier(opSym, arg.Location);

            return new SyntaxPair(op, Cons.Truct(arg, Datum.NullSyntax()), info);
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

            SourceCode info = SynthesizeLexicalSource(lead, close);

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
                SourceCode info = SynthesizeLexicalSource(lead, close);
                return new Datum(Nil.Value, info);
            }

            Token lastReadToken = tokens.Peek(); // stash the last token read for reporting purposes

            List<Syntax> contents = new List<Syntax>();
            bool dottedList = false;

            while (tokens.Peek().TType != TokenType.DotOperator
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
            SourceCode listContext = SynthesizeLexicalSource(lead, tokens.Pop());

            Term maybeList = dottedList
                ? Cons.ImproperList(contents)
                : Cons.ProperList(contents);

            return Syntax.WrapRaw(maybeList, listContext);
        }

        private static SourceCode SynthesizeLexicalSource(ISourceTraceable first, ISourceTraceable rest)
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
