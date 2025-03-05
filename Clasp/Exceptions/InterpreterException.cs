using System;
using System.Collections.Generic;

using Clasp.Data.AbstractSyntax;
using Clasp.Data.VirtualMachine;

namespace Clasp.Exceptions
{
    public class InterpreterException : ClaspException
    {
        internal Stack<VmInstruction> ContinuationTrace;

        internal InterpreterException(MachineState machine, string format, params object?[] args)
            : base(format, args)
        {
            ContinuationTrace = machine.Continuation;
        }

        internal InterpreterException(MachineState machine, Exception innerException, string format, params object?[] args)
            : base(innerException, format, args)
        {
            ContinuationTrace = machine.Continuation;
        }

        public class InvalidBinding : InterpreterException
        {
            internal InvalidBinding(string varName, MachineState machine) : base(
                machine,
                "Unable to dereference binding of identifier '{0}'.",
                varName)
            { }
        }

        public class InvalidTopBinding : InterpreterException
        {
            internal InvalidTopBinding(string varName, MachineState machine) : base(
                machine,
                "Unable to dereference identifier '{0}' that hasn't yet been defined.",
                varName)
            { }
        }

        public class InvalidOperation : InterpreterException
        {
            internal InvalidOperation(VmInstruction operation, MachineState machine, Exception innerException) : base(
                machine,
                innerException,
                "Error evaluating the operation:\n\t{0}",
                operation)
            { }
        }

        public class ExceptionalSubProcess : InterpreterException
        {
            internal ExceptionalSubProcess(VmInstruction subProcessLauncher, MachineState machine, Exception innerException) : base(
                machine,
                innerException,
                "An error occurred while executing a sub-process prompted by {0}.",
                subProcessLauncher.ToString()
                )
            { }
        }
    }

}
