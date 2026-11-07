namespace Managed.Emulation.Execution;

// Translation classification and immutable Plan authentication only. This is
// a projection into existing classifier grammars, never a validity decoder or
// a runtime semantic executor. REP reauthentication uses the same projection.
// The original XED legality/length/mode guard and published bytes are mandatory.
// No operand/immediate/displacement is synthesized and no native computation,
// context, helper, ABI, frame, or feature admission is changed here.
internal static class FullCanonicalEncoding
{
    [Flags]
    internal enum PrefixDomain : byte
    {
        SinglePerGroup = 0, RepeatedLegacy = 1, MultipleRex = 2, RexBeforeLegacy = 4
    }

    // Classification evidence only: decoder acceptance is not an assertion
    // that a redundant/undefined sequence is portable architectural syntax.
    // Intel documents at most one REX; the selected scanner accepts aliases.
    internal static PrefixDomain InspectPrefixDomain(ReadOnlySpan<byte> bytes)
    {
        PrefixDomain domain = PrefixDomain.SinglePerGroup;
        int groups = 0, rexes = 0;
        bool sawRex = false;
        foreach (byte b in bytes)
        {
            if (b is >= 0x40 and <= 0x4f)
            { if (++rexes > 1) domain |= PrefixDomain.MultipleRex; sawRex = true; continue; }
            int group = b switch
            {
                0xf0 or 0xf2 or 0xf3 => 1,
                0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 => 2,
                0x66 => 4, 0x67 => 8, _ => 0
            };
            if (group == 0) break;
            if ((groups & group) != 0) domain |= PrefixDomain.RepeatedLegacy;
            if (sawRex) domain |= PrefixDomain.RexBeforeLegacy;
            groups |= group;
        }
        return domain;
    }

    [Flags]
    internal enum Role : ushort
    {
        None = 0, Modrm = 1, Reg = 2, Rm = 4, OpcodeReg = 8,
        Width = 16, Byte = 32, ImplicitMemory = 64, Counter = 128,
        Mandatory = 256, RepSelector = 512, Keep66 = 1024, Lockable = 2048,
        Ignore66 = 4096, NoData = 8192, Setcc = 16384
    }

    // Bit masks below refer to REX.W/R/X/B, not to opcode bits. Byte denotes
    // possible ENCODED GPR byte operands (implicit AH in LAHF is not one).
    // Explicit software x87 profile for exact authoritative decoded spans:
    // repeated/conflicting ignored REP and segment prefixes use the final
    // effective segment/REX, preserving cancellation at every legacy prefix.
    // They do not gain a hardware identity or ordinary-ISA coverage claim.
    private static bool IsSelectedX87Encoding(ReadOnlySpan<byte> bytes)
    {
        int p = 0;
        while (p < bytes.Length && bytes[p] is 0x66 or 0x67 or 0xf0 or 0xf2 or 0xf3 or
            0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or >= 0x40 and <= 0x4f) ++p;
        return p < bytes.Length && (bytes[p] is >= 0xd8 and <= 0xdf or 0x9b ||
            bytes[p] == 0x0f && p + 2 < bytes.Length && bytes[p + 1] == 0xae &&
            bytes[p + 2] < 0xc0 && (bytes[p + 2] >> 3 & 7) <= 1);
    }

    internal static bool TryNormalize(ReadOnlySpan<byte> original, Span<byte> target,
        out int length, out string? reason)
    {
        length = 0; reason = "CanonicalEncodingInvalidLength";
        if (original.Length is < 1 or > 15 || target.Length < 15) return false;
        bool selectedX87 = IsSelectedX87Encoding(original);
        int p = 0, segment = 0, rep = 0, rex = 0, repCount = 0;
        bool word = false, address32 = false, locked = false;
        while (p < original.Length)
        {
            int b = original[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) word = true;
            else if (b == 0x67) address32 = true;
            else if (b == 0xf0) locked = true;
            else if (b is 0xf2 or 0xf3)
            {
                if (!selectedX87 && rep != 0 && rep != b)
                { reason = "CanonicalEncodingConflictingRepUndefinedMISSING"; return false; }
                rep = b; ++repCount;
            }
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
            {
                if (!selectedX87 && segment != 0 && segment != b)
                { reason = "CanonicalEncodingConflictingSegmentsUndefinedMISSING"; return false; }
                segment = b;
            }
            else break;
            // A legacy prefix after a REX cancels the earlier REX, including
            // its presence (AH versus SPL). Never OR together repeated REX.
            rex = 0; ++p;
        }
        int opcodeStart = p, map = 0;
        if (p == original.Length) { reason = "CanonicalEncodingPrefixOnly"; return false; }
        int op = original[p++];
        if (op == 0x0f)
        {
            map = 1;
            if (p == original.Length) { reason = "CanonicalEncodingTruncatedMap"; return false; }
            op = original[p++];
            if (op is 0x38 or 0x3a)
            {
                map = op;
                if (p == original.Length) { reason = "CanonicalEncodingTruncatedMap"; return false; }
                op = original[p++];
            }
        }
        int selector = rep != 0 ? rep : word ? 0x66 : 0;
        if (!TryDescribe(map, op, selector, out Role role))
        { reason = "CanonicalEncodingOpcodePolicyMISSING"; return false; }
        bool hasModrm = (role & Role.Modrm) != 0;
        int modrmAt = p, modrm = hasModrm && p < original.Length ? original[p++] : 0;
        if (hasModrm && modrmAt == original.Length)
        { reason = "CanonicalEncodingTruncatedModrm"; return false; }
        bool memory = hasModrm && modrm < 0xc0 && (role & Role.NoData) == 0;
        int group = modrm >> 3 & 7;

        // Group-dependent operand roles: opcode extensions never use REX.R.
        if (map == 0 && op == 0xff && group is 2 or 4)
            role = (role & ~Role.Width) | Role.Ignore66;
        if (map == 0 && op == 0x8c && memory) role = (role & ~Role.Width) | Role.Ignore66;
        if (map == 0 && op is >= 0xd8 and <= 0xdf)
        {
            // Only FLDENV/FNSTENV/FRSTOR/FNSAVE select a 14/28 or94/108-byte
            // format with66; numeric x87 widths come from opcode/group.
            bool environment = memory && (op == 0xd9 && group is 4 or 6 || op == 0xdd && group is 4 or 6);
            if (environment) role |= Role.Keep66 | Role.Width;
            else role |= Role.Ignore66;
        }
        if (map == 1 && op == 0xae)
        {
            // FXSAVE/FXRSTOR64 have a meaningful W bit. MXCSR operations,
            // fences and FSGSBASE have their own opcode-extension roles.
            if (memory && group is 0 or 1) role |= Role.Width;
            if (!memory && rep == 0xf3 && group <= 3) role |= Role.Width | Role.Rm;
        }

        if (locked && (rep != 0 || !memory || (role & Role.Lockable) == 0 || !LockGroup(map, op, group)))
        {
            reason = rep != 0 ? "CanonicalEncodingHleOrGroup1ConflictMISSING" : "CanonicalEncodingLockNotLegalForRole";
            return false;
        }
        // AMD24592r3.24 6.6 explicitly ignores a single F2/F3 on x87.
        // Keep effective REX.W above: it selects the large legacy image.
        // Repeated group1 prefixes are a distinct undefined alias domain.
        if (map == 0 && (op is >= 0xd8 and <= 0xdf || op == 0x9b) && rep != 0)
        {
            if (!selectedX87 && repCount != 1)
            { reason = "CanonicalX87RepeatedRepUndefinedMISSING"; return false; }
            rep = 0;
        }
        if (rep != 0 && (role & (Role.Mandatory | Role.RepSelector)) == 0)
        { reason = "CanonicalEncodingRepOnNonRepInstructionPolicyMISSING"; return false; }
        if (word && rep != 0 && (role & Role.Mandatory) != 0 && (role & Role.Keep66) == 0)
        { reason = "CanonicalEncodingConflictingMandatorySelectorsMISSING"; return false; }

        int mask = (role & Role.Width) != 0 ? 8 : 0;
        if ((role & Role.Reg) != 0) mask |= 4;
        if ((role & Role.OpcodeReg) != 0 || !memory && (role & Role.Rm) != 0) mask |= 1;
        if (memory)
        {
            int mod = modrm >> 6, rm = modrm & 7;
            if (rm == 4)
            {
                if (p == original.Length) { reason = "CanonicalEncodingTruncatedSib"; return false; }
                int sib = original[p];
                mask |= 2; // X selects R12 even when raw index=4.
                if (mod != 0 || (sib & 7) != 5) mask |= 1;
            }
            else if (mod != 0 || rm != 5) mask |= 1;
            // mod0/rm5 and mod0/SIB.base5 are special even with REX.B.
        }
        bool bytePresence = rex != 0 && (role & Role.Byte) != 0;
        rex = rex == 0 ? 0 : 0x40 | (rex & mask);
        if (rex == 0x40 && !bytePresence) rex = 0;

        bool usesAddress = memory || (role & (Role.ImplicitMemory | Role.Counter)) != 0;
        if (!usesAddress) address32 = false;
        // SS is preserved for ALL memory forms: it affects #SS versus #GP.
        // FS/GS remain attached after the common EA32 truncation.
        if (!memory && (role & Role.ImplicitMemory) == 0) segment = 0;
        if ((role & Role.Ignore66) != 0 || (role & Role.Byte) != 0 && (role & Role.Width) == 0 &&
            (role & (Role.Mandatory | Role.Keep66)) == 0) word = false;
        if ((role & Role.Width) != 0 && (rex & 8) != 0 &&
            (role & (Role.Mandatory | Role.Keep66)) == 0) word = false;

        int n = 0;
        if (locked) target[n++] = 0xf0;
        if (segment != 0) target[n++] = (byte)segment;
        if (address32) target[n++] = 0x67;
        if (word) target[n++] = 0x66;
        if (rep != 0) target[n++] = (byte)rep;
        if (rex != 0) target[n++] = (byte)rex;
        int bodyLength = original.Length - opcodeStart;
        if (bodyLength > 15 - n) { reason = "CanonicalEncodingOutputOverflow"; return false; }
        original[opcodeStart..].CopyTo(target[n..]);
        // SETcc reg is architecturally unused, unlike a /digit extension.
        if ((role & Role.Setcc) != 0) target[n + modrmAt - opcodeStart] &= 0xc7;
        length = n + bodyLength;
        if (length > original.Length) { length = 0; reason = "CanonicalEncodingCannotGrow"; return false; }
        reason = null; return true;
    }

    // All authoritative REP classification/identity sites must use this wrapper,
    // including emitter binding and returned issued-Plan authentication. The
    // BASE classifier stays unchanged, preserving its independent unit tests.
    internal static bool TryClassifyRep(ulong ip, ReadOnlySpan<byte> original,
        out FullRepInstruction instruction, out string? reason)
    {
        if (FullRepLowering.TryClassify(ip, original, out instruction, out reason)) return true;
        Span<byte> canonical = stackalloc byte[15];
        if (!TryNormalize(original, canonical, out int n, out var normalizationReason))
        { reason = normalizationReason; return false; }
        if (original.SequenceEqual(canonical[..n])) return false;
        ulong adjustedIp = unchecked(ip + (uint)(original.Length - n));
        if (!FullRepLowering.TryClassify(adjustedIp, canonical[..n], out var candidate, out reason)) return false;
        if (candidate.Ip != adjustedIp || candidate.Length != n)
        { reason = "CanonicalRepBaseMetadataMismatch"; return false; }
        instruction = candidate with { Ip = ip, Length = original.Length };
        reason = null; return true;
    }

    // Exhaustive opcode-policy table for the EXISTING ordinary lowering
    // families. Returning true is not a claim that every group/selector is
    // valid or emitted: original XED and Core still perform those checks.
    internal static bool TryDescribe(int map, int op, int selector, out Role role)
    {
        const Role MR = Role.Modrm | Role.Reg | Role.Rm;
        const Role MG = Role.Modrm | Role.Rm;
        const Role W = Role.Width;
        const Role B = Role.Byte;
        const Role L = Role.Lockable;
        const Role V = Role.Mandatory;
        role = Role.None;
        if (map == 0)
        {
            if (op <= 0x3d && (op & 7) <= 5)
            {
                role = (op & 7) <= 3 ? MR | ((op & 2) == 0 && op < 0x38 ? L : Role.None) : Role.None;
                role |= (op & 1) == 0 ? B | Role.Ignore66 : W;
                return true;
            }
            if (op is >= 0x50 and <= 0x5f) { role = Role.OpcodeReg | W | Role.Keep66; return true; }
            if (op is >= 0x70 and <= 0x7f) { role = Role.Ignore66; return true; }
            if (op is >= 0x90 and <= 0x97)
            { role = Role.OpcodeReg | W | (op == 0x90 ? Role.RepSelector : Role.None); return true; }
            if (op is >= 0xb0 and <= 0xbf)
            { role = Role.OpcodeReg | (op < 0xb8 ? B | Role.Ignore66 : W); return true; }
            if (op == 0x9b) { role = Role.NoData | Role.Ignore66; return true; }
            if (op is >= 0xd8 and <= 0xdf) { role = Role.Modrm; return true; }
            role = op switch
            {
                0x63 or 0x69 or 0x6b => MR | W,
                0x68 or 0x6a => W | Role.Keep66,
                0x80 => MG | B | L | Role.Ignore66,
                0x81 or 0x83 => MG | W | L,
                0x84 or 0x88 or 0x8a => MR | B | Role.Ignore66,
                0x86 => MR | B | L | Role.Ignore66,
                0x85 or 0x89 or 0x8b or 0x8d => MR | W,
                0x87 => MR | W | L,
                0x8c => MG | W,
                0x8f => MG | W | Role.Keep66,
                0x98 or 0x99 => W,
                0x9c or 0x9d or 0xc8 or 0xc9 => W | Role.Keep66,
                0x9e or 0x9f => Role.Ignore66,
                0xa0 or 0xa2 => B | Role.ImplicitMemory | Role.Ignore66,
                0xa1 or 0xa3 => W | Role.ImplicitMemory,
                0xa4 or 0xa6 or 0xaa or 0xac or 0xae => B | Role.ImplicitMemory | Role.RepSelector | Role.Ignore66,
                0xa5 or 0xa7 or 0xab or 0xad or 0xaf => W | Role.ImplicitMemory | Role.RepSelector,
                0xa8 => B | Role.Ignore66,
                0xa9 => W,
                0xc0 or 0xd0 or 0xd2 => MG | B | Role.Ignore66,
                0xc1 or 0xd1 or 0xd3 => MG | W,
                0xc2 or 0xc3 or 0xe8 or 0xe9 => Role.Keep66,
                0xc6 => MG | B | Role.Ignore66,
                0xc7 => MG | W,
                0xd7 => B | Role.ImplicitMemory | Role.Ignore66,
                0xe0 or 0xe1 or 0xe2 or 0xe3 => Role.Counter | Role.Ignore66,
                0xeb => Role.Ignore66,
                0xf5 or 0xf8 or 0xf9 or 0xfc or 0xfd or 0x9b => Role.Ignore66,
                0xf6 or 0xfe => MG | B | L | Role.Ignore66,
                0xf7 or 0xff => MG | W | L,
                _ => (Role)ushort.MaxValue
            };
        }
        else if (map == 1)
        {
            if (op is >= 0x40 and <= 0x4f) { role = MR | W; return true; }
            if (op is >= 0x80 and <= 0x8f) { role = Role.Keep66; return true; }
            if (op is >= 0x90 and <= 0x9f) { role = MG | B | Role.Ignore66 | Role.Setcc; return true; }
            if (op is >= 0xc8 and <= 0xcf) { role = Role.OpcodeReg | W | Role.Keep66; return true; }
            role = op switch
            {
                0x0d or 0x18 => Role.Modrm | Role.Ignore66,
                0x1e => Role.Modrm | Role.NoData | Role.RepSelector | Role.Keep66,
                0x1f => Role.Modrm | Role.NoData | Role.Ignore66,
                0xa3 or 0xa4 or 0xa5 or 0xac or 0xad or 0xaf => MR | W,
                0xab or 0xb3 or 0xbb => MR | W | L,
                0xba => MG | W | L,
                0xb0 or 0xc0 => MR | B | L | Role.Ignore66,
                0xb1 or 0xc1 => MR | W | L,
                0xb6 or 0xbe => MR | W | B,
                0xb7 or 0xbf or 0xc3 => MR | W,
                0xb8 or 0xbc or 0xbd => MR | W | Role.RepSelector | Role.Keep66,
                0xc7 => Role.Modrm | W | L | Role.Ignore66,
                0xae => Role.Modrm | Role.RepSelector | Role.Keep66,
                0x77 => Role.Ignore66,
                _ => (Role)ushort.MaxValue
            };
            if (role != (Role)ushort.MaxValue) return true;
            // Legacy SIMD/MMX table; these are the opcodes of the actual
            // vector/move/float/float-MMX/MMX/masked-store lowerers.
            bool simd = op is >= 0x10 and <= 0x17 or >= 0x28 and <= 0x2f or
                >= 0x50 and <= 0x76 or 0x7c or 0x7d or 0x7e or 0x7f or
                0xc2 or 0xc4 or 0xc5 or 0xc6 or >= 0xd0 and <= 0xfe;
            if (!simd) return false;
            role = MR | V;
            // MASKMOVQ/DQU encodes two registers but addresses implicit
            // (E)DI. Address-size and the source segment remain meaningful.
            if (op == 0xf7) role |= Role.ImplicitMemory;
            bool mmx = selector == 0 && (op is >= 0x60 and <= 0x76 or 0x7e or 0x7f or >= 0xd1 and <= 0xfe);
            if (mmx) role &= ~(Role.Reg | Role.Rm);
            if (op is 0x71 or 0x72 or 0x73) role &= ~Role.Reg;
            if (op is 0x6e or 0x7e && selector is 0 or 0x66) role |= Role.Rm | W;
            if (op is 0xc4 && selector == 0) role |= Role.Rm;
            if (op is 0xc5 or 0xd7 && selector == 0) role |= Role.Reg;
            if (op == 0xd6 && selector == 0xf2) role &= ~Role.Reg;
            if (op == 0xd6 && selector == 0xf3) role &= ~Role.Rm;
            if (op is 0x2a or 0x2c or 0x2d)
            {
                if (selector is 0xf2 or 0xf3) role |= W;
                else if (op == 0x2a) role &= ~Role.Rm;
                else role &= ~Role.Reg;
            }
            return true;
        }
        else if (map is 0x38 or 0x3a)
        {
            if (map == 0x38 && op is 0xf0 or 0xf1)
            { role = MR | W | V | Role.Keep66 | (op == 0xf0 ? B : Role.None); return true; }
            bool known = map == 0x38 ? op is >= 0 and <= 0x0b or 0x1c or 0x1d or 0x1e or 0x2a or 0x40 : op is 0x0f or 0x44;
            if (!known) return false;
            role = Role.Modrm | V | (selector == 0 ? Role.None : Role.Reg | Role.Rm);
        }
        else return false;
        return role != (Role)ushort.MaxValue;
    }

    private static bool LockGroup(int map, int op, int group) => map == 0 ? op switch
    {
        <= 0x31 when (op & 7) <= 1 => true,
        0x80 or 0x81 or 0x83 => group != 7,
        0x86 or 0x87 => true,
        0xf6 or 0xf7 => group is 2 or 3,
        0xfe or 0xff => group <= 1,
        _ => false
    } : map == 1 && (op is 0xab or 0xb3 or 0xbb or 0xb0 or 0xb1 or 0xc0 or 0xc1 ||
        op == 0xba && group >= 5 || op == 0xc7 && group == 1);
}
