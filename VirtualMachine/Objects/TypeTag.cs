using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Objects
{
    internal enum TypeTag : byte
    {
        /* 000-099 reserved for Static Instance types */

        Nil          = 000, // Specially-instanced types
        Void         = 001,
        Undefined    = 002,

        /* 100-199 reserved for Raw Value types */

        //100-119 are unsigned, for the sake of considering them numerically
        Boolean      = 100,
        Byte         = 101,
        Character    = 102, // Not sure how C# is representing these on the backend? Presumably unsigned

        FixNum       = 120, // Signed integer

        FloNum       = 130, // Signed floating-point

        /* 200-255 reserved for C# managed Reference types */

        Symbol       = 200, // Kind of like a string, but semantically different
        CharString   = 201, // C#-Managed string, which is a reference type
        Box          = 202,
        Cons         = 203,
        Vector       = 204,
        Functional   = 205,
        PortReader   = 206, // C# Stream object
        PortWriter   = 207, // Ditto

    }
}
