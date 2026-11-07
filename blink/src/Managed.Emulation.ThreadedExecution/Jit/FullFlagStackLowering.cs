namespace Managed.Emulation.Execution;

// Exact authoritative long-mode decoder span. Stack addressing is always RSP,
// even with 67 or an ignored segment override. No semantic operation is called.
internal static class FullFlagStackLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeFlagStackEncodingMissing";
        if (bytes.Length is < 1 or > 15) return false;
        int p = 0; bool word = false; byte rex = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) word = true;
            else if (b != 0x67 && b is not (0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)) break;
            rex = 0; ++p;
        }
        if (p != bytes.Length - 1 || bytes[p] is not (0x9c or 0x9d)) return false;
        // Like the other long-mode stack operations, an effective REX.W wins
        // over 66; no 32-bit stack operand form exists in long mode.
        int width = word && (rex & 8) == 0 ? 16 : 64;
        instruction = new(ip, bytes.Length,
            bytes[p] == 0x9c ? JitOperation.PushFlags : JitOperation.PopFlags,
            width, default, default);
        reason = null; return true;
    }
}
