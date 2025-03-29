namespace VirtualMachine
{
    public class Chunk
    {
        private readonly byte[] _code;
        private readonly byte[] _constants;

        public Chunk(byte[] code, byte[] constants)
        {
            _code = code;
            _constants = constants;
        }

        public OpCode ReadOpCode(int i) => (OpCode)_code[i];

        public Span<byte> ReadCode(int i, int l) => new Span<byte>(_code, i, l);
        public Span<byte> ReadConstant(int i, int l) => new Span<byte>(_constants, i, l);
    }
}
