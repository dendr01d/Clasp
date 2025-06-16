using System.Collections.Immutable;

using ClaspCompiler.CompilerData;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal enum PrimitiveOperator
    {
        Read,

        Add, Sub, Neg,

        Eq, Lt, LtE, Gt, GtE,

        Not,

        And, Or,

        Vector, VectorRef, VectorSet
    }

    internal static class PrimitiveOperatorExtensions
    {
        public static string Stringify(this PrimitiveOperator op)
        {
            return op switch
            {
                PrimitiveOperator.Read => Keyword.READ,

                PrimitiveOperator.Add => Keyword.PLUS,
                PrimitiveOperator.Sub => Keyword.MINUS,
                PrimitiveOperator.Neg => Keyword.MINUS,

                PrimitiveOperator.Eq => Keyword.EQ,
                PrimitiveOperator.Lt => Keyword.LT,
                PrimitiveOperator.LtE => Keyword.LTE,
                PrimitiveOperator.Gt => Keyword.GT,
                PrimitiveOperator.GtE => Keyword.GTE,

                PrimitiveOperator.Not => Keyword.NOT,

                PrimitiveOperator.And => Keyword.AND,
                PrimitiveOperator.Or => Keyword.OR,

                PrimitiveOperator.Vector => Keyword.VECTOR,
                PrimitiveOperator.VectorRef => Keyword.VECTORREF,
                PrimitiveOperator.VectorSet => Keyword.VECTORSET,

                _ => throw new Exception($"Can't stringify unknown operator: {op}")
            };
        }

        private static readonly PrimitiveOperator[] _impureOps =
        [
            PrimitiveOperator.Read,
            PrimitiveOperator.VectorSet
        ];

        public static bool HasSideEffect(this PrimitiveOperator op) => _impureOps.Contains(op);

        private static readonly PrimitiveOperator[] _cmpOps =
        [
            PrimitiveOperator.Eq,
            PrimitiveOperator.Lt,
            PrimitiveOperator.LtE,
            PrimitiveOperator.Gt,
            PrimitiveOperator.GtE
        ];

        public static bool IsComparison(this PrimitiveOperator op) => _cmpOps.Contains(op);

        //public static FunctionType GetSchemeType(this PrimitiveOperator op)
        //{
        //    return op switch
        //    {
        //        PrimitiveOperator.Read => new(AtomicType.Integer),

        //        PrimitiveOperator.Add => new(AtomicType.Integer, AtomicType.Integer, AtomicType.Integer),
        //        PrimitiveOperator.Sub => new(AtomicType.Integer, AtomicType.Integer, AtomicType.Integer),
        //        PrimitiveOperator.Neg => new(AtomicType.Integer, AtomicType.Integer),

        //        PrimitiveOperator.Lt => new(AtomicType.Boole, AtomicType.Integer, AtomicType.Integer),
        //        PrimitiveOperator.LtE => new(AtomicType.Boole, AtomicType.Integer, AtomicType.Integer),
        //        PrimitiveOperator.Gt => new(AtomicType.Boole, AtomicType.Integer, AtomicType.Integer),
        //        PrimitiveOperator.GtE => new(AtomicType.Boole, AtomicType.Integer, AtomicType.Integer),

        //        PrimitiveOperator.Not => new(AtomicType.Boole, AtomicType.Boole),

        //        PrimitiveOperator.And => new(AtomicType.Boole, AtomicType.Boole, AtomicType.Boole),
        //        PrimitiveOperator.Or => new(AtomicType.Boole, AtomicType.Boole, AtomicType.Boole),

        //        _ => throw new Exception($"Can't automatically type operator: {op}")
        //    };
        //}
    }
}
