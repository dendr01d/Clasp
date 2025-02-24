using System;
using System.Collections.Generic;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Exceptions
{
    public abstract class ParserException : ClaspException, ISourceTraceable
    {
        public SourceCode Location { get; private set; }
        private ParserException(SourceCode loc, string format, params object?[] args) : base(format, args)
        {
            Location = loc;
        }

        private ParserException(SourceCode loc, Exception innerException, string format, params object?[] args)
            : base(innerException, format, args)
        {
            Location = loc;
        }

        public class InvalidSyntax : ParserException
        {
            internal InvalidSyntax(Syntax badSyntax) : base(
                badSyntax.Location,
                "The parser is unable to parse this syntax:\n\t{0}",
                badSyntax)
            { }

            internal InvalidSyntax(Syntax badSyntax, ClaspException innerException) : base(
                badSyntax.Location,
                innerException,
                "The parser is unable to parse this syntax:\n\t{0}",
                badSyntax)
            { }
        }

        public class UnboundMacro : ParserException
        {
            internal UnboundMacro(Identifier macroBindingId) : base(
                macroBindingId.Location,
                "The variable name '{0}' wasn't bound to a macro procedure as expected.",
                macroBindingId.Name)
            { }
        }

        public class UnboundIdentifier : ParserException
        {
            internal UnboundIdentifier(Identifier id) : base(
                id.Location,
                "The variable name '{0}' is free (unbound) within the given context.",
                id.Name)
            { }
        }
        public class AmbiguousIdentifier : ParserException
        {
            internal AmbiguousIdentifier(Identifier ambId, IEnumerable<ExpansionVarNameBinding> matches) : base(
                ambId.Location,
                "The variable name '{0}' ambiguously refers to multiple bindings within the given context: {1}",
                ambId.Name,
                FormatListItems(matches.Select(x => string.Format("'{0}' @ {1}", x.Id.Name, x.Id.Location))))
            { }
        }

        public class InvalidOperator : ParserException
        {
            internal InvalidOperator(CoreForm badOperator, Syntax badApplication) : base(
                badApplication.Location,
                "Form of type '{0}' can't be used as the operator of a function application:\n\t{1}",
                badOperator.FormName,
                badApplication)
            { }
        }

        public class InvalidForm : ParserException
        {
            internal InvalidForm(string formName, Syntax invalidForm) : base(
                invalidForm.Location,
                "Error parsing '{0}' form:\n\t{1}",
                formName,
                invalidForm)
            { }

            internal InvalidForm(string formName, Syntax invalidForm, Exception innerException) : base(
                invalidForm.Location,
                innerException,
                "Error parsing '{0}' form:\n\t{1}",
                formName,
                invalidForm)
            { }
        }
        public class InvalidArguments : ParserException
        {
            internal InvalidArguments(Syntax invalid) : base(
                invalid.Location,
                "Argument has the wrong shape:\n\t{1}",
                invalid)
            { }

            internal InvalidArguments(Cons invalid, LexInfo info) : base(
                info.Location,
                "Arguments have the wrong shape:\n\t{0}",
                invalid)
            { }

            internal InvalidArguments(Cons invalid, string preQualifier, int expectedNumber, LexInfo info) : base(
                info.Location,
                "The form requires{0} {1} argument{2}:\n\t{3}",
                string.IsNullOrWhiteSpace(preQualifier) ? string.Empty : " " + preQualifier,
                expectedNumber.ToString(),
                expectedNumber == 1 ? string.Empty : "s",
                invalid
                )
            { }
        }

        public class ExpectedExpression : ParserException
        {
            internal ExpectedExpression(CoreForm wrongInput, LexInfo info) : base(
                info.Location,
                "Expected expression form, but received imperative '{0}' form instead:\n\t{1}",
                wrongInput.FormName,
                wrongInput)
            { }
        }

        public class ExpectedProperList : ParserException
        {
            internal ExpectedProperList(Term notAProperList, LexInfo info) : base(
                info.Location,
                "Expected to parse proper list:\n\t{0}",
                notAProperList)
            { }

            internal ExpectedProperList(string expectedType, Term notAProperList, LexInfo info) : base(
                info.Location,
                "Expected proper list with '{0}' elements:\n\t{1}",
                expectedType,
                notAProperList)
            { }
        }
    }

}
