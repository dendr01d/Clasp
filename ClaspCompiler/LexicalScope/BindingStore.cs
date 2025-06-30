using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax;

namespace ClaspCompiler.LexicalScope
{
    internal sealed class BindingStore : IPrintable
    {
        private uint _idCounter;
        private List<ScopedBindingMap> _scopes;

        public readonly ScopedBindingMap DefaultScope;

        public BindingStore()
        {
            _idCounter = 0;
            _scopes = [];

            DefaultScope = GetBindingMap([]);
            foreach (var binding in DefaultBindings.Bindings)
            {
                DefaultScope.Bind(binding.Key, new(binding.Key, binding.Value));
            }
        }

        public uint GetFreshScopeToken() => _idCounter++;

        private ScopedBindingMap GetBindingMap(IEnumerable<uint> scopeTokens)
        {
            if (_scopes.FirstOrDefault(x => x.ScopeTokens.SetEquals(scopeTokens)) is ScopedBindingMap extant)
            {
                return extant;
            }
            else
            {
                ScopedBindingMap newMap = new([.. scopeTokens], this);
                _scopes.Add(newMap);
                return newMap;
            }
        }

        private bool TryLookupMap(ImmutableHashSet<uint> scopeTokens, Symbol freeSym, [NotNullWhen(true)] out ScopedBindingMap? result)
        {
            var validSets = _scopes.Where(x => x.ScopeTokens.IsSubsetOf(scopeTokens) && x.Binds(freeSym));

            var scoredMaps = validSets.GroupBy(x => scopeTokens.Except(x.ScopeTokens).Count)
                .OrderBy(x => x.Key);

            var candidates = scoredMaps
                .FirstOrDefault()
                ?.ToArray()
                ?? [];

            if (candidates.Length > 1)
            {
                string msg = string.Format("Ambiguous scope set matched by {0} recorded sets: {1}",
                    candidates.Length,
                    $"[{string.Join(", ", scopeTokens)}]");
                throw new Exception(msg);
            }
            else if (candidates.Length == 0)
            {
                result = null;
                return false;
            }
            else
            {
                result = candidates[0];
                return true;
            }
        }

        public void RecordBinding(Identifier id, Binding newBinding)
        {
            ScopedBindingMap map = GetBindingMap(id.SurroundingScope);
            map.Bind(id.Symbol, newBinding);
        }

        public bool TryResolve(Identifier id, [NotNullWhen(true)] out Binding? binding)
        {
            binding = null;

            return TryLookupMap(id.SurroundingScope, id.Symbol, out var map)
                && map.TryLookup(id.Symbol, out binding);
        }

        public bool BreaksLine => true;
        public string AsString => $"[{nameof(BindingStore)} with {_scopes.Count} scopes]";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            writer.WriteLineIndent(DefaultScope, indent);
            writer.WriteLineByLine(_scopes, indent);
            writer.Write(')');
        }
        public override string ToString() => AsString;
    }
}
