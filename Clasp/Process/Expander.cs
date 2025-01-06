using System.Collections.Generic;
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
        public static Syntax Expand(Syntax input, EnvFrame env)
        {
            //MetaBinder mb = new MetaBinder(input, env);

            //return Expand(input, mb, ExpansionContext.TopLevel);

            return input;
        }

        //private static Syntax Expand(Syntax input, MetaBinder mb, ExpansionContext context)
        //{
        //    if (TryExposeApplicative(input, out SyntaxList? list, out Identifier? opId, out Syntax? args)
        //        && mb.ResolveBoundValue(opId) is Term op)
        //    {
        //        if (op == Symbol.Quote)
        //        {
        //            return input;
        //        }
        //        else if (op == Symbol.Syntax)
        //        {
        //            return input;
        //        }
        //        else if (op == Symbol.Lambda)
        //        {
        //            SyntaxList expandedArgs = ExpandLambdaArgs(args, mb, context);
        //            return new SyntaxList(opId, expandedArgs, list);
        //        }
        //        else if (op == Symbol.Define)
        //        {
        //            if (context == ExpansionContext.Expression) throw new ExpanderException.InvalidContext(opId, context);

        //            SyntaxList expandedArgs = ExpandDefinitionArgs(args, mb, context);
        //            return new SyntaxList(opId, expandedArgs, list);
        //        }
        //        else if (op == Symbol.DefineSyntax)
        //        {
        //            if (context == ExpansionContext.Expression) throw new ExpanderException.InvalidContext(opId, context);

        //            SyntaxList expandedArgs = ExpandStxDefinitionArgs(args, mb, context);
        //            return new SyntaxList(opId, expandedArgs, list);
        //        }
        //        else if (op is MacroProcedure tx)
        //        {
        //            Syntax transformedSyntax = InvokeMacroTransformation(list, tx, mb, context);
        //            return Expand(transformedSyntax, mb, context);
        //        }
        //    }
        //    //else if (TryExposeStxList(input, out SyntaxList? cons, out Syntax? car, out Syntax? cdr))
        //    //{
        //    //    Syntax newCar = Expand(car, mb, context);
        //    //    Syntax newCdr = Expand(cdr, mb, context);

        //    //    return (newCar == car && newCdr == cdr)
        //    //        ? cons
        //    //        : new SyntaxList(newCdr, newCdr, cons);
        //    //}
        //    else if (input is Identifier id
        //        && mb.ResolveBoundValue(id) is Term deref)
        //    {
        //        if (deref is MacroProcedure tx)
        //        {
        //            Syntax transformedSyntax = InvokeMacroTransformation(id, tx, mb, context);
        //        }
        //        //else if (deref is Syntax newStx)
        //        //{
        //        //    return newStx;
        //        //}
        //    }

        //    throw new ExpanderException.UnknownForm(input);
        //}

        //private static SyntaxList ExpandLambdaArgs(Syntax args, MetaBinder mb, ExpansionContext context)
        //{
        //    MetaBinder subScope = mb.ExtendScope(args);

        //    if (TryExposeStxList(args, out SyntaxList? cons, out Syntax? car, out Syntax? cdr))
        //    {
        //        Syntax parameters = ExpandParameters(car, subScope, ExpansionContext.Expression);
        //        Syntax body = ExpandSequence(cdr, subScope, ExpansionContext.InternalDefinition);

        //        return new SyntaxList(parameters, body, cons);
        //    }

        //    throw new ExpanderException.InvalidFormShape(Symbol.Lambda, args);
        //}

        //private static Syntax ExpandParameters(Syntax paramList, MetaBinder mb, ExpansionContext context)
        //{
        //    if (paramList is SyntaxAtom sa && sa.WrappedValue is Nil)
        //    {
        //        return paramList;
        //    }
        //    else if (paramList is Identifier id)
        //    {
        //        return mb.CreateFreshBinding(id);
        //    }
        //    // not really an applicative, but "a list where the car is an ID" is the same thing we want here
        //    else if (TryExposeApplicative(paramList, out SyntaxList? cons, out Identifier? first, out Syntax? rest))
        //    {
        //        Identifier newParam = mb.CreateFreshBinding(first);
        //        Syntax moreParams = ExpandParameters(rest, mb, context);
        //        return new SyntaxList(newParam, moreParams, cons);
        //    }

        //    throw new ExpanderException.InvalidFormShape("parameter list", paramList);
        //}

        //private static Syntax ExpandSequence(Syntax body, MetaBinder mb, ExpansionContext context)
        //{
        //    if (TryExposeStxList(body, out SyntaxList? cons, out Syntax? car, out Syntax? cdr))
        //    {
        //        // if the car is a definition, do that first, THEN recur
        //        // otherwise recur first and hit the car on the way "back up" the recursion
        //        // in this way, all definitions will be expanded with their bindings first, then everything else

        //        if (TryExposeApplicative(car, out SyntaxList? _, out Identifier? op, out Syntax? _)
        //            && (mb.ResolveBoundValue(op) == Symbol.Define
        //             || mb.ResolveBoundValue(op) == Symbol.DefineSyntax))
        //        {
        //            Syntax first = Expand(car, mb, context);

        //            if (cdr is SyntaxAtom sa && sa.WrappedValue is Nil)
        //            {
        //                return new SyntaxList(first, sa, cons);
        //            }
        //            else if (cdr is not SyntaxAtom)
        //            {
        //                Syntax rest = ExpandSequence(cdr, mb, context);
        //                return new SyntaxList(first, rest, cons);
        //            }
        //        }
        //        else if (cdr is SyntaxAtom sa && sa.WrappedValue is Nil)
        //        {
        //            Syntax first = Expand(car, mb, context);
        //            return new SyntaxList(first, sa, cons);
        //        }
        //        else if (cdr is not SyntaxAtom)
        //        {
        //            Syntax rest = ExpandSequence(cdr, mb, context);
        //            Syntax first = Expand(car, mb, context);
        //            return new SyntaxList(first, rest, cons);
        //        }
        //    }

        //    throw new ExpanderException.InvalidFormShape(Symbol.Begin, body);
        //}

        //private static SyntaxList ExpandDefinitionArgs(Syntax args, MetaBinder mb, ExpansionContext context)
        //{
        //    if (TryExposeDefinitionArgs(args, out Identifier? key, out Syntax? value))
        //    {
        //        Identifier newParam = mb.CreateFreshBinding(key);
        //        return new SyntaxList(newParam, value, args);
        //    }

        //    throw new ExpanderException.InvalidFormShape(Symbol.Define, args);
        //}

        //private static SyntaxList ExpandStxDefinitionArgs(Syntax args, MetaBinder mb, ExpansionContext context)
        //{
        //    if (TryExposeDefinitionArgs(args, out Identifier? key, out Syntax? value))
        //    {
        //        MetaBinder nextPhase = mb.EnterSubExpansion(value);
        //        Syntax expandedValue = Expand(value, nextPhase, ExpansionContext.Expression);
        //        MacroProcedure tx = ExpiditeTransformerCreation(expandedValue, nextPhase);

        //        Identifier newTxId = mb.CreateFreshTransformerBinding(key, tx);

        //        return new SyntaxList(newTxId, value, args);
        //    }

        //    throw new ExpanderException.InvalidFormShape(Symbol.DefineSyntax, args);
        //}

        //private static MacroProcedure ExpiditeTransformerCreation(Syntax stx, MetaBinder mb)
        //{
        //    AstNode parsedTx = Parser.ParseAST(stx);
        //    Term evaluated = Evaluator.Evaluate(parsedTx);

        //    if (evaluated is MacroProcedure output)
        //    {
        //        return output;
        //    }

        //    throw new ExpanderException.Uncategorized("Tried to expand/parse/eval presumed macro, but got incompatible output: {0}", evaluated);
        //}

        //private static Syntax InvokeMacroTransformation(Syntax input, MacroProcedure tx, MetaBinder mb, ExpansionContext context)
        //{
        //    //something about applying a macro definition and/or use scope
        //    //then flipping one of them?
        //    //idfk
        //}

        //// -----------------
        //// Helpers

        //private static bool TryExposeStxList(Syntax stx,
        //    [NotNullWhen(true)] out SyntaxList? cons,
        //    [NotNullWhen(true)] out Syntax? car,
        //    [NotNullWhen(true)] out Syntax? cdr)
        //{
        //    return Syntax.TryExposeSyntaxList(stx, out cons, out car, out cdr);
        //}

        //private static bool TryExposeApplicative(Syntax stx,
        //    [NotNullWhen(true)] out SyntaxList? cons,
        //    [NotNullWhen(true)] out Identifier? carId,
        //    [NotNullWhen(true)] out Syntax? cdr)
        //{
        //    if (Syntax.TryExposeSyntaxList(stx, out cons, out Syntax? car, out cdr)
        //        && car is Identifier id)
        //    {
        //        carId = id;
        //        return true;
        //    }
        //    carId = null;
        //    return false;
        //}

        //private static bool TryExposeDefinitionArgs(Syntax stx,
        //    [NotNullWhen(true)] out Identifier? key,
        //    [NotNullWhen(true)] out Syntax? value)
        //{
        //    if (TryExposeStxList(stx, out SyntaxList? cons, out Syntax? car, out Syntax? cdr))
        //    {
        //        if (car is Identifier keyId)
        //        {
        //            key = keyId;
        //            value = cdr;
        //            return true;
        //        }
        //        else if (TryExposeApplicative(car, out SyntaxList? _, out key, out Syntax? formals))
        //        {
        //            SyntaxList lambdaArgs = new SyntaxList(formals, cdr, cons);
        //            Identifier lambdaId = new Identifier(Symbol.Lambda, cons);
        //            value = new SyntaxList(lambdaId, lambdaArgs, cons);

        //            return true;
        //        }
        //    }

        //    key = null;
        //    value = null;
        //    return false;
        //}
    }
}
