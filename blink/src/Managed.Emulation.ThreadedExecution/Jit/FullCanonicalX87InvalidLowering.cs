namespace Managed.Emulation.Execution;

// Only explicit reserved/selected-UD selectors become #UD. A missing ordinary
// lowering is an activation blocker, never evidence that the guest opcode is UD.
internal static class FullCanonicalX87InvalidLowering
{
    internal const uint ReservedUd = 0xd001, LockUd = 0xd002;

    internal static bool IsReservedSelector(byte opcode, byte m)
    {
        if (m < 0xc0)
            return (opcode, m >> 3 & 7) is (0xd9, 1) or (0xdb, 4 or 6) or (0xdd, 5);
        return opcode switch
        {
            0xd9 => m is 0xe2 or 0xe3 or 0xe6 or 0xe7 or 0xef,
            0xda => m is >= 0xe0 and <= 0xe8 or >= 0xea,
            0xdb => m is 0xe0 or 0xe1 or >= 0xe4 and <= 0xe7 or >= 0xf8,
            // Reserved comparison aliases are deliberately UD in this software
            // profile; legacy selected OpFpu computed some of these. They are
            // not the 54 explicitly adopted stack/status aliases.
            0xdc => m is >= 0xd0 and <= 0xdf,
            0xdd => m >= 0xf0,
            0xde => m is >= 0xd0 and <= 0xd8 or >= 0xda and <= 0xdf,
            0xdf => m >= 0xf8,
            _ => false
        };
    }

    internal static bool IsStateOperation(ulong rde)
    {
        int op = (int)(rde >> 40 & 2047);
        return op is >= 0xd8 and <= 0xdf or 0x9b ||
            op == 0x1ae && (rde >> 22 & 3) != 3 && (rde & 7) <= 1;
    }

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes, out JitInstruction instruction)
    {
        instruction = default;
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0; byte rex = 0; bool as32 = false, locked = false;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0xf0) locked = true;
            else if (b == 0x67) as32 = true;
            else if (b is not (0x66 or 0xf2 or 0xf3 or 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)) break;
            rex = 0; ++p;
        }
        if (p == bytes.Length) return false;
        byte opcode = bytes[p++];
        ulong fop = 0;
        if (opcode == 0x9b)
        {
            if (!locked || p != bytes.Length) return false;
        }
        else
        {
            if (opcode is < 0xd8 or > 0xdf || p == bytes.Length) return false;
            byte m = bytes[p++];
            if (!locked && !IsReservedSelector(opcode, m)) return false;
            if (m < 0xc0 && !FullMemoryLowering.Address(bytes, ref p, m, rex, as32, -1, out _)) return false;
            if (p != bytes.Length) return false;
            fop = (ulong)((opcode & 7) << 8 | m);
        }
        // No memory operand/EA capability: #UD precedes address evaluation,
        // pending #MF, numeric effects, or reading even a missing guest page.
        instruction = new(ip, bytes.Length, JitOperation.X87, 80, default, default,
            Extra: new(0, fop, IsImmediate: true), Options: locked ? LockUd : ReservedUd);
        return true;
    }
}
