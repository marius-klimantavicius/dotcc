namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    // Core must call these before its historical Nop/general scalar paths.
    // Recognition is by operation, so malformed hint IR cannot fall through
    // to the old unchecked Nop return. Validation remains translation-time.
    internal static bool CanInlineHint(JitInstruction i) =>
        i.Operation is JitOperation.Nop or JitOperation.Pause;

    internal static bool ValidateHint(JitInstruction i)
    {
        if (!CanInlineHint(i)) return false;
        if (i.Length is < 1 or > 15 || i.Width != 64 ||
            i.Operation == JitOperation.Pause && i.Length < 2 ||
            i.Destination != default || i.Source != default || i.Extra != default ||
            i.Address != default || i.Target != 0 || i.Condition != 0 ||
            i.SourceWidth != 0 || i.Options != 0)
            throw new ArgumentException("Invalid canonical native hint IR");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryHintOperation(in JitInstruction i)
        {
            if (!CanInlineHint(i)) return false;
            // One literal baseline host PAUSE at this instruction site. It
            // changes no guest flags/registers and performs no guest access.
            // Poll, lock-xadd issue, architectural commit and completion are
            // supplied ONCE by the existing common instruction scaffold.
            if (i.Operation == JitOperation.Pause) Bytes(0xf3, 0x90);
            return true;
        }
    }
}
