using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotCC;

public static partial class Compiler
{
    // A JSON sidecar travels with generated projects. It is compiler metadata,
    // never inferred from postprocessed C# bodies or compiled into the library.
    internal const string ManagedSymbolsFile = "Dotcc.ManagedSymbols.json";
    private const string ManagedSymbolsMarker = "//!!dotcc-managed-symbols:1 ";
    private sealed record ManagedTypeSymbol(string Target, bool Incomplete, bool Union, string Signature);
    private sealed record ManagedSymbols(int Version, string Owner, string RuntimeHash,
        Dictionary<string, ManagedTypeSymbol> Types, string[] Functions);

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ManagedSymbols))]
    private partial class ManagedSymbolsContext : JsonSerializerContext { }

    private static ManagedSymbols? ReadManagedSymbols(CSharpOutputOptions? options)
    {
        if (options?.ManagedReferences is not { Count: > 0 } paths) return null;
        var text = File.ReadAllText(paths[0]).Trim();
        // Continue accepting the original comment-wrapped manifests so already
        // generated libraries can be referenced without regeneration.
        if (text.StartsWith(ManagedSymbolsMarker, StringComparison.Ordinal))
            text = text[ManagedSymbolsMarker.Length..];
        ManagedSymbols symbols;
        try { symbols = JsonSerializer.Deserialize(text, ManagedSymbolsContext.Default.ManagedSymbols)!; }
        catch (JsonException) { throw new CompileException("invalid managed symbol JSON: " + paths[0]); }
        static bool Qualified(string? name) => name is { Length: > 0 } && name.Split('.').All(part => CPreprocessingOptions.Identifier(part.TrimStart('@')));
        if (symbols is not { Version: 1, Types: not null, Functions: not null }
            || !Qualified(symbols.Owner) || symbols.RuntimeHash != AggregateIdentities.Hash(_cRuntimeBlock.Value)
            || symbols.Types.Any(pair => !CPreprocessingOptions.Identifier(pair.Key.TrimStart('@'))
                || pair.Value == null || !Qualified(pair.Value.Target) || pair.Value.Signature is not { Length: 64 } || !pair.Value.Signature.All(Uri.IsHexDigit))
            || symbols.Functions.Any(name => name == null || !CPreprocessingOptions.Identifier(name.TrimStart('@'))))
            throw new CompileException("invalid or incompatible managed symbols/runtime: " + paths[0]);
        return symbols;
    }

    private static Dictionary<string, string> ImportManagedTypes(ManagedSymbols? symbols,
        Dictionary<string, string> declarations, IReadOnlyDictionary<string, ObjectAggregateMetadata> aggregates,
        IReadOnlyList<AggregateIdentity>?[] graphs, IReadOnlyDictionary<string, string>[] variants)
    {
        var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        if (symbols == null) return aliases;
        for (int index = 0; index < graphs.Length; index++)
        foreach (var node in graphs[index] ?? Array.Empty<AggregateIdentity>())
        {
            var name = variants[index].GetValueOrDefault(node.Name, node.Name);
            if (!declarations.ContainsKey(name) || !symbols.Types.TryGetValue(node.Name, out var imported)) continue;
            var shape = aggregates.GetValueOrDefault(name, node.Shape);
            if (shape.IsUnion != imported.Union || (!shape.IsIncomplete && !imported.Incomplete && shape.Signature != imported.Signature)
                || (!shape.IsIncomplete && imported.Incomplete))
                throw new CompileException("incompatible managed reference aggregate '" + node.OriginalName + "'");
            aliases[name] = imported.Target;
        }
        foreach (var name in aliases.Keys) declarations.Remove(name);
        return aliases;
    }

    private static IReadOnlyDictionary<string, string> ExportManagedSymbols(IReadOnlyDictionary<string, string> files,
        CSharpOutputOptions? options, string owner, IReadOnlyDictionary<string, string> declarations,
        IReadOnlyDictionary<string, ObjectAggregateMetadata> aggregates, IReadOnlyList<AggregateIdentity>?[] graphs,
        IReadOnlyDictionary<string, string>[] variants, IReadOnlyDictionary<string, string> anonymousTypedefs,
        NestedTagLayout tags, IEnumerable<string> definitions)
    {
        if (options?.ExportManagedSymbols != true) return files;
        var names = AnonymousTypedefNames.Plan(anonymousTypedefs, declarations.Keys, definitions);
        var candidates = new Dictionary<string, List<ManagedTypeSymbol>>(StringComparer.Ordinal);
        for (int index = 0; index < graphs.Length; index++)
        foreach (var node in graphs[index] ?? Array.Empty<AggregateIdentity>())
        {
            var name = variants[index].GetValueOrDefault(node.Name, node.Name);
            if (!declarations.ContainsKey(name)) continue;
            var shape = aggregates.GetValueOrDefault(name, node.Shape);
            var target = tags.TypeNames.GetValueOrDefault(name, owner + "." + name);
            target = AnonymousTypedefNames.Rewrite(target, names);
            if (!candidates.TryGetValue(node.Name, out var list)) candidates[node.Name] = list = [];
            list.Add(new(target, shape.IsIncomplete, shape.IsUnion, shape.Signature));
        }
        var types = candidates.Where(pair => pair.Value.Distinct().Count() == 1)
            .OrderBy(pair => pair.Key, StringComparer.Ordinal).ToDictionary(pair => pair.Key, pair => pair.Value[0]);
        var metadata = new ManagedSymbols(1, owner, AggregateIdentities.Hash(_cRuntimeBlock.Value), types,
            definitions.Distinct().Order(StringComparer.Ordinal).ToArray());
        var result = new Dictionary<string, string>(files, StringComparer.Ordinal);
        result.Add(ManagedSymbolsFile, JsonSerializer.Serialize(metadata, ManagedSymbolsContext.Default.ManagedSymbols) + "\n");
        return result;
    }
}
