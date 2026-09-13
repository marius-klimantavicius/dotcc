using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    internal IReadOnlyDictionary<string, string>? AnonymousTypeNames { get; init; }
    private readonly HashSet<string> _anonymousAggregateTypes = new(StringComparer.Ordinal);

    private string NameAnonymousAggregate(string original)
    {
        _anonymousAggregateTypes.Add(original);
        return AnonymousTypeNames is not null && AnonymousTypeNames.TryGetValue(original, out var name) ? name : original;
    }

    // Resolve C names from the bound type graph; hidden storage names are never
    // part of the selector. No recognition or replacement of emitted C# text.
    internal void ResolveFieldTypeNames(CPreprocessingOptions options, Dictionary<string, string> names, HashSet<string> matched)
    {
        CType? Field(CType type, string member)
        {
            if (type.Unqualified is not CType.Named owner || !_structFields.TryGetValue(owner.Name, out var fields)) return null;
            foreach (var direct in fields)
                if (!direct.IsAnonymousAggregate && direct.Name == member) return direct.Type;
            CType? found = null;
            foreach (var container in fields.Where(f => f.IsAnonymousAggregate))
                if (Field(container.Type, member) is { } promoted)
                {
                    if (found is not null) throw new CompileException("ambiguous fieldTypeNames member: " + member);
                    found = promoted;
                }
            return found;
        }

        foreach (var rule in options.FieldTypeNames)
        {
            var path = rule.Field.Split('.');
            var type = _typedefs.TryGetValue(path[0], out var alias) ? alias
                : _structFields.ContainsKey(path[0]) ? new CType.Named(path[0]) : null;
            if (type is null) continue; // Profiles may span several source units.
            foreach (var member in path.Skip(1))
                type = Field(type, member) ?? throw new CompileException("fieldTypeNames field path not found: " + rule.Field);
            if (type.Unqualified is not CType.Named aggregate || !_anonymousAggregateTypes.Contains(aggregate.Name))
                throw new CompileException("fieldTypeNames requires an anonymous struct or union field type: " + rule.Field);
            if (names.TryGetValue(aggregate.Name, out var previous) && previous != rule.Name)
                throw new CompileException($"fieldTypeNames assigns conflicting names '{previous}' and '{rule.Name}' to one type: {rule.Field}");
            if (names.Any(p => p.Key != aggregate.Name && p.Value == rule.Name))
                throw new CompileException("fieldTypeNames name collision between distinct types: " + rule.Name);
            names[aggregate.Name] = rule.Name;
            if (matched.Add(rule.Field)) CPreprocessingOptions.WriteEvent(options.Report, "field-type-name",
                ("field", rule.Field), ("original", aggregate.Name), ("name", rule.Name));
        }
    }

    internal void ValidateFieldTypeNames(IReadOnlyDictionary<string, string> names)
    {
        var existing = new HashSet<string>(Types.Select(t => t.Name).Concat(Enums.Select(e => e.Name))
            .Concat(_typedefs.Keys).Concat(Globals.Select(g => g.Sym.Name)).Concat(Functions.Select(f => f.Sym.Name)), StringComparer.Ordinal);
        foreach (var pair in names)
        {
            if (existing.Contains(pair.Value) || Compiler.PredefinedTypeNames.Contains(pair.Value)
                || pair.Value is "Libc" or "Cond" or "System" or "DotCcProgram" or "DotCcEntryPoint")
                throw new CompileException("fieldTypeNames name conflicts with an existing type or symbol: " + pair.Value);
            if (_structFields[pair.Key].Any(f => f.Name == pair.Value)
                || (_promoted.TryGetValue(pair.Key, out var promoted) && promoted.ContainsKey(pair.Value)))
                throw new CompileException("fieldTypeNames name conflicts with a member of the renamed type: " + pair.Value);
        }
    }
}
