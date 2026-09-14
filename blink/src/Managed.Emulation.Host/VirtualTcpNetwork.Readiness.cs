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
                short ready = 0;
                bool reading = entry.Socket.Poll(0, SelectMode.SelectRead);
                bool writing = entry.Socket.Poll(0, SelectMode.SelectWrite);
                bool error = entry.Socket.Poll(0, SelectMode.SelectError);
                if (reading) ready |= (short)(requested & (1 | 64));
                if (writing) ready |= (short)(requested & (4 | 256));
                if (error) ready |= 8;
                if (!entry.Listening && entry.Remote == null) ready |= 16;
                else if (!entry.Listening && entry.WriteShutdown && reading && entry.Socket.Available == 0) ready |= 16;
                return HostResult<short>.Success(ready);
            }
            catch (SocketException error) { return Fail<short>(ConvertError(error)); }
            catch (ObjectDisposedException) { return Fail<short>(GuestError.BadDescriptor); }
        }
    }
}
