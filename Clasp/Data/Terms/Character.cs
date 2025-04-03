using System;

namespace Clasp.Data.Terms
{
    internal readonly struct Character : ITerm, IEquatable<Character>
    {
        private readonly byte _value;
        public readonly char Value => (char)_value;

        public Character(char c) => _value = (byte)c;
        public bool Equals(Character other) => Value == other.Value;
        public bool Equals(ITerm? other) => other is Character c && Equals(c);
        public override bool Equals(object? other) => other is Character c && Equals(c);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString()
        {
            return string.Format("#\\{0}", Value switch
            {
                '\n' => "newline",
                '\t' => "tab",
                ' ' => "space",
                _ => Value
            });
        }
    }
}
