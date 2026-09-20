using global::System;
using global::System.Buffers.Binary;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe int blink_host_getsockopt(int fd,int level,int option,void* value,uint* length)
    {
        try
        {
            if(io==null)return IoError(19);
            if(length==null || value==null && *length!=0)return IoError(14);
            if (level == 1 && option == 13)
            {
                var linger = io.GetSocketLinger(fd);
                if (!linger.Succeeded) return IoError((int)linger.Error);
                Span<byte> encoded = stackalloc byte[8];
                BinaryPrimitives.WriteInt32LittleEndian(encoded, linger.Value.Enabled ? 1 : 0);
                BinaryPrimitives.WriteInt32LittleEndian(encoded[4..], linger.Value.Seconds);
                int copied = (int)global::System.Math.Min(*length, 8u);
                encoded[..copied].CopyTo(new Span<byte>(value, copied));
                *length = (uint)copied;
                return 0;
            }
            if (level == 1 && option is 20 or 21)
            {
                var timeout = io.GetSocketTimeout(fd, option == 20);
                if (!timeout.Succeeded) return IoError((int)timeout.Error);
                Span<byte> encoded = stackalloc byte[16];
                BinaryPrimitives.WriteInt64LittleEndian(encoded, timeout.Value.Seconds);
                BinaryPrimitives.WriteInt64LittleEndian(encoded[8..], timeout.Value.Microseconds);
                int copied = (int)global::System.Math.Min(*length, 16u);
                encoded[..copied].CopyTo(new Span<byte>(value, copied));
                *length = (uint)copied;
                return 0;
            }
            var result=io.GetSocketOption(fd,level,option);
            if(!result.Succeeded)return IoError((int)result.Error);
            Span<byte> bytes=stackalloc byte[4];BinaryPrimitives.WriteInt32LittleEndian(bytes,result.Value);
            int count=(int)global::System.Math.Min(*length,4u);
            bytes[..count].CopyTo(new Span<byte>(value,count));*length=(uint)count;
            return 0;
        }
        catch(Exception e){return IoException(e);}
    }
}
