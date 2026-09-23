using global::System;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static int blink_host_dup2(int source,int target)
    {
        try{return io==null?IoError(19):(int)IoResult(io.DuplicateTo(source,target));}
        catch(Exception e){return IoException(e);}
    }
    public static int blink_host_dup3(int source,int target,int flags)
    {
        try{
            if(io==null)return IoError(19);
            if((flags&~524288)!=0)return IoError(22);
            return (int)IoResult(io.DuplicateTo(source,target,(flags&524288)!=0,true));
        }catch(Exception e){return IoException(e);}
    }
}
