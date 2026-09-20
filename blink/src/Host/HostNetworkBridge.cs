using global::System;
using global::System.Buffers.Binary;
using global::System.Net.Sockets;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    private static unsafe bool ReadEndpoint(blink_host_sockaddr* address, uint length, out Managed.Emulation.Host.GuestEndpoint endpoint)
    {
        endpoint = default;
        if (address == null) { IoError(14); return false; }
        if (length < 16) { IoError(22); return false; }
        byte* bytes = (byte*)address;
        if (bytes[0] != 2 || bytes[1] != 0) { IoError(97); return false; }
        endpoint = new(((uint)bytes[4] << 24) | ((uint)bytes[5] << 16) | ((uint)bytes[6] << 8) | bytes[7], (ushort)((bytes[2] << 8) | bytes[3]));
        return true;
    }
    private static unsafe void WriteEndpoint(blink_host_sockaddr* address, uint* length, Managed.Emulation.Host.GuestEndpoint endpoint)
    {
        Span<byte> bytes = stackalloc byte[16]; bytes.Clear();
        bytes[0] = 2;
        bytes[2] = (byte)(endpoint.Port >> 8); bytes[3] = (byte)endpoint.Port;
        bytes[4] = (byte)(endpoint.Address >> 24); bytes[5] = (byte)(endpoint.Address >> 16);
        bytes[6] = (byte)(endpoint.Address >> 8); bytes[7] = (byte)endpoint.Address;
        int count = (int)global::System.Math.Min(*length, 16u);
        bytes[..count].CopyTo(new Span<byte>(address, count)); *length = 16;
    }
    public static int blink_host_socket(int domain, int type, int protocol)
    {
        try
        {
            if (io == null) return IoError(19);
            if (domain != 2) return IoError(97);
            if (type != 1 || protocol is not (0 or 6)) return IoError(95);
            return (int)IoResult(io.Socket());
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_bind(int fd, blink_host_sockaddr* address, uint length)
    {
        try
        {
            if (io == null) return IoError(19);
            if (!ReadEndpoint(address, length, out var endpoint)) return -1;
            var result = io.Bind(fd, endpoint);
            return result.Succeeded ? 0 : IoError((int)result.Error);
        }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_host_listen(int fd, int backlog)
    {
        try { return io == null ? IoError(19) : (int)IoResult(io.Listen(fd, backlog)); }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_accept(int fd, blink_host_sockaddr* address, uint* length)
    {
        try
        {
            if (io == null) return IoError(19);
            if (address != null && length == null) return IoError(14);
            var result = io.AcceptAsync(fd, ioCancellation).GetAwaiter().GetResult();
            if (!result.Succeeded) return IoError((int)result.Error);
            if (address != null) WriteEndpoint(address, length, result.Value.Remote);
            return result.Value.Handle;
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_connect(int fd, blink_host_sockaddr* address, uint length)
    {
        try
        {
            if (io == null) return IoError(19);
            if (!ReadEndpoint(address, length, out var endpoint)) return -1;
            return (int)IoResult(io.ConnectAsync(fd, endpoint, ioCancellation).GetAwaiter().GetResult());
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_getsockname(int fd, blink_host_sockaddr* address, uint* length)
    {
        try
        {
            if (io == null) return IoError(19);
            if (address == null || length == null) return IoError(14);
            var result = io.LocalEndpoint(fd);
            if (!result.Succeeded) return IoError((int)result.Error);
            WriteEndpoint(address, length, result.Value); return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe long blink_host_send(int fd, void* source, ulong length, int flags)
        => SocketTransfer(fd, source, length, flags, true);
    public static unsafe long blink_host_recv(int fd, void* destination, ulong length, int flags)
        => SocketTransfer(fd, destination, length, flags, false);
    private static unsafe long SocketTransfer(int fd, void* pointer, ulong length, int flags, bool writing)
    {
        try
        {
            if (io == null) return IoError(19);
            if (flags != 0) return IoError(95);
            if (pointer == null && length != 0) return IoError(14);
            int count = (int)global::System.Math.Min(length, (ulong)IoChunk);
            byte[] buffer = new byte[count];
            if (writing) new ReadOnlySpan<byte>(pointer, count).CopyTo(buffer);
            var result = (writing ? io.SendAsync(fd, buffer, ioCancellation) : io.ReceiveAsync(fd, buffer, ioCancellation)).GetAwaiter().GetResult();
            if (!writing && result.Succeeded) buffer.AsSpan(0, result.Value).CopyTo(new Span<byte>(pointer, count));
            return IoResult(result);
        }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_host_shutdown(int fd, int direction)
    {
        try
        {
            if (io == null) return IoError(19);
            if ((uint)direction > 2) return IoError(22);
            return (int)IoResult(io.Shutdown(fd, (SocketShutdown)direction));
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_setsockopt(int fd, int level, int option, void* value, uint length)
    {
        try
        {
            if (io == null) return IoError(19);
            if (value == null) return IoError(14);
            if (level == 1 && option is 20 or 21)
            {
                // Reviewed LP64 timeval: signed seconds and microseconds, 16 bytes.
                if (length < 16) return IoError(22);
                var timeoutBytes = new ReadOnlySpan<byte>(value, 16);
                var timeout = new Managed.Emulation.Host.SocketTimeout(
                    BinaryPrimitives.ReadInt64LittleEndian(timeoutBytes),
                    BinaryPrimitives.ReadInt64LittleEndian(timeoutBytes[8..]));
                return (int)IoResult(io.SetSocketTimeout(fd, option == 20, timeout));
            }
            if (length < 4) return IoError(22);
            Managed.Emulation.Host.TcpHostOption selected;
            if (level == 1 && option == 2) selected = Managed.Emulation.Host.TcpHostOption.ReuseAddress;
            else if (level == 1 && option == 7) selected = Managed.Emulation.Host.TcpHostOption.SendBufferSize;
            else if (level == 1 && option == 8) selected = Managed.Emulation.Host.TcpHostOption.ReceiveBufferSize;
            else if (level == 6 && option == 1) selected = Managed.Emulation.Host.TcpHostOption.NoDelay;
            else return IoError(92);
            byte* bytes = (byte*)value;
            int setting = bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24;
            return (int)IoResult(io.SetOption(fd, selected, setting));
        }
        catch (Exception error) { return IoException(error); }
    }
}
