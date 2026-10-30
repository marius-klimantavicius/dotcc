using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using DotCC.Ir;

namespace DotCC;

// Tags have no C linkage. Equal complete definitions can share a managed type;
// unrelated private definitions must not acquire linkage merely from their tag.
// Pointer and callback edges matter even though they do not affect sizeof.
internal sealed record AggregateIdentity(string Name, string OriginalName, bool Tagged,
    bool Exposed, string[] Dependencies, ObjectAggregateMetadata Shape, bool IsEnum = false);

// Linkage belongs to the ordinary external symbol, never to a tag merely
// because some external declaration mentions it. References follow the
// canonical type's traversal order, including pointers and callbacks.
internal sealed record AggregateLinkageContract(string Symbol, string Type, string[] References, int?[] ArrayBounds);

internal sealed record AggregateIdentitySet(IReadOnlyList<AggregateIdentity> Nodes,
    IReadOnlyList<AggregateLinkageContract> Linkage) : IReadOnlyList<AggregateIdentity>
{
    // Per-tag projections of external function declarations. These constrain
    // matching tags without binding an unused function's entire signature.
    internal IReadOnlyList<AggregateLinkageContract> Declarations { get; init; } = [];
    public int Count => Nodes.Count;
    public AggregateIdentity this[int index] => Nodes[index];
    public IEnumerator<AggregateIdentity> GetEnumerator() => Nodes.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class AggregateIdentities
{
    internal const string Version = "//!!dotcc-obj aggregate-identities:1";
    internal const string Prefix = "//!!dotcc-obj aggregate-identity:";
    internal const string LinkageVersion = "//!!dotcc-obj aggregate-linkage:4";
    private const string EnumLinkageVersion = "//!!dotcc-obj aggregate-linkage:3";
    private const string ArrayLinkageVersion = "//!!dotcc-obj aggregate-linkage:2";
    private const string LegacyLinkageVersion = "//!!dotcc-obj aggregate-linkage:1";
    internal const string LinkagePrefix = "//!!dotcc-obj aggregate-linkage-root:";
    internal const string DeclarationPrefix = "//!!dotcc-obj aggregate-declaration-root:";
    private const string TokenPrefix = "__DotCcAggregate_";
    internal static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    internal static string Token(string name) => TokenPrefix + Hash(name);

    internal static IEnumerable<string> References(CType type)
    {
        switch (type)
        {
            case CType.Named n when !n.IsExternal: yield return n.Name; break;
            case CType.Enum e: yield return e.Name; break;
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

    internal static ObjectAggregateMetadata EnumShape(EnumTypeDef type) => new(false, false,
        Hash("enum|" + ObjectAggregateMetadata.Describe(type.Underlying) + "|"
            + string.Join("|", type.Members.OrderBy(m => m.Name, StringComparer.Ordinal)
                .Select(m => Encode(m.Name) + ":" + m.Value.ToString(CultureInfo.InvariantCulture)))));

    internal static string Serialize(IReadOnlyList<AggregateIdentity> graph) => Version + "\n" + string.Concat(
        graph.OrderBy(n => n.Name, StringComparer.Ordinal).Select(n => Prefix + n.Name + " "
            + Convert.ToBase64String(Encoding.UTF8.GetBytes(n.OriginalName)) + " "
            + (n.Tagged ? "1" : "0") + " " + (n.Exposed ? "1" : "0") + " "
            + string.Join(",", n.Dependencies) + (n.IsEnum ? " 1" : "") + "\n")) + (graph is AggregateIdentitySet semantic
            ? LinkageVersion + "\n" + string.Concat(semantic.Linkage.OrderBy(c => c.Symbol, StringComparer.Ordinal)
                .Select(c => SerializeContract(LinkagePrefix, c)))
                + string.Concat(semantic.Declarations.OrderBy(c => c.Symbol, StringComparer.Ordinal)
                    .ThenBy(c => c.References.Single(), StringComparer.Ordinal).Select(c => SerializeContract(DeclarationPrefix, c)))
            : "");

    private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    private static string SerializeContract(string prefix, AggregateLinkageContract contract) => prefix + Encode(contract.Symbol)
        + " " + Encode(contract.Type) + " " + string.Join(",", contract.References) + " "
        + string.Join(",", contract.ArrayBounds.Select(n => n?.ToString(CultureInfo.InvariantCulture) ?? "-")) + "\n";

    internal static AggregateLinkageContract DeclarationContract(string symbol, CType type, string name, CType? enumUnderlying)
    {
        // Erase unrelated aggregate names, not type structure. Pointer depth,
        // callback positions, scalar types, qualifiers and array bounds retain
        // the same normalization as an actual linkage contract. An empty tag
        // is an internal wildcard; no authored C tag can collide with it.
        CType Project(CType value) => value switch
        {
            CType.Named n when n.Name != name => n with { Name = "" },
            CType.Enum e when e.Name != name => new CType.Named("") { Quals = e.Quals },
            CType.Pointer p => p with { Pointee = Project(p.Pointee) },
            CType.Array a => a with { Element = Project(a.Element) },
            CType.Func f => f with { Return = Project(f.Return), Params = f.Params.Select(Project).ToArray() },
            _ => value,
        };
        var contract = LinkageContract(symbol, Project(type));
        // A transitively referenced enum may not occur directly in the function
        // type. Preserve its underlying type as well as its definition's shape.
        return contract with { References = [name],
            Type = (enumUnderlying == null ? "" : ObjectAggregateMetadata.Describe(enumUnderlying) + "|") + contract.Type };
    }

    internal static AggregateLinkageContract LinkageContract(string symbol, CType type)
    {
        var bounds = new List<int?>();
        CType Normalize(CType value, bool parameter = false)
        {
            if (parameter) value = value.Unqualified;
            return value switch
            {
                CType.Array a => Array(a),
                CType.Pointer p => p with { Pointee = Normalize(p.Pointee) },
                CType.Func f => f with { Return = Normalize(f.Return), Params = f.Params.Select(p => Normalize(p, true)).ToArray(), IsNativeCallConv = false },
                _ => value,
            };
        }
        CType Array(CType.Array value)
        {
            bounds.Add(value.Count);
            return value with { Count = null, Element = Normalize(value.Element) };
        }
        // The native/managed call-convention annotation is not a C type
        // distinction. Top-level parameter qualifiers are likewise ignored.
        var normalized = Normalize(type);
        string Key(CType value) => ObjectAggregateMetadata.Describe(value);
        return new(symbol, Key(normalized), References(type).ToArray(), bounds.ToArray());
    }

    internal static IReadOnlyList<AggregateIdentity>? Read(string source)
    {
        var lines = source.Split('\n');
        if (!lines.Contains(Version, StringComparer.Ordinal))
        {
            if (source.Contains("struct " + TokenPrefix, StringComparison.Ordinal)
                || lines.Any(l => l.StartsWith(Prefix, StringComparison.Ordinal) || l.StartsWith(LinkagePrefix, StringComparison.Ordinal)
                    || l.StartsWith(DeclarationPrefix, StringComparison.Ordinal)))
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
            if (p.Length is not (5 or 6) || !Identifier(p[0]) || p[2] is not ("0" or "1") || p[3] is not ("0" or "1")
                || p.Length == 6 && p[5] is not ("0" or "1"))
                throw new CompileException("invalid aggregate identity metadata; regenerate objects");
            if (!shapes.TryGetValue(p[0], out var shape))
                throw new CompileException("aggregate metadata missing for '" + p[0] + "'; regenerate older objects before linking");
            string original;
            try { original = Encoding.UTF8.GetString(Convert.FromBase64String(p[1])); }
            catch (FormatException) { throw new CompileException("invalid aggregate identity name; regenerate objects"); }
            var deps = p[4].Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (!Identifier(original) || deps.Any(n => !Identifier(n))
                || !result.TryAdd(p[0], new(p[0], original, p[2] == "1", p[3] == "1", deps, shape, p.Length == 6 && p[5] == "1")))
                throw new CompileException("invalid aggregate identity graph; regenerate objects");
        }
        if (result.Count != shapes.Count || result.Values.Any(n => n.Dependencies.Any(d => !result.ContainsKey(d))))
            throw new CompileException("incomplete aggregate identity graph; regenerate objects");
        bool currentLinkage = lines.Contains(LinkageVersion, StringComparer.Ordinal);
        if (!currentLinkage && (result.Values.Any(n => n.IsEnum) || lines.Any(line => line.StartsWith("public enum ", StringComparison.Ordinal)
            || line.StartsWith("enum ", StringComparison.Ordinal))))
            throw new CompileException("object lacks enum identity metadata; regenerate older objects before linking");
        // Version 3 can drop unused struct/union declarations too. Its missing
        // roots cannot certify compatibility for a nonempty graph. An empty
        // graph has no typed declarations to prove and can keep linked checks
        // active for the other units, provided it contains no root records.
        if (!currentLinkage && lines.Contains(EnumLinkageVersion, StringComparer.Ordinal))
        {
            if (result.Count != 0 || lines.Any(line => line.StartsWith(LinkagePrefix, StringComparison.Ordinal)
                || line.StartsWith(DeclarationPrefix, StringComparison.Ordinal)))
                throw new CompileException("object lacks aggregate declaration metadata; regenerate older objects before linking");
            return new AggregateIdentitySet([], []);
        }
        if (!currentLinkage && !lines.Contains(ArrayLinkageVersion, StringComparer.Ordinal)
            && !lines.Contains(LegacyLinkageVersion, StringComparer.Ordinal))
        {
            if (lines.Any(l => l.StartsWith(LinkagePrefix, StringComparison.Ordinal)))
                throw new CompileException("aggregate linkage roots lack version metadata; regenerate objects");
            return result.Values.ToArray();
        }
        var roots = new Dictionary<string, AggregateLinkageContract>(StringComparer.Ordinal);
        var declarations = new Dictionary<(string Symbol, string Name), AggregateLinkageContract>();
        foreach (var line in lines.Where(l => l.StartsWith(LinkagePrefix, StringComparison.Ordinal)
            || currentLinkage && l.StartsWith(DeclarationPrefix, StringComparison.Ordinal)))
        {
            bool declaration = line.StartsWith(DeclarationPrefix, StringComparison.Ordinal);
            var p = line[(declaration ? DeclarationPrefix.Length : LinkagePrefix.Length)..].Split(' ');
            if (p.Length != 4) throw new CompileException("invalid aggregate linkage metadata; regenerate objects");
            try
            {
                string symbol = Encoding.UTF8.GetString(Convert.FromBase64String(p[0]));
                string type = Encoding.UTF8.GetString(Convert.FromBase64String(p[1]));
                var refs = p[2].Split(',', StringSplitOptions.RemoveEmptyEntries);
                var bounds = p[3].Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => n == "-" ? (int?)null : int.Parse(n, CultureInfo.InvariantCulture)).ToArray();
                if (symbol.Length == 0 || type.Length == 0 || refs.Length == 0 || refs.Any(n => !result.ContainsKey(n))
                    || bounds.Any(n => n < 0) || (declaration
                        ? refs.Length != 1 || !declarations.TryAdd((symbol, refs[0]), new(symbol, type, refs, bounds))
                        : !roots.TryAdd(symbol, new(symbol, type, refs, bounds))))
                    throw new CompileException("invalid aggregate linkage metadata; regenerate objects");
            }
            catch (Exception error) when (error is FormatException or OverflowException)
            { throw new CompileException("invalid aggregate linkage metadata; regenerate objects", error); }
        }
        // Version 1 loses incomplete-array declaration provenance; version 2
        // omits enum identities and their external/field edges. Keep both under
        // the conservative exposure gate, never infer missing semantic facts
        // from another TU or from generated code.
        return currentLinkage ? new AggregateIdentitySet(result.Values.ToArray(), roots.Values.ToArray())
            { Declarations = declarations.Values.ToArray() } : result.Values.ToArray();
    }

    internal static IReadOnlyDictionary<string, string>[] Plan(
        IReadOnlyList<(string Origin, IReadOnlyList<AggregateIdentity>? Graph)> units)
    {
        units = ZigStorageObjectMetadata.ScopePrivateIdentities(units);
        // Exposure-only objects cannot prove which extern declaration connects
        // two graphs. Preserve their conservative ABI gate, including mixed
        // old/new inputs, rather than guessing from emitted C# uses.
        if (units.Any(u => u.Graph is not AggregateIdentitySet)) return PlanLegacy(units);
        return PlanLinked(units);
    }

    private static IReadOnlyDictionary<string, string>[] PlanLinked(
        IReadOnlyList<(string Origin, IReadOnlyList<AggregateIdentity>? Graph)> units)
    {
        var all = units.SelectMany((u, i) => u.Graph!.Select(n => (Node: n, Unit: i))).ToArray();
        var graphs = units.Select(u => (AggregateIdentitySet)u.Graph!).ToArray();
        var affected = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in all.GroupBy(p => p.Node.Name, StringComparer.Ordinal))
        {
            if (group.Select(p => (p.Node.Shape.IsUnion, p.Node.IsEnum)).Distinct().Count() <= 1
                && group.Where(p => !p.Node.Shape.IsIncomplete).Select(p => p.Node.Shape.Signature).Distinct(StringComparer.Ordinal).Count() <= 1)
                continue;
            if (group.Any(p => !p.Node.Tagged)) Conflict(group.Key, group.Select(p => p.Unit));
            affected.Add(group.Key);
        }
        bool changed;
        do
        {
            changed = false;
            foreach (var (node, _) in all)
                if (node.Dependencies.Any(affected.Contains)) changed |= affected.Add(node.Name);
        } while (changed);

        var contracts = graphs.SelectMany((g, unit) => g.Linkage.Select(c => (Contract: c, Unit: unit)))
            .GroupBy(p => p.Contract.Symbol, StringComparer.Ordinal).ToArray();
        var declarations = graphs.SelectMany((g, unit) => g.Declarations.Select(c => (Contract: c, Unit: unit)))
            .GroupBy(p => (p.Contract.Symbol, Name: p.Contract.References.Single())).ToArray();
        for (int unit = 0; unit < graphs.Length; unit++)
        {
            var nodes = graphs[unit].ToDictionary(n => n.Name, StringComparer.Ordinal);
            var reachable = new HashSet<string>(StringComparer.Ordinal);
            void Visit(string name)
            {
                if (!nodes.TryGetValue(name, out var node))
                    throw new CompileException("incomplete aggregate linkage graph; regenerate objects");
                if (reachable.Add(name)) foreach (var dep in node.Dependencies) Visit(dep);
            }
            foreach (var root in graphs[unit].Linkage.Concat(graphs[unit].Declarations))
                foreach (var name in root.References) Visit(name);
            if (nodes.Values.Any(n => n.Exposed != reachable.Contains(n.Name)))
                throw new CompileException("incomplete aggregate linkage roots in '" + units[unit].Origin + "'; regenerate objects");
        }
        void CheckContracts(IEnumerable<(AggregateLinkageContract Contract, int Unit)> group, string symbol)
        {
            var first = group.First();
            var bounds = first.Contract.ArrayBounds.ToArray();
            foreach (var item in group.Skip(1))
            {
                if (first.Contract.Type != item.Contract.Type
                    || !first.Contract.References.SequenceEqual(item.Contract.References, StringComparer.Ordinal)
                    || bounds.Length != item.Contract.ArrayBounds.Length)
                    ContractConflict(symbol, first.Unit, item.Unit);
                for (int i = 0; i < bounds.Length; i++)
                {
                    if (bounds[i] is { } known && item.Contract.ArrayBounds[i] is { } other && known != other)
                        ContractConflict(symbol, first.Unit, item.Unit);
                    bounds[i] ??= item.Contract.ArrayBounds[i];
                }
            }
        }
        foreach (var group in contracts) CheckContracts(group, group.Key);
        foreach (var group in declarations) CheckContracts(group, group.Key.Symbol);
        if (affected.Count == 0)
            return units.Select(_ => (IReadOnlyDictionary<string, string>)new Dictionary<string, string>(StringComparer.Ordinal)).ToArray();

        var entries = all.Where(p => affected.Contains(p.Node.Name)).ToArray();
        var lookup = units.Select(_ => new Dictionary<string, int>(StringComparer.Ordinal)).ToArray();
        for (int i = 0; i < entries.Length; i++) lookup[entries[i].Unit].Add(entries[i].Node.Name, i);
        var parents = Enumerable.Range(0, entries.Length).ToArray();
        var sizes = Enumerable.Repeat(1, entries.Length).ToArray();
        var complete = entries.Select((p, i) => p.Node.Shape.IsIncomplete ? -1 : i).ToArray();
        var dependencies = entries.Select(p => p.Node.Dependencies.Where(affected.Contains)
            .Select(name => lookup[p.Unit][name]).ToArray()).ToArray();
        var pending = new Queue<(int Left, int Right)>();
        int Find(int index)
        {
            while (parents[index] != index) { parents[index] = parents[parents[index]]; index = parents[index]; }
            return index;
        }
        bool Join(int left, int right)
        {
            bool joined = false;
            pending.Enqueue((left, right));
            while (pending.TryDequeue(out var pair))
            {
                int a = Find(pair.Left), b = Find(pair.Right);
                if (a == b) continue;
                var x = entries[a]; var y = entries[b];
                int ca = complete[a], cb = complete[b];
                if (x.Node.Name != y.Node.Name || x.Node.Shape.IsUnion != y.Node.Shape.IsUnion || x.Node.IsEnum != y.Node.IsEnum)
                    Conflict(x.Node.Name, new[] { x.Unit, y.Unit });
                if (ca >= 0 && cb >= 0)
                {
                    if (entries[ca].Node.Shape.Signature != entries[cb].Node.Shape.Signature)
                        Conflict(x.Node.Name, new[] { entries[ca].Unit, entries[cb].Unit });
                    foreach (var children in dependencies[ca].Zip(dependencies[cb])) pending.Enqueue(children);
                }
                if (sizes[a] < sizes[b]) (a, b) = (b, a);
                parents[b] = a; sizes[a] += sizes[b];
                complete[a] = ca >= 0 ? ca : cb;
                joined = true;
            }
            return joined;
        }
        // Matching external symbol/type positions impose identity, even for
        // unused declarations. A complete container also constrains every typed
        // child edge, through callback cycles as well as by-value fields.
        foreach (var group in contracts)
        {
            var first = group.First();
            foreach (var item in group.Skip(1))
                foreach (string name in first.Contract.References.Where(affected.Contains))
                    Join(lookup[first.Unit][name], lookup[item.Unit][name]);
        }
        foreach (var group in declarations.Where(g => affected.Contains(g.Key.Name)))
        {
            var first = group.First();
            foreach (var item in group.Skip(1))
                Join(lookup[first.Unit][group.Key.Name], lookup[item.Unit][group.Key.Name]);
        }

        Dictionary<int, int> Colors()
        {
            var roots = Enumerable.Range(0, entries.Length).Where(i => Find(i) == i).ToArray();
            Dictionary<int, int> Label(Func<int, string> key) => roots.GroupBy(key, StringComparer.Ordinal)
                .OrderBy(g => g.Key, StringComparer.Ordinal).SelectMany((g, color) => g.Select(i => (i, color)))
                .ToDictionary(p => p.i, p => p.color);
            var colors = Label(i => entries[i].Node.Name + "|" + entries[i].Node.Shape.IsUnion + "|" + entries[i].Node.IsEnum + "|"
                + (complete[i] < 0 ? "opaque" : entries[complete[i]].Node.Shape.Signature));
            for (;;)
            {
                int count = colors.Values.Distinct().Count();
                var next = Label(i => colors[i].ToString(CultureInfo.InvariantCulture) + "|"
                    + (complete[i] < 0 ? "" : string.Join(",", dependencies[complete[i]]
                        .Select(d => colors[Find(d)].ToString(CultureInfo.InvariantCulture)))));
                colors = next;
                // Refinement includes the old color, so classes never merge.
                // Equal class counts prove a fixed partition without relying
                // on arbitrary numeric color labels reaching a fixed point.
                if (colors.Values.Distinct().Count() == count) return colors;
            }
        }

        Dictionary<int, int> partition;
        for (;;)
        {
            partition = Colors();
            bool joined = false;
            foreach (var group in Enumerable.Range(0, entries.Length).Where(i => Find(i) == i)
                .GroupBy(i => entries[i].Node.Name, StringComparer.Ordinal).ToArray())
            {
                var candidates = group.Where(i => complete[i] >= 0).ToArray();
                if (candidates.Length == 0 || candidates.Select(i => partition[i]).Distinct().Count() != 1) continue;
                int target = candidates[0];
                foreach (int opaque in group.Where(i => complete[i] < 0
                    && entries[i].Node.Shape.IsUnion == entries[target].Node.Shape.IsUnion
                    && entries[i].Node.IsEnum == entries[target].Node.IsEnum).ToArray())
                    joined |= Join(opaque, target);
            }
            if (!joined) break;
        }
        var split = entries.Select((p, i) => (p.Node.Name, Color: partition[Find(i)]))
            .GroupBy(p => p.Name, StringComparer.Ordinal).Where(g => g.Select(p => p.Color).Distinct().Count() > 1)
            .Select(g => g.Key).ToHashSet(StringComparer.Ordinal);
        return units.Select((_, unit) => (IReadOnlyDictionary<string, string>)lookup[unit]
            .Where(p => split.Contains(p.Key)).ToDictionary(p => p.Key,
                p => "__DotCcPrivate_" + Hash(p.Key + "\0" + partition[Find(p.Value)].ToString(CultureInfo.InvariantCulture)),
                StringComparer.Ordinal)).ToArray();

        void Conflict(string name, IEnumerable<int> indices) => throw new CompileException(
            "conflicting aggregate declarations for '" + name + "' in "
            + string.Join(" and ", indices.Distinct().Select(i => "'" + units[i].Origin + "'")));
        void ContractConflict(string symbol, int first, int second) => throw new CompileException(
            "conflicting external type contracts for '" + symbol + "' in '" + units[first].Origin + "' and '" + units[second].Origin + "'");
    }

    // Only conflicts trigger partitioning. The reverse closure preserves typed
    // callback cycles and containers instead of substituting a largest layout or
    // erasing a pointer. An exposed complete graph remains a strong ABI check.
    private static IReadOnlyDictionary<string, string>[] PlanLegacy(
        IReadOnlyList<(string Origin, IReadOnlyList<AggregateIdentity>? Graph)> units)
    {
        var all = units.SelectMany((u, i) => (u.Graph ?? Array.Empty<AggregateIdentity>()).Select(n => (Node: n, Unit: i))).ToArray();
        var split = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in all.GroupBy(p => p.Node.Name, StringComparer.Ordinal))
        {
            var complete = group.Where(p => !p.Node.Shape.IsIncomplete).ToArray();
            var conflict = group.Select(p => (p.Node.Shape.IsUnion, p.Node.IsEnum)).Distinct().Count() > 1
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
        var names = graph.ToDictionary(n => Token(n.Name),
            n => EmitHelpers.Id(variants.GetValueOrDefault(n.Name, n.Name)), StringComparer.Ordinal);
        // Enum references use type-only aliases to survive C tag/ordinary-name
        // shadowing. Their exact encoded aliases and semantic marker payloads
        // must follow the same validated identity plan as the type tokens.
        foreach (var line in source.Split('\n'))
        {
            if (!line.StartsWith(Compiler.EnumAliasMarker, StringComparison.Ordinal)) continue;
            string token = line[Compiler.EnumAliasMarker.Length..];
            if (!names.TryGetValue(token, out var relocated)) continue;
            names[Compiler.EnumAliasName(token)] = Compiler.EnumAliasName(relocated);
            source = source.Replace(line + "\n", Compiler.EnumAliasMarker + relocated + "\n", StringComparison.Ordinal);
        }
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
