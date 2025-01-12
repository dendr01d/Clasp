using System;
using System.Collections.Generic;
using System.Linq;

using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Text;

namespace Clasp.Data.AbstractSyntax
{
    internal abstract class AstNode : EvFrame
    {
        protected AstNode() : base() { }

        public abstract Term ToTerm();
    }

    #region Imperative Effects

    internal sealed class BindingDefinition : AstNode
    {
        public string VarName { get; private init; }
        public AstNode BoundValue { get; private init; }
        public BindingDefinition(string key, AstNode value) : base()
        {
            VarName = key;
            BoundValue = value;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new BindFresh(VarName));
            continuation.Push(BoundValue);
        }
        public override EvFrame CopyContinuation() => new BindingDefinition(VarName, BoundValue);
        public override string ToString() => string.Format("DEF({0}, {1})", VarName, BoundValue);
        public override Term ToTerm() => ConsList.ProperList(Symbol.Define, Symbol.Intern(VarName), BoundValue.ToTerm());
    }

    internal sealed class BindingMutation : AstNode
    {
        public string VarName { get; private init; }
        public AstNode BoundValue { get; private init; }

        public BindingMutation(string name, AstNode bound) : base()
        {
            VarName = name;
            BoundValue = bound;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new RebindExisting(VarName));
            continuation.Push(BoundValue);
        }
        public override EvFrame CopyContinuation() => new BindingMutation(VarName, BoundValue);
        public override string ToString() => string.Format("SET({0}, {1})", VarName, BoundValue);
        public override Term ToTerm() => ConsList.ProperList(Symbol.Set, Symbol.Intern(VarName), BoundValue.ToTerm());
    }

    #endregion

    #region Expression Types

    internal sealed class VariableLookup : AstNode
    {
        public string VarName { get; private init; }
        public VariableLookup(string key) : base()
        {
            VarName = key;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
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
        public override EvFrame CopyContinuation() => new VariableLookup(VarName);
        public override string ToString() => string.Format("VAR({0})", VarName);
        public override Term ToTerm() => Symbol.Intern(VarName);
    }

    internal sealed class ConstValue : AstNode
    {
        public Term Value { get; private init; }

        public ConstValue(Term value) : base()
        {
            Value = value;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
        {
            currentValue = Value;
        }
        public override EvFrame CopyContinuation() => new ConstValue(Value);
        public override string ToString() => Value is Atom
            ? Value.ToString()
            : string.Format("QUOTE({0})", Value);
        public override Term ToTerm() => ConsList.ProperList(Symbol.Quote, Value);
    }

    #endregion

    #region Execution Path

    internal sealed class ConditionalForm : AstNode
    {
        public readonly AstNode Test;
        public readonly AstNode Consequent;
        public readonly AstNode Alternate;
        public ConditionalForm(AstNode test, AstNode consequent, AstNode alternate)
        {
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new DispatchOnCondition(Consequent, Alternate));
            continuation.Push(new ChangeCurrentEnvironment(currentEnv));
            continuation.Push(Test);
        }
        public override EvFrame CopyContinuation() => new ConditionalForm(Test, Consequent, Alternate);
        public override string ToString() => string.Format("BRANCH({0}, {1}, {2})", Test, Consequent, Alternate);

        public override Term ToTerm() => ConsList.ProperList(Symbol.If,
            Test.ToTerm(), Consequent.ToTerm(), Alternate.ToTerm());
    }

    internal sealed class SequentialForm : AstNode
    {
        public readonly AstNode[] Sequence;
        public SequentialForm(AstNode[] series)
        {
            Sequence = series;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
        {
            foreach (AstNode node in Sequence.Reverse())
            {
                continuation.Push(node);
                continuation.Push(new ChangeCurrentEnvironment(currentEnv));
            }
        }
        public override EvFrame CopyContinuation() => new SequentialForm(Sequence);
        public override string ToString() => string.Format("SEQ({0})", string.Join(", ", Sequence.ToString()));

        public override Term ToTerm() => ConsList.Cons(Symbol.Begin,
            ConsList.ProperList(Sequence.Select(x => x.ToTerm()).ToArray()));
    }

    internal sealed class TopLevelSequentialForm : AstNode
    {
        public readonly AstNode[] Sequence;
        public TopLevelSequentialForm(AstNode[] series)
        {
            Sequence = series;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
        {
            if (Sequence.Length > 1)
            {
                continuation.Push(new TopLevelSequentialForm(Sequence[1..]));
            }
            continuation.Push(Sequence[0]);
        }
        public override EvFrame CopyContinuation() => new TopLevelSequentialForm(Sequence);
        public override string ToString() => string.Format("SEQ-D({0})", string.Join(", ", Sequence.ToString()));

        public override Term ToTerm() => ConsList.Cons(Symbol.Begin,
            ConsList.ProperList(Sequence.Select(x => x.ToTerm()).ToArray()));
    }

    //internal sealed class CallWithCurrentContinuation : AstNode
    //{
    //    // needs to copy the continuation of the runtime
    //    // but skipping over any top-level forms at the bottom of the stack
    //}

    #endregion

    #region Functional Expressions

    internal sealed class FunctionApplication : AstNode
    {
        public readonly AstNode Operator;
        public readonly AstNode[] Arguments;

        public FunctionApplication(AstNode op, AstNode[] args) : base()
        {
            Operator = op;
            Arguments = args;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new FunctionVerification(Arguments));
            //continuation.Push(new RollUpArguments(Arguments));
            continuation.Push(new ChangeCurrentEnvironment(currentEnv));
            continuation.Push(Operator);
        }

        public override EvFrame CopyContinuation() => new FunctionApplication(Operator, Arguments);

        public override string ToString() => string.Format(
            "APPL({0}; {1})",
            Operator,
            string.Join(", ", Arguments.ToArray<object>()));

        public override Term ToTerm() => ConsList.Cons(
            Operator.ToTerm(),
            ConsList.ProperList(Arguments.Select(x => x.ToTerm()).ToArray()));
    }

    internal sealed class FunctionCreation : AstNode
    {
        public readonly string[] Formals;
        public readonly string? DottedFormal;
        public readonly string[] Informals;
        public readonly SequentialForm Body;

        public FunctionCreation(string[] parameters, string? dottedParameter, string[] internalKeys, SequentialForm body)
        {
            Formals = parameters;
            DottedFormal = dottedParameter;
            Informals = internalKeys;
            Body = body;
        }
        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
        {
            continuation.Push(new ConstValue(new CompoundProcedure(Formals, DottedFormal, currentEnv, Body)));
        }
        public override EvFrame CopyContinuation() => new FunctionCreation(Formals, DottedFormal, Informals, Body);

        public override string ToString() => string.Format("FUN({0}{1}; {2})",
            string.Join(", ", Formals.ToArray<object>()),
            DottedFormal is null ? string.Empty : string.Format("; {0}", DottedFormal),
            string.Join(", ", Body));

        public override Term ToTerm() => ConsList.ProperList(Symbol.Lambda,
            ConsList.ConstructDirect(Formals
                .Select(x => Symbol.Intern(x))
                .ToList<Term>()
                .Append(DottedFormal is null ? Nil.Value : Symbol.Intern(DottedFormal))),
            Body.ToTerm());
    }

    #endregion

    #region Macro Expressions

    internal sealed class MacroApplication : AstNode
    {
        public readonly MacroProcedure Macro;
        public readonly Syntax Argument;

        public MacroApplication(MacroProcedure macro, Syntax arg)
        {
            Macro = macro;
            Argument = arg;
        }

        public override void RunOnMachine(Stack<EvFrame> continuation, ref Binding.Environment currentEnv, ref Term currentValue)
        {
            throw new NotImplementedException();
        }
        public override EvFrame CopyContinuation() => new MacroApplication(Macro, Argument);
        public override string ToString() => string.Format("MACRO-APPL({0}; {1})", Macro, Argument);

        public override Term ToTerm() => ConsList.ProperList(Macro, Argument);
    }

    #endregion
}
