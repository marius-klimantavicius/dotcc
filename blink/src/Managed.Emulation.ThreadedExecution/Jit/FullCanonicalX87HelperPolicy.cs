namespace Managed.Emulation.Execution;

internal enum FullCanonicalX87HelperKind { NonState, SyscallBoundary, NativeOnlyState, Unknown }

// Closed native12 selector audit: 537 explicit opcode arms plus default OpUd.
// Conservative for unconditional producer guards shared by MMX and SSE bodies:
// even their SSE variants cannot use those helpers with active canonical state.
// This is a safety classification, not native coverage credit for missing forms.
internal static class FullCanonicalX87HelperPolicy
{
    internal static FullCanonicalX87HelperKind Classify(ulong rde)
    {
        int op = (int)(rde >> 40 & 2047), word = (int)(rde >> 5 & 1), rep = (int)(rde >> 51 & 3);
        if (op == 0x105) return FullCanonicalX87HelperKind.SyscallBoundary;
        if (FullCanonicalX87InvalidLowering.IsStateOperation(rde)) return FullCanonicalX87HelperKind.NativeOnlyState;
        bool unsafeState = op switch
        {
            0x12a or 0x12c or 0x12d => rep < 2,
            0x16e or 0x1e7 => word == 0,
            0x16f or 0x17e or 0x17f => word == 0 && rep != 3,
            0x1d6 => rep >= 2,
            >= 0xd8 and <= 0xdf or
            >= 0x160 and <= 0x16d or
            >= 0x171 and <= 0x177 or
            >= 0x1d1 and <= 0x1d5 or
            >= 0x1d8 and <= 0x1e5 or
            >= 0x1e8 and <= 0x1ef or
            >= 0x1f1 and <= 0x1f6 or
            >= 0x1f8 and <= 0x1fe or
            >= 0x200 and <= 0x20b or
            >= 0x21c and <= 0x21e or
            0x240 or
            0x30f => true,
            _ => false
        };
        if (unsafeState) return FullCanonicalX87HelperKind.NativeOnlyState;
        return op is >= 0x0 and <= 0x20b or
            >= 0x21c and <= 0x21e or
            0x22a or
            0x240 or
            >= 0x2f0 and <= 0x2f1 or
            >= 0x2f5 and <= 0x2f7 or
            0x30f or
            0x344 or
            0x3f0 ? FullCanonicalX87HelperKind.NonState : FullCanonicalX87HelperKind.Unknown;
    }
}
