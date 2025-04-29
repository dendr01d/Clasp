using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace VirtualMachine.Objects
{
    /// <summary>
    /// Represents a single term in the CLASP language. May be comprised of either
    /// a static or dynamic value.
    /// </summary>
    [DebuggerDisplay("{Tag}: {ToString()}")]
    [StructLayout(LayoutKind.Explicit)]
    internal readonly partial struct Term : IEquatable<Term>, IComparable<Term>
    {
        #region Bit-Masking

        // Written in little-endian
        private const ulong QNAN = 0x_00_00_00_00_00_00_FC_7F;
        private const ulong BSTR = 0x_FF_FF_FF_FF_FF_FF_00_00;

        #endregion

        #region Primitive Fields
        // Raw values that can be represented by the term
        // These need to align such that their LEAST significant bits overlap

        [FieldOffset(0)] private readonly double _double; //8 bytes
        [FieldOffset(0)] private readonly ulong _ulong; //8 bytes

        [FieldOffset(4)] private readonly int _int; //4 bytes

        [FieldOffset(6)] private readonly char _char; //2 bytes

        [FieldOffset(7)] private readonly byte _byte; //1 byte
        [FieldOffset(7)] private readonly bool _bool; //1 byte
        #endregion

        #region Metadata Fields

        [FieldOffset(2)] private readonly byte _typeTag;
        #endregion

        #region Reference Fields
        // Heap-managed values that the term could also represent
        // The CLR absolutely forbids ANY overlap of value & reference fields
        // but multiple overlapping reference fields is no problem

        [FieldOffset(8)] private readonly string _string = null!;
        [FieldOffset(8)] private readonly Box _box = null!;
        [FieldOffset(8)] private readonly Cons _cons = null!;
        [FieldOffset(8)] private readonly Vector _vector = null!;
        [FieldOffset(8)] private readonly Functional _functional = null!;
        [FieldOffset(8)] private readonly PortReader _portReader = null!;
        [FieldOffset(8)] private readonly PortWriter _portWriter = null!;
        [FieldOffset(8)] private readonly object _ref = null!;
        #endregion

        #region Construction
        private Term(TypeTag type) : this()
        {
            _ulong = QNAN;
            this._typeTag = (byte)type;
        }

        private Term(bool b) : this(TypeTag.Boolean) => _bool = b;
        private Term(byte b) : this(TypeTag.Byte) => _byte = b;
        private Term(char c) : this(TypeTag.Character) => _char = c;
        private Term(int i) : this(TypeTag.FixNum) => _int = i;
        private Term(double d) : this(TypeTag.FloNum) => _double = d;
        private Term(ulong ul) : this(TypeTag.ByteString) => _ulong = ul;
        private Term(ulong ul, TypeTag tag) : this(tag) => _ulong = ul;

        private Term(string str, TypeTag tag) : this(tag) => _string = str; // Symbols AND strings
        private Term(Box boxed) : this(TypeTag.Box) => _box = boxed;
        private Term(Cons cons) : this(TypeTag.Cons) => _cons = cons;
        private Term(Vector vector) : this(TypeTag.Vector) => _vector = vector;
        private Term(Functional functional) : this(TypeTag.Functional) => _functional = functional;
        private Term(PortReader port) : this(TypeTag.PortReader) => _portReader = port;
        private Term(PortWriter port) : this(TypeTag.PortWriter) => _portWriter = port;

        private Term(ulong value, object? reference, TypeTag tag) : this(tag)
        {
            if (reference is not null)
            {
                this._ref = reference;
            }
            else
            {
                this._ulong = value;
            }
        }
        #endregion

        #region Static Instances
        public static readonly Term True = new(true);
        public static readonly Term False = new(false);
        public static readonly Term Nil = new(TypeTag.Nil);
        public static readonly Term Void = new(TypeTag.Void);
        public static readonly Term Undefined = new(TypeTag.Undefined);
        #endregion

        #region Public Constructors
        public static Term Boolean(bool b) => new(b);
        public static Term Byte(byte b) => new(b);
        public static Term Character(char c) => new(c);
        public static Term FixNum(int i) => new(i);
        public static Term FloNum(double d) => new(d);
        public static Term ByteString(ulong ul) => new(ul & BSTR);
        public static Term ByteString(ulong ul, TypeTag tag) => new(ul & BSTR, tag);

        public static Term Symbol(string name) => new(name, TypeTag.Symbol);

        public static Term Box(Term t) => new(new Box(t));
        public static Term Cons(Term car, Term cdr) => new(new Cons(car, cdr));
        public static Term Vector(int length) => new(new Vector(length));
        public static Term Functional(int arity, bool variadic, uint ip, params Box[] captured)
            => new(new Functional(arity, variadic, ip, captured));
        public static Term PortReader(string name, StreamReader str) => new(new PortReader(name, str));
        public static Term PortWriter(string name, StreamWriter str) => new(new PortWriter(name, str));

        public static Term CharString(string str) => new(str, TypeTag.CharString);

        public static Term ByType(ulong value, object reference, TypeTag tag)
        {
            return new Term(value, reference, tag);
        }
        #endregion

        #region Meta-Attributes

        private bool _isDouble => ((_ulong & QNAN) != QNAN);

        private bool _isByteString => ((_ulong & QNAN) == QNAN) && _double < 0;

        public TypeTag Tag => _isDouble
            ? TypeTag.FloNum
            : _isByteString
                ? TypeTag.ByteString
                : (TypeTag)_typeTag;

        public bool IsNil => _ulong == 0;
        public bool IsStaticType => (int)Tag < 100;
        public bool IsValueType => (int)Tag < 200;
        public bool IsReferenceType => (int)Tag >= 200;
        public bool IsFalsy => Tag == TypeTag.Boolean && _bool == false;
        public bool IsTruthy => !IsFalsy;
        public bool IsBoxed => Tag == TypeTag.Box;

        public bool IsUnsignedFixed => (int)Tag < 120;
        public bool IsSignedFixed => Tag == TypeTag.FixNum;
        public bool IsFloating => Tag == TypeTag.FloNum;

        #endregion

        #region Data Access
        public bool AsBoolean => IsTruthy;
        public byte AsByte => _byte;
        public char AsCharacter => _char;
        public ulong AsRawNum => _ulong;
        public int AsFixNum => _int;
        public double AsFloNum => _double;
        public byte[] AsByteString => BitConverter.GetBytes(_ulong);

        // Symbols only meaningfully exist as Terms
        public string AsCharString => _string;
        public Box AsBox => _box;
        public Cons AsCons => _cons;
        public Vector AsVector => _vector;
        public Functional AsFunctional => _functional;
        public PortReader AsPortReader => _portReader;
        public PortWriter AsPortWriter => _portWriter;

        public object AsObject => _ref;

        #endregion

        #region Equality Overrides
        /*
        Not sure how much of this is strictly necessary for my use case?
        It's what's stylistically recommended, though.
        And at least we establish basic value/ref equality.
        */
        public bool Equals(Term t)
        {
            if (Tag != t.Tag)
            {
                return false;
            }
            else if (Tag == TypeTag.Symbol || Tag == TypeTag.CharString)
            {
                return string.Equals(_string, t._string, StringComparison.InvariantCulture);
            }
            else if ((int)Tag < 100)
            {
                // No matter what kind of value type, to be equal they need to have the exact same bits
                return _ulong == t._ulong;
            }
            else
            {
                return ReferenceEquals(_ref, t._ref);
            }
        }

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Term t && Equals(t);

        public static bool operator ==(Term t1, Term t2) => t1.Equals(t2);
        public static bool operator !=(Term t1, Term t2) => !t1.Equals(t2);

        public override int GetHashCode()
        {
            if ((int)Tag < 100) // Static instance types
            {
                return (int)Tag;
            }
            else if ((int)Tag < 200) // Value-Type
            {
                return _int; // Let the raw value be interpreted as the hashcode
            }
            else // Reference-Type
            {
                return _ref?.GetHashCode() ?? 0;
            }
        }
        #endregion

        // Only meant for the sake of ordering lesser/greater. Regular equality is
        // already established.
        public int CompareTo(Term other)
        {
            if (Equals(other))
            {
                return 0;
            }
            else if (Tag == other.Tag)
            {
                if (IsUnsignedFixed)
                {
                    return AsRawNum.CompareTo(other.AsRawNum);
                }
                else if (IsSignedFixed)
                {
                    return AsFixNum.CompareTo(other.AsFixNum);
                }
                else if (IsFloating)
                {
                    return AsFloNum.CompareTo(other.AsFloNum);
                }
            }

            return -1;
        }

        #region Stringification
        public override string ToString()
        {
            return Tag switch
            {
                TypeTag.Boolean => IsTruthy ? "#t" : "#f",

                TypeTag.Byte => BitConverter.ToString([_byte]),
                TypeTag.Character => CharacterToString(_char),

                TypeTag.FixNum => _int.ToString(),

                TypeTag.FloNum => _double.ToString(),

                TypeTag.ByteString => BitConverter.ToString(AsByteString),

                TypeTag.Box => $"^{_ref.ToString()}",
                TypeTag.Cons => $"{_cons.Car.ToString()}{CdrToString(_cons.Cdr)}",
                TypeTag.Vector => $"#({string.Join(' ', _vector.Elements)})",
                TypeTag.Functional => $"ƒ({_functional.Arity}{(_functional.Variadic ? "+" : string.Empty)})",
                TypeTag.Symbol => _string,
                TypeTag.PortReader => $"Read:{{{_portReader.Name}}}",
                TypeTag.PortWriter => $"Write:{{{_portWriter.Name}}}",

                TypeTag.CharString => $"\"{_string}\"",

                TypeTag.Nil => "()",
                TypeTag.Void => "#<void>",
                TypeTag.Undefined => "#<undefined>",

                _ => "#<unknown>",
            };
        }

        private static string CharacterToString(char c)
        {
            return c switch
            {
                '\t' => "#\\tab",
                '\n' => "#\\newline",
                '\r' => "#\\return",
                ' ' => "#\\space",
                _ => $"#\\{c}"
            };
        }

        private static string CdrToString(Term t)
        {
            return t.Tag switch
            {
                TypeTag.Nil => string.Empty,
                TypeTag.Cons => $" {t._cons.Cdr.ToString()}{CdrToString(t._cons.Cdr)}",
                _ => $" . {t.ToString()}"
            };
        }
        #endregion
    }
}
