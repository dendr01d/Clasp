﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine
{
    public sealed class RegisterStack<T>
        where T : struct
    {
        private T[] _stack;
        private int _pointer;
        private int _size;

        public int Length => _pointer;
        public int Capacity => _size;

        public T this[int i]
        {
            get { return _stack[i]; }
        }

        private const int DEFAULT_SIZE = 8;

        public RegisterStack()
        {
            _stack = new T[DEFAULT_SIZE];
            _pointer = 0;
        }

        public Span<T> Slice(int index, int length) => new Span<T>(_stack).Slice(index, length);

        public void Push(T value)
        {
            if (_pointer >= _size)
            {
                ReAllocate();
            }

            _stack[_pointer++] = value;
        }

        public void Push(Span<T> values)
        {
            while (_pointer + values.Length >= _size)
            {
                ReAllocate();
            }

            for (int i = 0; i < values.Length; ++i)
            {
                _stack[_pointer++] = values[i];
            }
        }

        public T Pop() => _stack[_pointer--];
        public T Peek() => _stack[_pointer - 1];

        public Span<T> PopValues(int length)
        {
            _pointer -= length;
            return Slice(_pointer, length);
        }

        public Span<T> PeekValues(int length)
        {
            return Slice(_pointer - length, length);
        }

        private void ReAllocate()
        {
            _size *= 2;
            T[] resizedStack = new T[_size];
            _stack.CopyTo(new Span<T>(resizedStack));
            _stack = resizedStack;
        }
    }
}
