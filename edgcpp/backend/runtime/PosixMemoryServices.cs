#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    // These report host RAM, not GC heap limits or container memory budgets.
    // Linux uses its matching sysconf selector; other platforms convert host
    // memory bytes into pages without exposing their structures to translated C.
    private long PhysicalPages()
    {
        if (OperatingSystem.IsLinux())
        {
            long pages = MemorySysconfUnix(85);
            int error = Marshal.GetLastPInvokeError();
            if (pages < 0 && error != 0)
                SetError(PosixPlatform.NativeErrorNumber(error));
            return pages;
        }
        ulong bytes;
        if (OperatingSystem.IsMacOS())
        {
            bytes = 0;
            nuint size = sizeof(ulong);
            ReadOnlySpan<byte> name = "hw.memsize\0"u8;
            fixed (byte* key = name)
                if (MemorySysctlDarwin(key, &bytes, &size, null, 0) != 0)
                    return MemoryServiceError();
            if (size != sizeof(ulong)) { SetError(5); return -1; }
        }
        else if (OperatingSystem.IsWindows())
        {
            MemoryStatusWindows status = new() { Length = (uint)sizeof(MemoryStatusWindows) };
            if (MemoryStatusQueryWindows(&status) == 0) return MemoryServiceError();
            bytes = status.TotalPhysical;
        }
        else { SetError(95); return -1; }
        ulong count = bytes / (uint)Environment.SystemPageSize;
        if (count > long.MaxValue) { SetError(75); return -1; }
        return (long)count;
    }

    public int Mlock(void* address, ulong size)
    {
        if (!ValidateMemoryRange(address, size)) return -1;
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            return MemoryLockUnix(address, (nuint)size) == 0 ? 0 : MemoryServiceError();
        if (!OperatingSystem.IsWindows()) { SetError(95); return -1; }
        if (size == 0) return 0;
        return MemoryLockWindows(address, (nuint)size) != 0 ? 0 : MemoryServiceError();
    }

    public int Madvise(void* address, ulong length, int advice)
    {
        nuint page = (nuint)Environment.SystemPageSize, start = (nuint)address;
        if (advice < 0 || advice > 4 || start % page != 0 ||
            length > (ulong)nuint.MaxValue - page + 1)
        { SetError(22); return -1; }
        nuint size = ((nuint)length + page - 1) & ~(page - 1);
        if (!ValidateMemoryRange(address, (ulong)size)) return -1;
        if (OperatingSystem.IsLinux() || (OperatingSystem.IsMacOS() && advice != 4))
            return MemoryAdviseUnix(address, size, advice) == 0 ? 0 : MemoryServiceError();
        if ((!OperatingSystem.IsWindows() && !OperatingSystem.IsMacOS()) || advice != 4)
        { SetError(95); return -1; }
        if (size == 0) return 0;
        lock (mappingGate)
        {
            if (mappingsClosed) { SetError(9); return -1; }
            nuint end = start + size, covered = 0;
            foreach (Mapping item in mappings)
            {
                nuint lo = start > item.Address ? start : item.Address;
                nuint itemEnd = item.Address + item.Length;
                nuint hi = end < itemEnd ? end : itemEnd;
                if (lo >= hi) continue;
                // Only owned, writable, private anonymous mappings can be
                // zeroed without modifying files, aliases or protected memory.
                if (!item.PrivateAnonymous || (item.Protection & 2) == 0)
                { SetError(95); return -1; }
                covered += hi - lo;
            }
            if (covered != size) { SetError(12); return -1; }
            // Darwin's DONTNEED and Windows's MEM_RESET do not guarantee zero
            // contents. Clear first, then issue real discard advice. Retained
            // bytes and fresh zero-fill pages now agree with Linux DONTNEED.
            // This O(n) fallback preserves the mapping and its commit guarantee;
            // decommit/recommit could leave it inaccessible if recommit failed.
            NativeMemory.Clear(address, size);
            if (OperatingSystem.IsMacOS())
                return MemoryAdviseUnix(address, size, 5 /* MADV_FREE */) == 0 ? 0 : MemoryServiceError();
            // A coalesced request can span adjacent views; VirtualAlloc operates
            // within each native allocation, so issue one reset per view.
            foreach (Mapping item in mappings)
            {
                nuint lo = start > item.Address ? start : item.Address;
                nuint itemEnd = item.Address + item.Length;
                nuint hi = end < itemEnd ? end : itemEnd;
                if (lo < hi && MemoryResetWindows((void*)lo, hi - lo,
                    0x80000 /* MEM_RESET */, 1 /* PAGE_NOACCESS, ignored */) == null)
                    return MemoryServiceError();
            }
            return 0;
        }
    }

    public int Munlock(void* address, ulong size)
    {
        if (!ValidateMemoryRange(address, size)) return -1;
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            return MemoryUnlockUnix(address, (nuint)size) == 0 ? 0 : MemoryServiceError();
        if (!OperatingSystem.IsWindows()) { SetError(95); return -1; }
        if (size == 0) return 0;
        // POSIX allows unlocking mapped pages which are already unlocked.
        // Windows reports ERROR_NOT_LOCKED for such ranges. First reject holes,
        // then use a single fast call; on a mixed range finish each page so all
        // locks are released regardless of the Windows bulk-call failure.
        nuint start = (nuint)address, end = start + (nuint)size;
        for (nuint cursor = start; cursor < end;)
        {
            MemoryRegionWindows info;
            if (MemoryQueryWindows((void*)cursor, &info, (nuint)sizeof(MemoryRegionWindows)) == 0)
                return MemoryServiceError();
            if (info.State != 0x1000 || info.RegionSize == 0 || info.RegionSize > nuint.MaxValue - info.BaseAddress)
            { SetError(12); return -1; }
            nuint next = info.BaseAddress + info.RegionSize;
            if (next <= cursor) { SetError(12); return -1; }
            cursor = next;
        }
        if (MemoryUnlockWindows(address, (nuint)size) != 0) return 0;
        if (Marshal.GetLastPInvokeError() != 158) return MemoryServiceError();
        nuint pageSize = (nuint)Environment.SystemPageSize;
        for (nuint page = start - start % pageSize; page < end;)
        {
            if (MemoryUnlockWindows((void*)page, pageSize) == 0 && Marshal.GetLastPInvokeError() != 158)
                return MemoryServiceError();
            if (pageSize > nuint.MaxValue - page) break;
            page += pageSize;
        }
        return 0;
    }

    private bool ValidateMemoryRange(void* address, ulong size)
    {
        if (size > (ulong)nuint.MaxValue - (ulong)(nuint)address)
        { SetError(22); return false; }
        return true;
    }

    private int MemoryServiceError()
    {
        int error = Marshal.GetLastPInvokeError();
        // Invalid/unmapped addresses and exhausted lock quotas are ENOMEM in
        // the portable ABI. Windows's default generic errno map is insufficient.
        SetError(OperatingSystem.IsWindows() && error is 487 or 998 or 1453 or 1455
            ? 12 : PosixPlatform.NativeErrorNumber(error));
        return -1;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusWindows
    {
        internal uint Length, Load;
        internal ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile;
        internal ulong TotalVirtual, AvailableVirtual, AvailableExtendedVirtual;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryRegionWindows
    {
        internal nuint BaseAddress, AllocationBase;
        internal uint AllocationProtect;
        internal ushort PartitionId;
        internal nuint RegionSize;
        internal uint State, Protect, Type;
    }

    [LibraryImport("libc", EntryPoint = "sysconf", SetLastError = true)]
    private static partial long MemorySysconfUnix(int name);
    [LibraryImport("libc", EntryPoint = "sysctlbyname", SetLastError = true)]
    private static partial int MemorySysctlDarwin(byte* name, void* oldValue, nuint* oldSize, void* newValue, nuint newSize);
    [LibraryImport("libc", EntryPoint = "mlock", SetLastError = true)]
    private static partial int MemoryLockUnix(void* address, nuint size);
    [LibraryImport("libc", EntryPoint = "munlock", SetLastError = true)]
    private static partial int MemoryUnlockUnix(void* address, nuint size);
    [LibraryImport("libc", EntryPoint = "madvise", SetLastError = true)]
    private static partial int MemoryAdviseUnix(void* address, nuint size, int advice);
    [LibraryImport("kernel32.dll", EntryPoint = "GlobalMemoryStatusEx", SetLastError = true)]
    private static partial int MemoryStatusQueryWindows(MemoryStatusWindows* status);
    [LibraryImport("kernel32.dll", EntryPoint = "VirtualLock", SetLastError = true)]
    private static partial int MemoryLockWindows(void* address, nuint size);
    [LibraryImport("kernel32.dll", EntryPoint = "VirtualUnlock", SetLastError = true)]
    private static partial int MemoryUnlockWindows(void* address, nuint size);
    [LibraryImport("kernel32.dll", EntryPoint = "VirtualQuery", SetLastError = true)]
    private static partial nuint MemoryQueryWindows(void* address, MemoryRegionWindows* information, nuint size);
    [LibraryImport("kernel32.dll", EntryPoint = "VirtualAlloc", SetLastError = true)]
    private static partial void* MemoryResetWindows(void* address, nuint size, uint allocationType, uint protection);
}
