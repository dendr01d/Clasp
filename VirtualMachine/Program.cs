

using System.Buffers.Binary;

using VirtualMachine.Objects;

namespace VirtualMachine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "CLASP Virtual Machine";

            ChunkBuilder cb = new ChunkBuilder();

            cb.AppendCode((byte)OpCode.Const_FixNum);
            cb.AppendCode(BitConverter.GetBytes(5));

            cb.AppendCode((byte)OpCode.Local_Push);

            cb.AppendCode((byte)OpCode.Const_FloNum);
            cb.AppendCode(BitConverter.GetBytes(6.0));

            cb.AppendCode((byte)OpCode.Flo_To_Fix);

            cb.AppendCode((byte)OpCode.Fix_Add);

            cb.AppendCode((byte)OpCode.Op_Return);

            Chunk testChunk = cb.Finalize();

            string disassembledTest = Disassembler.Disassemble(testChunk, "test chunk");

            Console.WriteLine(disassembledTest);

            (MachineResult, Term) mxOutput = Machine.Run(testChunk);

            Console.WriteLine();
            Console.WriteLine(mxOutput.Item1);
            Console.WriteLine(mxOutput.Item2);

            Console.ReadKey(true);
        }
    }
}
