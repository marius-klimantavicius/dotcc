using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DotObj;

internal readonly record struct EdgGraphField(long NodeId, string Name, long Ordinal,
    string ValueKind, string? Value, long? TargetId, int? TargetEntryKind)
{
    public void Write(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteNumber("nodeId", NodeId); writer.WriteString("name", Name);
        writer.WriteNumber("ordinal", Ordinal); writer.WriteString("valueKind", ValueKind);
        writer.WriteString("value", Value);
        if (TargetId is long target) writer.WriteNumber("targetId", target); else writer.WriteNull("targetId");
        if (TargetEntryKind is int kind) writer.WriteNumber("targetEntryKind", kind); else writer.WriteNull("targetEntryKind");
        writer.WriteEndObject();
    }
}

internal sealed partial class ObjectFile
{
    private void WriteEdgGraphDescriptor(Utf8JsonWriter writer)
    {
        // A source map references the exact retained typed object, rather than
        // expanding millions of shared raw fields into a second giant copy.
        if (physicalReceipt is null)
        {
            if (File.Exists(Path + "-wal") || File.Exists(Path + "-journal"))
                throw new InvalidDataException("EDG graph map requires a standalone object without an active journal");
            using var input = new FileStream(Path,FileMode.Open,FileAccess.Read,FileShare.Read);
            physicalReceipt = (input.Length,Convert.ToHexStringLower(SHA256.HashData(input)));
        }
        var receipt = physicalReceipt.Value;
        writer.WriteStartObject("edgGraph");
        writer.WriteString("format","dotobj-edg-graph-reference-v1");
        writer.WriteString("path",Path); writer.WriteString("sha256",receipt.Hash);
        writer.WriteNumber("byteLength",receipt.Size); writer.WriteString("semanticDigest",Digest);
        writer.WriteString("unitId",UnitId); writer.WriteString("profile","scalar-control-v66");
        writer.WriteStartObject("counts");
        using (var counts = database.Prepare("SELECT (SELECT count(*) FROM edg_graph_node_entries),(SELECT coalesce(sum(row_count),0) FROM edg_graph_field_blocks),(SELECT count(*) FROM edg_graph_roots),(SELECT count(*) FROM edg_graph_field_definitions),(SELECT count(*) FROM edg_graph_field_blocks)"))
        {
            counts.Read();
            string[] names = ["nodes","fields","roots","definitions","blocks"];
            for (int index=0;index<names.Length;index++) writer.WriteNumber(names[index],counts.Integer(index));
        }
        writer.WriteEndObject();
        writer.WriteStartObject("tables");
        writer.WriteString("nodes","edg_graph_nodes"); writer.WriteString("definitions","edg_graph_field_definitions");
        writer.WriteString("nodeEntries","edg_graph_node_entries"); writer.WriteString("nodeKinds","edg_graph_node_kinds");
        writer.WriteString("blocks","edg_graph_field_blocks"); writer.WriteString("roots","edg_graph_roots");
        writer.WriteString("coverage","edg_graph_coverage"); writer.WriteEndObject();
        writer.WriteStartObject("nodeRows");
        writer.WriteString("view","edg_graph_nodes");
        writer.WriteStartArray("columns");
        foreach(string column in new[]{"id","entry_kind","entry_kind_name","capture_phase"}) writer.WriteStringValue(column);
        writer.WriteEndArray(); writer.WriteEndObject();
        writer.WriteStartObject("fieldRows");
        writer.WriteString("accessor","ObjectFile.ReadEdgGraphFields");
        writer.WriteStartArray("blockEncodings");
        using (var encodings = database.Prepare("SELECT DISTINCT encoding FROM edg_graph_field_blocks ORDER BY encoding"))
            while (encodings.Read()) writer.WriteStringValue(encodings.Text(0));
        writer.WriteEndArray();
        writer.WriteStartArray("columns");
        foreach(string column in new[]{"nodeId","name","ordinal","valueKind","value","targetId","targetEntryKind"}) writer.WriteStringValue(column);
        writer.WriteEndArray(); writer.WriteStartArray("inspectionCommand");
        writer.WriteStringValue("dotobj-link"); writer.WriteStringValue("inspect-edg-graph"); writer.WriteStringValue(Path);
        writer.WriteEndArray(); writer.WriteEndObject(); writer.WriteEndObject();
    }

    private sealed record GraphFieldDefinition(int OwnerKind, string Name, string Kind, int? TargetKind, int Slot);

    private List<GraphFieldDefinition> ReadGraphFieldDefinitions()
    {
        var result = new List<GraphFieldDefinition>();
        var slots = new Dictionary<(int, string), int>();
        var distinct = new HashSet<(int, string, string, int?)>();
        var counts = new Dictionary<int, int>();
        using var query = database.Prepare("SELECT id,owner_entry_kind,name,value_kind,target_entry_kind FROM edg_graph_field_definitions ORDER BY id");
        while (query.Read())
        {
            if (query.Integer(0) != result.Count + 1L || query.Integer(1) is < 0 or > int.MaxValue ||
                query.OptionalInteger(4) is < 0 or > int.MaxValue)
                throw new InvalidDataException("Invalid EDG graph field definition ID or kind");
            int owner = (int)query.Integer(1); string name = query.Text(2), kind = query.Text(3);
            int? target = query.IsNull(4) ? null : (int)query.Integer(4);
            if (name.Length == 0 || kind is not ("signed" or "unsigned" or "boolean" or "text" or "bytes" or "reference") ||
                (kind == "reference") != target.HasValue)
                throw new InvalidDataException("Invalid EDG graph field definition discriminator");
            if (!distinct.Add((owner, name, kind, target)))
                throw new InvalidDataException("Duplicate EDG graph field definition");
            if (!slots.TryGetValue((owner, name), out int slot))
            {
                slot = counts.GetValueOrDefault(owner);
                slots.Add((owner, name), slot); counts[owner] = slot + 1;
            }
            result.Add(new(owner, name, kind, target, slot));
        }
        return result;
    }

    // Only format and typed-edge integrity. Shared nodes and cycles need no
    // traversal, ownership inference, or interpretation of C++ language rules.
    private void ReadEdgGraphIntegrity()
    {
        using (var configuration = database.Prepare("SELECT detail FROM edg_graph_coverage WHERE entry_kind=0 AND field='$configuration' AND disposition='configuration'"))
            if (!configuration.Read() || configuration.Text(0).Length == 0 || configuration.Read())
                throw new InvalidDataException("Missing EDG graph configuration and field coverage identity");
        long kindCount;
        using (var kinds = database.Prepare("SELECT count(*),min(entry_kind),max(entry_kind),count(DISTINCT detail) FROM edg_graph_coverage WHERE field='$entryKind' AND disposition='configuration'"))
        {
            if (!kinds.Read() || kinds.Integer(0) < 2 || kinds.Integer(0) > int.MaxValue || kinds.Integer(1) != 0 ||
                kinds.Integer(2) != kinds.Integer(0) - 1 || kinds.Integer(3) != kinds.Integer(0))
                throw new InvalidDataException("Missing or invalid configured EDG entry-kind catalog");
            kindCount = kinds.Integer(0);
        }
        RejectRows("SELECT n.entry_kind FROM edg_graph_node_kinds n LEFT JOIN edg_graph_coverage k ON k.entry_kind=n.entry_kind AND k.field='$entryKind' AND k.disposition='configuration' WHERE n.entry_kind<=0 OR k.entry_kind IS NULL OR k.detail<>n.entry_kind_name", "EDG graph node has an unknown configured entry kind/name");
        var definitions = ReadGraphFieldDefinitions();
        int[] slotsPerKind = new int[(int)kindCount];
        foreach (var definition in definitions)
        {
            if (definition.OwnerKind == 0 || definition.OwnerKind >= kindCount || definition.TargetKind >= kindCount)
                throw new InvalidDataException("EDG graph field definition has an unknown configured entry kind");
            slotsPerKind[definition.OwnerKind] = Math.Max(slotsPerKind[definition.OwnerKind], definition.Slot + 1);
        }
        long nodeCount;
        using (var count = database.Prepare("SELECT count(*),coalesce(max(id),0) FROM edg_graph_node_entries"))
        {
            if (!count.Read() || count.Integer(0) != count.Integer(1) || count.Integer(0) >= Array.MaxLength)
                throw new InvalidDataException("EDG graph node IDs are not dense or exceed supported bounds");
            nodeCount = count.Integer(0);
        }
        int[] nodeKinds = new int[(int)nodeCount + 1], offsets = new int[(int)nodeCount + 1];
        long words = 0;
        using (var nodes = database.Prepare("SELECT id,entry_kind FROM edg_graph_node_entries ORDER BY id"))
            while (nodes.Read())
            {
                int id = (int)nodes.Integer(0), kind = (int)nodes.Integer(1);
                nodeKinds[id] = kind; offsets[id] = (int)words;
                words += (slotsPerKind[kind] + 63L) / 64;
                if (words > Array.MaxLength) throw new InvalidDataException("EDG graph field bitmap exceeds supported bounds");
            }
        ulong[] seen = new ulong[(int)words];
        var repeated = new Dictionary<(int Node, int Slot), long>();
        foreach (var (field, definition) in DecodeGraphFields(definitions, materialize: false))
        {
            if (field.NodeId <= 0 || field.NodeId > nodeCount || nodeKinds[(int)field.NodeId] != definition.OwnerKind)
                throw new InvalidDataException("EDG graph field has a missing or wrongly typed owner");
            if (field.TargetId is long target && (target > nodeCount || nodeKinds[(int)target] != definition.TargetKind))
                throw new InvalidDataException("EDG graph reference has a missing or wrongly typed target");
            int node = (int)field.NodeId, word = offsets[node] + definition.Slot / 64;
            ulong mask = 1UL << (definition.Slot % 64);
            bool present = (seen[word] & mask) != 0;
            if (field.Ordinal == 0)
            {
                if (present) throw new InvalidDataException("Duplicate EDG graph field ordinal");
                seen[word] |= mask;
            }
            else
            {
                var key = (node, definition.Slot);
                if (!present || field.Ordinal != repeated.GetValueOrDefault(key, 1))
                    throw new InvalidDataException("EDG graph field ordinals are not contiguous");
                repeated[key] = field.Ordinal + 1;
            }
        }
    }

    // Public inspection exposes the exact logical typed columns. Storage order
    // is block/row order, and does not imply source-language traversal order.
    public IEnumerable<EdgGraphField> ReadEdgGraphFields()
    {
        foreach (var (field, _) in DecodeGraphFields(ReadGraphFieldDefinitions(), materialize: true)) yield return field;
    }

    private IEnumerable<(EdgGraphField Field, GraphFieldDefinition Definition)> DecodeGraphFields(
        List<GraphFieldDefinition> definitions, bool materialize)
    {
        using var blocks = database.Prepare("SELECT ordinal,row_count,bytes,codec,decoded_byte_length,sha256,encoding FROM edg_graph_field_blocks ORDER BY ordinal");
        long ordinal = 0;
        while (blocks.Read())
        {
            long rows = blocks.Integer(1), length = blocks.Integer(4);
            string encoding = blocks.Text(6);
            if (blocks.Integer(0) != ordinal++ || rows <= 0 || length is <= 0 or > int.MaxValue || rows > length)
                throw new InvalidDataException("Invalid EDG graph field block ordinal or extent");
            using var input = new GraphBlockInput(blocks.Bytes(2), blocks.Text(3), length, blocks.Text(5));
            GraphFieldDefinition Definition(long id) => id > 0 && id <= definitions.Count ? definitions[(int)id - 1]
                : throw new InvalidDataException("EDG graph field has an invalid definition ID");
            if (encoding == "graph-fields-v1")
            {
                if (rows > length / 4) throw new InvalidDataException("Invalid EDG graph flat field block extent");
                for (long row = 0; row < rows; row++)
                {
                    long node = input.NonnegativeInteger();
                    var definition = Definition(input.NonnegativeInteger());
                    long fieldOrdinal = input.NonnegativeInteger();
                    yield return (ReadGraphField(input, node, definition, fieldOrdinal, materialize), definition);
                }
            }
            else if (encoding == "graph-groups-v1")
            {
                long layoutCount = input.NonnegativeInteger(), declaredFields = 0;
                if (layoutCount <= 0 || layoutCount > rows || layoutCount > input.Remaining / 3 || layoutCount > Array.MaxLength)
                    throw new InvalidDataException("Invalid EDG graph group layout count");
                // Grow only after consuming actual metadata. Never allocate from
                // an untrusted count or retain layouts beyond the current block.
                var layouts = new List<(GraphFieldDefinition Definition, long Ordinal)[]>();
                for (long layout = 0; layout < layoutCount; layout++)
                {
                    long fields = input.NonnegativeInteger();
                    if (fields <= 0 || fields > rows - declaredFields || fields > input.Remaining / 2 || fields > Array.MaxLength)
                        throw new InvalidDataException("Invalid EDG graph group layout extent");
                    declaredFields += fields;
                    var entries = new List<(GraphFieldDefinition Definition, long Ordinal)>();
                    for (long field = 0; field < fields; field++)
                        entries.Add((Definition(input.NonnegativeInteger()), input.NonnegativeInteger()));
                    layouts.Add(entries.ToArray());
                }
                long groups = input.NonnegativeInteger(), decodedRows = 0;
                if (groups <= 0 || groups > rows || groups > input.Remaining / 3)
                    throw new InvalidDataException("Invalid EDG graph group count");
                bool[] used = new bool[layouts.Count];
                for (long group = 0; group < groups; group++)
                {
                    long node = input.NonnegativeInteger(), layoutId = input.NonnegativeInteger();
                    if (node <= 0 || layoutId <= 0 || layoutId > layouts.Count)
                        throw new InvalidDataException("EDG graph group has an invalid owner or layout ID");
                    var layout = layouts[(int)layoutId - 1];
                    if (layout.LongLength > rows - decodedRows)
                        throw new InvalidDataException("EDG graph groups exceed declared field row count");
                    used[(int)layoutId - 1] = true;
                    foreach (var entry in layout)
                        yield return (ReadGraphField(input, node, entry.Definition, entry.Ordinal, materialize), entry.Definition);
                    decodedRows += layout.Length;
                }
                if (decodedRows != rows || used.Any(value => !value))
                    throw new InvalidDataException("EDG graph group row count or layout usage mismatch");
            }
            else throw new InvalidDataException("Unsupported EDG graph field block encoding");
            input.Complete();
        }
    }

    private static EdgGraphField ReadGraphField(GraphBlockInput input, long node,
        GraphFieldDefinition definition, long ordinal, bool materialize)
    {
        if (node <= 0) throw new InvalidDataException("EDG graph field has an invalid owner ID");
        string? value = null; long? target = null;
        switch (definition.Kind)
        {
            case "signed":
                ulong encoded = input.VarUInt();
                long signed = (long)(encoded >> 1) ^ -(long)(encoded & 1);
                if (materialize) value = signed.ToString(CultureInfo.InvariantCulture);
                break;
            case "unsigned":
                ulong unsigned = input.VarUInt();
                if (materialize) value = unsigned.ToString(CultureInfo.InvariantCulture);
                break;
            case "boolean":
                int boolean = input.Byte();
                if (boolean > 1) throw new InvalidDataException("EDG graph boolean is not zero or one");
                if (materialize) value = boolean == 0 ? "0" : "1";
                break;
            case "text": case "bytes": value = input.Payload(definition.Kind == "text", materialize); break;
            case "reference": long reference = input.NonnegativeInteger(); target = reference == 0 ? null : reference; break;
        }
        return new(node, definition.Name, ordinal, definition.Kind, value, target, definition.TargetKind);
    }

    private sealed class GraphBlockInput : IDisposable
    {
        private static readonly UTF8Encoding Utf8 = new(false, true);
        private readonly Stream source;
        private readonly SourceSnapshotInput? compressedInput;
        private readonly byte[] encoded;
        private readonly long length;
        private readonly string digest;
        private readonly IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        private readonly byte[] buffer = new byte[65536];
        private int position, available;
        private long loaded, consumed;

        public GraphBlockInput(byte[] encoded, string codec, long length, string digest)
        {
            this.encoded = encoded; this.length = length; this.digest = digest;
            if (digest.Length != 64 || digest.Any(c => c is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
                throw new InvalidDataException("Invalid EDG graph block digest encoding");
            if (codec == "raw")
            {
                if (encoded.LongLength != length) throw new InvalidDataException("Raw EDG graph block length mismatch");
                source = new MemoryStream(encoded, writable: false);
            }
            else if (codec == "zlib")
            {
                if (encoded.Length < 6 || (encoded[0] & 15) != 8 || (encoded[0] >> 4) > 7 ||
                    ((encoded[0] << 8) + encoded[1]) % 31 != 0 || (encoded[1] & 32) != 0)
                    throw new InvalidDataException("Invalid compressed EDG graph block header");
                compressedInput = new SourceSnapshotInput(encoded);
                source = new ZLibStream(compressedInput, CompressionMode.Decompress);
            }
            else throw new InvalidDataException("Unsupported EDG graph block codec");
        }

        private bool Fill()
        {
            if (position < available) return true;
            position = 0;
            available = source.Read(buffer, 0, (int)Math.Min(buffer.Length, length - loaded + 1));
            loaded += available;
            if (loaded > length) throw new InvalidDataException("EDG graph block exceeds decoded length");
            if (available != 0) hash.AppendData(buffer, 0, available);
            return available != 0;
        }
        public long Remaining => length - consumed;

        public int Byte()
        {
            if (!Fill()) throw new InvalidDataException("Truncated EDG graph field payload");
            consumed++; return buffer[position++];
        }
        public ulong VarUInt()
        {
            ulong value = 0;
            for (int index = 0; index < 10; index++)
            {
                int next = Byte();
                if (index == 9 && next > 1) throw new InvalidDataException("EDG graph varuint exceeds 64 bits");
                value |= (ulong)(next & 127) << (index * 7);
                if ((next & 128) == 0)
                {
                    if (index != 0 && next == 0) throw new InvalidDataException("EDG graph varuint is not shortest form");
                    return value;
                }
            }
            throw new InvalidDataException("Invalid EDG graph varuint");
        }
        public long NonnegativeInteger()
        {
            ulong value = VarUInt();
            if (value > long.MaxValue) throw new InvalidDataException("EDG graph ID or ordinal exceeds signed 64-bit bounds");
            return (long)value;
        }
        public string? Payload(bool text, bool materialize)
        {
            ulong extent = VarUInt();
            if (extent > (ulong)(length - consumed)) throw new InvalidDataException("EDG graph field payload exceeds block bounds");
            long remaining = (long)extent;
            if (remaining <= available - position)
            {
                try
                {
                    var payload = buffer.AsSpan(position, (int)remaining);
                    string? result = materialize ? (text ? Utf8.GetString(payload) : Convert.ToHexStringLower(payload)) : null;
                    if (text && !materialize) _ = Utf8.GetCharCount(payload);
                    position += (int)remaining; consumed += remaining;
                    return result;
                }
                catch (DecoderFallbackException error) { throw new InvalidDataException("EDG graph text is not valid UTF-8", error); }
            }
            using var bytes = materialize ? new MemoryStream() : null;
            Decoder? decoder = text && !materialize ? Utf8.GetDecoder() : null;
            try
            {
                while (remaining != 0)
                {
                    if (!Fill()) throw new InvalidDataException("Truncated EDG graph field payload");
                    int take = (int)Math.Min(remaining, available - position);
                    bytes?.Write(buffer, position, take);
                    if (decoder is not null) _ = decoder.GetCharCount(buffer, position, take, remaining == take);
                    position += take; consumed += take; remaining -= take;
                }
                if (!materialize) return null;
                return text ? Utf8.GetString(bytes!.GetBuffer(), 0, (int)bytes.Length)
                    : Convert.ToHexStringLower(bytes!.GetBuffer().AsSpan(0, (int)bytes.Length));
            }
            catch (DecoderFallbackException error) { throw new InvalidDataException("EDG graph text is not valid UTF-8", error); }
        }
        public void Complete()
        {
            if (Fill() || consumed != length || compressedInput is not null && compressedInput.Position != encoded.LongLength)
                throw new InvalidDataException("EDG graph block row count, decoded length, or frame termination mismatch");
            if (Convert.ToHexStringLower(hash.GetHashAndReset()) != digest)
                throw new InvalidDataException("EDG graph block digest mismatch");
        }
        public void Dispose() { source.Dispose(); hash.Dispose(); }
    }
}
