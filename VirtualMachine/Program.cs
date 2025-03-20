using VirtualMachine.Terms;

namespace VirtualMachine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Chunk testChunk = new Chunk();
            int offset = testChunk.WriteConstant(new Character('c'));

            testChunk.WriteToEnd((byte)OpCode.Op_Constant, (byte)offset);
            testChunk.WriteToEnd((byte)OpCode.Op_Return);

            string disassembledTest = Disassembler.Disassemble(testChunk, "test chunk");

            Console.WriteLine(disassembledTest);

            Console.ReadKey(true);
        }
    }
}
