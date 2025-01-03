using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clasp.Data.AbstractSyntax
{
    internal abstract class AstNode : Instruction
    {
        public abstract override string ToString();
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
    }

    #endregion

    #region Expression Types

    internal sealed class VariableLookup : AstNode
    {
        public string VarName { get; private init; }
        public VariableLookup(string name) => VarName = name;
        public override string ToString() => string.Format("VAR({0})", VarName);
    }

    internal sealed class ConstantValue : AstNode
    {
        public Terms.Atom Value { get; private init; }
        public ConstantValue(Terms.Atom value) => Value = value;
        public override string ToString() => string.Format("CONST({0})", Value);
    }

    internal sealed class Quotation : AstNode
    {
        public Terms.Product Value { get; private init; }
        public Quotation(Terms.Product value) => Value = value;
        public override string ToString() => string.Format("QUOTE({0})", Value);
    }

    #endregion

    #region Functional Expressions

    internal sealed class FunctionApplication : AstNode
    {
        public readonly AstNode OperatorExpression;
        public readonly AstNode[] Args;
        public FunctionApplication(AstNode op, params AstNode[] args)
        {
            OperatorExpression = op;
            Args = args;
        }
        public override string ToString() => string.Format(
            "APPL({0}, {1})",
            OperatorExpression,
            string.Join(", ", Args.ToArray<object>()));
    }

    internal sealed class FunctionCreation : AstNode
    {
        public readonly string[] Formals;
        public readonly string[] Informals;
        public readonly SequentialForm Body;

        public FunctionCreation(string[] parameters, string[] internalKeys, SequentialForm body)
        {
            Formals = parameters;
            Informals = internalKeys;
            Body = body;
        }

        public override string ToString() => string.Format("FUN({0}; {1})",
            string.Join(", ", Formals.ToArray<object>()),
            string.Join(", ", Body));
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
    }

    internal sealed class SequentialForm : AstNode
    {
        public readonly AstNode[] Sequence;
        public SequentialForm(AstNode[] series)
        {
            Sequence = series;
        }
        public override string ToString() => string.Format("SEQ({0})", string.Join(", ", Sequence.ToString()));
    }

    #endregion
}
