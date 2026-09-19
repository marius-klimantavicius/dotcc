using global::System;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe int blink_host_getpeername(int fd, blink_host_sockaddr* address, uint* length)
    {
        try
        {
            if (io == null) return IoError(19);
            var result = io.PeerEndpoint(fd);
            if (!result.Succeeded) return IoError((int)result.Error);
            if (length == null || address == null && *length != 0) return IoError(14);
            WriteEndpoint(address, length, result.Value);
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }

    public static unsafe long blink_host_sendmsg(int fd, blink_host_msghdr* message, int flags)
        => MessageTransfer(fd, message, flags, true);
    public static unsafe long blink_host_recvmsg(int fd, blink_host_msghdr* message, int flags)
        => MessageTransfer(fd, message, flags, false);

    private static unsafe long MessageTransfer(int fd, blink_host_msghdr* message, int flags, bool writing)
    {
        try
        {
            if (io == null) return IoError(19);
            var peer = io.PeerEndpoint(fd);
            if (!peer.Succeeded) return IoError((int)peer.Error);
            if (message == null) return IoError(14);
            // Refuse before accessing any control or explicit destination bytes.
            // TCP has no supported ancillary channel in this private network.
            if (flags != 0 || message->msg_controllen != 0) return IoError(95);
            if (writing && message->msg_name != null) return IoError(106);
            ulong vectorCount = message->msg_iovlen;
            if (vectorCount > IoVectorLimit) return IoError(90); // EMSGSIZE
            blink_host_iovec* vectors = message->msg_iov;
            if (vectorCount != 0 && vectors == null) return IoError(14);
            if (vectorCount * (ulong)sizeof(blink_host_iovec) > (ulong)nuint.MaxValue - (ulong)(nuint)vectors)
                return IoError(14);
            ulong total = 0;
            for (int i = 0; i < (int)vectorCount; ++i)
            {
                ulong length = vectors[i].iov_len;
                if (length != 0 && vectors[i].iov_base == null) return IoError(14);
                if (length > (ulong)long.MaxValue - total) return IoError(22);
                if (length > (ulong)nuint.MaxValue - (ulong)(nuint)vectors[i].iov_base) return IoError(14);
                total += length;
            }
            int count = (int)global::System.Math.Min(total, (ulong)IoChunk);
            byte[] buffer = new byte[count];
            if (writing) CopyVectors(vectors, (int)vectorCount, buffer, true);
            // Preserve native stream readiness even for zero-capacity receives:
            // Linux recvmsg may wait until data or EOF is observable.
            var result = (writing ? io.SendAsync(fd, buffer, ioCancellation) : io.ReceiveAsync(fd, buffer, ioCancellation)).GetAwaiter().GetResult();
            if (!result.Succeeded) return IoError((int)result.Error);
            int transferred = result.Value;
            if (!writing)
            {
                CopyVectors(vectors, (int)vectorCount, buffer.AsSpan(0, transferred), false);
                // Native TCP recvmsg does not return a source sockaddr. All
                // result metadata is committed only after a successful transfer.
                message->msg_namelen = 0;
                message->msg_controllen = 0;
                message->msg_flags = 0;
            }
            return transferred;
        }
        catch (Exception error) { return IoException(error); }
    }
}
