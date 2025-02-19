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
    internal class LexInfo : ISourceTraceable
    {
        public SourceCode Location { get; private set; }

        private readonly Dictionary<int, HashSet<Scope>> _phasedScopeSets;

        public static readonly LexInfo StaticInfo = new LexInfo(SourceCode.StaticSource);

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

        public LexInfo RestrictPhaseUpTo(int phase)
        {
            return new LexInfo(Location, _phasedScopeSets.Where(x => x.Key < phase));
        }

        #endregion

        #region Binding as a Set of Scopes

        public bool TryBind(int phase, string symbolicName, CompileTimeBinding binding)
        {
            if (_phasedScopeSets[phase].MaxBy(x => x.Id) is Scope scp
                && !scp.Binds(symbolicName))
            {
                scp.AddBinding(symbolicName, binding);
                return true;
            }

            return false;
        }

        public CompileTimeBinding? ResolveBindings(int phase, string symbolicName)
        {
            if (_phasedScopeSets.TryGetValue(phase, out HashSet<Scope>? scopes))
            {
                foreach (Scope scp in scopes.OrderByDescending(x => x.Id))
                {
                    if (scp.TryResolve(symbolicName, out CompileTimeBinding? binding))
                    {
                        return binding;
                    }
                }
            }

            if (StaticEnv.StaticScope.TryResolve(symbolicName, out CompileTimeBinding? staticBinding))
            {
                return staticBinding;
            }

            return null;
        }

        #endregion
    }
}
