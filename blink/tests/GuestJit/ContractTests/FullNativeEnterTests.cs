using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using EnterFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // Authored UNRUN. Parent alone owns builds, emission and execution.
    static void CheckFullNativeEnter(byte* machine)
    {
        Check(FullBlockEmitter.HasExpectedEnterLayout(), "R127 strict ABI6/frame400/header160/descriptor128 layout");
        CheckEnterGrammar();
        CheckEnterAllAllocations(machine);
        CheckEnterBoundaries(machine);
        CheckEnterPolling(machine);
        CheckEnterCanonical(machine);
        CheckEnterTicketFuel(machine);
        using (var owner = CreateFullScaffold(out var scaffold))
        {
            fullOwnerScaffold = scaffold;
            try { CheckEnterBoundaries(machine); CheckEnterPolling(machine); CheckEnterCanonical(machine); CheckEnterTicketFuel(machine); }
            finally { fullOwnerScaffold = default; }
        }
        CheckEnterOracle(); CheckEnterMmu(); CheckEnterController(); CheckEnterControllerFaults(); CheckEnterWatchedController(); CheckEnterEncoding();
        ResetFull(machine); // only the caller-owned SYNTHETIC machine
    }

    static byte[] EnterEncoding(int width, int allocation, byte nesting = 0) => width == 16 ?
        [0x66, 0xc8, (byte)allocation, (byte)(allocation >> 8), nesting] :
        [0xc8, (byte)allocation, (byte)(allocation >> 8), nesting];

    static JitInstruction EnterClassify(byte[] bytes)
    {
        Check(FullEnterLowering.TryClassify(Ip, bytes, out var i, out var reason), reason ?? "R127 ENTER classify");
        Check(FullBlockEmitter.CanInlineEnter(i) && FullBlockEmitter.ValidateEnter(i) && FullBlockEmitter.CanInline(i),
            "R127 ENTER classifier and parent root native seams");
        return i;
    }

    static void EnterSeed(byte* m, ulong rsp, ulong rbp, uint flags = 0xa52c0e15u & ~0x50100u)
    {
        // Safe for a real fixture: preserve system/bus/page tables/owned machine.
        var cpu = (Blink.Machine*)m; cpu->ip = Ip; cpu->oplen = 0; cpu->attention = false;
        cpu->mode.omode = Blink.XED_MODE_LONG; cpu->mode.genmode = Blink.XED_GEN_MODE_PROTECTED;
        cpu->flags = flags;
        for (int r = 0; r < 16; ++r) Write64(m, 24 + r * 8, 0xfedcba9876543210UL ^ (uint)r);
        Write64(m, 56, rsp); Write64(m, 64, rbp);
    }

    static void CheckEnterGrammar()
    {
        foreach (int width in new[] { 16, 64 })
        for (int allocation = 0; allocation <= ushort.MaxValue; ++allocation)
        {
            var i = EnterClassify(EnterEncoding(width, allocation));
            Check(i.Width == width && i.SourceWidth == width && i.Extra == new JitOperand(0, (uint)allocation, true) &&
                i.Options == 0 && i.Source == default && i.Destination == default && i.Address == new JitAddress(4,-1,0,0),
                "R127 complete unsigned imm16 grammar and canonical IR");
        }
        foreach (int raw in Enumerable.Range(1,255))
        {
            Check(!FullEnterLowering.TryClassify(Ip, EnterEncoding(64,0,(byte)raw),out _,out var why) &&
                why == ((raw&31)==0?"NativeEnterRawHighBitsSelectedDifference":"NativeEnterNestingMissing"),
                "R127 architecture-valid nesting1..31 and rawhigh aliases remain explicit MISSING");
        }
        foreach (byte[] bytes in new byte[][] { [],[0xc8],[0xc8,0,0],[0xc8,0,0,0,0x90],
            [0xf0,0xc8,0,0,0],[0xf2,0xc8,0,0,0],[0xf3,0xc8,0,0,0],
            [0x66,0xc8,0,0], [0xc9] })
            Check(!FullEnterLowering.TryClassify(Ip,bytes,out _,out _),"R127 exact span/LOCK/REP rejection");
        foreach ((byte[] prefix,int width) in new (byte[],int)[] {
            ([],64),([0x66],16),([0x48],64),([0x66,0x48],64),([0x48,0x66],16),
            ([0x66,0x48,0x40],16),([0x66,0x40,0x48],64),([0x66,0x66],16),
            ([0x65,0x64,0x67],64),([0x67,0x36,0x66],16),
            ([0x40,0x66,0x48,0x67,0x65,0x66,0x64,0x67,0x66,0x40,0x48],64) })
            Check(EnterClassify([..prefix,0xc8,0xff,0xff,0]).Width == width,"R127 effective prefixes/15-byte exact span");
        var good = EnterClassify([0xc8,0,0,0]);
        foreach (var bad in new[] {good with {Width=32},good with {SourceWidth=0},good with {Options=1},
            good with {Length=3},good with {Length=16},good with {Target=1},good with {Condition=1},
            good with {Source=new(5,0)},good with {Destination=new(4,0)},good with {Extra=default},
            good with {Extra=new(0,65536,true)},good with {Extra=new(1,0,true)},
            good with {Extra=new(0,0,true,ByteOffset:1)},good with {Extra=new(0,0,true,IsMemory:true)},
            good with {Address=new(4,-1,0,0,AddressWidth:32)},good with {Address=new(4,-1,0,0,Segment:2)} })
        {
            bool rejected=false;try {FullBlockEmitter.ValidateEnter(bad);}catch(ArgumentException){rejected=true;}
            Check(rejected,"R127 malformed metadata fails at compile time");
        }
    }

    static void EnterReady(ref FullStackMemoryDescriptor d, byte* memory, ulong guest, ulong epoch, bool ticket = false)
    {
        StackMemoryPointers(ref d.Request0, memory+(int)((d.Request0.Address&~4095UL)-guest),
            memory+(int)((d.Request0.Address&~4095UL)-guest)+4096);
        StackMemoryPointers(ref d.Request1, memory+(int)((d.Request1.Address&~4095UL)-guest),
            memory+(int)((d.Request1.Address&~4095UL)-guest)+4096);
        d.Epoch=epoch;d.Ticket=ticket?epoch:0;
        d.State=ticket?FullStackMemoryState.TicketReady:FullStackMemoryState.Ready;
    }

    static void CheckEnterResult(byte* m, JitInstruction i, ulong rsp, ulong rbp, uint flags, byte* memory, ulong guest)
    {
        ulong pushed=unchecked(rsp-(uint)(i.Width/8)), final=unchecked(pushed-i.Extra.Immediate);
        Check(((ulong*)(m+56))[0]==final && ((ulong*)(m+64))[0]==(i.Width==64?pushed:(rbp&~65535UL)|(pushed&65535)) &&
            ((Blink.Machine*)m)->flags==flags && ((ulong*)m)[0]==Ip+(uint)i.Length && m[8]==0,
            "R127 full RSP64 allocation, partial BP16 preserving upper bits, flags/IP exact");
        for(int r=0;r<16;++r)if(r is not (4 or 5))
            Check(((ulong*)(m+24+r*8))[0]==(0xfedcba9876543210UL^(uint)r),"R127 implicit RSP/RBP leave every other GPR unchanged");
        for(int b=0;b<i.Width/8;++b)
            Check(memory[(int)(pushed-guest)+b]==(byte)(rbp>>(b*8)),"R127 actual emitted oldBP store image");
    }

    static void CheckEnterAllAllocations(byte* machine)
    {
        const ulong guest=0x100000, pushed=guest+0x18000;
        byte* memory=(byte*)NativeMemory.AlignedAlloc(0x20000,4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];FullBlockInstruction[] body=new FullBlockInstruction[1];
        try
        {
            new Span<byte>(memory,0x20000).Fill(0x5a);
            foreach(int width in new[]{16,64})
            for(int alloc=0;alloc<=ushort.MaxValue;++alloc)
            {
                var i=EnterClassify(EnterEncoding(width,alloc));body[0]=new(i);
                int length=FullBlockEmitter.Emit(body,staging);
                Check(NativeCodeAllocation.TryCreate(staging.AsSpan(0,length),out var allocation,out var reason),reason);
                using var code=allocation!;
                ResetFull(machine);const ulong bp=0x8877665544332211;ulong rsp=pushed+(uint)(width/8);
                EnterSeed(machine,rsp,bp);uint flags=((Blink.Machine*)machine)->flags;
                ulong epoch=11;long issued=0,completed=0;int stop=0;var header=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var frame=FullFrame(machine,&header,&epoch,&issued,&completed,&stop,limit:1);
                frame.StackMemory=&d;frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                for(int p=0;p<32;++p)SetFullMemoryEntry(tlb,guest+(uint)(p*4096),memory+p*4096);
                Check(RunFull(code,&frame)==FullBlockExit.NeedMemory&&issued==1&&completed==0,
                    "R127 final probe requires mapping-only permission, not writable cache authority");
                EnterReady(ref d,memory,guest,epoch);FullNativeMemory.ResumeSameIssue(&frame);
                Check(RunFull(code,&frame)==FullBlockExit.Continue&&issued==1&&completed==1&&frame.Executed==1&&
                    frame.NativeMemoryComputations==1,"R127 every imm16 executes one generated ENTER");
                CheckEnterResult(machine,i,rsp,bp,flags,memory,guest);
            }
        }
        finally{NativeMemory.AlignedFree(memory);}
    }

    static void CheckEnterBoundaries(byte* machine)
    {
        byte* memory=(byte*)NativeMemory.AlignedAlloc(0x20000,4096);
        byte[] expectedMemory=new byte[0x20000];
        try
        {
            const ulong guest=0x100000;
            foreach(int width in new[]{16,64})foreach(int alloc in new[]{0,1,2,7,8,15,16,31,32,127,128,255,256,4095,4096,4097,32767,32768,65534,65535})
            foreach(int offset in new[]{0,1,4095})foreach(bool ticket in new[]{false,true})
            {
                var i=EnterClassify(EnterEncoding(width,alloc));using var code=CreateFullBlock([new(i)]);
                new Span<byte>(memory,0x20000).Fill(0x5a);ResetFull(machine);
                ulong push=guest+0x10000+(uint)offset,rsp=push+(uint)(width/8);
                // BP alias equals RSP in half the cases; source must be old BP.
                ulong bp=ticket?rsp:0x8877665544332211;
                uint flags=(uint)(offset&255)<<24|0x00280ed5u;EnterSeed(machine,rsp,bp,flags);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var frame=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);frame.StackMemory=&d;
                Check(RunFull(code,&frame)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R127 cold miss stages both requests before CPU effects");
                Check(d.Version==FullEnterLowering.MemoryTag&&d.Count==2&&d.Request0.Address==push&&
                    d.Request1.Address==push-(uint)alloc&&d.Request0.Width==(uint)width&&d.Request1.Width==(uint)width&&
                    d.Request0.Access==FullMemoryAccess.Write&&d.Request1.Access==FullMemoryAccess.Write,
                    "R127 actual push then full-width final allocation write CHECK descriptor");
                Check(FullNativeMemory.AuthenticateEnter(&frame,in i,(Blink.Machine*)machine),
                    "R127 request binds exact instruction and live machine stack");
                ++d.Request1.Address;
                Check(!FullNativeMemory.AuthenticateEnter(&frame,in i,(Blink.Machine*)machine),
                    "R127 final probe address cannot differ from authenticated allocation");
                --d.Request1.Address;
                EnterReady(ref d,memory,guest,epoch,ticket);FullNativeMemory.ResumeSameIssue(&frame);frame.Budget=0;
                Check(FloatRunProbe(code,&frame)==FullBlockExit.Continue&&issued==1&&completed==1&&frame.NativeMemoryComputations==1&&
                    d.State==FullStackMemoryState.Consumed&&d.Ticket==0&&
                    frame.MemoryFlags==(ticket?FullMemoryFlags.BarrierConsumed:FullMemoryFlags.None),"R127 same approved issue/ticket consumed once");
                CheckEnterResult(machine,i,rsp,bp,flags,memory,guest);
                expectedMemory.AsSpan().Fill(0x5a);
                for(int b=0;b<width/8;++b)expectedMemory[(int)(push-guest)+b]=(byte)(bp>>(b*8));
                Check(new ReadOnlySpan<byte>(memory,0x20000).SequenceEqual(expectedMemory),
                    "R127 final allocation probe and intervening span never write a byte");
            }
            // Selected source16 discrepancy: Put16(SP) preserves the frame upper48.
            ulong old=0x110002, architecture=old-2-65535;
            ulong selected=((old-2)&~65535UL)|unchecked((ushort)((ushort)(old-2)-65535));
            Check(architecture==0x100001&&selected==0x110001,"R127 exact 66 C8 FF FF 00 source16 witness; no false selected parity");
        }
        finally{NativeMemory.AlignedFree(memory);}
    }

    static void CheckEnterPolling(byte* machine)
    {
        var i=EnterClassify([0xc8,0,0,0]);using var code=CreateFullBlock([new(i)]);
        foreach(string boundary in new[]{"stop","attention","budget","abi","size"})
        {
            ResetFull(machine);EnterSeed(machine,0x100088,0x1234);var cpu=(Blink.Machine*)machine;
            cpu->attention=boundary=="attention";ulong epoch=11;long issued=0,completed=0;int stop=boundary=="stop"?1:0;
            var h=FullHeader(Ip,code.Entry);var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:0);
            f.StackMemory=(FullStackMemoryDescriptor*)1;f.MemoryTlb=(FullMemoryTlbEntry*)1;
            if(boundary=="abi")f.Version=0x46420005;if(boundary=="size")f.Size=384;
            var expected=boundary=="stop"?FullBlockExit.Stop:boundary=="attention"?FullBlockExit.Attention:
                boundary=="budget"?FullBlockExit.Budget:FullBlockExit.Invalidated;
            Check(RunFull(code,&f)==expected&&completed==0&&issued==(boundary=="budget"?1:0)&&
                ((ulong*)(machine+56))[0]==0x100088&&cpu->ip==Ip,"R127 poll/budget/ABI before poisoned descriptor");
        }
        byte* memory=(byte*)NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            foreach(string boundary in new[]{"stop","attention","denied","stale","cancel"})
            {
                ResetFull(machine);EnterSeed(machine,0x100088,0x1234);ulong epoch=11;long issued=0,completed=0;int stop=0;
                var h=FullHeader(Ip,code.Entry);var d=new FullStackMemoryDescriptor{Version=1};
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0,"R127 approved operation before recovery boundary");
                EnterReady(ref d,memory,0x100000,epoch);FullNativeMemory.ResumeSameIssue(&f);f.Budget=0;
                if(boundary=="stop"||boundary=="cancel")stop=1;
                if(boundary=="attention")((Blink.Machine*)machine)->attention=true;
                if(boundary=="denied") {d.Count=0;d.Request0.Host0=(byte*)1;d.Request1.Host0=(byte*)1;}
                if(boundary=="stale")d.Epoch=epoch-1;
                var exit=RunFull(code,&f);
                if(boundary=="denied"||boundary=="stale")
                    Check(exit==FullBlockExit.NeedMemory&&issued==1&&completed==0,"R127 denied/stale direct pointers never reach store");
                else if(boundary=="attention")
                    Check(issued==1&&completed==1,"R127 approved first operation precedes attention poll");
                else
                {
                    Check(exit==FullBlockExit.Stop&&issued==1&&completed==0&&f.FirstPreissued==1,"R127 stop/cancel retains same issued operation");
                    if(boundary=="stop") {stop=0;Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1,"R127 approved resume ignores later budget0 exactly once");}
                    else {d=default;Check(d.Ticket==0&&d.Request0.Host0==null&&d.Request1.Host0==null,"R127 canceled worker descriptor can be completely cleared");}
                }
            }
        }
        finally{NativeMemory.AlignedFree(memory);}
    }

    static void CheckEnterCanonical(byte* machine)
    {
        byte* page=(byte*)NativeMemory.AlignedAlloc(4096,4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(int width in new[]{16,64})foreach(string kind in new[]{"start","end","wrap","final"})
            {
                var i=EnterClassify(EnterEncoding(width,kind=="final"?1:0));using var code=CreateFullBlock([new(i)]);
                ulong push=kind=="start"?0x800000000000UL:kind=="end"?0x7fffffffffffUL:
                    kind=="wrap"?ulong.MaxValue:0xffff800000000000UL;
                ResetFull(machine);EnterSeed(machine,unchecked(push+(uint)(width/8)),0x8877665544332211);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);f.StackMemory=&d;
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
                if(kind=="final")
                {
                    new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,push,page);
                    f.MemoryTlb=tlb; // valid push mapping; final address enters canonical hole
                }
                Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==12&&f.FaultCode==128&&f.FaultAddress==0&&
                    issued==1&&completed==0&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                    "R127 emitted start/end/wrap/final SS canonical check before any guest store");
            }
        }
        finally{NativeMemory.AlignedFree(page);}
    }

    static void CheckEnterOracle()
    {
        using var f=new EnterFixture(interpreterOracle:true);const ulong guest=EnterFixture.Base;
        f.Map(guest,0x20000);f.Fill(guest,0x20000);
        foreach(int alloc in new[]{0,1,8,255,4095,4096,65535})foreach(int offset in new[]{64,4095})
        foreach(byte[] prefix in new byte[][]{[],[0x48],[0x67],[0x64,0x67]})
        {
            byte[] bytes=[..prefix,..EnterEncoding(64,alloc)];var i=EnterClassify(bytes);byte* m=(byte*)f.Machine();
            ulong push=guest+0x10000+(uint)offset;EnterSeed(m,push+8,0x8877665544332211);
            var decoded=default(Blink.XedDecodedInst);
            fixed(byte* p=bytes)Check(f.Program.DecodeInstruction(&decoded,p,(ulong)bytes.Length,Blink.XED_MODE_LONG)==0&&
                decoded.length==bytes.Length,"R127 actual authoritative XED exact ENTER length");
            nint[] pages=[f.Physical(push&~4095UL),f.Physical((push&~4095UL)+4096)];
            byte[][] beforePages=pages.Select(p=>new ReadOnlySpan<byte>((void*)p,4096).ToArray()).ToArray();
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            f.DataScope(0,()=>VectorReference(f.Program,(byte*)f.Machine(),bytes));
            byte[] expected=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            byte[][] afterPages=pages.Select(p=>new ReadOnlySpan<byte>((void*)p,4096).ToArray()).ToArray();
            before.CopyTo(new Span<byte>(m,before.Length));for(int p=0;p<2;++p)beforePages[p].CopyTo(new Span<byte>((void*)pages[p],4096));
            using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullStackMemoryDescriptor{Version=1};var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop,limit:1);frame.StackMemory=&d;
            Check(RunFull(code,&frame)==FullBlockExit.NeedMemory,"R127 selected oracle native cold mapping setup");
            StackMemoryPointers(ref d.Request0,(byte*)pages[0],(byte*)pages[1]);
            ulong final=push-(uint)alloc;StackMemoryPointers(ref d.Request1,(byte*)f.Physical(final&~4095UL),(byte*)f.Physical((final&~4095UL)+4096));
            d.Epoch=epoch;d.State=FullStackMemoryState.Ready;FullNativeMemory.ResumeSameIssue(&frame);
            Check(RunFull(code,&frame)==FullBlockExit.Continue&&issued==1&&completed==1,"R127 selected common-domain generated computation");
            VectorCheckArchitecture(m,expected,"R127 normal GetOp ENTER64 common-domain comparison in interpreterOracle DataScope");
            for(int p=0;p<2;++p)Check(new ReadOnlySpan<byte>((void*)pages[p],4096).SequenceEqual(afterPages[p]),"R127 selected common-domain push pages");
        }
        // Prefix controls are decoded by the ACTUAL selected authoritative XED,
        // but their architectural result is independent (selected66/REX gap).
        foreach(byte[] prefix in new byte[][]{[0x66],[0x66,0x48],[0x48,0x66],[0x66,0x48,0x40],[0x66,0x40,0x48]})
        {
            byte[] bytes=[..prefix,0xc8,0,0,0];Blink.XedDecodedInst d=default;
            fixed(byte* p=bytes)Check(f.Program.DecodeInstruction(&d,p,(ulong)bytes.Length,Blink.XED_MODE_LONG)==0&&d.length==bytes.Length,
                "R127 actual XED ENTER prefix length independently from selected C width");
        }
    }

    static FullStackMemoryDescriptor EnterRequest(ulong push,ulong final,int width=64) => new()
    {
        Version=FullEnterLowering.MemoryTag,Count=2,State=FullStackMemoryState.Request,
        Request0=new(){Address=push,Access=FullMemoryAccess.Write,Width=(uint)width},
        Request1=new(){Address=final,Access=FullMemoryAccess.Write,Width=(uint)width}
    };

    static void CheckEnterMmu()
    {
        foreach(int width in new[]{16,64})foreach(string kind in new[]{"ok","pushmissing","finalmissing","finalreadonly","crossfinal","sspush","ssfinal","sparse"})
        {
            using var f=new EnterFixture();const ulong guest=EnterFixture.Base;
            ulong push=guest+0x10000+4095,final=guest+4095;
            f.Map(push&~4095UL,8192);f.Fill(push&~4095UL,8192);
            if(kind!="finalmissing") {f.Map(guest,kind=="crossfinal"?4096:8192);f.Fill(guest,kind=="crossfinal"?4096:8192);}
            if(kind=="finalreadonly")Check(f.Mutate("mprotect",guest+4096)==0,"R127 final second page readonly setup");
            if(kind=="pushmissing")push+=8192;
            if(kind=="sspush")push=0x800000000000UL;
            if(kind=="ssfinal")final=0x800000000000UL;
            if(kind=="sparse") {push=guest+0x10040;final=push-65535;}
            var d=EnterRequest(push,final,width);FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            var frame=StackMemoryMmuRequest(&d,tlb);var result=FillFullMemoryMmu(f,&frame);
            bool ok=kind is "ok" or "sparse";
            Check(result==(ok?FullMemoryAssistResult.Resolved:FullMemoryAssistResult.GuestFault),"R127 ordered actual MMU ENTER mapping-only preflight");
            if(ok)Check(d.State==FullStackMemoryState.Ready&&d.Request0.Host0!=null&&d.Request1.Host0!=null,"R127 independent checked pointers and sparse allocation range");
            else if(kind is "sspush" or "ssfinal")Check(frame.FaultTrap==12&&frame.FaultCode==128&&frame.FaultAddress==0,"R127 stack canonical #SS distinct from MMU #PF");
            else Check(frame.FaultTrap==14&&frame.FaultAddress==(kind=="pushmissing"?push:kind=="crossfinal"||kind=="finalreadonly"?guest+4096:final)&&
                frame.FaultCode==(kind=="finalreadonly"?2u:1u)&&d.State!=FullStackMemoryState.Ready,"R127 exact failing push/final page and permission");
        }
        using(var f=new EnterFixture())
        {
            const ulong guest=EnterFixture.Base;f.Map(guest);f.Fill(guest);
            var file=f.Io.CreateMemoryFile("r127-enter-eof",0);Check(file.Succeeded&&f.Io.TruncateFile(file.Value,4096).Succeeded,"R127 EOF backing setup");
            f.MapShared(file.Value,guest+4096);f.Fill(guest+4096);Check(f.Io.TruncateFile(file.Value,0).Succeeded,"R127 EOF withdrawal");
            var d=EnterRequest(guest+64,guest+4096);FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            var frame=StackMemoryMmuRequest(&d,tlb);
            Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.GuestFault&&frame.FaultTrap==14&&
                unchecked((int)frame.FaultCode)==-2&&frame.FaultAddress==guest+4096,"R127 final allocation check includes EOF with no guest write");
        }
    }

    static void CheckEnterController()
    {
        foreach(int width in new[]{16,64})foreach(bool fault in new[]{false,true})
        {
            using var f=new EnterFixture();const ulong pc=EnterFixture.Base,data=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(data,8192);f.Fill(data,8192);
            byte[] bytes=EnterEncoding(width,4096);WriteControllerCode(f,pc,[0x90,..bytes,0xeb,0xfe]);
            ulong push=data+4095,final=push-4096; // final page may be deliberately absent
            if(!fault){f.Map(data-4096);f.Fill(data-4096);}
            var m=f.Machine();Write64((byte*)m,56,push+(uint)(width/8));Write64((byte*)m,64,0x8877665544332211);
            uint flags=m->flags;ulong oldRsp=((ulong*)((byte*)m+56))[0],oldBp=((ulong*)((byte*)m+64))[0];
            f.Counts(fault?100UL:2UL);
            if(fault)
            {
                var signal=f.StepSynchronousFault(pc);
                Check(signal.Signal==11&&m->trapno==14&&m->faultaddr==(long)final&&m->ip==pc+1&&m->oplen==0&&
                    ((ulong*)((byte*)m+56))[0]==oldRsp&&((ulong*)((byte*)m+64))[0]==oldBp&&m->flags==flags&&f.Issued==2&&f.Completed==1&&
                    f.Statistics.SemanticHelperInstructions==0,"R127 real controller final-allocation fault suppresses BP/RSP/store commit");
            }
            else
            {
                Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;
                Check(f.Step(pc),"R127 controller initial issue");
                for(int attempt=0;f.Completed<2&&attempt<8;++attempt)Check(f.Continue(),"R127 controller bounded same-issue continuation");
                Check(f.Completed==2&&f.Issued==2&&m->ip==pc+1+(uint)bytes.Length&&((ulong*)((byte*)m+56))[0]==final&&
                    ((ulong*)((byte*)m+64))[0]==(width==64?push:(oldBp&~65535UL)|(push&65535))&&m->flags==flags&&
                    f.Statistics.NativeMemoryInstructions==1&&f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                    "R127 real controller native ENTER16/64 success without hidden helper");
                f.NativeReturned=null;
            }
            var stats=f.Jit.Store.GetStatistics();Check(stats.ActiveNativeEpochs==0&&stats.ActiveManagedLeases==0&&f.Statistics.ActiveDataBorrows==0,
                "R127 controller releases native/store/passive capabilities");
        }
    }

    static void CheckEnterEncoding()
    {
        foreach(int width in new[]{16,64})
        {
            FullBlockInstruction[] body=[new(EnterClassify(EnterEncoding(width,65535)))];byte[] buffer=new byte[FullBlockEmitter.CodeCapacity];
            foreach(bool win in new[]{false,true})
            {
                int n=FullBlockEmitter.Emit(body,buffer,windowsAbi:win);
                Check(n>0&&buffer[6]==(win?(byte)0xca:(byte)0xfa),"R127 both ABI entry encodings");
                bool rejected=false;try{FullBlockEmitter.Emit(body,new byte[1],windowsAbi:win);}catch(ArgumentException){rejected=true;}
                Check(rejected,"R127 destination span checked");
                byte[] slab=new byte[FullBlockEmitter.ScaffoldCapacity];
                int slabLength=FullBlockEmitter.EmitScaffold(slab,out var layout,windowsAbi:win);
                var shared=layout.Resolve((nint)0x100000);
                Check(slabLength>0&&FullBlockEmitter.Emit(body,buffer,in shared,windowsAbi:win)>0,
                    "R127 both ABI shared-scaffold entry/body encodings");
            }
            for(int n=0;n<32;++n)FullBlockEmitter.Emit(body,buffer);
            long allocated=GC.GetAllocatedBytesForCurrentThread();for(int n=0;n<100;++n)FullBlockEmitter.Emit(body,buffer);
            Check(allocated==GC.GetAllocatedBytesForCurrentThread(),"R127 warm span compiler has zero per-instruction allocations");
        }
    }

    static void CheckEnterTicketFuel(byte* machine)
    {
        var i=EnterClassify([0xc8,0,0,0]);
        using var code=CreateFullBlock([new(i),FullBlockInstruction.Helper(Ip+4,1)]);
        byte* memory=(byte*)NativeMemory.AlignedAlloc(4096,4096);
        try
        {
            ResetFull(machine);EnterSeed(machine,0x100088,0x1234);ulong epoch=11;long issued=0,completed=0;int stop=0;
            var h=FullHeader(Ip,code.Entry);var d=new FullStackMemoryDescriptor{Version=1};
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
            Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R127 ticket unbounded fuel setup");
            EnterReady(ref d,memory,0x100000,epoch,true);FullNativeMemory.ResumeSameIssue(&f);
            f.NativeLimit=0;f.Executed=(ulong)uint.MaxValue+7;f.Budget=0;
            Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==(ulong)uint.MaxValue+8&&
                ((Blink.Machine*)machine)->ip==Ip+4&&f.MemoryFlags==FullMemoryFlags.BarrierConsumed,
                "R127 NativeLimit0 ticket exits after one commit before helper successor, including >uint count");
        }
        finally{NativeMemory.AlignedFree(memory);}
    }

    static void CheckEnterWatchedController()
    {
        foreach(bool actualPushWatched in new[]{false,true})foreach(int width in new[]{16,64})
        {
            using var f=new EnterFixture();const ulong pc=EnterFixture.Base,data=pc+8192;
            f.Map(pc);f.Fill(pc);f.Map(data);f.Fill(data);
            int alloc=actualPushWatched?128:8192;ulong push=actualPushWatched?pc+256:data+128,final=push-(uint)alloc;
            byte[] bytes=EnterEncoding(width,alloc);WriteControllerCode(f,pc,[0x90,..bytes,0xeb,0xfe]);
            f.DataScope(0,()=>
            {
                byte* p=f.Program.LookupAddress2(f.Machine(),(long)final,0,0);
                for(int b=0;b<width/8;++b)p[b]=0x5a;
            });
            Write64((byte*)f.Machine(),56,push+(uint)(width/8));Write64((byte*)f.Machine(),64,0x1122334455667788);
            Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;bool consumed=false;
            f.NativePrepared=cap;f.NativeReturned=p=>
            {
                consumed|=((FullBlockFrame*)p)->MemoryFlags==FullMemoryFlags.BarrierConsumed;
                f.NativePrepared=cap;
            };
            f.Counts(2);Check(f.Step(pc),"R127 watched final/push initial controller issue");
            for(int attempt=0;f.Completed<2&&attempt<8;++attempt)Check(f.Continue(),"R127 watched bounded continuation");
            Check(f.Issued==2&&f.Completed==2&&consumed==actualPushWatched&&
                f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                "R127 only actual watched push consumes exclusive ticket; mapping probe needs no writable cache authority");
            for(int b=0;b<width/8;++b)Check(((byte*)f.Physical(final&~4095UL))[(int)(final&4095)+b]==0x5a,
                "R127 watched final range has no actual data store");
            var stats=f.Jit.Store.GetStatistics();Check(stats.ActiveNativeEpochs==0&&stats.ActiveManagedLeases==0&&f.Statistics.ActiveDataBorrows==0,
                "R127 actual watched push ticket and both passive mappings released after RET");
            f.NativeReturned=null;
        }
    }

    static void CheckEnterControllerFaults()
    {
        foreach(int width in new[]{16,64})foreach(string kind in new[]{"readonlypush","readonlyfinal","sspush"})
        {
            using var f=new EnterFixture();const ulong pc=EnterFixture.Base,data=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(data-4096,12288);f.Fill(data-4096,12288);
            byte[] bytes=EnterEncoding(width,4096);WriteControllerCode(f,pc,[0x90,..bytes]);
            ulong push=kind=="sspush"?0x800000000000UL:data+4095;
            ulong final=unchecked(push-4096);
            if(kind=="readonlypush")Check(f.Mutate("mprotect",data+4096)==0,"R127 actual push second page readonly");
            if(kind=="readonlyfinal")Check(f.Mutate("mprotect",data-4096)==0,"R127 final mapping first page readonly");
            var m=f.Machine();Write64((byte*)m,56,unchecked(push+(uint)(width/8)));Write64((byte*)m,64,0x8877665544332211);
            ulong oldRsp=((ulong*)((byte*)m+56))[0],oldBp=((ulong*)((byte*)m+64))[0];uint flags=m->flags;
            byte[] before=new ReadOnlySpan<byte>((void*)f.Physical(data),4096).ToArray();
            f.Counts(100);var signal=f.StepSynchronousFault(pc);bool ss=kind.StartsWith("ss",StringComparison.Ordinal);
            Check(signal.Signal==(ss?7:11)&&m->trapno==(ss?12:14)&&m->faultaddr==(ss?0:(long)(kind=="readonlypush"?data+4096:final))&&
                m->ip==pc+1&&m->oplen==0&&((ulong*)((byte*)m+56))[0]==oldRsp&&((ulong*)((byte*)m+64))[0]==oldBp&&m->flags==flags&&
                f.Issued==2&&f.Completed==1&&f.Statistics.SemanticHelperInstructions==0&&
                new ReadOnlySpan<byte>((void*)f.Physical(data),4096).SequenceEqual(before),
                "R127 actual generated controller readonly/canonical fault precedes push bytes and BP/RSP/flags commit");
        }
    }
}
