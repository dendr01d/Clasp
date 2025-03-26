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
        Jump_Eq,
        Jump_Ne,
        
        // Reading & Writing Values
        Load_Local,
        Mutate_Local,
        Push_Local,
        Pop_Local,

        Load_Global,
        Mutate_Global,
        Push_Global,
        Pop_Global,

        Load_Static,

        // List Ops
        Cons,
        Car,
        Cdr,
        Set_Car,
        Set_Cdr,

        // Comparisons
        Eq,
        Neq,
        Lt,
        Leq,
        Gt,
        Geq,

        // Integral Arithmetic
        IAdd,
        IMul,
        ISub,
        SDiv,
        UDiv,
        SMod,
        UMod,

        // Floating-Point Arithmetic
        FAdd,
        FMul,
        FSub,
        FDiv,
        FMod,

        // Bitwise Arithmetic
        Bitwise_Xor,
        Bitwise_Or, 
        Bitwise_And,

        // Comparisons
        //Const_Eq, Local_Eq,
        //Const_Ne, Local_Ne,
        //Const_Lt, Local_Lt,
        //Const_Le, Local_Le,
        //Const_Gt, Local_Gt,
        //Const_Ge, Local_Ge,

        // Integer (long) Arithmetic
        //Const_IAdd, Local_IAdd,
        //Const_IMul, Local_IMul,
        //Const_ISub, Local_ISub,
        //Const_SDiv, Local_SDiv,
        //Const_UDiv, Local_UDiv,
        //Const_SMod, Local_SMod,
        //Const_UMod, Local_UMod,

        // Floating-point (double) Arithmetic
        //Const_FAdd, Local_FAdd,
        //Const_FMul, Local_FMul,
        //Const_FSub, Local_FSub,
        //Const_FDiv, Local_FDiv,
        //Const_FMod, Local_FMod,
    }
}
