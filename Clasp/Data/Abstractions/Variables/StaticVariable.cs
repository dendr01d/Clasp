namespace Clasp.Data.Abstractions.Variables
{
    internal sealed class StaticVariable : AbstractVariable
    {
        public readonly object? Description; //what?

        public StaticVariable(string symbolicName) : base(symbolicName)
        {
            Description = null;
        }
    }
}
