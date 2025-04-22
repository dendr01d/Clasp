using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Objects
{
    /// <summary>
    /// Represents a heap-allocated <see cref="Term"/> that can be accessed only indirectly
    /// (via "unboxing"). The contents of the box are mutable, and changing the contents of one
    /// box changes the contents of all boxes that refer to the same heap-allocated <see cref="Term"/>.
    /// </summary>
    class Box
    {
        public Term Contents { get; private set; }

        public Box(Term slot) => Contents = slot;

        public void Mutate(Term newContents) => Contents = newContents;
    }
}
