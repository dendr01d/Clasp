using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Terms
{
    internal readonly struct Character : ITerm, IEquatable<Character>
    {
        public readonly char Value;

        public Character(char c) => Value = c;

        public byte[] GetBytes() => [(byte)Value];

        public bool Equals(Character other) => Value == other.Value;

        public bool Equals(ITerm? other) => other is Character c && Equals(c);

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Character c
                && Equals(c);
        }

        public override int GetHashCode() => (int)Value;
    }
}
