using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspCompiler.IntermediateCil.Abstract
{
    internal enum CilOp : int
    {
        Load,
        Store,
        Call = 0x28,

        Dupe = 0x25,
        Pop = 0x26,

        Return = 0x2A,

        Br = 0x38,
        BrIfn = 0x39,
        BrIf = 0x3A,
        BrEq = 0x3B,
        BrGeq = 0x3C,
        BrGt = 0x3D,
        BrLeq = 0x3E,
        BrLt = 0x3F,

        Add = 0x58,
        Sub = 0x59,
        Neg = 0x65,

        BitwiseAnd = 0x5F,
        BitwiseOr = 0x60,
        BitwiseXor = 0x61,
        BitwiseNot = 0x66,

        Tail = 0x14
    }
}
