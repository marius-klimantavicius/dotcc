namespace Managed.Emulation.Execution;

// Independently authored x64 lowering. All opcode selection occurs while emitting.
// Only RAX/RCX/RDX/R8/R9 are scratch; R10=frame, R11=Machine. No host stack,
// CALL, managed semantics, LAHF/SAHF, BMI, LZCNT, or other optional host ISA.
internal static partial class FullBlockEmitter
{
    private static bool IntegerRegister(JitOperand p, int width, bool immediate = false) =>
        !p.IsMemory && (p.IsImmediate ? immediate : p.Register is >= 0 and <= 15 &&
        p.ByteOffset is >= 0 and <= 1 && (p.ByteOffset == 0 || width == 8 && p.Register <= 3));

    private static bool CanInlineInteger(JitInstruction i)
    {
        int w = i.Width;
        if (w is not (8 or 16 or 32 or 64) || i.Destination.IsMemory || i.Source.IsMemory || i.Extra.IsMemory)
            return false;
        bool d = IntegerRegister(i.Destination, w), s = IntegerRegister(i.Source, w, true);
        if (i.Operation == JitOperation.MultiplySignedLow)
            return w != 8 && d && s && (i.Options == 0 || i.Options == 1 && i.Extra.IsImmediate);
        if (i.Options != 0) return false;
        return i.Operation switch
        {
            JitOperation.Add or JitOperation.Adc or JitOperation.Sub or JitOperation.Sbb or
            JitOperation.Cmp or JitOperation.And or JitOperation.Or or JitOperation.Xor or JitOperation.Test => d && s,
            JitOperation.Inc or JitOperation.Dec or JitOperation.Neg or JitOperation.Not => d,
            JitOperation.ShiftLeft or JitOperation.ShiftRight or JitOperation.ShiftArithmeticRight or
            JitOperation.RotateLeft or JitOperation.RotateRight or JitOperation.RotateCarryLeft or
            JitOperation.RotateCarryRight => d && IntegerRegister(i.Source, 8, true),
            JitOperation.ShiftDoubleLeft or JitOperation.ShiftDoubleRight => w != 8 && d &&
                IntegerRegister(i.Source, w) && IntegerRegister(i.Extra, 8, true),
            JitOperation.MultiplyUnsigned or JitOperation.MultiplySigned or
            JitOperation.DivideUnsigned or JitOperation.DivideSigned => IntegerRegister(i.Source, w),
            JitOperation.ByteSwap => w is 32 or 64 && d,
            JitOperation.BitScanForward or JitOperation.BitScanReverse => w != 8 && d && IntegerRegister(i.Source, w),
            JitOperation.BitTest or JitOperation.BitSet or JitOperation.BitReset or JitOperation.BitComplement => w != 8 && d && s,
            JitOperation.SetCondition => w == 8 && d && i.Condition is >= 0 and <= 15,
            JitOperation.ConditionalMove => w != 8 && d && IntegerRegister(i.Source, w) && i.Condition is >= 0 and <= 15,
            JitOperation.Exchange or JitOperation.ExchangeAdd or JitOperation.CompareExchange => d && IntegerRegister(i.Source, w),
            JitOperation.SignExtendAccumulator or JitOperation.SignExtendDividend => w != 8,
            JitOperation.ClearCarry or JitOperation.SetCarry or JitOperation.ComplementCarry or
            JitOperation.DirectionClear or JitOperation.DirectionSet => true,
            _ => false
        };
    }

    private ref partial struct Writer
    {
        // Physical register encoding helpers use only the volatile set above.
        private void IntegerPrefix(int width, int rm, int reg = 0)
        {
            if (width == 16) Bytes(0x66);
            int rex = 0x40 | (width == 64 ? 8 : 0) | ((reg >> 3) << 2) | (rm >> 3);
            if (rex != 0x40 || width == 8 && (rm >= 4 || reg >= 4)) Bytes((byte)rex);
        }
        private void IntegerRR(byte opcode, int rm, int reg, int width = 64)
        {
            IntegerPrefix(width, rm, reg);
            Bytes(opcode, (byte)(0xc0 | (reg & 7) << 3 | (rm & 7)));
        }
        private void IntegerMov(int destination, int source, int width = 64)
            => IntegerRR(width == 8 ? (byte)0x88 : (byte)0x89, destination, source, width);
        private void IntegerImmediate(int register, ulong value, int width = 64)
        {
            IntegerPrefix(width, register);
            Bytes((byte)(0xb8 + (register & 7)));
            if (width == 64) U64(value); else U32((uint)value);
        }
        private void IntegerConstant(byte group, int register, uint value, int width = 32)
        {
            IntegerPrefix(width, register);
            Bytes(0x81, (byte)(0xc0 | group << 3 | (register & 7))); U32(value);
        }
        private void IntegerShift(int group, int register, int width, int? count = null)
        {
            IntegerPrefix(width, register);
            Bytes(count.HasValue ? (width == 8 ? (byte)0xc0 : (byte)0xc1) :
                (width == 8 ? (byte)0xd2 : (byte)0xd3), (byte)(0xc0 | group << 3 | (register & 7)));
            if (count.HasValue) Bytes((byte)count.Value);
        }
        private void IntegerUnary(int group, int register, int width)
        {
            IntegerPrefix(width, register);
            Bytes(width == 8 ? (byte)0xf6 : (byte)0xf7, (byte)(0xc0 | group << 3 | (register & 7)));
        }
        private void IntegerLoad(JitOperand operand, int width, int register)
        {
            if (operand.IsMemory)
            {
                if (!memoryIntegerOperandStaged) throw new ArgumentException("Unprepared native integer memory operand");
                IntegerFrameLoad(208, register); // prechecked, zero-extended native word
                return;
            }
            if (operand.IsImmediate)
            {
                IntegerImmediate(register, operand.Immediate, width == 64 ? 64 : 32);
                if (width == 8) IntegerConstant(4, register, 255);
                if (width == 16) IntegerConstant(4, register, 65535);
                return;
            }
            int offset = 24 + operand.Register * 8 + operand.ByteOffset;
            // MOVZX is 32-bit and therefore also zeros the upper 32 bits.
            IntegerPrefix(width == 64 ? 64 : 32, 11, register);
            if (width is 8 or 16) Bytes(0x0f, width == 8 ? (byte)0xb6 : (byte)0xb7);
            else Bytes(0x8b);
            Memory(register & 7, 3, offset);
        }
        private void IntegerTruncate(int register, int width)
        {
            if (width == 8) IntegerConstant(4, register, 255);
            else if (width == 16) IntegerConstant(4, register, 65535);
            else if (width == 32) IntegerMov(register, register, 32);
        }
        private void IntegerSignExtend(int register, int width)
        {
            if (width == 64) return;
            IntegerPrefix(64, register, register);
            if (width == 32) Bytes(0x63);
            else Bytes(0x0f, width == 8 ? (byte)0xbe : (byte)0xbf);
            Bytes((byte)(0xc0 | (register & 7) << 3 | (register & 7)));
        }
        private int IntegerJump()
        { Bytes(0xe9); int at = position; U32(0); return at; }

        // EAX contains only the replacement Blink bits. The high byte stores
        // lazy parity input, not host PF. FLAGS_PF bit2 remains untouched.
        private void IntegerMergeFlags(uint replaced)
        {
            Bytes(0x41, 0x8b, 0x53, 12); // edx=old flags
            IntegerConstant(4, 2, ~replaced);
            IntegerRR(0x09, 0, 2, 32);
            Bytes(0x41, 0x89, 0x43, 12);
        }

        // R8=result; R9=original x XOR original y. Hardware captures only
        // defined arithmetic CF/OF/ZF/SF. AF comes from (x^y^result)&16.
        // SETcc is baseline AMD64; using DH here needs no REX prefix.
        private void IntegerArithmeticFlags(bool preserveCarry, bool logical)
        {
            Bytes(0x0f, 0x90, 0xc2); // seto dl
            Bytes(0x0f, 0x98, 0xc6); // sets dh
            Bytes(0x0f, 0x92, 0xc0); // setc al
            Bytes(0x0f, 0x94, 0xc1); // setz cl
            Bytes(0x0f, 0xb6, 0xc0, 0x0f, 0xb6, 0xc9, 0x0f, 0xb7, 0xd2);
            IntegerShift(4, 1, 32, 6); IntegerRR(0x09, 0, 1, 32);
            IntegerMov(1, 2, 32); IntegerShift(5, 1, 32, 1);
            IntegerConstant(4, 1, 128); IntegerRR(0x09, 0, 1, 32);
            IntegerShift(4, 2, 32, 11); IntegerConstant(4, 2, 2048);
            IntegerRR(0x09, 0, 2, 32);
            if (!logical)
            {
                IntegerRR(0x31, 9, 8, 32); IntegerConstant(4, 9, 16);
                IntegerRR(0x09, 0, 9, 32);
            }
            Bytes(0x41, 0x0f, 0xb6, 0xc8); // ecx = r8b
            IntegerShift(4, 1, 32, 24); IntegerRR(0x09, 0, 1, 32);
            if (preserveCarry) IntegerConstant(4, 0, 0xfffffffe);
            IntegerMergeFlags(preserveCarry ? 0xff0008d0u : 0xff0008d1u);
        }

        // R8=truncated result, R9=CF(0/1), RDX=OF(0/1). The selected Blink
        // Bsu routines clear AF and deterministically assign OF even for >1.
        private void IntegerResultFlags(int width)
        {
            IntegerRR(width == 8 ? (byte)0x84 : (byte)0x85, 8, 8, width);
            Bytes(0x0f, 0x94, 0xc0, 0x0f, 0x98, 0xc1); Bytes( 0x0f, 0xb6, 0xc0, 0x0f, 0xb6, 0xc9);
            IntegerShift(4, 0, 32, 6); IntegerShift(4, 1, 32, 7);
            IntegerRR(0x09, 0, 1, 32); IntegerRR(0x09, 0, 9, 32);
            IntegerShift(4, 2, 32, 11); IntegerRR(0x09, 0, 2, 32);
            Bytes(0x41, 0x0f, 0xb6, 0xc8); IntegerShift(4, 1, 32, 24);
            IntegerRR(0x09, 0, 1, 32); IntegerMergeFlags(0xff0008d1);
        }
        private void IntegerRotateFlags()
        {
            IntegerMov(0, 9, 32); IntegerShift(4, 2, 32, 11);
            IntegerRR(0x09, 0, 2, 32); IntegerMergeFlags(0x801);
        }

        private bool IntegerOperation(in JitInstruction i)
        {
            if (!CanInlineInteger(i)) return false;
            int w = i.Width;
            JitOperation op = i.Operation;
            switch (op)
            {
                case JitOperation.ClearCarry: case JitOperation.SetCarry: case JitOperation.ComplementCarry:
                case JitOperation.DirectionClear: case JitOperation.DirectionSet:
                    Bytes(0x41, 0x81, op is JitOperation.ClearCarry or JitOperation.DirectionClear ? (byte)0x63 :
                        op == JitOperation.ComplementCarry ? (byte)0x73 : (byte)0x4b, 12);
                    uint bit = op is JitOperation.DirectionClear or JitOperation.DirectionSet ? 0x400u : 1;
                    U32(op is JitOperation.ClearCarry or JitOperation.DirectionClear ? ~bit : bit);
                    return true;
                case JitOperation.SetCondition:
                    Condition(i.Condition); IntegerMov(8, 0); Store(i.Destination, 8); return true;
                case JitOperation.ConditionalMove:
                    // Blink WriteRegister runs even on false: CMOV32 clears high32.
                    IntegerLoad(i.Destination, w, 8); Condition(i.Condition);
                    IntegerRR(0x85, 0, 0, 32); int noMove = NearCondition(0x84);
                    IntegerLoad(i.Source, w, 8); Patch(noMove, position); Store(i.Destination, w); return true;
                case JitOperation.SignExtendAccumulator:
                    IntegerLoad(new(0, 0), w / 2, 8); IntegerSignExtend(8, w / 2);
                    IntegerTruncate(8, w); Store(new(0, 0), w); return true;
                case JitOperation.SignExtendDividend:
                    IntegerLoad(new(0, 0), w, 8); IntegerSignExtend(8, w);
                    IntegerShift(7, 8, 64, 63); IntegerTruncate(8, w); Store(new(2, 0), w); return true;
                case JitOperation.ByteSwap:
                    IntegerLoad(i.Destination, w, 8); IntegerPrefix(w, 8); Bytes(0x0f, 0xc8);
                    Store(i.Destination, w); return true;
                case JitOperation.BitScanForward: case JitOperation.BitScanReverse:
                    IntegerBitscan(in i); return true;
                case JitOperation.BitTest: case JitOperation.BitSet: case JitOperation.BitReset: case JitOperation.BitComplement:
                    IntegerBit(in i); return true;
                case JitOperation.ShiftLeft: case JitOperation.ShiftRight: case JitOperation.ShiftArithmeticRight:
                    IntegerSingleShift(in i); return true;
                case JitOperation.RotateLeft: case JitOperation.RotateRight:
                case JitOperation.RotateCarryLeft: case JitOperation.RotateCarryRight:
                    IntegerRotate(in i); return true;
                case JitOperation.ShiftDoubleLeft: case JitOperation.ShiftDoubleRight:
                    IntegerDoubleShift(in i); return true;
                case JitOperation.MultiplyUnsigned: case JitOperation.MultiplySigned: case JitOperation.MultiplySignedLow:
                    IntegerMultiply(in i); return true;
                case JitOperation.DivideUnsigned: case JitOperation.DivideSigned:
                    IntegerDivide(in i); return true;
                case JitOperation.Exchange:
                    IntegerLoad(i.Destination, w, 0); IntegerLoad(i.Source, w, 8);
                    Store(i.Destination, w); IntegerMov(8, 0); Store(i.Source, w); return true;
                case JitOperation.CompareExchange:
                    IntegerCompareExchange(in i); return true;
            }
            IntegerLoad(i.Destination, w, 0);
            if (op == JitOperation.Not)
            {
                IntegerUnary(2, 0, w); IntegerMov(8, 0); Store(i.Destination, w); return true;
            }
            bool bump = op is JitOperation.Inc or JitOperation.Dec;
            bool logical = op is JitOperation.And or JitOperation.Or or JitOperation.Xor or JitOperation.Test;
            if (bump) IntegerImmediate(1, 1, 32);
            else if (op == JitOperation.Neg) IntegerImmediate(1, 0, 32);
            else IntegerLoad(i.Source, w, 1);
            if (op == JitOperation.ExchangeAdd)
            {
                IntegerMov(8, 0); Store(i.Source, w); // both inputs already snapshotted
            }
            IntegerMov(9, 0); IntegerRR(0x31, 9, 1);
            if (op is JitOperation.Adc or JitOperation.Sbb)
                Bytes(0x41, 0x0f, 0xba, 0x63, 12, 0); // BT guest flags,0 => host CF
            int opcode = op switch
            {
                JitOperation.Add or JitOperation.Inc or JitOperation.ExchangeAdd => 0x01,
                JitOperation.Adc => 0x11, JitOperation.Sbb => 0x19,
                JitOperation.Sub or JitOperation.Cmp or JitOperation.Dec => 0x29,
                JitOperation.And or JitOperation.Test => 0x21,
                JitOperation.Or => 0x09, JitOperation.Xor => 0x31, JitOperation.Neg => 0,
                _ => throw new ArgumentException("Missing native integer lowering")
            };
            if (op == JitOperation.Neg) IntegerUnary(3, 0, w);
            else IntegerRR((byte)(w == 8 ? opcode - 1 : opcode), 0, 1, w);
            // MOV preserves status until all four SETcc captures finish.
            IntegerMov(8, 0);
            IntegerArithmeticFlags(bump, logical);
            // Narrow ALU leaves upper accumulator bits unchanged on host; input
            // loads are zero extended, so only NEG/NOT can require truncation.
            IntegerTruncate(8, w);
            if (op is not (JitOperation.Cmp or JitOperation.Test)) Store(i.Destination, w);
            return true;
        }

        private void IntegerSingleShift(in JitInstruction i)
        {
            int w = i.Width, hostWidth = w == 64 ? 64 : 32;
            IntegerLoad(i.Destination, w, 0); IntegerLoad(i.Source, 8, 1);
            IntegerMov(8, 0); IntegerConstant(4, 1, w == 64 ? 63u : 31u);
            int zero = NearCondition(0x84); // count0 preserves every flag
            IntegerMov(9, 0);
            if (i.Operation == JitOperation.ShiftLeft)
            {
                IntegerMov(2, 1, 32); IntegerUnary(3, 1, 32);
                IntegerConstant(0, 1, (uint)w); // ecx = width-count, native masked shift
                IntegerShift(5, 9, hostWidth); IntegerMov(1, 2, 32);
                IntegerShift(4, 0, hostWidth);
            }
            else
            {
                if (i.Operation == JitOperation.ShiftArithmeticRight)
                {
                    IntegerSignExtend(0, w); IntegerMov(9, 0);
                }
                IntegerConstant(5, 1, 1);
                IntegerShift(i.Operation == JitOperation.ShiftArithmeticRight ? 7 : 5, 9, hostWidth);
                IntegerConstant(0, 1, 1);
                IntegerShift(i.Operation == JitOperation.ShiftArithmeticRight ? 7 : 5, 0, hostWidth);
            }
            IntegerConstant(4, 9, 1); IntegerTruncate(0, w); IntegerMov(8, 0);
            IntegerMov(2, 8);
            if (i.Operation == JitOperation.ShiftLeft)
            {
                IntegerShift(5, 2, 64, w - 1); IntegerRR(0x31, 2, 9, 32);
            }
            else if (i.Operation == JitOperation.ShiftRight)
            {
                IntegerShift(4, 2, 64, 1); IntegerRR(0x31, 2, 8);
                IntegerShift(5, 2, 64, w - 1);
            }
            else IntegerImmediate(2, 0, 32);
            IntegerConstant(4, 2, 1); IntegerResultFlags(w);
            Patch(zero, position); Store(i.Destination, w);
        }

        private void IntegerRotate(in JitInstruction i)
        {
            int w = i.Width;
            bool right = i.Operation is JitOperation.RotateRight or JitOperation.RotateCarryRight;
            bool carry = i.Operation is JitOperation.RotateCarryLeft or JitOperation.RotateCarryRight;
            IntegerLoad(i.Destination, w, 0); IntegerMov(8, 0);
            IntegerLoad(i.Source, 8, 1); IntegerConstant(4, 1, w == 64 ? 63u : 31u);
            if (carry && w < 32)
            {
                for (int k = 0; k < (w == 8 ? 3 : 1); ++k)
                {
                    IntegerConstant(7, 1, (uint)(w + 1)); int less = NearCondition(0x82);
                    IntegerConstant(5, 1, (uint)(w + 1)); Patch(less, position);
                }
                IntegerRR(0x85, 1, 1, 32);
            }
            int zero = NearCondition(0x84);
            if (carry) Bytes(0x41, 0x0f, 0xba, 0x63, 12, 0);
            IntegerShift(carry ? right ? 3 : 2 : right ? 1 : 0, 0, w);
            if (carry) Bytes(0x41, 0x0f, 0x92, 0xc1, 0x45, 0x0f, 0xb6, 0xc9); // r9=CF
            IntegerTruncate(0, w); IntegerMov(8, 0);
            if (!carry)
            {
                IntegerMov(9, 8); if (right) IntegerShift(5, 9, 64, w - 1);
                IntegerConstant(4, 9, 1);
            }
            IntegerMov(2, 8); IntegerShift(5, 2, 64, w - 1);
            if (right)
            {
                IntegerMov(0, 8); IntegerShift(5, 0, 64, w - 2); IntegerRR(0x31, 2, 0, 32);
            }
            else IntegerRR(0x31, 2, 9, 32);
            IntegerConstant(4, 2, 1); IntegerRotateFlags();
            Patch(zero, position); Store(i.Destination, w);
        }

        private void IntegerDoubleShift(in JitInstruction i)
        {
            int w = i.Width;
            bool right = i.Operation == JitOperation.ShiftDoubleRight;
            IntegerLoad(i.Destination, w, 0); IntegerLoad(i.Source, w, 2);
            IntegerLoad(i.Extra, 8, 1); IntegerMov(8, 0);
            IntegerConstant(4, 1, w == 64 ? 63u : 31u);
            int zero = NearCondition(0x84);
            // Keep original sign and count in invocation-owned scratch only.
            IntegerFrameStore(184, 0); IntegerFrameStore(192, 1);
            IntegerMov(9, 0);
            if (right)
            {
                IntegerConstant(5, 1, 1); IntegerShift(5, 9, 64);
                IntegerConstant(0, 1, 1); IntegerShift(5, 0, 64);
                IntegerUnary(3, 1, 32); IntegerConstant(0, 1, (uint)w);
                IntegerShift(4, 2, 64);
            }
            else
            {
                IntegerShift(4, 0, 64); IntegerUnary(3, 1, 32);
                IntegerConstant(0, 1, (uint)w); IntegerShift(5, 9, 64); IntegerShift(5, 2, 64);
            }
            IntegerRR(0x09, 0, 2); IntegerTruncate(0, w); IntegerMov(8, 0);
            IntegerConstant(4, 9, 1);
            IntegerFrameLoad(184, 2); IntegerRR(0x31, 2, 8);
            IntegerShift(5, 2, 64, w - 1); IntegerConstant(4, 2, 1);
            IntegerFrameLoad(192, 1); IntegerConstant(7, 1, 1);
            int one = NearCondition(0x84); IntegerImmediate(2, 0, 32); Patch(one, position);
            IntegerResultFlags(w); Patch(zero, position); Store(i.Destination, w);
        }

        private void IntegerFrameStore(int offset, int register)
        { IntegerPrefix(64, 10, register); Bytes(0x89); Memory(register & 7, 2, offset); }
        private void IntegerFrameLoad(int offset, int register)
        { IntegerPrefix(64, 10, register); Bytes(0x8b); Memory(register & 7, 2, offset); }

        private void IntegerBitscan(in JitInstruction i)
        {
            IntegerLoad(i.Source, i.Width, 0); IntegerImmediate(8, 0, 32);
            IntegerRR(0x85, 0, 0); int zero = NearCondition(0x84);
            IntegerPrefix(64, 0, 8); Bytes(0x0f, i.Operation == JitOperation.BitScanForward ? (byte)0xbc : (byte)0xbd, 0xc0);
            Patch(zero, position);
            // Re-test source so zero and nonzero paths merge identical flags.
            IntegerRR(0x85, 0, 0); Bytes(0x0f, 0x94, 0xc0, 0x0f, 0xb6, 0xc0);
            IntegerShift(4, 0, 32, 6); IntegerMergeFlags(0x40); Store(i.Destination, i.Width);
        }
        private void IntegerBit(in JitInstruction i)
        {
            IntegerLoad(i.Destination, i.Width, 0); IntegerLoad(i.Source, i.Width, 1);
            IntegerConstant(4, 1, (uint)(i.Width - 1)); // register bit index modulo width
            IntegerPrefix(i.Width, 0, 1);
            Bytes(0x0f, i.Operation switch { JitOperation.BitSet => (byte)0xab,
                JitOperation.BitReset => (byte)0xb3, JitOperation.BitComplement => (byte)0xbb, _ => (byte)0xa3 }, 0xc8);
            IntegerMov(8, 0); Bytes(0x0f, 0x92, 0xc0, 0x0f, 0xb6, 0xc0);
            IntegerMergeFlags(1);
            if (i.Operation != JitOperation.BitTest) Store(i.Destination, i.Width);
        }
        private void IntegerMultiply(in JitInstruction i)
        {
            int w = i.Width;
            if (i.Operation == JitOperation.MultiplySignedLow)
            {
                IntegerLoad(i.Options == 1 ? i.Source : i.Destination, w, 0);
                IntegerLoad(i.Options == 1 ? i.Extra : i.Source, w, 1);
                IntegerPrefix(w, 1, 0); Bytes(0x0f, 0xaf, 0xc1);
                IntegerMov(8, 0); Store(i.Destination, w);
            }
            else
            {
                IntegerLoad(new(0, 0), w, 0); IntegerLoad(i.Source, w, 1);
                IntegerUnary(i.Operation == JitOperation.MultiplySigned ? 5 : 4, 1, w);
                IntegerMov(8, 0); Store(new(0, 0), w == 8 ? 16 : w);
                if (w != 8)
                {
                    IntegerMov(8, 2);
                    // Selected OpImulAxDx32 stores the signed high half with
                    // Put64. Preserve that bound semantic, including upper32.
                    bool signedHigh32 = w == 32 && i.Operation == JitOperation.MultiplySigned;
                    if (signedHigh32) IntegerSignExtend(8, 32);
                    Store(new(2, 0), signedHigh32 ? 64 : w);
                }
            }
            Bytes(0x0f, 0x90, 0xc0, 0x0f, 0xb6, 0xc0); // seto al, defined for MUL/IMUL
            IntegerMov(1, 0, 32); IntegerShift(4, 1, 32, 11); IntegerRR(0x09, 0, 1, 32);
            IntegerMergeFlags(0x801); // preserve undefined PF/AF/ZF/SF and lazy byte exactly
        }
        private void IntegerCompareExchange(in JitInstruction i)
        {
            int w = i.Width;
            // Selected Blink byte CMPXCHG always uses its atomic primitive,
            // even without LOCK. When p aliases AL, it restores the expected
            // old AL after computing flags from desired-old AL. Preserve this
            // selected-source behavior rather than silently substituting the
            // architectural alias behavior of a host CMPXCHG instruction.
            if (w == 8 && i.Destination.Register == 0 && i.Destination.ByteOffset == 0)
            {
                IntegerLoad(i.Source, 8, 0); IntegerLoad(new(0, 0), 8, 1);
                IntegerMov(9, 0); IntegerRR(0x31, 9, 1);
                IntegerRR(0x28, 0, 1, 8); IntegerMov(8, 0);
                IntegerArithmeticFlags(false, false); return;
            }
            IntegerLoad(new(0, 0), w, 0); IntegerLoad(i.Destination, w, 1);
            IntegerMov(9, 0); IntegerRR(0x31, 9, 1);
            IntegerRR(w == 8 ? (byte)0x28 : (byte)0x29, 0, 1, w);
            IntegerMov(8, 0); IntegerArithmeticFlags(false, false);
            Bytes(0x41, 0xf6, 0x43, 12, 0x40); int equal = NearCondition(0x85);
            IntegerLoad(i.Destination, w, 8); Store(new(0, 0), w);
            if (w == 32) Store(i.Destination, 32); // selected C clears p+4 even on failure
            int done = IntegerJump();
            Patch(equal, position); IntegerLoad(i.Source, w, 8); Store(i.Destination, w); Patch(done, position);
        }

        private void IntegerDivide(in JitInstruction i)
        {
            int w = i.Width;
            bool signed = i.Operation == JitOperation.DivideSigned;
            IntegerLoad(i.Source, w, 1);
            if (signed) IntegerSignExtend(1, w);
            IntegerRR(0x85, 1, 1); int divisorZero = NearCondition(0x84);
            if (w == 64)
            {
                IntegerLoad(new(0, 0), 64, 0); IntegerLoad(new(2, 0), 64, 2);
            }
            else
            {
                IntegerLoad(new(0, 0), w == 8 ? 16 : w, 0);
                if (w != 8)
                {
                    IntegerLoad(new(2, 0), w, 2); IntegerShift(4, 2, 64, w); IntegerRR(0x09, 0, 2);
                }
                if (signed) IntegerSignExtend(0, 2 * w);
                IntegerImmediate(2, 0, 32);
                if (signed) { IntegerMov(2, 0); IntegerShift(7, 2, 64, 63); }
            }
            if (signed)
            {
                // Work with unsigned magnitudes. R9=dividend sign, R8=divisor
                // sign. No IDIV is emitted, so even hostile 128-bit inputs can
                // never cause host #DE. Guest writes happen after all checks.
                IntegerMov(9, 2); IntegerShift(7, 9, 64, 63);
                IntegerMov(8, 1); IntegerShift(7, 8, 64, 63);
                IntegerRR(0x85, 9, 9); int positive = NearCondition(0x84);
                IntegerUnary(3, 0, 64); IntegerConstant(2, 2, 0, 64); IntegerUnary(3, 2, 64);
                Patch(positive, position);
                IntegerRR(0x31, 1, 8); IntegerRR(0x29, 1, 8);
            }
            IntegerRR(0x39, 2, 1); int overflowHigh = NearCondition(0x83); // high >= divisor
            IntegerUnary(6, 1, 64); // unsigned DIV rcx, safe after high<divisor
            int overflowQuotient = -1;
            if (signed)
            {
                IntegerRR(0x31, 8, 9); // sign of quotient
                IntegerImmediate(1, (1UL << (w - 1)) - 1);
                // Negative result permits one extra magnitude, exactly MIN.
                IntegerRR(0x29, 1, 8); IntegerRR(0x39, 0, 1);
                overflowQuotient = NearCondition(0x87);
                IntegerRR(0x31, 0, 8); IntegerRR(0x29, 0, 8);
                IntegerRR(0x31, 2, 9); IntegerRR(0x29, 2, 9);
            }
            else if (w != 64)
            {
                IntegerImmediate(1, (1UL << w) - 1); IntegerRR(0x39, 0, 1);
                overflowQuotient = NearCondition(0x87);
            }
            IntegerMov(8, 0); IntegerTruncate(8, w); Store(new(0, 0), w);
            IntegerMov(8, 2); IntegerTruncate(8, w); Store(w == 8 ? new(0, 0, ByteOffset: 1) : new(2, 0), w);
            int done = IntegerJump();
            Patch(divisorZero, position); Patch(overflowHigh, position);
            if (overflowQuotient >= 0) Patch(overflowQuotient, position);
            Bytes(0x41, 0xc7, 0x82); U32(168); U32(0); // FaultTrap=#DE
            Bytes(0x41, 0xc7, 0x82); U32(172); U32(0);
            IntegerImmediate(0, i.Ip); IntegerFrameStore(176, 0);
            // The operation guard has reserved exactly one issue. IP/oplen,
            // registers, flags, completion and Executed retain pre-fault state.
            JumpExit(FullBlockExit.GuestFault); Patch(done, position);
        }
    }
}
