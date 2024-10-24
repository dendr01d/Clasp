using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib.ExpressionTypes
{
    public interface IExpression
    {

    }

    public static class ExpressionExtensions
    {
        public static bool Pred_Eq(this IExpression ex, IExpression other)
        {
            return ReferenceEquals(ex, other);
        }

        public static bool Pred_Eqv(this IExpression ex, IExpression other)
        {

        }

        public static bool Pred_Equal(this IExpression ex, IExpression other)
        {

        }

        public static bool IsNull(this IExpression ex) => ex is IEmpty;
        public static bool IsFalse(this IExpression ex) => ex is IBoolean b && !b.Value;
        public static bool IsTrue(this IExpression ex) => !ex.IsFalse();
    }
}
