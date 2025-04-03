using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine
{
    public enum OpCode : byte
    {
        // General Operations
        Op_Return,

        // Jump
        Jump,
        Jump_If,
        Jump_IfNot,
        
        // Reading & Writing Values
        Local_Load1, Local_Store1, Local_Pop1,
        Local_Load2, Local_Store2, Local_Pop2,
        Local_Load4, Local_Store4, Local_Pop4,
        Local_Load8, Local_Store8, Local_Pop8,
        Local_Push,

        Global_Load1, Global_Store1, Global_Pop1,
        Global_Load2, Global_Store2, Global_Pop2,
        Global_Load4, Global_Store4, Global_Pop4,
        Global_Load8, Global_Store8, Global_Pop8,
        Global_Push,

        // List Ops
        Cons,
        Car,
        Cdr,
        Set_Car,
        Set_Cdr,

        // Comparisons
        Eq, Neq,
        Lt, Leq,
        Gt, Geq,

        // Integral Arithmetic
        Fix_Add, Fix_Mul, Fix_Sub, Fix_Div, Fix_Mod,

        // Floating-Point Arithmetic
        Flo_Add, Flo_Mul, Flo_Sub, Flo_Div, Flo_Mod,

        // Bitwise Arithmetic
        Shift_L, Shift_R,
        Bitwise_And, Bitwise_Or, Bitwise_Xor,
    }
}
