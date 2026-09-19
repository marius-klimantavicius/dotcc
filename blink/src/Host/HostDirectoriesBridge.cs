using global::System;
using global::System.Text;
using Managed.Emulation.Host;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    [ThreadStatic] private static HostDirectories? directories;
    public static void BindHostDirectories(HostDirectories owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if(directories!=null)throw new InvalidOperationException("Private directories already bound on this worker.");
        directories=owner;
    }
    public static void UnbindHostDirectories()=>directories=null;
    private static unsafe DIR* DirectoryError(int error){Libc.errno=error;return null;}
    public static unsafe DIR* blink_host_fdopendir(int fd)
    {
        try{if(directories==null)return DirectoryError(19);var result=directories.Open(fd);return result.Succeeded?(DIR*)result.Value:DirectoryError((int)result.Error);}
        catch(Exception error){IoException(error);return null;}
    }
    public static unsafe DIR* blink_host_opendir(byte* path)
    {
        try
        {
            if(directories==null)return DirectoryError(19);
            if(path==null)return DirectoryError(14);
            int length=0;while(length<PathLimit && path[length]!=0)++length;
            if(length==PathLimit)return DirectoryError(36);
            string name;try{name=PathEncoding.GetString(new ReadOnlySpan<byte>(path,length));}catch(DecoderFallbackException){return DirectoryError(22);}
            var result=directories.Open(name);return result.Succeeded?(DIR*)result.Value:DirectoryError((int)result.Error);
        }
        catch(Exception error){IoException(error);return null;}
    }
    public static unsafe dirent* blink_host_readdir(DIR* token)
    {
        try{if(directories==null){DirectoryError(19);return null;}var result=directories.Read((nuint)token);if(!result.Succeeded){DirectoryError((int)result.Error);return null;}return(dirent*)result.Value;}
        catch(Exception error){IoException(error);return null;}
    }
    public static unsafe int blink_host_closedir(DIR* token)
    {try{return directories==null?IoError(19):(int)IoResult(directories.Close((nuint)token));}catch(Exception error){return IoException(error);}}
    public static unsafe int blink_host_dirfd(DIR* token)
    {try{return directories==null?IoError(19):(int)IoResult(directories.Descriptor((nuint)token));}catch(Exception error){return IoException(error);}}
    public static unsafe long blink_host_telldir(DIR* token)
    {try{if(directories==null)return IoError(19);var result=directories.Tell((nuint)token);return result.Succeeded?result.Value:IoError((int)result.Error);}catch(Exception error){return IoException(error);}}
    public static unsafe void blink_host_seekdir(DIR* token,long position)
    {try{if(directories==null){IoError(19);return;}IoResult(directories.Seek((nuint)token,position));}catch(Exception error){IoException(error);}}
    public static unsafe void blink_host_rewinddir(DIR* token)=>blink_host_seekdir(token,0);
}
