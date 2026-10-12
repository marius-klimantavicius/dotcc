using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RoslynObj;

internal sealed class Row(IReadOnlyDictionary<string, object?> values)
{
    internal object? this[string key] => values[key];
    internal long Long(string key) => Convert.ToInt64(values[key], CultureInfo.InvariantCulture);
    internal long? OptionalLong(string key) => values[key] is null ? null : Long(key);
    internal int Int(string key) => checked((int)Long(key));
    internal string Text(string key) => (string)values[key]!;
    internal string? OptionalText(string key) => (string?)values[key];
    internal bool Bool(string key) => Long(key) != 0;
    internal bool? OptionalBool(string key) => values[key] is null ? null : Bool(key);
}

internal sealed class SnapshotStore : IDisposable
{
    internal const string ChildrenQuery = "SELECT n.* FROM structural_child c JOIN semantic_node n ON n.id=c.child_id WHERE c.owner_id=$owner ORDER BY c.role_id,c.ordinal";
    private readonly object sync = new();
    private readonly SqliteConnection connection;
    private readonly SqliteTransaction transaction;
    private readonly ReadOptions options;
    private readonly BoundedCache<(string Table, long Id), Row?> rows;
    private readonly BoundedCache<(string Query, long Owner, int Page), Row[]> pages;
    private bool disposed;
    private long commands, rowsRead;

    internal SnapshotStore(string path, ReadOptions options)
    {
        this.options = options;
        rows = new(options.CachedRowCount);
        pages = new(options.CachedPageCount);
        connection = new(new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(path), Mode = SqliteOpenMode.ReadOnly,
            Cache = SqliteCacheMode.Private, Pooling = false
        }.ToString());
        try
        {
            connection.Open();
            transaction = connection.BeginTransaction(deferred: true);
            Execute("PRAGMA query_only=ON");
        }
        catch { connection.Dispose(); throw; }
    }

    internal int PageSize => options.PageSize;

    internal void EnsureActive()
    {
        lock (sync) ObjectDisposedException.ThrowIf(disposed, nameof(RoslynObject));
    }

    internal Row? Get(string table, long id)
    {
        lock (sync)
        {
            EnsureActive();
            if (rows.TryGet((table, id), out var value)) return value;
            var result = Query($"SELECT * FROM {table} WHERE id=$owner", id);
            if (result.Length == 0) { rows.Set((table, id), null); return null; }
            rows.Set((table, id), result[0]);
            return result[0];
        }
    }

    internal Row Required(string table, long id) => Get(table, id)
        ?? throw new IncompatibleObjectException($"Required {table} row is missing.");

    internal Row[] Query(string sql, long owner = 0, CancellationToken token = default)
        => QueryParameters(sql, owner, token, []);

    private Row[] QueryParameters(string sql, long owner, CancellationToken token, IReadOnlyList<object?> parameters)
    {
        lock (sync)
        {
            EnsureActive();
            using var command = Create(sql, owner);
            for (var i = 0; i < parameters.Count; i++)
                command.Parameters.AddWithValue($"$key{i}", parameters[i] ?? DBNull.Value);
            using var registration = token.Register(command.Cancel);
            token.ThrowIfCancellationRequested();
            using var reader = command.ExecuteReader();
            var result = new List<Row>();
            while (reader.Read())
            {
                token.ThrowIfCancellationRequested();
                var fields = new Dictionary<string, object?>(reader.FieldCount, StringComparer.Ordinal);
                for (var i = 0; i < reader.FieldCount; i++)
                    fields.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
                result.Add(new(fields));
            }
            rowsRead += result.Count;
            return result.ToArray();
        }
    }

    internal Row[] Page(string sql, long owner, int page, string? entityTable, CancellationToken token = default)
        => LoadPage(sql, owner, page, null, entityTable, token, useOffset: true);

    internal Row[] NextPage(string sql, long owner, int page, Row? previous, string? entityTable, CancellationToken token = default)
        => LoadPage(sql, owner, page, previous, entityTable, token, useOffset: false);

    private Row[] LoadPage(string sql, long owner, int page, Row? previous, string? entityTable, CancellationToken token, bool useOffset)
    {
        lock (sync)
        {
            EnsureActive();
            token.ThrowIfCancellationRequested();
            if (pages.TryGet((sql, owner, page), out var cached))
            {
                if (sql == ChildrenQuery) PrimeChildPages(cached, token);
                return cached;
            }
            var (ordered, keys) = PagingQuery(sql);
            var predicate = previous is null ? "" : $" WHERE ({string.Join(',', keys)}) > ({string.Join(',', keys.Select((_, i) => $"$key{i}"))})";
            var offset = useOffset ? $" OFFSET {checked(page * options.PageSize)}" : "";
            var command = $"SELECT * FROM ({ordered}){predicate} ORDER BY {string.Join(',', keys)} LIMIT {options.PageSize}{offset}";
            var result = QueryParameters(command, owner, token, previous is null ? [] : keys.Select(k => previous[k]).ToArray());
            if (entityTable is not null)
            {
                foreach (var row in result) rows.Set((entityTable, row.Long("id")), row);
                PrimeCompanions(entityTable, result, token);
            }
            pages.Set((sql, owner, page), result);
            if (sql == ChildrenQuery) PrimeChildPages(result, token);
            return result;
        }
    }

    private void PrimeChildPages(Row[] nodes, CancellationToken token, int depth = 1)
    {
        // Prefetch one bounded page per immediate child, including known-empty leaves.
        // This prevents a block with many leaf operations from issuing one read per edge.
        var pending = nodes.Select(r => r.Long("id")).Distinct()
            .Where(id => !pages.TryGet((ChildrenQuery, id, 0), out _))
            .Take(Math.Min(options.PageSize, Math.Max(1, Math.Min(options.CachedPageCount / 2 + 1,
                options.CachedRowCount / options.PageSize)))).ToArray();
        if (pending.Length == 0) return;
        var parameters = pending.Select(id => (object?)id).ToArray();
        var placeholders = string.Join(',', pending.Select((_, i) => $"$key{i}"));
        var result = QueryParameters($"WITH ranked AS (SELECT n.*,c.owner_id AS __owner,c.role_id AS __key0,c.ordinal AS __key1,ROW_NUMBER() OVER (PARTITION BY c.owner_id ORDER BY c.role_id,c.ordinal) AS __rank FROM structural_child c JOIN semantic_node n ON n.id=c.child_id WHERE c.owner_id IN ({placeholders})) SELECT * FROM ranked WHERE __rank<={options.PageSize} ORDER BY __owner,__key0,__key1", 0, token, parameters);
        foreach (var group in result.GroupBy(r => r.Long("__owner")))
            foreach (var row in group) rows.Set(("semantic_node", row.Long("id")), row);
        foreach (var id in pending)
            pages.Set((ChildrenQuery, id, 0), result.Where(row => row.Long("__owner") == id).ToArray());
        PrimeCompanions("semantic_node", result, token);
        if (depth > 0 && result.Length != 0) PrimeChildPages(result, token, depth - 1);
    }

    private static (string Query, string[] Keys) PagingQuery(string sql)
    {
        var order = Regex.Match(sql, @" ORDER BY (.+)$", RegexOptions.IgnoreCase);
        if (!order.Success) throw new InvalidOperationException("A paged relation needs deterministic ordering.");
        var columns = order.Groups[1].Value.Split(',');
        var keys = columns.Select((_, i) => $"__key{i}").ToArray();
        var start = sql.StartsWith("WITH ", StringComparison.OrdinalIgnoreCase) ? sql.LastIndexOf(") SELECT ", StringComparison.Ordinal) + 2 : 0;
        var from = sql.IndexOf(" FROM ", start, StringComparison.OrdinalIgnoreCase);
        if (from < 0) throw new InvalidOperationException("Unsupported internal relation query.");
        var fields = string.Join(',', columns.Select((c, i) => $"{c} AS {keys[i]}"));
        return (sql[..from] + "," + fields + sql[from..order.Index], keys);
    }

    private void PrimeCompanions(string table, Row[] page, CancellationToken token)
    {
        if (page.Length == 0) return;
        var ids = page.Select(r => r.Long("id")).Distinct().ToArray();
        var companions = table switch
        {
            "routine_symbol" or "variable_symbol" or "field_symbol" or "namespace_symbol" or "label_symbol" => new[] { "entity", "symbol" },
            "type" => new[] { "entity", "record_type", "qualified_type" },
            "semantic_node" => new[] { "entity", "expression", "statement", "call_expression", "conversion_expression", "cleanup_state_operation" },
            "expression" => new[] { "entity", "semantic_node", "call_expression", "conversion_expression", "cleanup_state_operation" },
            "statement" => new[] { "entity", "semantic_node" },
            "symbol" => new[] { "entity" },
            _ => System.Array.Empty<string>()
        };
        foreach (var companion in companions)
        {
            foreach (var batch in ids.Chunk(900))
            {
                var placeholders = string.Join(',', batch.Select((_, i) => $"$key{i}"));
                var result = QueryParameters($"SELECT * FROM {companion} WHERE id IN ({placeholders})", 0, token, batch.Select(id => (object?)id).ToArray());
                var present = result.Select(row => row.Long("id")).ToHashSet();
                foreach (var id in batch)
                    if (!present.Contains(id)) rows.Set((companion, id), null);
                foreach (var row in result)
                    rows.Set((companion, row.Long("id")), row);
            }
        }
    }

    internal long Scalar(string sql, long owner = 0)
    {
        lock (sync)
        {
            EnsureActive();
            using var command = Create(sql, owner);
            return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        }
    }

    internal long? FindAtom(string value)
    {
        lock (sync)
        {
            EnsureActive();
            using var command = Create("SELECT id FROM text_atom WHERE value=$text COLLATE BINARY", 0);
            command.Parameters.AddWithValue("$text", value);
            var result = command.ExecuteScalar();
            return result is null ? null : Convert.ToInt64(result, CultureInfo.InvariantCulture);
        }
    }

    private SqliteCommand Create(string sql, long owner)
    {
        commands++;
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.CommandTimeout = options.CommandTimeoutSeconds;
        command.Parameters.AddWithValue("$owner", owner);
        return command;
    }

    private void Execute(string sql)
    {
        using var command = Create(sql, 0);
        command.ExecuteNonQuery();
    }

    internal ReaderStatistics Statistics
    {
        get { lock (sync) return new(commands, rowsRead, rows.Count, pages.Count); }
    }

    public void Dispose()
    {
        lock (sync)
        {
            if (disposed) return;
            disposed = true;
            rows.Clear();
            pages.Clear();
            transaction.Dispose();
            connection.Dispose();
        }
    }
}
