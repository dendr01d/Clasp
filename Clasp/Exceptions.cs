using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Text;

[assembly: InternalsVisibleTo("Tests")]
namespace Clasp
{
    public static class ExceptionExtensions
    {
        public static string SimplifyStackTrace(this Exception ex)
        {
            const string PATTERN = @"at (?<namespace>.*\(.*\))(?: in (?<path>\w\:(?>\\\w+)+(?:\.\w+)?\:.*))?";

            string trace = new string(ex.StackTrace);
            if (string.IsNullOrWhiteSpace(trace))
            {
                return "No stacktrace available.";
            }

            var matches = System.Text.RegularExpressions.Regex.Matches(trace, PATTERN);

            IEnumerable<string> condensedLines = matches
                .Select(x => new Tuple<string, string>(
                    x.Groups[1].Value,
                    x.Groups.Count > 2 ? x.Groups[2].Value : string.Empty))
                .Select(x => new Tuple<string, string>(
                    x.Item1.Split('.').Last(),
                    x.Item2.Split('\\').Last()))
                .Select(x => string.Format("   in {0}{1}{2}",
                    x.Item1,
                    string.IsNullOrEmpty(x.Item2) ? string.Empty : " at ",
                    x.Item2));

            return string.Join(System.Environment.NewLine, condensedLines);
        }
    }

    public abstract class ClaspException : Exception
    {
        protected ClaspException(string format, params object?[] args) : base(string.Format(format, args)) { }

        public class Uncategorized : ClaspException
        {
            public Uncategorized(string format, params object?[] args) : base(format, args) { }
        }
    }

    public abstract class LexerException : ClaspException
    {
        internal LexerException(string format, params object[] args) : base(format, args) { }

        public class MalformedInput : LexerException, ISourceTraceable
        {
            public SourceLocation Location { get; }
            public Blob SourceText { get; }
            internal MalformedInput(Token token) : base(
                "Malformed input on line {0} beginning at column {1}: {2}",
                token.Location.LineNumber,
                token.Location.Column,
                token.Text)
            {
                Location = token.Location;
                SourceText = token.SourceText;
            }
        }
    }

    public abstract class ReaderException : ClaspException
    {
        internal ReaderException(string format, params object?[] args) : base(format, args) { }

        public class UnmatchedParenthesis : ReaderException, ISourceTraceable
        {
            public SourceLocation Location { get; }
            public Blob SourceText { get; }
            internal UnmatchedParenthesis(Token errantParenToken) : base(
                "The reader found an {0} parenthesis on line {1}, column {2}.",
                errantParenToken.TType == TokenType.ClosingParen ? "un-opened" : "un-closed",
                errantParenToken.Location.LineNumber,
                errantParenToken.Location.Column)
            {
                Location = errantParenToken.Location;
                SourceText = errantParenToken.SourceText;
            }
        }

        public class UnexpectedToken : ReaderException, ISourceTraceable
        {
            public SourceLocation Location { get; }
            public Blob SourceText { get; }
            internal UnexpectedToken(Token errantToken) : base(
                "Unexpected token {0} at line {1}, column {2}.",
                errantToken,
                errantToken.Location.LineNumber,
                errantToken.Location.Column)
            {
                Location = errantToken.Location;
                SourceText = errantToken.SourceText;
            }
        }

        public class ExpectedToken : ReaderException, ISourceTraceable
        {
            public SourceLocation Location { get; }
            public Blob SourceText { get; }
            internal ExpectedToken(TokenType expectedType, Token receivedToken, Token prevToken) : base(
                "Expected {0} token to follow after {1} on line {2}, column {3}.",
                expectedType,
                prevToken,
                prevToken.Location.LineNumber,
                prevToken.Location.Column)
            {
                Location = receivedToken.Location;
                SourceText = receivedToken.SourceText;
            }
        }

        public class UnhandledToken : ReaderException, ISourceTraceable
        {
            public SourceLocation Location { get; }
            public Blob SourceText { get; }
            internal UnhandledToken(Token errantToken) : base(
                "Token {0} of type {1} (on line {2}, column {3}) is unhandled by CLASP at this time :)",
                errantToken,
                errantToken.TType,
                errantToken.Location.LineNumber,
                errantToken.Location.Column)
            {
                Location = errantToken.Location;
                SourceText = errantToken.SourceText;
            }
        }
    }

    public abstract class ExpanderException : ClaspException
    {
        protected ExpanderException(string format, params object?[] args) : base(format, args) { }

        public class UnknownForm : ExpanderException
        {
            internal UnknownForm(Syntax unknownForm) : base(
                "The given syntax is invalid for expandsion: {0}",
                unknownForm)
            { }
        }

        public class BindingResolution : ExpanderException
        {
            internal BindingResolution(Identifier id, string? additionalMsg) : base(
                "Unable to resolve compile-time binding of identifier{0}: {1}",
                additionalMsg is null ? string.Empty : string.Format(" ({0})", additionalMsg),
                id)
            { }

            internal BindingResolution(string symbolicName, ScopeSet ss) : base(
                "Unable to resolve binding of name '{0}' with scope: {1}",
                symbolicName,
                ss)
            { }

            internal BindingResolution(string symbolicName, ScopeSet ss, params KeyValuePair<ScopeSet, string>[] matches) : base(
                "Binding of name '{0}' and scope {1} ambiguously matches several bound names:{2}{3}",
                symbolicName,
                ss,
                System.Environment.NewLine,
                string.Join(System.Environment.NewLine, matches.Select(x => string.Format("   {0} @ {1}", x.Key, x.Value))))
            { }
        }

        public class InvalidContext : ExpanderException
        {
            internal InvalidContext(Identifier op, ExpansionContext ctx) : base(
                "Form with operator '{0}' is invalid to be expanded in '{1}' context.",
                op,
                ctx.ToString())
            { }
        }

        public class InvalidFormShape : ExpanderException
        {
            internal InvalidFormShape(Symbol formKeyword, Syntax given) : base(
                "Cannot expand as '{0}' form the syntax: {1}",
                formKeyword,
                given)
            { }

            internal InvalidFormShape(string abstractShapeName, Syntax given) : base(
                "Cannot expand as abstract \"{0}\" the syntax: {1}",
                abstractShapeName,
                given)
            { }
        }
    }

    public abstract class ParserException : ClaspException
    {
        internal ParserException(string format, params object?[] args) : base(format, args) { }

        public class UnknownSyntax : ParserException
        {
            internal UnknownSyntax(AstNode error) : base(
                "The parser couldn't recognize the form of this syntax: {0}",
                error)
            { }
        }

        public class WrongArity : ParserException
        {
            internal WrongArity(string formTag, int expectedArgNum, bool exact, IEnumerable<AstNode> given) : base(
                "The '{0}' form expects {1} {2}argument{3}, but was given: {4}",
                formTag,
                expectedArgNum,
                exact ? string.Empty : "or more ",
                expectedArgNum == 1 ? string.Empty : "s",
                string.Concat(given.Select(x => string.Format("{0}   -> {1}", System.Environment.NewLine, x.ToString()))))
            { }
        }

        public class WrongArgType : ParserException
        {
            internal WrongArgType(string formTag, string expectedType, IEnumerable<AstNode> given) : base(
                "The '{0}' form expects {1} of type {2}, but was given: {3}",
                formTag,
                given.Count() > 1 ? "arguments" : "an argument",
                expectedType,
                string.Concat(given.Select(x => string.Format("{0}   -> {1}", System.Environment.NewLine, x.ToString()))))
            { }

            internal WrongArgType(string formTag, string expectedType, int index, AstNode given) : base(
                "The '{0}' form expects an argument of type {1} at index {2}, but was given: {3}",
                formTag,
                expectedType,
                index,
                given)
            { }
        }
    }

    //internal class ExpectedTypeException<T> : ClaspException
    //    where T : Expression
    //{
    //    internal ExpectedTypeException(Expression erroneous) : base(FormatMsg(erroneous)) { }

    //    private static string FormatMsg(Expression erroneous)
    //    {
    //        return $"Expected Expression of type '{typeof(T).Name}' but received '{erroneous}' of type '{erroneous.GetType().Name}'.";
    //    }        
    //}

    //internal class IncompatibleTypeException<T, U> : ClaspException
    //    where T : Expression
    //    where U : Expression
    //{
    //    internal IncompatibleTypeException(T e1, U e2, string opName) : base(FormatMsg(e1, e2, opName)) { }

    //    private static string FormatMsg(T e1, U e2, string opName)
    //    {
    //        return $"{e1} of type '{typeof(T).Name}' and {e2} of type '{typeof(U).Name}' are incompatible arguments to operation '{opName}'.";
    //    }
    //}

    //public class ArityConflictException : Exception
    //{
    //    internal ArityConflictException(Procedure proc, Expression? argl = null) : 
    //        base(FormatMsg(proc, argl))
    //    { }

    //    private static string FormatMsg(Procedure proc, Expression? argl)
    //    {
    //        return string.Format("{0} arguments {1} provided to procedure {2}",
    //            argl is null ? "Insufficient" : "Extraneous",
    //            argl is null ? string.Empty : argl.ToString(),
    //            proc.ToString());
    //    }
    //}

    //public class MissingArgumentException : Exception
    //{
    //    internal MissingArgumentException(string funcName) :
    //        base($"Expected additional argument(s) for function '{funcName}'.")
    //    { }
    //}

    //public class DuplicateBindingException : Exception
    //{
    //    internal DuplicateBindingException(Symbol sym) :
    //        base($"Attempted to re-Define binding of Symbol '{sym}'.")
    //    { }
    //}

    public class MissingBindingException : ClaspException
    {
        internal MissingBindingException(string name) :
            base($"Attempted to access non-existent binding of name '{name}'.")
        { }
    }

    //public class UninitializedRegisterException : Exception
    //{
    //    internal UninitializedRegisterException(string registerName) :
    //        base($"Attempted to read value of uninitialized register '{registerName}'.")
    //    { }
    //}

    //public class StackUnderflowException<T> : Exception
    //{
    //    internal StackUnderflowException(Stack<T> stack) :
    //        base($"Attempted to pop data from empty {typeof(T).Name} stack in machine.")
    //    { }
    //}

    //public class InvalidNumericOperationException : Exception
    //{
    //    internal InvalidNumericOperationException(Number num, string op) :
    //        base(string.Format("Tried to perform illegal operation '{0}' on number {1}", op, num))
    //    { }

    //    internal InvalidNumericOperationException(Number num, string op, Number numArg) :
    //        base(string.Format("Tried to perform illegal operation '{0}' on numbers {1} and {2}", op, num, numArg))
    //    { }
    //}
}
