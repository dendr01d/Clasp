using System;
using System.Collections.Generic;
using System.Linq;

using Clasp.Binding;
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
                //TODO the racket spec has some particular semantics here that I know I need to fix

                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding, id, context);
                }
                else if (context.CompileTimeEnv.TryGetValue(binding.Name, out Term? boundDef))
                {
                    return Syntax.WrapWithRef(boundDef, id);
                }
                //else if (binding.BoundType == BindingType.Special)
                //{
                //    throw new ExpanderException.InvalidForm(binding.Name, id);
                //}
                else
                {
                    return id;
                    //return WrapImplicit(Symbols.S_Var, id);
                }
            }
            else if (context.Mode != ExpansionMode.TopLevel)
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
                if (binding.BoundType == BindingType.Transformer)
                {
                    return ExpandSyntaxTransformation(binding, stp, context);
                }
                else
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
                return WrapImplicit(Symbols.S_App, expandedList);
            }
            catch (ExpanderException ee)
            {
                throw new ExpanderException.InvalidForm(Keywords.S_APP, stp, ee);
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
                    Keywords.QUOTE => ExpandQuote(args, context),
                    Keywords.QUOTE_SYNTAX => ExpandQuoteSyntax(args, context),

                    Keywords.DEFINE => ExpandDefine(args, context),
                    Keywords.S_PARTIAL_DEFINE => ExpandPartialDefine(args, context),

                    Keywords.SET => ExpandSet(args, context),

                    Keywords.IF => ExpandIf(args, context),
                    Keywords.BEGIN => ExpandBegin(args, context),

                    Keywords.APPLY => ExpandApplyForm(args, context),
                    Keywords.LAMBDA => ExpandLambda(args, context),

                    Keywords.MODULE => ExpandModuleForm(args, context),
                    Keywords.S_VISIT_MODULE => ExpandModuleVisit(args, context),
                    //Keywords.IMPORT_FROM => ExpandImportFrom(args, context),

                    Keywords.DEFINE_SYNTAX => EvalDefineSyntax(args, context),
                    //Keywords.BEGIN_FOR_SYNTAX => MetaExpand(args, context),

                    Keywords.SYNTAX => ExpandSyntaxForm(args, context), // the special form, not just any syntax

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
                    Syntax macroDef = EvalDefineSyntax(args, context);
                    return context.Mode == ExpansionMode.TopLevel
                        ? macroDef
                        : null;
                }
                else if (binding.Name == Keywords.BEGIN_FOR_SYNTAX)
                {
                    EvalBeginForSyntax(args, context);
                    return null;
                }
                else if (binding.Name == Keywords.EXPORT)
                {
                    ExpandExportedBindingList(args, context);
                    return null;
                }
                else if (binding.Name == Keywords.IMPORT)
                {
                    return ExpandModuleImport(args, context);
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

        private static Syntax EvalDefineSyntax(SyntaxPair stp, CompilationContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keywords.DEFINE_SYNTAX, context.Mode, stp);
            }

            if (!stp.TryDelist(out Identifier? key, out Syntax? value))
            {
                throw new ExpanderException.InvalidArguments(stp);
            }

            context.SanitizeBindingKey(key);

            Term evaluatedValue = Accelerate(value, context);

            if (evaluatedValue is not CompoundProcedure cp
                || !cp.TryCoerceMacro(out MacroProcedure? macro))
            {
                throw new ExpanderException.InvalidTransformer(evaluatedValue, value);
            }

            RenameMacroBinding(key, context);

            Syntax macroStx = Syntax.WrapWithRef(macro, value);
            Syntax constMacro = WrapImplicit(Symbols.S_Const, macroStx);
            return Syntax.WrapWithRef(Cons.ProperList(Symbols.S_TopDefine, key, constMacro), stp);
        }

        private static Term Accelerate(Syntax stx, CompilationContext context)
        {
            try
            {
                CompilationContext nextPhaseCtx = context.InNextPhase();

                Syntax expandedStx = Expand(stx, nextPhaseCtx);
                CoreForm parsedStx = Parser.ParseSyntax(expandedStx, nextPhaseCtx.Phase);
                Term evaluatedStx = Interpreter.InterpretProgram(parsedStx, context.CompileTimeEnv);

                return evaluatedStx;
            }
            catch (Exception ex)
            {
                throw new ExpanderException.EvaluationError(stx, ex);
            }
        }

        private static Syntax ExpandSyntaxTransformation(RenameBinding binding, Syntax input, CompilationContext context)
        {
            if (context.TryLookupMacro(binding, out MacroProcedure? macro))
            {
                Scope intro = new Scope($"{binding.Name} Macro Intro", input.Location);
                Scope useSite = new Scope($"{binding.Name} Macro Use-Site", input.Location);

                input.AddScope(context.Phase, intro, useSite);

                Syntax output = ApplySyntaxTransformer(macro, input, context);

                CompilationContext macroContext = context.InTransformed(useSite);
                output.FlipScope(macroContext.Phase, useSite);
                //context.AddPendingInsideEdge(output);

                return Expand(output, macroContext);
            }
            else
            {
                throw new ExpanderException.UnboundMacro(binding.Id);
            }
        }

        private static Syntax ApplySyntaxTransformer(MacroProcedure macro, Syntax input, CompilationContext context)
        {
            Term output;

            try
            {
                Application program = Application.ForMacro(macro, input);
                output = Interpreter.InterpretProgram(program, context.CompileTimeEnv);
            }
            catch (Exception ex)
            {
                throw new ExpanderException.EvaluationError(input, ex);
            }

            if (output is not Syntax outputStx)
            {
                throw new ExpanderException.InvalidTransformation(output, macro, input);
            }

            return outputStx;
        }

        private static void EvalBeginForSyntax(SyntaxPair stp, CompilationContext context)
        {
            if (context.Mode == ExpansionMode.Expression)
            {
                throw new ExpanderException.InvalidContext(Keywords.DEFINE_SYNTAX, context.Mode, stp);
            }

            if (Nil.Is(stp))
            {
                throw new ExpanderException.InvalidArguments(stp);
            }

            Syntax implicitBegin = Syntax.WrapWithRef(Cons.Truct(Symbols.S_Begin, stp), stp);
            Accelerate(implicitBegin, context);
        }

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
            else
            {
                return Expand(stx, context);
            }

            //throw new ExpanderException.ExpectedProperList(nameof(Syntax), stx);
        }

        /// <summary>
        /// Partially expand a sequence of terms to capture any requisite bindings,
        /// then properly run through and expand each term in sequence.
        /// </summary>
        private static Syntax ExpandBody(Syntax stx, CompilationContext context)
        {
            IEnumerable<Syntax> bodyItems = FlattenNestedSequences(stx, context).ToArray();
            Syntax unrolledBody = Syntax.WrapWithRef(Cons.ProperList(bodyItems), stx);

            Syntax partiallyExpandedBody = PartiallyExpandSequence(unrolledBody, context);

            if (context.Mode == ExpansionMode.TopLevel || context.Mode == ExpansionMode.Module
                && context.ImportedScopes.Any())
            {
                stx.AddScope(context.Phase, context.ImportedScopes.ToArray());
            }

            return ExpandSequence(partiallyExpandedBody, context.AsPartial());
        }

        /// <summary>
        /// Without performing expansion, flatten a sequence by checking for nested sequences and splicing them together.
        /// </summary>
        private static IEnumerable<Syntax> FlattenNestedSequences(Syntax stx, CompilationContext context)
        {
            Stack<Syntax> pending = new Stack<Syntax>();
            pending.Push(stx);
            
            while(pending.Count > 0)
            {
                Syntax nextSequence = pending.Pop();

                if (nextSequence.TryUnpair(out Syntax? nextItem, out Syntax? remaining))
                {
                    pending.Push(remaining);

                    if (nextItem.TryUnpair(out Identifier? op, out Syntax? args)
                    && op.TryResolveBinding(context.Phase, out RenameBinding? binding)
                    && binding.BoundType == BindingType.Special
                    && (binding.Name == Keywords.BEGIN
                        || binding.Name == Keywords.S_TOP_BEGIN
                        || binding.Name == Keywords.S_BEGIN))
                    {
                        pending.Push(args);
                    }
                    else
                    {
                        yield return nextItem;
                    }

                }
                else if (!Nil.Is(nextSequence))
                {
                    throw new ExpanderException.ExpectedProperList(stx);
                }
            }
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

        private static Syntax ExpandSyntacticIdentifiers(Syntax stx, CompilationContext context)
        {
            if (Nil.Is(stx))
            {
                return stx;
            }
            else if (stx is Identifier id
                && id.TryResolveBinding(context.Phase, out RenameBinding? binding))
            {
                return binding.Id;
            }
            else if (stx.TryUnpair(out Syntax? car, out Syntax? cdr))
            {
                Syntax expandedCar = ExpandSyntacticIdentifiers(car, context);
                Syntax expandedCdr = ExpandSyntacticIdentifiers(cdr, context);
                return Syntax.WrapWithRef(Cons.Truct(expandedCar, expandedCdr), stx);
            }
            else
            {
                return stx;
            }
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
                //RenameVariableBinding(nextExport, context);
                context.ExportIdentifier(nextExport);

                target = tail;
            }

            if (!Nil.Is(target))
            {
                throw new ParserException.ExpectedProperList(stx);
            }
        }

        #endregion

        #region Syntactic Special Forms

        private static Syntax ExpandQuote(SyntaxPair stp, CompilationContext context)
        {
            if (stp.TryDelist(out Syntax? stx))
            {
                return WrapImplicit(Symbols.S_Const, stx);
            }
            throw new ExpanderException.InvalidArguments(stp);
        }

        private static Syntax ExpandQuoteSyntax(SyntaxPair stp, CompilationContext context)
        {
            if (stp.TryDelist(out Syntax? stx))
            {
                return WrapImplicit(Symbols.S_Const_Syntax, stx);
            }
            throw new ExpanderException.InvalidArguments(stp);
        }

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
                else if (context.Mode == ExpansionMode.TopLevel || context.Mode == ExpansionMode.Module)
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

        private static Syntax ExpandBegin(SyntaxPair stp, CompilationContext context)
        {
            if (Nil.Is(stp))
            {
                throw new ExpanderException.InvalidArguments(stp);
            }
            else
            {
                Syntax expandedSequence = ExpandSequence(stp, context);
                return Syntax.WrapWithRef(Cons.Truct(Symbols.S_Begin, expandedSequence), stp);
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
                Cons.ProperList(context.ExportedIdentifiers),
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

        private static Syntax ExpandSyntaxForm(SyntaxPair stp, CompilationContext context)
        {
            if (stp.TryDelist(out Syntax? value))
            {
                return ExpandSyntacticIdentifiers(value, context);
            }
            else
            {
                throw new ExpanderException.InvalidArguments(stp);
            }
        }

        private static Syntax ExpandApplyForm(SyntaxPair stp, CompilationContext context)
        {
            if (stp.TryUnpair(out Syntax? op, out Syntax? rest))
            {
                if (Nil.Is(rest))
                {
                    return ExpandApplication(stp, context);
                }
                else if (stp.TryDelist(out Syntax? args))
                {
                    return ExpandApplication(new SyntaxPair(op, args, stp.Location), context);
                }
            }

            throw new ExpanderException.InvalidArguments(stp);
        }

        #endregion
    }
}
