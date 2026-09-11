// Compiler layout evaluation and C# constant emission; no Roslyn or runtime dependencies.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DotCC.Layout;

internal sealed class LayoutField
{
    public string Name = "";
    public string Type = "";
    public int? BitWidth;
}
internal sealed class LayoutAggregate
{
    public string Name = "";
    public bool Union;
    public bool Packed;
    public readonly List<LayoutField> Fields = new List<LayoutField>();
}
internal sealed class LayoutInfo
{
    public int Size;
    public int Alignment;
    public bool HasBitFieldTailReuse;
    public readonly Dictionary<string, int> Offsets = new Dictionary<string, int>(StringComparer.Ordinal);
    // Each actual storage unit is identified by its first source field index.
    // Consecutive bit-fields can share one unit; zero-length tails have none.
    public readonly Dictionary<int, int> StorageOffsets = new Dictionary<int, int>();
}
internal sealed class OffsetLayoutException : Exception
{
    public OffsetLayoutException(string message) : base(message) { }
}

/// <summary>Versioned LP64 storage model. Bitfields describe dotcc's lowered
/// storage units, not a promise that every host C compiler uses that packing.</summary>
internal sealed class OffsetLayoutModel
{
    private readonly Func<string, LayoutAggregate> resolve;
    private readonly Dictionary<string, LayoutInfo> cache = new Dictionary<string, LayoutInfo>(StringComparer.Ordinal);
    private readonly HashSet<string> active = new HashSet<string>(StringComparer.Ordinal);
    public OffsetLayoutModel(Func<string, LayoutAggregate> resolve) { this.resolve = resolve; }
    public LayoutInfo Type(string type)
    {
        if (type.StartsWith("p:", StringComparison.Ordinal))
        {
            var pieces = type.Split(':');
            if (pieces.Length != 3 || !int.TryParse(pieces[1], out var size) || !int.TryParse(pieces[2], out var align)
                || size <= 0 || align <= 0 || (align & (align - 1)) != 0)
                throw new OffsetLayoutException("Invalid scalar layout: " + type);
            return new LayoutInfo { Size = size, Alignment = align };
        }
        if (type.StartsWith("a:", StringComparison.Ordinal))
        {
            var end = type.IndexOf(':', 2);
            if (end < 0 || !int.TryParse(type.Substring(2, end - 2), out var count) || count < 0)
                throw new OffsetLayoutException("Invalid array layout: " + type);
            var element = Type(type.Substring(end + 1));
            return new LayoutInfo { Size = checked(count * element.Size), Alignment = element.Alignment };
        }
        if (type.StartsWith("n:", StringComparison.Ordinal)) return Aggregate(type.Substring(2));
        throw new OffsetLayoutException("Unsupported or incomplete layout: " + type);
    }
    public LayoutInfo Aggregate(string name)
    {
        if (cache.TryGetValue(name, out var cached)) return cached;
        if (!active.Add(name)) throw new OffsetLayoutException("Recursive aggregate storage: " + name);
        try
        {
            var aggregate = resolve(name);
            var result = new LayoutInfo { Alignment = 1 };
            var cursor = 0;
            var unitBytes = 0;
            var usedBits = 0;
            var unitOffset = 0;
            for (var fieldIndex = 0; fieldIndex < aggregate.Fields.Count; fieldIndex++)
            {
                var field = aggregate.Fields[fieldIndex];
                var layout = Type(field.Type);
                var alignment = aggregate.Packed ? 1 : layout.Alignment;
                if (field.BitWidth is int width)
                {
                    if (width < 0 || width > checked(layout.Size * 8) || layout.Size > 8)
                        throw new OffsetLayoutException("Invalid bit-field width: " + name + "." + field.Name);
                    if (width == 0) { unitBytes = 0; usedBits = 0; continue; }
                    if (unitBytes == layout.Size && usedBits + width <= layout.Size * 8)
                    { usedBits += width; continue; }
                    unitBytes = layout.Size;
                    usedBits = width;
                }
                else
                {
                    // Ordinary members may start after the occupied bytes of
                    // the final bitfield unit, rather than after its declared
                    // integer width. Keep the unit's alignment/backing extent;
                    // C# must overlay storage when the next member uses its tail.
                    if (unitBytes != 0 && !aggregate.Union && !aggregate.Packed)
                    {
                        cursor = checked(unitOffset + (usedBits + 7) / 8);
                        if (RoundUp(cursor, alignment) < unitOffset + unitBytes)
                            result.HasBitFieldTailReuse = true;
                    }
                    unitBytes = 0; usedBits = 0;
                }
                result.Alignment = Math.Max(result.Alignment, alignment);
                var offset = aggregate.Union ? 0 : RoundUp(cursor, alignment);
                if (field.BitWidth is not null) unitOffset = offset;
                if (field.BitWidth is null) result.Offsets.Add(field.Name, offset);
                if (layout.Size != 0) result.StorageOffsets.Add(fieldIndex, offset);
                cursor = aggregate.Union ? Math.Max(cursor, layout.Size) : checked(offset + layout.Size);
            }
            result.Size = RoundUp(cursor, result.Alignment);
            cache.Add(name, result);
            return result;
        }
        finally { active.Remove(name); }
    }
    public int Offset(string name, IReadOnlyList<string> path)
    {
        if (path.Count == 0) throw new OffsetLayoutException("Empty offsetof member designator");
        var total = 0;
        var currentType = "n:" + name;
        foreach (var segment in path)
        {
            if (segment.StartsWith("[", StringComparison.Ordinal))
            {
                if (!segment.EndsWith("]", StringComparison.Ordinal)
                    || !int.TryParse(segment.Substring(1, segment.Length - 2), NumberStyles.None, CultureInfo.InvariantCulture, out var index)
                    || !currentType.StartsWith("a:", StringComparison.Ordinal))
                    throw new OffsetLayoutException("Invalid indexed offsetof designator: " + segment);
                var end = currentType.IndexOf(':', 2);
                currentType = currentType.Substring(end + 1);
                total = checked(total + checked(index * Type(currentType).Size));
                continue;
            }
            if (!currentType.StartsWith("n:", StringComparison.Ordinal))
                throw new OffsetLayoutException("Non-aggregate offsetof path component: " + segment);
            name = currentType.Substring(2);
            var aggregate = resolve(name);
            var field = aggregate.Fields.FirstOrDefault(f => f.Name == segment);
            if (field == null) throw new OffsetLayoutException("Unknown offsetof member: " + name + "." + segment);
            if (field.BitWidth != null) throw new OffsetLayoutException("offsetof cannot address bit-field: " + name + "." + field.Name);
            total = checked(total + Aggregate(name).Offsets[field.Name]);
            currentType = field.Type;
        }
        return total;
    }
    private static int RoundUp(int value, int alignment) => checked((value + alignment - 1) / alignment * alignment);
}

internal sealed class OffsetRequest
{
    public string Name = "";
    public string Aggregate = "";
    public string[] Path = Array.Empty<string>();
    public int Expected;
}

/// <summary>Line-oriented, culture-independent contract embedded in generated C#.
/// Base64 encodes free text. dotcc emits constants directly from this layout description.</summary>
internal sealed class OffsetDocument
{
    public const string Start = "/* dotcc-layout-v1\n";
    public const string End = "end-dotcc-layout */";
    public readonly Dictionary<string, LayoutAggregate> Aggregates = new Dictionary<string, LayoutAggregate>(StringComparer.Ordinal);
    public readonly List<OffsetRequest> Requests = new List<OffsetRequest>();
    public static string RequestName(string aggregate, IReadOnlyList<string> path) =>
        "__DotccOffset_" + BitConverter.ToString(Encoding.UTF8.GetBytes(aggregate + "." + string.Join(".", path))).Replace("-", "");
    public OffsetDocument ForRequest(OffsetRequest request)
    {
        var document = new OffsetDocument();
        void Include(string name)
        {
            if (document.Aggregates.ContainsKey(name)) return;
            if (!Aggregates.TryGetValue(name, out var aggregate)) throw new OffsetLayoutException("Unknown aggregate: " + name);
            document.Aggregates.Add(name, aggregate);
            foreach (var field in aggregate.Fields)
            {
                var type = field.Type;
                while (type.StartsWith("a:", StringComparison.Ordinal)) type = type.Substring(type.IndexOf(':', 2) + 1);
                if (type.StartsWith("n:", StringComparison.Ordinal)) Include(type.Substring(2));
            }
        }
        Include(request.Aggregate);
        document.Requests.Add(request);
        return document;
    }
    public static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    private static string Decode(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));
    public string Serialize()
    {
        var text = new StringBuilder(Start).Append("abi\tlp64-le-dotcc-v1\n");
        foreach (var pair in Aggregates.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            var aggregate = pair.Value;
            text.Append("aggregate\t").Append(Encode(aggregate.Name)).Append('\t').Append(aggregate.Union ? '1' : '0').Append('\t').Append(aggregate.Packed ? '1' : '0').Append('\n');
            foreach (var field in aggregate.Fields)
                text.Append("field\t").Append(Encode(field.Name)).Append('\t').Append(Encode(field.Type)).Append('\t')
                    .Append(field.BitWidth?.ToString(CultureInfo.InvariantCulture) ?? "-").Append('\n');
        }
        foreach (var request in Requests.OrderBy(r => r.Name, StringComparer.Ordinal))
            text.Append("request\t").Append(request.Name).Append('\t').Append(Encode(request.Aggregate)).Append('\t')
                .Append(Encode(string.Join(".", request.Path))).Append('\t').Append(request.Expected.ToString(CultureInfo.InvariantCulture)).Append('\n');
        return text.Append(End).Append('\n').ToString();
    }
    public static IEnumerable<OffsetDocument> ReadSource(string source)
    {
        var cursor = 0;
        while ((cursor = source.IndexOf(Start, cursor, StringComparison.Ordinal)) >= 0)
        {
            cursor += Start.Length;
            var end = source.IndexOf(End, cursor, StringComparison.Ordinal);
            if (end < 0) throw new OffsetLayoutException("Unterminated dotcc layout metadata");
            var document = new OffsetDocument();
            LayoutAggregate? aggregate = null;
            var lines = source.Substring(cursor, end - cursor).Split('\n');
            if (lines.Length == 0 || lines[0] != "abi\tlp64-le-dotcc-v1")
                throw new OffsetLayoutException("Unsupported dotcc offsetof ABI");
            foreach (var line in lines.Skip(1))
            {
                if (line.Length == 0) continue;
                var fields = line.Split('\t');
                switch (fields[0])
                {
                    case "aggregate" when fields.Length == 4:
                        aggregate = new LayoutAggregate { Name = Decode(fields[1]), Union = fields[2] == "1", Packed = fields[3] == "1" };
                        document.Aggregates.Add(aggregate.Name, aggregate); break;
                    case "field" when fields.Length == 4 && aggregate != null:
                        aggregate.Fields.Add(new LayoutField { Name = Decode(fields[1]), Type = Decode(fields[2]), BitWidth = fields[3] == "-" ? (int?)null : int.Parse(fields[3], CultureInfo.InvariantCulture) }); break;
                    case "request" when fields.Length == 5:
                        if (!fields[1].StartsWith("__DotccOffset_", StringComparison.Ordinal) || fields[1].Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                            throw new OffsetLayoutException("Invalid offset declaration name");
                        document.Requests.Add(new OffsetRequest { Name = fields[1], Aggregate = Decode(fields[2]), Path = Decode(fields[3]).Split('.'), Expected = int.Parse(fields[4], CultureInfo.InvariantCulture) }); break;
                    default: throw new OffsetLayoutException("Invalid dotcc layout metadata record");
                }
            }
            yield return document;
            cursor = end + End.Length;
        }
    }
    public string Materialize()
    {
        var model = new OffsetLayoutModel(name => Aggregates.TryGetValue(name, out var aggregate) ? aggregate : throw new OffsetLayoutException("Unknown aggregate: " + name));
        var source = new StringBuilder();
        foreach (var request in Requests.OrderBy(r => r.Name, StringComparer.Ordinal))
        {
            var offset = model.Offset(request.Aggregate, request.Path);
            if (offset != request.Expected) throw new OffsetLayoutException("Constant evaluation/emission offsetof disagreement: " + request.Name);
            var layout = model.Aggregate(request.Aggregate);
            source.Append("internal static class ").Append(request.Name).Append("\n{\n    public const ulong Value = ")
                .Append(offset.ToString(CultureInfo.InvariantCulture)).Append("UL;\n    public const int Size = ").Append(layout.Size.ToString(CultureInfo.InvariantCulture))
                .Append(";\n    public const int Alignment = ").Append(layout.Alignment.ToString(CultureInfo.InvariantCulture)).Append(";\n}\n\n");
        }
        return source.ToString();
    }
}
