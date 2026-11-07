using System.Buffers.Binary;
using M = Managed.Emulation.Execution.FullBlockEmitter.FullVectorMoveOperation;

namespace Managed.Emulation.Execution;

// Compile-time grammar, AFTER exact authoritative XED decoding and flat long
// mode admission. Legal prefix orders/repetitions outside this bounded grammar
// remain MISSING. No host SSE3/4 feature or semantic interpreter call is needed.
internal static class FullVectorMoveLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "NativeVectorMoveEncodingMissing";
        if (bytes.Length is < 3 or > 15) return false;
        int p = 0, segment = -1, mandatory = 0, rank = 0;
        bool address32 = false;
        byte rex = 0;
        // Same bounded segment/67/mandatory/REX grammar as R66. Deliberately
        // independent copy so R66's frozen admission is not broadened here.
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            int next = b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 ? 1 :
                b == 0x67 ? 2 : b is 0x66 or 0xf2 or 0xf3 ? 3 : b is >= 0x40 and <= 0x4f ? 4 : 0;
            if (next == 0) break;
            if (next <= rank) { reason = "NativeVectorMovePrefixMissing"; return false; }
            rank = next; ++p;
            if (next == 1) segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else if (next == 2) address32 = true;
            else if (next == 3) mandatory = b;
            else rex = b;
        }
        if (p + 3 > bytes.Length || bytes[p++] != 0x0f) return false;
        byte opcode = bytes[p++];
        bool map38 = opcode == 0x38;
        if (map38)
        {
            if (p + 2 > bytes.Length) return false;
            opcode = bytes[p++];
        }
        byte modrm = bytes[p++];
        bool memory = (modrm & 0xc0) != 0xc0;
        if (rex == 0x40 || !memory && (segment != -1 || address32 || (rex & 2) != 0) ||
            memory && (rex & 2) != 0 && (modrm & 7) != 4 ||
            memory && (modrm & 0xc7) == 5 && (rex & 1) != 0)
        { reason = "NativeVectorMoveUnusedPrefixMissing"; return false; }
        var r = new JitOperand((modrm >> 3 & 7) | ((rex & 4) << 1), 0);
        var rm = memory ? new JitOperand(0, 0, IsMemory: true) :
            new JitOperand((modrm & 7) | ((rex & 1) << 3), 0);
        JitAddress address = default;
        if (memory && !Address(bytes, ref p, modrm, rex, address32, segment, out address))
        { reason = "NativeVectorMoveAddressMissing"; return false; }
        JitOperand dst = r, src = rm;
        M op;
        int width = 128;
        bool aligned = false, rexw = false;
        if (map38)
        {
            // MOVNTDQA's cache hint may be implemented by an ordinary aligned
            // load. This emits SSE2, not the guest's optional SSE4.1 opcode.
            if (opcode != 0x2a || mandatory != 0x66 || !memory) return false;
            op = M.Move128; aligned = true;
        }
        else if ((opcode is 0x10 or 0x11) && mandatory is 0xf2 or 0xf3)
        {
            op = M.MoveScalar; width = mandatory == 0xf3 ? 32 : 64;
            if (opcode == 0x11) (dst, src) = (rm, r);
        }
        else if ((opcode is 0x10 or 0x11 or 0x28 or 0x29) && mandatory is 0 or 0x66)
        {
            op = M.Move128; aligned = opcode is 0x28 or 0x29;
            if (opcode is 0x11 or 0x29) (dst, src) = (rm, r);
        }
        else if ((opcode is 0x6f or 0x7f) && mandatory is 0x66 or 0xf3)
        {
            op = M.Move128; aligned = mandatory == 0x66;
            if (opcode == 0x7f) (dst, src) = (rm, r);
        }
        else if (opcode == 0x12 && mandatory is 0 or 0x66 && (memory || mandatory == 0))
        { op = memory ? M.LoadLow64 : M.MoveHighToLow; width = 64; }
        else if (opcode == 0x16 && mandatory is 0 or 0x66 && (memory || mandatory == 0))
        { op = memory ? M.LoadHigh64 : M.MoveLowToHigh; width = 64; }
        else if ((opcode is 0x13 or 0x17) && mandatory is 0 or 0x66 && memory)
        { op = opcode == 0x13 ? M.StoreLow64 : M.StoreHigh64; width = 64; (dst, src) = (rm, r); }
        else if ((opcode is 0x14 or 0x15) && mandatory is 0 or 0x66)
        {
            op = (opcode, mandatory) switch
            {
                (0x14, 0) => M.UnpackLowSingle, (0x15, 0) => M.UnpackHighSingle,
                (0x14, _) => M.UnpackLowDouble, _ => M.UnpackHighDouble
            };
            // Intel permits the low unpack to fetch only the appropriate64;
            // the legacy 16-byte alignment requirement still applies.
            width = opcode == 0x14 ? 64 : 128; aligned = true;
        }
        else if ((opcode is 0x12 or 0x16) && mandatory == 0xf3)
        {
            op = opcode == 0x12 ? M.DuplicateLowSingle : M.DuplicateHighSingle;
            // Legacy single-precision duplicate memory forms require16-byte
            // alignment. MOVDDUP below permits an unaligned64-bit operand.
            aligned = true;
        }
        else if (opcode == 0x12 && mandatory == 0xf2)
        { op = M.DuplicateDouble; width = 64; }
        else if (opcode == 0xf0 && mandatory == 0xf2 && memory) op = M.Move128; // LDDQU
        else if (opcode == 0x6e && mandatory == 0x66)
        { op = M.ScalarToVector; width = (rex & 8) != 0 ? 64 : 32; rexw = true; }
        else if (opcode == 0x7e && mandatory == 0x66)
        { op = M.VectorToScalar; width = (rex & 8) != 0 ? 64 : 32; rexw = true; (dst, src) = (rm, r); }
        else if (opcode == 0x7e && mandatory == 0xf3)
        { op = M.MoveLow64Zero; width = 64; }
        else if (opcode == 0xd6 && mandatory == 0x66)
        { op = M.MoveLow64Zero; width = 64; (dst, src) = (rm, r); }
        else if (memory && ((opcode == 0x2b && mandatory is 0 or 0x66) || opcode == 0xe7 && mandatory == 0x66))
        { op = M.NonTemporalStore128; aligned = true; (dst, src) = (rm, r); }
        else
        {
            reason = opcode == 0xf7 ? "NativeVectorMoveMaskedMissing" :
                opcode is 0x6e or 0x7e or 0x6f or 0x7f or 0xd6 or 0xe7 ? "NativeVectorMoveMmxMissing" :
                "NativeVectorMoveEncodingMissing";
            return false;
        }
        if ((rex & 8) != 0 && !rexw) { reason = "NativeVectorMoveRexWMissing"; return false; }
        if (p != bytes.Length) { reason = "NativeVectorMoveEncodingLength"; return false; }
        instruction = new(ip, bytes.Length, JitOperation.VectorInteger, 128, dst, src,
            SourceWidth: width, Address: address,
            Options: (uint)op | (aligned && memory ? FullBlockEmitter.VectorAlignedMemory : 0));
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
        // EA32 truncation follows next-PC arithmetic and precedes FS/GS.
        address = new(b, index, scale, displacement, rip, address32 ? 32 : 64, segment);
        return true;
    }
}
