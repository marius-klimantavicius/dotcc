namespace Managed.Emulation.Execution;

// Translation-time grammar AFTER authoritative XED long-mode decoding. Input
// is exactly decoded.length bytes. REP belongs to the existing issued
// continuation grammar, never this single-iteration grammar.
internal static class FullStringLowering
{
    internal static bool IsString(JitInstruction i) => i.Operation is
        JitOperation.StringMove or JitOperation.StringStore or JitOperation.StringLoad or
        JitOperation.StringCompare or JitOperation.StringScan;

    internal static bool HasSource(JitInstruction i) => i.Operation is
        JitOperation.StringMove or JitOperation.StringLoad or JitOperation.StringCompare;

    internal static bool HasDestination(JitInstruction i) => IsString(i) &&
        i.Operation != JitOperation.StringLoad;

    internal static bool IsPair(JitInstruction i) => i.Operation is
        JitOperation.StringMove or JitOperation.StringCompare;

    // Parent mapping/controller authorization must check the authenticated
    // string operation, not infer a stack operation from descriptor.Count.
    internal static FullMemoryAccess PairSecondAccess(JitInstruction i) => i.Operation switch
    {
        JitOperation.StringMove => FullMemoryAccess.Write,
        JitOperation.StringCompare => FullMemoryAccess.Read,
        _ => throw new ArgumentException("Not a native string pair")
    };

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "MISSING native string encoding";
        if (bytes.Length is < 1 or > 15)
        { reason = "invalid string instruction length"; return false; }
        int p = 0, groups = 0, segment = -1;
        bool operand16 = false, address32 = false;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            int group = b switch
            {
                0x66 => 1, 0x67 => 2,
                0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 => 4,
                _ => 0
            };
            if (group == 0) break;
            if ((groups & group) != 0)
            { reason = "MISSING repeated string legacy prefix group"; return false; }
            groups |= group;
            if (b == 0x66) operand16 = true;
            else if (b == 0x67) address32 = true;
            else segment = b == 0x36 ? 2 : b == 0x64 ? 4 : b == 0x65 ? 5 : -1;
            ++p;
        }
        byte rex = 0;
        if (p < bytes.Length && bytes[p] is >= 0x40 and <= 0x4f) rex = bytes[p++];
        if (p == bytes.Length)
        { reason = "truncated string opcode"; return false; }
        byte opcode = bytes[p++];
        if (opcode is 0xf2 or 0xf3)
        { reason = "REP string requires existing issued continuation/fault-flags path"; return false; }
        if (opcode == 0xf0)
        { reason = "invalid LOCK string prefix"; return false; }
        JitOperation operation = opcode switch
        {
            0xa4 or 0xa5 => JitOperation.StringMove,
            0xaa or 0xab => JitOperation.StringStore,
            0xac or 0xad => JitOperation.StringLoad,
            0xa6 or 0xa7 => JitOperation.StringCompare,
            0xae or 0xaf => JitOperation.StringScan,
            _ => (JitOperation)(-1)
        };
        if ((int)operation < 0) return false;
        if (p != bytes.Length)
        { reason = "string encoding has trailing bytes"; return false; }
        int width = (opcode & 1) == 0 ? 8 : (rex & 8) != 0 ? 64 : operand16 ? 16 : 32;
        bool source = operation is JitOperation.StringMove or JitOperation.StringLoad or JitOperation.StringCompare;
        // Long-mode SS has base zero but retains source #SS12 identity.
        // Destination-only overrides are ignored: destination is always ES:DI.
        instruction = new(ip, bytes.Length, operation, width, default, default,
            SourceWidth: width, Address: new(source ? 6 : 7, -1, 0, 0, false,
                address32 ? 32 : 64, source ? segment : -1));
        reason = null;
        return true;
    }
}
