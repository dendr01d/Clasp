using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

    public class UncategorizedException : Exception
    {
        internal UncategorizedException(string msg) : base($"CLASP Exception: {msg}") { }
    }

    public class LexingException : Exception
    {
        internal LexingException(string msg) : base($"Lexing error: {msg}") { }
    }

    public class ParsingException : Exception
    {
        internal ParsingException(string msg, Token? problem) : base($"Parsing error{FormatToken(problem)}): {msg}") { }

        private static string FormatToken(Token? t)
        {
            if (t is null)
            {
                return string.Empty;
            }
            else
            {
                return $" @ Token {t} (line {t.SourceLine}, index {t.SourceIndex}";
            }
        }
    }

    public class LibraryException : Exception
    {
        internal LibraryException(Token leadingToken, Expression targetExpr, Exception inner)
            : base(FormatMsg(leadingToken, targetExpr, inner), inner) { }

        private static string FormatMsg(Token t, Expression e, Exception ex)
        {
            return string.Format("Error parsing library entry on line {0}:{1}{2}{1}{3}",
                t.SourceLine,
                System.Environment.NewLine,
                e.PrettyPrint(0),
                ex.Message);
        }
    }

    internal class ExpectedTypeException<T> : Exception
        where T : Expression
    {
        internal ExpectedTypeException(Expression erroneous) : base(FormatMsg(erroneous)) { }

        private static string FormatMsg(Expression erroneous)
        {
            return $"Expected Expression of type '{typeof(T).Name}' but received '{erroneous}' of type '{erroneous.GetType().Name}'.";
        }        
    }

    internal class IncompatibleTypeException<T, U> : Exception
        where T : Expression
        where U : Expression
    {
        internal IncompatibleTypeException(T e1, U e2, string opName) : base(FormatMsg(e1, e2, opName)) { }

        private static string FormatMsg(T e1, U e2, string opName)
        {
            return $"{e1} of type '{typeof(T).Name}' and {e2} of type '{typeof(U).Name}' are incompatible arguments to operation '{opName}'.";
        }
    }

    public class ArityConflictException : Exception
    {
        internal ArityConflictException(Procedure proc, Expression? argl = null) : 
            base(FormatMsg(proc, argl))
        { }

        private static string FormatMsg(Procedure proc, Expression? argl)
        {
            return string.Format("{0} arguments {1} provided to procedure {2}",
                argl is null ? "Insufficient" : "Extraneous",
                argl is null ? string.Empty : argl.ToString(),
                proc.ToString());
        }
    }

    public class MissingArgumentException : Exception
    {
        internal MissingArgumentException(string funcName) :
            base($"Expected additional argument(s) for function '{funcName}'.")
        { }
    }

    public class DuplicateBindingException : Exception
    {
        internal DuplicateBindingException(Symbol sym) :
            base($"Attempted to re-Define binding of Symbol '{sym}'.")
        { }
    }

    public class MissingBindingException : Exception
    {
        internal MissingBindingException(Symbol sym) :
            base($"Attempted to access non-existent binding of Symbol '{sym}'.")
        { }
    }

    public class UninitializedRegisterException : Exception
    {
        internal UninitializedRegisterException(string registerName) :
            base($"Attempted to read value of uninitialized register '{registerName}'.")
        { }
    }

    public class StackUnderflowException<T> : Exception
    {
        internal StackUnderflowException(Stack<T> stack) :
            base($"Attempted to pop data from empty {typeof(T).Name} stack in machine.")
        { }
    }

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
