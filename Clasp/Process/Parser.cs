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

        public static CoreForm ParseSyntax(Syntax stx, BindingStore bs, int phase)
        {
            return Parse(stx, bs, phase, true);
        }

        private static CoreForm Parse(Syntax stx, BindingStore bs, int phase, bool topLevel = false)
        {
            if (stx.TryExposeIdentifier(out string? idName))
            {
                string bindingName = bs.ResolveBindingName(stx, idName, phase);
                return new VariableLookup(bindingName);
            }
            else if (stx.TryExposeList(out Syntax? stxOp, out Syntax? stxArgs))
            {
                return ParseApplication(stx, stxOp, stxArgs, bs, phase, topLevel);
            }
            else
            {
                //is that right...?
                // -> it has to be an id, a list, or constant data...
                return new ConstValue(Syntax.ToDatum(stx));
            }
        }

        private static CoreForm ParseTerm(Term maybeSyntax, SourceLocation loc, BindingStore bs, int phase)
        {
            if (maybeSyntax is Syntax stx)
            {
                return Parse(stx, bs, phase);
            }

            throw new ParserException.NotSyntax(maybeSyntax, loc);
        }

        private static CoreForm ParseApplication(Syntax stx, Syntax stxOp, Syntax stxArgs,
            BindingStore bs, int phase, bool topLevel)
        {
            // Parse the op-term first, then decide what to do
            CoreForm opTerm = Parse(stxOp, bs, phase);

            // Check to see if it's a special form
            if (opTerm is VariableLookup vl)
            {
                string keyword = vl.VarName;

                if (keyword == Symbol.Quote.Name)
                {
                    return ParseQuote(stx, stxArgs);
                }
                else if (keyword == Symbol.QuoteSyntax.Name)
                {
                    return ParseQuoteSyntax(stx, stxArgs);
                }
                else if (keyword == Symbol.Define.Name)
                {
                    return ParseDefinition(stx, stxArgs, bs, phase, topLevel);
                }
                else if (keyword == Symbol.Set.Name)
                {
                    return ParseSet(stx, stxArgs, bs, phase);
                }
                else if (keyword == Symbol.Lambda.Name)
                {
                    return ParseLambda(stx, stxArgs, bs, phase);
                }
                else if (keyword == Symbol.If.Name)
                {
                    return ParseConditional(stx, stxArgs, bs, phase);
                }
                else if (keyword == Symbol.Begin.Name)
                {
                    return ParseSequence(stx, stxArgs, bs, phase);
                }
            }

            // Check to make sure it's not an imperative command
            if (opTerm is BindingDefinition || opTerm is BindingMutation)
            {
                throw new ParserException.InvalidOperator(opTerm.GetType().Name.ToString(), stx);
            }

            // Otherwise we just have to trust that it'll make sense in the final program
            IEnumerable<CoreForm> argTerms = ParseList(stxArgs, bs, phase);
            return new FunctionApplication(opTerm, argTerms.ToArray());
        }

        #region Special Forms

        private static ConstValue ParseQuote(Syntax stx, Syntax stxArgs)
        {
            if (TryExposeOneArg(stxArgs, out Syntax? quotedValue))
            {
                Term strippedExpr = Syntax.ToDatum(quotedValue);
                return new ConstValue(strippedExpr);
            }

            throw new ParserException.WrongArity(Symbol.Quote.Name, "exactly one", stx);
        }

        private static ConstValue ParseQuoteSyntax(Syntax stx, Syntax stxArgs)
        {
            if (TryExposeOneArg(stxArgs, out Syntax? syntacticValue))
            {
                return new ConstValue(syntacticValue);
            }

            throw new ParserException.WrongArity(Symbol.QuoteSyntax.Name, "exactly one", stx);
        }

        private static BindingDefinition ParseDefinition(Syntax stx, Syntax stxArgs,
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

        private static BindingMutation ParseSet(Syntax stx, Syntax stxArgs, BindingStore bs, int phase)
        {
            if (TryExposeTwoArgs(stxArgs, out Syntax? arg1, out Syntax? arg2))
            {
                if (TryExposeBindingId(arg1, bs, phase, out string? key))
                {
                    CoreForm boundValueExpr = Parse(arg2, bs, phase);
                    return new BindingMutation(key, boundValueExpr);
                }

                throw new ParserException.WrongType(Symbol.Define.Name, "identifier", stx);
            }

            throw new ParserException.WrongArity(Symbol.Define.Name, "exactly two", stx);
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

            if (current.Exposee is not Nil)
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

        private static bool TryExposeOneArg(Syntax stx,
            [NotNullWhen(true)] out Syntax? arg1)
        {
            return stx.TryExposeList(out arg1, out Syntax? terminator)
                && terminator.Exposee is Nil;
        }

        private static bool TryExposeTwoArgs(Syntax stx,
            [NotNullWhen(true)] out Syntax? arg1,
            [NotNullWhen(true)] out Syntax? arg2)
        {
            arg2 = null;

            return stx.TryExposeList(out arg1, out Syntax? rest)
                && TryExposeOneArg(rest, out arg2);
        }

        private static bool TryExposeThreeArgs(Syntax stx,
            [NotNullWhen(true)] out Syntax? arg1,
            [NotNullWhen(true)] out Syntax? arg2,
            [NotNullWhen(true)] out Syntax? arg3)
        {
            arg2 = null;
            arg3 = null;

            return stx.TryExposeList(out arg1, out Syntax? rest)
                && TryExposeTwoArgs(stx, out arg2, out arg3);
        }



        #endregion
    }
}
