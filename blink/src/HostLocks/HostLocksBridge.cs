using global::System;
namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static int blink_io_flock(int descriptor, int operation)
    {
        try { return io == null ? IoError(19) : (int)IoResult(io.AdvisoryLock(descriptor, operation)); }
        catch (Exception error) { return IoException(error); }
    }
}
