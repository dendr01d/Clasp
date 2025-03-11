using System;
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
        public int Phase;
        public Term ReturningValue;
        public MutableEnv CurrentEnv;
        public Stack<VmInstruction> Continuation;

        public readonly Action<int, MachineState>? PostStepHook;

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

        public MachineState(CoreForm program, MutableEnv env, Action<int, MachineState>? postStepHook = null)
        {
            ReturningValue = VoidTerm.Value;
            CurrentEnv = env;
            Continuation = new Stack<VmInstruction>();
            PostStepHook = postStepHook;

            Continuation.Push(program);
        }

        public MachineState(MachineState machine)
        {
            ReturningValue = machine.ReturningValue;
            CurrentEnv = machine.CurrentEnv; // TODO: is this how call/cc works? do the threads share environment?
            Continuation = machine.CopyContinuation();
            PostStepHook = machine.PostStepHook;
        }
    }
}
