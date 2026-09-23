using System.Net.Sockets;

namespace Managed.Emulation.Host;

public readonly record struct SocketLinger(bool Enabled, int Seconds);
internal readonly record struct SocketEdgeReadiness(uint Events, ulong ReadEpoch, ulong WriteEpoch);

public sealed partial class VirtualTcpNetwork
{
    public HostResult<int> SetNonBlocking(int handle, bool enabled)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<int>(GuestError.BadDescriptor);
            entry.NonBlocking = enabled;
            return HostResult<int>.Success(0);
        }
    }

    public HostResult<int> SetLinger(int handle, SocketLinger value)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<int>(GuestError.BadDescriptor);
            if (value.Seconds is < 0 or > 65535) return Fail<int>(GuestError.Invalid);
            // The observed Kestrel listener shutdown selects enabled zero linger.
            // Positive durations need a separately qualified close/deadline contract.
            if (value.Seconds != 0) return Fail<int>(GuestError.Unsupported);
            try
            {
                entry.Socket.LingerState = new(value.Enabled, value.Seconds);
                return HostResult<int>.Success(0);
            }
            catch (SocketException error) { return Fail<int>(ConvertError(error)); }
        }
    }

    public HostResult<SocketLinger> GetLinger(int handle)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<SocketLinger>(GuestError.BadDescriptor);
            try
            {
                var value = entry.Socket.LingerState!;
                return HostResult<SocketLinger>.Success(new(value.Enabled, value.LingerTime));
            }
            catch (SocketException error) { return Fail<SocketLinger>(ConvertError(error)); }
        }
    }

    internal HostResult<SocketEdgeReadiness> EdgeReadiness(int handle)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<SocketEdgeReadiness>(GuestError.BadDescriptor);
            try
            {
                uint ready = SocketReadiness(entry);
                return HostResult<SocketEdgeReadiness>.Success(new(ready, entry.ReadEpoch, entry.WriteEpoch));
            }
            catch (SocketException error) { return Fail<SocketEdgeReadiness>(ConvertError(error)); }
            catch (ObjectDisposedException) { return Fail<SocketEdgeReadiness>(GuestError.BadDescriptor); }
        }
    }
}
