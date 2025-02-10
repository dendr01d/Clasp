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
                if (exResult.TryResolveBinding(id, out CompileTimeBinding[] candidates, out CompileTimeBinding? binding))
                {
                    if (binding.BoundType == BindingType.Variable)
                    {
                        return new VariableLookup(binding.Name);
                    }
                    else if (exResult.TryDereferenceMacro(binding, out MacroProcedure? macro))
                    {
                        return new ConstValue(macro);
                    }
                    else if (binding.BoundType == BindingType.Primitive)
                    {
                        throw new System.NotImplementedException();
                    }
                    else
                    {
                        throw new ParserException.InvalidForm(id.Name, id);
                    }
                }
                else if (candidates.Length > 1)
                {
                    throw new ParserException.AmbiguousIdentifier(id, candidates);
                }
                else
                {
                    throw new ParserException.UnboundIdentifier(id);
                }
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
                && exResult.TryResolveBinding(idOp, out _, out CompileTimeBinding? binding)
                && binding.BoundType == BindingType.Special)
            {
                return ParseSpecial(idOp.Name, stp, exResult);
            }

            CoreForm parsedOp = Parse(stp.Car, exResult);

            // check to make sure it's not an imperative form
            if (parsedOp.IsImperative)
            {
                throw new ParserException.InvalidOperator(parsedOp, stp);
            }

            CoreForm[] argTerms = ParseArguments(stp.Cdr, exResult).ToArray();

            return new FunctionApplication(parsedOp, argTerms.ToArray());
        }

        private static CoreForm ParseSpecial(string formName, SyntaxPair stx, ExpansionContext exResult)
        {
            // all special forms have at least one argument
            if (stx.Cdr is not SyntaxPair args)
            {
                throw new ParserException.WrongArity(formName, "at least one", stx);
            }

            CoreForm result;
            
            try
            {
                result = formName switch
                {
                    Keyword.IMP_TOP => ParseVariableLookup(args, exResult),
                    Keyword.IMP_VAR => ParseVariableLookup(args, exResult),

                    Keyword.QUOTE => ParseQuote(args),
                    Keyword.IMP_DATUM => ParseQuote(args),

                    Keyword.QUOTE_SYNTAX => ParseQuoteSyntax(args),

                    Keyword.APPLY => ParseApplication(args, exResult),
                    Keyword.IMP_APP => ParseApplication(args, exResult),

                    Keyword.IMP_PARDEF => ParseDefinition(args, exResult),
                    Keyword.DEFINE => ParseDefinition(args, exResult),
                    Keyword.DEFINE_SYNTAX => ParseDefinition(args, exResult),
                    Keyword.SET => ParseSet(args, exResult),

                    Keyword.LAMBDA => ParseLambda(args, exResult),
                    Keyword.IMP_LAMBDA => ParseLambda(args, exResult),

                    Keyword.IF => ParseIf(args, exResult),

                    Keyword.BEGIN => ParseBegin(args, exResult),

                    _ => throw new ParserException.InvalidSyntax(stx)
                };
            }
            catch (ParserException pe)
            {
                throw new ParserException.InvalidForm(formName, stx, pe);
            }

            return result;
        }

        #region Special Forms

        private static CoreForm ParseVariableLookup(Syntax stx, ExpansionContext exResult)
        {
            if (stx.TryDestruct(out Identifier? idArg, out Syntax? terminator, out _)
                && terminator.IsTerminator())
            {
                if (exResult.TryResolveBinding(idArg, out CompileTimeBinding[] candidates, out CompileTimeBinding? binding))
                {
                    return new VariableLookup(binding.Name);
                }
                else if (candidates.Length > 1)
                {
                    throw new ParserException.AmbiguousIdentifier(idArg, candidates);
                }
                else
                {
                    throw new ParserException.UnboundIdentifier(idArg);
                }
            }
            else
            {
                throw new ParserException.InvalidSyntax(stx);
            }
        }

        private static ConstValue ParseQuote(Syntax stx)
        {
            if (stx.TryDestruct(out Syntax? arg, out Syntax? terminator, out _)
                && terminator.IsTerminator())
            {
                return new ConstValue(arg.ToDatum());
            }

            throw new ParserException.WrongArity(Keyword.QUOTE, "exactly one", stx);
        }

        private static ConstValue ParseQuoteSyntax(Syntax stx)
        {
            if (stx.TryDestruct(out Syntax? arg, out Syntax? terminator, out _)
                && terminator.IsTerminator())
            {
                return new ConstValue(arg);
            }

            throw new ParserException.WrongArity(Keyword.QUOTE_SYNTAX, "exactly one", stx);
        }

        private static BindingDefinition ParseDefinition(Syntax stx, ExpansionContext exResult)
        {
            if (stx.TryDestruct(out Identifier? key, out SyntaxPair? keyTail, out _)
                && keyTail.TryDestruct(out Syntax? value, out Syntax? terminator, out _)
                && terminator.IsTerminator())
            {
                if (exResult.TryResolveBinding(key, out CompileTimeBinding[] candidates, out CompileTimeBinding? binding))
                {
                    CoreForm parsedValue = Parse(value, exResult);
                    return new BindingDefinition(binding.Name, parsedValue);
                }
                else if (candidates.Length > 1)
                {
                    throw new ParserException.AmbiguousIdentifier(key, candidates);
                }
                else
                {
                    throw new ParserException.UnboundIdentifier(key);
                }
            }
            
            throw new ParserException.WrongArity(Symbol.Define.Name, "exactly two", stx);
        }

        private static BindingMutation ParseSet(Syntax stx, ExpansionContext exResult)
        {
            if (stx.TryDestruct(out Identifier? key, out SyntaxPair? keyTail, out _)
                && keyTail.TryDestruct(out Syntax? value, out Syntax? terminator, out _)
                && terminator.IsTerminator())
            {
                if (exResult.TryResolveBinding(key,
                    out CompileTimeBinding[] candidates,
                    out CompileTimeBinding? binding))
                {
                    CoreForm parsedValue = Parse(value, exResult);
                    return new BindingMutation(binding.Name, parsedValue);
                }
                else if (candidates.Length > 1)
                {
                    throw new ParserException.AmbiguousIdentifier(key, candidates);
                }
                else
                {
                    throw new ParserException.UnboundIdentifier(key);
                }
            }

            throw new ParserException.WrongArity(Symbol.Define.Name, "exactly two", stx);
        }

        private static FunctionCreation ParseLambda(Syntax stx, ExpansionContext exResult)
        {
            if (stx.TryDestruct(out Syntax? formals, out SyntaxPair? body, out _))
            {
                System.Tuple<string[], string?> parameters = ParseParameters(formals, exResult);

                IEnumerable<CoreForm> bodyTerms = ParseSequence(body, exResult);
                AggregateKeyTerms(bodyTerms, out string[] informals, out CoreForm[] moddedBodyTerms);

                SequentialForm bodyForm = new SequentialForm(moddedBodyTerms);

                return new FunctionCreation(parameters.Item1, parameters.Item2, informals, bodyForm);
            }

            throw new ParserException.WrongArity(Symbol.Lambda.Name, "two or more", stx);
        }

        private static ConditionalForm ParseIf(Syntax stx, ExpansionContext exResult)
        {
            if (stx.TryDestruct(out Syntax? condValue, out SyntaxPair? thenPair, out _)
                && thenPair.TryDestruct(out Syntax? thenValue, out Syntax? elsePair, out _)
                && elsePair.TryDestruct(out Syntax? elseValue, out Syntax? terminator, out _)
                && terminator.IsTerminator())
            {
                CoreForm parsedCond = Parse(condValue, exResult);
                CoreForm parsedThen = Parse(thenValue, exResult);
                CoreForm parsedElse = Parse(elseValue, exResult);

                return new ConditionalForm(parsedCond, parsedThen, parsedElse);
            }

            throw new ParserException.WrongArity(Symbol.Lambda.Name, "exactly three", stx);
        }

        private static SequentialForm ParseBegin(Syntax stx, ExpansionContext exResult)
        {
            IEnumerable<CoreForm> sequence = ParseSequence(stx, exResult);
            return new SequentialForm(sequence.ToArray());
        }

        #endregion

        #region Auxiliary Structures

        /// <summary>
        /// Enumerate all the forms in a (proper) list of argument expressions.
        /// </summary>
        private static IEnumerable<CoreForm> ParseArguments(Syntax stx, ExpansionContext exResult)
        {
            Syntax current = stx;

            while (current.TryDestruct(out Syntax? first, out Syntax? rest, out _))
            {
                CoreForm nextArg = Parse(first, exResult);

                if (nextArg.IsImperative)
                {
                    throw new ParserException.ExpectedExpression(nextArg, stx);
                }

                yield return nextArg;
                current = rest;
            }

            if (!current.IsTerminator())
            {
                throw new ParserException.ExpectedProperList(stx);
            }

            yield break;
        }

        /// <summary>
        /// Enumerate all the identifiers in a list of parameter values.
        /// </summary>
        private static System.Tuple<string[], string?> ParseParameters(Syntax stx, ExpansionContext exResult)
        {
            List<string> ids = new List<string>();
            string? dotted = null;

            Syntax current = stx;

            while (current.TryDestruct(out Identifier? nextParam, out Syntax? rest, out _))
            {
                if (exResult.TryResolveBinding(nextParam, 
                    out CompileTimeBinding[] candidates,
                    out CompileTimeBinding? binding))
                {
                    ids.Add(binding.Name);
                    current = rest;
                }
                else if (candidates.Length > 1)
                {
                    throw new ParserException.AmbiguousIdentifier(nextParam, candidates);
                }
                else
                {
                    throw new ParserException.UnboundIdentifier(nextParam);
                }
            }

            if (current is Identifier lastParam)
            {
                if (exResult.TryResolveBinding(lastParam,
                    out CompileTimeBinding[] candidates,
                    out CompileTimeBinding? binding))
                {
                    dotted = binding.Name;
                }
                else if (candidates.Length > 1)
                {
                    throw new ParserException.AmbiguousIdentifier(lastParam, candidates);
                }
                else
                {
                    throw new ParserException.UnboundIdentifier(lastParam);
                }
            }

            return new System.Tuple<string[], string?>(ids.ToArray(), dotted);
        }

        /// <summary>
        /// Enumerate all the forms in a (proper) list of body terms, where the last must be an expression.
        /// </summary>
        private static IEnumerable<CoreForm> ParseSequence(Syntax stx, ExpansionContext exResult)
        {
            Syntax current = stx;

            while (current.TryDestruct(out Syntax? first, out Syntax? rest, out _))
            {
                CoreForm nextForm = Parse(first, exResult);

                if (rest.IsTerminator() && nextForm.IsImperative)
                {
                    throw new ParserException.ExpectedExpression(nextForm, stx);
                }
                else if (nextForm is SequentialForm sf)
                {
                    // flatten nested sequences
                    foreach(CoreForm form in sf.Sequence)
                    {
                        yield return form;
                    }
                }
                else
                {
                    yield return nextForm;
                }

                current = rest;
            }

            if (!current.IsTerminator())
            {
                throw new ParserException.ExpectedProperList(stx);
            }

            yield break;
        }

        /// <summary>
        /// Iterate through a list of body terms, aggregating the key variables from each
        /// <see cref="BindingDefinition"/> form and transforming them into <see cref="BindingMutation"/> forms.
        /// </summary>
        private static void AggregateKeyTerms(IEnumerable<CoreForm> initialForms,
            out string[] keyTerms,
            out CoreForm[] adjustedForms)
        {
            List<string> keys = new List<string>();
            List<CoreForm> adjustments = new List<CoreForm>();

            foreach(CoreForm form in initialForms)
            {
                if (form is BindingDefinition bd)
                {
                    keys.Add(bd.VarName);
                    adjustments.Add(new BindingMutation(bd.VarName, bd.BoundValue));
                }
                else
                {
                    adjustments.Add(form);
                }
            }

            keyTerms = keys.ToArray();
            adjustedForms = adjustments.ToArray();
        }

        #endregion
    }
}
