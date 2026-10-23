using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class MemoryResidencyTests
{
    private static long LockedKb() => long.Parse(File.ReadLines("/proc/self/status")
        .Single(line => line.StartsWith("VmLck:", StringComparison.Ordinal))
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)[1]);

    [Fact]
    public void Page_locking_and_dump_advice_have_observable_kernel_effects()
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) return;
        ulong size = (ulong)Environment.SystemPageSize;
        void* memory = mmap(null, size, 3, 0x22, -1, 0);
        ((nint)memory).ShouldNotBe(-1);
        try
        {
            madvise(memory, size, 16).ShouldBe(0); // MADV_DONTDUMP
            madvise((byte*)memory + 1, size - 1, 16).ShouldBe(-1);
            errno.ShouldBe(EINVAL);
            long before = LockedKb();
            mlock(memory, size).ShouldBe(0);
            try { LockedKb().ShouldBeGreaterThanOrEqualTo(before + (long)size / 1024); }
            finally { munlock(memory, size).ShouldBe(0); }
            LockedKb().ShouldBe(before);
            madvise(memory, size, 17).ShouldBe(0); // MADV_DODUMP
        }
        finally { munmap(memory, size).ShouldBe(0); }
    }
}
