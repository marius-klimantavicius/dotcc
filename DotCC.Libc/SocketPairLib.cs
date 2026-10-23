#nullable enable
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Create real unnamed Linux Unix-domain peers, then transfer their
    /// handles into the ordinary BCL-backed virtual descriptor table. Other
    /// platforms report ENOTSUP; no pathname/TCP substitute is used.</summary>
    public static int socketpair(int domain, int type, int protocol, int* descriptors)
    {
        if (descriptors == null) { errno = EFAULT; return -1; }
        if (!OperatingSystem.IsLinux()) { errno = ENOTSUP; return -1; }
        int* native = stackalloc int[2];
        if (LinuxSocketPair(domain, type, protocol, native) != 0)
        { errno = Marshal.GetLastPInvokeError(); return -1; }
        int firstRaw = native[0], secondRaw = native[1], firstFd = -1;
        SafeSocketHandle? firstHandle = null, secondHandle = null;
        Socket? first = null, second = null;
        try
        {
            firstHandle = new SafeSocketHandle((nint)firstRaw, ownsHandle: true); firstRaw = -1;
            secondHandle = new SafeSocketHandle((nint)secondRaw, ownsHandle: true); secondRaw = -1;
            first = new Socket(firstHandle);
            second = new Socket(secondHandle);
            first.Blocking = second.Blocking = (type & SOCK_NONBLOCK) == 0;
            int flags = (type & SOCK_CLOEXEC) == 0 ? 0 : 1;
            firstFd = RegisterSocketSlot(first, flags);
            int secondFd = RegisterSocketSlot(second, flags);
            descriptors[0] = firstFd; descriptors[1] = secondFd;
            first = second = null;
            firstHandle = secondHandle = null;
            return 0;
        }
        catch (Exception error)
        {
            if (firstFd >= 0) close(firstFd);
            errno = error is SocketException socketError ? SocketErrno(socketError.SocketErrorCode)
                : error is OutOfMemoryException ? ENOMEM : EINVAL;
            return -1;
        }
        finally
        {
            first?.Dispose(); second?.Dispose(); firstHandle?.Dispose(); secondHandle?.Dispose();
            if (firstRaw >= 0) LinuxPairClose(firstRaw);
            if (secondRaw >= 0) LinuxPairClose(secondRaw);
        }
    }

    [DllImport("libc", EntryPoint = "socketpair", SetLastError = true)]
    private static extern int LinuxSocketPair(int domain, int type, int protocol, int* descriptors);
    [DllImport("libc", EntryPoint = "close")]
    private static extern int LinuxPairClose(int descriptor);
}
