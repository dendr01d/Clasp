using ClaspCompiler.Common;
using ClaspCompiler.Data;
using ClaspCompiler.Semantics;
using ClaspCompiler.Syntax;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ParseSemantics
    {
        public static ProgR1 Execute(ISyntax stx)
        {
            ISemExp body = ParseSyntax(stx);
            return new ProgR1("()", body);
        }

        private static ISemExp ParseSyntax(ISyntax stx)
        {
            return stx switch
            {
                StxPair stp => ParseApplication(stp),
                StxDatum std => ParseDatum(std),
                Identifier id => new Var(id.SymbolicName),
                _ => throw new Exception($"Can't parse unknown syntax object: {stx}")
            };
        }

        private static ISemExp ParseApplication(StxPair stp)
        {
            ISemExp op = ParseSyntax(stp.Car);

            if (op is Var v)
            {
                return v.Symbol.Name switch
                {
                    "let" => ParseLet(stp.Cdr),
                    _ => ParseGenericApplication(v, stp.Cdr)
                };
            }

            //fill this in later...

            throw new Exception($"Can't parse application: {stp}");
        }

        private static ISemExp ParseGenericApplication(Var varOp, ISyntax args)
        {
            ISemExp[] pArgs = ParseArgs(args);

            return varOp.Symbol.Name switch
            {
                "read" when pArgs.Length == 0 => new Application(varOp),
                "-" when pArgs.Length == 1 => new Application(varOp, pArgs),
                "+" when pArgs.Length == 2 => new Application(varOp, pArgs),
                _ => throw new Exception($"Can't parse application of '{varOp}' to args: {args}")
            };
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
                return new(new Var(id.SymbolicName), argument);
            }
        }

        private static ISemExp ParseDatum(StxDatum std)
        {
            return std.Value switch
            {
                Integer i => new Literal<Integer>(i, "int"),
                _ => throw new Exception($"Can't parse datum: {std}")
            };
        }

        private static ISemExp[] ParseArgs(ISyntax stx)
        {
            if (stx.IsNil)
            {
                return Array.Empty<ISemExp>();
            }

            if (stx is not StxPair stp)
            {
                throw new Exception($"Can't parse syntax as args: {stx}");
            }

            ISyntax[] rawArgs = stp.ToArray();

            if (!rawArgs[^1].IsNil)
            {
                throw new Exception($"Can't parse dotted list as args: {stp}");
            }

            return rawArgs[..^1].Select(ParseSyntax).ToArray();
        }
    }
}
