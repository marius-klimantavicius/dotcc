namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    // Only in the mapping assist: Gate + passive physical lifetime, native has
    // returned. IO.sync is never acquired by native execution or its exit. The
    // Host observer closes admission/advances epochs/drains native without Gate.
    internal bool SharedReadBackingOwned(ulong lease, nint page)
        => io.MemoryFileSharedReadOwned(lease, page, this);
}
