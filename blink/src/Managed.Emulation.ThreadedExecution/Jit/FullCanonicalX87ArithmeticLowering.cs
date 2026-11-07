using System;

namespace Managed.Emulation.Execution;

// Compile-time operation namespace. Low16 identifies direction AND reversal;
// pop is bit16, legal only on the ST(i) destination family. Source.Register is
// always i; Destination is default. Neither Options nor FOP is read at runtime.
internal enum FullCanonicalX87ArithmeticOperation : ushort
{
    FaddSt0 = 0x7001, FmulSt0, FsubSt0, FsubrSt0, FdivSt0, FdivrSt0,
    FaddSti, FmulSti, FsubSti, FsubrSti, FdivSti, FdivrSti
}

internal static class FullCanonicalX87ArithmeticLowering
{
    internal const uint Pop = 1u << 16;

    // Source grammar: Intel XED D8/DC/DE MOD=3 groups 0,1,4,5,6,7.
    // No undocumented opcode aliases, memory decoding, old R116 IR or managed
    // arithmetic admission. Textual no-operand/popped aliases have the same DE
    // bytes (usually i=1) and therefore receive exactly the same typed lowering.
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "CanonicalX87ArithmeticEncodingMISSING";
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b == 0xf0) { reason = "CanonicalX87ArithmeticLockInvalid"; return false; }
            if (b is 0xf2 or 0xf3) { reason = "CanonicalX87ArithmeticRepPrefixPolicyMISSING"; return false; }
            if (b is 0x66 or 0x67 or 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or >= 0x40 and <= 0x4f) ++p;
            else break;
        }
        if (p + 2 != bytes.Length) return false;
        byte opcode = bytes[p], modrm = bytes[p + 1];
        if (opcode == 0x9b) { reason = "CanonicalX87WaitIsSeparateInstruction"; return false; }
        if (opcode is not (0xd8 or 0xdc or 0xde))
        { reason = "CanonicalX87ArithmeticCompareConversionFcmovFollowingWorkstreamMISSING"; return false; }
        if (modrm < 0xc0) { reason = "CanonicalX87ArithmeticMemoryFollowingWorkstreamMISSING"; return false; }
        int group = modrm >> 3 & 7, n = modrm & 7;
        if (!TryOperation(opcode, group, out var op))
        { reason = "CanonicalX87ArithmeticCompareFollowingWorkstreamMISSING"; return false; }
        instruction = new(ip, bytes.Length, JitOperation.X87, 80, default, new(n, 0),
            Extra: new(0, (ulong)((opcode & 7) << 8 | modrm), IsImmediate: true),
            Options: (uint)op | (opcode == 0xde ? Pop : 0));
        reason = null; return true;
    }

    internal static bool TryOperation(byte opcode, int group, out FullCanonicalX87ArithmeticOperation op)
    {
        // DC/DE reverse subtraction and division group order relative to D8.
        int k = group switch { 0 => 0, 1 => 1, 4 => 2, 5 => 3, 6 => 4, 7 => 5, _ => -1 };
        op = 0;
        if (k < 0 || opcode is not (0xd8 or 0xdc or 0xde)) return false;
        if (opcode != 0xd8 && k >= 2) k ^= 1;
        op = (FullCanonicalX87ArithmeticOperation)(0x7001 + k + (opcode == 0xd8 ? 0 : 6));
        return true;
    }
}
