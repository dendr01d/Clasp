using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Interfaces;

[assembly: InternalsVisibleTo("Tests")]
namespace Clasp
{
    public abstract class ClaspException : Exception
    {
        protected ClaspException(string format, params object?[] args)
            : base(string.Format(format, args))
        { }

        protected ClaspException(Exception innerException, string format, params object?[] args)
            : base(string.Format(format, args), innerException)
        { }

        //public class Uncategorized : ClaspException
        //{
        //    public Uncategorized(string format, params object?[] args) : base(format, args) { }
        //}

        protected static string FormatListItems(IEnumerable<string> items)
        {
            return string.Concat(items.Select(x => string.Format("\n\t- {0}", x)));
        }
    }

    public sealed class ClaspGeneralException : ClaspException
    {
        internal ClaspGeneralException(string format, params object?[] args) : base(format, args) { }
    }

    public abstract class LexerException : ClaspException, ISourceTraceable
    {
        public SourceCode Location { get; private set; }

        private LexerException(SourceCode loc, string format, params object[] args) : base(format, args)
        {
            Location = loc;
        }

        public class MalformedInput : LexerException
        {
            internal MalformedInput(Token token) : base(
                token.Location,
                "Malformed input on line {0} beginning at column {1}: {2}",
                token.Location.LineNumber,
                token.Location.Column,
                token.Text)
            { }
        }
    }

    public abstract class ReaderException : ClaspException
    {
        private ReaderException(string format, params object?[] args) : base(format, args) { }

        public class EmptyTokenStream : ReaderException
        {
            internal EmptyTokenStream() : base(
                "The reader received an empty token stream.")
            { }
        }

        public class AmbiguousParenthesis : ReaderException
        {
            internal AmbiguousParenthesis(bool opening) : base(
                "The reader counted an extra {0} parenthesis, but was unable to determine where it is.",
                opening ? "opening" : "closing")
            { }
        }

        public class UnmatchedParenthesis : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal UnmatchedParenthesis(Token errantParenToken) : base(
                "The reader found an {0} parenthesis on line {1}, column {2}.",
                errantParenToken.TType == TokenType.ClosingParen ? "un-opened" : "un-closed",
                errantParenToken.Location.LineNumber,
                errantParenToken.Location.Column)
            {
                Location = errantParenToken.Location;
            }
        }

        public class UnexpectedToken : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal UnexpectedToken(Token errantToken) : base(
                "Unexpected token {0} at line {1}, column {2}.",
                errantToken,
                errantToken.Location.LineNumber,
                errantToken.Location.Column)
            {
                Location = errantToken.Location;
            }
        }

        public class ExpectedToken : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal ExpectedToken(TokenType expectedType, Token receivedToken, Token prevToken) : base(
                "Expected {0} token to follow after {1} on line {2}, column {3}.",
                expectedType,
                prevToken,
                prevToken.Location.LineNumber,
                prevToken.Location.Column)
            {
                Location = receivedToken.Location;
            }
        }

        public class ExpectedListEnd : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal ExpectedListEnd(Token receivedToken, Token previous) : base(
                "Expected {0} token to complete list structure following {1} on line {2}, column {3}.",
                TokenType.ClosingParen,
                previous,
                previous.Location.LineNumber,
                previous.Location.Column)
            {
                Location = receivedToken.Location;
            }
        }

        public class UnhandledToken : ReaderException, ISourceTraceable
        {
            public SourceCode Location { get; private set; }

            internal UnhandledToken(Token errantToken) : base(
                "Token {0} of type {1} (on line {2}, column {3}) is unhandled by CLASP at this time :)",
                errantToken,
                errantToken.TType,
                errantToken.Location.LineNumber,
                errantToken.Location.Column)
            {
                Location = errantToken.Location;
            }
        }
    }

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
                "The given syntax is invalid for expansion: {0}",
                unknownForm)
            { }

            internal InvalidSyntax(Syntax unknownForm, ClaspException innerException) : base(
                unknownForm.Location,
                innerException,
                "The given syntax is invalid for expansion: {0}",
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
            internal AmbiguousIdentifier(Identifier ambId, IEnumerable<CompileTimeBinding> matches) : base(
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
            internal InvalidBindingOperation(Identifier unboundId, ExpansionContext context) : base(
                unboundId.Location,
                "Failed to bind '{0}' in phase {1} in scope ({2}).",
                unboundId.Name,
                context.Phase,
                string.Join(", ", unboundId.LexContext[context.Phase].Select(x => x.Id)))
            { }
        }


        public class WrongEvaluatedType : ExpanderException
        {
            internal WrongEvaluatedType(string expectedType, Term received, Syntax source) : base(
                source.Location,
                "Expected evaluation to yield term of type '{0}', but received '{1}' instead: {2} --> {3}",
                expectedType,
                received.TypeName,
                source,
                received)
            { }
        }

        public class EvaluationError : ExpanderException
        {
            internal EvaluationError(string inputType, Syntax source, Exception ce) : base(
                source.Location,
                ce,
                "An error occurred while accelerating & evaluating the '{0}' form: {1}",
                inputType,
                source)
            { }

            internal EvaluationError(string inputType, Syntax source, string msg) : base(
                source.Location,
                "A system-level exception occurred while accelerating & evaluating the '{0}' form: {1}: {2}",
                inputType,
                source,
                msg)
            { }
        }

        public class ExpectedProperList : ExpanderException
        {
            internal ExpectedProperList(Term notAProperList, LexInfo info) : base(
                info.Location,
                "Expected proper list: {0}",
                notAProperList)
            { }

            internal ExpectedProperList(string expectedType, Term notAProperList, LexInfo info) : base(
                info.Location,
                "Expected proper list with '{0}' elements: {1}",
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
                "Argument has the wrong shape: {1}",
                invalid)
            { }

            internal InvalidArguments(StxPair invalid, LexInfo info) : base(
                info.Location,
                "Arguments have the wrong shape: {0}",
                invalid)
            { }
        }

        public class InvalidContext : ExpanderException
        {
            internal InvalidContext(string invalidType, ExpansionMode mode, Syntax wrongSyntax) : base(
                wrongSyntax.Location,
                "Input of type '{0}' is invalid in '{1}' expansion context: {2}",
                invalidType,
                mode.ToString(),
                wrongSyntax)
            { }

            internal InvalidContext(string invalidType, ExpansionMode mode, Term wrongTerm, LexInfo info) : base(
                info.Location,
                "Input of type '{0}' is invalid in '{1}' expansion context: {2}",
                invalidType,
                mode.ToString(),
                wrongTerm)
            { }
        }
    }

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
                "The parser is unable to parse this syntax: {0}",
                badSyntax)
            { }

            internal InvalidSyntax(Syntax badSyntax, ClaspException innerException) : base(
                badSyntax.Location,
                innerException,
                "The parser is unable to parse this syntax: {0}",
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
            internal AmbiguousIdentifier(Identifier ambId, IEnumerable<CompileTimeBinding> matches) : base(
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
                "Form of type '{0}' can't be used as the operator of a function application: {1}",
                badOperator.FormName,
                badApplication)
            { }
        }

        public class InvalidForm : ParserException
        {
            internal InvalidForm(string formName, Syntax invalidForm) : base(
                invalidForm.Location,
                "Error parsing '{0}' form: {1}",
                formName,
                invalidForm)
            { }

            internal InvalidForm(string formName, Syntax invalidForm, Exception innerException) : base(
                invalidForm.Location,
                innerException,
                "Error parsing '{0}' form: {1}",
                formName,
                invalidForm)
            { }
        }
        public class InvalidArguments : ParserException
        {
            internal InvalidArguments(Syntax invalid) : base(
                invalid.Location,
                "Argument has the wrong shape: {1}",
                invalid)
            { }

            internal InvalidArguments(StxPair invalid, LexInfo info) : base(
                info.Location,
                "Arguments have the wrong shape: {0}",
                invalid)
            { }

            internal InvalidArguments(StxPair invalid, string preQualifier, int expectedNumber, LexInfo info) : base(
                info.Location,
                "The form requires{0} {1} argument{2}: {3}",
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
                "Expected expression form, but received imperative '{0}' form instead: {1}",
                wrongInput.FormName,
                wrongInput)
            { }
        }

        public class ExpectedProperList : ParserException
        {
            internal ExpectedProperList(Term notAProperList, LexInfo info) : base(
                info.Location,
                "Expected to parse proper list: {0}",
                notAProperList)
            { }

            internal ExpectedProperList(string expectedType, Term notAProperList, LexInfo info) : base(
                info.Location,
                "Expected proper list with '{0}' elements: {1}",
                expectedType,
                notAProperList)
            { }
        }

        //public class WrongFormat : ParserException
        //{
        //    internal WrongFormat(string formKeyword, Syntax args) : base(
        //        "Wrong number or types of arguments for '{0}' form: {1}",
        //        formKeyword,
        //        args)
        //    { }
        //}

        //public class WrongArity : ParserException
        //{
        //    internal WrongArity(string formTag, int expectedArgNum, bool exact, IEnumerable<AstNode> given) : base(
        //        "The '{0}' form expects {1} {2}argument{3}, but was given: {4}",
        //        formTag,
        //        expectedArgNum,
        //        exact ? string.Empty : "or more ",
        //        expectedArgNum == 1 ? string.Empty : "s",
        //        string.Concat(given.Select(x => string.Format("{0}   -> {1}", System.Environment.NewLine, x.ToString()))))
        //    { }
        //}

        //public class WrongArgType : ParserException
        //{
        //    internal WrongArgType(string formTag, string expectedType, IEnumerable<AstNode> given) : base(
        //        "The '{0}' form expects {1} of type {2}, but was given: {3}",
        //        formTag,
        //        given.Count() > 1 ? "arguments" : "an argument",
        //        expectedType,
        //        string.Concat(given.Select(x => string.Format("{0}   -> {1}", System.Environment.NewLine, x.ToString()))))
        //    { }

        //    internal WrongArgType(string formTag, string expectedType, int index, AstNode given) : base(
        //        "The '{0}' form expects an argument of type {1} at index {2}, but was given: {3}",
        //        formTag,
        //        expectedType,
        //        index,
        //        given)
        //    { }
        //}
    }

    public class InterpreterException : ClaspException
    {
        internal Stack<VmInstruction> ContinuationTrace;

        internal InterpreterException(Stack<VmInstruction> cont, string format, params object?[] args)
            : base(format, args)
        {
            ContinuationTrace = cont;
        }

        internal InterpreterException(Stack<VmInstruction> cont, Exception innerException, string format, params object?[] args)
            : base(innerException, format, args)
        {
            ContinuationTrace = cont;
        }

        public class InvalidBinding : InterpreterException
        {
            internal InvalidBinding(string varName, Stack<VmInstruction> cont) : base(
                cont,
                "Unable to dereference binding of identifier '{0}'.",
                varName)
            { }
        }

        public class InvalidOperationException : InterpreterException
        {
            internal InvalidOperationException(VmInstruction operation, Stack<VmInstruction> cont, Exception innerException) : base(
                cont,
                innerException,
                "Error evaluating the operation: {0}",
                operation)
            { }
        }
    }

    public class ProcessingException : ClaspException
    {
        private ProcessingException(string format, params object?[] args) : base(format, args) { }

        public class InvalidPrimitiveArgumentsException : ProcessingException
        {
            internal InvalidPrimitiveArgumentsException(Term arg) : base(
                "Could not process the primitive operation with this argument: {0}",
                string.Format("{0} ({1})", arg, arg.TypeName))
            { }

            internal InvalidPrimitiveArgumentsException(params Term[] args) : base(
                "Could not process the primitive operation with these argument/s: {0}",
                FormatListItems(args.Select(x => string.Format("{0} ({1})", x, x.TypeName))))
            { }

            internal InvalidPrimitiveArgumentsException() : base(
                "Could not process the primitive operation without any arguments.")
            { }
        }

        public class UnknownNumericType : ProcessingException
        {
            internal UnknownNumericType(params Number[] unknownNumbers) : base(
                "Number arguments to mathematical function are of unknown type(s): {0}",
                string.Join(", ", unknownNumbers.AsEnumerable()))
            { }
        }
    }
}
