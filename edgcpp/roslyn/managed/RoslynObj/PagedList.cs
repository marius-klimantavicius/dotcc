using System.Collections;

namespace RoslynObj;

/// <summary>An immutable collection whose enumeration fetches bounded pages from the fixed snapshot.</summary>
public sealed class PagedList<T> : IReadOnlyList<T>
{
    private readonly SnapshotStore store;
    private readonly string query;
    private readonly long owner;
    private readonly string? entityTable;
    private readonly Func<Row, T> materialize;
    private int count = -1;

    internal PagedList(SnapshotStore store, string query, long owner, Func<Row, T> materialize, string? entityTable = null)
    {
        this.store = store;
        this.query = query;
        this.owner = owner;
        this.materialize = materialize;
        this.entityTable = entityTable;
    }

    public int Count
    {
        get
        {
            store.EnsureActive();
            var existing = Volatile.Read(ref count);
            if (existing >= 0) return existing;
            var loaded = checked((int)store.Scalar($"SELECT COUNT(*) FROM ({query})", owner));
            Interlocked.CompareExchange(ref count, loaded, -1);
            return Volatile.Read(ref count);
        }
    }

    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            if (index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var page = store.Page(query, owner, index / store.PageSize, entityTable);
            return materialize(page[index % store.PageSize]);
        }
    }

    public IEnumerable<T> Enumerate(CancellationToken cancellationToken = default)
    {
        Row? previous = null;
        for (var page = 0; ; page++)
        {
            var values = store.NextPage(query, owner, page, previous, entityTable, cancellationToken);
            foreach (var value in values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return materialize(value);
            }
            if (values.Length < store.PageSize) yield break;
            previous = values[^1];
        }
    }

    public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
