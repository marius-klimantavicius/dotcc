// Manual .NET adaptation of @electric-sql/pglite src/live/index.ts and interface.ts (Apache-2.0).
using System.Collections.ObjectModel;

namespace Managed.PGlite;

public sealed record PgliteLiveQueryOptions(string Query)
{
    public IReadOnlyList<object?> Parameters { get; init; } = [];
    public PgliteQueryOptions? QueryOptions { get; init; }
    public int? Offset { get; init; }
    public int? Limit { get; init; }
    /// <summary>Stops future refresh/callback delivery. Cancellation is observed
    /// on the next owner operation and never enters PostgreSQL from a pool thread.</summary>
    public CancellationToken CancellationToken { get; init; }
}

public sealed record PgliteLiveResult(PgliteQueryResult Response,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows,
    IReadOnlyList<PgliteColumn> Fields, int? TotalCount = null, int? Offset = null, int? Limit = null);
public enum PgliteLiveOperation { Insert, Delete, Update, Reset }
/// <summary>After is the preceding row's key (null for the first row). Update
/// contains the key and changed columns; Delete contains only the key.</summary>
public sealed record PgliteLiveChange(PgliteLiveOperation Operation,
    IReadOnlyDictionary<string, object?> Values, object? After, IReadOnlyList<string> ChangedColumns);

/// <summary>Live adapters refresh read-only SQL after successful local idle
/// commits. No polling, cross-process change feed, or table-trigger invalidation
/// is implied. Full result comparison supplies stable keyed changes and order.</summary>
public sealed class PgliteLive
{
    private readonly PgliteDatabase database;
    private readonly List<PgliteLiveQuery> queries = [];
    internal PgliteLive(PgliteDatabase database) { this.database = database; }
    public PgliteLiveQuery Query(string query, IReadOnlyList<object?>? parameters = null,
        Action<PgliteLiveResult>? callback = null, CancellationToken cancellationToken = default)
        => Query(new(query) { Parameters = parameters ?? [], CancellationToken = cancellationToken }, callback);
    public PgliteLiveQuery Query(PgliteLiveQueryOptions options, Action<PgliteLiveResult>? callback = null)
        => Create(options, null, callback);
    public PgliteLiveChanges Changes(string query, string key, IReadOnlyList<object?>? parameters = null,
        Action<IReadOnlyList<PgliteLiveChange>>? callback = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var subscription = Create(new(query) { Parameters = parameters ?? [], CancellationToken = cancellationToken }, key, null);
        var result = new PgliteLiveChanges(subscription);
        if (callback != null) result.Subscribe(callback);
        return result;
    }
    public PgliteLiveQuery IncrementalQuery(string query, string key, IReadOnlyList<object?>? parameters = null,
        Action<PgliteLiveResult>? callback = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return Create(new(query) { Parameters = parameters ?? [], CancellationToken = cancellationToken }, key, callback);
    }
    private PgliteLiveQuery Create(PgliteLiveQueryOptions options, string? key, Action<PgliteLiveResult>? callback)
    {
        ArgumentNullException.ThrowIfNull(options);
        database.RequireLiveOwner();
        var subscription = new PgliteLiveQuery(this, database, options, key);
        queries.Add(subscription);
        if (callback != null) subscription.Subscribe(callback);
        return subscription;
    }
    internal void Remove(PgliteLiveQuery query) => queries.Remove(query);
    internal void RefreshAll(List<Exception> errors)
    {
        foreach (var query in queries.ToArray())
        {
            try { query.RefreshCore(errors); }
            catch (Exception error) { errors.Add(error); }
        }
    }
    internal void CloseAll()
    {
        foreach (var query in queries.ToArray()) query.CloseCore();
        queries.Clear();
    }
}

public sealed class PgliteLiveQuery : IDisposable
{
    private readonly PgliteLive live;
    private readonly PgliteDatabase database;
    private readonly PgliteLiveQueryOptions options;
    private readonly string? key;
    private readonly List<Action<PgliteLiveResult>> callbacks = [];
    private readonly List<Action<IReadOnlyList<PgliteLiveChange>>> changeCallbacks = [];
    private PgliteResultSet state;
    private PgliteLiveResult current;
    private bool closed;
    public bool Closed => closed || options.CancellationToken.IsCancellationRequested;
    public PgliteLiveResult InitialResults { get; }
    public PgliteLiveResult Current => current;
    public IReadOnlyList<PgliteLiveChange> InitialChanges { get; }

    internal PgliteLiveQuery(PgliteLive live, PgliteDatabase database, PgliteLiveQueryOptions options, string? key)
    {
        options.CancellationToken.ThrowIfCancellationRequested();
        if ((options.Offset == null) != (options.Limit == null))
            throw new ArgumentException("Live offset and limit must be provided together.");
        if (options.Offset < 0 || options.Limit < 0) throw new ArgumentOutOfRangeException(nameof(options));
        this.live = live; this.database = database;
        this.options = options with { Parameters = Array.AsReadOnly(options.Parameters.ToArray()) }; this.key = key;
        var response = database.ReadLiveQuery(this.options.Query, this.options.Parameters, this.options.QueryOptions);
        state = RequireSingleSet(response);
        ValidateKeys(state);
        current = InitialResults = MakeResult(response, state, options.Offset, options.Limit);
        InitialChanges = key == null ? [] : Diff(null, state);
    }

    public void Subscribe(Action<PgliteLiveResult> callback)
    { ArgumentNullException.ThrowIfNull(callback); RequireOpen(); callbacks.Add(callback); }
    public void Unsubscribe(Action<PgliteLiveResult>? callback = null)
    {
        RequireOpen();
        if (callback == null) { Dispose(); return; }
        callbacks.RemoveAll(candidate => candidate == callback);
        if (callbacks.Count == 0 && changeCallbacks.Count == 0) Dispose();
    }
    internal void SubscribeChanges(Action<IReadOnlyList<PgliteLiveChange>> callback)
    { ArgumentNullException.ThrowIfNull(callback); RequireOpen(); changeCallbacks.Add(callback); }
    internal void UnsubscribeChanges(Action<IReadOnlyList<PgliteLiveChange>>? callback)
    {
        RequireOpen();
        if (callback == null) { Dispose(); return; }
        changeCallbacks.RemoveAll(candidate => candidate == callback);
        if (callbacks.Count == 0 && changeCallbacks.Count == 0) Dispose();
    }
    public void Refresh(int? offset = null, int? limit = null)
    {
        RequireOpen();
        if ((offset != null || limit != null) && options.Offset == null)
            throw new ArgumentException("Window updates require a windowed live query.");
        if (offset < 0 || limit < 0) throw new ArgumentOutOfRangeException(nameof(offset));
        var errors = new List<Exception>();
        RefreshCore(errors, offset ?? current.Offset, limit ?? current.Limit, force: true);
        if (errors.Count != 0) throw new AggregateException("A live callback failed after query execution.", errors);
    }
    internal void RefreshCore(List<Exception> errors, int? offset = null, int? limit = null, bool force = false)
    {
        if (closed) return;
        if (options.CancellationToken.IsCancellationRequested) { CloseCore(); live.Remove(this); return; }
        var response = database.ReadLiveQuery(options.Query, options.Parameters, options.QueryOptions);
        var next = RequireSingleSet(response);
        ValidateKeys(next);
        bool schemaChanged = !state.Columns.SequenceEqual(next.Columns);
        bool changed = schemaChanged || !RowsEqual(state.Rows, next.Rows);
        var changes = key == null ? [] : schemaChanged
            ? new[] { new PgliteLiveChange(PgliteLiveOperation.Reset, EmptyValues, null, []) }.Concat(Diff(null, next)).ToArray()
            : Diff(state, next);
        state = next;
        current = MakeResult(response, next, offset ?? current.Offset, limit ?? current.Limit);
        if (changed || force)
            foreach (var callback in callbacks.ToArray()) database.DeliverLiveCallback(() => callback(current), errors);
        if (changes.Count != 0 || force)
            foreach (var callback in changeCallbacks.ToArray()) database.DeliverLiveCallback(() => callback(changes), errors);
    }
    private static readonly IReadOnlyDictionary<string, object?> EmptyValues =
        new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
    private PgliteLiveResult MakeResult(PgliteQueryResult response, PgliteResultSet set, int? offset, int? limit)
    {
        var rows = set.TypedRows.Select(row => ToObject(set, row)).ToArray();
        IReadOnlyList<IReadOnlyDictionary<string, object?>> selected = Array.AsReadOnly(rows);
        if (offset != null) selected = Array.AsReadOnly(rows.Skip(offset.Value).Take(limit!.Value).ToArray());
        return new(response, selected, set.Columns, offset == null ? null : rows.Length, offset, limit);
    }
    private static IReadOnlyDictionary<string, object?> ToObject(PgliteResultSet set, IReadOnlyList<object?> row)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        for (int i = 0; i < set.Columns.Count; i++)
            if (!values.TryAdd(set.Columns[i].Name, row[i]))
                throw new ArgumentException("Live result columns must have unique names; alias duplicate columns.");
        return new ReadOnlyDictionary<string, object?>(values);
    }
    private int KeyIndex(PgliteResultSet set)
    {
        for (int i = 0; i < set.Columns.Count; i++) if (set.Columns[i].Name == key) return i;
        throw new ArgumentException($"Live key column '{key}' is missing from the query.");
    }
    private void ValidateKeys(PgliteResultSet set)
    {
        if (key == null) return;
        int index = KeyIndex(set);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in set.Rows)
            if (row[index] == null || !keys.Add(row[index]!))
                throw new ArgumentException("Live changes require a unique, non-NULL key in every result row.");
    }
    private IReadOnlyList<PgliteLiveChange> Diff(PgliteResultSet? previous, PgliteResultSet next)
    {
        int keyIndex = KeyIndex(next);
        var old = new Dictionary<string, int>(StringComparer.Ordinal);
        if (previous != null)
            for (int i = 0; i < previous.Rows.Count; i++) old.Add(previous.Rows[i][keyIndex]!, i);
        var nextKeys = new HashSet<string>(next.Rows.Select(row => row[keyIndex]!), StringComparer.Ordinal);
        var changes = new List<PgliteLiveChange>();
        if (previous != null)
            for (int i = 0; i < previous.Rows.Count; i++)
                if (!nextKeys.Contains(previous.Rows[i][keyIndex]!))
                    changes.Add(new(PgliteLiveOperation.Delete,
                        new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { [key!] = previous.TypedRows[i][keyIndex] }), null, []));
        for (int i = 0; i < next.Rows.Count; i++)
        {
            string identity = next.Rows[i][keyIndex]!;
            object? after = i == 0 ? null : next.TypedRows[i - 1][keyIndex];
            if (previous == null || !old.TryGetValue(identity, out int oldIndex))
                changes.Add(new(PgliteLiveOperation.Insert, ToObject(next, next.TypedRows[i]), after, []));
            else
            {
                var columns = new List<string>();
                var values = new Dictionary<string, object?> { [key!] = next.TypedRows[i][keyIndex] };
                for (int column = 0; column < next.Columns.Count; column++)
                    if (next.Rows[i][column] != previous.Rows[oldIndex][column])
                    {
                        columns.Add(next.Columns[column].Name);
                        values[next.Columns[column].Name] = next.TypedRows[i][column];
                    }
                string? oldAfter = oldIndex == 0 ? null : previous.Rows[oldIndex - 1][keyIndex];
                string? nextAfter = i == 0 ? null : next.Rows[i - 1][keyIndex];
                if (oldAfter != nextAfter) columns.Add("__after__");
                if (columns.Count != 0)
                    changes.Add(new(PgliteLiveOperation.Update, new ReadOnlyDictionary<string, object?>(values), after, columns.AsReadOnly()));
            }
        }
        return changes.AsReadOnly();
    }
    private static bool RowsEqual(IReadOnlyList<IReadOnlyList<string?>> left, IReadOnlyList<IReadOnlyList<string?>> right)
    {
        if (left.Count != right.Count) return false;
        for (int i = 0; i < left.Count; i++) if (!left[i].SequenceEqual(right[i])) return false;
        return true;
    }
    private static PgliteResultSet RequireSingleSet(PgliteQueryResult result)
    {
        result.EnsureSuccess();
        if (result.ResultSets.Count != 1 || result.ResultSets[0].Columns.Count == 0)
            throw new ArgumentException("A live query must return exactly one result set with named columns.");
        return result.ResultSets[0];
    }
    private void RequireOpen()
    { database.RequireLiveOwner(); options.CancellationToken.ThrowIfCancellationRequested(); if (closed) throw new ObjectDisposedException(nameof(PgliteLiveQuery)); }
    internal void CloseCore() { closed = true; callbacks.Clear(); changeCallbacks.Clear(); }
    public void Dispose() { if (closed) return; database.RequireLiveOwner(); CloseCore(); live.Remove(this); }
}

public sealed class PgliteLiveChanges : IDisposable
{
    private readonly PgliteLiveQuery query;
    internal PgliteLiveChanges(PgliteLiveQuery query) { this.query = query; }
    public IReadOnlyList<PgliteColumn> Fields => query.InitialResults.Fields;
    public IReadOnlyList<PgliteLiveChange> InitialChanges => query.InitialChanges;
    public bool Closed => query.Closed;
    public void Subscribe(Action<IReadOnlyList<PgliteLiveChange>> callback) => query.SubscribeChanges(callback);
    public void Unsubscribe(Action<IReadOnlyList<PgliteLiveChange>>? callback = null) => query.UnsubscribeChanges(callback);
    public void Refresh() => query.Refresh();
    public void Dispose() => query.Dispose();
}

public sealed unsafe partial class PgliteDatabase
{
    public PgliteLive Live { get { RequireLiveOwner(); return serviceLive ??= new(this); } }
    internal void RequireLiveOwner() { RequireServiceIdle(); BeforeQuery(); }
    internal PgliteQueryResult ReadLiveQuery(string sql, IReadOnlyList<object?> parameters, PgliteQueryOptions? queryOptions)
    {
        RequireServiceIdle();
        bool previousRefreshing = serviceRefreshing;
        serviceRefreshing = true;
        serviceBatchDepth++;
        try
        {
            Query("BEGIN READ ONLY").EnsureSuccess();
            try
            {
                var response = Query(sql, parameters, queryOptions).EnsureSuccess();
                Query("COMMIT").EnsureSuccess();
                return response;
            }
            catch
            {
                if (Ready) Query("ROLLBACK").EnsureSuccess();
                throw;
            }
        }
        finally { serviceBatchDepth--; serviceRefreshing = previousRefreshing; }
    }
}
