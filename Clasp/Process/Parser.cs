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

        public static AstNode ParseAST(SyntaxWrapper stx, int startingPhase)
        {
            return ParseAST(stx, new BindingStore(), startingPhase);
        }

        public static AstNode ParseAST(SyntaxWrapper stx, BindingStore bs, int phase)
        {
            if (stx.TryExposeIdentifier(out Symbol? sym, out string? _))
            {
                string bindingName = bs.ResolveBindingName(sym.Name, stx.Context[phase]);
                return new VariableLookup(bindingName);
            }
            else if (stx.TryExposeList(out SyntaxWrapper? car, out SyntaxWrapper? cdr))
            {
                return ParseApplication(car, cdr, bs, phase);
            }
            else
            {
                return ParseQuote(stx);
            }
        }

        private static AstNode ParseApplication(SyntaxWrapper car, SyntaxWrapper cdr, BindingStore bs, int phase)
        {
            // Parse the op-term first, then decide what to do
            AstNode opTerm = ParseAST(car, bs, phase);

            // Check to see if it's a special form
            if (opTerm is VariableLookup vl)
            {
                string keyword = vl.VarName;

                if (keyword == Symbol.Quote.Name)
                {
                    return ParseQuote(cdr);
                }
                else if (keyword == Symbol.Syntax.Name)
                {
                    return ParseSyntax(cdr);
                }
                else if (keyword == Symbol.Define.Name)
                {
                    return ParseDefinition(cdr, bs, phase);
                }
                else if (keyword == Symbol.Set.Name)
                {
                    return ParseSet(cdr, bs, phase);
                }
                else if (keyword == Symbol.Lambda.Name)
                {
                    return ParseLambda(cdr, bs, phase);
                }
                else if (keyword == Symbol.If.Name)
                {
                    return ParseBranch(cdr, bs, phase);
                }
                else if (keyword == Symbol.Begin.Name)
                {
                    return ParseBegin(cdr, bs, phase);
                }
            }

            // Check to make sure it's not an imperative command
            if (opTerm is BindingDefinition || opTerm is BindingMutation)
            {
                throw new ParserException.Uncategorized("Can't use imperative command as op term in function application");
            }

            // Otherwise we just have to trust that it'll make sense in the final program
            IEnumerable<AstNode> argTerms = ParseArgs(cdr, bs, phase);
            return new FunctionApplication(opTerm, argTerms.ToArray());
        }

        #region Special Forms

        private static ConstValue ParseQuote(SyntaxWrapper args)
        {
            if (TryExposeOneArg(args, out SyntaxWrapper? quotedValue))
            {
                Term strippedExpr = quotedValue.Strip();
                return new ConstValue(strippedExpr);
            }

            throw new ParserException.WrongFormat(Symbol.Quote.Name, args);
        }

        private static ConstValue ParseSyntax(SyntaxWrapper args)
        {
            if (TryExposeOneArg(args, out SyntaxWrapper? syntacticValue))
            {
                return new ConstValue(syntacticValue);
            }

            throw new ParserException.WrongFormat(Symbol.Syntax.Name, args);
        }

        private static BindingDefinition ParseDefinition(SyntaxWrapper args, BindingStore bs, int phase)
        {
            if (TryExposeTwoArgs(args, out SyntaxWrapper? arg1, out SyntaxWrapper? arg2)
                && TryExposeBindingId(arg1, bs, phase, out string? key))
            {
                AstNode boundValueExpr = ParseAST(arg2, bs, phase);
                return new BindingDefinition(key, boundValueExpr);
            }

            throw new ParserException.WrongFormat(Symbol.Define.Name, args);
        }

        private static BindingMutation ParseSet(SyntaxWrapper args, BindingStore bs, int phase)
        {
            if (TryExposeTwoArgs(args, out SyntaxWrapper? arg1, out SyntaxWrapper? arg2)
                && TryExposeBindingId(arg1, bs, phase, out string? key))
            {
                AstNode boundValueExpr = ParseAST(arg2, bs, phase);
                return new BindingMutation(key, boundValueExpr);
            }

            throw new ParserException.WrongFormat(Symbol.Set.Name, args);
        }

        private static string[] AnalyzeInternalDefinitions(SequentialForm seq)
        {
            List<string> internalKeys = new List<string>();

            foreach(AstNode node in seq.Sequence)
            {
                if (node is BindingDefinition bd)
                {
                    internalKeys.Add(bd.VarName);
                }
            }

            return internalKeys.ToArray();
        }

        private static FunctionCreation ParseLambda(SyntaxWrapper args, BindingStore bs, int phase)
        {
            if (TryExposeTwoArgs(args, out SyntaxWrapper? paramStx, out SyntaxWrapper? bodyStx))
            {
                Tuple<string[], string?> parameters = ParseParams(paramStx, bs, phase);
                SequentialForm body = ParseBegin(bodyStx, bs, phase);

                string[] internalKeys = AnalyzeInternalDefinitions(body);

                return new FunctionCreation(parameters.Item1, parameters.Item2, internalKeys, body);
            }

            throw new ParserException.WrongFormat(Symbol.Lambda.Name, args);
        }

        private static ConditionalForm ParseBranch(SyntaxWrapper args, BindingStore bs, int phase)
        {
            if (TryExposeThreeArgs(args, out SyntaxWrapper? arg1, out SyntaxWrapper? arg2, out SyntaxWrapper? arg3))
            {
                AstNode test = ParseAST(arg1, bs, phase);
                AstNode consequent = ParseAST(arg2, bs, phase);
                AstNode alternate = ParseAST(arg3, bs, phase);

                return new ConditionalForm(test, consequent, alternate);
            }

            throw new ParserException.WrongFormat(Symbol.If.Name, args);
        }

        private static SequentialForm ParseBegin(SyntaxWrapper args, BindingStore bs, int phase)
        {
            AstNode[] series = ParseArgs(args, bs, phase).ToArray();

            if (series.Length == 0)
            {
                throw new ParserException.WrongFormat(Symbol.Begin.Name, args);
            }
            else
            {
                return new SequentialForm(series);
            }
        }

        #endregion

        #region Auxiliary Structures

        private static IEnumerable<AstNode> ParseArgs(SyntaxWrapper argList, BindingStore bs, int phase)
        {
            SyntaxWrapper current = argList;

            while (current.TryExposeList(out SyntaxWrapper? first, out SyntaxWrapper? rest))
            {
                AstNode newArg = ParseAST(first, bs, phase);
                yield return newArg;
                current = rest;
            }

            if (current.Expose() is not Nil)
            {
                throw new ParserException.Uncategorized("Cannot supply dotted list as function arguments.");
            }

            yield break;
        }

        private static Tuple<string[], string?> ParseParams(SyntaxWrapper paramList, BindingStore bs, int phase)
        {
            List<string> regularParams = new List<string>();

            SyntaxWrapper current = paramList;

            while (current.TryExposeList(out SyntaxWrapper? first, out SyntaxWrapper? rest))
            {
                if (TryExposeBindingId(first, bs, phase, out string? paramName))
                {
                    regularParams.Add(paramName);
                }
                else
                {
                    throw new ParserException.WrongFormat("Lambda parameter list", paramList);
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

        private static bool TryExposeBindingId(SyntaxWrapper stx, BindingStore bs, int phase,
            [NotNullWhen(true)] out string? bindingName)
        {
            if (stx.TryExposeIdentifier(out Symbol? _, out string? symbolicName))
            {
                bindingName = bs.ResolveBindingName(symbolicName, stx.Context[phase]);
                return true;
            }
            bindingName = null;
            return false;
        }

        private static bool TryExposeOneArg(SyntaxWrapper stx,
            [NotNullWhen(true)] out SyntaxWrapper? arg1)
        {
            return stx.TryExposeList(out arg1, out SyntaxWrapper? terminator)
                && terminator.Expose() is Nil;
        }

        private static bool TryExposeTwoArgs(SyntaxWrapper stx,
            [NotNullWhen(true)] out SyntaxWrapper? arg1,
            [NotNullWhen(true)] out SyntaxWrapper? arg2)
        {
            arg2 = null;

            return stx.TryExposeList(out arg1, out SyntaxWrapper? rest)
                && rest.TryExposeList(out arg2, out SyntaxWrapper? terminator)
                && terminator.Expose() is Nil;
        }

        private static bool TryExposeThreeArgs(SyntaxWrapper stx,
            [NotNullWhen(true)] out SyntaxWrapper? arg1,
            [NotNullWhen(true)] out SyntaxWrapper? arg2,
            [NotNullWhen(true)] out SyntaxWrapper? arg3)
        {
            arg2 = null;
            arg3 = null;

            return stx.TryExposeList(out arg1, out SyntaxWrapper? rest1)
                && rest1.TryExposeList(out arg2, out SyntaxWrapper? rest2)
                && rest2.TryExposeList(out arg3, out SyntaxWrapper? terminator)
                && terminator.Expose() is Nil;
        }



        #endregion
    }
}
