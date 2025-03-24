using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Terms
{
    internal struct Box<T> : ITerm, IEquatable<Box<T>>
        where T : ITerm
    {
        public readonly T BoxedValue => (T)_ref;
        private readonly ITerm _ref;

        public Box(T value)
        {
            _ref = value;
        }

        public bool Equals(Box<T> other) => ReferenceEquals(_ref, other._ref);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Box<T> bx && Equals(bx);
        public override int GetHashCode() => HashCode.Combine(BoxedValue.GetHashCode());
    }
}
