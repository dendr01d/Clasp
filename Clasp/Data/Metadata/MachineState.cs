using System.Collections.Generic;

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

        public MachineState(CoreForm program, Environment env)
        {
            ReturningValue = Undefined.Value;
            CurrentEnv = env;
            Continuation = new Stack<MxInstruction>();

            Continuation.Push(program);
        }
    }
}
