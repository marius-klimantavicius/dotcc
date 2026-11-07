namespace Managed.Emulation.Execution;

// Independently authored baseline x64 operations. All semantic selection is at
// translation/emission time; no CALL, stack use, host LAHF/SAHF or optional ISA.
internal static partial class FullBlockEmitter
{
    internal static bool IsNativeCounterBranch(JitInstruction i) => i.Operation is
        JitOperation.Loop or JitOperation.LoopEqual or JitOperation.LoopNotEqual or JitOperation.CounterZeroJump;

    private static bool IsControlFlag(JitInstruction i) => IsNativeCounterBranch(i) ||
        i.Operation is JitOperation.LoadFlagsAh or JitOperation.StoreFlagsAh;

    internal static bool CanInlineControlFlag(JitInstruction i)
    {
        if (!IsControlFlag(i) || i.Length is < 1 or > 15 || i.Destination != default ||
            i.Source != default || i.Extra != default || i.Address != default || i.Condition != 0 ||
            i.SourceWidth != 0 || i.Options != 0) return false;
        if (!IsNativeCounterBranch(i)) return i.Width == 8 && i.Target == 0;
        ulong displacement = unchecked(i.Target - (i.Ip + (uint)i.Length));
        return i.Width is 32 or 64 && i.Length >= (i.Width == 32 ? 3 : 2) &&
            (displacement <= 127 || displacement >= unchecked(0UL - 128));
    }

    // True consumes a matching-family IR. Malformed family IR must not slip
    // through the older generic scalar validators. Root enforces final-site
    // placement for IsNativeCounterBranch, just as for other control transfers.
    internal static bool ValidateControlFlag(JitInstruction i)
    {
        if (!IsControlFlag(i)) return false;
        if (!CanInlineControlFlag(i)) throw new ArgumentException("Invalid canonical native control/flags IR");
        return true;
    }

    private ref partial struct Writer
    {
        // Guard/Poll/Issue have run ONCE before this operation. This method
        // owns the chosen architectural Commit and static Dispatch. The caller
        // must not append ordinary Operation/Commit/Dispatch afterward.
        private void NativeCounterBranch(JitInstruction i) => NativeNearCounter(i);

        private bool TryControlFlagOperation(in JitInstruction i)
        {
            if (i.Operation is not (JitOperation.LoadFlagsAh or JitOperation.StoreFlagsAh)) return false;
            if (i.Operation == JitOperation.LoadFlagsAh)
            {
                Bytes(0x41, 0x8b, 0x43, 12); // eax = immutable guest software flags
                IntegerMov(1, 0, 32);
                IntegerShift(5, 1, 32, 24); // ecx = lazy parity input byte
                for (int shift = 4; shift != 0; shift >>= 1)
                {
                    IntegerMov(2, 1, 32);
                    IntegerShift(5, 2, 32, shift);
                    IntegerRR(0x31, 1, 2, 32);
                }
                IntegerConstant(6, 1, 1); // invert odd parity
                IntegerConstant(4, 1, 1);
                IntegerShift(4, 1, 32, 2); // software PF -> AH bit2
                // Parent-approved architecture: selected ExportFlags forces bit3=1
                // in error. Preserve that discrepancy explicitly in test/report.
                IntegerConstant(4, 0, 0xd1); // SF,ZF,AF,CF; exclude stale raw PF and reserved bits
                IntegerRR(0x09, 0, 1, 32);
                IntegerConstant(1, 0, 2); // architectural fixed AH[5:3:1] = 0:0:1
                IntegerMov(8, 0, 32);
                Store(new(0, 0, ByteOffset: 1), 8); // only guest AH byte; all other RAX bits preserved
            }
            else
            {
                IntegerLoad(new(0, 0, ByteOffset: 1), 8, 0); // eax = guest AH, not host AH
                IntegerMov(1, 0, 32);
                IntegerShift(5, 1, 32, 2);
                IntegerConstant(4, 1, 1);
                IntegerConstant(6, 1, 1);
                IntegerShift(4, 1, 32, 24); // canonical lazy input 0 for even,1 for odd
                IntegerConstant(4, 0, 0xd5); // selected representation also updates raw PF bit2
                IntegerRR(0x09, 0, 1, 32);
                IntegerMergeFlags(0xff0000d5u); // preserve OF,control,reserved bits; RF already excluded by guard
            }
            return true;
        }
    }
}
