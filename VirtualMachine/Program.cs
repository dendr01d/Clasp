using VirtualMachine.Terms;

namespace VirtualMachine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ClosureBuilder testChunk = new ClosureBuilder();
            int offset = testChunk.AddConstant(new Character('c'));

            testChunk.AppendCode((byte)OpCode.Op_Constant, (byte)offset);
            testChunk.AppendCode((byte)OpCode.Op_Return);

            string disassembledTest = Disassembler.Disassemble(testChunk, "test chunk");

            Console.WriteLine(disassembledTest);

            Console.ReadKey(true);
        }
    }
}
