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
        public static Prog_Sem Execute(Prog_Stx program)
        {
            Dictionary<Symbol, ISemVar> varMap = [];

            Body bod = ParseBody(program.TopLevelForms, varMap);

            return new Prog_Sem(bod);
        }

        #region Body-Parsing

        private static Body ParseBody(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            if (stx is not StxPair stp)
            {
                throw new Exception($"Can't parse syntax as form body: {stx}");
            }

            IEnumerable<ISemForm> forms = ParseSerialForms(stp, varMap);

            IEnumerable<ISemAstNode> flattenedNodeList = FlattenBody(forms);

            Definition[] defs = [.. flattenedNodeList.OfType<Definition>()];
            ISemCmd[] cmds = [.. flattenedNodeList.OfType<ISemCmd>()];

            if (cmds.Length == 0 || cmds[^1] is not ISemExp lastExp)
            {
                throw new Exception($"Expected form body to conclude with expression: {stx}");
            }

            return new Body(defs, cmds[..^1], lastExp);
        }

        private static IEnumerable<ISemAstNode> FlattenBody(IEnumerable<ISemForm> seq)
        {
            Stack<Queue<ISemAstNode>> remainingSeqs = new();
            remainingSeqs.Push(new(seq));

            while (remainingSeqs.Count > 0)
            {
                Queue<ISemAstNode> nextSeq = remainingSeqs.Pop();

                while (nextSeq.TryDequeue(out ISemAstNode? nextItem))
                {
                    if (nextItem is Body bod)
                    {
                        remainingSeqs.Push(new(nextSeq));
                        nextSeq = new(bod);
                    }
                    else
                    {
                        yield return nextItem;
                    }
                }
            }
        }

        #endregion

        #region General Dispatch

        private static ISemForm ParseSemanticForm(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            return stx switch
            {
                StxPair stp => ParseCompoundForm(stp, varMap),
                Identifier id when DefaultBindings.TryLookupPrimitive(id.ExpandedSymbol, out PrimitiveOperator? op)
                    => new Primitive(op),
                Identifier id => ParseIdentifier(id, varMap),
                StxDatum std => ParseDatum(std),
                _ => throw new Exception($"Can't parse unknown syntax form: {stx}")
            };
        }
        private static IEnumerable<ISemForm> ParseSerialForms(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            static IEnumerable<ISemForm> Helper(ISyntax _stx, Dictionary<Symbol, ISemVar> _varMap, List<ISemForm> _acc)
            {
                if (_stx.IsNil)
                {
                    return _acc;
                }
                else if (_stx.TryDestruct(out ISyntax? next, out ISyntax? rest))
                {
                    ISemForm newNode = ParseSemanticForm(next, _varMap);
                    _acc.Add(newNode);
                    return Helper(rest, _varMap, _acc);
                }
                else
                {
                    throw new Exception($"Expected {nameof(StxPair)} or {nameof(Nil)}-terminator: {_stx}");
                }
            }

            try
            {
                return Helper(stx, varMap, []);
            }
            catch (Exception ex)
            {
                throw new Exception($"Expected list of syntax objects: {stx}", ex);
            }
        }

        #endregion

        #region Basic Syntax Types
        private static ISemVar ParseIdentifier(Identifier id, Dictionary<Symbol, ISemVar> varMap)
        {
            if (!varMap.TryGetValue(id.ExpandedSymbol, out ISemVar? extantVar))
            {
                extantVar = new Variable(id.ExpandedSymbol.Name, id.Source);
                varMap[id.ExpandedSymbol] = extantVar;
            }
            return extantVar;
        }

        private static ISemExp ParseDatum(ISyntax exp)
        {
            static ISchemeExp StripSyntax(ISyntax stx)
            {
                return stx switch
                {
                    StxDatum std => std.Datum,
                    Identifier id => id.ExpandedSymbol,
                    StxPair stp => new Cons(StripSyntax(stp.Car), StripSyntax(stp.Cdr)),
                    _ => throw new Exception($"Can't strip unknown syntax form: {stx}")
                };
            }

            ISchemeExp stripped = StripSyntax(exp);

            if (stripped is IValue val)
            {
                return new Constant(val, exp.Source);
            }
            else
            {
                return new Quotation(exp, exp.Source);
            }
        }

        private static ISemForm ParseCompoundForm(StxPair stp, Dictionary<Symbol, ISemVar> varMap)
        {
            // TODO review transitivity of special bindings
            if (stp.Car is Identifier id
                && id.BindingInfo?.BoundType == LexicalScope.BindingType.Special
                && id.ExpandedSymbol is Symbol specSym)
            {
                if (specSym == SpecialKeyword.Apply.Symbol) return ParseExplicitApplication(stp.Cdr, varMap);
                else if (specSym == SpecialKeyword.Begin.Symbol) return ParseExplicitSequence(stp.Cdr, varMap);
                else if (specSym == SpecialKeyword.BeginMeta.Symbol) return new Discard(stp.Source);
                else if (specSym == SpecialKeyword.Define.Symbol) return ParseDefinition(stp.Cdr, varMap);
                else if (specSym == SpecialKeyword.If.Symbol) return ParseConditional(stp.Cdr, varMap);
                else if (specSym == SpecialKeyword.Lambda.Symbol) return ParseLambda(stp.Cdr, varMap);
                else if (specSym == SpecialKeyword.Quote.Symbol) return ParseDatum(stp.Cdr);
                else if (specSym == SpecialKeyword.SetBang.Symbol) return ParseAssignment(stp.Cdr, varMap);
                //else if (specSym == SpecialKeyword.Values.Symbol) return ParseMultipleValues(stp.Cdr, varMap);
                else
                {
                    throw new NotImplementedException($"Can't parse unknown special form: {stp}");
                }
            }
            else
            {
                return ParseImplicitApplication(stp, varMap);
            }
        }

        #endregion

        private static Application ParseExplicitApplication(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            if (!stx.TryDestruct(out ISyntax? opTerm, out StxPair? tail)
                || !tail.Cdr.IsNil)
            {
                throw new Exception($"Arguments to explicit {SpecialKeyword.Apply} form don't match expected shape: {stx}");
            }

            if (ParseSemanticForm(opTerm, varMap) is not ISemExp op)
            {
                throw new Exception($"Expected to parse operator term of explicit {SpecialKeyword.Apply} form as expression: {stx}");
            }

            ISemExp[] args = ParseArgumentList(tail, varMap);
            return new(op, args, stx.Source);
        }

        private static Application ParseImplicitApplication(StxPair stx, Dictionary<Symbol, ISemVar> varMap)
        {
            if (!stx.TryDestruct(out ISyntax? opTerm, out ISyntax? tail))
            {
                throw new Exception($"Arguments to implicit {SpecialKeyword.Apply} form don't match expected shape: {stx}");
            }

            if (ParseSemanticForm(opTerm, varMap) is not ISemExp op)
            {
                throw new Exception($"Expected to parse operator term of implicit {SpecialKeyword.Apply} form as expression: {stx}");
            }

            ISemExp[] args = ParseArgumentList(tail, varMap);
            return new(op, args, stx.Source);
        }

        private static Definition ParseDefinition(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            if (!stx.TryDestruct(out Identifier? id, out StxPair? tail)
                || !tail.Cdr.IsNil)
            {
                throw new Exception($"Arguments to {SpecialKeyword.Define} form don't match expected shape: {stx}");
            }

            if (ParseSemanticForm(tail.Car, varMap) is not ISemExp val)
            {
                throw new Exception($"Expected to parse value term of {SpecialKeyword.Define} form as expression: {stx}");
            }

            ISemVar sv = ParseIdentifier(id, varMap);
            return new(sv, val, stx.Source);
        }

        private static Assignment ParseAssignment(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            if (!stx.TryDestruct(out Identifier? id, out StxPair? tail)
                || !tail.Cdr.IsNil)
            {
                throw new Exception($"Arguments to {SpecialKeyword.SetBang} form don't match expected shape: {stx}");
            }

            if (ParseSemanticForm(tail.Car, varMap) is not ISemExp val)
            {
                throw new Exception($"Expected to parse value term of {SpecialKeyword.SetBang} form as expression: {stx}");
            }

            ISemVar sv = ParseIdentifier(id, varMap);
            return new(sv, val, stx.Source);
        }

        private static Lambda ParseLambda(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            if (!stx.TryDestruct(out ISyntax? formals, out ISyntax? tail))
            {
                throw new Exception($"Arguments to {SpecialKeyword.Lambda} form don't match expected shape: {stx}");
            }

            var parms = ParseParameterList(formals, varMap);
            Body bod = ParseBody(tail, varMap);
            return new(parms.Item1, parms.Item2, bod, stx.Source);
        }

        private static Conditional ParseConditional(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            if (stx.TryDestruct(out ISyntax? arg1, out ISyntax? rest1))
            {
                if (ParseSemanticForm(arg1, varMap) is not ISemExp cond)
                {
                    throw new Exception($"Expected to parse 'condition' term of {SpecialKeyword.If} form as expression: {arg1}");
                }
                else if (rest1.TryDestruct(out ISyntax? arg2, out ISyntax? rest2))
                {
                    if (ParseSemanticForm(arg2, varMap) is not ISemExp consq)
                    {
                        throw new Exception($"Expected to parse 'consequent' term of {SpecialKeyword.If} form as expression: {arg2}");
                    }
                    else if (rest2.IsNil)
                    {
                        Constant implicitAlt = new(SchemeData.Boolean.False, stx.Source);
                        return new Conditional(cond, consq, implicitAlt, stx.Source);
                    }
                    else if (rest2.TryDestruct(out ISyntax? arg3, out ISyntax? rest3))
                    {
                        if (ParseSemanticForm(arg3, varMap) is not ISemExp alt)
                        {
                            throw new Exception($"Expected to parse 'alternative' term of {SpecialKeyword.If} form as expression: {arg3}");
                        }
                        else if (rest3.IsNil)
                        {
                            return new Conditional(cond, consq, alt, stx.Source);
                        }
                    }
                }
            }

            throw new Exception($"Arguments to {SpecialKeyword.If} form don't match expected shape: {stx}");
        }

        private static Sequence ParseExplicitSequence(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            try
            {
                Body bod = ParseBody(stx, varMap);
                return new Sequence(bod, stx.Source);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse body of {SpecialKeyword.Begin} form: {stx}", ex);
            }
        }

        private static ISemExp[] ParseArgumentList(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            static ISemExp[] Unroll(ISyntax _stx, Dictionary<Symbol, ISemVar> _varMap, List<ISemExp> acc)
            {
                if (_stx.IsNil)
                {
                    return [.. acc];
                }
                else if (_stx.TryDestruct(out ISyntax? arg, out ISyntax? rest)
                    && ParseSemanticForm(arg, _varMap) is ISemExp parsedArg)
                {
                    acc.Add(parsedArg);
                    return Unroll(rest, _varMap, acc);
                }
                else
                {
                    throw new Exception($"Arguments to {SpecialKeyword.Apply} form don't match expected shape: {_stx}");
                }
            }
            return Unroll(stx, varMap, []);
        }

        private static (ISemVar[], ISemVar?) ParseParameterList(ISyntax stx, Dictionary<Symbol, ISemVar> varMap)
        {
            static (ISemVar[], ISemVar?) Unroll(ISyntax _stx, Dictionary<Symbol, ISemVar> _varMap, List<ISemVar> acc)
            {
                if (_stx.IsNil)
                {
                    return ([.. acc], null);
                }
                else if (_stx is Identifier id)
                {
                    ISemVar v = ParseIdentifier(id, _varMap);
                    return ([.. acc], v);
                }
                else if (_stx.TryDestruct(out Identifier? param, out ISyntax? rest))
                {
                    ISemVar v = ParseIdentifier(param, _varMap);
                    acc.Add(v);
                    return Unroll(rest, _varMap, acc);
                }
                else
                {
                    throw new Exception($"Parameters of {SpecialKeyword.Lambda} form don't match expected shape: {_stx}");
                }
            }
            return Unroll(stx, varMap, []);
        }
    }
}