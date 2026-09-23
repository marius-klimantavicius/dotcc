using System;
using System.Net.Sockets;
using static Managed.Smb.LibSmb2;
using C = Managed.Smb.LibSmb2.Libc;

namespace Managed.Smb;

public static partial class HostSockets
{
    public static int GetError(t_socket handle, out int error)
    {
        error = 0;
        var entry = Find(handle);
        if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Closed) { C.errno = C.EBADF; return -1; }
            error = entry.Error;
            return 0;
        }
    }
    public static int SetNonblocking(t_socket handle)
    {
        var entry = Find(handle);
        if (entry is null) return -1;
        lock (entry.Gate) entry.Flags |= O_NONBLOCK;
        return 0;
    }
    public static int SetOption(t_socket handle, int level, int name, int value, int lingerSeconds = 0)
    {
        var entry = Find(handle);
        if (entry is null) return -1;
        try
        {
            lock (entry.Gate)
            {
                if (entry.Closed) { C.errno = C.EBADF; return -1; }
                if (level == IPPROTO_TCP && name == TCP_NODELAY) entry.Socket.NoDelay = value != 0;
                else if (level == SOL_SOCKET && name == SO_REUSEADDR) entry.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value);
                else if (level == SOL_SOCKET && name == SO_LINGER) entry.Socket.LingerState = new(value != 0, lingerSeconds);
                else if (level == SOL_SOCKET && name == SO_RCVBUF) entry.Socket.ReceiveBufferSize = value;
                else if (level == SOL_SOCKET && name == SO_SNDBUF) entry.Socket.SendBufferSize = value;
                else { C.errno = C.ENOPROTOOPT; return -1; }
                return 0;
            }
        }
        catch (Exception ex) when (IsIoException(ex) || ex is ArgumentException) { C.errno = ErrorCode(ex); return -1; }
    }
    public static int Shutdown(t_socket handle, int how)
    {
        var entry = Find(handle);
        if (entry is null) return -1;
        if (how != SHUT_RD && how != SHUT_WR && how != SHUT_RDWR) { C.errno = C.EINVAL; return -1; }
        // The product path closes owned sockets; half-close cannot discard queued sends.
        C.errno = C.EOPNOTSUPP;
        return -1;
    }
}
