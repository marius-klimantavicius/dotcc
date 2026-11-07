namespace Managed.Emulation.Execution;

// Translation-time finite grammar AFTER authoritative XED long-mode decoding.
// The input is exactly decoded.length bytes. This never runs guest semantics.
internal static class FullControlFlagLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "MISSING native control/flags encoding";
        if (bytes.Length is < 1 or > 15)
        { reason = "invalid control/flags instruction length"; return false; }
        int p = 0, groups = 0;
        bool address32 = false;
        while (p < bytes.Length)
        {
            int group = bytes[p] switch
            {
                0x66 => 2,
                0x67 => 4,
                _ => 0
            };
            if (group == 0) break;
            if ((groups & group) != 0)
            { reason = "MISSING repeated control/flags legacy prefix group"; return false; }
            groups |= group;
            if (bytes[p] == 0x67) address32 = true;
            ++p;
        }
        // One final REX is ignored: the operands are implicit RCX/ECX or AH.
        // In particular REX.W does not choose the counter width, and REX.B
        // cannot redirect the implicit AH operand to an extended register.
        if (p < bytes.Length && bytes[p] is >= 0x40 and <= 0x4f) ++p;
        if (p == bytes.Length)
        { reason = "truncated control/flags opcode"; return false; }
        int opcode = bytes[p++];
        if (opcode is 0xf0 or 0xf2 or 0xf3 or 0x66 or 0x67 or
            0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 || opcode is >= 0x40 and <= 0x4f)
        { reason = "MISSING control/flags prefix combination (LOCK/REP/segment/reordered REX)"; return false; }
        JitOperation operation;
        int width;
        ulong target = 0;
        if (opcode is >= 0xe0 and <= 0xe3)
        {
            if (p != bytes.Length - 1)
            { reason = "control branch requires exactly one rel8 byte"; return false; }
            operation = opcode switch
            {
                0xe0 => JitOperation.LoopNotEqual,
                0xe1 => JitOperation.LoopEqual,
                0xe2 => JitOperation.Loop,
                _ => JitOperation.CounterZeroJump
            };
            // Operand-size66 is ignored. In long mode67 selects ECX, never CX.
            width = address32 ? 32 : 64;
            target = unchecked(ip + (uint)bytes.Length + (ulong)(sbyte)bytes[p]);
            // Target canonicality is checked by the native near family only
            // when this branch is taken; classification must retain the site.
        }
        else if (opcode is 0x9e or 0x9f)
        {
            if (p != bytes.Length)
            { reason = "control flags encoding has trailing bytes"; return false; }
            operation = opcode == 0x9f ? JitOperation.LoadFlagsAh : JitOperation.StoreFlagsAh;
            width = 8;
        }
        else return false;
        instruction = new(ip, bytes.Length, operation, width, default, default, Target: target);
        reason = null;
        return true;
    }

    // Matches the existing guest's 48-bit canonical-address baseline. This is
    // an emission-time target proof predicate, never an eligibility filter.
    internal static bool CanonicalTarget(ulong target) =>
        unchecked((ulong)((long)(target << 16) >> 16)) == target;
}
