using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC;

public static partial class Compiler
{
    private static bool UsesInlineOptions(CSharpOutputOptions? options) =>
        options?.DeduplicateInline == true || options?.ExportInline is { Count: > 0 };

    private sealed record InlineOutput(string Functions, IReadOnlyList<CSharpFunctionSource> Parts,
        IReadOnlyDictionary<string, string> Types, string Globals);

    private static InlineOutput ProcessInlineFunctions(IReadOnlyList<CSharpFunctionSource> sources,
        IReadOnlyDictionary<string, InlineFunctionMetadata> metadata, IReadOnlyDictionary<string, string> types,
        string globals, CSharpOutputOptions? options, IEnumerable<string> globalNames)
    {
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        var removed = new HashSet<string>(StringComparer.Ordinal);
        bool enabled = UsesInlineOptions(options);
        if (enabled)
        {
            var entries = metadata.OrderBy(p => p.Key, StringComparer.Ordinal).ToArray();
            // Start with typed body equivalence, then refine by the identities of
            // bound callees. Refinement only splits, so recursive groups converge.
            // Unproven/address-used/external definitions remain unique identities.
            var bases = entries.ToDictionary(p => p.Key, p =>
                p.Value.IsStatic && p.Value.Shape != "-" && !p.Value.AddressUsed
                    ? "shape:" + p.Value.Shape : "identity:" + p.Key, StringComparer.Ordinal);
            static Dictionary<string, int> Partition(IEnumerable<KeyValuePair<string, string>> keys)
            {
                var groups = new Dictionary<string, int>(StringComparer.Ordinal);
                var result = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var (name, key) in keys)
                {
                    if (!groups.TryGetValue(key, out int group)) groups.Add(key, group = groups.Count);
                    result.Add(name, group);
                }
                return result;
            }
            var partition = Partition(bases);
            while (true)
            {
                var next = Partition(entries.Select(p => new KeyValuePair<string, string>(p.Key,
                    partition[p.Key] + ":" + string.Join(";", p.Value.Dependencies.Select(d =>
                        partition.TryGetValue(d, out var group) ? "group:" + group : "symbol:" + d)))));
                if (entries.All(p => next[p.Key] == partition[p.Key])) break;
                partition = next;
            }
            var groups = entries.GroupBy(p => partition[p.Key]).ToArray();
            var representatives = groups.ToDictionary(g => g.Key, g => g
                .OrderBy(p => p.Key == EmitHelpers.Id(p.Value.OriginalName) ? 0 : 1)
                .ThenBy(p => p.Key, StringComparer.Ordinal).First().Key);
            var selectors = (options?.ExportInline ?? Array.Empty<string>()).Select(pattern =>
                (Pattern: pattern, Selector: new MacroExportSelector(new[] { pattern }, "--export-inline"))).ToArray();
            foreach (var (pattern, selector) in selectors)
                if (!entries.Any(p => selector.Matches(p.Value.OriginalName)))
                    throw new CompileException("--export-inline pattern '" + pattern + "' matched no inline definitions");
            var occupied = new HashSet<string>(globalNames.Concat(types.Keys), StringComparer.Ordinal);
            foreach (var original in entries.Where(p => selectors.Any(s => s.Selector.Matches(p.Value.OriginalName)))
                         .GroupBy(p => p.Value.OriginalName))
            {
                var candidates = original.Select(p => partition[p.Key]).Distinct().ToArray();
                if (candidates.Length != 1)
                    throw new CompileException("ambiguous --export-inline '" + original.Key
                        + "': definitions differ or depend on distinct state/function addresses; use a C wrapper in one translation unit");
                var representative = representatives[candidates[0]];
                var exportName = EmitHelpers.Id(original.Key);
                if (occupied.Contains(exportName) || sources.Any(s => s.Name == exportName && s.Name != representative))
                    throw new CompileException("--export-inline name '" + original.Key + "' conflicts with an existing declaration");
                names[representative] = exportName;
            }
            if (options?.DeduplicateInline == true)
                foreach (var group in groups)
                {
                    var representative = representatives[group.Key];
                    var target = names.GetValueOrDefault(representative, representative);
                    foreach (var entry in group.Where(p => p.Key != representative))
                    { names[entry.Key] = target; removed.Add(entry.Key); }
                }
        }
        var parts = sources.Where(s => !removed.Contains(s.Name)).Select(s =>
            new CSharpFunctionSource(names.GetValueOrDefault(s.Name, s.Name), InlineFunctionReferences.Rewrite(s.Text, names))).ToArray();
        var declarations = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, text) in types)
        {
            // Public managed visibility is not a C address use. Retain real C
            // callback caches, but don't manufacture addresses for inline APIs.
            if (enabled && key.StartsWith(FunctionPointerNames.TypeKeyPrefix, StringComparison.Ordinal)
                && metadata.TryGetValue(key[FunctionPointerNames.TypeKeyPrefix.Length..], out var function)
                && !function.AddressUsed) continue;
            declarations.Add(key, InlineFunctionReferences.Rewrite(text, names));
        }
        return new(string.Join("\n\n", parts.Select(p => p.Text)), parts, declarations,
            InlineFunctionReferences.Rewrite(globals, names));
    }
}
