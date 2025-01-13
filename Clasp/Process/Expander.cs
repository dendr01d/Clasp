using System.Linq;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Process
{
    internal static class Expander
    {
        // credit to https://github.com/mflatt/expander/blob/pico/main.rkt

        public static Syntax Expand(Syntax input, Environment env, BindingStore bs, ScopeTokenGenerator gen)
        {
            ExpansionState exState = new ExpansionState(new EnvFrame(env), bs, 0, gen);
            return Expand(input, exState);
        }

        private static Syntax Expand(Syntax input, ExpansionState exState)
        {
            if (input.TryExposeIdentifier(out Symbol? sym, out string? _))
            {
                return ExpandIdentifier(sym, input, exState);
            }
            else if (input.TryExposeList(out Syntax? car, out Syntax? cdr)
                && car.TryExposeIdentifier(out Symbol? op, out string? _))
            {
                return ExpandIdApplication(op, car, cdr, input, exState);
            }
            else if (input.TryExposeVector(out Syntax[]? values))
            {
                return ExpandVector(values, input, exState);
            }
            else if (input.Expose() is Term t
                && (t is ConsList || t is Nil))
            {
                return ExpandList(input, exState);
            }
            else
            {
                return input;
                //throw new ExpanderException.Uncategorized("Unknown syntax: {0}", input);
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

        private static Syntax ExpandIdentifier(Symbol sym, Syntax input, ExpansionState exState)
        {
            string binding = exState.Store.ResolveBindingName(sym.Name, input.GetContext(exState.Phase), input);

            if (_corePrimitives.Contains(binding))
            {
                return Syntax.Wrap(Symbol.Intern(binding), input);
            }
            //else if (_coreForms.Contains(binding))
            //{
            //    throw new ExpanderException.Uncategorized("Symbol '{0}' erroneously expands to core form.", binding);
            //}
            else if (!exState.Env.ContainsKey(binding))
            {
                throw new ExpanderException.UnboundIdentifier(binding, input);
            }
            else if (exState.IsVariable(binding))
            {
                return Syntax.Wrap(Symbol.Intern(binding), input);
            }
            else
            {
                throw new ExpanderException.InvalidSyntax(input);
                //Term value = exState.Env[binding];
                //throw new ExpanderException.Uncategorized("Cannot expand bound symbol '{0}': {1}", binding, value);
            }
        }

        private static Syntax ExpandIdApplication(Symbol op, Syntax opStx, Syntax argsStx, Syntax input, ExpansionState exState)
        {
            string binding = exState.Store.ResolveBindingName(op.Name, opStx.GetContext(exState.Phase), opStx);

            if (binding == Symbol.Lambda.Name)
            {
                return ExpandLambda(opStx, argsStx, input, exState);
            }
            else if (binding == Symbol.LetSyntax.Name)
            {
                return ExpandLetSyntax(argsStx, input, exState);
            }
            else if (binding == Symbol.Quote.Name
                || binding == Symbol.QuoteSyntax.Name)
            {
                return input;
            }
            else if (exState.TryGetMacro(binding, out MacroProcedure? macro))
            {
                return ApplySyntaxTransformer(macro, input, exState);
            }
            else
            {
                return ExpandList(input, exState);
            }
        }

        private static Syntax ApplySyntaxTransformer(MacroProcedure macro, Syntax input, ExpansionState exState)
        {
            uint introScope = exState.TokenGen.FreshToken();
            input.Paint(exState.Phase, introScope);

            AstNode acceleratedProgram = new MacroApplication(macro, input);
            Term output = Interpreter.Interpret(acceleratedProgram, macro.CapturedEnv);

            if (output is not Syntax stxOutput)
            {
                throw new ExpanderException.ExpectedEvaluation(typeof(Syntax).ToString(), output, input);
            }
            else
            {
                stxOutput.FlipScope(exState.Phase, introScope);
                return stxOutput;
            }
        }

        private static Syntax ExpandLambda(Syntax lambdaId, Syntax args, Syntax input, ExpansionState exState)
        {
            if (!args.TryExposeList(out Syntax? paramList, out Syntax? body))
            {
                throw new ExpanderException.InvalidFormInput(Symbol.Lambda.Name, input);
            }
            else
            {
                uint newScope = exState.TokenGen.FreshToken();
                paramList.Paint(exState.Phase, newScope);
                body.Paint(exState.Phase, newScope);

                ExpansionState innerState = exState.WithExtendedEnv();

                Syntax newParamList = BindLocalVariableList(paramList, innerState);
                Syntax expandedBody = Expand(body, innerState);

                Syntax newArgs = Syntax.Wrap(ConsList.Cons(newParamList, expandedBody), args);
                Syntax output = Syntax.Wrap(ConsList.Cons(lambdaId, newArgs), input);
                return output;
            }
        }

        private static Syntax BindLocalVariableList(Syntax parameters, ExpansionState exState)
        {
            if (parameters.Expose() is Nil)
            {
                return parameters;
            }
            else if (parameters.TryExposeIdentifier(out Symbol? _, out string? dottedParamName))
            {
                return BindLocalVariable(dottedParamName, parameters, exState);
            }
            else if (parameters.TryExposeList(out Syntax? car, out Syntax? cdr)
                && car.TryExposeIdentifier(out Symbol? _, out string? nextParamName))
            {
                Syntax newIdentifier = BindLocalVariable(nextParamName, car, exState);
                Syntax remainingParams = BindLocalVariableList(cdr, exState);

                return Syntax.Wrap(ConsList.Cons(newIdentifier, remainingParams), parameters);
            }
            else
            {
                throw new ExpanderException.InvalidFormInput(Symbol.Lambda.Name, "parameter list", parameters);
            }
        }

        private static Syntax BindLocalVariable(string symbolicName, Syntax input, ExpansionState exState)
        {
            Symbol newSym = new GenSym(symbolicName);
            exState.MarkVariable(newSym.Name);

            Syntax newIdentifier = Syntax.Wrap(newSym, input);
            exState.Store.BindName(symbolicName, newIdentifier.GetContext(exState.Phase), newSym.Name);

            return newIdentifier;
        }

        private static Syntax ExpandLetSyntax(Syntax args, Syntax input, ExpansionState exState)
        {
            if (!args.TryExposeList(out Syntax? letList, out Syntax? body))
            {
                throw new ExpanderException.InvalidFormInput(Symbol.LetSyntax.Name, input);
            }
            else
            {
                uint newScope = exState.TokenGen.FreshToken();
                letList.Paint(exState.Phase, newScope);
                body.Paint(exState.Phase, newScope);

                ExpansionState innerState = exState.WithExtendedEnv();

                ExpandLetBindingList(letList, innerState);

                // the point of this form is to install temporary macros over the body
                // we only care about returning the expanded body itself, however
                return Expand(body, innerState);
            }
        }

        private static void ExpandLetBindingList(Syntax letList, ExpansionState exState)
        {
            if (letList.Expose() is Nil)
            {
                return;
            }
            else if (letList.TryExposeList(out Syntax? nextBinding, out Syntax? remainingBindings))
            {
                ExpandLetBinding(nextBinding, exState);
                ExpandLetBindingList(remainingBindings, exState);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(letList);
            }
        }

        private static void ExpandLetBinding(Syntax bindingPair, ExpansionState exState)
        {
            if (bindingPair.TryExposeList(out Syntax? car, out Syntax? cdr)
                && car.TryExposeIdentifier(out Symbol? _, out string? lhs)
                && cdr.TryExposeList(out Syntax? rhs, out Syntax? terminator)
                && terminator.Expose() is Nil)
            {
                MacroProcedure macro = ParseAndEvalMacro(rhs, exState);
                BindLocalMacro(lhs, car, macro, exState);
            }
            else
            {
                throw new ExpanderException.InvalidFormInput(Symbol.LetSyntax.Name, "binding pair", bindingPair);
            }
        }

        private static MacroProcedure ParseAndEvalMacro(Syntax input, ExpansionState exState)
        {
            ExpansionState nextPhaseState = exState.WithNextPhase();

            Syntax expandedInput = Expand(input, nextPhaseState);
            AstNode parsedInput = Parser.ParseAST(expandedInput, nextPhaseState.Store, nextPhaseState.Phase);
            Term output = Interpreter.Interpret(parsedInput, StandardEnv.CreateNew());

            if (output is not MacroProcedure macro)
            {
                throw new ExpanderException.ExpectedEvaluation(typeof(MacroProcedure).ToString(), output, input);
            }
            else
            {
                return macro;
            }
        }

        private static void BindLocalMacro(string symbolicName, Syntax input, MacroProcedure value, ExpansionState exState)
        {
            Symbol newSym = new GenSym(symbolicName);
            exState.BindMacro(newSym.Name, value);

            Syntax newIdentifier = Syntax.Wrap(newSym, input);
            exState.Store.BindName(symbolicName, newIdentifier.GetContext(exState.Phase), newSym.Name);
        }

        private static Syntax ExpandVector(Syntax[] values, Syntax input, ExpansionState exState)
        {
            Syntax[] expandedValues = values.Select(x => Expand(x, exState)).ToArray();
            return Syntax.Wrap(new Vector(expandedValues), input);
        }

        private static Syntax ExpandList(Syntax input, ExpansionState exState)
        {
            if (input.Expose() is Nil)
            {
                return input;
            }
            else if (input.TryExposeList(out Syntax? car, out Syntax? cdr))
            {
                Syntax newCar = Expand(car, exState);
                Syntax newCdr = Expand(cdr, exState);

                return Syntax.Wrap(ConsList.Cons(newCar, newCdr), input);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(input);
            }
        }
    }
}
