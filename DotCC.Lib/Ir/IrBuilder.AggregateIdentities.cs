using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    internal IReadOnlyDictionary<string, string>? AggregateNames { get; set; }
    private readonly HashSet<string> _taggedAggregates = new(StringComparer.Ordinal);
    private string AggregateName(string name) => AggregateNames?.GetValueOrDefault(name, name) ?? name;

    internal IReadOnlyList<AggregateIdentity> AggregateIdentityGraph()
    {
        var definitions = Types.ToDictionary(t => t.Name, StringComparer.Ordinal);
        var exposed = new HashSet<string>(StringComparer.Ordinal);
        void Expose(string name)
        {
            if (!definitions.TryGetValue(name, out var type) || !exposed.Add(name)) return;
            foreach (var field in type.Fields)
                foreach (var dependency in AggregateIdentities.References(field.Type)) Expose(dependency);
        }
        // This is C linkage, not the overloaded historical Storage.Static flag
        // on global data. Include declarations as well as definitions, including
        // extern arrays and callback-valued objects.
        var symbols = FunctionPointerCandidates.Where(s => s.Storage != Storage.Static)
            .Concat(_symbols.FileScopeSymbols.Where(s => s.Kind == SymKind.Var && !_internalGlobals.Contains(s)))
            .Concat(Globals.Select(g => g.Sym).Where(s => !_internalGlobals.Contains(s)));
        foreach (var symbol in symbols)
            foreach (var name in AggregateIdentities.References(symbol.Type)) Expose(name);
        return Types.Select(t => new AggregateIdentity(t.Name, t.Name, _taggedAggregates.Contains(t.Name),
            exposed.Contains(t.Name), t.Fields.SelectMany(f => AggregateIdentities.References(f.Type))
                .Where(definitions.ContainsKey).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            ObjectAggregateMetadata.From(t))).ToArray();
    }
}
