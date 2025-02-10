using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Primitives
{
    internal enum Primitive
    {
        /// <summary> Pair Constructor </summary>
        CONS,
        /// <summary> Pair Destructor </summary>
        CAR,
        /// <summary> Pair Destructor </summary>
        CDR,

        LIST,
        VECTOR,

        /// <summary> Arithmetic Add </summary>
        PLUS,
        /// <summary> Arithmetic Subtract </summary>
        MINUS,
        /// <summary> Arithmetic Multiply </summary>
        MULTIPLY,
        /// <summary> Arithmetic Divide </summary>
        DIVIDE,
        /// <summary> Arithmetic Quotient </summary>
        QUOTIENT,

        /// <summary> Numeric Less-Than Comparison </summary>
        LT,
        /// <summary> Numeric Greater-Than Comparison </summary>
        GT,
        /// <summary> Numeric Value-Equality Comparison </summary>
        EQ,
        /// <summary> Numeric Less-Than-Or-Equal Comparison </summary>
        LEQ,
        /// <summary> Numeric Greater-Than-Or-Equal Comparison </summary>
        GEQ,

        /// <summary> Syntax-Object Constructor </summary>
        MK_SYNTAX,
        /// <summary> Syntax-Object Destructor </summary>
        SYNTAX_E,
    }
}
