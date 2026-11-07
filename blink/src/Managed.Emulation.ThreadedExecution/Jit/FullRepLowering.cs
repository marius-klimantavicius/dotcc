namespace Managed.Emulation.Execution;

// R100 translation-time IR, independent of the ABI proposal and R94 enum.
// This file does not admit execution. Parent must wire it only with an approved
// issued-instruction continuation, native kernel and ordered mapping route.
internal enum FullRepOperation : uint { Move, Store, Load, Compare, Scan }
internal enum FullRepRepeat : uint { Count = 1, WhileEqual, WhileNotEqual }

internal readonly record struct FullRepInstruction(ulong Ip, int Length,
    FullRepOperation Operation, int Width, int AddressWidth, int SourceSegment,
    FullRepRepeat Repeat)
{
    internal bool HasSource => Operation is FullRepOperation.Move or
        FullRepOperation.Load or FullRepOperation.Compare;
    internal bool HasDestination => Operation != FullRepOperation.Load;
    internal bool IsPair => Operation is FullRepOperation.Move or FullRepOperation.Compare;
    internal bool Compares => Operation is FullRepOperation.Compare or FullRepOperation.Scan;
    internal bool WritesMemory => Operation is FullRepOperation.Move or FullRepOperation.Store;
}

internal static class FullRepLowering
{
    // After authoritative long-mode XED decoding; bytes is exactly one decoded
    // instruction. Deliberately no runtime count, memory, flags or CPU input.
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out FullRepInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "MISSING native REP encoding";
        if (bytes.Length is < 2 or > 15)
        { reason = "invalid REP instruction length"; return false; }
        int p = 0, groups = 0, segment = -1;
        byte repeat = 0;
        bool operand16 = false, address32 = false;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            int group = b switch
            {
                0xf2 or 0xf3 => 1, 0x66 => 2, 0x67 => 4,
                0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 => 8,
                _ => 0
            };
            if (group == 0) break;
            if ((groups & group) != 0)
            { reason = "MISSING repeated REP legacy prefix group"; return false; }
            groups |= group;
            if (group == 1) repeat = b;
            else if (b == 0x66) operand16 = true;
            else if (b == 0x67) address32 = true;
            else
            {
                segment = b == 0x36 ? 2 : b == 0x64 ? 4 : b == 0x65 ? 5 : -1;
            }
            ++p;
        }
        byte rex = 0;
        if (p < bytes.Length && bytes[p] is >= 0x40 and <= 0x4f) rex = bytes[p++];
        if (p == bytes.Length)
        { reason = "truncated REP opcode"; return false; }
        byte opcode = bytes[p++];
        if (opcode == 0xf0)
        { reason = "invalid LOCK REP string prefix"; return false; }
        FullRepOperation operation;
        switch (opcode)
        {
            case 0xa4: case 0xa5: operation = FullRepOperation.Move; break;
            case 0xaa: case 0xab: operation = FullRepOperation.Store; break;
            case 0xac: case 0xad: operation = FullRepOperation.Load; break;
            case 0xa6: case 0xa7: operation = FullRepOperation.Compare; break;
            case 0xae: case 0xaf: operation = FullRepOperation.Scan; break;
            default: return false;
        }
        if (p != bytes.Length)
        { reason = "REP encoding has trailing or misplaced prefix bytes"; return false; }
        if (repeat == 0)
        { reason = "not a REP instruction; ordinary R94 path required"; return false; }
        bool compares = operation is FullRepOperation.Compare or FullRepOperation.Scan;
        // Intel REP table and AMD APM Vol3 1.2.6 document F3 for MOVS/STOS/LODS,
        // F3 REPE/REPZ and F2 REPNE/REPNZ for CMPS/SCAS. The selected
        // interpreter also repeats F2 noncomparison forms; that permissiveness
        // is not an architectural definition, so retain an explicit exclusion.
        if (repeat == 0xf2 && !compares)
        { reason = "MISSING undocumented F2 MOVS/STOS/LODS repeat alias"; return false; }
        bool source = operation is FullRepOperation.Move or FullRepOperation.Load or FullRepOperation.Compare;
        int width = (opcode & 1) == 0 ? 8 : (rex & 8) != 0 ? 64 : operand16 ? 16 : 32;
        instruction = new(ip, bytes.Length, operation, width, address32 ? 32 : 64,
            source ? segment : -1, !compares ? FullRepRepeat.Count :
                repeat == 0xf3 ? FullRepRepeat.WhileEqual : FullRepRepeat.WhileNotEqual);
        reason = null;
        return true;
    }

    internal static bool IsCanonical(in FullRepInstruction i)
    {
        if (i.Length is < 2 or > 15 || i.Width is not (8 or 16 or 32 or 64) ||
            i.AddressWidth is not (32 or 64) || i.SourceSegment is not (-1 or 2 or 4 or 5) ||
            (uint)i.Operation > (uint)FullRepOperation.Scan ||
            i.Repeat is not (FullRepRepeat.Count or FullRepRepeat.WhileEqual or FullRepRepeat.WhileNotEqual))
            return false;
        return (!i.HasSource ? i.SourceSegment == -1 : true) &&
            (i.Compares ? i.Repeat != FullRepRepeat.Count : i.Repeat == FullRepRepeat.Count);
    }

    internal static FullMemoryAccess DestinationAccess(in FullRepInstruction i)
    {
        if (!IsCanonical(in i) || !i.HasDestination)
            throw new ArgumentException("REP has no canonical destination mapping");
        return i.WritesMemory ? FullMemoryAccess.Write : FullMemoryAccess.Read;
    }
}
