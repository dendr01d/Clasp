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
            public Dictionary<SemVar, SchemeType> Env { get; } = [];
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

            Body bod = InferTypedBody(program.AbstractSyntaxTree, ctx);

            return new Prog_Sem(bod)
            {
                SourceLookup = program.SourceLookup,
                VariableTypes = ctx.Env.ToDictionary(),
                TypeConstraints = [.. ctx.TypeConstraints],
                TypeUnification = ctx.TypeUnifier
            };
        }

        #region General Dispatch

        private static ISemAstNode InferTypedNode(ISemAstNode node, Context ctx)
        {
            return node switch
            {
                Definition def => InferTypedDefinition(def, ctx),
                FormalArguments args => InferTypedArguments(args, ctx),
                FormalParameters parms => InferTypedParameters(parms, ctx),
                ISemCmd cmd => InferTypedCommand(cmd, ctx),
                _ => throw new Exception($"Can't infer type of unknown AST node: {node}")
            };
        }

        private static ISemCmd InferTypedCommand(ISemCmd cmd, Context ctx)
        {
            return cmd switch
            {
                Assignment set => InferTypedAssignment(set, ctx),
                ISemExp exp => InferTypedExpression(exp, ctx),
                _ => throw new Exception($"Can't infer type of unknown command node: {cmd}")
            };
        }

        private static ISemExp InferTypedExpression(ISemExp exp, Context ctx)
        {
            return exp switch
            {
                Application app => InferTypedApplication(app, ctx),
                Conditional iff => InferTypedConditional(iff, ctx),
                Lambda lam => InferTypedFunction(lam, ctx),
                SemVar var => InferTypedFreeVariable(var, ctx),
                SemValue or Primitive or Quotation => exp,
                Sequence seq => seq with { Body = InferTypedBody(seq.Body, ctx) },
                _ => throw new Exception($"Can't infer type of unknown expression node: {exp}")
            };
        }

        #endregion

        #region Implicitly-Typed Forms
        // in the sense that their types are automatically derived from their composite parts
        // type inference for these forms simply involves recurring through their subforms

        private static Assignment InferTypedAssignment(Assignment set, Context ctx)
        {
            SemVar var = InferTypedBindingVariable(set.Variable, ctx);
            ISemExp checkedValue = CheckExpressionTypeEquality(set.AstId, set.Value, var.Type, ctx);

            return set with
            {
                Variable = var,
                Value = checkedValue
            };
        }

        private static Body InferTypedBody(Body bod, Context ctx)
        {
            Definition[] defs = [.. bod.Definitions.Select(x => InferTypedDefinition(x, ctx))];
            ISemCmd[] cmds = [.. bod.Commands.Select(x => InferTypedCommand(x, ctx))];
            ISemExp val = InferTypedExpression(bod.Value, ctx);

            return bod with
            {
                Definitions = defs,
                Commands = cmds,
                Value = val
            };
        }

        private static Conditional InferTypedConditional(Conditional iff, Context ctx)
        {
            ISemExp cond = InferTypedExpression(iff.Condition, ctx);
            ISemExp consq = InferTypedExpression(iff.Consequent, ctx);
            ISemExp alt = InferTypedExpression(iff.Alternative, ctx);

            SchemeType ty = consq.Type == alt.Type
                ? consq.Type
                : new UnionType(consq.Type, alt.Type);

            return iff with
            {
                Condition = cond,
                Consequent = consq,
                Alternative = alt
            };
        }

        private static Definition InferTypedDefinition(Definition def, Context ctx)
        {
            SemVar var = InferTypedBindingVariable(def.Variable, ctx);
            ISemExp checkedValue = CheckExpressionTypeEquality(def.AstId, def.Value, var.Type, ctx);

            return def with
            {
                Variable = var,
                Value = checkedValue
            };
        }

        private static FormalArguments InferTypedArguments(FormalArguments args, Context ctx)
        {
            ISemExp[] typedArgs = [.. args.Values.Select(x => InferTypedExpression(x, ctx))];

            return args with
            {
                Values = typedArgs
            };
        }

        private static FormalParameters InferTypedParameters(FormalParameters parms, Context ctx)
        {
            List<SemVar> typedVars = [];

            IEnumerable<SemVar> normalVars = parms.Variadic
                ? parms.Values.SkipLast(1)
                : parms.Values;

            foreach(SemVar sv in normalVars)
            {
                typedVars.Add(InferTypedBindingVariable(sv, ctx));
            }

            if (parms.Variadic)
            {
                SemVar varParam = parms.Values.Last();
                typedVars.Add(InferTypedVariadicBindingVariable(varParam, ctx));
            }

            return parms with
            {
                Values = [.. typedVars]
            };
        }

        private static Lambda InferTypedFunction(Lambda lam, Context ctx)
        {
            FormalParameters parms = InferTypedParameters(lam.Parameters, ctx);
            Body bod = InferTypedBody(lam.Body, ctx);

            return lam with
            {
                Parameters = parms,
                Body = bod,
            };
        }

        #endregion

        #region Explicit Type Inference

        private static Application InferTypedApplication(Application app, Context ctx)
        {
            FormalArguments args = InferTypedArguments(app.Arguments, ctx);
            SchemeType outputType = ctx.NewVarType();
            FunctionType assumedFunType = new(outputType, args.Type);

            ISemExp checkedProc = CheckExpressionTypeEquality(app.AstId, app.Procedure, assumedFunType, ctx);

            return app with
            {
                Procedure = checkedProc,
                Arguments = args,
                Type = outputType
            };
        }

        private static SemVar InferTypedBindingVariable(SemVar variable, Context ctx)
        {
            VarType outType = ctx.NewVarType();
            ctx.Env[variable] = outType;

            return variable with
            {
                Type = outType
            };
        }

        private static SemVar InferTypedVariadicBindingVariable(SemVar variable, Context ctx)
        {
            VarType recurring = ctx.NewVarType();
            ListOfType variadicType = new(recurring);
            ctx.Env[variable] = variadicType;

            return variable with
            {
                Type = variadicType
            };
        }

        private static SemVar InferTypedFreeVariable(SemVar variable, Context ctx)
        {
            // the semantics should be arranged in such a way that bindings/uses happen in order
            if (!ctx.Env.TryGetValue(variable, out SchemeType? outType))
            {
                throw new Exception($"Can't infer type of unbound variable: {variable}");
            }

            return variable with
            {
                Type = outType
            };
        }



        #endregion

        #region Checking Equality

        /// <summary>
        /// Constrains the type of <paramref name="exp"/> to be equal to <paramref name="expectedType"/>, then returns
        /// the inferred type of <paramref name="exp"/>.
        /// </summary>
        private static ISemExp CheckExpressionTypeEquality(uint astId, ISemExp exp, SchemeType expectedType, Context ctx)
        {
            switch (exp)
            {
                case SemValue lit when lit.Value.Type == expectedType:
                case Quotation quo when quo.Value.Type == expectedType:
                case Primitive prim when prim.Operator.Type == expectedType:
                    return exp;

                case Lambda lam when expectedType is FunctionType ft:
                    FormalParameters parms = CheckParameterTypeEquality(astId, lam.Parameters, ft.InputType, ctx);
                    Body bod = CheckBodyTypeEquality(astId, lam.Body, ft.OutputType, ctx);
                    return lam with
                    {
                        Parameters = parms,
                        Body = bod
                    };

                    default:
                ISemExp inferred = InferTypedExpression(exp, ctx);
                ctx.TypeConstraints.Add(new EqualType(astId, inferred.Type, expectedType));
                return inferred;
            }
        }

        private static Body CheckBodyTypeEquality(uint astId, Body bod, SchemeType expectedType, Context ctx)
        {
            Body inferred = InferTypedBody(bod, ctx);
            ctx.TypeConstraints.Add(new EqualType(astId, inferred.Type, expectedType));
            return inferred;
        }

        //private static FormalArguments CheckArgumentTypeEquality(uint astId, FormalArguments args, ProductType prodType, Context ctx)
        //{
        //    List<ISemExp> checkedArgs = [];

        //    int i = 0;
        //    for (; i < int.Min(args.Values.Length, prodType.Types.Length); ++i)
        //    {
        //        if (prodType.Types[i] is HomogenousListType hlt)
        //        {
        //            while (i < args.Values.Length)
        //            {
        //                ISemExp checkedArg = InferTypedExpression(args.Values[i], ctx);
        //                ctx.TypeConstraints.Add(new SubType(astId, checkedArg.Type, hlt.RepeatingType));
        //                checkedArgs.Add(checkedArg);
        //            }
        //        }
        //        else
        //        {
        //            ISemExp checkedArg = InferTypedExpression(args.Values[i], ctx);
        //            ctx.TypeConstraints.Add(new SubType(astId, checkedArg.Type, prodType.Types[i]));
        //            checkedArgs.Add(checkedArg);
        //        }
        //    }

        //    return args with
        //    {
        //        Values = [.. checkedArgs]
        //    };
        //}

        //private static FormalParameters CheckParameterTypeEquality(uint astId, FormalParameters parms, SchemeType prodType, Context ctx)
        //{
        //    List<SemVar> checkedParms = [];

        //    int i = 0;
        //    for (; i < int.Min(parms.Values.Length, prodType.Types.Length); ++i)
        //    {
        //        if (prodType.Types[i] is HomogenousListType hlt)
        //        {
        //            while (i < parms.Values.Length)
        //            {
        //                SemVar checkedParm = InferTypedBindingVariable(parms.Values[i], ctx);
        //                ctx.TypeConstraints.Add(new SubType(astId, checkedParm.Type, hlt.RepeatingType));
        //                checkedParms.Add(checkedParm);
        //            }
        //        }
        //        else
        //        {
        //            SemVar checkedParm = InferTypedBindingVariable(parms.Values[i], ctx);
        //            ctx.TypeConstraints.Add(new SubType(astId, checkedParm.Type, prodType.Types[i]));
        //            checkedParms.Add(checkedParm);
        //        }
        //    }

        //    return parms with
        //    {
        //        Values = [.. checkedParms]
        //    };
        //}

        //private static Lambda CheckFunctionTypeEquality(uint astId, Lambda lam, FunctionType funType, Context ctx)
        //{
        //    List<SemVar> checkedParameters = [];
        //    SemVar? checkedVariad = null;

        //    int i = 0;

        //    // consume all parameters up to the point at which the inputs differ in length
        //    for (; i < int.Min(lam.Parameters.Length, funType.InputTypes.Length); ++i)
        //    {
        //        SemVar inferredVar = InferTypedBindingVariable(lam.Parameters[i], ctx);
        //        checkedParameters.Add(inferredVar);
        //        ctx.TypeConstraints.Add(new EqualType(astId, inferredVar.Type, funType.InputTypes[i]));
        //    }

        //    // are there more parameters presented than types for them to hold?
        //    if (i < lam.Parameters.Length
        //        && lam.Variad is not null)
        //    {
        //        //maybe they're just variadic??
        //        foreach (SemVar extra in lam.Parameters.Skip(i))
        //        {
        //            SemVar checkedExtra = InferTypedBindingVariable(extra, ctx);
        //            ctx.TypeConstraints.Add(new EqualType(astId, checkedExtra.Type, funType.VariadicType));
        //        }
        //    }

        //    // are there more types than parameters that can hold them all?
        //    if (i < funType.InputTypes.Length
        //        && lam.Variad is not null)
        //    {
        //        //try cramming them into the variad if we can
        //        SchemeType variadType = new UnionType(funType.InputTypes.Skip(i));
        //        checkedVariad = InferTypedBindingVariable(lam.Variad, ctx);
        //        ctx.TypeConstraints.Add(new EqualType(astId, checkedVariad.Type, variadType));
        //    }

        //    Body bod = InferTypedBody(lam.Body, ctx);

        //    ctx.TypeConstraints.Add(new EqualType(astId, bod.Type, funType.OutputType));

        //    return lam with
        //    {
        //        Parameters = [.. checkedParameters],
        //        Variad = checkedVariad,
        //        Body = bod
        //    };
        //}

        #endregion

    }
}