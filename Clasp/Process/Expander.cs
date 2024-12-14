using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Process
{
    internal static class Expander
    {
        public static Syntax Expand(Syntax input, Environment env)
        {
            ExpansionHarness harness = new ExpansionHarness(env);

            return Expand(input, harness, ExpansionContext.TopLevel);
        }

        private static Syntax Expand(Syntax input, ExpansionHarness harness, ExpansionContext context)
        {
            if (input is SyntaxList product
                && product.WrappedValue is ConsList cl
                && cl.Car is Identifier opId
                && ebm.ResolveBindingName(opId) is string bindingName)
            {
                if (bindingName == Symbol.Quote.Name)
                {
                    return input;
                }
                else if (bindingName == Symbol.Syntax.Name)
                {
                    return input;
                }
                else if (bindingName == Symbol.Lambda.Name)
                {
                    return ExpandLambdaForm(cl, ebm);
                }
                else if (bindingName == Symbol.Define.Name)
                {
                    return ExpandDefinition(cl, ebm);
                }
                else if (bindingName == Symbol.DefineSyntax.Name)
                {
                    return ExpandDefinition(cl, ebm);
                }
                else if (cte.TryGetValue(bindingName, out AstNode? node)
                    && node is Fixed f
                    && f.Value is Transformer tx)
                {
                    Syntax transformedStx = InvokeMacroTransformation(tx, input, ebm);
                    return Expand(transformedStx, ebm);
                }
            }
            else if (input is Identifier id
                && ebm.ResolveBoundForm(id) is Term boundTerm)
            {
                if (boundTerm is Syntax stx)
                {
                    return stx;
                }
                else if (boundTerm is Transformer tx)
                {
                    Syntax transformedStx = InvokeMacroTransformation(tx, input, ebm);
                    return Expand(transformedStx, ebm);
                }
            }

            throw new ExpanderException.UnknownForm(input);
        }

        private static Syntax ExpandLambdaForm(ConsList input, ExpansionBindingMatrix ebm)
        {
            ExpansionBindingMatrix extended = ebm.ExtendScope();



            //a lambda form creates a new scope
            //bindings must be created in this scope for each parameter name
            //  as well as each defined name in the body
            //  (both regular and transformers)

            //then the body itself must be expanded in the context of the extended scope


            Binding.Environment subEnv = new Binding.Environment(cte);
            ScopeSet extendedScope = opId.Context[phaseLevel].Extend(subEnv);

            // bindings need to be created for the parameter names. how to generate the new names?
            //this can be accomplished in the course of expanding the formal term
            Syntax expandedParameters;

            // then the generated new names need to be mapped in the CTE closure to variables built from those new names

            // finally, the body term needs to be expanded in the resulting cte and bs
            // as a part of this process, the body must first be partially expanded
            // -- in order to catch and process any informal parameters introduced via define forms
            // -- and also any macros defined in the local scope

            Syntax expandedBody;

            Term form = ConsList.ImproperList(opId, expandedParameters, expandedBody);
            return Syntax.Wrap(form, product.Context, product.Source);
        }

        private static void ExpandParameters(Syntax input, ExpansionBindingMatrix ebm)
        {
            if (TryExposePair<Identifier>(input, out Identifier? carId, out Syntax? cdrStx))
            {
                ebm.RebindAsFresh(carId);
            }
            else if (input is Identifier id)
            {
                ebm.RebindAsFresh(id);
            }
            else if (input is not SyntaxAtom sa || sa.WrappedValue is not Nil)
            {
                throw new ExpanderException.UnknownForm(input, "the parameter list of a Lambda form");
            }
        }

        private static void AggregateBodyDefinitions(Syntax input, ExpansionBindingMatrix ebm)
        {
            if (TryExposePair(input, out Syntax? listItem, out Syntax? listTail))
            {
                // is it a define form?
                if (TryExposePair<Identifier>(listItem, out Identifier? op, out Syntax? defArgs)
                    && (op.Name == Symbol.Define.Name || op.Name == Symbol.DefineSyntax.Name))
                {
                    // does it have a proper key/value pair?
                    if (TryExposePair(defArgs, out Syntax? key, out Syntax? value))
                    {
                        // is it using shorthand function style?
                        if (TryExposePair<Identifier>(key, out Identifier? name, out Syntax? tail))
                        {
                            ebm.RebindAsFresh(name);
                        }
                        else if (key is Identifier id)
                        {
                            ebm.RebindAsFresh(id);
                        }
                    }
                    else
                    {
                        throw new ExpanderException.UnknownForm(listItem, "the definitions of a Lambda form body");
                    }
                }

                AggregateBodyDefinitions(listTail, ebm);
            }
            else if (!IsSyntacticNil(input))
            {
                throw new ExpanderException.UnknownForm(input, "the definitions of a Lambda form body");
            }
        }

        private static Syntax ExpandBody(Term input, Binding.Environment cte, BindingStore bs, int phaseLevel)
        {

        }

        private static Syntax ExpandDefinition(ConsList input, Binding.Environment cte, BindingStore bs, int phaseLevel)
        {

        }

        private static Syntax ExpandSyntaxDefinition(ConsList input, Binding.Environment cte, BindingStore bs, int phaseLevel)
        {

        }

        private static Syntax InvokeMacroTransformation(Transformer tx, Syntax input, Binding.Environment cte, BindingStore bs, int phaseLevel)
        {

        }

        #region Deconstructive Helpers

        private static bool TryExposePair(Syntax stx, [MaybeNullWhen(false)] out Syntax car, [MaybeNullWhen(false)] out Syntax cdr)
        {
            if (stx is SyntaxList sp
                && sp.WrappedValue is ConsList cl
                && cl.Car is Syntax stxCar
                && cl.Cdr is Syntax stxCdr)
            {
                car = stxCar;
                cdr = stxCdr;
                return true;
            }

            car = null;
            cdr = null;
            return false;
        }

        private static bool TryExposePair<T>(Syntax stx, [MaybeNullWhen(false)] out T car, [MaybeNullWhen(false)] out Syntax cdr)
            where T : Syntax
        {
            if (TryExposePair(stx, out Syntax? stxCar, out cdr)
                && stxCar is T typedCar)
            {
                car = typedCar;
                return true;
            }

            car = null;
            return false;
        }

        private static bool IsSyntacticNil(Syntax stx) => stx is SyntaxAtom sa && sa.WrappedValue is Nil;

        #endregion
    }
}
