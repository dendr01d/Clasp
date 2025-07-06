using ClaspCompiler.CompilerData;
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
            public Dictionary<ISemVar, SchemeType> Env { get; } = [];
            public HashSet<TypeConstraint> TypeConstraints { get; } = [];
            public DisjointTypeSet TypeUnifier { get; } = new();

            public VarType NewVarType()
            {
                VarType vt = new();
                TypeUnifier.Add(vt);
                return vt;
            }
        }

        public static Prog_Sem Execute(Prog_Sem program)
        {
            Context ctx = new();

            Body bod = InferThroughBody(program.AbstractSyntaxTree, ctx);

            return new Prog_Sem(bod)
            {
                SourceLookup = program.SourceLookup,
                VariableTypes = ctx.Env.ToDictionary(),
                TypeConstraints = [.. ctx.TypeConstraints],
                TypeUnification = ctx.TypeUnifier
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
                    break;

                case Conditional cond:
                    break;

                case Lambda lam:
                    break;

                case Sequence seq:
                    Body typedBod = InferThroughBody(seq.Body, ctx);
                    Sequence typedSeq = seq with { Body = typedBod };
                    return new AnnotatedExpression(typedSeq, ((ISemAnnotated)typedBod.Value).Type); // blech

                case Variable v:
                    return InferVariableType(v, ctx);

                default:
                    throw new Exception($"Can't infer type of unknown expression: {expr}");
            }
        }

        private static TypedVariable InferVariableType(ISemVar sv, Context ctx)
        {
            return sv switch
            {
                TypedVariable tv => tv,
                Variable v when ctx.Env.TryGetValue(sv, out SchemeType? varType) => new TypedVariable(v, varType),
                Variable => throw new Exception($"Can't infer type of free variable: {sv}"),
                _ => throw new Exception($"Can't infer type of unknown variable: {sv}")
            };
        }

        private static Body InferThroughBody(Body bod, Context ctx)
        {
            Definition[] defs = [.. bod.Definitions.Select(x => InferThroughDefinition(x, ctx))];
            ISemCmd[] cmds = [.. bod.Commands.Select(x => InferThroughCommand(x, ctx))];
            ISemExp val = InferExpressionType(bod.Value, ctx);

            return bod with
            {
                Definitions = defs,
                Commands = cmds,
                Value = val
            };
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

        #region Checking

        private static ISemAnnotated CheckExpressionType(ISemExp expr, SchemeType ty, Context ctx)
        {
            switch (expr)
            {
                case ISemVar sv:
                    return CheckVariableType(sv, ty, ctx);

                case ISemAnnotated typed:
                    ctx.TypeConstraints.Add(new TypeEquality(expr, typed.Type, ty));
                    return typed;

                case Application app:

                    break;

                case Conditional cond:
                    break;

                case Lambda lam when ty is FunctionType fty:

                    break;

                case Sequence seq:
                    Body typedBod = CheckThroughBody(seq.Body, ty, ctx);
                    Sequence typedSeq = seq with { Body = typedBod };
                    return new AnnotatedExpression(typedSeq, ty); // is this okay, vs extracting the resulting type directly from the body?

                default:
                    ISemAnnotated inferred = InferExpressionType(expr, ctx);
                    ctx.TypeConstraints.Add(new TypeEquality(expr, inferred.Type, ty));
                    return inferred;
            }
        }

        private static TypedVariable CheckVariableType(ISemVar sv, SchemeType ty, Context ctx)
        {
            if (sv is Variable v)
            {
                ctx.Env[v] = ty;
                return new(v, ty);
            }
            else if (sv is TypedVariable tv)
            {
                ctx.TypeConstraints.Add(new TypeEquality(sv, tv.Type, ty));
                ctx.Env[tv.Variable] = ty;
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
            ISemAnnotated val = CheckExpressionType(bod.Value, ty, ctx);

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