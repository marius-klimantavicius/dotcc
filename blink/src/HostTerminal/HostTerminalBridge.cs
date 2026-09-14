using global::System;

namespace Managed.Emulation;

public static partial class Blink
{
    public static int blink_io_terminal(int descriptor)
    {
        try { return io == null ? IoError(19) : (int)IoResult(io.TerminalOperation(descriptor)); }
        catch (Exception error) { return IoException(error); }
    }
}
