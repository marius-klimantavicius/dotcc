using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static JitInstruction FloatExtendedClassify(byte[] bytes)
    {
        Check(FullFloatExtendedLowering.TryClassify(Ip,bytes,out var i,out var reason),reason??"extended float classify");
        Check(FullBlockEmitter.ValidateFloatExtended(i),"extended float canonical IR");return i;
    }

    static void CheckFullNativeFloatExtended(byte* machine)
    {
        CheckFloatExtendedLeaves(machine);
        CheckFloatExtendedMemory(machine);
        using(var common=CreateFullScaffold(out var shared))
        {fullOwnerScaffold=shared;try{CheckFloatExtendedLeaves(machine);CheckFloatExtendedMemory(machine);}finally{fullOwnerScaffold=default;}}
        FullBlockInstruction[] steps=[new(FloatExtendedClassify([0xf2,0x0f,0x7c,0xc1]))];
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool windows in new[]{false,true})Check(FullBlockEmitter.TryEmit(steps,staging,out int n,windows)&&n>0,"extended float bothABI encoding");
        for(int n=0;n<32;++n)FullBlockEmitter.Emit(steps,staging);
        long start=GC.GetAllocatedBytesForCurrentThread();
        for(int n=0;n<1000;++n)FullBlockEmitter.Emit(steps,staging);
        Check(GC.GetAllocatedBytesForCurrentThread()==start,"extended float warm span zeroallocation");
        ResetFull(machine);
    }

    static void CheckFloatExtendedLeaves(byte* machine)
    {
        foreach(bool pd in new[]{false,true})
        foreach(byte opcode in new byte[]{0x7c,0x7d,0xd0})
        foreach(bool alias in new[]{false,true})
        {
            var i=FloatExtendedClassify([pd?(byte)0x66:(byte)0xf2,0x0f,opcode,alias?(byte)0xc0:(byte)0xc1]);
            FloatSeed(machine);int width=pd?64:32,lanes=pd?2:4;
            for(int lane=0;lane<lanes;++lane)
            {
                FloatWrite(machine,0,lane,width,pd?BitConverter.DoubleToUInt64Bits(lane+1.0):BitConverter.SingleToUInt32Bits(lane+1f));
                FloatWrite(machine,1,lane,width,pd?BitConverter.DoubleToUInt64Bits((lane+1)*10.0):BitConverter.SingleToUInt32Bits((lane+1)*10f));
            }
            using var code=CreateFullBlock([new(i)]);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1,
                "native SSE3 arithmetic oneissue/commit, restored hostenv");
            for(int lane=0;lane<lanes;++lane)
            {
                double a,b;
                if(opcode==0xd0){a=lane+1;b=alias?a:(lane+1)*10;}
                else
                {
                    int at=lane%(lanes/2)*2;double scale=lane<lanes/2||alias?1:10;
                    a=(at+1)*scale;b=(at+2)*scale;
                }
                double result=opcode==0x7d||opcode==0xd0&&(lane&1)==0?a-b:a+b;
                ulong expected=pd?BitConverter.DoubleToUInt64Bits(result):BitConverter.SingleToUInt32Bits((float)result);
                Check(FloatRead(machine,0,lane,width)==expected,"horizontal/alternating laneorder andalias independent exactmath");
            }
            Check(((Blink.Machine*)machine)->mxcsr==0x1f80,"exact SSE3 integer inputs have no sticky updates");
        }
        foreach(bool scalar in new[]{false,true})
        foreach(byte opcode in new byte[]{0x52,0x53})
        foreach(uint guest in new uint[]{0,0x1f80,0xffc0,0x1fbf})
        {
            var i=FloatExtendedClassify(scalar?[0xf3,0x0f,opcode,0xc1]:[0x0f,opcode,0xc1]);
            FloatSeed(machine,guest);byte[] before=new ReadOnlySpan<byte>(machine+160,16).ToArray();
            float[] values=[1,4,16,256];
            for(int lane=0;lane<4;++lane)FloatWrite(machine,1,lane,32,BitConverter.SingleToUInt32Bits(values[lane]));
            using var code=CreateFullBlock([new(i)]);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&((Blink.Machine*)machine)->mxcsr==guest,
                "RCP/RSQRT no guestFP exceptions/sticky even when masks clear, exacthostenvironment");
            for(int lane=0;lane<(scalar?1:4);++lane)
            {
                double expected=opcode==0x53?1.0/values[lane]:1.0/Math.Sqrt(values[lane]);
                double actual=BitConverter.UInt32BitsToSingle((uint)FloatRead(machine,0,lane,32));
                Check(Math.Abs(actual/expected-1)<=1.5/4096,"architectural reciprocal approximation bound");
            }
            if(scalar)Check(new ReadOnlySpan<byte>(machine+164,12).SequenceEqual(before.AsSpan(4)),"estimate scalar upper96 preserved");
        }
        foreach(byte opcode in new byte[]{0x52,0x53})
        foreach(uint input in new uint[]{0,0x80000000,0x7f800000,1,0x7f800001,0x7fc12345})
        {
            var i=FloatExtendedClassify([0xf3,0x0f,opcode,0xc1]);FloatSeed(machine,0);
            FloatWrite(machine,1,0,32,input);using var code=CreateFullBlock([new(i)]);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&((Blink.Machine*)machine)->mxcsr==0,
                "estimate zero/subnormal/NaN/Inf produces no numeric guestexception");
            uint result=(uint)FloatRead(machine,0,0,32);
            if(input==0||input==1)Check(result==0x7f800000,"estimate positivezero/denormal treated aszero");
            else if(input==0x80000000)Check(result==0xff800000,"estimate signedzero infinity sign");
            else if(input==0x7f800000)Check(result==0,"estimate positiveInf iszero");
            else Check((result&0x7fc00000)==0x7fc00000,"estimate quiets NaN without stickyIE");
        }
        // A high-lane invalid must withhold the entire packed result.
        var packed=FloatExtendedClassify([0xf2,0x0f,0x7c,0xc1]);FloatSeed(machine,0x1f00);
        for(int n=0;n<4;++n){FloatWrite(machine,0,n,32,0x3f800000);FloatWrite(machine,1,n,32,0x3f800000);}
        FloatWrite(machine,1,2,32,0x7f800001);var old=new ReadOnlySpan<byte>(machine+160,16).ToArray();
        using(var code=CreateFullBlock([new(packed)]))
        {
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            Check(FloatRunProbe(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==19&&completed==0&&issued==1&&
                new ReadOnlySpan<byte>(machine+160,16).SequenceEqual(old)&&(((Blink.Machine*)machine)->mxcsr&1)!=0,
                "horizontal SIMD unmasked lateinvalid has sticky and nopartialdestination");
        }
    }

    static void CheckFloatExtendedMemory(byte* machine)
    {
        byte* data=(byte*)System.Runtime.InteropServices.NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            var i=FloatExtendedClassify([0xf3,0x0f,0x53,0x03]);using var code=CreateFullBlock([new(i)]);
            FloatSeed(machine);Write64(machine,48,0x100fff);AtomicWrite(data+4095,data+4096,1,0,32,0x3f800000);
            byte[] old=new ReadOnlySpan<byte>(machine+160,16).ToArray();
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            f.MemoryTlb=tlb;f.MemoryTlbMask=63;
            Check(FloatRunProbe(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&
                new ReadOnlySpan<byte>(machine+160,16).SequenceEqual(old),"estimate split mapping misses before destination/FP effects");
            SetFullMemoryEntry(tlb,0x100000,data,FullMemoryAccess.Read);SetFullMemoryEntry(tlb,0x101000,data+4096,FullMemoryAccess.Read);
            FullNativeMemory.ResumeSameIssue(&f);
            Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                Math.Abs(BitConverter.UInt32BitsToSingle((uint)FloatRead(machine,0,0,32))-1.0)<1.5/4096&&
                new ReadOnlySpan<byte>(machine+164,12).SequenceEqual(old.AsSpan(4)),"estimate native scalar split-read sameissue/upper96");
            var packed=FloatExtendedClassify([0xf2,0x0f,0x7c,0x03]);using var packedCode=CreateFullBlock([new(packed)]);
            FloatSeed(machine);Write64(machine,48,0x100001);old=new ReadOnlySpan<byte>(machine+160,16).ToArray();
            issued=completed=0;h=FullHeader(Ip,packedCode.Entry);f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
            Check(FloatRunProbe(packedCode,&f)==FullBlockExit.GuestFault&&f.FaultTrap==13&&f.FaultCode==128&&
                issued==1&&completed==0&&new ReadOnlySpan<byte>(machine+160,16).SequenceEqual(old),
                "legacy horizontal alignment #GP precedes poisoned page lookup and FP/destination effects");
        }
        finally{System.Runtime.InteropServices.NativeMemory.AlignedFree(data);}
    }
}
