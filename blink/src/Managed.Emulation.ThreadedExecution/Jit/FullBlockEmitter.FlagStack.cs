namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineFlagStack(JitInstruction i) =>
        i.Operation is JitOperation.PushFlags or JitOperation.PopFlags;

    internal static bool ValidateFlagStack(JitInstruction i)
    {
        if (!CanInlineFlagStack(i)) return false;
        if (i.Length is < 1 or > 15 || i.Width is not (16 or 64) ||
            i.Destination != default || i.Source != default || i.Extra != default ||
            i.Address != default || i.Target != 0 || i.Condition != 0 || i.SourceWidth != 0 || i.Options != 0)
            throw new ArgumentException("Native flag stack metadata");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryFlagStackOperation(in JitInstruction i)
        {
            if (!CanInlineFlagStack(i)) return false;
            int width = i.Width;
            bool push = i.Operation == JitOperation.PushFlags;
            var memory = i with { Address = new(4, -1, 0, push ? -width / 8 : 0, Segment: 2) };
            // No RSP, flags, parity, or guest byte effect before BOTH endpoint
            // and mapping/permission checks. Implicit stack faults use #SS.
            ScalarMiscCheckCanonical(memory, width);
            NativeMemoryPrepare(memory, push ? FullMemoryAccess.Write : FullMemoryAccess.Read, width);
            if (push)
            {
                Bytes(0x41, 0x8b, 0x43, 12); // guest software flags, never host PUSHF
                IntegerMov(1, 0, 32); IntegerShift(5, 1, 32, 24);
                for (int shift = 4; shift != 0; shift >>= 1)
                {
                    IntegerMov(2, 1, 32); IntegerShift(5, 2, 32, shift);
                    IntegerRR(0x31, 1, 2, 32);
                }
                IntegerConstant(6, 1, 1); IntegerConstant(4, 1, 1);
                IntegerShift(4, 1, 32, 2);
                // Only architectural defined flag bits, RF/VM cleared, bit1=1.
                // Unlike selected ExportFlags, do not invent IOPL=3 or bit3=1.
                IntegerConstant(4, 0, 0x003c7fd1);
                IntegerRR(0x09, 0, 1, 32); IntegerConstant(1, 0, 2);
                IntegerMov(8, 0, 32); NativeMemoryStore(width);
                Bytes(0x49, 0x83, 0x6b, 56, (byte)(width / 8));
            }
            else
            {
                NativeMemoryLoad(width); IntegerMov(8, 0, 32);
                // Linux user execution is CPL3. IOPL cannot be changed by
                // POPF; IF is writable only for an existing IOPL of 3.
                uint writable = width == 16 ? 0x00004dd5u : 0x00244dd5u;
                IntegerConstant(4, 0, writable);
                Bytes(0x41, 0x8b, 0x53, 12);
                IntegerMov(1, 2, 32); IntegerConstant(4, 1, 0x3000);
                IntegerConstant(7, 1, 0x3000); int keepIf = NearCondition(0x85);
                IntegerMov(1, 8, 32); IntegerConstant(4, 1, 0x200);
                IntegerRR(0x09, 0, 1, 32);
                IntegerConstant(4, 2, ~(writable | 0xff010200u));
                int merge = NativeNearJump();
                Patch(keepIf, position);
                IntegerConstant(4, 2, ~(writable | 0xff010000u));
                Patch(merge, position); IntegerRR(0x09, 0, 2, 32);
                // Canonical Blink parity input is !PF; raw PF is also retained
                // for literal flag import, matching the existing representation.
                IntegerMov(1, 8, 32); IntegerShift(5, 1, 32, 2);
                IntegerConstant(4, 1, 1); IntegerConstant(6, 1, 1);
                IntegerShift(4, 1, 32, 24); IntegerRR(0x09, 0, 1, 32);
                Bytes(0x41, 0x89, 0x43, 12);
                Bytes(0x49, 0x83, 0x43, 56, (byte)(width / 8));
            }
            NativeMemoryCount(); return true;
        }
    }
}
