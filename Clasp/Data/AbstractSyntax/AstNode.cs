using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Clasp.Data.Terms;
using Clasp.Data.Text;

namespace Clasp.Data.AbstractSyntax
{
    internal abstract class AstNode : Instruction
    {
        public abstract override string ToString();

        public abstract Terms.Term ToTerm();
    }

    #region Imperative Effects

    internal sealed class BindingDefinition : AstNode
    {
        public string VarName { get; private init; }
        public AstNode BoundValue { get; private init; }

        public BindingDefinition(string name, AstNode bound)
        {
            VarName = name;
            BoundValue = bound;
        }
        public override string ToString() => string.Format("DEF({0}, {1})", VarName, BoundValue);

        public override Term ToTerm() => ConsList.ProperList(Symbol.Define, Symbol.Intern(VarName), BoundValue.ToTerm());
    }

    internal sealed class BindingMutation : AstNode
    {
        public string VarName { get; private init; }
        public AstNode BoundValue { get; private init; }

        public BindingMutation(string name, AstNode bound)
        {
            VarName = name;
            BoundValue = bound;
        }
        public override string ToString() => string.Format("SET({0}, {1})", VarName, BoundValue);

        public override Term ToTerm() => ConsList.ProperList(Symbol.Set, Symbol.Intern(VarName), BoundValue.ToTerm());
    }

    #endregion

    #region Expression Types

    internal sealed class VariableLookup : AstNode
    {
        public string VarName { get; private init; }
        public VariableLookup(string name) => VarName = name;
        public override string ToString() => string.Format("VAR({0})", VarName);

        public override Term ToTerm() => Symbol.Intern(VarName);
    }

    internal sealed class ConstantValue : AstNode
    {
        public Terms.Atom Value { get; private init; }
        public ConstantValue(Terms.Atom value) => Value = value;
        public override string ToString() => string.Format("CONST({0})", Value);

        public override Term ToTerm() => Value;
    }

    internal sealed class Quotation : AstNode
    {
        public Terms.Term Value { get; private init; }
        public Quotation(Terms.Term value) => Value = value;
        public override string ToString() => string.Format("QUOTE({0})", Value);

        public override Term ToTerm() => ConsList.ProperList(Symbol.Quote, Value);
    }

    #endregion

    #region Functional Expressions

    internal sealed class FunctionApplication : AstNode
    {
        public readonly AstNode Operator;
        public readonly AstNode[] Args;

        public FunctionApplication(AstNode op, AstNode[] args)
        {
            Operator = op;
            Args = args;
        }
        public override string ToString() => string.Format(
            "APPL({0}; {1})",
            Operator,
            string.Join(", ", Args.ToArray<object>()));

        public override Term ToTerm() => ConsList.ProperList(
            new Term[] { Operator.ToTerm() }.Append(Args.Select(x => x.ToTerm())).ToArray());
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

        public override string ToString() => string.Format("FUN({0}{1}; {2})",
            string.Join(", ", Formals.ToArray<object>()),
            DottedFormal is null ? string.Empty : string.Format("; {0}", DottedFormal),
            string.Join(", ", Body));

        public override Term ToTerm() => ConsList.ProperList(Symbol.Lambda,
            ConsList.ConstructDirect(Formals.Select(x => Symbol.Intern(x)).ToList<Term>()
                .Append(DottedFormal is null ? Nil.Value : Symbol.Intern(DottedFormal))),
            Body.ToTerm());
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
        public override string ToString() => string.Format("SEQ({0})", string.Join(", ", Sequence.ToString()));

        public override Term ToTerm() => ConsList.Cons(Symbol.Begin,
            ConsList.ProperList(Sequence.Select(x => x.ToTerm()).ToArray()));
    }

    #endregion
}
