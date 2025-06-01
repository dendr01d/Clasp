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
        public static ProgR1 Execute(ProgS1 program)
        {
            ISemanticExp body = ParseSyntax(program.Body);
            return new ProgR1("()", body);
        }

        private static ISemanticExp ParseSyntax(ISyntax stx)
        {
            return stx switch
            {
                StxPair stp => ParseApplication(stp),
                StxDatum std => ParseDatum(std),
                Identifier id => new Var(id.BindingSymbol.Name),
                _ => throw new Exception($"Can't parse unknown syntax object: {stx}")
            };
        }

        private static ISemanticExp ParseApplication(StxPair stp)
        {
            ISemanticExp op = ParseSyntax(stp.Car);

            if (op is Var v)
            {
                return v.Name switch
                {
                    "let" => ParseLet(stp.Cdr),
                    _ => ParseGenericApplication(v, stp.Cdr)
                };
            }

            //fill this in later...

            throw new Exception($"Can't parse application: {stp}");
        }

        private static ISemanticExp ParseGenericApplication(Var varOp, ISyntax args)
        {
            ISemanticExp[] pArgs = ParseArgs(args);

            return varOp.Name switch
            {
                "read" when pArgs.Length == 0 => new Application("read"),
                "-" when pArgs.Length == 1 => new Application("-", pArgs[0]),
                "+" when pArgs.Length == 2 => new Application("+", pArgs[0], pArgs[1]),
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
                ISemanticExp body = ParseSyntax(bodyCns.Car);

                return new Let(pair.Item1, pair.Item2, body);
            }
        }

        private static Tuple<Var, ISemanticExp> ParseLetBinding(StxPair stp)
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
                ISemanticExp argument = ParseSyntax(pr3.Car);
                return new(new Var(id.BindingSymbol.Name), argument);
            }
        }

        private static ISemanticExp ParseDatum(StxDatum std)
        {
            return std.Value;
        }

        private static ISemanticExp[] ParseArgs(ISyntax stx)
        {
            if (stx.IsNil)
            {
                return Array.Empty<ISemanticExp>();
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
