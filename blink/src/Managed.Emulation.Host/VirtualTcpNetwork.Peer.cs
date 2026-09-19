namespace Managed.Emulation.Host;

public sealed partial class VirtualTcpNetwork
{
    public HostResult<GuestEndpoint> PeerEndpoint(int handle)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<GuestEndpoint>(GuestError.BadDescriptor);
            return entry.Remote is { } remote ? HostResult<GuestEndpoint>.Success(remote)
                : Fail<GuestEndpoint>(GuestError.NotConnected);
        }
    }
}
