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
            ISemExp body = ParseSyntax(program.Body);
            return new Prog_Sem(body);
        }

        private static ISemExp ParseSyntax(ISyntax stx)
        {
            return stx switch
            {
                StxPair stp => ParseApplication(stp),
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

        private static ISemExp ParseApplication(StxPair stp)
        {
            ISemExp op = ParseSyntax(stp.Car);

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
                Keyword.READ => new PrimitiveApplication(PrimitiveOperator.Read),

                Keyword.MINUS when pArgs.Length == 1 => new PrimitiveApplication(PrimitiveOperator.Neg, pArgs[0]),
                Keyword.MINUS when pArgs.Length == 2 => new PrimitiveApplication(PrimitiveOperator.Sub, pArgs[0]),

                Keyword.PLUS when pArgs.Length == 0 => new Integer(0),
                Keyword.PLUS when pArgs.Length == 1 => pArgs[0],
                Keyword.PLUS when pArgs.Length >= 2 => ParseRecursiveApplication(PrimitiveOperator.Add, pArgs[0], pArgs[1], pArgs[2..]),

                Keyword.EQ => new PrimitiveApplication(PrimitiveOperator.Eq, pArgs),

                Keyword.LT => new PrimitiveApplication(PrimitiveOperator.Lt, pArgs),
                Keyword.LTE => new PrimitiveApplication(PrimitiveOperator.LtE, pArgs),
                Keyword.GT => new PrimitiveApplication(PrimitiveOperator.Gt, pArgs),
                Keyword.GTE => new PrimitiveApplication(PrimitiveOperator.GtE, pArgs),

                Keyword.NOT => new PrimitiveApplication(PrimitiveOperator.Not, pArgs),

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

            return preArgs[..^1].Select(ParseSyntax).ToArray();
        }

        private static PrimitiveApplication ParseRecursiveApplication(PrimitiveOperator op, ISemExp arg1, ISemExp arg2, ISemExp[] moreArgs)
        {
            if (moreArgs.Length > 0)
            {
                return new PrimitiveApplication(op, arg1, ParseRecursiveApplication(op, arg2, moreArgs[0], moreArgs[1..]));
            }
            else
            {
                return new PrimitiveApplication(op, arg1, arg2);
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
                ISemExp body = ParseSyntax(bodyCns.Car);

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
                ISemExp argument = ParseSyntax(pr3.Car);
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
