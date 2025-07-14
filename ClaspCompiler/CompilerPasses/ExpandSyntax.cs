using ClaspCompiler.CompilerData;
using ClaspCompiler.LexicalScope;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeSyntax.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ExpandSyntax
    {
        public static Prog_Stx Execute(Prog_Stx program)
        {
            var ctx = ExpansionContext.NewTopLevelContext(program.SymFactory);
            ISyntax newBody = ExpandExpression(program.TopLevelForms, ctx);

            return new Prog_Stx(newBody, program.SymFactory);
        }

        private static ISyntax ExpandExpression(ISyntax stx, ExpansionContext ctx)
        {
            return stx switch
            {
                Identifier id when ctx.Lexicon != ContextType.Partial => ExpandLooseIdentifier(id, ctx),
                StxPair stp => ExpandForm(stp, ctx),
                _ => stx
            };
        }

        #region Identifier Expansion

        private static Identifier ExpandLooseIdentifier(Identifier id, ExpansionContext ctx)
        {
            // thanks to recursive scope operations, this id already has its final scope
            // first, we need to make sure that it's been bound by one of the surrounding scopes
            // then we dispatch according to the type of binding

            if (ctx.BindingStore.TryResolve(id, out Binding? binding))
            {
                return binding.BoundType switch
                {
                    BindingType.Module => throw new Exception($"Can't expand {binding.BoundType} {binding.UniqueName} as loose identifier."),
                    BindingType.Macro => throw new NotImplementedException(),
                    _ => id with { BindingInfo = binding }
                };
            }
            throw new Exception($"Can't expand unbound loose identifier: {id}");
        }

        private static Identifier ExpandBindingIdentifier(Identifier id, BindingType boundType, ExpansionContext ctx)
        {
            // the identifier (should have?) already been painted with all required scopes
            // to represent the extent of its binding
            // now we construct a new name for the identifier, record the binding info in the store,
            // and return an identifier updated to hold information about the binding

            Symbol renamedSymbol = ctx.SymFactory.GenerateUnique(id.Symbol);
            Binding newBinding = new(renamedSymbol, boundType);

            ctx.BindingStore.RecordBinding(id, newBinding);

            return id with
            {
                BindingInfo = newBinding
            };
        }

        #endregion

        private static ISyntax ExpandForm(StxPair stp, ExpansionContext ctx)
        {
            ISyntax opTerm = ExpandExpression(stp.Car, ctx.InStandardContext());

            if (opTerm is Identifier id)
            {
                if (id.BindingInfo?.BoundType == BindingType.Module)
                {
                    throw new Exception($"Can't expand generic application with module in operator position: {stp}");
                }
                else if (id.BindingInfo?.BoundType == BindingType.Macro)
                {
                    // dereference the macro from the compilation environment
                    // execute it???
                    // also do some scoping operations in/around everything
                }
                else if (id.BindingInfo?.BoundType == BindingType.Special)
                {
                    return stp with
                    {
                        Car = opTerm,
                        Cdr = ExpandSpecialFormArguments(id.BindingInfo.UniqueName, stp.Cdr, ctx)
                    };
                }
            }

            // opTerm is either a variable, primitive, or some other expression altogether
            // expand the argument forms individually in sequence

            return stp with
            {
                Car = opTerm,
                Cdr = ExpandSequentialExpressions(stp.Cdr, ctx)
            };
        }

        private static ISyntax ExpandSpecialFormArguments(Symbol kwSym, ISyntax args, ExpansionContext ctx)
        {
            if (ctx.Lexicon == ContextType.Partial)
            {
                if (kwSym == SpecialKeyword.Begin.Symbol) return ExpandSequentialExpressions(args, ctx);
                else if (kwSym == SpecialKeyword.Define.Symbol) return PartiallyExpandBindingPair(args, ctx);
                else
                {
                    return args;
                }
            }
            else if (kwSym == SpecialKeyword.Apply.Symbol) return ExpandSequentialExpressions(args, ctx);
            else if (kwSym == SpecialKeyword.Begin.Symbol) return ExpandBody(args, ctx);
            else if (kwSym == SpecialKeyword.Define.Symbol
                && ctx.Lexicon == ContextType.Completional) return FinallyExpandBindingPair(args, ctx);
            else if (kwSym == SpecialKeyword.Define.Symbol) return ExpandBindingPair(args, ctx.InSubLevel());
            else if (kwSym == SpecialKeyword.If.Symbol) return ExpandSequentialExpressions(args, ctx);
            else if (kwSym == SpecialKeyword.Lambda.Symbol) return ExpandLambda(args, ctx.InSubLevel());
            else if (kwSym == SpecialKeyword.SetBang.Symbol) return ExpandMutatingPair(args, ctx.InSubLevel());
            else if (kwSym == SpecialKeyword.Quote.Symbol) return args;
            else
            {
                throw new Exception($"Can't expand arguments of unknown special form: {kwSym}");
            }
        }

        private static ISyntax ExpandSequentialExpressions(ISyntax stx, ExpansionContext ctx)
        {
            if (stx.IsNil)
            {
                return stx;
            }
            else if (stx.TryDestruct(out ISyntax? next, out ISyntax? rest))
            {
                return new StxPair(stx)
                {
                    Car = ExpandExpression(next, ctx),
                    Cdr = ExpandSequentialExpressions(rest, ctx)
                };
            }
            else
            {
                throw new Exception($"Expected proper nil-terminated list: {stx}");
            }
        }

        #region Whole and Partial-Definition Expansion

        private static StxPair ExpandBindingPair(ISyntax stx, ExpansionContext ctx)
            => FinallyExpandBindingPair(PartiallyExpandBindingPair(stx, ctx), ctx);

        //map and replace the id of the binding, but skip the value for now
        private static StxPair PartiallyExpandBindingPair(ISyntax stx, ExpansionContext ctx)
        {
            if (stx.TryDestruct(out Identifier? id, out ISyntax? def))
            {
                return new StxPair(stx)
                {
                    Car = ExpandBindingIdentifier(id, BindingType.Variable, ctx),
                    Cdr = def
                };
            }

            throw new Exception($"Can't expand arguments to {SpecialKeyword.Define} form: {stx}");
        }

        //expand the value of the binding without altering the id
        private static StxPair FinallyExpandBindingPair(ISyntax stx, ExpansionContext ctx)
        {
            if (stx.TryDestruct(out Identifier? id, out StxPair? def)
                && def.Cdr.IsNil)
            {
                uint newScopeToken = ctx.BindingStore.GetFreshScopeToken();
                Identifier paintedId = id.AddScopes([newScopeToken]);
                ISyntax paintedDef = def.Car.AddScopes([newScopeToken]);

                return new StxPair(stx)
                {
                    Car = paintedId,
                    Cdr = new StxPair(def)
                    {
                        Car = ExpandExpression(paintedDef, ctx.InStandardContext()),
                        Cdr = def.Cdr
                    }
                };
            }

            throw new Exception($"Can't expand arguments to {SpecialKeyword.Define} form: {stx}");
        }

        #endregion

        private static StxPair ExpandMutatingPair(ISyntax stx, ExpansionContext ctx)
        {
            if (stx.TryDestruct(out Identifier? id, out StxPair? def)
                && def.Cdr.IsNil)
            {
                return new StxPair(stx)
                {
                    Car = ExpandLooseIdentifier(id, ctx),
                    Cdr = new StxPair(def)
                    {
                        Car = ExpandExpression(def.Car, ctx),
                        Cdr = def.Cdr
                    }
                };
            }

            throw new Exception($"Can't expand arguments to mutation form: {stx}");
        }

        private static StxPair ExpandLambda(ISyntax stx, ExpansionContext ctx)
        {
            if (stx.TryDestruct(out ISyntax? parameters, out StxPair? funBody))
            {
                uint newScopeToken = ctx.BindingStore.GetFreshScopeToken();
                ISyntax paintedParams = parameters.AddScopes([newScopeToken]);
                ISyntax paintedBody = funBody.AddScopes([newScopeToken]);

                return new StxPair(stx)
                {
                    Car = ExpandParameterList(paintedParams, ctx),
                    Cdr = ExpandBody(paintedBody, ctx)
                };
            }

            throw new Exception($"Can't expand arguments to lambda form: {stx}");
        }

        private static ISyntax ExpandParameterList(ISyntax stx, ExpansionContext ctx)
        {
            if (stx.IsNil)
            {
                return stx;
            }
            else if (stx is Identifier variad)
            {
                return ExpandBindingIdentifier(variad, BindingType.Variable, ctx);
            }
            else if (stx.TryDestruct(out Identifier? param, out ISyntax? more))
            {
                return new StxPair(stx)
                {
                    Car = ExpandBindingIdentifier(param, BindingType.Variable, ctx),
                    Cdr = ExpandParameterList(more, ctx)
                };
            }

            throw new Exception($"Can't expand parameter list of lambda form: {stx}");
        }

        private static ISyntax ExpandBody(ISyntax stx, ExpansionContext ctx)
        {
            if (stx is not StxPair body)
            {
                throw new Exception($"Can't expand sequential form: {stx}");
            }

            uint outsideEdgeToken = ctx.BindingStore.GetFreshScopeToken();
            uint insideEdgeToken = ctx.BindingStore.GetFreshScopeToken();

            ISyntax painted1 = body.AddScopes([outsideEdgeToken, insideEdgeToken]);
            ISyntax partial = ExpandSequentialExpressions(painted1, ctx.InPartialContext());

            ISyntax painted2 = partial.AddScopes([insideEdgeToken]);
            ISyntax complete = ExpandSequentialExpressions(painted2, ctx.InCompletionalContext());

            return complete;
        }
    }
}
