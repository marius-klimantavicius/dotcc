using System.Buffers.Binary;
using V = Managed.Emulation.Execution.FullBlockEmitter.FullVectorOperation;

namespace Managed.Emulation.Execution;

// Bounded compile-time grammar. Caller supplies EXACT authoritative XED bytes
// in eligible flat long mode. No runtime decoder, managed opcode or helper.
// Rejected encodings remain visible as MISSING in the parent's full inventory.
internal static class FullVectorLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "NativeVectorEncodingMissing";
        if (bytes.Length is < 3 or > 15) return false;
        int p = 0, segment = -1, mandatory = 0, rank = 0;
        bool address32 = false;
        byte rex = 0;
        // One prefix per group, in this admitted order: segment,67,mandatory,
        // REX. Other legal orders are explicitly MISSING, never normalized.
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            int next = b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 ? 1 :
                b == 0x67 ? 2 : b is 0x66 or 0xf2 or 0xf3 ? 3 : b is >= 0x40 and <= 0x4f ? 4 : 0;
            if (next == 0) break;
            if (next <= rank) { reason = "NativeVectorPrefixMissing"; return false; }
            rank = next; ++p;
            if (next == 1) segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else if (next == 2) address32 = true;
            else if (next == 3) mandatory = b;
            else rex = b;
        }
        if (p + 3 > bytes.Length || bytes[p++] != 0x0f) return false;
        byte opcode = bytes[p++], modrm = bytes[p++];
        bool memory = (modrm & 0xc0) != 0xc0;
        if (rex == 0x40 || !memory && (segment != -1 || address32 || (rex & 2) != 0) ||
            memory && (rex & 2) != 0 && (modrm & 7) != 4 ||
            memory && (modrm & 0xc7) == 5 && (rex & 1) != 0)
        { reason = "NativeVectorUnusedPrefixMissing"; return false; }
        int reg = (modrm >> 3 & 7) | ((rex & 4) << 1);
        var r = new JitOperand(reg, 0);
        var rm = memory ? new JitOperand(0, 0, IsMemory: true) : new JitOperand((modrm & 7) | ((rex & 1) << 3), 0);
        JitAddress address = default;
        if (memory && !Address(bytes, ref p, modrm, rex, address32, segment, out address))
        { reason = "NativeVectorAddressMissing"; return false; }
        JitOperand dst = r, src = rm, extra = default;
        V op;
        int sourceWidth = 128;
        bool aligned = false, rexw = false, imm8 = false;

        if ((opcode is 0x6f or 0x7f) && mandatory is 0x66 or 0xf3)
        {
            op = V.Move; aligned = mandatory == 0x66;
            if (opcode == 0x7f) (dst, src) = (rm, r);
        }
        else if ((opcode is 0x10 or 0x11 or 0x28 or 0x29) && mandatory is 0 or 0x66)
        {
            op = V.Move; aligned = opcode is 0x28 or 0x29;
            if (opcode is 0x11 or 0x29) (dst, src) = (rm, r);
        }
        else if (opcode == 0x6e && mandatory == 0x66)
        { op = V.ScalarToVector; sourceWidth = (rex & 8) != 0 ? 64 : 32; rexw = true; }
        else if (opcode == 0x7e && mandatory == 0x66)
        { op = V.VectorToScalar; sourceWidth = (rex & 8) != 0 ? 64 : 32; rexw = true; (dst, src) = (rm, r); }
        else if (opcode == 0x7e && mandatory == 0xf3)
        { op = V.MoveLow64; sourceWidth = 64; }
        else if (opcode == 0xd6 && mandatory == 0x66)
        { op = V.MoveLow64; sourceWidth = 64; (dst, src) = (rm, r); }
        else if ((opcode is >= 0x54 and <= 0x57) && mandatory is 0 or 0x66)
            op = opcode switch { 0x54 => V.And, 0x55 => V.AndNot, 0x56 => V.Or, _ => V.Xor };
        else if (opcode == 0x70 && mandatory is 0x66 or 0xf2 or 0xf3)
        { op = mandatory == 0x66 ? V.ShuffleDword : mandatory == 0xf2 ? V.ShuffleLowWord : V.ShuffleHighWord; imm8 = true; }
        else if (opcode == 0xc6 && mandatory is 0 or 0x66)
        { op = mandatory == 0 ? V.ShuffleSingle : V.ShuffleDouble; imm8 = true; }
        else if (opcode == 0xd7 && mandatory == 0x66 && !memory)
            op = V.MaskByte;
        else if (opcode == 0x50 && mandatory is 0 or 0x66 && !memory)
            op = mandatory == 0 ? V.MaskSingle : V.MaskDouble;
        else if (opcode == 0xc4 && mandatory == 0x66)
        { op = V.InsertWord; sourceWidth = 16; imm8 = true; }
        else if (opcode == 0xc5 && mandatory == 0x66 && !memory)
        { op = V.ExtractWord; sourceWidth = 16; imm8 = true; }
        else if ((opcode is 0x71 or 0x72 or 0x73) && mandatory == 0x66 && !memory && (rex & 4) == 0)
        {
            int group = modrm >> 3 & 7;
            op = (opcode, group) switch
            {
                (0x71, 2) => V.ShiftRightWord, (0x71, 4) => V.ShiftArithmeticWord, (0x71, 6) => V.ShiftLeftWord,
                (0x72, 2) => V.ShiftRightDword, (0x72, 4) => V.ShiftArithmeticDword, (0x72, 6) => V.ShiftLeftDword,
                (0x73, 2) => V.ShiftRightQword, (0x73, 3) => V.ShiftRightBytes,
                (0x73, 6) => V.ShiftLeftQword, (0x73, 7) => V.ShiftLeftBytes, _ => 0
            };
            if (op == 0 || p == bytes.Length) return false;
            dst = rm; src = new(0, bytes[p++], true);
        }
        else if (mandatory == 0x66 && Binary(opcode, out op)) aligned = true;
        else return false;

        if ((rex & 8) != 0 && !rexw) { reason = "NativeVectorRexWMissing"; return false; }
        if (imm8)
        {
            if (p == bytes.Length) return false;
            extra = new(0, bytes[p++], true);
        }
        if (p != bytes.Length) { reason = "NativeVectorEncodingLength"; return false; }
        instruction = new(ip, bytes.Length, JitOperation.VectorInteger, 128, dst, src,
            SourceWidth: sourceWidth, Address: address, Extra: extra,
            Options: (uint)op | (aligned && memory ? FullBlockEmitter.VectorAlignedMemory : 0));
        reason = null;
        return true;
    }

    private static bool Binary(byte opcode, out V operation)
    {
        operation = opcode switch
        {
            0xdb => V.And, 0xdf => V.AndNot, 0xeb => V.Or, 0xef => V.Xor,
            0xfc => V.AddByte, 0xfd => V.AddWord, 0xfe => V.AddDword, 0xd4 => V.AddQword,
            0xf8 => V.SubByte, 0xf9 => V.SubWord, 0xfa => V.SubDword, 0xfb => V.SubQword,
            0xec => V.AddSignedByte, 0xed => V.AddSignedWord, 0xdc => V.AddUnsignedByte, 0xdd => V.AddUnsignedWord,
            0xe8 => V.SubSignedByte, 0xe9 => V.SubSignedWord, 0xd8 => V.SubUnsignedByte, 0xd9 => V.SubUnsignedWord,
            0x74 => V.EqualByte, 0x75 => V.EqualWord, 0x76 => V.EqualDword,
            0x64 => V.GreaterByte, 0x65 => V.GreaterWord, 0x66 => V.GreaterDword,
            0xda => V.MinUnsignedByte, 0xde => V.MaxUnsignedByte, 0xea => V.MinSignedWord, 0xee => V.MaxSignedWord,
            0x63 => V.PackSignedWord, 0x6b => V.PackSignedDword, 0x67 => V.PackUnsignedWord,
            0x60 => V.UnpackLowByte, 0x61 => V.UnpackLowWord, 0x62 => V.UnpackLowDword, 0x6c => V.UnpackLowQword,
            0x68 => V.UnpackHighByte, 0x69 => V.UnpackHighWord, 0x6a => V.UnpackHighDword, 0x6d => V.UnpackHighQword,
            0xd5 => V.MultiplyLowWord, 0xe5 => V.MultiplyHighSignedWord, 0xe4 => V.MultiplyHighUnsignedWord,
            0xf4 => V.MultiplyUnsignedDword, 0xf5 => V.MultiplyAddWord, 0xf6 => V.SumAbsoluteByte,
            0xe0 => V.AverageByte, 0xe3 => V.AverageWord,
            0xd1 => V.ShiftRightWord, 0xd2 => V.ShiftRightDword, 0xd3 => V.ShiftRightQword,
            0xe1 => V.ShiftArithmeticWord, 0xe2 => V.ShiftArithmeticDword,
            0xf1 => V.ShiftLeftWord, 0xf2 => V.ShiftLeftDword, 0xf3 => V.ShiftLeftQword,
            _ => 0
        };
        return operation != 0;
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
            if (mod == 0 && sb == 5 && (rex & 1) != 0) return false; // unused REX.B
            if (mod == 0 && sb == 5) size = 4;
            else b = sb | ((rex & 1) << 3);
            if (si != 4 || (rex & 2) != 0) index = si | ((rex & 2) << 2);
        }
        else if (mod == 0 && rm == 5) { size = 4; rip = true; }
        else b = rm | ((rex & 1) << 3);
        if (size > bytes.Length - p) return false;
        long displacement = size == 1 ? (sbyte)bytes[p] : size == 4 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[p..]) : 0;
        p += size;
        // 67+r/m5 is EIP-relative: next PC addition, modulo32, then FS/GS.
        address = new(b, index, scale, displacement, rip, address32 ? 32 : 64, segment);
        return true;
    }
}
