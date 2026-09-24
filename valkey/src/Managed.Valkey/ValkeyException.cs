namespace Managed.Valkey;

public class ValkeyException : Exception
{
    public ValkeyException(string message, Exception? innerException = null) : base(message, innerException) { }
}

/// <summary>The requested upstream shutdown failed. The instance remains available.</summary>
public sealed class ValkeyStopException(string message) : ValkeyException(message);

/// <summary>The runtime owner was retained because its cleanup could not be proved complete.</summary>
public sealed class ValkeyCleanupException(Guid instanceId, string message, Exception? innerException = null)
    : ValkeyException(message, innerException)
{
    public Guid InstanceId { get; } = instanceId;
}
