using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using ClaspCompiler.SchemeData;

namespace ClaspCompiler.LexicalScope
{
    internal sealed record ScopedBindingMap : IPrintable
    {
        private readonly BindingStore _progenitor;

        private readonly Dictionary<Symbol, Binding> _bindingStore;
        public readonly ImmutableHashSet<uint> ScopeTokens;

        public ScopedBindingMap(IEnumerable<uint> tokens, BindingStore parent)
        {
            ScopeTokens = [.. tokens];

            _bindingStore = [];
            _progenitor = parent;
        }

        public bool Binds(Symbol sym) => _bindingStore.ContainsKey(sym);

        public void Bind(Symbol oldName, Binding newBinding)
        {
            _bindingStore.Add(oldName, newBinding);
        }

        public bool TryLookup(Symbol oldName, [NotNullWhen(true)] out Binding? result)
            => _bindingStore.TryGetValue(oldName, out result);

        public bool Equals(ScopedBindingMap? other) => ReferenceEquals(_progenitor, other?._progenitor) && ScopeTokens.SetEquals(other.ScopeTokens);
        public override int GetHashCode() => HashCode.Combine(_progenitor, ScopeTokens);

        public bool BreaksLine => true;
        public string AsString => $"Binding Map [{string.Join(", ", ScopeTokens)}] w/ {_bindingStore.Count} binding{(ScopeTokens.Count == 1 ? string.Empty : "s")}";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(scope ", ref indent);
            writer.Write("#(");
            writer.Write(string.Join(' ', ScopeTokens.Order()));
            writer.WriteLineIndent(")", indent);

            writer.WriteIndenting('(', ref indent);
            writer.WriteLineByLine(_bindingStore, PrintBindingPair, indent);
            writer.Write("))");

        }
        public sealed override string ToString() => AsString;

        private static void PrintBindingPair(TextWriter writer, KeyValuePair<Symbol, Binding> pair, int indent)
        {
            writer.Write('[');
            writer.Write(pair.Key, indent);
            writer.Write(' ');
            writer.Write(pair.Value, indent);
            writer.Write(']');
        }
    }
}
