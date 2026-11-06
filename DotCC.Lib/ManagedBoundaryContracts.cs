using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotCC.Backends;
using DotCC.Ir;
using DotCC.Layout;
using IrType = DotCC.Ir.CType;

namespace DotCC;

// A managed C caller and a Zig definition retain their own nominal types. The
// linker can connect them only through a checked adapter, never by treating a
// matching byte count (or a matching tag spelling) as declaration identity.
internal static class ManagedBoundaryContracts
{
    private const string Version = "//!!dotcc-obj managed-boundary-contracts:1";
    private const string Prefix = "//!!dotcc-obj managed-boundary-type:";
    private const string RootPrefix = "//!!dotcc-obj managed-boundary-root:";
    internal const string SourceProfilePrefix = "//!!dotcc-obj managed-boundary-source-profile:";
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private static string Encode(string value) => Convert.ToBase64String(Utf8.GetBytes(value));
    private static string Decode(string value) => Utf8.GetString(Convert.FromBase64String(value));

    internal static string Serialize(IReadOnlyList<AggregateIdentity> graph)
    {
        if (graph is not AggregateIdentitySet set) return "";
        return Version + "\n" + string.Concat(graph.Where(n => n.Shape.SourceShape is not null).OrderBy(n => n.Name, StringComparer.Ordinal)
            .Select(n => Prefix + n.Name + " " + Encode(n.Shape.SourceShape!) + "\n"))
            + string.Concat(set.Linkage.OrderBy(r => r.Symbol, StringComparer.Ordinal).Select(r => RootPrefix + Encode(r.Symbol)
                + " " + AggregateIdentities.Hash(r.Type) + " " + (r.ManagedBoundarySupported ? "1" : "0") + "\n"));
    }

    // These source attributes are not encoded by the older CGraph descriptor.
    // Withhold an adapter witness instead of silently dropping their semantics.
    internal static bool RepresentedType(IrType type) => type.AlignmentOverride == 0 && (type switch {
        IrType.Prim or IrType.VoidType or IrType.Named => true,
        IrType.Enum enumeration => RepresentedType(enumeration.Underlying),
        IrType.Pointer pointer => (pointer.ZigSourceAlignment is null or 0) && RepresentedType(pointer.Pointee),
        IrType.Array array => array.ZigSentinel is null && array.RuntimeCount is null && RepresentedType(array.Element),
        IrType.Func function => RepresentedType(function.Return) && function.Params.All(RepresentedType),
        _ => false,
    });

    internal static IReadOnlyList<AggregateIdentity>? Attach(string source, IReadOnlyList<AggregateIdentity>? graph)
    {
        var lines = source.Split('\n');
        var records = lines.Where(l => l.StartsWith(Prefix, StringComparison.Ordinal)).ToArray();
        var roots = lines.Where(l => l.StartsWith(RootPrefix, StringComparison.Ordinal)).ToArray();
        if (!lines.Contains(Version, StringComparer.Ordinal))
        {
            if (records.Length != 0 || roots.Length != 0) throw new CompileException("managed boundary contracts lack version metadata; regenerate objects");
            return graph;
        }
        if (graph is null || lines.Count(l => l == Version) != 1)
            throw new CompileException("invalid managed boundary contract version/identity metadata");
        var shapes = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            foreach (var line in records)
            {
                var p = line[Prefix.Length..].Split(' ');
                if (p.Length != 2 || !shapes.TryAdd(p[0], Decode(p[1])))
                    throw new FormatException();
            }
        }
        catch (Exception error) when (error is FormatException or DecoderFallbackException)
        { throw new CompileException("invalid managed boundary declared shape metadata", error); }
        if (shapes.Any(p => graph.SingleOrDefault(n => n.Name == p.Key) is not { } node || AggregateIdentities.Hash(p.Value) != node.Shape.Signature))
            throw new CompileException("managed boundary declared shapes do not match aggregate identity proof; regenerate objects");
        var nodes = graph.Select(n => n with { Shape = n.Shape with { SourceShape = shapes.GetValueOrDefault(n.Name) } }).ToArray();
        if (graph is not AggregateIdentitySet set) return nodes;
        var witnesses = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var line in roots)
        {
            var p = line[RootPrefix.Length..].Split(' ');
            if (p.Length != 3 || p[2] is not ("0" or "1")) throw new CompileException("invalid managed boundary root witness");
            string symbol;
            try { symbol = Decode(p[0]); }
            catch (Exception error) when (error is FormatException or DecoderFallbackException)
            { throw new CompileException("invalid managed boundary root name", error); }
            if (set.Linkage.SingleOrDefault(r => r.Symbol == symbol) is not { } root || AggregateIdentities.Hash(root.Type) != p[1]
                || !witnesses.TryAdd(symbol, p[2] == "1"))
                throw new CompileException("managed boundary root witness does not match declared contract");
        }
        return new AggregateIdentitySet(nodes, set.Linkage.Select(r => r with { ManagedBoundarySupported = witnesses.GetValueOrDefault(r.Symbol) }).ToArray())
            { Declarations = set.Declarations };
    }

    internal sealed record Bridge(int Caller, int Definition, string Symbol, string Name, IrType.Func CallerType,
        IrType.Func DefinitionType, IReadOnlyList<(string C, string Zig, int Size)> Storage);
    internal sealed record Plan(IReadOnlyList<Bridge> Bridges, IReadOnlyDictionary<string, string>[] Renames);

    internal static Plan Prepare(IReadOnlyList<string> texts, IReadOnlyList<string> paths,
        IReadOnlyList<AggregateIdentity>?[] graphs, EmitMode emit, bool instanceMethods)
    {
        var renames = texts.Select(_ => new Dictionary<string, string>(StringComparer.Ordinal)).ToArray();
        var bridges = new List<Bridge>();
        bool IsZig(int unit) => texts[unit].Split('\n').Contains("//!!dotcc-obj source-language:zig", StringComparer.Ordinal);
        var definitions = texts.Select(text => text.Split('\n').Where(l => l.StartsWith("//!!dotcc-obj def:", StringComparison.Ordinal))
            .Select(l => l["//!!dotcc-obj def:".Length..]).ToHashSet(StringComparer.Ordinal)).ToArray();
        // One caller-side handle cannot mean two different Zig objects, even
        // across distinct functions. Each nominal pair is established by a
        // declared signature position and remains local to these two units.
        var identities = new Dictionary<(int C, int Zig, string Name), string>();
        var reverse = new Dictionary<(int C, int Zig, string Name), string>();
        for (int caller = 0; caller < texts.Count; caller++)
        {
            if (IsZig(caller) || graphs[caller] is not AggregateIdentitySet cgraph) continue;
            foreach (var root in cgraph.Linkage)
            {
                if (definitions[caller].Contains(root.Symbol)) continue;
                var targets = Enumerable.Range(0, texts.Count).Where(unit => IsZig(unit) && definitions[unit].Contains(root.Symbol)).ToArray();
                if (targets.Length != 1) continue; // Existing duplicate/unresolved-symbol checks retain ownership.
                int target = targets[0];
                if (graphs[target] is not AggregateIdentitySet zgraph
                    || zgraph.Linkage.SingleOrDefault(r => r.Symbol == root.Symbol) is not { } zroot) continue;
                if (root.Type == zroot.Type && root.References.SequenceEqual(zroot.References, StringComparer.Ordinal)) continue;
                if (emit != EmitMode.ManagedLib || instanceMethods)
                    throw new CompileException("cross-language C/Zig type conversion requires static managed-library output: " + root.Symbol);
                // Curated lowering intentionally erases some source pointer
                // attributes. It cannot certify the actual source C boundary.
                if (!texts[target].Split('\n').Contains(SourceProfilePrefix + "source", StringComparer.Ordinal))
                    throw new CompileException("managed C/Zig boundary requires source stdlib type witnesses; regenerate the Zig object with stdlib_types source");
                if (!root.Type.StartsWith("function|", StringComparison.Ordinal) || !zroot.Type.StartsWith("function|", StringComparison.Ordinal))
                    throw new CompileException("cross-language managed object contracts are unsupported: " + root.Symbol);
                if (!root.ManagedBoundarySupported || !zroot.ManagedBoundarySupported)
                    throw new CompileException("missing or unsupported managed boundary signature witness for '" + root.Symbol + "'; regenerate objects");
                if (texts[caller].Split('\n').Contains("//!!dotcc-obj function-address:" + root.Symbol, StringComparer.Ordinal))
                    throw new CompileException("cross-language managed function-address adapters are unsupported: " + root.Symbol);
                var context = new Proof(cgraph, zgraph, root.Symbol, paths[caller], paths[target],
                    (c, z) => {
                        var key = (caller, target, c); var other = (caller, target, z);
                        if (identities.TryGetValue(key, out var prior) && prior != z
                            || reverse.TryGetValue(other, out var previous) && previous != c)
                            throw new CompileException("conflicting managed boundary nominal correspondences for '" + root.Symbol + "'");
                        identities[key] = z; reverse[other] = c;
                    });
                var ctype = ParseType(root.Type["function|".Length..]) as IrType.Func
                    ?? throw new CompileException("invalid managed boundary function contract");
                var ztype = ParseType(zroot.Type["function|".Length..]) as IrType.Func
                    ?? throw new CompileException("invalid managed boundary function contract");
                context.Compare(ctype, ztype);
                string name = "__DotCcManagedCBridge_" + AggregateIdentities.Hash(caller + "|" + target + "|" + root.Symbol)[..24];
                renames[caller].Add(root.Symbol, name);
                bridges.Add(new(caller, target, root.Symbol, name, ctype, ztype, context.Storage.ToArray()));
            }
        }
        if (bridges.Count != 0)
        {
            // Redirecting two C callers to separate adapters must not erase
            // their original mutual C declaration obligations.
            var callers = Enumerable.Range(0, texts.Count).Where(unit => !IsZig(unit)).ToArray();
            if (callers.Length > 1) AggregateIdentities.Plan(callers.Select(unit => (paths[unit], graphs[unit])).ToArray());
        }
        // The original cross-language symbol contract was discharged above.
        // The caller now refers to a real adapter with its exact declared type;
        // all ordinary C and Zig nominal linkage checks still run unmodified.
        foreach (var group in bridges.GroupBy(b => b.Caller))
        {
            var graph = (AggregateIdentitySet)graphs[group.Key]!;
            var map = renames[group.Key];
            AggregateLinkageContract Rename(AggregateLinkageContract c) => c with { Symbol = map.GetValueOrDefault(c.Symbol, c.Symbol) };
            graphs[group.Key] = new AggregateIdentitySet(graph.Nodes, graph.Linkage.Select(Rename).ToArray())
                { Declarations = graph.Declarations.Select(Rename).ToArray() };
        }
        return new(bridges, renames);
    }

    internal static CSharpFunctionSource Emit(Bridge bridge, IReadOnlyDictionary<string, string>[] variants)
    {
        var ct = new CSharpTarget(aggregateNames: variants[bridge.Caller]);
        var zt = new CSharpTarget(aggregateNames: variants[bridge.Definition]);
        string ConvertValue(IrType from, IrType to, string value, CSharpTarget target)
            => from is IrType.Named && to is IrType.Named
                ? "*(" + target.RenderType(to) + "*)&" + value
                : "(" + target.RenderType(to) + ")(" + value + ")";
        // Library shells promote the backend's ordinary `static unsafe`
        // functions. This compiler-owned adapter stays private in either shell.
        var code = new StringBuilder("private unsafe static ").Append(ct.RenderType(bridge.CallerType.Return)).Append(' ')
            .Append(bridge.Name).Append('(').Append(string.Join(", ", bridge.CallerType.Params.Select((p, i) => ct.RenderType(p) + " p" + i))).Append(")\n{\n");
        // Validate the emitted unmanaged storage before any copy or alias. This
        // catches a backend/layout-model disagreement without inventing bytes.
        foreach (var item in bridge.Storage.Distinct())
            code.Append("    if (sizeof(").Append(ct.RenderType(new IrType.Named(item.C))).Append(") != ").Append(item.Size)
                .Append(" || sizeof(").Append(zt.RenderType(new IrType.Named(item.Zig))).Append(") != ").Append(item.Size)
                .Append(") throw new global::System.InvalidOperationException(\"managed C/Zig boundary layout mismatch\");\n");
        string call = EmitHelpers.Id(bridge.Symbol) + "(" + string.Join(", ", bridge.CallerType.Params.Select((p, i) =>
            ConvertValue(p, bridge.DefinitionType.Params[i], "p" + i, zt))) + ")";
        if (bridge.CallerType.Return is IrType.VoidType) code.Append("    ").Append(call).Append(";\n");
        else if (bridge.CallerType.Return is IrType.Named)
            code.Append("    var result = ").Append(call).Append(";\n    return ").Append(ConvertValue(bridge.DefinitionType.Return, bridge.CallerType.Return, "result", ct)).Append(";\n");
        else code.Append("    return ").Append(ConvertValue(bridge.DefinitionType.Return, bridge.CallerType.Return, call, ct)).Append(";\n");
        return new(bridge.Name, code.Append("}\n").ToString());
    }

    private sealed class Proof(AggregateIdentitySet cgraph, AggregateIdentitySet zgraph, string symbol, string cpath,
        string zpath, Action<string, string> bind)
    {
        private readonly Dictionary<string, AggregateIdentity> c = cgraph.ToDictionary(n => n.Name, StringComparer.Ordinal);
        private readonly Dictionary<string, AggregateIdentity> z = zgraph.ToDictionary(n => n.Name, StringComparer.Ordinal);
        private readonly HashSet<(string, string)> visited = [];
        internal readonly List<(string C, string Zig, int Size)> Storage = [];
        private void Fail(string reason) => throw new CompileException("conflicting managed C/Zig boundary contracts for '" + symbol
            + "' in '" + cpath + "' and '" + zpath + "': " + reason);
        private AggregateIdentity Node(Dictionary<string, AggregateIdentity> nodes, string name, bool requireShape = true)
        {
            if (!nodes.TryGetValue(name, out var node) || requireShape && node.Shape.SourceShape is null)
            {
                Fail("missing declared shape witness for '" + name + "'; regenerate objects");
                throw new InvalidOperationException();
            }
            return node;
        }
        internal void Compare(IrType left, IrType right, bool pointee = false, bool parameter = false)
        {
            if (!parameter && left.Quals != right.Quals) Fail("different qualifiers");
            switch (left, right)
            {
                case (IrType.VoidType, IrType.VoidType): return;
                case (IrType.Prim a, IrType.Prim b):
                    if (a.Name != b.Name || a.Bytes != b.Bytes || a.Integer != b.Integer || a.Signed != b.Signed) Fail("different scalar representation");
                    return;
                case (IrType.Pointer a, IrType.Pointer b):
                    // Declared void* is an explicitly untyped address, unlike an
                    // invented replacement for a typed aggregate/pointer edge.
                    if (a.Pointee is IrType.VoidType || b.Pointee is IrType.VoidType)
                    {
                        if (a.Pointee.Quals != b.Pointee.Quals) Fail("different untyped pointer qualifiers");
                        return;
                    }
                    Compare(a.Pointee, b.Pointee, pointee: true); return;
                case (IrType.Func a, IrType.Func b):
                    if (a.Variadic || b.Variadic || a.IsNativeCallConv != b.IsNativeCallConv || a.Params.Count != b.Params.Count)
                        Fail("different or unsupported callback/function signature");
                    Compare(a.Return, b.Return);
                    for (int i = 0; i < a.Params.Count; i++) Compare(a.Params[i], b.Params[i], parameter: true);
                    return;
                case (IrType.Array a, IrType.Array b):
                    if (a.Count != b.Count || a.Count is null) Fail("different or incomplete array extent");
                    Compare(a.Element, b.Element); return;
                case (IrType.Enum a, IrType.Enum b):
                    Compare(a.Underlying, b.Underlying);
                    var ca = Node(c, a.Name); var zb = Node(z, b.Name);
                    if (!ca.IsEnum || !zb.IsEnum) Fail("different enum kind");
                    bind(a.Name, b.Name);
                    var cm = ParseEnum(ca.Shape.SourceShape!); var zm = ParseEnum(zb.Shape.SourceShape!);
                    var commonMembers = cm.Members.Join(zm.Members, m => m.Name, m => m.Name, (x, y) => (x, y));
                    if (commonMembers.Any(pair => pair.x.Value != pair.y.Value)) Fail("different shared enum member values");
                    // C's INT_MAX sentinel reserves the representation without
                    // adding an engine state. All real integer values must agree;
                    // member spellings remain in their own nominal source enum.
                    if (!EnumValues(cm).SequenceEqual(EnumValues(zm))) Fail("different enum value domains");
                    return;
                case (IrType.Named a, IrType.Named b):
                    var cn = Node(c, a.Name, false); var zn = Node(z, b.Name, false);
                    if (cn.IsEnum || zn.IsEnum || cn.Shape.IsUnion != zn.Shape.IsUnion) Fail("different aggregate kind");
                    bind(a.Name, b.Name);
                    if (cn.Shape.IsIncomplete || zn.Shape.IsIncomplete)
                    {
                        if (!pointee) Fail("incomplete by-value aggregate");
                        return;
                    }
                    _ = Node(c, a.Name); _ = Node(z, b.Name);
                    if (!visited.Add((a.Name, b.Name))) return;
                    var cs = ParseStruct(a.Name, cn.Shape.SourceShape!); var zs = ParseStruct(b.Name, zn.Shape.SourceShape!);
                    if (!zs.IsUnion && zs.Layout != AggregateLayout.Sequential) Fail("Zig aggregate lacks extern storage contract");
                    if (cs.Fields.Any(f => f.BitWidth is not null) || zs.Fields.Any(f => f.BitWidth is not null))
                        Fail("unsupported bitfield storage");
                    var cl = Layout(a.Name, c); var zl = Layout(b.Name, z);
                    if (cl.Size != zl.Size || cl.Alignment != zl.Alignment) Fail("different aggregate extent/alignment");
                    if (cl.Size == 0 || cl.Alignment > 8) Fail("unsupported managed boundary aggregate storage profile");
                    if (cs.IsUnion)
                    {
                        // A C union can expose one alias where Zig's reflected
                        // extern union exposes several names for that exact
                        // alternative (Point.CValue is such a source contract).
                        // Every distinct declared alternative must occur on both
                        // sides, including padding arrays. Struct member names,
                        // enum domains and nested layouts remain in each key.
                        foreach (var common in cs.Fields.Join(zs.Fields, f => f.Name, f => f.Name, (x, y) => (x, y)))
                            Compare(common.x.Type, common.y.Type);
                        var cfields = cs.Fields.GroupBy(f => AlternativeKey(f.Type, c)).ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
                        var zfields = zs.Fields.GroupBy(f => AlternativeKey(f.Type, z)).ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
                        if (!cfields.Keys.Order(StringComparer.Ordinal).SequenceEqual(zfields.Keys.Order(StringComparer.Ordinal)))
                            Fail("different union alternatives");
                        foreach (var pair in cfields)
                        {
                            var other = zfields[pair.Key];
                            foreach (var cf in pair.Value) Compare(cf.Type, other[0].Type);
                            foreach (var zf in other.Skip(1)) Compare(pair.Value[0].Type, zf.Type);
                        }
                    }
                    else
                    {
                        if (cs.Fields.Count != zs.Fields.Count) Fail("different aggregate member counts");
                        for (int i = 0; i < cs.Fields.Count; i++)
                        {
                            var cf = cs.Fields[i]; var zf = zs.Fields[i];
                            if (cf.Name != zf.Name || cl.Offsets[cf.Name] != zl.Offsets[zf.Name]) Fail("different aggregate member names/offsets");
                            Compare(cf.Type, zf.Type);
                        }
                    }
                    Storage.Add((a.Name, b.Name, cl.Size));
                    return;
                default: Fail("different or unsupported source type kinds"); return;
            }
        }
        private string AlternativeKey(IrType type, Dictionary<string, AggregateIdentity> nodes, int depth = 0)
        {
            if (depth > 64) { Fail("excessive union alternative type depth"); return ""; }
            string Key(IrType t) => AlternativeKey(t, nodes, depth + 1);
            return ((int)type.Quals).ToString(CultureInfo.InvariantCulture) + ":" + (type switch {
                IrType.Prim => ObjectAggregateMetadata.Describe(type.Unqualified),
                IrType.Array array when array.Count is not null => "array:" + array.Count + ":" + Key(array.Element),
                IrType.Enum enumeration => EnumKey(Node(nodes, enumeration.Name).Shape.SourceShape!),
                IrType.Named named => AggregateKey(named.Name),
                // Alternative matching does not speculate about recursive
                // pointers/callbacks. Exact source descriptors can still match.
                IrType.Pointer or IrType.Func => ObjectAggregateMetadata.Describe(type.Unqualified),
                _ => throw new CompileException("unsupported managed boundary union alternative type"),
            });
            string EnumKey(string shape)
            {
                var enumeration = ParseEnum(shape);
                return "enum:" + Key(enumeration.Underlying) + ":" + string.Join(",", EnumValues(enumeration));
            }
            string AggregateKey(string name)
            {
                var definition = Node(nodes, name);
                if (definition.Shape.IsIncomplete) { Fail("incomplete union alternative"); return ""; }
                var aggregate = ParseStruct(name, definition.Shape.SourceShape!);
                if (aggregate.Fields.Any(f => f.BitWidth is not null)) { Fail("unsupported union alternative bitfields"); return ""; }
                var layout = Layout(name, nodes);
                var fields = aggregate.Fields.Select(f => (aggregate.IsUnion ? "" : Encode(f.Name) + ":")
                    + layout.Offsets[f.Name] + ":" + Key(f.Type));
                if (aggregate.IsUnion) fields = fields.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
                return (aggregate.IsUnion ? "union:" : "struct:") + layout.Size + ":" + layout.Alignment + ":" + string.Join("|", fields);
            }
        }
        private static IEnumerable<long> EnumValues(EnumTypeDef e) => e.Members.Where(m => !(m.Name.EndsWith("_MAX_VALUE", StringComparison.Ordinal)
            && e.Underlying is IrType.Prim { Bytes: 4, Signed: true } && m.Value == int.MaxValue)).Select(m => m.Value).Distinct().Order();
        private static LayoutInfo Layout(string name, Dictionary<string, AggregateIdentity> nodes)
        {
            var model = new OffsetLayoutModel(n => {
                if (!nodes.TryGetValue(n, out var node) || node.Shape.IsIncomplete || node.Shape.SourceShape is null)
                    throw new CompileException("missing managed boundary aggregate storage witness: " + n);
                var type = ParseStruct(n, node.Shape.SourceShape);
                var aggregate = new LayoutAggregate { Name = n, Union = type.IsUnion, Packed = type.Layout == AggregateLayout.Packed,
                    Pack = type.Pack, Alignment = type.Alignment };
                foreach (var f in type.Fields) aggregate.Fields.Add(new LayoutField { Name = f.Name,
                    Type = IrBuilder.LayoutType(f.Type), BitWidth = f.BitWidth, Alignment = f.Alignment });
                return aggregate;
            });
            try { return model.Aggregate(name); }
            catch (OffsetLayoutException error) { throw new CompileException("unsupported managed boundary layout: " + error.Message, error); }
        }
    }

    private static EnumTypeDef ParseEnum(string shape)
    {
        var parts = shape.Split('|');
        if (parts.Length < 3 || parts[0] != "enum") throw new CompileException("invalid managed boundary enum witness");
        return new("", ParseType(parts[1]), parts.Skip(2).Where(p => p.Length != 0).Select(p => {
            var pair = p.Split(':');
            if (pair.Length != 2) throw new CompileException("invalid managed boundary enum member witness");
            return new EnumMember(Decode(pair[0]), long.Parse(pair[1], CultureInfo.InvariantCulture));
        }).ToArray());
    }
    private static StructTypeDef ParseStruct(string name, string shape)
    {
        var parts = shape.Split('|');
        if (parts.Length < 4 || parts[0] is not ("struct" or "union") || parts.Contains("zig-storage:1", StringComparer.Ordinal))
            throw new CompileException("unsupported managed boundary aggregate witness: " + name);
        var fields = parts.Skip(4).Select(p => {
            int colon = p.IndexOf(':');
            if (colon < 0) throw new CompileException("invalid managed boundary field witness");
            var parser = new TypeParser(p[(colon + 1)..]);
            var type = parser.Read();
            var tail = parser.Remaining.Split(':');
            if (tail.Length != 4 || tail[0] != "" || tail[3] is not ("True" or "False"))
                throw new CompileException("invalid managed boundary field attributes");
            return new StructField(Decode(p[..colon]), type, tail[1] == "-" ? null : int.Parse(tail[1], CultureInfo.InvariantCulture),
                int.Parse(tail[2], CultureInfo.InvariantCulture), tail[3] == "True");
        }).ToArray();
        return new(name, fields, parts[0] == "union", (AggregateLayout)int.Parse(parts[1], CultureInfo.InvariantCulture),
            int.Parse(parts[2], CultureInfo.InvariantCulture), int.Parse(parts[3], CultureInfo.InvariantCulture));
    }
    private static IrType ParseType(string text)
    {
        var parser = new TypeParser(text); var type = parser.Read();
        if (parser.Remaining.Length != 0) throw new CompileException("trailing managed boundary type witness data");
        return type;
    }
    // Parse the already versioned, canonical declaration descriptors. No source
    // or target-language text is guessed, and unsupported kinds fail closed.
    private sealed class TypeParser(string text)
    {
        private int at;
        internal string Remaining => text[at..];
        private string Take(char end)
        {
            int next = text.IndexOf(end, at);
            if (next < 0) throw new CompileException("invalid managed boundary type witness");
            string value = text[at..next]; at = next + 1; return value;
        }
        private void End()
        {
            if (at >= text.Length || text[at++] != ')') throw new CompileException("invalid managed boundary type witness end");
        }
        internal IrType Read()
        {
            var quals = (TypeQual)int.Parse(Take(':'), CultureInfo.InvariantCulture);
            IrType result;
            if (text.AsSpan(at).StartsWith("void", StringComparison.Ordinal)) { at += 4; result = new IrType.VoidType(); }
            else
            {
                string kind = Take('(');
                switch (kind)
                {
                    case "prim":
                        string name = Decode(Take(',')); int bytes = int.Parse(Take(','), CultureInfo.InvariantCulture);
                        bool integer = bool.Parse(Take(',')); bool signed = bool.Parse(Take(')'));
                        result = new IrType.Prim(name, bytes, integer, signed); break;
                    case "named": result = new IrType.Named(Decode(Take(')'))); break;
                    case "enum":
                        string ename = Decode(Take(',')); var underlying = Read(); End();
                        result = new IrType.Enum(ename, underlying); break;
                    case "ptr": result = new IrType.Pointer(Read()); End(); break;
                    case "array":
                        string count = Take(','); var element = Read(); End();
                        result = new IrType.Array(element, count == "" ? null : int.Parse(count, CultureInfo.InvariantCulture)); break;
                    case "func":
                        var ret = Read();
                        if (at >= text.Length || text[at++] != ',') throw new CompileException("invalid managed boundary function witness");
                        bool variadic = bool.Parse(Take(',')); bool native = bool.Parse(Take(','));
                        var parameters = new List<IrType>();
                        while (at < text.Length && text[at] != ')')
                        {
                            parameters.Add(Read());
                            if (at < text.Length && text[at] == ';') at++;
                            else break;
                        }
                        End(); result = new IrType.Func(ret, parameters, variadic) { IsNativeCallConv = native }; break;
                    default: throw new CompileException("unsupported managed boundary declared type: " + kind);
                }
            }
            return result with { Quals = quals };
        }
    }
}
