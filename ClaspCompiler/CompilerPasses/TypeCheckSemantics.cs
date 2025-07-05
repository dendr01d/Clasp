namespace ClaspCompiler.CompilerPasses
{
    //internal static class TypeCheckSemantics
    //{
    //    public static Prog_Sem Execute(Prog_Sem program)
    //    {
    //        Dictionary<Var, SchemeType> map = [];

    //        ISemExp typedBody = TypeCheckExpression(program.Body, map).Expression;

    //        return new Prog_Sem(map, typedBody);
    //    }

    //    private static TypedExpression TypeCheckExpression(ISemExp exp, Dictionary<Var, SchemeType> map)
    //    {
    //        return exp switch
    //        {
    //            TypedExpression tExp => tExp,

    //            Let let => TypeCheckLet(let.Variable, let.Argument, let.Body, map),
    //            If iff => TypeCheckIf(iff.Condition, iff.Consequent, iff.Alternative, map),

    //            PrimitiveApplication primApp => TypeCheckPrimitiveApp(primApp.Operator, primApp.Arguments, map),

    //            IAtom atm => TypeCheckValue(atm),
    //            Var v => TypeCheckVariable(v, map),

    //            _ => throw new Exception($"Can't type unknown expression: {exp}")
    //        };
    //    }

    //    private static TypedExpression TypeCheckValue(IAtom val)
    //    {
    //        return val switch
    //        {
    //            Integer i => new(AtomicType.Integer, i),
    //            Boole b => new(AtomicType.Boole, b),

    //            _ => throw new Exception($"Can't type unknown value: {val}")
    //        };
    //    }

    //    private static TypedExpression TypeCheckVariable(Var v, Dictionary<Var, SchemeType> map)
    //    {
    //        if (map.TryGetValue(v, out SchemeType? type))
    //        {
    //            return new(type, v);
    //        }
    //        throw new Exception($"Can't type undefined variable: {v}");
    //    }

    //    private static TypedExpression TypeCheckPrimitiveApp(PrimitiveOperator op, ISemExp[] args, Dictionary<Var, SchemeType> map)
    //    {
    //        if (op == PrimitiveOperator.Eq)
    //        {
    //            if (args.Length == 2
    //                && TypeCheckExpression(args[0], map) is TypedExpression argA
    //                && TypeCheckExpression(args[1], map) is TypedExpression argB
    //                && argA.Type == argB.Type)
    //            {
    //                return new TypedExpression(AtomicType.Boole, new PrimitiveApplication(op, args));
    //            }
    //            else
    //            {
    //                throw new Exception($"Type error: Wrong arguments to {op}: {args}");
    //            }
    //        }
    //        else
    //        {
    //            FunctionType fType = op.GetSchemeType();

    //            foreach(var (First, Second) in args.Zip(fType.ArgumentTypes))
    //            {
    //                AssertType(First, Second, map);
    //            }

    //            return new TypedExpression(fType.OutputType, new PrimitiveApplication(op, args));
    //        }
    //    }

    //    private static void AssertType(ISemExp exp, SchemeType expectedType, Dictionary<Var, SchemeType> map)
    //    {
    //        TypedExpression result = TypeCheckExpression(exp, map);

    //        if (result.Type != expectedType)
    //        {
    //            throw new Exception($"Type error: Expected type of {expectedType}: {result}");
    //        }
    //    }

    //    private static TypedExpression TypeCheckLet(Var v, ISemExp value, ISemExp body, Dictionary<Var, SchemeType> map)
    //    {
    //        TypedExpression typedValue = TypeCheckExpression(value, map);
    //        map[v] = typedValue.Type;

    //        TypedExpression typedBody = TypeCheckExpression(body, map);

    //        return new TypedExpression(typedBody.Type, new Let(v, value, body));
    //    }

    //    private static TypedExpression TypeCheckIf(ISemExp cond, ISemExp consq, ISemExp alt, Dictionary<Var, SchemeType> map)
    //    {
    //        TypedExpression typedCond = TypeCheckExpression(cond, map);

    //        if (typedCond.Type != AtomicType.Boole)
    //        {
    //            throw new Exception($"Type error: expected boolean condition: {typedCond}");
    //        }

    //        TypedExpression typedConsq = TypeCheckExpression(consq, map);
    //        TypedExpression typedAlt = TypeCheckExpression(alt, map);

    //        if (typedConsq.Type != typedAlt.Type)
    //        {
    //            throw new Exception($"Type error: mismatched conditional branches: {typedConsq} | {typedAlt}");
    //        }

    //        return new TypedExpression(typedConsq.Type, new If(cond, consq, alt));
    //    }
    //}
}
