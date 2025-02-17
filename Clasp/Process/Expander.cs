using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Exceptions;
using Clasp.ExtensionMethods;

namespace Clasp.Process
{
    internal static class Expander
    {
        public static Syntax ExpandSyntax(Syntax input, ExpansionContext context)
        {
            return Expand(input, context);
        }

        private static Syntax Expand(Syntax stx, ExpansionContext context)
        {
            try
            {
                if (stx is Identifier id)
                {
                    return ExpandIdentifier(id, context);
                }
                else if (stx is SyntaxList idApp && idApp.Expose().Car is Identifier op)
                {
                    return ExpandIdApplication(op, idApp, context);
                }
                else if (stx is SyntaxList app)
                {
                    return ExpandApplication(app, context);
                }
                else
                {
                    return ExpandImplicit(Implicit.Sp_Datum, AsArg(stx), context);
                }
            }
            catch (ClaspException cex)
            {
                throw new ExpanderException.InvalidSyntax(stx, cex);
            }
        }

        #region Basic Expansion

        /// <summary>
        /// Expand an identifier as a standalone expression.
        /// </summary>
        private static Syntax ExpandIdentifier(Identifier id, ExpansionContext context)
        {
            if (id.TryResolveBinding(context.Phase, out CompileTimeBinding? binding))
            {
                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding, id, context);
                }
                else if (binding.BoundType == BindingType.Variable
                    || binding.BoundType == BindingType.Primitive)
                {
                    return ExpandImplicit(Implicit.Sp_Var, AsArg(id), context);
                }
            }
            else if (context.Mode != ExpansionMode.Module)
            {
                // indicate that it must be a top-level binding that doesn't exist yet
                return ExpandImplicit(Implicit.Sp_Top, AsArg(id), context);
            }

            throw new ExpanderException.InvalidSyntax(id);
        }

        /// <summary>
        /// Expand a function application form containing an identifier in the operator position.
        /// </summary>
        private static Syntax ExpandIdApplication(Identifier op, SyntaxList stl, ExpansionContext context)
        {
            if (op.TryResolveBinding(context.Phase, out CompileTimeBinding? binding))
            {
                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding, stl, context);
                }
                else if (binding.BoundType == BindingType.Special)
                {
                    return ExpandSpecialForm(binding.Name, stl, context);
                }
            }

            return ExpandApplication(stl, context);
        }

        /// <summary>
        /// Expand a function application form containing an arbitrary expression in the operator position.
        /// </summary>
        private static Syntax ExpandApplication(SyntaxList stl, ExpansionContext context)
        {
            try
            {
                Cons<Syntax, Term> args = ExpandExpressionList(stl.Expose(), stl.LexContext, context);
                SyntaxList stp = new SyntaxList(args, stl.LexContext);
                return ExpandImplicit(Implicit.Sp_Apply, stp, context);
            }
            catch (ExpanderException ee)
            {
                throw new ExpanderException.InvalidForm(Keyword.IMP_APP, stl, ee);
            }
        }

        private static SyntaxList AsArg(Syntax stx)
        {
            return new SyntaxList(stx, stx.LexContext);
        }

        /// <summary>
        /// Prepend <paramref name="stl"/> with a special <see cref="Identifier"/> that shares its
        /// <see cref="LexInfo"/>, indicating how it should be handled by the <see cref="Parser"/>.
        /// </summary>
        private static SyntaxList ExpandImplicit(Implicit formSym, SyntaxList stl, ExpansionContext context)
        {
            Identifier op = new Identifier(formSym, stl);
            return stl.Push(op);
        }

        /// <summary>
        /// Expand the invocation of a special syntactic form.
        /// </summary>
        /// <param name="formName">The the form's default keyword within the surface language.</param>
        /// <param name="stl">The entirety of the form's application expression.</param>
        private static Syntax ExpandSpecialForm(string formName, SyntaxList stl, ExpansionContext context)
        {
            if (stl.Expose() is not Cons<Syntax, Term> cns
                || cns.Car is not Identifier op
                || cns.Cdr is not Cons<Syntax, Term> args)
            {
                // all special forms require arguments
                throw new ExpanderException.InvalidSyntax(stl);
            }


            LexInfo info = stl.LexContext;
            Cons<Syntax, Term> expandedTail;

            try
            {
                if (formName == Keyword.BEGIN_FOR_SYNTAX)
                {
                    BeginForSyntax(stl, context);
                    return new Datum(VoidTerm.Value, stl);
                }

                expandedTail = formName switch
                {
                    Keyword.IMP_PARDEF => ExpandPartialDefineArgs(args, info, context),

                    Keyword.DEFINE => ExpandDefineArgs(args, info, context),
                    Keyword.DEFINE_SYNTAX => ExpandDefineSyntaxArgs(args, info, context),
                    Keyword.SET => ExpandSetArgs(args, info, context),

                    Keyword.QUOTE => args, // just leave them alone for now
                    Keyword.QUOTE_SYNTAX => args,

                    Keyword.LAMBDA => ExpandLambdaArgs(args, info, context),
                    Keyword.IMP_LAMBDA => ExpandLambdaArgs(args, info, context),

                    Keyword.IF => ExpandIfArgs(args, info, context),
                    Keyword.BEGIN => ExpandSequence(args, info, context),

                    Keyword.MODULE => ExpandModuleArgs(args, info, context),

                    Keyword.IMPORT => args,

                    _ => throw new ExpanderException.InvalidSyntax(stl)
                };
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.InvalidForm(formName, stl, e);
            }

            return new SyntaxList(Cons.Truct<Syntax, Term>(op, expandedTail), info);
        }

        /// <summary>
        /// Partially expand <paramref name="stx"/>
        /// </summary>
        private static Syntax? PartiallyExpandBodyTerm(Syntax stx, ExpansionContext context)
        {
            // essentially just check the path to special forms and disregard otherwise
            if (stx is SyntaxList stp
                && stp.Expose().Car is Identifier op
                && op.TryResolveBinding(context.Phase, out CompileTimeBinding? binding)
                && binding.BoundType == BindingType.Special)
            {
                Cons<Syntax, Term> args = stp.Expose();

                if (binding.Name == Keyword.DEFINE_SYNTAX)
                {
                    // expand and bind the macro, then discard the syntax
                    ExpandDefineSyntaxArgs(args, stx.LexContext, context);
                    return null;
                }
                else if (binding.Name == Keyword.DEFINE)
                {
                    // extract and rename the key, then rewrite the form to indicate we did so

                    if (TryRewriteDefineArgs(args, out Identifier? key, out Syntax? value))
                    {
                        if (!key.TryRenameAsVariable(context.Phase, out _))
                        {
                            throw new ExpanderException.InvalidBindingOperation(key, context);
                        }

                        Identifier newOp = new Identifier(Implicit.Par_Def, op);

                        return new SyntaxList(value, op.LexContext)
                            .Push(key)
                            .Push(newOp);
                    }
                    else
                    {
                        throw new ExpanderException.InvalidForm(Keyword.DEFINE, stx);
                    }
                }

                // let-syntax isn't partially expanded because it cannot legally expand into a definition
            }
            return stx;
        }

        #endregion

        #region Macro-Related

        private static Syntax ExpandSyntaxTransformation(CompileTimeBinding binding, Syntax input, ExpansionContext context)
        {
            if (context.TryLookupMacro(binding, out MacroProcedure? macro))
            {
                Scope introScope = new Scope(input);
                Scope useSiteScope = new Scope(input);

                input.AddScope(context.Phase, introScope, useSiteScope);

                Syntax output = ApplySyntaxTransformer(macro, input);

                ExpansionContext macroContext = context.InTransformed(useSiteScope);
                output.FlipScope(macroContext.Phase, useSiteScope);
                context.AddPendingInsideEdge(output);

                return Expand(output, macroContext);
            }
            else
            {
                throw new ExpanderException.UnboundMacro(binding.Id);
            }
        }

        private static Syntax ApplySyntaxTransformer(MacroProcedure macro, Syntax input)
        {
            Term output;

            try
            {
                MacroApplication program = new MacroApplication(macro, input);
                output = Interpreter.InterpretProgram(program);
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.EvaluationError(nameof(MacroProcedure), input, e);
            }

            if (output is not Syntax outputStx)
            {
                throw new ExpanderException.InvalidTransformation(output, macro, input);
            }

            return outputStx;
        }

        private static MacroProcedure ExpandAndEvalMacro(Syntax input, ExpansionContext context)
        {
            ExpansionContext subState = context.InNewPhase();

            Term output;

            try
            {
                Syntax expandedInput = ExpandSyntax(input, subState);
                CoreForm parsedInput = Parser.ParseSyntax(expandedInput, subState);
                output = Interpreter.InterpretProgram(parsedInput, context.CompileTimeEnv.GlobalEnv.Enclose());
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.EvaluationError(nameof(MacroProcedure), input, e);
            }

            if (output is CompoundProcedure cp
                && cp.TryCoerceMacro(out MacroProcedure? macro))
            {
                return macro;
            }

            throw new ExpanderException.InvalidTransformer(output, input);
        }

        private static void BeginForSyntax(SyntaxList form, ExpansionContext context)
        {
            Cons<Syntax, Term> terms = form.Expose();
            LexInfo info = form.LexContext;
            ExpansionContext subState = context.InNewPhase();

            Term output;

            try
            {
                // Treat the sequent terms like a regular Begin form, but in the substate
                Cons<Syntax, Term> expandedSequence = ExpandSequence(terms, info, subState);
                SyntaxList stxSequence = new SyntaxList(expandedSequence, info);
                SyntaxList beginStx = ExpandImplicit(Implicit.Sp_Begin, stxSequence, subState);

                // Nest the first Begin form inside a second, to add an implicit return value (#t)
                SyntaxList nestedStx = new SyntaxList(new Datum(Boolean.True, info), info)
                    .Push(beginStx);
                nestedStx = ExpandImplicit(Implicit.Sp_Begin, nestedStx, subState);

                // Parse (still in the substate), which will de-nest the final list of terms
                CoreForm parsedInput = Parser.ParseSyntax(beginStx, subState);

                // Now interpret the parse, but back in THIS expansion's context
                // Which allows for mutation of the current compile-time environment
                output = Interpreter.InterpretProgram(parsedInput, context.CompileTimeEnv.GlobalEnv.Enclose());
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.EvaluationError(Keyword.BEGIN_FOR_SYNTAX, form, e);
            }

            // Check that the interpretation returned #t as expected
            if (output is not Boolean b || b != Boolean.True)
            {
                throw new ExpanderException.EvaluationError(Keyword.BEGIN_FOR_SYNTAX, form,
                    string.Format("Compile-Time Interpretation yielded an unexpected value: {0}", output));
            }
        }

        #endregion

        #region Recurrent Forms

        /// <summary>
        /// Expand a proper list of expressions.
        /// </summary>
        private static Cons<Syntax, Term> ExpandExpressionList(Cons<Syntax, Term> stp, LexInfo ctx, ExpansionContext context)
        {
            Syntax expandedCar = Expand(stp.Car, context.AsExpression());

            if (stp.Cdr is Nil n)
            {
                return Cons.Truct<Syntax, Term>(expandedCar, n);
            }
            else if (stp.Cdr is Cons<Syntax, Term> cdr)
            {
                Cons<Syntax, Term> expandedCdr = ExpandExpressionList(cdr, ctx, context);
                return Cons.Truct<Syntax, Term>(expandedCar, expandedCdr);
            }

            throw new ExpanderException.ExpectedProperList(nameof(Syntax), stp, ctx);
        }

        /// <summary>
        /// Expand and bind renames for a list of Identifier terms. No mutation takes place, so nothing is returned.
        /// </summary>
        private static void ExpandParameterList(Term t, LexInfo info, ExpansionContext context)
        {
            if (t is Nil || (t is Datum dat && dat.Expose() is Nil))
            {
                return;
            }
            else if (t is Identifier dottedParam)
            {
                if (!dottedParam.TryRenameAsVariable(context.Phase, out _))
                {
                    throw new ExpanderException.InvalidBindingOperation(dottedParam, context);
                }
                return;
            }
            else if (t is Cons cns && cns.Car is Identifier nextParam)
            {
                if (!nextParam.TryRenameAsVariable(context.Phase, out _))
                {
                    throw new ExpanderException.InvalidBindingOperation(nextParam, context);
                }
                ExpandParameterList(cns.Cdr, info, context);
            }
            else if (t is SyntaxList stl)
            {
                ExpandParameterList(stl.Expose(), info, context);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(nameof(Identifier), t, info);
            }
        }

        /// <summary>
        /// Partially expand a sequence of terms to capture any requisite bindings,
        /// then properly run through and expand each term in sequence.
        /// </summary>
        private static Cons<Syntax, Term> ExpandBody(Cons<Syntax, Term> stp, LexInfo info, ExpansionContext context)
        {
            Cons<Syntax, Term> partiallyExpandedBody = PartiallyExpandBody(stp, info, context);

            return ExpandSequence(partiallyExpandedBody, info, context);
        }

        /// <summary>
        /// Recur through a sequence of terms, recording bindings for any definitions,
        /// (and discarding those definitions if they bind macros)
        /// </summary>
        private static Cons<Syntax, Term> PartiallyExpandBody(Cons<Syntax, Term> stp, LexInfo info, ExpansionContext context)
        {
            Syntax? partiallyExpandedBodyTerm = PartiallyExpandBodyTerm(stp.Car, context);

            if (partiallyExpandedBodyTerm is null)
            {
                // internal syntax definitions are recorded, then discarded

                if (stp.Cdr is Nil)
                {
                    throw new ExpanderException.InvalidContext("Definition",
                        ExpansionMode.Expression, stp, info);
                }
                else if (stp.Cdr is Cons<Syntax, Term> cdr)
                {
                    return PartiallyExpandBody(cdr, info, context);
                }
            }
            else
            {
                if (stp.Cdr is Nil n)
                {
                    return Cons.Truct<Syntax, Term>(partiallyExpandedBodyTerm, n);
                }
                else if (stp.Cdr is Cons<Syntax, Term> cdr)
                {
                    Cons<Syntax, Term> partiallyExpandedTail = PartiallyExpandBody(cdr, info, context);
                    return Cons.Truct<Syntax, Term>(partiallyExpandedBodyTerm, partiallyExpandedTail);
                }
            }

            throw new ExpanderException.ExpectedProperList(nameof(Syntax), stp, info);
        }

        /// <summary>
        /// Recur through a sequence of terms, expanding and replacing each one.
        /// The final term is expected to be a <see cref="ExpansionMode.Expression"/>.
        /// </summary>
        private static Cons<Syntax, Term> ExpandSequence(Cons<Syntax, Term> stxList, LexInfo info, ExpansionContext context)
        {
            if (stxList.Car is Syntax stx)
            {
                if (stxList.Cdr is Nil n)
                {
                    ExpansionContext finalTermContext = context.Mode != ExpansionMode.Module
                        ? context.AsExpression()
                        : context;

                    Syntax expandedCar = Expand(stx, finalTermContext);
                    return Cons.Truct<Syntax, Term>(expandedCar, n);
                }
                else if (stxList.Cdr is Cons<Syntax, Term> cdr)
                {
                    Syntax expandedCar = Expand(stx, context);
                    Cons<Syntax, Term> expandedCdr = ExpandSequence(cdr, info, context);
                    return Cons.Truct<Syntax, Term>(expandedCar, expandedCdr);
                }
            }

            throw new ExpanderException.ExpectedProperList(nameof(Syntax), stxList, info);
        }

        /// <summary>
        /// Recur through a sequence of terms, where each is expected to be a let-syntax binding pair.
        /// </summary>
        private static void ExpandLetSyntaxBindingList(Cons<Syntax, Term> stp, LexInfo info, ExpansionContext context)
        {
            if (stp.Car is SyntaxList stl)
            {
                ExpandDefineSyntaxArgs(stl.Expose(), info, context);

                if (stp.Cdr is Nil)
                {
                    return;
                }
                else if (stp.Cdr is Cons<Syntax, Term> cdr)
                { 
                    ExpandLetSyntaxBindingList(cdr, info, context);
                }
            }

            throw new ExpanderException.ExpectedProperList(nameof(SyntaxList), stp, info);
        }

        #endregion

        #region Native Rewriting Methods

        /// <summary>
        /// Given the arguments to a <see cref="Keyword.DEFINE"/> form,
        /// rewrite them explicitly into the standard key/value pair format.
        /// </summary>
        private static bool TryRewriteDefineArgs(Cons<Syntax, Term> stp,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? value)
        {
            // The arguments to 'define' can come in two formats:
            // - (define key value)
            // - (define (key . formals) . body)

            if (TryDestructKeyValuePair(stp, out key, out value))
            {
                return true;
            }
            else if (TryDestructImplicitLambda(stp,
                out key, out Syntax? formals, out Cons<Syntax, Term>? body))
            {
                value = BuildLambda(formals, body);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Given the arguments to a <see cref="Keyword.DEFINE_SYNTAX"/> form,
        /// rewrite them explicitly into the standard key/lambda pair format.
        /// </summary>
        private static bool TryRewriteDefineSyntaxArgs(Cons<Syntax, Term> stp, ExpansionContext context,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? value)
        {
            // The arguments to 'define-syntax' can come in three formats:
            // - (define key (lambda ...))
            // - (define key value)
            // - (define (key . formals) . body)

            if (TryDestructExplicitLambda(stp, context, out key, out value))
            {
                return true;
            }
            else if (TryDestructKeyValuePair(stp, out key, out value))
            {
                // rewrite the non-lambda term as a parameter-less procedure that returns the term
                Datum formals = new Datum(Nil.Value, value.LexContext);
                Cons<Syntax, Term> body = Cons.Truct<Syntax, Term>(value, Nil.Value);
                value = BuildLambda(formals, body);

                return true;
            }
            else if (TryDestructImplicitLambda(stp, out key,
                out Syntax? formals, out Cons<Syntax, Term>? body))
            {
                value = BuildLambda(formals, body);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static SyntaxList BuildLambda(Syntax formals, Cons<Syntax, Term> body)
        {
            Cons<Syntax, Term> args = Cons.Truct<Syntax, Term>(formals, body);
            Identifier op = new Identifier(Implicit.Sp_Lambda, formals);
            Cons<Syntax, Term> lambda = Cons.Truct<Syntax, Term>(op, args);

            return new SyntaxList(lambda, body.Car.LexContext);
        }

        // (define-whatever (key . formals) . body)
        private static bool TryDestructImplicitLambda(Cons<Syntax, Term> stp,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? formals,
            [NotNullWhen(true)] out Cons<Syntax, Term>? body)
        {
            if (stp.TryMatchLeading(out SyntaxList? nameAndFormals, out Term? tail)
                && nameAndFormals.Expose().TryMatchLeading(out key, out Term? maybeFormals)
                && (tail is Cons<Syntax, Term> outBody))
            {
                body = outBody;

                if (maybeFormals is Nil n)
                {
                    formals = new Datum(n, key.LexContext);
                    return true;
                }
                else if (maybeFormals is Identifier dottedFormal)
                {
                    formals = dottedFormal;
                    return true;
                }
                else if (maybeFormals is Cons<Syntax, Term> fp)
                {
                    formals = new SyntaxList(fp, key.LexContext);
                    return true;
                }
            }

            key = null;
            formals = null;
            body = null;
            return false;
        }

        // (define-whatever key value)
        private static bool TryDestructKeyValuePair(Cons<Syntax, Term> stp,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? value)
        {
            return stp.TryMatchOnly(out key, out value);
        }

        // (define-whatever key (lambda ...))
        private static bool TryDestructExplicitLambda(Cons<Syntax, Term> stp, ExpansionContext context,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? value)
        {
            return TryDestructKeyValuePair(stp, out key, out value)
                && value.Expose() is Cons<Syntax, Term> maybeLambda
                && maybeLambda.Car is Identifier maybeOp
                && maybeOp.TryResolveBinding(context.Phase, out CompileTimeBinding? binding)
                && (binding.Name == Keyword.LAMBDA || binding.Name == Keyword.IMP_LAMBDA);
        }

        /// <summary>
        /// Given the arguments to a <see cref="Keyword.IF"/> form,
        /// rewrite them explicitly into the standard cond/then/else format.
        /// </summary>
        private static bool TryRewriteIfArgs(Cons<Syntax, Term> stp,
            [NotNullWhen(true)] out Syntax? condValue,
            [NotNullWhen(true)] out Syntax? thenValue,
            [NotNullWhen(true)] out Syntax? elseValue)
        {
            // The arguments to 'if' can come in two formats:
            // - (if cond then else)
            // - (if cond then)

            if (stp.TryMatchLeading(out condValue, out thenValue, out Term? tail))
            {
                if (tail is Nil n)
                {
                    elseValue = new Datum(Boolean.False, thenValue.LexContext);
                    return true;
                }
                else if (tail is Cons cns && cns.TryMatchOnly(out elseValue))
                {
                    return true;
                }
            }

            condValue = null;
            thenValue = null;
            elseValue = null;
            return false;

        }
        #endregion

        #region Syntactic Form Expansion

        private static Cons<Syntax, Term> ExpandPartialDefineArgs(Cons<Syntax, Term> stp, LexInfo info, ExpansionContext context)
        {
            if (context.Mode != ExpansionMode.InternalDefinition)
            {
                throw new ExpanderException.InvalidContext(Keyword.IMP_PARDEF, context.Mode, stp, info);
            }
            else if (TryRewriteDefineArgs(stp, out Identifier? key, out Syntax? value))
            {
                // key has already been renamed in the partial pass

                Syntax expandedValue = Expand(value, context.AsExpression());

                return Cons.Truct<Syntax, Term>(key, value);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static Cons<Syntax, Term> ExpandDefineArgs(Cons<Syntax, Term> stp, LexInfo info, ExpansionContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keyword.DEFINE, context.Mode, stp, info);
            }
            else if (TryRewriteDefineArgs(stp, out Identifier? key, out Syntax? value))
            {
                context.SanitizeIdentifier(key);

                if (!key.TryRenameAsVariable(context.Phase, out _)
                    && !key.TryResolveBinding(context.Phase, out _))
                {
                    throw new ExpanderException.InvalidBindingOperation(key, context);
                }

                Syntax expandedValue = Expand(value, context.AsExpression());

                return new Cons<Syntax, Term>(key, Cons.Truct(value, Nil.Value));
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static Cons<Syntax, Term> ExpandDefineSyntaxArgs(Cons<Syntax, Term> stp, LexInfo info, ExpansionContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keyword.DEFINE_SYNTAX, context.Mode, stp, info);
            }
            else if (TryRewriteDefineSyntaxArgs(stp, context, out Identifier? key, out Syntax? value))
            {
                context.SanitizeIdentifier(key);

                MacroProcedure macro = ExpandAndEvalMacro(value, context);
                if (!key.TryRenameAsMacro(context.Phase, out Identifier? bindingId))
                {
                    throw new ExpanderException.InvalidBindingOperation(key, context);
                }
                else
                {
                    context.CompileTimeEnv[bindingId.Name] = macro;
                }

                Syntax evaluatedMacro = ExpandImplicit(Implicit.Sp_Datum, AsArg(new Datum(macro, value)), context);

                return new Cons<Syntax, Term>(key, Cons.Truct(evaluatedMacro, Nil.Value));
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static Cons<Syntax, Term> ExpandSetArgs(Cons<Syntax, Term> stp, LexInfo info, ExpansionContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keyword.SET, context.Mode, stp, info);
            }
            else if (TryRewriteDefineArgs(stp, out Identifier? key, out Syntax? value))
            {
                context.SanitizeIdentifier(key);

                CompileTimeBinding binding = ResolveBinding(key, context);
                Syntax expandedValue = Expand(value, context.AsExpression());

                return new Cons<Syntax, Term>(key, Cons.Truct(expandedValue, Nil.Value));
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static Cons<Syntax, Term> ExpandIfArgs(Cons<Syntax, Term> stp, LexInfo info, ExpansionContext context)
        {
            if (TryRewriteIfArgs(stp,
                out Syntax? condValue,
                out Syntax? thenValue,
                out Syntax? elseValue))
            {
                Syntax expandedCond = Expand(condValue, context.AsExpression());
                Syntax expandedThen = Expand(thenValue, context.AsExpression());
                Syntax expandedElse = Expand(elseValue, context.AsExpression());

                return new Cons<Syntax, Term>(expandedCond, Cons.Truct(expandedThen, Cons.Truct(expandedElse, Nil.Value)));
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static Cons<Syntax, Term> ExpandLambdaArgs(Cons<Syntax, Term> stp, LexInfo info, ExpansionContext context)
        {
            if (stp.TryMatchLeading(out Syntax? formals, out Term? cdr)
                && (formals.Expose() is Nil or Cons<Syntax, Term> || formals is Identifier)
                && cdr is Cons<Syntax, Term> body)
            {
                Scope outsideEdge = new Scope(formals);
                Scope insideEdge = new Scope(formals);

                formals.AddScope(context.Phase, outsideEdge, insideEdge);
                formals.AddScope(context.Phase, outsideEdge, insideEdge);

                ExpansionContext bodyContext = context.InBody(insideEdge);

                ExpandParameterList(formals, info, context);

                Cons<Syntax, Term> expandedBody = ExpandBody(body, info, bodyContext);

                return Cons.Truct<Syntax, Term>(formals, expandedBody);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static Cons<Syntax, Term> ExpandModuleArgs(Cons<Syntax, Term> cns, LexInfo info, ExpansionContext context)
        {
            if (cns.TryMatchLeading(out Identifier? id, out Term rest)
                && rest is Cons<Syntax, Term> body)
            {
                ExpansionContext moduleContext = context.InModule();
                Cons<Syntax, Term> expandedBody = ExpandBody(body, info, moduleContext);

                return Cons.Truct<Syntax, Term>(id, expandedBody);
            }

            throw new ExpanderException.InvalidArguments(cns, info);
        }

        #endregion

        private static CompileTimeBinding ResolveBinding(Identifier id, ExpansionContext context)
        {
            if (id.TryResolveBinding(context.Phase, out CompileTimeBinding? binding))
            {
                return binding;
            }
            else
            {
                throw new ExpanderException.UnboundIdentifier(id);
            }
        }
    }
}
