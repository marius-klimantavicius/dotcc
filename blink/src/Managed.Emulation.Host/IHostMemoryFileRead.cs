namespace Managed.Emulation.Host;

/// <summary>Excludes native atomic updates during a bounded published-backing
/// copy, including reads on host threads without a current guest worker.</summary>
public interface IHostMemoryFileRead
{
    /// <summary>Nonblocking, allocation-free admission. Called under InstanceIo's
    /// gate, never its static lease gate. False permits no byte access; the caller
    /// must release the I/O gate before retrying. No authority lock or wait.</summary>
    bool TryBeginMemoryFileRead();

    /// <summary>Release a successful admission, without throwing or allocating.
    /// Invariant failure quarantines execution; it cannot reopen admission.</summary>
    void EndMemoryFileRead();
}
