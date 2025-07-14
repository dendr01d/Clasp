using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.SchemeTypes.TypeConstraints;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ConstrainSemanticTypes
    {
        private sealed record Context
        {
            public Dictionary<ISemVar, SchemeType> Gamma { get; init; } = [];
            public HashSet<VarType> Delta { get; init; } = [];
            public Dictionary<ISemVar, DottedPreType> Sigma { get; init; } = [];
            public HashSet<TypeConstraint> TypeConstraints { get; init; } = [];

            public Context WithVisiblePredicate(IVisibleTypePredicate? pred)
            {
                if (pred is ISemVar sv && Gamma.TryGetValue(sv, out SchemeType? sigma))
                {
                    if (sv is TypedVariable tv)
                    {
                        return this with
                        {
                            Gamma = new(Gamma)
                            {
                                { sv, Restrict(sigma, tv.Type) }
                            }
                        };
                    }
                    else
                    {
                        return this with
                        {
                            Gamma = new(Gamma)
                            {
                                { sv, Remove(sigma, SchemeType.False) }
                            }
                        };
                    }
                }
                else
                {
                    return this;
                }
            }

            public Context WithoutVisiblePredicate(IVisibleTypePredicate? pred)
            {
                if (pred is ISemVar sv && Gamma.TryGetValue(sv, out SchemeType? sigma))
                {
                    if (sv is TypedVariable tv)
                    {
                        return this with
                        {
                            Gamma = new(Gamma)
                            {
                                { sv, Remove(sigma, tv.Type) }
                            }
                        };
                    }
                    else
                    {
                        return this with
                        {
                            Gamma = new(Gamma)
                            {
                                { sv, SchemeType.False }
                            }
                        };
                    }
                }
                else
                {
                    return this;
                }
            }

            private static SchemeType Restrict(SchemeType sigma, SchemeType tau)
            {
                if (sigma < tau)
                {
                    return sigma;
                }
                else if (tau is UnionType ut)
                {
                    return UnionType.Join(ut.Types.Select(x => Restrict(sigma, x)));
                }
                else
                {
                    return tau;
                }
            }

            private static SchemeType Remove(SchemeType sigma, SchemeType tau)
            {
                if (sigma < tau)
                {
                    return SchemeType.Bottom;
                }
                else if (tau is UnionType ut)
                {
                    return UnionType.Join(ut.Types.Select(x => Remove(sigma, x)));
                }
                else
                {
                    return sigma;
                }
            }
        }

        public static Prog_Sem Execute(Prog_Sem program)
        {
            Context ctx = new();

            var output = InferThroughBody(program.AbstractSyntaxTree, ctx);

            return new Prog_Sem(output.Item1)
            {
                VariableTypes = ctx.Gamma.ToDictionary(),
                TypeConstraints = [.. ctx.TypeConstraints],
                ProgramType = output.Item2.Type
            };
        }

        #region Inference

        private static ISemAnnotated InferExpressionType(ISemExp expr, Context ctx)
        {
            switch (expr)
            {
                case ISemAnnotated typed:
                    return typed;

                case Application app:
                    // We know there's SOME kind of procedure, but we can't know exactly what kind
                    // So we constrain it based on what we know from the arguments
                    ISemAnnotated[] typedArgs = [.. app.Arguments.Select(x => InferExpressionType(x, ctx))];
                    VarType funOut = new();
                    FunctionType presumedFunType = new(funOut, typedArgs.Select(x => x.Type));
                    ISemAnnotated typedProc = CheckEquivalentType(app.Procedure, presumedFunType, ctx);
                    Application typedApp = app with
                    {
                        Arguments = typedArgs,
                        Procedure = typedProc
                    };
                    return new AnnotatedExpression(typedApp, funOut);

                case Conditional cond:
                    ISemAnnotated annoCond = InferExpressionType(cond.Condition, ctx);
                    ISemAnnotated annoConsq = InferExpressionType(cond.Consequent, ctx.WithVisiblePredicate(annoCond.VisiblePredicate));
                    ISemAnnotated annoAlt = InferExpressionType(cond.Alternative, ctx.WithoutVisiblePredicate(annoCond.VisiblePredicate));
                    //if (Boole.True.Equals(annoCond.VisiblePredicate))
                    //{
                    //    return annoConsq;
                    //}
                    //else if (Boole.False.Equals(annoCond.VisiblePredicate))
                    //{
                    //    return annoAlt;
                    //}
                    //else
                    //{
                        IVisibleTypePredicate? combPred = CombinePredicates(annoCond, annoConsq, annoAlt);
                        SchemeType combinedType = UnionType.Join(annoConsq.Type, annoAlt.Type);
                        Conditional typedCond = cond with
                        {
                            Condition = annoCond,
                            Consequent = annoConsq,
                            Alternative = annoAlt
                        };
                        return new AnnotatedExpression(typedCond, combinedType, combPred);
                    //}

                case Lambda lam:
                    TypedVariable[] typedParams = [.. lam.Parameters.Select(x => InferNewVariableType(x, ctx))];
                    TypedVariable? typedDotParam = lam.DottedParameter is null ? null : InferNewVariableType(lam.DottedParameter, ctx);
                    var typedBody = InferThroughBody(lam.Body, ctx);
                    Lambda typedFun = lam with
                    {
                        Parameters = typedParams,
                        DottedParameter = typedDotParam,
                        Body = typedBody.Item1
                    };
                    if (typedParams.Length == 1
                        && typedDotParam is null
                        && typedBody.Item2.VisiblePredicate is TypedVariable tv
                        && tv.Equals(typedParams[0]))
                    {
                        FunctionType funType = new(typedBody.Item2.Type, typedParams.Select(x => x.Type), typedDotParam?.Type)
                        {
                            LatentPredicate = tv.Type
                        };
                        return new AnnotatedExpression(typedFun, funType, tv);
                    }
                    else
                    {
                        FunctionType funType = new(typedBody.Item2.Type, typedParams.Select(x => x.Type), typedDotParam?.Type);
                        return new AnnotatedExpression(typedFun, funType);
                    }

                case Sequence seq:
                    var annotatedBody = InferThroughBody(seq.Body, ctx);
                    Sequence typedSeq = seq with { Body = annotatedBody.Item1 };
                    return new AnnotatedExpression(typedSeq, annotatedBody.Item2.Type);

                case Variable v:
                    return InferVariableType(v, ctx);

                default:
                    throw new Exception($"Can't infer type of unknown expression: {expr}");
            }
        }

        private static TypedVariable InferNewVariableType(ISemVar sv, Context ctx)
        {
            return CheckVariableType(sv, new VarType(), ctx);
        }

        private static TypedVariable InferVariableType(ISemVar sv, Context ctx)
        {
            return sv switch
            {
                TypedVariable tv => tv,
                Variable v when ctx.Gamma.TryGetValue(sv, out SchemeType? varType) => new TypedVariable(v, varType),
                Variable => throw new Exception($"Can't infer type of free variable: {sv}"),
                _ => throw new Exception($"Can't infer type of unknown variable: {sv}")
            };
        }

        private static (Body, ISemAnnotated) InferThroughBody(Body bod, Context ctx)
        {
            Definition[] defs = [.. bod.Definitions.Select(x => InferThroughDefinition(x, ctx))];
            ISemCmd[] cmds = [.. bod.Commands.Select(x => InferThroughCommand(x, ctx))];
            ISemAnnotated val = InferExpressionType(bod.Value, ctx);

            Body output = bod with
            {
                Definitions = defs,
                Commands = cmds,
                Value = val
            };

            return (output, val);
        }

        private static Definition InferThroughDefinition(Definition def, Context ctx)
        {
            ISemAnnotated val = InferExpressionType(def.Value, ctx);
            TypedVariable tv = CheckVariableType(def.Variable, val.Type, ctx);

            return def with
            {
                Variable = tv,
                Value = val
            };
        }

        private static Assignment InferThroughAssignment(Assignment set, Context ctx)
        {
            ISemAnnotated val = InferExpressionType(set.Value, ctx);
            TypedVariable tv = CheckVariableType(set.Variable, val.Type, ctx);

            return set with
            {
                Variable = tv,
                Value = val
            };
        }

        private static ISemCmd InferThroughCommand(ISemCmd cmd, Context ctx)
        {
            return cmd switch
            {
                Assignment set => InferThroughAssignment(set, ctx),
                ISemExp exp => InferExpressionType(exp, ctx),
                _ => throw new Exception($"Can't infer type of unknown command: {cmd}")
            };
        }

        #endregion

        private static IVisibleTypePredicate? CombinePredicates(ISemAnnotated anno1, ISemAnnotated anno2, ISemAnnotated anno3)
        {
            return (anno1.VisiblePredicate, anno2.VisiblePredicate, anno3.VisiblePredicate) switch
            {
                (_, var a, var b) when a == b => a,
                (TypedVariable t, SchemeData.Boolean b, TypedVariable s) when b.Value && t.Variable == s.Variable => new TypedVariable(t.Variable, UnionType.Join(t.Type, s.Type)),
                (SchemeData.Boolean b, var consq, _) when b.Value => consq,
                (SchemeData.Boolean b, _, var alt) when !b.Value => alt,
                (var cond, SchemeData.Boolean b1, SchemeData.Boolean b2) when b1.Value && !b2.Value => cond,
                _ => null,
            };
        }


        #region Checking

        private static ISemAnnotated CheckEquivalentType(ISemExp expr, SchemeType ty, Context ctx)
        {
            switch (expr)
            {
                //case ISemVar sv:
                //    return CheckVariableType(sv, ty, ctx);

                case ISemAnnotated typed when typed.Type == ty:
                    return typed;

                //case ISemAnnotated typed:
                //    ctx.TypeConstraints.Add(new TypeEqualsType(typed.Type, ty, expr));
                //    return typed;

                //case Application app:

                //    break;

                //case Conditional cond:
                //    break;

                case Lambda lam when ty is FunctionType fty:
                    {
                        List<TypedVariable> typedParams = [];
                        TypedVariable? typedDotted = null;

                        int i = 0;
                        for (; i < int.Min(lam.Parameters.Length, fty.InputTypes.Length); ++i)
                        {
                            typedParams.Add(CheckVariableType(lam.Parameters[i], fty.InputTypes[i], ctx));
                        }

                        for (; i < lam.Parameters.Length; ++i)
                        {
                            typedParams.Add(CheckVariableType(lam.Parameters[i], SchemeType.Bottom, ctx));
                        }

                        if (i < fty.InputTypes.Length && lam.DottedParameter is not null)
                        {
                            if (lam.DottedParameter is null)
                            {
                                for(; i < fty.InputTypes.Length; ++i)
                                {
                                    ctx.TypeConstraints.Add(new TypeEqualsType(fty.InputTypes[i], SchemeType.Bottom, lam));
                                }
                            }
                            else
                            {
                                typedDotted = CheckVariableType(lam.DottedParameter, new DottedPreType(fty.InputTypes[i..^1]), ctx);
                            }
                        }

                        Body typedBody = CheckThroughBody(lam.Body, fty.OutputType, ctx);

                        Lambda typedFun = lam with
                        {
                            Parameters = [.. typedParams],
                            DottedParameter = typedDotted,
                            Body = typedBody
                        };
                        return new AnnotatedExpression(typedFun, fty);
                    }

                case Sequence seq:
                    Body typedBod = CheckThroughBody(seq.Body, ty, ctx);
                    Sequence typedSeq = seq with { Body = typedBod };
                    return new AnnotatedExpression(typedSeq, ty); // is this okay, vs extracting the resulting type directly from the body?

                default:
                    ISemAnnotated inferred = InferExpressionType(expr, ctx);
                    ctx.TypeConstraints.Add(new TypeEqualsType(inferred.Type, ty, expr));
                    return inferred;
            }
        }

        private static TypedVariable CheckVariableType(ISemVar sv, SchemeType ty, Context ctx)
        {
            if (sv is Variable v)
            {
                ctx.Gamma[v] = ty;
                return new(v, ty);
            }
            else if (sv is TypedVariable tv)
            {
                ctx.TypeConstraints.Add(new TypeEqualsType(tv.Type, ty, sv));
                ctx.Gamma[tv.Variable] = ty;
                return new(tv.Variable, ty);
            }
            else
            {
                throw new Exception($"Can't check type of unknown variable: {sv}");
            }
        }

        private static Body CheckThroughBody(Body bod, SchemeType ty, Context ctx)
        {
            Definition[] defs = [.. bod.Definitions.Select(x => InferThroughDefinition(x, ctx))];
            ISemCmd[] cmds = [.. bod.Commands.Select(x => InferThroughCommand(x, ctx))];
            ISemAnnotated val = CheckEquivalentType(bod.Value, ty, ctx);

            return bod with
            {
                Definitions = defs,
                Commands = cmds,
                Value = val
            };
        }



        #endregion
    }
}