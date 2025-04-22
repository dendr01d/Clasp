namespace VirtualMachine.Objects
{
    internal class PortWriter
    {
        public readonly string Name;
        public readonly StreamWriter Stream;

        public PortWriter(string name, StreamWriter reader)
        {
            Name = name;
            Stream = reader;
        }
    }
}
