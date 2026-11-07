using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static void CheckFullMaskedStoreControllerBoundaries()
    {
        CheckMaskedUnboundedTicket();CheckMaskedDeniedReady();
    }

    static void CheckMaskedUnboundedTicket()
    {
        foreach(bool xmm in new[]{false,true})foreach(bool alias in new[]{false,true})
        {
            using var f=new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base,other=pc+0x20000;
            if(alias)
            {
                var file=f.Io.CreateMemoryFile("masked-unbounded-alias",0);Check(file.Succeeded,"masked boundary shared backing");
                Check(f.Io.TruncateFile(file.Value,4096).Succeeded,"masked boundary shared extent");
                f.MapShared(file.Value,pc);f.Fill(pc);f.MapShared(file.Value,other);
            }
            else{f.Map(pc);f.Fill(pc);}
            byte[] bytes=MaskedEncoding(xmm);WriteControllerCode(f,pc,[..bytes,0x0f,0xa2,0xeb,0xfe]);
            if(alias)WarmLastExit(f,other+256);
            WarmLastExit(f,pc);var i=MaskedClassify(bytes);
            MaskedSeed((byte*)f.Machine(),i,(alias?other:pc)+128,0x81);
            bool observed=false,precise=false;Action<nint> unbounded=p=>((FullBlockFrame*)p)->NativeLimit=0;
            f.NativePrepared=unbounded;
            f.NativeReturned=p=>
            {
                var frame=(FullBlockFrame*)p;f.NativePrepared=unbounded;
                if(frame->MemoryFlags!=FullMemoryFlags.BarrierConsumed)return;
                observed=true;precise=frame->NativeLimit==0&&frame->Exit==FullBlockExit.Continue&&
                    frame->Executed==1&&frame->HelperIndex==FullBlockFrame.NoHelperIndex&&
                    f.Issued==1&&f.Completed==1&&f.Machine()->ip==pc+(uint)bytes.Length;
            };
            f.Counts(100);using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            Check(f.Step(pc),"unbounded masked ticket controller progress");
            for(int n=0;n<8&&f.Completed==0;++n)Check(f.Continue(),"unbounded masked ticket same issue");
            f.NativeReturned=null;f.NativePrepared=null;
            Check(observed&&precise&&f.Issued==1&&f.Completed==1&&f.Statistics.SemanticHelperInstructions==0&&
                *((byte*)f.Physical(pc)+128)==0x41&&*((byte*)f.Physical(pc)+135)==0x72,
                "unbounded real watched/physical-alias ticket returns before helper successor issue independently of cap or budget");
            LastExitClean(f);
        }
    }

    static void CheckMaskedDeniedReady()
    {
        using var f=new ThreadedGuestExecution.FullControllerFixture();
        const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base,data=pc+8192;
        f.Map(pc);f.Fill(pc);f.Map(data,8192);f.Fill(data,8192);
        byte[] bytes=MaskedEncoding(true);WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
        WarmLastExit(f,pc);var i=MaskedClassify(bytes);MaskedSeed((byte*)f.Machine(),i,data+4095,3);
        byte* first=(byte*)f.Physical(data),second=(byte*)f.Physical(data+4096);
        byte oldFirst=first[4095],oldSecond=second[0];int foreign=f.WorkerId(1);
        bool reserved=false,poisoned=false,readyCurrent=false,deniedWithoutEffects=false;
        Action<nint>? prepared=null;
        prepared=p=>
        {
            var frame=(FullBlockFrame*)p;frame->NativeLimit=1;
            if(poisoned||!FullNativeMemory.IsMaskedStoreRequest(frame))return;
            var d=(FullMaskedStoreDescriptor*)frame->StackMemory;
            if(d->State!=FullMaskedStoreState.Ready)return;
            readyCurrent=d->Epoch==frame->ObservedGlobalEpoch&&frame->ObservedGlobalEpoch==f.Jit.Authority.Epoch&&
                d->HostPage0==first&&d->HostPage1==second;
            // Inject at the last pre-enrollment boundary, then poison that
            // actual Ready. R205 correctly defers an already trusted source
            // while a foreign reservation persists; this same-thread fixture
            // must release it at the denied RET, not wait for a second Prepare.
            // NativeReturned still retains enrollment, so never drain there.
            reserved=f.Jit.Authority.TryReserveNativeTicket(foreign);
            Check(reserved,"foreign pending reservation after actual fresh Ready");
            d->HostPage0=(byte*)1;d->HostPage1=(byte*)1;poisoned=true;
        };
        f.NativePrepared=prepared;
        f.NativeReturned=p=>
        {
            var frame=(FullBlockFrame*)p;f.NativePrepared=prepared;
            if(poisoned && frame->Exit==FullBlockExit.Invalidated)
            {
                // The reservation advanced the polled epoch AFTER Ready. The
                // checked cap1 reset must clear every poisoned direct pointer
                // before its entry guard returns Invalidated. Release here;
                // R205 intentionally prevents a second entry while held.
                var d=frame->StackMemory;
                deniedWithoutEffects=frame->NativeLimit==1&&frame->MemoryTlb==null&&
                    d->Version==FullStackMemoryDescriptor.CurrentVersion&&d->Count==0&&
                    d->Request0.Host0==null&&d->Request0.Host1==null&&f.Issued==1&&f.Completed==0&&
                    first[4095]==oldFirst&&second[0]==oldSecond;
                f.Jit.Authority.AbortNativeReservation(foreign);
            }
        };
        try
        {
            f.Counts(100);using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            Check(f.Step(pc),"denied sparse Ready controller eventually progresses");
            for(int n=0;n<8&&f.Completed==0;++n)Check(f.Continue(),"denied sparse Ready keeps same approved issue");
            Check(reserved&&poisoned&&readyCurrent&&deniedWithoutEffects&&f.Issued==1&&f.Completed==1&&
                first[4095]==0x41&&second[0]==0x48&&f.Statistics.SemanticHelperInstructions==0,
                "fresh sparse Ready poison is cleared on failed enrollment before register-only cap1; retry remaps and commits once");
        }
        finally{f.NativeReturned=null;f.NativePrepared=null;f.Jit.Authority.AbortNativeReservation(foreign);}
        LastExitClean(f);
    }
}
