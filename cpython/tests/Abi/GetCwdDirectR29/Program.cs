using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using L = DotCC.Libc.Libc;

internal static unsafe class Program
{
    private static int assertions, escaped;
    private static void Check(bool ok, string message)
    { assertions++; if (!ok) throw new Exception(message); }

    [DllImport("b29_getcwd_contract", CallingConvention = CallingConvention.Cdecl)]
    private static extern int b29_getcwd_contract(byte* root,
        delegate* unmanaged[Cdecl]<byte*, ulong, byte*> cwd,
        delegate* unmanaged[Cdecl]<byte*, int> change,
        delegate* unmanaged[Cdecl]<void*, void> release,
        delegate* unmanaged[Cdecl]<int> error,
        delegate* unmanaged[Cdecl]<int, void> setError, int virtualOwner);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static byte* Cwd(byte* buffer, ulong size)
    {
        try { return L.getcwd(buffer, size); }
        catch (Exception error) { escaped++; Console.Error.WriteLine("ESCAPED " + error.GetType().FullName); L.errno = L.EIO; return null; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Change(byte* path) => L.chdir(path);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Release(void* pointer) => L.free(pointer);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Error() => L.errno;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void SetError(int value) => L.errno = value;

    private static int SameC(string path, int owner)
    {
        fixed (byte* root = Encoding.UTF8.GetBytes(path + "\0"))
            return b29_getcwd_contract(root, &Cwd, &Change, &Release, &Error, &SetError, owner);
    }
    private static string ReadCwd()
    {
        byte* path = L.getcwd(null, 0);
        if (path == null) throw new Exception("cwd errno=" + L.errno);
        try { return Marshal.PtrToStringUTF8((nint)path)!; }
        finally { L.free(path); }
    }
    private static void ChangeTo(string path)
    {
        fixed (byte* bytes = Encoding.UTF8.GetBytes(path + "\0"))
            Check(L.chdir(bytes) == 0, "chdir " + path);
    }
    private static void OwnerChecks(string root)
    {
        string process = Directory.GetCurrentDirectory();
        using var first = new L.RuntimeContext();
        using var second = new L.RuntimeContext();
        Directory.CreateDirectory(root);
        using (first.Enter())
        {
            ChangeTo(root);
            byte* allocation = L.getcwd(null, 4096);
            Check(allocation != null && first.NativeAllocations.Contains((nuint)allocation), "getcwd allocation registered to actual owner");
            using (second.Enter())
            {
                Check(ReadCwd() == process, "second owner cwd isolation");
                bool rejected = false;
                try { L.free(allocation); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected, "foreign allocator ownership rejected");
            }
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            Check(ReadCwd() == root, "cwd and allocation survive compacting GC");
            Check(Marshal.PtrToStringUTF8((nint)allocation) == root, "owned output survives GC");
            L.free(allocation);
            Check(first.NativeAllocations.Count == 0, "all returned allocations freed");
            bool blocked = false;
            try { first.Dispose(); } catch (InvalidOperationException) { blocked = true; }
            Check(blocked, "active binding blocks disposal");
        }
        Check(Directory.GetCurrentDirectory() == process, "virtual cwd never changes process cwd");
        Exception? workerError = null;
        var worker = new Thread(() =>
        {
            try
            {
                Check(L.RuntimeContext.Current is null, "explicit binding does not flow to worker");
                Check(ReadCwd() == process, "unbound worker uses process cwd");
                using (first.Enter()) { L.errno = 74; Check(ReadCwd() == root && L.errno == 74, "explicit worker entry uses owner cwd and errno"); }
            }
            catch (Exception error) { workerError = error; }
        });
        worker.Start(); Check(worker.Join(TimeSpan.FromSeconds(5)), "worker completes");
        if (workerError != null) throw workerError;
        Check(L.RuntimeContext.Current is null, "worker binding does not change caller");
        using (first.Enter()) { Check(L.errno != 74, "errno isolated by thread"); L.getcwd(null, 1024); }
        Check(first.NativeAllocations.Count == 1, "unfreed getcwd owned for disposal");
        using (second.Enter()) first.Dispose();
        Check(first.NativeAllocations.Count == 0, "disposal reclaims getcwd through captured owner");
        bool disposed = false;
        try { first.Enter(); } catch (ObjectDisposedException) { disposed = true; }
        Check(disposed, "disposed owner cannot reenter");
    }
    private static int Main(string[] args)
    {
        int result = SameC(args[0] + "-process-λ", 0);
        using (var owner = new L.RuntimeContext())
        using (owner.Enter()) result |= SameC(args[0] + "-owner-λ", 1);
        Console.WriteLine($"current-Libc callbacks escaped={escaped}; dynamicCode={RuntimeFeature.IsDynamicCodeSupported}");
        if (result != 0 || escaped != 0) return 29;
        OwnerChecks(args[0] + "-isolation-λ");
        Console.WriteLine($"current-Libc owner/GC/thread/disposal assertions={assertions}");
        return 0;
    }
}
