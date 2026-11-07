namespace Managed.Emulation.Host;

/// <summary>Optional authentication for retained native reads of local memfd pages.</summary>
/// <remarks>The mutation observer returns the exact authority whose native frames
/// its Begin callback closes and drains before bytes, EOF, lease release or free.
/// The identity is stable for the observer lifetime. This getter must not lock,
/// allocate, reenter I/O or acquire authority/store locks: it runs under IO.sync.
/// Implementing this contract grants no pointer or write permission.</remarks>
public interface IHostMemoryFileNativeReadOwner
{
    object MemoryFileNativeReadAuthority { get; }
}
