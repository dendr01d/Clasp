namespace VirtualMachine
{
    public class Chunk
    {
        /// <summary>
        /// The size (in bytes) of the chunk.
        /// </summary>
        public int Size => _code.Length;

        private readonly byte[] _code;

        public byte this[int i]
        {
            get => _code[i];
        }

        public Chunk(byte[] code)
        {
            _code = code;
        }
        public byte ReadByte(int ip) => _code[ip];
        public Span<byte> ReadBytes(int ip, int len) => new Span<byte>(_code, ip, len);
    }
}
