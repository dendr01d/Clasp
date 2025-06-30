using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;

namespace ClaspCompiler.SchemeSyntax.Abstract
{
    //internal sealed class ScopeSetMap : IPrintable
    //{
    //    private Dictionary<ImmutableHashSet<uint>, Dictionary<Symbol, Symbol>> _mapsByScopeSet;

    //    public ScopeSetMap()
    //    {
    //        _mapsByScopeSet = new();
    //    }

    //    public static ScopeSetMap BuildDefault()
    //    {
    //        ScopeSetMap output = new();
    //        output._mapsByScopeSet.Add([0], SpecialKeyword.DefaultBindings);

    //        return output;
    //    }

    //    private static string PrintScopeSet(ImmutableHashSet<uint> scopeSet)
    //    {
    //        return string.Format("{{{0}}}", string.Join(' ', scopeSet.Order()));
    //    }

    //    private bool TryLookupMap(ImmutableHashSet<uint> scopeSet, Symbol freeSym, [NotNullWhen(true)] out Dictionary<Symbol, Symbol>? result)
    //    {
    //        var validSets = _mapsByScopeSet.Where(x => scopeSet.IsSupersetOf(x.Key));
    //        validSets = validSets.Where(x => x.Value.ContainsKey(freeSym));
            
    //        var scoredMaps = validSets.GroupBy(x => scopeSet.Except(x.Key).Count())
    //            .OrderBy(x => x.Key);

    //        var candidates = scoredMaps
    //            .FirstOrDefault()
    //            ?.Select(x => x.Value)
    //            ?.ToArray() ?? [];

    //        if (candidates.Length > 1)
    //        {
    //            string msg = string.Format("Ambiguous scope set matched by {0} recorded sets: {1}",
    //                candidates.Length,
    //                PrintScopeSet(scopeSet));
    //            throw new Exception(msg);
    //        }
    //        else if (candidates.Length == 0)
    //        {
    //            result = null;
    //            return false;
    //        }
    //        else
    //        {
    //            result = candidates[0];
    //            return true;
    //        }
    //    }

    //    public void AddMapping(ImmutableHashSet<uint> scopeSet, Symbol freeSym, Symbol bindingSym)
    //    {
    //        if (!_mapsByScopeSet.TryGetValue(scopeSet, out var map))
    //        {
    //            map = new();
    //            _mapsByScopeSet[scopeSet] = map;
    //        }
    //        map[freeSym] = bindingSym;
    //    }

    //    public bool TryLookupMapping(ImmutableHashSet<uint> scopeSet, Symbol freeSym, [NotNullWhen(true)] out Symbol? bindingSym)
    //    {
    //        bindingSym = null;

    //        return TryLookupMap(scopeSet, freeSym, out var map)
    //            && map.TryGetValue(freeSym, out bindingSym);
    //    }

    //    public bool TryLookupMapping(Identifier id, [NotNullWhen(true)] out Symbol? bindingSym)
    //    {
    //        return TryLookupMapping(id.ScopeSet, id.FreeSymbol, out bindingSym);
    //    }

    //    public bool BreaksLine => _mapsByScopeSet.Any();
    //    public string AsString => _mapsByScopeSet.ToString() ?? "()";
    //    public void Print(TextWriter writer, int indent)
    //    {
    //        writer.WriteIndenting('(', ref indent);

    //        foreach(var pair in _mapsByScopeSet)
    //        {
    //            int subIndent = indent;

    //            if (!Equals(pair, _mapsByScopeSet.First()))
    //            {
    //                writer.WriteLineIndent(subIndent);
    //            }

    //            writer.WriteIndenting('[', ref subIndent);
    //            writer.Write("#(");
    //            writer.Write(string.Join(' ', pair.Key.Order()));
    //            writer.WriteLineIndent(") .", subIndent);

    //            writer.WriteIndenting('(', ref subIndent);

    //            foreach(var symLink in pair.Value)
    //            {
    //                if (!Equals(symLink, pair.Value.First()))
    //                {
    //                    writer.WriteLineIndent(subIndent);
    //                }
    //                writer.Write('(');
    //                writer.Write(symLink.Key);
    //                writer.Write(" . ");
    //                writer.Write(symLink.Value);
    //                writer.Write(')');
    //            }

    //            writer.Write(")]");
    //        }

    //        writer.Write(')');
    //    }
    //    public override string ToString() => AsString;
    //}

}
