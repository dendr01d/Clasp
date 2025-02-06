using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.ConcreteSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Syntax;
using Clasp.ExtensionMethods;

namespace Clasp.Process
{
    internal static class Parser
    {
        // the MOST IMPORTANT thing to remember here is that every syntactic form must break down
        // into ONLY core forms

        public static CoreForm ParseSyntax(Syntax stx, ExpansionContext exResult)
        {
            return Parse(stx, exResult);
        }

        private static CoreForm Parse(Syntax stx, ExpansionContext exResult)
        {
            if (stx is Identifier id)
            {
                CompileTimeBinding binding = exResult.ResolveBinding(id);
                return new VariableLookup(binding.BindingName);
            }
            else if (stx is SyntaxPair stp)
            {
                return ParseApplication(stp, exResult);
            }
            else
            {
                return new ConstValue(stx.ToDatum());
            }
        }

        private static CoreForm ParseApplication(SyntaxPair stp, ExpansionContext exResult)
        {
            if (stp.Car is Identifier idOp
                && exResult.TryResolveBinding(idOp, out CompileTimeBinding? binding))
            {
                if (binding.BoundType == BindingType.Transformer
                    && exResult.TryGetMacro(binding.BindingName, out MacroProcedure? macro))
                {
                    return new ConstValue(macro);
                }
                else if (binding.BoundType == BindingType.Special)
                {
                    return ParseSpecial(idOp, stp, exResult);
                }
            }

            CoreForm parsedOp = Parse(stp.Car, exResult);

            // check to make sure it's not an imperative form
            if (parsedOp.IsImperative)
            {
                throw new ParserException.InvalidOperator(parsedOp, stp);
            }

            // otherwise we just have to trust that it'll make sense in the final program
            IEnumerable<CoreForm> argTerms = ParseList(stp.Cdr, exResult);
            return new FunctionApplication(parsedOp, argTerms.ToArray());
        }

        private static CoreForm ParseSpecial(Identifier idOp, SyntaxPair form, ExpansionContext exResult)
        {
            // all special forms have at least one argument
            if (form.Cdr is not SyntaxPair args)
            {
                throw new ParserException.InvalidFormInput(idOp.Name, "arguments", form);
            }

            CoreForm result = idOp.Name switch
            {
                Keyword.IMP_TOP => ParseIdentifier(args, exResult),
                Keyword.IMP_VAR => ParseIdentifier(args, exResult),

                Keyword.QUOTE => ParseQuote(args, exResult),
                Keyword.IMP_DATUM => ParseQuote(args, exResult),
                
                Keyword.QUOTE_SYNTAX => ParseQuoteSyntax(args, exResult),

                Keyword.IMP_APP => ParseApplication(args, exResult),

                Keyword.DEFINE => ParseDefinition(args, exResult),
                Keyword.DEFINE_SYNTAX => ParseDefinition(args, exResult),
                Keyword.IMP_PARDEF => ParseDefinition(args, exResult),
                Keyword.SET => ParseSet(args, exResult),

                Keyword.LAMBDA => ParseLambda(args, exResult),
                Keyword.IMP_LAMBDA => ParseLambda(args, exResult),

                Keyword.IF => ParseConditional(args, exResult),

                Keyword.BEGIN => ParseSequence(args, exResult),
                Keyword.IMP_SEQ => ParseSequence(args, exResult),

                _ => throw new ParserException.InvalidSyntax(form)
            };

            return result;
        }

        #region Special Forms

        private static ConstValue ParseQuote(Syntax stx, Syntax stxArgs)
        {
            if (stx.TryExposeOneArg(out Syntax? arg))
            {
                return new ConstValue(arg.ToDatum());
            }

            throw new ParserException.WrongArity(Symbol.Quote.Name, "exactly one", stx);
        }

        private static ConstValue ParseQuoteSyntax(Syntax stx, Syntax stxArgs)
        {
            if (stx.TryExposeOneArg(out Syntax? arg))
            {
                return new ConstValue(arg);
            }

            throw new ParserException.WrongArity(Symbol.QuoteSyntax.Name, "exactly one", stx);
        }

        private static TopLevelDefine ParseDefinition(Syntax stx, Syntax stxArgs,
            BindingStore bs, int phase, bool topLevel)
        {
            throw new NotImplementedException(); //TODO: top level definitions?

            //if (TryExposeTwoArgs(args, out Syntax? arg1, out Syntax? arg2))
            //{
            //    if (TryExposeBindingId(arg1, bs, phase, out string? key))
            //    {
            //        CoreForm boundValueExpr = Parse(arg2, bs, phase);
            //        return new BindingDefinition(key, boundValueExpr);
            //    }

            //    throw new ParserException.WrongType(Symbol.Define.Name, "identifier", full);
            //}

            //throw new ParserException.WrongArity(Symbol.Define.Name, "exactly two", full);
        }

        private static BindingMutation ParseSet(Syntax stx, ExpansionContext exResult)
        {
            if (stx.TryExposeTwoArgs(out Syntax? key, out Syntax? value))
            {
                if (key is Identifier id
                    && exResult.TryResolveBinding(id, out CompileTimeBinding? binding))
                {
                    CoreForm boundValue = Parse(value, exResult);

                    if (boundValue.IsImperative) throw new ParserException.ExpectedExpression(Keyword.SET, boundValue, stx);

                    return new BindingMutation(binding.BindingName, boundValue);
                }

                throw new ParserException.WrongType(Keyword.SET, nameof(Identifier), stx);
            }

            throw new ParserException.WrongArity(Symbol.Define.Name, "exactly two", stx);
        }

        private static string[] AnalyzeInternalDefinitions(SequentialForm seq)
        {
            List<string> internalKeys = new List<string>();

            foreach(CoreForm node in seq.Sequence)
            {
                if (node is TopLevelDefine bd)
                {
                    internalKeys.Add(bd.VarName);
                }
            }

            return internalKeys.ToArray();
        }

        private static FunctionCreation ParseLambda(Syntax stx, Syntax stxArgs, BindingStore bs, int phase)
        {
            if (stxArgs.TryExposeList(out Syntax? stxParams, out Syntax? stxBody))
            {
                Tuple<string[], string?> parameters = ParseParams(stxParams, bs, phase);
                SequentialForm body = ParseSequence(stxBody, stx, bs, phase);

                string[] internalKeys = AnalyzeInternalDefinitions(body);

                return new FunctionCreation(parameters.Item1, parameters.Item2, internalKeys, body);
            }

            throw new ParserException.WrongArity(Symbol.Lambda.Name, "two or more", stx);
        }

        private static ConditionalForm ParseConditional(Syntax stx, Syntax stxArgs, BindingStore bs, int phase)
        {
            if (TryExposeThreeArgs(stxArgs, out Syntax? arg1, out Syntax? arg2, out Syntax? arg3))
            {
                CoreForm test = Parse(arg1, bs, phase);
                CoreForm consequent = Parse(arg2, bs, phase);
                CoreForm alternate = Parse(arg3, bs, phase);

                return new ConditionalForm(test, consequent, alternate);
            }

            throw new ParserException.WrongArity(Symbol.Lambda.Name, "exactly three", stx);
        }

        private static SequentialForm ParseSequence(Syntax stx, Syntax stxArgs, BindingStore bs, int phase)
        {
            CoreForm[] series = ParseList(stxArgs, bs, phase).ToArray();

            if (series.Length == 0)
            {
                throw new ParserException.WrongArity("Begin/Body", "at least one", stx);
            }
            else
            {
                return new SequentialForm(series);
            }
        }

        #endregion

        #region Auxiliary Structures

        private static IEnumerable<CoreForm> ParseList(Syntax argList, BindingStore bs, int phase)
        {
            Syntax current = argList;

            while (current.TryExposeList(out Syntax? first, out Syntax? rest))
            {
                CoreForm newArg = Parse(first, bs, phase);
                yield return newArg;
                current = rest;
            }

            if (current._wrapped is not Nil)
            {
                throw new ParserException.InvalidSyntax(argList);
            }

            yield break;
        }

        private static Tuple<string[], string?> ParseParams(Syntax paramList, BindingStore bs, int phase)
        {
            List<string> regularParams = new List<string>();

            Syntax current = paramList;

            while (current.TryExposeList(out Syntax? first, out Syntax? rest))
            {
                if (TryExposeBindingId(first, bs, phase, out string? paramName))
                {
                    regularParams.Add(paramName);
                }
                else
                {
                    throw new ParserException.InvalidFormInput(Symbol.Lambda.Name, "parameter list", paramList);
                }

                current = rest;
            }

            if (TryExposeBindingId(current, bs, phase, out string? dottedParam))
            {
                return new Tuple<string[], string?>(regularParams.ToArray(), dottedParam);
            }
            else
            {
                return new Tuple<string[], string?>(regularParams.ToArray(), null);
            }
        }

        #endregion

        #region Helpers

        private static bool TryExposeBindingId(Syntax stx, BindingStore bs, int phase,
            [NotNullWhen(true)] out string? bindingName)
        {
            if (stx.TryExposeIdentifier(out string? idName))
            {
                bindingName = bs.ResolveBindingName(stx, idName, phase);
                return true;
            }
            bindingName = null;
            return false;
        }

        #endregion
    }
}
