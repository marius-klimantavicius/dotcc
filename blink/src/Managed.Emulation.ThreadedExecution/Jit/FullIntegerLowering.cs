using System.Buffers.Binary;

namespace Managed.Emulation.Execution;

// Translation-time register-form recognizer, chained AFTER the authoritative
// guest XED decode, on exactly xedd.length bytes in flat long mode. This is not
// a replacement runtime decoder. Prefixes/forms outside this finite grammar
// return false with a named MISSING reason, never approximate their semantics.
internal static class FullIntegerLowering
{
    internal const uint ThreeOperandMultiply = 1;

    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction = default;
        reason = "MISSING native integer encoding";
        if (bytes.Length is < 1 or > 15) { reason = "invalid integer instruction length"; return false; }
        var p = new Parser(bytes);
        bool operand16 = false;
        if (p.Peek == 0x66) { operand16 = true; p.Read(); }
        int rex = 0;
        if (p.Peek is >= 0x40 and <= 0x4f) rex = p.Read();
        int opcode = p.Read(), width = (rex & 8) != 0 ? 64 : operand16 ? 16 : 32;
        // Reject repeated/reordered legacy/REX, REP/LOCK, segments and address
        // overrides. Parent may select other compile-time classifiers for them.
        if (opcode is 0x66 or 0x67 or 0xf0 or 0xf2 or 0xf3 or 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 ||
            opcode is >= 0x40 and <= 0x4f)
        { reason = "MISSING native integer prefix combination"; return false; }
        JitOperation op;
        JitOperand destination = default, source = default, extra = default;
        int sourceWidth = 0, condition = 0;
        uint options = 0;
        if (opcode is 0xf8 or 0xf9 or 0xf5 or 0xfc or 0xfd or 0x98 or 0x99)
        {
            op = opcode switch { 0xf8 => JitOperation.ClearCarry, 0xf9 => JitOperation.SetCarry,
                0xf5 => JitOperation.ComplementCarry, 0xfc => JitOperation.DirectionClear,
                0xfd => JitOperation.DirectionSet, 0x98 => JitOperation.SignExtendAccumulator,
                _ => JitOperation.SignExtendDividend };
        }
        else if (opcode is >= 0x90 and <= 0x97)
        {
            int r = (opcode & 7) | ((rex & 1) << 3);
            op = r == 0 ? JitOperation.Nop : JitOperation.Exchange;
            destination = new(0, 0); source = new(r, 0);
        }
        else if (opcode is >= 0xb0 and <= 0xbf)
        {
            op = JitOperation.Mov;
            if (opcode < 0xb8) width = 8;
            destination = Register((opcode & 7) | ((rex & 1) << 3), width, rex != 0);
            source = new(0, p.Immediate(width / 8), true);
        }
        else if (opcode < 0x40 && (opcode & 7) is 4 or 5)
        {
            op = Alu(opcode >> 3);
            if ((opcode & 1) == 0) width = 8;
            destination = new(0, 0);
            source = new(0, p.Immediate(width == 64 ? 4 : width / 8, width == 64), true);
        }
        else if (opcode == 0x0f)
        {
            int second = p.Read();
            if (second is >= 0xc8 and <= 0xcf)
            {
                if (operand16) { reason = "MISSING native BSWAP16 undefined form"; return false; }
                op = JitOperation.ByteSwap;
                destination = new((second & 7) | ((rex & 1) << 3), 0);
            }
            else
            {
                int actualWidth = second is >= 0x90 and <= 0x9f or 0xb0 or 0xc0 ? 8 : width;
                if (!p.Modrm(actualWidth, rex, out var rm, out var reg, out int group))
                { reason = "MISSING native integer memory form"; return false; }
                if (second is >= 0x40 and <= 0x4f)
                { op = JitOperation.ConditionalMove; destination = reg; source = rm; condition = second & 15; }
                else if (second is >= 0x90 and <= 0x9f)
                {
                    if (group != 0) { reason = "MISSING reserved SETcc ModRM form"; return false; }
                    op = JitOperation.SetCondition; width = 8; destination = rm; condition = second & 15;
                }
                else if (second is 0xb6 or 0xb7 or 0xbe or 0xbf)
                {
                    sourceWidth = (second & 1) == 0 ? 8 : 16;
                    if (width <= sourceWidth) { reason = "MISSING non-widening integer extension"; return false; }
                    // Byte high-register interpretation applies only to the
                    // source, while the ModRM.reg destination is full width.
                    source = Register(p.LastRm, sourceWidth, rex != 0);
                    destination = new(p.LastReg, 0);
                    op = second is 0xbe or 0xbf ? JitOperation.Movsx : JitOperation.Movzx;
                }
                else if (second is 0xa4 or 0xa5 or 0xac or 0xad)
                {
                    op = second is 0xac or 0xad ? JitOperation.ShiftDoubleRight : JitOperation.ShiftDoubleLeft;
                    destination = rm; source = reg;
                    extra = (second & 1) == 0 ? new(0, p.Immediate(1), true) : new(1, 0);
                }
                else if (second == 0xaf)
                { op = JitOperation.MultiplySignedLow; destination = reg; source = rm; }
                else if (second is 0xbc or 0xbd)
                { op = second == 0xbc ? JitOperation.BitScanForward : JitOperation.BitScanReverse; destination = reg; source = rm; }
                else if (second is 0xa3 or 0xab or 0xb3 or 0xbb or 0xba)
                {
                    int kind = second == 0xba ? group : (second >> 3) & 7;
                    if (kind < 4) { reason = "MISSING reserved bit operation"; return false; }
                    op = kind switch { 4 => JitOperation.BitTest, 5 => JitOperation.BitSet,
                        6 => JitOperation.BitReset, _ => JitOperation.BitComplement };
                    destination = rm; source = second == 0xba ? new(0, p.Immediate(1), true) : reg;
                }
                else if (second is 0xb0 or 0xb1 or 0xc0 or 0xc1)
                {
                    width = actualWidth; destination = rm; source = reg;
                    op = second is 0xb0 or 0xb1 ? JitOperation.CompareExchange : JitOperation.ExchangeAdd;
                }
                else return false;
            }
        }
        else
        {
            int actualWidth = opcode is 0x80 or 0x84 or 0x86 or 0x88 or 0x8a or 0xc0 or 0xc6 or 0xd0 or 0xd2 or 0xf6 or 0xfe ||
                opcode < 0x40 && (opcode & 1) == 0 ? 8 : width;
            if (!p.Modrm(actualWidth, rex, out var rm, out var reg, out int group))
            { reason = "MISSING native integer memory form"; return false; }
            width = actualWidth;
            if (opcode < 0x40 && (opcode & 7) <= 3)
            {
                op = Alu(opcode >> 3);
                destination = (opcode & 2) == 0 ? rm : reg;
                source = (opcode & 2) == 0 ? reg : rm;
            }
            else if (opcode is 0x80 or 0x81 or 0x83)
            {
                op = Alu(group); destination = rm;
                source = new(0, p.Immediate(opcode is 0x80 or 0x83 ? 1 : width == 16 ? 2 : 4,
                    opcode == 0x83 || width == 64), true);
            }
            else if (opcode is 0x84 or 0x85 or 0x86 or 0x87 or 0x88 or 0x89 or 0x8a or 0x8b)
            {
                op = opcode <= 0x85 ? JitOperation.Test : opcode <= 0x87 ? JitOperation.Exchange : JitOperation.Mov;
                bool reverse = opcode is 0x8a or 0x8b;
                destination = reverse ? reg : rm; source = reverse ? rm : reg;
            }
            else if (opcode is 0xc6 or 0xc7)
            {
                if (group != 0) return false;
                op = JitOperation.Mov; destination = rm;
                source = new(0, p.Immediate(width == 64 ? 4 : width / 8, width == 64), true);
            }
            else if (opcode is 0xf6 or 0xf7)
            {
                op = group switch { 0 => JitOperation.Test, 2 => JitOperation.Not, 3 => JitOperation.Neg,
                    4 => JitOperation.MultiplyUnsigned, 5 => JitOperation.MultiplySigned,
                    6 => JitOperation.DivideUnsigned, _ => JitOperation.DivideSigned };
                if (group == 1) return false;
                if (group >= 4) source = rm;
                else
                {
                    destination = rm;
                    if (group == 0) source = new(0, p.Immediate(width == 64 ? 4 : width / 8, width == 64), true);
                }
            }
            else if (opcode is 0xfe or 0xff)
            {
                if (group > 1) return false;
                op = group == 0 ? JitOperation.Inc : JitOperation.Dec; destination = rm;
            }
            else if (opcode is 0xc0 or 0xc1 or 0xd0 or 0xd1 or 0xd2 or 0xd3)
            {
                op = group switch { 0 => JitOperation.RotateLeft, 1 => JitOperation.RotateRight,
                    2 => JitOperation.RotateCarryLeft, 3 => JitOperation.RotateCarryRight,
                    4 or 6 => JitOperation.ShiftLeft, 5 => JitOperation.ShiftRight, _ => JitOperation.ShiftArithmeticRight };
                destination = rm;
                source = opcode is 0xc0 or 0xc1 ? new(0, p.Immediate(1), true) :
                    opcode is 0xd0 or 0xd1 ? new(0, 1, true) : new(1, 0);
            }
            else if (opcode is 0x69 or 0x6b)
            {
                op = JitOperation.MultiplySignedLow; destination = reg; source = rm;
                extra = new(0, p.Immediate(opcode == 0x6b ? 1 : width == 16 ? 2 : 4, true), true);
                options = ThreeOperandMultiply;
            }
            else if (opcode == 0x63 && width is 32 or 64)
            {
                op = width == 64 ? JitOperation.Movsx : JitOperation.Mov;
                destination = reg; source = rm; sourceWidth = width == 64 ? 32 : 0;
            }
            else return false;
        }
        if (!p.Complete) { reason = "integer encoding truncated or has trailing bytes"; return false; }
        var candidate = new JitInstruction(ip, bytes.Length, op, width, destination, source,
            Condition: condition, SourceWidth: sourceWidth, Extra: extra, Options: options);
        if (!FullBlockEmitter.CanInline(candidate)) { reason = "MISSING native integer operand shape"; return false; }
        instruction = candidate; reason = null; return true;
    }

    private static JitOperation Alu(int group) => group switch
    {
        0 => JitOperation.Add, 1 => JitOperation.Or, 2 => JitOperation.Adc, 3 => JitOperation.Sbb,
        4 => JitOperation.And, 5 => JitOperation.Sub, 6 => JitOperation.Xor, _ => JitOperation.Cmp
    };
    private static JitOperand Register(int encoded, int width, bool rex) =>
        width == 8 && !rex && encoded is >= 4 and <= 7 ? new(encoded - 4, 0, ByteOffset: 1) : new(encoded, 0);

    private ref struct Parser
    {
        private readonly ReadOnlySpan<byte> bytes;
        private int position;
        private bool failed;
        internal int LastRm, LastReg;
        internal Parser(ReadOnlySpan<byte> bytes)
        { this.bytes = bytes; position = 0; failed = false; LastRm = LastReg = 0; }
        internal int Peek => position < bytes.Length ? bytes[position] : -1;
        internal int Read()
        {
            if (position < bytes.Length) return bytes[position++];
            failed = true; return -1;
        }
        internal bool Complete => !failed && position == bytes.Length;
        internal ulong Immediate(int size, bool signed = false)
        {
            if (position > bytes.Length - size) { failed = true; return 0; }
            var data = bytes.Slice(position, size); position += size;
            return size switch
            {
                1 => signed ? unchecked((ulong)(sbyte)data[0]) : data[0],
                2 => signed ? unchecked((ulong)BinaryPrimitives.ReadInt16LittleEndian(data)) : BinaryPrimitives.ReadUInt16LittleEndian(data),
                4 => signed ? unchecked((ulong)BinaryPrimitives.ReadInt32LittleEndian(data)) : BinaryPrimitives.ReadUInt32LittleEndian(data),
                8 => BinaryPrimitives.ReadUInt64LittleEndian(data),
                _ => 0
            };
        }
        internal bool Modrm(int width, int rex, out JitOperand rm, out JitOperand reg, out int group)
        {
            int m = Read(); group = (m >> 3) & 7;
            LastRm = (m & 7) | ((rex & 1) << 3); LastReg = group | ((rex & 4) << 1);
            rm = Register(LastRm, width, rex != 0); reg = Register(LastReg, width, rex != 0);
            return m >= 0 && (m >> 6) == 3;
        }
    }
}
