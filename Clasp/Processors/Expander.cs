using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Binding.Modules;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Exceptions;
using Clasp.ExtensionMethods;

namespace Clasp.Process
{
    internal static class Expander
    {
        public static Syntax Expand(Syntax stx, CompilationContext context)
        {
            try
            {
                if (stx is Identifier id)
                {
                    return ExpandIdentifier(id, context);
                }
                else if (stx is SyntaxPair idApp && idApp.Expose().Car is Identifier op)
                {
                    return ExpandIdApplication(op, idApp, context);
                }
                else if (stx is SyntaxPair app)
                {
                    return ExpandApplication(app, context);
                }
                else
                {
                    return ExpandImplicit(Symbols.S_Const, stx, context);
                }
            }
            catch (ClaspException cex)
            {
                throw new ExpanderException.InvalidSyntax(stx, cex);
            }
            throw new ExpanderException.InvalidSyntax(stx);
        }

        public static Syntax ExpandAnticipatedForm(string keyword, Syntax stx, CompilationContext context)
        {
            if (stx is SyntaxPair idApp
                && idApp.Expose().Car is Identifier op
                && op.TryResolveBinding(context.Phase, out RenameBinding? binding)
                && binding.Name == keyword)
            {
                return ExpandSpecialForm(keyword, idApp, context);
            }
            throw new ExpanderException.InvalidForm(keyword, stx);
        }

        #region Basic Expansion

        /// <summary>
        /// Expand an identifier as a standalone expression.
        /// </summary>
        private static Syntax ExpandIdentifier(Identifier id, CompilationContext context)
        {
            if (id.TryResolveBinding(context.Phase, out RenameBinding? binding))
            {
                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding, id, context);
                }
                else if (binding.BoundType == BindingType.Special)
                {
                    throw new ExpanderException.InvalidForm(binding.Name, id);
                }

                throw new ExpanderException.UnboundIdentifier(id);
            }
            else if (context.Mode != ExpansionMode.Module)
            {
                return ExpandImplicit(Symbols.S_TopVar, id, context);
            }

            throw new ExpanderException.InvalidSyntax(id);
        }

        /// <summary>
        /// Expand a function application form containing an identifier in the operator position.
        /// </summary>
        private static Syntax ExpandIdApplication(Identifier op, SyntaxPair stp, CompilationContext context)
        {
            if (op.TryResolveBinding(context.Phase, out RenameBinding? binding))
            {
                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding, stp, context);
                }
                else if (binding.BoundType == BindingType.Special)
                {
                    return ExpandSpecialForm(binding.Name, stp, context);
                }
            }

            return ExpandApplication(stp, context);
        }

        /// <summary>
        /// Expand a function application form containing an arbitrary expression in the operator position.
        /// </summary>
        private static Syntax ExpandApplication(SyntaxPair stp, CompilationContext context)
        {
            try
            {
                stp = ExpandExpressionList(stp, context);
                return ExpandImplicit(Symbols.S_Apply, stp, context);
            }
            catch (ExpanderException ee)
            {
                throw new ExpanderException.InvalidForm(Keywords.S_APPLY, stp, ee);
            }
        }

        /// <summary>
        /// Prepend <paramref name="stl"/> with a special <see cref="Identifier"/> that shares its
        /// <see cref="ScopeSet"/>, indicating how it should be handled by the <see cref="Parser"/>.
        /// </summary>
        private static SyntaxPair ExpandImplicit(ReservedSymbol formSym, Syntax stx, CompilationContext context)
        {
            Syntax implicitOp = Syntax.WrapWithRef(formSym, stx);
            return stx.ListPrepend(implicitOp);
        }

        /// <summary>
        /// Expand the invocation of a special syntactic form (an IdApplication with a special form operator)
        /// </summary>
        /// <param name="boundName">The binding name of the operator for the form.</param>
        /// <param name="stp">The entirety of the form's application expression.</param>
        private static Syntax ExpandSpecialForm(string boundName, SyntaxPair stp, CompilationContext context)
        {
            if (Keywords.SecretKeywords.Contains(boundName))
            {
                // These keywords can ONLY appear as a result of expansion,
                // ergo a form starting with one must already have been expanded
                return stp;
            }

            if (!stp.TryDeconstruct(out Identifier? op, out SyntaxPair? args))
            {
                throw new ExpanderException.InvalidArguments(stp);
            }

            try
            {
                // certain meta-syntactic forms need to be expanded and returned all at once
                Syntax? metaSyntax = boundName switch
                {
                    Keywords.MODULE => ExpandModuleForm(stp),
                    Keywords.DEFINE_SYNTAX => DefineTransformer(args, context),
                    Keywords.S_META => MetaExpand(args, context),

                    _ => null
                };

                if (metaSyntax is not null)
                {
                    return metaSyntax;
                }

                // else it must be a "normal" special form where we expand the arguments and rebuild the form
                Syntax expandedTail = boundName switch
                {
                    Keywords.QUOTE => args,
                    Keywords.QUOTE_SYNTAX => args,

                    Keywords.DEFINE => ExpandDefineArgs(args, context),
                    Keywords.S_PARTIAL_DEFINE => ExpandPartialDefineArgs(args, context),

                    Keywords.SET => ExpandSetArgs(args, context),

                    Keywords.IF => ExpandIfArgs(args, info, context),
                    Keywords.BEGIN => ExpandSequence(args, info, context),

                    Keywords.APPLY => ExpandExpressionList(args, info, context),
                    Keywords.LAMBDA => ExpandLambdaArgs(args, info, context),

                    Keywords.MODULE => ExpandModuleForm(args, context),
                    Keywords.IMPORT => ExpandImportForm(args, context),
                    Keywords.EXPORT => ExpandExportForm(args, context),

                    _ => throw new ExpanderException.InvalidSyntax(stp)
                };

                return expandedTail.ListPrepend(op);
            }
            catch (System.Exception ex)
            {
                throw new ExpanderException.InvalidForm(boundName, stp, ex);
            }
        }

        private static Syntax MetaExpand()

        ///// <summary>
        ///// Partially expand <paramref name="stx"/> as a term in the body of a sequential form.
        ///// </summary>
        ///// <remarks>i.e. a <see cref="Keywords.LAMBDA"/>, <see cref="Keywords.BEGIN"/>, or <see cref="Keywords.MODULE"/></remarks>
        //private static Syntax? PartiallyExpandSeqBodyTerm(Syntax stx, CompilationContext context)
        //{
        //    // essentially just check the path to special forms and disregard otherwise
        //    if (stx is SyntaxPair stp
        //        && stp.Expose().Car is Identifier op
        //        && op.TryResolveBinding(context.Phase, out RenameBinding? binding)
        //        && binding.BoundType == BindingType.Special)
        //    {
        //        Cons<Syntax, Term> args = stp.Expose();

        //        if (binding.Name == Keywords.DEFINE_SYNTAX)
        //        {
        //            // expand and bind the macro, then discard the syntax
        //            ExpandDefineSyntaxArgs(args, stx.LexContext, context);
        //            return null;
        //        }
        //        else if (binding.Name == Keywords.DEFINE)
        //        {
        //            // extract and rename the key, then rewrite the form to indicate we did so

        //            if (TryRewriteDefineArgs(args, out Identifier? key, out Syntax? value))
        //            {
        //                if (!key.TryRenameAsVariable(context.Phase, out _))
        //                {
        //                    throw new ExpanderException.InvalidBindingOperation(key, context);
        //                }

        //                Identifier newOp = new Identifier(Symbols.StaticParDef, op);

        //                return new SyntaxList(value, op.LexContext)
        //                    .Push(key)
        //                    .Push(newOp);
        //            }
        //            else
        //            {
        //                throw new ExpanderException.InvalidForm(Keywords.DEFINE, stx);
        //            }
        //        }

        //        // let-syntax isn't partially expanded because it cannot legally expand into a definition
        //    }
        //    return stx;
        //}

        //private static Syntax? PartiallyExpandModuleBodyTerm(Syntax stx, CompilationContext context, out Identifier[] exportations)
        //{
        //    exportations = [];

        //    if (stx is SyntaxPair stp
        //        && stp.Expose().Car is Identifier op
        //        && op.TryResolveBinding(context.Phase, out RenameBinding? binding)
        //        && binding.BoundType == BindingType.Special)
        //    {
        //        Cons<Syntax, Term> args = stp.Expose();

        //        if (binding.Name == Keywords.DEFINE_SYNTAX)
        //        {
        //            // expand and bind the macro AND return the syntax
        //            return new SyntaxList(ExpandDefineSyntaxArgs(args, stx.LexContext, context), stx.LexContext)
        //                .Push(op);
        //        }
        //        else if (binding.Name == Keywords.DEFINE)
        //        {
        //            // extract and rename the key, then rewrite the form to indicate we did so

        //            if (TryRewriteDefineArgs(args, out Identifier? key, out Syntax? value))
        //            {
        //                if (!key.TryRenameAsVariable(context.Phase, out _))
        //                {
        //                    throw new ExpanderException.InvalidBindingOperation(key, context);
        //                }

        //                Identifier newOp = new Identifier(Symbols.StaticParDef, op);

        //                return new SyntaxList(value, op.LexContext)
        //                    .Push(key)
        //                    .Push(newOp);
        //            }
        //            else
        //            {
        //                throw new ExpanderException.InvalidForm(Keywords.DEFINE, stx);
        //            }
        //        }
        //        else if (binding.Name == Keywords.EXPORT)
        //        {
        //            List<Identifier> ids = new List<Identifier>();
        //            Term cdr = args;

        //            while (args.TryMatchLeading(out Identifier? id, out cdr))
        //            {
        //                ids.Add(id);
        //            }

        //            if (cdr is not Nil)
        //            {
        //                throw new ExpanderException.ExpectedProperList(nameof(Identifier), args, stx.LexContext);
        //            }

        //            exportations = ids.ToArray();
        //            return null;
        //        }
        //    }
        //    return stx;
        //}

        #endregion

        #region Meta-Syntax Related


        private static Syntax ExpandDefineSyntaxArgs(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keywords.DEFINE_SYNTAX, context.Mode, stp, info);
            }
            else if (TryRewriteDefineSyntaxArgs(stp, context, out Identifier? key, out Syntax? value))
            {
                context.SanitizeBindingKey(key);

                MacroProcedure macro = ExpandAndEvalMacro(value, context);
                if (!key.TryRenameAsMacro(context.Phase, out Identifier? bindingId))
                {
                    throw new ExpanderException.InvalidBindingOperation(key, context);
                }
                else
                {
                    context.CompileTimeEnv.Define(bindingId.Name, macro);
                }

                Syntax evaluatedMacro = ExpandImplicit(Symbols.StaticQuote, new Datum(macro, value), context);

                return Datum.FromDatum(VoidTerm.Value, info);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static Syntax ExpandSyntaxTransformation(RenameBinding binding, Syntax input, CompilationContext context)
        {
            if (context.TryLookupMacro(binding, out MacroProcedure? macro))
            {
                Scope introScope = new Scope(input);
                Scope useSiteScope = new Scope(input);

                input.AddScope(context.Phase, introScope, useSiteScope);

                Syntax output = ApplySyntaxTransformer(macro, input);

                CompilationContext macroContext = context.InTransformed(useSiteScope);
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

        private static MacroProcedure ExpandAndEvalMacro(Syntax input, CompilationContext context)
        {
            CompilationContext subState = context.InNextPhase();

            Term output;

            try
            {
                Syntax expandedInput = ExpandSyntax(input, subState);
                CoreForm parsedInput = Parser.ParseSyntax(expandedInput, subState.Phase);
                output = Interpreter.InterpretInVacuum(parsedInput);
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

        private static Syntax BeginForSyntax(SyntaxPair form, CompilationContext context)
        {
            Cons<Syntax, Term> terms = form.Expose();
            ScopeSet info = form.LexContext;
            CompilationContext subState = context.InNextPhase();

            Term output;

            try
            {
                // Treat the sequent terms like a regular Begin form, but in the substate

                Cons<Syntax, Term> expandedSequence = ExpandSequence(terms, info, subState);
                SyntaxPair stxSequence = new SyntaxList(expandedSequence, info);
                SyntaxPair beginStx = ExpandImplicit(Symbols.StaticBegin, stxSequence, subState);

                // Nest the first Begin form inside a second, to add an implicit return value (#t)
                SyntaxPair nestedStx = new SyntaxList(new Datum(Boolean.True, info), info)
                    .Push(beginStx);
                nestedStx = ExpandImplicit(Symbols.StaticBegin, nestedStx, subState);

                // Parse (still in the substate), which will de-nest the final list of terms
                CoreForm parsedInput = Parser.ParseSyntax(beginStx, subState.Phase);

                // Now interpret the parse, but back in THIS expansion's context
                // Which allows for mutation of the current compile-time environment
                output = Interpreter.InterpretProgram(parsedInput, subState.CompileTimeEnv.Root);
            }
            catch (System.Exception e)
            {
                throw new ExpanderException.EvaluationError(Keywords.BEGIN_FOR_SYNTAX, form, e);
            }

            // Check that the interpretation returned #t as expected
            if (output is not Boolean b || b != Boolean.True)
            {
                throw new ExpanderException.EvaluationError(Keywords.BEGIN_FOR_SYNTAX, form,
                    string.Format("Compile-Time Interpretation yielded an unexpected value: {0}", output));
            }

            return Datum.FromDatum(VoidTerm.Value, form);
        }

        private static Syntax ImportForSyntax(SyntaxPair form, CompilationContext context)
        {
            Term ls = form.Expose().Cdr;

            while (ls is Cons<Syntax, Term> remaining
                && remaining.TryMatchLeading(out Identifier? id, out ls))
            {
                context.CompileTimeEnv.Root.InstallModule(Module.InterpretModule(id.Name));
            }

            if (ls is not null)
            {
                throw new ExpanderException.InvalidSyntax(form);
            }

            return Datum.FromDatum(VoidTerm.Value, form);
        }

        #endregion

        #region Recurrent Forms

        /// <summary>
        /// Expand a proper list of expressions.
        /// </summary>
        private static SyntaxPair ExpandExpressionList(Syntax stx, CompilationContext context)
        {
            Syntax expandedCar = Expand(stp.Car, context.AsExpression());

            if (stp.Cdr == Datum.NullSyntax)
            {
                return new (expandedCar, stp.Cdr);
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
        private static void ExpandParameterList(Term t, ScopeSet info, CompilationContext context)
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
            else if (t is SyntaxPair stl)
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
        private static Cons<Syntax, Term> ExpandBody(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
        {
            Cons<Syntax, Term> partiallyExpandedBody = PartiallyExpandBody(stp, info, context);

            return ExpandSequence(partiallyExpandedBody, info, context);
        }

        /// <summary>
        /// Recur through a sequence of terms, recording bindings for any definitions,
        /// (and discarding those definitions if they bind macros)
        /// </summary>
        private static Cons<Syntax, Term> PartiallyExpandBody(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
        {
            Syntax? partiallyExpandedBodyTerm = PartiallyExpandSeqBodyTerm(stp.Car, context);

            if (partiallyExpandedBodyTerm is null)
            {
                // internal syntax definitions are recorded, then discarded
                // being inside closures, they couldn't be accessed again anyways

                if (stp.Cdr is Nil)
                {
                    throw new ExpanderException.InvalidContext("Definition", ExpansionMode.Expression, stp, info);
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
        private static Cons<Syntax, Term> ExpandSequence(Cons<Syntax, Term> stxList, ScopeSet info, CompilationContext context)
        {
            if (stxList.Car is Syntax stx)
            {
                if (stxList.Cdr is Nil n)
                {
                    CompilationContext finalTermContext = context.Mode != ExpansionMode.Module
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
        private static void ExpandLetSyntaxBindingList(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
        {
            if (stp.Car is SyntaxPair stl)
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

            throw new ExpanderException.ExpectedProperList(nameof(SyntaxPair), stp, info);
        }

        /// <summary>
        /// Partially expand the sequence of terms as the body elements of a module.
        /// </summary>
        private static Cons<Syntax, Term> PartiallyExpandModuleBody(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context, List<Identifier> exportations)
        {
            Syntax? partiallyExpandedBodyTerm = PartiallyExpandModuleBodyTerm(stp.Car, context, out Identifier[] exports);
            exportations.AddRange(exports);

            if (partiallyExpandedBodyTerm is null)
            {
                // export forms are discarded after being partially evaluated

                if (stp.Cdr is Nil n)
                {
                    return new Cons<Syntax, Term>(Datum.FromDatum(VoidTerm.Value, info), Nil.Value);
                }
                else if (stp.Cdr is Cons<Syntax, Term> cdr)
                {
                    return PartiallyExpandModuleBody(cdr, info, context, exportations);
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

        #endregion

        #region Native Rewriting Methods

        /// <summary>
        /// Given the arguments to a <see cref="Keywords.DEFINE"/> form,
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
        /// Given the arguments to a <see cref="Keywords.DEFINE_SYNTAX"/> form,
        /// rewrite them explicitly into the standard key/lambda pair format.
        /// </summary>
        private static bool TryRewriteDefineSyntaxArgs(Cons<Syntax, Term> stp, CompilationContext context,
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

        private static SyntaxPair BuildLambda(Syntax formals, Cons<Syntax, Term> body)
        {
            Cons<Syntax, Term> args = Cons.Truct<Syntax, Term>(formals, body);
            Identifier op = new Identifier(Symbols.StaticLambda, formals);
            Cons<Syntax, Term> lambda = Cons.Truct<Syntax, Term>(op, args);

            return new SyntaxList(lambda, body.Car.LexContext);
        }

        // (define-whatever (key . formals) . body)
        private static bool TryDestructImplicitLambda(Cons<Syntax, Term> stp,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? formals,
            [NotNullWhen(true)] out Cons<Syntax, Term>? body)
        {
            if (stp.TryMatchLeading(out SyntaxPair? nameAndFormals, out Term? tail)
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
        private static bool TryDestructExplicitLambda(Cons<Syntax, Term> stp, CompilationContext context,
            [NotNullWhen(true)] out Identifier? key,
            [NotNullWhen(true)] out Syntax? value)
        {
            return TryDestructKeyValuePair(stp, out key, out value)
                && value.Expose() is Cons<Syntax, Term> maybeLambda
                && maybeLambda.Car is Identifier maybeOp
                && maybeOp.TryResolveBinding(context.Phase, out RenameBinding? binding)
                && (binding.Name == Keywords.LAMBDA || binding.Name == Keywords.STATIC_LAMBDA);
        }

        /// <summary>
        /// Given the arguments to a <see cref="Keywords.IF"/> form,
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

        #region Syntactic Special Forms

        private static Cons<Syntax, Term> ExpandPartialDefineArgs(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
        {
            if (context.Mode != ExpansionMode.InternalDefinition)
            {
                throw new ExpanderException.InvalidContext(Keywords.STATIC_PARDEF, context.Mode, stp, info);
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

        private static Cons<Syntax, Term> ExpandDefineArgs(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keywords.DEFINE, context.Mode, stp, info);
            }
            else if (TryRewriteDefineArgs(stp, out Identifier? key, out Syntax? value))
            {
                context.SanitizeBindingKey(key);

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

        private static Cons<Syntax, Term> ExpandSetArgs(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keywords.SET, context.Mode, stp, info);
            }
            else if (TryRewriteDefineArgs(stp, out Identifier? key, out Syntax? value))
            {
                context.SanitizeBindingKey(key);

                RenameBinding binding = ResolveBinding(key, context);
                Syntax expandedValue = Expand(value, context.AsExpression());

                return new Cons<Syntax, Term>(key, Cons.Truct(expandedValue, Nil.Value));
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        private static Cons<Syntax, Term> ExpandIfArgs(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
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

        private static Cons<Syntax, Term> ExpandLambdaArgs(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
        {
            if (stp.TryMatchLeading(out Syntax? formals, out Term? cdr)
                && (formals.Expose() is Nil or Cons<Syntax, Term> || formals is Identifier)
                && cdr is Cons<Syntax, Term> body)
            {
                Scope outsideEdge = new Scope(formals);
                Scope insideEdge = new Scope(formals);

                formals.AddScope(context.Phase, outsideEdge, insideEdge);
                formals.AddScope(context.Phase, outsideEdge, insideEdge);

                CompilationContext bodyContext = context.InLambdaBody(insideEdge);

                ExpandParameterList(formals, info, context);

                Cons<Syntax, Term> expandedBody = ExpandBody(body, info, bodyContext);

                return Cons.Truct<Syntax, Term>(formals, expandedBody);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp, info);
            }
        }

        #endregion

        private static Syntax ExpandModuleForm(SyntaxPair stl)
        {
            if (stl.Expose() is Cons cns
                && cns.Car is Identifier id
                && stl.PopFront() is SyntaxPair body)
            {
                FreshModule fm = FreshModule.FromSyntax(id, body);
                ExpandedModule em = ExpandedModule.Expand(fm);

                return Datum.FromDatum(new ModuleTerm(em), stl.LexContext);
            }

            throw new ExpanderException.InvalidForm(Keywords.MODULE, stl);
        }

        #region Helpers
        private static RenameBinding ResolveBinding(Identifier id, CompilationContext context)
        {
            if (id.TryResolveBinding(context.Phase, out RenameBinding? binding))
            {
                return binding;
            }
            else
            {
                throw new ExpanderException.UnboundIdentifier(id);
            }
        }

        #endregion
    }
}
