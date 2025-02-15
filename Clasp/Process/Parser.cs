using System.Collections.Generic;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.ExtensionMethods;

namespace Clasp.Process
{
    internal static class Parser
    {
        // the MOST IMPORTANT thing to remember here is that every syntactic form must break down
        // into ONLY core forms

        public static CoreForm ParseSyntax(Syntax stx, ParseContext context)
        {
            return Parse(stx, context);
        }

        public static CoreForm ParseSyntax(Syntax stx, Environment env, int phase)
        {
            ParseContext ctx = new ParseContext(env, phase);
            return Parse(stx, ctx);
        }

        private static CoreForm Parse(Syntax stx, ParseContext context)
        {
            if (stx is Identifier id)
            {
                CompileTimeBinding binding = ResolveBinding(id, context);

                if (binding.BoundType == BindingType.Variable)
                {
                    return new VariableLookup(binding.Name);
                }
                else if (binding.BoundType == BindingType.Transformer)
                {
                    if (context.TryLookupMacro(binding, out MacroProcedure? macro))
                    {
                        return new ConstValue(macro);
                    }

                    throw new ParserException.UnboundMacro(binding.Id);
                }

                throw new ParserException.InvalidForm(id.Name, stx);
            }
            else if (stx is SyntaxList stl)
            {
                return ParseApplication(stl, context);
            }
            else
            {
                return new ConstValue(stx.ToDatum());
            }
        }

        private static CoreForm ParseApplication(SyntaxList stl, ParseContext context)
        {
            if (stl.Car is Identifier op
                && op.TryResolveBinding(context.Phase, out CompileTimeBinding? binding)
                && binding.BoundType == BindingType.Special)
            {
                return ParseSpecial(op.Name, stl, context);
            }

            CoreForm parsedOp = Parse(stl.Car, context);

            if (parsedOp.IsImperative)
            {
                throw new ParserException.InvalidOperator(parsedOp, stl);
            }

            CoreForm[] argTerms = stl.Cdr switch
            {
                Nil => [],
                StxPair stp => ParseArguments(stp, stl.LexContext, context).ToArray(),
                _ => throw new ParserException.InvalidSyntax(stl)
            };

            return new FunctionApplication(parsedOp, argTerms);
        }

        private static CoreForm ParseSpecial(string formName, SyntaxList stx, ParseContext context)
        {
            // all special forms have at least one argument
            if (stx.Cdr is not SyntaxList args)
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

        private static CoreForm ParseVariableLookup(Syntax stx, ParseContext context)
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

        private static BindingDefinition ParseDefinition(Syntax stx, ParseContext context)
        {
            if (stx.TryDestruct(out Identifier? key, out SyntaxList? keyTail, out _)
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

        private static BindingMutation ParseSet(Syntax stx, ParseContext context)
        {
            if (stx.TryDestruct(out Identifier? key, out SyntaxList? keyTail, out _)
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

        private static FunctionCreation ParseLambda(Syntax stx, ParseContext context)
        {
            if (stx.TryDestruct(out Syntax? formals, out SyntaxList? body, out _))
            {
                System.Tuple<string[], string?> parameters = ParseParameters(formals, exResult);

                IEnumerable<CoreForm> bodyTerms = ParseSequence(body, exResult);
                AggregateKeyTerms(bodyTerms, out string[] informals, out CoreForm[] moddedBodyTerms);

                SequentialForm bodyForm = new(moddedBodyTerms);

                return new FunctionCreation(parameters.Item1, parameters.Item2, informals, bodyForm);
            }

            throw new ParserException.WrongArity(Symbol.Lambda.Name, "two or more", stx);
        }

        private static ConditionalForm ParseIf(Syntax stx, ParseContext context)
        {
            if (stx.TryDestruct(out Syntax? condValue, out SyntaxList? thenPair, out _)
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

        private static SequentialForm ParseBegin(Syntax stx, ParseContext context)
        {
            IEnumerable<CoreForm> sequence = ParseSequence(stx, exResult);
            return new SequentialForm(sequence.ToArray());
        }

        #endregion

        #region Auxiliary Structures

        /// <summary>
        /// Enumerate all the forms in a (proper) list of argument expressions.
        /// </summary>
        private static IEnumerable<CoreForm> ParseArguments(StxPair stp, LexInfo info, ParseContext context)
        {
            Term current = stp;

            while (current is StxPair currentStp)
            {
                CoreForm nextArg = Parse(currentStp.Car, context);

                if (nextArg.IsImperative)
                {
                    throw new ParserException.ExpectedExpression(nextArg, currentStp.Car);
                }

                yield return nextArg;
                current = currentStp.Cdr;
            }

            if (current is not Nil)
            {
                throw new ParserException.ExpectedProperList(stp, info);
            }

            yield break;
        }

        /// <summary>
        /// Enumerate all the identifiers in a list of parameter values.
        /// </summary>
        private static System.Tuple<string[], string?> ParseParameters(Term t, LexInfo info, ParseContext context)
        {
            List<string> ids = [];
            string? dotted = null;

            Term current = t;

            while(current is StxPair stp)
            {
                if (stp.Car is Identifier nextParam)
                {
                    CompileTimeBinding binding = ResolveBinding(nextParam, context);
                    ids.Add(binding.Name);
                }
                else
                {
                    throw new ParserException.ExpectedProperList(nameof(Identifier), t, info);
                }

                current = stp.Cdr;
            }

            if (current is Identifier lastParam)
            {
                CompileTimeBinding binding = ResolveBinding(lastParam, context);
                dotted = binding.Name;
            }

            return new System.Tuple<string[], string?>(ids.ToArray(), dotted);
        }

        /// <summary>
        /// Enumerate all the forms in a (proper) list of body terms, where the last must be an expression.
        /// </summary>
        private static IEnumerable<CoreForm> ParseSequence(StxPair stp, LexInfo info, ParseContext context)
        {
            Term current = stp;

            while (current is StxPair nextSequent)
            {
                CoreForm nextForm = Parse(nextSequent.Car, context);

                if (nextSequent.Cdr is Nil && nextForm.IsImperative)
                {
                    throw new ParserException.ExpectedExpression(nextForm, info);
                }
                else if (nextForm is SequentialForm sf)
                {
                    foreach(CoreForm form in sf.Sequence)
                    {
                        yield return form;
                    }
                }
                else
                {
                    yield return nextForm;
                }

                current = nextSequent.Cdr;
            }

            if (current is not Nil)
            {
                throw new ParserException.ExpectedProperList(stp, info);
            }

            yield break;
        }

        #endregion

        #region Helpers


        /// <summary>
        /// Iterate through a list of body terms, aggregating the key variables from each
        /// <see cref="BindingDefinition"/> form and transforming them into <see cref="BindingMutation"/> forms.
        /// </summary>
        private static void AggregateKeyTerms(IEnumerable<CoreForm> initialForms,
            out string[] keyTerms,
            out CoreForm[] adjustedForms)
        {
            List<string> keys = [];
            List<CoreForm> adjustments = [];

            foreach (CoreForm form in initialForms)
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

        private static CompileTimeBinding ResolveBinding(Identifier id, ParseContext context)
        {
            if (id.TryResolveBinding(context.Phase,
                out CompileTimeBinding? binding,
                out CompileTimeBinding[] candidates))
            {
                return binding;
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

        #endregion
    }
}
