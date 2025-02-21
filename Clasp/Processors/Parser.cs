using System.Collections.Generic;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Exceptions;
using Clasp.ExtensionMethods;

using static System.Net.WebRequestMethods;

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

        public static CoreForm ParseSyntax(Syntax stx, ClaspEnvironment env, int phase)
        {
            ParseContext ctx = new ParseContext(env, phase);
            return Parse(stx, ctx);
        }

        private static CoreForm Parse(Syntax stx, ParseContext context)
        {
            try
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
            catch (ClaspException cex)
            {
                throw new ParserException.InvalidSyntax(stx, cex);
            }

        }

        private static CoreForm ParseApplication(SyntaxList stl, ParseContext context)
        {
            if (stl.Expose().Car is Identifier op
                && ResolveBinding(op, context).BoundType == BindingType.Special)
            {
                return ParseSpecial(op.Name, stl, context);
            }

            CoreForm parsedOp = Parse(stl.Expose().Car, context);

            if (parsedOp.IsImperative)
            {
                throw new ParserException.InvalidOperator(parsedOp, stl);
            }

            CoreForm[] argTerms = stl.Expose().Cdr switch
            {
                Nil => [],
                Cons cns => ParseArguments(cns, stl.LexContext, context).ToArray(),
                _ => throw new ParserException.InvalidSyntax(stl)
            };

            return new FunctionApplication(parsedOp, argTerms);
        }

        private static CoreForm ParseSpecial(string formName, SyntaxList stl, ParseContext context)
        {
            // all special forms have at least one argument
            if (stl.Expose().Cdr is not Cons args)
            {
                throw new ParserException.InvalidForm(formName, stl);
            }

            LexInfo info = stl.LexContext;
            CoreForm result;

            try
            {
                result = formName switch
                {
                    Keyword.IMP_TOP => ParseVariableLookup(args, info, context),
                    Keyword.IMP_VAR => ParseVariableLookup(args, info, context),

                    Keyword.QUOTE => ParseQuote(args, info),
                    Keyword.IMP_DATUM => ParseQuote(args, info),

                    Keyword.QUOTE_SYNTAX => ParseQuoteSyntax(args, info, context),

                    Keyword.APPLY => ParseApplication(stl.PopFront(), context),
                    Keyword.STATIC_APPLY => ParseApplication(stl.PopFront(), context),

                    Keyword.STATIC_PARDEF => ParseDefinition(args, info, context),
                    Keyword.DEFINE => ParseDefinition(args, info, context),
                    Keyword.DEFINE_SYNTAX => ParseDefinition(args, info, context),
                    Keyword.SET => ParseSet(args, info, context),

                    Keyword.LAMBDA => ParseLambda(args, info, context),
                    Keyword.STATIC_LAMBDA => ParseLambda(args, info, context),

                    Keyword.IF => ParseIf(args, info, context),

                    Keyword.BEGIN => ParseBegin(args, info, context),

                    Keyword.MODULE => ParseModule(args, info, context),
                    Keyword.IMPORT => ParseImport(args, info, context),

                    _ => throw new ParserException.InvalidSyntax(stl)
                };
            }
            catch (ParserException pe)
            {
                throw new ParserException.InvalidForm(formName, stl, pe);
            }

            return result;
        }

        #region Special Forms

        private static CoreForm ParseVariableLookup(Cons cns, LexInfo info, ParseContext context)
        {
            if (cns.TryMatchOnly(out Identifier? id))
            {
                CompileTimeBinding binding = ResolveBinding(id, context);

                return new VariableLookup(binding.Name);
            }

            throw new ParserException.InvalidArguments(cns, "exactly", 1, info);
        }

        private static ConstValue ParseQuote(Cons cns, LexInfo info)
        {
            if (cns.TryMatchOnly(out Syntax? stx))
            {
                return new ConstValue(stx.ToDatum());
            }

            throw new ParserException.InvalidArguments(cns, "exactly", 1, info);
        }

        private static ConstValue ParseQuoteSyntax(Cons cns, LexInfo info, ParseContext context)
        {
            if (cns.TryMatchOnly(out Syntax? stx))
            {
                return new ConstValue(stx.StripScopes(context.Phase));
            }

            throw new ParserException.InvalidArguments(cns, "exactly", 1, info);
        }

        private static BindingDefinition ParseDefinition(Cons cns, LexInfo info, ParseContext context)
        {
            if (cns.TryMatchOnly(out Identifier? key, out Syntax? value))
            {
                CompileTimeBinding binding = ResolveBinding(key, context);
                CoreForm parsedValue = Parse(value, context);

                return new BindingDefinition(binding.Name, parsedValue);
            }

            throw new ParserException.InvalidArguments(cns, "exactly", 2, info);
        }

        private static BindingMutation ParseSet(Cons cns, LexInfo info, ParseContext context)
        {
            if (cns.TryMatchOnly(out Identifier? key, out Syntax? value))
            {
                CompileTimeBinding binding = ResolveBinding(key, context);
                CoreForm parsedValue = Parse(value, context);

                return new BindingMutation(binding.Name, parsedValue);
            }

            throw new ParserException.InvalidArguments(cns, "exactly", 2, info);
        }

        private static FunctionCreation ParseLambda(Cons cns, LexInfo info, ParseContext context)
        {
            if (cns.TryMatchLeading(out Syntax? formals, out Term maybeBody)
                && maybeBody is Cons body)
            {
                System.Tuple<string[], string?> parameters = ParseParameters(formals, info, context);

                IEnumerable<CoreForm> bodyTerms = ParseSequence(body, info, context);
                AggregateKeyTerms(bodyTerms, out string[] informals, out CoreForm[] moddedBodyTerms);

                SequentialForm bodyForm = new(moddedBodyTerms);

                return new FunctionCreation(parameters.Item1, parameters.Item2, informals, bodyForm);
            }

            throw new ParserException.InvalidArguments(cns, "at least", 2, info);
        }

        private static ConditionalForm ParseIf(Cons cns, LexInfo info, ParseContext context)
        {
            if (cns.TryMatchOnly(out Syntax? condStx, out Syntax? thenStx, out Syntax? elseStx))
            {
                CoreForm parsedCond = Parse(condStx, context);
                CoreForm parsedThen = Parse(condStx, context);
                CoreForm parsedElse = Parse(condStx, context);

                return new ConditionalForm(parsedCond, parsedThen, parsedElse);
            }

            throw new ParserException.InvalidArguments(cns, "exactly", 3, info);
        }

        private static SequentialForm ParseBegin(Cons stp, LexInfo info, ParseContext context)
        {
            IEnumerable<CoreForm> sequence = ParseSequence(stp, info, context);
            return new SequentialForm(sequence.ToArray());
        }

        private static Importation ParseImport(Cons cns, LexInfo info, ParseContext context)
        {
            if (cns.TryMatchOnly(out Datum? maybePath)
                && maybePath.Expose() is CharString path)
            {
                return new Importation(path.Value);
            }

            throw new ParserException.InvalidArguments(cns, info);
        }

        private static CoreForm ParseModule(Cons cns, LexInfo info, ParseContext context)
        {
            if (cns.TryMatchLeading(out Identifier? id, out Term maybeBody)
                && maybeBody is Cons body)
            {
                if (context.Phase == 1)
                {
                    // if running at phase 1, then this is the content of the current execution
                    // therefore we just switch to evaluating its contents directly
                    return ParseBegin(body, info, context);
                }

                IEnumerable<CoreForm> bodyTerms = ParseSequence(body, info, context);
                AggregateExportedNames(bodyTerms, out string[] exportedNames, out CoreForm[] filteredBodyTerms);

                SequentialForm moduleBody = new SequentialForm(filteredBodyTerms);

                return new ModuleForm(id.Name, exportedNames, moduleBody);
            }

            throw new ParserException.InvalidArguments(cns, "at least", 2, info);
        }

        #endregion

        #region Auxiliary Structures

        /// <summary>
        /// Enumerate all the forms in a (proper) list of argument expressions.
        /// </summary>
        private static IEnumerable<CoreForm> ParseArguments(Cons cns, LexInfo info, ParseContext context)
        {
            Term current = cns;

            while (current is Cons currentCons && currentCons.Car is Syntax nextStx)
            {
                CoreForm nextArg = Parse(nextStx, context);

                if (nextArg.IsImperative)
                {
                    throw new ParserException.ExpectedExpression(nextArg, info);
                }

                yield return nextArg;
                current = currentCons.Cdr;
            }

            if (current is not Nil)
            {
                throw new ParserException.ExpectedProperList(cns, info);
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

            if (current is SyntaxList stl)
            {
                current = stl.Expose();
            }

            while(current is Cons stp)
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
        private static IEnumerable<CoreForm> ParseSequence(Cons stp, LexInfo info, ParseContext context)
        {
            Term current = stp;

            while (current is Cons nextSequent && nextSequent.Car is Syntax nextStx)
            {
                CoreForm nextForm = Parse(nextStx, context);

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
        private static CompileTimeBinding ResolveBinding(Identifier id, ParseContext context)
        {
            if (id.TryResolveBinding(context.Phase,
                out CompileTimeBinding? binding))
            {
                return binding;
            }
            //else if (candidates.Length > 1)
            //{
            //    throw new ParserException.AmbiguousIdentifier(id, candidates);
            //}
            else
            {
                throw new ParserException.UnboundIdentifier(id);
            }
        }

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

        private static void AggregateExportedNames(IEnumerable<CoreForm> initialBody,
            out string[] exportedNames,
            out CoreForm[] filteredBody)
        {
            List<string> names = new List<string>();
            List<CoreForm> filtered = new List<CoreForm>();

            foreach(CoreForm form in initialBody)
            {
                if (form is Exportation exprt)
                {
                    names.Add(exprt.Name);
                }
                else
                {
                    filtered.Add(form);
                }
            }

            exportedNames = names.ToArray();
            filteredBody = filtered.ToArray();
        }

        #endregion
    }
}
