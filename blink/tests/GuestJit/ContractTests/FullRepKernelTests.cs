using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // SOURCE-AUTHORED / UNRUN. Synthetic checked pointers test generated
    // computation only; real enrollment/alias/MMU safety belongs to controller
    // tests and parent qualification. These bounded allocations are test-owned.
    static void CheckFullRepKernels(byte* machine)
    {
        Check(sizeof(FullRepState)==256&&sizeof(FullBlockFrame)==416&&FullBlockFrame.AbiVersion==0x46420007&&
            Marshal.OffsetOf<FullBlockFrame>("Rep").ToInt32()==392&&
            Marshal.OffsetOf<FullRepState>(nameof(FullRepState.QuantumLimit)).ToInt32()==200&&
            Marshal.OffsetOf<FullRepState>(nameof(FullRepState.DecodedBytes)).ToInt32()==184,
            "R100 approved ABI6/frame400/state256/Rep392/diagnostic quantum200");
        CheckRepKernelEmission();
        CheckRepKernelNormal(machine);
        CheckRepKernelFaultFinalize(machine);
        CheckRepKernelTicketBoundary(machine);
        CheckRepKernelAuthentication(machine);
        CheckRepIssueScaffolds(machine);
        CheckRepKernelPolls(machine);
        ResetFull(machine);
    }

    static NativeCodeAllocation RepKernel(FullRepInstruction i,out FullRepKernelLayout layout)
    {
        byte[] b=new byte[FullBlockEmitter.RepKernelCapacity];
        int n=FullBlockEmitter.EmitRepKernel(i,73,91,b,out layout);
        Check(NativeCodeAllocation.TryCreate(b.AsSpan(0,n),out var code,out string reason),reason);
        return code!;
    }

    static FullRepState RepActive(FullRepInstruction i,byte* machine,ulong epoch)=>new()
    {
        Version=FullRepState.CurrentVersion,Size=256,Phase=FullRepPhase.Active,
        OwnerCookie=73,WorkerCookie=43,AttemptGeneration=1,OriginBodyGeneration=17,
        OriginPc=Ip,FallthroughPc=Ip+(uint)i.Length,Context=0x1234,KernelCookie=91,
        OriginalFlags=((Blink.Machine*)machine)->flags,LastFlags=((Blink.Machine*)machine)->flags,
        Operation=i.Operation,Width=(uint)i.Width,AddressWidth=(uint)i.AddressWidth,
        RepeatMode=i.Repeat,SourceSegment=i.SourceSegment,InstructionLength=(uint)i.Length,
        RemainingCount=((ulong*)(machine+32))[0],SourceIndex=((ulong*)(machine+72))[0],
        DestinationIndex=((ulong*)(machine+80))[0],Accumulator=((ulong*)(machine+24))[0],
        AdmissionEpoch=epoch,RequestGeneration=1
    };

    static FullBlockExit RunRep(nint entry,FullBlockFrame* frame,FullRepState* state)
    {
        frame->Rep=state;frame->OwnerHandle=43;frame->Temporary3=state->AttemptGeneration;
        frame->NextIp=state->OriginPc;state->AdmissionEpoch=frame->ObservedGlobalEpoch;
        int result=((delegate* unmanaged<FullBlockFrame*,int>)entry)(frame);
        Check(result==(int)frame->Exit&&frame->NextIp==((Blink.Machine*)frame->Machine)->ip&&state->AdmissionEpoch==0,
            "R100 kernel RET materializes IP/typed exit and ends admission permit");
        return frame->Exit;
    }

    static void CheckRepKernelEmission()
    {
        var i=new FullRepInstruction(Ip,3,FullRepOperation.Compare,64,64,-1,FullRepRepeat.WhileEqual);
        byte[] bytes=new byte[FullBlockEmitter.RepKernelCapacity],shortBytes=new byte[1];
        foreach(bool windows in new[]{false,true})
        {
            int n=FullBlockEmitter.EmitRepKernel(i,73,91,bytes,out var layout,windows);
            Check(n==layout.CodeLength&&layout.ResumeOffset==0&&layout.FaultFinalizeOffset>0&&
                bytes[6]==(windows?(byte)0xca:(byte)0xfa)&&bytes[layout.FaultFinalizeOffset]==0xf3&&
                bytes[layout.FaultFinalizeOffset+1]==0x0f,"R100 both host ABI entries and CET fault-finalize landing");
            var noScaffold=default(FullSharedScaffold);
            Check(FullBlockEmitter.EmitRepSite(i,[0xf3,0x48,0xa7],73,91,(nint)1,bytes,in noScaffold,windows)>0&&
                bytes[6]==(windows?(byte)0xca:(byte)0xfa),"R100 both host ABI site prologues before exact Issue/latch");
            bool rejected=false;
            try{FullBlockEmitter.EmitRepKernel(i,73,91,shortBytes,out _,windows);}catch(ArgumentException){rejected=true;}
            Check(rejected,"R100 short staging never publishes a partial kernel");
        }
        for(int n=0;n<8;++n)FullBlockEmitter.EmitRepKernel(i,73,91,bytes,out _);
        long before=GC.GetAllocatedBytesForCurrentThread();
        for(int n=0;n<32;++n)FullBlockEmitter.EmitRepKernel(i,73,91,bytes,out _);
        Check(GC.GetAllocatedBytesForCurrentThread()==before,"R100 warmed span kernel emission allocates zero managed bytes");
        var standalone=default(FullSharedScaffold);
        for(int n=0;n<8;++n)FullBlockEmitter.EmitRepSite(i,[0xf3,0x48,0xa7],73,91,(nint)1,bytes,in standalone);
        before=GC.GetAllocatedBytesForCurrentThread();
        for(int n=0;n<32;++n)FullBlockEmitter.EmitRepSite(i,[0xf3,0x48,0xa7],73,91,(nint)1,bytes,in standalone);
        Check(GC.GetAllocatedBytesForCurrentThread()==before,"R100 warmed span site/latch emission allocates zero managed bytes");
    }

    static void CheckRepKernelNormal(byte* m)
    {
        byte* p=(byte*)NativeMemory.Alloc(4096),q=(byte*)NativeMemory.Alloc(4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(var op in new[]{FullRepOperation.Move,FullRepOperation.Store,FullRepOperation.Load,FullRepOperation.Compare,FullRepOperation.Scan})
            foreach(int width in new[]{8,16,32,64})
            foreach(int addressWidth in new[]{32,64})
            foreach(bool df in new[]{false,true})
            foreach(int segment in new[]{-1,4,5})
            foreach(int count in new[]{0,1,3,257})
            {
                bool compares=op is FullRepOperation.Compare or FullRepOperation.Scan;
                if(segment!=-1&&op is FullRepOperation.Store or FullRepOperation.Scan)continue;
                int instructionLength=2+(width is 16 or 64?1:0)+(addressWidth==32?1:0)+(segment==-1?0:1);
                var i=new FullRepInstruction(Ip,instructionLength,op,width,addressWidth,segment,compares?FullRepRepeat.WhileEqual:FullRepRepeat.Count);
                using var kernel=RepKernel(i,out _);ResetFull(m);
                int n=width/8,offset=df?64+count*n:64;
                ulong si=count==0?0x800000000000UL:0x100000+(uint)offset;
                ulong di=count==0?ulong.MaxValue:0x702000+(uint)offset;
                if(addressWidth==32){si|=0x1234567800000000UL;di|=0x8765432100000000UL;}
                ulong cx=(ulong)count|(addressWidth==32?0x1234567800000000UL:0);
                Write64(m,72,si);Write64(m,80,di);Write64(m,32,cx);Write64(m,24,5);
                ((Blink.Machine*)m)->flags=(0xa52c0e15u&~0x10540u)|(df?1024u:0);
                uint flags=((Blink.Machine*)m)->flags;
                new Span<byte>(p,4096).Fill(0xcc);new Span<byte>(q,4096).Fill(0xcc);
                for(int k=0;k<count;++k)
                {int at=offset+(df?-k:k)*n;RepPutElement(p+at,width,5);RepPutElement(q+at,width,5);}
                ulong epoch=11;long issued=1,completed=0;int stop=0;
                var h=FullHeader(Ip,kernel.Entry);h.Live=0; // kernel is independent of retired TB authority
                var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);
                var state=RepActive(i,m,epoch);var d=new FullStackMemoryDescriptor{Version=1};frame.StackMemory=&d;
                state.SourceBase=segment==4?0x300000UL:segment==5?0x500000UL:0;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
                StringTlb(&frame,0x100000+state.SourceBase,p,FullMemoryAccess.Read);StringTlb(&frame,0x702000,q,FullMemoryAccess.Read|FullMemoryAccess.Write);
                Check(state.QuantumLimit==0&&RunRep(kernel.Entry,&frame,&state)==FullBlockExit.Continue&&
                    state.Phase==FullRepPhase.Done&&state.CompletedIterations==(ulong)count&&state.Reason==FullRepReason.Complete&&
                    state.IterationsThisCall==0&&
                    issued==1&&completed==1&&frame.Executed==1&&frame.NativeMemoryComputations==1,
                    "R100 direct generated loop completes257 iterations in ONE native call, zero quantum, no reissue");
                ulong mask=addressWidth==32?uint.MaxValue:ulong.MaxValue,delta=(ulong)(count*n);
                Check(((ulong*)(m+32))[0]==(count==0?cx:0)&&
                    ((ulong*)(m+72))[0]==(count==0||!i.HasSource?si:unchecked(si+(df?0UL-delta:delta))&mask)&&
                    ((ulong*)(m+80))[0]==(count==0||!i.HasDestination?di:unchecked(di+(df?0UL-delta:delta))&mask)&&
                    ((Blink.Machine*)m)->flags==(compares&&count!=0?(flags&~0xff0008d1u)|64u:flags),
                    "R100 native address-width/DF/count0/last flags with independent arithmetic expectations");
            }
        }
        finally{NativeMemory.Free(p);NativeMemory.Free(q);}
    }

    static void CheckRepKernelFaultFinalize(byte* m)
    {
        foreach(var repeat in new[]{FullRepRepeat.WhileEqual,FullRepRepeat.WhileNotEqual})
        {
            var i=new FullRepInstruction(Ip,2,FullRepOperation.Scan,8,64,-1,repeat);
            using var kernel=RepKernel(i,out var layout);byte* page=(byte*)NativeMemory.Alloc(4096);
            try
            {
                ResetFull(m);Write64(m,80,0x702040);Write64(m,32,3);Write64(m,24,5);
                ((Blink.Machine*)m)->flags=(0xa52c0e15u&~0x10540u)|(repeat==FullRepRepeat.WhileNotEqual?64u:0);
                uint original=((Blink.Machine*)m)->flags;new Span<byte>(page,4096).Fill(repeat==FullRepRepeat.WhileEqual?(byte)5:(byte)2);
                ulong epoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,kernel.Entry);
                var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop);var state=RepActive(i,m,epoch);state.QuantumLimit=1;
                FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;StringTlb(&frame,0x702000,page,FullMemoryAccess.Read);
                Check(RunRep(kernel.Entry,&frame,&state)==FullRepState.YieldExit&&state.Reason==FullRepReason.Quantum&&
                    state.CompletedIterations==1&&completed==0&&state.LastFlags!=original,"R100 generated partial compare changes current flags");
                // Fresh frame destroys ALL old Temporary0..3 values. Only the
                // controller's new authenticated expected-generation operand
                // and durable state are required for native fault finalization.
                frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop);state.Phase=FullRepPhase.FaultPending;
                frame.FaultTrap=14;frame.FaultCode=1;frame.FaultAddress=0x703000;
                Check(RunRep(kernel.Entry+layout.FaultFinalizeOffset,&frame,&state)==FullBlockExit.GuestFault&&
                    state.Phase==FullRepPhase.FaultFinal&&((Blink.Machine*)m)->flags==original&&
                    ((ulong*)(m+32))[0]==2&&((ulong*)(m+80))[0]==0x702041&&issued==1&&completed==0,
                    "R100 native REPE/REPNE fault-finalize restores ORIGINAL flags after frame reconstruction");
            }
            finally{NativeMemory.Free(page);}
        }
    }

    static void CheckRepKernelTicketBoundary(byte* m)
    {
        foreach(var op in new[]{FullRepOperation.Move,FullRepOperation.Compare})
        foreach(int width in new[]{8,64})
        {
            var i=new FullRepInstruction(Ip,width==64?3:2,op,width,64,-1,op==FullRepOperation.Move?FullRepRepeat.Count:FullRepRepeat.WhileEqual);
            using var kernel=RepKernel(i,out _);byte* p=(byte*)NativeMemory.Alloc(12288),q=(byte*)NativeMemory.Alloc(12288);
            try
            {
                int offset=width==64?4095:64;
                ResetFull(m);Write64(m,72,0x100000+(uint)offset);Write64(m,80,0x702000+(uint)offset);Write64(m,32,3);
                ((Blink.Machine*)m)->flags&=~0x10500u;new Span<byte>(p,12288).Fill(5);new Span<byte>(q,12288).Fill(op==FullRepOperation.Move?(byte)2:(byte)5);
                ulong epoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,kernel.Entry);
                var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop);var state=RepActive(i,m,epoch);
                var d=new FullStackMemoryDescriptor{Version=1};frame.StackMemory=&d;
                Check(RunRep(kernel.Entry,&frame,&state)==FullBlockExit.NeedMemory&&state.RequestGeneration==1,
                    "R100 cold pair stages same first iteration");
                StringSupply(&frame,p,p+8192,q,q+8192,true); // independent guest pages separated by a host4KiB gap
                Check(RunRep(kernel.Entry,&frame,&state)==FullRepState.YieldExit&&state.Reason==FullRepReason.TicketBoundary&&
                    state.CompletedIterations==1&&state.RequestGeneration==2&&((ulong*)(m+32))[0]==2&&
                    frame.MemoryFlags==FullMemoryFlags.BarrierConsumed&&completed==0&&issued==1&&
                    (width==64?q[8199]:q[65])==(op==FullRepOperation.Move?2:5),
                    "R100 RW MOVS/RR CMPS, including BOTH noncontiguous page pairs, consume ONE iteration per ticket");
            }
            finally{NativeMemory.Free(p);NativeMemory.Free(q);}
        }
    }

    static void CheckRepKernelAuthentication(byte* m)
    {
        var i=new FullRepInstruction(Ip,2,FullRepOperation.Move,8,64,-1,FullRepRepeat.Count);
        using var kernel=RepKernel(i,out _);
        foreach(string bad in new[]{"frame-version","frame-size","version","size","phase","owner","worker","attempt","body",
            "pc","fallthrough","context","kernel","operation","width","address-width","repeat","segment","length",
            "site","count","source","destination","accumulator","flags","admission","progress","request","progress-wrap","request-max"})
        {
            ResetFull(m);Write64(m,32,0); // no data operation can hide a broken entry guard
            ulong epoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,kernel.Entry);
            var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop);var state=RepActive(i,m,epoch);
            frame.Rep=&state;frame.OwnerHandle=43;frame.Temporary3=1;frame.NextIp=Ip;
            switch(bad)
            {
                case "frame-version":frame.Version=0x46420005;break;
                case "frame-size":frame.Size=399;break;
                case "version":++state.Version;break;case "size":--state.Size;break;
                case "phase":state.Phase=FullRepPhase.Done;break;case "owner":++state.OwnerCookie;break;
                case "worker":++state.WorkerCookie;break;case "attempt":++state.AttemptGeneration;break;
                case "body":++state.OriginBodyGeneration;break;case "pc":++state.OriginPc;break;
                case "fallthrough":++state.FallthroughPc;break;case "context":++state.Context;break;
                case "kernel":++state.KernelCookie;break;case "operation":state.Operation=FullRepOperation.Store;break;
                case "width":state.Width=16;break;case "address-width":state.AddressWidth=32;break;
                case "repeat":state.RepeatMode=FullRepRepeat.WhileEqual;break;case "segment":state.SourceSegment=4;break;
                case "length":state.InstructionLength=1;break;case "site":++state.OriginSite;break;
                case "count":++state.RemainingCount;break;case "source":++state.SourceIndex;break;
                case "destination":++state.DestinationIndex;break;case "accumulator":++state.Accumulator;break;
                case "flags":state.LastFlags^=64;break;case "admission":state.AdmissionEpoch=0;break;
                case "progress":++state.CompletedIterations;break;case "request":++state.RequestGeneration;break;
                case "progress-wrap":state.CompletedIterations=ulong.MaxValue;state.RequestGeneration=0;break;
                case "request-max":state.CompletedIterations=ulong.MaxValue-1;state.RequestGeneration=ulong.MaxValue;break;
            }
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            int result=((delegate* unmanaged<FullBlockFrame*,int>)kernel.Entry)(&frame);
            Check(result==(int)FullBlockExit.Invalidated&&frame.Exit==FullBlockExit.Invalidated&&
                completed==0&&issued==1&&frame.Executed==0&&new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),
                "R100 native continuation refuses forged "+bad+" before data or completion");
        }
    }

    static void CheckRepIssueScaffolds(byte* m)
    {
        Check(FullRepLowering.TryClassify(Ip,[0xf3,0xa4],out var i,out _),"R100 issued-site REP grammar");
        using var kernel=RepKernel(i,out _);
        using var common=CreateFullScaffold(out var scaffold);
        byte[] staging=new byte[FullBlockEmitter.RepKernelCapacity];
        byte* p=(byte*)NativeMemory.Alloc(4096),q=(byte*)NativeMemory.Alloc(4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(bool shared in new[]{false,true})
            foreach(bool preissued in new[]{false,true})
            {
                var selected=shared?scaffold:default;
                int length=FullBlockEmitter.EmitRepSite(i,[0xf3,0xa4],73,91,kernel.Entry,staging,in selected);
                Check(NativeCodeAllocation.TryCreate(staging.AsSpan(0,length),out var allocation,out var reason),reason);
                using var body=allocation!;
                ResetFull(m);Write64(m,72,0x100040);Write64(m,80,0x702040);Write64(m,32,3);
                new Span<byte>(p,4096).Fill(5);new Span<byte>(q,4096).Fill(2);
                ulong epoch=11;long issued=preissued?1:0,completed=0;int stop=0;var h=FullHeader(Ip,body.Entry);
                var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);
                var state=new FullRepState{Version=FullRepState.CurrentVersion,Size=256,OwnerCookie=73,WorkerCookie=43,AdmissionEpoch=epoch};
                frame.Rep=&state;frame.OwnerHandle=43;frame.FirstPreissued=preissued?1u:0;
                frame.Temporary3=0xdeadbeef; // deliberately not the new expected generation
                var d=new FullStackMemoryDescriptor{Version=1};frame.StackMemory=&d;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
                StringTlb(&frame,0x100000,p,FullMemoryAccess.Read);StringTlb(&frame,0x702000,q,FullMemoryAccess.Read|FullMemoryAccess.Write);
                Check(RunFull(body,&frame)==(preissued?FullRepState.YieldExit:FullBlockExit.Budget)&&issued==1&&
                    completed==0&&((ulong*)(m+32))[0]==3&&state.CompletedIterations==0&&
                    state.Phase==(preissued?FullRepPhase.Active:FullRepPhase.Idle)&&
                    state.AttemptGeneration==(preissued?1UL:0UL)&&q[64]==2,
                    "R100 BOTH real scaffolds consume approved issue once and hand off BEFORE data, or lock-xadd reject");
                if(preissued)
                {
                    Check(state.Reason==FullRepReason.Latched,"R100 explicit first issued handoff reason");
                    body.Dispose();h.Live=0; // original native body really gone before independent REP computation
                    frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);frame.Header=null;
                    frame.StackMemory=&d;frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
                    Check(RunRep(kernel.Entry,&frame,&state)==FullBlockExit.Continue&&issued==1&&completed==1&&
                        state.Phase==FullRepPhase.Done&&((ulong*)(m+32))[0]==0&&q[64]==5,
                        "R100 independent kernel completes SAME issue after original body disposal and fresh Header=null frame");
                }
            }
        }
        finally{NativeMemory.Free(p);NativeMemory.Free(q);}
    }

    static void CheckRepKernelPolls(byte* m)
    {
        var i=new FullRepInstruction(Ip,2,FullRepOperation.Scan,8,64,-1,FullRepRepeat.WhileEqual);
        using var kernel=RepKernel(i,out _);byte* page=(byte*)NativeMemory.Alloc(4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(string boundary in new[]{"stop","attention","runtime","code"})
            {
                ResetFull(m);Write64(m,80,0x702040);Write64(m,32,3);Write64(m,24,5);
                ((Blink.Machine*)m)->flags=0xa52c0e15u&~0x10500u;new Span<byte>(page,4096).Fill(5);
                ulong epoch=11,codeEpoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,kernel.Entry);
                var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop,codeEpoch:&codeEpoch);
                var state=RepActive(i,m,epoch);state.QuantumLimit=1;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
                StringTlb(&frame,0x702000,page,FullMemoryAccess.Read);
                Check(RunRep(kernel.Entry,&frame,&state)==FullRepState.YieldExit&&state.CompletedIterations==1,
                    "R100 native first comparison establishes real partial progress before poll challenge");
                uint last=((Blink.Machine*)m)->flags;state.QuantumLimit=0;
                if(boundary=="stop")stop=1;else if(boundary=="attention")((Blink.Machine*)m)->attention=true;
                else if(boundary=="runtime")epoch=12;else codeEpoch=12;
                Check(RunRep(kernel.Entry,&frame,&state)==FullRepState.YieldExit&&state.Reason==
                    (boundary=="stop"?FullRepReason.Stop:boundary=="attention"?FullRepReason.Attention:FullRepReason.Authority)&&
                    state.CompletedIterations==1&&state.RequestGeneration==2&&((ulong*)(m+32))[0]==2&&
                    ((ulong*)(m+80))[0]==0x702041&&((Blink.Machine*)m)->flags==last&&issued==1&&completed==0,
                    "R100 partial REP "+boundary+" poll withholds next iteration and preserves latest flags/approved issue");
            }
        }
        finally{NativeMemory.Free(page);}
    }
}
