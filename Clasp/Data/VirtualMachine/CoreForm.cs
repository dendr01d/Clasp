using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Data.VirtualMachine;
using Clasp.Process;

namespace Clasp.Data.AbstractSyntax
{
    /// <summary>
    /// The core forms of the Scheme being described. All programs are represented in terms of
    /// these objects, and all source code must be parsed to a tree of these values.
    /// </summary>
    internal abstract class CoreForm : VmInstruction
    {
        protected CoreForm() : base() { }

        public virtual bool IsImperative { get; } = false;

        public abstract string FormName { get; }
        public abstract Term ToTerm();
    }

    #region Imperative Effects

    internal sealed class BindingDefinition : CoreForm
    {
        public string VarName { get; private init; }
        public CoreForm BoundValue { get; private init; }
        public override bool IsImperative => true;
        public override string FormName => nameof(BindingDefinition);
        public BindingDefinition(string key, CoreForm value) : base()
        {
            VarName = key;
            BoundValue = value;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new BindFresh(VarName));
            continuation.Push(BoundValue);
        }
        public override VmInstruction CopyContinuation() => new BindingDefinition(VarName, BoundValue);
        public override string ToString() => string.Format("DEF({0}, {1})", VarName, BoundValue);
        public override Term ToTerm() => Cons.ProperList(Symbol.Define, Symbol.Intern(VarName), BoundValue.ToTerm());
    }

    internal sealed class BindingMutation : CoreForm
    {
        public string VarName { get; private init; }
        public CoreForm BoundValue { get; private init; }
        public override bool IsImperative => true;
        public override string FormName => nameof(BindingMutation);
        public BindingMutation(string name, CoreForm bound) : base()
        {
            VarName = name;
            BoundValue = bound;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new RebindExisting(VarName));
            continuation.Push(BoundValue);
        }
        public override VmInstruction CopyContinuation() => new BindingMutation(VarName, BoundValue);
        public override string ToString() => string.Format("SET({0}, {1})", VarName, BoundValue);
        public override Term ToTerm() => Cons.ProperList(Symbol.Set, Symbol.Intern(VarName), BoundValue.ToTerm());
    }

    #endregion

    #region Expression Types

    internal sealed class VariableLookup : CoreForm
    {
        public string VarName { get; private init; }
        public override string FormName => nameof(VariableLookup);
        public VariableLookup(string key) : base()
        {
            VarName = key;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
        {
            if (currentEnv.TryGetValue(VarName, out Term? boundValue))
            {
                currentValue = boundValue;
            }
            else
            {
                throw new InterpreterException.InvalidBinding(VarName, continuation);
            }
        }
        public override VmInstruction CopyContinuation() => new VariableLookup(VarName);
        public override string ToString() => string.Format("VAR({0})", VarName);
        public override Term ToTerm() => Symbol.Intern(VarName);
    }

    internal sealed class ConstValue : CoreForm
    {
        public Term Value { get; private init; }
        public override string FormName => nameof(ConstValue);

        public ConstValue(Term value) : base()
        {
            Value = value;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
        {
            currentValue = Value;
        }
        public override VmInstruction CopyContinuation() => new ConstValue(Value);
        public override string ToString() => Value is Terms.Atom
            ? Value.ToString()
            : string.Format("QUOTE({0})", Value);
        public override Term ToTerm() => Cons.ProperList(Symbol.Quote, Value);
    }

    #endregion

    #region Execution Path

    internal sealed class ConditionalForm : CoreForm
    {
        public readonly CoreForm Test;
        public readonly CoreForm Consequent;
        public readonly CoreForm Alternate;
        public override string FormName => nameof(ConditionalForm);
        public ConditionalForm(CoreForm test, CoreForm consequent, CoreForm alternate)
        {
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new DispatchOnCondition(Consequent, Alternate));
            continuation.Push(new ChangeCurrentEnvironment(currentEnv));
            continuation.Push(Test);
        }
        public override VmInstruction CopyContinuation() => new ConditionalForm(Test, Consequent, Alternate);
        public override string ToString() => string.Format("BRANCH({0}, {1}, {2})", Test, Consequent, Alternate);

        public override Term ToTerm() => Cons.ProperList(Symbol.If,
            Test.ToTerm(), Consequent.ToTerm(), Alternate.ToTerm());
    }

    internal sealed class SequentialForm : CoreForm
    {
        public readonly CoreForm[] Sequence;
        public override string FormName => nameof(Sequence);
        public SequentialForm(CoreForm[] series)
        {
            Sequence = series;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
        {
            foreach (CoreForm node in Sequence.Reverse())
            {
                continuation.Push(node);
                continuation.Push(new ChangeCurrentEnvironment(currentEnv));
            }
        }
        public override VmInstruction CopyContinuation() => new SequentialForm(Sequence);
        public override string ToString() => string.Format("SEQ({0})", string.Join(", ", Sequence.ToArray<object>()));

        public override Term ToTerm() => Cons.Truct(Symbol.Begin, ToImplicitTerm());
        public Term ToImplicitTerm() => Cons.ProperList(Sequence.Select(x => x.ToTerm()).ToArray());
    }

    //internal sealed class TopLevelSequentialForm : CoreForm
    //{
    //    public readonly CoreForm[] Sequence;
    //    public override string FormName => nameof(TopLevelSequentialForm);
    //    public TopLevelSequentialForm(CoreForm[] series)
    //    {
    //        Sequence = series;
    //    }
    //    protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
    //    {
    //        if (Sequence.Length > 1)
    //        {
    //            continuation.Push(new TopLevelSequentialForm(Sequence[1..]));
    //        }
    //        continuation.Push(Sequence[0]);
    //    }
    //    public override VmInstruction CopyContinuation() => new TopLevelSequentialForm(Sequence);
    //    public override string ToString() => string.Format("SEQ-D({0})", string.Join(", ", Sequence.ToArray<object>()));

    //    public override Term ToTerm() => Pair.Cons(Symbol.Begin,
    //        Cons.ProperList(Sequence.Select(x => x.ToTerm()).ToArray()));
    //}

    //internal sealed class CallWithCurrentContinuation : AstNode
    //{
    //    // needs to copy the continuation of the runtime
    //    // but skipping over any top-level forms at the bottom of the stack
    //}

    #endregion

    #region Functional Expressions

    internal sealed class FunctionApplication : CoreForm
    {
        public readonly CoreForm Operator;
        public readonly CoreForm[] Arguments;

        public override string FormName => nameof(FunctionApplication);

        public FunctionApplication(CoreForm op, CoreForm[] args) : base()
        {
            Operator = op;
            Arguments = args;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new FunctionVerification(Arguments));
            //continuation.Push(new RollUpArguments(Arguments));
            continuation.Push(new ChangeCurrentEnvironment(currentEnv));
            continuation.Push(Operator);
        }

        public override VmInstruction CopyContinuation() => new FunctionApplication(Operator, Arguments);

        public override string ToString() => string.Format(
            "APPL({0}; {1})",
            Operator,
            string.Join(", ", Arguments.ToArray<object>()));

        public override Term ToTerm() => Cons.Truct(
            Operator.ToTerm(),
            Cons.ProperList(Arguments.Select(x => x.ToTerm()).ToArray()));
    }

    internal sealed class FunctionCreation : CoreForm
    {
        public readonly string[] Formals;
        public readonly string? DottedFormal;
        public readonly string[] Informals;
        public readonly SequentialForm Body;
        public override string FormName => nameof(FunctionCreation);

        public FunctionCreation(string[] parameters, string? dottedParameter, string[] internalKeys, SequentialForm body)
        {
            Formals = parameters;
            DottedFormal = dottedParameter;
            Informals = internalKeys;
            Body = body;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
        {
            currentValue = new CompoundProcedure(Formals, DottedFormal, Informals, currentEnv, Body);
        }
        public override VmInstruction CopyContinuation() => new FunctionCreation(Formals, DottedFormal, Informals, Body);

        public override string ToString() => string.Format("FUN({0}{1}; {2})",
            string.Join(", ", Formals.ToArray<object>()),
            DottedFormal is null ? string.Empty : string.Format("; {0}", DottedFormal),
            string.Join(", ", Body.Sequence.ToArray<object>()));

        public override Term ToTerm()
        {
            Term[] parameters = Formals.Append(DottedFormal)
                .Select<string?, Term>(x => x is null ? Nil.Value : Symbol.Intern(x))
                .ToArray();

            return Cons.ImproperList(Symbol.Lambda, Cons.ImproperList(parameters), Body.ToImplicitTerm());
        }
    }

    #endregion

    #region Macro Expressions

    internal sealed class MacroApplication : CoreForm
    {
        public readonly MacroProcedure Macro;
        public readonly Syntax Argument;
        public override string FormName => nameof(MacroApplication);

        public MacroApplication(MacroProcedure macro, Syntax arg)
        {
            Macro = macro;
            Argument = arg;
        }
        public override void RunOnMachine(MachineState machine)
            => RunOnMachine(machine.Continuation, ref machine.CurrentEnv, ref machine.ReturningValue);
        protected override void RunOnMachine(Stack<VmInstruction> continuation, ref Binding.Environments.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new FunctionDispatch(Macro, Argument));
        }
        public override VmInstruction CopyContinuation() => new MacroApplication(Macro, Argument);
        public override string ToString() => string.Format("MACRO-APPL({0}; {1})", Macro, Argument);

        public override Term ToTerm() => Cons.ProperList<Term>(Macro, Argument);
    }

    #endregion
}
