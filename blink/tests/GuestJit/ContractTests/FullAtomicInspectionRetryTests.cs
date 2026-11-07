using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using AtomicRetryFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void CheckAtomicInspectionRetry()
    {
        // Initial TryFill inspection=1, first ticket inspection=2, final
        // post-promotion reinspection=3. Both can independently return Retry.
        foreach(int width in new[]{8,16,32,64})
        foreach(int phase in new[]{2,3})
        foreach(bool stopAfterRetry in new[]{false,true})
        {
            using var f=new AtomicRetryFixture();using var deadline=f.AtomicDeadline();
            ulong pc=AtomicRetryFixture.Base,data=pc+8192;
            AtomicFamilySetup(f,pc,data);
            byte[] bytes=AtomicEncoding(width,[width==8?(byte)0x00:(byte)0x01,0x0b]);
            WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);AtomicFamilySeed(f,data+64);
            byte* address=(byte*)f.Physical(data)+64;
            new Span<byte>(address,8).Clear();*address=40;
            lock(f.Jit.Authority.Gate)
                Check(f.Jit.Authority.PrepareCodeSnapshot(f.Physical(data)), "c129 retry fixture retains ticket-only authority");
            uint beforeFlags=f.Machine()->flags;int inspections=0;bool injected=false;
            FullNativeMemory.TestAtomicTicketMapped=pointer=>
            {
                if(++inspections!=phase)return;
                var frame=(FullBlockFrame*)pointer;
                Check(f.Issued==1&&f.Completed==0&&*address==40&&f.Machine()->flags==beforeFlags&&
                    f.Machine()->ip==pc&&frame->MemoryTicket==0,
                    "atomic reinspection retry precedes all guest effects/ticket publication");
                f.Jit.Authority.BeginHostWriter();
                try{f.Jit.Authority.WaitNativeFrames();}
                finally{f.Jit.Authority.EndHostWriter();}
                if(stopAfterRetry)f.Stop.RequestStop();
                injected=true;
            };
            try
            {
                Check(f.Step(pc,signalActor:true),"atomic mapping retry returns ordinary controller outcome");
                Check(injected&&f.Issued==1&&!f.Jit.Authority.SemanticQuarantined,
                    "atomic mapping retry retains the same approved issue without internal failure");
                if(stopAfterRetry)
                    Check(f.Completed==0&&*address==40&&f.Machine()->flags==beforeFlags&&f.Machine()->ip==pc,
                        "atomic cancellation after read-only retry has no RMW/flags/register effects");
                else
                    Check(f.Completed==1&&*address==45&&f.Machine()->ip==pc+(uint)bytes.Length&&
                        f.Machine()->flags==AtomicExpectedFlags(FullAtomicOperation.Add,width,40,5,beforeFlags)&&
                        f.Statistics.NativeMemoryInstructions==1&&f.Statistics.SemanticHelperInstructions==0,
                        "atomic retry resolves fresh mappings/ticket and commits emitted RMW exactly once");
                AtomicFamilyAssertClean(f);
            }
            finally{FullNativeMemory.TestAtomicTicketMapped=null;}
        }
    }
}

namespace Managed.Emulation.Execution
{
    internal static unsafe partial class FullNativeMemory
    {
        [ThreadStatic] internal static Action<nint>? TestAtomicTicketMapped;
        static partial void AtomicTicketMapped(FullBlockFrame* frame)=>TestAtomicTicketMapped?.Invoke((nint)frame);
    }
}
