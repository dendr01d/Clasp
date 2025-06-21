using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeSyntax.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class PaintLexicalScopes
    {
        public static Prog_Stx Execute(Prog_Stx program)
        {
            uint scopeCounter = 0;
            ISyntax newBody = program.TopLevelForms.AddScopes(scopeCounter++);

            newBody = PaintExpression(newBody, ref scopeCounter);

            return new Prog_Stx(new(), newBody);
        }

        private static ISyntax PaintExpression(ISyntax stx, ref uint scopeCounter)
        {
            return stx is StxPair stp
                ? PaintCompoundForm(stp, ref scopeCounter)
                : stx;
        }

        private static StxPair PaintCompoundForm(StxPair stp, ref uint scopeCounter)
        {
            ISyntax op = PaintExpression(stp.Car, ref scopeCounter);

            if (op is Identifier id)
            {
                ISyntax args = id.BindingSymbol.Name switch
                {
                    Keyword.LET => PaintLetArgs(stp.Cdr, ref scopeCounter),
                    Keyword.IF => PaintIfArgs(stp.Cdr, ref scopeCounter),
                    _ => PaintArgs(stp.Cdr, ref scopeCounter)
                };

                return new StxPair(stp.Source, stp.ScopeSet)
                {
                    Car = id,
                    Cdr = args
                };
            }
            else
            {
                throw new Exception($"Can't (yet) paint complex application form: {stp}");
            }
        }

        private static ISyntax PaintArgs(ISyntax args, ref uint scopeCounter)
        {
            if (args is StxPair stp)
            {
                return new StxPair(stp.Source, stp.ScopeSet)
                {
                    Car = PaintExpression(stp.Car, ref scopeCounter),
                    Cdr = PaintArgs(stp.Cdr, ref scopeCounter)
                };
            }
            else if (args.IsNil)
            {
                return args;
            }
            else
            {
                throw new Exception($"Syntax error: Malformed argument list: {args}");
            }
        }

        private static ISyntax PaintIfArgs(ISyntax args, ref uint scopeCounter) => PaintArgs(args, ref scopeCounter);

        private static StxPair PaintLetArgs(ISyntax args, ref uint scopeCounter)
        {
            if (args.TryDestruct(out ISyntax? bindingList, out StxPair? cdr)
                && cdr.TryDestructLast(out ISyntax? body))
            {
                ISyntax paintedBindings = PaintBindingList(bindingList, ref body, ref scopeCounter);
                ISyntax paintedBody = PaintExpression(body, ref scopeCounter);

                return new StxPair(args.Source, args.ScopeSet)
                {
                    Car = paintedBindings,
                    Cdr = new StxPair(cdr.Source, cdr.ScopeSet)
                    {
                        Car = paintedBody,
                        Cdr = cdr.Cdr
                    }
                };
            }
            else
            {
                throw new Exception($"Syntax error: Malformed let args: {args}");
            }
        }

        private static ISyntax PaintBindingList(ISyntax bindingList, ref ISyntax body, ref uint scopeCounter)
        {
            if (bindingList.IsNil)
            {
                return bindingList;
            }
            else if (bindingList.TryDestruct(out StxPair? firstHalf, out ISyntax? tail)
                && firstHalf.TryDestruct(out Identifier? id, out StxPair? secondHalf)
                && secondHalf.TryDestructLast(out ISyntax? value))
            {
                uint newScope = scopeCounter++;
                Identifier paintedId = id.AddScopes(newScope);
                body = body.AddScopes(newScope);

                ISyntax paintedValue = PaintExpression(value, ref scopeCounter);

                return new StxPair(bindingList.Source, bindingList.ScopeSet)
                {
                    Car = new StxPair(firstHalf.Source, firstHalf.ScopeSet)
                    {
                        Car = paintedId,
                        Cdr = new StxPair(secondHalf.Source, secondHalf.ScopeSet)
                        {
                            Car = paintedValue,
                            Cdr = secondHalf.Cdr
                        }
                    },
                    Cdr = PaintBindingList(tail, ref body, ref scopeCounter)
                };
            }
            else
            {
                throw new Exception($"Syntax error: Malformed let binding: {bindingList}");
            }
        }
    }
}
