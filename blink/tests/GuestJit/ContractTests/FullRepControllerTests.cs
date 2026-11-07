using System.Buffers.Binary;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using RepFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE-AUTHORED / UNRUN. Actual production demand compiler/controller,
    // no fake REP executor and no dependency on proposed frame/state offsets.
    // Parent must integrate the native backend before invoking this group.
    static void CheckFullRepController()
    {
        CheckFullRepNormalController();
        CheckFullRepEarlyExit();
        CheckFullRepFaultFlags();
        CheckFullRepDataFaults();
        CheckFullRepSplitWriteFaults();
        CheckFullRepIdentity();
        CheckFullRepBytePermissions();
        CheckFullRepLazyDestination();
        CheckFullRepSelfModifyingAlias();
        CheckFullRepStop();
        CheckFullRepRejectedIssue();
    }

    static void RepSeed(RepFixture f,ulong si,ulong di,ulong ax,ulong cx,uint flags)
    {
        var m=f.Machine();byte* p=(byte*)m;
        m->oplen=0;m->attention=false;m->stashaddr=0;
        m->mode.omode=Blink.XED_MODE_LONG;m->mode.genmode=Blink.XED_GEN_MODE_PROTECTED;
        Write64(p,72,si);Write64(p,80,di);Write64(p,24,ax);Write64(p,32,cx);m->flags=flags;
    }

    static ulong RepRegister(RepFixture f,int offset)=>BinaryPrimitives.ReadUInt64LittleEndian(new((byte*)f.Machine()+offset,8));
    static void RepPutElement(byte* p,int width,ulong value)
    {for(int k=0;k<width/8;++k)p[k]=(byte)(value>>(8*k));}
    static void RepClean(RepFixture f)
    {
        var s=f.Jit.Store.GetStatistics();
        Check(s.ActiveManagedLeases==0&&s.ActiveNativeEpochs==0&&f.Statistics.ActiveDataBorrows==0&&
            f.Statistics.ActivePhysicalCodeBorrows==0&&f.Jit.Authority.PendingAtomicWorker==-1,
            "R100 all data/store/native/ticket capabilities released");
        Check(f.Statistics.SemanticHelperCalls==0&&f.Statistics.SemanticHelperInstructions==0&&
            f.Statistics.InterpreterAttempts==0,"R100 native REP has no managed semantic/helper/interpreter execution");
    }

    // Inactive-owner fixtures use their conservative TF/RF/AC-clear admission.
    // Active finite-profile REP flags are covered separately by R201.
    static void CheckFullRepNormalController()
    {
        foreach(byte opcode in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
        foreach(int width in new[]{8,16,32,64})
        foreach(bool as32 in new[]{false,true})
        foreach(bool df in new[]{false,true})
        foreach(int count in new[]{0,1,3,257})
        foreach(byte repeat in new byte[]{0xf3,0xf2})
        {
            bool compares=opcode is 0xa6 or 0xae;
            if(repeat==0xf2&&!compares)continue;
            using var f=new RepFixture();const ulong pc=RepFixture.Base,src=pc+0x10000,dst=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(src,8192);f.Fill(src,8192);f.Map(dst,8192);f.Fill(dst,8192);
            byte[] bytes=RepLoweringBytes(opcode,width,as32,0,repeat);WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
            int size=width/8,offset=df?4096+count*size:4095;
            ulong si=count==0?0x800000000000UL:src+(uint)offset,di=count==0?ulong.MaxValue:dst+(uint)offset;
            if(as32){si|=0xabcd123400000000UL;di|=0x8765432100000000UL;}
            ulong cx=(ulong)count|(as32?0x7766554400000000UL:0);
            ulong ax=compares?5UL:0xaabbccdd00000005UL;
            uint flags=(0xa52c0e15u&~0x50540u)|(df?1024u:0u)|(repeat==0xf2?64u:0u);
            // Incoming ZF deliberately disagrees with the continuation rule.
            // Every comparison continues, so the final successful compare is
            // 5-5 for REPE, or 5-2 for REPNE, with exact independent flags below.
            f.DataScope(0,()=>
            {
                for(int k=0;k<count;++k)
                {
                    int at=offset+(df?-k:k)*size;
                    // Split element writes through the real page resolver;
                    // host adjacency is never assumed for page-crossing data.
                    for(int t=0;t<size;++t)
                    {
                        f.Program.LookupAddress2(f.Machine(),(long)(src+(uint)(at+t)),0,0)[0]=(byte)(5UL>>(8*t));
                        f.Program.LookupAddress2(f.Machine(),(long)(dst+(uint)(at+t)),0,0)[0]=
                            (byte)((repeat==0xf2?2UL:5UL)>>(8*t));
                    }
                }
            });
            RepSeed(f,si,di,ax,cx,flags);f.Counts(1);
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            Check(f.Step(pc,signalActor:true)&&f.Issued==1&&f.Completed==1&&f.Machine()->ip==pc+(uint)bytes.Length&&
                f.Machine()->oplen==0,"R100 count0/1/many/quantum executes exactly one whole attempt");
            bool source=opcode is 0xa4 or 0xac or 0xa6,destination=opcode!=0xac;
            ulong delta=(ulong)(count*size),mask=as32?uint.MaxValue:ulong.MaxValue;
            ulong nextSi=count==0||!source?si:unchecked(si+(df?0UL-delta:delta))&mask;
            ulong nextDi=count==0||!destination?di:unchecked(di+(df?0UL-delta:delta))&mask;
            ulong nextAx=opcode==0xac&&count!=0?(width==64?5UL:width==32?5UL:
                (ax&~((1UL<<width)-1))|5UL):ax;
            uint nextFlags=!compares||count==0?flags:(flags&~0xff0008d1u)|(repeat==0xf3?64u:0x03000000u);
            Check(RepRegister(f,72)==nextSi&&RepRegister(f,80)==nextDi&&RepRegister(f,24)==nextAx&&
                RepRegister(f,32)==(count==0?cx:0)&&f.Machine()->flags==nextFlags,
                "R100 native count/index/partial accumulator/last flags and count0 untouched high halves");
            if(opcode is 0xa4 or 0xaa)
                f.DataScope(0,()=>
                {
                    for(int k=0;k<count;++k)
                    for(int t=0;t<size;++t)
                    {
                        int at=offset+(df?-k:k)*size+t;
                        byte actual=f.Program.LookupAddress2(f.Machine(),(long)(dst+(uint)at),0,0)[0];
                        byte expected=(byte)((opcode==0xa4?5UL:ax)>>(8*t));
                        Check(actual==expected,"R100 generated REP writes exact element bytes across independent pages");
                    }
                });
            RepClean(f);
        }
    }

    static void CheckFullRepFaultFlags()
    {
        foreach(byte opcode in new byte[]{0xa6,0xae})
        foreach(int width in new[]{8,16,32,64})
        foreach(byte repeat in new byte[]{0xf2,0xf3})
        foreach(bool sourceFault in new[]{false,true})
        {
            if(opcode==0xae&&sourceFault)continue;
            using var f=new RepFixture();const ulong pc=RepFixture.Base,src=pc+0x10000,dst=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(src,sourceFault?4096:8192);f.Fill(src,sourceFault?4096:8192);
            f.Map(dst,4096);f.Fill(dst);
            int size=width/8;ulong si=src+4096-(uint)(2*size),di=dst+4096-(uint)(2*size);
            byte[] bytes=RepLoweringBytes(opcode,width,false,0,repeat);WriteControllerCode(f,pc,bytes);
            f.DataScope(0,()=>
            {
                for(int k=0;k<2;++k)
                {
                    RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)(si+(uint)(k*size)),0,0),width,5);
                    RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)(di+(uint)(k*size)),0,0),width,repeat==0xf3?5UL:2UL);
                }
            });
            uint original=(0xa52c0e15u&~0x50540u)|(repeat==0xf2?64u:0u);
            RepSeed(f,si,di,5,3,original);f.Counts(1);
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var signal=f.StepSynchronousFault(pc);
            Check(signal.Signal==11&&f.Machine()->trapno==14&&f.Machine()->faultaddr==(long)(sourceFault?src+4096:dst+4096)&&
                f.Machine()->ip==pc&&f.Machine()->oplen==0&&f.Machine()->flags==original&&
                RepRegister(f,32)==1&&RepRegister(f,80)==di+(uint)(2*size)&&
                RepRegister(f,72)==si+(opcode==0xa6?(uint)(2*size):0)&&RepRegister(f,24)==5&&
                f.Issued==1&&f.Completed==0,
                "R100 REPE/REPNE CMPS/SCAS third-iteration fault restores original flags and retains exactly two iterations");
            RepClean(f);
        }
    }

    static void CheckFullRepEarlyExit()
    {
        foreach(byte opcode in new byte[]{0xa6,0xae})
        foreach(int width in new[]{8,16,32,64})
        foreach(byte repeat in new byte[]{0xf2,0xf3})
        foreach(bool df in new[]{false,true})
        foreach(bool as32 in new[]{false,true})
        {
            using var f=new RepFixture();const ulong pc=RepFixture.Base,src=pc+0x10000,dst=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(src);f.Fill(src);f.Map(dst);f.Fill(dst);
            int n=width/8,offset=df?2*n:4096-3*n;
            byte[] bytes=RepLoweringBytes(opcode,width,as32,0,repeat);WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
            f.DataScope(0,()=>
            {
                for(int k=0;k<3;++k)
                {
                    int at=offset+(df?-k:k)*n;
                    RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)src+at,0,0),width,5);
                    ulong other=(repeat==0xf3)==(k<2)?5UL:2UL;
                    RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)dst+at,0,0),width,other);
                }
            });
            ulong si=src+(uint)offset,di=dst+(uint)offset;
            if(as32){si|=0x1234567800000000UL;di|=0x8765432100000000UL;}
            uint flags=(0xa52c0e15u&~0x50540u)|(df?1024u:0u)|(repeat==0xf2?64u:0u);
            RepSeed(f,si,di,5,5,flags);f.Counts(1);
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            ulong mask=as32?uint.MaxValue:ulong.MaxValue,delta=(ulong)(3*n);
            Check(f.Step(pc,signalActor:true)&&f.Issued==1&&f.Completed==1&&RepRegister(f,32)==2&&
                RepRegister(f,80)==(unchecked(di+(df?0UL-delta:delta))&mask)&&
                RepRegister(f,72)==(opcode==0xae?si:unchecked(si+(df?0UL-delta:delta))&mask)&&
                f.Machine()->flags==((flags&~0xff0008d1u)|(repeat==0xf3?0x03000000u:64u))&&
                f.Machine()->ip==pc+(uint)bytes.Length,
                "R100 third comparison ends REPE/REPNE before unmapped fourth element, with last successful flags");
            RepClean(f);
        }
    }

    static void CheckFullRepSelfModifyingAlias()
    {
        using var f=new RepFixture();const ulong pc=RepFixture.Base,src=pc+0x10000,alias=pc+0x20000;
        var file=f.Io.CreateMemoryFile("r100-rep-own-opcode",0);Check(file.Succeeded,"R100 alias memfd");
        Check(f.Io.TruncateFile(file.Value,4096).Succeeded,"R100 alias file extent");
        f.MapShared(file.Value,pc);f.Fill(pc);f.MapShared(file.Value,alias);f.Map(src);f.Fill(src);
        WriteControllerCode(f,pc,[0xf3,0xa4,0xeb,0xfe]);
        f.DataScope(0,()=>new Span<byte>(f.Program.LookupAddress2(f.Machine(),(long)src,0,0),3).Fill(0x90));
        uint flags=f.Machine()->flags&~0x10500u;RepSeed(f,src,alias,0x1234,3,flags);f.Counts(1);
        using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
        Check(f.Step(pc,signalActor:true)&&f.Issued==1&&f.Completed==1&&RepRegister(f,32)==0&&
            RepRegister(f,72)==src+3&&RepRegister(f,80)==alias+3&&f.Machine()->ip==pc+2&&f.Machine()->flags==flags,
            "R100 self-modifying REP retains original issued opcode and length across three exclusive ticket iterations");
        f.DataScope(0,()=>
        {
            byte* p=f.Program.LookupAddress2(f.Machine(),(long)alias,0,0);
            Check(p[0]==0x90&&p[1]==0x90&&p[2]==0x90,"R100 all three latched MOVSB stores execute after original code changes");
        });
        RepClean(f);
    }

    static void CheckFullRepDataFaults()
    {
        foreach(byte opcode in new byte[]{0xa4,0xaa,0xac})
        foreach(int width in new[]{8,16,32,64})
        foreach(bool sourceFault in new[]{false,true})
        {
            if(opcode==0xaa&&sourceFault||opcode==0xac&&!sourceFault)continue;
            using var f=new RepFixture();const ulong pc=RepFixture.Base,src=pc+0x10000,dst=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(src,sourceFault?4096:8192);f.Fill(src,sourceFault?4096:8192);
            f.Map(dst,sourceFault?8192:4096);f.Fill(dst,sourceFault?8192:4096);
            int n=width/8;ulong si=src+4096-(uint)(2*n),di=dst+4096-(uint)(2*n);
            byte[] bytes=RepLoweringBytes(opcode,width,false,0,0xf3);WriteControllerCode(f,pc,bytes);
            f.DataScope(0,()=>
            {
                RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)si,0,0),width,1);
                RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)si+n,0,0),width,2);
            });
            ulong ax=0xaabbccdd00000005UL;uint flags=f.Machine()->flags&~0x10500u;
            RepSeed(f,si,di,ax,3,flags);f.Counts(1);
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var signal=f.StepSynchronousFault(pc);
            ulong expectedAx=opcode!=0xac?ax:width==64||width==32?2UL:(ax&~((1UL<<width)-1))|2UL;
            Check(signal.Signal==11&&f.Machine()->trapno==14&&f.Machine()->faultaddr==(long)(sourceFault?src+4096:dst+4096)&&
                f.Machine()->ip==pc&&f.Machine()->oplen==0&&f.Machine()->flags==flags&&RepRegister(f,32)==1&&
                RepRegister(f,72)==si+(opcode==0xaa?0u:(uint)(2*n))&&
                RepRegister(f,80)==di+(opcode==0xac?0u:(uint)(2*n))&&RepRegister(f,24)==expectedAx&&
                f.Issued==1&&f.Completed==0,"R100 MOVS/STOS/LODS retain two successes without faulting iteration effects");
            if(opcode!=0xac)f.DataScope(0,()=>
            {
                for(int k=0;k<2;++k)for(int t=0;t<n;++t)
                    Check(f.Program.LookupAddress2(f.Machine(),(long)di+k*n+t,0,0)[0]==
                        (byte)((opcode==0xaa?ax:(ulong)(k+1))>>(8*t)),"R100 earlier REP stores survive subsequent fault");
            });
            RepClean(f);
        }
    }

    static void CheckFullRepSplitWriteFaults()
    {
        foreach(byte opcode in new byte[]{0xa4,0xaa})
        foreach(int width in new[]{16,32,64})
        foreach(bool readonlyPage in new[]{false,true})
        {
            using var f=new RepFixture();const ulong pc=RepFixture.Base,src=pc+0x10000,dst=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(src);f.Fill(src);f.Map(dst,readonlyPage?8192:4096);f.Fill(dst,readonlyPage?8192:4096);
            int n=width/8;ulong di=dst+4095-(uint)n;
            byte[] bytes=RepLoweringBytes(opcode,width,false,0,0xf3);WriteControllerCode(f,pc,bytes);
            f.DataScope(0,()=>
            {
                RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)src+128,0,0),width,5);
                RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)src+128+n,0,0),width,5);
            });
            if(readonlyPage)Check(RepMakeReadOnly(f,dst+4096)==0,"R100 second destination page read-only");
            RepSeed(f,src+128,di,5,3,f.Machine()->flags&~0x10500u);f.Counts(1);
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var signal=f.StepSynchronousFault(pc);
            Check(signal.Signal==11&&f.Machine()->faultaddr==(long)(dst+4096)&&f.Machine()->segvcode==(readonlyPage?2:1)&&
                RepRegister(f,32)==2&&RepRegister(f,80)==dst+4095&&
                RepRegister(f,72)==src+128+(opcode==0xa4?(uint)n:0)&&f.Issued==1&&f.Completed==0,
                "R100 crossing iteration requires both destination page permissions before store");
            f.DataScope(0,()=>Check(f.Program.LookupAddress2(f.Machine(),(long)dst+4095,0,0)[0]==0xcc,
                "R100 faulting crossing element cannot overwrite its writable first fragment"));
            RepClean(f);
        }
    }

    static void CheckFullRepIdentity()
    {
        foreach(bool readOnly in new[]{false,true})
        {
            using var f=new RepFixture();const ulong pc=RepFixture.Base,data=pc+0x10000;
            f.Map(pc);f.Fill(pc);f.Map(data);f.Fill(data);WriteControllerCode(f,pc,[0xf3,0xa4]);
            if(readOnly)Check(RepMakeReadOnly(f,data)==0,"R100 identity MOVSB read-only mapping");
            RepSeed(f,data+128,data+128,5,3,f.Machine()->flags&~0x10500u);f.Counts(1);
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            if(readOnly)
            {
                var signal=f.StepSynchronousFault(pc);
                Check(signal.Signal==11&&f.Machine()->faultaddr==(long)data+128&&f.Machine()->segvcode==2&&
                    RepRegister(f,32)==3&&RepRegister(f,72)==data+128&&RepRegister(f,80)==data+128&&
                    f.Issued==1&&f.Completed==0,"R100 identity MOVSB still requires write permission");
            }
            else Check(f.Step(pc,signalActor:true)&&RepRegister(f,32)==0&&RepRegister(f,72)==data+131&&
                RepRegister(f,80)==data+131&&f.Issued==1&&f.Completed==1,"R100 identity MOVSB consumes all count/index work");
            RepClean(f);
        }
    }

    static void CheckFullRepLazyDestination()
    {
        using var f=new RepFixture();const ulong pc=RepFixture.Base,src=pc+0x10000,dst=pc+0x20000;
        f.Map(pc);f.Fill(pc);f.Map(src);f.Fill(src);f.Map(dst,8192); // deliberately uncommitted
        WriteControllerCode(f,pc,[0xf3,0x48,0xa5]);
        f.DataScope(0,()=>
        {for(int k=0;k<3;++k)RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)src+128+k*8,0,0),64,(ulong)(k+1));});
        RepSeed(f,src+128,dst+4095,5,3,f.Machine()->flags&~0x10500u);f.Counts(1);
        int preissues=0;f.Preissued=(_,_)=>++preissues;
        using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
        Check(f.Step(pc,signalActor:true)&&f.Issued==1&&f.Completed==1&&preissues==1&&RepRegister(f,32)==0&&
            RepRegister(f,72)==src+152&&RepRegister(f,80)==dst+4119,
            "R100 lazy cross-page assists preserve one exhausted-budget approved REP issue");
        f.DataScope(0,()=>
        {for(int k=0;k<3;++k)for(int t=0;t<8;++t)Check(
            f.Program.LookupAddress2(f.Machine(),(long)dst+4095+k*8+t,0,0)[0]==(byte)((ulong)(k+1)>>(8*t)),
            "R100 native values survive both-page demand mapping without managed copy semantics");});
        RepClean(f);
    }

    static int RepMakeReadOnly(RepFixture f, ulong page)
    {
        int result = -1;
        f.DataScope(0, () => result = f.Program.ProtectVirtual(f.Machine()->blink_host_system,
            checked((long)page), 4096, 1, false));
        return result;
    }

    static void CheckFullRepBytePermissions()
    {
        foreach(byte opcode in new byte[]{0xa4,0xaa,0xa6})
        {
            using var f=new RepFixture();const ulong pc=RepFixture.Base,src=pc+0x10000,dst=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(src);f.Fill(src);f.Map(dst);f.Fill(dst);
            WriteControllerCode(f,pc,[0xf3,opcode]);
            f.DataScope(0,()=>
            {
                new Span<byte>(f.Program.LookupAddress2(f.Machine(),(long)src+128,0,0),3).Fill(5);
                new Span<byte>(f.Program.LookupAddress2(f.Machine(),(long)dst+128,0,0),3).Fill(5);
            });
            Check(RepMakeReadOnly(f,dst)==0,"R100 byte destination page read-only");
            RepSeed(f,src+128,dst+128,7,3,f.Machine()->flags&~0x10500u);f.Counts(1);
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            if(opcode==0xa6)
                Check(f.Step(pc,signalActor:true)&&f.Issued==1&&f.Completed==1&&RepRegister(f,32)==0,
                    "R100 CMPS RR mapping never demands destination write permission");
            else
            {
                var signal=f.StepSynchronousFault(pc);
                Check(signal.Signal==11&&f.Machine()->faultaddr==(long)dst+128&&f.Machine()->segvcode==2&&
                    f.Issued==1&&f.Completed==0&&RepRegister(f,32)==3&&RepRegister(f,72)==src+128&&RepRegister(f,80)==dst+128,
                    "R94-S5 native REP MOVSB/STOSB enforce actual destination PAGE_RW before any byte write");
            }
            f.DataScope(0,()=>
            {
                byte* p=f.Program.LookupAddress2(f.Machine(),(long)dst+128,0,0);
                Check(p[0]==5&&p[1]==5&&p[2]==5,"R100 read-only destination bytes remain unchanged");
            });
            RepClean(f);
        }
    }

    static void CheckFullRepStop()
    {
        using var f=new RepFixture();const ulong pc=RepFixture.Base,src=pc+0x10000,dst=pc+0x20000;
        f.Map(pc);f.Fill(pc);f.Map(src);f.Fill(src);f.Map(dst);f.Fill(dst);
        WriteControllerCode(f,pc,[0xf3,0xa4,0xeb,0xfe]);
        RepSeed(f,src,dst,5,1024,f.Machine()->flags&~0x10500u);f.Counts(1);bool interrupted=false;
        Action<nint>? diagnostic=null;
        diagnostic=p=>
        {
            var state=((FullBlockFrame*)p)->Rep;
            Check(state!=null,"R100 diagnostic has scope-owned REP state");state->QuantumLimit=1;
            f.NativePrepared=diagnostic;
        };
        f.NativePrepared=diagnostic;
        // Only request stop here. This existing hook is before ExitNative;
        // mutation, mapping work or reader-drain waits in it would deadlock.
        f.NativeReturned=_=>
        {if(RepRegister(f,32) is >0 and <1024){interrupted=true;f.Stop.RequestStop();}};
        using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
        try
        {
            Check(f.Step(pc,signalActor:true)&&interrupted&&f.Issued==1&&f.Completed==0&&f.Machine()->ip==pc&&
                RepRegister(f,32) is >0 and <1024&&RepRegister(f,72)-src==1024-RepRegister(f,32)&&
                RepRegister(f,80)-dst==1024-RepRegister(f,32),"R100 partial REP stop remains issued/uncompleted and preserves progress");
            RepClean(f);
        }
        finally{f.NativePrepared=null;f.NativeReturned=null;}
    }

    static void CheckFullRepRejectedIssue()
    {
        using var f=new RepFixture();const ulong pc=RepFixture.Base;
        f.Map(pc);f.Fill(pc);WriteControllerCode(f,pc,[0xf3,0xa4]);
        RepSeed(f,0x800000000000UL,ulong.MaxValue,0x1234,17,f.Machine()->flags&~0x10500u);f.Counts(0);
        Check(!f.Step(pc)&&f.Issued==1&&f.Completed==0&&RepRegister(f,32)==17&&RepRegister(f,24)==0x1234&&
            f.Machine()->ip==pc,"R100 exhausted initial attempt counts rejected issue without EA/data/register effects");
        RepClean(f);
    }
}
