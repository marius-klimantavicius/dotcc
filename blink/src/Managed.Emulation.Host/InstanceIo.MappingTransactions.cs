namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    // Bounded owned references, never entries in the guest descriptor table.
    // No mapping/memory-owner API is called while holding the I/O gate.
    private const int MappingTransactionCharge = 128;
    private readonly HashSet<MappingTransaction> mappingTransactions = new();

    public sealed class MappingTransaction
    {
        internal readonly InstanceIo Owner;
        internal readonly object Description;
        internal bool Released;
        internal MappingTransaction(InstanceIo owner, object description)
        { Owner = owner; Description = description; }
    }

    public HostResult<MappingTransaction> BeginMapping(int fd, CancellationToken cancellation = default)
    {
        lock (sync)
        {
            if (cancellation.IsCancellationRequested) return Fail<MappingTransaction>(GuestError.Canceled);
            if (!Find(fd, out var description)) return Fail<MappingTransaction>(GuestError.BadDescriptor);
            if (description.Kind is not (Kind.File or Kind.MemoryFile))
                return Fail<MappingTransaction>(GuestError.Unsupported);
            if (mappingTransactions.Count >= descriptorLimit || !CanChargeMemoryFile(MappingTransactionCharge) ||
                description.References == int.MaxValue) return Fail<MappingTransaction>(GuestError.NoMemory);
            try
            {
                var transaction = new MappingTransaction(this, description);
                mappingTransactions.Add(transaction);
                ++description.References;
                memoryFileBytes += MappingTransactionCharge;
                return HostResult<MappingTransaction>.Success(transaction);
            }
            catch (OutOfMemoryException) { return Fail<MappingTransaction>(GuestError.NoMemory); }
        }
    }

    public HostResult<int> EndMapping(MappingTransaction transaction)
    {
        lock (sync)
        {
            if (!ReferenceEquals(transaction.Owner, this) || transaction.Released)
                return Fail<int>(GuestError.BadDescriptor);
            transaction.Released = true;
            mappingTransactions.Remove(transaction);
            memoryFileBytes -= MappingTransactionCharge;
            return ReleaseDescription((Description)transaction.Description);
        }
    }

    private bool FindMapping(MappingTransaction transaction, out Description description)
    {
        description = (Description)transaction.Description;
        return !disposed && ReferenceEquals(transaction.Owner, this) && !transaction.Released;
    }

    public HostResult<int> MappingReadAt(MappingTransaction transaction, Span<byte> destination, long offset)
    {
        lock (sync)
        {
            if (!FindMapping(transaction, out var description)) return Fail<int>(GuestError.BadDescriptor);
            return description.Kind == Kind.MemoryFile ? ReadMemoryFile(description.MemoryFile!, destination, offset)
                : files.ReadAt(description.Handle, destination, offset);
        }
    }

    public HostResult<long> MappingReadAtLength(MappingTransaction transaction)
    {
        lock (sync)
        {
            if (!FindMapping(transaction, out var description)) return Fail<long>(GuestError.BadDescriptor);
            return description.Kind == Kind.MemoryFile ? HostResult<long>.Success(description.MemoryFile!.Length)
                : files.ReadAtLength(description.Handle);
        }
    }

    public HostResult<int> MappingSharedFileReadOnly(MappingTransaction transaction)
    {
        lock (sync) return FindMapping(transaction, out var description)
            ? SharedFileReadOnly(description) : Fail<int>(GuestError.BadDescriptor);
    }

    public HostResult<MemoryFilePage> MappingMemoryFilePage(MappingTransaction transaction, long offset,
        CancellationToken cancellation = default)
    {
        lock (sync)
        {
            if (cancellation.IsCancellationRequested) return Fail<MemoryFilePage>(GuestError.Canceled);
            return FindMapping(transaction, out var description) ? AcquireMemoryFilePage(description, offset)
                : Fail<MemoryFilePage>(GuestError.BadDescriptor);
        }
    }

    private void DisposeMappingTransactions()
    {
        // EndMapping also works after disposed becomes true. Obtain one entry
        // at a time so shutdown allocates no temporary collection.
        while (mappingTransactions.Count != 0)
        {
            using var entries = mappingTransactions.GetEnumerator();
            entries.MoveNext();
            EndMapping(entries.Current);
        }
    }
}
