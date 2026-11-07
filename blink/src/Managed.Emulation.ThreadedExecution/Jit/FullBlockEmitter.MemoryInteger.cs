namespace Managed.Emulation.Execution;

// Ordinary memory integer computation. The existing checked TLB and mapping
// ticket own every pointer lifetime. All semantic work is emitted native code.
internal static partial class FullBlockEmitter
{
    internal static bool CanInlineMemoryInteger(JitInstruction i) =>
        (i.Destination.IsMemory || i.Source.IsMemory) && i.Operation is (
        JitOperation.ShiftLeft or JitOperation.ShiftRight or JitOperation.ShiftArithmeticRight or
        JitOperation.RotateLeft or JitOperation.RotateRight or JitOperation.RotateCarryLeft or JitOperation.RotateCarryRight or
        JitOperation.ShiftDoubleLeft or JitOperation.ShiftDoubleRight or
        JitOperation.MultiplyUnsigned or JitOperation.MultiplySigned or JitOperation.MultiplySignedLow or
        JitOperation.DivideUnsigned or JitOperation.DivideSigned or
        JitOperation.BitScanForward or JitOperation.BitScanReverse or JitOperation.BitTest or
        JitOperation.BitSet or JitOperation.BitReset or JitOperation.BitComplement);

    private static bool MemoryIntegerRegister(JitOperand p, int width) =>
        IntegerRegister(p, width) && p.Immediate == 0;
    private static bool MemoryIntegerImmediate(JitOperand p) =>
        p.IsImmediate && !p.IsMemory && p.Register == 0 && p.ByteOffset == 0;
    private static bool MemoryIntegerCount(JitOperand p) =>
        p == new JitOperand(1, 0) || MemoryIntegerImmediate(p) && p.Immediate <= 255;

    internal static bool ValidateMemoryInteger(JitInstruction i)
    {
        if (!CanInlineMemoryInteger(i)) return false;
        var memory = new JitOperand(0, 0, IsMemory: true);
        int w = i.Width; var a = i.Address;
        if (w is not (8 or 16 or 32 or 64) || i.Length is < 1 or > 15 ||
            i.Target != 0 || i.Condition != 0 || i.SourceWidth != w || i.Extra.IsMemory ||
            i.Destination.IsMemory && i.Source.IsMemory ||
            i.Destination.IsMemory && i.Destination != memory || i.Source.IsMemory && i.Source != memory)
            throw new ArgumentException("Native memory integer metadata");
        if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
            a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
            a.RipRelative && (a.Base != -1 || a.Index != -1))
            throw new ArgumentException("Native memory integer address");
        bool valid = i.Operation switch
        {
            JitOperation.ShiftLeft or JitOperation.ShiftRight or JitOperation.ShiftArithmeticRight or
            JitOperation.RotateLeft or JitOperation.RotateRight or JitOperation.RotateCarryLeft or JitOperation.RotateCarryRight =>
                i.Destination.IsMemory && MemoryIntegerCount(i.Source) && i.Extra == default && i.Options == 0,
            JitOperation.ShiftDoubleLeft or JitOperation.ShiftDoubleRight =>
                w != 8 && i.Destination.IsMemory && MemoryIntegerRegister(i.Source, w) && MemoryIntegerCount(i.Extra) && i.Options == 0,
            JitOperation.MultiplyUnsigned or JitOperation.MultiplySigned or JitOperation.DivideUnsigned or JitOperation.DivideSigned =>
                i.Source.IsMemory && i.Destination == default && i.Extra == default && i.Options == 0,
            JitOperation.MultiplySignedLow => w != 8 && i.Source.IsMemory && MemoryIntegerRegister(i.Destination, w) &&
                (i.Options == 0 && i.Extra == default || i.Options == FullIntegerLowering.ThreeOperandMultiply && MemoryIntegerImmediate(i.Extra)),
            JitOperation.BitScanForward or JitOperation.BitScanReverse => w != 8 && i.Source.IsMemory &&
                MemoryIntegerRegister(i.Destination, w) && i.Extra == default && i.Options == 0,
            JitOperation.BitTest or JitOperation.BitSet or JitOperation.BitReset or JitOperation.BitComplement =>
                w != 8 && i.Destination.IsMemory && i.Extra == default && i.Options == 0 &&
                (MemoryIntegerRegister(i.Source, w) || MemoryIntegerImmediate(i.Source) && i.Source.Immediate <= 255),
            _ => false
        };
        if (!valid) throw new ArgumentException("Native memory integer operand shape");
        return true;
    }

    private ref partial struct Writer
    {
        // Emitter-local state, never a runtime operation selector or fake GPR.
        // Reset even if emission throws; no state may leak to the next site.
        private bool memoryIntegerOperandStaged;

        private bool TryStoreMemoryInteger(JitOperand operand, int width)
        {
            if (!operand.IsMemory) return false;
            if (!memoryIntegerOperandStaged) throw new ArgumentException("Unprepared native integer memory store");
            NativeMemoryStore(width); // consumes R8; guest flags already stored
            return true;
        }

        // Called after Address(i,false), before the ordinary address suffix.
        // Add the signed word displacement to the effective OFFSET, truncate
        // to the address size, then add FS/GS. The selected C masks after the
        // segment addition in error; that must not discard a high FS/GS base.
        // An immediate index has displacement0, not imm/8.
        private bool MemoryIntegerBitAddress(JitInstruction i)
        {
            if (i.Operation is not (JitOperation.BitTest or JitOperation.BitSet or JitOperation.BitReset or JitOperation.BitComplement) ||
                !i.Destination.IsMemory) return false;
            if (!i.Source.IsImmediate)
            {
                IntegerLoad(i.Source, i.Width, 1); IntegerSignExtend(1, i.Width);
                IntegerConstant(4, 1, unchecked((uint)-i.Width), 64);
                IntegerShift(7, 1, 64, 3); IntegerRR(0x01, 0, 1);
            }
            if (i.Address.AddressWidth == 32) IntegerMov(0, 0, 32);
            if (i.Address.Segment is 4 or 5)
            { Bytes(0x49, 0x03, 0x82); U32(i.Address.Segment == 4 ? 328u : 336u); }
            return true;
        }

        private bool TryMemoryIntegerOperation(in JitInstruction i)
        {
            if (!CanInlineMemoryInteger(i)) return false;
            ValidateMemoryInteger(i);
            ScalarMiscCheckCanonical(i,i.Width);
            // Count0 is still a selected read/write memory access. Prepare
            // validates BOTH pages/permissions before flags, GPRs or data.
            NativeMemoryPrepare(i, i.Destination.IsMemory && i.Operation != JitOperation.BitTest ?
                FullMemoryAccess.Read | FullMemoryAccess.Write : FullMemoryAccess.Read, i.Width);
            NativeMemoryLoad(i.Width); IntegerFrameStore(208, 0);
            memoryIntegerOperandStaged = true;
            try
            {
                // Call the actual computation routines, never IntegerOperation
                // with invented guest registers or a managed semantic Op helper.
                switch (i.Operation)
                {
                    case JitOperation.ShiftLeft: case JitOperation.ShiftRight: case JitOperation.ShiftArithmeticRight:
                        IntegerSingleShift(in i); break;
                    case JitOperation.RotateLeft: case JitOperation.RotateRight:
                    case JitOperation.RotateCarryLeft: case JitOperation.RotateCarryRight:
                        IntegerRotate(in i); break;
                    case JitOperation.ShiftDoubleLeft: case JitOperation.ShiftDoubleRight:
                        IntegerDoubleShift(in i); break;
                    case JitOperation.MultiplyUnsigned: case JitOperation.MultiplySigned: case JitOperation.MultiplySignedLow:
                        IntegerMultiply(in i); break;
                    case JitOperation.DivideUnsigned: case JitOperation.DivideSigned:
                        IntegerDivide(in i); break;
                    case JitOperation.BitScanForward: case JitOperation.BitScanReverse:
                        IntegerBitscan(in i); break;
                    case JitOperation.BitTest: case JitOperation.BitSet: case JitOperation.BitReset:
                    case JitOperation.BitComplement: IntegerBit(in i); break;
                    default: throw new ArgumentException("Native memory integer operation");
                }
            }
            finally { memoryIntegerOperandStaged = false; }
            // Divide fault exits before this counter and the core's Commit.
            NativeMemoryCount(); return true;
        }
    }
}
