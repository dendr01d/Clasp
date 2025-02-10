using System.Diagnostics.CodeAnalysis;

using Clasp.Binding;
using Clasp.Data;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Syntax;
using Clasp.ExtensionMethods;

namespace Clasp.Process
{
    internal static class Expander
    {
        public static Syntax ExpandSyntax(Syntax input, ExpansionContext exState)
        {
            //exState = ExpansionContext.FreshExpansion(env, gen);
            return ExpandAsTop(input, exState);
        }

        #region Dispatching

        private static Syntax ExpandAsTop(Syntax stx, ExpansionContext exState)
            => Expand(stx, exState.InSyntaxMode(SyntaxMode.TopLevel));

        private static Syntax ExpandAsExpression(Syntax stx, ExpansionContext exState)
            => Expand(stx, exState.InSyntaxMode(SyntaxMode.Expression));

        private static Syntax ExpandAsBodyTerm(Syntax stx, ExpansionContext exState)
            => Expand(stx, exState.InSyntaxMode(SyntaxMode.InternalDefinition));

        private static Syntax Expand(Syntax stx, ExpansionContext exState)
        {
            if (stx is Identifier id)
            {
                return ExpandIdentifier(id, exState);
            }
            else if (stx is SyntaxPair idApp && idApp.Car is Identifier op)
            {
                return ExpandIdApplication(op, idApp, exState);
            }
            else if (stx is SyntaxPair app)
            {
                return ExpandApplication(app, exState);
            }
            else
            {
                return ExpandImplicit(Implicit.SpDatum, AsArg(stx), exState);
            }
        }

        #endregion

        #region Basic Expansion

        /// <summary>
        /// Expand an identifier as a standalone expression.
        /// </summary>
        private static Syntax ExpandIdentifier(Identifier id, ExpansionContext exState)
        {
            if (exState.TryResolveBinding(id,
                out _,
                out CompileTimeBinding? binding))
            {
                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding, id, exState);
                }
                else if (binding.BoundType == BindingType.Variable)
                {
                    return ExpandImplicit(Implicit.SpVar, AsArg(id), exState);
                }
                else
                {
                    throw new ExpanderException.InvalidSyntax(id);
                }
            }
            else
            {
                // indicate that it must be a top-level binding that doesn't exist yet
                return ExpandImplicit(Implicit.SpTop, AsArg(id), exState);
            }
        }

        /// <summary>
        /// Expand a function application form containing an identifier in the operator position.
        /// </summary>
        private static Syntax ExpandIdApplication(Identifier op, SyntaxPair stx, ExpansionContext exState)
        {
            if (exState.TryResolveBinding(op,
                out _,
                out CompileTimeBinding? binding))
            {
                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding, stx, exState);
                }
                else if (binding.BoundType == BindingType.Special)
                {
                    return ExpandSpecialForm(binding.Name, stx, exState);
                }
            }

            return ExpandApplication(stx, exState);
        }

        /// <summary>
        /// Expand a function application form containing an arbitrary expression in the operator position.
        /// </summary>
        private static Syntax ExpandApplication(SyntaxPair stx, ExpansionContext exState)
        {
            Syntax op;
            Syntax args;

            try
            {
                op = ExpandAsExpression(stx.Car, exState);
                args = ExpandOperands(stx.Cdr, exState);
            }
            catch (ExpanderException ee)
            {
                throw new ExpanderException.InvalidForm(Keyword.IMP_APP, stx, ee);
            }

            SyntaxPair expandedApp = new SyntaxPair(op, args, stx);
            return ExpandImplicit(Implicit.SpApply, expandedApp, exState);
        }

        private static SyntaxPair AsArg(Syntax stx)
        {
            return new SyntaxPair(stx, Datum.Implicit(Nil.Value), stx);
        }

        /// <summary>
        /// Prepend <paramref name="stx"/> with a special <see cref="Identifier"/> that shares its
        /// <see cref="LexInfo"/>, indicating how it should be handled by the <see cref="Parser"/>.
        /// </summary>
        private static Syntax ExpandImplicit(Implicit formName, SyntaxPair stx, ExpansionContext exState)
        {
            Identifier op = Identifier.Implicit(formName);
            return new SyntaxPair(op, stx, stx);
        }

        /// <summary>
        /// Expand the invocation of a special syntactic form.
        /// </summary>
        /// <param name="formName">The the form's default keyword within the surface language.</param>
        /// <param name="stx">The entirety of the form's application expression.</param>
        private static Syntax ExpandSpecialForm(string formName, Syntax stx, ExpansionContext exState)
        {
            if (!stx.TryDestruct(out Identifier? op, out SyntaxPair? tail, out LexInfo? info))
            {
                // all special forms require arguments
                throw new ExpanderException.InvalidSyntax(stx);
            }

            Syntax expandedTail;

            try
            {
                expandedTail = formName switch
                {
                    Keyword.IMP_PARDEF => ExpandPartialDefineArgs(tail, exState),

                    Keyword.DEFINE => ExpandDefineArgs(tail, exState),
                    Keyword.DEFINE_SYNTAX => ExpandDefineSyntaxArgs(tail, exState),
                    Keyword.SET => ExpandSetArgs(tail, exState),

                    Keyword.QUOTE => ExpandQuoteArgs(tail, exState),
                    Keyword.QUOTE_SYNTAX => ExpandQuoteArgs(tail, exState),

                    Keyword.LAMBDA => ExpandLambdaArgs(tail, exState),

                    Keyword.IF => ExpandIfArgs(tail, exState),
                    Keyword.BEGIN => ExpandSequence(tail, exState),

                    _ => throw new ExpanderException.InvalidSyntax(stx)
                };
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.InvalidForm(formName, stx, e);
            }

            return new SyntaxPair(op, expandedTail, info);
        }

        /// <summary>
        /// Partially expand <paramref name="stx"/>
        /// </summary>
        private static Syntax? PartiallyExpandBodyTerm(Syntax stx, ExpansionContext exState)
        {
            // essentially just check the path to special forms and disregard otherwise
            if (stx is SyntaxPair idApp
                && idApp.Car is Identifier op
                && exState.TryResolveBinding(op, out _, out CompileTimeBinding? binding)
                && binding.BoundType == BindingType.Special)
            {
                Syntax args = idApp.Cdr;

                if (binding.Name == Keyword.DEFINE_SYNTAX)
                {
                    // expand and bind the macro, then discard the definition syntax
                    ExpandDefineSyntaxArgs(args, exState);
                    return null;
                }
                else if (binding.Name == Keyword.DEFINE)
                {
                    // rewrite the form, both because we need to extract the key,
                    // and to mark that it's been partially expanded
                    if (TryRewriteDefineArgs(args,
                        out Identifier? key, out LexInfo? keyContext,
                        out Syntax? value, out LexInfo? valueContext,
                        out Syntax? terminator))
                    {
                        if (!exState.TryBindVariable(key, out _))
                        {
                            throw new ExpanderException.InvalidBindingOperation(key, exState);
                        }

                        return terminator
                            .Cons(value, valueContext)
                            .Cons(key, keyContext)
                            .Cons(new Identifier(Implicit.ParDef, op), stx.LexContext);
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

        private static Syntax ExpandSyntaxTransformation(CompileTimeBinding binding, Syntax input, ExpansionContext exState)
        {
            if (exState.TryDereferenceMacro(binding, out MacroProcedure? macro))
            {
                uint introScope = exState.FreshScopeToken();
                uint useSiteScope = exState.FreshScopeToken();
                exState.AddScope(input, introScope, useSiteScope);

                Syntax output = ApplySyntaxTransformer(macro, input);

                ExpansionContext exMacro = exState.AsMacroResult(useSiteScope);

                exMacro.FlipScope(output, useSiteScope);
                exMacro.AddPendingInsideEdgeScope(output);

                return Expand(output, exMacro);
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

        private static MacroProcedure ExpandAndEvalMacro(Syntax input, ExpansionContext exState)
        {
            ExpansionContext subState = exState.InNewPhase();

            Term output;

            try
            {
                Syntax expandedInput = Expand(input, subState);
                CoreForm parsedInput = Parser.ParseSyntax(expandedInput, subState);
                output = Interpreter.InterpretProgram(parsedInput, StandardEnv.CreateNew());
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.EvaluationError(nameof(MacroProcedure), input, e);
            }

            if (output is CompoundProcedure cp
                && cp.Arity == 1
                && !cp.IsVariadic)
            {
                return new MacroProcedure(cp.Parameters[0], cp.Body);
            }

            throw new ExpanderException.WrongEvaluatedType(nameof(MacroProcedure), output, input);
        }

        #endregion

        #region Recurrent Forms

        /// <summary>
        /// Expand a proper list of expressions.
        /// </summary>
        private static Syntax ExpandOperands(Syntax stx, ExpansionContext exState)
        {
            if (stx.IsTerminator())
            {
                return stx;
            }
            else if (stx.TryDestruct(out Syntax? car, out Syntax? cdr, out LexInfo? info))
            {
                Syntax expandedCar = ExpandAsExpression(car, exState);
                Syntax expandedCdr = ExpandOperands(cdr, exState);

                return new SyntaxPair(expandedCar, expandedCdr, info);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }

        /// <summary>
        /// Expand and bind renames for a list of Identifier terms. No mutation takes place, so nothing is returned.
        /// </summary>
        private static void ExpandParameterList(Syntax stx, ExpansionContext exState)
        {
            if (stx.IsTerminator())
            {
                return;
            }
            else if (stx is Identifier dotted)
            {
                if (!exState.TryBindVariable(dotted, out _))
                {
                    throw new ExpanderException.InvalidBindingOperation(dotted, exState);
                }
            }
            else if (stx.TryDestruct(out Identifier? id, out Syntax? cdr, out LexInfo? info))
            {
                if (!exState.TryBindVariable(id, out _))
                {
                    throw new ExpanderException.InvalidBindingOperation(id, exState);
                }
                ExpandParameterList(cdr, exState);
            }
            else
            {
                // the only way this can go wrong is if a non-identifier is encountered
                throw new ExpanderException.ExpectedProperList(nameof(Identifier), stx);
            }
        }

        /// <summary>
        /// Partially expand a sequence of terms to capture any requisite bindings,
        /// then properly run through and expand each term in sequence.
        /// </summary>
        private static SyntaxPair ExpandBody(Syntax stx, ExpansionContext exState)
        {
            Syntax partiallyExpandedBody = PartiallyExpandBody(stx, exState);

            // TODO inside/outside edge scopes

            return ExpandSequence(partiallyExpandedBody, exState);
        }

        /// <summary>
        /// Recur through a sequence of terms, recording bindings for any definitions,
        /// (and discarding those definitions if they bind macros)
        /// </summary>
        private static Syntax PartiallyExpandBody(Syntax stx, ExpansionContext exState)
        {
            if (stx.IsTerminator())
            {
                return stx;
            }
            else if (stx.TryDestruct(out Syntax? bodyTerm, out Syntax? tail, out LexInfo? info))
            {
                Syntax? partiallyExpandedBodyTerm = PartiallyExpandBodyTerm(bodyTerm, exState);

                // internal syntax definitions are recorded, then discarded
                if (partiallyExpandedBodyTerm is null)
                {
                    return PartiallyExpandBody(tail, exState);
                }

                Syntax partiallyExpandedTail = PartiallyExpandBody(tail, exState);
                return new SyntaxPair(partiallyExpandedBodyTerm, partiallyExpandedTail, info);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }

        /// <summary>
        /// Recur through a sequence of terms, expanding and replacing each one.
        /// The final term is expected to be a <see cref="SyntaxMode.Expression"/>.
        /// </summary>
        private static SyntaxPair ExpandSequence(Syntax stx, ExpansionContext exState)
        {
            if (stx.TryDestruct(out Syntax? car, out Syntax? cdr, out LexInfo? info))
            {
                Syntax outCar;
                Syntax outCdr;

                if (cdr.IsTerminator())
                {
                    outCar = ExpandAsExpression(car, exState);
                    outCdr = cdr;
                }
                else
                {
                    outCar = ExpandAsBodyTerm(car, exState);
                    outCdr = ExpandSequence(cdr, exState);
                }

                return new SyntaxPair(outCar, outCdr, info);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }

        /// <summary>
        /// Recur through a sequence of terms, where each is expected to be a let-syntax binding pair.
        /// </summary>
        private static void ExpandLetSyntaxBindingList(Syntax stx, ExpansionContext exState)
        {
            if (stx.TryDestruct(out SyntaxPair? car, out Syntax? cdr, out LexInfo? _))
            {
                ExpandDefineSyntaxArgs(car, exState);
                ExpandLetSyntaxBindingList(cdr, exState);
            }
            else if (stx.IsTerminator())
            {
                return;
            }
            else
            {
                throw new ExpanderException.ExpectedProperList("Let-Syntax Binding Pair", stx);
            }
        }

        #endregion

        #region Native Rewriting Methods

        /// <summary>
        /// Given the arguments to a <see cref="Keyword.DEFINE"/> form,
        /// rewrite them explicitly into the standard key/value pair format.
        /// </summary>
        private static bool TryRewriteDefineArgs(Syntax stx,
            [NotNullWhen(true)] out Identifier? key, [NotNullWhen(true)] out LexInfo? keyContext,
            [NotNullWhen(true)] out Syntax? value, [NotNullWhen(true)] out LexInfo? valueContext,
            [NotNullWhen(true)] out Syntax? terminator)
        {
            // The arguments to 'define' can come in two formats:
            // - (define key value)
            // - (define (key . formals) . body)

            if (TryDestructKeyValuePair(stx,
                out key, out keyContext,
                out value, out valueContext,
                out terminator))
            {
                return true;
            }
            else if (TryDestructImplicitLambda(stx,
                out key, out keyContext,
                out Syntax? formals,
                out Syntax? body))
            {
                valueContext = body.LexContext;
                value = body
                    .Cons(formals, body.LexContext)
                    .Cons(Identifier.Implicit(Implicit.SpLambda), body.LexContext);

                terminator = Datum.Implicit(Nil.Value);

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
        private static bool TryRewriteDefineSyntaxArgs(Syntax stx, ExpansionContext exState,
            [NotNullWhen(true)] out Identifier? key, [NotNullWhen(true)] out LexInfo? keyContext,
            [NotNullWhen(true)] out Syntax? value, [NotNullWhen(true)] out LexInfo? valueContext,
            [NotNullWhen(true)] out Syntax? terminator)
        {
            // The arguments to 'define-syntax' can come in three formats:
            // - (define key (lambda ...))
            // - (define key value)
            // - (define (key . formals) . body)

            if (TryDestructExplicitLambda(stx, exState,
                out key, out keyContext,
                out value, out valueContext,
                out terminator))
            {
                return true;
            }
            else if (TryDestructKeyValuePair(stx,
                out key, out keyContext,
                out Syntax? returnValue, out valueContext,
                out terminator))
            {
                // rewrite the non-lambda term as a parameter-less procedure that returns the term

                value = Datum.Implicit(Nil.Value)
                    .Cons(returnValue, valueContext)
                    .Cons(Datum.FromDatum(Nil.Value, valueContext), valueContext)
                    .Cons(Identifier.Implicit(Implicit.SpLambda), valueContext);
                return true;
            }
            else if (TryDestructImplicitLambda(stx,
                out key, out keyContext,
                out Syntax? formals,
                out Syntax? body))
            {
                valueContext = body.LexContext;
                value = body
                    .Cons(formals, body.LexContext)
                    .Cons(Identifier.Implicit(Implicit.SpLambda), body.LexContext);

                terminator = Datum.Implicit(Nil.Value);

                return true;
            }
            else
            {
                return false;
            }
        }

        // (define-whatever key value)
        private static bool TryDestructKeyValuePair(Syntax stx,
            [NotNullWhen(true)] out Identifier? key, [NotNullWhen(true)] out LexInfo? keyContext,
            [NotNullWhen(true)] out Syntax? value, [NotNullWhen(true)] out LexInfo? valueContext,
            [NotNullWhen(true)] out Syntax? terminator)
        {
            key = null;
            keyContext = null;
            value = null;
            valueContext = null;
            terminator = null;

            return stx.TryDestruct(out key, out SyntaxPair? keyTail, out keyContext)
                && keyTail.TryDestruct(out value, out terminator, out valueContext)
                && terminator.IsTerminator();
        }

        // (define-whatever (key . formals) . body)
        private static bool TryDestructImplicitLambda(Syntax stx,
            [NotNullWhen(true)] out Identifier? key, [NotNullWhen(true)] out LexInfo? keyContext,
            [NotNullWhen(true)] out Syntax? formals,
            [NotNullWhen(true)] out Syntax? body)
        {
            key = null;
            keyContext = null;
            formals = null;
            body = null;

            return stx.TryDestruct(out SyntaxPair? keyWithFormals, out body, out keyContext)
                && keyWithFormals.TryDestruct(out key, out formals, out LexInfo? _);
        }

        // (define-whatever key (lambda ...))
        private static bool TryDestructExplicitLambda(Syntax stx, ExpansionContext exState,
            [NotNullWhen(true)] out Identifier? key, [NotNullWhen(true)] out LexInfo? keyContext,
            [NotNullWhen(true)] out Syntax? value, [NotNullWhen(true)] out LexInfo? valueContext,
            [NotNullWhen(true)] out Syntax? terminator)
        {
            return TryDestructKeyValuePair(stx, out key, out keyContext, out value, out valueContext, out terminator)
                && value.TryDestruct(out Identifier? maybeLambda, out Syntax? _, out LexInfo? _)
                && exState.TryResolveBinding(maybeLambda, out _, out CompileTimeBinding? binding)
                && (binding.Name == Keyword.LAMBDA || binding.Name == Keyword.IMP_LAMBDA);
        }

        /// <summary>
        /// Given the arguments to a <see cref="Keyword.IF"/> form,
        /// rewrite them explicitly into the standard cond/then/else format.
        /// </summary>
        private static bool TryRewriteIfArgs(Syntax stx,
            [NotNullWhen(true)] out Syntax? condValue, [NotNullWhen(true)] out LexInfo? condContext,
            [NotNullWhen(true)] out Syntax? thenValue, [NotNullWhen(true)] out LexInfo? thenContext,
            [NotNullWhen(true)] out Syntax? elseValue, [NotNullWhen(true)] out LexInfo? elseContext,
            [NotNullWhen(true)] out Syntax? terminator)
        {
            // The arguments to 'if' can come in two formats:
            // - (if cond then else)
            // - (if cond then)

            if (stx.TryDestruct(out condValue, out SyntaxPair? thenPair, out condContext)
                && thenPair.TryDestruct(out thenValue, out Syntax? elsePair, out thenContext))
            {
                if (elsePair.IsTerminator())
                {
                    elseValue = Datum.Implicit(Boolean.False);
                    elseContext = thenContext;
                    terminator = elsePair;

                    return true;
                }
                else if (elsePair.TryDestruct(out elseValue, out terminator, out elseContext)
                    && terminator.IsTerminator())
                {
                    return true;
                }
            }

            condValue = null;
            condContext = null;
            thenValue = null;
            thenContext = null;
            elseValue = null;
            elseContext = null;
            terminator = null;

            return false;

        }
        #endregion

        #region Special Form Expansion

        private static Syntax ExpandPartialDefineArgs(Syntax stx, ExpansionContext exState)
        {
            if (exState.Mode != SyntaxMode.InternalDefinition)
            {
                throw new ExpanderException.InvalidContext(Keyword.IMP_PARDEF, exState.Mode, stx);
            }
            else if (TryRewriteDefineArgs(stx,
                out Identifier? key, out LexInfo? keyContext,
                out Syntax? value, out LexInfo? valueContext,
                out Syntax? terminator))
            {
                Syntax expandedValue = ExpandAsExpression(value, exState);

                return terminator
                    .Cons(expandedValue, valueContext)
                    .Cons(key, keyContext);
            }
            else
            {
                throw new ExpanderException.InvalidForm(Keyword.IMP_PARDEF, stx);
            }
        }

        private static Syntax ExpandDefineArgs(Syntax stx, ExpansionContext exState)
        {
            if (exState.Mode == SyntaxMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keyword.DEFINE, exState.Mode, stx);
            }
            else if (TryRewriteDefineArgs(stx,
                out Identifier? key, out LexInfo? keyContext,
                out Syntax? value, out LexInfo? valueContext,
                out Syntax? terminator))
            {
                exState.SanitizeIdentifier(key);
                if (!exState.TryBindVariable(key, out _))
                {
                    throw new ExpanderException.InvalidBindingOperation(key, exState);
                }

                Syntax expandedValue = ExpandAsExpression(value, exState);

                return terminator
                    .Cons(expandedValue, valueContext)
                    .Cons(key, keyContext);
            }
            else
            {
                throw new ExpanderException.InvalidForm(Keyword.DEFINE, stx);
            }
        }

        private static Syntax ExpandDefineSyntaxArgs(Syntax stx, ExpansionContext exState)
        {
            if (exState.Mode == SyntaxMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keyword.DEFINE_SYNTAX, exState.Mode, stx);
            }
            else if (TryRewriteDefineSyntaxArgs(stx, exState,
                out Identifier? key, out LexInfo? keyContext,
                out Syntax? value, out LexInfo? valueContext,
                out Syntax? terminator))
            {
                exState.SanitizeIdentifier(key);

                MacroProcedure macro = ExpandAndEvalMacro(value, exState);
                if (!exState.TryBindMacro(key, macro, out _))
                {
                    throw new ExpanderException.InvalidBindingOperation(key, exState);
                }

                Syntax evaluatedMacro = ExpandImplicit(Implicit.SpDatum, AsArg(Datum.FromDatum(macro, valueContext)), exState);

                return terminator
                    .Cons(evaluatedMacro, valueContext)
                    .Cons(key, keyContext);
            }
            else
            {
                throw new ExpanderException.InvalidForm(Keyword.DEFINE_SYNTAX, stx);
            }
        }

        private static Syntax ExpandSetArgs(Syntax stx, ExpansionContext exState)
        {
            if (exState.Mode == SyntaxMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keyword.SET, exState.Mode, stx);
            }
            else if (TryDestructKeyValuePair(stx,
                out Identifier? key, out LexInfo? keyContext,
                out Syntax? value, out LexInfo? valueContext,
                out Syntax? terminator))
            {
                exState.SanitizeIdentifier(key);

                if (exState.TryResolveBinding(key, out CompileTimeBinding[] candidates, out CompileTimeBinding? binding))
                {
                    Syntax expandedValue = ExpandAsExpression(value, exState);

                    return terminator
                        .Cons(expandedValue, valueContext)
                        .Cons(key, keyContext);
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
                throw new ExpanderException.InvalidForm(Keyword.SET, stx);
            }
        }

        private static Syntax ExpandQuoteArgs(SyntaxPair stx, ExpansionContext exState)
        {
            return stx; //it'll get stripped later during parsing
        }

        private static Syntax ExpandIfArgs(SyntaxPair stx, ExpansionContext exState)
        {
            if (TryRewriteIfArgs(stx,
                out Syntax? condValue, out LexInfo? condContext,
                out Syntax? thenValue, out LexInfo? thenContext,
                out Syntax? elseValue, out LexInfo? elseContext,
                out Syntax? terminator))
            {
                Syntax expandedCond = ExpandAsExpression(condValue, exState);
                Syntax expandedThen = ExpandAsExpression(thenValue, exState);
                Syntax expandedElse = ExpandAsExpression(elseValue, exState);

                return terminator
                    .Cons(expandedElse, elseContext)
                    .Cons(expandedThen, thenContext)
                    .Cons(expandedCond, condContext);
            }
            else
            {
                throw new ExpanderException.InvalidForm(Keyword.IF, stx);
            }
        }

        private static Syntax ExpandLambdaArgs(Syntax stx, ExpansionContext exState)
        {
            if (stx.TryDestruct(out Syntax? formals, out SyntaxPair? body, out LexInfo? info))
            {
                uint outsideEdge = exState.FreshScopeToken();
                uint insideEdge = exState.FreshScopeToken();

                exState.AddScope(formals, outsideEdge, insideEdge);
                exState.AddScope(body, outsideEdge, insideEdge);

                ExpansionContext exBody = exState.InBody(insideEdge);

                ExpandParameterList(formals, exBody);
                SyntaxPair expandedBody = ExpandBody(body, exBody);

                return new SyntaxPair(formals, expandedBody, info);
            }
            else
            {
                throw new ExpanderException.InvalidForm(Keyword.LAMBDA, stx);
            }
        }

        #endregion
    }
}
