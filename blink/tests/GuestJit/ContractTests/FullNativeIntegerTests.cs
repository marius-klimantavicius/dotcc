using System.Buffers.Binary;
using System.Numerics;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // Authored R60 contracts. Parent wires this into Main and owns all serial
    // builds/execution. This file was NOT executed by the implementation task.
    // GetOp below is the actual selected interpreter operation, called only by
    // this test oracle after authoritative XED decoding; product lowering never
    // calls it or any managed operation semantics.
    static void CheckFullNativeInteger(byte* machine)
    {
        CheckFullIntegerDecoder();
        // The selected interpreter register accessors use its initialized bus.
        // Reuse the real owner fixture rather than a zeroed global bus pointer.
        using var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle: true);
        var program = fixture.Program;
        CheckFullIntegerArithmetic(program, machine);
        CheckFullIntegerShifts(program, machine);
        CheckFullIntegerBitsAndConditions(program, machine);
        CheckFullIntegerMultiplyDivide(program, machine);
        CheckFullIntegerExchanges(program, machine);
        CheckFullIntegerFaultBoundaries(machine);
        CheckFullIntegerSpanAllocation();
    }

    static byte[] IntegerEncoding(int width, params byte[] opcode)
    {
        byte[] result = new byte[opcode.Length + (width is 16 or 64 ? 1 : 0)];
        int p = 0;
        if (width == 16) result[p++] = 0x66;
        if (width == 64) result[p++] = 0x48;
        opcode.CopyTo(result, p); return result;
    }
    static ulong IntegerMask(int width) => width == 64 ? ulong.MaxValue : (1UL << width) - 1;
    static void IntegerSeed(byte* m, uint flags = 0xa52c0e15)
    {
        ResetFull(m); m[8] = 0;
        ((uint*)(m + 12))[0] = flags & ~0x50100u; // TF/RF/AC are eligibility boundaries
        for (int r = 0; r != 16; ++r) Write64(m, 24 + r * 8, 0xfedcba9876543210UL ^ (0x0101010101010101UL * (uint)r));
    }
    static void IntegerRegisterWrite(byte* machine, JitOperand operand, int width, ulong value)
    {
        byte* pointer = machine + 24 + operand.Register * 8 + operand.ByteOffset;
        if (width == 8) *pointer = (byte)value;
        else if (width == 16) ((ushort*)pointer)[0] = (ushort)value;
        else ((ulong*)pointer)[0] = width == 32 ? (uint)value : value;
    }
    static void IntegerReference(Blink program, byte* machine, ReadOnlySpan<byte> encoding)
    {
        Blink.XedDecodedInst decoded = default;
        fixed (byte* p = encoding)
            Check(program.DecodeInstruction(&decoded, p, (ulong)encoding.Length, Blink.XED_MODE_LONG) == 0 &&
                decoded.length == encoding.Length, "R60 oracle authoritative XED length");
        var m = (Blink.Machine*)machine;
        var previous = m->xedd;
        m->xedd = &decoded; m->oplen = (byte)encoding.Length; m->ip += (uint)encoding.Length;
        try
        {
            var op = decoded.op;
            program.GetOp((long)((op.rde >> 40) & 2047))(program, m, op.rde, op.disp, op.uimm0);
            if (m->stashaddr != 0) program.CommitStash(m);
            m->oplen = 0;
        }
        finally { m->xedd = previous; }
    }
    static void IntegerCompare(Blink program, byte* machine, byte[] encoding)
    {
        Check(FullIntegerLowering.TryClassify(Ip, encoding, out var instruction, out string? reason), reason ?? "integer classification");
        byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
        IntegerReference(program, machine, encoding);
        byte[] expected = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
        before.AsSpan().CopyTo(new Span<byte>(machine, sizeof(Blink.Machine)));
        using var code = CreateFullBlock([new(instruction)]);
        ulong epoch = 11; long issued = 0, completed = 0; int stopped = 0;
        TranslationBlockHeader header = FullHeader(Ip, code.Entry);
        FullBlockFrame frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stopped);
        Check(RunFull(code, &frame) == FullBlockExit.Continue && issued == 1 && completed == 1 && frame.Executed == 1,
            "R60 ordinary integer completes natively without helper exit");
        Check(new ReadOnlySpan<byte>(machine, 9).SequenceEqual(expected.AsSpan(0, 9)) &&
            new ReadOnlySpan<byte>(machine + 12, 4).SequenceEqual(expected.AsSpan(12, 4)) &&
            new ReadOnlySpan<byte>(machine + 24, 128).SequenceEqual(expected.AsSpan(24, 128)),
            $"R60 selected interpreter/native state: {Convert.ToHexString(encoding)} {instruction.Operation}/{instruction.Width}");
    }

    static void CheckFullIntegerArithmetic(Blink program, byte* machine)
    {
        foreach (int width in new[] { 8, 16, 32, 64 })
        {
            ulong mask = IntegerMask(width), sign = 1UL << (width - 1);
            foreach (byte opcode in new byte[] { 0x01, 0x11, 0x19, 0x29, 0x09, 0x21, 0x31, 0x39, 0x85 })
            foreach (var values in new[] { (0UL, 0UL), (mask, 1UL), (sign - 1, 1UL), (sign, mask),
                (0x0fUL, 1UL), (0x10UL, 1UL), (mask, mask), (sign, sign) })
            foreach (uint carry in new uint[] { 0, 1 })
            {
                IntegerSeed(machine, 0xc42c0e14 | carry);
                IntegerRegisterWrite(machine, new(3, 0), width, values.Item1);
                IntegerRegisterWrite(machine, new(6, 0), width, values.Item2);
                IntegerCompare(program, machine, IntegerEncoding(width,
                    width == 8 ? (byte)(opcode - 1) : opcode, 0xf3));
            }
            foreach (int group in new[] { 2, 3, 8, 9 })
            foreach (ulong value in new[] { 0UL, 1UL, 15UL, 16UL, sign - 1, sign, mask })
            {
                IntegerSeed(machine);
                IntegerRegisterWrite(machine, new(3, 0), width, value);
                bool bump = group >= 8;
                byte opcode = bump ? width == 8 ? (byte)0xfe : (byte)0xff : width == 8 ? (byte)0xf6 : (byte)0xf7;
                IntegerCompare(program, machine, IntegerEncoding(width, opcode, (byte)(0xc3 | (group & 7) << 3)));
            }
            // Sign-extended imm8, imm16/32, and self-source aliases.
            IntegerSeed(machine); IntegerCompare(program, machine, IntegerEncoding(width,
                width == 8 ? (byte)0x80 : (byte)0x83, 0xd3, 0xff)); // ADC immediate -1
            IntegerSeed(machine); IntegerCompare(program, machine, IntegerEncoding(width,
                width == 8 ? (byte)0x10 : (byte)0x11, 0xdb)); // ADC destination=source
        }
        foreach (byte[] encoding in new byte[][] { [0x10, 0xfc], [0x18, 0xe7], [0x40, 0x10, 0xf4],
            [0x44, 0x10, 0xfc], [0x66, 0x83, 0xd3, 0x80], [0x48, 0x81, 0xdb, 0xff, 0xff, 0xff, 0x7f],
            [0x66, 0x98], [0x98], [0x48, 0x98], [0x66, 0x99], [0x99], [0x48, 0x99],
            [0x0f, 0xbe, 0xdc], [0x48, 0x0f, 0xbf, 0xde], [0x48, 0x63, 0xde],
            [0x0f, 0xb6, 0xdc], [0x66, 0x0f, 0xb6, 0xdc], [0x48, 0x0f, 0xb7, 0xde] })
        { IntegerSeed(machine); IntegerCompare(program, machine, encoding); }
    }

    static void CheckFullIntegerShifts(Blink program, byte* machine)
    {
        foreach (int width in new[] { 8, 16, 32, 64 })
        foreach (int group in new[] { 0, 1, 2, 3, 4, 5, 7 })
        foreach (int count in new[] { 0, 1, 2, 7, 8, 9, 15, 16, 17, 31, 32, 33, 63, 64, 65, 255 })
        foreach (uint carry in new uint[] { 0, 1 })
        {
            IntegerSeed(machine, 0x552c0e14 | carry);
            IntegerRegisterWrite(machine, new(3, 0), width, (1UL << (width - 1)) | 0x13);
            IntegerRegisterWrite(machine, new(1, 0), 8, (uint)count);
            IntegerCompare(program, machine, IntegerEncoding(width, width == 8 ? (byte)0xd2 : (byte)0xd3,
                (byte)(0xc3 | group << 3)));
        }
        foreach (int width in new[] { 16, 32, 64 })
        foreach (bool right in new[] { false, true })
        foreach (int count in new[] { 0, 1, 2, 15, 16, 17, 31, 32, 63, 64, 255 })
        {
            IntegerSeed(machine); IntegerRegisterWrite(machine, new(1, 0), 8, (uint)count);
            IntegerCompare(program, machine, IntegerEncoding(width, 0x0f, right ? (byte)0xad : (byte)0xa5, 0xf3));
            IntegerSeed(machine);
            IntegerCompare(program, machine, IntegerEncoding(width, 0x0f, right ? (byte)0xac : (byte)0xa4, 0xf3, (byte)count));
        }
        // CL aliases the destination, high-byte operands, and source=destination.
        foreach (byte[] encoding in new byte[][] { [0xd2, 0xe1], [0xc0, 0xd4, 0x11], [0xc0, 0xcc, 8],
            [0x48, 0x0f, 0xa5, 0xc9], [0x66, 0x0f, 0xac, 0xf3, 17] })
        { IntegerSeed(machine); IntegerCompare(program, machine, encoding); }
    }

    static void CheckFullIntegerBitsAndConditions(Blink program, byte* machine)
    {
        foreach (int width in new[] { 16, 32, 64 })
        foreach (byte second in new byte[] { 0xbc, 0xbd, 0xa3, 0xab, 0xb3, 0xbb })
        foreach (ulong value in new[] { 0UL, 1UL, 0x10000UL, ulong.MaxValue, 0x8000000000000000UL })
        {
            IntegerSeed(machine); IntegerRegisterWrite(machine, new(6, 0), width, value);
            IntegerCompare(program, machine, IntegerEncoding(width, 0x0f, second,
                second is 0xbc or 0xbd ? (byte)0xde : (byte)0xf3));
        }
        foreach (int width in new[] { 32, 64 })
        { IntegerSeed(machine); IntegerCompare(program, machine, IntegerEncoding(width, 0x0f, 0xcb)); }
        foreach (int condition in Enumerable.Range(0, 16))
        foreach (uint flags in new uint[] { 0, 1, 0x40, 0x80, 0x800, 0x880, 0x40 | 1, 0x03000000, 0x01000000 })
        {
            IntegerSeed(machine, flags); IntegerCompare(program, machine, [0x0f, (byte)(0x90 | condition), 0xc4]); // AH
            foreach (int width in new[] { 16, 32, 64 })
            {
                IntegerSeed(machine, flags); IntegerCompare(program, machine, IntegerEncoding(width, 0x0f, (byte)(0x40 | condition), 0xde));
            }
        }
        foreach (byte opcode in new byte[] { 0xf8, 0xf9, 0xf5, 0xfc, 0xfd })
        { IntegerSeed(machine); IntegerCompare(program, machine, [opcode]); }
    }

    static void CheckFullIntegerMultiplyDivide(Blink program, byte* machine)
    {
        foreach (int width in new[] { 8, 16, 32, 64 })
        foreach (int group in new[] { 4, 5 })
        foreach (ulong operand in new[] { 0UL, 1UL, 3UL, 127UL, 128UL, ulong.MaxValue, 0x8000000000000000UL })
        {
            IntegerSeed(machine); IntegerRegisterWrite(machine, new(3, 0), width, operand);
            IntegerCompare(program, machine, IntegerEncoding(width, width == 8 ? (byte)0xf6 : (byte)0xf7, (byte)(0xc3 | group << 3)));
        }
        foreach (int width in new[] { 16, 32, 64 })
        {
            IntegerSeed(machine); IntegerCompare(program, machine, IntegerEncoding(width, 0x0f, 0xaf, 0xde));
            IntegerSeed(machine); IntegerCompare(program, machine, IntegerEncoding(width, 0x6b, 0xde, 0x80));
            IntegerSeed(machine); IntegerCompare(program, machine, width == 16 ? [0x66, 0x69, 0xde, 0xff, 0x7f] :
                IntegerEncoding(width, 0x69, 0xde, 0xff, 0xff, 0xff, 0x7f));
        }
        foreach (int width in new[] { 8, 16, 32, 64 })
        foreach (bool signed in new[] { false, true })
        foreach (long dividend in new[] { 0L, 1L, 63L, 127L, -1L, -127L })
        {
            if (!signed && dividend < 0) continue;
            IntegerSeed(machine);
            IntegerRegisterWrite(machine, new(0, 0), width == 8 ? 16 : width, unchecked((ulong)dividend));
            if (width != 8) IntegerRegisterWrite(machine, new(2, 0), width, dividend < 0 ? ulong.MaxValue : 0);
            IntegerRegisterWrite(machine, new(3, 0), width, signed ? unchecked((ulong)-3L) : 3);
            IntegerCompare(program, machine, IntegerEncoding(width, width == 8 ? (byte)0xf6 : (byte)0xf7, signed ? (byte)0xfb : (byte)0xf3));
        }
    }

    static void CheckFullIntegerExchanges(Blink program, byte* machine)
    {
        foreach (int width in new[] { 8, 16, 32, 64 })
        foreach (byte[] opcode in new byte[][] { [0x87], [0x0f, 0xc1], [0x0f, 0xb1] })
        foreach (byte modrm in new byte[] { 0xf3, 0xdb, 0xf0 }) // distinct/self/accumulator aliases
        foreach (bool match in new[] { false, true })
        {
            IntegerSeed(machine);
            if (match) IntegerRegisterWrite(machine, new(0, 0), width, ((ulong*)(machine + 24 + (modrm & 7) * 8))[0]);
            byte[] bytes = (byte[])opcode.Clone(); if (width == 8) --bytes[^1];
            IntegerCompare(program, machine, IntegerEncoding(width, bytes.Concat(new[] { modrm }).ToArray()));
        }
        foreach (byte[] encoding in new byte[][] { [0x0f, 0xb0, 0xdc], [0x0f, 0xb0, 0xd8],
            [0x86, 0xe0], [0x0f, 0xc0, 0xe0], [0x41, 0x97] })
        { IntegerSeed(machine); IntegerCompare(program, machine, encoding); }
    }

    static void IntegerCheckDivide(byte* machine, int width, bool signed, ulong low, ulong high, ulong divisor)
    {
        IntegerSeed(machine);
        IntegerRegisterWrite(machine, new(0, 0), width == 8 ? 16 : width, low);
        if (width != 8) IntegerRegisterWrite(machine, new(2, 0), width, high);
        IntegerRegisterWrite(machine, new(3, 0), width, divisor);
        ulong mask = IntegerMask(width);
        BigInteger dividend = width == 8 ? low & 65535 : ((BigInteger)(high & mask) << width) | (low & mask);
        int doubleWidth = width * 2;
        if (signed && (dividend & (BigInteger.One << (doubleWidth - 1))) != 0) dividend -= BigInteger.One << doubleWidth;
        BigInteger denominator = divisor & mask;
        if (signed && (denominator & (BigInteger.One << (width - 1))) != 0) denominator -= BigInteger.One << width;
        bool fault = denominator == 0;
        BigInteger quotient = 0, remainder = 0;
        if (!fault)
        {
            quotient = BigInteger.DivRem(dividend, denominator, out remainder);
            fault = signed ? quotient < -(BigInteger.One << (width - 1)) || quotient >= (BigInteger.One << (width - 1)) :
                quotient < 0 || quotient > mask;
        }
        byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
        JitInstruction instruction = new(Ip, 3, signed ? JitOperation.DivideSigned : JitOperation.DivideUnsigned,
            width, default, new(3, 0));
        using var code = CreateFullBlock([new(instruction)]);
        ulong epoch = 11; long issued = 0, completed = 0; int stopped = 0;
        TranslationBlockHeader header = FullHeader(Ip, code.Entry);
        FullBlockFrame frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stopped);
        FullBlockExit exit = RunFull(code, &frame);
        if (fault)
        {
            Check(exit == FullBlockExit.GuestFault && issued == 1 && completed == 0 && frame.Executed == 0 &&
                frame.FaultTrap == 0 && frame.FaultCode == 0 && frame.FaultAddress == Ip &&
                new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(before),
                "R60 divide fault is issued once, precise, and has no guest writeback or host exception");
        }
        else
        {
            Check(exit == FullBlockExit.Continue && issued == 1 && completed == 1 && frame.Executed == 1 &&
                ((uint*)(machine + 12))[0] == BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12)),
                "R60 checked divide completes with preserved undefined Blink flags");
            ulong q = (ulong)(quotient & mask), r = (ulong)(remainder & mask);
            byte[] expected = (byte[])before.Clone();
            fixed (byte* pointer = expected)
            {
                IntegerRegisterWrite(pointer, new(0, 0), width, q);
                IntegerRegisterWrite(pointer, width == 8 ? new(0, 0, ByteOffset: 1) : new(2, 0), width, r);
            }
            Check(new ReadOnlySpan<byte>(machine + 24, 128).SequenceEqual(expected.AsSpan(24, 128)),
                "R60 divide quotient/remainder match independent BigInteger oracle including sign and high halves");
        }
    }

    static void CheckFullIntegerFaultBoundaries(byte* machine)
    {
        foreach (int width in new[] { 8, 16, 32, 64 })
        {
            ulong mask = IntegerMask(width), sign = 1UL << (width - 1);
            IntegerCheckDivide(machine, width, false, 17, 0, 0);
            IntegerCheckDivide(machine, width, true, 17, 0, 0);
            IntegerCheckDivide(machine, width, false, width == 8 ? 0x100UL : 0, width == 8 ? 0UL : 1UL, 1);
            IntegerCheckDivide(machine, width, false, width == 8 ? 0xffUL : mask, 0, 1);
            IntegerCheckDivide(machine, width, true, width == 8 ? 0x80UL : sign, 0, 1); // positive overflow
            IntegerCheckDivide(machine, width, true, width == 8 ? 0xff80UL : sign, mask, mask); // MIN/-1
            IntegerCheckDivide(machine, width, true, width == 8 ? 0xff80UL : sign, mask, 1); // exact MIN
            IntegerCheckDivide(machine, width, true, width == 8 ? 0x8000UL : 0, sign, mask); // double-width MIN
            IntegerCheckDivide(machine, width, true, width == 8 ? 0xffffUL : mask, mask, sign);
            IntegerCheckDivide(machine, width, false, width == 8 ? 0xfeUL : mask, 0, 3);
        }
        // Budget/stop/attention take precedence over a would-fault division;
        // rejected lock-xadd still reserves its exact historical issue count.
        foreach (int boundary in new[] { 0, 1, 2 })
        {
            IntegerSeed(machine); IntegerRegisterWrite(machine, new(3, 0), 64, 0);
            using var code = CreateFullBlock([new(new(Ip, 3, JitOperation.DivideUnsigned, 64, default, new(3, 0)))]);
            ulong epoch = 11; long issued = 0, completed = 0; int stopped = boundary == 1 ? 1 : 0;
            if (boundary == 2) machine[11] = 1;
            TranslationBlockHeader header = FullHeader(Ip, code.Entry);
            FullBlockFrame frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stopped, budget: 0);
            Check(RunFull(code, &frame) == (boundary == 0 ? FullBlockExit.Budget : boundary == 1 ? FullBlockExit.Stop : FullBlockExit.Attention) &&
                issued == (boundary == 0 ? 1 : 0) && completed == 0 && frame.Executed == 0 && frame.FaultAddress == 0,
                "R60 ordinary fault lowering preserves boundary and rejected-issue precedence");
        }
    }

    static void CheckFullIntegerDecoder()
    {
        byte[] padded = [0xcc, 0x48, 0x81, 0xd3, 0x00, 0x00, 0x00, 0x80, 0xcc];
        Check(FullIntegerLowering.TryClassify(Ip, padded.AsSpan(1, 7), out var adc, out _) &&
            adc.Operation == JitOperation.Adc && adc.Source.Immediate == 0xffffffff80000000UL,
            "R60 unaligned imm32 is sign extended to qword");
        Check(FullIntegerLowering.TryClassify(Ip, [0x10, 0xfc], out var high, out _) &&
            high.Destination == new JitOperand(0, 0, ByteOffset: 1) && high.Source == new JitOperand(3, 0, ByteOffset: 1),
            "R60 AH/BH byte operands without REX");
        Check(FullIntegerLowering.TryClassify(Ip, [0x40, 0x10, 0xfc], out var low, out _) &&
            low.Destination.Register == 4 && low.Destination.ByteOffset == 0 && low.Source.Register == 7,
            "R60 REX switches high bytes to SPL/DIL");
        foreach (byte[] rejected in new byte[][] { [], [0x48], [0x48, 0x81, 0xd3, 1], [0x10, 0xfc, 0x90],
            [0xf0, 0x11, 0xf3], [0xf3, 0x0f, 0xbc, 0xde], [0x67, 0xd3, 0xe3], [0x64, 0x11, 0xf3],
            [0x66, 0x66, 0x11, 0xf3], [0x48, 0x66, 0x11, 0xf3], [0x0f, 0xaf, 0x03], [0xf7, 0xc8],
            [0x66, 0x0f, 0xcb], [0x0f, 0xba, 0xc3, 2] })
            Check(!FullIntegerLowering.TryClassify(Ip, rejected, out _, out string? reason) && reason != null,
                "R60 unsupported/reserved/truncated/prefix/memory forms are explicit misses");
    }

    static void CheckFullIntegerSpanAllocation()
    {
        FullBlockInstruction[] instructions = [
            new(new(Ip, 3, JitOperation.Adc, 64, new(15, 0), new(0, 0))),
            new(new(Ip + 3, 3, JitOperation.RotateCarryLeft, 8, new(0, 0, ByteOffset: 1), new(1, 0))),
            new(new(Ip + 6, 3, JitOperation.DivideSigned, 64, default, new(3, 0))) ];
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity];
        foreach (bool windows in new[] { false, true })
        {
            for (int warm = 0; warm < 10; ++warm)
                Check(FullBlockEmitter.TryEmit(instructions, staging, out _, windows), "R60 warm emit fits");
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool good = true;
            for (int repeat = 0; repeat != 32; ++repeat)
                good &= FullBlockEmitter.TryEmit(instructions, staging, out int length, windows) && length > 0;
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Check(good && allocated == 0, "R60 both ABI warmed span emit allocates zero bytes");
            Check(staging.AsSpan(0, 7).SequenceEqual(windows ?
                new byte[] { 0xf3, 0x0f, 0x1e, 0xfa, 0x49, 0x89, 0xca } :
                new byte[] { 0xf3, 0x0f, 0x1e, 0xfa, 0x49, 0x89, 0xfa }), "R60 SysV/Win64 argument encoding");
            byte[] tiny = new byte[11];
            Check(!FullBlockEmitter.TryEmit(instructions, tiny, out int shortLength, windows) && shortLength == 0,
                "R60 expanded integer oversize output is never publishable");
        }
    }
}
