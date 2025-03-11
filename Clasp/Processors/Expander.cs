using System;
using System.Collections.Generic;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Modules;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
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
                    return WrapImplicit(Symbols.S_Const, stx);
                }
            }
            catch (ClaspException cex)
            {
                throw new ExpanderException.InvalidSyntax(stx, cex);
            }
            throw new ExpanderException.InvalidSyntax(stx);
        }

        #region Basic Expansion

        /// <summary>
        /// Expand an identifier as a standalone expression.
        /// </summary>
        private static Syntax ExpandIdentifier(Identifier id, CompilationContext context)
        {
            if (id.TryResolveBinding(context.Phase, out RenameBinding? binding))
            {
                //if (binding.BoundType == BindingType.Transformer)
                //{
                //    return ExpandSyntaxTransformation(binding, id, context);
                //}
                //else
                if (binding.BoundType == BindingType.Special)
                {
                    throw new ExpanderException.InvalidForm(binding.Name, id);
                }

                return WrapImplicit(Symbols.S_Var, id);
            }
            else if (context.Mode != ExpansionMode.Module)
            {
                return WrapImplicit(Symbols.S_TopVar, id);
            }

            throw new ExpanderException.UnboundIdentifier(id);
        }

        /// <summary>
        /// Expand a function application form containing an identifier in the operator position.
        /// </summary>
        private static Syntax ExpandIdApplication(Identifier op, SyntaxPair stp, CompilationContext context)
        {
            if (op.TryResolveBinding(context.Phase, out RenameBinding? binding))
            {
                //if (binding.BoundType == BindingType.Transformer)
                //{
                //    return ExpandSyntaxTransformation(binding, stp, context);
                //}
                //else
                if (binding.BoundType == BindingType.Special)
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
                Syntax expandedList = ExpandExpressionList(stp, context);
                return WrapImplicit(Symbols.S_Apply, expandedList);
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
        private static SyntaxPair WrapImplicit(ReservedSymbol formSym, Syntax stx)
        {
            Syntax implicitOp = Syntax.WrapWithRef(formSym, stx);
            return new SyntaxPair(implicitOp, stx, stx.Location);
        }

        /// <summary>
        /// Expand the invocation of a special syntactic form (an IdApplication with a special form operator)
        /// </summary>
        /// <param name="boundName">The binding name of the operator for the form.</param>
        /// <param name="stp">The entirety of the form's application expression.</param>
        private static Syntax ExpandSpecialForm(string boundName, SyntaxPair stp, CompilationContext context)
        {
            if (Keywords.ExpandedKeywords.Contains(boundName))
            {
                // These keywords can ONLY appear as a result of expansion,
                // ergo a form starting with one must already have been expanded
                return stp;
            }

            if (!stp.TryUnpair(out Identifier? _, out SyntaxPair? args))
            {
                throw new ExpanderException.InvalidArguments(stp);
            }

            try
            {
                return boundName switch
                {
                    Keywords.QUOTE => WrapImplicit(Symbols.S_Const, args),
                    Keywords.QUOTE_SYNTAX => WrapImplicit(Symbols.S_Const_Syntax, args),

                    Keywords.DEFINE => ExpandDefine(args, context),
                    Keywords.S_PARTIAL_DEFINE => ExpandPartialDefine(args, context),

                    Keywords.SET => ExpandSet(args, context),

                    Keywords.IF => ExpandIf(args, context),
                    Keywords.BEGIN => ExpandSequence(args, context),

                    Keywords.APPLY => ExpandApplication(args, context),
                    Keywords.LAMBDA => ExpandLambda(args, context),

                    Keywords.MODULE => ExpandModuleForm(args, context),
                    Keywords.S_VISIT_MODULE => ExpandModuleVisit(args, context),
                    //Keywords.IMPORT => ExpandModuleImport(args, context),

                    //Keywords.DEFINE_SYNTAX => DefineTransformer(args, context),
                    //Keywords.IMPORT_FOR_SYNTAX => null,
                    //Keywords.BEGIN_FOR_SYNTAX => MetaExpand(args, context),

                    _ => throw new ExpanderException.InvalidSyntax(stp)
                };
            }
            catch (System.Exception ex)
            {
                throw new ExpanderException.InvalidForm(boundName, stp, ex);
            }
        }

        /// <summary>
        /// Partially expand <paramref name="stx"/> as a term in the body of a sequential form.
        /// </summary>
        /// <remarks>i.e. a <see cref="Keywords.LAMBDA"/>, <see cref="Keywords.BEGIN"/>, or <see cref="Keywords.MODULE"/> form</remarks>
        private static Syntax? PartiallyExpand(Syntax stx, CompilationContext context)
        {
            // essentially just check the direct path to special forms and disregard otherwise
            if (stx is SyntaxPair stp
                && stp.TryUnpair(out Identifier? op, out SyntaxPair? args)
                && op.TryResolveBinding(context.Phase, out RenameBinding? binding)
                && binding.BoundType == BindingType.Special)
            {
                if (binding.Name == Keywords.DEFINE_SYNTAX)
                {
                    // expand and bind the macro, then discard the syntax
                    //ExpandDefineSyntaxArgs(args, context);
                    return null;
                }
                else if (binding.Name == Keywords.IMPORT)
                {
                    return ExpandModuleImport(args, context);
                }
                else if (binding.Name == Keywords.EXPORT)
                {
                    ExpandExportedBindingList(args, context);
                    return null;
                }
                else if (binding.Name == Keywords.DEFINE)
                {
                    return ExpandDefine(args, context);
                }
            }
            return stx;
        }

        #endregion

        #region Meta-Syntax Related

        /// <summary>
        /// RETURNS UNDEFINED IF NOT TOP-LEVEL
        /// </summary>
        //private static Syntax ExpandDefineSyntaxArgs(Cons<Syntax, Term> stp, ScopeSet info, CompilationContext context)
        //{
        //    if (context.Mode == ExpansionMode.Expression)
        //    {
        //        throw new ExpanderException.InvalidContext(Keywords.DEFINE_SYNTAX, context.Mode, stp, info);
        //    }
        //    else if (TryRewriteDefineSyntaxArgs(stp, context, out Identifier? key, out Syntax? value))
        //    {
        //        context.SanitizeBindingKey(key);

        //        MacroProcedure macro = ExpandAndEvalMacro(value, context);
        //        if (!key.TryRenameAsMacro(context.Phase, out Identifier? bindingId))
        //        {
        //            throw new ExpanderException.InvalidBindingOperation(key, context);
        //        }
        //        else
        //        {
        //            context.CompileTimeEnv.Define(bindingId.Name, macro);
        //        }

        //        Syntax evaluatedMacro = ExpandImplicit(Symbols.StaticQuote, new Datum(macro, value), context);

        //        return Datum.FromDatum(VoidTerm.Value, info);
        //    }
        //    else
        //    {
        //        throw new ExpanderException.InvalidArguments(stp, info);
        //    }
        //}

        //private static Syntax ExpandSyntaxTransformation(RenameBinding binding, Syntax input, CompilationContext context)
        //{
        //    if (context.TryLookupMacro(binding, out MacroProcedure? macro))
        //    {
        //        Scope introScope = new Scope(input);
        //        Scope useSiteScope = new Scope(input);

        //        input.AddScope(context.Phase, introScope, useSiteScope);

        //        Syntax output = ApplySyntaxTransformer(macro, input);

        //        CompilationContext macroContext = context.InTransformed(useSiteScope);
        //        output.FlipScope(macroContext.Phase, useSiteScope);
        //        context.AddPendingInsideEdge(output);

        //        return Expand(output, macroContext);
        //    }
        //    else
        //    {
        //        throw new ExpanderException.UnboundMacro(binding.Id);
        //    }
        //}

        //private static Syntax ApplySyntaxTransformer(MacroProcedure macro, Syntax input)
        //{
        //    Term output;

        //    try
        //    {
        //        MacroApplication program = new MacroApplication(macro, input);
        //        output = Interpreter.InterpretProgram(program);
        //    }
        //    catch (System.Exception e)
        //    {
        //        throw new ExpanderException.EvaluationError(nameof(MacroProcedure), input, e);
        //    }

        //    if (output is not Syntax outputStx)
        //    {
        //        throw new ExpanderException.InvalidTransformation(output, macro, input);
        //    }

        //    return outputStx;
        //}

        //private static MacroProcedure ExpandAndEvalMacro(Syntax input, CompilationContext context)
        //{
        //    CompilationContext subState = context.InNextPhase();

        //    Term output;

        //    try
        //    {
        //        Syntax expandedInput = ExpandSyntax(input, subState);
        //        CoreForm parsedInput = Parser.ParseSyntax(expandedInput, subState.Phase);
        //        output = Interpreter.InterpretInVacuum(parsedInput);
        //    }
        //    catch (System.Exception e)
        //    {
        //        throw new ExpanderException.EvaluationError(nameof(MacroProcedure), input, e);
        //    }

        //    if (output is CompoundProcedure cp
        //        && cp.TryCoerceMacro(out MacroProcedure? macro))
        //    {
        //        return macro;
        //    }

        //    throw new ExpanderException.InvalidTransformer(output, input);
        //}

        //private static Syntax BeginForSyntax(SyntaxPair form, CompilationContext context)
        //{
        //    Cons<Syntax, Term> terms = form.Expose();
        //    ScopeSet info = form.LexContext;
        //    CompilationContext subState = context.InNextPhase();

        //    Term output;

        //    try
        //    {
        //        // Treat the sequent terms like a regular Begin form, but in the substate

        //        Cons<Syntax, Term> expandedSequence = ExpandSequence(terms, info, subState);
        //        SyntaxPair stxSequence = new SyntaxList(expandedSequence, info);
        //        SyntaxPair beginStx = ExpandImplicit(Symbols.StaticBegin, stxSequence, subState);

        //        // Nest the first Begin form inside a second, to add an implicit return value (#t)
        //        SyntaxPair nestedStx = new SyntaxList(new Datum(Boolean.True, info), info)
        //            .Push(beginStx);
        //        nestedStx = ExpandImplicit(Symbols.StaticBegin, nestedStx, subState);

        //        // Parse (still in the substate), which will de-nest the final list of terms
        //        CoreForm parsedInput = Parser.ParseSyntax(beginStx, subState.Phase);

        //        // Now interpret the parse, but back in THIS expansion's context
        //        // Which allows for mutation of the current compile-time environment
        //        output = Interpreter.InterpretProgram(parsedInput, subState.CompileTimeEnv.Root);
        //    }
        //    catch (System.Exception e)
        //    {
        //        throw new ExpanderException.EvaluationError(Keywords.BEGIN_FOR_SYNTAX, form, e);
        //    }

        //    // Check that the interpretation returned #t as expected
        //    if (output is not Boolean b || b != Boolean.True)
        //    {
        //        throw new ExpanderException.EvaluationError(Keywords.BEGIN_FOR_SYNTAX, form,
        //            string.Format("Compile-Time Interpretation yielded an unexpected value: {0}", output));
        //    }

        //    return Datum.FromDatum(VoidTerm.Value, form);
        //}

        //private static Syntax ImportForSyntax(SyntaxPair form, CompilationContext context)
        //{
        //    Term ls = form.Expose().Cdr;

        //    while (ls is Cons<Syntax, Term> remaining
        //        && remaining.TryMatchLeading(out Identifier? id, out ls))
        //    {
        //        context.CompileTimeEnv.Root.InstallModule(Module.InterpretModule(id.Name));
        //    }

        //    if (ls is not null)
        //    {
        //        throw new ExpanderException.InvalidSyntax(form);
        //    }

        //    return Datum.FromDatum(VoidTerm.Value, form);
        //}

        #endregion

        #region Recurrent Forms

        /// <summary>
        /// Expand a proper list of expressions.
        /// </summary>
        private static Syntax ExpandExpressionList(Syntax stx, CompilationContext context)
        {
            if (Nil.Is(stx))
            {
                return stx;
            }
            else if (stx.TryUnpair(out Syntax? nextExpr, out Syntax? tail))
            {
                Syntax expr = Expand(nextExpr, context.AsExpression());
                Syntax rest = ExpandExpressionList(tail, context);

                return Syntax.WrapWithRef(Cons.Truct(expr, rest), stx);
            }

            throw new ExpanderException.ExpectedProperList(nameof(Syntax), stx);
        }

        /// <summary>
        /// Partially expand a sequence of terms to capture any requisite bindings,
        /// then properly run through and expand each term in sequence.
        /// </summary>
        private static Syntax ExpandBody(Syntax stx, CompilationContext context)
        {
            Syntax partiallyExpandedBody = PartiallyExpandSequence(stx, context);
            
            if (context.Mode == ExpansionMode.TopLevel || context.Mode == ExpansionMode.Module
                && context.ImportedScopes.Any())
            {
                stx.AddScope(context.Phase, context.ImportedScopes.ToArray());
            }

            return ExpandSequence(partiallyExpandedBody, context.AsPartial());
        }

        /// <summary>
        /// Recur through a sequence of terms, recording bindings for any definitions,
        /// and meta-evaluating and binding values for any macros
        /// </summary>
        private static Syntax PartiallyExpandSequence(Syntax stx, CompilationContext context)
        {
            if (Nil.Is(stx))
            {
                return stx;
            }
            else if (stx.TryUnpair(out Syntax? nextItem, out Syntax? tail))
            {
                if (context.Mode == ExpansionMode.Sequential && Nil.Is(tail))
                {
                    Syntax? expandedItem = PartiallyExpand(nextItem, context.AsExpression());

                    if (expandedItem is null)
                    {
                        throw new ExpanderException.InvalidContext("Vanishing Form", context.Mode, stx);
                    }
                    else
                    {
                        return Syntax.WrapWithRef(Cons.Truct(expandedItem, tail), stx);
                    }
                }
                else
                {
                    Syntax? expandedItem = PartiallyExpand(nextItem, context);

                    if (expandedItem is null)
                    {
                        return PartiallyExpandSequence(tail, context);
                    }
                    else
                    {
                        Syntax expandedTail = PartiallyExpandSequence(tail, context);
                        return Syntax.WrapWithRef(Cons.Truct(expandedItem, expandedTail), stx);
                    }
                }
            }

            throw new ExpanderException.ExpectedProperList(stx);
        }

        /// <summary>
        /// Recur through a sequence of terms, expanding and replacing each one.
        /// </summary>
        private static Syntax ExpandSequence(Syntax stx, CompilationContext context)
        {
            if (Nil.Is(stx))
            {
                return stx;
            }
            else if (stx.TryUnpair(out Syntax? nextItem, out Syntax? tail))
            {
                if (Nil.Is(tail))
                {
                    Syntax expandedItem = context.Mode == ExpansionMode.Sequential
                        ? Expand(nextItem, context.AsExpression())
                        : Expand(nextItem, context);

                    return Syntax.WrapWithRef(Cons.Truct(expandedItem, tail), stx);
                }
                else
                {
                    Syntax expandedItem = Expand(nextItem, context);
                    Syntax expandedTail = ExpandSequence(tail, context);

                    return Syntax.WrapWithRef(Cons.Truct(expandedItem, expandedTail), stx);
                }

            }

            throw new ExpanderException.ExpectedProperList(nameof(Syntax), stx);
        }

        #endregion

        #region Binding

        private static Identifier RenameVariableBinding(Syntax maybeId, CompilationContext context)
        {
            if (maybeId is Identifier id)
            {
                return id.TryRenameAsVariable(context.Phase, out Identifier bound)
                    ? bound
                    : id;
            }
            throw new ExpanderException.InvalidSyntax(maybeId);
        }

        private static Identifier RenameMacroBinding(Syntax maybeId, CompilationContext context)
        {
            if (maybeId is Identifier id)
            {
                return id.TryRenameAsMacro(context.Phase, out Identifier bound)
                    ? bound
                    : id;
            }
            throw new ExpanderException.InvalidSyntax(maybeId);
        }

        private static Identifier RenameModuleBinding(Syntax maybeId, CompilationContext context)
        {
            if (maybeId is Identifier id)
            {
                return id.TryRenameAsModule(context.Phase, out Identifier bound)
                    ? bound
                    : id;
            }
            throw new ExpanderException.InvalidSyntax(maybeId);
        }

        /// <summary>
        /// Extracts a list of parameters from a lambda's parameter list while binding each, possibly including a variadic
        /// "dotted" parameter from the end.
        /// </summary>
        /// <exception cref="ExpanderException.ExpectedProperList"></exception>
        private static Syntax ExpandVariableBindingList(Syntax stx, CompilationContext context, out Identifier? dotted)
        {
            dotted = null;

            if (Nil.Is(stx))
            {
                return stx;
            }
            else if (stx is Identifier lastParam)
            {
                RenameVariableBinding(lastParam, context);
                dotted = lastParam;
                return Datum.NullSyntax();
            }
            else if (stx.TryUnpair(out Identifier? nextParam, out Syntax? tail))
            {
                RenameVariableBinding(nextParam, context);
                Syntax expandedTail = ExpandVariableBindingList(tail, context, out dotted);
                return Syntax.WrapWithRef(Cons.Truct(nextParam, expandedTail), stx);
            }
            else
            {
                throw new ExpanderException.InvalidForm("Lambda Parameter", stx);
            }
        }

        private static void ExpandImportedModuleList(Syntax stx, CompilationContext context)
        {
            if (Nil.Is(stx))
            {
                return;
            }
            else if (stx.TryUnpair(out Identifier? nextModule, out Syntax? remaining))
            {
                string mdlName = nextModule.Expose().Name;
                context.ImportScope(ModuleCache.GetScope(mdlName));

                ExpandImportedModuleList(remaining, context);
            }
            else
            {
                throw new ExpanderException.InvalidForm("Module Importation", stx);
            }
        }

        private static void ExpandExportedBindingList(Syntax stx, CompilationContext context)
        {
            Syntax target = stx;

            while (target.TryUnpair(out Identifier? nextExport, out Syntax? tail))
            {
                //context.SanitizeBindingKey(nextExport);
                RenameVariableBinding(nextExport, context);
                context.CollectIdentifier(nextExport);

                target = tail;
            }

            if (!Nil.Is(target))
            {
                throw new ParserException.ExpectedProperList(stx);
            }
        }

        #endregion

        #region Syntactic Special Forms

        private static Syntax ExpandDefine(SyntaxPair stp, CompilationContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keywords.DEFINE, context.Mode, stp);
            }
            else if (stp.TryDelist(out Identifier? key, out Syntax? value))
            {
                context.SanitizeBindingKey(key);
                RenameVariableBinding(key, context);

                if (context.Mode == ExpansionMode.Sequential)
                {
                    context.CollectIdentifier(key);
                }
                else if (context.Mode == ExpansionMode.TopLevel)
                {
                    value = Expand(value, context.AsExpression());
                    return Syntax.WrapWithRef(Cons.ProperList(Symbols.S_TopDefine, key, value), stp);
                }

                return Syntax.WrapWithRef(Cons.ProperList(Symbols.S_PartialDefine, key, value), stp);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp);
            }
        }

        private static Syntax ExpandPartialDefine(SyntaxPair stp, CompilationContext context)
        {
            if (context.Mode != ExpansionMode.Partial)
            {
                throw new ExpanderException.InvalidContext(Keywords.S_PARTIAL_DEFINE, context.Mode, stp);
            }
            else if (stp.TryDelist(out Identifier? key, out Syntax? value))
            {
                // key has already been renamed in the partial pass
                value = Expand(value, context.AsExpression());

                return Syntax.WrapWithRef(Cons.ProperList(Symbols.S_Set, key, value), stp);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp);
            }
        }

        private static Syntax ExpandSet(SyntaxPair stp, CompilationContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keywords.SET, context.Mode, stp);
            }
            else if (stp.TryDelist(out Identifier? key, out Syntax? value))
            {
                context.SanitizeBindingKey(key);
                RenameVariableBinding(key, context);
                value = Expand(value, context.AsExpression());

                return Syntax.WrapWithRef(Cons.ProperList(Symbols.S_Set, key, value), stp);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp);
            }
        }

        private static Syntax ExpandIf(SyntaxPair stp, CompilationContext context)
        {
            if (stp.TryDelist(out Syntax? condValue, out Syntax? thenValue, out Syntax? elseValue))
            {
                Syntax expandedCond = Expand(condValue, context.AsExpression());
                Syntax expandedThen = Expand(thenValue, context.AsExpression());
                Syntax expandedElse = Expand(elseValue, context.AsExpression());

                return Syntax.WrapWithRef(Cons.ProperList(Symbols.S_If, expandedCond, expandedElse, expandedThen), stp);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp);
            }
        }

        private static Syntax ExpandLambda(SyntaxPair stp, CompilationContext context)
        {
            if (stp.TryUnpair(out Syntax? formals, out SyntaxPair? tail))
            {
                Scope outsideEdge = new Scope($"{Keywords.LAMBDA} Outside-Edge", stp.Location);
                Scope insideEdge = new Scope($"{Keywords.LAMBDA} Inside-Edge", stp.Location);

                stp.AddScope(context.Phase, outsideEdge, insideEdge);
                stp.AddScope(context.Phase, outsideEdge, insideEdge);

                CompilationContext bodyContext = context.InLambdaBody(insideEdge);

                Syntax expandedFormals = ExpandVariableBindingList(formals, bodyContext, out Identifier? dottedParam);

                Syntax expandedBody = ExpandBody(tail, bodyContext);

                Term fullForm = Cons.ImproperList(
                    Symbols.S_Lambda,
                    expandedFormals, dottedParam ?? (Term)Nil.Value,
                    Cons.ProperList(context.CollectedIdentifiers),
                    expandedBody);

                return Syntax.WrapWithRef(fullForm, stp);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp);
            }
        }

        private static Syntax ExpandModuleForm(SyntaxPair stp, CompilationContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keywords.MODULE, context.Mode, stp);
            }
            else if (stp.TryUnpair(out Identifier? mdlId, out Syntax? tail))
            {
                if (Module.DetectCircularReference(mdlId.Expose().Name))
                {
                    throw new ExpanderException.CircularModuleReference(ModuleCache.Get(mdlId.Name), stp);
                }

                RenameModuleBinding(mdlId, context);

                Syntax visitTail = WrapImplicit(Symbols.S_VisitModule, tail);

                try
                {
                    Module.Visit(mdlId.Expose().Name, visitTail);
                }
                catch (Exception ex)
                {
                    throw new ExpanderException.ErrorVisitingModule(ModuleCache.Get(mdlId.Name), stp, ex);
                }

                return WrapImplicit(Symbols.S_Const, Syntax.WrapWithRef(VoidTerm.Value, stp));
            }

            throw new ExpanderException.InvalidArguments(stp);
        }

        private static Syntax ExpandModuleVisit(SyntaxPair stp, CompilationContext context)
        {
            if (context.Mode != ExpansionMode.Module)
            {
                throw new ExpanderException.InvalidContext(Keywords.S_VISIT_MODULE, context.Mode, stp);
            }

            Syntax expandedBody = ExpandBody(stp, context);

            Term fullForm = Cons.ImproperList(
                Symbols.S_Module_Begin,
                Cons.ProperList(context.CollectedIdentifiers),
                expandedBody);

            return Syntax.WrapWithRef(fullForm, stp);
        }

        private static Syntax ExpandModuleImport(SyntaxPair stp, CompilationContext context)
        {
            // Imports should be expanded during the partial phase, when the bodies still have their original context
            if (context.Mode != ExpansionMode.TopLevel && context.Mode != ExpansionMode.Module)
            {
                throw new ExpanderException.InvalidContext(Keywords.IMPORT, context.Mode, stp);
            }

            ExpandImportedModuleList(stp, context);

            return Syntax.WrapWithRef(Cons.Truct(Symbols.S_Import, stp), stp);
        }

        #endregion
    }
}
