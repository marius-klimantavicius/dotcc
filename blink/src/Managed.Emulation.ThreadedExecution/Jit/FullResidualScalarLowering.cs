namespace Managed.Emulation.Execution;

// These are selected-emulator domains, not assertions that every encoding is
// defined by Intel/AMD. Invoke only after authoritative long-mode exact decode.
internal enum FullResidualScalarDomain
{
    OrdinaryHint,
    SelectedHintExtension,
    SelectedSalcExtension,
    SelectedEnterPrefixAlias,
    SelectedProcessorExtension
}

internal static class FullResidualScalarLowering
{
    internal const uint Salc = 0x6001, ClearInterrupt = 0x6002, SetInterrupt = 0x6003, CacheWritebackFence = 0x6004;

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason) =>
        TryClassify(ip, bytes, out instruction, out _, out reason);

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out FullResidualScalarDomain domain, out string? reason)
    {
        instruction = default; domain = default; reason = "NativeResidualScalarEncodingMissing";
        if (bytes.Length is < 1 or > 15) return false;
        int p = 0, rep = 0; byte rex = 0; bool word = false;
        bool ordinaryPrefixes = FullCanonicalEncoding.InspectPrefixDomain(bytes) ==
            FullCanonicalEncoding.PrefixDomain.SinglePerGroup;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b == 0xf0) { reason = "NativeResidualScalarLockInvalid"; return false; }
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b is 0xf2 or 0xf3) rep = b;
            else if (b == 0x66) word = true;
            else if (b is not (0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or 0x66 or 0x67)) break;
            rex = 0; ++p; // legacy prefix cancels preceding REX
        }
        if (p == bytes.Length) return false;
        byte opcode = bytes[p++];
        if ((opcode is 0xfa or 0xfb) && p == bytes.Length)
        {
            // Selected DISABLE_METAL semantics change software IF only. This
            // deliberately does not claim architectural CPL3 CLI/STI legality.
            domain = FullResidualScalarDomain.SelectedProcessorExtension;
            instruction = new(ip, bytes.Length, JitOperation.SystemState, 32, default, default,
                Options: opcode == 0xfa ? ClearInterrupt : SetInterrupt);
            reason = null; return true;
        }
        if (opcode == 0x0f && p + 1 == bytes.Length && bytes[p] == 0x09)
        {
            // Selected OpWbinvd delegates to OpMfence. Emit its ordering, never
            // execute the privileged host WBINVD opcode or flush host caches.
            domain = FullResidualScalarDomain.SelectedProcessorExtension;
            instruction = new(ip, bytes.Length, JitOperation.SystemState, 64, default, default,
                Options: CacheWritebackFence);
            reason = null; return true;
        }
        if (opcode == 0x0f && p + 2 == bytes.Length && bytes[p] == 0x01 && bytes[p + 1] == 0xf8)
        {
            // Only selected DISABLE_METAL Swapgs() is empty. Architectural
            // SWAPGS at CPL3 raises #GP(0); do not execute it on the host or
            // claim privileged architectural coverage. RDTSCP F9 stays outside.
            domain = FullResidualScalarDomain.SelectedProcessorExtension;
            instruction = new(ip, bytes.Length, JitOperation.Nop, 64, default, default);
            reason = null; return true;
        }
        if (opcode == 0xc8)
        {
            if (p + 3 != bytes.Length) return false;
            if (bytes[p + 2] != 0)
            { reason = "SelectedEnterNonzeroRawNestingUnsupported"; return false; }
            if (rep == 0) return false; // existing FullEnterLowering owns ordinary forms
            int width = word && (rex & 8) == 0 ? 16 : 64;
            ushort allocation = (ushort)(bytes[p] | bytes[p + 1] << 8);
            // The selected handler ignores REP and rejects every nonzero raw
            // imm8, INCLUDING modulo32 aliases. Reuse the exact nesting0 IR.
            domain = FullResidualScalarDomain.SelectedEnterPrefixAlias;
            instruction = new(ip, bytes.Length, JitOperation.Enter, width, default, default,
                SourceWidth: width, Address: new(4, -1, 0, 0), Extra: new(0, allocation, IsImmediate: true));
        }
        else if (opcode == 0xd6)
        {
            if (p != bytes.Length) return false;
            // AMD APM vol2 table2-3 lists SALC D6 as invalid in64-bit mode.
            // Selected machine.c nevertheless implements AL=CF?255:0. Never
            // emit host SALC; retain this explicit emulator extension identity.
            domain = FullResidualScalarDomain.SelectedSalcExtension;
            instruction = new(ip, bytes.Length, JitOperation.SystemState, 8,
                new(0, 0), default, Options: Salc);
        }
        else
        {
            JitOperation operation = JitOperation.Nop;
            bool prefixed = rep != 0;
            if (opcode == 0x90)
            {
                if (p != bytes.Length || (rex & 1) != 0) return false;
                // Selected OpNop gives REX.B exchange priority over REP3.
                operation = rep == 0xf3 ? JitOperation.Pause : JitOperation.Nop;
                domain = !ordinaryPrefixes || prefixed && rep != 0xf3 ? FullResidualScalarDomain.SelectedHintExtension :
                    FullResidualScalarDomain.OrdinaryHint;
            }
            else
            {
                if (opcode != 0x0f || p + 1 >= bytes.Length) return false;
                opcode = bytes[p++];
                if (opcode != 0x0d && opcode is not (>= 0x18 and <= 0x1f)) return false;
                int modrm = bytes[p++], mod = modrm >> 6, rm = modrm & 7, group = modrm >> 3 & 7;
                if (mod != 3)
                {
                    int displacement = mod == 1 ? 1 : mod == 2 ? 4 : 0;
                    if (rm == 4)
                    {
                        if (p == bytes.Length) return false;
                        int sib = bytes[p++];
                        if (mod == 0 && (sib & 7) == 5) displacement = 4;
                    }
                    else if (mod == 0 && rm == 5) displacement = 4;
                    if (displacement > bytes.Length - p) return false;
                    p += displacement;
                }
                if (p != bytes.Length) return false;
                // Only documented NOP /0 and PREFETCHh m8 /0../3 receive
                // OrdinaryHint classification. The other selected empty
                // handlers are explicitly extensions/reserved-domain aliases.
                // CET/MPX/newer feature interpretations require another profile.
                domain = ordinaryPrefixes && !prefixed && (opcode == 0x1f && group == 0 ||
                    opcode == 0x18 && mod != 3 && group <= 3) ?
                    FullResidualScalarDomain.OrdinaryHint : FullResidualScalarDomain.SelectedHintExtension;
            }
            instruction = new(ip, bytes.Length, operation, 64, default, default);
        }
        reason = null; return true;
    }
}
