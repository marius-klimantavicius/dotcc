using global::System;
using global::System.Buffers.Binary;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe int blink_host_getsockopt(int fd,int level,int option,void* value,uint* length)
    {
        try
        {
            if(io==null)return IoError(19);
            if(length==null)return IoError(14);
            uint available = 0; SnapshotGuestBytes(length, new Span<byte>(&available, 4));
            if(value==null && available!=0)return IoError(14);
            if (level == 1 && option == 13)
            {
                var linger = io.GetSocketLinger(fd);
                if (!linger.Succeeded) return IoError((int)linger.Error);
                Span<byte> encoded = stackalloc byte[8];
                BinaryPrimitives.WriteInt32LittleEndian(encoded, linger.Value.Enabled ? 1 : 0);
                BinaryPrimitives.WriteInt32LittleEndian(encoded[4..], linger.Value.Seconds);
                int copied = (int)global::System.Math.Min(available, 8u);
                StoreGuestBytes(value, encoded[..copied]);
                uint written = (uint)copied; StoreGuestBytes(length, new ReadOnlySpan<byte>(&written, 4));
                return 0;
            }
            if (level == 1 && option is 20 or 21)
            {
                var timeout = io.GetSocketTimeout(fd, option == 20);
                if (!timeout.Succeeded) return IoError((int)timeout.Error);
                Span<byte> encoded = stackalloc byte[16];
                BinaryPrimitives.WriteInt64LittleEndian(encoded, timeout.Value.Seconds);
                BinaryPrimitives.WriteInt64LittleEndian(encoded[8..], timeout.Value.Microseconds);
                int copied = (int)global::System.Math.Min(available, 16u);
                StoreGuestBytes(value, encoded[..copied]);
                uint written = (uint)copied; StoreGuestBytes(length, new ReadOnlySpan<byte>(&written, 4));
                return 0;
            }
            var result=io.GetSocketOption(fd,level,option);
            if(!result.Succeeded)return IoError((int)result.Error);
            Span<byte> bytes=stackalloc byte[4];BinaryPrimitives.WriteInt32LittleEndian(bytes,result.Value);
            int count=(int)global::System.Math.Min(available,4u);
            StoreGuestBytes(value, bytes[..count]);
            uint writtenCount = (uint)count; StoreGuestBytes(length, new ReadOnlySpan<byte>(&writtenCount, 4));
            return 0;
        }
        catch(Exception e){return IoException(e);}
    }
}
