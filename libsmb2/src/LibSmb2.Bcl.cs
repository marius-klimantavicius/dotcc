using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using C = Managed.Smb.LibSmb2.Libc;

namespace Managed.Smb;

public static unsafe partial class LibSmb2
{
    // The bundled C declarations retain int parameters. These audited adapters are
    // the only boundary between those declarations and the typed host registry.
    public static int HostSocket(int domain, int type, int protocol) => (int)HostSockets.Create(domain, type, protocol);
    public static int HostClose(int descriptor) => HostSockets.Close((t_socket)descriptor);
    public static void HostSetNonblocking(t_socket descriptor) => HostSockets.SetNonblocking(descriptor);
    public static int HostShutdown(int descriptor, int how) => HostSockets.Shutdown((t_socket)descriptor, how);
    public static int HostConnect(int descriptor, sockaddr* address, uint length)
    {
        if (address == null) { C.errno = C.EFAULT; return -1; }
        IPEndPoint endpoint;
        if (address->sa_family == AF_INET && length >= sizeof(sockaddr_in))
        {
            var ipv4 = (sockaddr_in*)address;
            endpoint = new(new IPAddress(new ReadOnlySpan<byte>(&ipv4->sin_addr.s_addr, 4)),
                BinaryPrimitives.ReadUInt16BigEndian(new ReadOnlySpan<byte>(&ipv4->sin_port, 2)));
        }
        else if (address->sa_family == AF_INET6 && length >= sizeof(sockaddr_in6))
        {
            var ipv6 = (sockaddr_in6*)address;
            endpoint = new(new IPAddress(new ReadOnlySpan<byte>(&ipv6->sin6_addr, 16), ipv6->sin6_scope_id),
                BinaryPrimitives.ReadUInt16BigEndian(new ReadOnlySpan<byte>(&ipv6->sin6_port, 2)));
        }
        else { C.errno = C.EAFNOSUPPORT; return -1; }
        return HostSockets.Connect((t_socket)descriptor, endpoint);
    }
    public static long HostReadv(int descriptor, iovec* vectors, int count) => TransferVectors(descriptor, vectors, count, false);
    public static long HostWritev(int descriptor, iovec* vectors, int count) => TransferVectors(descriptor, vectors, count, true);
    private static long TransferVectors(int descriptor, iovec* vectors, int count, bool writing)
    {
        if (count < 0 || (count > 0 && vectors == null)) { C.errno = C.EINVAL; return -1; }
        long total = 0;
        for (int i = 0; i < count; i++)
        {
            if (vectors[i].iov_len == 0) continue;
            if (vectors[i].iov_base == null) { C.errno = C.EFAULT; return total == 0 ? -1 : total; }
            int length = (int)Math.Min(vectors[i].iov_len, (ulong)HostSockets.BufferCapacity);
            int copied = writing
                ? HostSockets.Write((t_socket)descriptor, new ReadOnlySpan<byte>(vectors[i].iov_base, length))
                : HostSockets.Read((t_socket)descriptor, new Span<byte>(vectors[i].iov_base, length));
            if (copied < 0) return total == 0 ? -1 : total;
            total += copied;
            if ((ulong)copied < vectors[i].iov_len) break;
        }
        return total;
    }
    public static long HostRecv(int descriptor, void* buffer, ulong length, int flags)
    {
        if (flags != 0) { C.errno = C.EOPNOTSUPP; return -1; }
        if (buffer == null && length != 0) { C.errno = C.EFAULT; return -1; }
        return HostSockets.Read((t_socket)descriptor, new Span<byte>(buffer, (int)Math.Min(length, (ulong)HostSockets.BufferCapacity)));
    }
    public static long HostSend(int descriptor, void* buffer, ulong length, int flags)
    {
        if (flags != 0) { C.errno = C.EOPNOTSUPP; return -1; }
        if (buffer == null && length != 0) { C.errno = C.EFAULT; return -1; }
        return HostSockets.Write((t_socket)descriptor, new ReadOnlySpan<byte>(buffer, (int)Math.Min(length, (ulong)HostSockets.BufferCapacity)));
    }
    public static int HostGetsockopt(int descriptor, int level, int name, void* value, uint* length)
    {
        if (length == null || value == null || *length < sizeof(int)) { C.errno = C.EINVAL; return -1; }
        if (level != SOL_SOCKET || name != SO_ERROR) { C.errno = C.ENOPROTOOPT; return -1; }
        int result = HostSockets.GetError((t_socket)descriptor, out int error);
        if (result == 0) { *(int*)value = error; *length = sizeof(int); }
        return result;
    }
    public static int HostSetsockopt(int descriptor, int level, int name, void* value, uint length)
    {
        if (value == null || length < sizeof(int)) { C.errno = C.EINVAL; return -1; }
        if (level == SOL_SOCKET && name == SO_LINGER)
        {
            if (length < sizeof(linger)) { C.errno = C.EINVAL; return -1; }
            var option = (linger*)value;
            return HostSockets.SetOption((t_socket)descriptor, level, name, option->l_onoff, option->l_linger);
        }
        return HostSockets.SetOption((t_socket)descriptor, level, name, *(int*)value);
    }
    public static int HostGetaddrinfo(byte* node, byte* service, addrinfo* hints, addrinfo** result)
        => HostSockets.GetAddresses(node, service, hints, result);
    public static void HostFreeaddrinfo(addrinfo* value) => HostSockets.FreeAddresses(value);

    // Server hosting and C synchronous wait loops are deliberately unsupported in
    // this callback-only product profile. They never reach Libc's fd registry.
    public static int HostBind(int fd, sockaddr* address, uint length) { C.errno = C.EOPNOTSUPP; return -1; }
    public static int HostListen(int fd, int backlog) { C.errno = C.EOPNOTSUPP; return -1; }
    public static int HostAccept(int fd, sockaddr* address, uint* length) { C.errno = C.EOPNOTSUPP; return -1; }
    public static int HostPoll(pollfd* fds, ulong count, int timeout) { C.errno = C.EOPNOTSUPP; return -1; }
}
