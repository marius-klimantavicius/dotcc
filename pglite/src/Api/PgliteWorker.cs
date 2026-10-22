// Manual .NET adaptation of @electric-sql/pglite src/worker/index.ts (Apache-2.0).
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Managed.PGlite;

/// <summary>A dedicated .NET thread owns the translated engine for its entire
/// lifetime. Callers receive asynchronous promises; every database operation and
/// callback still executes synchronously on that dedicated owner thread.</summary>
public sealed class PgliteWorker : IDisposable, IAsyncDisposable
{
    private readonly object gate = new();
    private readonly BlockingCollection<IWorkItem> queue = new();
    private readonly Thread ownerThread;
    private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private volatile bool isReady, isClosed;
    private bool closing;
    public bool Ready => isReady;
    public bool Closed => isClosed;
    public int OwnerThreadId => ownerThread.ManagedThreadId;

    private PgliteWorker(PgliteOptions options, bool initialize)
    {
        ArgumentNullException.ThrowIfNull(options);
        ownerThread = new Thread(() => Run(options, initialize)) { IsBackground = true, Name = "PGlite owner" };
        ownerThread.Start();
    }
    public static PgliteWorker Create(PgliteOptions options) => new(options, initialize: true);
    public static PgliteWorker Open(PgliteOptions options) => new(options, initialize: false);
    public Task WaitReady(CancellationToken cancellationToken = default) => ready.Task.WaitAsync(cancellationToken);

    public Task<T> InvokeAsync<T>(Func<PgliteDatabase, T> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        RejectAsync<T>(operation);
        // Queuing is safe from callbacks, but blocking for that queued result on
        // the owner would deadlock. Reject owner calls instead of hiding it.
        if (Environment.CurrentManagedThreadId == ownerThread.ManagedThreadId)
            throw new InvalidOperationException("A worker operation cannot enqueue another operation from its owner callback.");
        lock (gate)
        {
            if (closing) return Task.FromException<T>(new ObjectDisposedException(nameof(PgliteWorker)));
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<T>(cancellationToken);
            var item = new WorkItem<T>(database =>
            {
                try { return operation(database); }
                finally { isReady = database.Ready; }
            }, cancellationToken);
            queue.Add(item);
            return item.Task;
        }
    }
    public Task InvokeAsync(Action<PgliteDatabase> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        RejectAsyncVoid(operation);
        return InvokeAsync(database => { operation(database); return 0; }, cancellationToken);
    }
    public Task<PgliteQueryResult> QueryAsync(string sql, CancellationToken cancellationToken = default)
        => InvokeAsync(database => database.Query(sql), cancellationToken);
    public Task<PgliteQueryResult> QueryAsync(string sql, IReadOnlyList<object?> parameters,
        PgliteQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        var snapshot = parameters.ToArray();
        return InvokeAsync(database => database.Query(sql, snapshot, options), cancellationToken);
    }
    public Task<PgliteQueryResult> ExecAsync(string sql, CancellationToken cancellationToken = default)
        => InvokeAsync(database => database.Exec(sql), cancellationToken);
    public Task<PgliteQueryResult> SqlAsync(PgliteSql sql, CancellationToken cancellationToken = default)
        => InvokeAsync(database => database.Sql(sql), cancellationToken);
    public Task<T> TransactionAsync<T>(Func<PgliteTransaction, T> callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback); RejectAsync<T>(callback);
        return InvokeAsync(database => database.Transaction(callback), cancellationToken);
    }
    public Task TransactionAsync(Action<PgliteTransaction> callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback); RejectAsyncVoid(callback);
        return InvokeAsync(database => database.Transaction(callback), cancellationToken);
    }
    public Task<byte[]> DumpDataDirAsync(PgliteDumpCompression compression = PgliteDumpCompression.Auto,
        CancellationToken cancellationToken = default)
        => InvokeAsync(database => database.DumpDataDir(compression), cancellationToken);
    public Task SyncToFsAsync(CancellationToken cancellationToken = default)
        => InvokeAsync(database => database.SyncToFs(), cancellationToken);

    /// <summary>Drain operations already accepted, then close on the owner thread.
    /// Cancellation applies to awaiting, never to an executing PostgreSQL query.</summary>
    public Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (Environment.CurrentManagedThreadId == ownerThread.ManagedThreadId)
            throw new InvalidOperationException("Close the worker from its caller, after the owner callback returns.");
        lock (gate)
        {
            if (!closing) { closing = true; queue.CompleteAdding(); }
        }
        return closed.Task.WaitAsync(cancellationToken);
    }
    public void Dispose() => CloseAsync().GetAwaiter().GetResult();
    public ValueTask DisposeAsync() => new(CloseAsync());

    private void Run(PgliteOptions options, bool initialize)
    {
        PgliteDatabase? database = null;
        Exception? failure = null;
        try
        {
            database = initialize ? PgliteDatabase.Create(options) : PgliteDatabase.Open(options);
            isReady = true;
            ready.TrySetResult();
            foreach (var item in queue.GetConsumingEnumerable())
            {
                item.Run(database);
                if (database.Closed) break;
            }
        }
        catch (Exception error) { failure = error; ready.TrySetException(error); }
        finally
        {
            lock (gate)
            {
                closing = true;
                if (!queue.IsAddingCompleted) queue.CompleteAdding();
            }
            while (queue.TryTake(out var item)) item.Fail(failure ?? new ObjectDisposedException(nameof(PgliteWorker)));
            if (database != null)
            {
                try { database.Dispose(); }
                catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
            }
            isReady = false;
            isClosed = database?.Closed ?? false;
            if (failure == null) closed.TrySetResult(); else closed.TrySetException(failure);
            queue.Dispose();
        }
    }
    private static void RejectAsyncVoid(Delegate operation)
    {
        if (operation.Method.IsDefined(typeof(AsyncStateMachineAttribute), inherit: false))
            throw new ArgumentException("Worker delegates must be synchronous; await the returned queue task from the caller.");
    }
    private static void RejectAsync<T>(Delegate operation)
    {
        RejectAsyncVoid(operation);
        Type type = typeof(T);
        if (typeof(Task).IsAssignableFrom(type) || type == typeof(ValueTask) ||
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            throw new ArgumentException("Worker delegates cannot return Task or ValueTask; the engine must remain on its dedicated thread.");
    }
    private interface IWorkItem
    { void Run(PgliteDatabase database); void Fail(Exception error); }
    private sealed class WorkItem<T>(Func<PgliteDatabase, T> operation, CancellationToken cancellationToken) : IWorkItem
    {
        private readonly TaskCompletionSource<T> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<T> Task => completion.Task;
        public void Run(PgliteDatabase database)
        {
            if (cancellationToken.IsCancellationRequested) { completion.TrySetCanceled(cancellationToken); return; }
            try { completion.TrySetResult(operation(database)); }
            catch (Exception error) { completion.TrySetException(error); }
        }
        public void Fail(Exception error) => completion.TrySetException(error);
    }
}
