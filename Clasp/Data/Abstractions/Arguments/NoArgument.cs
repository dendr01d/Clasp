namespace Clasp.Data.Abstractions.Arguments
{
    internal sealed class NoArgument : AbstractArgument
    {
        public static readonly NoArgument Instance = new();

        private NoArgument() { }

        public override string Express() => string.Empty;
    }
}
