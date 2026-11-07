namespace Managed.Emulation.Execution;

// Translation-time only, after authoritative XED length/mode admission. Uses
// the ordinary memory grammar; no guest decode or semantic dispatch at runtime.
internal static class FullMemoryIntegerLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "MISSING native memory integer encoding";
        if (!FullMemoryLowering.Prefix(bytes, out int p, out byte op, out byte rex,
            out bool operand16, out bool address32, out int segment, ref reason)) return false;
        int w = (rex & 8) != 0 ? 64 : operand16 ? 16 : 32;
        byte extension = 0;
        if (op == 0x0f)
        {
            if (p == bytes.Length) return false;
            extension = bytes[p++];
            if (extension is not (0xa4 or 0xa5 or 0xac or 0xad or 0xaf or 0xbc or 0xbd or 0xa3 or 0xab or 0xb3 or 0xbb or 0xba)) return false;
        }
        else if (op is not (0xc0 or 0xc1 or 0xd0 or 0xd1 or 0xd2 or 0xd3 or 0xf6 or 0xf7 or 0x69 or 0x6b)) return false;
        if (p == bytes.Length) return false;
        byte modrm = bytes[p++]; int group = (modrm >> 3) & 7;
        if ((modrm & 0xc0) == 0xc0 ||
            !FullMemoryLowering.Address(bytes, ref p, modrm, rex, address32, segment, out var address)) return false;
        if (op is 0xc0 or 0xd0 or 0xd2 or 0xf6) w = 8;
        JitOperand memory = new(0, 0, IsMemory: true), dst = default, src = default, extra = default;
        JitOperation operation;
        uint options = 0;
        if (op is 0xc0 or 0xc1 or 0xd0 or 0xd1 or 0xd2 or 0xd3)
        {
            if ((rex & 4) != 0) return false;
            operation = group switch
            {
                0 => JitOperation.RotateLeft, 1 => JitOperation.RotateRight,
                2 => JitOperation.RotateCarryLeft, 3 => JitOperation.RotateCarryRight,
                4 or 6 => JitOperation.ShiftLeft, 5 => JitOperation.ShiftRight,
                _ => JitOperation.ShiftArithmeticRight
            };
            dst = memory;
            if (op is 0xc0 or 0xc1)
            {
                if (!FullMemoryLowering.Immediate(bytes, ref p, 1, false, out src)) return false;
            }
            else src = op is 0xd0 or 0xd1 ? new(0, 1, true) : new(1, 0);
        }
        else if (op is 0xf6 or 0xf7)
        {
            if ((rex & 4) != 0 || group < 4) return false;
            operation = group switch { 4 => JitOperation.MultiplyUnsigned, 5 => JitOperation.MultiplySigned,
                6 => JitOperation.DivideUnsigned, _ => JitOperation.DivideSigned };
            src = memory;
        }
        else if (op is 0x69 or 0x6b)
        {
            operation = JitOperation.MultiplySignedLow;
            dst = FullMemoryLowering.Register(group | ((rex & 4) << 1), w, rex != 0); src = memory;
            options = FullIntegerLowering.ThreeOperandMultiply;
            if (!FullMemoryLowering.Immediate(bytes, ref p, op == 0x6b ? 1 : w == 16 ? 2 : 4, true, out extra)) return false;
        }
        else
        {
            var register = FullMemoryLowering.Register(group | ((rex & 4) << 1), w, rex != 0);
            if (extension is 0xa4 or 0xa5 or 0xac or 0xad)
            {
                operation = extension is 0xac or 0xad ? JitOperation.ShiftDoubleRight : JitOperation.ShiftDoubleLeft;
                dst = memory; src = register;
                if ((extension & 1) == 0)
                {
                    if (!FullMemoryLowering.Immediate(bytes, ref p, 1, false, out extra)) return false;
                }
                else extra = new(1, 0);
            }
            else if (extension is 0xa3 or 0xab or 0xb3 or 0xbb or 0xba)
            {
                operation = extension switch { 0xab => JitOperation.BitSet, 0xb3 => JitOperation.BitReset,
                    0xbb => JitOperation.BitComplement, _ => JitOperation.BitTest }; dst = memory;
                if (extension == 0xba)
                {
                    if (group < 4 || (rex & 4) != 0 ||
                        !FullMemoryLowering.Immediate(bytes, ref p, 1, false, out src)) return false;
                    operation = group switch { 5 => JitOperation.BitSet, 6 => JitOperation.BitReset,
                        7 => JitOperation.BitComplement, _ => JitOperation.BitTest };
                }
                else src = register;
            }
            else
            {
                operation = extension == 0xaf ? JitOperation.MultiplySignedLow :
                    extension == 0xbc ? JitOperation.BitScanForward : JitOperation.BitScanReverse;
                dst = register; src = memory;
            }
        }
        if (p != bytes.Length) { reason = "MISSING native memory integer exact length"; return false; }
        instruction = new(ip, bytes.Length, operation, w, dst, src, SourceWidth: w,
            Address: address, Extra: extra, Options: options);
        reason = null; return true;
    }
}
