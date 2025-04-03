using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;

using Clasp.AbstractMachine;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;

namespace Clasp.Process
{
    internal static class Interpreter
    {
        public static Term InterpretProgram(CoreForm program, MutableEnv env, System.Action<int, MachineState>? postStepHook = null)
        {
            MachineState machine = new MachineState(program, env, postStepHook);
            RunToCompletion(machine);
            return machine.ReturningValue;
        }

        public static Term InterpretInVacuum(CoreForm program) => InterpretProgram(program, new RootEnv());

        public static Term InterpretApplication(Procedure proc, Term[] args, RootEnv env)
        {
            Application program = new Application(new ConstValue(proc), args.Select(x => new ConstValue(x)).ToArray());
            return InterpretProgram(program, env);
        }

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
            int counter = 0;

            while (!machine.Complete)
            {
                Step(machine);
                machine.PostStepHook?.Invoke(counter++, machine);
            }
        }

        public static string PrintMachineState(MachineState machine)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<──┐ [RETURN]");

            int counter = 1;

            foreach (VmInstruction frame in machine.Continuation.Reverse())
            {
                sb.AppendLine(frame.PrintAsStackFrame(counter++));
            }

            sb.Append(string.Format("   └─> {0}", machine.ReturningValue));

            return sb.ToString();
        }
    }
}
