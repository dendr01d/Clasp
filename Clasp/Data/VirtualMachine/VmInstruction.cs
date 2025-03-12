using System.Collections.Generic;
using System.Linq;

using Clasp.Binding.Environments;
using Clasp.Binding.Modules;
using Clasp.Data.Static;
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
        protected const string HOLE = "░░";

        public abstract string AppCode { get; }
        public abstract void RunOnMachine(MachineState machine);
        public abstract VmInstruction CopyContinuation();
        public sealed override string ToString() => string.Format("{0}({1})", AppCode, FormatArgs());
        protected abstract string FormatArgs();
        public string PrintAsStackFrame() => ToString();
        public string PrintAsStackFrame(int i) => string.Format("{0,3}: {1}", i, ToString());
    }

    #region Binding Operations

    /// <summary>
    /// Bind the returning value to the given name in the current environment.
    /// </summary>
    internal sealed class BindFresh : VmInstruction
    {
        private string _key;
        public override string AppCode => "BIND";
        public BindFresh(string key) : base()
        {
            _key = key;
        }
        public override void RunOnMachine(MachineState machine)
        {
            if (!machine.CurrentEnv.ContainsKey(_key))
            {
                machine.CurrentEnv.Define(_key, machine.ReturningValue);
                machine.ReturningValue = VoidTerm.Value;
            }
            else
            {
                throw new InterpreterException(machine, "Attempted to re-define existing binding of identifier '{0}'.", _key);
            }
        }
        public override VmInstruction CopyContinuation() => new BindFresh(_key);
        protected override string FormatArgs() => _key.ToString();
    }

    /// <summary>
    /// Rebind the returning value to the given name in the current environment.
    /// </summary>
    internal sealed class RebindExisting : VmInstruction
    {
        private string _key;
        public override string AppCode => "RBND";
        public RebindExisting(string key) => _key = key;
        public override void RunOnMachine(MachineState machine)
        {
            if (machine.CurrentEnv.ContainsKey(_key))
            {
                machine.CurrentEnv.Mutate(_key, machine.ReturningValue);
                machine.ReturningValue = VoidTerm.Value;
            }
            else
            {
                throw new InterpreterException(machine, "Attempted to mutate non-existent binding of identifier '{0}'.", _key);
            }
        }
        public override VmInstruction CopyContinuation() => new RebindExisting(_key);
        protected override string FormatArgs() => _key.ToString();
    }

    #endregion

    #region Branching

    /// <summary>
    /// Conditionally evaluate next either the consequent or alternate depending on the truthiness of the current value
    /// </summary>
    internal sealed class DispatchOnCondition : VmInstruction
    {
        private readonly CoreForm _consequent;
        private readonly CoreForm _alternative;
        public override string AppCode => "DSPX";
        public DispatchOnCondition(CoreForm consequent, CoreForm alternate) : base()
        {
            _consequent = consequent;
            _alternative = alternate;
        }
        public override void RunOnMachine(MachineState machine)
        {
            if (machine.ReturningValue == Boolean.False)
            {
                machine.Continuation.Push(_alternative);
            }
            else
            {
                machine.Continuation.Push(_consequent);
            }
        }

        public override VmInstruction CopyContinuation() => new DispatchOnCondition(_consequent, _alternative);
        protected override string FormatArgs() => string.Join(", ", _consequent, _alternative);
    }

    #endregion

    #region Function Application

    /// <summary>
    /// Checks the received term to verify that it's a procedure and that it's been invoked with the proper number of arguments.
    /// Creates a <see cref="FunctionArgs"/>.
    /// </summary>
    internal sealed class FunctionVerification : VmInstruction
    {
        private readonly CoreForm[] _arguments;
        private readonly CoreForm? _varArg;
        public override string AppCode => "FUN-CHK";
        public FunctionVerification(CoreForm[] arguments, CoreForm? varArg)
        {
            _arguments = arguments;
            _varArg = varArg;
        }
        public override void RunOnMachine(MachineState machine)
        {
            if (machine.ReturningValue is not Procedure proc)
            {
                throw new InterpreterException(machine, "Tried to perform function application using non-procedure operator: {0}", machine.ReturningValue);
            }
            else if (proc is MacroProcedure macro)
            {
                throw new InterpreterException(machine, "Tried to invoke macro at runtime: {0}", macro);
            }
            else if (proc is CompoundProcedure cp1 && _arguments.Length > cp1.Arity && !cp1.IsVariadic)
            {
                throw new InterpreterException(machine,
                    "Tried to invoke non-variadic compound procedure {0} with too many arguments: {1}",
                    cp1, string.Join(", ", _arguments.AsEnumerable()));
            }
            else if (proc is CompoundProcedure cp2 && _arguments.Length < cp2.Arity)
            {
                throw new InterpreterException(machine,
                    "Tried to invoke compound procedure {0} with invalid number ({1}) of argument/s: {2}",
                    cp2, _arguments.Length, string.Join(", ", _arguments.AsEnumerable()));
            }
            else if (_arguments.Length == 0)
            {
                machine.Continuation.Push(new FunctionDispatch(proc, []));
            }
            else
            {
                machine.Continuation.Push(new FunctionArgs(proc, _arguments, _varArg));
            }
        }

        public override VmInstruction CopyContinuation() => new FunctionVerification(
            _arguments.Select(x => x.CopyContinuation()).ToArray(), _varArg?.CopyContinuation());
        protected override string FormatArgs() => string.Join(", ", _arguments.Select(x => x.ToString()));
    }

    /// <summary>
    /// Iterates through the argument terms of a function application, evaluating each one individually in a random order.
    /// When all arguments have been evaluated, creates a <see cref="FunctionDispatch"/>.
    /// </summary>
    internal sealed class FunctionArgs : VmInstruction
    {
        private static readonly System.Random _rng = new System.Random();

        private readonly Procedure _op;
        private readonly CoreForm[] _rawArg; // Arguments that need to be evaluated
        private readonly CoreForm? _varArg;
        private readonly Term[] _evaluatedArgs; // Arguments that completed evaluation

        private readonly int[] EvaluationOrder; // The index order in which to evaluate arguments
        private int CurrentIndex; // The index^2 of the evaluated argument we expect to receive
        public override string AppCode => "FUN-ARGS";

        public FunctionArgs(Procedure op, CoreForm[] arguments, CoreForm? varArg)
        {
            _op = op;
            _rawArg = arguments;
            _varArg = varArg;

            _evaluatedArgs = Enumerable.Repeat<Term>(Undefined.Value, _rawArg.Length).ToArray();

            EvaluationOrder = Enumerable.Range(0, _rawArg.Length).ToArray();
            _rng.Shuffle(EvaluationOrder);

            CurrentIndex = -1;
        }

        private FunctionArgs(Procedure op, CoreForm[] rawArgs, Term[] doneArgs, int[] evalOrder, int currentIndex)
        {
            // All this stuff needs to be statically identical for continuations to work properly.
            _op = op;
            _rawArg = rawArgs.ToArray();
            _evaluatedArgs = doneArgs.ToArray();
            EvaluationOrder = evalOrder.ToArray();
            CurrentIndex = currentIndex;
        }

        public override void RunOnMachine(MachineState machine)
        {
            if (CurrentIndex >= 0)
            {
                _evaluatedArgs[EvaluationOrder[CurrentIndex]] = machine.ReturningValue;
            }

            ++CurrentIndex;

            if (CurrentIndex < _rawArg.Length)
            {
                CoreForm nextArg = _rawArg[EvaluationOrder[CurrentIndex]];

                if (nextArg.IsImperative)
                {
                    throw new InterpreterException(machine, "Illegal use of imperative form as a function argument: {0}", nextArg);
                }

                machine.Continuation.Push(this); // TODO verify it's safe to reuse the frame this way multiple times
                machine.Continuation.Push(new ChangeEnv(this, machine.CurrentEnv));
                machine.Continuation.Push(nextArg);
            }
            else
            {
                machine.Continuation.Push(new FunctionDispatch(_op, _evaluatedArgs));
            }
        }

        public override VmInstruction CopyContinuation() => new FunctionArgs(_op, _rawArg, _evaluatedArgs, EvaluationOrder, CurrentIndex);

        protected override string FormatArgs()
        {
            string[] output = new string[_rawArg.Length];

            for (int i = 0; i < EvaluationOrder.Length; ++i)
            {
                int randomIndex = EvaluationOrder[i];

                if (i < CurrentIndex)
                {
                    output[randomIndex] = _evaluatedArgs[randomIndex].ToString();
                }
                else if (i == CurrentIndex)
                {
                    output[randomIndex] = HOLE;
                }
                else
                {
                    output[randomIndex] = _rawArg[randomIndex].ToString();
                }
            }

            return string.Join(", ", output);
        }
    }

    internal sealed class FunctionVArg : VmInstruction
    {
        private static readonly System.Random _rng = new System.Random();

        private readonly Procedure _op;
        private readonly Term[] _normalArgs;
        private readonly List<Term> _varArgs;
        public override string AppCode => "FUN-VARG";

        public FunctionVArg(Procedure op, Term[] arguments)
        {
            _op = op;
            _normalArgs = arguments;
            _varArgs = new List<Term>();
        }

        public override void RunOnMachine(MachineState machine)
        {
            Term varArg = machine.ReturningValue;

            while (varArg is Cons cns)
            {
                _varArgs.Add(cns.Car);
                varArg = 
            }

            if (CurrentIndex >= 0)
            {
                _evaluatedArgs[EvaluationOrder[CurrentIndex]] = machine.ReturningValue;
            }

            ++CurrentIndex;

            if (CurrentIndex < _rawArg.Length)
            {
                CoreForm nextArg = _rawArg[EvaluationOrder[CurrentIndex]];

                if (nextArg.IsImperative)
                {
                    throw new InterpreterException(machine, "Illegal use of imperative form as a function argument: {0}", nextArg);
                }

                machine.Continuation.Push(this); // TODO verify it's safe to reuse the frame this way multiple times
                machine.Continuation.Push(new ChangeEnv(this, machine.CurrentEnv));
                machine.Continuation.Push(nextArg);
            }
            else
            {
                machine.Continuation.Push(new FunctionDispatch(_op, _evaluatedArgs));
            }
        }

        public override VmInstruction CopyContinuation() => new FunctionArgs(_op, _rawArg, _evaluatedArgs, EvaluationOrder, CurrentIndex);

        protected override string FormatArgs()
        {
            string[] output = new string[_rawArg.Length];

            for (int i = 0; i < EvaluationOrder.Length; ++i)
            {
                int randomIndex = EvaluationOrder[i];

                if (i < CurrentIndex)
                {
                    output[randomIndex] = _evaluatedArgs[randomIndex].ToString();
                }
                else if (i == CurrentIndex)
                {
                    output[randomIndex] = HOLE;
                }
                else
                {
                    output[randomIndex] = _rawArg[randomIndex].ToString();
                }
            }

            return string.Join(", ", output);
        }
    }

    internal sealed class FunctionDispatch : VmInstruction
    {
        private readonly Procedure _op;
        private readonly Term[] _arguments;
        public override string AppCode => "FUN-DSPX";
        public FunctionDispatch(Procedure op, params Term[] args)
        {
            _op = op;
            _arguments = args;
        }
        public override void RunOnMachine(MachineState machine)
        {
            if (_op is PrimitiveProcedure pp)
            {
                try
                {
                    machine.ReturningValue = pp.Operate(machine, _arguments);
                }
                catch (System.Exception ex)
                {
                    throw new InterpreterException.InvalidOperation(this, machine, ex);
                }
            }
            else if (_op is CompoundProcedure cp)
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
                    machine.Continuation.Push(new ConstValue(_arguments[i]));
                }

                if (cp.VariadicParameter is not null)
                {
                    machine.Continuation.Push(new BindFresh(cp.VariadicParameter));
                    machine.Continuation.Push(_arguments.Length > cp.Parameters.Length
                        ? new ConstValue(Cons.ProperList(_arguments[i..]))
                        : new ConstValue(Nil.Value));
                }

                machine.Continuation.Push(new ChangeEnv(this, cp.GetClosure()));
            }
            else 
            {
                throw new InterpreterException(machine, "Tried to dispatch on unknown procedure type(!?): {0}", _op);
            }
        }
        public override VmInstruction CopyContinuation() => new FunctionDispatch(_op, _arguments.ToArray());
        protected override string FormatArgs() => string.Join(", ", _op.ToString(), string.Join(", ", _arguments.Select(x => x.ToString())));
    }

    #endregion

    internal sealed class ChangeEnv : VmInstruction
    {
        public readonly string Prompter;
        public readonly MutableEnv NewEnv;
        public override string AppCode => "ENV";
        public ChangeEnv(VmInstruction prompter, MutableEnv newEnv) : this(prompter.AppCode, newEnv) { }
        private ChangeEnv(string prompter, MutableEnv newEnv)
        {
            Prompter = prompter;
            NewEnv = newEnv;
        }
        public override void RunOnMachine(MachineState machine)
        {
            machine.CurrentEnv = NewEnv;
        }
        public override VmInstruction CopyContinuation() => new ChangeEnv(Prompter, NewEnv);
        protected override string FormatArgs() => Prompter;
    }

    //internal sealed class CacheEnvAsModule : VmInstruction
    //{
    //    private readonly Symbol _key;
    //    private readonly string[] _exportedNames;
    //    public override string AppCode => "INSTL";
    //    public CacheEnvAsModule(Symbol key, string[] exportedNames)
    //    {
    //        _key = key;
    //        _exportedNames = exportedNames;
    //    }
    //    public override void RunOnMachine(MachineState machine)
    //    {
    //        StaticEnv.CacheModule(new Binding.Module(_key.Name, machine.CurrentEnv.Root, _exportedNames));
    //        machine.ReturningValue = VoidTerm.Value;
    //    }
    //    public override CacheEnvAsModule CopyContinuation() => new CacheEnvAsModule(_key, _exportedNames);
    //    protected override string FormatArgs() => string.Join(", ", _key, string.Join(", ", _exportedNames));
    //}

    internal sealed class InstallModule : VmInstruction
    {
        private readonly string _moduleName;
        public override string AppCode => "INST";
        public InstallModule(string mdlName) => _moduleName = mdlName;
        public override void RunOnMachine(MachineState machine)
        {
            Module.Instantiate(_moduleName, machine.PostStepHook);

            if (ModuleCache.TryGet(_moduleName, out Module? mdl)
                && mdl is InstantiatedModule instModule)
            {
                machine.CurrentEnv.Root.InstallModule(instModule);
                machine.ReturningValue = VoidTerm.Value;
            }
            else
            {
                throw new InterpreterException(machine, "Module '{0}' failed to instantiate.", _moduleName);
            }
        }
        public override InstallModule CopyContinuation() => new InstallModule(_moduleName);
        protected override string FormatArgs() => _moduleName;
    }
}
