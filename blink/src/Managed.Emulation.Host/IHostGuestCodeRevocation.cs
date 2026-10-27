namespace Managed.Emulation.Host;

/// <summary>Optional owner callback before a guest data pointer escapes or a
/// mapping changes. Drain native execution before returning. Initially enrolled
/// code is permanently revoked before overlapping content, identity or lifetime
/// changes. Dynamic data descriptors additionally require a live mapping mutex
/// through native return; invalidation alone grants no lifetime or pointer lease.
/// Never retain the native gate across the caller's subsequent helper or syscall.
/// Bind only for the audited managed private-backing protocol.</summary>
public interface IHostGuestCodeRevocation
{
    /// <summary>Data-pointer exposure permanently revokes initial code only.
    /// Private data contents may remain concurrently mutable.</summary>
    void Revoke(long address, long size);

    /// <summary>Before mapping permissions, identity or lifetime can change,
    /// drain native execution, invalidate data descriptors and permanently revoke
    /// overlapping initial code. Live mapping writers retain their mapping mutex
    /// through the operation; loader/final cleanup require owner quiescence.
    /// Addresses are intersected with the lower user half before either callback.</summary>
    void RevokeMapping(long address, long size);
}
