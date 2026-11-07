using System.Buffers.Binary;
using F = Managed.Emulation.Execution.FullBlockEmitter.FullFloatOperation;

namespace Managed.Emulation.Execution;

// Compile-time bounded grammar AFTER authoritative XED decode, exact bytes,
// and the parent's flat/protected long-mode admission. No runtime decoding.
internal static class FullFloatLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "NativeFloatEncodingMissing";
        if (bytes.Length is < 3 or > 15) return false;
        int p = 0, segment = -1, mandatory = 0, rank = 0;
        bool address32 = false;
        byte rex = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            int next = b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 ? 1 :
                b == 0x67 ? 2 : b is 0x66 or 0xf2 or 0xf3 ? 3 : b is >= 0x40 and <= 0x4f ? 4 : 0;
            if (next == 0) break;
            if (next <= rank) { reason = "NativeFloatPrefixMissing"; return false; }
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
        { reason = "NativeFloatUnusedPrefixMissing"; return false; }
        var destination = new JitOperand((modrm >> 3 & 7) | ((rex & 4) << 1), 0);
        var source = memory ? new JitOperand(0, 0, IsMemory: true) :
            new JitOperand((modrm & 7) | ((rex & 1) << 3), 0);
        JitAddress address = default;
        if (memory && !Address(bytes, ref p, modrm, rex, address32, segment, out address))
        { reason = "NativeFloatAddressMissing"; return false; }
        F op;
        bool scalar = mandatory is 0xf2 or 0xf3, dbl = mandatory is 0x66 or 0xf2;
        bool rexw = false, imm8 = false;
        int sourceWidth = scalar ? (dbl ? 64 : 32) : 128;
        if (opcode is 0x58 or 0x5c or 0x59 or 0x5e or 0x5d or 0x5f or 0x51 or 0xc2)
        {
            op = opcode switch
            {
                0x58 => F.Add, 0x5c => F.Subtract, 0x59 => F.Multiply, 0x5e => F.Divide,
                0x5d => F.Minimum, 0x5f => F.Maximum, 0x51 => F.SquareRoot, _ => F.CompareMask
            };
            imm8 = opcode == 0xc2;
        }
        else if ((opcode is 0x2e or 0x2f) && mandatory is 0 or 0x66)
        {
            op = opcode == 0x2e ? F.CompareUnorderedFlags : F.CompareOrderedFlags;
            scalar = true; sourceWidth = dbl ? 64 : 32;
        }
        else if (opcode == 0x2a && scalar)
        { op = F.IntegerToScalar; sourceWidth = (rex & 8) != 0 ? 64 : 32; rexw = true; }
        else if ((opcode is 0x2c or 0x2d) && scalar)
        { op = opcode == 0x2c ? F.ScalarToIntegerTruncate : F.ScalarToInteger; rexw = true; }
        else if (opcode == 0x5a)
        {
            op = dbl ? F.DoubleToSingle : F.SingleToDouble;
            sourceWidth = dbl ? (scalar ? 64 : 128) : (scalar ? 32 : 64);
            dbl = false; // operation supplies the input and output lane types
        }
        else if (opcode == 0x5b && mandatory is 0 or 0x66 or 0xf3)
        {
            op = mandatory == 0 ? F.DwordToSingle : mandatory == 0x66 ? F.SingleToDword : F.SingleToDwordTruncate;
            scalar = dbl = false; sourceWidth = 128;
        }
        else if (opcode == 0xe6 && mandatory is 0x66 or 0xf2 or 0xf3)
        {
            op = mandatory == 0xf3 ? F.DwordToDouble : mandatory == 0xf2 ? F.DoubleToDword : F.DoubleToDwordTruncate;
            sourceWidth = mandatory == 0xf3 ? 64 : 128; scalar = dbl = false;
        }
        else
        {
            reason = opcode is 0x2a or 0x2c or 0x2d ? "NativeFloatMmxConversionMissing" :
                opcode is 0x52 or 0x53 ? "NativeFloatReciprocalApproximationMissing" :
                opcode is 0x7c or 0x7d or 0xd0 ? "NativeFloatSse3Missing" : "NativeFloatEncodingMissing";
            return false;
        }
        if ((rex & 8) != 0 && !rexw) { reason = "NativeFloatRexWMissing"; return false; }
        JitOperand extra = default;
        if (imm8)
        {
            if (p == bytes.Length) return false;
            // Legacy CMP predicates use imm8[2:0]; high bits are ignored.
            extra = new(0, bytes[p++], true);
        }
        if (p != bytes.Length) { reason = "NativeFloatEncodingLength"; return false; }
        instruction = new(ip, bytes.Length, JitOperation.VectorFloat, 128, destination, source,
            SourceWidth: sourceWidth, Address: address, Extra: extra,
            Options: (uint)op | (dbl ? FullBlockEmitter.FloatDouble : 0) |
                (scalar ? FullBlockEmitter.FloatScalar : 0) |
                (rexw && (rex & 8) != 0 ? FullBlockEmitter.FloatWideInteger : 0));
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
            if (mod == 0 && sb == 5 && (rex & 1) != 0) return false;
            if (mod == 0 && sb == 5) size = 4;
            else b = sb | ((rex & 1) << 3);
            if (si != 4 || (rex & 2) != 0) index = si | ((rex & 2) << 2);
        }
        else if (mod == 0 && rm == 5) { size = 4; rip = true; }
        else b = rm | ((rex & 1) << 3);
        if (size > bytes.Length - p) return false;
        long displacement = size == 1 ? (sbyte)bytes[p] : size == 4 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[p..]) : 0;
        p += size;
        address = new(b, index, scale, displacement, rip, address32 ? 32 : 64, segment);
        return true;
    }
}
