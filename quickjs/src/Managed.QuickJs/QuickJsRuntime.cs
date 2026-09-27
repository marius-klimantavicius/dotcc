using System.Runtime.InteropServices;
using System.Text;
using VM = Managed.Interpreters.QuickJs;

namespace Managed.Interpreters;

public sealed class QuickJsException(string message) : Exception(message);

/// <summary>Owns a translated QuickJS runtime. Calls on one runtime must not overlap or reenter.</summary>
public sealed unsafe class QuickJsRuntime : IDisposable
{
    private readonly object gate = new();
    private readonly List<QuickJsContext> contexts = [];
    private readonly Dictionary<string, string> modules;
    private readonly record struct Rejection(VM.JSValue Promise, string Message);
    private readonly Dictionary<nint, Rejection> rejections = [];
    private readonly QuickJsHost.AllocationAccount allocations = new();
    private GCHandle allocationHandle;
    private GCHandle runtimeHandle;
    internal VM.JSRuntime* Pointer;
    private int activeThread;
    private volatile bool disposing;
    private bool disposed;
    private CancellationToken cancellation;
    public int ModuleLoads { get; private set; }
    public long AllocatedBlocks => Interlocked.Read(ref allocations.Blocks);
    public long AllocatedBytes => Interlocked.Read(ref allocations.Bytes);

    public QuickJsRuntime(IReadOnlyDictionary<string, string>? modules = null,
        ulong memoryLimit = 64 * 1024 * 1024, ulong stackLimit = 256 * 1024)
    {
        if (IntPtr.Size != 8 || !BitConverter.IsLittleEndian)
            throw new PlatformNotSupportedException("This product is qualified for little-endian Linux x64.");
        if (stackLimit < 64 * 1024 || stackLimit > 512 * 1024)
            throw new ArgumentOutOfRangeException(nameof(stackLimit), "Stack budget must be between 64 and 512 KiB.");
        this.modules = modules is null ? [] : new(modules, StringComparer.Ordinal);
        foreach (var name in this.modules.Keys) QuickJsText.CheckName(name);
        try
        {
            allocationHandle = GCHandle.Alloc(allocations);
            // Weak root permits finalization if an embedding forgets Dispose; values/contexts retain their owner.
            runtimeHandle = GCHandle.Alloc(new WeakReference<QuickJsRuntime>(this));
            VM.JSMallocFunctions allocator = new()
            {
                js_malloc = &QuickJsHost.Allocate, js_free = &QuickJsHost.Free,
                js_realloc = &QuickJsHost.Reallocate, js_malloc_usable_size = &QuickJsHost.UsableSize
            };
            Pointer = VM.JS_NewRuntime2(&allocator, (void*)GCHandle.ToIntPtr(allocationHandle));
            if (Pointer == null) throw new OutOfMemoryException("QuickJS runtime initialization failed.");
            VM.JS_SetMemoryLimit(Pointer, memoryLimit);
            VM.JS_SetMaxStackSize(Pointer, stackLimit);
            void* token = (void*)GCHandle.ToIntPtr(runtimeHandle);
            VM.JS_SetRuntimeOpaque(Pointer, token);
            VM.JS_SetInterruptHandler(Pointer, &Interrupt, token);
            VM.JS_SetModuleLoaderFunc(Pointer, null, &LoadModule, token);
            VM.JS_SetHostPromiseRejectionTracker(Pointer, &TrackRejection, token);
        }
        catch { Release(); throw; }
    }

    ~QuickJsRuntime()
    {
        try { Release(); }
        catch { /* Finalizers must never terminate the host process. Dispose reports failures. */ }
    }

    internal void Enter(CancellationToken token = default)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed || disposing, this);
            if (activeThread != 0) throw new InvalidOperationException("Concurrent entry and callback reentry into one QuickJS runtime are unsupported.");
            activeThread = Environment.CurrentManagedThreadId;
            cancellation = token;
            VM.JS_UpdateStackTop(Pointer);
        }
    }

    internal void Leave()
    {
        lock (gate) { activeThread = 0; cancellation = default; Monitor.PulseAll(gate); }
        GC.KeepAlive(this);
    }

    public QuickJsContext CreateContext()
    {
        Enter();
        try
        {
            var pointer = VM.JS_NewContext(Pointer);
            if (pointer == null) throw new OutOfMemoryException("QuickJS context initialization failed.");
            QuickJsContext? context = null;
            try
            {
                context = new QuickJsContext(this, pointer);
                contexts.Add(context);
                return context;
            }
            catch
            {
                if (context is null) VM.JS_FreeContext(pointer);
                else { context.Release(); context.ReleaseCallbackRoots(); }
                throw;
            }
        }
        finally { Leave(); }
    }

    /// <summary>Execute at most maxJobs pending Promise jobs; returns the number executed.</summary>
    public int DrainJobs(int maxJobs = 10000, CancellationToken cancellationToken = default)
    {
        if (maxJobs < 1) throw new ArgumentOutOfRangeException(nameof(maxJobs));
        Enter(cancellationToken);
        try
        {
            int count = 0;
            while (VM.JS_IsJobPending(Pointer) != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (count == maxJobs) throw new QuickJsException("Pending-job budget exhausted.");
                VM.JSContext* context = null;
                if (VM.JS_ExecutePendingJob(Pointer, &context) < 0) throw QuickJsContext.TakeException(context);
                count++;
            }
            if (rejections.Count != 0)
            {
                string message = string.Join("; ", rejections.Values.Select(value => value.Message));
                ClearRejections();
                throw new QuickJsException("Unhandled Promise rejection: " + message);
            }
            return count;
        }
        finally { Leave(); }
    }

    public void CollectGarbage()
    {
        Enter();
        try { VM.JS_RunGC(Pointer); }
        finally { Leave(); }
    }

    internal static QuickJsRuntime Owner(void* token)
    {
        var weak = (WeakReference<QuickJsRuntime>)GCHandle.FromIntPtr((nint)token).Target!;
        return weak.TryGetTarget(out var runtime) ? runtime : throw new ObjectDisposedException(nameof(QuickJsRuntime));
    }

    private static int Interrupt(VM.JSRuntime* runtime, void* opaque)
    {
        try { var owner = Owner(opaque); return owner.disposing || owner.cancellation.IsCancellationRequested ? 1 : 0; }
        catch { return 1; }
    }

    private static void TrackRejection(VM.JSContext* context, VM.JSValue promise, VM.JSValue reason, int handled, void* opaque)
    {
        try
        {
            var owner = Owner(opaque);
            nint key = (nint)promise.u.ptr;
            if (handled != 0)
            {
                if (owner.rejections.Remove(key, out var rejection)) VM.JS_FreeValueRT(owner.Pointer, rejection.Promise);
            }
            else
            {
                string message;
                try { message = QuickJsContext.String(context, reason); }
                catch { message = "Rejection reason could not be converted to text."; }
                if (owner.rejections.TryGetValue(key, out var prior))
                    owner.rejections[key] = prior with { Message = message };
                else
                {
                    owner.rejections.EnsureCapacity(owner.rejections.Count + 1);
                    owner.rejections.Add(key, new(VM.JS_DupValueRT(owner.Pointer, promise), message));
                }
            }
        }
        catch { /* A managed exception must not cross a translated callback boundary. */ }
    }

    private static VM.JSModuleDef* LoadModule(VM.JSContext* context, byte* name, void* opaque)
    {
        try
        {
            var owner = Owner(opaque);
            ulong length = 0;
            while (name[length] != 0) length++;
            string key = QuickJsText.Decode(name, length);
            if (!owner.modules.TryGetValue(key, out var source))
            {
                QuickJsContext.ThrowManaged(context, new QuickJsException($"Unknown in-memory module '{key}'."));
                return null;
            }
            owner.ModuleLoads++;
            byte[] bytes = QuickJsText.Encode(source);
            fixed (byte* input = bytes)
            {
                var result = VM.JS_Eval(context, (byte*)input, (ulong)(bytes.Length - 1), name, VM.JS_EVAL_TYPE_MODULE | VM.JS_EVAL_FLAG_COMPILE_ONLY);
                if (result.tag == 6) return null;
                var module = (VM.JSModuleDef*)result.u.ptr;
                VM.JS_FreeValue(context, result);
                return module;
            }
        }
        catch (Exception error) { QuickJsContext.ThrowManaged(context, error); return null; }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            if (activeThread == Environment.CurrentManagedThreadId)
                throw new InvalidOperationException("A callback cannot dispose its active runtime.");
            disposing = true;
            while (activeThread != 0) Monitor.Wait(gate);
            Release();
        }
        GC.SuppressFinalize(this);
        if (AllocatedBlocks != 0) throw new InvalidOperationException($"QuickJS leaked {AllocatedBlocks} owned allocations.");
    }

    private void Release()
    {
        if (disposed) return;
        disposed = true;
        ClearRejections();
        foreach (var context in contexts) context.Release();
        if (Pointer != null) { VM.JS_FreeRuntime(Pointer); Pointer = null; }
        foreach (var context in contexts) context.ReleaseCallbackRoots();
        contexts.Clear();
        if (runtimeHandle.IsAllocated) runtimeHandle.Free();
        if (allocationHandle.IsAllocated) allocationHandle.Free();
    }

    private void ClearRejections()
    {
        foreach (var rejection in rejections.Values) VM.JS_FreeValueRT(Pointer, rejection.Promise);
        rejections.Clear();
    }
}

internal static class QuickJsText
{
    internal static void CheckName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (name.Contains('\0')) throw new ArgumentException("This embedding name must not contain NUL.", nameof(name));
    }

    // Encode pairs as UTF-8 (including supplementary identifiers); retain lone
    // surrogates as WTF-8, which QuickJS accepts for JavaScript string contents.
    public static byte[] Encode(string text)
    {
        var result = new byte[checked(text.Length * 3 + 1)];
        int count = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                int scalar = char.ConvertToUtf32(c, text[++i]);
                result[count++] = (byte)(0xf0 | scalar >> 18);
                result[count++] = (byte)(0x80 | scalar >> 12 & 63);
                result[count++] = (byte)(0x80 | scalar >> 6 & 63);
                result[count++] = (byte)(0x80 | scalar & 63);
                continue;
            }
            if (c < 128) result[count++] = (byte)c;
            else if (c < 2048) { result[count++] = (byte)(0xc0 | c >> 6); result[count++] = (byte)(0x80 | c & 63); }
            else { result[count++] = (byte)(0xe0 | c >> 12); result[count++] = (byte)(0x80 | c >> 6 & 63); result[count++] = (byte)(0x80 | c & 63); }
        }
        Array.Resize(ref result, count + 1);
        return result;
    }

    public static unsafe string Decode(byte* pointer, ulong length)
    {
        if (length > int.MaxValue) throw new OverflowException("JavaScript string exceeds managed string capacity.");
        var bytes = new ReadOnlySpan<byte>(pointer, (int)length);
        var result = new StringBuilder(bytes.Length);
        for (int i = 0; i < bytes.Length;)
        {
            int first = bytes[i++];
            if (first < 128) result.Append((char)first);
            else if ((first & 0xe0) == 0xc0) result.Append((char)((first & 31) << 6 | bytes[i++] & 63));
            else if ((first & 0xf0) == 0xe0) { int second = bytes[i++]; result.Append((char)((first & 15) << 12 | (second & 63) << 6 | bytes[i++] & 63)); }
            else { int second = bytes[i++], third = bytes[i++], fourth = bytes[i++]; result.Append(char.ConvertFromUtf32((first & 7) << 18 | (second & 63) << 12 | (third & 63) << 6 | fourth & 63)); }
        }
        return result.ToString();
    }
}
