namespace Managed.Emulation.Execution;

// Translation-time grammar only, after authoritative long-mode decode. The
// runtime mapping assist never decodes an instruction or computes its result.
internal static class FullStackMemoryLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeStackMemoryEncodingMissing";
        if (!FullMemoryLowering.Prefix(bytes, out int p, out byte op, out byte rex,
            out bool operand16, out bool address32, out int segment, ref reason)) return false;
        int width = (rex & 8) != 0 ? 64 : operand16 ? 16 : 64;
        JitOperation operation;
        JitOperand destination = default, source = default;
        JitAddress address;
        if (op == 0xc9)
        {
            operation = JitOperation.Leave;
            // Stack-address size is always64 in long mode, independently of
            // 67 or the explicit-operand segment prefix. 66 selects pop16.
            address = new(5, -1, 0, 0);
        }
        else if (op is 0xff or 0x8f)
        {
            if (p == bytes.Length) return false;
            byte modrm = bytes[p++]; int group = (modrm >> 3) & 7;
            if ((rex & 4) != 0 || (modrm & 0xc0) == 0xc0 ||
                op == 0xff && group is not (2 or 6) || op == 0x8f && group != 0 ||
                !FullMemoryLowering.Address(bytes, ref p, modrm, rex, address32, segment, out address)) return false;
            operation = op == 0x8f ? JitOperation.Pop : group == 2 ? JitOperation.IndirectCall : JitOperation.Push;
            if (operation == JitOperation.IndirectCall) width = 64; // no near CALL16 in long mode
            if (operation == JitOperation.Pop) destination = new(0, 0, IsMemory: true);
            else source = new(0, 0, IsMemory: true);
        }
        else return false;
        if (p != bytes.Length) { reason = "NativeStackMemoryExactLengthMissing"; return false; }
        instruction = new(ip, bytes.Length, operation, width, destination, source,
            SourceWidth: width, Address: address);
        reason = null; return true;
    }
}
