using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using ResidualFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // One grouped entry. AUTHORED UNRUN; parent owns serial build/runtime gate.
    // Extension rows prove selected semantics only, never architectural legality.
    readonly record struct ResidualCase(string Name, byte[] Bytes, int Dispatch,
        FullResidualScalarDomain Domain, bool Salc = false, bool Enter = false, uint ClearFlags = 0, uint SetFlags = 0, bool Fence = false);

    static readonly ResidualCase[] ResidualCases =
    [
        new("CLI selected software IF", [0xfa], 0xfa, FullResidualScalarDomain.SelectedProcessorExtension, ClearFlags: 1u << 9),
        new("CLI ignored prefixes", [0xf3,0x66,0x67,0x4f,0xfa], 0xfa, FullResidualScalarDomain.SelectedProcessorExtension, ClearFlags: 1u << 9),
        new("STI selected software IF", [0xfb], 0xfb, FullResidualScalarDomain.SelectedProcessorExtension, SetFlags: 1u << 9),
        new("STI ignored prefixes", [0xf2,0x66,0x67,0x4f,0xfb], 0xfb, FullResidualScalarDomain.SelectedProcessorExtension, SetFlags: 1u << 9),
        new("WBINVD selected fence", [0x0f,0x09], 0x109, FullResidualScalarDomain.SelectedProcessorExtension, Fence: true),
        new("WBINVD ignored prefixes", [0xf3,0x66,0x67,0x4f,0x0f,0x09], 0x109, FullResidualScalarDomain.SelectedProcessorExtension, Fence: true),
        new("SALC", [0xd6], 0xd6, FullResidualScalarDomain.SelectedSalcExtension, Salc: true),
        new("SALC ignored width/address/REX", [0x66,0x67,0x4f,0xd6], 0xd6,
            FullResidualScalarDomain.SelectedSalcExtension, Salc: true),
        new("SALC selected REP", [0xf3,0xd6], 0xd6, FullResidualScalarDomain.SelectedSalcExtension, Salc: true),
        new("SALC exact15", [..Enumerable.Repeat((byte)0x66,14),0xd6], 0xd6,
            FullResidualScalarDomain.SelectedSalcExtension, Salc: true),
        new("PREFETCH existing ordinary", [0x0f,0x18,0x03], 0x118, FullResidualScalarDomain.OrdinaryHint),
        new("PREFETCH register extension", [0x0f,0x18,0xc3], 0x118, FullResidualScalarDomain.SelectedHintExtension),
        new("PREFETCH group extension", [0x0f,0x18,0x3b], 0x118, FullResidualScalarDomain.SelectedHintExtension),
        new("0F0D selected nonaccess", [0x65,0x67,0x0f,0x0d,0x04,0x25,0xff,0xff,0xff,0xff],
            0x10d, FullResidualScalarDomain.SelectedHintExtension),
        new("0F19", [0x0f,0x19,0x03], 0x119, FullResidualScalarDomain.SelectedHintExtension),
        new("0F1A", [0x0f,0x1a,0xc0], 0x11a, FullResidualScalarDomain.SelectedHintExtension),
        new("0F1B", [0x0f,0x1b,0x43,0x80], 0x11b, FullResidualScalarDomain.SelectedHintExtension),
        new("0F1C", [0x0f,0x1c,0x83,0,0,0,0x80], 0x11c, FullResidualScalarDomain.SelectedHintExtension),
        new("0F1D", [0x0f,0x1d,0x04,0x25,0,0,0,0x80], 0x11d, FullResidualScalarDomain.SelectedHintExtension),
        new("0F1D exact15", [..Enumerable.Repeat((byte)0x66,12),0x0f,0x1d,0x03],
            0x11d, FullResidualScalarDomain.SelectedHintExtension),
        new("0F1E", [0x0f,0x1e,0x03], 0x11e, FullResidualScalarDomain.SelectedHintExtension),
        new("ENDBR existing baseline", [0xf3,0x0f,0x1e,0xfa], 0x11e, FullResidualScalarDomain.SelectedHintExtension),
        new("NOP documented /0", [0x0f,0x1f,0x03], 0x11f, FullResidualScalarDomain.OrdinaryHint),
        new("NOP reserved extension", [0x0f,0x1f,0xfb], 0x11f, FullResidualScalarDomain.SelectedHintExtension),
        new("NOP selected REP", [0xf2,0x0f,0x1f,0x03], 0x11f, FullResidualScalarDomain.SelectedHintExtension),
        new("NOP repeated prefixes", [0x67,0x67,0x48,0x66,0x65,0x64,0x0f,0x1f,0x03],
            0x11f, FullResidualScalarDomain.SelectedHintExtension),
        new("PAUSE effective REP3", [0xf2,0xf3,0x90], 0x90, FullResidualScalarDomain.SelectedHintExtension),
        new("NOP effective REP2", [0xf3,0xf2,0x90], 0x90, FullResidualScalarDomain.SelectedHintExtension),
        new("ENTER64 selected REP2", [0xf2,0xc8,0,0,0], 0xc8,
            FullResidualScalarDomain.SelectedEnterPrefixAlias, Enter: true),
        new("ENTER16 selected REP3", [0xf3,0x66,0xc8,0,0,0], 0xc8,
            FullResidualScalarDomain.SelectedEnterPrefixAlias, Enter: true),
        new("ENTER REX.W after66", [0xf3,0x66,0x48,0xc8,0,0,0], 0xc8,
            FullResidualScalarDomain.SelectedEnterPrefixAlias, Enter: true),
        new("ENTER legacy cancels REX", [0xf2,0x48,0x66,0xc8,0,0,0], 0xc8,
            FullResidualScalarDomain.SelectedEnterPrefixAlias, Enter: true),
        new("selected empty SWAPGS", [0x0f,0x01,0xf8], 0x101,
            FullResidualScalarDomain.SelectedProcessorExtension),
        new("selected empty SWAPGS ignored prefixes", [0xf3,0x66,0x67,0x4f,0x0f,0x01,0xf8], 0x101,
            FullResidualScalarDomain.SelectedProcessorExtension)
    ];

    static void CheckFullNativeResidualScalars(byte* machine)
    {
        CheckResidualScalarGrammar();
        using (var oracle = new ResidualFixture(interpreterOracle: true)) CheckResidualSelectedOracle(oracle);
        CheckResidualScalarLeaves(machine);
        using (var common = CreateFullScaffold(out var shared))
        {
            fullOwnerScaffold = shared;
            try { CheckResidualScalarLeaves(machine); }
            finally { fullOwnerScaffold = default; }
        }
        CheckResidualScalarEmissions();
        CheckResidualScalarControllers();
        ResetFull(machine);
    }

    static JitInstruction ResidualClassify(ResidualCase row, ulong ip = Ip)
    {
        Check(FullResidualScalarLowering.TryClassify(ip, row.Bytes, out var i, out var domain, out var why),
            why ?? row.Name);
        Check(domain == row.Domain && i.Length == row.Bytes.Length && i.Ip == ip,
            "R172 exact original span and explicit architecture/extension domain");
        Check(FullBlockEmitter.CanInline(i), "R172 parent native admission seam");
        Check(row.Salc || row.ClearFlags != 0 || row.SetFlags != 0 || row.Fence ? FullBlockEmitter.ValidateResidualScalar(i) : row.Enter ?
            FullBlockEmitter.ValidateEnter(i) : FullBlockEmitter.ValidateHint(i), "R172 canonical IR owner");
        Check(FullTranslationJit.TryClassifyNative(ip, row.Bytes, out var composed) && composed == i,
            "R172 composed classifier agrees including already-covered hint forms");
        return i;
    }

    static void CheckResidualScalarGrammar()
    {
        foreach (var row in ResidualCases)
        {
            _ = ResidualClassify(row);
            for (int cut = 0; cut < row.Bytes.Length; ++cut)
                Check(!FullResidualScalarLowering.TryClassify(Ip, row.Bytes.AsSpan(0, cut), out _, out _),
                    "R172 exact span truncation");
            Check(!FullResidualScalarLowering.TryClassify(Ip, [..row.Bytes,0x90], out _, out _),
                "R172 rejects trailing bytes");
            Check(!FullResidualScalarLowering.TryClassify(Ip, [0xf0,..row.Bytes], out _, out _),
                "R172 LOCK remains invalid, including after other prefixes");
        }
        // Complete ModRM and SIB encoding grammar for every selected no-data
        // opcode, with all group codes. This does NOT certify ISA legality.
        foreach (byte opcode in new byte[] { 0x0d,0x18,0x19,0x1a,0x1b,0x1c,0x1d,0x1e,0x1f })
        foreach (byte[] prefix in new byte[][] { [],[0x66],[0x67],[0x4f],[0xf2],[0xf3],
            [0x67,0x64,0x66,0x4f],[0x48,0x66,0x67,0x67] })
        for (int modrm = 0; modrm < 256; ++modrm)
        for (int sib = 0; sib < ((modrm >> 6) != 3 && (modrm & 7) == 4 ? 256 : 1); ++sib)
        {
            int mod = modrm >> 6, rm = modrm & 7;
            bool hasSib = mod != 3 && rm == 4;
            int displacement = mod == 1 ? 1 : mod == 2 || mod == 0 &&
                (rm == 5 || hasSib && (sib & 7) == 5) ? 4 : 0;
            byte[] bytes = [..prefix,0x0f,opcode,(byte)modrm,
                ..(hasSib ? new byte[] { (byte)sib } : []),..new byte[displacement]];
            Check(FullResidualScalarLowering.TryClassify(Ip, bytes, out var i, out _) &&
                i == new JitInstruction(Ip,bytes.Length,JitOperation.Nop,64,default,default),
                "R172 all selected ModRM/SIB/displacement forms erase the data operand");
        }
        foreach (byte rex in Enumerable.Range(0x40,16).Select(x=>(byte)x))
        {
            Check(FullResidualScalarLowering.TryClassify(Ip,[0xf3,rex,0x90],out var i,out _) == ((rex&1)==0),
                "R172 real REX.B exchanges remain with integer classifier");
            Check(FullResidualScalarLowering.TryClassify(Ip,[rex,0x66,0xf3,0x90],out i,out _) &&
                i.Operation==JitOperation.Pause,"R172 legacy prefix cancels REX.B before NOP/PAUSE selection");
        }
        for(int modrm=0;modrm<256;++modrm)
            Check(FullResidualScalarLowering.TryClassify(Ip,[0x0f,0x01,(byte)modrm],out _,out _) == (modrm==0xf8),
                "R172 SWAPGS exact selected F8 only; RDTSCP and all other Op101 leaves remain excluded");
        foreach(byte[] bytes in new byte[][] { [],[0xf0],[0x0f],[0x0f,0x19],[0x0f,0x19,0x84],
            Enumerable.Repeat((byte)0x66,15).Append((byte)0xd6).ToArray() })
            Check(!FullResidualScalarLowering.TryClassify(Ip,bytes,out _,out _),"R172 invalid/truncated/overlong spans");
        foreach (int width in new[] { 16,64 }) foreach (byte rep in new byte[] { 0xf2,0xf3 })
        foreach (int allocation in new[] { 0,1,255,256,4095,4096,65535 })
        {
            byte[] prefix = width==16 ? [rep,0x66] : [rep];
            byte[] bytes = [..prefix,0xc8,(byte)allocation,(byte)(allocation>>8),0];
            Check(FullResidualScalarLowering.TryClassify(Ip,bytes,out var i,out var domain,out _) &&
                i.Width==width && i.Extra.Immediate==(uint)allocation &&
                domain==FullResidualScalarDomain.SelectedEnterPrefixAlias,"R172 raw0 ENTER shared mapping IR");
            for (int raw=1;raw<256;++raw)
            {
                bytes[^1]=(byte)raw;
                Check(!FullResidualScalarLowering.TryClassify(Ip,bytes,out _,out var why) &&
                    why=="SelectedEnterNonzeroRawNestingUnsupported" &&
                    !FullTranslationJit.TryClassifyNative(Ip,bytes,out _),
                    "R172 every nonzero RAW byte retains selected UD, including modulo32 aliases");
            }
        }
        var salc=ResidualClassify(ResidualCases[0]);
        foreach (var bad in new[] { salc with{Width=16},salc with{SourceWidth=8},salc with{Length=0},
            salc with{Length=16},salc with{Source=new(1,0)},salc with{Extra=new(1,0)},
            salc with{Address=new(0,-1,0,0)},salc with{Destination=new(1,0)},salc with{Target=1},
            salc with{Condition=1},salc with{Destination=new(0,0,ByteOffset:1)} })
        {
            bool rejected=false;try{FullBlockEmitter.ValidateResidualScalar(bad);}catch(ArgumentException){rejected=true;}
            Check(rejected,"R172 malformed SALC fails before emission");
        }
    }

    static void ResidualSeed(byte* m,uint flags)
    {
        var cpu=(Blink.Machine*)m;cpu->ip=Ip; m[8]=7; m[11]=0;cpu->flags=flags;
        for(int r=0;r<16;++r)Write64(m,24+r*8,0xfedcba9876543200UL+(uint)r);
    }

    static ulong ResidualRead64(byte* m,int offset) =>
        BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(m+offset,8));

    static byte[] ResidualExpected(byte[] before,ResidualCase row,ulong ip)
    {
        byte[] result=(byte[])before.Clone();
        BinaryPrimitives.WriteUInt64LittleEndian(result,ip);result[8]=0;
        uint flags = BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12));
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(12), (flags & ~row.ClearFlags) | row.SetFlags);
        if(row.Salc)result[24]=(BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12))&1)!=0?(byte)255:(byte)0;
        return result;
    }

    static void CheckResidualSelectedOracle(ResidualFixture f)
    {
        const ulong data=ResidualFixture.Base+0x20000;
        f.Map(data,8192);f.Fill(data,8192);
        foreach(var row in ResidualCases)foreach(uint cf in new uint[]{0,1})foreach(uint ifBit in new uint[]{0,1u<<9})
        {
            f.At(Ip);byte* m=(byte*)f.Machine();ResidualSeed(m,(0xa52c0ed4u&~0x50300u)|cf|ifBit);
            Blink.XedDecodedInst decoded=default;
            fixed(byte* bytes=row.Bytes)
                Check(f.Program.DecodeInstruction(&decoded,bytes,(ulong)row.Bytes.Length,Blink.XED_MODE_LONG)==0 &&
                    decoded.length==row.Bytes.Length && (int)((decoded.op.rde>>40)&2047)==row.Dispatch,
                    "R172 selected XED exact span AND handler identity, not ISA legality");
            var i=ResidualClassify(row);
            if(row.Enter){Write64(m,56,data+0x100);Write64(m,64,0x8877665544332211);}
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            f.DataScope(0,()=>HintReference(f.Program,(byte*)f.Machine(),row.Bytes));
            if(!row.Enter)
                Check(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(
                    ResidualExpected(before,row,Ip+(uint)row.Bytes.Length)),"R172 whole-machine selected oracle");
            else
            {
                // Selected kStackOsz uses raw OSZ bit5 even when effective
                // REX.W overrides66 for the native/architectural ENTER width.
                // This inherited R127 source discrepancy is not an equality
                // oracle for that form; independent native tables remain below.
                int selectedWidth=(decoded.op.rde&32)==0?64:16;
                ulong pushed=data+0x100-(uint)(selectedWidth/8);
                Check(ResidualRead64(m,56)==pushed && ResidualRead64(m,64)==(selectedWidth==64?pushed:
                    0x8877665544330000UL|(pushed&65535)) && f.Machine()->flags==
                    BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12)),
                    $"R172 selected REP ENTER profile width={selectedWidth}, native width={i.Width}");
            }
        }
    }

    static void CheckResidualScalarLeaves(byte* m)
    {
        foreach(var row in ResidualCases)
        {
            var i=ResidualClassify(row);
            if(row.Enter){CheckResidualEnterAlias(m,i);continue;}
            using var code=CreateFullBlock([new(i)]);
            foreach(uint cf in new uint[]{0,1})foreach(uint ifBit in new uint[]{0,1u<<9})foreach(ulong address in new[]{0UL,0x800000000000UL,ulong.MaxValue})
            {
                ResetFull(m);ResidualSeed(m,(0xa52c0ed4u&~0x50300u)|cf|ifBit);
                for(int r=0;r<16;++r)Write64(m,24+r*8,address);
                byte[] expected=ResidualExpected(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray(),row,Ip+(uint)i.Length);
                ulong epoch=11;long issue=0,done=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var frame=FullFrame(m,&h,&epoch,&issue,&done,&stop,limit:1);
                frame.MemoryTlb=(FullMemoryTlbEntry*)1;frame.MemoryTlbMask=63;
                frame.DataDescriptors=(JitDataDescriptor*)1;frame.StackMemory=(FullStackMemoryDescriptor*)1;
                frame.FsBase=frame.GsBase=0x100000000000000UL;
                Check(RunFull(code,&frame)==FullBlockExit.Continue && issue==1 && done==1 && frame.Executed==1 &&
                    frame.MemoryAssists==0 && frame.NativeMemoryComputations==0 && frame.MemoryResume==0 &&
                    frame.HelperIndex==FullBlockFrame.NoHelperIndex &&
                    new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(expected),
                    "R172 emitted scalar/hint no data evaluation, full state and precise issue/completion");
            }
            foreach(string boundary in new[]{"stop","attention","budget","invalidated","cancel"})
            {
                ResetFull(m);ResidualSeed(m,0x202);ulong epoch=11;long issue=0,done=0;int stop=boundary=="stop"?1:0;
                using var cancel=new CancellationTokenSource();using var owner=new HostExecutionStop(cancellation:cancel.Token);
                using var poll=owner.AcquireNativePoll();if(boundary=="cancel")cancel.Cancel();
                m[11]=boundary=="attention"?(byte)1:(byte)0;
                var h=FullHeader(Ip,code.Entry);if(boundary=="invalidated")h.Live=0;
                byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                var frame=FullFrame(m,&h,&epoch,&issue,&done,boundary=="cancel"?poll.Pointer:&stop,
                    budget:boundary=="budget"?0UL:100UL,limit:1);
                frame.MemoryTlb=(FullMemoryTlbEntry*)1;frame.StackMemory=(FullStackMemoryDescriptor*)1;
                var expected=boundary is "stop" or "cancel"?FullBlockExit.Stop:boundary=="attention"?
                    FullBlockExit.Attention:boundary=="budget"?FullBlockExit.Budget:FullBlockExit.Invalidated;
                Check(RunFull(code,&frame)==expected && done==0 && issue==(boundary=="budget"?1:0) &&
                    new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(before),
                    "R172 stop/attention/authority/cancellation/budget before guest effects");
                if(boundary=="budget")
                {
                    Check(frame.FirstPreissued==0,"R172 denied budget attempt is issued but never approved");
                    frame.Budget=2;
                    Check(RunFull(code,&frame)==FullBlockExit.Continue && issue==2 && done==1,
                        "R172 later approved attempt has its own issue after a denied budget attempt");
                }
            }
            // A cold controller approval is distinct from a budget denial.
            // Stop preserves that approval; resume consumes it once at budget0.
            ResetFull(m);ResidualSeed(m,0x203);
            ulong approvedEpoch=11;long approvedIssue=1,approvedDone=0;int stopped=1;
            var approvedHeader=FullHeader(Ip,code.Entry);
            var approved=FullFrame(m,&approvedHeader,&approvedEpoch,&approvedIssue,&approvedDone,&stopped,0,1);
            approved.FirstPreissued=1;
            Check(RunFull(code,&approved)==FullBlockExit.Stop && approvedIssue==1 && approvedDone==0 &&
                approved.FirstPreissued==1,"R172 stop retains unused cold approval");
            stopped=0;
            Check(RunFull(code,&approved)==FullBlockExit.Continue && approvedIssue==1 && approvedDone==1 &&
                approved.FirstPreissued==0,"R172 cold approval consumed once without a second issue");
        }
    }

    static void CheckResidualEnterAlias(byte* m,JitInstruction i)
    {
        const ulong guest=0x100000;byte* memory=(byte*)NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            foreach(bool ticket in new[]{false,true})
            {
                new Span<byte>(memory,8192).Fill(0x5a);ResetFull(m);
                ulong rsp=guest+4095+(uint)(i.Width/8),bp=0x8877665544332211;
                EnterSeed(m,rsp,bp);uint flags=((Blink.Machine*)m)->flags;using var code=CreateFullBlock([new(i)]);
                ulong epoch=11;long issue=0,done=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var descriptor=new FullStackMemoryDescriptor{Version=1};
                var frame=FullFrame(m,&h,&epoch,&issue,&done,&stop,limit:1);frame.StackMemory=&descriptor;
                Check(RunFull(code,&frame)==FullBlockExit.NeedMemory && issue==1 && done==0 &&
                    FullNativeMemory.AuthenticateEnter(&frame,in i,(Blink.Machine*)m),
                    "R172 REP ENTER reuses exact ordered two-request mapping authentication");
                EnterReady(ref descriptor,memory,guest,epoch,ticket);FullNativeMemory.ResumeSameIssue(&frame);
                Check(RunFull(code,&frame)==FullBlockExit.Continue && issue==1 && done==1 &&
                    frame.NativeMemoryComputations==1 && descriptor.State==FullStackMemoryState.Consumed &&
                    frame.MemoryFlags==(ticket?FullMemoryFlags.BarrierConsumed:FullMemoryFlags.None),
                    "R172 REP ENTER same issue, checked crossing pointers, ticket consumed once");
                CheckEnterResult(m,i,rsp,bp,flags,memory,guest);
            }
        }
        finally{NativeMemory.AlignedFree(memory);}
    }

    static void CheckResidualScalarEmissions()
    {
        byte[] buffer=new byte[FullBlockEmitter.CodeCapacity],slab=new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach(var row in ResidualCases)
        {
            FullBlockInstruction[] body=[new(ResidualClassify(row))];
            foreach(bool windows in new[]{false,true})
            {
                Check(FullBlockEmitter.Emit(body,buffer,windowsAbi:windows)>0,"R172 SysV/Windows encoding only");
                FullBlockEmitter.EmitScaffold(slab,out var layout,windowsAbi:windows);
                var addresses=layout.Resolve((nint)0x10000000);
                Check(FullBlockEmitter.Emit(body,buffer,in addresses,windowsAbi:windows)>0,
                    "R172 shared and standalone scaffold encodings");
            }
        }
    }

    static void CheckResidualScalarControllers()
    {
        foreach(var row in ResidualCases)foreach(bool crossFetch in new[]{false,true})
        {
            using var f=new ResidualFixture();const ulong codePage=ResidualFixture.Base,data=codePage+0x20000;
            f.Map(codePage,8192);f.Fill(codePage,8192);
            ulong pc=crossFetch?codePage+4095:codePage;
            // Each byte is written separately so code spanning the boundary
            // does not assume contiguous physical backing in the real MMU.
            byte[] bytes=[0x90,..row.Bytes,0xeb,0xfe];
            f.DataScope(0,()=>
            {
                for(int n=0;n<bytes.Length;++n)
                {
                    byte* destination=f.Program.LookupAddress2(f.Machine(),(long)(pc+(uint)n),0,0);
                    Check(destination!=null,"R172 cross-fetch code byte has owned MMU backing");
                    destination[0]=bytes[n];
                }
            });
            var m=f.Machine();ResidualSeed((byte*)m,0xa52c0ed5u&~0x50100u);
            // No data mapping exists for hints, even with a noncanonical EA.
            for(int r=0;r<16;++r)Write64((byte*)m,24+r*8,0x800000000000UL);
            var i=ResidualClassify(row,pc+1);
            if(row.Enter){f.Map(data,8192);f.Fill(data,8192);Write64((byte*)m,56,data+4095+(uint)(i.Width/8));
                Write64((byte*)m,64,0x8877665544332211);}
            byte[] registers=new ReadOnlySpan<byte>((byte*)m+24,128).ToArray();uint flags=m->flags;
            if(row.Salc)registers[0]=255;
            Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;
            f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;f.Counts(2);
            Check(f.Step(pc),"R172 real selected compiler/controller entry");
            for(int attempt=0;f.Completed<2&&attempt<12;++attempt)Check(f.Continue(),"R172 bounded controller continuation");
            Check(f.Completed==2 && f.Issued==2 && m->ip==pc+1+(uint)row.Bytes.Length && m->oplen==0 &&
                m->flags==((flags & ~row.ClearFlags) | row.SetFlags) && f.Statistics.SemanticHelperInstructions==0 && f.Statistics.InterpreterAttempts==0 &&
                f.Statistics.NativeMemoryInstructions==(row.Enter?1L:0L),
                "R172 real original-byte decode/compile/MMU/controller with zero semantic helpers");
            if(!row.Enter)Check(new ReadOnlySpan<byte>((byte*)m+24,128).SequenceEqual(registers),
                "R172 noncanonical hint operands have no real MMU access; SALC changes AL only");
            else
            {
                ulong pushed=data+4095;
                Check(ResidualRead64((byte*)m,56)==pushed && ResidualRead64((byte*)m,64)==(i.Width==64?pushed:
                    0x8877665544330000UL|(pushed&65535)),"R172 real REP ENTER shared existing mapping path");
            }
            var statistics=f.Jit.Store.GetStatistics();
            Check(statistics.ActiveNativeEpochs==0 && statistics.ActiveManagedLeases==0 &&
                f.Statistics.ActiveDataBorrows==0,"R172 every native/store/data capability released after RET");
            f.NativeReturned=null;
        }
    }
}
