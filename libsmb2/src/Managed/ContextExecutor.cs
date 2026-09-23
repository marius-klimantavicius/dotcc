using System.Collections.Concurrent;

namespace Managed.Smb;

/// <summary>A serial queue of short, synchronous context turns. Idle queues own no thread.</summary>
internal sealed class ContextExecutor
{
    [ThreadStatic] private static ContextExecutor? _current;
    private readonly ConcurrentQueue<Action> _queue = new();
    private int _scheduled;
    internal bool IsCurrent => ReferenceEquals(_current, this);

    internal void Post(Action action)
    {
        _queue.Enqueue(action);
        Schedule();
    }

    internal Task<T> Invoke<T>(Func<T> action)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        Post(() => { try { completion.SetResult(action()); } catch (Exception e) { completion.SetException(e); } });
        return completion.Task;
    }

    internal Task Invoke(Action action) => Invoke(() => { action(); return true; });

    private void Schedule()
    {
        if (Interlocked.CompareExchange(ref _scheduled, 1, 0) == 0)
            ThreadPool.UnsafeQueueUserWorkItem(static executor => executor.Drain(), this, preferLocal: false);
    }

    private void Drain()
    {
        _current = this;
        try
        {
            // Yield to other contexts after a bounded number of completion turns.
            for (int count = 0; count < 64 && _queue.TryDequeue(out var work); count++) work();
        }
        finally
        {
            _current = null;
            Volatile.Write(ref _scheduled, 0);
            if (!_queue.IsEmpty) Schedule();
        }
    }
}
