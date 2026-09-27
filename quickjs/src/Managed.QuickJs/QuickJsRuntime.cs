using System.Runtime.InteropServices;
using VM = Managed.Interpreters.QuickJs;

namespace Managed.Interpreters;

public sealed class QuickJsException(string message) : Exception(message);

/// <summary>Owns a translated QuickJS runtime. Calls on one runtime must not overlap or reenter.</summary>
public sealed unsafe class QuickJsRuntime : IDisposable
{
    private readonly object _gate = new object();
    private readonly List<QuickJsContext> _contexts = [];
    private readonly Dictionary<string, string> _modules;

    private readonly record struct Rejection(VM.JSValue Promise, string Message);

    private readonly Dictionary<nint, Rejection> _rejections = [];
    private readonly QuickJsHost.AllocationAccount _allocations = new QuickJsHost.AllocationAccount();
    private GCHandle _allocationHandle;
    private GCHandle _runtimeHandle;
    internal VM.JSRuntime* NativeRuntime;
    private int _activeThread;
    private volatile bool _disposing;
    private bool _disposed;
    private CancellationToken _cancellation;
    public int ModuleLoads { get; private set; }
    public long AllocatedBlocks => Interlocked.Read(ref _allocations.Blocks);
    public long AllocatedBytes => Interlocked.Read(ref _allocations.Bytes);

    public QuickJsRuntime(IReadOnlyDictionary<string, string>? modules = null, ulong memoryLimit = 64 * 1024 * 1024, ulong stackLimit = 256 * 1024)
    {
        if (IntPtr.Size != 8 || !BitConverter.IsLittleEndian)
            throw new PlatformNotSupportedException("This product is qualified for little-endian Linux x64.");

        if (stackLimit < 64 * 1024 || stackLimit > 512 * 1024)
            throw new ArgumentOutOfRangeException(nameof(stackLimit), "Stack budget must be between 64 and 512 KiB.");

        _modules = modules is null ? [] : new Dictionary<string, string>(modules, StringComparer.Ordinal);

        foreach (var name in _modules.Keys)
            QuickJsText.CheckName(name);

        try
        {
            _allocationHandle = GCHandle.Alloc(_allocations);
            // Weak root permits finalization if an embedding forgets Dispose; values/contexts retain their owner.
            _runtimeHandle = GCHandle.Alloc(new WeakReference<QuickJsRuntime>(this));
            var allocator = new VM.JSMallocFunctions
            {
                js_malloc = &QuickJsHost.Allocate, js_free = &QuickJsHost.Free,
                js_realloc = &QuickJsHost.Reallocate, js_malloc_usable_size = &QuickJsHost.UsableSize
            };

            NativeRuntime = VM.JS_NewRuntime2(&allocator, (void*)GCHandle.ToIntPtr(_allocationHandle));
            if (NativeRuntime == null)
                throw new OutOfMemoryException("QuickJS runtime initialization failed.");

            VM.JS_SetMemoryLimit(NativeRuntime, memoryLimit);
            VM.JS_SetMaxStackSize(NativeRuntime, stackLimit);

            var token = (void*)GCHandle.ToIntPtr(_runtimeHandle);
            VM.JS_SetRuntimeOpaque(NativeRuntime, token);
            VM.JS_SetInterruptHandler(NativeRuntime, &Interrupt, token);
            VM.JS_SetModuleLoaderFunc(NativeRuntime, null, &LoadModule, token);
            VM.JS_SetHostPromiseRejectionTracker(NativeRuntime, &TrackRejection, token);
        }
        catch
        {
            Release();
            throw;
        }
    }

    ~QuickJsRuntime()
    {
        try
        {
            Release();
        }
        catch
        {
            /* Finalizers must never terminate the host process. Dispose reports failures. */
        }
    }

    internal void Enter(CancellationToken token = default)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed || _disposing, this);

            if (_activeThread != 0)
                throw new InvalidOperationException("Concurrent entry and callback reentry into one QuickJS runtime are unsupported.");

            _activeThread = Environment.CurrentManagedThreadId;
            _cancellation = token;
            VM.JS_UpdateStackTop(NativeRuntime);
        }
    }

    internal void Leave()
    {
        lock (_gate)
        {
            _activeThread = 0;
            _cancellation = CancellationToken.None;
            Monitor.PulseAll(_gate);
        }

        GC.KeepAlive(this);
    }

    public QuickJsContext CreateContext()
    {
        Enter(CancellationToken.None);
        try
        {
            var pointer = VM.JS_NewContext(NativeRuntime);
            if (pointer == null)
                throw new OutOfMemoryException("QuickJS context initialization failed.");

            QuickJsContext? context = null;
            try
            {
                context = new QuickJsContext(this, pointer);
                _contexts.Add(context);
                return context;
            }
            catch
            {
                if (context is null)
                    VM.JS_FreeContext(pointer);
                else
                {
                    context.Release();
                    context.ReleaseCallbackRoots();
                }

                throw;
            }
        }
        finally
        {
            Leave();
        }
    }

    /// <summary>Execute at most maxJobs pending Promise jobs; returns the number executed.</summary>
    public int DrainJobs(int maxJobs = 10000, CancellationToken cancellationToken = default)
    {
        if (maxJobs < 1)
            throw new ArgumentOutOfRangeException(nameof(maxJobs));

        Enter(cancellationToken);
        try
        {
            var count = 0;
            while (VM.JS_IsJobPending(NativeRuntime) != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (count == maxJobs)
                    throw new QuickJsException("Pending-job budget exhausted.");

                VM.JSContext* context = null;
                if (VM.JS_ExecutePendingJob(NativeRuntime, &context) < 0)
                    throw QuickJsContext.TakeException(context);

                count++;
            }

            if (_rejections.Count != 0)
            {
                var message = string.Join("; ", _rejections.Values.Select(value => value.Message));
                ClearRejections();
                throw new QuickJsException("Unhandled Promise rejection: " + message);
            }

            return count;
        }
        finally
        {
            Leave();
        }
    }

    public void CollectGarbage()
    {
        Enter(CancellationToken.None);
        try
        {
            VM.JS_RunGC(NativeRuntime);
        }
        finally
        {
            Leave();
        }
    }

    internal static QuickJsRuntime Owner(void* token)
    {
        var weak = (WeakReference<QuickJsRuntime>)GCHandle.FromIntPtr((nint)token).Target!;
        return weak.TryGetTarget(out var runtime) ? runtime : throw new ObjectDisposedException(nameof(QuickJsRuntime));
    }

    private static int Interrupt(VM.JSRuntime* runtime, void* opaque)
    {
        try
        {
            var owner = Owner(opaque);
            return owner._disposing || owner._cancellation.IsCancellationRequested ? 1 : 0;
        }
        catch { return 1; }
    }

    private static void TrackRejection(VM.JSContext* context, VM.JSValue promise, VM.JSValue reason, int handled, void* opaque)
    {
        try
        {
            var owner = Owner(opaque);
            var key = (nint)promise.u.ptr;
            if (handled != 0)
            {
                if (owner._rejections.Remove(key, out var rejection)) VM.JS_FreeValueRT(owner.NativeRuntime, rejection.Promise);
            }
            else
            {
                string message;
                try
                {
                    message = QuickJsContext.String(context, reason);
                }
                catch
                {
                    message = "Rejection reason could not be converted to text.";
                }

                if (owner._rejections.TryGetValue(key, out var prior))
                {
                    owner._rejections[key] = prior with { Message = message };
                }
                else
                {
                    owner._rejections.EnsureCapacity(owner._rejections.Count + 1);
                    owner._rejections.Add(key, new Rejection(VM.JS_DupValueRT(owner.NativeRuntime, promise), message));
                }
            }
        }
        catch
        {
            /* A managed exception must not cross a translated callback boundary. */
        }
    }

    private static VM.JSModuleDef* LoadModule(VM.JSContext* context, byte* name, void* opaque)
    {
        scoped var sourceMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            var owner = Owner(opaque);
            ulong length = 0;
            while (name[length] != 0)
                length++;

            var key = QuickJsText.Decode(name, length);
            if (!owner._modules.TryGetValue(key, out var source))
            {
                QuickJsContext.ThrowManaged(context, new QuickJsException($"Unknown in-memory module '{key}'."));
                return null;
            }

            owner.ModuleLoads++;

            sourceMarshaller.FromManaged(source, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);
            {
                var result = VM.JS_Eval(context, sourceMarshaller.ToUnmanaged(), (ulong)sourceMarshaller.Count, name, VM.JS_EVAL_TYPE_MODULE | VM.JS_EVAL_FLAG_COMPILE_ONLY);
                if (result.tag == VM.JS_TAG_EXCEPTION)
                    return null;

                var module = (VM.JSModuleDef*)result.u.ptr;
                VM.JS_FreeValue(context, result);
                return module;
            }
        }
        catch (Exception error)
        {
            QuickJsContext.ThrowManaged(context, error);
            return null;
        }
        finally
        {
            sourceMarshaller.Free();
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;

            if (_activeThread == Environment.CurrentManagedThreadId)
                throw new InvalidOperationException("A callback cannot dispose its active runtime.");

            _disposing = true;

            while (_activeThread != 0)
                Monitor.Wait(_gate);

            Release();
        }

        GC.SuppressFinalize(this);
        if (AllocatedBlocks != 0)
            throw new InvalidOperationException($"QuickJS leaked {AllocatedBlocks} owned allocations.");
    }

    private void Release()
    {
        if (_disposed) return;

        _disposed = true;
        ClearRejections();
        foreach (var context in _contexts) context.Release();
        if (NativeRuntime != null)
        {
            VM.JS_FreeRuntime(NativeRuntime);
            NativeRuntime = null;
        }

        foreach (var context in _contexts) context.ReleaseCallbackRoots();
        _contexts.Clear();
        if (_runtimeHandle.IsAllocated) _runtimeHandle.Free();
        if (_allocationHandle.IsAllocated) _allocationHandle.Free();
    }

    private void ClearRejections()
    {
        foreach (var rejection in _rejections.Values)
            VM.JS_FreeValueRT(NativeRuntime, rejection.Promise);

        _rejections.Clear();
    }
}