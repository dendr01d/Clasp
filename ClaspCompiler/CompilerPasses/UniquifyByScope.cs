using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeSyntax.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class UniquifyByScope
    {
        public static Prog_Stx Execute(Prog_Stx program)
        {
            ScopeSetMap map = ScopeSetMap.BuildDefault();
            ISyntax newBody = UniquifyExpression(program.TopLevelForms, map);

            return new Prog_Stx(map, newBody);
        }

        private static ISyntax UniquifyExpression(ISyntax stx, ScopeSetMap map)
        {
            return stx switch
            {
                Identifier id => UniquifyLooseIdentifier(id, map),
                StxDatum => stx,
                StxPair stp => UniquifyCompoundForm(stp, map),
                _ => throw new Exception($"Can't paint unknown syntax: {stx}"),
            };
        }

        private static Identifier UniquifyLooseIdentifier(Identifier id, ScopeSetMap map)
        {
            if (map.TryLookupMapping(id, out Symbol? bindingSym))
            {
                return new Identifier(id.Source, id.ScopeSet)
                {
                    FreeSymbol = id.FreeSymbol,
                    BindingSymbol = bindingSym
                };
            }
            else
            {
                throw new Exception($"Can't paint unidentified loose identifier: {id}");
            }
        }

        private static Identifier UniquifyBindingIdentifier(Identifier id, ScopeSetMap map)
        {
            Symbol bindingSym = Symbol.GenSym(id.FreeSymbol.Name);
            map.AddMapping(id.ScopeSet, id.FreeSymbol, bindingSym);

            return new Identifier(id.Source, id.ScopeSet)
            {
                FreeSymbol = id.FreeSymbol,
                BindingSymbol = bindingSym
            };
        }

        private static StxPair UniquifyCompoundForm(StxPair stp, ScopeSetMap map)
        {
            ISyntax op = UniquifyExpression(stp.Car, map);

            if (op is Identifier id)
            {
                if (id.BindingSymbol.Name == Keyword.LET)
                {
                    return new StxPair(stp.Source, stp.ScopeSet)
                    {
                        Car = op,
                        Cdr = UniquifyLetArgs(stp.Cdr, map)
                    };
                }
            }

            return new StxPair(stp.Source, stp.ScopeSet)
            {
                Car = op,
                Cdr = UniquifyArgs(stp.Cdr, map)
            };
        }

        private static ISyntax UniquifyArgs(ISyntax stx, ScopeSetMap map)
        {
            if (stx is StxPair stp)
            {
                return new StxPair(stp.Source, stp.ScopeSet)
                {
                    Car = UniquifyExpression(stp.Car, map),
                    Cdr = UniquifyArgs(stp.Cdr, map)
                };
            }
            else if (stx.IsNil)
            {
                return stx;
            }
            else
            {
                throw new Exception($"Syntax error: Malformed argument list: {stx}");
            }
        }

        private static StxPair UniquifyLetArgs(ISyntax args, ScopeSetMap map)
        {
            if (args.TryDestruct(out ISyntax? bindingList, out StxPair? cdr)
                && cdr.TryDestructLast(out ISyntax? body))
            {
                return new StxPair(args.Source, args.ScopeSet)
                {
                    Car = UniquifyBindingList(bindingList, map),
                    Cdr = new StxPair(cdr.Source, cdr.ScopeSet)
                    {
                        Car = UniquifyExpression(body, map),
                        Cdr = cdr.Cdr
                    }
                };
            }
            else
            {
                throw new Exception($"Syntax error: Malformed let args: {args}");
            }
        }

        private static ISyntax UniquifyBindingList(ISyntax bindingList, ScopeSetMap map)
        {
            if (bindingList.IsNil)
            {
                return bindingList;
            }
            else if (bindingList.TryDestruct(out StxPair? firstHalf, out ISyntax? tail)
                && firstHalf.TryDestruct(out Identifier? id, out StxPair? secondHalf)
                && secondHalf.TryDestructLast(out ISyntax? val))
            {
                return new StxPair(bindingList.Source, bindingList.ScopeSet)
                {
                    Car = new StxPair(firstHalf.Source, firstHalf.ScopeSet)
                    {
                        Car = UniquifyBindingIdentifier(id, map),
                        Cdr = new StxPair(secondHalf.Source, secondHalf.ScopeSet)
                        {
                            Car = UniquifyExpression(val, map),
                            Cdr = secondHalf.Cdr
                        }
                    },
                    Cdr = UniquifyBindingList(tail, map)
                };
            }
            else
            {
                throw new Exception($"Syntax error: Malformed let binding: {bindingList}");
            }
        }
    }
}
