using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// Compile-time subopcodes, never a runtime semantic dispatch ABI.
internal enum FullAtomicOperation : uint
{
    Exchange = 1, ExchangeAdd, CompareExchange, CompareExchangePair,
    Add, Or, Adc, Sbb, And, Sub, Xor, Inc, Dec, Neg, Not,
    BitSet, BitReset, BitComplement
}

internal static class FullAtomicLowering
{
    internal const uint OperationMask = 255;
    internal const uint Locked = 256;

    // Caller supplies exactly one authoritative XED instruction, in flat long
    // mode. This bounded grammar does no runtime decoding or Op* selection.
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default; reason = "NativeAtomicEncodingMissing";
        if (bytes.Length is < 2 or > 15) return false;
        int p = 0, segment = -1;
        bool locked = false, word = false, address32 = false;
        byte rex = 0;
        // Same finite prefix normalization as selected XED: repeated legacy
        // prefixes are idempotent, last segment/REX wins, and any intervening
        // legacy prefix cancels an earlier REX. REP/HLE hints have no semantic
        // effect for these operations. Authoritative decoder supplies length.
        while (p < bytes.Length)
        {
            byte prefix = bytes[p];
            if (prefix is >= 0x40 and <= 0x4f) { rex = prefix; ++p; continue; }
            if (prefix == 0xf0) locked = true;
            else if (prefix == 0x66) word = true;
            else if (prefix == 0x67) address32 = true;
            else if (prefix is 0xf2 or 0xf3) { }
            else if (prefix is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment = prefix switch { 0x26 => 0, 0x2e => 1, 0x36 => 2, 0x3e => 3, 0x64 => 4, _ => 5 };
            else break;
            rex = 0; ++p;
        }
        if (p == bytes.Length) return false;
        byte opcode = bytes[p++];
        bool extended = opcode == 0x0f;
        if (extended) { if (p == bytes.Length) return false; opcode = bytes[p++]; }
        if (p == bytes.Length) return false;
        byte modrm = bytes[p++];
        if ((modrm & 0xc0) == 0xc0) return false;
        if (!Address(bytes, ref p, modrm, rex, address32, segment, out var address)) return false;
        int width = (rex & 8) != 0 ? 64 : word ? 16 : 32;
        int group = (modrm >> 3) & 7;
        var source = default(JitOperand);
        FullAtomicOperation operation;
        bool register = false, immediate = false;
        if (!extended && opcode is 0x86 or 0x87)
        {
            operation = FullAtomicOperation.Exchange; register = true;
            if (opcode == 0x86) width = 8;
        }
        else if (extended && opcode is 0xc0 or 0xc1 or 0xb0 or 0xb1)
        {
            operation = opcode < 0xc0 ? FullAtomicOperation.CompareExchange : FullAtomicOperation.ExchangeAdd;
            register = true; if ((opcode & 1) == 0) width = 8;
        }
        else if (extended && opcode == 0xc7)
        {
            if (group != 1) return false; // Group field is not extended by REX.R.
            operation = FullAtomicOperation.CompareExchangePair;
            width = (rex & 8) != 0 ? 128 : 64;
        }
        else if (extended && opcode is 0xab or 0xb3 or 0xbb or 0xba)
        {
            if (!locked) return false;
            if (opcode == 0xba)
            {
                if (group is < 5 or > 7) return false;
                operation = group == 5 ? FullAtomicOperation.BitSet : group == 6 ? FullAtomicOperation.BitReset : FullAtomicOperation.BitComplement;
                if (!Immediate(bytes, ref p, 1, false, out source)) return false;
            }
            else
            {
                operation = opcode == 0xab ? FullAtomicOperation.BitSet : opcode == 0xb3 ? FullAtomicOperation.BitReset : FullAtomicOperation.BitComplement;
                register = true;
            }
        }
        else if (!extended && locked && (opcode is 0x80 or 0x81 or 0x82 or 0x83 || opcode <= 0x31 && (opcode & 7) <= 1))
        {
            immediate = opcode is 0x80 or 0x81 or 0x82 or 0x83;
            int alu = immediate ? group : opcode >> 3;
            if (alu > 6) return false; // LOCK CMP is not an RMW instruction.
            operation = alu switch
            {
                0 => FullAtomicOperation.Add, 1 => FullAtomicOperation.Or, 2 => FullAtomicOperation.Adc,
                3 => FullAtomicOperation.Sbb, 4 => FullAtomicOperation.And, 5 => FullAtomicOperation.Sub,
                _ => FullAtomicOperation.Xor
            };
            if (opcode is 0x80 or 0x82 || !immediate && (opcode & 1) == 0) width = 8;
            register = !immediate;
        }
        else if (!extended && locked && opcode is 0xf6 or 0xf7 or 0xfe or 0xff)
        {
            if (opcode is 0xfe or 0xff)
            {
                if (group > 1) return false;
                operation = group == 0 ? FullAtomicOperation.Inc : FullAtomicOperation.Dec;
            }
            else
            {
                if (group is not (2 or 3)) return false;
                operation = group == 2 ? FullAtomicOperation.Not : FullAtomicOperation.Neg;
            }
            if (opcode is 0xf6 or 0xfe) width = 8;
        }
        else return false;
        if (register)
        {
            int r = group | ((rex & 4) << 1);
            source = width == 8 && rex == 0 && r is >= 4 and <= 7 ? new(r - 4, 0, ByteOffset: 1) : new(r, 0);
        }
        if (immediate && !Immediate(bytes, ref p, opcode == 0x83 ? 1 : width == 64 ? 4 : width / 8,
            opcode == 0x83 || width == 64, out source)) return false;
        if (p != bytes.Length) { reason = "NativeAtomicEncodingLength"; return false; }
        instruction = new(ip, bytes.Length, JitOperation.AtomicMemory, width, new(0, 0, IsMemory: true), source,
            SourceWidth: width, Address: address, Options: (uint)operation | (locked ? Locked : 0));
        reason = null; return true;
    }

    private static bool Address(ReadOnlySpan<byte> bytes, ref int p, byte modrm, byte rex,
        bool address32, int segment, out JitAddress address)
    {
        address = default;
        int mod = modrm >> 6, rm = modrm & 7, b = -1, index = -1, scale = 0;
        int count = mod == 1 ? 1 : mod == 2 ? 4 : 0;
        bool rip = false;
        if (rm == 4)
        {
            if (p == bytes.Length) return false;
            byte sib = bytes[p++]; scale = sib >> 6;
            int sb = sib & 7, si = (sib >> 3) & 7;
            if (mod == 0 && sb == 5) count = 4; else b = sb | ((rex & 1) << 3);
            if (si != 4 || (rex & 2) != 0) index = si | ((rex & 2) << 2);
        }
        else if (mod == 0 && rm == 5) { count = 4; rip = true; }
        else b = rm | ((rex & 1) << 3);
        long displacement = 0;
        if (count != 0)
        {
            if (!Immediate(bytes, ref p, count, true, out var value)) return false;
            displacement = unchecked((long)value.Immediate);
        }
        address = new(b, index, scale, displacement, rip, address32 ? 32 : 64, segment);
        return true;
    }

    private static bool Immediate(ReadOnlySpan<byte> bytes, ref int p, int count, bool sign, out JitOperand value)
    {
        value = default;
        if (count > bytes.Length - p) return false;
        var data = bytes.Slice(p, count);
        ulong v = count switch
        {
            1 => sign ? unchecked((ulong)(sbyte)data[0]) : data[0],
            2 => sign ? unchecked((ulong)BinaryPrimitives.ReadInt16LittleEndian(data)) : BinaryPrimitives.ReadUInt16LittleEndian(data),
            4 => sign ? unchecked((ulong)BinaryPrimitives.ReadInt32LittleEndian(data)) : BinaryPrimitives.ReadUInt32LittleEndian(data),
            _ => throw new ArgumentOutOfRangeException(nameof(count))
        };
        p += count; value = new(0, v, true); return true;
    }
}
