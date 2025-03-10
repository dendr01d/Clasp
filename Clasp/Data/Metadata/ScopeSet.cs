using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Interfaces;


namespace Clasp.Data.Metadata
{
    /// <summary>
    /// Represents the lexical context of a <see cref="Syntax"/>, including
    /// its source location and effective scope within the program.
    /// </summary>
    internal class ScopeSet
    {
        private readonly Dictionary<int, HashSet<Scope>> _phasedScopeSets;

        public static readonly ScopeSet Empty = new ScopeSet();

        private ScopeSet(IEnumerable<KeyValuePair<int, HashSet<Scope>>> dict)
        {
            _phasedScopeSets = new Dictionary<int, HashSet<Scope>>(dict);
        }

        public ScopeSet() : this([]) { }

        public ScopeSet(ScopeSet original) : this(original._phasedScopeSets) { }

        public IEnumerable<Scope> this[int i]
        {
            get => _phasedScopeSets.TryGetValue(i, out HashSet<Scope>? scopes)
                ? scopes
                : [];
        }

        public IEnumerable<KeyValuePair<int, Scope[]>> Enumerate()
        {
            return _phasedScopeSets.Select<KeyValuePair<int, HashSet<Scope>>, KeyValuePair<int, Scope[]>>(x => new(x.Key, x.Value.ToArray()));
        }

        public bool SameScopes(ScopeSet other)
        {
            foreach(var entry in _phasedScopeSets)
            {
                if (!entry.Value.SetEquals(other._phasedScopeSets[entry.Key]))
                {
                    return false;
                }
            }

            return true;
        }

        #region Scope Manipulation

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

        public ScopeSet RestrictPhaseUpTo(int inclusivePhaseThreshold)
        {
            return new ScopeSet(_phasedScopeSets.Where(x => x.Key < inclusivePhaseThreshold));
        }

        #endregion

        #region Binding as a Set of Scopes

        public bool TryBind(int phase, string symbolicName, RenameBinding binding)
        {
            if (this[phase].MaxBy(x => x.Id) is Scope scp
                && !scp.Binds(symbolicName))
            {
                scp.AddBinding(symbolicName, binding);
                return true;
            }

            return false;
        }

        public RenameBinding? ResolveBindings(int phase, string symbolicName)
        {
            if (_phasedScopeSets.TryGetValue(phase, out HashSet<Scope>? scopes))
            {
                foreach (Scope scp in scopes.OrderByDescending(x => x.Id))
                {
                    if (scp.TryResolve(symbolicName, out RenameBinding? binding))
                    {
                        return binding;
                    }
                }
            }

            if (StaticEnv.ImplicitScope.TryResolve(symbolicName, out RenameBinding? staticBinding))
            {
                return staticBinding;
            }

            return null;
        }

        #endregion
    }
}
