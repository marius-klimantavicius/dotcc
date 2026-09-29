using System.Runtime.InteropServices;
namespace Managed.Libuv.Probes;
// These imports are Linux glibc-only probe material, loaded only when invoked.
public static unsafe partial class NativeBoundary
{
    [LibraryImport("libc.so.6", EntryPoint = "pipe")]
    public static partial int Pipe(int* fds);
    [LibraryImport("libc.so.6", EntryPoint = "close", SetLastError = true)]
    private static partial int NativeClose(int fd);
    [System.ThreadStatic] public static int CapturedCloseError;
    public static int Close(int fd)
    {
        int result = NativeClose(fd);
        CapturedCloseError = Marshal.GetLastPInvokeError();
        return result;
    }
    [LibraryImport("libc.so.6", EntryPoint = "read")]
    public static partial long Read(int fd, void* data, ulong size);
    [LibraryImport("libc.so.6", EntryPoint = "write")]
    public static partial long Write(int fd, void* data, ulong size);
    [LibraryImport("libc.so.6", EntryPoint = "__errno_location")]
    public static partial int* ErrnoLocation();
}

public static unsafe partial class NativeThreadBoundary
{
    private static readonly System.Threading.ManualResetEventSlim Entered = new(false);
    private static readonly System.Threading.ManualResetEventSlim Release = new(false);
    [LibraryImport("libc.so.6", EntryPoint = "pthread_create")]
    private static partial int Create(ulong* thread, void* attributes,
        delegate* unmanaged[Cdecl]<void*, void*> start, void* context);
    [LibraryImport("libc.so.6", EntryPoint = "pthread_join")]
    private static partial int Join(ulong thread, void** result);

    // Exceptions never cross the reverse P/Invoke boundary. Context remains in
    // unmanaged storage until pthread_join establishes that the callback ended.
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static void* Start(void* context)
    {
        try
        {
            Entered.Set();
            Release.Wait();
            return Probe.translated_worker(context);
        }
        catch { return null; }
    }

    public static int Run()
    {
        int* context = (int*)NativeMemory.Alloc((nuint)sizeof(int));
        *context = 41;
        ulong thread;
        int error = Create(&thread, null, &Start, context);
        if (error != 0) { NativeMemory.Free(context); return 10; }
        if (!Entered.Wait(System.TimeSpan.FromSeconds(10)))
        {
            Release.Set();
            return 13;
        }
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        Release.Set();
        void* result;
        error = Join(thread, &result);
        // A failed join does not establish callback completion; avoid freeing
        // storage a native thread could still reach, and fail the probe.
        if (error != 0) return 11;
        int status = result == context && *context == 42 ? 0 : 12;
        NativeMemory.Free(context);
        return status;
    }
}
