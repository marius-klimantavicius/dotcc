using System.Buffers.Binary;
using M = Managed.Emulation.Execution.FullBlockEmitter.FullMmxOperation;

namespace Managed.Emulation.Execution;

// R113: compile-time legacy MMX grammar over an EXACT authoritative XED span.
// MM0..7 use the selected producer's low Machine.xmm storage, with architectural
// register masks. This does not repair the inherited x87 alias/tag model.
internal static class FullMmxLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "NativeMmxEncodingMissing";
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0, segment = -1, rep = 0;
        bool operand66 = false, address32 = false;
        byte rex = 0;
        // Legacy groups may repeat or interleave. A subsequent legacy prefix
        // cancels preceding REX; the final contiguous REX is effective. Fields
        // ignored by the instruction do not unnecessarily restrict admission.
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) operand66 = true;
            else if (b == 0x67) address32 = true;
            else if (b is 0xf2 or 0xf3) rep = b;
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else if (b == 0xf0) { reason = "NativeMmxLockPrefixUnknown"; return false; }
            else break;
            rex = 0; ++p;
        }
        if (p + 2 > bytes.Length || bytes[p++] != 0x0f) return false;
        byte opcode = bytes[p++];
        if (opcode == 0x77)
        {
            if (rep != 0) { reason = "NativeMmxEmmsRepUnknown"; return false; }
            if (p != bytes.Length) { reason = "NativeMmxEncodingLength"; return false; }
            instruction = new(ip, bytes.Length, JitOperation.VectorInteger, 64,
                default, default, Options: (uint)M.Emms);
            reason = null; return true;
        }
        if (operand66) { reason = "NativeMmxXmmOrConflictingMandatoryPrefix"; return false; }
        int map = 0;
        if (opcode is 0x38 or 0x3a)
        {
            map = opcode;
            if (p == bytes.Length) return false;
            opcode = bytes[p++];
        }
        if (p == bytes.Length) return false;
        byte modrm = bytes[p++];
        bool memory = (modrm & 0xc0) != 0xc0;
        int rawReg = modrm >> 3 & 7, rawRm = modrm & 7;
        var mmReg = new JitOperand(rawReg, 0);
        var mmRm = memory ? new JitOperand(0, 0, IsMemory: true) : new JitOperand(rawRm, 0);
        var gprReg = new JitOperand(rawReg | ((rex & 4) << 1), 0);
        var gprRm = memory ? mmRm : new JitOperand(rawRm | ((rex & 1) << 3), 0);
        JitOperand destination = mmReg, source = mmRm, extra = default;
        M op = 0;
        int width = 64;
        bool imm8 = false;
        if (map == 0 && opcode == 0xd6 && rep is 0xf2 or 0xf3 && !memory)
        {
            if (rep == 0xf3)
            { op = M.MmxToXmm; destination = gprReg; }
            else { op = M.XmmToMmx; source = gprRm; }
        }
        else if (rep != 0)
        { reason = "NativeMmxRepPrefixUnknown"; return false; }
        else if (map == 0 && opcode is 0x6e or 0x7e)
        {
            width = (rex & 8) != 0 ? 64 : 32;
            if (opcode == 0x6e) { op = M.ScalarToMmx; source = gprRm; }
            else { op = M.MmxToScalar; destination = gprRm; source = mmReg; }
        }
        else if (map == 0 && opcode is 0x6f or 0x7f)
        {
            op = M.Move;
            if (opcode == 0x7f) (destination, source) = (mmRm, mmReg);
        }
        else if (map == 0 && opcode == 0xe7 && memory)
        { op = M.NonTemporalStore; destination = mmRm; source = mmReg; }
        else if (map == 0 && opcode == 0xd7 && !memory)
        { op = M.MaskByte; destination = gprReg; }
        else if (map == 0 && opcode == 0xc4)
        { op = M.InsertWord; width = 16; source = gprRm; imm8 = true; }
        else if (map == 0 && opcode == 0xc5 && !memory)
        { op = M.ExtractWord; width = 16; destination = gprReg; imm8 = true; }
        else if (map == 0 && opcode == 0x70)
        { op = M.ShuffleWord; imm8 = true; }
        else if (map == 0 && opcode == 0xf7 && !memory)
        {
            // A mask may select no memory, or only one side of a page split.
            // The current full-width checked request cannot express that set.
            reason = "NativeMmxMaskedStoreSelectedPageRequestMissing"; return false;
        }
        else if (map == 0 && opcode is 0x71 or 0x72 or 0x73 && !memory)
        {
            op = (opcode, rawReg) switch
            {
                (0x71, 2) => M.ShiftRightWord, (0x71, 4) => M.ShiftArithmeticWord, (0x71, 6) => M.ShiftLeftWord,
                (0x72, 2) => M.ShiftRightDword, (0x72, 4) => M.ShiftArithmeticDword, (0x72, 6) => M.ShiftLeftDword,
                (0x73, 2) => M.ShiftRightQword, (0x73, 6) => M.ShiftLeftQword, _ => 0
            };
            if (op == 0)
            {
                if (opcode == 0x73 && rawReg is 3 or 7) reason = "NativeMmxSelectedOnlyFormUnknown";
                return false;
            }
            if (p == bytes.Length) return false;
            destination = mmRm; source = new(0, bytes[p++], true);
        }
        else if (map == 0) op = Binary(opcode);
        else if (map == 0x38) op = opcode switch
        {
            0x00 => M.ShuffleByte, 0x01 => M.HorizontalAddWord, 0x02 => M.HorizontalAddDword,
            0x03 => M.HorizontalAddSignedWord, 0x04 => M.MultiplyAddUnsignedSignedByte,
            0x05 => M.HorizontalSubWord, 0x06 => M.HorizontalSubDword, 0x07 => M.HorizontalSubSignedWord,
            0x08 => M.SignByte, 0x09 => M.SignWord, 0x0a => M.SignDword, 0x0b => M.MultiplyRoundedSignedWord,
            0x1c => M.AbsoluteByte, 0x1d => M.AbsoluteWord, 0x1e => M.AbsoluteDword, _ => 0
        };
        else if (map == 0x3a && opcode == 0x0f) { op = M.AlignRightBytes; imm8 = true; }
        // No invented NP PMULLD/PCLMULQDQ/PUNPCK*QDQ or byte-shift forms:
        // selected source branches exist, but no architectural MMX encoding.
        if (op == 0)
        {
            if (map == 0 && opcode is 0x6c or 0x6d || map == 0x38 && opcode == 0x40 || map == 0x3a && opcode == 0x44)
                reason = "NativeMmxSelectedOnlyFormUnknown";
            return false;
        }
        JitAddress address = default;
        if (memory && !Address(bytes, ref p, modrm, rex, address32, segment, out address))
        { reason = "NativeMmxAddressMissing"; return false; }
        if (imm8)
        {
            if (p == bytes.Length) return false;
            extra = new(0, bytes[p++], true);
        }
        if (p != bytes.Length) { reason = "NativeMmxEncodingLength"; return false; }
        instruction = new(ip, bytes.Length, JitOperation.VectorInteger, 64, destination, source,
            SourceWidth: width, Address: address, Extra: extra, Options: (uint)op);
        reason = null; return true;
    }

    private static M Binary(byte opcode) => opcode switch
    {
        0xdb => M.And, 0xdf => M.AndNot, 0xeb => M.Or, 0xef => M.Xor,
        0xfc => M.AddByte, 0xfd => M.AddWord, 0xfe => M.AddDword, 0xd4 => M.AddQword,
        0xf8 => M.SubByte, 0xf9 => M.SubWord, 0xfa => M.SubDword, 0xfb => M.SubQword,
        0xec => M.AddSignedByte, 0xed => M.AddSignedWord, 0xdc => M.AddUnsignedByte, 0xdd => M.AddUnsignedWord,
        0xe8 => M.SubSignedByte, 0xe9 => M.SubSignedWord, 0xd8 => M.SubUnsignedByte, 0xd9 => M.SubUnsignedWord,
        0x74 => M.EqualByte, 0x75 => M.EqualWord, 0x76 => M.EqualDword,
        0x64 => M.GreaterByte, 0x65 => M.GreaterWord, 0x66 => M.GreaterDword,
        0xda => M.MinUnsignedByte, 0xde => M.MaxUnsignedByte, 0xea => M.MinSignedWord, 0xee => M.MaxSignedWord,
        0x63 => M.PackSignedWord, 0x6b => M.PackSignedDword, 0x67 => M.PackUnsignedWord,
        0x60 => M.UnpackLowByte, 0x61 => M.UnpackLowWord, 0x62 => M.UnpackLowDword,
        0x68 => M.UnpackHighByte, 0x69 => M.UnpackHighWord, 0x6a => M.UnpackHighDword,
        0xd5 => M.MultiplyLowWord, 0xe5 => M.MultiplyHighSignedWord, 0xe4 => M.MultiplyHighUnsignedWord,
        0xf4 => M.MultiplyUnsignedDword, 0xf5 => M.MultiplyAddWord, 0xf6 => M.SumAbsoluteByte,
        0xe0 => M.AverageByte, 0xe3 => M.AverageWord,
        0xd1 => M.ShiftRightWord, 0xd2 => M.ShiftRightDword, 0xd3 => M.ShiftRightQword,
        0xe1 => M.ShiftArithmeticWord, 0xe2 => M.ShiftArithmeticDword,
        0xf1 => M.ShiftLeftWord, 0xf2 => M.ShiftLeftDword, 0xf3 => M.ShiftLeftQword, _ => 0
    };

    internal static bool UsesStackSegment(JitAddress a) => FullScalarMiscLowering.UsesStackSegment(a);

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
            if (mod == 0 && sb == 5) size = 4; // REX.B ignored for no-base SIB
            else b = sb | ((rex & 1) << 3);
            if (si != 4 || (rex & 2) != 0) index = si | ((rex & 2) << 2);
        }
        else if (mod == 0 && rm == 5) { size = 4; rip = true; }
        else b = rm | ((rex & 1) << 3);
        if (size > bytes.Length - p) return false;
        long displacement = size == 1 ? (sbyte)bytes[p] : size == 4 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[p..]) : 0;
        p += size;
        // The common writer adds nextPC then truncates EA32, before FS/GS.
        address = new(b, index, scale, displacement, rip, address32 ? 32 : 64, segment);
        return true;
    }
}
