namespace Managed.Emulation.Host;

/// <summary>One private process namespace. UID/GID zero matches private file
/// ownership; it grants no host credentials or host process capabilities.</summary>
public sealed class HostIdentity
{
    public HostIdentity(int processId = 1, int parentProcessId = 0, string? hostName = null)
    {
        if (processId <= 0) throw new ArgumentOutOfRangeException(nameof(processId));
        if (parentProcessId < 0 || parentProcessId == processId) throw new ArgumentOutOfRangeException(nameof(parentProcessId));
        hostName ??= "blink-" + processId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (hostName.Length is < 1 or > 63 || hostName.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '.'))
            throw new ArgumentException("Host name must contain 1–63 ASCII letters, digits, dots or hyphens.", nameof(hostName));
        ProcessId = processId; ParentProcessId = parentProcessId;
        HostName = hostName;
    }
    public int ProcessId { get; }
    public int ParentProcessId { get; }
    public string HostName { get; }
    public uint UserId => 0;
    public uint GroupId => 0;
}
