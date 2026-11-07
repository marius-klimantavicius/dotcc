using global::System;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe long blink_host_pwrite(int fd,void* source,ulong length,long offset)
    {
        try {
            if(io==null)return IoError(19);
            if(source==null && length!=0)return IoError(14);
            int count=(int)global::System.Math.Min(length,(ulong)IoChunk);
            using var staging = new GuestIoBuffer(count);
            byte[] buffer = staging.Bytes;
            SnapshotGuestBytes(source, buffer);
            return IoResult(io.WriteAt(fd,buffer,offset));
        } catch(Exception e){return IoException(e);}
    }
    public static int blink_host_ftruncate(int fd,long length)
    {
        try {return io==null?IoError(19):(int)IoResult(io.TruncateFile(fd,length));}
        catch(Exception e){return IoException(e);}
    }
    public static unsafe int blink_host_truncate(byte* path,long length)
    {
        try {
            if(io==null)return IoError(19);
            if(path==null)return IoError(14);
            if (!ReadGuestPath(path, out string name)) return -1;
            return (int)IoResult(io.TruncateFile(name,length));
        }catch(Exception e){return IoException(e);}
    }
    public static int blink_host_fsync(int fd)
    {
        try{return io==null?IoError(19):(int)IoResult(io.SynchronizeFile(fd));}
        catch(Exception e){return IoException(e);}
    }
    public static int blink_host_fdatasync(int fd)=>blink_host_fsync(fd);
}
