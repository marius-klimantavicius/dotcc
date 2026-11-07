using System;

namespace Managed.Emulation.Execution;

// R155 complete ordinary memory computation, separate from R150 conversions.
internal enum FullCanonicalX87MemoryArithmeticOperation : ushort
{
    Add = 0xb001, Multiply, Subtract, SubtractReverse, Divide, DivideReverse,
    Fcom, Fcomp, Fbld, Fbstp
}

internal static class FullCanonicalX87MemoryArithmeticLowering
{
    // Input is EXACTLY the authoritative XED instruction span, never a stream.
    // 66/REX.W do not change x87 operand widths; effective final REX.B/X do
    // extend addressing. A legacy prefix after REX cancels that REX.
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "CanonicalX87MemoryComputationEncodingMISSING";
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0, segment = -1; bool as32 = false; byte rex = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b == 0xf0) { reason = "CanonicalX87MemoryComputationLockInvalid"; return false; }
            if (b is 0xf2 or 0xf3) { reason = "CanonicalX87MemoryComputationRepPrefixPolicyMISSING"; return false; }
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x67) as32 = true;
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else if (b != 0x66) break;
            rex = 0; ++p;
        }
        if (p + 2 > bytes.Length) return false;
        byte opcode = bytes[p++], modrm = bytes[p++];
        if (opcode == 0x9b) { reason = "CanonicalX87MemoryComputationWaitIsSeparateInstruction"; return false; }
        if (opcode is < 0xd8 or > 0xdf) return false;
        if (modrm >= 0xc0) { reason = "CanonicalX87MemoryComputationRegisterOrUndefinedAliasMISSING"; return false; }
        if (!Decode(opcode, modrm, out var op, out int width))
        {
            reason = (opcode, modrm >> 3 & 7) switch
            {
                (0xd9, 4 or 5 or 6 or 7) or (0xdd, 4 or 6 or 7) => "CanonicalX87MemoryEnvironmentOwnedByR154MISSING",
                _ => "CanonicalX87MemoryComputationUndefinedEncodingMISSING"
            };
            return false;
        }
        if (!FullMemoryLowering.Address(bytes, ref p, modrm, rex, as32, segment, out var address) || p != bytes.Length)
        { reason = "CanonicalX87MemoryComputationExactAddressLengthMISSING"; return false; }
        var memory = new JitOperand(0, 0, IsMemory: true);
        bool load = op != FullCanonicalX87MemoryArithmeticOperation.Fbstp;
        instruction = new(ip, bytes.Length, JitOperation.X87, 80, load ? default : memory, load ? memory : default,
            SourceWidth: width, Address: address,
            Extra: new(0, (ulong)((opcode & 7) << 8 | modrm), IsImmediate: true),
            Options: (uint)op);
        reason = null; return true;
    }

    // One parent catalog batch; no runtime selector is stored in Work.
    internal static bool TryEmitKernel(byte key, Span<byte> destination, out int length)
    {
        if (key is >= 32 and <= 55)
            return FullExtended80MemoryArithmeticEmitter.TryEmitKernel(key, destination, out length);
        if (key is >= 56 and <= 63)
            return FullExtended80MemoryCompareEmitter.TryEmitContinuation(
                (FullExtended80MemoryArithmeticFormat)((key - 56) / 2), (key & 1) != 0, destination, out length);
        if (key is 64 or 65)
            return FullExtended80BcdEmitter.TryEmitContinuation(key == 64 ?
                FullExtended80BcdOperation.LoadPackedBcd : FullExtended80BcdOperation.StorePackedBcd, destination, out length);
        length = 0; return false;
    }

    internal static bool IsArithmetic(FullCanonicalX87MemoryArithmeticOperation op) =>
        op is >= FullCanonicalX87MemoryArithmeticOperation.Add and <= FullCanonicalX87MemoryArithmeticOperation.DivideReverse;
    internal static bool IsCompare(FullCanonicalX87MemoryArithmeticOperation op) =>
        op is FullCanonicalX87MemoryArithmeticOperation.Fcom or FullCanonicalX87MemoryArithmeticOperation.Fcomp;
    internal static FullExtended80MemoryArithmeticFormat Format(byte opcode) => opcode switch
    {
        0xd8 => FullExtended80MemoryArithmeticFormat.Binary32,
        0xdc => FullExtended80MemoryArithmeticFormat.Binary64,
        0xda => FullExtended80MemoryArithmeticFormat.Int32,
        0xde => FullExtended80MemoryArithmeticFormat.Int16,
        _ => throw new ArgumentOutOfRangeException(nameof(opcode))
    };
    internal static bool Decode(byte opcode, byte modrm, out FullCanonicalX87MemoryArithmeticOperation op, out int width)
    {
        op = 0; width = 0;
        if (modrm >= 0xc0) return false;
        int group = modrm >> 3 & 7;
        if (opcode == 0xdf && group is 4 or 6)
        {
            op = group == 4 ? FullCanonicalX87MemoryArithmeticOperation.Fbld : FullCanonicalX87MemoryArithmeticOperation.Fbstp;
            width = 80; return true;
        }
        if (opcode is not (0xd8 or 0xdc or 0xda or 0xde)) return false;
        width = opcode == 0xdc ? 64 : opcode == 0xde ? 16 : 32;
        op = group switch
        {
            0 => FullCanonicalX87MemoryArithmeticOperation.Add,
            1 => FullCanonicalX87MemoryArithmeticOperation.Multiply,
            2 => FullCanonicalX87MemoryArithmeticOperation.Fcom,
            3 => FullCanonicalX87MemoryArithmeticOperation.Fcomp,
            4 => FullCanonicalX87MemoryArithmeticOperation.Subtract,
            5 => FullCanonicalX87MemoryArithmeticOperation.SubtractReverse,
            6 => FullCanonicalX87MemoryArithmeticOperation.Divide,
            _ => FullCanonicalX87MemoryArithmeticOperation.DivideReverse
        };
        return true;
    }
}
