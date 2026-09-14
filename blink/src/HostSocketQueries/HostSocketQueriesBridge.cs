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
