using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    public class LexingException : Exception
    {
        public LexingException(string msg) : base($"Lexing error: {msg}") { }
    }

    internal class ParsingException : Exception
    {
        public ParsingException(string msg, Token? problem) : base($"Parsing error{FormatToken(problem)}): {msg}") { }

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

    internal class ExpectedTypeException<T> : Exception
        where T : Expression
    {
        public ExpectedTypeException(Expression erroneous) : base(FormatMsg(erroneous)) { }

        private static string FormatMsg(Expression erroneous)
        {
            return $"Expected Expression of type '{typeof(T).Name}' but received '{erroneous}' of type '{erroneous.GetType().Name}'.";
        }        
    }

    internal class IncompatibleTypeException<T, U> : Exception
        where T : Expression
        where U : Expression
    {
        public IncompatibleTypeException(T e1, U e2, string opName) : base(FormatMsg(e1, e2, opName)) { }

        private static string FormatMsg(T e1, U e2, string opName)
        {
            return $"{e1} of type '{typeof(T).Name}' and {e2} of type '{typeof(U).Name}' are incompatible arguments to operation '{opName}'.";
        }
    }

    internal class MissingArgumentException : Exception
    {
        public MissingArgumentException(string funcName) :
            base($"Expected additional argument(s) for function '{funcName}'.")
        { }
    }

    internal class DuplicateBindingException : Exception
    {
        public DuplicateBindingException(Symbol sym) :
            base($"Attempted to re-Define binding of Symbol '{sym}'.")
        { }
    }

    internal class MissingBindingException : Exception
    {
        public MissingBindingException(Symbol sym) :
            base($"Attempted to access non-existent binding of Symbol '{sym}'.")
        { }
    }

    internal class UninitializedRegisterException : Exception
    {
        public UninitializedRegisterException(string registerName) :
            base($"Attempted to read value of uninitialized register '{registerName}'.")
        { }
    }

    internal class StackUnderflowException<T> : Exception
    {
        public StackUnderflowException(Stack<T> stack) :
            base($"Attempted to pop data from empty {typeof(T).Name} stack in machine.")
        { }
    }
}
