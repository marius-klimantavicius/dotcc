using System.Buffers.Binary;
using E = Managed.Emulation.Execution.FullBlockEmitter.FullVectorExtendedOperation;

namespace Managed.Emulation.Execution;

// R111: compile-time legacy XMM integer grammar over authoritative XED bytes.
// MMX, VEX/EVEX and unselected opcode maps remain explicit MISSING domains.
internal static class FullVectorExtendedLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "NativeVectorExtendedEncodingMissing";
        if (bytes.Length is < 5 or > 15) return false;
        int p = 0, segment = -1;
        bool operand66 = false, address32 = false;
        byte rex = 0;
        // Effective prefix groups, without the older partial's ordering gate.
        // A legacy prefix after REX cancels that REX; the last contiguous REX
        // immediately before the opcode is effective. Ignored REX.W/X/B and
        // redundant 66/67/segment prefixes do not narrow the admitted profile.
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) operand66 = true;
            else if (b == 0x67) address32 = true;
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else if (b is 0xf0 or 0xf2 or 0xf3)
            { reason = "NativeVectorExtendedGroup1PrefixMissing"; return false; }
            else break;
            rex = 0; ++p;
        }
        if (!operand66) { reason = "NativeVectorExtendedMmxOrMandatoryPrefixMissing"; return false; }
        if (p + 4 > bytes.Length || bytes[p++] != 0x0f) return false;
        byte map = bytes[p++], opcode = bytes[p++];
        E op = (map, opcode) switch
        {
            (0x38, 0x00) => E.ShuffleByte,
            (0x38, 0x01) => E.HorizontalAddWord,
            (0x38, 0x02) => E.HorizontalAddDword,
            (0x38, 0x03) => E.HorizontalAddSignedWord,
            (0x38, 0x04) => E.MultiplyAddUnsignedSignedByte,
            (0x38, 0x05) => E.HorizontalSubWord,
            (0x38, 0x06) => E.HorizontalSubDword,
            (0x38, 0x07) => E.HorizontalSubSignedWord,
            (0x38, 0x08) => E.SignByte,
            (0x38, 0x09) => E.SignWord,
            (0x38, 0x0a) => E.SignDword,
            (0x38, 0x0b) => E.MultiplyRoundedSignedWord,
            (0x38, 0x1c) => E.AbsoluteByte,
            (0x38, 0x1d) => E.AbsoluteWord,
            (0x38, 0x1e) => E.AbsoluteDword,
            (0x38, 0x40) => E.MultiplyLowDword,
            (0x3a, 0x0f) => E.AlignRightBytes,
            (0x3a, 0x44) => E.CarrylessMultiplyQword,
            _ => 0
        };
        if (op == 0) return false;
        byte modrm = bytes[p++];
        bool memory = (modrm & 0xc0) != 0xc0;
        var destination = new JitOperand((modrm >> 3 & 7) | ((rex & 4) << 1), 0);
        var source = memory ? new JitOperand(0, 0, IsMemory: true) :
            new JitOperand((modrm & 7) | ((rex & 1) << 3), 0);
        JitAddress address = default;
        if (memory && !Address(bytes, ref p, modrm, rex, address32, segment, out address))
        { reason = "NativeVectorExtendedAddressMissing"; return false; }
        JitOperand extra = default;
        if (op is E.AlignRightBytes or E.CarrylessMultiplyQword)
        {
            if (p == bytes.Length) return false;
            extra = new(0, bytes[p++], true);
        }
        if (p != bytes.Length) { reason = "NativeVectorExtendedEncodingLength"; return false; }
        instruction = new(ip, bytes.Length, JitOperation.VectorInteger, 128, destination, source,
            SourceWidth: 128, Address: address, Extra: extra,
            Options: (uint)op | (memory ? FullBlockEmitter.VectorAlignedMemory : 0));
        reason = null;
        return true;
    }

    private static bool Address(ReadOnlySpan<byte> bytes, ref int p, byte modrm, byte rex,
        bool address32, int segment, out JitAddress address)
    {
        address = default;
        int mod = modrm >> 6, rm = modrm & 7;
        int b = -1, index = -1, scale = 0, size = mod == 1 ? 1 : mod == 2 ? 4 : 0;
        bool rip = false;
        if (rm == 4)
        {
            if (p == bytes.Length) return false;
            byte sib = bytes[p++]; scale = sib >> 6;
            int sb = sib & 7, si = sib >> 3 & 7;
            // SIB base5/mod0 is displacement-only even with ignored REX.B.
            if (mod == 0 && sb == 5) size = 4;
            else b = sb | ((rex & 1) << 3);
            if (si != 4 || (rex & 2) != 0) index = si | ((rex & 2) << 2);
        }
        else if (mod == 0 && rm == 5) { size = 4; rip = true; }
        else b = rm | ((rex & 1) << 3);
        if (size > bytes.Length - p) return false;
        long displacement = size == 1 ? (sbyte)bytes[p] : size == 4 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[p..]) : 0;
        p += size;
        // Address-size truncation is done by the common writer before FS/GS.
        address = new(b, index, scale, displacement, rip, address32 ? 32 : 64, segment);
        return true;
    }
}
