
using System.Collections.Generic;
using System.IO;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;

namespace Clasp
{
    internal class Machine
    {
        private Term _returnValue;
        private Environment _currentEnv;

        private readonly Stack<EvFrame> _continuation;

        private int _instructionCounter;

        public bool ComputationComplete => _continuation.Count == 0;

        private Machine(AstNode evaluatee, EnvFrame env)
        {
            _returnValue = Undefined.Value;
            _currentEnv = env;

            _continuation = new Stack<EvFrame>();

            _instructionCounter = 0;

            _continuation.Push(evaluatee);
        }

        public bool Step()
        {
            if (!ComputationComplete)
            {
                EvFrame nextInstruction = _continuation.Pop();

                DispatchOnInstruction(nextInstruction);
            }

            ++_instructionCounter;

            return !ComputationComplete;
        }

        public Term Compute()
        {
            while (Step()) { }

            return _returnValue;
        }

        #region Instruction Dispatch

        private void DispatchOnInstruction(EvFrame instr)
        {
            instr.RunOnMachine(_continuation, ref _currentEnv, ref _returnValue);
        }

        #endregion

        #region Printing

        //private const string STACK_SEP = "≤";
        //private const string EMPTY_PTR = "ε";

        public void Print(TextWriter tw)
        {
            foreach (EvFrame instr in _continuation)
            {
                tw.WriteLine(instr);
            }


        }

        #endregion
    }
}