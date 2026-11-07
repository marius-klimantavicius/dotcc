using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using ScalarAtomicFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // One entry, called by the existing R156 family. SOURCE_AUTHORED_UNRUN.
    // Synthetic leaves qualify arithmetic, not real enrollment or exclusion.
    static void CheckFullScalarAtomicR183(byte* machine)
    {
        ScalarAtomicTable(machine);
        ScalarAtomicInterference(machine);
        ScalarAtomicControllerAdmission();
        ScalarAtomicReaderClosure();
        ScalarAtomicActualBackingRead();
        ScalarAtomicMixedTicket();
        CheckR186LiveFastVsTicket();
        ScalarAtomicMapperDenials();
        ScalarAtomicAdmissionForgery();
        ScalarAtomicSnapshotInputs();
        ScalarAtomicRevocation();
        ScalarAtomicFileDenials();
        // R156 in the SAME entry continues to run: full controller family table,
        // real fast/fast races (offset64), ticket/ticket (65/4095), aliases,
        // EOF/permissions/canonical/SS, cancellation, pairs and c129 Retry2/3.
    }

    static IEnumerable<byte[]> ScalarAtomicRows() => AtomicEncodings().Concat(
        AtomicFamilyRows().Select(r=>r.Bytes)).Concat(new byte[][] {
        [0x64,0x67,0xf0,0x48,0x01,0x44,0x8b,0xf8],
        [0x65,0xf0,0x48,0x83,0x03,0xff], [0x66,0xf0,0x66,0x83,0x03,0xff],
        [0xf3,0xf0,0x48,0x01,0x0b], [0xf2,0xf0,0x48,0x01,0x0b],
        [0xf0,0x4d,0x0f,0xc1,0x13], [0xf0,0x48,0x0f,0xb1,0x03] });

    static ulong ScalarAtomicReadReg(byte* machine, JitOperand operand, int width) =>
        (((ulong*)(machine+24))[operand.Register] >> (operand.ByteOffset*8)) & IntegerMask(width);

    static void ScalarAtomicTable(byte* machine)
    {
        byte* page=(byte*)NativeMemory.AlignedAlloc(4096,4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(byte[] bytes in ScalarAtomicRows())
            {
                var i=AtomicClassify(bytes);if(!FullBlockEmitter.ScalarAtomicFast(i))continue;
                foreach(uint flags in new uint[]{0,1,0xa52c0e15u & ~0x50100u,0xff0008d5u})
                foreach(bool equal in new[]{false,true})
                {
                    AtomicSeed(machine);var m=(Blink.Machine*)machine;m->flags=flags;
                    Write64(machine,48,0x200040);Write64(machine,112,0x200040);Write64(machine,32,2);
                    m->fs.@base=0x100000000UL;m->gs.@base=0x200000000UL;
                    ulong ea=FullNativeMemory.AtomicEffectiveAddress(in i,m);
                    // Retain actual prefix/address grammar, align through base.
                    if(i.Address.Base>=0)((ulong*)(machine+24))[i.Address.Base]+=(0UL-ea)&(uint)(i.Width/8-1);
                    ea=FullNativeMemory.AtomicEffectiveAddress(in i,m);
                    Check((ea&(uint)(i.Width/8-1))==0&&(ea&4095)<=4096UL-(uint)i.Width/8,"R183 table complete naturally aligned operand");
                    ulong x=(equal?0x7fffffffffffffffUL:0x8000000000000000UL)&IntegerMask(i.Width);
                    byte* operand=page+(int)(ea&4095);new Span<byte>(page,4096).Fill(0xa5);
                    AtomicWrite(operand,null,i.Width/8,0,i.Width,x);
                    if(FullBlockEmitter.AtomicOperation(i)==FullAtomicOperation.CompareExchange)
                        IntegerRegisterWrite(machine,new(0,0),i.Width,x^(equal?0UL:1UL));
                    ulong y=i.Source.IsImmediate?i.Source.Immediate:
                        ScalarAtomicReadReg(machine,i.Source,i.Width);
                    ulong ax=ScalarAtomicReadReg(machine,new(0,0),i.Width);
                    var model=AtomicFamilyModel(FullBlockEmitter.AtomicOperation(i),i.Width,x,y,ax,flags);
                    byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                    new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                    using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;
                    var h=FullHeader(Ip,code.Entry);var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
                    f.MemoryTlb=tlb;f.MemoryTlbMask=63;f.FsBase=m->fs.@base;f.GsBase=m->gs.@base;
                    SetFullMemoryEntry(tlb,ea&~4095UL,page);
                    Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1&&
                        f.NativeMemoryComputations==1&&f.MemoryFlags==FullMemoryFlags.None&&f.MemoryTicket==0&&f.MemoryResume==0,
                        "R183 synthetic RW leaf commits exactly once without ticket exit");
                    Check(AtomicRead(operand,null,i.Width/8,0,i.Width)==(ulong)model.Value&&m->flags==model.Flags,
                        $"R183 actual successful old drives full flags/value {Convert.ToHexString(bytes)}/{flags:x}/{equal}");
                    var op=FullBlockEmitter.AtomicOperation(i);
                    byte[] expected=(byte[])before.Clone();
                    fixed(byte* e=expected)
                    {
                        if(op is FullAtomicOperation.Exchange or FullAtomicOperation.ExchangeAdd)
                            IntegerRegisterWrite(e,i.Source,i.Width,x);
                        if(op==FullAtomicOperation.CompareExchange&&ax!=x)IntegerRegisterWrite(e,new(0,0),i.Width,x);
                    }
                    Check(new ReadOnlySpan<byte>(machine+24,128).SequenceEqual(expected.AsSpan(24,128)),
                        "R183 aliases/AH/SPL/high/width32 register writes match selected rules");
                }
            }
        }
        finally{NativeMemory.AlignedFree(page);}
    }

    static void ScalarAtomicInterference(byte* machine)
    {
        byte* page=(byte*)NativeMemory.AlignedAlloc(4096,4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(int width in new[]{8,16,32,64})foreach(int mode in new[]{1,2,3})
            {
                // Test-only emission makes the first CAS fail by changing ONLY
                // the operand; modes2/3 also change stop/epoch before that failure.
                FullBlockEmitter.ScalarAtomicProbeMode=mode;
                var i=AtomicClassify(AtomicEncoding(width,[width==8?(byte)0x10:(byte)0x11,0x0b])); // ADC
                using var code=CreateFullBlock([new(i)]);FullBlockEmitter.ScalarAtomicProbeMode=0;
                AtomicSeed(machine);Write64(machine,48,0x200040);Write64(machine,32,5);
                ulong old=40,newOld=(~old)&IntegerMask(width);AtomicWrite(page+64,null,width/8,0,width,old);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,0x200000,page);
                ulong epoch=11,codeEpoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,codeEpoch:&codeEpoch);f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                var exit=RunFull(code,&f);
                if(mode!=1)
                {
                    Check(exit==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.Temporary2==1&&f.MemoryResume==1&&
                        new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before)&&
                        AtomicRead(page+64,null,width/8,0,width)==newOld,
                        "R183 failed CAS stop/epoch restages same issue with all guest inputs immutable");
                    if(mode==2)continue; // parent cancellation must not execute another attempt
                    f.ObservedGlobalEpoch=epoch;SetFullMemoryEntry(tlb,0x200000,page,epoch:epoch);
                    FullNativeMemory.ResumeSameIssue(&f);exit=RunFull(code,&f);
                }
                var expected=AtomicFamilyModel(FullAtomicOperation.Adc,width,newOld,5,0,BitConverter.ToUInt32(before,12));
                Check(exit==FullBlockExit.Continue&&issued==1&&completed==1&&f.Temporary2==2&&
                    AtomicRead(page+64,null,width/8,0,width)==(ulong)expected.Value&&((Blink.Machine*)machine)->flags==expected.Flags,
                    "R183 CAS interference recomputes actual-old ADC flags, retries one issue and commits once");
            }
        }
        finally{FullBlockEmitter.ScalarAtomicProbeMode=0;NativeMemory.AlignedFree(page);}
    }

    static void ScalarAtomicControllerAdmission()
    {
        foreach(int width in new[]{8,16,32,64})
        {
            using var f=new ScalarAtomicFixture();using var deadline=f.AtomicDeadline();
            ulong pc=ScalarAtomicFixture.Base,data=pc+8192;AtomicFamilySetup(f,pc,data);
            WriteControllerCode(f,pc,AtomicEncoding(width,[width==8?(byte)0x00:(byte)0x01,0x0b]));
            int fast=0;long epochsAfterFirst=0;long assistsAfterFirst=0;
            for(int n=0;n<2;++n)
            {
                AtomicFamilySeed(f,data+64);
                f.NativeReturned=p=>{
                    var frame=(FullBlockFrame*)p;if(frame->NativeMemoryComputations==0)return;
                    Check(frame->MemoryFlags==FullMemoryFlags.None&&frame->MemoryTicket==0,"R183 actual controller fast admission");
                    lock(f.Jit.Authority.Gate)
                        Check(f.Jit.Authority.HasScalarAtomicWriteAuthority(f.Physical(data),frame->ObservedGlobalEpoch),
                            "R183 actual native enrollment and physical write authority remain live through RET");
                    ++fast;
                };
                Check(f.Step(pc,signalActor:true),"R183 actual scalar issue");
                if(n==0){epochsAfterFirst=f.Jit.Authority.EpochAdvances;assistsAfterFirst=f.Statistics.NativeMemoryAssists;}
                else Check(f.Jit.Authority.EpochAdvances==epochsAfterFirst&&f.Statistics.NativeMemoryAssists==assistsAfterFirst,
                    "R183 warm scalar reuses RW authority without atomic ticket/epoch/fill churn");
            }
            Check(fast==2&&f.Issued==2&&f.Completed==2,"R183 both native scalar entries complete");AtomicFamilyAssertClean(f);
        }
    }

    static void ScalarAtomicReaderClosure()
    {
        foreach(string kind in new[]{"raw","copy","managed-atomic","backing"})
        {
            using var f=new ScalarAtomicFixture();using var deadline=f.AtomicDeadline();
            ulong pc=ScalarAtomicFixture.Base,data=pc+8192;AtomicFamilySetup(f,pc,data);
            WriteControllerCode(f,pc,[0xf0,0x48,0x0f,0xc1,0x0b]);AtomicFamilySeed(f,data+64);
            *(ulong*)((byte*)f.Physical(data)+64)=0;
            using var ready=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
            using var reader=new AtomicFamilyThread(f,1,()=>{
                if(kind=="backing")
                {
                    var observer=(IHostMemoryFileRead)f.Owner;
                    Check(observer.TryBeginMemoryFileRead(),"R183 real backing reader admission");
                    try {ready.Set();AtomicFamilyWait(release,"R183 backing reader held");}
                    finally{observer.EndMemoryFileRead();}
                }
                else if(kind=="managed-atomic")
                {
                    f.DataScope(1,()=>{
                        ready.Set();AtomicFamilyWait(release,"R183 managed atomic envelope held");
                        // A normal C LockBus-family body, not a native writer fallback.
                        byte[] bytes=[0xf0,0x48,0x83,0x03,1];var m=f.Machine(1);Write64((byte*)m,48,data+64);
                        Blink.XedDecodedInst d=default;fixed(byte* b=bytes)
                            Check(f.Program.DecodeInstruction(&d,b,(ulong)bytes.Length,Blink.XED_MODE_LONG)==0,"R183 managed atomic decode");
                        var saved=m->xedd;m->xedd=&d;
                        try {
                            f.Program.SetHostGuestCurrentMachine(null);
                            delegate*<Blink,Blink.Machine*,ulong,long,ulong,void> op;
                            try{op=f.Program.GetOp((long)(d.op.rde>>40&2047));}
                            finally{f.Program.SetHostGuestCurrentMachine(m);}
                            op(f.Program,m,d.op.rde,d.op.disp,d.op.uimm0);
                        }finally{m->xedd=saved;}
                        if(m->stashaddr!=0)f.Program.CommitStash(m);
                    });
                }
                else AtomicFamilyNormalRead(f,1,data+64,kind,()=>{ready.Set();AtomicFamilyWait(release,"R183 actual guest reader held");});
            });
            AtomicFamilyWait(ready,"R183 foreign reader published");
            lock(f.Jit.Authority.Gate)
                Check(!f.Jit.Authority.TryEnterNative(f.Jit.Authority.Epoch),"R183 no native entry while semantic/backing reader active");
            bool attempted=false;
            f.NativePrepared=_=>{
                Check(f.Completed==0&&*(ulong*)((byte*)f.Physical(data)+64)==0,
                    "R183 concurrent controller reaches prepare without touching reader's bytes");
                attempted=true;release.Set();
            };
            try { Check(f.Step(pc,signalActor:true)&&f.Completed==1&&attempted,
                "R183 actual controller/read envelope overlap closes admission then progresses"); }
            finally {release.Set();}
            reader.Join();
            Check(*(ulong*)((byte*)f.Physical(data)+64)==(kind=="managed-atomic"?6UL:5UL),"R183 managed atomic/native RMW no lost update");
            AtomicFamilyAssertClean(f);
        }
    }

    static void ScalarAtomicActualBackingRead()
    {
        using var f=new ScalarAtomicFixture();using var deadline=f.AtomicDeadline();
        ulong pc=ScalarAtomicFixture.Base,data=pc+8192;AtomicFamilySetup(f,pc,data);
        WriteControllerCode(f,pc,[0xf0,0x48,0x01,0x0b]);AtomicFamilySeed(f,data+64);
        int fd=f.Io.CreateMemoryFile("r183-real-backing-read",0).Value;
        byte[] expected=new byte[4096];Array.Fill(expected,(byte)0x5a);
        Check(f.Io.WriteAt(fd,expected,0).Value==4096,"R183 actual backing source initialized");
        byte[] result=new byte[4096];Exception? error=null;
        using var start=new ManualResetEventSlim();using var published=new ManualResetEventSlim();using var done=new ManualResetEventSlim();
        var reader=new Thread(()=>{
            try {AtomicFamilyWait(start,"R183 actual backing read starts during native RET scope");
                Check(f.Io.ReadAt(fd,result,0).Value==4096,"R183 actual backing copy succeeds");}
            catch(Exception e){error=e;}
            finally{done.Set();}
        }){IsBackground=true};
        f.Jit.Authority.TestHostAdmission=atPublication=>{
            if(atPublication&&Environment.CurrentManagedThreadId==reader.ManagedThreadId)published.Set();
        };
        bool observed=false;
        f.NativeReturned=p=>{
            var frame=(FullBlockFrame*)p;if(frame->NativeMemoryComputations==0)return;
            Check(frame->MemoryFlags==FullMemoryFlags.None,"R183 actual backing race uses scalar fast leaf");
            start.Set();AtomicFamilyWait(published,"R183 actual ReadAt publishes its production backing-reader count");
            Check(!done.IsSet&&f.Jit.Authority.HostShortReaders>0,
                "R183 actual copy cannot complete before live native enrollment releases through RET");
            observed=true;
        };
        reader.Start();
        try
        {
            Check(f.Step(pc,signalActor:true)&&observed&&f.Completed==1,"R183 fast scalar returns through counted backing race");
            Check(reader.Join(TimeSpan.FromSeconds(5))&&error==null&&result.AsSpan().SequenceEqual(expected),
                "R183 real backing read progresses after native release without torn publication");
            AtomicFamilyAssertClean(f);
        }
        finally
        {
            start.Set();reader.Join(TimeSpan.FromSeconds(5));f.NativeReturned=null;f.Jit.Authority.TestHostAdmission=null;
            Check(f.Io.Close(fd).Succeeded,"R183 actual backing reader descriptor cleanup");
        }
    }

    static void ScalarAtomicMixedTicket()
    {
        // Exact same bytes overlap. One aligned8 fast RMW versus an unaligned8
        // ticket RMW at +1; bit8 represents disjoint increments without carry.
        using var f=new ScalarAtomicFixture();using var deadline=f.AtomicDeadline();
        ulong pc=ScalarAtomicFixture.Base,data=pc+8192;AtomicFamilySetup(f,pc,data);
        WriteControllerCode(f,pc,[0xf0,0x48,0x01,0x0b]);new Span<byte>((byte*)f.Physical(data)+64,16).Clear();
        using var ready=new ManualResetEventSlim();using var start=new ManualResetEventSlim();int tickets=0,fast=0;
        f.Jit.Authority.TestReservationPublished=_=>Interlocked.Increment(ref tickets);
        using var other=new AtomicFamilyThread(f,1,()=>{
            ready.Set();AtomicFamilyWait(start,"R183 mixed fast/ticket start");
            for(int k=0;k<32;++k){AtomicFamilySeed(f,data+65,1);Write64((byte*)f.Machine(1),32,1);Check(f.Step(pc,1,true),"R183 real slow unaligned worker");}
        });
        AtomicFamilyWait(ready,"R183 mixed worker ready");start.Set();
        try {
            for(int k=0;k<32;++k){
                AtomicFamilySeed(f,data+64);Write64((byte*)f.Machine(),32,256);
                f.NativeReturned=p=>{var frame=(FullBlockFrame*)p;if(frame->NativeMemoryComputations!=0&&frame->MemoryFlags==FullMemoryFlags.None)++fast;};
                Check(f.Step(pc,signalActor:true),"R183 real aligned worker");
            }
            other.Join();
            Check(*(ulong*)((byte*)f.Physical(data)+64)==64UL*256&&f.Issued==64&&f.Completed==64&&tickets>=32,
                "R183 overlapping fast/ticket RMWs serialize through same native drain");
            AtomicFamilySeed(f,data+64);Write64((byte*)f.Machine(),32,0);
            f.NativeReturned=p=>{if(((FullBlockFrame*)p)->MemoryFlags==FullMemoryFlags.None)++fast;};
            Check(f.Step(pc,signalActor:true)&&fast>0&&f.Completed==65&&f.Issued==65&&
                *(ulong*)((byte*)f.Physical(data)+64)==64UL*256,"R183 fast admission remains available after ticket drain");
            AtomicFamilyAssertClean(f);
        }finally{f.Jit.Authority.TestReservationPublished=null;}
    }

    static void ScalarAtomicMapperDenials()
    {
        foreach(string kind in new[]{"unaligned","cross-page","pair8","pair16","code","readonly","unmapped","epoch"})
        {
            using var f=new ScalarAtomicFixture();ulong pc=ScalarAtomicFixture.Base,data=pc+8192;
            AtomicFamilySetup(f,pc,data);byte[] bytes=kind=="pair8"?[0x0f,0xc7,0x0b]:kind=="pair16"?[0x48,0x0f,0xc7,0x0b]:[0xf0,0x48,0x01,0x0b];
            WriteControllerCode(f,pc,bytes);AtomicFamilySeed(f,data+(kind=="unaligned"?65UL:kind=="cross-page"?4095UL:64UL));
            var i=AtomicClassify(bytes) with {Ip=pc};f.Machine()->ip=pc;
            if(kind=="code")lock(f.Jit.Authority.Gate)Check(f.Jit.Authority.PrepareCodeSnapshot(f.Physical(data)),"R183 code write denied");
            if(kind=="readonly")Check(f.Mutate("mprotect",data)==0,"R183 readonly mapping");
            if(kind=="unmapped")Check(f.Mutate("munmap",data)==0,"R183 absent mapping");
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            var frame=new FullBlockFrame {Machine=f.Machine(),MemoryResume=1,MemoryWidth=(uint)i.Width,MemoryAccess=FullNativeMemory.AtomicReadWrite,
                MemoryAddress=FullNativeMemory.AtomicEffectiveAddress(in i,f.Machine()),MemoryTlb=tlb,MemoryTlbMask=63,
                ObservedGlobalEpoch=f.Jit.Authority.Epoch,FsBase=f.Machine()->fs.@base,GsBase=f.Machine()->gs.@base};
            if(kind=="epoch")--frame.ObservedGlobalEpoch;
            // Same passive lifetime the production mapper requires.
            f.Jit.BeginRead(f.Program,semantic:false);
            FullMemoryAssistResult result;
            try{lock(f.Jit.Authority.Gate)result=FullNativeMemory.TryFillScalarAtomic(f.Program,f.Machine(),&frame,f.Jit.Authority,in i);}
            finally{f.Jit.EndRead(f.Program);}
            Check(result==(kind is "readonly" or "unmapped"?FullMemoryAssistResult.GuestFault:kind=="epoch"?FullMemoryAssistResult.Retry:FullMemoryAssistResult.WriteBarrier),
                $"R183 exact typed conservative mapper outcome {kind}");
            Check(tlb[FullNativeMemory.TlbIndex(frame.MemoryAddress,63)].Host==null,"R183 denial publishes no RW hint");
            AtomicFamilyAssertClean(f);
        }
    }

    static void ScalarAtomicSnapshotInputs()
    {
        foreach(int width in new[]{8,16,32,64})
        foreach(string field in new[]{"base","source","implicit","flags","fs","gs"})
        {
            using var f=new ScalarAtomicFixture();ulong pc=ScalarAtomicFixture.Base,data=pc+8192;
            AtomicFamilySetup(f,pc,data);WriteControllerCode(f,pc,AtomicEncoding(width,[0x0f,width==8?(byte)0xb0:(byte)0xb1,0x0b]));
            AtomicFamilySeed(f,data+64);
            f.AtomicAssertSnapshot(pc,()=>{
                var m=f.Machine();
                if(field=="flags")m->flags^=1;else if(field=="fs")++m->fs.@base;else if(field=="gs")++m->gs.@base;
                else ++((ulong*)((byte*)m+24))[field=="base"?3:field=="source"?1:0];
            });
            AtomicFamilyAssertClean(f);
        }
    }

    static void ScalarAtomicRevocation()
    {
        using var f=new ScalarAtomicFixture();using var deadline=f.AtomicDeadline();
        ulong pc=ScalarAtomicFixture.Base,data=pc+8192;AtomicFamilySetup(f,pc,data);
        WriteControllerCode(f,pc,[0xf0,0x48,0x01,0x0b]);AtomicFamilySeed(f,data+64);
        *(ulong*)((byte*)f.Physical(data)+64)=0;
        Check(f.Step(pc,signalActor:true),"R183 first scalar populates native RW hint");
        long before=f.Jit.Authority.EpochAdvances;
        lock(f.Jit.Authority.Gate)
            Check(f.Jit.Authority.PrepareCodeSnapshot(f.Physical(data)),"R183 physical alias becomes code before snapshot");
        Check(f.Jit.Authority.EpochAdvances>before,"R183 code registration revokes cached write epoch");
        bool consumed=false;f.NativeReturned=p=>{consumed=((FullBlockFrame*)p)->MemoryFlags==FullMemoryFlags.BarrierConsumed;};
        AtomicFamilySeed(f,data+64);
        Check(f.Step(pc,signalActor:true)&&consumed&&f.Completed==2&&f.Issued==2&&
            *(ulong*)((byte*)f.Physical(data)+64)==10,"R183 stale RW hint cannot fast-write newly watched physical code");
        AtomicFamilyAssertClean(f);
    }

    static void ScalarAtomicFileDenials()
    {
        foreach(bool eof in new[]{false,true})foreach(int width in new[]{8,16,32,64})
        {
            using var f=new ScalarAtomicFixture();using var deadline=f.AtomicDeadline();
            ulong pc=ScalarAtomicFixture.Base,data=pc+8192;f.Map(pc);f.Fill(pc);f.Counts(100);
            int fd=f.Io.CreateMemoryFile("r183-scalar-file-denial",0).Value;
            Check(f.Io.WriteAt(fd,new byte[4096],0).Value==4096,"R183 scalar file initialized");f.MapShared(fd,data);
            WriteControllerCode(f,pc,AtomicEncoding(width,[width==8?(byte)0x00:(byte)0x01,0x0b]));AtomicFamilySeed(f,data+64);
            try
            {
                if(eof)
                {
                    Check(f.Io.TruncateFile(fd,0).Succeeded,"R183 scalar EOF before mapping");
                    var signal=f.StepSynchronousFault(pc);
                    Check(signal.Signal==7&&f.Completed==0&&f.Issued==1,"R183 scalar EOF is guest SIGBUS before locked instruction");
                }
                else
                {
                    bool ticket=false;f.NativeReturned=p=>{ticket=((FullBlockFrame*)p)->MemoryFlags==FullMemoryFlags.BarrierConsumed;};
                    Check(f.Step(pc,signalActor:true)&&ticket&&f.Completed==1&&f.Issued==1,
                        "R183 shared scalar keeps existing exclusive ticket, never ordinary fast RW");
                }
                AtomicFamilyAssertClean(f);
            }
            finally{Check(f.Io.Close(fd).Succeeded,"R183 scalar file descriptor cleanup");}
        }
    }

    static void ScalarAtomicAdmissionForgery()
    {
        using var f=new ScalarAtomicFixture();ulong pc=ScalarAtomicFixture.Base,data=pc+8192;
        AtomicFamilySetup(f,pc,data);AtomicFamilySeed(f,data+64);var i=AtomicClassify([0xf0,0x48,0x01,0x0b]) with {Ip=pc};
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        var frame=new FullBlockFrame{Machine=f.Machine(),MemoryResume=1,MemoryWidth=64,MemoryAccess=FullNativeMemory.AtomicReadWrite,
            MemoryAddress=data+64,MemoryTlb=tlb,MemoryTlbMask=63,ObservedGlobalEpoch=f.Jit.Authority.Epoch};
        f.Machine()->ip=pc;f.Jit.BeginRead(f.Program,semantic:false);
        try
        {
            lock(f.Jit.Authority.Gate)
            {
                // First registration may legitimately Retry at the advanced epoch.
                var result=FullNativeMemory.TryFillScalarAtomic(f.Program,f.Machine(),&frame,f.Jit.Authority,in i);
                if(result==FullMemoryAssistResult.Retry){frame.ObservedGlobalEpoch=f.Jit.Authority.Epoch;result=FullNativeMemory.TryFillScalarAtomic(f.Program,f.Machine(),&frame,f.Jit.Authority,in i);}
                Check(result==FullMemoryAssistResult.Resolved,"R183 typed mapper resolves existing write authority");
                Check(!FullNativeMemory.AuthenticateScalarAtomicMapping(&frame,in i,f.Machine(),f.Jit.Authority,false),"R183 Flags.None and exact mapping cannot substitute for actual enrollment");
                Check(f.Jit.Authority.TryEnterNative(frame.ObservedGlobalEpoch),"R183 actual test enrollment");
                try
                {
                    int at=FullNativeMemory.TlbIndex(data,63);var saved=tlb[at];
                    Check(FullNativeMemory.AuthenticateScalarAtomicMapping(&frame,in i,f.Machine(),f.Jit.Authority,true),"R183 exact scalar admission positive control");
                    foreach(string corrupt in new[]{"identity","context","flags","bounds","rights","epoch","host"})
                    {
                        tlb[at]=saved;
                        if(corrupt=="identity")++tlb[at].Identity;else if(corrupt=="context")++tlb[at].Context;
                        else if(corrupt=="flags")tlb[at].Flags=FullMemoryFlags.BarrierConsumed;else if(corrupt=="bounds")--tlb[at].ValidEnd;
                        else if(corrupt=="rights")tlb[at].Permissions=FullNativeMemory.AtomicReadWrite;else if(corrupt=="epoch")++tlb[at].AuthorityEpoch;
                        else {tlb[at].Host=(byte*)0x1000;tlb[at].Identity=0x1000;}
                        Check(!FullNativeMemory.AuthenticateScalarAtomicMapping(&frame,in i,f.Machine(),f.Jit.Authority,true),"R183 forged mapping rejected without registering/draining unknown page");
                    }
                    tlb[at]=saved;
                }
                finally{f.Jit.Authority.ExitNative();}
            }
        }
        finally{f.Jit.EndRead(f.Program);}
        AtomicFamilyAssertClean(f);
    }
}

namespace Managed.Emulation.Execution
{
    internal static partial class FullBlockEmitter
    {
        [ThreadStatic] internal static int ScalarAtomicProbeMode;
        private ref partial struct Writer
        {
            partial void AtomicFastCompareExchangeProbe(int width)
            {
                int mode=ScalarAtomicProbeMode;if(mode==0)return;
                // No CALL or guest output. A deterministic foreign-write model
                // changes memory before only the first CAS, forcing failure.
                Bytes(0x49,0x83,0xba);U32(200);Bytes(0);
                int later=NearCondition(0x85);
                if(width==16)Bytes(0x66);if(width==64)Bytes(0x48);
                Bytes(width==8?(byte)0xf6:(byte)0xf7,0x12); // NOT [rdx]
                if(mode==2){Bytes(0x4d,0x8b,0x4a,32,0x41,0xc7,0x01);U32(1);}
                if(mode==3){Bytes(0x4d,0x8b,0x8a);U32(128);Bytes(0xf0,0x49,0xff,0x01);}
                Patch(later,position);
                Bytes(0x49,0xff,0x82);U32(200);
            }
        }
    }
}
