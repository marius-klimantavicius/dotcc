using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using C = DotCC.Libc.Libc;

public static unsafe class ZigHostExternRuntimeControl
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Iovec { public void* Base; public ulong Length; }
    private sealed class Utf8(string value) : IDisposable
    {
        public readonly nint Address = Marshal.StringToCoTaskMemUTF8(value);
        public byte* Pointer => (byte*)Address;
        public void Dispose() => Marshal.FreeCoTaskMem(Address);
    }
    private static int checks;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message + "; errno=" + C.errno);
        checks++;
    }
    [DllImport("libc", EntryPoint = "malloc_usable_size")]
    private static extern ulong NativeUsable(void* pointer);

    public static int Run(string disposableRoot)
    {
        checks = 0;
        string root = Path.Combine(disposableRoot, "runtime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "before"));
        using var first = new C.RuntimeContext();
        using var second = new C.RuntimeContext();
        try
        {
            using (first.Enter())
            using (var cwd = new Utf8(root))
            using (var directoryName = new Utf8("before"))
            using (var filename = new Utf8("data"))
            using (var missing = new Utf8("absent"))
            {
                Check(C.chdir(cwd.Pointer) == 0, "context cwd");
                int directory = C.__dotcc_zig_c_openat64(-100, directoryName.Pointer, 0x10000U, 0);
                Check(directory >= 3, "open guest directory");
                Directory.Move(Path.Combine(root, "before"), Path.Combine(root, "after"));
                int duplicate = C.dup(directory);
                Check(duplicate >= 3 && duplicate != directory, "duplicate directory");
                Check(C.close(directory) == 0, "close original directory");
                int file = C.__dotcc_zig_c_openat64(duplicate, filename.Pointer, 0x242U, 0x180U);
                Check(file >= 3 && File.Exists(Path.Combine(root, "after", "data")), "relative create after directory rename");
                byte* message = stackalloc byte[6] { 97, 98, 99, 100, 101, 102 };
                Check(C.write(file, message, 6) == 6, "write through owned descriptor");
                Check(C.__dotcc_zig_c_lseek64(file, 2, 0) == 2, "seek");
                Check(C.__dotcc_zig_c_lseek64(file, -1, 0) == -1 && C.errno == C.EINVAL, "negative seek errno");
                Check(C.__dotcc_zig_c_lseek64(file, 0, 99) == -1 && C.errno == C.EINVAL, "invalid seek whence errno");
                byte* result = stackalloc byte[4];
                Iovec* vectors = stackalloc Iovec[2];
                vectors[0] = new() { Base = result, Length = 2 };
                vectors[1] = new() { Base = result + 2, Length = 2 };
                Check(C.__dotcc_zig_c_preadv64(file, vectors, 2, 1) == 4 && result[0] == 98 && result[3] == 101,
                    "positioned vector contents");
                Check(C.__dotcc_zig_c_lseek64(file, 0, 1) == 2, "preadv preserves cursor");
                Check(C.__dotcc_zig_c_preadv64(file, vectors, uint.MaxValue, 0) == -1 && C.errno == C.EINVAL,
                    "unsigned vector count validation");
                void* mappedFile = C.__dotcc_zig_c_mmap64(null, 4096, 1, 2, file, 0);
                Check(mappedFile != (void*)(nint)(-1) && ((byte*)mappedFile)[0] == 97 && ((byte*)mappedFile)[5] == 102,
                    "file mmap translates guest descriptor");
                Check(C.munmap(mappedFile, 4096) == 0, "unmap file");
                Check(C.__dotcc_zig_c_openat64(duplicate, missing.Pointer, 0, 0) == -1 && C.errno == C.ENOENT,
                    "missing open errno");
                int* error = C.__dotcc_zig_c___errno_location();
                *error = 123;
                Check(C.errno == 123, "errno pointer and property share storage");
                nint originalError = (nint)error;
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                Check((nint)C.__dotcc_zig_c___errno_location() == originalError && *error == 123, "errno pointer survives GC");
                void* heap = C.malloc(17UL);
                Check(heap != null, "malloc");
                ulong capacity = C.__dotcc_zig_c_malloc_usable_size(heap);
                Check(capacity == NativeUsable(heap) && capacity >= 17, "actual allocator usable capacity");
                ((byte*)heap)[capacity - 1] = 77;
                Check(((byte*)heap)[capacity - 1] == 77, "usable capacity is writable");
                void* mapping = C.__dotcc_zig_c_mmap64(null, 4096, 3, 0x22, -1, 0);
                Check(mapping != (void*)(nint)(-1), "anonymous mapping");
                ((byte*)mapping)[0] = 42; ((byte*)mapping)[4095] = 99;
                using (second.Enter())
                {
                    Check(C.errno == 0 && (nint)C.__dotcc_zig_c___errno_location() != originalError, "context errno isolation");
                    Check(C.__dotcc_zig_c_openat64(duplicate, filename.Pointer, 0, 0) == -1 && C.errno == C.EBADF,
                        "foreign guest descriptor rejected");
                    Check(C.__dotcc_zig_c_mremap(mapping, 4096, 8192, 1, null) == (void*)(nint)(-1) && C.errno == C.EINVAL,
                        "foreign mapping rejected");
                    bool rejected = false;
                    try { C.__dotcc_zig_c_malloc_usable_size(heap); }
                    catch (InvalidOperationException) { rejected = true; }
                    Check(rejected, "foreign heap rejected");
                }
                Check(C.errno == 123, "nested context preserves errno");
                bool threadResult = false;
                Exception? threadFailure = null;
                var worker = new Thread(() =>
                {
                    try
                    {
                        using var binding = first.Enter();
                        bool isolated = C.errno == 0 && (nint)C.__dotcc_zig_c___errno_location() != originalError;
                        C.errno = 47;
                        threadResult = isolated && *C.__dotcc_zig_c___errno_location() == 47;
                    }
                    catch (Exception failure) { threadFailure = failure; }
                });
                worker.Start(); worker.Join();
                if (threadFailure is not null) throw new Exception("errno worker failed", threadFailure);
                Check(threadResult && C.errno == 123, "thread errno isolation");
                void* grown = C.__dotcc_zig_c_mremap(mapping, 4096, 8192, 1, null);
                Check(grown != (void*)(nint)(-1) && ((byte*)grown)[0] == 42 && ((byte*)grown)[4095] == 99, "mremap preserves bytes");
                ((byte*)grown)[8191] = 81;
                Check(((byte*)grown)[8191] == 81, "expanded mapping writable");
                void* shrunk = C.__dotcc_zig_c_mremap(grown, 8192, 4096, 0, null);
                Check(shrunk == grown && ((byte*)shrunk)[0] == 42, "mremap shrink");
                Check(C.__dotcc_zig_c_mremap(shrunk, 4096, 0, 0, null) == (void*)(nint)(-1) && C.errno == C.EINVAL,
                    "invalid remap preserves original");
                Check(C.munmap(shrunk, 4096) == 0, "unmap remapped owner interval");
                C.free(heap);
                Check(C.__dotcc_zig_c_malloc_usable_size(null) == 0, "null usable capacity");
                void* aligned = null;
                Check(C.posix_memalign(&aligned, 64, 73) == 0 && C.__dotcc_zig_c_malloc_usable_size(aligned) >= 73,
                    "aligned allocation capacity");
                C.free(aligned);
                Check(C.close(file) == 0, "close file");
                Check(C.__dotcc_zig_c_lseek64(file, 0, 0) == -1 && C.errno == C.EBADF, "closed seek errno");
                Check(C.__dotcc_zig_c_unlinkat(duplicate, filename.Pointer, 0) == 0, "relative unlink");
                File.CreateSymbolicLink(Path.Combine(root, "after", "dangling"), "absent-target");
                using (var dangling = new Utf8("dangling"))
                    Check(C.__dotcc_zig_c_unlinkat(duplicate, dangling.Pointer, 0) == 0, "unlink dangling symlink");
                Directory.CreateDirectory(Path.Combine(root, "after", "child"));
                using (var child = new Utf8("child"))
                    Check(C.__dotcc_zig_c_unlinkat(duplicate, child.Pointer, 0x200) == 0, "unlinkat directory");
                Check(C.__dotcc_zig_c_unlinkat(duplicate, missing.Pointer, 1) == -1 && C.errno == C.EINVAL, "unlink invalid flags");
                Check(C.close(duplicate) == 0, "last directory close");
                Check(C.__dotcc_zig_c_openat64(duplicate, missing.Pointer, 0, 0) == -1 && C.errno == C.EBADF,
                    "closed guest descriptor rejected");
            }
            Console.WriteLine(checks + " ordinary I/O, ownership, errno and memory checks passed");
            return 42;
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
