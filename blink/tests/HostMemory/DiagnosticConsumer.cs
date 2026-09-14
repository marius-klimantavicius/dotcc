using MemoryApi = Managed.Emulation.DiagnosticHost;

if (!global::System.BitConverter.IsLittleEndian)
    throw new global::System.PlatformNotSupportedException("Qualified diagnostic profile requires little-endian storage");
string Mask() => global::System.Array.Find(
    global::System.IO.File.ReadAllLines("/proc/thread-self/status"),
    line => line.StartsWith("SigBlk:", global::System.StringComparison.Ordinal));
var initial = Mask();
int result = MemoryApi.DiagnosticSelfTest();
unsafe
{
    if (MemoryApi.BlinkHostMemoryBegin(1024 * 1024) != 0) throw new global::System.Exception("owner begin failed");
    void* allocation = MemoryApi.blink_host_mmap(null, 4096, 3, 34, -1, 0);
    if (allocation == (void*)(-1)) throw new global::System.Exception("owner allocation failed");
    nuint address = (nuint)allocation;
    global::System.Exception failure = null;
    var worker = new global::System.Threading.Thread(() =>
    {
        try
        {
            if (MemoryApi.BlinkHostMemoryBegin(1024 * 1024) != 0) throw new global::System.Exception("second owner begin failed");
            if (MemoryApi.BlinkHostMemoryContains((void*)address, 8) != 0 ||
                MemoryApi.ReadWordSafely(2, (byte*)address) != 0x6660666066660666L)
                throw new global::System.Exception("diagnostic crossed owner boundary");
            if (MemoryApi.BlinkHostMemoryEnd() != 0) throw new global::System.Exception("second owner end failed");
        }
        catch (global::System.Exception error) { failure = error; }
    });
    worker.Start();
    global::System.GC.Collect(global::System.GC.MaxGeneration,
        global::System.GCCollectionMode.Forced, blocking: true, compacting: true);
    worker.Join();
    if (failure != null) throw failure;
    if (MemoryApi.BlinkHostMemoryContains(allocation, 4096) != 1 ||
        MemoryApi.ReadWordSafely(2, (byte*)allocation) != 0)
        throw new global::System.Exception("owner allocation changed");
    if (MemoryApi.blink_host_munmap(allocation, 4096) != 0 || MemoryApi.BlinkHostMemoryEnd() != 0)
        throw new global::System.Exception("owner cleanup failed");
}
if (initial == null || initial != Mask())
    throw new global::System.Exception("Diagnostic changed the real host mask");
return result;
