using Managed.Emulation.Execution;
using RepPauseFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        private Action<nint,FullRepReason>? testRepPaused;
        internal Action<nint,FullRepReason>? RepPausedForTests { set=>testRepPaused=value; }
        partial void FullControllerRepPaused(FullRepState* state,FullRepReason reason)
            =>testRepPaused?.Invoke((nint)state,reason);
    }
}

unsafe partial class Program
{
    static void CheckFullRepAttemptIdentities()
    {
        using var f=new RepPauseFixture();const ulong pc=RepPauseFixture.Base,data=pc+0x10000;
        f.Map(pc);f.Fill(pc);f.Map(data);f.Fill(data);WriteControllerCode(f,pc,[0xf3,0xaa,0xf3,0xaa,0xeb,0xfe]);
        RepSeed(f,0x800000000000UL,data+128,7,1,f.Machine()->flags&~0x10500u);f.Counts(2);
        var attempts=new List<ulong>(2);
        f.Owner.RepPausedForTests=(address,reason)=>
        {if(reason==FullRepReason.Latched)attempts.Add(((FullRepState*)address)->AttemptGeneration);};
        using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
        try
        {
            Check(f.Step(pc,signalActor:true)&&f.Completed==1,"R100 first scope-local REP attempt");
            Write64((byte*)f.Machine(),32,1);
            Check(f.Continue(signalActor:true)&&f.Issued==2&&f.Completed==2&&f.Machine()->ip==pc+4&&
                RepRegister(f,80)==data+130&&attempts.Count==2&&attempts[0]!=0&&attempts[1]>attempts[0],
                "R100 same-worker separate synchronous invocations seed/commit monotonic attempt identities, never reset1");
            RepClean(f);
        }
        finally{f.Owner.RepPausedForTests=null;}
    }

    // SOURCE-AUTHORED / UNRUN. Requires the parent's post-cleanup seam above;
    // doing these mutations in the old NativeReturned hook would be incorrect.
    static void CheckFullRepManagedPauseChanges()
    {
        foreach(bool changeCode in new[]{false,true})
        {
            using var f=new RepPauseFixture();const ulong pc=RepPauseFixture.Base,src=pc+0x10000,dst=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(src,8192);f.Fill(src,8192);f.Map(dst);f.Fill(dst);
            WriteControllerCode(f,pc,[0xf3,0xa4,0xeb,0xfe]);
            f.DataScope(0,()=>
            {
                f.Program.LookupAddress2(f.Machine(),(long)src+4095,0,0)[0]=0x11;
                byte* p=f.Program.LookupAddress2(f.Machine(),(long)src+4096,0,0);p[0]=0x22;p[1]=0x33;
            });
            RepSeed(f,src+4095,dst+64,0xee,3,f.Machine()->flags&~0x10500u);f.Counts(1);
            bool changed=false;
            f.Owner.RepPausedForTests=(address,reason)=>
            {
                var state=(FullRepState*)address;
                if(changed||reason!=FullRepReason.NeedMemory||state->CompletedIterations!=1)return;
                var resources=f.Jit.Store.GetStatistics();
                Check(resources.ActiveNativeEpochs==0&&resources.ActiveManagedLeases==0&&
                    f.Statistics.ActiveDataBorrows==0&&f.Jit.Authority.PendingAtomicWorker==-1,
                    "R100 assist mutation begins only after complete native/ticket/store/passive cleanup");
                Check(state->AttemptGeneration!=0&&state->RequestGeneration==2&&RepRegister(f,32)==2&&
                    RepRegister(f,72)==src+4096&&RepRegister(f,80)==dst+65&&f.Issued==1&&f.Completed==0,
                    "R100 exact partial issued continuation before mapping-only managed assist");
                changed=true;
                if(changeCode)WriteControllerCode(f,pc,[0xf3,0xaa,0xeb,0xfe]); // different opcode: STOSB AL=ee
                else
                {
                    Check(f.Mutate("MAP_FIXED",src+4096)==0,"R100 replace second source page during assist");f.Fill(src+4096);
                    f.DataScope(0,()=>
                    {byte* p=f.Program.LookupAddress2(f.Machine(),(long)src+4096,0,0);p[0]=0x44;p[1]=0x55;});
                }
            };
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            try
            {
                Check(f.Step(pc,signalActor:true)&&changed&&f.Issued==1&&f.Completed==1&&RepRegister(f,32)==0&&
                    RepRegister(f,72)==src+4098&&RepRegister(f,80)==dst+67&&f.Machine()->ip==pc+2,
                    "R100 managed code/remap assistance completes original latched issue without re-decode/re-issue");
                f.DataScope(0,()=>
                {
                    byte* p=f.Program.LookupAddress2(f.Machine(),(long)dst+64,0,0);
                    Check(p[0]==0x11&&p[1]==(changeCode?0x22:0x44)&&p[2]==(changeCode?0x33:0x55),
                        "R100 changed opcode is not executed mid-REP; changed mapping IS re-resolved before next native load");
                });
                RepClean(f);
            }
            finally{f.Owner.RepPausedForTests=null;}
        }
    }
}
