namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineCanonicalX87Invalid(JitInstruction i) =>
        i.Operation == JitOperation.X87 && i.Options is
            FullCanonicalX87InvalidLowering.ReservedUd or FullCanonicalX87InvalidLowering.LockUd;

    internal static bool ValidateCanonicalX87Invalid(JitInstruction i)
    {
        if (!CanInlineCanonicalX87Invalid(i)) return false;
        ulong fop = i.Extra.Immediate;
        bool selector = fop <= 0x7ff && (i.Options == FullCanonicalX87InvalidLowering.LockUd ||
            FullCanonicalX87InvalidLowering.IsReservedSelector((byte)(0xd8 | (fop >> 8)), (byte)fop));
        if (!selector || i.Length is < 2 or > 15 || i.Width != 80 || i.Destination != default ||
            i.Source != default || i.SourceWidth != 0 || i.Address != default || i.Target != 0 ||
            i.Condition != 0 || i.Extra != new JitOperand(0, fop, IsImmediate: true))
            throw new ArgumentException("Invalid canonical x87 UD IR");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryCanonicalX87InvalidOperation(in JitInstruction i)
        {
            if (!ValidateCanonicalX87Invalid(i)) return false;
            // Ordinary Guard/Poll/Issue already ran. Architectural delivery is
            // after RET, native lease/enrollment release, and work validation.
            // No host UD2, managed OpFpu, metadata update, or numerical kernel.
            Bytes(0x41, 0xc7, 0x82); U32(168); U32(6);
            Bytes(0x41, 0xc7, 0x82); U32(172); U32(1);
            // MOV r/m64,imm32 would sign-extend; use the existing full64 store
            // sequence for arbitrary high guest instruction addresses instead.
            Bytes(0x48, 0xb8); U64(i.Ip);
            Bytes(0x49, 0x89, 0x82); U32(176);
            JumpExit(FullBlockExit.GuestFault);
            return true;
        }
    }
}
