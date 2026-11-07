namespace Managed.Emulation.Execution;

// Translation-time grammar for EXACT authoritative XED-length bytes, in the
// fixed flat long-mode guest profile. This does not fetch or decode at runtime.
// Every accepted instruction has no guest data operand, including ModRM/SIB
// NOPs: consume their encoding, never calculate/translate/dereference their EA.
internal static class FullHintLowering
{
    // Policy, not host feature detection: native8's guest CPUID(7,0) advertises
    // neither CET_SS (ECX7) nor CET_IBT (EDX20); guest CET cannot be enabled in
    // this profile. Exact ENDBR64/32 are therefore architectural no-ops. If a
    // future guest context exposes/enables CET, this classifier must be gated
    // by that context before it is reused. Host CET is unrelated to this policy.
    internal const bool GuestCetDisabledBaseline = true;

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "MISSING native hint encoding";
        if (bytes.Length is < 1 or > 15)
        { reason = "invalid hint instruction length"; return false; }

        // Do not admit the entire historical 0F1E hint/reserved opcode space,
        // or extra prefixes on ENDBR, merely because Blink's handler is empty.
        if (bytes.Length == 4 && bytes[0] == 0xf3 && bytes[1] == 0x0f &&
            bytes[2] == 0x1e && bytes[3] is 0xfa or 0xfb)
            return Finish(ip, bytes.Length, JitOperation.Nop, out instruction, out reason);

        int p = 0, rex = 0;
        bool rep3 = false, segment = false, addressSize = false;
        // One legacy prefix from each group, in any inter-group order, except
        // repeated 66 is explicitly supported for compiler alignment padding.
        // One REX may follow ALL legacy prefixes, immediately before the opcode.
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b == 0x66) { ++p; continue; }
            if (b == 0x67)
            {
                if (addressSize) { reason = "MISSING repeated hint address prefix"; return false; }
                addressSize = true; ++p; continue;
            }
            if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
            {
                if (segment) { reason = "MISSING repeated hint segment prefix"; return false; }
                segment = true; ++p; continue;
            }
            if (b == 0xf3)
            {
                if (rep3) { reason = "MISSING repeated hint REP prefix"; return false; }
                rep3 = true; ++p; continue;
            }
            if (b is 0xf0 or 0xf2)
            { reason = b == 0xf0 ? "invalid LOCK hint prefix" : "MISSING hint REPNE prefix"; return false; }
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; }
            break;
        }
        if (p == bytes.Length) { reason = "hint prefix without opcode"; return false; }
        byte opcode = bytes[p++];
        if (IsPrefix(opcode)) { reason = "MISSING hint prefix after REX"; return false; }

        JitOperation operation;
        if (opcode == 0x90)
        {
            // Preserve the selected normal decoder/OpNop precedence, including
            // F3+REX.B. This is an exchange with R8, never an accumulator NOP.
            // Unprefixed/REX.B=0 90 must NOT zero-extend the guest's EAX.
            if ((rex & 1) != 0)
            { reason = "MISSING hint: REX.B 90 is a real register exchange"; return false; }
            operation = rep3 ? JitOperation.Pause : JitOperation.Nop;
        }
        else if (opcode == 0x0f && p < bytes.Length && bytes[p++] == 0x1f)
        {
            if (rep3) { reason = "MISSING REP on multi-byte NOP"; return false; }
            if (p == bytes.Length) { reason = "truncated NOP ModRM"; return false; }
            int modrm = bytes[p++], mod = modrm >> 6, rm = modrm & 7;
            // Only the documented /0, not the reserved /1../7 aliases. REX.R
            // does not extend an opcode-extension field. REX.B/X/W, 66, 67 and
            // segment overrides cannot turn this nonaccess into a memory read.
            if ((modrm & 0x38) != 0)
            { reason = "MISSING reserved NOP group code"; return false; }
            if (mod != 3)
            {
                int displacement = mod == 1 ? 1 : mod == 2 ? 4 : 0;
                if (rm == 4)
                {
                    if (p == bytes.Length) { reason = "truncated NOP SIB"; return false; }
                    int sib = bytes[p++];
                    if (mod == 0 && (sib & 7) == 5) displacement = 4;
                }
                else if (mod == 0 && rm == 5) displacement = 4;
                if (displacement > bytes.Length - p)
                { reason = "truncated NOP displacement"; return false; }
                p += displacement;
            }
            // All /0 EAs have architectural no-effects semantics, including
            // ModRM45/07/47/87. Blink's BOFRAM/onbinbase hooks for these bytes
            // are nonarchitectural metadata/callbacks, deliberately not run by
            // this Linux flat profile (onbinbase is unbound). A future profile
            // requiring those hooks needs separate qualification/admission.
            operation = JitOperation.Nop;
        }
        else return false;

        if (p != bytes.Length) { reason = "hint encoding has trailing bytes"; return false; }
        return Finish(ip, bytes.Length, operation, out instruction, out reason);
    }

    private static bool IsPrefix(byte b) => b is 0x26 or 0x2e or 0x36 or 0x3e or
        0x64 or 0x65 or 0x66 or 0x67 or 0xf0 or 0xf2 or 0xf3 or >= 0x40 and <= 0x4f;

    private static bool Finish(ulong ip, int length, JitOperation operation,
        out JitInstruction instruction, out string? reason)
    {
        instruction = new(ip, length, operation, 64, default, default);
        reason = null;
        return true;
    }
}
