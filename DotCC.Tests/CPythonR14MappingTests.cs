using System;
using System.IO;
using System.Linq;
using System.Text;
using static DotCC.Libc.Libc;
#if !MAPPING_DIRECT
using Xunit;
#endif

namespace DotCC.Tests;

#if !MAPPING_DIRECT
[Collection("Runtime")]
public sealed class CPythonR14MappingTests
{
    [Fact]
    public void Kernel_storage_identity_tracks_partial_mappings_and_owner_lifetime()
        => CPythonR14MappingChecks.Run();
}
#endif

public static unsafe class CPythonR14MappingChecks
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    // This observes kernel VMAs, not the service's private ownership registry.
    // The unique live file also prevents an unrelated anonymous/JIT mapping
    // at the same address from being mistaken for retained backing storage.
    private static bool Mapped(string path) => File.ReadLines("/proc/self/maps")
        .Any(line => line.EndsWith(" " + path, StringComparison.Ordinal));

    public static void Run()
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) return;
        string path = Path.GetTempFileName();
        using var owner = new RuntimeContext();
        using var other = new RuntimeContext();
        ulong page = (ulong)Environment.SystemPageSize;
        try
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                file.SetLength((long)(page * 3));
            Check(!Mapped(path), "unmapped file reported as mapped");
            byte* memory;
            using (owner.Enter())
            {
                int fd;
                fixed (byte* name = Encoding.UTF8.GetBytes(path + "\0")) fd = open(name, 2, 0);
                Check(fd >= 0, "open");
                memory = (byte*)mmap(null, page * 3, 3, 1, fd, 0);
                Check((nint)memory != -1, "file mmap");
                memory[0] = 42; memory[page] = 17; memory[page * 2] = 91;
                Check(Mapped(path), "retained-map positive oracle control");
                Check(close(fd) == 0, "close mapped descriptor");
                Check(Mapped(path) && memory[page] == 17, "close must retain file mapping");
                using (owner.Enter())
                {
                    bool rejected = false;
                    try { owner.Dispose(); } catch (InvalidOperationException) { rejected = true; }
                    Check(rejected && Mapped(path), "recursive active owner disposal");
                }
                using (other.Enter())
                {
                    Check(munmap(memory + page, page) == -1 && errno == EINVAL,
                        "foreign partial unmap");
                    Check(memory[page] == 17 && Mapped(path), "foreign unmap changed storage");
                }
                Check(munmap(memory + page, page) == 0, "split mapping");
                Check(memory[0] == 42 && memory[page * 2] == 91 && Mapped(path),
                    "partial unmap removed surviving intervals");
                Check(munmap(memory + page, page) == 0, "overlapping hole unmap");
                Check(memory[0] == 42 && memory[page * 2] == 91, "hole unmap changed live bytes");
            }
            using (other.Enter())
            {
                byte* independent = (byte*)mmap(null, page, 3, 0x22, -1, 0);
                Check((nint)independent != -1, "second owner mmap");
                independent[0] = 63;
                owner.Dispose();
                Check(!Mapped(path), "owner disposal retained file-backed interval");
                Check(independent[0] == 63, "owner disposal affected ambient owner");
                owner.Dispose();
                Check(independent[0] == 63 && munmap(independent, page) == 0,
                    "repeated disposal affected ambient owner");
            }
            Check(File.ReadAllBytes(path)[0] == 42 && File.ReadAllBytes(path)[(int)(page * 2)] == 91,
                "shared mapping bytes did not reach backing file");
            bool disposed = false;
            try { using var binding = owner.Enter(); } catch (ObjectDisposedException) { disposed = true; }
            Check(disposed, "disposed owner accepted entry");
        }
        finally { File.Delete(path); }
    }
}
