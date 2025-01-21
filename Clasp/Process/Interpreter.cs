using System.Diagnostics.Metrics;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Process
{
    internal static class Interpreter
    {
        public static Term InterpretProgram(CoreForm program, Environment env)
        {
            MachineState machine = new MachineState(program, env);
            RunToCompletion(machine);
            return machine.ReturningValue;
        }

        public static Term Interpret(CoreForm program, Environment env, System.Action<int, MachineState> postStepHook)
        {
            MachineState machine = new MachineState(program, env);
            RunToCompletion(machine, postStepHook);
            return machine.ReturningValue;
        }

        public static Term Interpret(MacroApplication macroAppl) => InterpretProgram(macroAppl, macroAppl.Macro.CapturedEnv);

        public static MachineState InterpretToCompletion(MachineState machine)
        {
            RunToCompletion(machine);
            return machine;
        }

        // ---

        private static bool Step(MachineState machine)
        {
            if (!machine.Complete)
            {
                MxInstruction nextInstruction = machine.Continuation.Pop();

                nextInstruction.RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
            }

            return !machine.Complete;
        }

        private static MachineState StepPurely(MachineState machine)
        {
            if (machine.Complete)
            {
                return machine;
            }
            else
            {
                MachineState outputState = new MachineState(machine);

                MxInstruction nextInstruction = outputState.Continuation.Pop();

                nextInstruction.RunOnMachine(outputState);

                return outputState;
            }
        }

        private static void RunToCompletion(MachineState machine)
        {
            while (Step(machine)) ;
        }

        private static void RunToCompletion(MachineState machine, System.Action<int, MachineState> postStepHook)
        {
            int counter = 0;

            while (!machine.Complete)
            {
                Step(machine);
                postStepHook(++counter, machine);
            }
        }

        public static void PrintMachineState(MachineState machine, System.IO.StreamWriter sw)
        {
            sw.WriteLine("<──┐ [RETURN]");

            int counter = 1;

            foreach (MxInstruction frame in machine.Continuation.Reverse())
            {
                frame.PrintAsStackFrame(sw, counter++);
            }

            sw.Write("   └─> {0}", machine.ReturningValue);
        }
    }
}
