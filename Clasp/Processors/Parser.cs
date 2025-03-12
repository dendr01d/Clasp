using System.Collections.Generic;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Modules;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Exceptions;
using Clasp.ExtensionMethods;

namespace Clasp.Process
{
    internal static class Parser
    {
        public static CoreForm ParseSyntax(Syntax stx, int phase)
        {
            try
            {
                if (stx is SyntaxPair stl)
                {
                    return ParseApplication(stl, phase);
                }
                else if (stx is Identifier id)
                {
                    return ParseVar(id, phase);
                }
                else
                {
                    return ParseConst(stx, phase);
                }
            }
            catch (ClaspException cex)
            {
                throw new ParserException.InvalidSyntax(stx, cex);
            }
        }

        //private static CoreForm ParseIdentifier(Identifier id, int phase)
        //{
        //    RenameBinding? binding;

        //    if (id.TryResolveBinding(phase, BindingType.Variable, out binding)
        //        || id.TryResolveBinding(phase, BindingType.Module, out binding)
        //        || id.TryResolveBinding(phase, BindingType.Primitive, out binding))
        //    {
        //        return new VariableReference(binding.BindingSymbol, false);
        //    }

        //    throw new ParserException.UnboundIdentifier(id);
        //}

        private static CoreForm ParseApplication(SyntaxPair stp, int phase)
        {
            if (!stp.TryUnpair(out Syntax? op, out Syntax? args))
            {
                throw new ParserException.InvalidSyntax(stp);
            }

            if (op is Identifier id
                && id.TryResolveBinding(phase, BindingType.Special, out RenameBinding? binding))
            {
                return ParseSpecialArgs(binding.Name, args, phase);
            }

            CoreForm parsedOp = ParseSyntax(op, phase);

            if (parsedOp.IsImperative)
            {
                throw new ParserException.InvalidOperator(parsedOp, stp);
            }

            CoreForm[] parsedArgs = ParseArguments(args, phase, out CoreForm? varArg).ToArray();

            return new Application(parsedOp, parsedArgs, varArg);
        }

        private static CoreForm ParseSpecialArgs(string formName, Syntax args, int phase)
        {
            try
            {
                return formName switch
                {
                    Keywords.S_TOP_VAR => ParseTopVar(args, phase),
                    Keywords.S_TOP_BEGIN => ParseTopBegin(args, phase),
                    Keywords.S_TOP_DEFINE => ParseTopDefine(args, phase),
                    Keywords.S_MODULE_BEGIN => ParseModuleBegin(args, phase),
                    Keywords.S_IMPORT => ParseImport(args, phase),
                    //Keywords.S_IMPORT_FROM => ParseImportFrom(args, phase),
                    Keywords.S_SET => ParseSet(args, phase),
                    Keywords.S_IF => ParseIf(args, phase),
                    Keywords.S_BEGIN => ParseBegin(args, phase),
                    Keywords.S_APPLY => ParseApply(args, phase),
                    Keywords.S_LAMBDA => ParseLambda(args, phase),
                    Keywords.S_VAR => ParseVar(args, phase),
                    Keywords.S_CONST => ParseConst(args, phase),
                    Keywords.S_CONST_SYNTAX => ParseConstSyntax(args, phase),

                    _ => throw new ParserException.InvalidArguments(formName, args)
                };
            }
            catch (ParserException pe)
            {
                throw new ParserException.InvalidForm(formName, args, pe);
            }
        }

        #region Special Forms

        private static VariableReference ParseTopVar(Syntax args, int phase)
        {
            if (args is not Identifier id)
            {
                throw new ParserException.InvalidArguments(Keywords.S_TOP_VAR, args);
            }

            if (!id.TryResolveBinding(phase, BindingType.Variable, out RenameBinding? binding))
            {
                throw new ParserException.UnboundIdentifier(id);
            }

            return new VariableReference(binding.BindingSymbol, true);
        }

        private static TopBegin ParseTopBegin(Syntax args, int phase)
        {
            Syntax stx = args;
            List<CoreForm> forms = new List<CoreForm>();

            while (stx.TryUnpair(out Syntax? nextForm, out Syntax? tail))
            {
                CoreForm outForm = ParseSyntax(nextForm, phase);
                forms.Add(outForm);

                stx = tail;
            }

            if (!Nil.Is(stx))
            {
                throw new ParserException.ExpectedProperList(args);
            }

            return new TopBegin(forms);
        }

        private static TopDefine ParseTopDefine(Syntax args, int phase)
        {
            if (!args.TryDelist(out Identifier? key, out Syntax? value))
            {
                throw new ParserException.InvalidArguments(Keywords.S_TOP_DEFINE, args);
            }

            if (!key.TryResolveBinding(phase, BindingType.Variable, out RenameBinding? binding))
            {
                throw new ParserException.UnboundIdentifier(key);
            }

            CoreForm parsedValue = ParseSyntax(value, phase);

            return new TopDefine(binding.BindingSymbol, parsedValue);
        }

        private static Sequential ParseModuleBegin(Syntax args, int phase)
        {
            if (!args.TryUnpair(out Syntax? exports, out Syntax? body))
            {
                throw new ParserException.InvalidArguments(Keywords.S_MODULE_BEGIN, args);
            }

            List<CoreForm> forms = new List<CoreForm>();

            Syntax stx = exports;
            //while (stx.TryUnpair(out Identifier? nextExport, out Syntax? moreExports))
            //{
            //    if (nextExport.TryResolveBinding(phase, out RenameBinding? binding)
            //        && binding.BoundType != BindingType.Module)
            //    {
            //        forms.Add(new Undefine(binding.BindingSymbol));
            //    }
            //    else
            //    {
            //        throw new ParserException.UnboundIdentifier(nextExport);
            //    }
            //    stx = moreExports;
            //}

            //if (!Nil.Is(stx))
            //{
            //    throw new ParserException.ExpectedProperList(exports);
            //}

            stx = body;
            while (stx.TryUnpair(out Syntax? nextForm, out Syntax? tail))
            {
                CoreForm outForm = ParseSyntax(nextForm, phase);
                forms.Add(outForm);

                stx = tail;
            }

            if (!Nil.Is(stx))
            {
                throw new ParserException.ExpectedProperList(args);
            }

            return new Sequential(forms);
        }

        private static Importation ParseImport(Syntax args, int phase)
        {
            Syntax stx = args;
            List<Symbol> mdlSymbols = new List<Symbol>();

            while (stx.TryUnpair(out Identifier? nextImport, out Syntax? tail))
            {
                mdlSymbols.Add(nextImport.Expose());
                stx = tail;
            }

            if (!Nil.Is(stx))
            {
                throw new ParserException.ExpectedProperList(nameof(Identifier), args);
            }

            return new Importation(mdlSymbols);
        }

        private static Mutation ParseSet(Syntax args, int phase)
        {
            if (!args.TryDelist(out Identifier? key, out Syntax? value))
            {
                throw new ParserException.InvalidArguments(Keywords.S_SET, args);
            }

            if (!key.TryResolveBinding(phase, BindingType.Variable, out RenameBinding? binding))
            {
                throw new ParserException.UnboundIdentifier(key);
            }

            CoreForm parsedValue = ParseSyntax(value, phase);

            return new Mutation(binding.BindingSymbol, parsedValue);
        }

        private static Conditional ParseIf(Syntax args, int phase)
        {
            if (!args.TryDelist(out Syntax? arg1, out Syntax? arg2, out Syntax? arg3))
            {
                throw new ParserException.InvalidArguments(Keywords.S_IF, args);
            }

            CoreForm parsedIf = ParseSyntax(arg1, phase);
            CoreForm parsedThen = ParseSyntax(arg2, phase);
            CoreForm parsedElse = ParseSyntax(arg3, phase);

            return new Conditional(parsedIf, parsedThen, parsedElse);
        }

        private static Sequential ParseBegin(Syntax args, int phase)
        {
            Syntax stx = args;
            List<CoreForm> forms = new List<CoreForm>();

            while (stx.TryUnpair(out Syntax? nextForm, out Syntax? tail))
            {
                CoreForm outForm = ParseSyntax(nextForm, phase);
                forms.Add(outForm);

                stx = tail;
            }

            if (!Nil.Is(stx))
            {
                throw new ParserException.ExpectedProperList(args);
            }

            if (forms.Last().IsImperative)
            {
                throw new ParserException.InvalidArguments(Keywords.S_BEGIN, args);
            }

            return new Sequential(forms);
        }

        private static CoreForm ParseApply(Syntax args, int phase)
        {
            if (args is not SyntaxPair stp)
            {
                throw new ParserException.InvalidArguments(Keywords.S_APPLY, args);
            }

            return ParseApplication(stp, phase);
        }

        private static Procedural ParseLambda(Syntax args, int phase)
        {
            if (!args.TryUnpair(out Syntax? formals, out Syntax? variad, out Syntax? informals, out SyntaxPair? body))
            {
                throw new ParserException.InvalidArguments(Keywords.S_LAMBDA, args);
            }

            IEnumerable<Symbol> parsedFormals = ParseParameterList(formals, phase);
            Symbol? parsedVariad = ParseMaybeParameter(variad, phase);
            IEnumerable<Symbol> parsedInformals = ParseParameterList(informals, phase);
            Sequential parsedBody = ParseBegin(body, phase);

            return new Procedural(parsedFormals, parsedVariad, parsedInformals, parsedBody);
        }

        private static CoreForm ParseVar(Syntax args, int phase)
        {
            if (args is not Identifier id)
            {
                throw new ParserException.InvalidArguments(Keywords.S_VAR, args);
            }

            if (!id.TryResolveBinding(phase, out RenameBinding? binding))
            {
                throw new ParserException.UnboundIdentifier(id);
            }

            if (binding.BoundType == BindingType.Module)
            {
                return new ConstValue(new ModuleHandle(id.Name));
            }

            return new VariableReference(binding.BindingSymbol, false);
        }

        private static ConstValue ParseConst(Syntax args, int phase)
        {
            return new ConstValue(args.ExposeAll());
        }

        private static ConstValue ParseConstSyntax(Syntax args, int phase)
        {
            return new ConstValue(args.StripScopes(phase));
        }

        #endregion

        #region Auxiliary Structures

        /// <summary>
        /// Enumerate all the forms in a (proper) list of argument expressions.
        /// </summary>
        private static List<CoreForm> ParseArguments(Syntax stx, int phase, out CoreForm? varArg)
        {
            List<CoreForm> outArgs = new List<CoreForm>();
            Syntax target = stx;

            while (target.TryUnpair(out Syntax? nextArg, out Syntax? tail))
            {
                CoreForm outArg = ParseSyntax(nextArg, phase);

                if (outArg.IsImperative)
                {
                    throw new ParserException.ExpectedExpression(outArg, nextArg.Location);
                }

                outArgs.Add(outArg);
                
                target = tail;
            }

            if (!Nil.Is(target))
            {
                CoreForm outArg = ParseSyntax(target, phase);

                if (outArg.IsImperative)
                {
                    throw new ParserException.ExpectedExpression(outArg, target.Location);
                }

                varArg = outArg;
            }
            else
            {
                varArg = null;
            }

            return outArgs;
        }

        private static IEnumerable<Symbol> ParseParameterList(Syntax stx, int phase)
        {
            Syntax target = stx;

            while (target.TryUnpair(out Identifier? nextParam, out Syntax? tail))
            {
                if (nextParam is not Identifier outParam)
                {
                    throw new ParserException.InvalidArguments("Parameter List", stx);
                }
                else if (!outParam.TryResolveBinding(phase, BindingType.Variable, out RenameBinding? binding))
                {
                    throw new ParserException.UnboundIdentifier(outParam, BindingType.Variable);
                }
                else
                {
                    yield return binding.BindingSymbol;
                }

                target = tail;
            }

            if (!Nil.Is(target))
            {
                throw new ParserException.ExpectedProperList(stx);
            }

            yield break;
        }

        private static Symbol? ParseMaybeParameter(Syntax stx, int phase)
        {
            if (Nil.Is(stx))
            {
                return null;
            }
            else if (stx is Identifier id)
            {
                if (id.TryResolveBinding(phase, BindingType.Variable, out RenameBinding? binding))
                {
                    return binding.BindingSymbol;
                }
                throw new ParserException.UnboundIdentifier(id);
            }
            throw new ParserException.InvalidArguments("Variadic Parameter", stx);
        }

        #endregion

    }
}
