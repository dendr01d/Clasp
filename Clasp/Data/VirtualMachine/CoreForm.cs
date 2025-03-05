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
    /// The core forms of the Scheme being described. All programs are represented in terms of
    /// these objects, and all source code must be parsed to a tree of these values.
    /// </summary>
    internal abstract class CoreForm : VmInstruction
    {
        protected CoreForm() : base() { }
        public virtual bool IsImperative { get; } = false;
        public abstract string ImplicitKeyword { get; }
        public abstract Term ToTerm();
    }

    internal sealed class TopBegin : CoreForm
    {
        private readonly CoreForm[] _bodyForms;
        public override string AppCode => "T-SEQ";
        public override string ImplicitKeyword => Keywords.S_TOP_DEFINE;
        public TopBegin(IEnumerable<CoreForm> bodyForms)
        {
            _bodyForms = bodyForms.ToArray();
        }
        public override void RunOnMachine(MachineState machine)
        {
            if (_bodyForms.Length > 1)
            {
                machine.Continuation.Push(new TopBegin(_bodyForms[1..]));
            }
            machine.Continuation.Push(_bodyForms[0]);
        }
        public override VmInstruction CopyContinuation()
        {
            // The continuation can't contain any of the other forms in the sequence
            // Act as if it were ONLY the next form to be evaluated
            return _bodyForms[0];
        }
        protected override string FormatArgs() => string.Join(", ", _bodyForms.Select(x => x.ToString()));
        public override Term ToTerm() => Cons.Truct(Symbols.Begin, Cons.ProperList(_bodyForms.Select(x => x.ToTerm())));
    }

    internal sealed class TopDefine : CoreForm
    {
        private readonly Symbol _key;
        private readonly CoreForm _value;
        public override string AppCode => "T-DEF";
        public override string ImplicitKeyword => Keywords.S_TOP_DEFINE;
        public TopDefine(Symbol key, CoreForm value)
        {
            _key = key;
            _value = value;
        }
        public override void RunOnMachine(MachineState machine)
        {
            VmInstruction setOrDefine = machine.CurrentEnv.ContainsKey(_key.Name)
                ? new RebindExisting(_key.Name)
                : new BindFresh(_key.Name);

            machine.Continuation.Push(setOrDefine);
            machine.Continuation.Push(_value);
        }
        public override VmInstruction CopyContinuation() => new TopDefine(_key, _value);
        protected override string FormatArgs() => string.Format("{0}, {1}", _key, _value.ToString());
        public override Term ToTerm() => Cons.ProperList(Symbols.Define, _key, _value.ToTerm());
    }

    internal sealed class Importation : CoreForm
    {
        private readonly Symbol[] _keys;
        public override string AppCode => "IMPRT";
        public override string ImplicitKeyword => Keywords.S_IMPORT;
        public Importation(IEnumerable<Symbol> keys) => _keys = keys.ToArray();
        public override void RunOnMachine(MachineState machine)
        {
            machine.Continuation.Push(new ChangeEnv(this, machine.CurrentEnv));

            foreach (Symbol key in _keys)
            {
                if (!ModuleCache.TryGet(key.Name, out Module? module))
                {
                    throw new InterpreterException(machine, "Unable to uncache module '{0}'.", key.Name);
                }

                machine.Continuation.Push(new InstallModule(module));
            }
        }
        public override VmInstruction CopyContinuation() => new Importation(_keys);
        protected override string FormatArgs() => string.Join(", ", _keys.Select(x => x.ToString()));
        public override Term ToTerm() => Cons.Truct(Symbols.Import, Cons.ProperList(_keys));
    }

    internal sealed class Mutation : CoreForm
    {
        private readonly Symbol _key;
        private readonly CoreForm _value;
        public override string AppCode => "SET";
        public override string ImplicitKeyword => Keywords.S_SET;
        public Mutation(Symbol key, CoreForm value)
        {
            _key = key;
            _value = value;
        }
        public override void RunOnMachine(MachineState machine)
        {
            machine.Continuation.Push(new RebindExisting(_key.Name));
            machine.Continuation.Push(_value);
        }
        public override VmInstruction CopyContinuation() => new Mutation(_key, _value);
        protected override string FormatArgs() => string.Format("{0}, {1}", _key, _value.ToString());
        public override Term ToTerm() => Cons.ProperList(Symbols.Set, _key, _value.ToTerm());
    }

    internal sealed class Conditional : CoreForm
    {
        private readonly CoreForm _test;
        private readonly CoreForm _consequent;
        private readonly CoreForm _alternative;
        public override string AppCode => "IF";
        public override string ImplicitKeyword => Keywords.S_IF;
        public Conditional(CoreForm test, CoreForm consequent, CoreForm alternate)
        {
            _test = test;
            _consequent = consequent;
            _alternative = alternate;
        }
        public override void RunOnMachine(MachineState machine)
        {
            machine.Continuation.Push(new DispatchOnCondition(_consequent, _alternative));
            machine.Continuation.Push(new ChangeEnv(this, machine.CurrentEnv));
            machine.Continuation.Push(_test);
        }
        public override VmInstruction CopyContinuation() => new Conditional(_test, _consequent, _alternative);
        protected override string FormatArgs() => string.Join(", ", _test, _consequent, _alternative);
        public override Term ToTerm() => Cons.ProperList(Symbols.If,
            _test.ToTerm(), _consequent.ToTerm(), _alternative.ToTerm());
    }

    internal sealed class Sequential : CoreForm
    {
        private readonly CoreForm[] _bodyForms;
        public override string AppCode => "SEQ";
        public override string ImplicitKeyword => Keywords.S_BEGIN;
        public Sequential(IEnumerable<CoreForm> bodyForms)
        {
            _bodyForms = bodyForms.ToArray();
        }
        public override void RunOnMachine(MachineState machine)
        {
            foreach (CoreForm form in _bodyForms.Reverse())
            {
                machine.Continuation.Push(new ChangeEnv(this, machine.CurrentEnv));
                machine.Continuation.Push(form);
            }
        }
        public override Sequential CopyContinuation() => new Sequential(_bodyForms);
        protected override string FormatArgs() => string.Join(", ", _bodyForms.Select(x => x.ToString()));
        public override Term ToTerm() => Cons.Truct(Symbols.Begin, Cons.ProperList(_bodyForms.Select(x => x.ToTerm())));
    }

    internal sealed class Application : CoreForm
    {
        private readonly CoreForm _operator;
        private readonly CoreForm[] _arguments;
        public override string AppCode => "APPL";
        public override string ImplicitKeyword => Keywords.S_APPLY;
        public Application(CoreForm op, CoreForm[] args) : base()
        {
            _operator = op;
            _arguments = args;
        }
        public override void RunOnMachine(MachineState machine)
        {
            machine.Continuation.Push(new FunctionVerification(_arguments));
            machine.Continuation.Push(new ChangeEnv(this, machine.CurrentEnv));
            machine.Continuation.Push(_operator);
        }
        public override Application CopyContinuation() => new Application(_operator, _arguments);
        protected override string FormatArgs() => string.Join(", ", _operator, string.Join(", ", _arguments.Select(x => x.ToString())));
        public override Term ToTerm() => Cons.ImproperList(Symbols.Apply, _operator.ToTerm(), Cons.ProperList(_arguments.Select(x => x.ToTerm())));
    }

    internal sealed class Procedural : CoreForm
    {
        private readonly Symbol[] _formals;
        private readonly Symbol? _formalVariad;
        private readonly Symbol[] _informals;
        private readonly Sequential _body;
        public override string AppCode => "FUNC";
        public override string ImplicitKeyword => Keywords.S_LAMBDA;
        public Procedural(IEnumerable<Symbol> formalParams, Symbol? variadicParam, IEnumerable<Symbol> informalParams, Sequential body)
        {
            _formals = formalParams.ToArray();
            _formalVariad = variadicParam;
            _informals = informalParams.ToArray();
            _body = body;
        }
        public override void RunOnMachine(MachineState machine)
        {
            machine.ReturningValue = new CompoundProcedure(
                _formals.Select(x => x.Name).ToArray(),
                _formalVariad?.Name,
                _informals.Select(x => x.Name).ToArray(),
                machine.CurrentEnv, _body);
        }
        public override Procedural CopyContinuation() => new Procedural(_formals, _formalVariad, _informals, _body);
        protected override string FormatArgs() => string.Join(", ",
            string.Format("({0})", string.Join(", ", _formals.Select(x => x.Name))),
            string.Format("({0})", _formalVariad?.Name ?? string.Empty),
            string.Format("({0})", string.Join(", ", _informals.Select(x => x.Name))),
            _body.ToString());
        public override Term ToTerm() => Cons.ImproperList(Symbols.Lambda, Cons.ImproperList(_formals.Append((Term?)_formalVariad ?? Nil.Value)), _body.ToTerm());
    }

    internal sealed class VariableReference : CoreForm
    {
        private readonly Symbol _key;
        private readonly bool _top;
        public override string AppCode => "VAR";
        public override string ImplicitKeyword => Keywords.S_VAR;
        public VariableReference(Symbol key, bool top)
        {
            _key = key;
            _top = top;
        }
        public override void RunOnMachine(MachineState machine)
        {
            if (machine.CurrentEnv.TryGetValue(_key.Name, out Term? value))
            {
                machine.ReturningValue = value;
            }
            else if (_top)
            {
                throw new InterpreterException.InvalidTopBinding(_key.Name, machine);
            }
            else
            {
                throw new InterpreterException.InvalidBinding(_key.Name, machine);
            }
        }
        public override VariableReference CopyContinuation() => new VariableReference(_key, _top);
        protected override string FormatArgs() => _key.Name;
        public override Term ToTerm() => _key;
    }

    internal sealed class ConstValue : CoreForm
    {
        private readonly Term _value;
        public override string AppCode => "CONST";
        public override string ImplicitKeyword => Keywords.S_CONST;
        public ConstValue(Term value) => _value = value;
        public override void RunOnMachine(MachineState machine)
        {
            machine.ReturningValue = _value;
        }
        public override ConstValue CopyContinuation() => new ConstValue(_value);
        protected override string FormatArgs() => _value.ToString();
        public override Term ToTerm() => _value is Atom
            ? _value
            : Cons.ProperList(Symbols.Quote, _value);
    }
}
