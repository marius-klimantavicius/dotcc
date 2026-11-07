namespace Managed.Emulation.Execution;

// R120 translation-time grammar over an EXACT authoritative long-mode XED span.
// No VEX/EVEX, REP, LOCK, arbitrary memory ModRM, or managed semantic fallback.
internal static class FullMaskedStoreLowering
{
    internal const uint Mmx = 0x3007; // R113 reserved slot; core must dispatch R120 first
    internal const uint Xmm = 0x5001;

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeMaskedStoreEncodingMissing";
        if (bytes.Length is < 3 or > 15) return false;
        int p = 0, segment = -1;
        byte rex = 0; bool xmm = false, address32 = false;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) xmm = true;
            else if (b == 0x67) address32 = true;
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else if (b == 0xf0) { reason = "NativeMaskedStoreLockIllegal"; return false; }
            else if (b is 0xf2 or 0xf3) { reason = "NativeMaskedStoreRepFormExcluded"; return false; }
            else break;
            rex = 0; ++p; // any later legacy prefix cancels preceding REX
        }
        if (p + 3 != bytes.Length || bytes[p++] != 0x0f || bytes[p++] != 0xf7) return false;
        byte modrm = bytes[p];
        if ((modrm & 0xc0) != 0xc0) { reason = "NativeMaskedStoreModrmMustBeRegister"; return false; }
        int source = modrm >> 3 & 7, mask = modrm & 7;
        if (xmm) { source |= (rex & 4) << 1; mask |= (rex & 1) << 3; }
        // MMX extensions are ignored; REX.B NEVER extends implicit (E)DI.
        int width = xmm ? 128 : 64;
        instruction = new(ip, bytes.Length, JitOperation.VectorInteger, width,
            new(0, 0, IsMemory: true), new(source, 0), SourceWidth: width,
            Address: new(7, -1, 0, 0, false, address32 ? 32 : 64, segment),
            Extra: new(mask, 0), Options: xmm ? Xmm : Mmx);
        reason = null; return true;
    }
}
