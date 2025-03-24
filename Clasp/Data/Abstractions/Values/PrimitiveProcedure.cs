using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Values
{
    /// <summary>
    /// Representsa primitive operation on <see cref="IAbstractValue"/> data.
    /// </summary>
    internal readonly struct PrimitiveProcedure : IAbstractValue
    {
        public readonly PrimitiveOperation Value;
        public int Size => sizeof(PrimitiveOperation);
    }

    internal enum PrimitiveOperation: uint
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,

        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,

        Bitwise_Not,
        Bitwise_And,
        Bitwise_Or,
        Bitwise_Xor
    }
}
