using System.Runtime.InteropServices;
using System.Text;

namespace Managed.Emulation.Host;

/// <summary>A trusted backing lease, never a guest pointer or guest descriptor.</summary>
public readonly record struct MemoryFilePage(nint Address, ulong Lease);
public readonly record struct MemoryFileUsage(long Bytes, long Limit, int Files, int Leases);

public sealed partial class InstanceIo
{
    // Sparse logical length is independent of resident storage. In particular,
    // CoreCLR truncates its executable-code store to 2 TiB before mapping pages.
    // Conservative charges cover managed nodes/dictionaries and native payload;
    // the C memory owner separately charges each alias and its own metadata.
    private const int MemoryFileNodeCharge = 1024, MemoryFilePageCharge = 4096 + 128,
        MemoryFileLeaseCharge = 128;
    private long memoryFileLimit = 64L << 20, memoryFileBytes;
    private long memoryOwnerBytes, memoryCombinedLimit = long.MaxValue;
    private ulong memoryBudgetToken;
    private int memoryFileCount;
    private ulong memoryFileInode;
    private sealed class MemoryFile(ulong inode, string name)
    {
        internal readonly ulong Inode = inode;
        internal readonly string Name = name;
        internal readonly Dictionary<long, MemoryPage> Pages = new();
        internal long Length;
        internal bool Open = true;
        internal bool Regular;
        internal Description? Reader;
        internal int Leases;
        internal long AccessTicks = DateTime.UtcNow.Ticks, ModifyTicks = DateTime.UtcNow.Ticks,
            ChangeTicks = DateTime.UtcNow.Ticks;
    }
    private sealed class MemoryPage(nint address)
    {
        internal readonly nint Address = address;
        internal int Leases;
    }
    private sealed record MemoryLease(InstanceIo Owner, MemoryFile File, long Offset, MemoryPage Page);
    private readonly Dictionary<ulong, MemoryLease> memoryFileLeases = new();
    // Registry roots only active leases. It permits final C-owner cleanup after
    // worker IO unbinding, without storing CLR references in translated C memory.
    // Registry lookup releases its gate before taking an instance gate. All
    // mutation takes instance -> registry, so concurrent map/release cannot invert.
    private static readonly object memoryLeaseGate = new();
    private static readonly Dictionary<ulong, MemoryLease> memoryLeases = new();
    private static ulong nextMemoryLease;
    private static readonly Dictionary<ulong, InstanceIo> memoryBudgets = new();
    private static ulong nextMemoryBudget;

    private bool CanChargeMemoryFile(long charge) => charge <= memoryFileLimit - memoryFileBytes &&
        charge <= memoryCombinedLimit - memoryOwnerBytes - memoryFileBytes;
    public HostResult<ulong> BindMemoryFileBudget(long limit, long ownerBytes)
    {
        lock (sync)
        {
            if (disposed) return Fail<ulong>(GuestError.BadDescriptor);
            if (limit < 0 || ownerBytes < 0 || ownerBytes > limit || memoryFileBytes > limit - ownerBytes)
                return Fail<ulong>(GuestError.NoMemory);
            if (memoryBudgetToken != 0) return Fail<ulong>(GuestError.Busy);
            try
            {
                lock (memoryLeaseGate)
                {
                    if (nextMemoryBudget == ulong.MaxValue) return Fail<ulong>(GuestError.NoMemory);
                    ulong token = ++nextMemoryBudget;
                    memoryBudgets.Add(token, this);
                    memoryBudgetToken = token;
                }
                memoryCombinedLimit = limit;
                memoryOwnerBytes = ownerBytes;
                return HostResult<ulong>.Success(memoryBudgetToken);
            }
            catch (OutOfMemoryException) { return Fail<ulong>(GuestError.NoMemory); }
        }
    }
    public static HostResult<int> AccountMemoryFileBudget(ulong token, long bytes, bool release = false)
    {
        InstanceIo? instance;
        lock (memoryLeaseGate) memoryBudgets.TryGetValue(token, out instance);
        if (instance == null) return Fail<int>(GuestError.Invalid);
        lock (instance.sync)
        {
            if (instance.memoryBudgetToken != token) return Fail<int>(GuestError.Invalid);
            if (!release && (bytes < 0 || bytes > instance.memoryCombinedLimit - instance.memoryFileBytes))
                return Fail<int>(GuestError.NoMemory);
            instance.memoryOwnerBytes = release ? 0 : bytes;
            if (release)
            {
                lock (memoryLeaseGate) memoryBudgets.Remove(token);
                instance.memoryBudgetToken = 0;
            }
            return HostResult<int>.Success(0);
        }
    }

    public MemoryFileUsage MemoryFilesUsage()
    {
        lock (sync) return new(memoryFileBytes, memoryFileLimit, memoryFileCount, memoryFileLeases.Count);
    }
    public HostResult<int> SharedFileReadOnly(int fd)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<int>(GuestError.BadDescriptor);
            return SharedFileReadOnly(description);
        }
    }
    private HostResult<int> SharedFileReadOnly(Description description)
    {
        if (description.ProcessSnapshot) return Fail<int>(GuestError.Unsupported);
        if (description.Kind == Kind.MemoryFile) return HostResult<int>.Success(0);
        if (description.Kind != Kind.File) return Fail<int>(GuestError.Unsupported);
        var stat = files.FStat(description.Handle);
        if (!stat.Succeeded) return Fail<int>(stat.Error);
        return (stat.Value.Mode & 0xf000) == 0x8000 ? HostResult<int>.Success(stat.Value.Immutable ? 1 : 2)
            : Fail<int>(GuestError.Unsupported);
    }
    public HostResult<int> SetMemoryFileLimit(long limit)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if (limit < 4096 + 1024 || limit > 256L << 20) return Fail<int>(GuestError.Invalid);
            if (memoryFileCount != 0 || mappingTransactions.Count != 0) return Fail<int>(GuestError.Busy);
            memoryFileLimit = limit;
            return HostResult<int>.Success(0);
        }
    }
    public HostResult<int> CreateMemoryFile(string name, uint flags, CancellationToken cancellation = default)
        => CreateMemoryFile(Encoding.UTF8.GetBytes(name), flags, cancellation);
    public HostResult<int> CreateMemoryFile(ReadOnlySpan<byte> name, uint flags, CancellationToken cancellation = default)
    {
        lock (sync)
        {
            if (cancellation.IsCancellationRequested) return Fail<int>(GuestError.Canceled);
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if ((flags & ~7u) != 0) return Fail<int>(GuestError.Invalid);
            // Seals and hugetlb require additional contracts; never claim them.
            if ((flags & ~1u) != 0) return Fail<int>(GuestError.Unsupported);
            if (name.Contains((byte)0) || name.Length > 249) return Fail<int>(GuestError.Invalid);
            int fd = Allocate();
            if (fd < 0) return Fail<int>(GuestError.TooManyFiles);
            if (memoryFileCount >= descriptorLimit) return Fail<int>(GuestError.NoSpace);
            if (!CanChargeMemoryFile(MemoryFileNodeCharge)) return Fail<int>(GuestError.NoMemory);
            if (memoryFileInode == 0x7fffffffffffffffUL) return Fail<int>(GuestError.NoSpace);
            try
            {
                var file = new MemoryFile(0x8000000000000000UL | ++memoryFileInode, Encoding.Latin1.GetString(name));
                var description = new Description(Kind.MemoryFile, statusFlags: 2) { MemoryFile = file };
                if ((flags & 1) != 0) closeOnExec.Add(fd);
                try { descriptors.Add(fd, description); }
                catch { closeOnExec.Remove(fd); throw; }
                memoryFileBytes += MemoryFileNodeCharge;
                ++memoryFileCount;
                return HostResult<int>.Success(fd);
            }
            catch (OutOfMemoryException) { return Fail<int>(GuestError.NoMemory); }
        }
    }
    private static VirtualFileStat MemoryFileStat(MemoryFile file) => new(file.Length, false, false,
        file.Inode, 0x81ff, file.AccessTicks, file.ModifyTicks, file.ChangeTicks, Links: 0);
    private HostResult<long> SeekMemoryFile(Description description, long offset, SeekOrigin origin)
    {
        long basis = origin == SeekOrigin.Begin ? 0 : origin == SeekOrigin.Current ? description.Position
            : origin == SeekOrigin.End ? description.MemoryFile!.Length : -1;
        if (basis < 0 || offset < -basis || offset > long.MaxValue - basis) return Fail<long>(GuestError.Invalid);
        description.Position = basis + offset;
        return HostResult<long>.Success(description.Position);
    }
    private unsafe HostResult<int> ReadMemoryFile(MemoryFile file, Span<byte> destination, long offset)
    {
        if (offset < 0) return Fail<int>(GuestError.Invalid);
        int count = offset < file.Length ? (int)Math.Min(destination.Length, file.Length - offset) : 0;
        destination[..count].Clear();
        for (int copied = 0; copied < count;)
        {
            long position = offset + copied, pageOffset = position & ~4095L;
            int skew = (int)(position & 4095), bytes = Math.Min(4096 - skew, count - copied);
            if (file.Pages.TryGetValue(pageOffset, out var page))
                new ReadOnlySpan<byte>((void*)(page.Address + skew), bytes).CopyTo(destination.Slice(copied, bytes));
            copied += bytes;
        }
        file.AccessTicks = DateTime.UtcNow.Ticks;
        return HostResult<int>.Success(count);
    }
    private unsafe MemoryPage AllocateMemoryPage(MemoryFile file, long offset)
    {
        if (!CanChargeMemoryFile(MemoryFilePageCharge)) throw new OutOfMemoryException();
        void* pointer = NativeMemory.AlignedAlloc(4096, 4096);
        if (pointer == null) throw new OutOfMemoryException();
        NativeMemory.Clear(pointer, 4096);
        try
        {
            var page = new MemoryPage((nint)pointer);
            file.Pages.Add(offset, page);
            memoryFileBytes += MemoryFilePageCharge;
            return page;
        }
        catch { NativeMemory.AlignedFree(pointer); throw; }
    }
    private unsafe void FreeMemoryPage(MemoryFile file, long offset, MemoryPage page)
    {
        file.Pages.Remove(offset);
        NativeMemory.AlignedFree((void*)page.Address);
        memoryFileBytes -= MemoryFilePageCharge;
    }
    private unsafe HostResult<int> WriteMemoryFile(MemoryFile file, ReadOnlySpan<byte> source, long offset)
    {
        if (offset < 0 || source.Length > long.MaxValue - offset) return Fail<int>(GuestError.Invalid);
        if (source.IsEmpty) return HostResult<int>.Success(0);
        long last = (offset + source.Length - 1) & ~4095L;
        // Prepare missing pages before copying any bytes. Failure reclaims only
        // pages added by this write, preserving old bytes, length and position.
        List<long>? added = null;
        try
        {
            added = new();
            for (long page = offset & ~4095L;; page += 4096)
            {
                if (!file.Pages.ContainsKey(page))
                {
                    added.Add(page);
                    AllocateMemoryPage(file, page);
                }
                if (page == last) break;
            }
        }
        catch (OutOfMemoryException)
        {
            if (added != null) foreach (long page in added)
                if (file.Pages.TryGetValue(page, out var allocation)) FreeMemoryPage(file, page, allocation);
            return Fail<int>(GuestError.NoMemory);
        }
        for (int copied = 0; copied < source.Length;)
        {
            long position = offset + copied;
            int skew = (int)(position & 4095), count = Math.Min(4096 - skew, source.Length - copied);
            source.Slice(copied, count).CopyTo(new Span<byte>((void*)(file.Pages[position & ~4095L].Address + skew), count));
            copied += count;
        }
        file.Length = Math.Max(file.Length, offset + source.Length);
        file.ModifyTicks = file.ChangeTicks = DateTime.UtcNow.Ticks;
        return HostResult<int>.Success(source.Length);
    }
    private unsafe HostResult<int> TruncateMemoryFile(MemoryFile file, long length)
    {
        if (length < 0) return Fail<int>(GuestError.Invalid);
        if (length < file.Length)
        {
            // Keep leased addresses stable, zeroing all removed bytes so regrow
            // cannot resurrect them. Unleased removed pages can be reclaimed.
            foreach (var item in file.Pages)
            {
                long offset = item.Key;
                var page = item.Value;
                if (offset >= length && page.Leases == 0) FreeMemoryPage(file, offset, page);
                else if (offset > length - 4096)
                {
                    int start = offset >= length ? 0 : (int)(length - offset);
                    new Span<byte>((void*)(page.Address + start), 4096 - start).Clear();
                }
            }
        }
        file.Length = length;
        file.ModifyTicks = file.ChangeTicks = DateTime.UtcNow.Ticks;
        return HostResult<int>.Success(0);
    }
    private HostResult<int> CloseMemoryFile(MemoryFile file)
    {
        file.Open = false;
        ReclaimMemoryFile(file);
        return HostResult<int>.Success(0);
    }
    private void ReclaimMemoryFile(MemoryFile file)
    {
        if (file.Open || file.Leases != 0) return;
        if (file.Regular)
        {
            regularSharedFiles.Remove(file.Inode);
            if (file.Reader is { } reader)
            {
                file.Reader = null;
                ReleaseDescription(reader);
            }
        }
        // Dictionary.Remove does not invalidate its enumerators on .NET 10.
        // Teardown and lease release must not allocate a temporary key array.
        foreach (var item in file.Pages) FreeMemoryPage(file, item.Key, item.Value);
        memoryFileBytes -= MemoryFileNodeCharge;
        --memoryFileCount;
    }
    public HostResult<MemoryFilePage> AcquireMemoryFilePage(int fd, long offset, CancellationToken cancellation = default)
    {
        lock (sync)
        {
            if (cancellation.IsCancellationRequested) return Fail<MemoryFilePage>(GuestError.Canceled);
            if (!Find(fd, out var description)) return Fail<MemoryFilePage>(GuestError.BadDescriptor);
            return AcquireMemoryFilePage(description, offset);
        }
    }
    private HostResult<MemoryFilePage> AcquireMemoryFilePage(Description description, long offset)
    {
        if (description.ProcessSnapshot) return Fail<MemoryFilePage>(GuestError.Unsupported);
        if (offset < 0 || (offset & 4095) != 0 || offset > long.MaxValue - 4096) return Fail<MemoryFilePage>(GuestError.Invalid);
        if (description.Kind == Kind.File) return AcquireRegularSharedPage(description, offset);
        return description.Kind == Kind.MemoryFile ? AcquireMemoryFilePage(description.MemoryFile!, offset)
            : Fail<MemoryFilePage>(GuestError.Unsupported);
    }
    private HostResult<MemoryFilePage> AcquireMemoryFilePage(MemoryFile file, long offset)
    {
        bool existing = file.Pages.TryGetValue(offset, out var page);
        long charge = MemoryFileLeaseCharge + (existing ? 0 : MemoryFilePageCharge);
        if (!CanChargeMemoryFile(charge)) return Fail<MemoryFilePage>(GuestError.NoMemory);
        ulong token = 0;
        try
        {
            page ??= AllocateMemoryPage(file, offset);
            if (!existing && file.Reader is { } reader)
            {
                var loaded = LoadRegularSharedPage(file, reader, offset, page);
                if (!loaded.Succeeded)
                {
                    FreeMemoryPage(file, offset, page);
                    return Fail<MemoryFilePage>(loaded.Error);
                }
            }
            var lease = new MemoryLease(this, file, offset, page);
            lock (memoryLeaseGate)
            {
                if (nextMemoryLease == ulong.MaxValue) throw new OutOfMemoryException();
                token = ++nextMemoryLease;
                memoryLeases.Add(token, lease);
            }
            memoryFileLeases.Add(token, lease);
            ++file.Leases; ++page.Leases;
            memoryFileBytes += MemoryFileLeaseCharge;
            return HostResult<MemoryFilePage>.Success(new(page.Address, token));
        }
        catch (OutOfMemoryException)
        {
            if (token != 0) lock (memoryLeaseGate) memoryLeases.Remove(token);
            if (!existing && page != null) FreeMemoryPage(file, offset, page);
            return Fail<MemoryFilePage>(GuestError.NoMemory);
        }
    }
    public static HostResult<int> ReleaseMemoryFilePage(ulong token)
    {
        MemoryLease? lease;
        lock (memoryLeaseGate) memoryLeases.TryGetValue(token, out lease);
        if (lease == null) return Fail<int>(GuestError.Invalid);
        lock (lease.Owner.sync)
        {
            if (!lease.Owner.memoryFileLeases.Remove(token)) return Fail<int>(GuestError.Invalid);
            lock (memoryLeaseGate) memoryLeases.Remove(token);
            --lease.File.Leases; --lease.Page.Leases;
            lease.Owner.memoryFileBytes -= MemoryFileLeaseCharge;
            if (lease.Page.Leases == 0 && (lease.File.Regular || lease.Offset >= lease.File.Length || MemoryPageEmpty(lease.Page)))
                lease.Owner.FreeMemoryPage(lease.File, lease.Offset, lease.Page);
            lease.Owner.ReclaimMemoryFile(lease.File);
            return HostResult<int>.Success(0);
        }
    }
    private static unsafe bool MemoryPageEmpty(MemoryPage page)
        => new ReadOnlySpan<byte>((void*)page.Address, 4096).IndexOfAnyExcept((byte)0) < 0;
    public static bool MemoryFilePageAvailable(ulong token)
    {
        MemoryLease? lease;
        lock (memoryLeaseGate) memoryLeases.TryGetValue(token, out lease);
        if (lease == null) return false;
        lock (lease.Owner.sync) return lease.Owner.memoryFileLeases.ContainsKey(token) && lease.Offset < lease.File.Length;
    }
}
