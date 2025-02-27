namespace Clasp.Binding.Environments
{
    internal abstract class DynamicEnv : ClaspEnvironment
    {
        public ClaspEnvironment Predecessor { get; protected set; }

        protected DynamicEnv(ClaspEnvironment pred)
        {
            Predecessor = pred;
        }
    }
}
