
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

        private Machine(AstNode evaluatee, Environment env)
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
                nextInstruction.RunOnMachine(_continuation, ref _currentEnv, ref _returnValue);
            }

            ++_instructionCounter;

            return !ComputationComplete;
        }

        public static Term Interpret(AstNode program, Environment env)
        {
            Machine mx = new Machine(program, env);

            while (mx.Step()) ;

            return mx._returnValue;
        }

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