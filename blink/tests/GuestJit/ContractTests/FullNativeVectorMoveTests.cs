using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using M = Managed.Emulation.Execution.FullBlockEmitter.FullVectorMoveOperation;

unsafe partial class Program
{
    // R79 authored, UNRUN by its implementation agent. Parent owns build,
    // emission/native execution, producer selection and signal qualification.
    static void CheckFullNativeVectorMove(byte* machine)
    {
        Check(FullBlockEmitter.HasExpectedVectorLayout() && sizeof(FullBlockFrame) == 416 &&
            FullBlockFrame.AbiVersion == 0x46420007 && sizeof(TranslationBlockHeader) == 160,
            "R79 frame384/ABI4/header160/XMM layout");
        CheckFullVectorMoveClassifier();
        Check(!fullOwnerScaffold.IsValid, "R79 begin with standalone leaves");
        // Never retain this oracle's thread binding while constructing another
        // MMU/controller fixture (the R77/R78 fixture lifetime counterexample).
        using (var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle: true))
        {
            const ulong guest = ThreadedGuestExecution.FullControllerFixture.Base;
            fixture.Map(guest, 8192); fixture.Fill(guest, 8192);
            CheckFullVectorMoveOracle(fixture);
            CheckFullVectorMoveIndependent(machine);
            CheckFullVectorMoveMemoryBoundaries(machine);
            CheckFullVectorMoveIssues(machine);
            using var common = CreateFullScaffold(out var addresses);
            fullOwnerScaffold = addresses;
            try
            {
                CheckFullVectorMoveOracle(fixture);
                CheckFullVectorMoveIndependent(machine);
                CheckFullVectorMoveMemoryBoundaries(machine);
                CheckFullVectorMoveIssues(machine);
            }
            finally { fullOwnerScaffold = default; }
        }
        CheckFullVectorMoveMmu();
        CheckFullVectorMoveController();
        CheckFullVectorMoveEmission();
    }

    sealed record VectorMoveForm(string Name, byte Prefix, byte Opcode, M Operation, int Width,
        bool Reverse = false, bool MemoryOnly = false, bool RegisterOnly = false,
        bool Aligned = false, bool Wide = false, bool Map38 = false);

    static VectorMoveForm[] VectorMoveForms() => [
        new("movups-load",0,0x10,M.Move128,128), new("movups-store",0,0x11,M.Move128,128,Reverse:true),
        new("movupd-load",0x66,0x10,M.Move128,128), new("movupd-store",0x66,0x11,M.Move128,128,Reverse:true),
        new("movaps-load",0,0x28,M.Move128,128,Aligned:true), new("movaps-store",0,0x29,M.Move128,128,Reverse:true,Aligned:true),
        new("movapd-load",0x66,0x28,M.Move128,128,Aligned:true), new("movapd-store",0x66,0x29,M.Move128,128,Reverse:true,Aligned:true),
        new("movdqa-load",0x66,0x6f,M.Move128,128,Aligned:true), new("movdqa-store",0x66,0x7f,M.Move128,128,Reverse:true,Aligned:true),
        new("movdqu-load",0xf3,0x6f,M.Move128,128), new("movdqu-store",0xf3,0x7f,M.Move128,128,Reverse:true),
        new("movss-load",0xf3,0x10,M.MoveScalar,32), new("movss-store",0xf3,0x11,M.MoveScalar,32,Reverse:true),
        new("movsd-load",0xf2,0x10,M.MoveScalar,64), new("movsd-store",0xf2,0x11,M.MoveScalar,64,Reverse:true),
        new("movlps-load",0,0x12,M.LoadLow64,64,MemoryOnly:true), new("movlpd-load",0x66,0x12,M.LoadLow64,64,MemoryOnly:true),
        new("movhps-load",0,0x16,M.LoadHigh64,64,MemoryOnly:true), new("movhpd-load",0x66,0x16,M.LoadHigh64,64,MemoryOnly:true),
        new("movlps-store",0,0x13,M.StoreLow64,64,Reverse:true,MemoryOnly:true), new("movlpd-store",0x66,0x13,M.StoreLow64,64,Reverse:true,MemoryOnly:true),
        new("movhps-store",0,0x17,M.StoreHigh64,64,Reverse:true,MemoryOnly:true), new("movhpd-store",0x66,0x17,M.StoreHigh64,64,Reverse:true,MemoryOnly:true),
        new("movhlps",0,0x12,M.MoveHighToLow,64,RegisterOnly:true), new("movlhps",0,0x16,M.MoveLowToHigh,64,RegisterOnly:true),
        new("unpcklps",0,0x14,M.UnpackLowSingle,64,Aligned:true), new("unpckhps",0,0x15,M.UnpackHighSingle,128,Aligned:true),
        new("unpcklpd",0x66,0x14,M.UnpackLowDouble,64,Aligned:true), new("unpckhpd",0x66,0x15,M.UnpackHighDouble,128,Aligned:true),
        new("movsldup",0xf3,0x12,M.DuplicateLowSingle,128,Aligned:true), new("movshdup",0xf3,0x16,M.DuplicateHighSingle,128,Aligned:true),
        new("movddup",0xf2,0x12,M.DuplicateDouble,64), new("lddqu",0xf2,0xf0,M.Move128,128,MemoryOnly:true),
        new("movd-to-xmm",0x66,0x6e,M.ScalarToVector,32), new("movq-to-xmm",0x66,0x6e,M.ScalarToVector,64,Wide:true),
        new("movd-from-xmm",0x66,0x7e,M.VectorToScalar,32,Reverse:true), new("movq-from-xmm",0x66,0x7e,M.VectorToScalar,64,Reverse:true,Wide:true),
        new("movq-f3",0xf3,0x7e,M.MoveLow64Zero,64), new("movq-d6",0x66,0xd6,M.MoveLow64Zero,64,Reverse:true),
        new("movntps",0,0x2b,M.NonTemporalStore128,128,Reverse:true,MemoryOnly:true,Aligned:true),
        new("movntpd",0x66,0x2b,M.NonTemporalStore128,128,Reverse:true,MemoryOnly:true,Aligned:true),
        new("movntdq",0x66,0xe7,M.NonTemporalStore128,128,Reverse:true,MemoryOnly:true,Aligned:true),
        new("movntdqa",0x66,0x2a,M.Move128,128,MemoryOnly:true,Aligned:true,Map38:true)
    ];

    static byte[] VectorMoveEncoding(VectorMoveForm form, bool memory, bool high = false, bool alias = false)
    {
        var bytes = new List<byte>(6);
        if (form.Prefix != 0) bytes.Add(form.Prefix);
        int rex = (form.Wide ? 8 : 0) | (high ? memory ? 4 : 5 : 0);
        if (rex != 0) bytes.Add((byte)(0x40 | rex));
        bytes.Add(0x0f); if (form.Map38) bytes.Add(0x38); bytes.Add(form.Opcode);
        bytes.Add(memory ? (byte)0x03 : alias ? (byte)0xc0 : (byte)0xc1);
        return bytes.ToArray();
    }

    static JitInstruction VectorMoveClassify(ReadOnlySpan<byte> bytes)
    {
        Check(FullVectorMoveLowering.TryClassify(Ip, bytes, out var i, out var reason), reason ?? "R79 classify");
        Check(FullBlockEmitter.CanInlineVectorMove(i) && FullBlockEmitter.ValidateVectorMove(i), "R79 typed move shape");
        return i;
    }

    static void CheckFullVectorMoveClassifier()
    {
        foreach (var form in VectorMoveForms())
        foreach (bool memory in new[] { false, true })
        foreach (bool high in new[] { false, true })
        {
            if (memory ? form.RegisterOnly : form.MemoryOnly) continue;
            var i = VectorMoveClassify(VectorMoveEncoding(form, memory, high));
            Check(FullBlockEmitter.VectorMoveOperation(i) == form.Operation && i.SourceWidth == form.Width &&
                i.Destination.IsMemory == (memory && form.Reverse) && i.Source.IsMemory == (memory && !form.Reverse) &&
                ((i.Options & FullBlockEmitter.VectorAlignedMemory) != 0) == (form.Aligned && memory), "R79 source-derived per-form contract");
            if (!memory) Check(i.Destination.Register == (high ? 8 : 0) + (form.Reverse ? 1 : 0) &&
                i.Source.Register == (high ? 8 : 0) + (form.Reverse ? 0 : 1), "R79 REX sources and reverse register direction");
        }
        Check(VectorMoveClassify([0x64,0x67,0x44,0x0f,0x12,0x54,0x8b,0xf8]).Address ==
            new JitAddress(3,1,2,-8,false,32,4), "R79 FS/EA32/SIB/signed disp8");
        Check(VectorMoveClassify([0x67,0xf2,0x0f,0x10,0x05,1,0,0,0]).Address ==
            new JitAddress(-1,-1,0,1,true,32), "R79 EIP-relative exact bytes");
        foreach (byte[] bytes in new byte[][] {
            [],[0x66],[0x0f,0x12],[0xf3,0x0f,0x10],[0xf3,0x0f,0x10,0xc1,0x90],
            [0x66,0x66,0x0f,0x12,0x03],[0xf2,0xf3,0x0f,0x10,0xc1],[0x48,0x66,0x0f,0x6e,0xc1],
            [0x67,0xf3,0x0f,0x10,0xc1],[0x40,0x0f,0x12,0xc1],[0x42,0x0f,0x12,0x03],
            [0xf0,0x0f,0x12,0x03],[0x48,0x0f,0x12,0xc1],[0x66,0x0f,0x12,0xc1],[0x66,0x0f,0x16,0xc1],
            [0x0f,0x13,0xc1],[0x0f,0x17,0xc1],[0xf2,0x0f,0x16,0xc1],[0xf2,0x0f,0xf0,0xc1],
            [0x0f,0x2b,0xc1],[0x66,0x0f,0xe7,0xc1],[0x66,0x0f,0x38,0x2a,0xc1],
            [0x0f,0x6e,0xc1],[0xf3,0x0f,0xd6,0xc1],[0xf2,0x0f,0xd6,0xc1],[0x66,0x0f,0xf7,0xc1],
            [0xc5,0xfa,0x10,0xc1],[0x62,0xf1,0x7e,0x08,0x10,0xc1],
            [0x41,0x0f,0x12,0x05,0,0,0,0],[0x41,0x0f,0x12,0x04,0x25,0,0,0,0] })
            Check(!FullVectorMoveLowering.TryClassify(Ip, bytes, out _, out var reason) && reason != null,
                "R79 explicit missing prefixes/reserved/length/MMX/masked/VEX/EVEX");
        var good = VectorMoveClassify([0xf2,0x0f,0x10,0xc1]);
        foreach (var bad in new[] {
            good with { Width=64 }, good with { SourceWidth=128 }, good with { Options=0x1000 },
            good with { Options=0x1fff }, good with { Options=good.Options | 0x20000 },
            good with { Options=good.Options | FullBlockEmitter.VectorAlignedMemory },
            good with { Target=1 }, good with { Condition=1 }, good with { Length=0 },
            good with { Extra=new(0,1,true) }, good with { Destination=new(16,0) },
            good with { Source=new(1,0,ByteOffset:1) }, good with { Address=new(3,-1,0,0) },
            good with { Source=new(1,0,IsMemory:true) },
            VectorMoveClassify([0x0f,0x12,0x03]) with { Source=new(1,0), Address=default },
            VectorMoveClassify([0x0f,0x14,0x03]) with { Options=(uint)M.UnpackLowSingle },
            VectorMoveClassify([0xf3,0x0f,0x12,0x03]) with { Options=(uint)M.DuplicateLowSingle },
            VectorMoveClassify([0xf3,0x0f,0x16,0x03]) with { Options=(uint)M.DuplicateHighSingle },
            VectorMoveClassify([0x0f,0x13,0x03]) with { Destination=new(0,0), Address=default } })
        {
            bool rejected = false;
            try { FullBlockEmitter.ValidateVectorMove(bad); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "R79 malformed typed metadata rejected before native emission");
        }
        Check(!FullBlockEmitter.CanInlineVectorMove(VectorClassify([0x66,0x0f,0xfc,0xc1])), "R79 distinct R66 option namespace");
    }

    // Independent bit/byte model. These operations neither interpret floating
    // values nor update flags/MXCSR; arbitrary NaN/denormal/sign bits survive.
    static byte[] VectorMoveBits(M op, int width, ReadOnlySpan<byte> destination, ReadOnlySpan<byte> source, bool memorySource)
    {
        byte[] result = new byte[16];
        destination.CopyTo(result);
        int n = width / 8;
        switch (op)
        {
            case M.Move128: case M.NonTemporalStore128: source[..16].CopyTo(result); break;
            case M.MoveScalar:
                if (memorySource) Array.Clear(result);
                source[..n].CopyTo(result); break;
            case M.LoadLow64: case M.StoreLow64: source[..8].CopyTo(result); break;
            case M.LoadHigh64: case M.MoveLowToHigh: source[..8].CopyTo(result.AsSpan(8)); break;
            case M.StoreHigh64: case M.MoveHighToLow: source.Slice(8,8).CopyTo(result); break;
            case M.MoveLow64Zero: case M.ScalarToVector: case M.VectorToScalar:
                Array.Clear(result); source[..n].CopyTo(result); break;
            case M.UnpackLowSingle: case M.UnpackHighSingle:
            case M.UnpackLowDouble: case M.UnpackHighDouble:
                int lane = op is M.UnpackLowDouble or M.UnpackHighDouble ? 8 : 4;
                int start = op is M.UnpackHighSingle or M.UnpackHighDouble ? 8 : 0;
                for (int at=0; at<8; at+=lane)
                {
                    destination.Slice(start+at,lane).CopyTo(result.AsSpan(at*2,lane));
                    source.Slice(start+at,lane).CopyTo(result.AsSpan(at*2+lane,lane));
                }
                break;
            case M.DuplicateLowSingle: case M.DuplicateHighSingle: case M.DuplicateDouble:
                int size = op == M.DuplicateDouble ? 8 : 4;
                for (int at=0; at<16; at+=size)
                {
                    int selected = size == 8 ? 0 : (at / 8) * 8 + (op == M.DuplicateHighSingle ? 4 : 0);
                    source.Slice(selected,size).CopyTo(result.AsSpan(at,size));
                }
                break;
            default: throw new InvalidOperationException("R79 missing independent model form");
        }
        return result;
    }

    static byte[] VectorMoveExpected(JitInstruction i, byte[] before, ReadOnlySpan<byte> memory, out byte[] result)
    {
        var op = FullBlockEmitter.VectorMoveOperation(i);
        bool gprSource = op == M.ScalarToVector, gprDestination = op == M.VectorToScalar;
        ReadOnlySpan<byte> source = i.Source.IsMemory ? memory : before.AsSpan(
            (gprSource ? 24 : 160) + i.Source.Register * (gprSource ? 8 : 16), gprSource ? 8 : 16);
        ReadOnlySpan<byte> destination = i.Destination.IsMemory || gprDestination ? ReadOnlySpan<byte>.Empty :
            before.AsSpan(160+i.Destination.Register*16,16);
        result = VectorMoveBits(op,i.SourceWidth,destination,source,i.Source.IsMemory);
        byte[] expected = (byte[])before.Clone();
        if (!i.Destination.IsMemory)
            result.AsSpan(0,gprDestination?8:16).CopyTo(expected.AsSpan(
                (gprDestination?24:160)+i.Destination.Register*(gprDestination?8:16)));
        BinaryPrimitives.WriteUInt64LittleEndian(expected,Ip+(uint)i.Length); expected[8]=0;
        return expected;
    }

    static void VectorMoveRun(byte* machine, JitInstruction i, FullMemoryTlbEntry* tlb = null, ulong fs = 0)
    {
        using var code=CreateFullBlock([new(i)]);
        ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop); f.MemoryTlb=tlb; f.MemoryTlbMask=tlb==null?0u:63u; f.FsBase=fs;
        Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1 &&
            f.NativeMemoryComputations==(i.Source.IsMemory||i.Destination.IsMemory?1UL:0UL), "R79 actual native bit computation and one commit");
    }

    static void CheckFullVectorMoveOracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        byte* machine=(byte*)fixture.Machine(), p0=(byte*)fixture.Physical(guest), p1=(byte*)fixture.Physical(guest+4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach (var form in VectorMoveForms())
        foreach (bool memory in new[]{false,true})
        foreach (bool high in new[]{false,true})
        foreach (bool alias in memory?new[]{false}:new[]{false,true})
        {
            if (memory?form.RegisterOnly:form.MemoryOnly) continue;
            foreach(int offset in memory?(form.Aligned?new[]{64}:new[]{65,4095}):new[]{0})
            {
                byte[] bytes=VectorMoveEncoding(form,memory,high,alias); var i=VectorMoveClassify(bytes);
                VectorSeed(machine); if(memory) Write64(machine,48,guest+(uint)offset);
                for(int b=0;b<4096;++b) { p0[b]=(byte)(b*37+11); p1[b]=(byte)(b*19+7); }
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                byte[] before0=new ReadOnlySpan<byte>(p0,4096).ToArray(), before1=new ReadOnlySpan<byte>(p1,4096).ToArray();
                byte[] input=new byte[i.SourceWidth/8];
                for(int b=0;b<input.Length;++b) input[b]=offset+b<4096?p0[offset+b]:p1[offset+b-4096];
                byte[] expected=VectorMoveExpected(i,before,input,out byte[] output);
                // Normal authoritative GetOp and CommitStash run only inside
                // this initialized fixture's CPU semantic read/data scope.
                fixture.DataScope(0,()=>VectorReference(fixture.Program,(byte*)fixture.Machine(),bytes));
                bool gap=form.Operation==M.UnpackHighDouble && (memory || !alias);
                if(gap)
                {
                    byte[] selected=new ReadOnlySpan<byte>(machine+160+i.Destination.Register*16,16).ToArray();
                    ReadOnlySpan<byte> oldSource=memory?input:before.AsSpan(160+i.Source.Register*16,16);
                    Check(selected.AsSpan(0,8).SequenceEqual(oldSource.Slice(8,8)) &&
                        selected.AsSpan(8,8).SequenceEqual(oldSource.Slice(8,8)) && !selected.AsSpan().SequenceEqual(output),
                        "R79 EXPECTED SELECTED GAP: native7 UNPCKHPD duplicates source high instead of retaining old destination high");
                }
                else VectorCheckArchitecture(machine,expected,$"R79 selected GetOp/independent bits {form.Name}");
                if(i.Destination.IsMemory)
                    for(int b=0;b<input.Length;++b) Check((offset+b<4096?p0[offset+b]:p1[offset+b-4096])==output[b],"R79 selected store exact bits");
                before.CopyTo(new Span<byte>(machine,before.Length));
                before0.CopyTo(new Span<byte>(p0,4096)); before1.CopyTo(new Span<byte>(p1,4096));
                new Span<FullMemoryTlbEntry>(tlb,64).Clear(); SetFullMemoryEntry(tlb,guest,p0); SetFullMemoryEntry(tlb,guest+4096,p1);
                VectorMoveRun(machine,i,memory?tlb:null);
                VectorCheckArchitecture(machine,expected,$"R79 independent native bits {form.Name}/high={high}/alias={alias}");
                if(i.Destination.IsMemory)
                    for(int b=0;b<input.Length;++b) (offset+b<4096?before0:before1)[(offset+b)%4096]=output[b];
                Check(new ReadOnlySpan<byte>(p0,4096).SequenceEqual(before0) && new ReadOnlySpan<byte>(p1,4096).SequenceEqual(before1),
                    "R79 exact memory results and untouched surrounding bytes");
            }
        }
        // Explicit independent architectural witness with distinct bit payloads.
        VectorSeed(machine); Write64(machine,168,0x1122334455667788); Write64(machine,184,0xaabbccddeeff0011);
        var hpd=VectorMoveClassify([0x66,0x0f,0x15,0xc1]);
        VectorMoveRun(machine,hpd);
        Check(((ulong*)(machine+160))[0]==0x1122334455667788 && ((ulong*)(machine+168))[0]==0xaabbccddeeff0011,
            "R79 architectural UNPCKHPD {old-dst.high,src.high} discriminates selected defect");
        // Selected low-unpack accepts an unaligned operand; architecture #GP
        // precedes mapping lookup. Keep this source gap explicit as well.
        VectorSeed(machine); Write64(machine,48,guest+65);
        fixture.DataScope(0,()=>VectorReference(fixture.Program,(byte*)fixture.Machine(),new byte[]{0x0f,0x14,0x03}));
        Check(fixture.Machine()->ip==Ip+3,"R79 EXPECTED SELECTED GAP: unaligned low-unpack succeeds in native7 interpreter");
        // Actual native hardware control distinguishes single duplicate forms
        // from MOVDDUP. Keep the selected interpreter's permissive behavior
        // visible; it is not an oracle for the architectural alignment fault.
        foreach (byte opcode in new byte[] { 0x12, 0x16 })
        {
            VectorSeed(machine); Write64(machine,48,guest+65);
            byte[] bytes = [0xf3,0x0f,opcode,0x03];
            fixture.DataScope(0,()=>VectorReference(fixture.Program,(byte*)fixture.Machine(),bytes));
            Check(fixture.Machine()->ip==Ip+4,
                "R81 EXPECTED SELECTED GAP: unaligned MOVSLDUP/MOVSHDUP succeeds in native7 interpreter; native hardware raises #GP");
        }
    }

    static void CheckFullVectorMoveIndependent(byte* machine)
    {
        foreach (var form in VectorMoveForms())
        {
            if(form.MemoryOnly) continue;
            foreach(bool high in new[]{false,true}) foreach(bool alias in new[]{false,true})
            {
                ResetFull(machine); VectorSeed(machine);
                var i=VectorMoveClassify(VectorMoveEncoding(form,false,high,alias));
                // Raw signaling-NaN, denormal and sign-bit patterns are data.
                Write64(machine,160+i.Source.Register*16,0x7f80000180000000);
                Write64(machine,168+i.Source.Register*16,0xfff0000000000001);
                var before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                var expected=VectorMoveExpected(i,before,ReadOnlySpan<byte>.Empty,out _);
                VectorMoveRun(machine,i); VectorCheckArchitecture(machine,expected,"R79 independent raw NaN/sign/lane bits; flags/MXCSR unchanged");
            }
        }
    }

    static void CheckFullVectorMoveMemoryBoundaries(byte* machine)
    {
        const ulong guest=0x400000;
        byte* allocation=(byte*)NativeMemory.AllocZeroed(12288);
        try
        {
            byte* first=allocation,second=allocation+8192;
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(var form in VectorMoveForms())
            {
                if(form.RegisterOnly) continue;
                var i=VectorMoveClassify(VectorMoveEncoding(form,true)); int n=i.SourceWidth/8;
                foreach(int offset in form.Aligned?new[]{64,4080}:new[]{65,4096-n,4095})
                foreach(string mapping in new[]{"both","second-missing","readonly"})
                {
                    ResetFull(machine); VectorSeed(machine); Write64(machine,48,guest+(uint)offset);
                    new Span<byte>(allocation,12288).Fill(0x5a);
                    for(int b=0;b<4096;++b) { first[b]=(byte)(b*13+1); second[b]=(byte)(b*29+3); }
                    byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                    byte[] memoryBefore=new ReadOnlySpan<byte>(allocation,12288).ToArray(), input=new byte[n];
                    for(int b=0;b<n;++b) input[b]=offset+b<4096?first[offset+b]:second[offset+b-4096];
                    var expected=VectorMoveExpected(i,before,input,out byte[] output);
                    new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                    var permissions=mapping=="readonly"?FullMemoryAccess.Read:FullMemoryAccess.Read|FullMemoryAccess.Write;
                    SetFullMemoryEntry(tlb,guest,first,permissions);
                    if(mapping!="second-missing") SetFullMemoryEntry(tlb,guest+4096,second,permissions);
                    using var code=CreateFullBlock([new(i)]);
                    ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
                    var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop); f.MemoryTlb=tlb; f.MemoryTlbMask=63;
                    bool miss=(offset+n>4096&&mapping=="second-missing")||(i.Destination.IsMemory&&mapping=="readonly");
                    if(miss)
                    {
                        Check(RunFull(code,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 && f.Executed==0 &&
                            f.MemoryAddress==guest+(uint)offset && f.MemoryWidth==i.SourceWidth &&
                            f.MemoryAccess==(i.Destination.IsMemory?FullMemoryAccess.Write:FullMemoryAccess.Read) &&
                            f.NativeMemoryComputations==0 && new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before) &&
                            new ReadOnlySpan<byte>(allocation,12288).SequenceEqual(memoryBefore),"R79 both mappings/permissions checked before ANY CPU or memory effects");
                        FullNativeMemory.PublishBarrierTicket(&f,first+offset,offset+n>4096?second:null,
                            (uint)Math.Min(n,4096-offset),epoch,123);
                        FullNativeMemory.ResumeSameIssue(&f);
                    }
                    Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1 &&
                        f.NativeMemoryComputations==1,"R79 exact operand width; same issue resumes and commits once");
                    VectorCheckArchitecture(machine,expected,"R79 independent checked single/cross-page result");
                    if(i.Destination.IsMemory)
                        for(int b=0;b<n;++b) memoryBefore[offset+b<4096?offset+b:8192+offset+b-4096]=output[b];
                    Check(new ReadOnlySpan<byte>(allocation,12288).SequenceEqual(memoryBefore),"R79 scatter exact width with untouched noncontiguous gap");
                }
                if(!form.Aligned) continue;
                ResetFull(machine); VectorSeed(machine); Write64(machine,48,guest+4095);
                byte[] saved=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                using var bad=CreateFullBlock([new(i)]);
                ulong e=11; long iss=0,done=0; int stopped=0; var header=FullHeader(Ip,bad.Entry);
                var frame=FullFrame(machine,&header,&e,&iss,&done,&stopped);
                Check(RunFull(bad,&frame)==FullBlockExit.GuestFault && frame.FaultTrap==13 && frame.FaultCode==128 &&
                    frame.FaultAddress==0 && frame.NativeMemoryComputations==0 && frame.MemoryResume==0 &&
                    iss==1 && done==0 && frame.Executed==0 && new ReadOnlySpan<byte>(machine,saved.Length).SequenceEqual(saved),
                    "R79 architectural #GP/SI_KERNEL128/addr0 alignment precedes missing page and all effects");
            }
            // Address arithmetic truncates before FS, while upper destination
            // bytes are retained; no second8 bytes of a MOVHPS source required.
            ResetFull(machine); VectorSeed(machine); Write64(machine,48,0x100000008); Write64(machine,32,0x100000002);
            var fs=VectorMoveClassify([0x64,0x67,0x44,0x0f,0x16,0x54,0x8b,0xf8]);
            byte[] old=new ReadOnlySpan<byte>(machine+160+10*16,8).ToArray();
            new Span<FullMemoryTlbEntry>(tlb,64).Clear(); SetFullMemoryEntry(tlb,guest,first);
            VectorMoveRun(machine,fs,tlb,guest);
            Check(new ReadOnlySpan<byte>(machine+160+10*16,8).SequenceEqual(old) &&
                new ReadOnlySpan<byte>(machine+168+10*16,8).SequenceEqual(new ReadOnlySpan<byte>(first+8,8)), "R79 EA32/SIB/FS and REX XMM10 half retention");
        }
        finally { NativeMemory.Free(allocation); }
    }

    static void CheckFullVectorMoveIssues(byte* machine)
    {
        var i=VectorMoveClassify([0xf2,0x45,0x0f,0x10,0xc1]);
        using var code=CreateFullBlock([new(i)]);
        foreach(string boundary in new[]{"stop","attention","budget","approved-attention"})
        {
            ResetFull(machine); VectorSeed(machine); machine[11]=(byte)(boundary.Contains("attention")?1:0);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11; long issued=boundary=="approved-attention"?1:0,completed=0; int stop=boundary=="stop"?1:0;
            var h=FullHeader(Ip,code.Entry); var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:boundary=="budget"?0UL:1UL);
            f.FirstPreissued=(uint)(boundary=="approved-attention"?1:0); var exit=RunFull(code,&f);
            if(boundary=="approved-attention") Check(exit==FullBlockExit.Continue && issued==1 && completed==1,"R79 approved move precedes late attention");
            else Check(exit==(boundary=="stop"?FullBlockExit.Stop:boundary=="attention"?FullBlockExit.Attention:FullBlockExit.Budget) &&
                completed==0 && issued==(boundary=="budget"?1:0) && new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R79 rejected issue has no effects");
        }
    }

    static void CheckFullVectorMoveMmu()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture();
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(guest); fixture.Fill(guest);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        var f=new FullBlockFrame { MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=guest+4088,
            MemoryAccess=FullMemoryAccess.Read,MemoryWidth=64,MemoryResume=1 };
        Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.Resolved,"R79 true m64 half read does not require unmapped next8 bytes");
        f.MemoryAddress=guest+4095; f.MemoryResume=1;
        Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.GuestFault && f.FaultTrap==14 && f.FaultAddress==guest+4096,
            "R79 crossing half source does require its own second mapping");
        fixture.Map(guest+4096); fixture.Fill(guest+4096);
        fixture.DataScope(0,()=>Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system,
            (long)(guest+4096),4096,1,false)==0,"R79 read-only second page"));
        f.MemoryAccess=FullMemoryAccess.Write; f.MemoryResume=1;
        Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.GuestFault && f.FaultTrap==14 && f.FaultCode==2 &&
            f.FaultAddress==guest+4096,"R79 real MMU readonly crossing store fault before either page changes");
    }

    static void CheckFullVectorMoveController()
    {
        foreach(byte[] operation in new byte[][] { [0x0f,0x14,0x03],[0x66,0x0f,0x15,0x03],[0x66,0x0f,0xe7,0x03] })
        {
            using var fixture=new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;
            fixture.Map(pc); fixture.Fill(pc);
            byte[] body=new byte[operation.Length+1]; body[0]=0x90; operation.CopyTo(body,1); WriteControllerCode(fixture,pc,body);
            var m=fixture.Machine(); Write64((byte*)m,48,pc+8192+4095); // unaligned AND entirely unmapped
            byte[] xmm=new ReadOnlySpan<byte>((byte*)m+160,256).ToArray(); uint flags=m->flags,mxcsr=m->mxcsr;
            fixture.Counts(100);
            using var deadline=new Timer(_=>fixture.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var signal=fixture.StepSynchronousFault(pc);
            Check(signal.Signal==11 && signal.Detail?.EndsWith("signal=11 code=128")==true && m->trapno==13 &&
                m->faultaddr==0 && m->ip==pc+1 && m->oplen==0 && m->flags==flags && m->mxcsr==mxcsr &&
                new ReadOnlySpan<byte>((byte*)m+160,256).SequenceEqual(xmm),"R79 actual controller #GP beats #PF with precise guest signal metadata");
            Check(fixture.Issued==2 && fixture.Completed==1 && fixture.Statistics.SemanticHelperInstructions==0 &&
                fixture.Statistics.InterpreterAttempts==0 && fixture.Statistics.ActiveDataBorrows==0 &&
                fixture.Jit.Store.GetStatistics().ActiveNativeEpochs==0 && fixture.Jit.Store.GetStatistics().ActiveManagedLeases==0,
                "R79 no semantic trampoline; alignment fault unwinds native/data/store capabilities");
        }
    }

    static void CheckFullVectorMoveEmission()
    {
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool windows in new[]{false,true})
        foreach(var form in VectorMoveForms())
        {
            var i=VectorMoveClassify(VectorMoveEncoding(form,!form.RegisterOnly));
            FullBlockInstruction[] instructions=[new(i)];
            for(int warm=0;warm<8;++warm) Check(FullBlockEmitter.TryEmit(instructions,staging,out _,windows),"R79 warm span");
            long before=GC.GetAllocatedBytesForCurrentThread(); bool good=true;
            for(int repeat=0;repeat<16;++repeat) good&=FullBlockEmitter.TryEmit(instructions,staging,out int length,windows)&&length>0;
            Check(good && GC.GetAllocatedBytesForCurrentThread()==before,"R79 SysV/Win64 warmed span allocates zero");
            Check(staging.AsSpan(0,7).SequenceEqual(windows?new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xca}:
                new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xfa}),"R79 ABI frame argument encoding");
            Check(!FullBlockEmitter.TryEmit(instructions,new byte[7],out int tiny,windows)&&tiny==0,"R79 no partial publication on short destination");
        }
        foreach((byte[] guest,byte[] host) in new (byte[],byte[])[] {
            ([0xf3,0x0f,0x12,0xc1],[0x66,0x0f,0x70,0xc1,0xa0]),
            ([0xf3,0x0f,0x16,0xc1],[0x66,0x0f,0x70,0xc1,0xf5]),
            ([0xf2,0x0f,0x12,0xc1],[0x66,0x0f,0x70,0xc1,0x44]),
            ([0x66,0x0f,0x15,0xc1],[0x66,0x0f,0x15,0xc1]) })
        {
            Check(FullBlockEmitter.TryEmit([new(VectorMoveClassify(guest))],staging,out int length) &&
                staging.AsSpan(0,length).IndexOf(host)>=0,"R79 real baseline SSE2 instruction bytes");
        }
    }
}
