namespace Managed.Emulation.Host;

/// <summary>One private process namespace. UID/GID zero matches private file
/// ownership; it grants no host credentials or host process capabilities.</summary>
public sealed class HostIdentity
{
    public HostIdentity(int processId = 1, int parentProcessId = 0)
    {
        if (processId <= 0) throw new ArgumentOutOfRangeException(nameof(processId));
        if (parentProcessId < 0 || parentProcessId == processId) throw new ArgumentOutOfRangeException(nameof(parentProcessId));
        ProcessId = processId; ParentProcessId = parentProcessId;
    }
    public int ProcessId { get; }
    public int ParentProcessId { get; }
    public uint UserId => 0;
    public uint GroupId => 0;
}
