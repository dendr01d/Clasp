namespace VirtualMachine.Objects
{
    internal class PortReader
    {
        public readonly string Name;
        public readonly StreamReader Stream;

        public PortReader(string name, StreamReader reader)
        {
            Name = name;
            Stream = reader;
        }
    }
}
