using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using StackFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // Source-authored for the parent's serial runner; no execution by R85.
    static void CheckFullNativeStackMemory(byte* machine)
    {
        Check(sizeof(FullBlockFrame) == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(TranslationBlockHeader) == 160 && sizeof(FullStackMemoryDescriptor) == 128 &&
            sizeof(FullStackMemoryRequest) == 48 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.StackMemory)).ToInt32() == 384 &&
            Marshal.OffsetOf<FullStackMemoryDescriptor>(nameof(FullStackMemoryDescriptor.Request1)).ToInt32() == 80,
            "R85 ABI5 frame400/header160 and fixed two-access descriptor");
        CheckFullStackMemoryClassifier();
        CheckFullStackMemoryLeaves(machine);
        if (!fullOwnerScaffold.IsValid)
        {
            using var common = CreateFullScaffold(out var addresses); fullOwnerScaffold = addresses;
            try { CheckFullStackMemoryLeaves(machine); }
            finally { fullOwnerScaffold = default; }
        }
        // Dispose the oracle fixture before creating any MMU/controller fixture.
        using (var f = new StackFixture(interpreterOracle: true))
        {
            f.Map(StackFixture.Base, 16384); f.Fill(StackFixture.Base, 16384);
            CheckFullStackMemoryOracle(f);
            if (!fullOwnerScaffold.IsValid)
            {
                using var common = CreateFullScaffold(out var addresses); fullOwnerScaffold = addresses;
                try { CheckFullStackMemoryOracle(f); }
                finally { fullOwnerScaffold = default; }
            }
        }
        CheckFullStackMemoryMmu();
        CheckFullStackMemoryChangedMapping();
        CheckFullStackMemoryController();
        CheckFullStackMemoryWatchedDestination();
        CheckFullStackMemoryEmission();
    }

    static JitInstruction StackMemoryClassify(byte[] bytes)
    {
        Check(FullStackMemoryLowering.TryClassify(Ip, bytes, out var i, out string? reason), reason ?? "R85 classify");
        Check(FullBlockEmitter.CanInlineStackMemory(i) && FullBlockEmitter.ValidateStackMemory(i), "R85 exact stack IR");
        return i;
    }

    static void CheckFullStackMemoryClassifier()
    {
        foreach (byte[] bytes in new byte[][] {
            [0xff,0x33], [0x66,0xff,0x33], [0x8f,0x03], [0x66,0x8f,0x03],
            [0xff,0x13], [0x66,0xff,0x13], [0xc9], [0x66,0xc9], [0x67,0xc9],
            [0x64,0x67,0xff,0x74,0x8b,0xf8], [0x65,0x4b,0x8f,0x44,0xcc,0x80],
            [0x67,0xff,0x15,0xf0,0xff,0xff,0xff], [0x8f,0x04,0x24], [0xff,0x14,0x24] })
            StackMemoryClassify(bytes);
        Check(StackMemoryClassify([0x66,0xff,0x13]).Width == 64 &&
            StackMemoryClassify([0x66,0xc9]).Address == new JitAddress(5,-1,0,0) &&
            StackMemoryClassify([0x67,0xc9]).Width == 64, "R85 near CALL64 and implicit LEAVE stack64");
        foreach (byte[] bytes in new byte[][] { [0x66,0x48,0xff,0x33], [0x66,0x48,0x8f,0x03], [0x66,0x48,0xc9] })
            Check(StackMemoryClassify(bytes).Width == 64,
                "R75 actual-native control: REX.W overrides66 for PUSH/POP/LEAVE; selected C width gap retained");
        Check(StackMemoryClassify([0x64,0x67,0xff,0x74,0x8b,0xf8]).Address == new JitAddress(3,1,2,-8,false,32,4),
            "R85 explicit memory uses shared FS+EA32 SIB grammar");
        foreach (byte[] bytes in new byte[][] {
            [], [0xff], [0x8f], [0x66], [0xc9,0x90], [0xff,0x33,0x90], [0x8f,0x43],
            [0xff,0xd3], [0xff,0xf3], [0x8f,0xc3], [0xff,0x1b], [0xff,0x2b], [0x8f,0x0b],
            [0xc8,0,0,0], [0xf0,0xff,0x33], [0xf2,0xc9], [0xf3,0x8f,0x03],
            [0x66,0x66,0xc9], [0x64,0x65,0xff,0x33], [0x48,0x66,0xc9], [0x44,0xff,0x33] })
            Check(!FullStackMemoryLowering.TryClassify(Ip, bytes, out _, out _), "R85 missing far/ENTER/register/prefix/length stays explicit");
        var good = StackMemoryClassify([0xff,0x33]);
        foreach (var bad in new[] { good with { Width=32 }, good with { SourceWidth=16 }, good with { Options=1 },
            good with { Target=1 }, good with { Condition=1 }, good with { Extra=new(1,0) },
            good with { Destination=new(0,0,IsMemory:true) }, good with { Source=new(1,0,IsMemory:true) },
            good with { Address=new(16,-1,0,0) }, good with { Address=new(3,-1,0,0,true) } })
        {
            bool rejected=false; try { FullBlockEmitter.ValidateStackMemory(bad); } catch (ArgumentException) { rejected=true; }
            Check(rejected, "R85 malformed typed stack IR rejects before emission");
        }
    }

    static void StackMemoryPointers(ref FullStackMemoryRequest request, byte* firstPage, byte* nextPage)
    {
        request.Length0=Math.Min(request.Width/8,4096u-(uint)(request.Address&4095));
        request.Host0=firstPage+(int)(request.Address&4095);
        request.Host1=request.Length0==request.Width/8?null:nextPage;
    }

    static ulong StackMemoryRead(byte* first, byte* second, int firstLength, int width)
    {
        ulong value=0;
        for(int b=0;b<width/8;++b) value|=(ulong)(b<firstLength?first[b]:second[b-firstLength])<<(8*b);
        return value;
    }

    static void StackMemoryWrite(byte* first, byte* second, int firstLength, int width, ulong value)
    {
        for(int b=0;b<width/8;++b)
            if(b<firstLength) first[b]=(byte)(value>>(8*b)); else second[b-firstLength]=(byte)(value>>(8*b));
    }

    static void StackMemorySeed(byte* machine, JitInstruction i, ulong read, ulong write)
    {
        ResetFull(machine);
        Write64(machine,48,i.Operation==JitOperation.Pop?write:read);
        Write64(machine,56,i.Operation==JitOperation.Leave?0x700080:
            i.Operation is JitOperation.Push or JitOperation.IndirectCall?write+(uint)(i.Width/8):read);
        Write64(machine,64,i.Operation==JitOperation.Leave?read:0x8877665544332211);
        ((Blink.Machine*)machine)->flags=0xa52c0e15u&~0x50100u;
    }

    static void CheckFullStackMemoryLeaves(byte* machine)
    {
        CheckFullStackMemoryComputed(machine);
        CheckFullStackMemoryRspAddresses(machine);
        CheckFullStackMemoryTlb(machine);
        CheckFullStackMemoryAuthentication(machine);
        CheckFullStackMemoryPollAndAbi(machine);
        CheckFullStackMemoryCallCache(machine);
    }

    static void CheckFullStackMemoryComputed(byte* machine)
    {
        byte* p=(byte*)NativeMemory.Alloc(4096),p1=(byte*)NativeMemory.Alloc(4096);
        byte* q=(byte*)NativeMemory.Alloc(4096),q1=(byte*)NativeMemory.Alloc(4096);
        try
        {
            foreach(byte[] encoding in new byte[][] { [0xff,0x33],[0x66,0xff,0x33], [0x8f,0x03],
                [0x66,0x8f,0x03],[0xff,0x13],[0x66,0x48,0xff,0x33],[0x66,0x48,0x8f,0x03],[0x66,0x48,0xc9],[0xc9],[0x66,0xc9] })
            foreach(int offset in new[]{0,1,4095})
            foreach(bool ticket in new[]{false,true})
            {
                var i=StackMemoryClassify(encoding); int bytes=i.Width/8;
                ulong read=0x100000+(uint)offset,write=0x302000+(uint)offset,value=0x123456789abc;
                new Span<byte>(p,4096).Fill(0xa5);new Span<byte>(p1,4096).Fill(0x5a);
                new Span<byte>(q,4096).Fill(0xcc);new Span<byte>(q1,4096).Fill(0xdd);
                int n=Math.Min(bytes,4096-offset);StackMemoryWrite(p+offset,p1,n,i.Width,value);
                byte[] source0=new ReadOnlySpan<byte>(p,4096).ToArray(),source1=new ReadOnlySpan<byte>(p1,4096).ToArray();
                byte[] expected0=new ReadOnlySpan<byte>(q,4096).ToArray(),expected1=new ReadOnlySpan<byte>(q1,4096).ToArray();
                ulong stored=i.Operation==JitOperation.IndirectCall?Ip+(uint)i.Length:value;
                if(i.Operation!=JitOperation.Leave)
                    for(int b=0;b<bytes;++b) if(b<n) expected0[offset+b]=(byte)(stored>>(8*b)); else expected1[b-n]=(byte)(stored>>(8*b));
                StackMemorySeed(machine,i,read,write);
                ulong beforeBp=((ulong*)(machine+64))[0];uint flags=((Blink.Machine*)machine)->flags;
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                using var code=CreateFullBlock([new(i)]);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
                f.StackMemory=&d;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryResume==1&&
                    d.Count==(i.Operation==JitOperation.Leave?1u:2u)&&d.State==FullStackMemoryState.Request&&
                    d.Request0.Address==read&&d.Request0.Access==FullMemoryAccess.Read&&d.Request0.Width==i.Width&&
                    (d.Count==1||d.Request1.Address==write&&d.Request1.Access==FullMemoryAccess.Write)&&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R85 both native requests precede every CPU/memory effect");
                StackMemoryPointers(ref d.Request0,p,p1);if(d.Count==2) StackMemoryPointers(ref d.Request1,q,q1);
                d.Epoch=epoch;d.Ticket=ticket?123u:0u;d.State=ticket?FullStackMemoryState.TicketReady:FullStackMemoryState.Ready;
                if(ticket){f.MemoryFlags=FullMemoryFlags.BarrierTicket;f.MemoryEpoch=epoch;}
                if(ticket)f.Executed=uint.MaxValue;
                // The already approved issue survives a now-exhausted budget.
                f.Budget=0;FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                    f.Executed==(ticket?(ulong)uint.MaxValue+1:1)&&f.NativeLimit==0&&f.NativeMemoryComputations==1&&
                    f.MemoryResume==0&&f.FirstPreissued==0&&d.State==FullStackMemoryState.Consumed&&d.Ticket==0&&
                    f.MemoryFlags==(ticket?FullMemoryFlags.BarrierConsumed:FullMemoryFlags.None),"R85 Ready/ticket resumes and commits exactly one approved issue");
                ulong expectedRsp=i.Operation is JitOperation.Push or JitOperation.IndirectCall?write:read+(uint)bytes;
                ulong expectedBp=i.Operation==JitOperation.Leave?(i.Width==16?(beforeBp&~65535UL)|(value&65535):value):beforeBp;
                Check(((ulong*)(machine+56))[0]==expectedRsp&&((ulong*)(machine+64))[0]==expectedBp&&
                    ((Blink.Machine*)machine)->flags==flags&&machine[8]==0&&
                    ((ulong*)machine)[0]==(i.Operation==JitOperation.IndirectCall?value:Ip+(uint)i.Length),
                    "R85 PUSH/POP/CALL/LEAVE native data, full RSP, partial BP, flags and final IP");
                Check(new ReadOnlySpan<byte>(p,4096).SequenceEqual(source0)&&new ReadOnlySpan<byte>(p1,4096).SequenceEqual(source1)&&
                    new ReadOnlySpan<byte>(q,4096).SequenceEqual(expected0)&&new ReadOnlySpan<byte>(q1,4096).SequenceEqual(expected1),
                    "R85 exact widths and both noncontiguous page pairs preserve all sentinels");
            }
        }
        finally {NativeMemory.Free(p);NativeMemory.Free(p1);NativeMemory.Free(q);NativeMemory.Free(q1);}
    }

    static void CheckFullStackMemoryRspAddresses(byte* machine)
    {
        byte* page=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            foreach(byte[] encoding in new byte[][] { [0x8f,0x04,0x24],[0x8f,0x44,0x24,0xf8],
                [0x66,0x8f,0x44,0x24,0xfe],[0x67,0x8f,0x04,0x24],[0x64,0x67,0x8f,0x04,0x24],
                [0xff,0x34,0x24],[0xff,0x14,0x24],[0xff,0x74,0x24,0xf8],[0xff,0x54,0x24,0xf8] })
            {
                var i=StackMemoryClassify(encoding);bool pop=i.Operation==JitOperation.Pop;
                ulong oldRsp=i.Address.AddressWidth==32?0x100300080UL:0x300080UL,fs=i.Address.Segment==4?0x100000UL:0;
                ResetFull(machine);Write64(machine,56,oldRsp);
                ulong explicitAddress=unchecked(oldRsp+(pop?(uint)(i.Width/8):0)+(ulong)i.Address.Displacement);
                if(i.Address.AddressWidth==32)explicitAddress=(uint)explicitAddress;explicitAddress+=fs;
                ulong read=pop?oldRsp:explicitAddress,write=pop?explicitAddress:oldRsp-(uint)(i.Width/8);
                ulong value=Ip+16;int readOffset=(int)(read&4095),writeOffset=(int)(write&4095);
                new Span<byte>(page,4096).Clear();StackMemoryWrite(page+readOffset,null,i.Width/8,i.Width,value);
                using var code=CreateFullBlock([new(i)]);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
                f.StackMemory=&d;f.FsBase=fs;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&d.Request0.Address==read&&d.Request1.Address==write&&
                    ((ulong*)(machine+56))[0]==oldRsp,"R85 PUSH/CALL source oldRSP; POP destination postRSP before EA32/FS");
                // Two different guest addresses can name overlapping physical
                // bytes. Load the source before even an identical write target.
                d.Request0.Host0=page+readOffset;d.Request0.Length0=(uint)(i.Width/8);
                d.Request1.Host0=page+writeOffset;d.Request1.Length0=(uint)(i.Width/8);
                d.Epoch=epoch;d.State=FullStackMemoryState.Ready;FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                    ((ulong*)(machine+56))[0]==(pop?oldRsp+(uint)(i.Width/8):oldRsp-(uint)(i.Width/8))&&
                    StackMemoryRead(page+writeOffset,null,i.Width/8,i.Width)==
                        (i.Operation==JitOperation.IndirectCall?Ip+(uint)i.Length:i.Width==16?value&65535:value),
                    "R85 physical alias captures source before stack/destination store");
            }
        }
        finally{NativeMemory.Free(page);}
    }

    static void CheckFullStackMemoryTlb(byte* machine)
    {
        byte* p=(byte*)NativeMemory.AllocZeroed(8192),q=(byte*)NativeMemory.AllocZeroed(8192);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(int failure in new[]{0,1,2,3,4,5})
            {
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();new Span<byte>(q,8192).Fill(0xcc);
                var i=StackMemoryClassify([0xff,0x33]);ulong read=0x100fff,write=0x302fff;
                StackMemorySeed(machine,i,read,write);p[4095]=0x11;for(int b=0;b<7;++b)p[4096+b]=(byte)(0x22+b);
                SetFullMemoryEntry(tlb,0x100000,p,FullMemoryAccess.Read);SetFullMemoryEntry(tlb,0x101000,p+4096,FullMemoryAccess.Read);
                SetFullMemoryEntry(tlb,0x302000,q);SetFullMemoryEntry(tlb,0x303000,q+4096);
                if(failure==1)tlb[FullNativeMemory.TlbIndex(0x101000,63)].Host=null;
                if(failure==2)tlb[FullNativeMemory.TlbIndex(0x303000,63)].Host=null;
                if(failure==3)tlb[FullNativeMemory.TlbIndex(0x302000,63)].Permissions=FullMemoryAccess.Read;
                if(failure==4)tlb[FullNativeMemory.TlbIndex(0x302000,63)].Flags=FullMemoryFlags.WriteBarrier;
                if(failure==5)tlb[FullNativeMemory.TlbIndex(0x100000,63)].AuthorityEpoch=10;
                using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;
                var h=FullHeader(Ip,code.Entry);var d=new FullStackMemoryDescriptor{Version=1};
                if(failure==0)d.State=FullStackMemoryState.Ready; // Count0 is empty even after a canceled assist
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);f.StackMemory=&d;f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                if(failure==0)
                    Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                        StackMemoryRead(q+4095,q+4096,1,64)==StackMemoryRead(p+4095,p+4096,1,64),"R85 native two-access TLB hit checks four pointers before crossing store");
                else
                    Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&
                        f.MemoryAddress==(failure is 1 or 5?read:write)&&d.Request0.Address==read&&d.Request1.Address==write&&
                        new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before)&&q[4095]==0xcc&&q[4096]==0xcc,
                        "R85 source/second-page/readonly/watch/epoch miss has no partial effects");
            }
        }
        finally{NativeMemory.Free(p);NativeMemory.Free(q);}
    }

    static void CheckFullStackMemoryAuthentication(byte* machine)
    {
        byte* page=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            foreach(string invalid in new[]{"count","site","address","access","width","length","host0","host1","ticket","epoch"})
            {
                var i=StackMemoryClassify([0xff,0x33]);StackMemorySeed(machine,i,0x100080,0x300080);
                using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;
                var h=FullHeader(Ip,code.Entry);var d=new FullStackMemoryDescriptor{Version=1};
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R85 authentication setup reserves one issue");
                StackMemoryPointers(ref d.Request0,page,null);StackMemoryPointers(ref d.Request1,page,null);
                d.Epoch=epoch;d.Ticket=123;d.State=FullStackMemoryState.TicketReady;
                f.MemoryFlags=FullMemoryFlags.BarrierTicket;f.MemoryEpoch=epoch;
                switch(invalid)
                {
                    case "count":d.Count=1;break;case "site":d.Site=1;break;
                    case "address":++d.Request1.Address;break;case "access":d.Request1.Access=FullMemoryAccess.Read;break;
                    case "width":d.Request1.Width=16;break;case "length":d.Request1.Length0=7;break;
                    case "host0":d.Request1.Host0=null;break;case "host1":d.Request1.Host1=page;break;
                    case "ticket":d.Ticket=0;break;case "epoch":d.Epoch=10;break;
                }
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Invalidated&&
                    issued==1&&completed==0&&f.NativeMemoryComputations==0&&f.MemoryFlags==FullMemoryFlags.BarrierTicket&&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R169 BOTH requests authenticate; stale owned ticket is rejected intact before native load");
                if(invalid=="epoch")Check(d.Ticket==123&&d.State==FullStackMemoryState.TicketReady,
                    "R169 stale ticket remains owned until controller cleanup after RET");
            }
        }
        finally{NativeMemory.Free(page);}
    }

    static void CheckFullStackMemoryTicketTuples(byte* machine)
    {
        byte* page=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            foreach(byte[] encoding in new byte[][]{[0xff,0x13],[0xff,0x33],[0x8f,0x03],[0xc9]})
            foreach(string bad in new[]{"frame-only","frame-value","descriptor-only","count0","request","consumed","ready","site","epoch","memory-epoch"})
            {
                var i=StackMemoryClassify(encoding);StackMemorySeed(machine,i,0x100080,0x300080);
                using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;
                var h=FullHeader(Ip,code.Entry);var d=new FullStackMemoryDescriptor{Version=1};
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R170 tuple setup issues once before mapping");
                StackMemoryPointers(ref d.Request0,page,null);if(d.Count==2)StackMemoryPointers(ref d.Request1,page,null);
                d.State=FullStackMemoryState.TicketReady;d.Ticket=123;d.Epoch=epoch;
                f.MemoryFlags=FullMemoryFlags.BarrierTicket;f.MemoryEpoch=epoch;
                switch(bad)
                {
                    case "frame-only":d.Count=0;d.State=FullStackMemoryState.Request;d.Ticket=0;f.MemoryTicket=123;break;
                    case "frame-value":f.MemoryTicket=123;break;
                    case "descriptor-only":f.MemoryFlags=FullMemoryFlags.None;break;
                    case "count0":d.Count=0;break;
                    case "request":d.State=FullStackMemoryState.Request;break;
                    case "consumed":d.State=FullStackMemoryState.Consumed;break;
                    case "ready":d.State=FullStackMemoryState.Ready;break;
                    case "site":++d.Site;break;
                    case "epoch":--d.Epoch;f.MemoryEpoch=d.Epoch;break;
                    case "memory-epoch":--f.MemoryEpoch;break;
                }
                var flags=f.MemoryFlags;ulong marker=f.MemoryTicket,ticket=d.Ticket;
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Invalidated&&issued==1&&completed==0&&
                    f.MemoryFlags==flags&&f.MemoryTicket==marker&&d.Ticket==ticket&&
                    f.NativeMemoryComputations==0&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                    $"R170 incoming dense ownership preserved on rejection: {i.Operation}/{bad}");
            }
        }
        finally{NativeMemory.Free(page);}
    }

    static void CheckFullStackMemoryPollAndAbi(byte* machine)
    {
        var i=StackMemoryClassify([0xff,0x33]);using var code=CreateFullBlock([new(i)]);
        foreach(string boundary in new[]{"stop","attention","budget","abi4","size384"})
        {
            StackMemorySeed(machine,i,0x100080,0x300080);ulong epoch=11;long issued=0,completed=0;int stop=boundary=="stop"?1:0;
            var h=FullHeader(Ip,code.Entry);var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:0);
            ((Blink.Machine*)machine)->attention=boundary=="attention";
            f.StackMemory=(FullStackMemoryDescriptor*)1; // must not be inspected before the boundary
            if(boundary=="abi4")f.Version=0x46420004;if(boundary=="size384")f.Size=384;
            var expected=boundary=="stop"?FullBlockExit.Stop:boundary=="attention"?FullBlockExit.Attention:
                boundary=="budget"?FullBlockExit.Budget:FullBlockExit.Invalidated;
            Check(RunFull(code,&f)==expected&&completed==0&&issued==(boundary=="budget"?1:0)&&
                ((ulong*)machine)[0]==Ip&&((ulong*)(machine+56))[0]==0x300088,"R85 poll/budget/old ABI reject before descriptor or stack effects");
        }
        byte* page=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            StackMemorySeed(machine,i,0x100080,0x300080);ulong epoch=11;long issued=0,completed=0;int stop=0;
            var h=FullHeader(Ip,code.Entry);var d=new FullStackMemoryDescriptor{Version=1};
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
            Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R85 same-issue stop setup");
            StackMemoryPointers(ref d.Request0,page,null);StackMemoryPointers(ref d.Request1,page,null);
            d.State=FullStackMemoryState.Ready;d.Epoch=epoch;FullNativeMemory.ResumeSameIssue(&f);stop=1;
            Check(RunFull(code,&f)==FullBlockExit.Stop&&issued==1&&completed==0&&f.FirstPreissued==1,"R85 stop retains approved issue before re-entry");
            stop=0;f.Budget=0;
            Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1,"R85 resumed issue ignores later exhausted budget once");
        }
        finally{NativeMemory.Free(page);}
    }

    static void CheckFullStackMemoryCallCache(byte* machine)
    {
        byte* page=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            var i=StackMemoryClassify([0xff,0x13]);using var code=CreateFullBlock([new(i)]);
            using var target=CreateFullBlock([new(new(Ip+16,1,JitOperation.Nop,64,default,default))]);
            FullNativeJumpEntry* cache=stackalloc FullNativeJumpEntry[256];
            foreach(int stale in new[]{0,1,2,3})
            {
                new Span<FullNativeJumpEntry>(cache,256).Clear();StackMemorySeed(machine,i,0x100080,0x300080);
                ((ulong*)(page+128))[0]=Ip+16;ulong epoch=11,generation=123;long issued=0,completed=0;int stop=0;
                var h=FullHeader(Ip,code.Entry);var t=FullHeader(Ip+16,target.Entry,18);t.Reserved136=1;
                FullNativeMemory.PublishJump(cache,255,generation,&t,in t);
                if(stale==1){++generation;cache[FullNativeMemory.JumpIndex(Ip+16,255)].Header=(TranslationBlockHeader*)1;}
                if(stale==2)t.ValidatedGlobalEpoch=10;if(stale==3)t.Reserved136=0;
                var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:2);
                f.StackMemory=&d;f.JumpCache=cache;f.JumpCacheMask=255;f.StoreGenerationPointer=&generation;f.ObservedStoreGeneration=123;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R85 memory CALL cache setup");
                StackMemoryPointers(ref d.Request0,page,null);StackMemoryPointers(ref d.Request1,page,null);
                // Overlap source and return-address store; target must survive.
                d.State=FullStackMemoryState.Ready;d.Epoch=epoch;FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Continue&&
                    completed==(stale==0?2:1)&&issued==completed&&f.TailJumps==(stale==0?1u:0u)&&
                    ((ulong*)machine)[0]==(stale==0?Ip+17:Ip+16)&&((ulong*)(page+128))[0]==Ip+(uint)i.Length,
                    "R85 memory CALL uses guarded existing cache; stable generation precedes poisoned target");
            }
        }
        finally{NativeMemory.Free(page);}
    }

    static void CheckFullStackMemoryOracle(StackFixture f)
    {
        // Successful selected normal paths only. LEAVE16 is independently
        // checked above: selected stack.c's osz0 path is not a valid oracle.
        foreach(byte[] encoding in new byte[][] { [0xff,0x33],[0x66,0xff,0x33],
            [0x8f,0x03],[0x66,0x8f,0x03],[0xff,0x13],[0xc9],[0xff,0x34,0x24],[0xff,0x14,0x24] })
        foreach(int offset in new[]{64,4095})
        {
            var i=StackMemoryClassify(encoding);byte* m=(byte*)f.Machine();MemoryIntegerSeed(m);
            ulong read=StackFixture.Base+(uint)offset,write=StackFixture.Base+8192+(uint)offset;
            Write64(m,48,i.Operation==JitOperation.Pop?write:read);
            Write64(m,56,i.Operation is JitOperation.Push or JitOperation.IndirectCall?write+(uint)(i.Width/8):read);
            Write64(m,64,read);
            bool rspSource=i.Source.IsMemory&&i.Address.Base==4;
            if(rspSource)read=((ulong*)(m+56))[0];
            byte* p0=(byte*)f.Physical(read&~4095UL);
            byte* p1=(read&4095)+(uint)(i.Width/8)>4096?(byte*)f.Physical((read&~4095UL)+4096):null;
            byte* q0=(byte*)f.Physical(write&~4095UL),q1=(byte*)f.Physical((write&~4095UL)+4096);
            int sourceOffset=(int)(read&4095);ulong value=Ip+16;
            StackMemoryWrite(p0+sourceOffset,p1,Math.Min(i.Width/8,4096-sourceOffset),i.Width,value);
            byte[][] pages=new byte[4][];nint[] pointers=[f.Physical(StackFixture.Base),f.Physical(StackFixture.Base+4096),
                f.Physical(StackFixture.Base+8192),f.Physical(StackFixture.Base+12288)];
            for(int p=0;p<4;++p)pages[p]=new ReadOnlySpan<byte>((void*)pointers[p],4096).ToArray();
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            f.DataScope(0,()=>IntegerReference(f.Program,(byte*)f.Machine(),encoding));
            byte[] expected=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();byte[][] after=new byte[4][];
            for(int p=0;p<4;++p)after[p]=new ReadOnlySpan<byte>((void*)pointers[p],4096).ToArray();
            before.CopyTo(new Span<byte>(m,before.Length));for(int p=0;p<4;++p)pages[p].CopyTo(new Span<byte>((void*)pointers[p],4096));
            using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullStackMemoryDescriptor{Version=1};var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop,limit:1);frame.StackMemory=&d;
            Check(RunFull(code,&frame)==FullBlockExit.NeedMemory,"R85 initialized selected oracle mapping setup");
            StackMemoryPointers(ref d.Request0,p0,p1);if(d.Count==2)StackMemoryPointers(ref d.Request1,q0,q1);
            d.Epoch=epoch;d.State=FullStackMemoryState.Ready;FullNativeMemory.ResumeSameIssue(&frame);
            Check(RunFull(code,&frame)==FullBlockExit.Continue&&issued==1&&completed==1&&
                new ReadOnlySpan<byte>(m,9).SequenceEqual(expected.AsSpan(0,9))&&
                new ReadOnlySpan<byte>(m+12,4).SequenceEqual(expected.AsSpan(12,4))&&
                new ReadOnlySpan<byte>(m+24,128).SequenceEqual(expected.AsSpan(24,128))&&
                new ReadOnlySpan<byte>(m+160,256).SequenceEqual(expected.AsSpan(160,256))&&
                new ReadOnlySpan<byte>(m+456,236).SequenceEqual(expected.AsSpan(456,236)),
                "R85 selected normal GetOp successful architectural state and unchanged vector/FP state");
            for(int p=0;p<4;++p)Check(new ReadOnlySpan<byte>((void*)pointers[p],4096).SequenceEqual(after[p]),"R85 selected normal GetOp all four pages");
        }
    }

    static FullBlockFrame StackMemoryMmuRequest(FullStackMemoryDescriptor* descriptor, FullMemoryTlbEntry* tlb)
        =>new(){Version=FullBlockFrame.AbiVersion,Size=(uint)sizeof(FullBlockFrame),MemoryResume=1,MemoryWidth=descriptor->Request0.Width,
            MemoryAccess=FullMemoryAccess.Read,MemoryAddress=descriptor->Request0.Address,Context=0x1234,
            MemoryTlb=tlb,MemoryTlbMask=63,StackMemory=descriptor};

    static FullStackMemoryDescriptor StackMemoryRequest(ulong read,ulong write,int width=64)
        =>new(){Version=1,Count=2,State=FullStackMemoryState.Request,
            Request0=new(){Address=read,Access=FullMemoryAccess.Read,Width=(uint)width},
            Request1=new(){Address=write,Access=FullMemoryAccess.Write,Width=(uint)width}};

    static void CheckFullStackMemoryMmu()
    {
        using var f=new StackFixture();const ulong read=StackFixture.Base,write=read+0x42000; // current folded-index collision
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        f.Map(read,8192);f.Fill(read,8192);f.Map(write,8192);f.Fill(write,8192);
        var d=StackMemoryRequest(read+4095,write+4095);var frame=StackMemoryMmuRequest(&d,tlb);
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.Resolved&&d.State==FullStackMemoryState.Ready&&
            d.Request0.Host0!=null&&d.Request0.Host1!=null&&d.Request1.Host0!=null&&d.Request1.Host1!=null&&
            d.Request0.Length0==1&&d.Request1.Length0==1,"R85 real MMU returns four checked pointers despite TLB collision");
        foreach(bool destination in new[]{false,true})
        {
            d=StackMemoryRequest(destination?read+4095:read+8192,write+8192);
            frame=StackMemoryMmuRequest(&d,tlb);
            Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.GuestFault&&frame.FaultTrap==14&&frame.FaultCode==1&&
                frame.FaultAddress==(destination?write+8192:read+8192)&&d.State!=FullStackMemoryState.Ready,
                "R85 source MAPERR precedes destination MAPERR; valid source reaches exact destination fault");
        }
        Check(f.Mutate("mprotect",write+4096)==0,"R85 destination second page readonly setup");
        d=StackMemoryRequest(read+4095,write+4095);frame=StackMemoryMmuRequest(&d,tlb);
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.GuestFault&&frame.FaultCode==2&&frame.FaultAddress==write+4096,
            "R85 both destination pages need write permission before effects");
        const ulong lazy=write+8192;f.Map(lazy);
        d=StackMemoryRequest(lazy,write);frame=StackMemoryMmuRequest(&d,tlb);
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.DemandPage&&frame.MemoryAddress==lazy&&
            frame.MemoryAccess==FullMemoryAccess.Read&&d.Request1.Address==write,"R85 source lazy mapping summary preserves both EAs");
        d=StackMemoryRequest(read,lazy);frame=StackMemoryMmuRequest(&d,tlb);
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.DemandPage&&frame.MemoryAddress==lazy&&
            frame.MemoryAccess==FullMemoryAccess.Write&&frame.MemoryWidth==64,"R85 destination lazy materializer gets exact failing request");
        d=StackMemoryRequest(0x800000000000UL,write);frame=StackMemoryMmuRequest(&d,tlb);
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.GuestFault&&frame.FaultAddress==0x800000000000UL,
            "R85 noncanonical source rejected before destination mapping");
    }

    static void CheckFullStackMemoryController()
    {
        foreach(byte[] bytes in new byte[][]{[0xff,0x33],[0x8f,0x03],[0xff,0x13],[0xc9],[0x66,0xc9]})
        {
            using var f=new StackFixture();const ulong code=StackFixture.Base,source=code+8192,destination=code+0x42000;
            f.Map(code);f.Fill(code);f.Map(source,8192);f.Fill(source,8192);f.Map(destination,8192); // lazy write stack
            WriteControllerCode(f,code,[0x90,..bytes,0xeb,0xfe]);
            var i=StackMemoryClassify(bytes);var m=f.Machine();ulong read=source+4095,write=destination+4095;
            Write64((byte*)m,48,i.Operation==JitOperation.Pop?write:read);
            Write64((byte*)m,56,i.Operation==JitOperation.Leave?0x700080:
                i.Operation is JitOperation.Push or JitOperation.IndirectCall?write+(uint)(i.Width/8):read);
            Write64((byte*)m,64,read);
            f.DataScope(0,()=>
            {
                byte* p=f.Program.LookupAddress2(m,(long)source,0,0),p1=f.Program.LookupAddress2(m,(long)(source+4096),0,0);
                StackMemoryWrite(p+4095,p1,1,i.Width,code+1+(uint)bytes.Length);
            });
            // Keep this fixture at exactly the intended postcommit boundary;
            // the ordinary product default remains unbounded native fuel.
            Action<nint> cap=address=>((FullBlockFrame*)address)->NativeLimit=1;
            f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;
            f.Counts(2);using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            Check(f.Step(code),"R85 controller initial checked code cap");
            for(int attempt=0;f.Completed<2&&attempt<8;++attempt)Check(f.Continue(),"R85 bounded same-issue controller continuation");
            Check(f.Completed==2&&f.Issued==2&&m->ip==code+1+(uint)bytes.Length&&m->oplen==0&&
                f.Statistics.NativeMemoryInstructions==1&&f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                "R85 actual controller lazy two-access/LEAVE native completion without semantic helper");
            var stats=f.Jit.Store.GetStatistics();Check(stats.ActiveNativeEpochs==0&&stats.ActiveManagedLeases==0&&
                f.Statistics.ActiveDataBorrows==0,"R85 controller releases all native/store/passive lifetimes");
            f.NativeReturned=null;
        }
        foreach(bool sourceFault in new[]{false,true})
        foreach(byte[] op in new byte[][]{[0xff,0x33],[0x8f,0x03],[0xff,0x13],[0xc9],[0x66,0xc9]})
        {
            var i=StackMemoryClassify(op);bool pop=i.Operation==JitOperation.Pop;
            if(i.Operation==JitOperation.Leave&&!sourceFault)continue;
            using var f=new StackFixture();const ulong code=StackFixture.Base,data=code+8192,missing=data+8192;
            f.Map(code);f.Fill(code);f.Map(data);f.Fill(data);
            WriteControllerCode(f,code,[0x90,..op]);
            ulong read=sourceFault?missing:data,write=sourceFault?missing+4096:missing;
            Write64((byte*)f.Machine(),48,pop?write:read);
            Write64((byte*)f.Machine(),56,i.Operation==JitOperation.Leave?0x700080:pop?read:write+8);
            Write64((byte*)f.Machine(),64,read);
            ulong rsp=((ulong*)((byte*)f.Machine()+56))[0];uint flags=f.Machine()->flags;
            f.Counts(100);using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var signal=f.StepSynchronousFault(code);
            Check(signal.Signal==11&&f.Machine()->trapno==14&&f.Machine()->faultaddr==(long)(sourceFault?read:write)&&
                f.Machine()->ip==code+1&&f.Machine()->oplen==0&&((ulong*)((byte*)f.Machine()+56))[0]==rsp&&f.Machine()->flags==flags&&
                f.Issued==2&&f.Completed==1&&f.Statistics.SemanticHelperInstructions==0,"R85 real controller source-first fault before RSP/IP/flags effects");
        }
    }

    static void CheckFullStackMemoryChangedMapping()
    {
        using var f=new StackFixture();const ulong read=StackFixture.Base,write=read+8192;
        f.Map(read);f.Fill(read);f.Map(write);f.Fill(write);
        var i=StackMemoryClassify([0xff,0x33]);byte* m=(byte*)f.Machine();MemoryIntegerSeed(m);
        Write64(m,48,read+64);Write64(m,56,write+72);
        using var code=CreateFullBlock([new(i)]);var h=FullHeader(Ip,code.Entry);
        h.ValidatedGlobalEpoch=f.Jit.Authority.CodeEpoch;
        long issued=0,completed=0;int stop=0;FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        new Span<FullMemoryTlbEntry>(tlb,64).Clear();var d=new FullStackMemoryDescriptor{Version=1};
        var frame=FullFrame(m,&h,f.Jit.Authority.EpochPointer,&issued,&completed,&stop,limit:1,
            codeEpoch:f.Jit.Authority.CodeEpochPointer);frame.StackMemory=&d;frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
        Check(RunFull(code,&frame)==FullBlockExit.NeedMemory&&FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.Resolved,
            "R85 changed mapping starts with both requests checked");
        ulong oldEpoch=d.Epoch;
        // No native invocation overlaps this actual mapping replacement. The
        // next entry must reject the old descriptor BEFORE its first data load.
        Check(f.Mutate("MAP_FIXED",read)==0,"R85 actual source mapping replacement");f.Fill(read);
        f.DataScope(0,()=>
        {
            byte* p=f.Program.LookupAddress2(f.Machine(),(long)read,0,0);
            ((ulong*)(p+64))[0]=0x8877665544332211;
        });
        frame.ObservedGlobalEpoch=f.Jit.Authority.Epoch;h.ValidatedGlobalEpoch=f.Jit.Authority.CodeEpoch;
        frame.ObservedCodeAuthorityEpoch=h.ValidatedGlobalEpoch;
        byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();FullNativeMemory.ResumeSameIssue(&frame);
        Check(oldEpoch!=frame.ObservedGlobalEpoch&&RunFull(code,&frame)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&
            d.State==FullStackMemoryState.Request&&new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),
            "R85 stale mapping descriptor retries setup with original issue and no CPU effects");
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.Resolved,"R85 both requests rechecked after remap");
        frame.ObservedGlobalEpoch=f.Jit.Authority.Epoch;frame.ObservedCodeAuthorityEpoch=f.Jit.Authority.CodeEpoch;
        h.ValidatedGlobalEpoch=frame.ObservedCodeAuthorityEpoch;FullNativeMemory.ResumeSameIssue(&frame);
        Check(RunFull(code,&frame)==FullBlockExit.Continue&&issued==1&&completed==1&&
            ((ulong*)((byte*)f.Physical(write)+64))[0]==0x8877665544332211,
            "R85 re-entry loads changed mapping natively rather than reusing a pre-assist scalar value");
    }

    static void CheckFullStackMemoryWatchedDestination()
    {
        using var f=new StackFixture();const ulong code=StackFixture.Base,source=code+8192,write=code+256;
        f.Map(code);f.Fill(code);f.Map(source);f.Fill(source);
        WriteControllerCode(f,code,[0x90,0xff,0x33,0xeb,0xfe]);
        f.DataScope(0,()=>
        {
            byte* p=f.Program.LookupAddress2(f.Machine(),(long)source,0,0);
            ((ulong*)(p+64))[0]=0x1122334455667788;
        });
        Write64((byte*)f.Machine(),48,source+64);Write64((byte*)f.Machine(),56,write+8);
        Action<nint> cap=address=>((FullBlockFrame*)address)->NativeLimit=1;
        f.NativePrepared=cap;bool consumed=false;
        f.NativeReturned=address=>
        {
            var frame=(FullBlockFrame*)address;
            consumed|=frame->MemoryFlags==FullMemoryFlags.BarrierConsumed;
            f.NativePrepared=cap;
        };
        f.Counts(2);using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
        Check(f.Step(code),"R85 watched destination initial code acquisition");
        for(int attempt=0;f.Completed<2&&attempt<8;++attempt)Check(f.Continue(),"R85 watched destination bounded continuation");
        f.NativeReturned=null;
        Check(consumed&&f.Issued==2&&f.Completed==2&&f.Machine()->ip==code+3&&
            ((ulong*)((byte*)f.Physical(code)+256))[0]==0x1122334455667788&&
            f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
            "R85 actual code-watched stack destination requires checked exclusive ticket and generated store");
        var s=f.Jit.Store.GetStatistics();Check(s.ActiveNativeEpochs==0&&s.ActiveManagedLeases==0&&
            f.Statistics.ActiveDataBorrows==0,"R85 watched destination releases ticket/native/store/passive lifetime");
    }

    static void CheckFullStackMemoryEmission()
    {
        var i=StackMemoryClassify([0xff,0x13]);FullBlockInstruction[] sites=[new(i)];
        byte[] buffer=new byte[FullBlockEmitter.CodeCapacity],shortBuffer=new byte[1];
        foreach(bool windows in new[]{false,true})
        {
            int length=FullBlockEmitter.Emit(sites,buffer,windowsAbi:windows);
            Check(length>0&&buffer[6]==(windows?(byte)0xca:(byte)0xfa),"R85 both host ABI prologues encode volatile leaf entry");
            bool rejected=false;try{FullBlockEmitter.Emit(sites,shortBuffer,windowsAbi:windows);}catch(ArgumentException){rejected=true;}
            Check(rejected,"R85 short native destination rejected");
        }
        for(int n=0;n<8;++n)FullBlockEmitter.Emit(sites,buffer);long before=GC.GetAllocatedBytesForCurrentThread();
        for(int n=0;n<32;++n)FullBlockEmitter.Emit(sites,buffer);
        Check(GC.GetAllocatedBytesForCurrentThread()==before,"R85 warmed span emission adds no per-instruction allocation");
    }
}
