namespace Managed.Emulation.Execution;

// Exact, translation-time grammar AFTER authoritative XED long-mode decode.
// This is not a validity decoder and never examines runtime target values.
internal static class FullNearControlLowering
{
    // Family ownership only, not legality. Core uses this to prevent an older
    // classifier from admitting a rejected width/prefix as another near alias.
    internal static bool IsEncoding(ReadOnlySpan<byte> bytes)
    {
        int p=0;
        while(p<bytes.Length && (bytes[p] is 0xf0 or 0xf2 or 0xf3 or 0x66 or 0x67 or
            0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or >=0x40 and <=0x4f)) ++p;
        if(p==bytes.Length)return false;
        int op=bytes[p++];
        if(op==0xff)return p<bytes.Length && ((bytes[p]>>3)&7) is 2 or 4;
        if(op==0x0f)return p<bytes.Length && bytes[p] is >=0x80 and <=0x8f;
        return op is >=0x70 and <=0x7f or >=0xe0 and <=0xe3 or 0xe8 or 0xe9 or 0xeb or 0xc2 or 0xc3;
    }

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "MISSING native near-control encoding";
        if (bytes.Length is < 1 or > 15) return false;
        int p = 0, groups = 0, segment = -1, rep = 0;
        bool word = false, address32 = false;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            int group = b switch { 0xf0 or 0xf2 or 0xf3 => 1,
                0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 => 2,
                0x66 => 4, 0x67 => 8, _ => 0 };
            if (group == 0) break;
            if ((groups & group) != 0)
            { reason = "MISSING repeated/undefined near-control legacy prefix group"; return false; }
            groups |= group; ++p;
            if (b == 0xf0) { reason = "invalid LOCK near-control"; return false; }
            if (group == 1) rep = b;
            else if (group == 2) segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else if (group == 4) word = true;
            else address32 = true;
        }
        byte rex = 0;
        if (p < bytes.Length && bytes[p] is >= 0x40 and <= 0x4f) rex = bytes[p++];
        if (p == bytes.Length) return false;
        int op = bytes[p++], width = 64, condition = 0;
        JitOperation operation;
        JitOperand source = default;
        JitAddress address = default;
        ulong target = 0;
        if (op == 0x0f)
        {
            if (p == bytes.Length || bytes[p] is < 0x80 or > 0x8f) return false;
            // CURRENT explicit software CPU profile: long-mode near Jcc uses
            // rel32/target64 even with66. Exact selected XED span is authority.
            condition = bytes[p++] & 15; operation = JitOperation.ConditionalJump;
            if (!Relative(bytes, ref p, ip, 4, out target)) return false;
        }
        else if (op is >= 0x70 and <= 0x7f)
        { operation = JitOperation.ConditionalJump; condition = op & 15; if (!Relative(bytes, ref p, ip, 1, out target)) return false; }
        else if (op is 0xe8 or 0xe9 or 0xeb)
        {
            // CURRENT selected XED/profile ignores66: E8/E9 rel32/target64,
            // CALL push8 with full RSP. EB stays rel8/full RIP. The parent's
            // AMD rel16 hardware rows describe a DIFFERENT vendor profile.
            operation = op == 0xe8 ? JitOperation.Call : JitOperation.Jump;
            if (!Relative(bytes, ref p, ip, op == 0xeb ? 1 : 4, out target)) return false;
            if (op == 0xe8) address = new(4, -1, 0, -8);
        }
        else if (op is >= 0xe0 and <= 0xe3)
        {
            width = address32 ? 32 : 64;
            operation = op switch { 0xe0 => JitOperation.LoopNotEqual, 0xe1 => JitOperation.LoopEqual,
                0xe2 => JitOperation.Loop, _ => JitOperation.CounterZeroJump };
            if (!Relative(bytes, ref p, ip, 1, out target)) return false;
        }
        else if (op is 0xc2 or 0xc3)
        {
            // CURRENT near-return profile uses kStackOsz[Osz][LONG]:66
            // chooses pop2, including an ignored REX.W; address stays fullRSP.
            // PUSH/POP/LEAVE width policy belongs to their existing family.
            width = word ? 16 : 64;
            operation = op == 0xc2 ? JitOperation.ReturnAdjust : JitOperation.Ret;
            address = new(4, -1, 0, 0);
            if (op == 0xc2 && !FullMemoryLowering.Immediate(bytes, ref p, 2, false, out source)) return false;
        }
        else if (op == 0xff)
        {
            if (p == bytes.Length) return false;
            byte modrm = bytes[p++]; int extension = modrm >> 3 & 7;
            // REX.R is ignored for an opcode extension. /3 and /5 are FAR,
            // never aliases of /2 and /4; /6 is PUSH, /7 is reserved.
            if (extension is not (2 or 4)) return false;
            operation = extension == 2 ? JitOperation.IndirectCall : JitOperation.IndirectJump;
            if (modrm >= 0xc0) source = new((modrm & 7) | ((rex & 1) << 3), 0);
            else
            {
                if (!FullMemoryLowering.Address(bytes, ref p, modrm, rex, address32, segment, out address)) return false;
                source = new(0, 0, IsMemory: true);
            }
        }
        else return false;
        // F3 RET is the established REP-RET hint; F2 is BND on a separate MPX
        // domain, and REP on other branches is not asserted portable here.
        if (rep != 0 && !(rep == 0xf3 && op is 0xc2 or 0xc3))
        { reason = "MISSING near-control REP/BND feature or undefined-prefix domain"; return false; }
        if (p != bytes.Length) return false;
        int sourceWidth = operation is JitOperation.Call or JitOperation.Ret or JitOperation.ReturnAdjust or
            JitOperation.IndirectCall or JitOperation.IndirectJump ? width : 0;
        instruction = new(ip, bytes.Length, operation, width, default, source, Target: target,
            Condition: condition, SourceWidth: sourceWidth, Address: address);
        reason = null; return true;

    }

    private static bool Relative(ReadOnlySpan<byte> bytes, ref int p, ulong ip, int count, out ulong target)
    {
        target = 0;
        if (!FullMemoryLowering.Immediate(bytes, ref p, count, true, out var displacement)) return false;
        target = unchecked(ip + (uint)bytes.Length + displacement.Immediate);
        return true;
    }
}
