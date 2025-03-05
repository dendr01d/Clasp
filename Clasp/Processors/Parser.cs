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
            }
            catch (ClaspException cex)
            {
                throw new ParserException.InvalidSyntax(stx, cex);
            }

            throw new ParserException.ExpectedCoreKeywordForm(stx);
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
            stp.Expose(out Syntax op, out Syntax args);

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

            CoreForm[] parsedArgs = ParseArguments(args, phase).ToArray();

            return new Application(parsedOp, parsedArgs);
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

            while (stx is SyntaxPair stp)
            {
                stp.Expose(out Syntax nextForm, out stx);

                CoreForm outForm = ParseSyntax(nextForm, phase);
                forms.Add(outForm);
            }

            if (stx != Datum.NilDatum)
            {
                throw new ParserException.ExpectedProperList(args);
            }

            return new TopBegin(forms);
        }

        private static TopDefine ParseTopDefine(Syntax args, int phase)
        {
            if (args is not SyntaxPair stp
                || !stp.TryMatchOnly(out Identifier? key, out Syntax? value))
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
            Syntax stx = args;
            List<CoreForm> forms = new List<CoreForm>();

            while (stx is SyntaxPair stp)
            {
                stp.Expose(out Syntax nextForm, out stx);

                CoreForm outForm = ParseSyntax(nextForm, phase);
                forms.Add(outForm);
            }

            if (stx != Datum.NilDatum)
            {
                throw new ParserException.ExpectedProperList(args);
            }

            return new Sequential(forms);
        }

        private static Importation ParseImport(Syntax args, int phase)
        {
            Syntax stx = args;
            List<Symbol> mdlSymbols = new List<Symbol>();

            while (stx is SyntaxPair stp)
            {
                stp.Expose(out Syntax nextForm, out stx);

                if (nextForm is not Identifier outForm)
                {
                    throw new ParserException.InvalidArguments(Keywords.S_IMPORT, args);
                }

                if (!outForm.TryResolveBinding(phase, BindingType.Module, out RenameBinding? binding))
                {
                    throw new ParserException.UnboundIdentifier(outForm, BindingType.Module);
                }

                Module.Visit(binding.Name);
                mdlSymbols.Add(binding.BindingSymbol);
            }

            if (stx != Datum.NilDatum)
            {
                throw new ParserException.ExpectedProperList(args);
            }

            return new Importation(mdlSymbols);
        }

        private static Mutation ParseSet(Syntax args, int phase)
        {
            if (args is not SyntaxPair stp
                || !stp.TryMatchOnly(out Identifier? key, out Syntax? value))
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
            if (args is not SyntaxPair stp
                || !stp.TryMatchOnly(out Syntax? arg1, out Syntax? arg2, out Syntax? arg3))
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

            while (stx is SyntaxPair stp)
            {
                stp.Expose(out Syntax nextForm, out stx);

                CoreForm outForm = ParseSyntax(nextForm, phase);
                forms.Add(outForm);
            }

            if (stx != Datum.NilDatum)
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
            if (args is not SyntaxPair stp
                || !stp.TryMatchLeading(out Syntax? p1, out Syntax? pv, out Syntax? p2, out Term? rest)
                || rest is not SyntaxPair body)
            {
                throw new ParserException.InvalidArguments(Keywords.S_LAMBDA, args);
            }

            IEnumerable<Symbol> formals = ParseParameterList(p1, phase);
            Symbol? variad = ParseMaybeParameter(pv, phase);
            IEnumerable<Symbol> informals = ParseParameterList(p2, phase);
            Sequential parsedBody = ParseBegin(body, phase);

            return new Procedural(formals, variad, informals, parsedBody);
        }

        private static VariableReference ParseVar(Syntax args, int phase)
        {
            if (args is not Identifier id)
            {
                throw new ParserException.InvalidArguments(Keywords.S_VAR, args);
            }

            if (!id.TryResolveBinding(phase, BindingType.Variable, out RenameBinding? binding))
            {
                throw new ParserException.UnboundIdentifier(id);
            }

            return new VariableReference(binding.BindingSymbol, false);
        }

        private static ConstValue ParseConst(Syntax args, int phase)
        {
            return new ConstValue(args.StripScopes(1));
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
        private static IEnumerable<CoreForm> ParseArguments(Syntax stx, int phase)
        {
            while (stx is SyntaxPair stp)
            {
                stp.Expose(out Syntax nextArg, out stx);

                CoreForm outArg = ParseSyntax(nextArg, phase);

                if (outArg.IsImperative)
                {
                    throw new ParserException.ExpectedExpression(outArg, nextArg.Location);
                }

                yield return outArg;
            }

            if (stx != Datum.NilDatum)
            {
                throw new ParserException.ExpectedProperList(stx);
            }

            yield break;
        }

        private static IEnumerable<Symbol> ParseParameterList(Syntax stx, int phase)
        {
            Syntax target = stx;

            while (target is SyntaxPair stp)
            {
                stp.Expose(out Syntax? nextParam, out target);

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
            }

            if (target != Datum.NilDatum)
            {
                throw new ParserException.ExpectedProperList(stx);
            }
            yield break;
        }

        private static Symbol? ParseMaybeParameter(Syntax stx, int phase)
        {
            if (stx is Identifier id)
            {
                if (id.TryResolveBinding(phase, BindingType.Variable, out RenameBinding? binding))
                {
                    return binding.BindingSymbol;
                }
                throw new ParserException.UnboundIdentifier(id);
            }
            else if (stx == Datum.NilDatum)
            {
                return null;
            }
            else
            {
                throw new ParserException.InvalidArguments("Variadic Parameter", stx);
            }
        }

        #endregion

    }
}
