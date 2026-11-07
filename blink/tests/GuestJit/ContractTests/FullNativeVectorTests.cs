using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // R66: authored UNRUN. Parent wires this entry and owns serial builds,
    // emission, native execution, producer selection and qualification.
    static void CheckFullNativeVector(byte* machine)
    {
        Check(FullBlockEmitter.HasExpectedVectorLayout(), "R66 exact generated XMM/frame offsets");
        CheckFullVectorClassifier();
        using var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle: true);
        CheckFullVectorOracle(fixture);
        CheckFullVectorMemoryOracle(fixture);
        CheckFullVectorIndependent(machine);
        CheckFullVectorMemoryBoundaries(machine);
        CheckFullVectorIssueBoundaries(machine);
        CheckFullVectorSpanAllocation();
    }

    static byte[] VectorBinaryOpcodes() => [
        0xdb,0xdf,0xeb,0xef,0xfc,0xfd,0xfe,0xd4,0xf8,0xf9,0xfa,0xfb,
        0xec,0xed,0xdc,0xdd,0xe8,0xe9,0xd8,0xd9,0x74,0x75,0x76,0x64,0x65,0x66,
        0xda,0xde,0xea,0xee,0x63,0x6b,0x67,0x60,0x61,0x62,0x6c,0x68,0x69,0x6a,0x6d,
        0xd5,0xe5,0xe4,0xf4,0xf5,0xf6,0xe0,0xe3,0xd1,0xd2,0xd3,0xe1,0xe2,0xf1,0xf2,0xf3 ];

    static void VectorSeed(byte* machine)
    {
        // Preserve a real fixture's system/bus/pagetables. Raw leaf tests first
        // ResetFull their synthetic machine; the oracle machine is never reset.
        var m = (Blink.Machine*)machine;
        m->ip = Ip; m->oplen = 0; m->attention = false; m->stashaddr = 0;
        m->mode.omode = Blink.XED_MODE_LONG; m->mode.genmode = Blink.XED_GEN_MODE_PROTECTED;
        m->flags = 0xa52c0e15u & ~0x50100u;
        m->mxcsr = 0x9fc0; // integer operations must preserve MXCSR exactly
        for (int r = 0; r < 16; ++r)
        {
            Write64(machine, 24 + r * 8, 0xfedcba9876543210UL ^ ((uint)r * 0x0101010101010101UL));
            for (int b = 0; b < 16; ++b) machine[160 + r * 16 + b] = (byte)(b * 43 + r * 29);
        }
    }

    static JitInstruction VectorClassify(ReadOnlySpan<byte> bytes)
    {
        Check(FullVectorLowering.TryClassify(Ip, bytes, out var i, out string? reason), reason ?? "R66 classifier");
        Check(FullBlockEmitter.ValidateVector(i), "R66 exact typed vector validation");
        return i;
    }

    // Actual selected normal interpreter operation, after authoritative XED.
    // GetOp is confined to this test oracle; product files contain no dispatch.
    static void VectorReference(Blink program, byte* machine, ReadOnlySpan<byte> encoding)
    {
        Blink.XedDecodedInst decoded = default;
        fixed (byte* bytes = encoding)
            Check(program.DecodeInstruction(&decoded, bytes, (ulong)encoding.Length, Blink.XED_MODE_LONG) == 0 &&
                decoded.length == encoding.Length, "R66 authoritative oracle length");
        var m = (Blink.Machine*)machine;
        var previous = m->xedd;
        m->xedd = &decoded; m->oplen = (byte)encoding.Length; m->ip += (uint)encoding.Length;
        try
        {
            var op = decoded.op;
            program.GetOp((long)(op.rde >> 40 & 2047))(program, m, op.rde, op.disp, op.uimm0);
            if (m->stashaddr != 0) program.CommitStash(m);
            m->oplen = 0;
        }
        finally { m->xedd = previous; }
    }

    static void VectorCheckArchitecture(byte* machine, byte[] expected, string label)
    {
        Check(new ReadOnlySpan<byte>(machine, 9).SequenceEqual(expected.AsSpan(0, 9)) &&
            new ReadOnlySpan<byte>(machine + 12, 4).SequenceEqual(expected.AsSpan(12, 4)) &&
            new ReadOnlySpan<byte>(machine + 24, 128).SequenceEqual(expected.AsSpan(24, 128)) &&
            new ReadOnlySpan<byte>(machine + 160, 256).SequenceEqual(expected.AsSpan(160, 256)) &&
            new ReadOnlySpan<byte>(machine + 456, 236).SequenceEqual(expected.AsSpan(456, 236)), label);
    }

    static void VectorCompare(Blink program, byte* machine, byte[] encoding)
    {
        var instruction = VectorClassify(encoding);
        byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
        VectorReference(program, machine, encoding);
        byte[] expected = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
        before.CopyTo(new Span<byte>(machine, before.Length));
        using var code = CreateFullBlock([new(instruction)]);
        ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
        var header = FullHeader(Ip, code.Entry);
        var frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stop);
        Check(RunFull(code, &frame) == FullBlockExit.Continue && issued == 1 && completed == 1 &&
            frame.Executed == 1 && frame.NativeMemoryComputations == 0, "R66 generated register computation/commit");
        VectorCheckArchitecture(machine, expected, $"R66 selected oracle {Convert.ToHexString(encoding)}");
    }

    static void CheckFullVectorOracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        byte* machine = (byte*)fixture.Machine();
        foreach (byte opcode in VectorBinaryOpcodes())
        foreach (byte modrm in new byte[] { 0xc1, 0xc0, 0xd3, 0xff })
        {
            VectorSeed(machine); VectorCompare(fixture.Program, machine, [0x66,0x0f,opcode,modrm]);
            VectorSeed(machine); VectorCompare(fixture.Program, machine, [0x66,0x45,0x0f,opcode,modrm]);
        }
        foreach (byte opcode in new byte[] { 0xd1,0xd2,0xd3,0xe1,0xe2,0xf1,0xf2,0xf3 })
        foreach (ulong count in new ulong[] { 0,1,15,16,31,32,63,64,255,0x100000000,ulong.MaxValue })
        foreach (bool alias in new[] { false, true })
        {
            VectorSeed(machine);
            int source = alias ? 0 : 1;
            Write64(machine, 160 + source * 16, count);
            Write64(machine, 168 + source * 16, 0x8765432101234567);
            VectorCompare(fixture.Program, machine, [0x66,0x0f,opcode,alias ? (byte)0xc0 : (byte)0xc1]);
        }
        foreach (byte opcode in new byte[] { 0x71,0x72,0x73 })
        foreach (int group in new[] { 2,3,4,6,7 })
        foreach (byte count in new byte[] { 0,1,7,8,15,16,17,31,32,63,64,255 })
        {
            if (opcode != 0x73 && group is 3 or 7 || opcode == 0x73 && group == 4) continue;
            VectorSeed(machine);
            VectorCompare(fixture.Program, machine, [0x66,0x41,0x0f,opcode,(byte)(0xc0 | group << 3 | 3),count]);
        }
        foreach (byte prefix in new byte[] { 0x66,0xf2,0xf3 })
        foreach (byte imm in new byte[] { 0,0x1b,0xe4,0xff })
        foreach (byte modrm in new byte[] { 0xc0,0xc1 })
        {
            VectorSeed(machine); VectorCompare(fixture.Program, machine, [prefix,0x0f,0x70,modrm,imm]);
        }
        foreach (byte[] encoding in new byte[][] {
            [0x66,0x45,0x0f,0x6f,0xd3], [0xf3,0x45,0x0f,0x7f,0xd3],
            [0x0f,0x10,0xc1], [0x66,0x0f,0x11,0xc1], [0x0f,0x28,0xc1], [0x66,0x0f,0x29,0xc1],
            [0x66,0x0f,0x6e,0xc3], [0x66,0x48,0x0f,0x6e,0xc3],
            [0x66,0x0f,0x7e,0xc3], [0x66,0x48,0x0f,0x7e,0xc3],
            [0x66,0x4d,0x0f,0x6e,0xd3], [0x66,0x4d,0x0f,0x7e,0xd3],
            [0xf3,0x0f,0x7e,0xc1], [0x66,0x0f,0xd6,0xc1], [0x66,0x0f,0xd6,0xc0],
            [0x66,0x0f,0xd7,0xc1], [0x0f,0x50,0xc1], [0x66,0x0f,0x50,0xc1],
            [0x66,0x0f,0xc4,0xc3,0xff], [0x66,0x0f,0xc5,0xc1,0xff],
            [0x0f,0x54,0xc1], [0x66,0x0f,0x55,0xc1], [0x0f,0x56,0xc1], [0x66,0x0f,0x57,0xc1],
            [0x0f,0xc6,0xc1,0x1b], [0x66,0x0f,0xc6,0xc0,0xff] })
        { VectorSeed(machine); VectorCompare(fixture.Program, machine, encoding); }
        // Selected C dispatch/kernels do not consult CR0.TS/EM or CR4.OSFXSR.
        // Match that existing semantic choice; do not install host control state.
        var system = fixture.Machine()->blink_host_system;
        ulong previousCr0 = system->cr0;
        try
        {
            foreach (ulong bits in new ulong[] { 0,4,8,12 })
            {
                system->cr0 = previousCr0 | bits;
                VectorSeed(machine); VectorCompare(fixture.Program, machine, [0x66,0x0f,0xfc,0xc1]);
            }
        }
        finally { system->cr0 = previousCr0; }
    }

    static void CheckFullVectorMemoryOracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        const ulong guest = ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(guest, 8192); fixture.Fill(guest, 8192);
        byte* first = (byte*)fixture.Physical(guest), second = (byte*)fixture.Physical(guest + 4096);
        byte* machine = (byte*)fixture.Machine();
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        // Parent-owned runtime integration must admit128 and count sixteen
        // bytes. This is intentionally a contract, never a helper computation.
        var request=new FullBlockFrame { MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=guest+4095,
            MemoryWidth=128,MemoryAccess=FullMemoryAccess.Read,MemoryResume=1 };
        Check(FillFullMemoryMmu(fixture,&request)==FullMemoryAssistResult.Resolved && request.MemoryEpoch!=0 &&
            tlb[FullNativeMemory.TlbIndex(guest,63)].Host==first &&
            tlb[FullNativeMemory.TlbIndex(guest+4096,63)].Host==second,
            "R66 runtime width128 both-page admission publishes mappings; native resume computes exact host pointers");
        request.MemoryAddress=guest+8191; request.MemoryResume=1;
        Check(FillFullMemoryMmu(fixture,&request)==FullMemoryAssistResult.GuestFault &&
            request.FaultTrap==14 && request.FaultAddress==guest+8192,"R66 real MMU width128 second-page CPU14 fault");
        var encodings = new List<byte[]>();
        foreach (byte opcode in VectorBinaryOpcodes()) encodings.Add([0x66,0x0f,opcode,0x03]);
        encodings.AddRange(new byte[][] {
            [0xf3,0x0f,0x6f,0x03], [0xf3,0x0f,0x7f,0x03],
            [0x66,0x0f,0x6f,0x03], [0x66,0x0f,0x7f,0x03],
            [0x0f,0x10,0x03], [0x66,0x0f,0x11,0x03], [0x0f,0x28,0x03], [0x66,0x0f,0x29,0x03],
            [0x66,0x0f,0x6e,0x03], [0x66,0x48,0x0f,0x6e,0x03],
            [0x66,0x0f,0x7e,0x03], [0x66,0x48,0x0f,0x7e,0x03],
            [0xf3,0x0f,0x7e,0x03], [0x66,0x0f,0xd6,0x03],
            [0x66,0x0f,0xc4,0x03,5], [0x66,0x0f,0x70,0x03,0x1b],
            [0xf2,0x0f,0x70,0x03,0xe4], [0xf3,0x0f,0x70,0x03,0x1b],
            [0x0f,0xc6,0x03,0x1b], [0x66,0x0f,0xc6,0x03,3],
            [0x0f,0x54,0x03], [0x66,0x0f,0x55,0x03], [0x0f,0x56,0x03], [0x66,0x0f,0x57,0x03] });
        foreach (byte[] encoding in encodings)
        {
            var instruction = VectorClassify(encoding);
            foreach (int offset in (instruction.Options & FullBlockEmitter.VectorAlignedMemory) != 0 ? new[] { 64 } : new[] { 65,4095 })
            {
                VectorSeed(machine); Write64(machine, 48, guest + (uint)offset);
                for (int b = 0; b < 4096; ++b) { first[b] = (byte)(b * 37); second[b] = (byte)(b * 19 + 3); }
                byte[] before = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                byte[] before0 = new ReadOnlySpan<byte>(first,4096).ToArray(), before1 = new ReadOnlySpan<byte>(second,4096).ToArray();
                fixture.DataScope(0, () => VectorReference(fixture.Program, (byte*)fixture.Machine(), encoding));
                byte[] expected = new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                byte[] expected0 = new ReadOnlySpan<byte>(first,4096).ToArray(), expected1 = new ReadOnlySpan<byte>(second,4096).ToArray();
                before.CopyTo(new Span<byte>(machine,before.Length));
                before0.CopyTo(new Span<byte>(first,4096)); before1.CopyTo(new Span<byte>(second,4096));
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                SetFullMemoryEntry(tlb,guest,first); SetFullMemoryEntry(tlb,guest+4096,second);
                using var code = CreateFullBlock([new(instruction)]);
                ulong epoch=11; long issued=0,completed=0; int stop=0; var header=FullHeader(Ip,code.Entry);
                var frame=FullFrame(machine,&header,&epoch,&issued,&completed,&stop);
                frame.MemoryTlb=tlb; frame.MemoryTlbMask=63;
                Check(RunFull(code,&frame)==FullBlockExit.Continue && issued==1 && completed==1 &&
                    frame.NativeMemoryComputations==1,"R66 native memory computation count");
                VectorCheckArchitecture(machine,expected,$"R66 memory oracle {Convert.ToHexString(encoding)}/{offset}");
                Check(new ReadOnlySpan<byte>(first,4096).SequenceEqual(expected0) && new ReadOnlySpan<byte>(second,4096).SequenceEqual(expected1),
                    "R66 oracle exact bytes on both mappings");
            }
        }
    }

    static void VectorRunIndependent(byte* machine, byte[] encoding)
    {
        var i=VectorClassify(encoding);
        using var code=CreateFullBlock([new(i)]);
        uint flags=((uint*)(machine+12))[0], mxcsr=((Blink.Machine*)machine)->mxcsr;
        ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
        Check(RunFull(code,&f)==FullBlockExit.Continue && completed==1 && issued==1 &&
            ((uint*)(machine+12))[0]==flags && ((Blink.Machine*)machine)->mxcsr==mxcsr,"R66 independent native flags/MXCSR/accounting");
    }

    static void CheckFullVectorIndependent(byte* machine)
    {
        // Independent lane arithmetic. Expected math does not use host vectors.
        foreach (int width in new[] { 8,16,32,64 })
        foreach (bool subtract in new[] { false,true })
        {
            ResetFull(machine); VectorSeed(machine);
            int bytes=width/8; ulong mask=width==64 ? ulong.MaxValue : (1UL<<width)-1;
            for(int lane=0;lane<16/bytes;++lane)
            {
                ulong a=mask-(uint)lane, b=(uint)(lane+3);
                for(int k=0;k<bytes;++k) { machine[160+lane*bytes+k]=(byte)(a>>(k*8)); machine[176+lane*bytes+k]=(byte)(b>>(k*8)); }
            }
            byte opcode=(width,subtract) switch { (8,false)=>0xfc,(16,false)=>0xfd,(32,false)=>0xfe,(64,false)=>0xd4,
                (8,true)=>0xf8,(16,true)=>0xf9,(32,true)=>0xfa,_=>0xfb };
            VectorRunIndependent(machine,[0x66,0x0f,opcode,0xc1]);
            for(int lane=0;lane<16/bytes;++lane)
            {
                ulong expected=unchecked(subtract ? mask-(uint)lane-(uint)(lane+3) : mask-(uint)lane+(uint)(lane+3)) & mask;
                for(int k=0;k<bytes;++k) Check(machine[160+lane*bytes+k]==(byte)(expected>>(k*8)),"R66 independent wrapping lane width");
            }
        }
        foreach(int width in new[]{8,16})
        foreach(bool signed in new[]{false,true})
        foreach(bool subtract in new[]{false,true})
        {
            ResetFull(machine); VectorSeed(machine);
            int bytes=width/8, min=signed ? -(1<<(width-1)) : 0, max=signed ? (1<<(width-1))-1 : (1<<width)-1;
            int[] values=[min,max,0,1,-1,max-1,min+1,17];
            byte[] expected=new byte[16];
            for(int lane=0;lane<16/bytes;++lane)
            {
                int a=values[lane%8], b=values[(lane+3)%8];
                if(!signed) { a&=max; b&=max; }
                int result=Math.Clamp(subtract ? a-b : a+b,min,max);
                for(int k=0;k<bytes;++k) { machine[160+lane*bytes+k]=(byte)(a>>(k*8)); machine[176+lane*bytes+k]=(byte)(b>>(k*8)); expected[lane*bytes+k]=(byte)(result>>(k*8)); }
            }
            byte opcode=(width,signed,subtract) switch { (8,true,false)=>0xec,(16,true,false)=>0xed,(8,false,false)=>0xdc,(16,false,false)=>0xdd,
                (8,true,true)=>0xe8,(16,true,true)=>0xe9,(8,false,true)=>0xd8,_=>0xd9 };
            VectorRunIndependent(machine,[0x66,0x0f,opcode,0xc1]);
            Check(new ReadOnlySpan<byte>(machine+160,16).SequenceEqual(expected),"R66 independent signed/unsigned saturation");
        }
        foreach(int width in new[]{8,16,32})
        foreach(bool greater in new[]{false,true})
        {
            ResetFull(machine); VectorSeed(machine); int bytes=width/8;
            long[] values=[0,-1,1,-128,127,-32768,32767,int.MinValue];
            byte[] expected=new byte[16];
            for(int lane=0;lane<16/bytes;++lane)
            {
                long a=values[lane%8], b=values[(lane+1)%8];
                a=width==8?(sbyte)a:width==16?(short)a:(int)a;
                b=width==8?(sbyte)b:width==16?(short)b:(int)b;
                bool selected=greater?a>b:a==b;
                for(int k=0;k<bytes;++k)
                { machine[160+lane*bytes+k]=(byte)(a>>(8*k)); machine[176+lane*bytes+k]=(byte)(b>>(8*k)); expected[lane*bytes+k]=(byte)(selected?255:0); }
            }
            byte opcode=(width,greater) switch { (8,false)=>0x74,(16,false)=>0x75,(32,false)=>0x76,(8,true)=>0x64,(16,true)=>0x65,_=>0x66 };
            VectorRunIndependent(machine,[0x66,0x0f,opcode,0xc1]);
            Check(new ReadOnlySpan<byte>(machine+160,16).SequenceEqual(expected),"R66 independent signed comparisons and all-one lanes");
        }
        foreach(bool signed in new[]{false,true})
        foreach(bool maximum in new[]{false,true})
        {
            ResetFull(machine); VectorSeed(machine); int bytes=signed?2:1;
            byte[] expected=new byte[16];
            for(int lane=0;lane<16/bytes;++lane)
            {
                int a=signed?(lane%2==0?-32768:32767):lane*17, b=signed?lane-3:255-lane*7;
                int value=maximum?Math.Max(a,b):Math.Min(a,b);
                for(int k=0;k<bytes;++k)
                { machine[160+lane*bytes+k]=(byte)(a>>(8*k)); machine[176+lane*bytes+k]=(byte)(b>>(8*k)); expected[lane*bytes+k]=(byte)(value>>(8*k)); }
            }
            byte opcode=signed?(maximum?(byte)0xee:(byte)0xea):(maximum?(byte)0xde:(byte)0xda);
            VectorRunIndependent(machine,[0x66,0x0f,opcode,0xc1]);
            Check(new ReadOnlySpan<byte>(machine+160,16).SequenceEqual(expected),"R66 independent signed/unsigned min/max");
        }
        // Self-source unpack/pack/shuffle, byte-sign mask, exact scalar widths.
        foreach(int laneBytes in new[]{1,2,4,8})
        foreach(bool high in new[]{false,true})
        {
            ResetFull(machine); VectorSeed(machine); byte[] original=new ReadOnlySpan<byte>(machine+160,16).ToArray();
            byte opcode=(laneBytes,high) switch { (1,false)=>0x60,(2,false)=>0x61,(4,false)=>0x62,(8,false)=>0x6c,
                (1,true)=>0x68,(2,true)=>0x69,(4,true)=>0x6a,_=>0x6d };
            VectorRunIndependent(machine,[0x66,0x0f,opcode,0xc0]);
            for(int lane=0;lane<8/laneBytes;++lane)
            for(int k=0;k<laneBytes;++k)
                Check(machine[160+lane*2*laneBytes+k]==original[(high?8:0)+lane*laneBytes+k] &&
                    machine[160+lane*2*laneBytes+laneBytes+k]==original[(high?8:0)+lane*laneBytes+k],"R66 alias unpack lane order");
        }
        ResetFull(machine); VectorSeed(machine);
        for(int lane=0;lane<8;++lane) ((short*)(machine+160+lane*2))[0]=(short)(lane%2==0 ? -300 : 300);
        VectorRunIndependent(machine,[0x66,0x0f,0x67,0xc0]);
        for(int lane=0;lane<16;++lane) Check(machine[160+lane]==(lane%2==0?0:255),"R66 self pack saturation");
        ResetFull(machine); VectorSeed(machine);
        for(int lane=0;lane<4;++lane) ((uint*)(machine+160+lane*4))[0]=(uint)(lane+7);
        VectorRunIndependent(machine,[0x66,0x0f,0x70,0xc0,0x1b]);
        for(int lane=0;lane<4;++lane) Check(((uint*)(machine+160+lane*4))[0]==10-lane,"R66 independent reverse shuffle");
        ResetFull(machine); VectorSeed(machine);
        uint byteMask=0; for(int b=0;b<16;++b) { machine[176+b]=(byte)(b%3==0?0x80:0x7f); if(b%3==0) byteMask|=1u<<b; }
        VectorRunIndependent(machine,[0x66,0x0f,0xd7,0xc1]);
        Check(((ulong*)(machine+24))[0]==byteMask,"R66 movemask bit order and zero upper GPR");
        foreach(bool qword in new[]{false,true})
        {
            ResetFull(machine); VectorSeed(machine); const ulong value=0xfedcba9876543210;
            Write64(machine,48,value);
            VectorRunIndependent(machine,qword ? [0x66,0x48,0x0f,0x6e,0xc3] : [0x66,0x0f,0x6e,0xc3]);
            Check(((ulong*)(machine+160))[0]==(qword?value:unchecked((uint)value)) && ((ulong*)(machine+168))[0]==0,"R66 scalar MOVD/Q zeros remaining vector");
            Write64(machine,0,Ip);
            VectorRunIndependent(machine,qword ? [0x66,0x48,0x0f,0x7e,0xc3] : [0x66,0x0f,0x7e,0xc3]);
            Check(((ulong*)(machine+48))[0]==(qword?value:unchecked((uint)value)),"R66 scalar MOVD/Q exact GPR width");
        }
        ResetFull(machine); VectorSeed(machine); Write64(machine,160,64); Write64(machine,168,0x123456789abcdef0);
        VectorRunIndependent(machine,[0x66,0x0f,0xd3,0xc0]);
        Check(((ulong*)(machine+160))[0]==0 && ((ulong*)(machine+168))[0]==0x123456789abcdef0,
            "R66 selected portable self shift rereads changed count for high half");
        ResetFull(machine); VectorSeed(machine); ulong old=((ulong*)(machine+24))[0]; ((ushort*)(machine+176+14))[0]=0x1234;
        VectorRunIndependent(machine,[0x66,0x0f,0xc5,0xc1,0xff]);
        Check(((ulong*)(machine+24))[0]==((old&~65535UL)|0x1234),"R66 selected PEXTRW preserves upper48");
    }

    static void CheckFullVectorMemoryBoundaries(byte* machine)
    {
        // Deliberately disjoint host mappings with a full intervening page.
        byte* allocation=(byte*)NativeMemory.Alloc(12288), first=allocation, second=allocation+8192;
        try
        {
            const ulong guest=0x200000;
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(bool store in new[]{false,true})
            foreach(int offset in new[]{0,1,4080,4081,4095})
            foreach(string failure in new[]{"none","first","second","permission","epoch","context","watch","second-permission","second-epoch","second-watch"})
            {
                if(failure.StartsWith("second") && offset<=4080 || failure.Contains("watch") && !store) continue;
                ResetFull(machine); VectorSeed(machine); Write64(machine,48,guest+(uint)offset);
                new Span<byte>(allocation,12288).Fill(0x5a);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                SetFullMemoryEntry(tlb,guest,first); SetFullMemoryEntry(tlb,guest+4096,second);
                ref var entry=ref tlb[FullNativeMemory.TlbIndex(guest,63)];
                if(failure=="first") entry=default;
                if(failure=="second") tlb[FullNativeMemory.TlbIndex(guest+4096,63)]=default;
                if(failure=="permission") entry.Permissions=store?FullMemoryAccess.Read:FullMemoryAccess.Write;
                if(failure=="epoch") entry.AuthorityEpoch=10;
                if(failure=="context") entry.Context=0;
                if(failure=="watch") entry.Flags=FullMemoryFlags.WriteBarrier;
                ref var secondEntry=ref tlb[FullNativeMemory.TlbIndex(guest+4096,63)];
                if(failure=="second-permission") secondEntry.Permissions=store?FullMemoryAccess.Read:FullMemoryAccess.Write;
                if(failure=="second-epoch") secondEntry.AuthorityEpoch=10;
                if(failure=="second-watch") secondEntry.Flags=FullMemoryFlags.WriteBarrier;
                var i=VectorClassify([0xf3,0x0f,store?(byte)0x7f:(byte)0x6f,0x03]);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                using var code=CreateFullBlock([new(i)]);
                ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
                var frame=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
                frame.MemoryTlb=tlb; frame.MemoryTlbMask=63;
                FullBlockExit exit=RunFull(code,&frame);
                if(failure!="none")
                {
                    Check(exit==FullBlockExit.NeedMemory && issued==1 && completed==0 && frame.Executed==0 &&
                        frame.MemoryWidth==128 && frame.MemoryResume==1,"R66 width128 precise miss request");
                    Check(new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(before) &&
                        new ReadOnlySpan<byte>(allocation,12288).IndexOfAnyExcept((byte)0x5a)<0,"R66 miss has no partial CPU or either-page effects");
                    SetFullMemoryEntry(tlb,guest,first); SetFullMemoryEntry(tlb,guest+4096,second);
                    FullNativeMemory.ResumeSameIssue(&frame);
                    Check(RunFull(code,&frame)==FullBlockExit.Continue,"R66 same approved issue vector resume");
                }
                else Check(exit==FullBlockExit.Continue,"R66 disjoint crossing native success");
                Check(issued==1 && completed==1 && frame.NativeMemoryComputations==1 && frame.Executed==1,
                    "R66 exactly one issue/commit/memory computation");
                for(int b=0;b<16;++b)
                {
                    byte* address=offset+b<4096?first+offset+b:second+offset+b-4096;
                    Check(store ? *address==before[160+b] : machine[160+b]==0x5a,"R66 gather/scatter byte order");
                }
                Check(new ReadOnlySpan<byte>(allocation+4096,4096).IndexOfAnyExcept((byte)0x5a)<0,"R66 crossing never accesses contiguous unchecked gap");
            }
            // Aligned selected-C access faults before MMU lookup or effects.
            ResetFull(machine); VectorSeed(machine); Write64(machine,48,guest+1);
            ((Blink.Machine*)machine)->segvcode=2;
            using var aligned=CreateFullBlock([new(VectorClassify([0x66,0x0f,0xfc,0x03]))]);
            ulong ep=11; long iss=0,done=0; int stopped=0; var header=FullHeader(Ip,aligned.Entry);
            var f=FullFrame(machine,&header,&ep,&iss,&done,&stopped);
            byte[] saved=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            Check(RunFull(aligned,&f)==FullBlockExit.GuestFault && f.FaultTrap==14 && f.FaultCode==2 &&
                f.FaultAddress==guest+1 && iss==1 && done==0 && f.NativeMemoryComputations==0 &&
                new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(saved),"R66 CPU14 alignment fault precedes any memory/CPU effect");
            ResetFull(machine); VectorSeed(machine); Write64(machine,48,0x100000008); Write64(machine,32,0x100000002);
            using var fs=CreateFullBlock([new(VectorClassify([0x64,0x67,0xf3,0x0f,0x6f,0x44,0x8b,0xf8]))]);
            iss=done=0; header=FullHeader(Ip,fs.Entry); f=FullFrame(machine,&header,&ep,&iss,&done,&stopped);
            new Span<FullMemoryTlbEntry>(tlb,64).Clear(); SetFullMemoryEntry(tlb,guest,first);
            f.MemoryTlb=tlb; f.MemoryTlbMask=63; f.FsBase=guest;
            Check(RunFull(fs,&f)==FullBlockExit.Continue && new ReadOnlySpan<byte>(machine+160,16).SequenceEqual(new ReadOnlySpan<byte>(first+8,16)),
                "R66 generated EA32 truncation precedes FS base addition");
            // Ticket consumes only one instruction even when Executed>uint.Max.
            ResetFull(machine); VectorSeed(machine); Write64(machine,48,guest+4095);
            using var ticket=CreateFullBlock([new(VectorClassify([0xf3,0x0f,0x7f,0x03]))]);
            iss=done=0; header=FullHeader(Ip,ticket.Entry); f=FullFrame(machine,&header,&ep,&iss,&done,&stopped);
            Check(RunFull(ticket,&f)==FullBlockExit.NeedMemory && iss==1,"R66 ticket starts with approved miss");
            f.Executed=uint.MaxValue;
            FullNativeMemory.PublishBarrierTicket(&f,first+4095,second,1,ep,123);
            FullNativeMemory.ResumeSameIssue(&f);
            Check(RunFull(ticket,&f)==FullBlockExit.Continue && iss==1 && done==1 && f.NativeLimit==0 &&
                f.Executed==(ulong)uint.MaxValue+1 && f.MemoryFlags==FullMemoryFlags.BarrierConsumed,
                "R66 width128 ticket commits and exits independent of uint NativeLimit");
        }
        finally { NativeMemory.Free(allocation); }
    }

    static void CheckFullVectorIssueBoundaries(byte* machine)
    {
        var i=VectorClassify([0x66,0x0f,0xfc,0xc1]);
        using var code=CreateFullBlock([new(i)]);
        foreach(string boundary in new[]{"stop","attention","budget","approved-attention"})
        {
            ResetFull(machine); VectorSeed(machine); byte[] before=new ReadOnlySpan<byte>(machine+160,256).ToArray();
            machine[11]=(byte)(boundary.Contains("attention")?1:0);
            ulong epoch=11; long issued=boundary=="approved-attention"?1:0,completed=0; int stop=boundary=="stop"?1:0;
            var h=FullHeader(Ip,code.Entry); var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:boundary=="budget"?0UL:1UL);
            f.FirstPreissued=(uint)(boundary=="approved-attention"?1:0);
            var exit=RunFull(code,&f);
            if(boundary=="approved-attention") Check(exit==FullBlockExit.Continue && issued==1 && completed==1,"R66 preissued vector executes before late attention");
            else Check(exit==(boundary=="stop"?FullBlockExit.Stop:boundary=="attention"?FullBlockExit.Attention:FullBlockExit.Budget) &&
                completed==0 && issued==(boundary=="budget"?1:0) && new ReadOnlySpan<byte>(machine+160,256).SequenceEqual(before),"R66 vector boundary precedence/rejected issue");
        }
    }

    static void CheckFullVectorClassifier()
    {
        foreach(byte opcode in VectorBinaryOpcodes())
        {
            Check(FullBlockEmitter.VectorOperation(VectorClassify([0x66,0x0f,opcode,0xc1]))!=0,"R66 SSE2 register inventory");
            Check((VectorClassify([0x66,0x0f,opcode,0x03]).Options&FullBlockEmitter.VectorAlignedMemory)!=0,"R66 selected packed memory alignment");
        }
        var ea=VectorClassify([0x64,0x67,0xf3,0x45,0x0f,0x6f,0x54,0x8b,0xf8]);
        Check(ea.Destination.Register==10 && ea.Address==new JitAddress(11,1,2,-8,false,32,4),"R66 EA32/FS/SIB/REX exact typed metadata");
        Check(VectorClassify([0x67,0xf3,0x0f,0x6f,0x05,1,0,0,0]).Address==new JitAddress(-1,-1,0,1,true,32,-1),"R66 exact EIP-relative metadata");
        foreach(byte[] rejected in new byte[][] { [],[0x66],[0x66,0x0f,0xfc],[0x66,0x66,0x0f,0xfc,0xc1],
            [0xf3,0x66,0x0f,0xfc,0xc1],[0x48,0x66,0x0f,0x6e,0xc1],[0xf0,0x66,0x0f,0xfc,0x03],
            [0x66,0x48,0x0f,0xfc,0xc1],[0x66,0x0f,0xfc,0xc1,0x90],[0x0f,0xfc,0xc1],
            [0x66,0x0f,0x71,0xc0,1],[0x66,0x44,0x0f,0x71,0xd0,1],[0x66,0x0f,0x73,0xe0,1],
            [0x66,0x0f,0xd7,0x03],[0x66,0x0f,0xc5,0x03,1],[0x66,0x0f,0x70,0xc1],
            [0x67,0x66,0x0f,0xfc,0xc1],[0x66,0x42,0x0f,0xfc,0xc1],
            [0x66,0x40,0x0f,0xfc,0xc1],[0x66,0x42,0x0f,0xfc,0x03],
            [0x66,0x41,0x0f,0xfc,0x05,0,0,0,0],[0x66,0x41,0x0f,0xfc,0x04,0x25,0,0,0,0],
            [0x66,0x0f,0x38,0x00,0xc1],[0x66,0x0f,0x38,0x40,0xc1],
            [0xc5,0xf9,0xfc,0xc1],[0x62,0xf1,0x7d,0x48,0xfc,0xc1],
            [0xf3,0x0f,0x58,0xc1],[0x0f,0xae,0x13],[0x0f,0x77] })
            Check(!FullVectorLowering.TryClassify(Ip,rejected,out _,out var reason) && reason!=null,"R66 explicit MISSING/reserved/prefix/length rejection");
        var valid=VectorClassify([0x66,0x0f,0xfc,0xc1]);
        foreach(var invalid in new[] { valid with { Width=64 },valid with { Options=0xffffffff },valid with { SourceWidth=64 },
            valid with { Destination=new(16,0) },valid with { Source=new(0,0,ByteOffset:1) },valid with { Extra=new(0,3,true) } })
        {
            bool rejected=false; try { FullBlockEmitter.ValidateVector(invalid); } catch(ArgumentException) { rejected=true; }
            Check(rejected,"R66 malformed typed IR rejected before emission");
        }
    }

    static void CheckFullVectorSpanAllocation()
    {
        var first=VectorClassify([0x66,0x45,0x0f,0xfc,0xd3]);
        var second=VectorClassify([0xf3,0x0f,0x7f,0x03]) with { Ip=Ip+(uint)first.Length };
        FullBlockInstruction[] instructions=[new(first),new(second)];
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool windows in new[]{false,true})
        {
            for(int warm=0;warm<10;++warm) Check(FullBlockEmitter.TryEmit(instructions,staging,out _,windows),"R66 warm span emission");
            long before=GC.GetAllocatedBytesForCurrentThread(); bool good=true;
            for(int repeat=0;repeat<32;++repeat) good&=FullBlockEmitter.TryEmit(instructions,staging,out int length,windows) && length>0;
            Check(good && GC.GetAllocatedBytesForCurrentThread()==before,"R66 both ABI warmed span emission zero allocation");
            Check(staging.AsSpan(0,7).SequenceEqual(windows ? new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xca} :
                new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xfa}),"R66 Win64/SysV frame argument encoding");
            Check(staging.AsSpan().IndexOf(new byte[]{0x66,0x0f,0xfc,0xc1})>=0 &&
                staging.AsSpan().IndexOf(new byte[]{0xf3,0x0f,0x7f,0x02})>=0,"R66 actual SSE2 arithmetic and MOVDQU host store encoding");
            Check(!FullBlockEmitter.TryEmit(instructions,new byte[11],out int tiny,windows) && tiny==0,"R66 no partial publication on oversize");
        }
    }
}
