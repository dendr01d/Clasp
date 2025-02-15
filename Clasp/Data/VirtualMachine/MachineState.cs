using System.Collections.Generic;
using System.Linq;

using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;
using Clasp.Process;

namespace Clasp.Data.VirtualMachine
{
    internal class MachineState
    {
        public readonly Processor ParentProcess;
        public Term ReturningValue;
        public Environment CurrentEnv;
        public Stack<VmInstruction> Continuation;

        public bool Complete => Continuation.Count == 0;

        /// <summary>
        /// Create a deep copy of the machine's <see cref="Continuation"/>.
        /// </summary>
        public Stack<VmInstruction> CopyContinuation()
        {
            IEnumerable<VmInstruction> copiedInstructions = Continuation
                .Select(x => x.CopyContinuation())
                .Reverse();

            return new Stack<VmInstruction>(copiedInstructions);
        }

        public MachineState(CoreForm program, Processor pross, Environment env)
        {
            ParentProcess = pross;
            ReturningValue = VoidTerm.Value;
            CurrentEnv = env;
            Continuation = new Stack<VmInstruction>();

            Continuation.Push(program);
        }

        public MachineState(CoreForm program, Processor pross)
            : this(program, pross, pross.RuntimeEnv)
        { }


        public MachineState(MachineState machine)
        {
            ParentProcess = machine.ParentProcess;
            ReturningValue = machine.ReturningValue;
            CurrentEnv = machine.CurrentEnv; // TODO: is this how call/cc works? do the threads share environment?
            Continuation = machine.CopyContinuation();
        }
    }
}
