using System.Runtime.CompilerServices;

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

    public class LexerException : ClaspException
    {
        internal LexerException(string format, params object[] args) : base(format, args) { }

        public class MalformedInput : LexerException, ISourceTraceable
        {
            public Lexer.Token SourceTrace { get; }
            internal MalformedInput(Lexer.Token token) : base(
                "Malformed input on line {0} beginning at index {1}: {2}",
                token.LineNum,
                token.LineIdx,
                token.Text)
            {
                SourceTrace = token;
            }
        }
    }

    public class ReaderException : ClaspException
    {
        internal ReaderException(string format, params object?[] args) : base(format, args) { }

        public class UnmatchedParenthesis : ReaderException, ISourceTraceable
        {
            public Lexer.Token SourceTrace { get; }
            internal UnmatchedParenthesis(Lexer.Token errantParenToken) : base(
                "The reader found an {0} parenthesis on line {1}, index {2}.",
                errantParenToken.TType == Lexer.TokenType.ClosingParen ? "un-opened" : "un-closed",
                errantParenToken.LineNum,
                errantParenToken.LineIdx)
            {
                SourceTrace = errantParenToken;
            }
        }

        public class UnexpectedToken : ReaderException, ISourceTraceable
        {
            public Lexer.Token SourceTrace { get; }
            internal UnexpectedToken(Lexer.Token errantToken) : base(
                "Unexpected token {0} at line {1}, index {2}.",
                errantToken,
                errantToken.LineNum,
                errantToken.LineIdx)
            {
                SourceTrace = errantToken;
            }
        }

        public class ExpectedToken : ReaderException, ISourceTraceable
        {
            public Lexer.Token SourceTrace { get; }
            internal ExpectedToken(Lexer.TokenType expectedType, Lexer.Token receivedToken, Lexer.Token prevToken) : base(
                "Expected {0} token to follow after {1} at line {2}, index {3}.",
                expectedType,
                prevToken,
                prevToken.LineNum,
                prevToken.LineIdx)
            {
                SourceTrace = receivedToken;
            }
        }

        public class UnhandledToken : ReaderException, ISourceTraceable
        {
            public Lexer.Token SourceTrace { get; }
            internal UnhandledToken(Lexer.Token errantToken) : base(
                "Token {0} of type {1} (on line {2}, index {3}) is unhandled by CLASP at this time :)",
                errantToken,
                errantToken.TType,
                errantToken.LineNum,
                errantToken.LineIdx)
            {
                SourceTrace = errantToken;
            }
        }
    }

    public class IdResolutionException : ClaspException
    {
        public IdResolutionException(string format, params object[] args) : base(format, args) { }
    }

    public class ParserException : ClaspException
    {
        internal ParserException(string format, params object?[] args) : base(format, args) { }

        //public class ExpectedNestedSyntax : ParserException
        //{
        //    internal ExpectedNestedSyntax(AST.AstNode error, bool car, AST.Syntax context) : base(
        //        "Expected {0} of {1} in {2} to be syntax, but found {3}",
        //        car ? "car" : "cdr",
        //        nameof(AST.ConsCell),
        //        context,
        //        error)
        //    { }

        //    internal ExpectedNestedSyntax(AST.AstNode error, int index, AST.Syntax context) : base(
        //        "Expected {0} element at index {1} in {2} to be syntax, but found {3}",
        //        nameof(AST.Vector),
        //        index,
        //        context,
        //        error)
        //    { }
        //}

        public class UnknownSyntax : ParserException
        {
            internal UnknownSyntax(AST.AstNode error) : base(
                "The parser couldn't recognize the form of this syntax: {0}",
                error)
            { }
        }

        public class WrongArity : ParserException
        {
            internal WrongArity(string formTag, int expectedArgNum, bool exact, IEnumerable<AST.AstNode> given) : base(
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
            internal WrongArgType(string formTag, string expectedType, IEnumerable<AST.AstNode> given) : base(
                "The '{0}' form expects {1} of type {2}, but was given: {3}",
                formTag,
                given.Count() > 1 ? "arguments" : "an argument",
                expectedType,
                string.Concat(given.Select(x => string.Format("{0}   -> {1}", System.Environment.NewLine, x.ToString()))))
            { }

            internal WrongArgType(string formTag, string expectedType, int index, AST.AstNode given) : base(
                "The '{0}' form expects an argument of type {1} at index {2}, but was given: {3}",
                formTag,
                expectedType,
                index,
                given)
            { }
        }

        //public class ErroneousSyntax : ParserException
        //{
        //    internal ErroneousSyntax(Syntax[] trace, string format, object?[] args) : base(
        //        "{0}{2}",
        //        string.Format(format, args),
        //        trace.Select(x => string.Format(
        //            "{0}\t-> ({1}, {2}) : {3}",
        //            System.Environment.NewLine,
        //            x.,
        //            x.SourceIndex,
        //            x.ToString())))
        //    { }
        //}
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

    //public class MissingBindingException : Exception
    //{
    //    internal MissingBindingException(Symbol sym) :
    //        base($"Attempted to access non-existent binding of Symbol '{sym}'.")
    //    { }
    //}

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
