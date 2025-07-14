using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeTypes;
using ClaspCompiler.SchemeTypes.TypeConstraints;

namespace ClaspCompiler.CompilerPasses
{
    internal static class ResolveSemanticTypes
    {
        public static Prog_Sem Execute(Prog_Sem program)
        {
            //foreach (TypeConstraint constraint in program.TypeConstraints)
            //{
            //    if (ResolveConstraint(constraint, program.TypeUnification))
            //    {
            //        Console.WriteLine("Resolved: {0}", constraint);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Failed to resolve: {0}", constraint);
            //    }
            //}
            //Console.WriteLine();

            return program;
        }

        private static bool ResolveConstraint(TypeConstraint constraint, DisjointTypeSet unifier)
        {
            return constraint switch
            {
                TypeEqualsType eq => UnifyTypes(eq.TypeA, eq.TypeB, unifier),
                _ => throw new Exception($"Can't resolve unknown constraint type: {constraint}"),
            };
        }

        private static bool UnifyTypes(SchemeType typeA, SchemeType typeB, DisjointTypeSet unifier)
        {
            throw new NotImplementedException();
        }

        //private static bool UnifyTypes(SchemeType typeA, SchemeType typeB, DisjointTypeSet unifier)
        //{
        //    SchemeType normalizedA = NormalizeType(typeA, unifier);
        //    SchemeType normalizedB = NormalizeType(typeB, unifier);

        //    return (normalizedA, normalizedB) switch
        //    {
        //        (AtomicType atA, AtomicType atB) => atA == atB,

        //        (FunctionType ftA, FunctionType ftB) => UnifyFunctionTypes(ftA, ftB, unifier),

        //        (PairType ptA, PairType ptB) => UnifyPairTypes(ptA, ptB, unifier),

        //        (ProductType prA, ProductType prB) => UnifyProductTypes(prA, prB, unifier),

        //        //(UnionType ut, _) => UnifyUnionType(ut, normalizedB, unifier),
        //        //(_, UnionType ut) => UnifyUnionType(ut, normalizedA, unifier),

        //        (VarType vtA, VarType vtB) => unifier.Union(vtA, vtB) || unifier.Find(vtA) == unifier.Find(vtB),
        //        (VarType vt, _) => UnifyVarTypes(vt, normalizedB, unifier),
        //        (_, VarType vt) => UnifyVarTypes(vt, normalizedA, unifier),

        //        _ => throw new Exception($"Can't unify with unknown types: {normalizedA} // {normalizedB}")
        //    };
        //}

        //private static bool UnifyFunctionTypes(FunctionType typeA, FunctionType typeB, DisjointTypeSet unifier)
        //{
        //    return UnifyTypes(typeA.InputType, typeB.InputType, unifier)
        //        && UnifyTypes(typeA.OutputType, typeB.OutputType, unifier);
        //}

        //private static bool UnifyPairTypes(PairType typeA, PairType typeB, DisjointTypeSet unifier)
        //{
        //    return UnifyTypes(typeA.Car, typeB.Car, unifier)
        //        && UnifyTypes(typeA.Cdr, typeB.Cdr, unifier);
        //}

        //private static bool UnifyProductTypes(ProductType typeA, ProductType typeB, DisjointTypeSet unifier)
        //{
        //    //return typeA.Types.Length == typeB.Types.Length
        //    //    && typeA.Types.Zip(typeB.Types).All(x => UnifyTypes(x.First, x.Second, unifier));

        //    int i = 0;
        //    for (; i < int.Min(typeA.Types.Length, typeB.Types.Length); ++i)
        //    {
        //        if (typeA.Types[i] is HomogenousListType hlt)
        //        {
        //            while (i < typeB.Values.Length)
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
        //}

        // this is probably VERY wrong
        //private static bool UnifyUnionType(UnionType typeA, SchemeType typeB, DisjointTypeSet unifier)
        //{
        //    if (typeB is UnionType castB)
        //    {
        //        return typeA.Types.SetEquals(castB.Types);
        //    }
        //    else
        //    {
        //        return typeA.Types.Contains(typeB);
        //    }
        //}

        //private static bool UnifyVarTypes(VarType typeA, SchemeType typeB, DisjointTypeSet unifier)
        //{
        //    //if (CheckRecurrence(typeA, typeB))
        //    //{
        //    //    throw new Exception($"Infinite type (I think...?)");
        //    //}
        //    //else
        //    //{
        //    //    return unifier.Union(typeA, typeB);
        //    //}
        //}

        //private static bool CheckRecurrence(VarType vt, SchemeType type)
        //{
        //    if (vt == type)
        //    {
        //        return true;
        //    }

        //    return type switch
        //    {
        //        FunctionType ft => CheckRecurrence(vt, ft.OutputType) || CheckRecurrence(vt, ft.InputType),
        //        HomogenousListType hlt => CheckRecurrence(vt, hlt.RepeatingType),
        //        PairType pt => CheckRecurrence(vt, pt.Car) || CheckRecurrence(vt, pt.Cdr),
        //        ProductType pr => pr.Types.Any(x => CheckRecurrence(vt, x)),
        //        UnionType ut => ut.Types.Any(x => CheckRecurrence(vt, x)),
        //        _ => false
        //    };
        //}

        //private static SchemeType NormalizeType(SchemeType type, DisjointTypeSet uni)
        //{
        //    switch (type)
        //    {
        //        case AtomicType:
        //            return type;

        //        case FunctionType ft:
        //            SchemeType ftOut = NormalizeType(ft.OutputType, uni);
        //            ProductType ftIn = new ProductType(ft.InputType.Types.Select(x => NormalizeType(x, uni)));
        //            return new FunctionType(ftOut, ftIn);

        //        case HomogenousListType hlt:
        //            return new HomogenousListType(NormalizeType(hlt.RepeatingType, uni));

        //        case PairType pt:
        //            SchemeType ptCar = NormalizeType(pt.Car, uni);
        //            SchemeType ptCdr = NormalizeType(pt.Cdr, uni);
        //            return new PairType(ptCar, ptCdr);

        //        case ProductType pr:
        //            return new ProductType(pr.Types.Select(x => NormalizeType(x, uni)));
        //        //return pr.Types.Length switch
        //        //{
        //        //    0 => AtomicType.Void,
        //        //    1 => NormalizeType(pr.Types.First(), uni),
        //        //    _ => new ProductType(pr.Types.Select(x => NormalizeType(x, uni)))
        //        //};

        //        case UnionType ut:
        //            return ut.Types.Count switch
        //            {
        //                0 => AtomicType.Void,
        //                1 => NormalizeType(ut.Types.First(), uni),
        //                _ => new UnionType(ut.Types.Select(x => NormalizeType(x, uni)))
        //            };

        //        case VarType vt:
        //            SchemeType outType = uni.Find(vt);
        //            return outType == vt
        //                ? outType
        //                : NormalizeType(outType, uni);

        //        default:
        //            throw new Exception($"Can't normalize unknown type: {type}");
        //    }
        //}


        private static bool CheckSubtypingRelation(SchemeType? subType, SchemeType? superType, DisjointTypeSet unifier)
        {
            if (subType is null || superType is null)
            {
                return false;
            }
            else if (subType is FunctionType f1 && superType is FunctionType f2)
            {
                return (f1.LatentPredicate == f2.LatentPredicate || f2.LatentPredicate is null)
                    && f1.InputTypes.Length == f2.InputTypes.Length
                    && f1.InputTypes.Zip(f2.InputTypes).All(x => CheckSubtypingRelation(x.First, x.Second, unifier))
                    && ((f1.PreType is null && f2.PreType is null) || CheckSubtypingRelation(f1.PreType, f2.PreType, unifier))
                    && CheckSubtypingRelation(f1.OutputType, f2.OutputType, unifier);
            }
            else if (superType is UnionType superUnion)
            {
                return superUnion.Types.All(x => CheckSubtypingRelation(subType, x, unifier));
            }
            else if (subType is UnionType subUnion)
            {
                return subUnion.Types.All(x => CheckSubtypingRelation(x, superType, unifier));
            }
            else
            {
                return UnifyTypes(subType, superType, unifier);
            }
        }

    }
}
