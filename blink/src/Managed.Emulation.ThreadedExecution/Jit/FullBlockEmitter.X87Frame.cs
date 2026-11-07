namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    private ref partial struct Writer
    {
        // Version/Size are already authenticated. RAX/RCX only; preserve shared
        // entry R9 continuation and the frame/Machine registers R10/R11.
        // Generation/token are checked against the exact binding by the parent
        // before publication. The native leaf additionally rejects zero identity,
        // wrong owning run, inactive/malformed headers and reserved fields.
        private void GuardX87Frame(bool independentRep)
        {
            Bytes(0x49, 0x83, 0xba); U32(408); Bytes(0);
            X87FrameReject(0x85, independentRep);
            IntegerFrameLoad(400, 0); IntegerRR(0x85, 0, 0);
            int inactive = NearCondition(0x84);
            Bytes(0x48, 0xf7, 0xc0); U32(15); X87FrameReject(0x85, independentRep);
            Bytes(0x83, 0x38, 1); X87FrameReject(0x85, independentRep);
            Bytes(0x81, 0x78, 4); U32(FullX87RegisterFile.ByteSize); X87FrameReject(0x85, independentRep);
            Bytes(0x83, 0x78, 8, 1); X87FrameReject(0x85, independentRep);
            Bytes(0x83, 0x78, 12, 1); X87FrameReject(0x85, independentRep);
            Bytes(0x4c, 0x39, 0x58, 16); X87FrameReject(0x85, independentRep);
            Bytes(0x48, 0x83, 0x78, 24, 0); X87FrameReject(0x84, independentRep);
            Bytes(0x48, 0x83, 0x78, 32, 0); X87FrameReject(0x84, independentRep);
            IntegerFrameLoad(104, 1); Bytes(0x48, 0x39, 0x48, 32); X87FrameReject(0x85, independentRep);
            Bytes(0x83, 0x78, 44, 0); X87FrameReject(0x85, independentRep);
            Bytes(0x48, 0x83, 0x78, 48, 0); X87FrameReject(0x85, independentRep);
            Bytes(0x48, 0x83, 0x78, 56, 0); X87FrameReject(0x85, independentRep);
            Patch(inactive, position);
        }
        // Called after Poll/Header/PC/cap/context but before this site's Issue.
        // All nonnull file metadata was already authenticated by Prologue.
        private void RequireCanonicalX87()
        {
            IntegerFrameLoad(400, 0); IntegerRR(0x85, 0, 0);
            ExitCondition(0x84, FullBlockExit.Invalidated);
        }
        private void X87FrameReject(byte condition, bool independentRep)
        {
            if (independentRep) RepAbiReject(condition);
            else ExitCondition(condition, FullBlockExit.Invalidated);
        }
    }
}
