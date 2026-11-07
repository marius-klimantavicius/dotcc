namespace Managed.Emulation.Execution;

internal enum FullProcessorQueryOperation : uint { Cpuid = 0x4001, Lsl, Rdpid }

// Translation-time grammar of an EXACT authoritative long-mode decode span.
// No runtime decoder/table dispatcher and no host feature-dependent semantics.
internal static class FullProcessorQueryLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeProcessorQueryEncodingMissing";
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0, rep = 0, segment = -1; bool word = false, address32 = false;
        byte rex = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b == 0xf0) { reason = "NativeProcessorQueryLockReserved"; return false; }
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) word = true;
            else if (b == 0x67) address32 = true;
            else if (b is 0xf2 or 0xf3) rep = b;
            else if (b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = b switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else break;
            // An intervening legacy prefix cancels all fields of an earlier REX.
            rex = 0; ++p;
        }
        if (p + 2 > bytes.Length || bytes[p++] != 0x0f) return false;
        byte opcode = bytes[p++];
        if (opcode == 0xa2)
        {
            if (p != bytes.Length) return false;
            instruction = new(ip, bytes.Length, JitOperation.SystemState, 32, default, default,
                Options: (uint)FullProcessorQueryOperation.Cpuid);
        }
        else if (opcode == 0x03 && p < bytes.Length)
        {
            // 66 selects DEST16; REX.W overrides it. The selector is always16.
            byte modrm = bytes[p++]; var address = default(JitAddress);
            JitOperand source;
            if ((modrm & 0xc0) == 0xc0) source = new((modrm & 7) | ((rex & 1) << 3), 0);
            else
            {
                if (!FullMemoryLowering.Address(bytes, ref p, modrm, rex, address32, segment, out address)) return false;
                source = new(0, 0, IsMemory: true);
            }
            if (p != bytes.Length) return false;
            instruction = new(ip, bytes.Length, JitOperation.SystemState, (rex & 8) != 0 ? 64 : word ? 16 : 32,
                new((modrm >> 3 & 7) | ((rex & 4) << 1), 0), source,
                SourceWidth: 16, Address: address, Options: (uint)FullProcessorQueryOperation.Lsl);
        }
        else if (opcode == 0xc7 && rep == 0xf3 && p < bytes.Length)
        {
            byte modrm = bytes[p++];
            // REX.R/X do not extend the /7 opcode extension. Memory is reserved.
            if ((modrm & 0xf8) != 0xf8 || p != bytes.Length) return false;
            // Intel RDPID: CS.D, 66 and REX.W have no effect; long mode is r64.
            instruction = new(ip, bytes.Length, JitOperation.SystemState, 64,
                new((modrm & 7) | ((rex & 1) << 3), 0), default,
                Options: (uint)FullProcessorQueryOperation.Rdpid);
        }
        else return false;
        reason = null; return true;
    }
}
