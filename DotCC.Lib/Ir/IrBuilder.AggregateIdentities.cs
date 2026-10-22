using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    internal IReadOnlyDictionary<string, string>? AggregateNames { get; set; }
    private readonly HashSet<string> _taggedAggregates = new(StringComparer.Ordinal);
    internal Dictionary<string, string> GeneratedAggregateOrigins { get; } = new(StringComparer.Ordinal);
    // Execution lowering historically decays an incomplete extern array to a
    // pointer (and uses zero for an omitted outer row bound). Keep its actual
    // declaration type separately for cross-TU C compatibility, without changing
    // that lowering or confusing an explicit GNU [0] with an omitted bound.
    private readonly Dictionary<Symbol, CType> _externalArrayLinkageTypes = new();
    private string AggregateName(string name)
    {
        string relocated = AggregateNames?.GetValueOrDefault(name, name) ?? name;
        if (relocated != name) GeneratedAggregateOrigins[relocated] = name;
        return relocated;
    }

    internal Dictionary<string, ObjectAggregateMetadata> IdentityMetadata() => Types
        .Select(t => (t.Name, Shape: ObjectAggregateMetadata.From(t)))
        .Concat(Enums.Select(e => (e.Name, Shape: AggregateIdentities.EnumShape(e))))
        .ToDictionary(p => p.Name, p => p.Shape, StringComparer.Ordinal);

    internal IReadOnlyList<AggregateIdentity> AggregateIdentityGraph()
    {
        var definitions = Types.ToDictionary(t => t.Name, StringComparer.Ordinal);
        var enums = Enums.ToDictionary(e => e.Name, StringComparer.Ordinal);
        bool Defined(string name) => definitions.ContainsKey(name) || enums.ContainsKey(name);
        var exposed = new HashSet<string>(StringComparer.Ordinal);
        void Expose(string name)
        {
            if (!Defined(name) || !exposed.Add(name) || !definitions.TryGetValue(name, out var type)) return;
            foreach (var field in type.Fields)
                foreach (var dependency in AggregateIdentities.References(field.Type)) Expose(dependency);
        }
        // This is C linkage, not the overloaded historical Storage.Static flag
        // on global data. Include declarations as well as definitions, including
        // extern arrays and callback-valued objects.
        var symbols = FunctionPointerCandidates.Where(s => s.Storage != Storage.Static)
            .Concat(Globals.Select(g => g.Sym).Where(s => !_internalGlobals.Contains(s)))
            .Concat(_symbols.FileScopeSymbols.Where(s => s.Kind == SymKind.Var && !_internalGlobals.Contains(s)))
            .DistinctBy(s => s.TargetName).ToArray();
        foreach (var symbol in symbols)
            foreach (var name in AggregateIdentities.References(symbol.Type)) Expose(name);
        var nodes = Types.Select(t => new AggregateIdentity(t.Name, GeneratedAggregateOrigins.GetValueOrDefault(t.Name, t.Name), _taggedAggregates.Contains(t.Name),
            exposed.Contains(t.Name), t.Fields.SelectMany(f => AggregateIdentities.References(f.Type))
                .Where(Defined).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            ObjectAggregateMetadata.From(t)))
            .Concat(Enums.Select(e => new AggregateIdentity(e.Name, GeneratedAggregateOrigins.GetValueOrDefault(e.Name, e.Name),
                _taggedAggregates.Contains(e.Name), exposed.Contains(e.Name), [], AggregateIdentities.EnumShape(e), IsEnum: true))).ToArray();
        var roots = symbols.Select(s =>
        {
            var contract = AggregateIdentities.LinkageContract(s.TargetName, _externalArrayLinkageTypes.GetValueOrDefault(s, s.Type));
            return contract with { Type = (s.Kind == SymKind.Func ? "function|" : "object|") + contract.Type,
                References = contract.References.Where(Defined).ToArray() };
        }).Where(c => c.References.Length != 0).ToArray();
        return new AggregateIdentitySet(nodes, roots);
    }
}
