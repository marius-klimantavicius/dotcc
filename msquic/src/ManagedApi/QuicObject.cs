using System.Collections.Concurrent;

namespace Managed.Transport.Api;

/// <summary>Common asynchronous lifetime for owning QUIC objects.</summary>
public abstract class QuicObject : IAsyncDisposable
{
    private static readonly ConcurrentDictionary<nint, QuicObject> contexts = new();
    private static long nextContext;
    internal static int LiveContextCount => contexts.Count;
    private readonly nint context;
    private int contextState; // 0: not published, 1: callback root, 2: retired
    private int operations;
    private TaskCompletionSource? operationsDrained;
    private Task? closeTask;
    private Exception? callbackFailure;
    private protected object? applicationContext;

    /// <summary>A managed association retained by this owner. It never replaces
    /// the rooted native callback token and cannot be accessed after close begins.</summary>
    public object? ApplicationContext
    {
        get { using var operation = EnterOperation(); lock (Gate) return applicationContext; }
        set { using var operation = EnterOperation(); lock (Gate) applicationContext = value; }
    }
    private protected readonly object Gate = new();
    private protected bool IsClosing { get; private set; }
    internal QuicRuntime Runtime { get; }
    internal unsafe QUIC_HANDLE* Handle { get; private protected set; }
    internal unsafe void* Context
    {
        get
        {
            lock (Gate)
            {
                ObjectDisposedException.ThrowIf(contextState == 2, this);
                if (contextState == 0)
                {
                    try
                    {
                        if (!contexts.TryAdd(context, this)) throw new InvalidOperationException("QUIC callback token collision.");
                        contextState = 1;
                    }
                    catch
                    {
                        contexts.TryRemove(new KeyValuePair<nint, QuicObject>(context, this));
                        throw;
                    }
                }
                return (void*)context;
            }
        }
    }
    internal bool HasNativeHandle { get { unsafe { return Handle != null; } } }

    private protected QuicObject(QuicRuntime runtime)
    {
        Runtime = runtime;
        long token = Interlocked.Increment(ref nextContext);
        if (token <= 0 || IntPtr.Size != 8) throw new InvalidOperationException("QUIC callback token space exhausted.");
        context = (nint)token;
    }

    private protected static unsafe T FromContext<T>(void* pointer) where T : QuicObject
    {
        if (contexts.TryGetValue((nint)pointer, out var value) && value is T owner) return owner;
        Environment.FailFast("QUIC callback used a retired or foreign owning context.");
        throw new InvalidOperationException();
    }

    private protected void RetireContext()
    {
        lock (Gate)
        {
            if (contextState == 2) throw new InvalidOperationException("QUIC callback context was retired twice.");
            if (contextState == 1 && !contexts.TryRemove(new KeyValuePair<nint, QuicObject>(context, this)))
                throw new InvalidOperationException("QUIC callback context ownership was lost.");
            contextState = 2;
            applicationContext = null;
        }
    }

    private protected void RecordCallbackFailure(Exception error)
        => Interlocked.CompareExchange(ref callbackFailure, error, null);
    internal Exception? CallbackFailure => callbackFailure;

    private protected HandleOperation EnterOperation()
    {
        lock (Gate)
        {
            ObjectDisposedException.ThrowIf(IsClosing, this);
            int next = checked(operations + 1);
            var operation = new HandleOperation(this);
            operations = next;
            return operation;
        }
    }

    private protected sealed class HandleOperation : IDisposable
    {
        private QuicObject? owner;
        internal HandleOperation(QuicObject owner) => this.owner = owner;
        public void Dispose()
        {
            var value = Interlocked.Exchange(ref owner, null);
            if (value == null) return;
            lock (value.Gate)
            {
                if (--value.operations == 0) value.operationsDrained?.TrySetResult();
            }
        }
    }

    private protected Task BeginCloseAsync()
    {
        lock (Gate)
        {
            IsClosing = true;
            if (operations == 0) return Task.CompletedTask;
            return (operationsDrained ??= new(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
        }
    }

    // Scheduling the delegate ensures a completed drain cannot run native close
    // synchronously while the caller still holds Gate or is in a core callback.
    private protected Task CloseOnce(Func<Task> close)
    {
        lock (Gate)
        {
            if (closeTask != null) return closeTask;
            Task drain = BeginCloseAsync();
            closeTask = Task.Run(async () =>
            {
                await drain.ConfigureAwait(false);
                await close().ConfigureAwait(false);
            });
            return closeTask;
        }
    }

    public abstract ValueTask DisposeAsync();
}
