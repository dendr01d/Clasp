using System.Diagnostics.Metrics;
using System.Linq;

using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;
using Clasp.Data.VirtualMachine;

namespace Clasp.Process
{
    internal static class Interpreter
    {
        public static Term InterpretProgram(CoreForm program, ClaspEnvironment env)
        {
            MachineState machine = new MachineState(program, env);
            RunToCompletion(machine);
            return machine.ReturningValue;
        }

        public static Term InterpretInVacuum(CoreForm program) => InterpretProgram(program, new RootEnv());

        public static Term InterpretProgram(MacroApplication macroApp) => InterpretProgram(macroApp, macroApp.Macro.CapturedEnv);

        public static Term Interpret(CoreForm program, ClaspEnvironment env, System.Action<int, MachineState> postStepHook)
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
                VmInstruction nextInstruction = machine.Continuation.Pop();

                nextInstruction.RunOnMachine(machine);
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

                VmInstruction nextInstruction = outputState.Continuation.Pop();

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

            foreach (VmInstruction frame in machine.Continuation.Reverse())
            {
                frame.PrintAsStackFrame(sw, counter++);
            }

            sw.Write("   └─> {0}", machine.ReturningValue);
        }
    }
}
