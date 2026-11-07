namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    private IHostMemoryFileMutation? memoryFileMutation;

    /// <summary>Bind one execution owner's observer before admitting execution.
    /// Rebinding the same observer is harmless; replacing a bound owner is rejected.</summary>
    public void BindMemoryFileMutation(IHostMemoryFileMutation observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (memoryFileMutation != null && !ReferenceEquals(memoryFileMutation, observer))
                throw new InvalidOperationException("A memory-file mutation observer is already bound.");
            memoryFileMutation = observer;
        }
    }

    /// <summary>Unbind the same observer after execution and mutation callbacks
    /// are quiescent. This is also allowed after I/O disposal and final lease cleanup.</summary>
    public void UnbindMemoryFileMutation(IHostMemoryFileMutation observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        lock (sync)
        {
            if (!ReferenceEquals(memoryFileMutation, observer))
                throw new InvalidOperationException("The memory-file mutation observer does not match.");
            // Revoke every cached pointer before removing its mutation proof.
            // Begin runs under sync but never acquires the authority gate, and
            // drains only finite native frames that cannot call I/O. End uses
            // the captured observer even after the binding has been cleared.
            // With no active leases, release already revoked every prior hint.
            using var mutation = memoryFileLeases.Count != 0 && memoryFileMutation is IHostMemoryFileNativeReadOwner
                ? BeginMemoryFileMutation(0, 0, logical: true) : default;
            memoryFileMutation = null;
        }
    }

    /// <summary>Whether this active lease belongs to observer-enrolled local
    /// memfd backing. Regular files can have writers outside this InstanceIo and
    /// must not gain multi-instruction native eligibility from this observer.</summary>
    public bool MemoryFileMutationOwned(ulong lease)
    {
        lock (sync) return memoryFileMutation != null && memoryFileLeases.TryGetValue(lease, out var backing) &&
            ReferenceEquals(backing.Owner, this) && !backing.File.Regular;
    }

    /// <summary>Authenticate an exact local memfd page against the authority
    /// actually drained by the currently bound observer. No regular/external,
    /// foreign-instance, unbound, disposed, released or beyond-EOF lease qualifies.
    /// This is read-only ownership evidence, not a lifetime lease or permission.</summary>
    public bool MemoryFileSharedReadOwned(ulong lease, nint page, object authority)
    {
        lock (sync) return !disposed && authority != null && page != 0 && ((nuint)page & 4095) == 0 &&
            memoryFileMutation is IHostMemoryFileNativeReadOwner owner &&
            ReferenceEquals(owner.MemoryFileNativeReadAuthority, authority) &&
            memoryFileLeases.TryGetValue(lease, out var backing) &&
            ReferenceEquals(backing.Owner, this) && !backing.File.Regular &&
            !backing.File.BackingUnavailable && backing.Offset < backing.File.Length &&
            backing.Page.Address == page;
    }

    // All callers hold sync, outside memoryLeaseGate. Keep the admitted observer
    // in the value scope: Dispose must not consult a potentially changed binding.
    private MemoryFileMutationScope BeginMemoryFileMutation(nint page, long length, bool logical)
        => new(memoryFileMutation, page, length, logical);

    private struct MemoryFileMutationScope : IDisposable
    {
        private IHostMemoryFileMutation? observer;

        internal MemoryFileMutationScope(IHostMemoryFileMutation? observer, nint page, long length, bool logical)
        {
            this.observer = null;
            if (observer == null) return;
            observer.BeginMemoryFileMutation(page, length, logical);
            this.observer = observer;
        }

        public void Dispose()
        {
            var admitted = observer;
            observer = null;
            admitted?.EndMemoryFileMutation();
        }
    }
}
