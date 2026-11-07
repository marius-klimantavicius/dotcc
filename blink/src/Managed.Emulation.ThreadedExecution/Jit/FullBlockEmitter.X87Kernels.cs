namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    private ref partial struct Writer
    {
        // Common Poll/Header/PC/cap/context and file authentication precede
        // this guard. All denials precede Issue and never touch Work contents.
        private void RequireCanonicalX87Kernel(in FullX87KernelCall call, bool arithmetic)
        {
            RequireCanonicalX87();
            IntegerFrameLoad(96, 0); IntegerRR(0x85, 0, 0);
            ExitCondition(0x84, FullBlockExit.Invalidated);
            IntegerFrameLoad(104, 0);
            Bytes(0x48, 0xb9); U64(call.OwnerCookie);
            IntegerRR(0x39, 0, 1);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            // Every raw CW encoding is admitted. Arithmetic/sqrt kernels decode
            // PC01 as PC64 at precision use; Work.Control remains raw. The
            // arithmetic argument retains the call-site contract without any
            // guest-state rewrite or Admission3 escape.
        }
    }
}
