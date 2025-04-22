

using System.Buffers.Binary;

namespace VirtualMachine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChunkBuilder cb = new ChunkBuilder();

            cb.AppendCode((byte)OpCode.Const_FixNum);
            cb.AppendCode(BitConverter.GetBytes(5).Reverse().ToArray());

            cb.AppendCode((byte)OpCode.Local_Push);

            cb.AppendCode((byte)OpCode.Const_FixNum);
            cb.AppendCode(BitConverter.GetBytes(6).Reverse().ToArray());

            cb.AppendCode((byte)OpCode.Fix_Add);

            cb.AppendCode((byte)OpCode.Op_Return);

            Chunk testChunk = cb.Finalize();

            string disassembledTest = Disassembler.Disassemble(testChunk, "test chunk");

            Console.WriteLine(disassembledTest);

            Console.ReadKey(true);
        }
    }
}
