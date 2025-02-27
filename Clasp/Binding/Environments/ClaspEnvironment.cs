using System.Diagnostics.CodeAnalysis;

using Clasp.Data.Terms;

namespace Clasp.Binding.Environments
{
    internal abstract class ClaspEnvironment
    {
        public abstract bool TryGetValue(string key, [NotNullWhen(true)] out Term? value);
    }
}
