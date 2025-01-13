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
            ExpansionParameters exParams = new ExpansionParameters(new ExpansionEnv(env), bs, 0, gen);
            return Expand(input, exParams);
        }

        private static Syntax Expand(Syntax input, ExpansionParameters exParams)
        {
            if (input.TryExposeIdentifier(out Symbol? sym, out string? _))
            {
                return ExpandIdentifier(sym, input, exParams);
            }
            else if (input.TryExposeList(out Syntax? car, out Syntax? cdr)
                && car.TryExposeIdentifier(out Symbol? op, out string? _))
            {
                return ExpandIdApplication(op, car, cdr, input, exParams);
            }
            else if (input.Expose() is Term t
                && (t is ConsList || t is Nil))
            {
                return ExpandList(input, exParams);
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

        private static Syntax ExpandIdentifier(Symbol sym, Syntax input, ExpansionParameters exParams)
        {
            string binding = exParams.Store.ResolveBindingName(sym.Name, input.GetContext(exParams.Phase), input);

            if (_corePrimitives.Contains(binding))
            {
                return Syntax.Wrap(Symbol.Intern(binding), input);
            }
            //else if (_coreForms.Contains(binding))
            //{
            //    throw new ExpanderException.Uncategorized("Symbol '{0}' erroneously expands to core form.", binding);
            //}
            else if (!exParams.Env.ContainsKey(binding))
            {
                throw new ExpanderException.UnboundIdentifier(binding, input);
            }
            else if (exParams.Env.IsVariable(binding))
            {
                return Syntax.Wrap(Symbol.Intern(binding), input);
            }
            else
            {
                throw new ExpanderException.InvalidSyntax(input);
                //Term value = exParams.Env[binding];
                //throw new ExpanderException.Uncategorized("Cannot expand bound symbol '{0}': {1}", binding, value);
            }
        }

        private static Syntax ExpandIdApplication(Symbol op, Syntax opStx, Syntax argsStx, Syntax input, ExpansionParameters exParams)
        {
            string binding = exParams.Store.ResolveBindingName(op.Name, opStx.GetContext(exParams.Phase), opStx);

            if (binding == Symbol.Lambda.Name)
            {
                return ExpandLambda(opStx, argsStx, input, exParams);
            }
            else if (binding == Symbol.LetSyntax.Name)
            {
                return ExpandLetSyntax(argsStx, input, exParams);
            }
            else if (binding == Symbol.Quote.Name
                || binding == Symbol.QuoteSyntax.Name)
            {
                return input;
            }
            else if (exParams.Env.TryGetMacro(binding, out MacroProcedure? macro))
            {
                return ApplySyntaxTransformer(macro, input, exParams);
            }
            else
            {
                return ExpandList(input, exParams);
            }
        }

        private static Syntax ApplySyntaxTransformer(MacroProcedure macro, Syntax input, ExpansionParameters exParams)
        {
            uint introScope = exParams.TokenGen.FreshToken();
            input.Paint(exParams.Phase, introScope);

            AstNode acceleratedProgram = new MacroApplication(macro, input);
            Term output = Interpreter.Interpret(acceleratedProgram, macro.CapturedEnv);

            if (output is not Syntax stxOutput)
            {
                throw new ExpanderException.ExpectedEvaluation(typeof(Syntax).ToString(), output, input);
            }
            else
            {
                stxOutput.FlipScope(exParams.Phase, introScope);
                return stxOutput;
            }
        }

        private static Syntax ExpandLambda(Syntax lambdaId, Syntax args, Syntax input, ExpansionParameters exParams)
        {
            if (!args.TryExposeList(out Syntax? paramList, out Syntax? body))
            {
                throw new ExpanderException.InvalidFormInput(Symbol.Lambda.Name, input);
            }
            else
            {
                uint newScope = exParams.TokenGen.FreshToken();
                paramList.Paint(exParams.Phase, newScope);
                body.Paint(exParams.Phase, newScope);

                ExpansionParameters innerParams = exParams.WithExtendedEnv();

                Syntax newParamList = BindLocalVariableList(paramList, innerParams);
                Syntax expandedBody = Expand(body, innerParams);

                Syntax newArgs = Syntax.Wrap(ConsList.Cons(newParamList, expandedBody), args);
                Syntax output = Syntax.Wrap(ConsList.Cons(lambdaId, newArgs), input);
                return output;
            }
        }

        private static Syntax BindLocalVariableList(Syntax parameters, ExpansionParameters exParams)
        {
            if (parameters.Expose() is Nil)
            {
                return parameters;
            }
            else if (parameters.TryExposeIdentifier(out Symbol? _, out string? dottedParamName))
            {
                return BindLocalVariable(dottedParamName, parameters, exParams);
            }
            else if (parameters.TryExposeList(out Syntax? car, out Syntax? cdr)
                && car.TryExposeIdentifier(out Symbol? _, out string? nextParamName))
            {
                Syntax newIdentifier = BindLocalVariable(nextParamName, car, exParams);
                Syntax remainingParams = BindLocalVariableList(cdr, exParams);

                return Syntax.Wrap(ConsList.Cons(newIdentifier, remainingParams), parameters);
            }
            else
            {
                throw new ExpanderException.InvalidFormInput(Symbol.Lambda.Name, "parameter list", parameters);
            }
        }

        private static Syntax BindLocalVariable(string symbolicName, Syntax input, ExpansionParameters exParams)
        {
            Symbol newSym = new GenSym(symbolicName);
            exParams.Env.MarkVariable(newSym.Name);

            Syntax newIdentifier = Syntax.Wrap(newSym, input);
            exParams.Store.BindName(symbolicName, newIdentifier.GetContext(exParams.Phase), newSym.Name);

            return newIdentifier;
        }

        private static Syntax ExpandLetSyntax(Syntax args, Syntax input, ExpansionParameters exParams)
        {
            if (!args.TryExposeList(out Syntax? letList, out Syntax? body))
            {
                throw new ExpanderException.InvalidFormInput(Symbol.LetSyntax.Name, input);
            }
            else
            {
                uint newScope = exParams.TokenGen.FreshToken();
                letList.Paint(exParams.Phase, newScope);
                body.Paint(exParams.Phase, newScope);

                ExpansionParameters innerParams = exParams.WithExtendedEnv();

                ExpandLetBindingList(letList, innerParams);

                // the point of this form is to install temporary macros over the body
                // we only care about returning the expanded body itself, however
                return Expand(body, innerParams);
            }
        }

        private static void ExpandLetBindingList(Syntax letList, ExpansionParameters exParams)
        {
            if (letList.Expose() is Nil)
            {
                return;
            }
            else if (letList.TryExposeList(out Syntax? nextBinding, out Syntax? remainingBindings))
            {
                ExpandLetBinding(nextBinding, exParams);
                ExpandLetBindingList(remainingBindings, exParams);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(letList);
            }
        }

        private static void ExpandLetBinding(Syntax bindingPair, ExpansionParameters exParams)
        {
            if (bindingPair.TryExposeList(out Syntax? car, out Syntax? cdr)
                && car.TryExposeIdentifier(out Symbol? _, out string? lhs)
                && cdr.TryExposeList(out Syntax? rhs, out Syntax? terminator)
                && terminator.Expose() is Nil)
            {
                MacroProcedure macro = ParseAndEvalMacro(rhs, exParams);
                BindLocalMacro(lhs, car, macro, exParams);
            }
            else
            {
                throw new ExpanderException.InvalidFormInput(Symbol.LetSyntax.Name, "binding pair", bindingPair);
            }
        }

        private static MacroProcedure ParseAndEvalMacro(Syntax input, ExpansionParameters exParams)
        {
            ExpansionParameters newParams = exParams.WithNextPhase();

            Syntax expandedInput = Expand(input, newParams);
            AstNode parsedInput = Parser.ParseAST(expandedInput, newParams.Store, newParams.Phase);
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

        private static void BindLocalMacro(string symbolicName, Syntax input, MacroProcedure value, ExpansionParameters exParams)
        {
            Symbol newSym = new GenSym(symbolicName);
            exParams.Env.BindMacro(newSym.Name, value);

            Syntax newIdentifier = Syntax.Wrap(newSym, input);
            exParams.Store.BindName(symbolicName, newIdentifier.GetContext(exParams.Phase), newSym.Name);
        }

        private static Syntax ExpandList(Syntax input, ExpansionParameters exParams)
        {
            if (input.Expose() is Nil)
            {
                return input;
            }
            else if (input.TryExposeList(out Syntax? car, out Syntax? cdr))
            {
                Syntax newCar = Expand(car, exParams);
                Syntax newCdr = Expand(cdr, exParams);

                return Syntax.Wrap(ConsList.Cons(newCar, newCdr), input);
            }
            else
            {
                throw new ExpanderException.ExpectedProperList(input);
            }
        }
    }
}
