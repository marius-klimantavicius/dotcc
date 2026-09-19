using global::System;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static int blink_io_terminal(int descriptor)
    {
        try { return io == null ? IoError(19) : (int)IoResult(io.TerminalOperation(descriptor)); }
        catch (Exception error) { return IoException(error); }
    }
}
