using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Edg.Semantic;

/// <summary>Indexed, readonly graph storage. Decoded nodes own their JSON independently of the bounded cache.</summary>
internal sealed class SqliteSemanticStore : IDisposable
{
    private readonly SqliteDatabase db;
    private readonly SemanticGraph graph;
    private readonly long unit;
    private readonly Dictionary<long, Kind> kinds = new();
    private readonly Dictionary<string, Kind> byName = new(StringComparer.Ordinal);
    private readonly Cache<ulong, SemanticNode> cache = new(4096, 32 * 1024 * 1024);
    private readonly Cache<long, byte[]> literals = new(4096, 8 * 1024 * 1024);
    private readonly Cache<ulong, string> kindCache = new(16384, 1024 * 1024);
    private readonly Cache<ulong, bool> macroCache = new(16384, 128 * 1024);
    private readonly SqliteStatement lookup, literalLookup, edgeLookup, macroLookup;
    private readonly JsonElement manifest;
    public JsonElement Metadata { get; }
    public IReadOnlyDictionary<ulong, SemanticNode> Nodes { get; }
    public int Count { get; }
    public int DiagnosticCount { get; }
    public int MacroCount { get; }
    private sealed record Field(string Name, string ValueKind);
    private sealed record Kind(long Id, string Name, string Table, List<Field> Fields) { public SqliteStatement? Lookup; }

    public SqliteSemanticStore(SemanticGraph graph, string path)
    {
        this.graph = graph;
        db = new SqliteDatabase(path);
        try
        {
            if (db.Scalar("PRAGMA application_id") != 1162102611 || db.Scalar("PRAGMA user_version") != 1)
                throw new InvalidDataException("Unsupported semantic SQLite format");
            if (db.Scalar("SELECT count(*) FROM units") != 1) throw new InvalidDataException("Exactly one translation unit is required");
            unit = db.Scalar("SELECT unit_id FROM units");
            if (unit <= 0) throw new InvalidDataException("Invalid translation unit identity");
            manifest = ReadInfo("manifest");
            if (manifest.GetProperty("schema").GetString() != "edg-semantic-package/2" || manifest.GetProperty("databaseFormat").GetString() != "edg-semantic-sqlite/1" || manifest.GetProperty("unitId").GetInt64() != unit)
                throw new InvalidDataException("Unsupported semantic SQLite package manifest");
            var actual = db.ContentHash(out var sourceHashes);
            if (!string.Equals(actual, manifest.GetProperty("contentSha256").GetString(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Semantic content SHA256 mismatch");
            using (var producer = db.Prepare("SELECT value_json FROM package_info WHERE key='producer'"))
            {
                if (!producer.Step()) throw new InvalidDataException("Missing embedded producer manifest");
                using var parsed = JsonDocument.Parse(producer.Bytes(0).ToArray());
                var hash = Convert.ToHexStringLower(SHA256.HashData(producer.Bytes(0)));
                if (!string.Equals(hash, manifest.GetProperty("producer").GetProperty("sha256").GetString(), StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Producer SHA256 mismatch");
                var producerRoot = parsed.RootElement;
                if (producerRoot.GetProperty("schema").GetString() != "edg-semantic-producer/1") throw new InvalidDataException("Unsupported semantic producer schema");
                var binaryHash = producerRoot.GetProperty("binarySha256").GetString();
                if (binaryHash == null || binaryHash.Length != 64 || binaryHash.Any(c => !char.IsAsciiHexDigit(c)) || !string.Equals(binaryHash, manifest.GetProperty("invocation").GetProperty("sha256").GetString(), StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Producer/invocation binary SHA256 mismatch");
            }
            Metadata = MakeJson(writer => {
                writer.WriteStartObject();
                using var metadata = db.Prepare("SELECT key,value_json FROM graph_metadata WHERE unit_id=" + unit + " ORDER BY key");
                while (metadata.Step()) { writer.WritePropertyName(metadata.Text(0)); writer.WriteRawValue(metadata.Bytes(1)); }
                writer.WriteEndObject();
            });
            ValidateMetadata();
            using (var rows = db.Prepare("SELECT kind_id,name,table_name FROM node_kinds ORDER BY kind_id"))
                while (rows.Step())
                {
                    var kind = new Kind(rows.Integer(0), rows.Text(1), rows.Text(2), new());
                    if (kind.Id <= 0 || string.IsNullOrWhiteSpace(kind.Name) || !kind.Table.StartsWith("node_", StringComparison.Ordinal)) throw new InvalidDataException("Invalid node kind catalog");
                    kinds.Add(kind.Id, kind); byName.Add(kind.Name, kind);
                }
            using (var fields = db.Prepare("SELECT kind_id,ordinal,name,value_kind FROM node_fields ORDER BY kind_id,ordinal"))
                while (fields.Step())
                {
                    if (!kinds.TryGetValue(fields.Integer(0), out var kind) || fields.Integer(1) != kind.Fields.Count) throw new InvalidDataException("Invalid node field catalog order");
                    var field = new Field(fields.Text(2), fields.Text(3));
                    if (field.Name is "id" or "kind" or "form" or "nativeEdges" or "unit_id" or "node_id" || field.ValueKind is not ("integer" or "boolean" or "text" or "json" or "null")) throw new InvalidDataException("Invalid node field catalog");
                    if (string.IsNullOrEmpty(field.Name) || kind.Fields.Any(f => f.Name == field.Name)) throw new InvalidDataException("Duplicate or empty node field");
                    kind.Fields.Add(field);
                }
            ValidateTables();
            Count = checked((int)db.Scalar("SELECT count(*) FROM nodes WHERE unit_id=" + unit));
            DiagnosticCount = checked((int)db.Scalar("SELECT count(*) FROM diagnostics WHERE unit_id=" + unit));
            MacroCount = checked((int)db.Scalar("SELECT count(*) FROM macros WHERE unit_id=" + unit));
            lookup = db.Prepare("SELECT kind_id,form_literal_id,edge_count FROM nodes WHERE unit_id=" + unit + " AND node_id=?1");
            literalLookup = db.Prepare("SELECT payload FROM literals WHERE literal_id=?1");
            edgeLookup = db.Prepare("SELECT target_id,target_kind_id FROM native_edges WHERE unit_id=" + unit + " AND source_id=?1 ORDER BY ordinal");
            macroLookup = db.Prepare("SELECT 1 FROM macros WHERE unit_id=" + unit + " AND invocation_id=?1");
            Nodes = new NodeDictionary(this);
            ValidateSources(sourceHashes);
        }
        catch { Dispose(); throw; }
    }

    private void ValidateMetadata()
    {
        foreach (var name in new[] { "schema", "producerVersion", "sourceLanguage" })
            if (!Metadata.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String) throw new InvalidDataException("Missing or invalid graph metadata: " + name);
        foreach (var name in new[] { "lowered", "completeForEmission", "littleEndian", "plainCharSigned" })
            if (!Metadata.TryGetProperty(name, out var value) || value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new InvalidDataException("Missing or invalid graph metadata: " + name);
        foreach (var name in new[] { "standardVersion", "rootScope", "primarySource", "pointerBytes", "intBytes", "longBytes", "longDoubleBytes" })
            if (!Metadata.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number || !value.TryGetUInt64(out var number) || (number == 0 && name != "standardVersion")) throw new InvalidDataException("Missing or invalid graph metadata: " + name);
        if (!Metadata.TryGetProperty("configuration", out var configuration) || configuration.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Missing or invalid graph metadata: configuration");
    }

    private JsonElement ReadInfo(string key)
    {
        using var row = db.Prepare("SELECT value_json FROM package_info WHERE key=?1"); row.Bind(1, key);
        if (!row.Step()) throw new InvalidDataException("Incomplete SQLite package: missing " + key);
        using var parsed = JsonDocument.Parse(row.Bytes(0).ToArray()); return parsed.RootElement.Clone();
    }
    private void Reject(string query, string reason)
    { using var check = db.Prepare("SELECT 1 FROM (" + query + ") LIMIT 1"); if (check.Step()) throw new InvalidDataException(reason); }
    private static string Q(string value) => SqliteDatabase.Quote(value);
    private void ValidateTables()
    {
        var schemas = new Dictionary<string, string[]>(StringComparer.Ordinal) {
            ["units"] = ["unit_id"], ["graph_metadata"] = ["unit_id", "key", "value_json"],
            ["node_kinds"] = ["kind_id", "name", "table_name"], ["node_fields"] = ["kind_id", "ordinal", "name", "value_kind"],
            ["nodes"] = ["unit_id", "node_id", "kind_id", "form_literal_id", "edge_count"],
            ["native_edges"] = ["unit_id", "source_id", "ordinal", "target_id", "target_kind_id"],
            ["macros"] = ["unit_id", "invocation_id", "parent_index", "definition_id", "start_literal_id", "end_literal_id", "arguments_literal_id"],
            ["diagnostics"] = ["unit_id", "ordinal", "node_id", "code_literal_id", "message_literal_id"],
            ["source_blobs"] = ["sha256", "bytes"], ["source_files"] = ["unit_id", "node_id", "original_path", "content_sha256", "available", "byte_length"],
            ["package_info"] = ["key", "value_json"]
        };
        foreach (var schema in schemas)
        {
            ValidateColumns(schema.Key, schema.Value);
            string[] primary = schema.Key switch {
                "units" => ["unit_id"], "graph_metadata" => ["unit_id", "key"], "node_kinds" => ["kind_id"], "node_fields" => ["kind_id", "name"],
                "nodes" => ["unit_id", "node_id"], "native_edges" => ["unit_id", "source_id", "ordinal"], "macros" => ["unit_id", "invocation_id"],
                "diagnostics" => ["unit_id", "ordinal"], "source_blobs" => ["sha256"], "source_files" => ["unit_id", "node_id"], "package_info" => ["key"], _ => []
            };
            ValidatePrimaryKey(schema.Key, primary);
        }
        ValidatePrimaryKey("literals", ["literal_id"]);
        var literalColumns = Columns("literals");
        if (!literalColumns.SequenceEqual(new[] { "literal_id", "encoding", "payload" }) && !literalColumns.SequenceEqual(new[] { "literal_id", "encoding", "payload_hash", "payload" })) throw new InvalidDataException("Invalid literal table schema");
        if (literalColumns.Contains("payload_hash")) Reject("SELECT 1 FROM literals WHERE typeof(payload_hash)<>'integer'", "Invalid literal hash hint");
        using (var tables = db.Prepare("SELECT name,type FROM sqlite_master WHERE type IN ('table','view','trigger') AND substr(name,1,7)<>'sqlite_'"))
            while (tables.Step()) if (tables.Text(1) != "table" || (!schemas.ContainsKey(tables.Text(0)) && tables.Text(0) != "literals" && !kinds.Values.Any(k => k.Table == tables.Text(0)))) throw new InvalidDataException("Unknown semantic storage object " + tables.Text(0));

        foreach (var table in new[] { "graph_metadata", "nodes", "native_edges", "macros", "diagnostics", "source_files" })
            Reject("SELECT 1 FROM " + Q(table) + " WHERE typeof(unit_id)<>'integer' OR unit_id<>" + unit, "Unknown translation unit in " + table);
        Reject("SELECT 1 FROM nodes n LEFT JOIN node_kinds k ON k.kind_id=n.kind_id WHERE typeof(n.node_id)<>'integer' OR n.node_id<=0 OR k.kind_id IS NULL OR (n.edge_count IS NOT NULL AND (typeof(n.edge_count)<>'integer' OR n.edge_count<0))", "Invalid node registry");
        Reject("SELECT 1 FROM nodes n LEFT JOIN literals l ON l.literal_id=n.form_literal_id WHERE n.form_literal_id IS NOT NULL AND (typeof(n.form_literal_id)<>'integer' OR l.literal_id IS NULL OR l.encoding<>'text')", "Invalid node form literal");
        foreach (var kind in kinds.Values)
        {
            ValidateColumns(kind.Table, new[] { "unit_id", "node_id" }.Concat(kind.Fields.Select(f => f.Name)).ToArray());
            using (var info = db.Prepare("PRAGMA table_info(" + Q(kind.Table) + ")"))
                while (info.Step()) if (info.Integer(5) != (info.Integer(0) < 2 ? info.Integer(0) + 1 : 0)) throw new InvalidDataException("Invalid kind table primary key: " + kind.Name);
            var table = Q(kind.Table);
            Reject("SELECT 1 FROM " + table + " t LEFT JOIN nodes n ON n.unit_id=t.unit_id AND n.node_id=t.node_id WHERE n.node_id IS NULL OR n.kind_id<>" + kind.Id, "Orphan or incorrect kind row: " + kind.Name);
            Reject("SELECT 1 FROM nodes n LEFT JOIN " + table + " t ON t.unit_id=n.unit_id AND t.node_id=n.node_id WHERE n.kind_id=" + kind.Id + " AND t.node_id IS NULL", "Missing kind row: " + kind.Name);
            foreach (var field in kind.Fields)
            {
                var column = "t." + Q(field.Name);
                var bad = field.ValueKind switch {
                    "integer" => "typeof(" + column + ") NOT IN ('integer','blob')",
                    "boolean" => "(typeof(" + column + ")<>'integer' OR " + column + " NOT IN (0,1))",
                    "null" => "(typeof(" + column + ")<>'integer' OR " + column + "<>0)",
                    _ => "typeof(" + column + ")<>'integer'"
                };
                Reject("SELECT 1 FROM " + table + " t WHERE " + column + " IS NOT NULL AND " + bad, "Invalid field storage: " + kind.Name + "." + field.Name);
                if (SemanticGraph.ReferenceFields.Contains(field.Name) && !(kind.Name == "variable" && field.Name == "parameter"))
                {
                    if (field.ValueKind != "integer") throw new InvalidDataException("Noninteger reference field " + field.Name);
                    Reject("SELECT 1 FROM " + table + " t LEFT JOIN nodes n ON n.unit_id=t.unit_id AND n.node_id=" + column + " WHERE " + column + " IS NOT NULL AND (typeof(" + column + ")<>'integer' OR " + column + "<0 OR (" + column + "<>0 AND n.node_id IS NULL))", "Unknown node reference: " + kind.Name + "." + field.Name);
                }
                if (field.ValueKind is "text" or "json")
                    Reject("SELECT 1 FROM " + table + " t LEFT JOIN literals l ON l.literal_id=" + column + " WHERE " + column + " IS NOT NULL AND (l.literal_id IS NULL OR l.encoding<>'" + field.ValueKind + "')", "Invalid field literal: " + kind.Name + "." + field.Name);
                if (field.ValueKind == "integer")
                {
                    using var large = db.Prepare("SELECT " + column + " FROM " + table + " t WHERE typeof(" + column + ")='blob'");
                    while (large.Step())
                    {
                        var bytes = large.Bytes(0); var start = bytes.Length > 0 && bytes[0] == '-' ? 1 : 0;
                        if (bytes.Length == start || (bytes.Length > start + 1 && bytes[start] == '0')) throw new InvalidDataException("Invalid large integer");
                        foreach (var b in bytes[start..]) if (b < '0' || b > '9') throw new InvalidDataException("Invalid large integer");
                    }
                }
            }
            kind.Lookup = db.Prepare("SELECT " + (kind.Fields.Count == 0 ? "node_id" : string.Join(",", kind.Fields.Select(f => Q(f.Name)))) + " FROM " + table + " WHERE unit_id=" + unit + " AND node_id=?1");
        }
        Reject("SELECT 1 FROM native_edges e LEFT JOIN nodes s ON s.unit_id=e.unit_id AND s.node_id=e.source_id LEFT JOIN nodes t ON t.unit_id=e.unit_id AND t.node_id=e.target_id WHERE s.node_id IS NULL OR t.node_id IS NULL OR t.kind_id<>e.target_kind_id OR typeof(e.ordinal)<>'integer' OR e.ordinal<0", "Invalid native edge closure");
        Reject("SELECT 1 FROM nodes n LEFT JOIN (SELECT unit_id,source_id,count(*) c,min(ordinal) lo,max(ordinal) hi FROM native_edges GROUP BY unit_id,source_id) e ON e.unit_id=n.unit_id AND e.source_id=n.node_id WHERE (n.edge_count IS NULL AND e.c IS NOT NULL) OR (n.edge_count IS NOT NULL AND (n.edge_count<>coalesce(e.c,0) OR (n.edge_count>0 AND (e.lo<>0 OR e.hi<>n.edge_count-1))))", "Invalid native edge count/order");
        Reject("SELECT 1 FROM macros m LEFT JOIN nodes n ON n.unit_id=m.unit_id AND n.node_id=m.definition_id LEFT JOIN node_kinds k ON k.kind_id=n.kind_id LEFT JOIN macros p ON p.unit_id=m.unit_id AND p.invocation_id=m.parent_index WHERE m.invocation_id<=0 OR typeof(m.invocation_id)<>'integer' OR typeof(m.parent_index)<>'integer' OR typeof(m.definition_id)<>'integer' OR (m.parent_index<0)<>(m.definition_id=0) OR (m.definition_id<>0 AND (k.name IS NULL OR k.name<>'macro')) OR (m.parent_index>0 AND (p.invocation_id IS NULL OR p.invocation_id>=m.invocation_id OR p.definition_id=0))", "Invalid macro invocation identity or parent");
        foreach (var (column, encoding) in new[] { ("start_literal_id", "json"), ("end_literal_id", "json"), ("arguments_literal_id", "text") })
            Reject("SELECT 1 FROM macros m LEFT JOIN literals l ON l.literal_id=m." + column + " WHERE l.literal_id IS NULL OR l.encoding<>'" + encoding + "'", "Invalid macro literal");
        Reject("SELECT 1 FROM diagnostics d LEFT JOIN nodes n ON n.unit_id=d.unit_id AND n.node_id=d.node_id WHERE typeof(d.node_id)<>'integer' OR d.node_id<0 OR (d.node_id<>0 AND n.node_id IS NULL)", "Invalid diagnostic node");
        foreach (var column in new[] { "code_literal_id", "message_literal_id" })
            Reject("SELECT 1 FROM diagnostics d LEFT JOIN literals l ON l.literal_id=d." + column + " WHERE l.literal_id IS NULL OR l.encoding<>'text'", "Invalid diagnostic literal");
    }

    private void ValidatePrimaryKey(string table, string[] expected)
    {
        var keys = new SortedDictionary<long, string>();
        using var info = db.Prepare("PRAGMA table_info(" + Q(table) + ")");
        while (info.Step()) if (info.Integer(5) != 0) keys.Add(info.Integer(5), info.Text(1));
        if (!keys.Values.SequenceEqual(expected)) throw new InvalidDataException("Invalid semantic primary key: " + table);
    }
    private List<string> Columns(string table)
    { var columns = new List<string>(); using var info = db.Prepare("PRAGMA table_info(" + Q(table) + ")"); while (info.Step()) columns.Add(info.Text(1)); return columns; }
    private void ValidateColumns(string table, string[] expected)
    { if (!Columns(table).SequenceEqual(expected)) throw new InvalidDataException("Table does not match semantic storage schema: " + table); }

    public void ValidateLiterals(Action<JsonElement> validatePositions)
    {
        var utf8 = new UTF8Encoding(false, true);
        using var rows = db.Prepare("SELECT literal_id,encoding,payload FROM literals ORDER BY literal_id");
        while (rows.Step())
        {
            if (rows.Type(0) != 1 || rows.Integer(0) <= 0 || rows.Type(2) != 4) throw new InvalidDataException("Invalid literal pool row");
            if (rows.Text(1) == "text") { _ = utf8.GetCharCount(rows.Bytes(2)); continue; }
            if (rows.Text(1) != "json") throw new InvalidDataException("Invalid literal encoding");
            using var value = JsonDocument.Parse(rows.Bytes(2).ToArray());
            if (value.RootElement.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array)) throw new InvalidDataException("Nested JSON literal must be an object or array");
            validatePositions(value.RootElement);
        }
    }
    private void ValidateSources(Dictionary<string, (string Hash, long Length)> hashes)
    {
        foreach (var item in hashes) if (!string.Equals(item.Key, item.Value.Hash, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Source snapshot SHA256 mismatch");
        var seen = new HashSet<ulong>();
        using var row = db.Prepare("SELECT original_path,content_sha256,available,byte_length FROM source_files WHERE unit_id=" + unit + " AND node_id=?1");
        foreach (var source in manifest.GetProperty("sources").EnumerateArray())
        {
            var id = source.GetProperty("node").GetUInt64();
            if (!seen.Add(id) || KindOf(id) != "source-file" || source.GetProperty("encoding").GetString() != "bytes") throw new InvalidDataException("Invalid source snapshot identity");
            var sourceNode = Get(id);
            var fullPath = sourceNode.Text("fullPath");
            if ((string.IsNullOrEmpty(fullPath) ? sourceNode.Text("path") : fullPath) != source.GetProperty("originalPath").GetString()) throw new InvalidDataException("Source spelling does not match semantic node");
            row.Reset(); row.Bind(1, checked((long)id));
            if (!row.Step() || row.Text(0) != source.GetProperty("originalPath").GetString() || row.Type(2) != 1 || row.Integer(2) is not (0 or 1)) throw new InvalidDataException("Source snapshot mapping mismatch");
            var available = source.GetProperty("available").GetBoolean();
            if (available != (row.Integer(2) == 1)) throw new InvalidDataException("Source availability mismatch");
            if (available)
            {
                var sha = source.GetProperty("sha256").GetString()!; var length = source.GetProperty("byteLength").GetInt64();
                if (row.Type(3) != 1 || row.Text(1) != sha || row.Integer(3) != length || !hashes.TryGetValue(sha, out var blob) || blob.Length != length) throw new InvalidDataException("Source snapshot hash/length mapping mismatch");
            }
            else if (row.Type(1) != 5 || row.Type(3) != 5) throw new InvalidDataException("Unavailable source contains snapshot mapping");
        }
        if (seen.Count != db.Scalar("SELECT count(*) FROM source_files") || seen.Count != KindCounts().Where(k => k.Key == "source-file").Sum(k => k.Value)) throw new InvalidDataException("Package source coverage mismatch");
    }
    public void RequireEmissionSources()
    {
        if (manifest.GetProperty("diagnostics").GetArrayLength() != 0 || manifest.GetProperty("sources").GetArrayLength() == 0 || manifest.GetProperty("sources").EnumerateArray().Any(s => s.GetProperty("available").ValueKind != JsonValueKind.True)) throw new InvalidDataException("C emission requires available source snapshots and empty package diagnostics");
    }
    public string? KindOf(ulong id)
    {
        if (id == 0) return null;
        if (kindCache.TryGet(id, out var name)) return name;
        lookup.Reset(); lookup.Bind(1, checked((long)id));
        if (!lookup.Step()) throw new InvalidDataException("Unknown node ID " + id);
        name = kinds[lookup.Integer(0)].Name; kindCache.Add(id, name, name.Length * 2); return name;
    }
    public bool HasMacro(ulong id)
    { if (id > long.MaxValue) return false; if (macroCache.TryGet(id, out var found)) return found; macroLookup.Reset(); macroLookup.Bind(1, (long)id); found = macroLookup.Step(); macroCache.Add(id, found, 1); return found; }
    private byte[] Literal(long id)
    {
        if (literals.TryGet(id, out var value)) return value;
        literalLookup.Reset(); literalLookup.Bind(1, id);
        if (!literalLookup.Step()) throw new InvalidDataException("Unknown literal " + id);
        value = literalLookup.Bytes(0).ToArray(); literals.Add(id, value, value.Length); return value;
    }
    public SemanticNode Get(ulong id)
    {
        if (cache.TryGet(id, out var node)) return node;
        lookup.Reset(); lookup.Bind(1, checked((long)id));
        if (!lookup.Step()) throw new InvalidDataException("Unknown node ID " + id);
        var kind = kinds[lookup.Integer(0)]; var form = lookup.Type(1) == 5 ? 0 : lookup.Integer(1); var edges = lookup.Type(2) == 5 ? -1 : lookup.Integer(2);
        int size = 0;
        var raw = MakeJson(writer => {
            writer.WriteStartObject(); writer.WriteNumber("id", id); writer.WriteString("kind", kind.Name);
            if (form != 0) writer.WriteString("form", Encoding.UTF8.GetString(Literal(form)));
            var row = kind.Lookup!; row.Reset(); row.Bind(1, checked((long)id)); if (!row.Step()) throw new InvalidDataException("Missing node kind row");
            for (var i = 0; i < kind.Fields.Count; ++i)
            {
                if (row.Type(i) == 5) continue;
                var field = kind.Fields[i]; writer.WritePropertyName(field.Name);
                switch (field.ValueKind)
                {
                    case "integer": if (row.Type(i) == 4) writer.WriteRawValue(row.Bytes(i)); else writer.WriteNumberValue(row.Integer(i)); break;
                    case "boolean": writer.WriteBooleanValue(row.Integer(i) != 0); break;
                    case "null": writer.WriteNullValue(); break;
                    case "text": writer.WriteStringValue(Encoding.UTF8.GetString(Literal(row.Integer(i)))); break;
                    case "json": writer.WriteRawValue(Literal(row.Integer(i))); break;
                }
            }
            if (edges >= 0)
            {
                writer.WriteStartArray("nativeEdges"); edgeLookup.Reset(); edgeLookup.Bind(1, checked((long)id));
                while (edgeLookup.Step()) { writer.WriteStartObject(); writer.WriteNumber("target", edgeLookup.Integer(0)); writer.WriteString("kind", kinds[edgeLookup.Integer(1)].Name); writer.WriteEndObject(); }
                writer.WriteEndArray();
            }
            writer.WriteEndObject(); writer.Flush(); size = checked((int)writer.BytesCommitted);
        });
        node = new SemanticNode(graph, raw); cache.Add(id, node, size * 2L); return node;
    }
    public IEnumerable<ulong> Ids(string? kind = null)
    {
        if (kind != null && !byName.ContainsKey(kind)) yield break;
        using var rows = db.Prepare("SELECT node_id FROM nodes WHERE unit_id=" + unit + (kind == null ? "" : " AND kind_id=" + byName[kind].Id) + " ORDER BY node_id");
        while (rows.Step()) yield return checked((ulong)rows.Integer(0));
    }
    public IEnumerable<SemanticNode> OfKind(string kind) => Ids(kind).Select(Get);
    public IEnumerable<KeyValuePair<string, int>> KindCounts()
    {
        using var rows = db.Prepare("SELECT k.name,count(*) FROM nodes n JOIN node_kinds k ON k.kind_id=n.kind_id WHERE n.unit_id=" + unit + " GROUP BY n.kind_id ORDER BY k.name");
        while (rows.Step()) yield return new(rows.Text(0), checked((int)rows.Integer(1)));
    }
    public IEnumerable<JsonElement> Diagnostics()
    {
        using var rows = db.Prepare("SELECT node_id,code_literal_id,message_literal_id FROM diagnostics WHERE unit_id=" + unit + " ORDER BY ordinal");
        while (rows.Step()) yield return MakeJson(w => { w.WriteStartObject(); w.WriteNumber("node", rows.Integer(0)); w.WriteString("code", Encoding.UTF8.GetString(Literal(rows.Integer(1)))); w.WriteString("message", Encoding.UTF8.GetString(Literal(rows.Integer(2)))); w.WriteEndObject(); });
    }
    public IEnumerable<JsonElement> Macros()
    {
        using var rows = db.Prepare("SELECT invocation_id,parent_index,definition_id,start_literal_id,end_literal_id,arguments_literal_id FROM macros WHERE unit_id=" + unit + " ORDER BY invocation_id");
        while (rows.Step()) yield return MakeJson(w => { w.WriteStartObject(); w.WriteNumber("index", rows.Integer(0)); w.WriteNumber("parentIndex", rows.Integer(1)); w.WriteNumber("definition", rows.Integer(2)); w.WritePropertyName("start"); w.WriteRawValue(Literal(rows.Integer(3))); w.WritePropertyName("end"); w.WriteRawValue(Literal(rows.Integer(4))); w.WriteString("arguments", Encoding.UTF8.GetString(Literal(rows.Integer(5)))); w.WriteEndObject(); });
    }
    private static JsonElement MakeJson(Action<Utf8JsonWriter> write)
    { using var bytes = new MemoryStream(); using (var writer = new Utf8JsonWriter(bytes)) { write(writer); } using var doc = JsonDocument.Parse(bytes.ToArray()); return doc.RootElement.Clone(); }
    public void Dispose()
    { foreach (var k in kinds.Values) k.Lookup?.Dispose(); lookup?.Dispose(); literalLookup?.Dispose(); edgeLookup?.Dispose(); macroLookup?.Dispose(); db.Dispose(); }

    private sealed class NodeDictionary(SqliteSemanticStore store) : IReadOnlyDictionary<ulong, SemanticNode>
    {
        public int Count => store.Count;
        public IEnumerable<ulong> Keys => store.Ids();
        public IEnumerable<SemanticNode> Values => Keys.Select(store.Get);
        public SemanticNode this[ulong key] => store.Get(key);
        public bool ContainsKey(ulong key) { if (key == 0 || key > long.MaxValue) return false; using var q = store.db.Prepare("SELECT 1 FROM nodes WHERE unit_id=" + store.unit + " AND node_id=?1"); q.Bind(1, (long)key); return q.Step(); }
        public bool TryGetValue(ulong key, out SemanticNode value) { if (ContainsKey(key)) { value = store.Get(key); return true; } value = null!; return false; }
        public IEnumerator<KeyValuePair<ulong, SemanticNode>> GetEnumerator() => Keys.Select(id => new KeyValuePair<ulong, SemanticNode>(id, store.Get(id))).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    private sealed class Cache<TKey, TValue>(int capacity, long byteLimit) where TKey : notnull
    {
        private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value, long Bytes)>> map = new();
        private readonly LinkedList<(TKey Key, TValue Value, long Bytes)> lru = new();
        private long bytes;
        public bool TryGet(TKey key, out TValue value) { if (!map.TryGetValue(key, out var item)) { value = default!; return false; } lru.Remove(item); lru.AddFirst(item); value = item.Value.Value; return true; }
        public void Add(TKey key, TValue value, long size) { if (size > byteLimit) return; while (map.Count >= capacity || bytes + size > byteLimit) { var last = lru.Last!; bytes -= last.Value.Bytes; map.Remove(last.Value.Key); lru.RemoveLast(); } map.Add(key, lru.AddFirst((key, value, size))); bytes += size; }
    }
}
