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
        
        // Memory Management
        Frame_Load, Frame_Store, Frame_Swap,

        Local_Load, Local_Store, Local_Swap,
        Local_Push, Local_Pop, Local_Shuffle,

        Global_Load, Global_Store,
        Global_Push, Global_Pop,

        Const_Boolean, Const_Byte, Const_Char,
        Const_FixNum, Const_FloNum, Const_Raw,

        // Comparisons
        Eq, Neq,
        Lt, Leq,
        Gt, Geq,

        // Unsigned Integral Arithmetic
        Raw_Add, Raw_Mul, Raw_Sub, Raw_Div, Raw_Mod,

        // Signed Integral Arithmetic
        Fix_Add, Fix_Mul, Fix_Sub, Fix_Div, Fix_Mod,

        // Floating-Point Arithmetic
        Flo_Add, Flo_Mul, Flo_Sub, Flo_Div, Flo_Mod,

        // Bitwise Arithmetic
        Shift_L, Shift_R,
        Bitwise_And, Bitwise_Or, Bitwise_Xor,

        // Type-Casting
        TypeCast,
        Raw_To_Fix, Flo_To_Fix,
        Raw_To_Flo, Fix_To_Flo,
        Fix_To_Raw, Flo_To_Raw,

        // Box Ops
        Box,
        Unbox,

        // List Ops
        Cons,
        Car,
        Cdr,
        Set_Car,
        Set_Cdr,

        // Vector Ops
        Vec_Make,
        Vec_Emplace,
        Vec_Retrieve,
        Vec_Length,

        // Functional Ops
        Func_Make,
        Func_Apply,

        // Port Ops
        Port_Get_Console,
        Port_Get_File,
        Port_Open_Write,
        Port_Open_Read,
    }
}
