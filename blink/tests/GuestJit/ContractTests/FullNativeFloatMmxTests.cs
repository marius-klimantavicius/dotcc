using System.Buffers.Binary;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static JitInstruction FloatMmxClassify(byte[] bytes)
    {
        Check(FullFloatMmxLowering.TryClassify(Ip,bytes,out var i,out var reason),reason??"MMX conversion classify");
        Check(FullBlockEmitter.ValidateFloat(i)&&(i.Options&FullBlockEmitter.FloatMmxConversions)!=0,
            "MMX conversion generated float class");return i;
    }

    static void CheckFullNativeFloatMmx(byte* machine)
    {
        CheckFloatMmxLeaves(machine);
        using(var common=CreateFullScaffold(out var shared))
        {fullOwnerScaffold=shared;try{CheckFloatMmxLeaves(machine);}finally{fullOwnerScaffold=default;}}
        CheckFloatMmxController();
        FullBlockInstruction[] steps=[new(FloatMmxClassify([0x0f,0x2a,0xc1]))];
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool windows in new[]{false,true})Check(FullBlockEmitter.TryEmit(steps,staging,out int n,windows)&&n>0,"MMX conversion bothABI encoding");
        for(int n=0;n<32;++n)FullBlockEmitter.Emit(steps,staging);
        long start=GC.GetAllocatedBytesForCurrentThread();
        for(int n=0;n<1000;++n)FullBlockEmitter.Emit(steps,staging);
        Check(GC.GetAllocatedBytesForCurrentThread()==start,"MMX conversion warm span zeroallocation");
        ResetFull(machine);
    }

    static FullBlockExit FloatMmxRun(byte* machine,JitInstruction i,uint status=0,bool fault=false)
    {
        using var code=CreateFullBlock([new(i)]);
        ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
        uint old=((Blink.Machine*)machine)->mxcsr;
        var result=FloatRunProbe(code,&f);
        Check(result==(fault?FullBlockExit.GuestFault:FullBlockExit.Continue)&&issued==1&&completed==(fault?0:1)&&
            (((Blink.Machine*)machine)->mxcsr&63)==((old&63)|status),"MMX conversion exact issue/status/commit");
        if(fault)Check(f.FaultTrap==19&&f.FaultAddress==i.Ip,"MMX conversion precise unmasked #XM");
        return result;
    }

    static void CheckFloatMmxLeaves(byte* machine)
    {
        foreach(bool pd in new[]{false,true})
        foreach(bool alias in new[]{false,true})
        foreach(int rounding in new[]{0,1,2,3})
        {
            byte[] bytes=pd?[0x66,0x0f,0x2a,alias?(byte)0xc0:(byte)0xc1]:[0x0f,0x2a,alias?(byte)0xc0:(byte)0xc1];
            var i=FloatMmxClassify(bytes);FloatSeed(machine,0x1f80u|((uint)rounding<<13));
            ((Blink.Machine*)machine)->fpu.cw=0x37f;
            FloatWrite(machine,0,1,64,0xabcdef0123456789);
            int source=i.Source.Register;
            FloatWrite(machine,source,0,32,0x01000001);FloatWrite(machine,source,1,32,unchecked((uint)-16777217));
            ulong upper=FloatRead(machine,0,1,64);
            FloatMmxRun(machine,i,pd?0u:32u);
            if(pd)
                Check(FloatRead(machine,0,0,64)==BitConverter.DoubleToUInt64Bits(16777217.0)&&
                    FloatRead(machine,0,1,64)==BitConverter.DoubleToUInt64Bits(-16777217.0),"PI2PD signed dwords exact two lanes");
            else
                Check(FloatRead(machine,0,0,32)==(rounding==2?0x4b800001u:0x4b800000u)&&
                    FloatRead(machine,0,1,32)==(rounding==1?0xcb800001u:0xcb800000u)&&
                    FloatRead(machine,0,1,64)==upper,"PI2PS allRC/two lanes/preserve upper64");
        }
        foreach(bool pd in new[]{false,true})
        foreach(bool truncate in new[]{false,true})
        foreach(int rounding in new[]{0,1,2,3})
        {
            byte op=truncate?(byte)0x2c:(byte)0x2d;
            var i=FloatMmxClassify(pd?[0x66,0x0f,op,0xc1]:[0x0f,op,0xc1]);
            FloatSeed(machine,0x1f80u|((uint)rounding<<13));((Blink.Machine*)machine)->fpu.cw=0x37f;
            FloatWrite(machine,0,1,64,0xabcdef0123456789);
            FloatWrite(machine,1,0,pd?64:32,pd?0x3ff8000000000000UL:0x3fc00000UL);
            FloatWrite(machine,1,1,pd?64:32,pd?0xbff8000000000000UL:0xbfc00000UL);
            FloatMmxRun(machine,i,32);
            int rc=truncate?3:rounding;int positive=rc is 0 or 2?2:1,negative=rc is 0 or 1?-2:-1;
            Check(FloatRead(machine,0,0,32)==(uint)positive&&FloatRead(machine,0,1,32)==unchecked((uint)negative)&&
                FloatRead(machine,0,1,64)==0xabcdef0123456789,"PS/PD2PI rounding/truncate and physical upper64 preservation");
            FloatSeed(machine);((Blink.Machine*)machine)->fpu.cw=0x37f;
            FloatWrite(machine,1,0,pd?64:32,pd?0x7ff0000000000001UL:0x7f800001UL);
            FloatWrite(machine,1,1,pd?64:32,pd?0x3ff0000000000000UL:0x3f800000UL);
            FloatMmxRun(machine,i,1);
            Check(FloatRead(machine,0,0,32)==0x80000000&&FloatRead(machine,0,1,32)==1,
                "MMX conversion masked invalid indefinite and valid second lane");
            FloatSeed(machine,0x1f00);((Blink.Machine*)machine)->fpu.cw=0x37f;
            FloatWrite(machine,0,0,64,0x123456789abcdef0);FloatWrite(machine,0,1,64,0xfedcba9876543210);
            FloatWrite(machine,1,0,pd?64:32,pd?0x7ff0000000000001UL:0x7f800001UL);
            FloatWrite(machine,1,1,pd?64:32,pd?0x3ff0000000000000UL:0x3f800000UL);
            FloatMmxRun(machine,i,1,true);
            Check(FloatRead(machine,0,0,64)==0x123456789abcdef0&&FloatRead(machine,0,1,64)==0xfedcba9876543210,
                "MMX conversion unmasked invalid withholds every destination lane");
        }
        foreach(byte[] encoding in new byte[][]{[0x0f,0x2a,0xc1],[0x66,0x0f,0x2a,0xc1],
            [0x0f,0x2c,0xc1],[0x66,0x0f,0x2d,0xc1]})
        {
            var i=FloatMmxClassify(encoding);using var code=CreateFullBlock([new(i)]);FloatSeed(machine);
            ((Blink.Machine*)machine)->fpu.cw=0x37b;((Blink.Machine*)machine)->fpu.sw=4;
            var before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);f.MemoryTlb=(FullMemoryTlbEntry*)1;
            Check(FloatRunProbe(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==16&&f.FaultCode==0&&f.FaultAddress==Ip&&
                issued==1&&completed==0&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                "MMX transition pending x87 #MF precedes operation/mapping with no effects");
        }
    }

    static void CheckFloatMmxController()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture();
        const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(pc);fixture.Fill(pc);WriteControllerCode(fixture,pc,[0x0f,0x2a,0xc1,0xeb,0xfe]);
        var m=fixture.Machine();m->fpu.sw=4;m->fpu.cw=0x37b;fixture.Counts(10);
        var fault=fixture.StepSynchronousFault(pc);
        Check(fault.Signal==8&&fault.Detail?.EndsWith("signal=8 code=3")==true&&m->trapno==16&&fixture.Completed==0&&m->ip==pc,
            "fresh producer delivers x87 zero-divide #MF before native MMX conversion");
    }
}
