using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotCC.Ir;

namespace DotCC;

/// <summary>Conditional extension. CGraph4 and all C-only shape bytes retain their existing representation.</summary>
internal static class ZigStorageObjectMetadata
{
    private const string Version = "//!!dotcc-obj zig-storage-version:1";
    private const string Prefix = "//!!dotcc-obj zig-storage:";
    internal const string BodyPrefix = "//!!dotcc-zig-storage:";
    private static string Encode(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static string Decode(string text)
    {
        try { return StrictUtf8.GetString(Convert.FromBase64String(text)); }
        catch (Exception error) when (error is FormatException or DecoderFallbackException)
        { throw new CompileException("invalid Zig storage UTF-8/base64 descriptor", error); }
    }
    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    internal static string Contract(ZigStorageLayout proof) => string.Join("|", new[] {
        "zig-storage-v1", Number((int)proof.Kind), Encode(proof.SourceIdentity), Encode(proof.ProofOrigin),
        Encode(proof.BackingKind), proof.BackingSigned ? "1" : "0", Number(proof.LogicalBits),
        Number(proof.NativeSize), Number(proof.NativeAlignment), Encode(proof.Profile), Encode(proof.UnsupportedReason ?? ""),
        string.Join(";", proof.SourceMembers.Select(f => string.Join(",", Encode(f.Name),
            Encode(ObjectAggregateMetadata.Describe(f.SourceType)), Encode(f.TargetName), Number(f.BitOffset),
            Number(f.LogicalBits), f.Signed ? "1" : "0", f.HasDefault ? "1" : "0", f.HasSentinel ? "1" : "0"))) });

    internal static string ZeroOwnerContract(ZigZeroOwnerLayout proof) => string.Join("|", new[] {
        "zig-zero-owner-v1", Encode(proof.SourceCType.Name), Encode(proof.Profile), Number(proof.NativeSize), Number(proof.NativeAlignment),
        string.Join(";", proof.SourceMembers.Select((f, i) => string.Join(",", Encode(f.Name), Encode(ObjectAggregateMetadata.Describe(f.Type)),
            Encode(f.TargetName ?? EmitHelpers.Id(f.Name)), Number(proof.NativeOffsets[i] * 8), i == 0 ? "64" : i == 1 ? "0" : "8"))) });

    internal sealed record FieldProof(string Name, int Offset, int Width, string SourceType, string TargetName, bool Signed = false);
    internal sealed record ContractProof(ZigStorageKind Kind, string Identity, int Bits, int Size, int Alignment,
        string Profile, string Reason, FieldProof[] Fields, bool IsZeroOwner = false);
    internal static ContractProof ReadContract(string contract)
    {
        try
        {
            var p = contract.Split('|');
            if (p.Length == 6 && p[0] == "zig-zero-owner-v1")
            {
                var ownerFields = p[5].Split(';').Select(text =>
                {
                    var f = text.Split(',');
                    if (f.Length != 5) throw new FormatException();
                    _ = Decode(f[1]); _ = Decode(f[2]);
                    return new FieldProof(Decode(f[0]), int.Parse(f[3], CultureInfo.InvariantCulture), int.Parse(f[4], CultureInfo.InvariantCulture), Decode(f[1]), Decode(f[2]));
                }).ToArray();
                return ValidateContract(new(ZigStorageKind.Zero, Decode(p[1]), 0, int.Parse(p[3], CultureInfo.InvariantCulture),
                    int.Parse(p[4], CultureInfo.InvariantCulture), Decode(p[2]), "", ownerFields, true), "zero-owner", false, "r47-structural-native");
            }
            if (p.Length != 12 || p[0] != "zig-storage-v1" || !int.TryParse(p[1], out var kind) || kind is < 0 or > 2
                || p[5] is not ("0" or "1")) throw new FormatException();
            var fields = p[11].Length == 0 ? [] : p[11].Split(';').Select(s =>
            {
                var f = s.Split(',');
                if (f.Length != 8 || f.Skip(5).Any(v => v is not ("0" or "1"))) throw new FormatException();
                _ = Decode(f[1]); _ = Decode(f[2]);
                return new FieldProof(Decode(f[0]), int.Parse(f[3], CultureInfo.InvariantCulture), int.Parse(f[4], CultureInfo.InvariantCulture), Decode(f[1]), Decode(f[2]), f[5] == "1");
            }).ToArray();
            _ = Decode(p[3]); _ = Decode(p[4]);
            return ValidateContract(new((ZigStorageKind)kind, Decode(p[2]), int.Parse(p[6], CultureInfo.InvariantCulture),
                int.Parse(p[7], CultureInfo.InvariantCulture), int.Parse(p[8], CultureInfo.InvariantCulture), Decode(p[9]), Decode(p[10]), fields),
                Decode(p[4]), p[5] == "1", Decode(p[3]));
        }
        catch (Exception error) when (error is FormatException or OverflowException)
        { throw new CompileException("invalid Zig storage descriptor; regenerate objects", error); }
    }

    private static bool SourceName(string name) => name.Length > 0 && !name.Contains('\0');
    private static bool TargetName(string target)
    {
        var name = target.StartsWith('@') ? target[1..] : target;
        return name.Length > 0 && (char.IsAsciiLetter(name[0]) || name[0] == '_')
            && name.All(c => char.IsAsciiLetterOrDigit(c) || c == '_');
    }
    private static bool Primitive(string type, out int bytes, out bool signed, out bool boolean)
    {
        bytes = 0; signed = false; boolean = false;
        if (!type.StartsWith("0:prim(", StringComparison.Ordinal) || !type.EndsWith(')')) return false;
        var parts = type[7..^1].Split(',');
        if (parts.Length != 4 || parts[2] != "True" || parts[3] is not ("True" or "False")
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out bytes) || bytes is not (1 or 2 or 4 or 8)) return false;
        var name = Decode(parts[0]);
        signed = parts[3] == "True"; boolean = name == "_Bool";
        return SourceName(name) && (!boolean || bytes == 1 && !signed);
    }
    internal static string? NamedSource(string source) => source.StartsWith("0:named(", StringComparison.Ordinal) && source.EndsWith(')')
        ? Decode(source[8..^1]) : null;

    private static bool EnumSource(string source, out string identity, out string underlying)
    {
        identity = ""; underlying = "";
        if (!source.StartsWith("0:enum(", StringComparison.Ordinal) || !source.EndsWith(')')) return false;
        var comma = source.IndexOf(',', 7);
        if (comma < 0) return false;
        identity = Decode(source[7..comma]); underlying = source[(comma + 1)..^1];
        return SourceName(identity);
    }

    private static ContractProof ValidateContract(ContractProof proof, string backing, bool backingSigned, string origin)
    {
        void Invalid() => throw new CompileException("invalid Zig storage semantic descriptor (range/type/target/physical proof)");
        if (!SourceName(proof.Identity) || proof.Profile != ZigStorageLayout.AcceptedProfile || origin.Length == 0
            || proof.Fields.Length > 4096 || proof.Fields.Select(f => f.Name).Distinct(StringComparer.Ordinal).Count() != proof.Fields.Length
            || proof.Fields.Select(f => f.TargetName.TrimStart('@')).Distinct(StringComparer.Ordinal).Count() != proof.Fields.Length
            || proof.Fields.Any(f => !SourceName(f.Name) || !TargetName(f.TargetName) || f.Offset < 0 || f.Width < 0)) Invalid();
        if (proof.IsZeroOwner)
        {
            if (proof.Size != 16 || proof.Alignment != 8 || proof.Fields.Length != 3
                || !proof.Fields.Select(f => f.Offset).SequenceEqual(new[] { 0, 64, 64 })
                || !proof.Fields.Select(f => f.Width).SequenceEqual(new[] { 64, 0, 8 })) Invalid();
            if (!Primitive(proof.Fields[0].SourceType, out var headBytes, out var headSigned, out var headBool)
                || headBytes != 8 || headSigned || headBool
                || NamedSource(proof.Fields[1].SourceType) is not { Length: > 0 }
                || !Primitive(proof.Fields[2].SourceType, out var tailBytes, out var tailSigned, out var tailBool)
                || tailBytes != 1 || tailSigned || tailBool) Invalid();
            return proof;
        }
        if (proof.Kind == ZigStorageKind.Unsupported)
        {
            if (proof.Reason.Length == 0 || proof.Bits != 0 || proof.Size != 0 || proof.Alignment != 0 || proof.Fields.Length != 0) Invalid();
            return proof;
        }
        if (proof.Kind == ZigStorageKind.Zero)
        {
            if (proof.Bits != 0 || proof.Size != 0 || proof.Alignment != 1 || proof.Fields.Length != 0 || backing != "zero" || backingSigned) Invalid();
            return proof;
        }
        if (proof.Kind != ZigStorageKind.PackedInteger || proof.Bits is < 1 or > 64 || proof.Fields.Length == 0
            || !origin.StartsWith("r44-owner-ast:", StringComparison.Ordinal)
            || backing == "inferred-unsignedness-unspecified" && backingSigned
            || backing != "inferred-unsignedness-unspecified" && backing != "explicit-" + (backingSigned ? "i" : "u") + proof.Bits) Invalid();
        var physical = proof.Bits switch { <= 8 => (1, 1), <= 16 => (2, 2), <= 32 => (4, 4), _ => (8, 8) };
        if (proof.Size != physical.Item1 || proof.Alignment != physical.Item2) Invalid();
        long cursor = 0;
        foreach (var field in proof.Fields)
        {
            if (field.Width is < 1 or > 64 || field.Offset != cursor || (long)field.Offset + field.Width > proof.Bits) Invalid();
            if (Primitive(field.SourceType, out var bytes, out var signed, out var boolean))
            {
                if (field.Width > bytes * 8 || signed != field.Signed || boolean && field.Width != 1) Invalid();
            }
            else if (EnumSource(field.SourceType, out _, out var underlying))
            {
                if (!Primitive(underlying, out bytes, out signed, out boolean) || boolean || field.Width > bytes * 8 || signed != field.Signed) Invalid();
            }
            else if (NamedSource(field.SourceType) is not { Length: > 0 } || field.Signed) Invalid();
            cursor += field.Width;
        }
        if (cursor != proof.Bits) Invalid();
        return proof;
    }

    internal static string Marker(ZigStorageLayout proof) => BodyPrefix + AggregateIdentities.Hash(Contract(proof));
    internal static string Serialize(IReadOnlyDictionary<string, ObjectAggregateMetadata>? metadata)
    {
        var proofs = metadata?.Where(p => p.Value.ZigStorageContract is not null).ToArray();
        if (proofs is not { Length: > 0 }) return "";
        return Version + "\n" + string.Concat(proofs.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p =>
            Prefix + p.Key + " " + p.Value.Signature + " " + Encode(p.Value.ZigStorageContract!) + " " + Encode(p.Value.ZigStorageBaseShape!) + "\n"));
    }

    internal static IReadOnlyList<AggregateIdentity>? Attach(string source, IReadOnlyList<AggregateIdentity>? graph)
    {
        var proofs = new Dictionary<string, string>(StringComparer.Ordinal);
        var lines = source.Split('\n');
        if (lines.Any(l => l.StartsWith("//!!dotcc-obj zig-storage-version:", StringComparison.Ordinal) && l != Version)
            || lines.Count(l => l == Version) > 1)
            throw new CompileException("unsupported/malformed Zig storage metadata version");
        foreach (var line in lines.Where(l => l.StartsWith(Prefix, StringComparison.Ordinal)))
        {
            var p = line[Prefix.Length..].Split(' ');
            if (!lines.Contains(Version, StringComparer.Ordinal) || p.Length != 4 || graph is null)
                throw new CompileException("missing Zig storage version/identity proof; regenerate objects");
            var contract = Decode(p[2]);
            var proof = ReadContract(contract);
            if (p[1] != AggregateIdentities.Hash(Decode(p[3]) + "|zig-storage:1|" + contract)
                || proof.Identity != p[0] || proof.Profile != ZigStorageLayout.AcceptedProfile
                || !ZigStorageLayout.CompilerModelHostMatches
                || graph.SingleOrDefault(n => n.Name == p[0]) is not { } node || node.Shape.Signature != p[1]
                || source.Contains("//!!dotcc-obj type:" + p[0] + "\n", StringComparison.Ordinal)
                    && (!source.Contains(BodyPrefix + AggregateIdentities.Hash(contract), StringComparison.Ordinal) || proof.Kind == ZigStorageKind.Unsupported)
                || !proofs.TryAdd(p[0], contract))
                throw new CompileException("mismatched Zig storage witness for '" + p[0] + "'; regenerate objects");
        }
        var parsed = proofs.ToDictionary(p => p.Key, p => ReadContract(p.Value), StringComparer.Ordinal);
        foreach (var proof in parsed.Values)
            foreach (var field in proof.Fields)
                if (NamedSource(field.SourceType) is { } child)
                {
                    if (!parsed.TryGetValue(child, out var nested) || nested.IsZeroOwner
                        || nested.Kind != (proof.IsZeroOwner ? ZigStorageKind.Zero : ZigStorageKind.PackedInteger)
                        || nested.Bits != field.Width)
                        throw new CompileException("missing/mismatched Zig source member storage dependency: " + child);
                }
                else if (EnumSource(field.SourceType, out var enumName, out _) && graph?.Any(n => n.Name == enumName && n.IsEnum) != true)
                    throw new CompileException("missing Zig source enum identity dependency: " + enumName);
        var active = new HashSet<string>(StringComparer.Ordinal);
        var done = new HashSet<string>(StringComparer.Ordinal);
        void Visit(string name, int depth)
        {
            if (done.Contains(name)) return;
            if (depth >= 32 || !active.Add(name)) throw new CompileException("cyclic/excessive Zig storage proof dependency");
            foreach (var field in parsed[name].Fields)
                if (NamedSource(field.SourceType) is { } child) Visit(child, depth + 1);
            active.Remove(name); done.Add(name);
        }
        foreach (var name in parsed.Keys) Visit(name, 0);
        var markers = lines.Where(l => l.TrimStart().StartsWith(BodyPrefix, StringComparison.Ordinal)).Select(l => l.Trim()).ToArray();
        if (markers.Any(m => !proofs.Values.Any(p => BodyPrefix + AggregateIdentities.Hash(p) == m)))
            throw new CompileException("object lacks complete Zig storage witnesses; regenerate objects");
        // An old Zig Pack=1 body has no native integer/zero proof. Do not upgrade it by layout coincidence.
        if (proofs.Count == 0 && lines.Contains("//!!dotcc-obj source-language:zig", StringComparer.Ordinal)
            && source.Contains("LayoutKind.Sequential, Pack = 1", StringComparison.Ordinal))
            throw new CompileException("legacy Zig packed object has no storage proof; regenerate objects");
        if (graph is null || proofs.Count == 0) return graph;
        var nodes = graph.Select(n => proofs.TryGetValue(n.Name, out var proof)
            ? n with { Shape = n.Shape with { ZigStorageContract = proof } } : n).ToArray();
        return graph is AggregateIdentitySet set ? new AggregateIdentitySet(nodes, set.Linkage) { Declarations = set.Declarations } : nodes;
    }

    internal static IReadOnlyList<(string Origin, IReadOnlyList<AggregateIdentity>? Graph)> ScopePrivateIdentities(
        IReadOnlyList<(string Origin, IReadOnlyList<AggregateIdentity>? Graph)> units)
    {
        if (!units.Any(u => u.Graph?.Any(n => n.Shape.ZigStorageContract is not null) == true)) return units;
        // Graph4's existing partition/Join logic decides identity from semantic linkage roots.
        // Zig source-scoped nodes may split just as private C tags do; name coincidence is not linkage.
        // Hashes include the proof, so every actual Join still checks missing/conflicting storage.
        return units.Select(u =>
        {
            if (u.Graph is null) return u;
            var scopedNames = u.Graph.Where(n => n.Shape.ZigStorageContract is not null).Select(n => n.Name).ToHashSet(StringComparer.Ordinal);
            foreach (var node in u.Graph.Where(n => n.Shape.ZigStorageContract is not null))
                foreach (var field in ReadContract(node.Shape.ZigStorageContract!).Fields)
                    if (EnumSource(field.SourceType, out var name, out _)) scopedNames.Add(name);
            var nodes = u.Graph.Select(n => scopedNames.Contains(n.Name) ? n with { Tagged = true } : n).ToArray();
            IReadOnlyList<AggregateIdentity> scoped = u.Graph is AggregateIdentitySet set
                ? new AggregateIdentitySet(nodes, set.Linkage) { Declarations = set.Declarations } : nodes;
            return (u.Origin, (IReadOnlyList<AggregateIdentity>?)scoped);
        }).ToArray();
    }
}
