using System.Collections.Generic;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;

namespace Clasp.Data.Metadata
{
    internal class MachineState
    {
        public Term ReturningValue;
        public Environment CurrentEnv;
        public Stack<MxInstruction> Continuation;

        public bool Complete => Continuation.Count == 0;

        /// <summary>
        /// Create a deep copy of the machine's <see cref="Continuation"/>.
        /// </summary>
        public Stack<MxInstruction> CopyContinuation()
        {
            IEnumerable<MxInstruction> copiedInstructions = Continuation
                .Select(x => x.CopyContinuation())
                .Reverse();

            return new Stack<MxInstruction>(copiedInstructions);
        }

        public MachineState(CoreForm program, Environment env)
        {
            ReturningValue = Undefined.Value;
            CurrentEnv = env;
            Continuation = new Stack<MxInstruction>();

            Continuation.Push(program);
        }

        public MachineState(MachineState machine)
        {
            ReturningValue = machine.ReturningValue;
            CurrentEnv = machine.CurrentEnv; // TODO: is this how call/cc works? do the threads share environment?
            Continuation = machine.CopyContinuation();
        }
    }
}
