using System;
using System.Collections.Generic;

using Clasp.Data.AbstractSyntax;

namespace Clasp.Exceptions
{
    public class InterpreterException : ClaspException
    {
        internal Stack<VmInstruction> ContinuationTrace;

        internal InterpreterException(Stack<VmInstruction> cont, string format, params object?[] args)
            : base(format, args)
        {
            ContinuationTrace = cont;
        }

        internal InterpreterException(Stack<VmInstruction> cont, Exception innerException, string format, params object?[] args)
            : base(innerException, format, args)
        {
            ContinuationTrace = cont;
        }

        public class InvalidBinding : InterpreterException
        {
            internal InvalidBinding(string varName, Stack<VmInstruction> cont) : base(
                cont,
                "Unable to dereference binding of identifier '{0}'.",
                varName)
            { }
        }

        public class InvalidOperationException : InterpreterException
        {
            internal InvalidOperationException(VmInstruction operation, Stack<VmInstruction> cont, Exception innerException) : base(
                cont,
                innerException,
                "Error evaluating the operation:\n\t{0}",
                operation)
            { }
        }
    }

}
