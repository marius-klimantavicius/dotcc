namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    // Distinct low16 namespace under VectorInteger. Core must dispatch this
    // validator/writer BEFORE R66. bit16 retains VectorAlignedMemory's meaning.
    internal enum FullVectorMoveOperation : ushort
    {
        Move128 = 0x1001, MoveScalar, LoadLow64, LoadHigh64, StoreLow64, StoreHigh64,
        MoveHighToLow, MoveLowToHigh, UnpackLowSingle, UnpackHighSingle,
        UnpackLowDouble, UnpackHighDouble, DuplicateLowSingle, DuplicateHighSingle,
        DuplicateDouble, ScalarToVector, VectorToScalar, MoveLow64Zero, NonTemporalStore128
    }

    internal static bool CanInlineVectorMove(JitInstruction i) =>
        i.Operation == JitOperation.VectorInteger && (i.Options & 0xf000) == 0x1000;
    internal static FullVectorMoveOperation VectorMoveOperation(JitInstruction i) =>
        (FullVectorMoveOperation)(i.Options & 0xffff);
    private static bool VectorMoveUnpack(FullVectorMoveOperation op) =>
        (uint)op >= (uint)FullVectorMoveOperation.UnpackLowSingle &&
        (uint)op <= (uint)FullVectorMoveOperation.UnpackHighDouble;

    internal static bool ValidateVectorMove(JitInstruction i)
    {
        if (!CanInlineVectorMove(i)) return false;
        var op = VectorMoveOperation(i);
        if (i.Width != 128 || i.Length is < 1 or > 15 || i.Target != 0 || i.Condition != 0 ||
            i.Extra != default || (i.Options & ~(0xffffu | VectorAlignedMemory)) != 0 ||
            (uint)op < (uint)FullVectorMoveOperation.Move128 ||
            (uint)op > (uint)FullVectorMoveOperation.NonTemporalStore128)
            throw new ArgumentException("Native vector move operation/metadata");
        bool memory = i.Destination.IsMemory || i.Source.IsMemory;
        if (i.Destination.IsMemory && i.Source.IsMemory ||
            !(VectorRegister(i.Destination) || VectorMemory(i.Destination)) ||
            !(VectorRegister(i.Source) || VectorMemory(i.Source)))
            throw new ArgumentException("Native vector move operand");
        bool dynamicWidth = op is FullVectorMoveOperation.MoveScalar or
            FullVectorMoveOperation.ScalarToVector or FullVectorMoveOperation.VectorToScalar;
        int width = dynamicWidth ? i.SourceWidth : op is FullVectorMoveOperation.Move128 or
            FullVectorMoveOperation.NonTemporalStore128 or FullVectorMoveOperation.UnpackHighSingle or
            FullVectorMoveOperation.UnpackHighDouble or FullVectorMoveOperation.DuplicateLowSingle or
            FullVectorMoveOperation.DuplicateHighSingle ? 128 : 64;
        if (i.SourceWidth != width || (dynamicWidth && width is not (32 or 64)))
            throw new ArgumentException("Native vector move width");
        if (i.Destination.IsMemory && op is not (FullVectorMoveOperation.Move128 or
                FullVectorMoveOperation.MoveScalar or FullVectorMoveOperation.StoreLow64 or
                FullVectorMoveOperation.StoreHigh64 or FullVectorMoveOperation.VectorToScalar or
                FullVectorMoveOperation.MoveLow64Zero or FullVectorMoveOperation.NonTemporalStore128) ||
            op is FullVectorMoveOperation.LoadLow64 or FullVectorMoveOperation.LoadHigh64 && !i.Source.IsMemory ||
            op is FullVectorMoveOperation.StoreLow64 or FullVectorMoveOperation.StoreHigh64 or
                FullVectorMoveOperation.NonTemporalStore128 && !i.Destination.IsMemory ||
            op is FullVectorMoveOperation.MoveHighToLow or FullVectorMoveOperation.MoveLowToHigh && memory ||
            op == FullVectorMoveOperation.VectorToScalar && i.Source.IsMemory)
            throw new ArgumentException("Native vector move direction");
        bool aligned = (i.Options & VectorAlignedMemory) != 0;
        bool duplicateSingle = op is FullVectorMoveOperation.DuplicateLowSingle or FullVectorMoveOperation.DuplicateHighSingle;
        if (aligned && (!memory || !(op is FullVectorMoveOperation.Move128 or FullVectorMoveOperation.NonTemporalStore128 || VectorMoveUnpack(op) || duplicateSingle)) ||
            memory && (op == FullVectorMoveOperation.NonTemporalStore128 || VectorMoveUnpack(op) || duplicateSingle) && !aligned)
            throw new ArgumentException("Native vector move alignment metadata");
        if (memory)
        {
            var a = i.Address;
            if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
                a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
                a.RipRelative && (a.Base != -1 || a.Index != -1))
                throw new ArgumentException("Native vector move address");
        }
        else if (i.Address != default) throw new ArgumentException("Native vector move unused address");
        if (!VectorLayout.Supported) throw new PlatformNotSupportedException("Translated vector layout differs");
        return true;
    }

    private ref partial struct Writer
    {
        // Legacy SSE alignment is #GP(0), never #PF at the operand address.
        // The controller maps trap13 to SIGSEGV/SI_KERNEL128/faultaddr0. R66's
        // old trap14 alignment convention is intentionally not reused here.
        private void VectorMoveCheckAlignment(JitInstruction i)
        {
            NativeMemoryAddress(i);
            Bytes(0xa8, 15); int aligned = NearCondition(0x84);
            Bytes(0x41, 0xc7, 0x82); U32(168); U32(13);
            Bytes(0x41, 0xc7, 0x82); U32(172); U32(128);
            Bytes(0x49, 0xc7, 0x82); U32(176); U32(0);
            JumpExit(FullBlockExit.GuestFault);
            Patch(aligned, position);
        }

        private bool TryVectorMoveOperation(in JitInstruction i)
        {
            if (!CanInlineVectorMove(i)) return false;
            var op = VectorMoveOperation(i);
            bool memory = i.Destination.IsMemory || i.Source.IsMemory;
            if (memory)
            {
                if ((i.Options & VectorAlignedMemory) != 0) VectorMoveCheckAlignment(i);
                NativeMemoryPrepare(i, i.Destination.IsMemory ? FullMemoryAccess.Write : FullMemoryAccess.Read, i.SourceWidth);
            }
            // Prepare authenticates both mappings BEFORE source data reads and
            // every guest output. Memory helpers use only the actual width.
            if (op == FullVectorMoveOperation.ScalarToVector)
            {
                if (i.Source.IsMemory)
                { VectorMemoryLoad(i.SourceWidth); Bytes(0x66, 0x0f, 0x6f, 0xc1); }
                else
                {
                    IntegerLoad(i.Source, i.SourceWidth, 0);
                    Bytes(0x66); if (i.SourceWidth == 64) Bytes(0x48);
                    Bytes(0x0f, 0x6e, 0xc0); // movd/q xmm0,eax/rax; zero upper
                }
            }
            else if (op == FullVectorMoveOperation.VectorToScalar)
            {
                VectorGuestLoad(0, i.Source.Register);
                if (i.Destination.IsMemory) VectorMemoryStore(i.SourceWidth);
                else
                {
                    Bytes(0x66, i.SourceWidth == 64 ? (byte)0x49 : (byte)0x41, 0x0f, 0x7e, 0xc0);
                    Store(i.Destination, i.SourceWidth); // r8d zero-extends guest GPR
                }
                if (memory) NativeMemoryCount();
                return true;
            }
            else if (i.Destination.IsMemory)
            {
                VectorGuestLoad(0, i.Source.Register);
                if (op == FullVectorMoveOperation.StoreHigh64) Bytes(0x66, 0x0f, 0x73, 0xd8, 8);
                // NT hints are optional. An ordinary checked write supplies
                // stronger ordering without issuing host streaming stores to
                // arbitrary backing/cache types; no managed semantic helper.
                VectorMemoryStore(i.SourceWidth); NativeMemoryCount(); return true;
            }
            else
            {
                // Host registers are snapshots. Even src==dst observes all
                // input lanes before the final full guest-XMM write.
                VectorGuestLoad(0, i.Destination.Register);
                if (i.Source.IsMemory) VectorMemoryLoad(i.SourceWidth);
                else VectorGuestLoad(1, i.Source.Register);
                switch (op)
                {
                    case FullVectorMoveOperation.Move128:
                        Bytes(0x66, 0x0f, 0x6f, 0xc1); break;
                    case FullVectorMoveOperation.MoveScalar:
                        if (i.Source.IsMemory) Bytes(0x66, 0x0f, 0x6f, 0xc1); // memory zeros upper96/64
                        else Bytes(i.SourceWidth == 32 ? (byte)0xf3 : (byte)0xf2, 0x0f, 0x10, 0xc1);
                        break;
                    case FullVectorMoveOperation.LoadLow64:
                        Bytes(0xf2, 0x0f, 0x10, 0xc1); break; // keep destination high64
                    case FullVectorMoveOperation.LoadHigh64:
                    case FullVectorMoveOperation.MoveLowToHigh:
                        Bytes(0x0f, 0x16, 0xc1); break;
                    case FullVectorMoveOperation.MoveHighToLow:
                        Bytes(0x0f, 0x12, 0xc1); break;
                    case FullVectorMoveOperation.UnpackLowSingle:
                        Bytes(0x0f, 0x14, 0xc1); break;
                    case FullVectorMoveOperation.UnpackHighSingle:
                        Bytes(0x0f, 0x15, 0xc1); break;
                    case FullVectorMoveOperation.UnpackLowDouble:
                        Bytes(0x66, 0x0f, 0x14, 0xc1); break;
                    case FullVectorMoveOperation.UnpackHighDouble:
                        // Architectural {old-dst.high,src.high}. Selected C's
                        // distinct-register {src.high,src.high} defect is an
                        // explicit expected-mismatch oracle test, not copied.
                        Bytes(0x66, 0x0f, 0x15, 0xc1); break;
                    case FullVectorMoveOperation.DuplicateLowSingle:
                        Bytes(0x66, 0x0f, 0x70, 0xc1, 0xa0); break;
                    case FullVectorMoveOperation.DuplicateHighSingle:
                        Bytes(0x66, 0x0f, 0x70, 0xc1, 0xf5); break;
                    case FullVectorMoveOperation.DuplicateDouble:
                        Bytes(0x66, 0x0f, 0x70, 0xc1, 0x44); break;
                    case FullVectorMoveOperation.MoveLow64Zero:
                        Bytes(0xf3, 0x0f, 0x7e, 0xc1); break;
                    default: throw new ArgumentException("Native vector move emission operation");
                }
            }
            VectorGuestStore(0, i.Destination.Register);
            if (memory) NativeMemoryCount();
            return true;
        }
    }
}
