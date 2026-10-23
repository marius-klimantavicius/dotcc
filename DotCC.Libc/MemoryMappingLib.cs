#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Real Linux page residency and dump advice. These operate on
    /// caller-owned addresses; failure preserves the kernel errno.</summary>
    public static int madvise(void* address, ulong length, int advice)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) { errno = ENOTSUP; return -1; }
        int result = PosixMadvise(address, length, advice);
        if (result != 0) errno = Marshal.GetLastPInvokeError();
        return result;
    }

    public static int mlock(void* address, ulong length)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) { errno = ENOTSUP; return -1; }
        int result = PosixMlock(address, length);
        if (result != 0) errno = Marshal.GetLastPInvokeError();
        return result;
    }

    public static int munlock(void* address, ulong length)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) { errno = ENOTSUP; return -1; }
        int result = PosixMunlock(address, length);
        if (result != 0) errno = Marshal.GetLastPInvokeError();
        return result;
    }

    private readonly record struct PosixMapping(ulong Start, ulong End, RuntimeContext? Owner);
    private static readonly Lock mappingGate = new();
    private static readonly List<PosixMapping> posixMappings = new();

    /// <summary>Linux LP64 mapping service, including real anonymous pymalloc
    /// arenas. File descriptors are translated through the owner's descriptor
    /// table; they are never mistaken for OS descriptor numbers. MAP_FIXED is
    /// unsupported because it could overwrite the managed runtime's storage.</summary>
    public static void* mmap(void* address, ulong length, int protection, int flags, int descriptor, long offset)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8 || (flags & 0x10) != 0)
        { errno = ENOTSUP; return (void*)(nint)(-1); }
        bool anonymous = (flags & 0x20) != 0;
        FileStream? file = anonymous ? null : SlotByFd(descriptor)?.Stream as FileStream;
        if (!anonymous && file is null) { errno = EBADF; return (void*)(nint)(-1); }
        bool retained = false;
        try
        {
            if (file != null) file.SafeFileHandle.DangerousAddRef(ref retained);
            int nativeFd = file is null ? -1 : checked((int)file.SafeFileHandle.DangerousGetHandle());
            lock (mappingGate)
            {
                posixMappings.EnsureCapacity(checked(posixMappings.Count + 1));
                void* result = PosixMmap(address, length, protection, flags, nativeFd, offset);
                if (result == (void*)(nint)(-1)) { errno = Marshal.GetLastPInvokeError(); return result; }
                ulong rounded = RoundMappingLength(length);
                posixMappings.Add(new((ulong)result, (ulong)result + rounded, RuntimeContext.Current));
                return result;
            }
        }
        catch (ObjectDisposedException) { errno = EBADF; return (void*)(nint)(-1); }
        catch (OutOfMemoryException) { errno = ENOMEM; return (void*)(nint)(-1); }
        finally { if (retained) file!.SafeFileHandle.DangerousRelease(); }
    }

    private static ulong RoundMappingLength(ulong length)
    {
        ulong page = (ulong)Environment.SystemPageSize;
        return (length + page - 1) & ~(page - 1);
    }

    /// <summary>Real page unmapping. Partial unmaps retain the remaining owned
    /// intervals for deterministic context disposal and foreign-owner checks.</summary>
    public static int munmap(void* address, ulong length)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) { errno = ENOTSUP; return -1; }
        ulong start = (ulong)address, rounded = RoundMappingLength(length);
        if (length == 0 || rounded == 0 || start > ulong.MaxValue - rounded
            || start % (ulong)Environment.SystemPageSize != 0) { errno = EINVAL; return -1; }
        ulong end = start + rounded;
        lock (mappingGate)
        {
            foreach (var mapping in posixMappings)
                if (mapping.Start < end && start < mapping.End && !ReferenceEquals(mapping.Owner, RuntimeContext.Current))
                { errno = EINVAL; return -1; }
            try { posixMappings.EnsureCapacity(checked(posixMappings.Count + 1)); }
            catch (OutOfMemoryException) { errno = ENOMEM; return -1; }
            int result = PosixMunmap(address, length);
            if (result != 0) { errno = Marshal.GetLastPInvokeError(); return result; }
            for (int i = posixMappings.Count - 1; i >= 0; i--)
            {
                var mapping = posixMappings[i];
                if (mapping.Start >= end || start >= mapping.End) continue;
                posixMappings.RemoveAt(i);
                if (mapping.Start < start) posixMappings.Add(mapping with { End = start });
                if (mapping.End > end) posixMappings.Add(mapping with { Start = end });
            }
            return 0;
        }
    }

    private static void DisposeOwnedMappings(RuntimeContext owner)
    {
        lock (mappingGate)
            for (int i = posixMappings.Count - 1; i >= 0; i--)
                if (ReferenceEquals(posixMappings[i].Owner, owner))
                {
                    var mapping = posixMappings[i];
                    PosixMunmap((void*)mapping.Start, mapping.End - mapping.Start);
                    posixMappings.RemoveAt(i);
                }
    }

    [DllImport("libc", EntryPoint = "mmap", SetLastError = true)]
    private static extern void* PosixMmap(void* address, ulong length, int protection, int flags, int descriptor, long offset);
    [DllImport("libc", EntryPoint = "munmap", SetLastError = true)]
    private static extern int PosixMunmap(void* address, ulong length);
    [DllImport("libc", EntryPoint = "madvise", SetLastError = true)]
    private static extern int PosixMadvise(void* address, ulong length, int advice);
    [DllImport("libc", EntryPoint = "mlock", SetLastError = true)]
    private static extern int PosixMlock(void* address, ulong length);
    [DllImport("libc", EntryPoint = "munlock", SetLastError = true)]
    private static extern int PosixMunlock(void* address, ulong length);
}
