using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.Terms.Syntax;


namespace Clasp.Data.Metadata
{
    /// <summary>
    /// Represents the lexical context of a <see cref="Syntax"/>, including
    /// its source location and effective scope within the program.
    /// </summary>
    internal class LexInfo : ISourceTraceable
    {
        public SourceCode Location { get; private set; }

        private readonly Dictionary<int, HashSet<Scope>> _phasedScopeSets;

        private LexInfo(SourceCode loc, IEnumerable<KeyValuePair<int, HashSet<Scope>>> dict)
        {
            Location = loc;
            _phasedScopeSets = new Dictionary<int, HashSet<Scope>>(dict);
        }

        public LexInfo(SourceCode source) : this(source, []) { }

        public LexInfo(LexInfo original)
            : this(original.Location, original._phasedScopeSets)
        { }

        public IEnumerable<Scope> this[int i]
        {
            get => _phasedScopeSets.TryGetValue(i, out HashSet<Scope>? scopes)
                ? scopes
                : [];
        }

        public void AddScope(int phase, params Scope[] scopes)
        {
            if (!_phasedScopeSets.ContainsKey(phase))
            {
                _phasedScopeSets[phase] = new HashSet<Scope>();
            }
            _phasedScopeSets[phase].UnionWith(scopes);
        }

        public void FlipScope(int phase, params Scope[] scopes)
        {
            if (!_phasedScopeSets.ContainsKey(phase))
            {
                _phasedScopeSets[phase] = new HashSet<Scope>();
            }
            _phasedScopeSets[phase].SymmetricExceptWith(scopes);
        }

        public void RemoveScope(int phase, params Scope[] scopes)
        {

            if (!_phasedScopeSets.ContainsKey(phase))
            {
                _phasedScopeSets[phase] = new HashSet<Scope>();
            }
            _phasedScopeSets[phase].ExceptWith(scopes);
        }

        public LexInfo RestrictPhaseUpTo(int phase)
        {
            return new LexInfo(Location, _phasedScopeSets.Where(x => x.Key <= phase));
        }

        public bool TryResolveBinding(int phase, string symbolicName,
            [NotNullWhen(true)] out CompileTimeBinding? binding)
        {
            if (_phasedScopeSets.TryGetValue(phase, out HashSet<Scope>? scopes))
            {
                foreach(Scope scp in scopes.OrderByDescending(x => x.Id))
                {
                    if (scp.TryResolve(symbolicName, out binding))
                    {
                        return true;
                    }
                }
            }

            binding = null;
            return false;
        }
    }
}
