﻿using System.Collections.Generic;
using System.Linq;

using Clasp.Binding.Environments;
using Clasp.Binding.MutableEnvs;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.VirtualMachine;
using Clasp.Exceptions;

namespace Clasp.Data.AbstractSyntax
{
    /// <summary>
    /// Represents instructional forms that act upon a <see cref="VirtualMachine.MachineState"/>
    /// -- and in turn can be "interpreted" by the <see cref="Process.Interpreter"/>.
    /// These consist of <see cref="CoreForm"/> objects and tertiary supporting instructions.
    /// </summary>
    internal abstract class VmInstruction
    {
        protected const string HOLE = "[_]";

        protected virtual void RunOnMachine(
            Stack<VmInstruction> continuation,
            ref MutableEnv currentEnv,
            ref Term currentValue)
        { }

        public abstract void RunOnMachine(MachineState machine);

        public abstract VmInstruction CopyContinuation();

        public abstract override string ToString();

        public void PrintAsStackFrame(System.IO.StreamWriter sw) => sw.WriteLine(ToString());
        public void PrintAsStackFrame(System.IO.StreamWriter sw, int i)
        {
            sw.Write("{0,3}: ", i);
            PrintAsStackFrame(sw);
        }
    }

    #region Binding Operations

    /// <summary>
    /// Bind the return value to the given name in the current MutableEnv.
    /// </summary>
    internal sealed class BindFresh : VmInstruction
    {
        public string VarName { get; private init; }
        public BindFresh(string key) : base()
        {
            VarName = key;
        }

        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref MutableEnv currentEnv, ref Term currentValue)
        {
            if (!currentEnv.TryGetValue(VarName, out Term? def) || def is Undefined)
            {
                currentEnv[VarName] = currentValue;
                currentValue = VoidTerm.Value;
            }
            else
            {
                throw new InterpreterException(continuation, "Attempted to re-define existing binding of identifier '{0}'.", VarName);
            }
        }
        public override VmInstruction CopyContinuation() => new BindFresh(VarName);
        public override string ToString() => string.Format("*DEF({0}, {1})", VarName, HOLE);
    }

    /// <summary>
    /// Rebind the return value to the given name in the current MutableEnv.
    /// </summary>
    internal sealed class RebindExisting : VmInstruction
    {
        public string VarName { get; private init; }
        public RebindExisting(string key) : base()
        {
            VarName = key;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref MutableEnv currentEnv, ref Term currentValue)
        {
            if (currentEnv.ContainsKey(VarName))
            {
                currentEnv[VarName] = currentValue;
                currentValue = VoidTerm.Value;
            }
            else
            {
                throw new InterpreterException(continuation, "Attempted to change value of non-existent binding of identifier '{0}'.", VarName);
            }
        }
        public override VmInstruction CopyContinuation() => new RebindExisting(VarName);
        public override string ToString() => string.Format("*SET({0}, {1})", VarName, HOLE);

    }

    #endregion

    #region Branching

    /// <summary>
    /// Conditionally evaluate next either the consequent or alternate depending on the truthiness of the current value
    /// </summary>
    internal sealed class DispatchOnCondition : VmInstruction
    {
        public readonly CoreForm Consequent;
        public readonly CoreForm Alternate;
        public DispatchOnCondition(CoreForm consequent, CoreForm alternate) : base()
        {
            Consequent = consequent;
            Alternate = alternate;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref MutableEnv currentEnv, ref Term currentValue)
        {
            if (currentValue == Boolean.False)
            {
                continuation.Push(Alternate);
            }
            else
            {
                continuation.Push(Consequent);
            }
        }
        public override VmInstruction CopyContinuation() => new DispatchOnCondition(Consequent, Alternate);
        public override string ToString() => string.Format("*BRANCH({0}, {1}, {2})", HOLE, Consequent, Alternate);
    }

    #endregion

    #region Function Application

    /// <summary>
    /// Checks the received term to verify that it's a procedure and that it's been invoked with the proper number of arguments.
    /// Creates a <see cref="FunctionArgs"/>.
    /// </summary>
    internal sealed class FunctionVerification : VmInstruction
    {
        public readonly CoreForm[] Arguments;

        public FunctionVerification(CoreForm[] arguments) => Arguments = arguments;
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref MutableEnv currentEnv, ref Term currentValue)
        {
            if (currentValue is not Procedure proc)
            {
                throw new InterpreterException(continuation, "Tried to perform function application using non-procedure operator: {0}", currentValue);
            }
            else if (currentValue is MacroProcedure macro)
            {
                throw new InterpreterException(continuation, "Tried to invoke macro at runtime: {0}", macro);
            }
            else if (proc is CompoundProcedure cp1 && Arguments.Length > cp1.Arity && !cp1.IsVariadic)
            {
                throw new InterpreterException(continuation,
                    "Tried to invoke non-variadic compound procedure {0} with too many arguments: {1}",
                    cp1, string.Join(", ", Arguments.AsEnumerable()));
            }
            else if (proc is CompoundProcedure cp2 && Arguments.Length < cp2.Arity)
            {
                throw new InterpreterException(continuation,
                    "Tried to invoke compound procedure {0} with invalid number ({1}) of argument/s: {2}",
                    cp2, Arguments.Length, string.Join(", ", Arguments.AsEnumerable()));
            }
            else if (Arguments.Length == 0)
            {
                continuation.Push(new FunctionDispatch(proc, System.Array.Empty<Term>()));
            }
            else
            {
                continuation.Push(new FunctionArgs(proc, Arguments));
            }
        }
        public override VmInstruction CopyContinuation() => new FunctionVerification(Arguments.ToArray());
        public override string ToString() => string.Format("APPL-VERIF({0}; {1})", HOLE, string.Join(", ", Arguments.AsEnumerable()));
    }

    /// <summary>
    /// Iterates through the argument terms of a function application, evaluating each one individually in a random order.
    /// When all arguments have been evaluated, creates a <see cref="FunctionDispatch"/>.
    /// </summary>
    internal sealed class FunctionArgs : VmInstruction
    {
        private static readonly System.Random _rng = new System.Random();

        public readonly Procedure Op;
        public readonly CoreForm[] RawArguments; // Arguments that need to be evaluated
        public readonly Term[] EvaluatedArguments; // Arguments that completed evaluation

        private readonly int[] EvaluationOrder; // The index order in which to evaluate arguments
        private int CurrentIndex; // The index^2 of the evaluated argument we expect to receive

        public FunctionArgs(Procedure op, CoreForm[] arguments)
        {
            Op = op;
            RawArguments = arguments;
            EvaluatedArguments = Enumerable.Repeat<Term>(Undefined.Value, RawArguments.Length).ToArray();

            EvaluationOrder = Enumerable.Range(0, RawArguments.Length).ToArray();
            _rng.Shuffle(EvaluationOrder);

            CurrentIndex = -1;
        }

        private FunctionArgs(Procedure op, CoreForm[] rawArgs, Term[] doneArgs, int[] evalOrder, int currentIndex)
        {
            // All this stuff needs to be statically identical for continuations to work properly.
            Op = op;
            RawArguments = rawArgs.ToArray();
            EvaluatedArguments = doneArgs.ToArray();
            EvaluationOrder = evalOrder.ToArray();
            CurrentIndex = currentIndex;
        }

        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref MutableEnv currentEnv, ref Term currentValue)
        {
            if (CurrentIndex >= 0)
            {
                EvaluatedArguments[EvaluationOrder[CurrentIndex]] = currentValue as Term;
            }

            ++CurrentIndex;

            if (CurrentIndex < RawArguments.Length)
            {
                CoreForm nextArg = RawArguments[EvaluationOrder[CurrentIndex]];

                // Could do some type-checking here, eg:
                if (nextArg is BindingDefinition or BindingMutation)
                {
                    throw new InterpreterException(continuation, "Illegal to use this expression type as a function argument: {0}", nextArg);
                }

                continuation.Push(this); // is it safe to reuse a mutable evaluation frame multiple times ...?
                continuation.Push(new ChangeCurrentEnvironment(currentEnv));
                continuation.Push(nextArg);
            }
            else
            {
                continuation.Push(new FunctionDispatch(Op, EvaluatedArguments));
            }
        }

        public override VmInstruction CopyContinuation() => new FunctionArgs(Op, RawArguments, EvaluatedArguments, EvaluationOrder, CurrentIndex);

        public override string ToString() => string.Format("APPL-ARGS({0}; {1})", Op, string.Join(", ", BuildArgsList()));

        private string[] BuildArgsList()
        {
            string[] output = new string[RawArguments.Length];

            for (int i = 0; i < EvaluationOrder.Length; ++i)
            {
                int randomIndex = EvaluationOrder[i];

                if (i < CurrentIndex)
                {
                    output[randomIndex] = EvaluatedArguments[randomIndex].ToString();
                }
                else if (i == CurrentIndex)
                {
                    output[randomIndex] = HOLE;
                }
                else
                {
                    output[randomIndex] = RawArguments[randomIndex].ToString();
                }
            }

            return output;
        }
    }

    internal sealed class FunctionDispatch : VmInstruction
    {
        public readonly Procedure Op;
        public readonly Term[] Arguments;

        public FunctionDispatch(Procedure op, params Term[] args)
        {
            Op = op;
            Arguments = args;
        }

        public override void RunOnMachine(MachineState machine)
        {
            if (Op is SystemProcedure sp)
            {
                try
                {
                    machine.ReturningValue = sp.Operate(machine, Arguments);
                }
                catch (System.Exception ex)
                {
                    throw new InterpreterException.InvalidOperation(this, machine.Continuation, ex);
                }
            }
            else if (Op is NativeProcedure pp)
            {
                try
                {
                    machine.ReturningValue = pp.Operate(Arguments);
                }
                catch (System.Exception ex)
                {
                    throw new InterpreterException.InvalidOperation(this, machine.Continuation, ex);
                }
            }
            else if (Op is CompoundProcedure cp)
            {
                machine.Continuation.Push(cp.Body);

                foreach(string informal in cp.InformalParameters)
                {
                    machine.Continuation.Push(new BindFresh(informal));
                    machine.Continuation.Push(new ConstValue(Undefined.Value));
                }

                int i = 0;
                for (; i < cp.Parameters.Length; ++i )
                {
                    machine.Continuation.Push(new BindFresh(cp.Parameters[i]));
                    machine.Continuation.Push(new ConstValue(Arguments[i]));
                }

                if (cp.VariadicParameter is not null)
                {
                    machine.Continuation.Push(new BindFresh(cp.VariadicParameter));
                    machine.Continuation.Push(Arguments.Length > cp.Parameters.Length
                        ? new ConstValue(Cons.ProperList(Arguments[i..]))
                        : new ConstValue(Nil.Value));
                }

                machine.Continuation.Push(new ChangeCurrentMutableEnv(cp.CapturedEnv));
            }
            else 
            {
                throw new InterpreterException(machine.Continuation, "Tried to dispatch on unknown procedure type(!?): {0}", Op);
            }
        }
        public override VmInstruction CopyContinuation() => new FunctionDispatch(Op, Arguments.ToArray());
        public override string ToString() => string.Format("APPL-DSPX({0}; {1})", Op, string.Join(", ", Arguments.ToArray<object>()));
    }

    #endregion

    #region Module Operations

    internal sealed class ModuleInstallation : VmInstruction
    {
        public readonly string Name;
        public readonly string[] ExportedNames;

        public ModuleInstallation(string name, string[] exportedNames)
        {
            Name = name;
            ExportedNames = exportedNames;
        }

        public override void RunOnMachine(MachineState machine)
        {
            MutableEnv moduleEnv = machine.CurrentEnv.GlobalEnv.InstallNewModuleEnv(Name);
            MutableEnv defEnv = machine.CurrentEnv;

            machine.Continuation.Push(new ConstValue(VoidTerm.Value));

            // look up the exported value from the definition MutableEnv, then bind it in the module MutableEnv
            foreach(string export in ExportedNames)
            {
                machine.Continuation.Push(new BindFresh(export));
                machine.Continuation.Push(new ChangeCurrentEnvironment(moduleEnv));
                machine.Continuation.Push(new VariableLookup(export));
                machine.Continuation.Push(new ChangeCurrentEnvironment(defEnv));
            }
        }
        public override ModuleInstallation CopyContinuation() => new ModuleInstallation(Name, ExportedNames);
        public override string ToString() => string.Format("MDL-INSTL({0}; {1})", Name, string.Join(", ", ExportedNames));
    }

    #endregion

    internal sealed class ChangeCurrentEnvironment : VmInstruction
    {
        public readonly string Prompter;
        public readonly MutableEnv NewEnv;
        public ChangeCurrentEnvironment(string prompter, MutableEnv newEnv)
        {
            Prompter = prompter;
            NewEnv = newEnv;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref MutableEnv currentEnv, ref Term currentValue)
        {
            currentEnv = NewEnv;
        }
        public override VmInstruction CopyContinuation() => new ChangeCurrentEnvironment(Prompter, NewEnv);
        public override string ToString() => string.Format("MOD-ENV({0})", Prompter);
    }
}
