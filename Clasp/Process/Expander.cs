using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Process
{
    internal static class Expander
    {
        // certain core forms are represented implicitly
        // e.g. how a list is assumed to be a function application
        // tagging each core form makes parsing a lot easier

        //public const string MARK_APP = "#app";
        //public const string MARK_PARAMS = "#params";
        //public const string MARK_DEREF = "#var";

        // or maybe that's too much. lol


        // credit to https://github.com/mflatt/expander/blob/pico/main.rkt

        public static Syntax Expand(Syntax input, Environment env, BindingStore bs, ScopeTokenGenerator gen)
        {
            ExpansionState exState = new ExpansionState(new EnvFrame(env), bs, 0, gen);
            return Expand(input, exState);
        }

        private static Syntax Expand(Syntax stx, ExpansionState exState)
        {
            if (stx is Syntax<Symbol> identifier)
            {
                return ExpandIdentifier(identifier, exState);
            }
            else if (TryExposeList(stx, out Syntax<Symbol>? stxOp, out Syntax<ConsList>? stxArgs))
            {
                return ExpandIdApplication(stx, stxOp, stxArgs, exState);
            }
            else if (stx is Syntax<Vector> stxVec)
            {
                return ExpandVector(stxVec, exState);
            }
            else if (stx.Expose is ConsList or Nil)
            {
                return ExpandList(stx, exState);
            }
            else
            {
                throw new ExpanderException.InvalidSyntax(stx);
            }
        }

        private static readonly string[] _coreForms = new string[]
        {
            Symbol.Lambda.Name,
            Symbol.LetSyntax.Name, // has to be a core form because how else would we define it? macros? lol
            Symbol.Quote.Name,
            Symbol.QuoteSyntax.Name
        };

        private static readonly string[] _corePrimitives = new string[]
        {
            "datum->syntax",
            "syntax->datum",
            "syntax-e",
            "list",
            "cons",
            "car",
            "cdr",
            "map"
        };

        private static Syntax ExpandIdentifier(Syntax<Symbol> stx, ExpansionState exState)
        {
            string bindingName = exState.ResolveBindingName(stx);
            // TODO: need another step here where the name is dereferenced in the env
            // to see if the name is bound to the core form in question
            // (on case of shadowing)
            
            if (_corePrimitives.Contains(bindingName))
            {
                return Syntax.Wrap(Symbol.Intern(bindingName), stx);
            }
            //else if (_coreForms.Contains(binding))
            //{
            //    throw new ExpanderException.Uncategorized("Symbol '{0}' erroneously expands to core form.", binding);
            //}
            else if (!exState.Env.ContainsKey(bindingName))
            {
                throw new ExpanderException.UnboundIdentifier(bindingName, stx);
            }
            else if (exState.IsVariable(bindingName))
            {
                return Syntax.Wrap(Symbol.Intern(bindingName), stx);
            }
            else
            {
                throw new ExpanderException.InvalidSyntax(stx);
                //Term value = exState.Env[binding];
                //throw new ExpanderException.Uncategorized("Cannot expand bound symbol '{0}': {1}", binding, value);
            }
        }

        private static Syntax ExpandIdApplication(Syntax stx, Syntax<Symbol> stxOp, Syntax<ConsList> stxArgs, ExpansionState exState)
        {
            string bindingName = exState.ResolveBindingName(stxOp);

            if (bindingName == Symbol.Lambda.Name)
            {
                return ExpandLambda(stx, stxOp, stxArgs, exState);
            }
            else if (bindingName == Symbol.LetSyntax.Name)
            {
                return ExpandLetSyntax(stx, stxArgs, exState);
            }
            else if (bindingName == Symbol.Quote.Name
                || bindingName == Symbol.QuoteSyntax.Name)
            {
                return stx;
            }
            else if (exState.TryGetMacro(bindingName, out MacroProcedure? macro))
            {
                return ApplySyntaxTransformer(macro, stx, exState);
            }
            else
            {
                return ExpandList(stx, exState);
            }
        }

        private static Syntax ApplySyntaxTransformer(MacroProcedure macro, Syntax input, ExpansionState exState)
        {
            uint introScope = exState.TokenGen.FreshToken();
            exState.PaintScope(input, introScope);

            CoreForm acceleratedProgram = new MacroApplication(macro, input);
            Term output = Interpreter.Interpret(acceleratedProgram, macro.CapturedEnv);

            if (output is not Syntax stxOutput)
            {
                throw new ExpanderException.ExpectedEvaluation(typeof(Syntax).ToString(), output, input);
            }

            //TODO: do I need a use-site scope as well? I can't remember

            exState.FlipScope(stxOutput, introScope);
            return stxOutput;
        }

        private static Syntax ExpandLambda(Syntax stx, Syntax<Symbol> stxOp, Syntax<ConsList> stxArgs, ExpansionState exState)
        {
            if (stxArgs is Syntax<ConsList> stxPair
                && stxPair.Expose.Car is Syntax stxParams
                && stxPair.Expose.Cdr is Syntax<ConsList> stxBody)
            {
                uint newScope = exState.TokenGen.FreshToken();
                Syntax.PaintScope(stxParams, exState.Phase, newScope);
                Syntax.PaintScope(stxArgs, exState.Phase, newScope);

                ExpansionState subState = exState.WithExtendedEnv();

                Syntax expandedParams = ExpandParameterList(stxParams, subState);
                Syntax expandedBody = ExpandList(stxArgs, subState);

                Syntax expandedArgs = Syntax.Wrap(ConsList.Cons(expandedParams, expandedBody), stxArgs);
                Syntax expandedStx = Syntax.Wrap(ConsList.Cons(stxOp, expandedArgs), stx);
                return expandedStx;
            }

            throw new ExpanderException.InvalidFormInput(Symbol.Lambda.Name, stx);
        }

        private static Syntax ExpandParameterList(Syntax stx, ExpansionState exState)
        {
            if (stx.Expose is Nil)
            {
                return stx;
            }
            else if (stx is Syntax<Symbol> identifier)
            {
                return BindLocalVariable(identifier, exState);
            }
            else if (TryExposeList(stx, out Syntax<Symbol>? stxCar, out Syntax? stxCdr))
            {
                Syntax newIdentifier = BindLocalVariable(stxCar, exState);
                Syntax expandedTail = ExpandParameterList(stxCdr, exState);

                return Syntax.Wrap(ConsList.Cons(newIdentifier, expandedTail), stx);
            }
            else
            {
                throw new ExpanderException.InvalidFormInput(Symbol.Lambda.Name, "parameter list", stx);
            }
        }

        private static Syntax BindLocalVariable(Syntax<Symbol> stx, ExpansionState exState)
        {
            Symbol newSym = new GenSym(stx.Expose.Name);
            exState.MarkVariable(newSym.Name);

            Syntax newIdentifier = Syntax.Wrap(newSym, stx);
            exState.RenameInCurrentScope(newIdentifier, newSym.Name);

            return newIdentifier;
        }

        private static Syntax ExpandLetSyntax(Syntax stx, Syntax<ConsList> stxArgs, ExpansionState exState)
        {
            if (TryExposeList(stxArgs, out Syntax? stxLetBindings, out Syntax<ConsList>? stxBody))
            {
                uint newScope = exState.TokenGen.FreshToken();
                exState.PaintScope(stxLetBindings, newScope);
                exState.FlipScope(stxBody, newScope);

                ExpansionState subState = exState.WithExtendedEnv();

                ExpandLetBindingList(stxLetBindings, subState);

                // the point of this form is to install temporary macros over the body
                // we only care about returning the expanded body itself, however
                return Expand(stxBody, subState);
            }

            throw new ExpanderException.InvalidFormInput(Symbol.LetSyntax.Name, stx);
        }

        private static void ExpandLetBindingList(Syntax stx, ExpansionState exState)
        {
            if (stx.Expose is Nil)
            {
                return;
            }
            else if (TryExposeList(stx, out Syntax<ConsList>? stxCar, out Syntax? stxCdr))
            {
                ExpandLetBinding(stxCar, exState);
                ExpandLetBindingList(stxCdr, exState);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }

        private static void ExpandLetBinding(Syntax<ConsList> stx, ExpansionState exState)
        {
            if (TryExposeList(stx, out Syntax<Symbol>? identifier, out Syntax<ConsList>? stxRhs)
                && TryExposeList(stx, out Syntax? stxValue, out Syntax<Nil>? _))
            {
                MacroProcedure macro = ParseAndEvalMacro(stxRhs, exState);
                BindLocalMacro(identifier, macro, exState);
            }
            else
            {
                throw new ExpanderException.InvalidFormInput(Symbol.LetSyntax.Name, "binding pair", stx);
            }
        }

        private static MacroProcedure ParseAndEvalMacro(Syntax input, ExpansionState exState)
        {
            ExpansionState nextPhaseState = exState.WithNextPhase();

            Syntax expandedInput = Expand(input, nextPhaseState);
            CoreForm parsedInput = Parser.Parse(expandedInput, nextPhaseState.Store, nextPhaseState.Phase);
            Term output = Interpreter.Interpret(parsedInput, StandardEnv.CreateNew());

            if (output is MacroProcedure macro)
            {
                return macro;
            }

            throw new ExpanderException.ExpectedEvaluation(typeof(MacroProcedure).ToString(), output, input);
        }

        private static void BindLocalMacro(Syntax<Symbol> identifier, MacroProcedure value, ExpansionState exState)
        {
            Symbol newSym = new GenSym(identifier.Expose.Name);
            exState.BindMacro(newSym.Name, value);

            Syntax newIdentifier = Syntax.Wrap(newSym, identifier);
            exState.RenameInCurrentScope(newIdentifier, newSym.Name);
        }

        private static Syntax ExpandVector(Syntax<Vector> stx, ExpansionState exState)
        {
            if (TryExposeVector(stx, out IEnumerable<Syntax>? stxValues))
            {
                Syntax[] expandedValues = stxValues.Select(x => Expand(x, exState)).ToArray();
                return Syntax.Wrap(new Vector(expandedValues), stx);
            }

            throw new ExpanderException.InvalidFormInput(typeof(Vector).Name.ToString(), stx);
        }

        private static Syntax ExpandList(Syntax stx, ExpansionState exState)
        {
            if (stx.Expose is Nil)
            {
                return stx;
            }
            else if (TryExposeList(stx, out Syntax? car, out Syntax? cdr))
            {
                Syntax expandedCar = Expand(car, exState);
                Syntax expandedCdr = Expand(cdr, exState);

                return Syntax.Wrap(ConsList.Cons(expandedCar, expandedCdr), stx);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(stx);
            }
        }

        #region Helpers

        private static bool TryExposeList<T1, T2>(Syntax stx,
            [NotNullWhen(true)] out T1? car,
            [NotNullWhen(true)] out T2? cdr)
            where T1 : Syntax
            where T2 : Syntax
        {
            if (stx is Syntax<ConsList> stxCons
                && stxCons.Expose.Car is T1 stxCar
                && stxCons.Expose.Cdr is T2 stxCdr)
            {
                car = stxCar;
                cdr = stxCdr;
                return true;
            }

            car = null;
            cdr = null;
            return false;
        }

        private static bool TryExposeVector(Syntax stx,
            [NotNullWhen(true)] out IEnumerable<Syntax>? values)
        {
            if (stx is Syntax<Vector> stxVec)
            {
                List<Syntax> stxValues = new List<Syntax>();

                foreach(Term t in stxVec.Expose.Values)
                {
                    if (t is Syntax stxValue)
                    {
                        stxValues.Add(stxValue);
                    }
                    else
                    {
                        throw new ExpanderException.InvalidFormInput(typeof(Vector).Name.ToString(), stx);
                    }
                }

                values = stxValues;
                return true;
            }

            values = null;
            return false;
        }

        #endregion
    }
}
