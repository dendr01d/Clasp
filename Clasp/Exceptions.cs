﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Text;

[assembly: InternalsVisibleTo("Tests")]
namespace Clasp
{
    public abstract class ClaspException : Exception
    {
        protected ClaspException(string format, params object?[] args) : base(string.Format(format, args)) { }

        //public class Uncategorized : ClaspException
        //{
        //    public Uncategorized(string format, params object?[] args) : base(format, args) { }
        //}

        public class FailedDispatch : ClaspException
        {
            internal FailedDispatch() : base(
                "Unexpectedly fell out of case expression.")
            { }
        }
    }

    public sealed class ClaspGeneralException : ClaspException
    {
        internal ClaspGeneralException(string format, params object?[] args) : base(format, args) { }
    }

    public abstract class LexerException : ClaspException, ISourceTraceable
    {
        public SourceLocation Location { get; private set; }

        private LexerException(SourceLocation loc, string format, params object[] args) : base(format, args)
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
            public SourceLocation Location { get; private set; }

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
            public SourceLocation Location { get; private set; }

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
            public SourceLocation Location { get; private set; }

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

        public class UnhandledToken : ReaderException, ISourceTraceable
        {
            public SourceLocation Location { get; private set; }

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
        public SourceLocation Location { get; private set; }

        private ExpanderException(SourceLocation loc, string format, params object?[] args) : base(format, args)
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
        }

        public class UnboundIdentifier : ExpanderException
        {
            internal UnboundIdentifier(string name, Syntax unboundIdentifier) : base(
                unboundIdentifier.Location,
                "The variable name '{0}' is free (unbound) within the given context.",
                name)
            { }
        }

        public class AmbiguousIdentifier : ExpanderException
        {
            internal AmbiguousIdentifier(string name, Syntax ambiguousIdentifier) : base(
                ambiguousIdentifier.Location,
                "The variable name '{0}' ambiguously refers to multiple bindings within the given context.",
                name)
            { }
        }

        public class ExpectedEvaluation : ExpanderException
        {
            internal ExpectedEvaluation(string expectedTypeName, Term received, Syntax source) : base(
                source.Location,
                "Expected evaluation to yield Term of type '{0}', but instead: {1} --> {2}",
                expectedTypeName,
                source,
                received
                )
            { }
        }

        public class ExpectedProperList : ExpanderException
        {
            internal ExpectedProperList(Syntax notAProperList) : base(
                notAProperList.Location,
                "Expected proper list: {0}",
                notAProperList)
            { }
        }

        //public class InvalidContext : ExpanderException
        //{
        //    internal InvalidContext(Symbol op, ExpansionContext ctx) : base(
        //        "Form with operator '{0}' is invalid to be expanded in '{1}' context.",
        //        op,
        //        ctx.ToString())
        //    { }
        //}

        public class InvalidFormInput : ExpanderException
        {
            internal InvalidFormInput(string formName, Syntax invalidForm) : base(
                invalidForm.Location,
                "Invalid input in expansion of '{0}' form: {1}",
                formName,
                invalidForm)
            { }

            internal InvalidFormInput(string formName, string inputDescription, Syntax invalidForm) : base(
                invalidForm.Location,
                "Invalid input in expansion of {0} within '{1}' form: {2}",
                inputDescription,
                formName,
                invalidForm)
            { }
        }
    }

    public abstract class ParserException : ClaspException, ISourceTraceable
    {
        public SourceLocation Location { get; private set; }
        private ParserException(SourceLocation loc, string format, params object?[] args) : base(format, args)
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
        }

        public class InvalidOperator : ParserException
        {
            internal InvalidOperator(string receivedType, Syntax badApplication) : base(
                badApplication.Location,
                "Form of type '{0}' can't be used as the operator term of a function application: {1}",
                receivedType,
                badApplication)
            { }
        }

        public class WrongArity : ParserException
        {
            internal WrongArity(string formName, string howMany, Syntax badApplication) : base(
                badApplication.Location,
                "Form of type '{0}' requires {1} argument/s in a proper list: {2}",
                formName,
                howMany,
                badApplication)
            { }
        }

        public class WrongType : ParserException
        {
            internal WrongType(string formName, string expectedType, Syntax badApplication) : base(
                badApplication.Location,
                "An argument of type '{0}' is required for a '{1}' form: {2}",
                expectedType,
                formName,
                badApplication)
            { }
        }

        public class InvalidFormInput : ParserException
        {
            internal InvalidFormInput(string formName, string inputDescription, Syntax invalidForm) : base(
                invalidForm.Location,
                "Invalid syntax in expansion of {0} within '{1}' form: {2}",
                inputDescription,
                formName,
                invalidForm)
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
        internal MxInstruction[] ContinuationTrace;

        internal InterpreterException(Stack<MxInstruction> cont, string format, params object?[] args) : base(format, args)
        {
            ContinuationTrace = cont.ToArray();
        }

        public class InvalidBinding : InterpreterException
        {
            internal InvalidBinding(string varName, Stack<MxInstruction> cont) : base(
                cont,
                "Unable to dereference binding of identifier '{0}'.",
                varName)
            { }
        }
    }

    //internal class ExpectedTypeException<T> : ClaspException
    //    where T : Expression
    //{
    //    internal ExpectedTypeException(Expression erroneous) : base(FormatMsg(erroneous)) { }

    //    private static string FormatMsg(Expression erroneous)
    //    {
    //        return $"Expected Expression of type '{typeof(T).Name}' but received '{erroneous}' of type '{erroneous.GetType().Name}'.";
    //    }        
    //}

    //internal class IncompatibleTypeException<T, U> : ClaspException
    //    where T : Expression
    //    where U : Expression
    //{
    //    internal IncompatibleTypeException(T e1, U e2, string opName) : base(FormatMsg(e1, e2, opName)) { }

    //    private static string FormatMsg(T e1, U e2, string opName)
    //    {
    //        return $"{e1} of type '{typeof(T).Name}' and {e2} of type '{typeof(U).Name}' are incompatible arguments to operation '{opName}'.";
    //    }
    //}

    //public class ArityConflictException : Exception
    //{
    //    internal ArityConflictException(Procedure proc, Expression? argl = null) : 
    //        base(FormatMsg(proc, argl))
    //    { }

    //    private static string FormatMsg(Procedure proc, Expression? argl)
    //    {
    //        return string.Format("{0} arguments {1} provided to procedure {2}",
    //            argl is null ? "Insufficient" : "Extraneous",
    //            argl is null ? string.Empty : argl.ToString(),
    //            proc.ToString());
    //    }
    //}

    //public class MissingArgumentException : Exception
    //{
    //    internal MissingArgumentException(string funcName) :
    //        base($"Expected additional argument(s) for function '{funcName}'.")
    //    { }
    //}

    //public class DuplicateBindingException : Exception
    //{
    //    internal DuplicateBindingException(Symbol sym) :
    //        base($"Attempted to re-Define binding of Symbol '{sym}'.")
    //    { }
    //}

    public class MissingBindingException : ClaspException
    {
        internal MissingBindingException(string name) :
            base($"Attempted to access non-existent binding of name '{name}'.")
        { }
    }

    //public class UninitializedRegisterException : Exception
    //{
    //    internal UninitializedRegisterException(string registerName) :
    //        base($"Attempted to read value of uninitialized register '{registerName}'.")
    //    { }
    //}

    //public class StackUnderflowException<T> : Exception
    //{
    //    internal StackUnderflowException(Stack<T> stack) :
    //        base($"Attempted to pop data from empty {typeof(T).Name} stack in machine.")
    //    { }
    //}

    //public class InvalidNumericOperationException : Exception
    //{
    //    internal InvalidNumericOperationException(Number num, string op) :
    //        base(string.Format("Tried to perform illegal operation '{0}' on number {1}", op, num))
    //    { }

    //    internal InvalidNumericOperationException(Number num, string op, Number numArg) :
    //        base(string.Format("Tried to perform illegal operation '{0}' on numbers {1} and {2}", op, num, numArg))
    //    { }
    //}
}
