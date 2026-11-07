namespace Managed.Emulation.Execution;

// Exact, authoritative long-mode decoder spans only. Prefix processing follows
// effective groups: redundant legacy prefixes are permitted, and a legacy
// prefix after REX cancels that REX. LOCK/REP are not silently reinterpreted.
internal static class FullScalarMiscLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeScalarMiscEncodingMissing";
        if (bytes.Length is < 1 or > 15) return false;
        int p = 0, segment = -1; bool operand16 = false, address32 = false;
        byte rex = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is 0xf0 or 0xf2 or 0xf3)
            { reason = "NativeScalarMiscLockRepMissing"; return false; }
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) operand16 = true;
            else if (b == 0x67) address32 = true;
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else break;
            rex = 0; ++p;
        }
        if (p == bytes.Length) return false;
        byte opcode = bytes[p++];
        var memory = new JitOperand(0, 0, IsMemory: true);
        if (opcode == 0xd7)
        {
            if (p != bytes.Length) return false;
            instruction = new(ip, bytes.Length, JitOperation.TranslateByte, 8, new(0, 0), memory,
                SourceWidth: 8, Address: new(3, -1, 0, 0, AddressWidth: address32 ? 32 : 64, Segment: segment));
        }
        else if (opcode == 0x8c && p < bytes.Length)
        {
            byte modrm = bytes[p++]; int selector = modrm >> 3 & 7;
            if (selector >= 6) return false; // REX.R does not extend a segment selector.
            JitOperand destination; JitAddress address = default;
            int width = (rex & 8) != 0 ? 64 : operand16 ? 16 : 32;
            if ((modrm & 0xc0) == 0xc0) destination = new((modrm & 7) | ((rex & 1) << 3), 0);
            else
            {
                if (!FullMemoryLowering.Address(bytes, ref p, modrm, rex, address32, segment, out address)) return false;
                destination = memory; width = 16; // memory is ALWAYS exactly 16 bits
            }
            if (p != bytes.Length) return false;
            instruction = new(ip, bytes.Length, JitOperation.ReadSegmentSelector, width, destination, default,
                SourceWidth: 16, Address: address, Options: (uint)selector);
        }
        else if (opcode == 0x0f && p + 1 < bytes.Length && bytes[p++] == 0xc3)
        {
            byte modrm = bytes[p++];
            if ((modrm & 0xc0) == 0xc0) return false;
            if (!FullMemoryLowering.Address(bytes, ref p, modrm, rex, address32, segment, out var address) ||
                p != bytes.Length) return false;
            int width = (rex & 8) != 0 ? 64 : 32; // MOVNTI has no 16-bit form
            // A non-temporal hint permits the stronger ordinary-store ordering
            // used by our checked scatter path. The source is a GPR, not XMM.
            instruction = new(ip, bytes.Length, JitOperation.NonTemporalStore, width, memory,
                new((modrm >> 3 & 7) | ((rex & 4) << 1), 0), SourceWidth: width, Address: address);
        }
        else return false;
        reason = null; return true;
    }

    internal static bool UsesStackSegment(JitAddress a) => a.Segment == 2 ||
        a.Segment == -1 && a.Base >= 0 && (a.Base & 7) is 4 or 5;
}
