namespace Managed.Emulation.Host;

/// <summary>Execution-owner barrier for mutations of leased memory-file backing.</summary>
/// <remarks>
/// Both callbacks run synchronously under the InstanceIo gate, never under the
/// static memory-lease registry gate. They must not acquire the execution owner's
/// authority gate, fetch guest memory, reenter I/O, or wait for work that needs I/O.
/// Begin must revoke admission before waiting only for bounded native frames that
/// cannot touch I/O. Concurrent instances and nested scopes require a writer count,
/// not a Boolean. The observer remains bound until the execution owner is quiescent.
/// </remarks>
public interface IHostMemoryFileMutation
{
    /// <summary>Admit a writer before any backing mutation.</summary>
    /// <param name="page">Aligned physical page address, or zero for a logical change.</param>
    /// <param name="length">Physical byte count (whole pages), or zero if no range is supplied.</param>
    /// <param name="logical">EOF, backing identity, or reclamation may change;
    /// the observer must conservatively invalidate even without a physical range.</param>
    /// <remarks>A successful return is paired with exactly one End call. If Begin
    /// throws, it must undo its own admission; the host has not mutated backing and
    /// will not call End. Admission and revocation must not allocate.</remarks>
    void BeginMemoryFileMutation(nint page, long length, bool logical);

    /// <summary>Release this writer admission; must not throw or allocate.</summary>
    void EndMemoryFileMutation();
}
