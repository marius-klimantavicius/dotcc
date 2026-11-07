namespace Managed.Emulation.Execution;

// Translation-time grammar over ONE exact authoritative long-mode XED span.
// Selected native9 stack.c implements only raw nesting byte zero. Other raw
// values, including ISA modulo32 aliases, remain explicitly outside this slice.
internal static class FullEnterLowering
{
    internal const uint MemoryTag = 0x454e0127;

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeEnterEncodingMissing";
        if (bytes.Length is < 4 or > 15) return false;
        int p = 0; bool word = false; byte rex = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) word = true;
            else if (b != 0x67 && b is not (0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)) break;
            rex = 0; ++p; // legacy prefix invalidates a preceding REX
        }
        if (p != bytes.Length - 4 || bytes[p] != 0xc8)
        { reason = "NativeEnterExactLengthMissing"; return false; }
        if (bytes[p + 3] != 0)
        {
            reason = (bytes[p + 3] & 31) == 0 ?
                "NativeEnterRawHighBitsSelectedDifference" : "NativeEnterNestingMissing";
            return false;
        }
        // ENTER uses XED DF64(), not FORCE64 or IMMUNE_REXW. Effective REX.W
        // wins over66. No32-bit form; 67/segments do not change implicit RSP64.
        int width = word && (rex & 8) == 0 ? 16 : 64;
        ushort allocation = (ushort)(bytes[p + 1] | bytes[p + 2] << 8);
        instruction = new(ip, bytes.Length, JitOperation.Enter, width, default, default,
            SourceWidth: width, Address: new(4, -1, 0, 0),
            Extra: new(0, allocation, IsImmediate: true));
        reason = null; return true;
    }
}
