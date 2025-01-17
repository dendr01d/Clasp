using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.ConcreteSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Process
{
    internal static class Parser
    {
        // the MOST IMPORTANT thing to remember here is that every syntactic form must break down
        // into ONLY the forms representable by AstNodes

        public static CoreForm Parse(Syntax stx, BindingStore bs, int phase)
        {
            if (stx.TryExposeIdentifier(out Symbol? sym, out string? _))
            {
                string bindingName = bs.ResolveBindingName(sym.Name, stx.GetContext(phase), stx);
                return new VariableLookup(bindingName);
            }
            else if (stx.TryExposeList(out Syntax? car, out Syntax? cdr))
            {
                return ParseApplication(car, cdr, stx, bs, phase);
            }
            else
            {
                return new ConstValue(stx.Strip());
            }
        }

        private static CoreForm ParseApplication(Syntax car, Syntax cdr, Syntax input, BindingStore bs, int phase)
        {
            // Parse the op-term first, then decide what to do
            CoreForm opTerm = Parse(car, bs, phase);

            // Check to see if it's a special form
            if (opTerm is VariableLookup vl)
            {
                string keyword = vl.VarName;

                if (keyword == Symbol.Quote.Name)
                {
                    return ParseQuote(cdr, input);
                }
                else if (keyword == Symbol.QuoteSyntax.Name)
                {
                    return ParseQuoteSyntax(cdr, input);
                }
                else if (keyword == Symbol.Define.Name)
                {
                    return ParseDefinition(cdr, input, bs, phase);
                }
                else if (keyword == Symbol.Set.Name)
                {
                    return ParseSet(cdr, input, bs, phase);
                }
                else if (keyword == Symbol.Lambda.Name)
                {
                    return ParseLambda(cdr, input, bs, phase);
                }
                else if (keyword == Symbol.If.Name)
                {
                    return ParseBranch(cdr, input, bs, phase);
                }
                else if (keyword == Symbol.Begin.Name)
                {
                    return ParseBeginOrLambdaBody(cdr, input, bs, phase);
                }
            }

            // Check to make sure it's not an imperative command
            if (opTerm is BindingDefinition || opTerm is BindingMutation)
            {
                throw new ParserException.InvalidOperator(opTerm.GetType().ToString(), input);
            }

            // Otherwise we just have to trust that it'll make sense in the final program
            IEnumerable<CoreForm> argTerms = ParseList(cdr, bs, phase);
            return new FunctionApplication(opTerm, argTerms.ToArray());
        }

        #region Special Forms

        private static ConstValue ParseQuote(Syntax args, Syntax full)
        {
            if (TryExposeOneArg(args, out Syntax? quotedValue))
            {
                Term strippedExpr = quotedValue.Strip();
                return new ConstValue(strippedExpr);
            }

            throw new ParserException.WrongArity(Symbol.Quote.Name, "exactly one", full);
        }

        private static ConstValue ParseQuoteSyntax(Syntax args, Syntax full)
        {
            if (TryExposeOneArg(args, out Syntax? syntacticValue))
            {
                return new ConstValue(syntacticValue);
            }

            throw new ParserException.WrongArity(Symbol.QuoteSyntax.Name, "exactly one", full);
        }

        private static BindingDefinition ParseDefinition(Syntax args, Syntax full, BindingStore bs, int phase)
        {
            if (TryExposeTwoArgs(args, out Syntax? arg1, out Syntax? arg2))
            {
                if (TryExposeBindingId(arg1, bs, phase, out string? key))
                {
                    CoreForm boundValueExpr = Parse(arg2, bs, phase);
                    return new BindingDefinition(key, boundValueExpr);
                }

                throw new ParserException.WrongType(Symbol.Define.Name, "identifier", full);
            }

            throw new ParserException.WrongArity(Symbol.Define.Name, "exactly two", full);
        }

        private static BindingMutation ParseSet(Syntax args, Syntax full, BindingStore bs, int phase)
        {
            if (TryExposeTwoArgs(args, out Syntax? arg1, out Syntax? arg2))
            {
                if (TryExposeBindingId(arg1, bs, phase, out string? key))
                {
                    CoreForm boundValueExpr = Parse(arg2, bs, phase);
                    return new BindingMutation(key, boundValueExpr);
                }

                throw new ParserException.WrongType(Symbol.Define.Name, "identifier", full);
            }

            throw new ParserException.WrongArity(Symbol.Define.Name, "exactly two", full);
        }

        private static string[] AnalyzeInternalDefinitions(SequentialForm seq)
        {
            List<string> internalKeys = new List<string>();

            foreach(CoreForm node in seq.Sequence)
            {
                if (node is BindingDefinition bd)
                {
                    internalKeys.Add(bd.VarName);
                }
            }

            return internalKeys.ToArray();
        }

        private static FunctionCreation ParseLambda(Syntax args, Syntax full, BindingStore bs, int phase)
        {
            if (args.TryExposeList(out Syntax? paramStx, out Syntax? bodyStx))
            {
                Tuple<string[], string?> parameters = ParseParams(paramStx, bs, phase);
                SequentialForm body = ParseBeginOrLambdaBody(bodyStx, full, bs, phase);

                string[] internalKeys = AnalyzeInternalDefinitions(body);

                return new FunctionCreation(parameters.Item1, parameters.Item2, internalKeys, body);
            }

            throw new ParserException.WrongArity(Symbol.Lambda.Name, "two or more", full);
        }

        private static ConditionalForm ParseBranch(Syntax args, Syntax full, BindingStore bs, int phase)
        {
            if (TryExposeThreeArgs(args, out Syntax? arg1, out Syntax? arg2, out Syntax? arg3))
            {
                CoreForm test = Parse(arg1, bs, phase);
                CoreForm consequent = Parse(arg2, bs, phase);
                CoreForm alternate = Parse(arg3, bs, phase);

                return new ConditionalForm(test, consequent, alternate);
            }

            throw new ParserException.WrongArity(Symbol.Lambda.Name, "exactly three", full);
        }

        private static SequentialForm ParseBeginOrLambdaBody(Syntax args, Syntax full, BindingStore bs, int phase)
        {
            CoreForm[] series = ParseList(args, bs, phase).ToArray();

            if (series.Length == 0)
            {
                throw new ParserException.WrongArity("Begin/Body", "at least one", full);
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

            if (current.Expose() is not Nil)
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
            if (stx.TryExposeIdentifier(out Symbol? _, out string? symbolicName))
            {
                bindingName = bs.ResolveBindingName(symbolicName, stx.GetContext(phase), stx);
                return true;
            }
            bindingName = null;
            return false;
        }

        private static bool TryExposeOneArg(Syntax stx,
            [NotNullWhen(true)] out Syntax? arg1)
        {
            return stx.TryExposeList(out arg1, out Syntax? terminator)
                && terminator.Expose() is Nil;
        }

        private static bool TryExposeTwoArgs(Syntax stx,
            [NotNullWhen(true)] out Syntax? arg1,
            [NotNullWhen(true)] out Syntax? arg2)
        {
            arg2 = null;

            return stx.TryExposeList(out arg1, out Syntax? rest)
                && rest.TryExposeList(out arg2, out Syntax? terminator)
                && terminator.Expose() is Nil;
        }

        private static bool TryExposeThreeArgs(Syntax stx,
            [NotNullWhen(true)] out Syntax? arg1,
            [NotNullWhen(true)] out Syntax? arg2,
            [NotNullWhen(true)] out Syntax? arg3)
        {
            arg2 = null;
            arg3 = null;

            return stx.TryExposeList(out arg1, out Syntax? rest1)
                && rest1.TryExposeList(out arg2, out Syntax? rest2)
                && rest2.TryExposeList(out arg3, out Syntax? terminator)
                && terminator.Expose() is Nil;
        }



        #endregion
    }
}
