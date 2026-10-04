#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    private sealed class Mapping
    {
        internal nuint Address, Length;
        // Windows requires the original view address, including alignment padding.
        internal void* WindowsView;
        internal Mapping(nuint address, nuint length, void* windowsView = null)
        { Address = address; Length = length; WindowsView = windowsView; }
    }

    private readonly object mappingGate = new();
    private readonly List<Mapping> mappings = new();
    private bool mappingsClosed;
    private long mappingCreatedCount;
    public long MappingCreatedCount { get { lock (mappingGate) return mappingCreatedCount; } }
    public int MappedViewCount { get { lock (mappingGate) return mappings.Count; } }

    /// <summary>Portable PROT_READ/WRITE and MAP_SHARED/PRIVATE/ANONYMOUS.
    /// MAP_FIXED and executable mappings are intentionally unsupported.</summary>
    public void* Mmap(void* address, nuint length, int protection, int flags, int fd, long offset)
    {
        const int shared = 1, privateMap = 2, anonymous = 0x20;
        int sharing = flags & (shared | privateMap);
        if (length == 0 || offset < 0 || offset % Environment.SystemPageSize != 0 ||
            sharing != shared && sharing != privateMap || (protection & ~3) != 0)
            return MapError(22);
        if ((flags & ~(shared | privateMap | anonymous)) != 0) return MapError(95);
        nuint page = (nuint)Environment.SystemPageSize;
        if (length > (nuint)nint.MaxValue - page + 1) return MapError(12);
        nuint rounded = (length + page - 1) & ~(page - 1);
        bool isAnonymous = (flags & anonymous) != 0;
        Descriptor? descriptor = isAnonymous ? null : Find(fd);
        if (!isAnonymous && descriptor is null) return (void*)(-1);
        // mmap retains its own kernel reference to the file. Closing the source
        // descriptor later does not invalidate either a Unix or Windows view.
        lock (mappingGate)
        {
            if (mappingsClosed) return MapError(9);
            if (descriptor is null) return CreateMapping(address, length, rounded, protection, flags, null, offset);
            lock (descriptor.Gate)
            {
                if (descriptor.Closed) return MapError(9);
                if (descriptor.Device != 0) return MapError(19);
                if ((descriptor.Flags & 3) == 1 ||
                    ((protection & 2) != 0 && sharing == shared && (descriptor.Flags & 3) != 2))
                    return MapError(13);
                return CreateMapping(address, length, rounded, protection, flags, descriptor, offset);
            }
        }
    }

    private void* CreateMapping(void* address, nuint length, nuint rounded, int protection,
        int flags, Descriptor? descriptor, long offset)
    {
        try
        {
            // Reserve tracking capacity before acquiring an unmanaged resource.
            mappings.EnsureCapacity(checked(mappings.Count + 1));
            if (OperatingSystem.IsWindows())
            {
                // Windows file mappings must not silently extend the underlying
                // file as CreateFileMapping normally does. POSIX mmap never grows it.
                if (descriptor is not null && (ulong)offset + (ulong)length >
                    (ulong)System.IO.RandomAccess.GetLength(descriptor.Handle)) return MapError(22);
                if (address != null || protection == 0) return MapError(95);
                bool writable = (protection & 2) != 0, copy = descriptor is not null && writable && (flags & 2) != 0;
                uint protect = copy ? 8U : writable ? 4U : 2U;
                ulong maximum = descriptor is null ? (ulong)rounded : 0;
                nint handle = CreateFileMapping(descriptor?.Handle.DangerousGetHandle() ?? -1,
                    null, protect, (uint)(maximum >> 32), (uint)maximum, null);
                if (handle == 0) return WindowsMapError();
                try
                {
                    // Windows view offsets use allocation granularity (64 KiB),
                    // whereas the portable ABI accepts page-aligned offsets.
                    ulong viewOffset = (ulong)offset & ~65535UL;
                    nuint delta = (nuint)((ulong)offset - viewOffset);
                    if (rounded > (nuint)nint.MaxValue - delta) return MapError(12);
                    void* view = MapViewOfFile(handle, copy ? 1U : writable ? 2U : 4U,
                        (uint)(viewOffset >> 32), (uint)viewOffset, checked(length + delta));
                    if (view == null) return WindowsMapError();
                    nuint result = (nuint)view + delta;
                    try { mappings.Add(new Mapping(result, rounded, view)); }
                    catch { UnmapViewOfFile(view); throw; }
                    mappingCreatedCount++;
                    return (void*)result;
                }
                finally { CloseMappingHandle(handle); }
            }
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS()) return MapError(95);
            int nativeFlags = flags;
            if (OperatingSystem.IsMacOS() && (flags & 0x20) != 0)
                nativeFlags = (flags & ~0x20) | 0x1000;
            void* resultUnix = MapUnix(address, length, protection, nativeFlags,
                descriptor is null ? -1 : checked((int)descriptor.Handle.DangerousGetHandle()), offset);
            if (resultUnix == (void*)(-1)) return MapError(MappingUnixError());
            try { mappings.Add(new Mapping((nuint)resultUnix, rounded)); }
            catch { UnmapUnix(resultUnix, rounded); throw; }
            mappingCreatedCount++;
            return resultUnix;
        }
        catch (Exception error) { Fail(error); return (void*)(-1); }
    }

    /// <summary>Unmaps an owned mapping range. Unix supports partial ranges;
    /// Windows rejects partial views with ENOTSUP rather than losing live bytes.</summary>
    public int Munmap(void* address, nuint length)
    {
        nuint page = (nuint)Environment.SystemPageSize, start = (nuint)address;
        if (length == 0 || start % page != 0 || length > nuint.MaxValue - page + 1)
            return MappingError(22);
        nuint size = (length + page - 1) & ~(page - 1);
        if (start > nuint.MaxValue - size) return MappingError(22);
        nuint end = start + size;
        lock (mappingGate)
        {
            // Only touch ranges owned by this context. In particular, a stale
            // address must not unmap an unrelated CLR/native allocation.
            var overlaps = new List<Mapping>();
            nuint covered = 0;
            foreach (Mapping item in mappings)
            {
                nuint itemEnd = item.Address + item.Length;
                nuint lo = start > item.Address ? start : item.Address;
                nuint hi = end < itemEnd ? end : itemEnd;
                if (lo < hi) { overlaps.Add(item); covered += hi - lo; }
            }
            if (covered != size) return MappingError(22);
            if (OperatingSystem.IsWindows())
            {
                if (overlaps.Count != 1 || overlaps[0].Address != start || overlaps[0].Length != size)
                    return MappingError(95);
                if (UnmapViewOfFile(overlaps[0].WindowsView) == 0) return WindowsMappingError();
                mappings.Remove(overlaps[0]);
                return 0;
            }
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS()) return MappingError(95);
            // Construct all residual records before the unmap, so allocation
            // failure cannot make disposal forget a surviving mapping segment.
            var remnants = new List<Mapping>();
            foreach (Mapping item in overlaps)
            {
                nuint itemEnd = item.Address + item.Length;
                if (item.Address < start) remnants.Add(new Mapping(item.Address, start - item.Address));
                if (itemEnd > end) remnants.Add(new Mapping(end, itemEnd - end));
            }
            mappings.EnsureCapacity(checked(mappings.Count + remnants.Count));
            if (UnmapUnix(address, size) != 0) return MappingError(MappingUnixError());
            foreach (Mapping item in overlaps) mappings.Remove(item);
            mappings.AddRange(remnants);
            return 0;
        }
    }

    private void CloseMappings()
    {
        lock (mappingGate)
        {
            mappingsClosed = true;
            foreach (Mapping item in mappings)
            {
                if (OperatingSystem.IsWindows()) UnmapViewOfFile(item.WindowsView);
                else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) UnmapUnix((void*)item.Address, item.Length);
            }
            mappings.Clear();
        }
    }

    private void* MapError(int error) { SetError(error); return (void*)(-1); }
    private int MappingError(int error) { SetError(error); return -1; }
    private static int MappingUnixError()
    {
        return PosixPlatform.NativeErrorNumber(Marshal.GetLastPInvokeError());
    }
    private void* WindowsMapError() { WindowsMappingError(); return (void*)(-1); }
    private int WindowsMappingError() => MappingError(Marshal.GetLastPInvokeError() switch
    { 5 => 13, 6 => 9, 8 or 14 or 1455 => 12, 87 => 22, 1132 => 13, _ => 5 });

    [LibraryImport("libc", EntryPoint = "mmap", SetLastError = true)]
    private static partial void* MapUnix(void* address, nuint length, int protection, int flags, int fd, long offset);
    [LibraryImport("libc", EntryPoint = "munmap", SetLastError = true)]
    private static partial int UnmapUnix(void* address, nuint length);
    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileMappingW", SetLastError = true)]
    private static partial nint CreateFileMapping(nint file, void* attributes, uint protection, uint maximumHigh, uint maximumLow, char* name);
    [LibraryImport("kernel32.dll", EntryPoint = "MapViewOfFile", SetLastError = true)]
    private static partial void* MapViewOfFile(nint mapping, uint access, uint offsetHigh, uint offsetLow, nuint bytes);
    [LibraryImport("kernel32.dll", EntryPoint = "UnmapViewOfFile", SetLastError = true)]
    private static partial int UnmapViewOfFile(void* address);
    [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    private static partial int CloseMappingHandle(nint handle);
}
