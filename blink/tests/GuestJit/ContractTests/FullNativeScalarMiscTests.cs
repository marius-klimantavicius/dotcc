using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using ScalarMiscFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void SetScalarMiscSelector(Blink.Machine* m, int s, ushort value)
    {
        switch (s)
        {
            case 0: m->es.sel = value; break; case 1: m->cs.sel = value; break;
            case 2: m->ss.sel = value; break; case 3: m->ds.sel = value; break;
            case 4: m->fs.sel = value; break; case 5: m->gs.sel = value; break;
        }
    }
    static JitInstruction ScalarMiscClassify(ulong pc, byte[] bytes)
    {
        Check(FullScalarMiscLowering.TryClassify(pc, bytes, out var i, out var why), why ?? "scalar misc classifier");
        Check(FullBlockEmitter.CanInline(i) &&
            (FullBlockEmitter.ValidateScalarMisc(i) || FullBlockEmitter.ValidateMemory(i)), "scalar misc native validation");
        return i;
    }

    static void CheckFullNativeScalarMisc(byte* machine)
    {
        CheckFullScalarMiscGrammar();
        CheckFullScalarMiscLeaves(machine);
        CheckFullScalarMiscCanonicalFaults(machine);
        using (var common = CreateFullScaffold(out var shared))
        {
            fullOwnerScaffold = shared;
            try { CheckFullScalarMiscLeaves(machine); CheckFullScalarMiscCanonicalFaults(machine); }
            finally { fullOwnerScaffold = default; }
        }
        CheckFullScalarMiscOracle();
        CheckFullScalarMiscController();
        CheckFullScalarMovnti(machine);
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity];
        FullBlockInstruction[] steps = [new(ScalarMiscClassify(Ip, [0x8c,0xc8])),
            new(ScalarMiscClassify(Ip + 2, [0xd7]))];
        foreach (bool windows in new[] { false, true })
            Check(FullBlockEmitter.TryEmit(steps, staging, out int n, windows) && n > 0,
                "scalar misc SysV/Win64 encoding");
        for (int k = 0; k < 32; ++k) FullBlockEmitter.Emit(steps, staging);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int k = 0; k < 1000; ++k) FullBlockEmitter.Emit(steps, staging);
        Check(GC.GetAllocatedBytesForCurrentThread() == before, "scalar misc warm caller-owned span emission allocates zero");
        ResetFull(machine);
    }

    static void CheckFullScalarMiscGrammar()
    {
        foreach (byte[] prefix in new byte[][] { [], [0x66], [0x48], [0x66,0x48],
            [0x48,0x66], [0x66,0x66], [0x67,0x64,0x66], [0x64,0x65,0x67,0x40] })
        foreach (int selector in Enumerable.Range(0, 6))
        foreach (int register in Enumerable.Range(0, 8))
        {
            var bytes = prefix.Concat(new byte[] { 0x8c,(byte)(0xc0 | selector << 3 | register) }).ToArray();
            var i = ScalarMiscClassify(Ip, bytes);
            Check(i.Options == selector && i.Destination.Register == register && i.SourceWidth == 16,
                "all visible selectors and destination registers");
            if (prefix.SequenceEqual(new byte[] { 0x48,0x66 })) Check(i.Width == 16, "legacy prefix cancels previous REX.W");
            if (prefix.SequenceEqual(new byte[] { 0x66,0x48 })) Check(i.Width == 64, "effective REX.W overrides66");
        }
        foreach (byte[] bytes in new byte[][] { [0x8c,0x03], [0x48,0x8c,0x03], [0x66,0x8c,0x03],
            [0x65,0x67,0x4f,0x8c,0x44,0xfe,0x80], [0x8c,0x05,0,0,0,0] })
        {
            var i = ScalarMiscClassify(Ip, bytes);
            Check(i.Width == 16 && i.Destination.IsMemory, "MOV memory selector always stores exactly16 bits");
        }
        foreach (byte[] prefix in new byte[][] { [], [0x66], [0x67], [0x64], [0x65,0x67],
            [0x4f], [0x48,0x67,0x66,0x40], Enumerable.Repeat((byte)0x66,14).ToArray() })
        {
            var bytes = prefix.Concat(new byte[] { 0xd7 }).ToArray();
            var i = ScalarMiscClassify(Ip, bytes);
            Check(i.Width == 8 && i.Source.IsMemory && i.Destination == new JitOperand(0,0), "XLAT always writes only AL");
        }
        foreach (byte[] bytes in new byte[][] { [], [0x8c], [0x8c,0xf0], [0x8c,0xf8], [0xd7,0],
            [0xf0,0xd7], [0xf3,0xd7], [0xf2,0x8c,0xc0] })
            Check(!FullScalarMiscLowering.TryClassify(Ip, bytes, out _, out var why) && why != null,
                "invalid or explicitly missing scalar forms fail closed");
        var valid = ScalarMiscClassify(Ip, [0xd7]);
        foreach (var bad in new[] { valid with { Width = 64 }, valid with { Options = 1 },
            valid with { Address = new(0,-1,0,0) }, valid with { SourceWidth = 16 } })
        {
            bool rejected = false; try { FullBlockEmitter.ValidateScalarMisc(bad); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "malformed XLAT IR rejected before emission");
        }
    }

    static void ScalarMiscRun(byte* machine, JitInstruction i, byte* first = null, byte* second = null,
        ulong page = 0x100000, FullMemoryAccess access = FullMemoryAccess.Read | FullMemoryAccess.Write,
        ulong fs = 0, ulong gs = 0)
    {
        using var code = CreateFullBlock([new(i)]);
        ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
        var h = FullHeader(i.Ip, code.Entry); var f = FullFrame(machine, &h, &epoch, &issued, &completed, &stop, limit: 1);
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        f.MemoryTlb = tlb; f.MemoryTlbMask = 63; f.FsBase = fs; f.GsBase = gs;
        if (first != null) SetFullMemoryEntry(tlb,page,first,access);
        if (second != null) SetFullMemoryEntry(tlb,page+4096,second,access);
        Check(RunFull(code,&f) == FullBlockExit.Continue && issued == 1 && completed == 1 && f.Executed == 1,
            "scalar misc one issue and native completion");
    }

    static void CheckFullScalarMiscLeaves(byte* machine)
    {
        var m = (Blink.Machine*)machine;
        for (int selector = 0; selector < 6; ++selector)
        foreach (int width in new[] { 16,32,64 })
        foreach (int register in new[] { 0,3,4,15 })
        {
            byte[] prefix = width == 16 ? [0x66] : width == 64 ? [0x48] : [];
            if (register >= 8) prefix = width == 16 ? [0x66,0x41] : [(byte)(width == 64 ? 0x49 : 0x41)];
            var i = ScalarMiscClassify(Ip, [..prefix,0x8c,(byte)(0xc0 | selector << 3 | (register & 7))]);
            ResetFull(machine); m->flags = 0xa50008d5;
            for (int s = 0; s < 6; ++s) SetScalarMiscSelector(m,s,(ushort)(0x9001 + s * 0x111));
            for (int r = 0; r < 16; ++r) Write64(machine,24+r*8,0xdeadbeefcafebabe);
            ScalarMiscRun(machine,i);
            ulong value = (ushort)(0x9001 + selector * 0x111);
            ulong expected = width == 16 ? 0xdeadbeefcafe0000UL | value : value;
            Check(((ulong*)(machine+24))[register] == expected && m->flags == 0xa50008d5,
                "visible guest selector width/zeroextension and preserved software flags");
        }
        byte* data = (byte*)NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            new Span<byte>(data,8192).Fill(0xa5);
            ResetFull(machine); m->cs.sel = 0x1234; Write64(machine,48,0x100fff);
            ScalarMiscRun(machine,ScalarMiscClassify(Ip,[0x8c,0x0b]),data,data+4096);
            Check(data[4094] == 0xa5 && data[4095] == 0x34 && data[4096] == 0x12 && data[4097] == 0xa5,
                "selector store crosses independently checked pages and touches exactly two bytes");
            foreach (bool address32 in new[] { false,true })
            foreach (int segment in new[] { -1,4,5 })
            {
                ResetFull(machine); m->flags = 0x550008d1;
                Write64(machine,24,0xdeadbeefcafeba31);
                ulong baseValue = address32 ? 0xffffffff00100000UL : 0x100000UL;
                Write64(machine,48,baseValue - 0x31);
                byte[] prefix = segment < 0 ? [] : [(byte)(segment == 4 ? 0x64 : 0x65)];
                if (address32) prefix = [..prefix,0x67];
                ulong segmentBase = segment < 0 ? 0UL : 64;
                data[segmentBase] = 0x7c;
                ScalarMiscRun(machine,ScalarMiscClassify(Ip,[..prefix,0xd7]),data,
                    fs:segment == 4 ? segmentBase : 0, gs:segment == 5 ? segmentBase : 0);
                Check(m->al == 0x7c && ((ulong*)(machine+24))[0] == 0xdeadbeefcafeba7c && m->flags == 0x550008d1,
                    "XLAT unsigned old AL index, EA32 truncation, FS/GS addition, upper RAX and flags");
            }
            var xlat = ScalarMiscClassify(Ip,[0xd7]);
            using var code = CreateFullBlock([new(xlat)]);
            ResetFull(machine); Write64(machine,24,0x82); Write64(machine,48,0x100000);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
            var h = FullHeader(Ip,code.Entry); var f = FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            f.MemoryTlb = tlb; f.MemoryTlbMask = 63;
            Check(RunFull(code,&f) == FullBlockExit.NeedMemory && f.MemoryAddress == 0x100082 &&
                issued == 1 && completed == 0 && m->al == 0x82 && m->ip == Ip,
                "XLAT mapping miss reserves issue without partial destination effect");
            data[0x82] = 0x19; SetFullMemoryEntry(tlb,0x100000,data,FullMemoryAccess.Read);
            FullNativeMemory.ResumeSameIssue(&f);
            Check(RunFull(code,&f) == FullBlockExit.Continue && issued == 1 && completed == 1 && m->al == 0x19,
                "XLAT same approved issue resumes through readonly native data mapping");
            ResetFull(machine); m->metal = true;
            using var selectorCode = CreateFullBlock([new(ScalarMiscClassify(Ip,[0x8c,0xc8]))]);
            issued = completed = 0; h = FullHeader(Ip,selectorCode.Entry); f = FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            Check(RunFull(selectorCode,&f) == FullBlockExit.Invalidated && completed == 0,
                "selector user-only path rejects metal before architectural effects");
        }
        finally { NativeMemory.AlignedFree(data); }
    }

    static void CheckFullScalarMiscOracle()
    {
        using var fixture = new ScalarMiscFixture(interpreterOracle:true);
        byte* m = (byte*)fixture.Machine();
        foreach (byte[] bytes in new byte[][] { [0x8c,0xc8], [0x66,0x8c,0xd3], [0x49,0x8c,0xec] })
        {
            var i = ScalarMiscClassify(Ip,bytes);
            // This is a real owned Machine, not the synthetic leaf buffer.
            // Preserve system/page-table/TID/lifecycle fields for disposal.
            ((Blink.Machine*)m)->ip = Ip; ((Blink.Machine*)m)->oplen = 0;
            ((Blink.Machine*)m)->flags = 0x150008d1;
            for (int s = 0; s < 6; ++s) SetScalarMiscSelector((Blink.Machine*)m,s,(ushort)(0x30+s));
            for (int r = 0; r < 16; ++r) Write64(m,24+r*8,0x1234567887654321);
            var before = new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            fixture.DataScope(0,()=>ControlFlagReference(fixture.Program,(byte*)fixture.Machine(),bytes));
            var expected = new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            before.CopyTo(new Span<byte>(m,before.Length)); ScalarMiscRun(m,i);
            Check(new ReadOnlySpan<byte>(m,9).SequenceEqual(expected.AsSpan(0,9)) &&
                new ReadOnlySpan<byte>(m+12,4).SequenceEqual(expected.AsSpan(12,4)) &&
                new ReadOnlySpan<byte>(m+24,128).SequenceEqual(expected.AsSpan(24,128)),
                "selected normal GetOp selector oracle, not a managed native fallback");
        }
    }

    static void CheckFullScalarMiscController()
    {
        using var fixture = new ScalarMiscFixture();
        const ulong pc = ScalarMiscFixture.Base;
        // MOV EBX, imm32; MOV AL,1; XLAT; infinite branch. The read data is a
        // separate ordinary guest page; the controller must fill it at runtime.
        fixture.Map(pc); fixture.Fill(pc);
        fixture.Map(pc+4096); fixture.Fill(pc+4096);
        byte[] body = [0xbb,0,0,0,0,0xb0,1,0xd7,0xeb,0xfe];
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(1), (uint)(pc+4096));
        WriteControllerCode(fixture,pc,body);
        ((byte*)fixture.Physical(pc+4096))[1] = 0x72;
        fixture.Counts(3); fixture.Step(pc);
        for (int k = 0; k < 12 && fixture.Completed < 3; ++k) fixture.Continue();
        Check(fixture.Machine()->al == 0x72 && fixture.Completed == 3 &&
            fixture.Statistics.SemanticHelperInstructions == 0,
            "real controller compiles XLAT and fills its data mapping without a semantic helper");
    }

    static void CheckFullScalarMiscCanonicalFaults(byte* machine)
    {
        foreach(byte[] bytes in new byte[][] { [0xd7], [0x8c,0x0b], [0x48,0x0f,0xc3,0x0b] })
        foreach(ulong address in new[] {0x0000800000000000UL,0xffff7fffffffffffUL,
            bytes[0]==0xd7?0x0000800000000000UL:0x00007fffffffffffUL,
            bytes[0]==0xd7?0xffff7fffffffffffUL:ulong.MaxValue})
        {
            var i=ScalarMiscClassify(Ip,bytes);using var code=CreateFullBlock([new(i)]);
            ResetFull(machine);Write64(machine,24,0x1234567800);Write64(machine,48,address);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
            Check(RunFull(code,&f)==FullBlockExit.GuestFault && f.FaultTrap==13 && f.FaultCode==128 &&
                f.FaultAddress==0 && issued==1 && completed==0 && f.Executed==0 &&
                new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                "R112 noncanonical start/end/wrap gives precise #GP before poisoned MMU and all effects");
        }
        using var fixture=new ScalarMiscFixture();const ulong pc=ScalarMiscFixture.Base;
        fixture.Map(pc);fixture.Fill(pc);WriteControllerCode(fixture,pc,[0xd7,0xeb,0xfe]);
        fixture.Machine()->al=0;Write64((byte*)fixture.Machine(),48,0x0000800000000000);
        fixture.Counts(10);var fault=fixture.StepSynchronousFault(pc);
        Check(fault.Signal==11 && fault.Detail?.EndsWith("signal=11 code=128")==true &&
            fixture.Machine()->trapno==13 && fixture.Completed==0 && fixture.Machine()->ip==pc,
            "R112 actual controller delivers #GP for XLAT rather than mapping #PF");
    }

    static void CheckFullScalarMovnti(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            foreach (int width in new[] {32,64})
            foreach (int source in new[] {0,3,4,15})
            foreach (int at in new[] {64,65,4095})
            {
                byte rex=(byte)(0x40 | (width==64?8:0) | (source>=8?4:0));
                byte[] encoding=rex==0x40 ? [0x0f,0xc3,(byte)(3|(source&7)<<3)] :
                    [rex,0x0f,0xc3,(byte)(3|(source&7)<<3)];
                var i=ScalarMiscClassify(Ip,encoding);
                Check(i.Operation==JitOperation.NonTemporalStore && i.Source.Register==source && i.Width==width,
                    "MOVNTI selects GPR32/64 native checked store");
                ResetFull(machine);new Span<byte>(data,8192).Fill(0xa5);
                Write64(machine,24+source*8,0x1234567887654321);Write64(machine,48,0x100000+(uint)at);
                // A source that aliases the base reads the original guest base.
                ulong expected=((ulong*)(machine+24))[source];
                for(int b=0;b<16;++b)machine[160+source*16+b]=0x3c;
                ScalarMiscRun(machine,i,data,at+width/8>4096?data+4096:null);
                ulong actual=width==32?BinaryPrimitives.ReadUInt32LittleEndian(new(data+at,4)):
                    BinaryPrimitives.ReadUInt64LittleEndian(new(data+at,8));
                Check(actual==(width==32?(uint)expected:expected) && data[at-1]==0xa5 && data[at+width/8]==0xa5,
                    "MOVNTI stores GPR rather than selected inherited XMM-source defect, exact width and sentinels");
            }
        }
        finally {NativeMemory.AlignedFree(data);}
    }
}
