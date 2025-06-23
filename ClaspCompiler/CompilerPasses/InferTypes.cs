using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract.TypeConstraints;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.CompilerPasses
{
    internal static class InferTypes
    {
        public static Prog_Sem Execute(Prog_Sem program)
        {
            Dictionary<SemVar, SchemeType> variableTypes = [];
            HashSet<TypeConstraint> typeConstraints = [];

            ISemExp inferredBody = InferExpressionType(program.TopLevelForms, variableTypes, typeConstraints);

            return new Prog_Sem(inferredBody)
            {
                VariableTypes = variableTypes,
                TypeConstraints = typeConstraints
            };
        }

        private static ISemExp InferExpressionType(ISemExp exp, Dictionary<SemVar, SchemeType> varTypes, HashSet<TypeConstraint> constraints)
        {
            switch (exp)
            {
                case SemDatum datum:
                    // self-typed with whatever S-expression it contains
                    return datum;

                case SemVar v:
                    // expect to have typed it during its binding
                    if (varTypes.TryGetValue(v, out SchemeType? boundVarType))
                    {
                        return new SemVar(v.Name, new MetaData()
                        {
                            Type = boundVarType
                        });
                    }
                    throw new Exception($"Can't infer type of unbound variable: {v}");

                case Lambda lam:
                    SemVar inferredVar = BindVariableType(lam.Variable, new TypeVar(), varTypes);
                    ISemExp inferredBody = InferExpressionType(lam.Body, varTypes, constraints);
                    return new Lambda(inferredVar, inferredBody, new MetaData()
                    {
                        Type = new FunctionType(inferredVar.MetaData.Type, inferredBody.MetaData.Type)
                    });

                case SemApp app:
                    ISemExp[] typedArgs = [.. app.Arguments.Select(x => InferExpressionType(x, varTypes, constraints))];
                    SchemeType argsType = typedArgs.Length switch
                    {
                        0 => AtomicType.Void,
                        1 => typedArgs[0].MetaData.Type,
                        _ => new ProductType(typedArgs.Select(x => x.MetaData.Type))
                    };
                    SchemeType appReturns = new TypeVar();
                    SchemeType expectedFun = new FunctionType(argsType, appReturns);
                    return CheckExpressionType(app.Procedure, expectedFun, varTypes, constraints);

                case If iff:
                    ISemExp inferredCond = InferExpressionType(iff.Condition, varTypes, constraints);
                    ISemExp inferredConsq = InferExpressionType(iff.Consequent, varTypes, constraints);
                    ISemExp inferredAlt = InferExpressionType(iff.Alternative, varTypes, constraints);
                    return new If(inferredCond, inferredConsq, inferredAlt, new MetaData()
                    {
                        Type = SumType.Sum(inferredConsq.MetaData.Type, inferredAlt.MetaData.Type)
                    });

                default:
                    throw new Exception($"Can't infer type of unknown expression: {exp}");
            }
        }

        private static SemVar BindVariableType(SemVar v, SchemeType boundType, Dictionary<SemVar, SchemeType> varTypes)
        {
            if (varTypes.ContainsKey(v) || v.MetaData.Type != AtomicType.Unknown)
            {
                throw new Exception($"Can't re-bind variable {v} of type {varTypes[v]} to type {boundType}.");
            }

            varTypes[v] = boundType;
            return new SemVar(v.Name, new MetaData()
            {
                Type = boundType
            });
        }

        private static ISemExp CheckExpressionType(ISemExp exp, SchemeType expectedType,
            Dictionary<SemVar, SchemeType> varTypes, HashSet<TypeConstraint> constraints)
        {
            switch (exp)
            {
                case SemDatum dat when dat.Datum is Integer && expectedType == AtomicType.Integer:
                    return exp;

                case SemDatum dat when dat.Datum is Boole && expectedType == AtomicType.Boole:
                    return exp;

                case Lambda lam when expectedType is FunctionType funType:
                    SemVar checkedVar = BindVariableType(lam.Variable, funType.ArgumentType, varTypes);
                    ISemExp checkedBody = CheckExpressionType(lam.Body, funType.OutputType, varTypes, constraints);
                    return new Lambda(checkedVar, checkedBody, new MetaData()
                    {
                        Type = funType
                    });

                default:
                    ISemExp inferredExp = InferExpressionType(exp, varTypes, constraints);
                    constraints.Add(new TypeEqual(exp, expectedType, inferredExp.MetaData.Type));
                    return inferredExp;
            }
        }

    }
}
