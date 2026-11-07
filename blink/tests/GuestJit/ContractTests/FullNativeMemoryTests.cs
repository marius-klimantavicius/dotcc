using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // Authored for parent-owned serial validation. This agent does not execute
    // the emitter, native leaves, C#, guest programs, or builds.
    static void CheckFullNativeMemory(byte* machine)
    {
        Check(sizeof(FullBlockFrame) == FullBlockFrame.ByteSize && FullBlockFrame.ByteSize == 416 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.FaultTrap)).ToInt32() == 168 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.Temporary3)).ToInt32() == 208 &&
            sizeof(FullMemoryTlbEntry) == 64 && sizeof(FullNativeJumpEntry) == 64,
            "native memory v3 ABI sizes and fault/scratch offsets");
        CheckFullMemoryDecoder();
        CheckFullMemoryScalar(machine);
        CheckFullMemoryMissAndPermissions(machine);
        CheckFullMemoryStack(machine);
        CheckFullMemoryControl(machine);
        CheckFullMemoryAlu(machine);
        CheckFullMemoryFaultMetadata();
        CheckFullMemoryMmu();
        CheckFullMemoryAuthority();
        CheckFullMemoryRegistrationDrain();
        CheckFullMemoryConditional(machine);
        CheckFullMemoryStoreGeneration();
        CheckFullMemoryLargeTicket(machine);
    }
    static void CheckFullMemoryLargeTicket(byte* machine)
    {
        byte* data = (byte*)NativeMemory.AllocZeroed(8);
        try
        {
            var i = new JitInstruction(Ip, 3, JitOperation.StoreMov, 64,
                new(0, 0, IsMemory: true), new(1, 0), Address: new(3, -1, 0, 0));
            using var code = CreateFullBlock([new(i)]);
            ResetFull(machine); *(ulong*)(machine + 48) = 0x100000;
            *(ulong*)(machine + 32) = 0x123456789abcdef0;
            ulong global = 11; long issued = 0, completed = 0; int stop = 0;
            var header = FullHeader(Ip, code.Entry);
            var f = FullFrame(machine, &header, &global, &issued, &completed, &stop);
            Check(RunFull(code, &f) == FullBlockExit.NeedMemory && issued == 1, "large ticket retains approved miss");
            f.Executed = uint.MaxValue;
            FullNativeMemory.PublishBarrierTicket(&f, data, null, 8, global, 123);
            FullNativeMemory.ResumeSameIssue(&f);
            Check(RunFull(code, &f) == FullBlockExit.Continue && f.Executed == (ulong)uint.MaxValue + 1 &&
                f.NativeLimit == 0 && completed == 1 && issued == 1 && *(ulong*)data == 0x123456789abcdef0,
                "one-operation ticket exits after commit beyond uint execution count without cap overflow");
        }
        finally { NativeMemory.Free(data); }
    }

    static void SetFullMemoryEntry(FullMemoryTlbEntry* tlb, ulong guestPage, byte* host,
        FullMemoryAccess access = FullMemoryAccess.Read | FullMemoryAccess.Write,
        ulong epoch = 11, ulong context = 0x1234, uint mask = 63)
    {
        tlb[FullNativeMemory.TlbIndex(guestPage, mask)] = new() { Page = guestPage,
            Host = host, AuthorityEpoch = epoch, Context = context, Permissions = access,
            Identity = (ulong)host, ValidEnd = 4096 };
    }

    static void CheckFullMemoryDecoder()
    {
        (byte[] Bytes, JitOperation Op, int Width)[] accepted = [
            ([0x88,0x23], JitOperation.StoreMov,8), ([0x8a,0x23],JitOperation.LoadMov,8),
            ([0x66,0x89,0x03],JitOperation.StoreMov,16), ([0x48,0xc7,0x03,0xff,0xff,0xff,0xff],JitOperation.StoreMov,64),
            ([0x64,0x67,0x48,0x8b,0x44,0x8b,0xf8],JitOperation.LoadMov,64),
            ([0x48,0x0f,0xbe,0x03],JitOperation.Movsx,64), ([0x48,0x63,0x03],JitOperation.Movsx,64),
            ([0x66,0x48,0x50],JitOperation.Push,64), ([0x66,0x48,0x58],JitOperation.Pop,64),
            ([0x66,0x48,0x68,0x78,0x56,0x34,0x12],JitOperation.Push,64),
            ([0x66,0x50],JitOperation.Push,16), ([0x6a,0xff],JitOperation.Push,64),
            ([0xc2,0x10,0x00],JitOperation.ReturnAdjust,64), ([0xff,0xe0],JitOperation.IndirectJump,64),
            ([0xff,0x23],JitOperation.IndirectJump,64), ([0xff,0xd4],JitOperation.IndirectCall,64),
            ([0x48,0x83,0x03,0xff],JitOperation.Add,64), ([0x48,0x13,0x03],JitOperation.Adc,64),
            ([0x80,0x3b,0xff],JitOperation.Cmp,8), ([0x48,0xf7,0x1b],JitOperation.Neg,64),
            ([0x0f,0x94,0x03],JitOperation.SetCondition,8), ([0x48,0x0f,0x44,0x03],JitOperation.ConditionalMove,64)
        ];
        foreach (var c in accepted)
        {
            Check(FullMemoryLowering.TryClassify(Ip,c.Bytes,out var i,out _) && i.Operation == c.Op && i.Width == c.Width,
                "Full memory strict decoder accepted native shape");
            Check(FullBlockEmitter.ValidateMemory(i), "decoded memory IR passes exact native validation");
        }
        Check(FullMemoryLowering.TryClassify(Ip,[0x88,0x23],out var high,out _) && high.Source.Register == 0 && high.Source.ByteOffset == 1,
            "memory MOV preserves legacy high byte");
        Check(FullMemoryLowering.TryClassify(Ip,[0x40,0x88,0x23],out var low,out _) && low.Source.Register == 4 && low.Source.ByteOffset == 0,
            "REX selects SPL instead of AH");
        Check(FullMemoryLowering.TryClassify(Ip,[0x64,0x67,0x48,0x8b,0x44,0x8b,0xf8],out var ea,out _) &&
            ea.Address == new JitAddress(3,1,2,-8,false,32,4), "native address32 FS SIB and signed displacement");
        foreach (byte[] b in new byte[][] { [], [0x48], [0x66,0x66,0x89,0x03], [0xf0,0x48,0x01,0x03],
            [0xff,0x13], [0xff,0x33], [0x8f,0x03], [0x48,0x8b,0x03,0x90], [0x48,0x8b,0xc3] })
            Check(!FullMemoryLowering.TryClassify(Ip,b,out _,out _), "memory classifier rejects unsupported/prefix/length/multiaccess shape");
        Check(FullNativeMemory.ValidTlbMask(1) && FullNativeMemory.ValidTlbMask(255) &&
            FullNativeMemory.ValidTlbMask(4095) && !FullNativeMemory.ValidTlbMask(0) && !FullNativeMemory.ValidTlbMask(6) &&
            !FullNativeMemory.ValidTlbMask(8191), "configurable bounded power-of-two native TLB");
    }

    static void CheckFullMemoryScalar(byte* machine)
    {
        byte* data = (byte*)NativeMemory.AllocZeroed(8192);
        try
        {
            FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
            const ulong guest = 0x100000;
            foreach (int width in new[] {8,16,32,64})
            foreach (int offset in new[] {0,4088,4095})
            foreach (bool write in new[] {false,true})
            {
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                new Span<byte>(data,8192).Fill(0xa5);
                SetFullMemoryEntry(tlb,guest,data); SetFullMemoryEntry(tlb,guest+4096,data+4096);
                var i = new JitInstruction(Ip,3,write ? JitOperation.StoreMov : JitOperation.LoadMov,width,
                    write ? new(0,0,IsMemory:true) : new(0,0), write ? new(1,0) : new(0,0,IsMemory:true),
                    SourceWidth:width,Address:new(3,-1,0,0));
                using var code = CreateFullBlock([new(i)]);
                ResetFull(machine); *(ulong*)(machine+48)=guest+(uint)offset; // RBX
                *(ulong*)(machine+32)=0x8877665544332211; // RCX
                ulong global=11; long issued=0,completed=0; int stop=0;
                var header=FullHeader(Ip,code.Entry); var frame=FullFrame(machine,&header,&global,&issued,&completed,&stop,limit:1);
                frame.MemoryTlb=tlb; frame.MemoryTlbMask=63;
                uint flags=*(uint*)(machine+12);
                Check(RunFull(code,&frame)==FullBlockExit.Continue && issued==1 && completed==1 && frame.NativeMemoryComputations==1,
                    "native scalar/cross-page computation completes once");
                Check(*(uint*)(machine+12)==flags && *(ulong*)machine==Ip+3 && machine[8]==0, "MOV preserves flags and commits PC");
                if(write)
                {
                    for(int b=0;b<width/8;++b) Check(data[offset+b]==(byte)(0x8877665544332211UL>>(b*8)),"native store exact bytes");
                    Check(offset==0 || data[offset-1]==0xa5,"native store preserves preceding byte");
                    Check(data[offset+width/8]==0xa5,"native store preserves following byte");
                }
                else
                {
                    ulong mask=width==64 ? ulong.MaxValue : (1UL<<width)-1;
                    Check((*(ulong*)(machine+24)&mask)==(0xa5a5a5a5a5a5a5a5UL&mask),"native scalar load exact result");
                }
            }
            // Address-size truncates base/index before segment addition.
            var fsLoad=new JitInstruction(Ip,5,JitOperation.LoadMov,64,new(0,0),new(0,0,IsMemory:true),
                SourceWidth:64,Address:new(3,1,2,-8,false,32,4));
            using var fsCode=CreateFullBlock([new(fsLoad)]);
            ResetFull(machine); *(ulong*)(machine+48)=0x100000008; *(ulong*)(machine+32)=0x100000002;
            *(ulong*)(data+8)=0x0123456789abcdef;
            ulong ep=11; long iss=0,done=0; int st=0; var h=FullHeader(Ip,fsCode.Entry);
            var f=FullFrame(machine,&h,&ep,&iss,&done,&st,limit:1); f.MemoryTlb=tlb; f.MemoryTlbMask=63; f.FsBase=guest;
            Check(RunFull(fsCode,&f)==FullBlockExit.Continue && *(ulong*)(machine+24)==0x0123456789abcdef,
                "generated FS addition follows EA32 truncation");
        }
        finally { NativeMemory.Free(data); }
    }

    static void CheckFullMemoryMissAndPermissions(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AllocZeroed(8192);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            const ulong guest=0x200000;
            var i=new JitInstruction(Ip,3,JitOperation.StoreMov,64,new(0,0,IsMemory:true),new(0,0),SourceWidth:64,Address:new(3,-1,0,0));
            using var code=CreateFullBlock([new(i)]);
            foreach(string failure in new[]{"miss","permission","crossing","epoch","context","watch"})
            {
                new Span<FullMemoryTlbEntry>(tlb,64).Clear(); new Span<byte>(data,8192).Fill(0x5a);
                SetFullMemoryEntry(tlb,guest,data); SetFullMemoryEntry(tlb,guest+4096,data+4096);
                int offset=failure=="crossing" ? 4095 : 0;
                ref var e=ref tlb[FullNativeMemory.TlbIndex(guest,63)];
                if(failure=="miss") e=default;
                if(failure=="permission") e.Permissions=FullMemoryAccess.Read;
                if(failure=="crossing") tlb[FullNativeMemory.TlbIndex(guest+4096,63)]=default;
                if(failure=="epoch") e.AuthorityEpoch=10;
                if(failure=="context") e.Context=0;
                if(failure=="watch") e.Flags=FullMemoryFlags.WriteBarrier;
                ResetFull(machine); *(ulong*)(machine+48)=guest+(uint)offset; *(ulong*)(machine+24)=0x1122334455667788;
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                ulong global=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&global,&issued,&completed,&stop,limit:1); f.MemoryTlb=tlb; f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 && f.MemoryResume==1 && f.MemorySite==0,
                    "memory miss reserves one issue without architectural completion");
                Check(new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(before) &&
                    new ReadOnlySpan<byte>(data,8192).IndexOfAnyExcept((byte)0x5a)<0,
                    "miss/permission/cross-page/epoch/watch has no partial memory or CPU state");
                SetFullMemoryEntry(tlb,guest,data); SetFullMemoryEntry(tlb,guest+4096,data+4096);
                FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 && f.NativeMemoryComputations==1,
                    "MMU resume performs actual native store with same approved issue");
            }
        }
        finally { NativeMemory.Free(data); }
    }

    static void CheckFullMemoryStack(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(var op in new[]{JitOperation.Push,JitOperation.Pop,JitOperation.Call,JitOperation.Ret})
            {
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                var i=op==JitOperation.Call ? new JitInstruction(Ip,5,op,64,default,default,Target:Ip+16,Address:new(4,-1,0,-8)) :
                    op==JitOperation.Ret ? new JitInstruction(Ip,1,op,64,default,default,Address:new(4,-1,0,0)) :
                    new JitInstruction(Ip,1,op,64,new(4,0),new(4,0),Target:Ip+16,Address:new(4,-1,0,0));
                using var code=CreateFullBlock([new(i)]);
                ResetFull(machine); *(ulong*)(machine+56)=0x300800;
                ulong global=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&global,&issued,&completed,&stop,limit:1); f.MemoryTlb=tlb; f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory && *(ulong*)(machine+56)==0x300800 && completed==0,
                    "stack access faults before RSP/IP commit");
                *(ulong*)(data+0x800)=0x300900;
                SetFullMemoryEntry(tlb,0x300000,data); FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1,"stack same-issue native resume");
                if(op==JitOperation.Push) Check(*(ulong*)(machine+56)==0x3007f8 && *(ulong*)(data+0x7f8)==0x300800,"PUSH RSP captures original value");
                if(op==JitOperation.Pop) Check(*(ulong*)(machine+56)==0x300900,"POP RSP assigns popped value after stack increment");
                if(op==JitOperation.Call) Check(*(ulong*)machine==Ip+16 && *(ulong*)(machine+56)==0x3007f8 && *(ulong*)(data+0x7f8)==Ip+5,"native call pushes next PC before branch commit");
                if(op==JitOperation.Ret) Check(*(ulong*)machine==0x300900 && *(ulong*)(machine+56)==0x300808,"native RET cap exit retains dynamic target PC");
            }
        }
        finally { NativeMemory.Free(data); }
    }

    static void CheckFullMemoryControl(byte* machine)
    {
        var jump=new JitInstruction(Ip,2,JitOperation.IndirectJump,64,default,new(0,0));
        using var code=CreateFullBlock([new(jump)]);
        using var target=CreateFullBlock([new(new(Ip+16,1,JitOperation.Nop,64,default,default))]);
        FullNativeJumpEntry* cache=stackalloc FullNativeJumpEntry[256];
        foreach(int stale in new[]{0,1,2,3})
        {
            new Span<FullNativeJumpEntry>(cache,256).Clear();
            ResetFull(machine); *(ulong*)(machine+24)=Ip+16;
            ulong global=11,storeGeneration=123; long issued=0,completed=0; int stop=0;
            var h=FullHeader(Ip,code.Entry); var t=FullHeader(Ip+16,target.Entry,18); t.Reserved136=1;
            FullNativeMemory.PublishJump(cache,255,123,&t,in t);
            if(stale is 1 or 2) cache[FullNativeMemory.JumpIndex(Ip+16,255)].Header=(TranslationBlockHeader*)1;
            if(stale==1) storeGeneration=124;
            if(stale==2) cache[FullNativeMemory.JumpIndex(Ip+16,255)].StoreGeneration=122;
            if(stale==3) t.Reserved136=0;
            var f=FullFrame(machine,&h,&global,&issued,&completed,&stop,limit:2);
            f.JumpCache=cache; f.JumpCacheMask=255; f.StoreGenerationPointer=&storeGeneration; f.ObservedStoreGeneration=123;
            FullBlockExit exit=RunFull(code,&f);
            Check(stale==0 ? exit==FullBlockExit.Continue && completed==2 && f.TailJumps==1 && *(ulong*)machine==Ip+17 :
                exit==FullBlockExit.Continue && completed==1 && f.TailJumps==0 && *(ulong*)machine==Ip+16,
                "indirect cache authenticates generation BEFORE poisoned stale header dereference");
        }
    }

    static void CheckFullMemoryAlu(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(var op in new[]{JitOperation.Add,JitOperation.Adc,JitOperation.Sub,JitOperation.Sbb,JitOperation.And,
                JitOperation.Or,JitOperation.Xor,JitOperation.Cmp,JitOperation.Test,JitOperation.Inc,JitOperation.Dec,JitOperation.Neg,JitOperation.Not})
            foreach(int width in new[]{8,16,32,64})
            foreach(bool reverse in new[]{false,true})
            {
                if(reverse && op is JitOperation.Inc or JitOperation.Dec or JitOperation.Neg or JitOperation.Not) continue;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear(); SetFullMemoryEntry(tlb,0x400000,data);
                ulong x=0xfedcba98765432ff,y=0x1234567812345671;
                *(ulong*)data=x;
                var i=new JitInstruction(Ip,3,op,width,reverse ? new(0,0) : new(0,0,IsMemory:true),
                    reverse ? new(0,0,IsMemory:true) : new(1,0),Address:new(3,-1,0,0));
                var reference=new JitInstruction(Ip,3,op,width,new(0,0),new(1,0));
                using var code=CreateFullBlock([new(i)]); using var scalar=CreateFullBlock([new(reference)]);
                ResetFull(machine); *(ulong*)(machine+24)=reverse ? y : x; *(ulong*)(machine+32)=reverse ? x : y;
                *(uint*)(machine+12)|=1;
                ulong global=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,scalar.Entry);
                var f=FullFrame(machine,&h,&global,&issued,&completed,&stop,limit:1);
                RunFull(scalar,&f); ulong expected=*(ulong*)(machine+24); uint flags=*(uint*)(machine+12);
                ResetFull(machine); *(ulong*)(machine+24)=y; *(ulong*)(machine+32)=y; *(ulong*)(machine+48)=0x400000;
                *(uint*)(machine+12)|=1; issued=completed=0; h=FullHeader(Ip,code.Entry);
                f=FullFrame(machine,&h,&global,&issued,&completed,&stop,limit:1); f.MemoryTlb=tlb; f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.Continue && *(uint*)(machine+12)==flags,"native memory ALU baseline flags match scalar contract");
                ulong mask=width==64 ? ulong.MaxValue : (1UL<<width)-1;
                if(reverse) Check(*(ulong*)(machine+24)==expected,"memory-source ALU writes exact register width");
                else Check((*(ulong*)data&mask)==(expected&mask),"memory-destination ALU exact native result");
            }
        }
        finally { NativeMemory.Free(data); }
    }

    static void CheckFullMemoryFaultMetadata()
    {
        FullBlockFrame frame=default;
        foreach(uint code in new[]{1u,2u,unchecked((uint)-2)})
        {
            Check(FullNativeMemory.Fault(&frame,0xfeed000,code)==FullMemoryAssistResult.GuestFault &&
                frame.FaultTrap==14 && frame.FaultCode==code && frame.FaultAddress==0xfeed000,
                "page MAPERR/ACCERR/EOF metadata retains Blink trap14 and Linux segvcode sentinel");
        }
        Check(unchecked((int)frame.FaultCode)==-2 && frame.FaultTrap!=7,
            "EOF requests BUS_ADRERR through HaltMachine14, never processor trap7 or BUS_OBJERR3");
    }

    static FullMemoryAssistResult FillFullMemoryMmu(Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture fixture, FullBlockFrame* frame)
    {
        fixture.Jit.BeginRead(fixture.Program,false);
        try
        {
            fixture.Program.ResetTlb(fixture.Machine());
            lock(fixture.Jit.Authority.Gate)
            {
                for(int attempt=0;attempt<4;++attempt)
                {
                    // Model the controller's fresh authenticated frame on each
                    // mapping retry; a runtime admission pulse cannot restamp
                    // already emitted/native execution by itself.
                    frame->ObservedGlobalEpoch=fixture.Jit.Authority.Epoch;
                    var result=FullNativeMemory.TryFill(fixture.Program,fixture.Machine(),frame,fixture.Jit.Authority);
                    if(result!=FullMemoryAssistResult.Retry) return result;
                }
                return FullMemoryAssistResult.Retry;
            }
        }
        finally { fixture.Jit.EndRead(fixture.Program); }
    }

    static void CheckFullMemoryMmu()
    {
        using var fixture=new Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture();
        ulong guest=Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture.Base;
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        FullBlockFrame frame=new(){MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=guest,MemoryResume=1,
            MemoryAccess=FullMemoryAccess.Read,MemoryWidth=64};
        fixture.Map(guest);
        Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.DemandPage,"MMU reserved page requests runtime materialization only");
        fixture.Fill(guest);
        Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.Resolved && frame.MemoryAssists==2,
            "MMU materialized page publishes native mapping without scalar semantics");
        frame.MemoryAddress=guest+4095;
        Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.GuestFault && frame.FaultTrap==14 && frame.FaultCode==1 && frame.FaultAddress==guest+4096,
            "MMU cross-page MAPERR identifies second failing byte page");
        frame.MemoryAddress=guest; frame.MemoryAccess=FullMemoryAccess.Write;
        Check(fixture.Mutate("mprotect",guest)==0,"MMU permission test mprotect succeeds");
        Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.GuestFault && frame.FaultTrap==14 && frame.FaultCode==2,
            "MMU actual page permission fault reports ACCERR");
        Check(fixture.Mutate("MAP_FIXED",guest)==0,"MMU replacement mapping succeeds"); fixture.Fill(guest);
        Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.Resolved,"MMU remap fills with fresh authority epoch");
        var file=fixture.Io.CreateMemoryFile("native-eof-contract",0); Check(file.Succeeded,"MMU EOF file opens");
        Check(fixture.Io.TruncateFile(file.Value,4096).Succeeded,"MMU EOF initial extent");
        ulong shared=guest+0x10000; fixture.MapShared(file.Value,shared); fixture.Fill(shared);
        Check(fixture.Io.TruncateFile(file.Value,0).Succeeded,"MMU EOF backing becomes unavailable");
        frame.MemoryAddress=shared; frame.MemoryAccess=FullMemoryAccess.Read;
        Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.GuestFault && frame.FaultTrap==14 &&
            unchecked((int)frame.FaultCode)==-2 && frame.FaultAddress==shared,
            "actual EOF MMU assist requests HaltMachine14 with BUS_ADRERR sentinel");
    }

    static void CheckFullMemoryAuthority()
    {
        var io=new Managed.Emulation.Host.InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
        using var authority=new FullCodeAuthority(io,4096*16,1,1<<20);
        byte* data=(byte*)NativeMemory.AlignedAlloc(4096,4096);
        try
        {
            lock(authority.Gate)
            {
                Check(authority.TryGetNativeDataPermission((nint)data,true,out ulong oldEpoch),"first native data write becomes registered non-code");
                Check(authority.PrepareCodeSnapshot((nint)data) && authority.Epoch!=oldEpoch,"first code registration changes write-TLB authority before snapshot");
                Check(!authority.TryGetNativeDataPermission((nint)data,true,out _) && authority.TryGetNativeDataPermission((nint)data,false,out _),
                    "physical code aliases require write barrier but allow checked native reads");
                authority.BeginMappingMutation();
                Check(!authority.TryEnterNative(authority.Epoch),"mapping writer closes native data/code admission");
                authority.EndMappingMutation();
            }
        }
        finally { NativeMemory.AlignedFree(data); io.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
    }
    static void CheckFullMemoryRegistrationDrain()
    {
        var io=new Managed.Emulation.Host.InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
        using var authority=new FullCodeAuthority(io,4096*16,1,1<<20);
        nint physical=(nint)NativeMemory.AlignedAlloc(4096,4096);
        bool enrolled=false;
        Thread? writer=null;
        Exception? writerError=null;
        int finished=0;
        try
        {
            ulong oldEpoch;
            lock(authority.Gate) Check(authority.TryGetNativeDataPermission(physical,true,out oldEpoch),"registration drain begins with cacheable data");
            Check(enrolled=authority.TryEnterNative(oldEpoch),"registration drain enrolls old native data epoch");
            writer=new Thread(()=>
            {
                try { lock(authority.Gate) Check(authority.PrepareCodeSnapshot(physical),"new code registration completes after drain"); }
                catch(Exception error) { writerError=error; }
                finally { Volatile.Write(ref finished,1); }
            });
            writer.Start();
            Check(SpinWait.SpinUntil(()=>authority.Epoch!=oldEpoch,TimeSpan.FromSeconds(10)),"code registration advances epoch before waiting for native data writer");
            Check(Volatile.Read(ref finished)==0 && !authority.TryEnterNative(authority.Epoch),
                "first code snapshot closes new native admission while old data frame remains enrolled");
            authority.ExitNative(); enrolled=false;
            Check(writer.Join(TimeSpan.FromSeconds(10)) && writerError==null,"native writer drain finishes within bounded contract");
            lock(authority.Gate)
                Check(!authority.TryGetNativeDataPermission(physical,true,out _) && authority.TryGetNativeDataPermission(physical,false,out _),
                    "physical code watch invalidates previously cacheable alias writes");
        }
        finally
        {
            if(enrolled) authority.ExitNative();
            if(writer!=null && writer.IsAlive && !writer.Join(TimeSpan.FromSeconds(10)))
                throw new InvalidOperationException("Code registration contract writer did not drain.");
            NativeMemory.AlignedFree((void*)physical);
            io.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    static void CheckFullMemoryConditional(byte* machine)
    {
        var i=new JitInstruction(Ip,4,JitOperation.ConditionalMove,32,new(0,0),new(0,0,IsMemory:true),
            Condition:4,Address:new(3,-1,0,0)); // CMOVZ
        using var code=CreateFullBlock([new(i)]);
        foreach(bool take in new[]{false,true})
        {
            ResetFull(machine); *(ulong*)(machine+24)=0xfeedface12345678; *(ulong*)(machine+48)=0x700000;
            uint flags=*(uint*)(machine+12);
            *(uint*)(machine+12)=take ? flags|64u : flags&~64u;
            ulong global=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&global,&issued,&completed,&stop,limit:1);
            FullBlockExit exit=RunFull(code,&f); // deliberately absent TLB/mapping
            Check(take ? exit==FullBlockExit.NeedMemory && completed==0 && *(ulong*)(machine+24)==0xfeedface12345678 :
                exit==FullBlockExit.Continue && completed==1 && *(ulong*)(machine+24)==0x12345678 && f.MemoryResume==0 && f.NativeMemoryComputations==0,
                "selected CMOV memory reads only on true; false32 zero-extends without MMU or fault");
        }
    }

    static void CheckFullMemoryStoreGeneration()
    {
        var store=new RetainedTranslationStore(RetainedOptions() with {PayloadBytes=0});
        try
        {
            ulong* stable=store.GenerationPointer;
            ulong generation=store.Generation;
            Check(generation==1 && *stable==generation && store.GetStatistics().FixedChargedBytes>=RetainedTranslationStore.GenerationWordBytes,
                "stable generation word starts nonzero and belongs to fixed charge");
            var token=RetainedPublish(store,0x900000);
            Check(store.TryAcquireToken(token,out var lease),"generation lifetime managed lease");
            Check(!store.TryFlush(out _) && store.Generation==generation && store.GenerationPointer==stable,
                "managed lease prevents generation bump/reset and keeps stable word readable");
            lease.Dispose();
            Check(store.Generation>generation && *stable==store.Generation && store.GenerationPointer==stable,
                "pending whole flush changes word without replacing its pointer");
            generation=store.Generation;
            Check(store.TryBeginNativeEpoch(out var epoch),"generation lifetime native epoch");
            Check(!store.TryFlush(out _) && *stable==generation,"native epoch prevents store reset and stable-word reclamation");
            bool disposeRejected=false;
            try { store.Dispose(); } catch(InvalidOperationException) {disposeRejected=true;}
            Check(disposeRejected && *stable==generation,"store disposal cannot free generation word under native epoch");
            epoch.Dispose();
            Check(*stable>generation && store.GenerationPointer==stable,"native epoch release services pending generation change");
            Check(store.TryReserve(new(0xa00000,7),1,0,1,out var draft,out _),"generation draft lifetime");
            generation=store.Generation;
            Check(!store.TryFlush(out _) && *stable==generation,"draft pins generation until publication/abort");
            store.Abort(ref draft);
            Check(*stable>generation,"last draft abort permits monotonic generation change");
            // Code-heavy -> payload-heavy phase forces trimming old empty code
            // and data reservations. Stable word is separately owned throughout.
            for(int n=0;n<32;++n) RetainedPublish(store,0xb00000UL+(uint)n*16,61440,0);
            Check(store.TryFlush(out _),"generation trim starts with empty exclusive arenas");
            generation=store.Generation;
            var large=RetainedPublish(store,0xc00000,0,3<<20);
            Check(store.Generation>generation && store.GenerationPointer==stable && *stable==store.Generation,
                "arena trim increments stable word BEFORE old header/data mappings are unmapped");
            Check(store.TryAcquireToken(large,out var held),"trimmed store retains new payload body"); held.Dispose();
        }
        finally {store.Dispose();}
        bool inaccessible=false;
        try {_=store.Generation;} catch(ObjectDisposedException) {inaccessible=true;}
        Check(inaccessible && store.GetStatistics().ChargedBytes==0,"generation API closes after final owner disposal");
    }

}
