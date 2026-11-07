using System;

namespace Managed.Emulation.Execution;

// Compile-time domains only. Pop is a separate, encoding-validated option.
// The historical double IR and R143/R148 namespaces are disjoint.
internal enum FullCanonicalX87MemoryOperation : ushort
{
    LoadBinary32 = 0x8001, LoadBinary64, LoadInt16, LoadInt32, LoadInt64,
    StoreBinary32, StoreBinary64, StoreInt16, StoreInt32, StoreInt64,
    TruncateInt16, TruncateInt32, TruncateInt64, LoadRaw80, StoreRaw80
}

internal static class FullCanonicalX87MemoryLowering
{
    internal const uint Pop = 1u << 16;
    // Existing128-byte descriptor, mapping-only fullspan WRITE permission proof.
    // Parent owns tagged request authentication/resolution. No physical writer
    // intent, writable TLB publication, ticket or host pointer is authorized.
    internal const uint MemoryProbeTag = 0x58384d50;

    // Input is EXACTLY the authoritative XED instruction span, never a stream.
    // 66/REX.W do not change x87 operand widths; effective final REX.B/X do
    // extend addressing. A legacy prefix after REX cancels that REX.
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "CanonicalX87MemoryEncodingMISSING";
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0, segment = -1; bool as32 = false; byte rex = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b == 0xf0) { reason = "CanonicalX87MemoryLockInvalid"; return false; }
            if (b is 0xf2 or 0xf3) { reason = "CanonicalX87MemoryRepPrefixPolicyMISSING"; return false; }
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x67) as32 = true;
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else if (b != 0x66) break;
            rex = 0; ++p;
        }
        if (p + 2 > bytes.Length) return false;
        byte opcode = bytes[p++], modrm = bytes[p++];
        if (opcode == 0x9b) { reason = "CanonicalX87MemoryWaitIsSeparateInstruction"; return false; }
        if (opcode is < 0xd8 or > 0xdf) return false;
        if (modrm >= 0xc0) { reason = "CanonicalX87MemoryRegisterOrUndefinedAliasMISSING"; return false; }
        if (!Decode(opcode, modrm, out var op, out int width, out bool pop))
        {
            reason = (opcode, modrm >> 3 & 7) switch
            {
                (0xdf, 4 or 6) => "CanonicalX87PackedBcdFollowingWorkstreamMISSING",
                (0xd9, 4 or 5 or 6 or 7) or (0xdd, 4 or 6 or 7) => "CanonicalX87EnvironmentFollowingWorkstreamMISSING",
                (0xd8 or 0xda or 0xdc or 0xde, _) => "CanonicalX87MemoryArithmeticFollowingWorkstreamMISSING",
                _ => "CanonicalX87MemoryUndefinedEncodingMISSING"
            };
            return false;
        }
        if (!FullMemoryLowering.Address(bytes, ref p, modrm, rex, as32, segment, out var address) || p != bytes.Length)
        { reason = "CanonicalX87MemoryExactAddressLengthMISSING"; return false; }
        var memory = new JitOperand(0, 0, IsMemory: true);
        bool load = IsLoad(op);
        instruction = new(ip, bytes.Length, JitOperation.X87, 80, load ? default : memory, load ? memory : default,
            SourceWidth: width, Address: address,
            Extra: new(0, (ulong)((opcode & 7) << 8 | modrm), IsImmediate: true),
            Options: (uint)op | (pop ? Pop : 0));
        reason = null; return true;
    }

    internal static bool IsLoad(FullCanonicalX87MemoryOperation op) =>
        op is >= FullCanonicalX87MemoryOperation.LoadBinary32 and <= FullCanonicalX87MemoryOperation.LoadInt64
        or FullCanonicalX87MemoryOperation.LoadRaw80;

    internal static bool Decode(byte opcode, byte modrm, out FullCanonicalX87MemoryOperation op,
        out int width, out bool pop)
    {
        op = 0; width = 0; pop = false;
        if (modrm >= 0xc0) return false;
        int group = modrm >> 3 & 7;
        (op, width, pop) = (opcode, group) switch
        {
            (0xd9, 0) => (FullCanonicalX87MemoryOperation.LoadBinary32, 32, false),
            (0xdd, 0) => (FullCanonicalX87MemoryOperation.LoadBinary64, 64, false),
            (0xdb, 5) => (FullCanonicalX87MemoryOperation.LoadRaw80, 80, false),
            (0xdf, 0) => (FullCanonicalX87MemoryOperation.LoadInt16, 16, false),
            (0xdb, 0) => (FullCanonicalX87MemoryOperation.LoadInt32, 32, false),
            (0xdf, 5) => (FullCanonicalX87MemoryOperation.LoadInt64, 64, false),
            (0xd9, 2 or 3) => (FullCanonicalX87MemoryOperation.StoreBinary32, 32, group == 3),
            (0xdd, 2 or 3) => (FullCanonicalX87MemoryOperation.StoreBinary64, 64, group == 3),
            (0xdb, 7) => (FullCanonicalX87MemoryOperation.StoreRaw80, 80, true),
            (0xdf, 2 or 3) => (FullCanonicalX87MemoryOperation.StoreInt16, 16, group == 3),
            (0xdb, 2 or 3) => (FullCanonicalX87MemoryOperation.StoreInt32, 32, group == 3),
            (0xdf, 7) => (FullCanonicalX87MemoryOperation.StoreInt64, 64, true),
            (0xdf, 1) => (FullCanonicalX87MemoryOperation.TruncateInt16, 16, true),
            (0xdb, 1) => (FullCanonicalX87MemoryOperation.TruncateInt32, 32, true),
            (0xdd, 1) => (FullCanonicalX87MemoryOperation.TruncateInt64, 64, true),
            _ => ((FullCanonicalX87MemoryOperation)0, 0, false)
        };
        return width != 0;
    }
}
