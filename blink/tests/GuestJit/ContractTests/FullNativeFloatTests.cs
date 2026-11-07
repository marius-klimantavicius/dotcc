using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // R70 AUTHORED UNRUN. Parent must wire all authenticated native RET cleanup
    // sites BEFORE invoking this entry. Fresh producer5 + Normalize19 are also
    // required before qualifying controller/signal delivery of unmasked #XM.
    static void CheckFullNativeFloat(byte* machine)
    {
        Check(FullBlockEmitter.HasExpectedFloatLayout(), "R70 exact MXCSR/frame layout");
        CheckFullFloatClassifier();
        CheckFullFloatSpanAllocation();
        Check(!fullOwnerScaffold.IsValid, "R70 standalone/scaffold fixture begins clear");
        CheckFullFloatLeafContracts(machine);
        using (var common = CreateFullScaffold(out var addresses))
        {
            fullOwnerScaffold = addresses;
            try { CheckFullFloatLeafContracts(machine); }
            finally { fullOwnerScaffold = default; }
        }
        using var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle: true);
        CheckFullFloatOracle(fixture);
        CheckFullFloatMemoryOracle(fixture);
    }

    static void CheckFullFloatLeafContracts(byte* machine)
    {
        CheckFullFloatExact(machine);
        CheckFullFloatSpecialBits(machine);
        CheckFullFloatRoundingAndRange(machine);
        CheckFullFloatConversions(machine);
        CheckFullFloatPackedFaultCommit(machine);
        CheckFullFloatMemoryFaults(machine);
        CheckFullFloatRestoreExits(machine);
    }

    static JitInstruction FloatClassify(ReadOnlySpan<byte> bytes)
    {
        Check(FullFloatLowering.TryClassify(Ip, bytes, out var instruction, out var reason), reason ?? "R70 classify");
        Check(FullBlockEmitter.ValidateFloat(instruction), "R70 float IR validates");
        return instruction;
    }

    static byte[] FloatEncoding(byte prefix, byte opcode, byte modrm = 0xc1, byte? immediate = null, bool wide = false)
    {
        var bytes = new List<byte>(6);
        if (prefix != 0) bytes.Add(prefix);
        if (wide) bytes.Add(0x48);
        bytes.Add(0x0f); bytes.Add(opcode); bytes.Add(modrm);
        if (immediate.HasValue) bytes.Add(immediate.Value);
        return bytes.ToArray();
    }

    static void FloatSeed(byte* machine, uint mxcsr = 0x1f80, bool reset = true)
    {
        if (reset) ResetFull(machine);
        var m = (Blink.Machine*)machine;
        m->ip = Ip; m->oplen = 0; m->attention = false; m->stashaddr = 0;
        m->mode.omode = Blink.XED_MODE_LONG; m->mode.genmode = Blink.XED_GEN_MODE_PROTECTED;
        m->mxcsr = mxcsr; m->flags = 0xa52c0e15u & ~0x50100u;
        for (int r = 0; r < 16; ++r)
        {
            Write64(machine, 24 + r * 8, 0xabcdef1234567890UL ^ (uint)r);
            for (int b = 0; b < 16; ++b) machine[160 + r * 16 + b] = (byte)(17 * r + 31 * b);
        }
    }

    static void FloatWrite(byte* machine, int reg, int lane, int width, ulong bits)
    {
        byte* p = machine + 160 + reg * 16 + lane * (width / 8);
        if (width == 32) ((uint*)p)[0] = (uint)bits; else ((ulong*)p)[0] = bits;
    }
    static ulong FloatRead(byte* machine, int reg, int lane, int width) => width == 32 ?
        ((uint*)(machine + 160 + reg * 16 + lane * 4))[0] : ((ulong*)(machine + 160 + reg * 16 + lane * 8))[0];

    // This TEST-ONLY wrapper installs a distinctive, safely masked host control
    // and sticky state, calls the product leaf, captures its returned host state,
    // then restores the true original BEFORE any CLR code can run. Shadow space
    // and 16-byte call alignment are provided on both ABIs. The product remains
    // a no-CALL/no-stack volatile-register leaf. No test invokes the other ABI.
    static NativeCodeAllocation FloatHostProbe(nint entry, uint control)
    {
        var bytes = new List<byte>(96) { 0xf3,0x0f,0x1e,0xfa, 0x48,0x83,0xec,0x38,
            0x48,0x89, OperatingSystem.IsWindows() ? (byte)0x54 : (byte)0x74,0x24,0x20,
            0x0f,0xae,0x5c,0x24,0x28, 0xc7,0x44,0x24,0x2c };
        for (int b = 0; b < 4; ++b) bytes.Add((byte)(control >> (8 * b)));
        bytes.AddRange(new byte[] { 0x0f,0xae,0x54,0x24,0x2c, 0x48,0xb8 });
        for (int b = 0; b < 8; ++b) bytes.Add((byte)((ulong)entry >> (8 * b)));
        bytes.AddRange(new byte[] { 0xff,0xd0, 0x48,0x8b,0x54,0x24,0x20,
            0x0f,0xae,0x1a, 0x0f,0xae,0x54,0x24,0x28, 0x48,0x83,0xc4,0x38,0xc3 });
        Check(NativeCodeAllocation.TryCreate(bytes.ToArray(), out var code, out var reason), reason);
        return code!;
    }

    static FullBlockExit FloatRunProbe(NativeCodeAllocation code, FullBlockFrame* frame, uint host = 0x5fa1)
    {
        using var probe = FloatHostProbe(code.Entry, host);
        uint observed = 0;
        frame->NextIp = ulong.MaxValue;
        ulong beforeExecuted = frame->Executed;
        int result = ((delegate* unmanaged<FullBlockFrame*, uint*, int>)probe.Entry)(frame, &observed);
        RecordQualificationNative(frame, beforeExecuted);
        Check(observed == host && frame->HostMxcsrDirty == 0, "R70 exact HOST sticky/RC/masks restored before CLR return");
        Check(result == (int)frame->Exit && frame->NextIp == ((Blink.Machine*)frame->Machine)->ip,
            "R70 typed native return materializes actual PC");
        return (FullBlockExit)result;
    }

    static void FloatRunBits(byte* machine, byte[] encoding, uint status = 0, bool fault = false)
    {
        var i = FloatClassify(encoding);
        using var code = CreateFullBlock([new(i)]);
        ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
        var header = FullHeader(Ip, code.Entry); var frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stop);
        uint control = ((Blink.Machine*)machine)->mxcsr, flags = ((Blink.Machine*)machine)->flags;
        byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
        var exit = FloatRunProbe(code, &frame);
        Check(exit == (fault ? FullBlockExit.GuestFault : FullBlockExit.Continue) && issued == 1 &&
            completed == (fault ? 0 : 1) && frame.Executed == (fault ? 0UL : 1UL) && frame.NativeMemoryComputations == 0,
            "R70 one issue and precise fault/success completion");
        Check(((Blink.Machine*)machine)->mxcsr == (control | status), "R70 exact GUEST MXCSR sticky/control bits");
        if (fault)
        {
            Check(frame.FaultTrap == 19 && frame.FaultCode == 0 && frame.FaultAddress == Ip,
                "R70 CPU vector19 distinguished from internal Blink -9");
            BinaryPrimitives.WriteUInt32LittleEndian(before.AsSpan(688, 4), control | status);
            Check(new ReadOnlySpan<byte>(machine, before.Length).SequenceEqual(before),
                "R70 fault commits sticky only; no XMM/GPR/flags/PC/oplen/x87 partial commit");
        }
        else
        {
            Check(((Blink.Machine*)machine)->ip == Ip + (uint)i.Length && ((Blink.Machine*)machine)->oplen == 0,
                "R70 exact successful PC/oplen commit");
            if (FullBlockEmitter.FloatOperation(i) is not (FullBlockEmitter.FullFloatOperation.CompareOrderedFlags or FullBlockEmitter.FullFloatOperation.CompareUnorderedFlags))
                Check(((Blink.Machine*)machine)->flags == flags, "R70 arithmetic/conversion preserves integer flags");
            Check(new ReadOnlySpan<byte>(machine + 584, 104).SequenceEqual(before.AsSpan(584, 104)), "R70 x87 remains untouched");
        }
    }

    static void CheckFullFloatExact(byte* machine)
    {
        foreach (byte prefix in new byte[] { 0,0x66,0xf2,0xf3 })
        foreach (byte opcode in new byte[] { 0x58,0x5c,0x59,0x5e,0x5d,0x5f,0x51 })
        foreach (bool alias in new[] { false,true })
        {
            FloatSeed(machine);
            int width = prefix is 0x66 or 0xf2 ? 64 : 32, lanes = prefix is 0xf2 or 0xf3 ? 1 : 128 / width;
            ulong two = width == 64 ? 0x4000000000000000UL : 0x40000000,
                four = width == 64 ? 0x4010000000000000UL : 0x40800000;
            for (int lane = 0; lane < 128 / width; ++lane)
            { FloatWrite(machine, 0, lane, width, four); FloatWrite(machine, 1, lane, width, two); }
            byte[] upper = new ReadOnlySpan<byte>(machine + 160,16).ToArray();
            ulong expected = opcode switch
            {
                0x58 => width == 64 ? (alias ? 0x4020000000000000UL : 0x4018000000000000UL) : (alias ? 0x41000000UL : 0x40c00000),
                0x5c => alias ? 0 : two,
                0x59 => width == 64 ? (alias ? 0x4030000000000000UL : 0x4020000000000000UL) : (alias ? 0x41800000UL : 0x41000000),
                0x5e => alias ? (width == 64 ? 0x3ff0000000000000UL : 0x3f800000) : two,
                0x5d => alias ? four : two, 0x5f => four,
                _ => alias ? two : (width == 64 ? 0x3ff6a09e667f3bcdUL : 0x3fb504f3)
            };
            // sqrt(2) is inexact; sqrt(4) is exact. This exercises source-only
            // unary semantics separately from destination aliasing.
            FloatRunBits(machine, FloatEncoding(prefix, opcode, alias ? (byte)0xc0 : (byte)0xc1), opcode == 0x51 && !alias ? 0x20u : 0);
            for (int lane = 0; lane < lanes; ++lane) Check(FloatRead(machine, 0, lane, width) == expected, "R70 independent IEEE exact/sqrt bits");
            if (lanes == 1) Check(new ReadOnlySpan<byte>(machine + 160 + width / 8,16 - width / 8).SequenceEqual(upper.AsSpan(width / 8)),
                "R70 legacy scalar high lanes preserved");
        }
        // SQRTPD discriminator: true64-bit reads/writes of BOTH lanes. Selected
        // C's Read32/Write32 defect is not treated as an architectural oracle.
        FloatSeed(machine); FloatWrite(machine,1,0,64,0x4010000000000000); FloatWrite(machine,1,1,64,0x4022000000000000);
        FloatRunBits(machine,[0x66,0x0f,0x51,0xc1]);
        Check(FloatRead(machine,0,0,64)==0x4000000000000000 && FloatRead(machine,0,1,64)==0x4008000000000000,
            "R70 independent packed-double square root corrects selected-C width gap");
    }

    static void CheckFullFloatSpecialBits(byte* machine)
    {
        foreach (int width in new[] { 32,64 })
        {
            byte prefix = width == 64 ? (byte)0xf2 : (byte)0xf3;
            ulong sign = 1UL << (width - 1), one = width == 64 ? 0x3ff0000000000000UL : 0x3f800000,
                qnan = width == 64 ? 0xfff8123456789abcUL : 0xffc12345,
                snan = width == 64 ? 0x7ff0123456789abcUL : 0x7f812345,
                quiet = width == 64 ? 0x8000000000000UL : 0x400000;
            foreach (byte opcode in new byte[] { 0x5d,0x5f })
            foreach (ulong right in new[] { 0UL,sign,qnan,snan })
            {
                FloatSeed(machine); FloatWrite(machine,0,0,width,0); FloatWrite(machine,1,0,width,right);
                FloatRunBits(machine,FloatEncoding(prefix,opcode), right == snan || right == qnan ? 1u : 0);
                Check(FloatRead(machine,0,0,width)==right,"R70 MIN/MAX second-source NaN payload/SNaN/signed-zero selection");
            }
            FloatSeed(machine); FloatWrite(machine,0,0,width,snan); FloatWrite(machine,1,0,width,one);
            FloatRunBits(machine,FloatEncoding(prefix,0x58),1);
            Check(FloatRead(machine,0,0,width)==(snan|quiet),"R70 arithmetic quiets SNaN and preserves payload/sign");
            for (byte predicate=0; predicate<8; ++predicate)
            foreach (bool signaling in new[] { false,true })
            {
                FloatSeed(machine); FloatWrite(machine,0,0,width,one); FloatWrite(machine,1,0,width,signaling?snan:qnan);
                uint status=signaling || predicate is 1 or 2 or 5 or 6 ? 1u : 0;
                FloatRunBits(machine,FloatEncoding(prefix,0xc2,immediate:(byte)(0xf8|predicate)),status);
                bool truth=predicate is 3 or 4 or 5 or 6;
                Check(FloatRead(machine,0,0,width)==(truth?(width==64?ulong.MaxValue:uint.MaxValue):0),
                    "R70 CMP imm low3 predicates include unordered NLT/NLE");
            }
            foreach (byte opcode in new byte[] { 0x2e,0x2f })
            foreach (ulong right in new[] { one,0UL,sign,qnan,snan })
            {
                FloatSeed(machine); FloatWrite(machine,0,0,width,one); FloatWrite(machine,1,0,width,right);
                uint prior=((Blink.Machine*)machine)->flags;
                FloatRunBits(machine,FloatEncoding(width==64?(byte)0x66:(byte)0,opcode),
                    right==snan || opcode==0x2f && right==qnan ? 1u : 0);
                bool unordered=right==qnan || right==snan, equal=right==one;
                uint expected=(prior&~0xff0008d1u) | (unordered?1u:0) | (unordered||equal?64u:0) | (unordered?0u:0x01000000u);
                Check(((Blink.Machine*)machine)->flags==expected,"R70 COMIS/UCOMIS CF/ZF/lazy-PF, clear OF/SF/AF, retain raw PF/other bits");
            }
            FloatSeed(machine,0x1fc0); FloatWrite(machine,0,0,width,one); FloatWrite(machine,1,0,width,sign|1);
            FloatRunBits(machine,FloatEncoding(prefix,0x5e),4);
            Check(FloatRead(machine,0,0,width)==(sign|(width==64?0x7ff0000000000000UL:0x7f800000)),
                "R70 software DAZ preserves negative zero and generates divide-by-zero");
            FloatSeed(machine,0x1f80 & ~0x100u); FloatWrite(machine,0,0,width,one); FloatWrite(machine,1,0,width,1);
            FloatRunBits(machine,FloatEncoding(prefix,0x58),2,true);
            // Old sticky bits do not themselves trigger a new instruction fault.
            FloatSeed(machine,(0x1f80 & ~0x80u)|1); FloatWrite(machine,0,0,width,one); FloatWrite(machine,1,0,width,one);
            FloatRunBits(machine,FloatEncoding(prefix,0x58));
        }
    }

    static void CheckFullFloatRoundingAndRange(byte* machine)
    {
        foreach (int width in new[] { 32,64 })
        {
            byte prefix=width==64?(byte)0xf2:(byte)0xf3;
            ulong one=width==64?0x3ff0000000000000UL:0x3f800000,
                halfUlp=width==64?0x3ca0000000000000UL:0x33800000,
                minNormal=width==64?0x0010000000000000UL:0x00800000,
                half=width==64?0x3fe0000000000000UL:0x3f000000,
                largest=width==64?0x7fefffffffffffffUL:0x7f7fffff,
                two=width==64?0x4000000000000000UL:0x40000000;
            for(uint rc=0;rc<4;++rc)
            {
                FloatSeed(machine,0x1f80|(rc<<13)); FloatWrite(machine,0,0,width,one); FloatWrite(machine,1,0,width,halfUlp);
                FloatRunBits(machine,FloatEncoding(prefix,0x58),0x20);
                Check(FloatRead(machine,0,0,width)==one+(rc==2?1UL:0),"R70 guest rounding independent of seeded HOST RC");
                FloatSeed(machine,(0x1f80 & ~0x1000u)|(rc<<13)); FloatWrite(machine,0,0,width,one); FloatWrite(machine,1,0,width,halfUlp);
                FloatRunBits(machine,FloatEncoding(prefix,0x58),0x20,true);
            }
            foreach(bool ftz in new[]{false,true})
            foreach(bool mask in new[]{false,true})
            {
                FloatSeed(machine,(mask?0x1f80u:0x1780u)|(ftz?0x8000u:0));
                FloatWrite(machine,0,0,width,minNormal); FloatWrite(machine,1,0,width,half);
                FloatRunBits(machine,FloatEncoding(prefix,0x59),mask?(ftz?0x30u:0):0x10u,!mask);
                if(mask) Check(FloatRead(machine,0,0,width)==(ftz?0:minNormal>>1),"R70 exact tiny gradual/FTZ response");
            }
            FloatSeed(machine,0x1b80); FloatWrite(machine,0,0,width,largest); FloatWrite(machine,1,0,width,two);
            FloatRunBits(machine,FloatEncoding(prefix,0x59),8,true); // exact unbounded significand, no spurious PE
            FloatSeed(machine,0x1780); FloatWrite(machine,0,0,width,1); FloatWrite(machine,1,0,width,1);
            FloatRunBits(machine,FloatEncoding(prefix,0x59),0x12,true); // minsub*minsub: DE and exact unmasked UE
            FloatSeed(machine,0x1780); FloatWrite(machine,0,0,width,minNormal+1); FloatWrite(machine,1,0,width,half);
            FloatRunBits(machine,FloatEncoding(prefix,0x59),0x10,true); // exact normalized significand despite subnormal rounding
            FloatSeed(machine,0x1b80); FloatWrite(machine,0,0,width,largest); FloatWrite(machine,1,0,width,largest);
            FloatRunBits(machine,FloatEncoding(prefix,0x58),8,true); // ADD exact overflow has no forced PE
            for(uint rc=0;rc<4;++rc)
            foreach(bool negative in new[]{false,true})
            {
                ulong sign=negative?1UL<<(width-1):0;
                FloatSeed(machine,0x1f80|(rc<<13)); FloatWrite(machine,0,0,width,largest|sign); FloatWrite(machine,1,0,width,two);
                FloatRunBits(machine,FloatEncoding(prefix,0x59),0x28);
                bool infinity=rc==0 || rc==1 && negative || rc==2 && !negative;
                Check(FloatRead(machine,0,0,width)==(sign|(infinity?(width==64?0x7ff0000000000000UL:0x7f800000):largest)),
                    "R70 masked overflow sign/RC selects infinity or largest finite");
            }
        }
    }

    static void CheckFullFloatConversions(byte* machine)
    {
        foreach(bool dbl in new[]{false,true})
        foreach(bool wide in new[]{false,true})
        foreach(bool truncate in new[]{false,true})
        for(uint rc=0;rc<4;++rc)
        foreach(bool negative in new[]{false,true})
        {
            int width=dbl?64:32; byte prefix=dbl?(byte)0xf2:(byte)0xf3;
            FloatSeed(machine,0x1f80|(rc<<13));
            FloatWrite(machine,1,0,width,(dbl?0x3ff8000000000000UL:0x3fc00000)|(negative?1UL<<(width-1):0));
            FloatRunBits(machine,FloatEncoding(prefix,truncate?(byte)0x2c:(byte)0x2d,wide:wide),0x20);
            long expected=truncate?1:rc==0?2:rc==1?(negative?2:1):rc==2?(negative?1:2):1;
            if(negative) expected=-expected;
            Check(((ulong*)(machine+24))[0]==(wide?unchecked((ulong)expected):unchecked((uint)expected)),"R70 scalar signed conversion32/64 RC/truncation and GPR32 clearing");
        }
        foreach(bool wide in new[]{false,true})
        foreach(ulong bits in new[]{0x7ff0000000000000UL,0xfff0000000000000UL,0x7ff8123456789abcUL,0x43e0000000000000UL})
        {
            FloatSeed(machine); FloatWrite(machine,1,0,64,bits);
            FloatRunBits(machine,FloatEncoding(0xf2,0x2d,wide:wide),1);
            Check(((ulong*)(machine+24))[0]==(wide?0x8000000000000000UL:0x80000000),"R70 masked invalid conversion indefinite");
            FloatSeed(machine,0x1f00); FloatWrite(machine,1,0,64,bits);
            FloatRunBits(machine,FloatEncoding(0xf2,0x2d,wide:wide),1,true);
        }
        for(uint rc=0;rc<4;++rc)
        {
            FloatSeed(machine,0x1f80|(rc<<13)); Write64(machine,32,16777217);
            FloatRunBits(machine,FloatEncoding(0xf3,0x2a),0x20);
            Check(FloatRead(machine,0,0,32)==(rc==2?0x4b800001UL:0x4b800000),"R70 integer-to-single guest RC/PE");
        }
        foreach(bool daz in new[]{false,true})
        {
            FloatSeed(machine,0x1f80|(daz?0x40u:0)); FloatWrite(machine,1,0,32,1);
            FloatRunBits(machine,[0xf3,0x0f,0x2d,0xc1],daz?0:0x20u);
            Check(((ulong*)(machine+24))[0]==0,"R70 scalar float-to-int denormal never DE; DAZ suppresses PE");
        }
        foreach(byte[] encoding in new byte[][] { [0x0f,0x5b,0xc1],[0xf3,0x0f,0xe6,0xc1],
            [0x66,0x0f,0x5b,0xc1],[0xf3,0x0f,0x5b,0xc1],[0xf2,0x0f,0xe6,0xc1],[0x66,0x0f,0xe6,0xc1],
            [0x0f,0x5a,0xc1],[0x66,0x0f,0x5a,0xc1],[0xf3,0x0f,0x5a,0xc1],[0xf2,0x0f,0x5a,0xc1] })
        {
            FloatSeed(machine);
            var op=FullBlockEmitter.FloatOperation(FloatClassify(encoding));
            bool integers=op is FullBlockEmitter.FullFloatOperation.DwordToSingle or FullBlockEmitter.FullFloatOperation.DwordToDouble;
            bool doubleInput=op is FullBlockEmitter.FullFloatOperation.DoubleToDword or FullBlockEmitter.FullFloatOperation.DoubleToDwordTruncate or FullBlockEmitter.FullFloatOperation.DoubleToSingle;
            int input=doubleInput?64:32;
            for(int lane=0;lane<128/input;++lane) FloatWrite(machine,1,lane,input,integers?2:doubleInput?0x4000000000000000UL:0x40000000);
            FloatRunBits(machine,encoding);
            bool outDouble=op is FullBlockEmitter.FullFloatOperation.SingleToDouble or FullBlockEmitter.FullFloatOperation.DwordToDouble;
            bool outInteger=op is FullBlockEmitter.FullFloatOperation.SingleToDword or FullBlockEmitter.FullFloatOperation.SingleToDwordTruncate or FullBlockEmitter.FullFloatOperation.DoubleToDword or FullBlockEmitter.FullFloatOperation.DoubleToDwordTruncate;
            Check(FloatRead(machine,0,0,outDouble?64:32)==(outInteger?2:outDouble?0x4000000000000000UL:0x40000000),"R70 independent packed/scalar conversion bits");
            if(op is FullBlockEmitter.FullFloatOperation.DoubleToDword or FullBlockEmitter.FullFloatOperation.DoubleToDwordTruncate ||
                op==FullBlockEmitter.FullFloatOperation.DoubleToSingle && (FloatClassify(encoding).Options&FullBlockEmitter.FloatScalar)==0)
                Check(((ulong*)(machine+168))[0]==0,"R70 packed narrowing architectural high64 zero (selected-C gap)");
        }
        FloatSeed(machine,0x1780); FloatWrite(machine,1,0,64,0x3800000000000000); // 2^-127, exact float subnormal
        FloatRunBits(machine,[0xf2,0x0f,0x5a,0xc1],0x10,true);
        FloatSeed(machine,0x1b80); FloatWrite(machine,1,0,64,0x47f0000000000000); // 2^128, exact unbounded float
        FloatRunBits(machine,[0xf2,0x0f,0x5a,0xc1],8,true);
    }

    static void CheckFullFloatPackedFaultCommit(byte* machine)
    {
        foreach(byte prefix in new byte[]{0,0x66})
        {
            int width=prefix==0?32:64, lanes=128/width;
            FloatSeed(machine,0x1d80); // ZE unmasked
            for(int lane=0;lane<lanes;++lane)
            { FloatWrite(machine,0,lane,width,width==64?0x3ff0000000000000UL:0x3f800000); FloatWrite(machine,1,lane,width,lane==lanes-1?0:width==64?0x4000000000000000UL:0x40000000); }
            FloatRunBits(machine,FloatEncoding(prefix,0x5e),4,true);
            // Pre-computation unmasked DE suppresses another lane's post PE.
            FloatSeed(machine,0x1e80);
            FloatWrite(machine,0,0,width,width==64?0x3ff0000000000000UL:0x3f800000);
            FloatWrite(machine,1,0,width,width==64?0x3ca0000000000000UL:0x33800000);
            FloatWrite(machine,0,1,width,1); FloatWrite(machine,1,1,width,width==64?0x3ff0000000000000UL:0x3f800000);
            if(width==32) for(int lane=2;lane<4;++lane) { FloatWrite(machine,0,lane,32,0); FloatWrite(machine,1,lane,32,0); }
            FloatRunBits(machine,FloatEncoding(prefix,0x58),2,true);
            FloatSeed(machine,0x1380); // O/U both unmasked, DE/PE masked
            FloatWrite(machine,0,0,width,width==64?0x7fefffffffffffffUL:0x7f7fffff);
            FloatWrite(machine,1,0,width,width==64?0x4000000000000000UL:0x40000000);
            FloatWrite(machine,0,1,width,width==64?0x0010000000000000UL:0x00800000);
            FloatWrite(machine,1,1,width,width==64?0x3fe0000000000000UL:0x3f000000);
            if(width==32) for(int lane=2;lane<4;++lane) { FloatWrite(machine,0,lane,32,0); FloatWrite(machine,1,lane,32,0); }
            FloatRunBits(machine,FloatEncoding(prefix,0x59),0x18,true);
        }
    }

    static void CheckFullFloatMemoryFaults(byte* machine)
    {
        byte* allocation=(byte*)NativeMemory.Alloc(12288), first=allocation, second=allocation+8192;
        try
        {
            const ulong guest=0x200000;
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(byte prefix in new byte[]{0,0x66,0xf2,0xf3})
            foreach(bool missing in new[]{false,true})
            {
                FloatSeed(machine); int width=prefix is 0x66 or 0xf2?64:32;
                int sourceWidth=prefix is 0xf2 or 0xf3?width:128;
                for(int lane=0;lane<128/width;++lane) FloatWrite(machine,0,lane,width,width==64?0x3ff0000000000000UL:0x3f800000);
                new Span<byte>(allocation,12288).Clear();
                for(int b=0;b<sourceWidth/8;++b)
                { byte value=(byte)((width==64?0x4000000000000000UL:0x40000000)>>(8*(b%(width/8)))); (b==0?first+4095:second+b-1)[0]=value; }
                Write64(machine,48,guest+4095); new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                SetFullMemoryEntry(tlb,guest,first); if(!missing) SetFullMemoryEntry(tlb,guest+4096,second);
                using var code=CreateFullBlock([new(FloatClassify(FloatEncoding(prefix,0x58,0x03)))]);
                ulong epoch=11; long issued=0,completed=0; int stop=0; var header=FullHeader(Ip,code.Entry);
                var frame=FullFrame(machine,&header,&epoch,&issued,&completed,&stop); frame.MemoryTlb=tlb; frame.MemoryTlbMask=63;
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                var exit=FloatRunProbe(code,&frame);
                if(missing)
                {
                    Check(exit==FullBlockExit.NeedMemory && issued==1 && completed==0 && frame.HostMxcsrDirty==0 && frame.HostMxcsrSaved==0 &&
                        new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R70 second-page mapping miss BEFORE host-environment setup or architectural effects");
                    SetFullMemoryEntry(tlb,guest+4096,second); FullNativeMemory.ResumeSameIssue(&frame);
                    Check(FloatRunProbe(code,&frame)==FullBlockExit.Continue,"R70 same-approved-issue float resume");
                }
                else Check(exit==FullBlockExit.Continue,"R70 disjoint crosspage float read");
                Check(issued==1 && completed==1 && frame.NativeMemoryComputations==1,"R70 float memory issue/commit exactly once");
                Check(FloatRead(machine,0,0,width)==(width==64?0x4008000000000000UL:0x40400000),"R70 gathered operand independent arithmetic");
                Check(new ReadOnlySpan<byte>(allocation+4096,4096).IndexOfAnyExcept((byte)0)<0,"R70 unchecked contiguous host gap not accessed");
            }
        }
        finally { NativeMemory.Free(allocation); }
    }

    static void CheckFullFloatRestoreExits(byte* machine)
    {
        foreach(FullBlockExit expected in Enum.GetValues<FullBlockExit>())
        {
            if (expected == FullBlockExit.RepYield)
            {
                // A real native FP predecessor chains to the guarded REP
                // latch. Ordinary FP-only IR cannot produce this new exit.
                CheckRepLatchRestoreAfterFloat(machine);
                continue;
            }
            FloatSeed(machine,expected==FullBlockExit.GuestFault?0x1d80u:0x1f80u);
            FloatWrite(machine,0,0,32,0x3f800000); FloatWrite(machine,1,0,32,0x33800000);
            var first=FloatClassify([0xf3,0x0f,0x58,0xc1]);
            ulong next=Ip+(uint)first.Length;
            var body=new List<FullBlockInstruction>{new(first)};
            if(expected==FullBlockExit.NeedHelper) body.Add(FullBlockInstruction.Helper(next,1));
            else if(expected==FullBlockExit.NeedMemory) body.Add(new(FloatClassify([0xf3,0x0f,0x58,0x03]) with {Ip=next}));
            else if(expected==FullBlockExit.GuestFault)
            {
                FloatWrite(machine,2,0,32,0);
                body.Add(new(FloatClassify([0xf3,0x0f,0x5e,0xc2]) with {Ip=next}));
            }
            else if(expected is FullBlockExit.Stop or FullBlockExit.Attention or FullBlockExit.Invalidated)
            {
                body.Add(new(new(next,3,JitOperation.Mov,8,new(0,0,IsMemory:true),new(0,1,true),Address:new(3,-1,0,0))));
                body.Add(new(new(next+3,1,JitOperation.Nop,64,default,default)));
            }
            else if(expected==FullBlockExit.Budget) body.Add(new(new(next,1,JitOperation.Nop,64,default,default)));
            using var code=CreateFullBlock(body.ToArray());
            ulong epoch=11; long issued=0,completed=0; int stop=0;
            var header=FullHeader(Ip,code.Entry); var frame=FullFrame(machine,&header,&epoch,&issued,&completed,&stop,budget:expected==FullBlockExit.Budget?1UL:100UL);
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            const ulong guest=0x200000; Write64(machine,48,guest);
            if(expected is FullBlockExit.Stop or FullBlockExit.Attention or FullBlockExit.Invalidated)
            {
                // Synthetic mapping of a guarded control byte changes only at
                // the native memory instruction AFTER FP made the host dirty.
                byte* target=expected==FullBlockExit.Stop?(byte*)&stop:expected==FullBlockExit.Attention?machine+11:(byte*)&epoch;
                SetFullMemoryEntry(tlb,guest,target);
            }
            frame.MemoryTlb=tlb; frame.MemoryTlbMask=63;
            Check(FloatRunProbe(code,&frame)==expected,"R70 every typed exit restores original HOST environment after prior FP");
            Check(frame.HostMxcsrSaved==0x5fa1 && frame.HostMxcsrDirty==0,"R70 shared/standalone saved host state retained and dirty cleared");
            if(expected==FullBlockExit.GuestFault) Check(frame.FaultTrap==19 && completed==1 && issued==2,"R70 sticky-before-trap and previous FP commit retained");
            if(expected==FullBlockExit.Budget) Check(completed==1 && issued==2,"R70 rejected next issue keeps completed FP only");
        }
        // Poll/budget before the first FP must leave the host-save slot untouched.
        foreach(FullBlockExit expected in new[]{FullBlockExit.Stop,FullBlockExit.Attention,FullBlockExit.Budget,FullBlockExit.Invalidated})
        {
            FloatSeed(machine); using var code=CreateFullBlock([new(FloatClassify([0xf3,0x0f,0x58,0xc1]))]);
            ulong epoch=11; long issued=0,completed=0; int stop=expected==FullBlockExit.Stop?1:0;
            var header=FullHeader(Ip,code.Entry); if(expected==FullBlockExit.Invalidated) header.Live=0;
            if(expected==FullBlockExit.Attention) machine[11]=1;
            var frame=FullFrame(machine,&header,&epoch,&issued,&completed,&stop,budget:expected==FullBlockExit.Budget?0UL:100UL);
            Check(FloatRunProbe(code,&frame)==expected && frame.HostMxcsrSaved==0 && frame.HostMxcsrDirty==0 && completed==0,
                "R70 pre-FP boundary has no host FP setup/effects");
        }
    }

    static void FloatReference(Blink program, byte* machine, ReadOnlySpan<byte> encoding)
    {
        Blink.XedDecodedInst decoded=default;
        fixed(byte* bytes=encoding)
            Check(program.DecodeInstruction(&decoded,bytes,(ulong)encoding.Length,Blink.XED_MODE_LONG)==0 && decoded.length==encoding.Length,
                "R70 authoritative XED oracle length");
        var m=(Blink.Machine*)machine; var previous=m->xedd;
        m->xedd=&decoded; m->oplen=(byte)encoding.Length; m->ip+=(uint)encoding.Length;
        try
        {
            var op=decoded.op;
            program.GetOp((long)(op.rde>>40&2047))(program,m,op.rde,op.disp,op.uimm0);
            if(m->stashaddr!=0) program.CommitStash(m); m->oplen=0;
        }
        finally {m->xedd=previous;}
    }

    static byte[][] FloatNormalOracleEncodings() => [
        [0x0f,0x58,0xc1],[0x66,0x0f,0x59,0xc1],[0xf2,0x0f,0x5c,0xc1],[0xf3,0x0f,0x5e,0xc1],
        [0x0f,0x5d,0xc1],[0x66,0x0f,0x5f,0xc1],[0xf2,0x0f,0x51,0xc1],[0xf3,0x0f,0x51,0xc1],
        [0xf3,0x0f,0x2d,0xc1],[0xf2,0x48,0x0f,0x2c,0xc1],[0xf3,0x0f,0x2a,0xc1],
        [0xf2,0x48,0x0f,0x2a,0xc1],[0x0f,0x5a,0xc1],[0xf3,0x0f,0x5a,0xc1],
        [0xf2,0x0f,0x5a,0xc1],[0x0f,0x5b,0xc1],[0xf3,0x0f,0xe6,0xc1],
        [0x66,0x0f,0x5b,0xc1],[0xf3,0x0f,0x5b,0xc1],[0x0f,0x2e,0xc1],[0x66,0x0f,0x2f,0xc1],
        [0x0f,0xc2,0xc1,0],[0x66,0x0f,0xc2,0xc1,7],[0xf2,0x0f,0xc2,0xc1,2],[0xf3,0x0f,0xc2,0xc1,4] ];

    static void FloatSeedOracle(byte* machine, JitInstruction i)
    {
        FloatSeed(machine,reset:false);
        var op=FullBlockEmitter.FloatOperation(i);
        bool integerInput=op is FullBlockEmitter.FullFloatOperation.IntegerToScalar or FullBlockEmitter.FullFloatOperation.DwordToSingle or FullBlockEmitter.FullFloatOperation.DwordToDouble;
        bool dbl=(i.Options&FullBlockEmitter.FloatDouble)!=0 || op==FullBlockEmitter.FullFloatOperation.DoubleToSingle;
        int width=dbl?64:32;
        for(int lane=0;lane<128/width;++lane)
        { FloatWrite(machine,0,lane,width,dbl?0x4010000000000000UL:0x40800000); FloatWrite(machine,1,lane,width,integerInput?2:dbl?0x4010000000000000UL:0x40800000); }
        if(op==FullBlockEmitter.FullFloatOperation.IntegerToScalar) Write64(machine,32,2);
    }

    static void FloatCompareOracle(ThreadedGuestExecution.FullControllerFixture fixture, byte[] encoding, bool memory)
    {
        byte* machine=(byte*)fixture.Machine();
        var i=FloatClassify(encoding);
        byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
        if(memory) fixture.DataScope(0,()=>FloatReference(fixture.Program,(byte*)fixture.Machine(),encoding));
        else FloatReference(fixture.Program,machine,encoding);
        byte[] expected=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
        before.CopyTo(new Span<byte>(machine,before.Length));
        using var code=CreateFullBlock([new(i)]);
        ulong epoch=11; long issued=0,completed=0; int stop=0; var header=FullHeader(Ip,code.Entry);
        var frame=FullFrame(machine,&header,&epoch,&issued,&completed,&stop);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        if(memory)
        {
            const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
            SetFullMemoryEntry(tlb,guest,(byte*)fixture.Physical(guest)); SetFullMemoryEntry(tlb,guest+4096,(byte*)fixture.Physical(guest+4096));
            frame.MemoryTlb=tlb; frame.MemoryTlbMask=63;
        }
        Check(FloatRunProbe(code,&frame)==FullBlockExit.Continue && issued==1 && completed==1 && frame.NativeMemoryComputations==(memory?1UL:0),
            "R70 selected normal oracle native issue/commit");
        Check(new ReadOnlySpan<byte>(machine,9).SequenceEqual(expected.AsSpan(0,9)) &&
            new ReadOnlySpan<byte>(machine+12,4).SequenceEqual(expected.AsSpan(12,4)) &&
            new ReadOnlySpan<byte>(machine+24,392).SequenceEqual(expected.AsSpan(24,392)) &&
            new ReadOnlySpan<byte>(machine+584,108).SequenceEqual(expected.AsSpan(584,108)),
            $"R70 selected normal oracle {Convert.ToHexString(encoding)} (known C gaps use independent tests)");
    }

    static void CheckFullFloatOracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        foreach(byte[] encoding in FloatNormalOracleEncodings())
        { FloatSeedOracle((byte*)fixture.Machine(),FloatClassify(encoding)); FloatCompareOracle(fixture,encoding,false); }
    }

    static void CheckFullFloatMemoryOracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(guest,8192); fixture.Fill(guest,8192);
        byte* first=(byte*)fixture.Physical(guest),second=(byte*)fixture.Physical(guest+4096),machine=(byte*)fixture.Machine();
        foreach(byte[] register in FloatNormalOracleEncodings())
        foreach(int offset in new[]{64,4095})
        {
            byte[] encoding=(byte[])register.Clone();
            int modrm=Array.IndexOf(encoding,(byte)0x0f)+2; encoding[modrm]=0x03;
            var i=FloatClassify(encoding); FloatSeedOracle(machine,i);
            Write64(machine,48,guest+(uint)offset);
            // Source bytes are exactly the register-oracle operand, except GPR
            // integer inputs which use the corresponding integer's bytes.
            bool integer=FullBlockEmitter.FloatOperation(i)==FullBlockEmitter.FullFloatOperation.IntegerToScalar;
            for(int b=0;b<i.SourceWidth/8;++b)
            { byte value=integer?(b==0?(byte)2:(byte)0):machine[176+b]; (offset+b<4096?first+offset+b:second+offset+b-4096)[0]=value; }
            FloatCompareOracle(fixture,encoding,true); // owns fixture.DataScope
        }
        var request=new FullBlockFrame{MemoryAddress=guest+8191,MemoryWidth=128,MemoryAccess=FullMemoryAccess.Read,MemoryResume=1};
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear(); request.MemoryTlb=tlb; request.MemoryTlbMask=63;
        Check(FillFullMemoryMmu(fixture,&request)==FullMemoryAssistResult.GuestFault && request.FaultTrap==14 && request.FaultAddress==guest+8192,
            "R70 real MMU second-page PF14 before float effects");
    }

    static void CheckFullFloatClassifier()
    {
        foreach(byte prefix in new byte[]{0,0x66,0xf2,0xf3})
        foreach(byte opcode in new byte[]{0x58,0x5c,0x59,0x5e,0x5d,0x5f,0x51,0xc2})
        foreach(bool memory in new[]{false,true})
            FloatClassify(FloatEncoding(prefix,opcode,memory?(byte)0x03:(byte)0xc1,opcode==0xc2?(byte?)0xff:null));
        foreach(byte[] encoding in FloatNormalOracleEncodings()) FloatClassify(encoding);
        var ea=FloatClassify([0x64,0x67,0xf3,0x45,0x0f,0x58,0x54,0x8b,0xf8]);
        Check(ea.Destination.Register==10 && ea.Address==new JitAddress(11,1,2,-8,false,32,4),"R70 exact REX/FS/EA32/SIB metadata");
        foreach(byte[] encoding in new byte[][] { [],[0xf3],[0xf3,0x0f,0x58],[0xf3,0xf3,0x0f,0x58,0xc1],
            [0xf0,0xf3,0x0f,0x58,0x03],[0x48,0xf3,0x0f,0x58,0xc1],[0xf3,0x48,0x0f,0x58,0xc1],
            [0x0f,0x2a,0xc1],[0x66,0x0f,0x2d,0xc1],[0x0f,0x52,0xc1],[0xf3,0x0f,0x53,0xc1],
            [0xf2,0x0f,0x5b,0xc1],[0x0f,0xe6,0xc1],[0x0f,0xc2,0xc1],
            [0xf3,0x0f,0x58,0xc1,0x90],[0x67,0xf3,0x0f,0x58,0xc1],[0xf3,0x40,0x0f,0x58,0xc1],
            [0xf3,0x41,0x0f,0x58,0x05,0,0,0,0],[0xc5,0xf8,0x58,0xc1],
            [0x0f,0xae,0x13],[0xd9,0xe8],[0x0f,0x77] })
            Check(!FullFloatLowering.TryClassify(Ip,encoding,out _,out var reason) && reason!=null,"R70 explicit form-specific MISSING");
        var valid=FloatClassify([0xf3,0x0f,0x58,0xc1]);
        foreach(var bad in new[]{valid with{Width=64},valid with{Options=uint.MaxValue},valid with{Destination=new(16,0)},
            valid with{SourceWidth=128},valid with{Extra=new(0,1,true)},valid with{Source=new(0,0,ByteOffset:1)}})
        {
            bool rejected=false; try{FullBlockEmitter.ValidateFloat(bad);}catch(ArgumentException){rejected=true;}
            Check(rejected,"R70 malformed typed IR rejected before publication");
        }
    }

    static void CheckFullFloatSpanAllocation()
    {
        FullBlockInstruction[] body=[new(FloatClassify([0x66,0x0f,0x59,0xc1]))];
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool windows in new[]{false,true})
        {
            for(int warm=0;warm<10;++warm) Check(FullBlockEmitter.TryEmit(body,staging,out _,windows),"R70 warm span emission");
            long before=GC.GetAllocatedBytesForCurrentThread(); bool good=true;
            for(int repeat=0;repeat<32;++repeat) good&=FullBlockEmitter.TryEmit(body,staging,out int length,windows) && length>0;
            Check(good && GC.GetAllocatedBytesForCurrentThread()==before,"R70 both ABI warmed zero-allocation span");
            Check(staging.AsSpan().IndexOf(new byte[]{0xf2,0x0f,0x59,0xdc})>=0,"R70 actual baseline SSE2 scalarized packed computation");
            Check(staging.AsSpan().IndexOf(new byte[]{0x41,0x0f,0xae,0x92,0x60,1,0,0})>=0,"R70 original host MXCSR restore instruction present");
            Check(!FullBlockEmitter.TryEmit(body,new byte[11],out int tiny,windows) && tiny==0,"R70 oversized staging never publishes partial code");
        }
    }
}
