namespace RoslynObj;

/// <summary>Controls read costs without changing the semantic contents of an object.</summary>
public sealed record ReadOptions
{
    public int PageSize { get; init; } = 128;
    public int CachedRowCount { get; init; } = 4096;
    public int CachedPageCount { get; init; } = 128;
    public int CommandTimeoutSeconds { get; init; } = 30;

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(PageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(PageSize, 4096);
        ArgumentOutOfRangeException.ThrowIfLessThan(CachedRowCount, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(CachedPageCount, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(CommandTimeoutSeconds, 0);
    }
}

public sealed class IncompatibleObjectException(string message) : IOException(message);

/// <summary>Detached reader counters for measuring bounded database access.</summary>
public sealed record ReaderStatistics(long Commands, long RowsRead, int CachedRows, int CachedPages)
{
    /// <summary>Largest number of keys sent in one batched companion or child-page query.</summary>
    public int MaximumBatchSize { get; init; }
}
