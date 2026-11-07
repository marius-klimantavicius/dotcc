namespace Managed.Emulation.Host;

/// <summary>Actual-use exclusion for bounded synchronous guest-memory copies.
/// Pointer lifetime remains the caller's responsibility. Never retain admission
/// across I/O, guest execution, or a wait. Begin runs before acquiring any I/O or
/// futex lock; writes cover at most 4096 bytes. End cannot throw or allocate;
/// an invariant failure must quarantine execution before admission can reopen.</summary>
public interface IHostGuestMemoryCopies
{
    void BeginGuestRead();
    void EndGuestRead();
    void BeginGuestWrite(nint address, int count);
    void EndGuestWrite();
    /// <summary>Reserve owned staging bytes against the configured instance
    /// quota before allocation. False becomes guest ENOMEM. Includes allocation
    /// header and bookkeeping allowances and lasts through the actual free.</summary>
    bool TryReserveGuestCopyBytes(ulong count);
    /// <summary>Release exactly the charged allocation after its actual free.
    /// No current worker is required. Cannot throw/allocate; imbalance quarantines.</summary>
    void ReleaseGuestCopyBytes(ulong count);
}
