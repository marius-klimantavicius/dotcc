namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    // Only guest operations through this InstanceIo are synchronized here.
    // HostDirectoryFileSystem does not promise coherence with external host writers.
    // MountedFileSystem's stat inode is unique across the guest mount namespace.
    private readonly Dictionary<ulong, MemoryFile> regularSharedFiles = new();

    private HostResult<MemoryFilePage> AcquireRegularSharedPage(Description description, long offset)
    {
        var stat = files.FStat(description.Handle);
        if (!stat.Succeeded) return Fail<MemoryFilePage>(stat.Error);
        if (stat.Value.Immutable || (stat.Value.Mode & 0xf000) != 0x8000)
            return Fail<MemoryFilePage>(GuestError.Unsupported);
        // Also validate read access before reusing another description's cache.
        var length = files.ReadAtLength(description.Handle);
        if (!length.Succeeded) return Fail<MemoryFilePage>(length.Error);
        if (length.Value < 0) return Fail<MemoryFilePage>(GuestError.Io);
        bool created = false;
        if (!regularSharedFiles.TryGetValue(stat.Value.Inode, out var file))
        {
            if (memoryFileCount >= descriptorLimit) return Fail<MemoryFilePage>(GuestError.NoSpace);
            if (description.References == int.MaxValue ||
                !CanChargeMemoryFile(MemoryFileNodeCharge + MemoryFilePageCharge + MemoryFileLeaseCharge))
                return Fail<MemoryFilePage>(GuestError.NoMemory);
            try
            {
                file = new MemoryFile(stat.Value.Inode, "")
                { Length = length.Value, Open = false, Regular = true, Reader = description };
                regularSharedFiles.Add(file.Inode, file);
                ++description.References; // No guest fd slot; retain the original open file identity.
                ++memoryFileCount;
                memoryFileBytes += MemoryFileNodeCharge;
                created = true;
            }
            catch (OutOfMemoryException) { return Fail<MemoryFilePage>(GuestError.NoMemory); }
        }
        else
        {
            if (file.BackingUnavailable) return Fail<MemoryFilePage>(GuestError.Io);
            if (file.Length != length.Value) TruncateMemoryFile(file, length.Value);
        }
        var result = AcquireMemoryFilePage(file, offset);
        if (!result.Succeeded && created) ReclaimMemoryFile(file);
        return result;
    }

    private unsafe HostResult<int> LoadRegularSharedPage(MemoryFile file, Description reader, long offset, MemoryPage page)
    {
        // The current caller only loads freshly allocated, unpublished pages.
        // Keep reloads of an exposed address covered if a caller is added later.
        using var mutation = page.Published ? BeginMemoryFileMutation(page.Address, 4096, logical: false) : default;
        int count = offset < file.Length ? (int)Math.Min(4096, file.Length - offset) : 0;
        var destination = new Span<byte>((void*)page.Address, count);
        for (int copied = 0; copied < count;)
        {
            var read = files.ReadAt(reader.Handle, destination[copied..], offset + copied);
            if (!read.Succeeded) return read;
            if (read.Value <= 0 || read.Value > count - copied) return Fail<int>(GuestError.Io);
            copied += read.Value;
        }
        return HostResult<int>.Success(0);
    }

    // Caller holds sync. External filesystem writers are not enrolled in this
    // barrier; refreshing EOF is a guarded local cache update, not a coherence
    // promise with those writers. An open reader must retain its inode identity.
    private bool RefreshRegularSharedFile(MemoryFile file)
    {
        if (file.BackingUnavailable) return false;
        if (!file.Regular || file.Reader is not { } reader) return true;
        var stat = files.FStat(reader.Handle);
        if (!stat.Succeeded) return false;
        if (stat.Value.Inode != file.Inode || stat.Value.Immutable || (stat.Value.Mode & 0xf000) != 0x8000)
        {
            using var mutation = file.Published ? BeginMemoryFileMutation(0, 0, logical: true) : default;
            // Never silently repoint existing leases to a replacement backing.
            // A backend that changes an open handle's identity fails closed.
            file.BackingUnavailable = true;
            return false;
        }
        var length = files.ReadAtLength(reader.Handle);
        if (!length.Succeeded || length.Value < 0) return false;
        if (file.Length != length.Value) TruncateMemoryFile(file, length.Value);
        return true;
    }

    // Caller holds sync throughout backend mutation and cache publication.
    // Copy successful bytes directly, so coherence needs no fallible read/allocation
    // after a backend write has committed. Nonresident pages are read lazily later.
    private unsafe HostResult<int> WriteRegularFile(Description description, ReadOnlySpan<byte> source, long? offset)
    {
        MemoryFile? file = null;
        long position = 0;
        if (regularSharedFiles.Count != 0 && !source.IsEmpty && offset is not < 0)
        {
            var stat = files.FStat(description.Handle);
            if (!stat.Succeeded) return Fail<int>(stat.Error);
            if (regularSharedFiles.TryGetValue(stat.Value.Inode, out file))
            {
                if (file.BackingUnavailable) file = null;
                if ((description.StatusFlags & 1024) != 0) position = stat.Value.Length;
                else if (offset is long at) position = at;
                else
                {
                    var current = files.Seek(description.Handle, 0, SeekOrigin.Current);
                    if (!current.Succeeded) return Fail<int>(current.Error);
                    position = current.Value;
                }
            }
        }
        // Successful writes can extend EOF as well as change several resident
        // pages. One conservative logical scope covers backend commit, every
        // cache copy and length publication, including short writes or failures.
        using var mutation = file is { Published: true } ? BeginMemoryFileMutation(0, 0, logical: true) : default;
        var result = offset is long target ? files.WriteAt(description.Handle, source, target)
            : files.Write(description.Handle, source);
        if (result.Succeeded && result.Value > 0 && file != null)
        {
            long end = position + result.Value;
            foreach (var entry in file.Pages)
            {
                long start = Math.Max(position, entry.Key), stop = Math.Min(end, entry.Key + 4096);
                if (start < stop) source.Slice((int)(start - position), (int)(stop - start))
                    .CopyTo(new Span<byte>((void*)(entry.Value.Address + (int)(start - entry.Key)), (int)(stop - start)));
            }
            file.Length = Math.Max(file.Length, end);
        }
        return result;
    }

    private MemoryFile? RegularSharedFile(VirtualFileStat stat)
        => regularSharedFiles.GetValueOrDefault(stat.Inode);

    private void DisposeRegularSharedReaders()
    {
        // Resident leased pages outlive IO disposal; backend readers need not.
        foreach (var file in regularSharedFiles.Values)
            if (file.Reader is { } reader)
            {
                using var mutation = file.Published ? BeginMemoryFileMutation(0, 0, logical: true) : default;
                file.Reader = null;
                ReleaseDescription(reader);
            }
    }
}
