namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    /// <summary>Outstanding owned TCP operations, including blocking receives.</summary>
    public int PendingSocketOperations => network.PendingOperations;

    /// <summary>The peer in this owner's virtual IPv4 namespace.</summary>
    public HostResult<GuestEndpoint> PeerEndpoint(int fd) => SocketCall(fd, network.PeerEndpoint);
}
