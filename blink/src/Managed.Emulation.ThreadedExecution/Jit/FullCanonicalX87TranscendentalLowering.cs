using System;

namespace Managed.Emulation.Execution;

// R157: one documented register transcendental family. These names are IR
// selectors at translation time, never a native semantic dispatch table.
internal enum FullCanonicalX87TranscendentalOperation : ushort
{
    F2xm1 = 0xa001, Fyl2x, Fptan, Fpatan, Fyl2xp1, Fsincos, Fsin, Fcos
}

internal static class FullCanonicalX87TranscendentalLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "CanonicalX87TranscendentalEncodingMISSING";
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b == 0xf0) { reason = "CanonicalX87TranscendentalLockInvalid"; return false; }
            if (b is 0xf2 or 0xf3) { reason = "CanonicalX87TranscendentalRepPrefixPolicyMISSING"; return false; }
            if (b is 0x66 or 0x67 or 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or >= 0x40 and <= 0x4f) ++p;
            else break;
        }
        if (p + 2 != bytes.Length || bytes[p] != 0xd9 || !TryOperation(bytes[p+1], out var op)) return false;
        instruction = new(ip, bytes.Length, JitOperation.X87, 80, default,
            new(IsBinary(op) ? 1 : 0, 0),
            Extra: new(0, (ulong)(0x100 | bytes[p+1]), IsImmediate: true), Options: (uint)op);
        reason = null; return true;
    }

    internal static bool TryOperation(byte modrm, out FullCanonicalX87TranscendentalOperation op)
    {
        op = modrm switch
        {
            0xf0 => FullCanonicalX87TranscendentalOperation.F2xm1,
            0xf1 => FullCanonicalX87TranscendentalOperation.Fyl2x,
            0xf2 => FullCanonicalX87TranscendentalOperation.Fptan,
            0xf3 => FullCanonicalX87TranscendentalOperation.Fpatan,
            0xf9 => FullCanonicalX87TranscendentalOperation.Fyl2xp1,
            0xfb => FullCanonicalX87TranscendentalOperation.Fsincos,
            0xfe => FullCanonicalX87TranscendentalOperation.Fsin,
            0xff => FullCanonicalX87TranscendentalOperation.Fcos,
            _ => 0
        };
        return op != 0;
    }
    internal static bool IsBinary(FullCanonicalX87TranscendentalOperation op) =>
        op is FullCanonicalX87TranscendentalOperation.Fyl2x or
        FullCanonicalX87TranscendentalOperation.Fyl2xp1 or FullCanonicalX87TranscendentalOperation.Fpatan;
    internal static bool Pushes(FullCanonicalX87TranscendentalOperation op) =>
        op is FullCanonicalX87TranscendentalOperation.Fptan or FullCanonicalX87TranscendentalOperation.Fsincos;
}
