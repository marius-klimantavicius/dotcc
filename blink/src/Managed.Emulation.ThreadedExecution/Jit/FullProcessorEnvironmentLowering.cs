namespace Managed.Emulation.Execution;

internal enum FullProcessorEnvironmentOperation : uint
{
    Rdtsc = 0x7001, Rdtscp, Rdrand, Rdseed
}

// Exact spans supplied by the selected authoritative long-mode decoder. This
// is translation-time value IR, never a runtime operation or service selector.
internal static class FullProcessorEnvironmentLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeProcessorEnvironmentEncodingMissing";
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0, rep = 0; bool word = false; byte rex = 0;
        while (p < bytes.Length)
        {
            byte b = bytes[p];
            if (b is >= 0x40 and <= 0x4f) { rex = b; ++p; continue; }
            if (b == 0x66) word = true;
            else if (b is 0xf2 or 0xf3) rep = b;
            else if (b is not (0xf0 or 0x67 or 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)) break;
            // The selected decoder records LOCK; these four selected handlers
            // do not test it. This alias is selected compatibility, NOT ISA
            // legality: architectural LOCK here would #UD. No host LOCK op.
            rex = 0; ++p;
        }
        if (p + 2 > bytes.Length || bytes[p++] != 0x0f) return false;
        byte opcode = bytes[p++];
        FullProcessorEnvironmentOperation operation;
        int width = 64; JitOperand destination = default;
        if (opcode == 0x31) operation = FullProcessorEnvironmentOperation.Rdtsc;
        else if (opcode == 0x01 && p < bytes.Length && bytes[p++] == 0xf9)
            operation = FullProcessorEnvironmentOperation.Rdtscp;
        else if (opcode == 0xc7 && p < bytes.Length)
        {
            byte modrm = bytes[p++];
            if ((modrm & 0xc0) != 0xc0) return false;
            int group = modrm >> 3 & 7;
            if (group == 6) operation = FullProcessorEnvironmentOperation.Rdrand;
            else if (group == 7 && rep != 0xf3) operation = FullProcessorEnvironmentOperation.Rdseed;
            else return false; // effective F3 /7 remains existing RDPID ownership
            width = (rex & 8) != 0 ? 64 : word ? 16 : 32;
            destination = new((modrm & 7) | ((rex & 1) << 3), 0);
        }
        else return false;
        if (p != bytes.Length) return false;
        instruction = new(ip, bytes.Length, JitOperation.SystemState, width,
            destination, default, Options: (uint)operation);
        reason = null; return true;
    }
}
