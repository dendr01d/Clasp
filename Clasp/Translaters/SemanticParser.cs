using System;
using System.Collections.Generic;

using Clasp.InterLangs.SemanticScheme;
using Clasp.InterLangs.SyntacticScheme;

namespace Clasp.Translaters
{
    internal sealed class SemanticParser : Translater<Expr, Form>
    {
        public override Form Translate(Expr input)
        {
            if (input is Cons cns)
            {
                if (cns.Car is Symbol sym)
                {
                    return sym.Value switch
                    {
                        "lambda" => TranslateLambda(cns.Cdr),
                        "begin" => TranslateSequence(cns.Cdr),
                        "if" => TranslateConditional(cns.Cdr),
                        "quote" => TranslateQuote(cns.Cdr),
                        _ => TranslateApplication(sym, cns.Cdr)
                    };
                }

                throw new Exception($"Cannot semantically parse application of operator '{cns.Car}'.");
            }
            else if (input is Symbol sym)
            {
                return new Var(sym);
            }
            else
            {
                return new Val(input);
            }
        }

        private Lambda TranslateLambda(Expr input)
        {
            if (input is Cons lsArgs
                && lsArgs.Cdr is Cons lsBody)
            {
                var args = TranslateParams(lsArgs.Car);
                Sequence body = TranslateSequence(lsBody);

                return new Lambda(args.Item1, args.Item2, body);
            }

            throw new Exception($"Couldn't parse args of lambda form: {input}");
        }

        private (Arg[], Arg?) TranslateParams(Expr input)
        {
            List<Arg> args = [];
            Arg? varArg = null;

            Expr temp = input;
            while (temp is Cons cns && cns.Car is Symbol nextParam)
            {
                args.Add(new Arg(nextParam));
            }

            if (temp is Symbol finalParam)
            {
                varArg = new Arg(finalParam);
            }
            else if (temp is Nil)
            {
                return new(args.ToArray(), varArg);
            }

            throw new Exception($"Failed to parse parameters of lambda form: {input}");
        }

        private Sequence TranslateSequence(Expr input)
        {
            List<Form> sequents = [];
            Expr temp = input;

            while (temp is Cons cns)
            {
                sequents.Add(Translate(cns.Car));
                temp = cns.Cdr;
            }

            if (temp is not Nil)
            {
                throw new Exception($"Expected args of sequence to be nil-terminated: {input}");
            }

            return new Sequence(sequents.ToArray());
        }

        private Conditional TranslateConditional(Expr input)
        {
            if (input is Cons lsCond)
            {
                Form cond = Translate(lsCond.Car);

                if (lsCond.Cdr is Cons lsConsq)
                {
                    Form consq = Translate(lsConsq.Car);

                    if (lsConsq.Cdr is Cons lsAlt)
                    {
                        Form alt = Translate(lsAlt.Car);

                        if (lsAlt.Cdr is Nil)
                        {
                            return new Conditional(cond, consq, alt);
                        }
                    }
                    else if (lsConsq.Cdr is Nil)
                    {
                        return new Conditional(cond, consq, new Val(InterLangs.SyntacticScheme.Boolean.False));
                    }
                }
            }

            throw new Exception($"Couldn't parse args of conditional: {input}");
        }

        private Quote TranslateQuote(Expr input)
        {
            return new Quote(input);
        }

        private Application TranslateApplication(Symbol op, Expr args)
        {
            List<Form> sequents = [];
            Expr temp = args;

            while (temp is Cons cns)
            {
                sequents.Add(Translate(cns.Car));
                temp = cns.Cdr;
            }

            if (temp is not Nil)
            {
                throw new Exception($"Expected args of application to be nil-terminated: {args}");
            }

            return new Application(op, sequents[1..].ToArray());
        }
    }
}
