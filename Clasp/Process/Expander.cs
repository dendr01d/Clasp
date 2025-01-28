using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Binding.Scopes;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Product;
using Clasp.Data.Terms.Syntax;

namespace Clasp.Process
{
    /*
     
     I've been treating this as a single thing, when really it's doing a lot of things in tandem
    To wit:
    - identify each lexical scope described by the syntax and assign it a unique ID
    - associate each syntax term in the program with the set of scopes it's located within
    - rename all identifiers in the program such that there are no shadowed bindings
        (record the renamings by mapping (symbolic name, scope set) pairs to binding names)
    - perform rudimentary type analysis on identifiers to see if they're being used as
        variable names, macro invocations, or shadowings of core forms
    - tag implicit forms (function application, datum, top?, var?) with core operators
    - accelerate and bind as a compile-time value any macros defined
    - identify and invoke any macro application forms

    - macro invocations get wrapped in new scopes before and after executing
        in order to distinguish the varying stages at which new bindings are conceptually performed

    - internal definition contexts must keep track of the special scopes from that last point
        and remove them from identifiers that end up in binding positions --
        i.e. they need to have the same scope as if they HADN'T resulted from a macro

    - for the sake of efficiency, renamings should be discarded once they're no longer accessible
        (effected by treating the bindingstore essentially as a secondary environment?)

    - syntax objects need to support adding, flipping, and removal of scopes
        and these operations ideally need to be lazily recursive on their substructures
     
     */



    internal static class Expander
    {
        // certain core forms are represented implicitly
        // e.g. how a list is assumed to be a function application
        // tagging each core form makes parsing a lot easier

        public const string IMPLICIT_APP = "#%app";
        public const string IMPLICIT_DATUM = "#%datum";
        public const string IMPLICIT_TOP = "#%top";
        //public const string MARK_PARAMS = "#%params";
        //public const string MARK_DEREF = "#var";

        // or maybe that's too much. lol


        // credit to https://github.com/mflatt/expander/blob/pico/main.rkt

        public static Syntax ExpandSyntax(Syntax input, Environment env, ScopeTokenGenerator gen)
        {
            ExpansionContext exState = ExpansionContext.FreshExpansion(env, gen);
            return Expand(input, exState);
        }

        private static Syntax Expand(Syntax stx, ExpansionContext exState)
        {
            if (stx is Identifier id)
            {
                return ExpandIdentifier(id, exState);
            }
            else if (stx is SyntaxPair stp
                && stp.Car is Identifier idOp)
            {
                return ExpandIdApplication(idOp, stp.Cdr, exState);
            }
            //else if (stx.Exposee is Vector)
            //{
            //    return ExpandVector(stx, exState);
            //}
            else if (stx.Expose() is ConsList or Nil)
            {
                return ExpandImplicit(Symbol.ImplicitApp, stx, exState);
            }
            else
            {
                return ExpandImplicit(Symbol.ImplicitDatum, stx, exState);
            }
        }

        private static readonly Term[] _specialForms = new Term[]
        {
            Symbol.Lambda,
            Symbol.LetSyntax, // has to be a core form because how else would we define it? macros? lol
            Symbol.Quote,
            Symbol.QuoteSyntax
        };

        //private static readonly string[] _corePrimitives = new string[]
        //{
        //    "datum->syntax",
        //    "syntax->datum",
        //    "syntax-e",
        //    "list",
        //    "cons",
        //    "car",
        //    "cdr",
        //    "map"
        //};

        private static Syntax ExpandIdentifier(Identifier id, ExpansionContext exState)
        {
            if (exState.TryResolveBinding(id, out ExpansionBinding? binding))
            {
                return ExpandBoundIdentifier(binding, exState);
            }
            else
            {
                return ExpandImplicit(Symbol.ImplicitTop, id, exState);
            }
        }

        private static Syntax ExpandIdApplication(Identifier op, Syntax args, ExpansionContext exState)
        {
            if (exState.TryResolveBinding(op, out ExpansionBinding? binding))
            {
                if (binding.BoundType == BindingType.Variable)
                {
                    return ExpandImplicit(Symbol.ImplicitApp, binding.BindingIdentifier, args, exState);
                }
                else
                {
                    
                }
            }


            if (exState.TryResolveBindingName(stx, idName, out string? bindingName))
            {
                Term deref = exState.CurrentEnv.LookUp(bindingName);

                if (deref is Syntax stxVar
                    && stxVar.TryExposeIdentifier(out string? name)
                    && exState.IsVariable(name))
                {
                    return ExpandImplicit(Symbol.ImplicitApp, stx, exState);
                }
                else
                {
                    return ExpandBoundIdentifier(stx, bindingName, exState);
                }
            }
            else
            {
                return ExpandImplicit(Symbol.ImplicitApp, stx, exState);
            }
        }

        private static Syntax ExpandBoundIdentifier(Syntax stx, string bindingName, ExpansionContext exState)
        {
            Term deref = exState.CurrentEnv.LookUp(bindingName);

            if (_specialForms.Contains(deref))
            {
                if (exState.RestrictedToImmediate)
                {
                    return stx;
                }
                else
                {
                    // ?
                    // https://github.com/mflatt/expander/blob/demi/expand.rkt#L102
                }
            }
            else if (deref is MacroProcedure macro)
            {
                return ApplySyntaxTransformer(macro, stx, exState);
            }
            else if (deref is Syntax stxVar
                && stxVar.TryExposeIdentifier(out string? name)
                && exState.IsVariable(name))
            {
                return stxVar;
            }
            else
            {
                throw new ExpanderException.InvalidSyntax(stx);
            }
        }

        // https://github.com/mflatt/expander/blob/demi/expand.rkt#L75
        private static Syntax ExpandImplicit(Symbol formSym, Syntax stx, ExpansionContext exState)
        {
            Syntax formId = Syntax.Wrap(formSym, stx);

            if (stx._wrapped is Nil)
            {
                return Syntax.Wrap(formId, stx);
            }
            else if (stx.TryExposeList(out Syntax? car, out Syntax? cdr))
            {
                Syntax expandedCar = Expand(car, exState);
                Syntax expandedCdr = Expand(cdr, exState);
                Syntax expandedList = Syntax.Wrap(expandedCar, expandedCdr, stx);

                return Syntax.Wrap(formId, stx, stx);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }



        private static Syntax ApplySyntaxTransformer(MacroProcedure macro, Syntax input, ExpansionContext exState)
        {
            uint introScope = exState.TokenGen.FreshToken();
            exState.PaintScope(input, introScope);

            CoreForm acceleratedProgram = new MacroApplication(macro, input);
            Term output = Interpreter.InterpretProgram(acceleratedProgram, macro.CapturedEnv);

            if (output is not Syntax stxOutput)
            {
                throw new ExpanderException.ExpectedEvaluation(typeof(Syntax).Name.ToString(), output, input);
            }

            //TODO: do I need a use-site scope as well? I can't remember

            exState.FlipScope(stxOutput, introScope);
            return stxOutput;
        }

        private static Syntax ExpandLambda(Syntax stx, Syntax stxOp, Syntax stxArgs, ExpansionContext exState)
        {
            if (stxArgs.TryExposeList(out Syntax? stxParams, out Syntax? stxBody))
            {
                uint newScope = exState.TokenGen.FreshToken();
                Syntax.PaintScope(stxParams, exState.Phase, newScope);
                Syntax.PaintScope(stxBody, exState.Phase, newScope);

                ExpansionContext subState = exState.WithSubEnv();

                Syntax expandedParams = ExpandParameterList(stxParams, subState);
                Syntax expandedBody = ExpandImplicit(stxBody, subState);

                Syntax expandedArgs = Syntax.Wrap(ConsList.Cons(expandedParams, expandedBody), stxArgs);
                Syntax expandedStx = Syntax.Wrap(ConsList.Cons(stxOp, expandedArgs), stx);
                return expandedStx;
            }

            throw new ExpanderException.InvalidFormInput(Symbol.Lambda.Name, stx);
        }

        private static Syntax ExpandParameterList(Syntax stx, ExpansionContext exState)
        {
            if (stx._wrapped is Nil)
            {
                return stx;
            }
            else if (stx.TryExposeIdentifier(out string? idName))
            {
                return RenameLocalVariable(stx, idName, exState);
            }
            else if (stx.TryExposeList(out Syntax? stxCar, out Syntax? stxCdr)
                && stxCar.TryExposeIdentifier(out string? firstIdName))
            {
                Syntax newIdentifier = RenameLocalVariable(stxCar, firstIdName, exState);
                Syntax expandedTail = ExpandParameterList(stxCdr, exState);

                return Syntax.Wrap(ConsList.Cons(newIdentifier, expandedTail), stx);
            }
            else
            {
                throw new ExpanderException.InvalidFormInput(Symbol.Lambda.Name, "parameter list", stx);
            }
        }

        private static Syntax RenameLocalVariable(Syntax stx, string idName, ExpansionContext exState)
        {
            Symbol newSym = new GenSym(idName);
            exState.MarkVariable(newSym.Name);

            Syntax newIdentifier = Syntax.Wrap(newSym, stx);
            exState.RenameInCurrentScope(newIdentifier, idName, newSym.Name);

            return newIdentifier;
        }

        private static Syntax ExpandLetSyntax(Syntax stx, Syntax stxArgs, ExpansionContext exState)
        {
            if (stxArgs.TryExposeList(out Syntax? stxLetBindings, out Syntax? stxBody))
            {
                uint newScope = exState.TokenGen.FreshToken();
                exState.PaintScope(stxLetBindings, newScope);
                exState.FlipScope(stxBody, newScope);

                ExpansionContext subState = exState.WithSubEnv();

                ExpandLetBindingList(stxLetBindings, subState);

                // the point of this form is to install temporary macros over the body
                // we only care about returning the expanded body itself, however
                return Expand(stxBody, subState);
            }

            throw new ExpanderException.InvalidFormInput(Symbol.LetSyntax.Name, stx);
        }

        private static void ExpandLetBindingList(Syntax stx, ExpansionContext exState)
        {
            if (stx._wrapped is Nil)
            {
                return;
            }
            else if (stx.TryExposeList(out Syntax? stxCar, out Syntax? stxCdr))
            {
                ExpandLetBinding(stxCar, exState);
                ExpandLetBindingList(stxCdr, exState);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }

        private static void ExpandLetBinding(Syntax stx, ExpansionContext exState)
        {
            if (stx.TryExposeList(out Syntax? stxLhs, out Syntax? stxRhs)
                && stxLhs.TryExposeIdentifier(out string? idName)
                && stxRhs.TryExposeList(out Syntax? stxValue, out Syntax? _))
            {
                MacroProcedure macro = SyntaxAndEvalMacro(stxValue, exState);
                BindLocalMacro(stxLhs, idName, macro, exState);
            }
            else
            {
                throw new ExpanderException.InvalidFormInput(Symbol.LetSyntax.Name, "binding pair", stx);
            }
        }

        private static MacroProcedure SyntaxAndEvalMacro(Syntax input, ExpansionContext exState)
        {
            ExpansionContext nextPhaseState = exState.WithNextPhase();

            Syntax expandedInput = Expand(input, nextPhaseState);
            CoreForm SyntaxdInput = Parser.ParseSyntax(expandedInput, nextPhaseState.CurrentBlock, nextPhaseState.Phase);
            Term output = Interpreter.InterpretProgram(SyntaxdInput, StandardEnv.CreateNew());

            if (output is MacroProcedure macro)
            {
                return macro;
            }

            throw new ExpanderException.ExpectedEvaluation(typeof(MacroProcedure).ToString(), output, input);
        }

        private static void BindLocalMacro(Syntax identifier, string idName, MacroProcedure value, ExpansionContext exState)
        {
            Symbol newSym = new GenSym(idName);
            exState.BindMacro(newSym.Name, value);

            Syntax newIdentifier = Syntax.Wrap(newSym, identifier);
            exState.RenameInCurrentScope(newIdentifier, idName, newSym.Name);
        }

        //private static Syntax ExpandVector(Syntax stx, ExpansionState exState)
        //{
        //    if (stx.TryExposeVector(out Vector? _, out Term[]? values))
        //    {
                
        //    }
        //    else
        //    {
        //        throw new ExpanderException.InvalidFormInput(typeof(Vector).Name.ToString(), stx);
        //    }

        //    if (stx.TryExposeVector(out Vector? _, out IEnumerable<Syntax>? stxValues))
        //    {
        //        Syntax[] expandedValues = stxValues.Select(x => Expand(x, exState)).ToArray();
        //        return Syntax.Wrap(new Vector(expandedValues), stx);
        //    }

        //}

    }
}
