using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// Translation-time only. Input is exactly ONE instruction length supplied by
// the producer decoder. Prefix, displacement and immediate consumption must
// agree with that length; runtime never decodes guest bytes or selects an Op*.
internal static class FullMemoryLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeMemoryEncodingMissing";
        if (!Prefix(bytes, out int p, out byte op, out byte rex, out bool operand16,
            out bool address32, out int segment, ref reason)) return false;
        int width = (rex & 8) != 0 ? 64 : operand16 ? 16 : 32;
        int sourceWidth = 0;
        JitOperation operation;
        JitOperand dst = default, src = default;
        JitAddress address = new(-1, -1, 0, 0, AddressWidth: address32 ? 32 : 64, Segment: segment);
        ulong target = 0;
        if (op is >= 0x50 and <= 0x5f)
        {
            width = (rex & 8) != 0 ? 64 : operand16 ? 16 : 64;
            operation = op < 0x58 ? JitOperation.Push : JitOperation.Pop;
            if (operation == JitOperation.Push) src = new((op & 7) | ((rex & 1) << 3), 0);
            else dst = new((op & 7) | ((rex & 1) << 3), 0);
            address = new(4, -1, 0, operation == JitOperation.Push ? -width / 8 : 0);
        }
        else if (op is 0x68 or 0x6a)
        {
            width = (rex & 8) != 0 ? 64 : operand16 ? 16 : 64; operation = JitOperation.Push;
            if (!Immediate(bytes, ref p, op == 0x6a ? 1 : width == 16 ? 2 : 4, true, out src)) return false;
            address = new(4, -1, 0, -width / 8);
        }
        else if (op is 0xe8 or 0xc2 or 0xc3)
        {
            if (operand16) return false;
            width = 64;
            operation = op == 0xe8 ? JitOperation.Call : op == 0xc2 ? JitOperation.ReturnAdjust : JitOperation.Ret;
            if (op == 0xe8)
            {
                if (!Immediate(bytes, ref p, 4, true, out var displacement)) return false;
                target = unchecked(ip + (uint)bytes.Length + displacement.Immediate);
            }
            else if (op == 0xc2 && !Immediate(bytes, ref p, 2, false, out src)) return false;
            address = new(4, -1, 0, op == 0xe8 ? -8 : 0);
        }
        else if (op == 0xff && p < bytes.Length && (((bytes[p] >> 3) & 7) is 2 or 4 or 6))
        {
            byte modrm = bytes[p++]; int group = (modrm >> 3) & 7;
            if ((rex & 4) != 0 || group is not (2 or 4 or 6)) return false;
            width = group == 6 && operand16 && (rex & 8) == 0 ? 16 : 64;
            operation = group == 2 ? JitOperation.IndirectCall : group == 4 ? JitOperation.IndirectJump : JitOperation.Push;
            if ((modrm & 0xc0) == 0xc0) src = new((modrm & 7) | ((rex & 1) << 3), 0);
            else
            {
                if (group != 4) { reason = "NativeTwoAccessStackMissing"; return false; }
                if (!Address(bytes, ref p, modrm, rex, address32, segment, out address)) return false;
                src = new(0, 0, IsMemory: true);
            }
        }
        else if (op is >= 0xa0 and <= 0xa3)
        {
            if ((op & 1) == 0) width = 8;
            if (!Immediate(bytes, ref p, address32 ? 4 : 8, false, out var absolute)) return false;
            address = address with { Displacement = unchecked((long)absolute.Immediate) };
            operation = (op & 2) == 0 ? JitOperation.LoadMov : JitOperation.StoreMov;
            if ((op & 2) == 0) { dst = new(0, 0); src = new(0, 0, IsMemory: true); }
            else { dst = new(0, 0, IsMemory: true); src = new(0, 0); }
            sourceWidth = width;
        }
        else if ((op <= 0x3b && (op & 7) <= 3) || op is 0x80 or 0x81 or 0x83 or 0x84 or 0x85 or 0xf6 or 0xf7 or 0xfe or 0xff)
        {
            if (p == bytes.Length) return false;
            byte modrm = bytes[p++]; int group = (modrm >> 3) & 7;
            if ((modrm & 0xc0) == 0xc0) return false;
            if (!Address(bytes, ref p, modrm, rex, address32, segment, out address)) return false;
            if (op is 0x80 or 0x84 or 0xf6 or 0xfe || op < 0x40 && (op & 1) == 0) width = 8;
            bool immediate = op is 0x80 or 0x81 or 0x83;
            if (op is 0x84 or 0x85) operation = JitOperation.Test;
            else if (op is 0xf6 or 0xf7)
            {
                if ((rex & 4) != 0 || group is not (0 or 2 or 3)) return false;
                operation = group == 0 ? JitOperation.Test : group == 2 ? JitOperation.Not : JitOperation.Neg;
                immediate = group == 0;
            }
            else if (op is 0xfe or 0xff)
            {
                if ((rex & 4) != 0 || group > 1) return false;
                operation = group == 0 ? JitOperation.Inc : JitOperation.Dec;
            }
            else
            {
                int alu = immediate ? group : op >> 3;
                if (immediate && (rex & 4) != 0) return false;
                operation = alu switch
                {
                    0 => JitOperation.Add, 1 => JitOperation.Or, 2 => JitOperation.Adc, 3 => JitOperation.Sbb,
                    4 => JitOperation.And, 5 => JitOperation.Sub, 6 => JitOperation.Xor, 7 => JitOperation.Cmp,
                    _ => JitOperation.Nop
                };
                if (operation == JitOperation.Nop) return false;
            }
            dst = new(0, 0, IsMemory: true);
            if (immediate)
            {
                if (!Immediate(bytes, ref p, op == 0x83 ? 1 : width == 64 ? 4 : width / 8,
                    op == 0x83 || width == 64, out src)) return false;
            }
            else if (operation is not (JitOperation.Inc or JitOperation.Dec or JitOperation.Neg or JitOperation.Not))
            {
                src = Register(((modrm >> 3) & 7) | ((rex & 4) << 1), width, rex != 0);
                if (op < 0x40 && (op & 2) != 0) (dst, src) = (src, dst);
            }
            sourceWidth = width;
        }
        else if (op == 0x0f && p < bytes.Length && (bytes[p] is >= 0x40 and <= 0x4f or >= 0x90 and <= 0x9f))
        {
            byte extension = bytes[p++];
            if (p == bytes.Length) return false;
            byte modrm = bytes[p++];
            if ((modrm & 0xc0) == 0xc0 || !Address(bytes, ref p, modrm, rex, address32, segment, out address)) return false;
            if (extension >= 0x90)
            {
                if ((modrm & 0x38) != 0 || (rex & 4) != 0) return false;
                operation = JitOperation.SetCondition; width = 8; dst = new(0, 0, IsMemory: true);
            }
            else
            {
                operation = JitOperation.ConditionalMove;
                dst = new(((modrm >> 3) & 7) | ((rex & 4) << 1), 0); src = new(0, 0, IsMemory: true);
            }
            if (p != bytes.Length) { reason = "NativeMemoryEncodingLength"; return false; }
            instruction = new(ip, bytes.Length, operation, width, dst, src, Condition: extension & 15, SourceWidth: width, Address: address);
            reason = null; return true;
        }
        else
        {
            bool extend = false, signed = false;
            if (op == 0x0f)
            {
                if (p == bytes.Length || bytes[p] is not (0xb6 or 0xb7 or 0xbe or 0xbf)) return false;
                byte ext = bytes[p++]; extend = true; signed = (ext & 8) != 0;
                sourceWidth = (ext & 1) == 0 ? 8 : 16;
            }
            else if (op == 0x63)
            {
                if (width != 64) return false;
                extend = signed = true; sourceWidth = 32;
            }
            else if (op is not (0x88 or 0x89 or 0x8a or 0x8b or 0xc6 or 0xc7)) return false;
            if (p == bytes.Length) return false;
            byte modrm = bytes[p++];
            if ((modrm & 0xc0) == 0xc0) return false; // scalar decoder owns register forms
            if (!Address(bytes, ref p, modrm, rex, address32, segment, out address)) return false;
            if (extend)
            {
                if (sourceWidth >= width) return false;
                operation = signed ? JitOperation.Movsx : JitOperation.LoadMovzx;
                dst = new(((modrm >> 3) & 7) | ((rex & 4) << 1), 0); src = new(0, 0, IsMemory: true);
            }
            else
            {
                if ((op & 1) == 0) width = 8;
                sourceWidth = width;
                var register = Register(((modrm >> 3) & 7) | ((rex & 4) << 1), width, rex != 0);
                if (op is 0xc6 or 0xc7)
                {
                    if ((modrm & 0x38) != 0 || (rex & 4) != 0) return false;
                    operation = JitOperation.StoreMov; dst = new(0, 0, IsMemory: true);
                    if (!Immediate(bytes, ref p, width == 64 ? 4 : width / 8, width == 64, out src)) return false;
                }
                else if ((op & 2) != 0)
                { operation = JitOperation.LoadMov; dst = register; src = new(0, 0, IsMemory: true); }
                else { operation = JitOperation.StoreMov; dst = new(0, 0, IsMemory: true); src = register; }
            }
        }
        if (p != bytes.Length) { reason = "NativeMemoryEncodingLength"; return false; }
        instruction = new(ip, bytes.Length, operation, width, dst, src, target, SourceWidth: sourceWidth, Address: address);
        reason = null; return true;
    }

    // Shared bounded grammar for ordinary memory classifiers. One of each
    // legacy group precedes one final REX. LOCK/REP remain explicit misses.
    internal static bool Prefix(ReadOnlySpan<byte> bytes, out int p, out byte op, out byte rex,
        out bool operand16, out bool address32, out int segment, ref string? reason)
    {
        p = 0; op = rex = 0; operand16 = address32 = false; segment = -1;
        if (bytes.Length is < 1 or > 15) return false;
        while (p < bytes.Length)
        {
            byte prefix = bytes[p];
            if (prefix == 0x66) { if (operand16) return false; operand16 = true; ++p; }
            else if (prefix == 0x67) { if (address32) return false; address32 = true; ++p; }
            else if (prefix is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
            {
                if (segment != -1) return false;
                segment = prefix switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 }; ++p;
            }
            else break;
        }
        if (p == bytes.Length) return false;
        if (bytes[p] is >= 0x40 and <= 0x4f) { rex = bytes[p++]; if (p == bytes.Length) return false; }
        op = bytes[p++];
        if (op is 0xf0 or 0xf2 or 0xf3 || op is >= 0x40 and <= 0x4f)
        { reason = "NativeMemoryPrefixMissing"; return false; }
        return true;
    }

    internal static JitOperand Register(int register, int width, bool rex) => width == 8 && !rex && register is >= 4 and <= 7
        ? new(register - 4, 0, ByteOffset: 1) : new(register, 0);

    internal static bool Address(ReadOnlySpan<byte> bytes, ref int p, byte modrm, byte rex,
        bool address32, int segment, out JitAddress address)
    {
        address = default;
        int mod = modrm >> 6, rm = modrm & 7;
        if (mod == 3) return false;
        int b = -1, index = -1, scale = 0, displacementSize = mod == 1 ? 1 : mod == 2 ? 4 : 0;
        bool rip = false;
        if (rm == 4)
        {
            if (p == bytes.Length) return false;
            byte sib = bytes[p++]; scale = sib >> 6;
            int sb = sib & 7, si = (sib >> 3) & 7;
            if (mod == 0 && sb == 5) displacementSize = 4;
            else b = sb | ((rex & 1) << 3);
            if (si != 4 || (rex & 2) != 0) index = si | ((rex & 2) << 2);
        }
        else if (mod == 0 && rm == 5) { displacementSize = 4; rip = true; }
        else b = rm | ((rex & 1) << 3);
        long displacement = 0;
        if (displacementSize != 0)
        {
            if (!Immediate(bytes, ref p, displacementSize, true, out var value)) return false;
            displacement = unchecked((long)value.Immediate);
        }
        // 67+r/m=5 remains EIP-relative in 64-bit mode. Native EA performs
        // next-IP addition followed by modulo-32 truncation before FS/GS.
        address = new(b, index, scale, displacement, rip, address32 ? 32 : 64, segment);
        return true;
    }

    internal static bool Immediate(ReadOnlySpan<byte> bytes, ref int p, int count, bool sign, out JitOperand value)
    {
        value = default;
        if (count > bytes.Length - p) return false;
        var data = bytes.Slice(p, count);
        ulong v = count switch
        {
            1 => sign ? unchecked((ulong)(sbyte)data[0]) : data[0],
            2 => sign ? unchecked((ulong)BinaryPrimitives.ReadInt16LittleEndian(data)) : BinaryPrimitives.ReadUInt16LittleEndian(data),
            4 => sign ? unchecked((ulong)BinaryPrimitives.ReadInt32LittleEndian(data)) : BinaryPrimitives.ReadUInt32LittleEndian(data),
            8 => BinaryPrimitives.ReadUInt64LittleEndian(data), _ => throw new ArgumentOutOfRangeException(nameof(count))
        };
        p += count; value = new(0, v, true); return true;
    }
}
