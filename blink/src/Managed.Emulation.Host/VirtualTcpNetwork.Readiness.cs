using System.Net.Sockets;

namespace Managed.Emulation.Host;

public sealed partial class VirtualTcpNetwork
{
    internal HostResult<short> Readiness(int handle, short requested)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<short>(GuestError.BadDescriptor);
            try
            {
                uint snapshot = SocketReadiness(entry);
                short ready = (short)(snapshot & (8 | 16));
                if ((snapshot & 1) != 0) ready |= (short)(requested & (1 | 64));
                if ((snapshot & 4) != 0) ready |= (short)(requested & (4 | 256));
                return HostResult<short>.Success(ready);
            }
            catch (SocketException error) { return Fail<short>(ConvertError(error)); }
            catch (ObjectDisposedException) { return Fail<short>(GuestError.BadDescriptor); }
        }
    }

    // All snapshots and connect state transitions hold sync. BCL Poll does not
    // consume SO_ERROR; only the guest option getter clears our retained error.
    private static uint SocketReadiness(Entry entry)
    {
        if (entry.Connection == ConnectionState.Connecting) return 0;
        if (entry.Connection == ConnectionState.Closed) return 16;
        if (entry.Connection == ConnectionState.Failed)
            return 1u | 4u | 16u | (entry.PendingError == GuestError.None ? 0u : 8u);
        if (!entry.Listening && entry.Connection == ConnectionState.Unconnected) return 16;
        uint ready = 0;
        bool reading = entry.Socket.Poll(0, SelectMode.SelectRead);
        if (reading) ready |= 1;
        if (!entry.Listening && entry.Socket.Poll(0, SelectMode.SelectWrite)) ready |= 4;
        if (entry.Socket.Poll(0, SelectMode.SelectError)) ready |= 8;
        if (!entry.Listening && entry.WriteShutdown && reading && entry.Socket.Available == 0) ready |= 16;
        return ready;
    }

}
