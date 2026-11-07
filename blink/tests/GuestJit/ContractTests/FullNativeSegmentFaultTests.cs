using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static JitInstruction SegmentFaultClassify(byte[] bytes)
    {
        bool ok=FullScalarMiscLowering.TryClassify(Ip,bytes,out var i,out _) ||
            FullBitCountLowering.TryClassify(Ip,bytes,out i,out _) ||
            FullFloatMmxLowering.TryClassify(Ip,bytes,out i,out _);
        Check(ok&&FullBlockEmitter.CanInline(i),"new native SS/DS form admission");return i;
    }

    static void CheckFullNativeSegmentFaults(byte* machine)
    {
        CheckNativeSegmentFaultLeaves(machine);
        using(var common=CreateFullScaffold(out var shared))
        {fullOwnerScaffold=shared;try{CheckNativeSegmentFaultLeaves(machine);}finally{fullOwnerScaffold=default;}}
        foreach(byte[] bytes in new byte[][]{[0x36,0xd7],[0x8c,0x0c,0x24],[0xf3,0x48,0x0f,0xb8,0x0c,0x24],
            [0x36,0x0f,0x2a,0x03]})
        {
            using var f=new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;f.Map(pc);f.Fill(pc);
            WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
            var i=SegmentFaultClassify(bytes);var m=f.Machine();m->al=0;m->fpu.sw=0;m->fpu.cw=0x37f;
            Write64((byte*)m,24+i.Address.Base*8,0x0000800000000000);f.Counts(10);
            var fault=f.StepSynchronousFault(pc);
            Check(fault.Signal==7&&fault.Detail?.EndsWith("signal=7 code=128")==true&&m->trapno==12&&
                m->ip==pc&&f.Completed==0,"fresh producer real controller SS12/SIGBUS/SI_KERNEL before effects");
        }
        ResetFull(machine);
    }

    static void CheckNativeSegmentFaultLeaves(byte* machine)
    {
        foreach(byte[] bytes in new byte[][]{[0x36,0xd7],[0x8c,0x0c,0x24],[0x8c,0x4d,0],
            [0x41,0x8c,0x4d,0],[0x41,0x8c,0x0c,0x24],[0x3e,0x8c,0x0c,0x24],
            [0xf3,0x48,0x0f,0xb8,0x0c,0x24],[0x36,0xf3,0x48,0x0f,0xbc,0x03],
            [0x36,0x0f,0x2a,0x03],[0x0f,0x2a,0x04,0x24],[0x3e,0x0f,0x2a,0x04,0x24]})
        {
            var i=SegmentFaultClassify(bytes);using var code=CreateFullBlock([new(i)]);
            foreach(ulong address in new[]{0x0000800000000000UL,0xffff7fffffffffffUL})
            {
                ResetFull(machine);var m=(Blink.Machine*)machine;m->fpu.sw=0;m->fpu.cw=0x37f;
                Write64(machine,24,0x123400);Write64(machine,24+i.Address.Base*8,address);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
                uint trap=FullScalarMiscLowering.UsesStackSegment(i.Address)?12u:13u;
                Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==trap&&f.FaultCode==128&&f.FaultAddress==0&&
                    issued==1&&completed==0&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                    "explicit/default SS vs overriding DS selects exact vector before poisoned mapping");
            }
        }
        byte* page=(byte*)NativeMemory.AlignedAlloc(4096,4096);
        try
        {
            ResetFull(machine);Write64(machine,24,0x123400);Write64(machine,48,0x100010);page[16]=0x7b;
            ScalarMiscRun(machine,SegmentFaultClassify([0x36,0xd7]),page);
            Check(((Blink.Machine*)machine)->al==0x7b,"canonical explicit SS is flat and executes normal checked load");
        }
        finally{NativeMemory.AlignedFree(page);}
    }
}
