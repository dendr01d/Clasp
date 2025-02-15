using System.Diagnostics.CodeAnalysis;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.ExtensionMethods;

using static System.Net.WebRequestMethods;

namespace Clasp.Process
{
    internal static class Expander
    {
        public static Syntax ExpandSyntax(Syntax input, Environment compileTimeEnv, int phase = 1)
        {
            return Expand(input, new ExpansionContext(compileTimeEnv, phase));
        }

        private static Syntax Expand(Syntax stx, ExpansionContext context)
        {
            if (stx is Identifier id)
            {
                return ExpandIdentifier(id, context);
            }
            else if (stx is SyntaxList idApp && idApp.Car is Identifier op)
            {
                return ExpandIdApplication(op, idApp, context);
            }
            else if (stx is SyntaxList app)
            {
                return ExpandApplication(app, context);
            }
            else
            {
                return ExpandImplicit(Implicit.SpDatum, AsArg(stx), context);
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
                    return ExpandImplicit(Implicit.SpVar, AsArg(id), context);
                }
            }
            else if (context.Mode != ExpansionMode.Module)
            {
                // indicate that it must be a top-level binding that doesn't exist yet
                return ExpandImplicit(Implicit.SpTop, AsArg(id), context);
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
                StxPair args = ExpandExpressionList(stl.Expose(), stl.LexContext, context);
                SyntaxList stp = new SyntaxList(args, stl.LexContext);
                return ExpandImplicit(Implicit.SpApply, stp, context);
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
        private static Syntax ExpandImplicit(Implicit formSym, SyntaxList stl, ExpansionContext context)
        {
            Identifier op = new Identifier(formSym, stl);
            return stl.Prepend(op);
        }

        /// <summary>
        /// Expand the invocation of a special syntactic form.
        /// </summary>
        /// <param name="formName">The the form's default keyword within the surface language.</param>
        /// <param name="stl">The entirety of the form's application expression.</param>
        private static Syntax ExpandSpecialForm(string formName, SyntaxList stl, ExpansionContext context)
        {
            if (stl.Expose() is not StxPair stp
                || stp.Car is not Identifier op
                || stp.Cdr is not StxPair tail)
            {
                // all special forms require arguments
                throw new ExpanderException.InvalidSyntax(stl);
            }

            LexInfo info = stl.LexContext;
            StxPair expandedTail;

            try
            {
                expandedTail = formName switch
                {
                    Keyword.IMP_PARDEF => ExpandPartialDefineArgs(tail, info, context),

                    Keyword.DEFINE => ExpandDefineArgs(tail, info, context),
                    Keyword.DEFINE_SYNTAX => ExpandDefineSyntaxArgs(tail, info, context),
                    Keyword.SET => ExpandSetArgs(tail, info, context),

                    Keyword.QUOTE => tail, // just leave them alone for now
                    Keyword.QUOTE_SYNTAX => tail,

                    Keyword.LAMBDA => ExpandLambdaArgs(tail, info, context),

                    Keyword.IF => ExpandIfArgs(tail, info, context),
                    Keyword.BEGIN => ExpandSequence(tail, info, context),

                    _ => throw new ExpanderException.InvalidSyntax(stl)
                };
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.InvalidForm(formName, stl, e);
            }

            return new SyntaxList(StxPair.Cons(op, expandedTail), info);
        }

        /// <summary>
        /// Partially expand <paramref name="stx"/>
        /// </summary>
        private static Syntax? PartiallyExpandBodyTerm(Syntax stx, ExpansionContext context)
        {
            // essentially just check the path to special forms and disregard otherwise
            if (stx is SyntaxList stp
                && stp.Car is Identifier op
                && op.TryResolveBinding(context.Phase, out CompileTimeBinding? binding)
                && binding.BoundType == BindingType.Special)
            {
                StxPair args = stp.Expose();

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

                        Identifier newOp = new Identifier(Implicit.ParDef, op);
                        StxPair form = StxPair.ProperList(newOp, key, value);

                        return new SyntaxList(form, op.LexContext);
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
                throw new ExpanderException.WrongEvaluatedType(nameof(Syntax), output, input);
            }

            return outputStx;
        }

        private static MacroProcedure ExpandAndEvalMacro(Syntax input, ExpansionContext context)
        {
            ExpansionContext subState = context.InNewPhase();

            Term output;

            try
            {
                Syntax expandedInput = Expand(input, subState);
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

            throw new ExpanderException.WrongEvaluatedType(nameof(MacroProcedure), output, input);
        }

        #endregion

        #region Recurrent Forms

        /// <summary>
        /// Expand a proper list of expressions.
        /// </summary>
        private static StxPair ExpandExpressionList(StxPair stp, LexInfo ctx, ExpansionContext context)
        {
            Syntax expandedCar = Expand(stp.Car, context.AsExpression());

            if (stp.Cdr is Nil n)
            {
                return StxPair.Cons(expandedCar, n);
            }
            else if (stp.Cdr is StxPair cdr)
            {
                StxPair expandedCdr = ExpandExpressionList(cdr, ctx, context);
                return StxPair.Cons(expandedCar, expandedCdr);
            }

            throw new ExpanderException.ExpectedProperList(nameof(Syntax), stp, ctx);
        }

        /// <summary>
        /// Expand and bind renames for a list of Identifier terms. No mutation takes place, so nothing is returned.
        /// </summary>
        private static void ExpandParameterList(StxPair stp, LexInfo ctx, ExpansionContext context)
        {
            if (stp.Car is Identifier id)
            {
                if (!id.TryRenameAsVariable(context.Phase, out _))
                {
                    throw new ExpanderException.InvalidBindingOperation(id, context);
                }

                if (stp.Cdr is Nil)
                {
                    return;
                }
                else if (stp.Cdr is StxPair cdr)
                {
                    ExpandParameterList(cdr, ctx, context);
                }
            }

            throw new ExpanderException.ExpectedProperList(nameof(Identifier), stp, ctx);
        }

        /// <summary>
        /// Partially expand a sequence of terms to capture any requisite bindings,
        /// then properly run through and expand each term in sequence.
        /// </summary>
        private static StxPair ExpandBody(StxPair stp, LexInfo info, ExpansionContext context)
        {
            StxPair partiallyExpandedBody = PartiallyExpandBody(stp, info, context);

            return ExpandSequence(partiallyExpandedBody, info, context);
        }

        /// <summary>
        /// Recur through a sequence of terms, recording bindings for any definitions,
        /// (and discarding those definitions if they bind macros)
        /// </summary>
        private static StxPair PartiallyExpandBody(StxPair stp, LexInfo info, ExpansionContext context)
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
                else if (stp.Cdr is StxPair cdr)
                {
                    return PartiallyExpandBody(cdr, info, context);
                }
            }
            else
            {
                if (stp.Cdr is Nil n)
                {
                    return StxPair.Cons(partiallyExpandedBodyTerm, n);
                }
                else if (stp.Cdr is StxPair cdr)
                {
                    StxPair partiallyExpandedTail = PartiallyExpandBody(cdr, info, context);
                    return StxPair.Cons(partiallyExpandedBodyTerm, partiallyExpandedTail);
                }
            }

            throw new ExpanderException.ExpectedProperList(nameof(Syntax), stp, info);
        }

        /// <summary>
        /// Recur through a sequence of terms, expanding and replacing each one.
        /// The final term is expected to be a <see cref="ExpansionMode.Expression"/>.
        /// </summary>
        private static StxPair ExpandSequence(StxPair stxList, LexInfo info, ExpansionContext context)
        {
            if (stxList.Car is Syntax stx)
            {
                if (stxList.Cdr is Nil n)
                {
                    Syntax expandedCar = Expand(stx, context.AsExpression());
                    return StxPair.Cons(expandedCar, n);
                }
                else if (stxList.Cdr is StxPair cdr)
                {
                    Syntax expandedCar = Expand(stx, context.AsExpression());
                    StxPair expandedCdr = ExpandSequence(cdr, info, context);
                    return StxPair.Cons(expandedCar, expandedCdr);
                }
            }

            throw new ExpanderException.ExpectedProperList(nameof(Syntax), stxList, info);
        }

        /// <summary>
        /// Recur through a sequence of terms, where each is expected to be a let-syntax binding pair.
        /// </summary>
        private static void ExpandLetSyntaxBindingList(StxPair stp, LexInfo info, ExpansionContext context)
        {
            if (stp.Car is SyntaxList stl)
            {
                ExpandDefineSyntaxArgs(stl.Expose(), info, context);

                if (stp.Cdr is Nil)
                {
                    return;
                }
                else if (stp.Cdr is StxPair cdr)
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
        private static bool TryRewriteDefineArgs(StxPair stp,
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
                out key, out Syntax? formals, out StxPair? body))
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
        private static bool TryRewriteDefineSyntaxArgs(StxPair stp, ExpansionContext context,
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
                StxPair body = StxPair.Cons(value, Nil.Value);
                value = BuildLambda(formals, body);

                return true;
            }
            else if (TryDestructImplicitLambda(stp, out key,
                out Syntax? formals, out StxPair? body))
            {
                value = BuildLambda(formals, body);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static Syntax BuildLambda(Syntax formals, StxPair body)
        {
            StxPair args = StxPair.Cons(formals, body);
            Identifier op = new Identifier(Implicit.SpLambda, formals);
            StxPair lambda = StxPair.Cons(op, args);

            return new SyntaxList(lambda, body.Car.LexContext);
        }

        // (define-whatever (key . formals) . body)
        private static bool TryDestructImplicitLambda(StxPair stp,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? formals,
            [NotNullWhen(true)] out StxPair? body)
        {
            if (stp.TryMatchLeading(out SyntaxList? nameAndFormals, out Term? tail)
                && nameAndFormals.Expose().TryMatchLeading(out key, out Term? maybeFormals)
                && (tail is StxPair outBody))
            {

                if (maybeFormals is Nil n)
                {
                    formals = new Datum(n, key.LexContext);
                    body = outBody;
                    return true;
                }
                else if (maybeFormals is StxPair fp)
                {
                    formals = new SyntaxList(fp, key.LexContext);
                    body = outBody;
                    return true;
                }
            }

            key = null;
            formals = null;
            body = null;
            return false;
        }

        // (define-whatever key value)
        private static bool TryDestructKeyValuePair(StxPair stp,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? value)
        {
            return stp.TryMatchOnly(out key, out value);
        }

        // (define-whatever key (lambda ...))
        private static bool TryDestructExplicitLambda(StxPair stp, ExpansionContext context,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? value)
        {
            return TryDestructKeyValuePair(stp, out key, out value)
                && value.Expose() is StxPair maybeLambda
                && maybeLambda.Car is Identifier maybeOp
                && maybeOp.TryResolveBinding(context.Phase, out CompileTimeBinding? binding)
                && (binding.Name == Keyword.LAMBDA || binding.Name == Keyword.IMP_LAMBDA);
        }

        /// <summary>
        /// Given the arguments to a <see cref="Keyword.IF"/> form,
        /// rewrite them explicitly into the standard cond/then/else format.
        /// </summary>
        private static bool TryRewriteIfArgs(StxPair stp,
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

        #region Special Form Expansion

        private static StxPair ExpandPartialDefineArgs(StxPair stp, LexInfo info, ExpansionContext context)
        {
            if (context.Mode != ExpansionMode.InternalDefinition)
            {
                throw new ExpanderException.InvalidContext(Keyword.IMP_PARDEF, context.Mode, stp, info);
            }
            else if (TryRewriteDefineArgs(stp, out Identifier? key, out Syntax? value))
            {
                // key has already been renamed in the partial pass

                Syntax expandedValue = Expand(value, context.AsExpression());

                return StxPair.Cons(key, value);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static StxPair ExpandDefineArgs(StxPair stp, LexInfo info, ExpansionContext context)
        {
            if (context.Mode != ExpansionMode.InternalDefinition)
            {
                throw new ExpanderException.InvalidContext(Keyword.DEFINE, context.Mode, stp, info);
            }
            else if (TryRewriteDefineArgs(stp, out Identifier? key, out Syntax? value))
            {
                context.SanitizeIdentifier(key);
                if (!key.TryRenameAsVariable(context.Phase, out _))
                {
                    throw new ExpanderException.InvalidBindingOperation(key, context);
                }

                Syntax expandedValue = Expand(value, context.AsExpression());

                return StxPair.Cons(key, value);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static StxPair ExpandDefineSyntaxArgs(StxPair stp, LexInfo info, ExpansionContext context)
        {
            if (context.Mode != ExpansionMode.InternalDefinition)
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

                Syntax evaluatedMacro = ExpandImplicit(Implicit.SpDatum, AsArg(new Datum(macro, value)), context);

                return StxPair.Cons(key, evaluatedMacro);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static StxPair ExpandSetArgs(StxPair stp, LexInfo info, ExpansionContext context)
        {
            if (context.Mode != ExpansionMode.InternalDefinition)
            {
                throw new ExpanderException.InvalidContext(Keyword.SET, context.Mode, stp, info);
            }
            else if (TryRewriteDefineArgs(stp, out Identifier? key, out Syntax? value))
            {
                context.SanitizeIdentifier(key);

                if (key.TryResolveBinding(context.Phase,
                    out CompileTimeBinding? binding,
                    out CompileTimeBinding[] candidates))
                {
                    Syntax expandedValue = Expand(value, context.AsExpression());

                    return StxPair.Cons(key, expandedValue);
                }
                else if (candidates.Length > 1)
                {
                    throw new ExpanderException.AmbiguousIdentifier(key, candidates);
                }
                else
                {
                    throw new ExpanderException.UnboundIdentifier(key);
                }
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static StxPair ExpandIfArgs(StxPair stp, LexInfo info, ExpansionContext context)
        {
            if (TryRewriteIfArgs(stp,
                out Syntax? condValue,
                out Syntax? thenValue,
                out Syntax? elseValue))
            {
                Syntax expandedCond = Expand(condValue, context.AsExpression());
                Syntax expandedThen = Expand(thenValue, context.AsExpression());
                Syntax expandedElse = Expand(elseValue, context.AsExpression());

                return StxPair.ProperList(expandedCond, expandedThen, expandedElse);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static StxPair ExpandLambdaArgs(StxPair stp, LexInfo info, ExpansionContext context)
        {
            if (stp.TryMatchLeading(out Syntax? formals, out Term? cdr)
                && formals.Expose() is Nil or StxPair
                && cdr is StxPair body)
            {
                Scope outsideEdge = new Scope(formals);
                Scope insideEdge = new Scope(formals);

                formals.AddScope(context.Phase, outsideEdge, insideEdge);
                formals.AddScope(context.Phase, outsideEdge, insideEdge);

                ExpansionContext bodyContext = context.InBody(insideEdge);

                if (formals.Expose() is StxPair fp)
                {
                    ExpandParameterList(fp, info, bodyContext);
                }
                // nil parameter list needn't be expanded

                StxPair expandedBody = ExpandBody(body, info, bodyContext);

                return StxPair.Cons(formals, expandedBody);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        #endregion
    }
}
