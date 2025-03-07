using System;
using System.Collections.Generic;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Modules;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Exceptions
{
    public abstract class ExpanderException : ClaspException, ISourceTraceable
    {
        public SourceCode Location { get; private set; }

        private ExpanderException(SourceCode loc, string format, params object?[] args) : base(format, args)
        {
            Location = loc;
        }

        private ExpanderException(SourceCode loc, Exception innerException, string format, params object?[] args)
            : base(innerException, format, args)
        {
            Location = loc;
        }

        public class InvalidSyntax : ExpanderException
        {
            internal InvalidSyntax(Syntax unknownForm) : base(
                unknownForm.Location,
                "The given syntax is invalid for expansion:\n\t{0}",
                unknownForm)
            { }

            internal InvalidSyntax(Syntax unknownForm, ClaspException innerException) : base(
                unknownForm.Location,
                innerException,
                "The given syntax is invalid for expansion:\n\t{0}",
                unknownForm)
            { }
        }

        public class UnboundIdentifier : ExpanderException
        {
            internal UnboundIdentifier(Identifier id) : base(
                id.Location,
                "The variable name '{0}' is free (unbound) within the given context.",
                id.Name)
            { }
        }
        public class AmbiguousIdentifier : ExpanderException
        {
            internal AmbiguousIdentifier(Identifier ambId, IEnumerable<RenameBinding> matches) : base(
                ambId.Location,
                "The variable name '{0}' ambiguously refers to multiple bindings within the given context: {1}",
                ambId.Name,
                FormatListItems(matches.Select(x => string.Format("'{0}' @ {1}", x.Id.Name, x.Id.Location))))
            { }
        }

        public class UnboundMacro : ExpanderException
        {
            internal UnboundMacro(Identifier macroBindingId) : base(
                macroBindingId.Location,
                "The variable name '{0}' wasn't bound to a macro procedure as expected.",
                macroBindingId.Name)
            { }
        }

        public class InvalidBindingOperation : ExpanderException
        {
            internal InvalidBindingOperation(Identifier unboundId, CompilationContext context) : base(
                unboundId.Location,
                "Failed to bind '{0}' in phase {1} in scope ({2}).",
                unboundId.Name,
                context.Phase,
                string.Join(", ", unboundId.GetScopeSet()[context.Phase].Select(x => x.Id)))
            { }
        }


        public class InvalidTransformer : ExpanderException
        {
            internal InvalidTransformer(Term received, Syntax source) : base(
                source.Location,
                "Instead of type '{0}', expression was parsed and interpreted as:\n\t{1}{2} {3}",
                nameof(MacroProcedure),
                received.TypeName,
                received is CompoundProcedure cp
                    ? string.Format(" (Arity {0}) ({1} variadic)", cp.Arity, cp.IsVariadic ? "YES" : "NOT")
                    : string.Empty,
                received)
            { }
        }

        public class InvalidTransformation : ExpanderException
        {
            internal InvalidTransformation(Term received, MacroProcedure invalidMacro, Syntax input) : base(
                input.Location,
                "Syntax transformation did not yield '{0}' as expected: {1}",
                nameof(Syntax),
                FormatListItems([
                    string.Format("Macro: {0}", invalidMacro),
                    string.Format("Input: {0}", input),
                    string.Format("Output: {0}", received)
                    ]))
            { }
        }

        public class EvaluationError : ExpanderException
        {
            internal EvaluationError(string inputType, Syntax source, Exception ce) : base(
                source.Location,
                ce,
                "An error occurred while accelerating & evaluating the '{0}' form:\n\t{1}",
                inputType,
                source)
            { }

            internal EvaluationError(string inputType, Syntax source, string msg) : base(
                source.Location,
                "A system-level exception occurred while accelerating & evaluating the '{0}' form:\n\t{1}: {2}",
                inputType,
                source,
                msg)
            { }
        }

        public class ExpectedProperList : ExpanderException
        {
            internal ExpectedProperList(Syntax notAProperList) : base(
                notAProperList.Location,
                "Expected proper list:\n\t{0}",
                notAProperList)
            { }

            internal ExpectedProperList(string expectedType, Syntax notAProperList) : base(
                notAProperList.Location,
                "Expected proper list with '{0}' elements:\n\t{1}",
                expectedType,
                notAProperList)
            { }
        }

        /// <summary>For when you know the entirety of the form.</summary>
        public class InvalidForm : ExpanderException
        {
            internal InvalidForm(string formName, Syntax form) : base(
                form.Location,
                "Error expanding '{0}' form.",
                formName)
            { }

            internal InvalidForm(string formName, Syntax form, Exception innerException) : base(
                form.Location,
                innerException,
                "Error expanding '{0}' form.",
                formName
                )
            { }
        }

        /// <summary>For when you only know the subform, and you're relying on an <see cref="InvalidForm"/> to catch this.</summary>
        public class InvalidArguments : ExpanderException
        {
            internal InvalidArguments(Syntax invalid) : base(
                invalid.Location,
                "Wrong argument shape for form:\n\t{1}",
                invalid)
            { }
        }

        public class InvalidContext : ExpanderException
        {
            internal InvalidContext(string invalidType, ExpansionMode mode, Syntax wrongSyntax) : base(
                wrongSyntax.Location,
                "Input of type '{0}' is invalid in '{1}' expansion context:\n\t{2}",
                invalidType,
                mode.ToString(),
                wrongSyntax)
            { }
        }

        public class CircularModuleReference : ExpanderException
        {
            internal CircularModuleReference(DeclaredModule pendingModule, Syntax offendingStx) : base(
                offendingStx.Location,
                "A circular module reference occurred -- the expander prompted expansion of module '{0}', which is already pending:\n\t{1}",
                pendingModule.Name,
                offendingStx.ToSyntaxString())
            { }
        }
    }

}
