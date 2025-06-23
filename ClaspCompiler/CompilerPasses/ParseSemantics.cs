using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ParseSemantics
    {
        public static Prog_Sem Execute(Prog_Stx program)
        {
            ISemTop[] topForms = [.. program.TopLevelForms.Select(ParseTopExpression)];

            return new Prog_Sem(topForms);
        }

        private static ISemTop ParseTopExpression(ISyntax stx)
        {
            if (stx is StxPair stp
                && stp.Car is Identifier id)
            {
                if ()
            }

            return ParseExpression(stx);
        }


        private static ISemExp ParseExpression(ISyntax stx)
        {
            return stx switch
            {
                StxPair stp => ParseCompoundForm(stp),
                StxDatum std => ParseDatum(std),
                Identifier id => new Var(id.BindingSymbol.Name),
                _ => throw new Exception($"Can't parse unknown syntax: {stx}")
            };
        }

        private static ISemExp ParseDatum(StxDatum std)
        {
            return std.Value switch
            {
                IAtom atm => atm,
                _ => throw new Exception($"Cannot parse datum: {std}")
            };
        }

        private static ISemExp ParseCompoundForm(StxPair stp)
        {
            ISemExp op = ParseExpression(stp.Car);

            if (op is Var v)
            {
                return v.Name switch
                {
                    Keyword.LET => ParseLet(stp.Cdr),
                    Keyword.IF => ParseIf(stp.Cdr),
                    _ => ParseGenericApplication(v, stp.Cdr)
                };
            }

            //fill this in later...

            throw new Exception($"Can't (yet) parse compound application form: {stp}");
        }

        private static ISemExp ParseGenericApplication(Var varOp, ISyntax args)
        {
            ISemExp[] pArgs = ParseArgs(args);

            return varOp.Name switch
            {
                Keyword.READ => new SemApp(PrimitiveOperator.Read),

                Keyword.MINUS when pArgs.Length == 1 => new SemApp(PrimitiveOperator.Neg, pArgs[0]),
                Keyword.MINUS when pArgs.Length == 2 => new SemApp(PrimitiveOperator.Sub, pArgs[0]),

                Keyword.PLUS when pArgs.Length == 0 => new Integer(0),
                Keyword.PLUS when pArgs.Length == 1 => pArgs[0],
                Keyword.PLUS when pArgs.Length >= 2 => ParseRecursiveApplication(PrimitiveOperator.Add, pArgs[0], pArgs[1], pArgs[2..]),

                Keyword.EQ => new SemApp(PrimitiveOperator.Eq, pArgs),

                Keyword.LT => new SemApp(PrimitiveOperator.Lt, pArgs),
                Keyword.LTE => new SemApp(PrimitiveOperator.LtE, pArgs),
                Keyword.GT => new SemApp(PrimitiveOperator.Gt, pArgs),
                Keyword.GTE => new SemApp(PrimitiveOperator.GtE, pArgs),

                Keyword.NOT => new SemApp(PrimitiveOperator.Not, pArgs),

                _ => throw new Exception($"Can't parse application of '{varOp}' to args: {args}")
            };
        }

        private static ISemExp[] ParseArgs(ISyntax stx)
        {
            if (stx.IsNil)
            {
                return [];
            }

            if (stx is not StxPair stp)
            {
                throw new Exception($"Cannot parse syntax as arguments: {stx}");
            }

            ISyntax[] preArgs = stp.ToArray();

            if (!preArgs[^1].IsNil)
            {
                throw new Exception($"Cannot parse dotted list as arguments: {stp}");
            }

            return preArgs[..^1].Select(ParseExpression).ToArray();
        }

        private static SemApp ParseRecursiveApplication(PrimitiveOperator op, ISemExp arg1, ISemExp arg2, ISemExp[] moreArgs)
        {
            if (moreArgs.Length > 0)
            {
                return new PrimitiveApplication(op, arg1, ParseRecursiveApplication(op, arg2, moreArgs[0], moreArgs[1..]));
            }
            else
            {
                return new SemApp(op, arg1, arg2);
            }
        }

        private static Let ParseLet(ISyntax args)
        {
            if (args is not StxPair cns
                || cns.Car is not StxPair binding
                || cns.Cdr is not StxPair bodyCns
                || !bodyCns.Cdr.IsNil)
            {
                throw new Exception($"Can't parse args of let form: {args}");
            }
            else
            {
                var pair = ParseLetBinding(binding);
                ISemExp body = ParseExpression(bodyCns.Car);

                return new Let(pair.Item1, pair.Item2, body);
            }
        }

        private static Tuple<Var, ISemExp> ParseLetBinding(StxPair stp)
        {
            if (!stp.Cdr.IsNil
                || stp.Car is not StxPair pr2
                || pr2.Car is not Identifier id
                || pr2.Cdr is not StxPair pr3
                || !pr3.Cdr.IsNil)
            {
                throw new Exception($"Can't parse let binding: {stp}");
            }
            else
            {
                ISemExp argument = ParseExpression(pr3.Car);
                return new(new Var(id.BindingSymbol.Name), argument);
            }
        }

        private static If ParseIf(ISyntax args)
        {
            ISemExp[] pArgs = ParseArgs(args);

            if (pArgs.Length == 2)
            {
                return new If(pArgs[0], pArgs[1], Boole.False);
            }
            else if (pArgs.Length == 3)
            {
                return new If(pArgs[0], pArgs[1], pArgs[2]);
            }
            else
            {
                throw new Exception($"Cannot parse if form with args: {args}");
            }
        }
    }
}
