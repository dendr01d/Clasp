using System.Diagnostics.CodeAnalysis;

using Clasp.Data.Terms;

namespace Clasp.Binding.Environments
{
    internal abstract class ClaspEnvironment
    {
        public ClaspEnvironment? Predecessor { get; protected set; }
        public abstract bool TryGetValue(string key, [NotNullWhen(true)] out Term? value);
        public abstract bool ContainsKey(string key);

        protected ClaspEnvironment(ClaspEnvironment? pred)
        {
            Predecessor = pred;
        }
    }
}
