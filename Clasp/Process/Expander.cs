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

namespace Clasp.Process
{
    /*
     
     I've been treating this as a single thing, when really it's doing a lot of things in tandem
    To wit:
    - identify each lexical scope described by the syntax and assign it a unique ID
    - associate each syntax term in the program with the set of scopes it's located within
    - rename all identifiers in the program such that there are no shadowed bindings
        (record the renamings by mapping (symbolic name, scope set) pairs to binding names)
    - perform rudimentary type analysis on identifiers to see if they're being used as
        variable names, macro invocations, or shadowings of core forms
    - tag implicit forms (function application, datum, top?, var?) with core operators
    - accelerate and bind as a compile-time value any macros defined
    - identify and invoke any macro application forms

    - macro invocations get wrapped in new scopes before and after executing
        in order to distinguish the varying stages at which new bindings are conceptually performed

    - internal definition contexts must keep track of the special scopes from that last point
        and remove them from identifiers that end up in binding positions --
        i.e. they need to have the same scope as if they HADN'T resulted from a macro

    - for the sake of efficiency, renamings should be discarded once they're no longer accessible
        (effected by treating the bindingstore essentially as a secondary environment?)

    - syntax objects need to support adding, flipping, and removal of scopes
        and these operations ideally need to be lazily recursive on their substructures
     
     */



    internal static class Expander
    {
        public static Syntax ExpandSyntax(Syntax input, Environment env, ScopeTokenGenerator gen)
        {
            ExpansionContext exState = ExpansionContext.FreshExpansion(env, gen);
            return Expand(input, exState);
        }

        private static Syntax Expand(Syntax stx, ExpansionContext exState)
        {
            if (stx is Identifier id)
            {
                return ExpandIdentifier(id, exState);
            }
            else if (stx is SyntaxPair stp
                && stp.Car is Identifier idOp)
            {
                return ExpandIdApplication(stp, idOp, exState);
            }
            else if (stx.Expose() is ConsList or Nil)
            {
                return ExpandImplicit(Symbol.ImplicitApp, stx, exState);
            }
            else
            {
                return ExpandImplicit(Symbol.ImplicitDatum, stx, exState);
            }
        }

        /// <summary>
        /// Expand an identifier as a standalone expression.
        /// </summary>
        private static Syntax ExpandIdentifier(Identifier id, ExpansionContext exState)
        {
            if (exState.TryResolveBinding(id, out ExpansionBinding? binding))
            {
                return DispatchOnBinding(binding, id, exState);
            }
            else
            {
                // indicate that it must be a top-level binding that doesn't exist yet
                return ExpandImplicit(Symbol.ImplicitTop, id, exState);
            }
        }

        /// <summary>
        /// Expand a function application form with an identifier in the operator position.
        /// </summary>
        private static Syntax ExpandIdApplication(SyntaxPair idApp, Identifier op, ExpansionContext exState)
        {
            if (exState.TryResolveBinding(op, out ExpansionBinding? binding)
                && binding.BoundType != BindingType.Variable)
            {
                return DispatchOnBinding(binding, idApp, exState);
            }
            else
            {
                return ExpandImplicit(Symbol.ImplicitApp, idApp, exState);
            }
        }

        private static Syntax DispatchOnBinding(ExpansionBinding binding, Syntax stx, ExpansionContext exState)
        {
            if (binding.BoundType == BindingType.Special)
            {
                return ExpandCoreForm(binding.BindingName, stx, exState);
            }
            else if (binding.BoundType == BindingType.Transformer)
            {
                if (exState.TryGetMacro(binding.BindingName, out MacroProcedure? macro))
                {
                    Syntax transformedStx = ApplySyntaxTransformer(macro, stx, exState);
                    return Expand(transformedStx, exState);
                }
                else
                {
                    throw new ExpanderException.UnboundMacro(binding);
                }
            }
            else if (binding.BoundType == BindingType.Variable)
            {
                return stx;
            }
            else
            {
                throw new ExpanderException.InvalidSyntax(binding.BoundId);
            }
        }


        private static Syntax ExpandImplicit(Symbol formName, Syntax stx, ExpansionContext exState)
        {
            Identifier op = new Identifier(formName, stx);
            Syntax implicitArgs = ExpandOperands(stx, exState);

            return new SyntaxPair(op, stx, stx);

            //if (exState.TryResolveBinding(op, out ExpansionBinding? binding))
            //{
            //    if (binding.BoundType == BindingType.Transformer)
            //    {
            //        Syntax newStx = new SyntaxPair(op, stx, stx);
            //        return DispatchOnBinding(binding, newStx, exState);
            //    }
            //    else if (binding.BoundType == BindingType.Special)
            //    {
            //        if (formName == Symbol.ImplicitTop
            //            && binding.BindingSymbol == Symbol.ImplicitTop
            //            && ExpandContextInLocalExpand(exState))
            //        {
            //            return DispatchImplicitTopCoreForm(binding.BoundId, stx, exState);
            //        }
            //        else
            //        {
            //            Syntax newStx = new SyntaxPair(op, stx, stx);
            //            return DispatchOnBinding(binding, newStx, exState);
            //        }
            //    }
            //    else
            //    {
            //        throw new ExpanderException.UnboundMacro(binding);
            //    }
            //}
            //else
            //{
            //    throw new ExpanderException.UnboundIdentifier(op);
            //}
        }

        private static Syntax ApplySyntaxTransformer(MacroProcedure macro, Syntax input, ExpansionContext exState)
        {
            uint introScope = exState.TokenizeMacroScope();
            uint useSiteScope = exState.TokenizeMacroScope();
            exState.AddScope(input, introScope, useSiteScope);

            MacroApplication acceleratedProgram = new MacroApplication(macro, input);
            Term output = Interpreter.InterpretProgram(acceleratedProgram);

            if (output is not Syntax outputStx)
            {
                throw new ExpanderException.ExpectedEvaluation(nameof(Syntax), output, input);
            }

            exState.FlipScope(outputStx, introScope);

            //TODO remove macro scopes from keys of newly-inserted definitions(?)
            //TODO also do something with outside/inside edge scope(??)

            return outputStx;
        }

        private static readonly string[] _specialImperativeForms = new string[]
        {
            Keyword.DEFINE,
            Keyword.DEFINE_SYNTAX,
            Keyword.SET
        };

        private static readonly string[] _specialDefinitionForms = new string[]
        {
            Keyword.DEFINE,
            Keyword.DEFINE_SYNTAX
        };

        /// <summary>
        /// Expand the invocation of a core syntactic form.
        /// </summary>
        /// <param name="formName">The symbol corresponding to the form's default keyword.</param>
        /// <param name="stx">The entirety of the form's application expression.</param>
        private static Syntax ExpandCoreForm(string formName, Syntax stx, ExpansionContext exState)
        {
            if (stx is not SyntaxPair fullForm
                || fullForm.Car is not Identifier formId)
            {
                // all core forms require arguments
                throw new ExpanderException.InvalidFormInput(formName, stx);
            }
            
            if (fullForm.Cdr is not SyntaxPair args)
            {
                // those arguments are required to be a proper list of at least one element
                throw new ExpanderException.ExpectedProperList(fullForm);
            }

            // imperative commands are unallowed in expression contexts
            if (_specialImperativeForms.Contains(formName) && exState.Mode == ExpMode.Expression)
            {
                throw new ExpanderException.InvalidContext(formName, exState.Mode, stx);
            }

            // for each core form, expansion involves doing something with the arguments
            // dispatch to the handler depending on the keyword, then reassemble the final form

            Syntax tail = formName switch
            {
                Keyword.QUOTE => ExpandQuote(args, exState),
                Keyword.QUOTE_SYNTAX => ExpandQuote(args, exState),

                Keyword.LAMBDA => ExpandLambda(args, exState),
                Keyword.LET_SYNTAX => ExpandLetSyntax(args, exState),

                Keyword.DEFINE => ExpandDefine(args, exState),
                Keyword.DEFINE_SYNTAX => ExpandDefineSyntax(args, exState),

                _ => throw new ExpanderException.InvalidSyntax(stx)
            };

            return new SyntaxPair(formId, tail, stx);
        }

        private static Syntax ExpandQuote(SyntaxPair stx, ExpansionContext exState)
        {
            return stx; //it'll get stripped later during parsing
        }

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

                ExpansionContext subState = exState.ExpandInSubBlock(ExpMode.Body);

                foreach (Identifier parameter in EnumerateParameterList(stp.Car))
                {
                    RenameAndBindVariable(parameter, subState);
                }

                Syntax expandedBody = ExpandBody(stp.Cdr, subState);

                return new SyntaxPair(stp.Car, expandedBody, stx);
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
                Syntax expandedCar = Expand(stp.Car, exState.ExpandInMode(ExpMode.Expression));
                return new SyntaxPair(expandedCar, stp.Cdr, stx);
            }
            else
            {
                Syntax expandedCar = Expand(stp.Car, exState);
                Syntax expandedCdr = ExpandBody(stp.Cdr, exState);
                return new SyntaxPair(expandedCar, expandedCdr, stx);
            }
        }
        private static Syntax ExpandOperands(Syntax stx, ExpansionContext exState)
        {
            if (stx is SyntaxPair stp)
            {
                Syntax nextArg = Expand(stp.Car, exState.ExpandInMode(ExpMode.Expression));

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

                ExpansionContext subState = exState.ExpandInSubBlock(ExpMode.Body);

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

        private static Syntax ExpandDefine(Syntax stx, ExpansionContext exState)
        {
            UnpackDefinition(Keyword.DEFINE, stx, out Identifier key, out SyntaxPair rest);

            RenameAndBindVariable(key, exState);
            Syntax expandedRest = ExpandBody(rest, exState);

            return new SyntaxPair(key, expandedRest, stx);
        }

        private static Syntax ExpandDefineSyntax(Syntax stx, ExpansionContext exState)
        {
            UnpackDefinition(Keyword.DEFINE_SYNTAX, stx, out Identifier key, out SyntaxPair rest);

            Identifier renamedKey = RenameAndBindVariable(key, exState);
            MacroProcedure macro = ExpandAndEvalMacro(rest.Car, exState);

            exState.BindMacro(key, renamedKey, macro);

            return new Datum(VoidTerm.Value, stx.Location, stx);
        }

        private static MacroProcedure ExpandAndEvalMacro(Syntax input, ExpansionContext exState)
        {
            ExpansionContext subState = exState.ExpandInNewPhase();

            Syntax expandedInput = Expand(input, subState);
            CoreForm parsedInput = Parser.ParseSyntax(expandedInput, subState.GlobalBindingStore, subState.Phase);
            Term output = Interpreter.InterpretProgram(parsedInput, StandardEnv.CreateNew());

            if (output is MacroProcedure macro)
            {
                return macro;
            }

            throw new ExpanderException.ExpectedEvaluation(nameof(MacroProcedure), output, input);
        }

        #region Helpers

        private static Identifier RenameAndBindVariable(Identifier symId, ExpansionContext exState)
        {
            GenSym newSym = new GenSym(symId.SymbolicName);
            Identifier bindingId = new Identifier(newSym, symId);
            exState.BindVariable(symId, bindingId);

            return bindingId;
        }

        private static void UnpackDefinition(string formName, Syntax stx, out Identifier key, out SyntaxPair rest)
        {
            if (stx is SyntaxPair args)
            {
                if (args.Car is Identifier bindingKey1
                    && args.Cdr is SyntaxPair tail
                    && tail.Cdr.Expose() is Nil)
                {
                    key = bindingKey1;
                    rest = tail;
                    return;
                }
                else if (args.Car is SyntaxPair nameWithParams
                    && nameWithParams.Car is Identifier bindingKey2)
                {
                    key = bindingKey2;
                    rest = new SyntaxPair(
                        new Identifier(Symbol.ImplicitLambda, args),
                        new SyntaxPair(nameWithParams.Cdr, args.Cdr, args),
                        args);
                    return;
                }
            }

            throw new ExpanderException.InvalidFormInput(formName, stx);
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

    }
}
