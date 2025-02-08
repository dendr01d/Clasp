using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Clasp.Data.Terms.Syntax;


namespace Clasp.Data.Metadata
{
    /// <summary>
    /// Represents the lexical context of a <see cref="Syntax"/>, including
    /// its source location and effective scope within the program.
    /// </summary>
    internal class LexInfo : ISourceTraceable
    {
        public SourceLocation Location { get; private set; }

        private readonly Dictionary<int, ImmutableHashSet<uint>> _phasedScopeSets;

        private LexInfo(SourceLocation loc, IEnumerable<KeyValuePair<int, ImmutableHashSet<uint>>> dict)
        {
            Location = loc;
            _phasedScopeSets = new Dictionary<int, ImmutableHashSet<uint>>(dict);
        }

        public LexInfo(SourceLocation loc) : this(loc, []) { }

        public LexInfo(LexInfo original)
            : this(original.Location,
                  original._phasedScopeSets.ToDictionary(x => x.Key, x => x.Value.ToImmutableHashSet()))
        { }

        private static readonly ImmutableHashSet<uint> _emptySet = Array.Empty<uint>().ToImmutableHashSet();

        public ImmutableHashSet<uint> this[int i]
        {
            get => _phasedScopeSets.TryGetValue(i, out ImmutableHashSet<uint>? scopes)
                ? scopes
                : _emptySet;
        }

        public void AddScope(int phase, params uint[] tokens)
        {
            if (!_phasedScopeSets.ContainsKey(phase))
            {
                _phasedScopeSets[phase] = tokens.ToImmutableHashSet();
            }
            else
            {
                _phasedScopeSets[phase] = _phasedScopeSets[phase].Union(tokens);
            }
        }

        public void FlipScope(int phase, params uint[] tokens)
        {
            if (!_phasedScopeSets.ContainsKey(phase))
            {
                _phasedScopeSets[phase] = tokens.ToImmutableHashSet();
            }
            else
            {
                _phasedScopeSets[phase] = _phasedScopeSets[phase].SymmetricExcept(tokens);
            }
        }

        public void RemoveScope(int phase, params uint[] tokens)
        {

            if (!_phasedScopeSets.ContainsKey(phase))
            {
                _phasedScopeSets[phase] = _emptySet;
            }
            else
            {
                _phasedScopeSets[phase] = _phasedScopeSets[phase].Except(tokens);
            }
        }

        public LexInfo RestrictPhaseUpTo(int phase)
        {
            return new LexInfo(Location, _phasedScopeSets.Where(x => x.Key <= phase));
        }
    }
}
