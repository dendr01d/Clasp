using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.Text;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ParseSemantics
    {
        private sealed record Context
        {
            private uint _idCounter = 0;

            public Dictionary<uint, SourceRef> SourceLookup { get; } = [];
            public Dictionary<Symbol, SemVar> VarLookup { get; } = [];

            public uint GetFreshId() => _idCounter++;
        }

        public static Prog_Sem Execute(Prog_Stx program)
        {
            Context ctx = new();
            ISemAstNode prog = ParseAstNode(program.Body, ctx);

            if (prog is not Body body)
            {
                throw new Exception($"Parsed semantic program content as non-body expression: {prog}");
            }

            return new Prog_Sem(body)
            {
                VariablePool = [.. ctx.VarLookup.Values],
                SourceLookup = ctx.SourceLookup
            };
        }

        private static Body ParseBody(ISyntax stx, Context ctx)
        {
            if (stx is not StxPair stp)
            {
                throw new Exception($"Can't parse syntax as form body: {stx}");
            }

            ISemAstNode[] nodeList = [.. ParseAstNodeSeries(stp, ctx)];

            ISemDef[] defs = [.. nodeList.OfType<ISemDef>()];
            ISemCmd[] exprs = [.. nodeList.OfType<ISemCmd>()];

            if (exprs.Length == 0 || exprs[^1] is not ISemExp finalExp)
            {
                throw new Exception($"Expected form body to conclude with expression: {exprs[^1]}");
            }

            return MakeBody(defs, exprs[..^1], finalExp, stx.Source, ctx);
        }

        private static ISemAstNode ParseAstNode(ISyntax stx, Context ctx)
        {
            return stx switch
            {
                StxPair stp => ParseCompoundForm(stp, ctx),
                Identifier id => ParseFreeIdentifier(id, ctx),
                StxDatum std => ParseDatum(std.Datum, std.Source, ctx),
                _ => throw new Exception($"Can't parse unknown syntax form: {stx}")
            };
        }
        private static IEnumerable<ISemAstNode> ParseAstNodeSeries(StxPair stp, Context ctx)
        {
            return stp.SkipLast(1).Select(x => ParseAstNode(x, ctx));
        }

        private static ISemExp ParseExpression(ISyntax stx, Context ctx)
        {
            ISemAstNode node = ParseAstNode(stx, ctx);

            if (node is ISemExp exp)
            {
                return exp;
            }
            else
            {
                throw new Exception($"Expected to parse expression: {node}");
            }
        }
        private static IEnumerable<ISemExp> ParseExpressionSeries(StxPair stp, Context ctx)
        {
            return stp.SkipLast(1).Select(x => ParseExpression(x, ctx));
        }

        private static SemVar ParseFreeIdentifier(Identifier id, Context ctx)
        {
            if (ctx.VarLookup.TryGetValue(id.ExpandedSymbol, out SemVar? extantVar))
            {
                return extantVar;
            }
            else
            {
                // if it wasn't bound in the context of the program, and the expander didn't complain about it
                // it must be a special keyword or primitive operator

                SemVar output = MakeVariable(id.ExpandedSymbol, SourceRef.DefaultSyntax, ctx);
                ctx.VarLookup[id.ExpandedSymbol] = output;
                return output;
            }

            throw new Exception($"Tried to parse unknown identifier as bound variable: {id}");
        }

        private static SemVar ParseBindingIdentifier(Identifier id, Context ctx)
        {
            if (ctx.VarLookup.TryGetValue(id.ExpandedSymbol, out SemVar? extantVar))
            {
                throw new Exception($"Tried to re-parse binding variable that already exists: {extantVar} <--> {id}");
            }
            else
            {
                SemVar output = MakeVariable(id.ExpandedSymbol, id.Source, ctx);
                ctx.VarLookup[id.ExpandedSymbol] = output;
                return output;
            }
        }

        private static ISemExp ParseDatum(ISchemeExp exp, SourceRef src, Context ctx)
        {
            if (exp is IValue val)
            {
                return MakeLiteral(val, src, ctx);
            }
            else
            {
                return MakeQuotation(exp, src, ctx);
            }
        }

        private static ISemAstNode ParseCompoundForm(StxPair stp, Context ctx)
        {
            ISemAstNode opNode = ParseAstNode(stp.Car, ctx);

            if (opNode is SemVar opVar
                && SpecialKeyword.IsKeyword(opVar.Name))
            {
                if (opVar.Name == SpecialKeyword.Apply.Name)
                {
                    if (stp.Cdr is StxPair applyArgs)
                    {
                        ISemAstNode subOp = ParseAstNode(applyArgs.Car, ctx);
                        return ParseAppForm(subOp, applyArgs.Cdr, stp.Source, ctx);
                    }
                    else
                    {
                        throw new Exception($"Can't parse explicit application form: {stp}");
                    }
                }
                else if (opVar.Name == SpecialKeyword.SetBang.Name) return ParseAssignment(stp.Cdr, ctx);
                else if (opVar.Name == SpecialKeyword.Begin.Name) return ParseBody(stp.Cdr, ctx);
                else if (opVar.Name == SpecialKeyword.Define.Name) return ParseDefinition(stp.Cdr, ctx);
                else if (opVar.Name == SpecialKeyword.If.Name) return ParseConditional(stp.Cdr, ctx);
                else if (opVar.Name == SpecialKeyword.Lambda.Name) return ParseLambda(stp.Cdr, ctx);
                else if (opVar.Name == SpecialKeyword.Quote.Name) return ParseDatum(StripSyntax(stp.Cdr), stp.Source, ctx);
                else
                {
                    throw new NotImplementedException($"Parser doesn't know how to parse special form: {stp}");
                }
            }
            else
            {
                return ParseAppForm(opNode, stp.Cdr, stp.Source, ctx);
            }
        }

        private static Application ParseAppForm(ISemAstNode opNode, ISyntax args, SourceRef src, Context ctx)
        {
            if (opNode is not ISemExp op)
            {
                throw new Exception($"Expected expression in operator position of application form: {opNode}");
            }

            IEnumerable<ISemExp> parsedArgs = args switch
            {
                StxPair stp => ParseExpressionSeries(stp, ctx),
                _ when args.IsNil => [],
                _ => throw new Exception($"Expected argument list in application: {args}")
            };

            return MakeApplication(op, parsedArgs.OfType<ISemExp>(), src, ctx);
        }

        private static Definition ParseDefinition(ISyntax stx, Context ctx)
        {
            if (stx.TryDestruct(out Identifier? vari, out StxPair? tail)
                && tail.Cdr.IsNil)
            {
                SemVar defVar = ParseBindingIdentifier(vari, ctx);
                ISemExp defValue = ParseExpression(tail.Car, ctx);

                return MakeDefinition(defVar, defValue, stx.Source, ctx);
            }
            throw new Exception($"Can't parse operands of {SpecialKeyword.Define.Name} special form: {stx}");
        }

        private static Assignment ParseAssignment(ISyntax stx, Context ctx)
        {
            if (stx.TryDestruct(out Identifier? vari, out StxPair? tail)
                && tail.Cdr.IsNil)
            {
                SemVar defVar = ParseFreeIdentifier(vari, ctx);
                ISemExp defValue = ParseExpression(tail.Car, ctx);

                return MakeAssignment(defVar, defValue, stx.Source, ctx);
            }
            throw new Exception($"Can't parse operands of {SpecialKeyword.SetBang.Name} special form: {stx}");
        }

        private static Lambda ParseLambda(ISyntax stx, Context ctx)
        {
            if (stx.TryDestruct(out ISyntax? formalsTerm, out ISyntax? tail))
            {
                Formals formals = ParseFormals(formalsTerm, ctx);
                Body body = ParseBody(tail, ctx);

                return MakeLambda(formals, body, stx.Source, ctx);
            }
            throw new Exception($"Can't parse operands of {SpecialKeyword.Lambda.Name} special form: {stx}");
        }

        private static Formals ParseFormals(ISyntax stx, Context ctx)
        {
            static Formals ParseFormalsHelper(ISyntax _stx, Context _ctx, List<ISemVar> _acc)
            {
                if (_stx.IsNil)
                {
                    return new Formals([.. _acc], null);
                }
                else if (_stx is Identifier varPar)
                {
                    return new Formals([.. _acc], ParseBindingIdentifier(varPar, _ctx));
                }
                else if (_stx.TryDestruct(out Identifier? next, out ISyntax? rest))
                {
                    _acc.Add(ParseBindingIdentifier(next, _ctx));
                    return ParseFormalsHelper(rest, _ctx, _acc);
                }
                else
                {
                    throw new Exception($"Expected variable list in formals term of {SpecialKeyword.Lambda.Name} form: {_stx}");
                }
            }

            return ParseFormalsHelper(stx, ctx, []);
        }

        private static ISemExp ParseConditional(ISyntax args, Context ctx)
        {
            if (args is not StxPair stp)
            {
                throw new Exception($"Expected (at least) condition argument to {SpecialKeyword.If.Name} form: {args}");
            }

            ISemExp[] pArgs = [.. ParseExpressionSeries(stp, ctx)];

            if (pArgs.Length == 1)
            {
                return pArgs[0];
            }
            else if (pArgs.Length == 2)
            {
                Literal implicitAlt = MakeLiteral(Boole.False, args.Source, ctx);
                return MakeConditional(pArgs[0], pArgs[1], implicitAlt, args.Source, ctx);
            }
            else if (pArgs.Length == 3)
            {
                return MakeConditional(pArgs[0], pArgs[1], pArgs[2], args.Source, ctx);
            }
            else
            {
                throw new Exception($"Cannot parse arguments to {SpecialKeyword.If.Name} form: {args}");
            }
        }

        private static ISchemeExp StripSyntax(ISyntax stx)
        {
            if (stx is StxDatum std)
            {
                return std.Datum;
            }
            else if (stx is Identifier id)
            {
                return id.ExpandedSymbol;
            }
            else if (stx is StxPair stp)
            {
                return new Cons(StripSyntax(stp.Car), StripSyntax(stp.Cdr));
            }
            throw new Exception($"Can't strip unknown syntax form: {stx}");
        }


        #region Semantic Factorization

        private static uint IndexNewSource(SourceRef src, Context ctx)
        {
            uint newId = ctx.GetFreshId();
            ctx.SourceLookup[newId] = src;
            return newId;
        }

        private static Application MakeApplication(ISemExp op, IEnumerable<ISemExp> args, SourceRef src, Context ctx)
            => new(op, [.. args], IndexNewSource(src, ctx));

        private static Assignment MakeAssignment(ISemVar vari, ISemExp value, SourceRef src, Context ctx)
            => new(vari, value, IndexNewSource(src, ctx));

        private static Body MakeBody(IEnumerable<ISemDef> defs, IEnumerable<ISemCmd> cmds, ISemExp val, SourceRef src, Context ctx)
            => new([.. defs], [.. cmds], val, IndexNewSource(src, ctx));

        private static Conditional MakeConditional(ISemExp cond, ISemExp consq, ISemExp alt, SourceRef src, Context ctx)
            => new(cond, consq, alt, IndexNewSource(src, ctx));

        private static Definition MakeDefinition(ISemVar vari, ISemExp value, SourceRef src, Context ctx)
            => new(vari, value, IndexNewSource(src, ctx));

        private static Lambda MakeLambda(ISemFormals formals, ISemBody body, SourceRef src, Context ctx)
            => new(formals, body, IndexNewSource(src, ctx));

        private static Literal MakeLiteral(IValue value, SourceRef src, Context ctx)
            => new(value, IndexNewSource(src, ctx));

        private static Quotation MakeQuotation(ISchemeExp exp, SourceRef src, Context ctx)
            => new(exp, IndexNewSource(src, ctx));

        private static SemVar MakeVariable(Symbol sym, SourceRef src, Context ctx)
            => new(sym.Name, IndexNewSource(src, ctx));

        #endregion
    }
}