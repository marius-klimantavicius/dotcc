using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotCC.Ir;

namespace DotCC;

// Tags have no C linkage. Equal complete definitions can share a managed type;
// unrelated private definitions must not acquire linkage merely from their tag.
// Pointer and callback edges matter even though they do not affect sizeof.
internal sealed record AggregateIdentity(string Name, string OriginalName, bool Tagged,
    bool Exposed, string[] Dependencies, ObjectAggregateMetadata Shape);

internal static class AggregateIdentities
{
    internal const string Version = "//!!dotcc-obj aggregate-identities:1";
    internal const string Prefix = "//!!dotcc-obj aggregate-identity:";
    private const string TokenPrefix = "__DotCcAggregate_";
    internal static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    internal static string Token(string name) => TokenPrefix + Hash(name);

    internal static IEnumerable<string> References(CType type)
    {
        switch (type)
        {
            case CType.Named n when !n.IsExternal: yield return n.Name; break;
            case CType.Pointer p:
                foreach (var name in References(p.Pointee)) yield return name;
                break;
            case CType.Array a:
                foreach (var name in References(a.Element)) yield return name;
                break;
            case CType.Func f:
                foreach (var name in References(f.Return)) yield return name;
                foreach (var p in f.Params)
                    foreach (var name in References(p)) yield return name;
                break;
        }
    }

    internal static string Serialize(IReadOnlyList<AggregateIdentity> graph) => Version + "\n" + string.Concat(
        graph.OrderBy(n => n.Name, StringComparer.Ordinal).Select(n => Prefix + n.Name + " "
            + Convert.ToBase64String(Encoding.UTF8.GetBytes(n.OriginalName)) + " "
            + (n.Tagged ? "1" : "0") + " " + (n.Exposed ? "1" : "0") + " "
            + string.Join(",", n.Dependencies) + "\n"));

    internal static IReadOnlyList<AggregateIdentity>? Read(string source)
    {
        var lines = source.Split('\n');
        if (!lines.Contains(Version, StringComparer.Ordinal))
        {
            if (source.Contains("struct " + TokenPrefix, StringComparison.Ordinal)
                || lines.Any(l => l.StartsWith(Prefix, StringComparison.Ordinal)))
                throw new CompileException("object lacks aggregate identity metadata; regenerate objects");
            return null;
        }
        var shapes = new Dictionary<string, ObjectAggregateMetadata>(StringComparer.Ordinal);
        const string aggregate = "//!!dotcc-obj aggregate:";
        foreach (var line in lines.Where(l => l.StartsWith(aggregate, StringComparison.Ordinal)))
        {
            var p = line[aggregate.Length..].Split(' ');
            if (p.Length != 4 || p[1] is not ("0" or "1") || p[2] is not ("0" or "1")
                || p[3].Length != 64 || !p[3].All(Uri.IsHexDigit) || !shapes.TryAdd(p[0], new(p[1] == "1", p[2] == "1", p[3])))
                throw new CompileException("invalid aggregate metadata; regenerate objects");
        }
        var result = new Dictionary<string, AggregateIdentity>(StringComparer.Ordinal);
        static bool Identifier(string s) => s.Length > 0 && s.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '@');
        foreach (var line in lines.Where(l => l.StartsWith(Prefix, StringComparison.Ordinal)))
        {
            var p = line[Prefix.Length..].Split(' ');
            if (p.Length != 5 || !Identifier(p[0]) || p[2] is not ("0" or "1") || p[3] is not ("0" or "1"))
                throw new CompileException("invalid aggregate identity metadata; regenerate objects");
            if (!shapes.TryGetValue(p[0], out var shape))
                throw new CompileException("aggregate metadata missing for '" + p[0] + "'; regenerate older objects before linking");
            string original;
            try { original = Encoding.UTF8.GetString(Convert.FromBase64String(p[1])); }
            catch (FormatException) { throw new CompileException("invalid aggregate identity name; regenerate objects"); }
            var deps = p[4].Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (!Identifier(original) || deps.Any(n => !Identifier(n))
                || !result.TryAdd(p[0], new(p[0], original, p[2] == "1", p[3] == "1", deps, shape)))
                throw new CompileException("invalid aggregate identity graph; regenerate objects");
        }
        if (result.Count != shapes.Count || result.Values.Any(n => n.Dependencies.Any(d => !result.ContainsKey(d))))
            throw new CompileException("incomplete aggregate identity graph; regenerate objects");
        return result.Values.ToArray();
    }

    // Only conflicts trigger partitioning. The reverse closure preserves typed
    // callback cycles and containers instead of substituting a largest layout or
    // erasing a pointer. An exposed complete graph remains a strong ABI check.
    internal static IReadOnlyDictionary<string, string>[] Plan(
        IReadOnlyList<(string Origin, IReadOnlyList<AggregateIdentity>? Graph)> units)
    {
        var all = units.SelectMany((u, i) => (u.Graph ?? Array.Empty<AggregateIdentity>()).Select(n => (Node: n, Unit: i))).ToArray();
        var split = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in all.GroupBy(p => p.Node.Name, StringComparer.Ordinal))
        {
            var complete = group.Where(p => !p.Node.Shape.IsIncomplete).ToArray();
            var conflict = group.Select(p => p.Node.Shape.IsUnion).Distinct().Count() > 1
                || complete.Select(p => p.Node.Shape.Signature).Distinct(StringComparer.Ordinal).Count() > 1;
            if (!conflict) continue;
            if (group.Any(p => !p.Node.Tagged)) Conflict(group.Key, group.Select(p => p.Unit));
            split.Add(group.Key);
        }
        bool changed;
        do
        {
            changed = false;
            foreach (var node in all.Select(p => p.Node))
                if (node.Dependencies.Any(split.Contains)) changed |= split.Add(node.Name);
        } while (changed);
        foreach (var group in all.Where(p => split.Contains(p.Node.Name) && p.Node.Exposed).GroupBy(p => p.Node.Name))
            Conflict(group.Key, all.Where(p => p.Node.Name == group.Key).Select(p => p.Unit));
        if (split.Count != 0 && units.Any(u => u.Graph is null))
            throw new CompileException("private aggregate variants require identity metadata; regenerate older objects");
        return units.Select(u => (IReadOnlyDictionary<string, string>)(u.Graph ?? Array.Empty<AggregateIdentity>())
            .Where(n => split.Contains(n.Name)).ToDictionary(n => n.Name,
                n => "__DotCcPrivate_" + Hash(u.Origin + "\0" + n.Name), StringComparer.Ordinal)).ToArray();

        void Conflict(string name, IEnumerable<int> indices)
        {
            var paths = indices.Distinct().Select(i => "'" + units[i].Origin + "'");
            throw new CompileException("conflicting aggregate declarations for '" + name + "' in " + string.Join(" and ", paths));
        }
    }

    internal static string Relocate(string source, IReadOnlyList<AggregateIdentity> graph,
        IReadOnlyDictionary<string, string> variants)
    {
        var names = graph.ToDictionary(n => Token(n.Name), n => variants.GetValueOrDefault(n.Name, n.Name), StringComparer.Ordinal);
        if (variants.Count != 0)
        {
            var partition = Hash(string.Join("\0", variants.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => p.Value)));
            foreach (var line in source.Split('\n'))
            {
                const string factory = "//!!dotcc-obj type:__DotccAggregateInit_";
                if (line.StartsWith(factory, StringComparison.Ordinal))
                {
                    var name = line["//!!dotcc-obj type:".Length..];
                    names[name] = name + "__private_" + partition;
                }
                const string ownerInitializer = "//!!dotcc-obj type:" + Compiler.OwnerInitializerPrefix;
                if (line.StartsWith(ownerInitializer, StringComparison.Ordinal))
                {
                    var name = line[ownerInitializer.Length..];
                    var relocated = name + "__private_" + partition;
                    names[name] = relocated;
                    names[Compiler.OwnerInitializerPrefix + name] = Compiler.OwnerInitializerPrefix + relocated;
                }
            }
        }
        // Inline-array wrapper identifiers are generated from the rendered owner.
        foreach (var line in source.Split('\n'))
        {
            if (!line.Contains("struct __IA_", StringComparison.Ordinal)) continue;
            var start = line.IndexOf("struct ", StringComparison.Ordinal) + 7;
            var identifier = line[start..].Trim();
            foreach (var pair in names.ToArray())
                if (identifier.StartsWith("__IA_" + pair.Key + "_", StringComparison.Ordinal))
                    names[identifier] = "__IA_" + pair.Value.TrimStart('@') + identifier[(5 + pair.Key.Length)..];
        }
        // Layout metadata is semantic, base64 encoded, and is relocated as data.
        // The numeric offsets/size/pack and the generated constant bodies stay exact.
        foreach (var document in Layout.OffsetDocument.ReadSource(source).ToArray())
        {
            var before = document.Serialize();
            foreach (var request in document.Requests)
            {
                request.Aggregate = variants.GetValueOrDefault(request.Aggregate, request.Aggregate);
                var name = Layout.OffsetDocument.RequestName(request.Aggregate, request.Path);
                names[request.Name] = name;
                request.Name = name;
            }
            var aggregates = document.Aggregates.Values.ToArray();
            document.Aggregates.Clear();
            foreach (var aggregate in aggregates)
            {
                aggregate.Name = variants.GetValueOrDefault(aggregate.Name, aggregate.Name);
                foreach (var field in aggregate.Fields)
                    foreach (var pair in variants)
                        if (field.Type.EndsWith("n:" + pair.Key, StringComparison.Ordinal))
                            field.Type = field.Type[..^pair.Key.Length] + pair.Value;
                document.Aggregates.Add(aggregate.Name, aggregate);
            }
            source = source.Replace(before, document.Serialize(), StringComparison.Ordinal);
        }
        source = GlobalStorageReferences.RelocateAddressTypes(source, names);
        source = AnonymousTypedefNames.Rewrite(source, names);
        if (variants.Count == 0) return source;
        var lines = source.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            const string type = "//!!dotcc-obj type:";
            const string aggregate = "//!!dotcc-obj aggregate:";
            if (lines[i].StartsWith(type, StringComparison.Ordinal))
            {
                var key = lines[i][type.Length..];
                lines[i] = type + variants.GetValueOrDefault(key, names.GetValueOrDefault(key, key));
            }
            else if (lines[i].StartsWith(aggregate, StringComparison.Ordinal))
            {
                var p = lines[i][aggregate.Length..].Split(' ');
                p[0] = variants.GetValueOrDefault(p[0], p[0]);
                lines[i] = aggregate + string.Join(" ", p);
            }
            else if (lines[i].StartsWith(AnonymousTypedefNames.MetadataPrefix, StringComparison.Ordinal))
            {
                // Ambiguous private clones have no single public alias.
                var key = lines[i][AnonymousTypedefNames.MetadataPrefix.Length..].Split(' ')[0];
                if (variants.ContainsKey(key)) lines[i] = "";
            }
            else if (lines[i].StartsWith(InlineFunctionMetadata.Prefix, StringComparison.Ordinal))
            {
                var (name, metadata) = InlineFunctionMetadata.Parse(lines[i]);
                // The old proof used the original type graph. Keep this body and
                // its address, but do not use that proof to fold a private variant.
                lines[i] = (metadata with { Shape = "-" }).Serialize(name);
            }
        }
        return string.Join("\n", lines);
    }
}
