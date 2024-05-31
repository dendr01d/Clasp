using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal class ExpectedTypeException<T> : Exception
        where T : Expression
    {
        public ExpectedTypeException(Expression erroneous) : base(FormatMsg(erroneous)) { }

        private static string FormatMsg(Expression erroneous)
        {
            return $"Expected Expression of type '{typeof(T).Name}' but received '{erroneous}' of type '{erroneous.GetType().Name}'.";
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
