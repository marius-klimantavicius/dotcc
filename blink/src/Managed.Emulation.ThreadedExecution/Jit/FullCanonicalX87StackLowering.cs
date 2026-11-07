using System;

namespace Managed.Emulation.Execution;

// Existing identities stay stable. Appended operations are NOT constants.
internal enum FullCanonicalX87StackOperation : ushort
{
    Fld = 0x6001, Fst, Fstp, Fxch, Ffree, Fchs, Fabs, Fincstp, Fdecstp,
    Fnop, Fwait, Fninit, Fnclex, FnstswAx,
    Fld1, Fldl2t, Fldl2e, Fldpi, Fldlg2, Fldln2, Fldz,
    Ffreep = 0x6016
}

internal static class FullCanonicalX87StackLowering
{
    // Explicit selected-source compatibility profile, never ordinary ISA proof.
    // This bit AND original FOP distinguish aliases from malformed ordinary IR.
    internal const uint SelectedAlias = 1u << 16;

    internal static bool IsConstant(FullCanonicalX87StackOperation op) =>
        op is >= FullCanonicalX87StackOperation.Fld1 and <= FullCanonicalX87StackOperation.Fldz;

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "CanonicalX87StackEncodingMissing";
        if (bytes.Length is < 1 or > 15) return false;
        int p = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b == 0xf0) { reason = "CanonicalX87LockInvalid"; return false; }
            if (b is 0xf2 or 0xf3) { reason = "CanonicalX87RepPrefixPolicyMISSING"; return false; }
            if (b is 0x66 or 0x67 or 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or >= 0x40 and <= 0x4f) ++p;
            else break;
        }
        if (p == bytes.Length) return false;
        byte opcode = bytes[p++];
        FullCanonicalX87StackOperation op;
        int index = 0; ulong fop = 0; bool alias = false;
        if (opcode == 0x9b)
        {
            if (p != bytes.Length) { reason = "CanonicalX87WaitIsSeparateInstruction"; return false; }
            op = FullCanonicalX87StackOperation.Fwait;
        }
        else
        {
            if (p + 1 != bytes.Length || opcode is < 0xd8 or > 0xdf) return false;
            byte m = bytes[p];
            if (m < 0xc0) { reason = "CanonicalX87MemoryFollowingWorkstreamMISSING"; return false; }
            fop = (ulong)((opcode & 7) << 8 | m);
            if (!DecodeRegister(opcode, m, out op, out index, out alias))
            {
                // Intel obsolete integer-NOP behavior differs from the actual
                // selected OpFpu DB/4 switch: rm0/1/4 reach OpUdImpl there.
                reason = opcode == 0xdb && m is 0xe0 or 0xe1 or 0xe4 ?
                    "CanonicalX87ObsoleteNoEffectButSelectedUdExcluded" :
                    "CanonicalX87ArithmeticConversionEnvironmentFollowingWorkstreamMISSING";
                return false;
            }
        }
        instruction = new(ip, bytes.Length, JitOperation.X87, 80, default, new(index, 0),
            Extra: new(0, fop, IsImmediate: true), Options: (uint)op | (alias ? SelectedAlias : 0));
        reason = null; return true;
    }

    // Shared exact byte-to-IR relation, used at translation and validation.
    // No runtime decoder, helper selection or host FP path is emitted.
    internal static bool DecodeRegister(byte opcode, byte m,
        out FullCanonicalX87StackOperation op, out int index, out bool alias)
    {
        op = 0; index = 0; alias = false;
        if (m < 0xc0 || opcode is < 0xd8 or > 0xdf) return false;
        if (opcode == 0xd9 && m is >= 0xc0 and <= 0xc7)
            op = FullCanonicalX87StackOperation.Fld;
        else if (opcode == 0xd9 && m is >= 0xc8 and <= 0xcf)
            op = FullCanonicalX87StackOperation.Fxch;
        else if (opcode == 0xdd && m is >= 0xc0 and <= 0xc7)
            op = FullCanonicalX87StackOperation.Ffree;
        else if (opcode == 0xdf && m is >= 0xc0 and <= 0xc7)
            op = FullCanonicalX87StackOperation.Ffreep;
        else if (opcode == 0xdd && m is >= 0xd0 and <= 0xd7)
            op = FullCanonicalX87StackOperation.Fst;
        else if (opcode == 0xdd && m is >= 0xd8 and <= 0xdf)
            op = FullCanonicalX87StackOperation.Fstp;
        else if ((opcode == 0xd9 && m is >= 0xd8 and <= 0xdf) ||
            (opcode == 0xdf && m is >= 0xd0 and <= 0xdf))
        { op = FullCanonicalX87StackOperation.Fstp; alias = true; }
        else if (opcode is 0xdd or 0xdf && m is >= 0xc8 and <= 0xcf)
        { op = FullCanonicalX87StackOperation.Fxch; alias = true; }
        else if (opcode == 0xd9 && m is >= 0xd1 and <= 0xd7)
        { op = FullCanonicalX87StackOperation.Fnop; alias = true; return true; }
        else if (opcode == 0xdf && m is >= 0xe1 and <= 0xe7)
        { op = FullCanonicalX87StackOperation.FnstswAx; alias = true; return true; }
        else if (opcode == 0xd9 && m is >= 0xe8 and <= 0xee)
        {
            op = (FullCanonicalX87StackOperation)((int)FullCanonicalX87StackOperation.Fld1 + m - 0xe8);
            return true;
        }
        else
        {
            op = (opcode, m) switch
            {
                (0xd9, 0xd0) => FullCanonicalX87StackOperation.Fnop,
                (0xd9, 0xe0) => FullCanonicalX87StackOperation.Fchs,
                (0xd9, 0xe1) => FullCanonicalX87StackOperation.Fabs,
                (0xd9, 0xf6) => FullCanonicalX87StackOperation.Fdecstp,
                (0xd9, 0xf7) => FullCanonicalX87StackOperation.Fincstp,
                (0xdb, 0xe2) => FullCanonicalX87StackOperation.Fnclex,
                (0xdb, 0xe3) => FullCanonicalX87StackOperation.Fninit,
                (0xdf, 0xe0) => FullCanonicalX87StackOperation.FnstswAx,
                _ => 0
            };
            return op != 0;
        }
        index = m & 7; return true;
    }
}
