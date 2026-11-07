using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using EnvironmentFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    readonly record struct EnvironmentCase(byte[] Bytes, FullProcessorEnvironmentOperation Op, int Width, int Register);
    static IEnumerable<EnvironmentCase> EnvironmentCases()
    {
        byte[][] prefixes = [[],[0x66],[0x67],[0xf2],[0xf3],[0xf0],[0x64,0x67],
            [0x48,0x66],[0x4f,0x67],[0xf3,0xf2],[0xf2,0xf3],[0x66,0x66],
            [0x26,0x2e,0x36,0x3e,0x64,0x65],Enumerable.Repeat((byte)0x67,10).ToArray()];
        foreach (var prefix in prefixes)
        {
            yield return new([..prefix,0x0f,0x31],FullProcessorEnvironmentOperation.Rdtsc,64,0);
            yield return new([..prefix,0x0f,0x01,0xf9],FullProcessorEnvironmentOperation.Rdtscp,64,0);
            foreach (int reg in Enumerable.Range(0,16)) foreach (int width in new[]{16,32,64})
            foreach (var op in new[]{FullProcessorEnvironmentOperation.Rdrand,FullProcessorEnvironmentOperation.Rdseed})
            {
                // Explicit final width prefix/REX makes the expected independent
                // of earlier canceled REXs. /7 effective F3 belongs to RDPID.
                byte[] widthPrefix=width==16?[0x66]:[];
                byte rex=(byte)(0x40|(reg>=8?1:0)|(width==64?8:0));
                byte[] tail=[rex];
                if(width==32 && prefix.Contains((byte)0x66)) continue;
                int rep=prefix.LastOrDefault(x=>x is 0xf2 or 0xf3);
                if(op==FullProcessorEnvironmentOperation.Rdseed && rep==0xf3) continue;
                byte[] bytes=[..prefix,..widthPrefix,..tail,0x0f,0xc7,(byte)(0xc0|(op==FullProcessorEnvironmentOperation.Rdrand?6:7)<<3|(reg&7))];
                if(bytes.Length<=15)yield return new(bytes,op,width,reg);
            }
        }
    }

    static JitInstruction EnvironmentClassify(EnvironmentCase row,ulong ip=Ip)
    {
        Check(FullProcessorEnvironmentLowering.TryClassify(ip,row.Bytes,out var i,out var why),why??"R175 classify");
        Check(i.Options==(uint)row.Op&&i.Width==row.Width&&i.Destination.Register==row.Register&&i.Ip==ip&&i.Length==row.Bytes.Length,
            "R175 exact selected prefix/width/register/original nextIP grammar");
        Check(FullBlockEmitter.ValidateProcessorEnvironment(i)&&FullBlockEmitter.CanInline(i)&&
            FullTranslationJit.TryClassifyNative(ip,row.Bytes,out var composed)&&composed==i,"R175 composed typed owner");
        return i;
    }

    static void CheckFullNativeProcessorEnvironment(byte* machine)
    {
        var failures=new List<Exception>();
        void Family(string name,Action action)
        {
            try{action();Console.WriteLine("ENV PASS "+name);}
            catch(Exception error){Console.Error.WriteLine("ENV FAIL "+name+": "+error);failures.Add(error);}
        }
        Family("grammar",CheckEnvironmentGrammar);
        Family("selected oracle",CheckEnvironmentSelectedOracle);
        Family("standalone leaves",()=>CheckEnvironmentLeaves(machine));
        Family("shared leaves",()=>
        {
            using var common=CreateFullScaffold(out var shared);fullOwnerScaffold=shared;
            try{CheckEnvironmentLeaves(machine);}finally{fullOwnerScaffold=default;}
        });
        Family("union ownership",()=>CheckEnvironmentIncomingUnion(machine));
        Family("boundaries",()=>CheckEnvironmentBoundaries(machine));
        Family("sampling and concurrency",CheckEnvironmentSamplingAndConcurrency);
        Family("controllers",CheckEnvironmentControllers);
        Family("memory interop",CheckEnvironmentMemoryInterop);
        Family("controller denials",CheckEnvironmentControllerDenials);
        Family("approved request attention and invalidation",CheckEnvironmentApprovedRequestControllers);
        Family("emission and allocation",CheckEnvironmentEmissions);
        ResetFull(machine);
        if(failures.Count!=0)throw new AggregateException("Processor environment family groups",failures);
    }

    static void CheckEnvironmentGrammar()
    {
        foreach(var row in EnvironmentCases())
        {
            _=EnvironmentClassify(row);
            for(int cut=0;cut<row.Bytes.Length;++cut)
                Check(!FullProcessorEnvironmentLowering.TryClassify(Ip,row.Bytes.AsSpan(0,cut),out _,out _),"R175 truncation");
            Check(!FullProcessorEnvironmentLowering.TryClassify(Ip,[..row.Bytes,0x90],out _,out _),"R175 trailing byte");
        }
        for(int modrm=0;modrm<256;++modrm)foreach(byte rep in new byte[]{0xf2,0xf3})
        {
            bool want=(modrm&0xf8)==0xf0||(modrm&0xf8)==0xf8&&rep!=0xf3;
            Check(FullProcessorEnvironmentLowering.TryClassify(Ip,[rep,0x0f,0xc7,(byte)modrm],out _,out _)==want,
                "R175 memory/reserved groups rejected, F3 /7 retains RDPID");
        }
        foreach(byte rex in Enumerable.Range(0x40,16).Select(x=>(byte)x))
        {
            Check(FullProcessorEnvironmentLowering.TryClassify(Ip,[0x66,rex,0x0f,0xc7,0xf7],out var i,out _)&&
                i.Width==((rex&8)!=0?64:16)&&i.Destination.Register==(7|((rex&1)<<3)),"R175 all REX fields; R/X ignored");
            Check(FullProcessorEnvironmentLowering.TryClassify(Ip,[rex,0x66,0x0f,0xc7,0xf7],out i,out _)&&
                i.Width==16&&i.Destination.Register==7,"R175 legacy cancels complete REX");
        }
        foreach(byte[] bytes in new byte[][]{[],[0x0f],[0x0f,0x01],[0x0f,0x01,0xf8],
            [0x0f,0xc7,0x30],[0xf3,0x0f,0xc7,0xf8],[..Enumerable.Repeat((byte)0x66,14),0x0f,0x31]})
            Check(!FullProcessorEnvironmentLowering.TryClassify(Ip,bytes,out _,out _),"R175 exact unsupported span");
        var good=EnvironmentClassify(new([0x0f,0xc7,0xf0],FullProcessorEnvironmentOperation.Rdrand,32,0));
        foreach(var bad in new[]{good with{Width=8},good with{Destination=new(16,0)},good with{Source=new(1,0)},
            good with{Length=16},good with{SourceWidth=16},good with{Address=new(1,-1,0,0)},good with{Condition=1}})
        {
            bool failed=false;try{FullBlockEmitter.ValidateProcessorEnvironment(bad);}catch(ArgumentException){failed=true;}
            Check(failed,"R175 malformed IR rejected before emitter publication");
        }
    }

    // Independent mathematical oracle: BigInteger modulo arithmetic, no Vigna,
    // lowering/emitter methods or selected handler calls inside the model.
    static ulong EnvironmentVigna(ulong before,out ulong state)
    {
        BigInteger mask=(BigInteger.One<<64)-1;
        BigInteger x=((BigInteger)before+BigInteger.Parse("11400714819323198485"))&mask;state=(ulong)x;
        x=((x^(x>>30))*BigInteger.Parse("13787848793156543929"))&mask;
        x=((x^(x>>27))*BigInteger.Parse("10723151780598845931"))&mask;
        return(ulong)(x^(x>>31));
    }
    static ulong EnvironmentTimestamp(long sec,long ns)=>(ulong)((((BigInteger)sec*1000000000+ns)*3)&((BigInteger.One<<64)-1));
    static ulong EnvironmentMerge(ulong old,ulong value,int width)=>width==16?(old&~65535UL)|(value&65535):width==32?(uint)value:value;
    static ulong EnvironmentRead(byte* m,int reg)=>BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(m+24+reg*8,8));
    static void EnvironmentSeed(byte* m, bool realWorker=false)
    {
        // A real worker owns system, ctid, caches and runtime fields. Whole-
        // Machine reset is only valid for isolated synthetic leaf storage.
        if(!realWorker)ResetFull(m);
        else
        {
            var cpu=(Blink.Machine*)m;cpu->ip=Ip;cpu->oplen=0;cpu->attention=false;
            cpu->mode.omode=Blink.XED_MODE_LONG;cpu->mode.genmode=Blink.XED_GEN_MODE_PROTECTED;
        }
        ((Blink.Machine*)m)->flags=0xa52c0ed4u&~0x50100u;
        for(int r=0;r<16;++r)Write64(m,24+r*8,0xfedcba9876543200UL+(uint)r);
    }

    sealed class EnvironmentClock : TimeProvider
    {
        internal int Calls;internal bool Fail;internal long Ticks=123456789;
        public override long TimestampFrequency=>TimeSpan.TicksPerSecond;
        public override long GetTimestamp(){++Calls;if(Calls==1)return 0;if(Fail)throw new IOException("script clock");return Ticks;}
    }
    sealed class EnvironmentEntropy : IHostEntropy
    {
        internal int Calls;internal int Bytes=8;internal bool Fail;internal ulong Word=0x0123456789abcdefUL;
        internal Action? During;
        public void Fill(Span<byte> bytes)
        {
            ++Calls;Check(bytes.Length==8,"R175 exact8 entropy service request");During?.Invoke();
            for(int n=0;n<Bytes;++n)bytes[n]=(byte)(Word>>(n*8));
            if(Fail)throw new IOException("script entropy after partial write");
        }
        internal ulong Applied(ulong old)
        {
            ulong result=old;for(int n=0;n<Bytes;++n)result=(result&~(255UL<<(n*8)))|((Word>>(n*8)&255)<<(n*8));return result;
        }
    }

    static void EnvironmentBind(EnvironmentClock clock,EnvironmentEntropy entropy)
    {Blink.UnbindHostEnvironment();Blink.BindHostEnvironment(new HostEnvironment(clock,entropy));}

    static void CheckEnvironmentSelectedOracle()
    {
        using var f=new EnvironmentFixture(interpreterOracle:true);
        try
        {
            foreach(var row in EnvironmentCases())
            {
                var clock=new EnvironmentClock();var entropy=new EnvironmentEntropy();EnvironmentBind(clock,entropy);
                var m=f.Machine();EnvironmentSeed((byte*)m,realWorker:true);
                ref var global=ref f.Program.Globals.g_rdrand__unit_2D5AE066DBCACEBBF21F0F23C80E9C4B510F23E759CD47D8B553E5446EE49B6C;
                global.state=0x1122334455667788;global.count=16;
                ulong old=EnvironmentRead((byte*)m,row.Register);uint flags=m->flags;
                f.DataScope(0,()=>HintReference(f.Program,(byte*)f.Machine(),row.Bytes));
                ulong want=row.Op==FullProcessorEnvironmentOperation.Rdrand?EnvironmentVigna(entropy.Word,out _):
                    row.Op==FullProcessorEnvironmentOperation.Rdseed?entropy.Word:EnvironmentTimestamp(12,345678900);
                Check(m->flags==(row.Op is FullProcessorEnvironmentOperation.Rdrand or FullProcessorEnvironmentOperation.Rdseed?flags|1:flags),
                    "R175 selected CF only, clock preserves all guest flags");
                if(row.Op is FullProcessorEnvironmentOperation.Rdtsc or FullProcessorEnvironmentOperation.Rdtscp)
                    Check(EnvironmentRead((byte*)m,0)==(uint)want&&EnvironmentRead((byte*)m,2)==want>>32&&
                        (row.Op!=FullProcessorEnvironmentOperation.Rdtscp||EnvironmentRead((byte*)m,1)==0)&&clock.Calls==2&&entropy.Calls==0,
                        "R175 independent selected CLOCK_MONOTONIC scale3 and TSC_AUX0");
                else Check(EnvironmentRead((byte*)m,row.Register)==EnvironmentMerge(old,want,row.Width)&&entropy.Calls==1&&clock.Calls==1,
                    "R175 independent selected RNG width/register/service oracle");
            }
            foreach(int partial in new[]{0,3,8})foreach(uint count in new uint[]{0,1,15,16,31,uint.MaxValue})
            {
                var clock=new EnvironmentClock();var entropy=new EnvironmentEntropy{Bytes=partial,Fail=true};EnvironmentBind(clock,entropy);
                var m=f.Machine();EnvironmentSeed((byte*)m,realWorker:true);
                ref var global=ref f.Program.Globals.g_rdrand__unit_2D5AE066DBCACEBBF21F0F23C80E9C4B510F23E759CD47D8B553E5446EE49B6C;
                const ulong previous=0xffeeddccbbaa9988;global.state=previous;global.count=count;
                f.DataScope(0,()=>HintReference(f.Program,(byte*)f.Machine(),[0x48,0x0f,0xc7,0xf0]));
                ulong want=EnvironmentVigna((count&15)==0?entropy.Applied(previous):previous,out ulong state);
                Check(global.count==unchecked(count+1)&&global.state==state&&EnvironmentRead((byte*)m,0)==want&&
                    entropy.Calls==((count&15)==0?1:0)&&(m->flags&1)==1,"R175 selected reseed failures retain original global bytes/count wrap");
                EnvironmentSeed((byte*)m,realWorker:true);entropy.Calls=0;
                f.DataScope(0,()=>HintReference(f.Program,(byte*)f.Machine(),[0x48,0x0f,0xc7,0xf8]));
                Check(EnvironmentRead((byte*)m,0)==entropy.Applied(0)&&entropy.Calls==1&&(m->flags&1)==1,
                    "R175 selected RDSEED failure retains partial ZERO initial buffer and CF1");
            }
        }
        finally{f.RestoreEnvironment();}
    }

    static void EnvironmentReadyValue(FullBlockFrame* f,in JitInstruction i,ulong data0,ulong data1,ulong* state,uint* count)
    {
        var d=(FullEnvironmentDataDescriptor*)f->StackMemory;
        d->State=FullEnvironmentDataState.Ready;d->Data0=data0;d->Data1=data1;
        d->RngState=state;d->RngCount=count;d->LeaseCookie=17;
        f->Temporary0=(ulong)state;f->Temporary1=(ulong)count;f->Temporary3=17;
        f->FirstPreissued=1;f->NativeLimit=1;f->Executed=0;
    }

    static void CheckEnvironmentLeaves(byte* m)
    {
        foreach(var row in EnvironmentCases().Where(r=>r.Bytes.Length<=4))
        {
            var i=EnvironmentClassify(row);using var code=CreateFullBlock([new(i)]);
            foreach(ulong word in new[]{0UL,1UL,0xffffffffffffffffUL,0x0123456789abcdefUL})
            foreach(uint initialCount in row.Op==FullProcessorEnvironmentOperation.Rdrand?new uint[]{0,1,15,16,uint.MaxValue}:new uint[]{0})
            {
                EnvironmentSeed(m);ulong epoch=11;long issue=0,done=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                FullEnvironmentDataDescriptor descriptor=default;
                var f=FullFrame(m,&h,&epoch,&issue,&done,&stop,limit:1);f.StackMemory=(FullStackMemoryDescriptor*)&descriptor;
                byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                Check(RunFull(code,&f)==FullBlockExit.NeedEnvironment&&issue==1&&done==0&&f.Executed==0&&
                    new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),"R175 data request has no guest/global computation");
                _=FullProcessorEnvironmentProtocol.Capture(&f,in i);
                ulong initialState=word^0xdeadbeef00112233UL;ulong state=initialState;uint count=initialCount;
                bool rng=row.Op==FullProcessorEnvironmentOperation.Rdrand;
                EnvironmentReadyValue(&f,in i,word,row.Op is FullProcessorEnvironmentOperation.Rdtsc or FullProcessorEnvironmentOperation.Rdtscp?word:0,rng?&state:null,rng?&count:null);
                // Deliberately change both current epoch and stop AFTER data.
                ++epoch;stop=1;h.Live=0;h.CodeEpoch++;
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issue==1&&done==1&&f.Executed==1&&
                    descriptor.State==FullEnvironmentDataState.Consumed&&f.FirstPreissued==0&&f.ResumeIndex==1,
                    "R175 commit-only root path survives advancing writer/stop before BOTH scaffold entries");
                ulong want=row.Op==FullProcessorEnvironmentOperation.Rdrand?EnvironmentVigna((initialCount&15)==0?word:initialState,out _):
                    row.Op==FullProcessorEnvironmentOperation.Rdseed?word:EnvironmentTimestamp(unchecked((long)word),unchecked((long)word));
                if(row.Op is FullProcessorEnvironmentOperation.Rdtsc or FullProcessorEnvironmentOperation.Rdtscp)
                    Check(EnvironmentRead(m,0)==(uint)want&&EnvironmentRead(m,2)==want>>32&&
                        (row.Op!=FullProcessorEnvironmentOperation.Rdtscp||EnvironmentRead(m,1)==0),"R175 emitted timestamp wrap and zero extension");
                else
                {
                    ulong old=BinaryPrimitives.ReadUInt64LittleEndian(before.AsSpan(24+row.Register*8));
                    Check(EnvironmentRead(m,row.Register)==EnvironmentMerge(old,want,row.Width)&&
                        ((Blink.Machine*)m)->flags==(BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12))|1),"R175 emitted allwidth result/CF only");
                    if(rng){_=EnvironmentVigna((initialCount&15)==0?word:initialState,out ulong expected);Check(state==expected&&count==unchecked(initialCount+1),"R175 actual emitted count/state stores and nonreseed/count wrap");}
                }
                // Consumed never re-enters Ready. Both header liveness and
                // stop were revoked above; ordinary entry may reject either.
                byte[] committed=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                var repeated=RunFull(code,&f);
                Check(repeated is FullBlockExit.Stop or FullBlockExit.Invalidated && issue==1&&done==1&&
                    new ReadOnlySpan<byte>(m,committed.Length).SequenceEqual(committed),
                    "R175 consumed input cannot commit twice");
                // Synthetic leaf descriptors have no real service lease. They
                // are stack-owned and end here; production reset is exercised
                // with the actual lease below and in the mixed controller run.
            }
        }
        foreach(var op in new[]{FullProcessorEnvironmentOperation.Rdtsc,FullProcessorEnvironmentOperation.Rdtscp})
        foreach(sbyte codeValue in new sbyte[]{0,1,2,-2})
        {
            byte[] bytes=op==FullProcessorEnvironmentOperation.Rdtsc?[0x0f,0x31]:[0x0f,0x01,0xf9];
            var i=EnvironmentClassify(new(bytes,op,64,0));using var code=CreateFullBlock([new(i)]);
            EnvironmentSeed(m);((Blink.Machine*)m)->traprdtsc=true;((Blink.Machine*)m)->segvcode=codeValue;
            ulong epoch=11;long issue=0,done=0;int stop=0;var h=FullHeader(Ip,code.Entry);FullEnvironmentDataDescriptor d=default;
            var f=FullFrame(m,&h,&epoch,&issue,&done,&stop,limit:1);f.StackMemory=(FullStackMemoryDescriptor*)&d;
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==14&&f.FaultAddress==0&&
                f.FaultCode==unchecked((uint)(int)codeValue)&&d.Tag==0&&issue==1&&done==0&&
                new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),"R175 traprdtsc selected SIGSEGV before sample/RCX/guest writes");
        }
    }

    static void CheckEnvironmentIncomingUnion(byte* m)
    {
        var row=new EnvironmentCase([0x48,0x0f,0xc7,0xf0],FullProcessorEnvironmentOperation.Rdrand,64,0);
        var i=EnvironmentClassify(row);using var code=CreateFullBlock([new(i)]);
        foreach(uint tag in new[]{1u,FullCanonicalX87MemoryLowering.MemoryProbeTag,FullEnterLowering.MemoryTag,
            FullMaskedStoreDescriptor.CurrentTagVersion,FullEnvironmentDataDescriptor.TagValue,0xdeadbeefu})
        foreach(uint state in new[]{0u,1u,2u,3u,4u,5u})foreach(bool ticket in new[]{false,true})
        foreach(bool denseIdle in tag==1u?new[]{false,true}:new[]{false})
        {
            EnvironmentSeed(m);ulong epoch=11;long issue=0,done=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            FullEnvironmentDataDescriptor incoming=default;byte* raw=(byte*)&incoming;
            *(uint*)(raw+0)=tag;*(uint*)(raw+4)=tag==FullMaskedStoreDescriptor.CurrentTagVersion?
                FullMaskedStoreDescriptor.SparseKind:tag==FullEnterLowering.MemoryTag?2u:1u;
            if(denseIdle)*(uint*)(raw+4)=0;
            *(uint*)(raw+12)=state;*(ulong*)(raw+24)=ticket?123UL:0;
            if(tag==FullEnvironmentDataDescriptor.TagValue){incoming.LeaseCookie=123;incoming.Header=&h;}
            byte[] descriptorBefore=new ReadOnlySpan<byte>(raw,128).ToArray();
            byte[] machineBefore=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            var f=FullFrame(m,&h,&epoch,&issue,&done,&stop,limit:1);f.StackMemory=(FullStackMemoryDescriptor*)&incoming;
            bool rejected=tag==FullEnvironmentDataDescriptor.TagValue||tag==0xdeadbeefu||ticket||
                (denseIdle?state>4u||state==3u:state!=4u);
            var exit=RunFull(code,&f);
            if(rejected)Check(exit==FullBlockExit.Invalidated&&done==0&&
                new ReadOnlySpan<byte>(raw,128).SequenceEqual(descriptorBefore)&&
                new ReadOnlySpan<byte>(m,machineBefore.Length).SequenceEqual(machineBefore)&&f.MemoryFlags==0&&f.MemoryTicket==0,
                "R175 incoming union tag/state/count/descriptor ticket/ENV sampled identity denied BEFORE reset, all evidence retained");
            else Check(exit==FullBlockExit.NeedEnvironment&&done==0&&incoming.Tag==FullEnvironmentDataDescriptor.TagValue,
                "R175 only known idle/consumed no-ticket union is normalized");
        }
    }

    static void CheckEnvironmentBoundaries(byte* m)
    {
        foreach(var row in new[]{new EnvironmentCase([0x0f,0x31],FullProcessorEnvironmentOperation.Rdtsc,64,0),
            new EnvironmentCase([0x0f,0x01,0xf9],FullProcessorEnvironmentOperation.Rdtscp,64,0),
            new EnvironmentCase([0x48,0x0f,0xc7,0xf0],FullProcessorEnvironmentOperation.Rdrand,64,0),
            new EnvironmentCase([0x48,0x0f,0xc7,0xf8],FullProcessorEnvironmentOperation.Rdseed,64,0)})
        foreach(string boundary in new[]{"stop","cancel","budget","invalidated","attention"})
        {
            var i=EnvironmentClassify(row);using var code=CreateFullBlock([new(i)]);EnvironmentSeed(m);
            ulong epoch=11;long issue=0,done=0;int stopped=boundary=="stop"?1:0;var h=FullHeader(Ip,code.Entry);
            if(boundary=="invalidated")h.Live=0;if(boundary=="attention")m[11]=1;
            using var cancel=new CancellationTokenSource();using var owner=new HostExecutionStop(cancellation:cancel.Token);
            using var poll=owner.AcquireNativePoll();if(boundary=="cancel")cancel.Cancel();
            FullEnvironmentDataDescriptor d=default;byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            var f=FullFrame(m,&h,&epoch,&issue,&done,boundary=="cancel"?poll.Pointer:&stopped,boundary=="budget"?0UL:100UL,1);
            f.StackMemory=(FullStackMemoryDescriptor*)&d;
            var want=boundary is "stop" or "cancel"?FullBlockExit.Stop:boundary=="budget"?FullBlockExit.Budget:
                boundary=="attention"?FullBlockExit.Attention:FullBlockExit.Invalidated;
            Check(RunFull(code,&f)==want&&done==0&&issue==(boundary=="budget"?1:0)&&d.Tag==0&&
                new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),"R175 all4 boundaries precede data service/global/guest effects");
        }
    }

    static void CheckEnvironmentSamplingAndConcurrency()
    {
        using var f=new EnvironmentFixture();var clock=new EnvironmentClock();var entropy=new EnvironmentEntropy{Bytes=3,Fail=true};
        EnvironmentBind(clock,entropy);
        try
        {
            int worker=f.WorkerId(0),foreign=f.WorkerId(1);
            lock(f.Jit.Authority.Gate)Check(f.Jit.Authority.TryReserveNativeTicket(foreign),"R175 incoming ticket published");
            try
            {
                Check(!FullProcessorEnvironmentLease.TryAcquire(f.Program,f.Jit.Authority,worker,true,out var blocked)&&blocked==null&&entropy.Calls==0,
                    "R175 incoming ticket declines BEFORE global mutex/entropy admission");
            }
            finally{f.Jit.Authority.AbortNativeReservation(foreign);}
            ulong beforePrivateLeaseEpoch=f.Jit.Authority.Epoch;
            Check(FullProcessorEnvironmentLease.TryAcquire(f.Program,f.Jit.Authority,worker,true,out var owned),"R175 real pinned global lease");
            Check(f.Jit.Authority.Epoch==beforePrivateLeaseEpoch,"private ENV lease never invalidates its own checked issue");
            using var lease=owned!;
            *lease.State=0xffeeddccbbaa9988;*lease.Count=16;
            var competing=Task.Run(()=>
            {
                bool acquired=FullProcessorEnvironmentLease.TryAcquire(f.Program,f.Jit.Authority,foreign,true,out var other);
                other?.Dispose();return acquired;
            });
            Check(competing.Wait(TimeSpan.FromSeconds(5))&&!competing.Result,"R175 same actual mutex excludes foreign worker while main lease held");
            Check(f.Jit.Authority.CpuSemanticDepth(foreign)==0&&!f.Jit.Authority.SemanticAdmitted(foreign),"R175 busy-mutex attempt releases admission/runtime retain");
            var row=new EnvironmentCase([0x48,0x0f,0xc7,0xf0],FullProcessorEnvironmentOperation.Rdrand,64,0);
            var i=EnvironmentClassify(row);using var code=CreateFullBlock([new(i)]);
            byte* m=(byte*)f.Machine();EnvironmentSeed(m,realWorker:true);
            long issue=0,done=0;int stopped=0;var h=FullHeader(Ip,code.Entry);
            h.ValidatedGlobalEpoch=f.Jit.Authority.CodeEpoch;
            var frame=FullFrame(m,&h,f.Jit.Authority.EpochPointer,&issue,&done,&stopped,limit:1,codeEpoch:f.Jit.Authority.CodeEpochPointer);
            FullEnvironmentDataDescriptor d=default;frame.StackMemory=(FullStackMemoryDescriptor*)&d;
            Check(RunFull(code,&frame)==FullBlockExit.NeedEnvironment,"R175 first approved data-only exit");
            _=FullProcessorEnvironmentProtocol.Capture(&frame,in i);frame.FirstPreissued=1;frame.Executed=0;
            bool declined=false;try{FullProcessorEnvironmentProtocol.PrepareReady(&frame,in i,lease,actuallyEnrolled:false);}
            catch(InvalidOperationException){declined=true;}
            Check(declined&&!lease.Sampled&&entropy.Calls==0&&d.State==FullEnvironmentDataState.Request,
                "R175 register cap1/no enrollment can never sample or mint global Ready pointers");
            Check(f.Jit.Store.TryBeginNativeEpoch(out var epoch),"R175 actual store epoch before sampling");
            bool enrolled; lock(f.Jit.Authority.Gate)
                enrolled=f.Jit.Authority.TryEnterProcessorEnvironmentNative(worker,frame.ObservedGlobalEpoch);
            if(!enrolled) { epoch.Dispose(); throw new InvalidOperationException("ENV private commit could not enroll; store epoch released"); }
            Check(enrolled,"R175 actual typed native enrollment before sampling");
            using var writerPublished=new ManualResetEventSlim();using var writerFinished=new ManualResetEventSlim();Task? writer=null;
            FullProcessorEnvironmentProtocol.TestAfterSample=pointer=>
            {
                var ready=(FullBlockFrame*)pointer;*ready->Stop=1;
                writer=Task.Run(()=>
                {
                    f.Jit.Authority.BeginHostWriter();writerPublished.Set();
                    try{f.Jit.Authority.WaitNativeFrames();}finally{f.Jit.Authority.EndHostWriter();writerFinished.Set();}
                });
                Check(writerPublished.Wait(TimeSpan.FromSeconds(5))&&!writerFinished.IsSet,
                    "R175 writer advances runtime/code epochs AFTER sample but waits actual native enrollment");
            };
            try
            {
                FullProcessorEnvironmentProtocol.PrepareReady(&frame,in i,lease,enrolled);
                Check(entropy.Calls==1&&d.Data0==entropy.Applied(0xffeeddccbbaa9988)&&*lease.Count==16,
                    "R175 pure entropy assist retains pre-existing bytes; count/state arithmetic deferred to emitted code");
                Check(RunFull(code,&frame)==FullBlockExit.Continue,"R175 sampled Ready commits despite writer epoch+stop before root entry");
                FullProcessorEnvironmentProtocol.RequireConsumed(&frame,in i,lease);
                ulong want=EnvironmentVigna(entropy.Applied(0xffeeddccbbaa9988),out ulong state);
                Check(issue==1&&done==1&&entropy.Calls==1&&*lease.Count==17&&*lease.State==state&&EnvironmentRead(m,0)==want&&!writerFinished.IsSet,
                    "R175 one sample/one native commit, global lock and native enrollment retained through RET");
                FullProcessorEnvironmentProtocol.ResetConsumed(&frame,in i,lease);
                Check(frame.StackMemory->Version==1&&frame.StackMemory->Count==0&&frame.Temporary0==0&&frame.Temporary1==0&&frame.Temporary3==0,
                    "R175 acknowledged Consumed restores whole dense idle union before unlocking");
            }
            finally
            {
                FullProcessorEnvironmentProtocol.TestAfterSample=null;
                f.Jit.Authority.ExitNative();epoch.Dispose();
                Check(writer==null||writer.Wait(TimeSpan.FromSeconds(5)),"R175 writer completes after native RET/enrollment release");
            }
            // No sampled malformed input may trigger a service retry. Exercise
            // every descriptor identity component against emitted Ready entry.
            foreach(int offset in new[]{4,8,16,24,32,40,48,56,64,72,80,84,104,112,120})
            {
                EnvironmentSeed(m,realWorker:true);ulong localEpoch=11;long a=0,b=0;int c=0;var header=FullHeader(Ip,code.Entry);
                var bad=FullFrame(m,&header,&localEpoch,&a,&b,&c,limit:1);FullEnvironmentDataDescriptor value=default;
                bad.StackMemory=(FullStackMemoryDescriptor*)&value;Check(RunFull(code,&bad)==FullBlockExit.NeedEnvironment,"R175 auth request");
                EnvironmentReadyValue(&bad,in i,1,0,lease.State,lease.Count);
                ((byte*)&value)[offset]^=1;ulong beforeState=*lease.State;uint beforeCount=*lease.Count;
                Check(RunFull(code,&bad)==FullBlockExit.Invalidated&&b==0&&*lease.State==beforeState&&*lease.Count==beforeCount&&entropy.Calls==1,
                    "R175 malformed sampled plan/context/site/epochs/global pointers/cookie rejects before ANY global or guest write; no resampling");
            }
        }
        finally{f.RestoreEnvironment();}
        Check(f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0))==0&&!f.Jit.Authority.SemanticAdmitted(f.WorkerId(0)),"R175 final real lease cleanup");
        Check(FullProcessorEnvironmentLease.TryAcquire(f.Program,f.Jit.Authority,f.WorkerId(0),true,out var reacquired),"R175 RNG mutex available after all RET cleanup");
        reacquired!.Dispose();
    }

    static void CheckEnvironmentControllers()
    {
        foreach(bool afterStop in new[]{false,true})foreach(bool crossFetch in new[]{false,true})
        {
            using var f=new EnvironmentFixture();var clock=new EnvironmentClock();var entropy=new EnvironmentEntropy();EnvironmentBind(clock,entropy);
            try
            {
                const ulong page=EnvironmentFixture.Base;f.Map(page,8192);f.Fill(page,8192);
                ulong pc=page+(crossFetch?4095UL:0UL);var bytes=new List<byte>();
                // One mixed family corpus,32 original instructions, with actual
                // arithmetic between all four environment operations. No per-op
                // compilation/build/delivery queue; retained body resumes by site.
                for(int n=0;n<4;++n)bytes.AddRange(new byte[]{0x49,0x83,0xc4,1,0x49,0x83,0xc5,1,
                    0x49,0x0f,0xc7,0xf0,0x49,0x0f,0xc7,0xf9,0x0f,0x31,0x0f,0x01,0xf9,0x90,0x90});
                f.DataScope(0,()=>
                {
                    for(int n=0;n<bytes.Count;++n){byte* dst=f.Program.LookupAddress2(f.Machine(),(long)(pc+(uint)n),0,0);Check(dst!=null,"R175 code backing");*dst=bytes[n];}
                });
                EnvironmentSeed((byte*)f.Machine(),realWorker:true);Write64((byte*)f.Machine(),24+12*8,0);Write64((byte*)f.Machine(),24+13*8,0);
                ref var global=ref f.Program.Globals.g_rdrand__unit_2D5AE066DBCACEBBF21F0F23C80E9C4B510F23E759CD47D8B553E5446EE49B6C;
                global.state=3;global.count=0;int dataReady=0;
                using var published=new ManualResetEventSlim();using var finished=new ManualResetEventSlim();Task? writer=null;
                using var cancellation=new CancellationTokenSource();
                using var cancellationBinding=cancellation.Token.Register(()=>f.Stop.RequestStop());
                f.NativeReturned=pointer=>
                {
                    if(afterStop&&dataReady==1)Check(!finished.IsSet,
                        "R175 ACTUAL controller writer waits native enrollment through normal Ready RET");
                };
                FullProcessorEnvironmentProtocol.TestAfterSample=pointer=>
                {
                    ++dataReady;
                    if(afterStop&&dataReady==1)
                    {
                        cancellation.Cancel();
                        writer=Task.Run(()=>
                        {
                            f.Jit.Authority.BeginHostWriter();published.Set();
                            try{f.Jit.Authority.WaitNativeFrames();}
                            finally{f.Jit.Authority.EndHostWriter();finished.Set();}
                        });
                        Check(published.Wait(TimeSpan.FromSeconds(5))&&!finished.IsSet,
                            "R175 actual controller AFTER sample: writer code/runtime epoch change and stop before native root entry");
                    }
                };
                f.Counts(32);Check(f.Step(pc),"R175 real selected decode/compiler/controller starts one family corpus");
                for(int retry=0;f.Completed<(afterStop?3UL:32UL)&&retry<128;++retry)Check(f.Continue(),"R175 bounded retained family continuation");
                if(afterStop)
                    Check(dataReady==1&&entropy.Calls==1&&global.count==1&&f.Completed==3&&EnvironmentRead((byte*)f.Machine(),12)==1,
                        "R175 real controller defers post-sample stop through single global native commit");
                // Exhausting budget records the next rejected Issue, as in ExecuteOne.
                else Check(f.Completed==32&&f.Issued==33&&dataReady==16&&entropy.Calls==5&&clock.Calls==9&&global.count==4&&
                    EnvironmentRead((byte*)f.Machine(),12)==4&&EnvironmentRead((byte*)f.Machine(),13)==4&&f.Machine()->ip==pc+(uint)bytes.Count,
                    $"R175 mixed body: completed={f.Completed} issued={f.Issued} ready={dataReady} entropy={entropy.Calls} clock={clock.Calls} count={global.count} r12={EnvironmentRead((byte*)f.Machine(),12):x} r13={EnvironmentRead((byte*)f.Machine(),13):x} ip={f.Machine()->ip:x} expected={pc+(uint)bytes.Count:x}");
                Check(writer==null||writer.Wait(TimeSpan.FromSeconds(5)),"R175 actual controller releases writer after acknowledged RET");
                f.NativeReturned=null;
                Check(f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0&&
                    f.Jit.Store.GetStatistics().ActiveNativeEpochs==0&&f.Jit.Store.GetStatistics().ActiveManagedLeases==0&&
                    f.Statistics.ActiveDataBorrows==0&&!f.Jit.Authority.SemanticAdmitted(f.WorkerId(0)),
                    "R175 actual controller releases all code/data/semantic/global leases with zero semantic helpers");
            }
            finally{FullProcessorEnvironmentProtocol.TestAfterSample=null;f.RestoreEnvironment();}
        }
        // Service errors and missing binding are DATA, not synthetic guest
        // faults or CF0. Clock/RDSEED zero initialize; RDRAND preserves state.
        foreach(var op in new[]{FullProcessorEnvironmentOperation.Rdtsc,FullProcessorEnvironmentOperation.Rdtscp,
            FullProcessorEnvironmentOperation.Rdrand,FullProcessorEnvironmentOperation.Rdseed})
        foreach(bool missing in new[]{false,true})
        {
            using var f=new EnvironmentFixture();var clock=new EnvironmentClock{Fail=true};var entropy=new EnvironmentEntropy{Bytes=0,Fail=true};
            EnvironmentBind(clock,entropy);if(missing)Blink.UnbindHostEnvironment();
            try
            {
                ulong pc=EnvironmentFixture.Base;f.Map(pc);f.Fill(pc);
                byte[] body=op==FullProcessorEnvironmentOperation.Rdtsc?[0x0f,0x31]:op==FullProcessorEnvironmentOperation.Rdtscp?
                    [0x0f,0x01,0xf9]:[0x48,0x0f,0xc7,(byte)(op==FullProcessorEnvironmentOperation.Rdrand?0xf0:0xf8)];
                f.DataScope(0,()=>{byte* dst=f.Program.LookupAddress2(f.Machine(),(long)pc,0,0);body.CopyTo(new Span<byte>(dst,body.Length));});
                EnvironmentSeed((byte*)f.Machine(),realWorker:true);uint flags=f.Machine()->flags;
                ref var global=ref f.Program.Globals.g_rdrand__unit_2D5AE066DBCACEBBF21F0F23C80E9C4B510F23E759CD47D8B553E5446EE49B6C;
                global.state=77;global.count=0;f.Counts(1);
                Check(f.Step(pc)&&f.Completed==1&&f.Issued==1,"R175 missing/error service controller completes approved operation once");
                ulong want=op==FullProcessorEnvironmentOperation.Rdrand?EnvironmentVigna(77,out _):0;
                Check(EnvironmentRead((byte*)f.Machine(),0)==(op==FullProcessorEnvironmentOperation.Rdtsc||op==FullProcessorEnvironmentOperation.Rdtscp?(uint)want:want)&&
                    f.Machine()->flags==(op is FullProcessorEnvironmentOperation.Rdrand or FullProcessorEnvironmentOperation.Rdseed?flags|1:flags),
                    "R175 exact error numerical/destination/flags semantics from selected model");
            }
            finally{f.RestoreEnvironment();}
        }
    }

    static void CheckEnvironmentMemoryInterop()
    {
        // A pair operation leaves Consumed, then an ordinary load clears only
        // Count. The following approved environment operation must accept this
        // idle union without erasing any live ticket or resampling.
        foreach(var row in new[]{new EnvironmentCase([0x0f,0x31],FullProcessorEnvironmentOperation.Rdtsc,64,0),
            new EnvironmentCase([0x0f,0x01,0xf9],FullProcessorEnvironmentOperation.Rdtscp,64,0),
            new EnvironmentCase([0x48,0x0f,0xc7,0xf0],FullProcessorEnvironmentOperation.Rdrand,64,0),
            new EnvironmentCase([0x48,0x0f,0xc7,0xf8],FullProcessorEnvironmentOperation.Rdseed,64,0)})
        {
            using var f=new EnvironmentFixture();var clock=new EnvironmentClock();var entropy=new EnvironmentEntropy();EnvironmentBind(clock,entropy);
            try
            {
                const ulong pc=EnvironmentFixture.Base,data=pc+0x10000,stack=data+0x10000,value=0x123456789abcdef0;
                f.Map(pc);f.Fill(pc);f.Map(data);f.Fill(data);f.Map(stack);f.Fill(stack);
                byte[] body=[0xff,0x36,0x4c,0x8b,0x1e,..row.Bytes]; // push [rsi]; mov r11,[rsi]; ENV
                f.DataScope(0,()=>
                {
                    body.CopyTo(new Span<byte>(f.Program.LookupAddress2(f.Machine(),(long)pc,0,0),body.Length));
                    Write64(f.Program.LookupAddress2(f.Machine(),(long)data,0,0),0,value);
                });
                EnvironmentSeed((byte*)f.Machine(),realWorker:true);
                Write64((byte*)f.Machine(),24+6*8,data);Write64((byte*)f.Machine(),24+4*8,stack+0x800);
                int samples=0;FullProcessorEnvironmentProtocol.TestAfterSample=_=>++samples;
                f.Counts(3);Check(f.Step(pc),"ENV memory interop starts");
                for(int retry=0;f.Completed<3&&retry<64;++retry)Check(f.Continue(),"ENV memory interop continuation");
                Check(f.Completed==3&&f.Issued==3&&samples==1&&EnvironmentRead((byte*)f.Machine(),11)==value&&
                    EnvironmentRead((byte*)f.Machine(),4)==stack+0x7f8&&f.Machine()->ip==pc+(uint)body.Length,
                    "pair/ordinary memory/ENV share one issue stream and one data sample");
                f.DataScope(0,()=>Check(*(ulong*)f.Program.LookupAddress2(f.Machine(),(long)(stack+0x7f8),0,0)==value,
                    "memory interop preserves native push effect"));
                Check(f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0&&
                    f.Jit.Store.GetStatistics().ActiveNativeEpochs==0&&f.Jit.Store.GetStatistics().ActiveManagedLeases==0,
                    "memory interop has no semantic helper or retained lifetime leak");
            }
            finally{FullProcessorEnvironmentProtocol.TestAfterSample=null;f.RestoreEnvironment();}
        }
    }

    static void CheckEnvironmentControllerDenials()
    {
        foreach(string boundary in new[]{"budget0","cancel-before"})
        {
            using var f=new EnvironmentFixture();var clock=new EnvironmentClock();var entropy=new EnvironmentEntropy();EnvironmentBind(clock,entropy);
            using var cancellation=new CancellationTokenSource();
            using var binding=cancellation.Token.Register(()=>f.Stop.RequestStop());
            try
            {
                ulong pc=EnvironmentFixture.Base;f.Map(pc);f.Fill(pc);
                f.DataScope(0,()=>{byte* dst=f.Program.LookupAddress2(f.Machine(),(long)pc,0,0);new byte[]{0x48,0x0f,0xc7,0xf0}.CopyTo(new Span<byte>(dst,4));});
                EnvironmentSeed((byte*)f.Machine(),realWorker:true);
                f.Counts(boundary=="budget0"?0UL:10UL);if(boundary=="cancel-before")cancellation.Cancel();
                ulong old=EnvironmentRead((byte*)f.Machine(),0);
                _=f.Step(pc);
                Check(f.Completed==0&&f.Issued==(boundary=="budget0"?1UL:0UL)&&entropy.Calls==0&&clock.Calls==1&&
                    EnvironmentRead((byte*)f.Machine(),0)==old&&f.Jit.Store.GetStatistics().ActiveNativeEpochs==0&&
                    f.Jit.Store.GetStatistics().ActiveManagedLeases==0&&!f.Jit.Authority.SemanticAdmitted(f.WorkerId(0)),
                    "R175 actual controller budget/cancellation precedes service/global effects and releases all capabilities");
            }
            finally{f.RestoreEnvironment();}
        }
    }

    static void CheckEnvironmentEmissions()
    {
        byte[] code=new byte[FullBlockEmitter.CodeCapacity],slab=new byte[FullBlockEmitter.ScaffoldCapacity];
        var rows=EnvironmentCases().Where(r=>r.Bytes.Length<=4).Take(32).ToArray();
        foreach(bool windows in new[]{false,true})
        {
            FullBlockEmitter.EmitScaffold(slab,out var layout,windowsAbi:windows);
            var addresses=layout.Resolve((nint)0x10000000);
            foreach(bool shared in new[]{false,true})
            {
                var body=new FullBlockInstruction[rows.Length];ulong ip=Ip;
                for(int n=0;n<body.Length;++n){var i=EnvironmentClassify(rows[n],ip);body[n]=new(i);ip+=(uint)i.Length;}
                int length=shared?FullBlockEmitter.Emit(body,code,in addresses,windowsAbi:windows):FullBlockEmitter.Emit(body,code,windowsAbi:windows);
                Check(length>0&&CountFullPattern(code.AsSpan(0,length),[0x48,0x0f,0xaf,0xc1])>0,
                    "R175 substantial ABI/scaffold body contains literal emitted Vigna multiply, never host RNG instruction");
                Check(layout.ExitStrideCheckForEnvironment(),"R175 exact tenth shared typed exit fits unchanged descriptor/frame ABI");
            }
        }
    }
}

namespace Managed.Emulation.Execution
{
    internal static unsafe partial class FullProcessorEnvironmentProtocol
    {
        [ThreadStatic] internal static Action<nint>? TestAfterSample;
        static partial void DataReady(FullBlockFrame* frame)=>TestAfterSample?.Invoke((nint)frame);
    }
    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal sealed partial class FullControllerFixture
        {
            internal void RestoreEnvironment(){Blink.UnbindHostEnvironment();Blink.BindHostEnvironment(Owner.environment);}
            internal void AttachEnvironmentWriterMemory()
            {
                Require(Program.BlinkHostMemoryAttach(Owner.memoryToken)==0,"environment writer attaches actual shared memory owner");
                // This thread only fetches/compiles worker1 under Gate; it does
                // not execute that worker or claim its canonical native binding.
                try{Blink.BindHostGuestThreads(Owner,Program,Owner);}
                catch{Program.BlinkHostMemoryDetach();throw;}
            }
            internal void DetachEnvironmentWriterMemory()
            {
                try{Blink.UnbindHostGuestThreads();}
                finally{Require(Program.BlinkHostMemoryDetach()==0,"environment writer detaches shared memory owner");}
            }
        }
    }
    internal static class FullEnvironmentScaffoldTest
    {
        internal static bool ExitStrideCheckForEnvironment(this FullSharedScaffoldLayout layout)=>
            layout.ExitsOffset+((int)FullBlockExit.NeedEnvironment+1)*FullSharedScaffoldLayout.ExitStride<=layout.CodeLength;
    }
}
