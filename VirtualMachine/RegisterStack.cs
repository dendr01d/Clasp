using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMachine.Objects;

namespace VirtualMachine
{
    /// <summary>
    /// Implements a Stack of Terms that provides random access to its contents.
    /// </summary>
    internal sealed class RegisterStack
    {
        private Term[] _block;
        private int _stackTop;
        private int _stackSize;

        public int Length => _stackTop;
        public int Capacity => _stackSize;

        public Term this[int i]
        {
            get { return _block[i]; }
            set
            {
                if (i > _stackTop)
                {
                    throw new IndexOutOfRangeException($"Tried to access invalid index {i} in {nameof(RegisterStack)} of length {Length} and capacity {Capacity}.");
                }
                _block[i] = value;
            }
        }

        private const int DEFAULT_SIZE = 64;

        public RegisterStack()
        {
            _block = new Term[DEFAULT_SIZE];
            _stackTop = 0;
            _stackSize = DEFAULT_SIZE;
        }

        public void Push(Term t)
        {
            if (_stackTop >= _stackSize)
            {
                ReAllocate();
            }

            _block[_stackTop++] = t;
        }

        public Term Pop() => _block[_stackTop--];
        public Term Peek() => _block[_stackTop - 1];

        /// <summary>
        /// Pop the given number of values from the top of the stack. The last
        /// term to be popped is returned.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Term PopValues(int count)
        {
            _stackTop -= count;
            return _block[_stackTop + 1];
        }

        private void ReAllocate()
        {
            _stackSize *= 2;
            Term[] resizedStack = new Term[_stackSize];
            _block.CopyTo(resizedStack, 0);
            _block = resizedStack;
        }
    }
}
