using global::System;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe int blink_host_pipe(int* descriptors) => blink_host_pipe2(descriptors,0);
    public static unsafe int blink_host_pipe2(int* descriptors,int flags)
    {
        try {
            if(io==null) return IoError(19);
            if(descriptors==null) return IoError(14);
            var result=io.Pipe(flags);
            if(!result.Succeeded) return IoError((int)result.Error);
            // Neither output slot is touched until both ends are published.
            descriptors[0]=result.Value.Read;descriptors[1]=result.Value.Write;
            return 0;
        } catch(Exception error) {return IoException(error);}
    }
}
