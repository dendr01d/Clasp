using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Product;
using Clasp.Data.Terms.Syntax;
using Clasp.ExtensionMethods;

using static System.Net.WebRequestMethods;

namespace Clasp.Process
{
    internal static class Expander
    {
        public static Syntax ExpandSyntax(Syntax input, Environment env, ScopeTokenGenerator gen)
        {
            ExpansionContext exState = ExpansionContext.FreshExpansion(env, gen);
            return ExpandAsTop(input, exState);
        }

        #region Dispatching Methods

        private static Syntax ExpandAsTop(Syntax stx, ExpansionContext exState)
            => Expand(stx, exState.ExpandInMode(SyntaxMode.TopLevel));

        private static Syntax ExpandAsExpression(Syntax stx, ExpansionContext exState)
            => Expand(stx, exState.ExpandInMode(SyntaxMode.Expression));

        private static Syntax ExpandAsBodyTerm(Syntax stx, ExpansionContext exState)
            => Expand(stx, exState.ExpandInMode(SyntaxMode.Body));

        private static Syntax PartiallyExpandAsBodyTerm(Syntax stx, ExpansionContext exState)
            => Expand(stx, exState.ExpandInMode(SyntaxMode.Partial));

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
                return ExpandImplicit(ImplicitSym.SpDatum, stx, exState);
            }
        }

        /// <summary>
        /// Expand an identifier as a standalone expression.
        /// </summary>
        private static Syntax ExpandIdentifier(Identifier id, ExpansionContext exState)
        {
            if (exState.TryResolveBinding(id, out CompileTimeBinding? binding))
            {
                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding.BoundId, id, exState);
                }
                else if (binding.BoundType == BindingType.Variable)
                {
                    return ExpandImplicit(ImplicitSym.SpVar, id, exState);
                }
                else
                {
                    throw new ExpanderException.InvalidSyntax(id);
                }
            }
            else
            {
                RenameAndBindVariable(id, exState);
                // indicate that it must be a top-level binding that doesn't exist yet
                return ExpandImplicit(ImplicitSym.SpTop, id, exState);
            }
        }

        /// <summary>
        /// Expand a function application form containing an identifier in the operator position.
        /// </summary>
        private static Syntax ExpandIdApplication(Identifier op, SyntaxPair stx, ExpansionContext exState)
        {
            if (exState.TryResolveBinding(op, out CompileTimeBinding? binding))
            {
                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding.BoundId, stx, exState);
                }
                else if (binding.BoundType == BindingType.Special)
                {
                    return ExpandSpecialForm(binding.BindingName, stx, exState);
                }
            }

            return ExpandApplication(stx, exState);
        }

        /// <summary>
        /// Expand a function application form containing an arbitrary expression in the operator position.
        /// </summary>
        private static Syntax ExpandApplication(SyntaxPair stx, ExpansionContext exState)
        {
            Syntax op = Expand(stx.Car, exState);
            Syntax args = ExpandOperands(stx.Cdr, exState);

            SyntaxPair expandedApp = new SyntaxPair(op, args, stx);
            return ExpandImplicit(ImplicitSym.SpApply, expandedApp, exState);
        }
        
        /// <summary>
        /// Prepend <paramref name="stx"/> with a special <see cref="Identifier"/> that shares its
        /// <see cref="LexInfo"/>, indicating how it should be handled by the <see cref="Parser"/>.
        /// </summary>
        private static Syntax ExpandImplicit(ImplicitSym formName, Syntax stx, ExpansionContext exState)
        {
            Identifier op = new Identifier(formName, stx);
            return new SyntaxPair(op, stx, stx);
        }

        /// <summary>
        /// Expand the invocation of a special syntactic form.
        /// </summary>
        /// <param name="formName">The the form's default keyword within the surface language.</param>
        /// <param name="stx">The entirety of the form's application expression.</param>
        private static Syntax ExpandSpecialForm(string formName, Syntax stx, ExpansionContext exState)
        {
            if (!stx.TryDestruct(out Identifier? op, out SyntaxPair? args, out LexInfo? info))
            {
                // all special forms require arguments
                throw new ExpanderException.InvalidSyntax(stx);
            }

            // for each core form, expansion involves doing something with the arguments
            // dispatch to the handler depending on the keyword, then reassemble the final form

            Syntax tail;

            try
            {
                tail = formName switch
                {
                    Keyword.DEFINE => ExpandDefine(args, exState),
                    Keyword.DEFINE_SYNTAX => ExpandDefineSyntax(args, exState),
                    Keyword.SET => ExpandSet(args, exState),

                    Keyword.QUOTE => ExpandQuote(args, exState),
                    Keyword.QUOTE_SYNTAX => ExpandQuote(args, exState),

                    Keyword.IF => ExpandIf(args, exState),
                    Keyword.BEGIN => ExpandSequence(args, exState),

                    Keyword.LAMBDA => ExpandLambda(args, exState),
                    Keyword.LET_SYNTAX => ExpandLetSyntax(args, exState),

                    _ => throw new ExpanderException.InvalidSyntax(stx)
                };
            }
            catch (ExpanderException ce)
            {
                throw new ExpanderException.InvalidForm(formName, stx, ce);
            }

            return new SyntaxPair(op, tail, info);
        }

        #endregion

        #region Macro-Related

        private static Syntax ExpandSyntaxTransformation(Identifier macroBindingId, Syntax input, ExpansionContext exState)
        {
            if (exState.TryGetMacro(macroBindingId.Name, out MacroProcedure? macro))
            {
                Syntax transformedStx = ApplySyntaxTransformer(macro, input, exState);
                return Expand(transformedStx, exState);
            }
            else
            {
                throw new ExpanderException.UnboundMacro(macroBindingId);
            }
        }

        private static Syntax ApplySyntaxTransformer(MacroProcedure macro, Syntax input, ExpansionContext exState)
        {
            uint introScope = exState.TokenizeMacroScope();
            uint useSiteScope = exState.TokenizeMacroScope();
            exState.AddScope(input, introScope, useSiteScope);

            Term output;

            try
            {
                MacroApplication acceleratedProgram = new MacroApplication(macro, input);
                output = Interpreter.InterpretProgram(acceleratedProgram);
            }
            catch (ClaspException ce)
            {
                throw new ExpanderException.EvaluationError(nameof(MacroProcedure), input, ce);
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.EvaluationError(nameof(MacroProcedure), input, e.Message);
            }

            if (output is not Syntax outputStx)
            {
                throw new ExpanderException.WrongEvaluatedType(nameof(Syntax), output, input);
            }

            exState.FlipScope(outputStx, introScope);

            //TODO remove macro scopes from keys of newly-inserted definitions(?)
            //TODO also do something with outside/inside edge scope(??)

            return outputStx;
        }

        private static MacroProcedure ExpandAndEvalMacro(Syntax input, ExpansionContext exState)
        {
            ExpansionContext subState = exState.ExpandInNewPhase();

            Syntax expandedInput = Expand(input, subState);

            Term output;

            try
            {
                CoreForm parsedInput = Parser.ParseSyntax(expandedInput, exState); //TODO is it safe to just pass that in?
                output = Interpreter.InterpretProgram(parsedInput, StandardEnv.CreateNew());
            }
            catch (ClaspException ce)
            {
                throw new ExpanderException.EvaluationError(nameof(MacroProcedure), input, ce);
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.EvaluationError(nameof(MacroProcedure), input, e.Message);
            }

            if (output is MacroProcedure macro)
            {
                return macro;
            }

            throw new ExpanderException.WrongEvaluatedType(nameof(MacroProcedure), output, input);
        }

        #endregion


        #region Recurrent Forms

        private static void ExpandParameterList(Syntax stx, ExpansionContext exState)
        {
            if (stx.IsTerminator())
            {
                return;
            }
            else if (stx is Identifier dotted)
            {
                RenameAndBindVariable(dotted, exState);
            }
            else if (stx.TryDestruct(out Identifier? id, out Syntax? cdr, out LexInfo? info))
            {
                RenameAndBindVariable(id, exState);
                ExpandParameterList(cdr, exState);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(nameof(Identifier), stx);
            }
        }

        private static Syntax ExpandBody(Syntax stx, ExpansionContext exState)
        {
            Syntax partiallyExpanded = PartiallyExpandAsBodyTerm(stx, exState);
            return ExpandSequence(partiallyExpanded, exState);
        }

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

        private static void ExpandLetBindingList(Syntax stx, ExpansionContext exState)
        {
            if (stx.TryDestruct(out SyntaxPair? car, out Syntax? cdr, out LexInfo? _))
            {
                ExpandLetBinding(car, exState);
                ExpandLetBindingList(cdr, exState);
            }
            else if (stx.IsTerminator())
            {
                return;
            }
            else
            {
                throw new ExpanderException.ExpectedProperList("Let-Binding Pairs", stx);
            }
        }


        #endregion


        #region Definitional Sub-Forms

        private static void ExpandLetBinding(SyntaxPair stx, ExpansionContext exState)
        {

        }

        private static SyntaxPair ExpandAndBindTransformer(Identifier? key, Syntax? )

        #endregion



        #region Special Form Expansion

        private static Syntax ExpandQuote(SyntaxPair stx, ExpansionContext exState)
        {
            return stx; //it'll get stripped later during parsing
        }

        private static Syntax ExpandIf(SyntaxPair stx, ExpansionContext exState)
        {
            if (!stx.TryDestruct(out Syntax? condValue, out SyntaxPair? thenPair, out LexInfo? condInfo))
            {
                throw new ExpanderException.InvalidSubForm("conditional", stx);
            }

            Syntax condOut = ExpandAsExpression(condValue, exState);

            if (!thenPair.TryDestruct(out Syntax? thenValue, out Syntax? elsePair, out LexInfo? thenInfo))
            {
                throw new ExpanderException.InvalidSubForm("consequent", stx);
            }

            Syntax thenOut = ExpandAsExpression(thenValue, exState);

            // fill in implicit alternative
            if (elsePair.IsTerminator())
            {
                return elsePair
                    .Cons(Datum.Implicit(Boolean.False), elsePair.LexContext)
                    .Cons(thenOut, thenInfo)
                    .Cons(condOut, condInfo);
            }
            else if (elsePair.TryDestruct(out Syntax? elseValue, out Syntax? terminator, out LexInfo? elseInfo)
                && terminator.IsTerminator())
            {
                Syntax elseOut = ExpandAsExpression(elseValue, exState);

                return terminator
                    .Cons(elseOut, elseInfo)
                    .Cons(thenOut, thenInfo)
                    .Cons(condOut, condInfo);
            }

            throw new ExpanderException.InvalidSubForm("alternative", stx);
        }

        #endregion

        #region Imperative Special Forms

        private static Syntax ExpandDefine(Syntax stx, ExpansionContext exState)
        {
            if (exState.Mode == SyntaxMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keyword.DEFINE, exState.Mode, stx);
            }
            else if (TryDestructKVDefinition(stx, out Identifier? key, out SyntaxPair? value))
            {
                RenameAndBindVariable(key, exState);
                Syntax expandedValue = ExpandOperands(value, exState);
                return new SyntaxPair(key, expandedValue, stx);
            }
            else if (TryDestructImpLambda(stx, out Identifier? key2, out SyntaxPair? lambdaArgs))
            {
                RenameAndBindVariable(key2, exState);
                Syntax expandedLambdaArgs = ExpandLambda(lambdaArgs, exState);
                Syntax expandedLambda = new SyntaxPair(ImplicitLambda, expandedLambdaArgs, lambdaArgs);
                Syntax cdr = new SyntaxPair(expandedLambda, ImplicitNil, lambdaArgs);
                return new SyntaxPair(key2, cdr, stx);
            }

            throw new ExpanderException.InvalidFormInput(Keyword.DEFINE, "arguments", stx);
        }

        private static Syntax ExpandDefineSyntax(Syntax stx, ExpansionContext exState)
        {
            if (exState.Mode == SyntaxMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keyword.DEFINE_SYNTAX, exState.Mode, stx);
            }

            Identifier bindingKey;
            MacroProcedure macro;
            
            if (TryDestructKVDefinition(stx, out Identifier? key, out SyntaxPair? value))
            {
                bindingKey = RenameAndBindVariable(key, exState);
                macro = ExpandAndEvalMacro(value.Car, exState);
                exState.BindMacro(key, bindingKey, macro);

                return new SyntaxPair(key, new SyntaxPair(new Datum(macro, value), value.Cdr, value), stx);
            }
            else if (TryDestructImpLambda(stx, out Identifier? key2, out SyntaxPair? lambdaArgs))
            {
                bindingKey = RenameAndBindVariable(key2, exState);

                Syntax expandedLambdaArgs = ExpandLambda(lambdaArgs, exState);
                Syntax expandedLambda = new SyntaxPair(ImplicitLambda, expandedLambdaArgs, lambdaArgs);
                macro = ExpandAndEvalMacro(expandedLambda, exState);
                exState.BindMacro(key2, bindingKey, macro);

                return new SyntaxPair(key2, new SyntaxPair(new Datum(macro, expandedLambdaArgs), lambdaArgs.Cdr, lambdaArgs), stx);
            }
            else
            {
                throw new ExpanderException.InvalidFormInput(Keyword.DEFINE_SYNTAX, "arguments", stx);
            }
        }

        private static Syntax ExpandSet(Syntax stx, ExpansionContext exState)
        {
            if (exState.Mode == SyntaxMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keyword.SET, exState.Mode, stx);
            }
            else if (TryDestructKVDefinition(stx, out Identifier? key, out SyntaxPair? value))
            {
                RenameAndBindVariable(key, exState);
                Syntax expandedValue = ExpandOperands(value, exState);
                return new SyntaxPair(key, expandedValue, stx);
            }

            throw new ExpanderException.InvalidFormInput(Keyword.SET, "arguments", stx);
        }

        #endregion

        #region Lambda

        private static Syntax ExpandLambda(Syntax stx, ExpansionContext exState)
        {
            if (stx is not SyntaxPair stp)
            {
                throw new ExpanderException.InvalidFormInput(Keyword.LAMBDA, stx);
            }
            else
            {
                uint newScope = exState.TokenizeScope();

                exState.AddScope(stp.Car, newScope);
                exState.AddScope(stp.Cdr, newScope);

                ExpansionContext subState = exState.ExpandInSubBlock();

                foreach (Identifier parameter in EnumerateParameterList(stp.Car))
                {
                    RenameAndBindVariable(parameter, subState);
                }

                Syntax expandedBody = ExpandBody(stp.Cdr, subState);

                return new SyntaxPair(stp.Car, expandedBody, stx);
            }
        }

        private static Syntax PartiallyExpandBody(Syntax stx, ExpansionContext exState, Syntax? pending = null)
        {
            if (stx is SyntaxPair stp)
            {
                Syntax? newCar = stp.Car;

                if (stx.TryExposeIdList(out Identifier? idOp, out SyntaxPair? args)
                    && exState.TryResolveBinding(idOp, out CompileTimeBinding? binding))
                {
                    if (binding.BindingName == Keyword.DEFINE
                        && TryDestructDefinition(args.Cdr, out Identifier? defKey, out SyntaxPair? def))
                    {
                        RenameAndBindVariable(defKey, exState);
                        newCar = new SyntaxPair(ImplicitPartialDef, new SyntaxPair(defKey, def, args.Cdr), args);
                    }
                    else if (binding.BindingName == Keyword.DEFINE_SYNTAX)
                    {
                        ExpandDefineSyntax(args.Cdr, exState);
                        return PartiallyExpandBody(args.Cdr, exState, pending);
                    }
                    else if (binding.BindingName == Keyword.BEGIN)
                    {
                        return PartiallyExpandBody(args.Cdr, exState, stp.Cdr);
                    }
                }

                Syntax newCdr = PartiallyExpandBody(stp.Cdr, exState);
                return new SyntaxPair(newCar, newCdr, stx);
            }
            else if (stx.Expose() is Nil)
            {
                return pending is null
                    ? stx
                    : PartiallyExpandBody(pending, exState);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }

        private static Syntax ExpandBody(Syntax stx, ExpansionContext exState)
        {
            //TODO this needs to do a first-pass to check for definitions
            //TODO inside/outside edge scopes? idk

            if (stx is not SyntaxPair stp)
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
            else if (stp.Cdr.Expose() is Nil)
            {
                Syntax expandedCar = ExpandAsExpression(stp.Car, exState.ExpandInMode(SyntaxMode.Expression));
                return new SyntaxPair(expandedCar, stp.Cdr, stx);
            }
            else
            {
                Syntax expandedCar = Expand(stp.Car, exState);
                Syntax expandedCdr = ExpandBody(stp.Cdr, exState);
                return new SyntaxPair(expandedCar, expandedCdr, stx);
            }
        }

        #endregion

        private static Syntax ExpandOperands(Syntax stx, ExpansionContext exState)
        {
            if (stx is SyntaxPair stp)
            {
                Syntax nextArg = Expand(stp.Car, exState.ExpandInMode(SyntaxMode.Expression));

                if (stp.Cdr.Expose() is Nil)
                {
                    return new SyntaxPair(nextArg, stp.Cdr, stx);
                }
                else
                {
                    Syntax remainingArgs = ExpandOperands(stp.Cdr, exState);
                    return new SyntaxPair(nextArg, remainingArgs, stx);
                }
            }

            throw new ExpanderException.ExpectedProperList(stx);
        }

        private static Syntax ExpandLetSyntax(Syntax stx, ExpansionContext exState)
        {
            if (stx is not SyntaxPair stp)
            {
                throw new ExpanderException.InvalidFormInput(Symbol.LetSyntax.Name, stx);
            }
            else
            {
                uint newScope = exState.TokenizeScope();

                exState.AddScope(stp.Car, newScope);
                exState.AddScope(stp.Cdr, newScope);

                ExpansionContext subState = exState.ExpandInSubBlock();

                foreach (SyntaxPair letBinding in EnumerateLetBindingList(stp.Car))
                {
                    ExpandDefineSyntax(letBinding, subState);
                }

                Syntax expandedBody = ExpandBody(stp.Cdr, subState);

                // the point of this form is to install temporary macros over the body
                // we only care about returning the expanded body itself, however
                return expandedBody;
            }
        }


        #region Helpers

        /// <summary>
        /// Create a new <see cref="GenSym"/> to act as a binding name,
        /// then wrap an <see cref="Identifier"/> around it and bind it as a variable
        /// to <paramref name="symId"/> in <paramref name="exState"/>.
        /// </summary>
        /// <returns>The new binding <see cref="Identifier"/>.</returns>
        private static Identifier RenameAndBindVariable(Identifier symId, ExpansionContext exState)
        {
            //TODO somewhere in here I need to detect duplicate bindings

            GenSym newSym = new GenSym(symId.Name);
            Identifier bindingId = new Identifier(newSym, symId);
            exState.BindVariable(symId, bindingId);

            return bindingId;
        }

        private static readonly Identifier ImplicitPartialDef = new Identifier(ImplicitSym.SpParDef, SourceLocation.InherentSource);
        private static readonly Identifier ImplicitLambda = new Identifier(ImplicitSym.SpLambda, SourceLocation.InherentSource);
        private static readonly Datum ImplicitNil = new Datum(Nil.Value, SourceLocation.InherentSource);

        private static bool TryDestructDefinition(Syntax stx,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out SyntaxPair? bound)
        {
            return TryDestructKVDefinition(stx, out key, out bound)
                || TryDestructImpLambda(stx, out key, out bound);
        }

        private static bool TryDestructKVDefinition(Syntax stx,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out SyntaxPair? value)
        {
            if (stx is SyntaxPair args
                && args.Cdr is SyntaxPair tail
                && args.Car is Identifier bindingKey
                && tail.Cdr.Expose() is Nil)
            {
                key = bindingKey;
                value = tail;
                return true;
            }
            key = null;
            value = null;
            return false;
        }

        private static bool TryDestructImpLambda(Syntax stx,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out SyntaxPair? lambdaArgs)
        {
            if (stx is SyntaxPair args
                && args.Cdr is SyntaxPair tail
                && args.Car is SyntaxPair nameWithParams
                && nameWithParams.Car is Identifier bindingKey)
            {
                key = bindingKey;
                lambdaArgs = new SyntaxPair(tail, nameWithParams, tail);
                return true;
            }
            key = null;
            lambdaArgs = null;
            return false;
        }

        private static IEnumerable<Identifier> EnumerateParameterList(Syntax stx)
        {
            Syntax current = stx;

            while (current is SyntaxPair nextPair)
            {
                if (nextPair.Car is Identifier nextParam)
                {
                    yield return nextParam;
                    current = nextPair.Cdr;
                }
                else
                {
                    throw new ExpanderException.InvalidFormInput("Parameter list", stx);
                }
            }

            if (current is Identifier dottedParam)
            {
                yield return dottedParam;
            }
            else if (current.Expose() is Nil)
            {
                yield break;
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }

        private static IEnumerable<SyntaxPair> EnumerateLetBindingList(Syntax stx)
        {
            Syntax current = stx;

            while (current is SyntaxPair nextPair)
            {
                if (nextPair.Car is SyntaxPair nextLetBinding)
                {
                    yield return nextLetBinding;
                    current = nextPair.Cdr;
                }
                else
                {
                    throw new ExpanderException.InvalidFormInput("Let-binding list", nextPair);
                }
            }

            if (current.Expose() is Nil)
            {
                yield break;
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }

        #endregion


        #region Destructors


        #endregion

    }
}
