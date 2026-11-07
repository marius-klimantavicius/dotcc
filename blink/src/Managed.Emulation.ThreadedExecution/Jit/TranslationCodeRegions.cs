using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

internal readonly record struct TranslationCodeRun(int Region, int Offset, int Bytes)
{
    public bool IsEmpty => Bytes == 0;
}

// Independently authored, append-only executable arena. The owner serializes
// writers and excludes ALL native epochs for the entire staging transaction.
// Runs may share pages. Invalidation never changes protection or reuses bytes;
// only a quiescent whole-buffer reset rewinds these cursors. No RX/RW alias.
internal unsafe sealed partial class TranslationCodeRegions : IDisposable
{
    private struct Region { public nint Address; public int Cursor; }
    private readonly bool windows;
    private readonly int regionBytes, pagesPerRegion, maximumRegions, pageBytes;
    private Region* regions;
    private byte* pages; // 0 inaccessible, 1 RW staging, 2 RX, 3 quarantined.
    private byte* usedBefore;
    private void* metadata;
    private readonly long metadataBytes;
    private int regionCount, currentRegion;
    private bool disposed;
    public long ReservationCapacityBytes { get; }
    public long ReservedBytes { get; private set; }
    public long OccupiedBytes { get; private set; }
    public long TouchedPageBytes { get; private set; }
    public long PeakOccupiedBytes { get; private set; }
    public long RegionAllocations { get; private set; }
    // Compatibility: pages touched again after a BULK reset, never body reuse.
    public long ReusedPages { get; private set; }
    public long ProtectionFailures { get; private set; }
    public long ProtectionCalls { get; private set; }
    public bool Quarantined { get; private set; }
    // Erased from production; deterministic tests fail the OS boundary before
    // it runs, while preserving real quarantine/unmap/retry behavior.
    partial void ForceSealFailure(ref bool fail);
    partial void ForceRegionReleaseFailure(int region, ref bool fail);
    partial void ForceMetadataReleaseFailure(ref bool fail);
    public int PageBytes => pageBytes;

    internal static long MetadataBytes(long reservationBytes, int regionBytes)
    {
        long count = reservationBytes / regionBytes;
        return RoundPage(checked(count * sizeof(Region) +
            count * (regionBytes / Environment.SystemPageSize) * 2));
    }

    internal TranslationCodeRegions(long reservationBytes, int regionBytes,
        bool allowUnverifiedWindows, bool requireNativeFlags)
    {
        string? unavailable = NativeCodePlatform.UnavailableReason(allowUnverifiedWindows, requireNativeFlags);
        if (unavailable != null) throw new PlatformNotSupportedException(unavailable);
        pageBytes = Environment.SystemPageSize;
        if (regionBytes < 65536 || regionBytes % 65536 != 0 ||
            regionBytes % pageBytes != 0 || reservationBytes < regionBytes ||
            reservationBytes % regionBytes != 0 || reservationBytes / regionBytes > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(regionBytes));
        windows = OperatingSystem.IsWindows();
        this.regionBytes = regionBytes;
        pagesPerRegion = regionBytes / pageBytes;
        maximumRegions = checked((int)(reservationBytes / regionBytes));
        ReservationCapacityBytes = reservationBytes;
        metadataBytes = MetadataBytes(reservationBytes, regionBytes);
        metadata = AllocateData(metadataBytes);
        if (metadata == null) throw new OutOfMemoryException();
        NativeMemory.Clear(metadata, (nuint)metadataBytes);
        regions = (Region*)metadata;
        pages = (byte*)(regions + maximumRegions);
        usedBefore = pages + checked(maximumRegions * pagesPerRegion);
    }

    // Extra reservation needed for this append. The store checks its shared
    // byte quota (code + dynamically grown RW arenas) BEFORE allowing growth.
    internal long AdditionalReservationBytes(int capacity)
    {
        if (capacity == 0) return 0;
        int r = currentRegion;
        if (r < regionCount && regions[r].Cursor <= regionBytes - AlignCode(capacity)) return 0;
        if (r < regionCount) ++r;
        return r < regionCount ? 0 : regionBytes;
    }

    internal bool TryStage(int capacity, out TranslationCodeRun run, out nint pointer,
        out TranslationStoreFailure failure)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        run = default; pointer = 0; failure = TranslationStoreFailure.None;
        if (Quarantined) { failure = TranslationStoreFailure.NativeAllocation; return false; }
        if (capacity == 0) return true;
        if (capacity < 0 || capacity > regionBytes)
        { failure = TranslationStoreFailure.InvalidSize; return false; }
        int occupied = AlignCode(capacity);
        int r = currentRegion;
        if (r < regionCount && regions[r].Cursor > regionBytes - occupied) ++r;
        if (r == maximumRegions) { failure = TranslationStoreFailure.Capacity; return false; }
        if (r == regionCount && !TryAddRegion())
        { failure = TranslationStoreFailure.NativeAllocation; return false; }
        currentRegion = r;
        int offset = regions[r].Cursor;
        regions[r].Cursor += occupied;
        OccupiedBytes += occupied; PeakOccupiedBytes = Math.Max(PeakOccupiedBytes, OccupiedBytes);
        run = new(r, offset, occupied); pointer = Address(run);
        int first = offset / pageBytes, last = (offset + occupied - 1) / pageBytes;
        nint page = regions[r].Address + first * pageBytes;
        nuint bytes = (nuint)((last - first + 1) * pageBytes);
        bool success;
        if (windows)
        {
            success = VirtualAlloc(page, bytes, 0x1000, 0x04) == page;
            ++ProtectionCalls;
            success = success && VirtualProtect(page, bytes, 0x04, out _);
        }
        else { ++ProtectionCalls; success = mprotect(page, bytes, 3) == 0; }
        if (!success)
        {
            FailClosed(run); failure = TranslationStoreFailure.NativeAllocation; return false;
        }
        for (int p = first; p <= last; ++p)
        {
            int at = r * pagesPerRegion + p;
            if (pages[at] == 0)
            {
                TouchedPageBytes += pageBytes;
                if (usedBefore[at] != 0) ++ReusedPages;
            }
            pages[at] = 1; usedBefore[at] = 1;
        }
        // Clear only the new bytes, never prior translations on a shared page.
        NativeMemory.Clear((void*)pointer, (nuint)capacity);
        return true;
    }

    // Abort uses the same seal: previous translations sharing the tail page
    // must regain RX before the owner's writer admission can reopen.
    internal bool TrySeal(TranslationCodeRun run)
    {
        if (run.IsEmpty) return !Quarantined;
        if (Quarantined) return false;
        int first = run.Offset / pageBytes, last = (run.Offset + run.Bytes - 1) / pageBytes;
        nint address = regions[run.Region].Address + first * pageBytes;
        nuint bytes = (nuint)((last - first + 1) * pageBytes);
        ++ProtectionCalls;
        bool forced = false;
        ForceSealFailure(ref forced);
        bool success = !forced && (windows ? VirtualProtect(address, bytes, 0x20, out _) : mprotect(address, bytes, 5) == 0);
        if (success && windows) success = FlushInstructionCache(GetCurrentProcess(), address, bytes);
        if (!success) { FailClosed(run); return false; }
        SetState(run, 2); return true;
    }

    // Only unused emission capacity at the still-private tail is trimmed.
    // Invalidated or aborted bodies are never individually recycled.
    internal void Commit(TranslationCodeRun run, int usedBytes)
    {
        if (run.IsEmpty) return;
        int used = AlignCode(usedBytes);
        regions[run.Region].Cursor -= run.Bytes - used;
        OccupiedBytes -= run.Bytes - used;
    }

    internal bool TryReset()
    {
        bool success = true;
        for (int i = 0; i < regionCount; ++i)
        {
            ++ProtectionCalls;
            // Decommit also handles never-committed reservation pages.
            bool ok = windows ? VirtualFree(regions[i].Address, (nuint)regionBytes, 0x4000)
                : mprotect(regions[i].Address, (nuint)regionBytes, 0) == 0;
            if (!ok) { ++ProtectionFailures; success = false; }
        }
        if (!success) { Quarantined = true; return false; }
        NativeMemory.Clear(pages, checked((nuint)(maximumRegions * pagesPerRegion)));
        for (int i = 0; i < regionCount; ++i) regions[i].Cursor = 0;
        currentRegion = 0; OccupiedBytes = TouchedPageBytes = 0; Quarantined = false;
        return true;
    }

    // A changed code/data mix may need to rebalance the shared quota after
    // reset. Drop empty reservations only, never a live body's code pages.
    internal bool TryTrimEmpty()
    {
        if (OccupiedBytes != 0 || TouchedPageBytes != 0) throw new InvalidOperationException("Code arena is not empty");
        while (regionCount != 0)
        {
            int i = regionCount - 1; nint address = regions[i].Address;
            if (windows ? !VirtualFree(address, 0, 0x8000) : munmap(address, (nuint)regionBytes) != 0) return false;
            regions[i] = default; --regionCount; ReservedBytes -= regionBytes;
            NativeMemory.Clear(usedBefore + i * pagesPerRegion, (nuint)pagesPerRegion);
        }
        currentRegion = 0; return true;
    }

    internal nint Address(TranslationCodeRun run) => run.IsEmpty ? 0 : regions[run.Region].Address + run.Offset;
    private static int AlignCode(int bytes) => checked((bytes + 15) & ~15);
    private bool TryAddRegion()
    {
        nint address = windows ? VirtualAlloc(0, (nuint)regionBytes, 0x2000, 0x01)
            : mmap(0, (nuint)regionBytes, 0, 0x22, -1, 0);
        if (windows ? address == 0 : address == -1) return false;
        regions[regionCount++].Address = address;
        ReservedBytes += regionBytes; ++RegionAllocations; return true;
    }
    private void SetState(TranslationCodeRun run, byte state)
    {
        int first = run.Offset / pageBytes, last = (run.Offset + run.Bytes - 1) / pageBytes;
        for (int p = first; p <= last; ++p) pages[run.Region * pagesPerRegion + p] = state;
    }
    private void FailClosed(TranslationCodeRun run)
    {
        ++ProtectionFailures; Quarantined = true;
        int first = run.Offset / pageBytes, last = (run.Offset + run.Bytes - 1) / pageBytes;
        nint address = regions[run.Region].Address + first * pageBytes;
        ++ProtectionCalls;
        for (int p = first; p <= last; ++p)
        {
            int at = run.Region * pagesPerRegion + p;
            if (pages[at] == 0) TouchedPageBytes += pageBytes;
        }
        // Even if the quarantine protection fails, owner admission stays shut.
        if (windows) VirtualProtect(address, (nuint)((last - first + 1) * pageBytes), 0x01, out _);
        else mprotect(address, (nuint)((last - first + 1) * pageBytes), 0);
        SetState(run, 3);
    }

    public void Dispose()
    {
        if (disposed) return;
        for (int i = 0; i < regionCount; ++i)
        {
            nint address = regions[i].Address;
            if (address == 0) continue;
            bool forced = false;
            ForceRegionReleaseFailure(i, ref forced);
            if (forced) throw new InvalidOperationException("Injected translation code region release failure");
            if (windows ? !VirtualFree(address, 0, 0x8000) : munmap(address, (nuint)regionBytes) != 0)
                throw new InvalidOperationException("Could not release translation code region");
            regions[i].Address = 0; ReservedBytes -= regionBytes;
        }
        bool metadataFailure = false;
        ForceMetadataReleaseFailure(ref metadataFailure);
        if (metadataFailure) throw new InvalidOperationException("Injected translation code metadata release failure");
        ReleaseData(metadata, metadataBytes);
        metadata = null; regions = null; pages = null; usedBefore = null;
        disposed = true;
    }

    internal static long RoundPage(long value) => checked((value + Environment.SystemPageSize - 1) /
        Environment.SystemPageSize * Environment.SystemPageSize);

    // Data pools use exact page-rounded OS mappings too. A malloc arena can
    // reserve unaccounted capacity, so retained pools do not use malloc/GC.
    internal static void* AllocateData(long bytes)
    {
        nint address = OperatingSystem.IsWindows() ? VirtualAlloc(0, checked((nuint)bytes), 0x3000, 0x04)
            : mmap(0, checked((nuint)bytes), 3, 0x22, -1, 0);
        return (void*)(OperatingSystem.IsWindows() ? address : address == -1 ? 0 : address);
    }
    internal static void ReleaseData(void* address, long bytes)
    {
        if (address == null) return;
        if (OperatingSystem.IsWindows() ? !VirtualFree((nint)address, 0, 0x8000) : munmap((nint)address, checked((nuint)bytes)) != 0)
            throw new InvalidOperationException("Could not release translation data mapping");
    }

    [DllImport("libc", SetLastError = true)] private static extern nint mmap(nint address, nuint length, int protection, int flags, int fd, nint offset);
    [DllImport("libc", SetLastError = true)] private static extern int mprotect(nint address, nuint length, int protection);
    [DllImport("libc", SetLastError = true)] private static extern int munmap(nint address, nuint length);
    [DllImport("kernel32", SetLastError = true)] private static extern nint VirtualAlloc(nint address, nuint size, uint allocationType, uint protection);
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualProtect(nint address, nuint size, uint protection, out uint oldProtection);
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualFree(nint address, nuint size, uint freeType);
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlushInstructionCache(nint process, nint address, nuint size);
    [DllImport("kernel32")] private static extern nint GetCurrentProcess();
}
