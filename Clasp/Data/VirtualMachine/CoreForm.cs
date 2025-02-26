using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Clasp.Binding.Environments;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Data.VirtualMachine;
using Clasp.Exceptions;
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
        {
            machine.Continuation.Push(new BindFresh(VarName));
            machine.Continuation.Push(BoundValue);
        }

        public override VmInstruction CopyContinuation() => new BindingDefinition(VarName, BoundValue);
        public override string ToString() => string.Format("DEF({0}, {1})", VarName, BoundValue);
        public override Term ToTerm() => Cons.ProperList(Symbols.Define, Symbol.Intern(VarName), BoundValue.ToTerm());
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
        {
            machine.Continuation.Push(new RebindExisting(VarName));
            machine.Continuation.Push(BoundValue);
        }

        public override VmInstruction CopyContinuation() => new BindingMutation(VarName, BoundValue);
        public override string ToString() => string.Format("SET({0}, {1})", VarName, BoundValue);
        public override Term ToTerm() => Cons.ProperList(Symbols.Set, Symbol.Intern(VarName), BoundValue.ToTerm());
    }

    //internal sealed class Importation : CoreForm
    //{
    //    public readonly string FilePath;
    //    public override bool IsImperative => true;
    //    public override string FormName => nameof(Importation);
    //    public Importation(string path) : base()
    //    {
    //        FilePath = path;
    //    }
    //    public override void RunOnMachine(MachineState machine)
    //    {
    //        CoreForm importedProgram;

    //        try
    //        {
    //            Processor pross = machine.CurrentEnv.GlobalEnv.ParentProcess.CreateSubProcess();
    //            importedProgram = pross.ProcessProgram(FilePath);
    //        }
    //        catch (System.Exception ex)
    //        {
    //            throw new InterpreterException.ExceptionalSubProcess(this, machine.Continuation, ex);
    //        }

    //        if (importedProgram is not ModuleForm)
    //        {
    //            throw new InterpreterException(machine.Continuation,
    //                "Imported file failed to yield '{0}' program as expected.",
    //                nameof(ModuleForm));
    //        }

    //        machine.Continuation.Push(importedProgram);
    //    }
    //    public override Importation CopyContinuation() => new Importation(FilePath);
    //    public override string ToString() => string.Format("IMPRT(\"{0}\")", FilePath);
    //    public override Term ToTerm() => Cons.ProperList<Term>(Symbols.Import, new CharString(FilePath));
    //}

    //internal sealed class ModuleForm : CoreForm
    //{
    //    public readonly string Name;
    //    public readonly string[] ExportedNames;
    //    public readonly SequentialForm Body;

    //    // to run this form, the entire body is run in a pocket environment
    //    // then the exportations are gathered up and placed in a module-env in the super from beforehand
    //    public override bool IsImperative => true;
    //    public override string FormName => nameof(ModuleForm);
    //    public ModuleForm(string name, string[] exportedNames, SequentialForm body) : base()
    //    {
    //        Name = name;
    //        ExportedNames = exportedNames;
    //        Body = body;
    //    }
    //    public override void RunOnMachine(MachineState machine)
    //    {
    //        machine.Continuation.Push(new ChangeCurrentEnvironment(machine.CurrentEnv)); // switch back to current env
    //        machine.Continuation.Push(new ModuleInstallation(Name, ExportedNames)); // pluck exported defs out into module
    //        machine.Continuation.Push(Body); // enrich subEnv with definitions
    //        machine.Continuation.Push(new ChangeCurrentEnvironment(machine.CurrentEnv.Enclose())); // switch to subEnv
    //    }
    //    public override ModuleForm CopyContinuation() => new ModuleForm(Name, ExportedNames, Body.CopyContinuation());
    //    public override string ToString() => string.Format("MDL({0}: {1})", Name, Body);
    //    public override Term ToTerm() => Cons.ImproperList(Symbols.Module, Symbol.Intern(Name), Body.ToImplicitTerm());
    //}

    //internal sealed class Exportation : CoreForm
    //{
    //    public readonly string Name;
    //    public override bool IsImperative => false;
    //    public override string FormName => nameof(Importation);
    //    public Exportation(string name) => Name = name;
    //    public override void RunOnMachine(MachineState machine) { }
    //    public override Exportation CopyContinuation() => new Exportation(Name);
    //    public override string ToString() => string.Format("EXPRT({0})", Name);
    //    public override Term ToTerm() => Cons.ProperList<Term>(Symbols.Export, new CharString(Name));
    //}

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
        {
            if (machine.CurrentEnv.TryGetValue(VarName, out Term? value))
            {
                machine.ReturningValue = value;
            }
            else
            {
                throw new InterpreterException.InvalidBinding(VarName, machine);
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
        {
            machine.ReturningValue = Value;
        }
        public override VmInstruction CopyContinuation() => new ConstValue(Value);
        public override string ToString() => Value is Terms.Atom
            ? Value.ToString()
            : string.Format("QUOTE({0})", Value);
        public override Term ToTerm() => Cons.ProperList(Symbols.Quote, Value);
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
        {
            machine.Continuation.Push(new DispatchOnCondition(Consequent, Alternate));
            machine.Continuation.Push(new ChangeCurrentEnvironment(nameof(ConditionalForm), machine.CurrentEnv));
            machine.Continuation.Push(Test);
        }

        public override VmInstruction CopyContinuation() => new ConditionalForm(Test, Consequent, Alternate);
        public override string ToString() => string.Format("BRANCH({0}, {1}, {2})", Test, Consequent, Alternate);

        public override Term ToTerm() => Cons.ProperList(Symbols.If,
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
        {
            foreach (CoreForm node in Sequence.Reverse())
            {
                machine.Continuation.Push(new ChangeCurrentEnvironment(nameof(SequentialForm), machine.CurrentEnv));
                machine.Continuation.Push(node);
            }
        }

        public override SequentialForm CopyContinuation() => new SequentialForm(Sequence);
        public override string ToString() => string.Format("SEQ({0})", string.Join(", ", Sequence.ToArray<object>()));

        public override Term ToTerm() => Cons.Truct(Symbols.Begin, ToImplicitTerm());
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
    //    protected override void RunOnMachine(Stack<VmInstruction> continuation, ref MutableEnv currentEnv, ref Term currentValue)
    //    {
    //        if (Sequence.Length > 1)
    //        {
    //            continuation.Push(new TopLevelSequentialForm(Sequence[1..]));
    //        }
    //        continuation.Push(Sequence[0]);
    //    }
    //    public override VmInstruction CopyContinuation() => new TopLevelSequentialForm(Sequence);
    //    public override string ToString() => string.Format("SEQ-D({0})", string.Join(", ", Sequence.ToArray<object>()));

    //    public override Term ToTerm() => Pair.Cons(Symbols.Begin,
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
        {
            machine.Continuation.Push(new FunctionVerification(Arguments));
            machine.Continuation.Push(new ChangeCurrentEnvironment(Keywords.APPLY, machine.CurrentEnv));
            machine.Continuation.Push(Operator);
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
        {
            machine.ReturningValue = new CompoundProcedure(Formals, DottedFormal, Informals, machine.CurrentEnv, Body);
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

            return Cons.ImproperList(Symbols.Lambda, Cons.ImproperList(parameters), Body.ToImplicitTerm());
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
        {
            machine.Continuation.Push(new FunctionDispatch(Macro, Argument));
        }

        public override VmInstruction CopyContinuation() => new MacroApplication(Macro, Argument);
        public override string ToString() => string.Format("MACRO-APPL({0}; {1})", Macro, Argument);

        public override Term ToTerm() => Cons.ProperList<Term>(Macro, Argument);
    }

    #endregion

    internal sealed class ModularProgram : CoreForm
    {
        public readonly string Name;
        public readonly SequentialForm Program;

        public override string FormName => nameof(ModularProgram);

        public ModularProgram(string name, SequentialForm program)
        {
            Name = name;
            Program = program;
        }

        public override void RunOnMachine(MachineState machine)
        {
            machine.Continuation.Push(Program);
        }

        public override VmInstruction CopyContinuation() => new ModularProgram(Name, Program);
        public override string ToString() => string.Format("MODULE({0}: {1})", Name, Program.ToString());

        public override Term ToTerm() => Cons.ImproperList(Symbols.Module, Symbol.Intern(Name), Program.ToTerm());
    }
}
