using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static JitInstruction SystemStateClassify(byte[] bytes,ulong pc=Ip)
    {
        Check(FullSystemStateLowering.TryClassify(pc,bytes,out var i,out var reason),reason??"state classify");
        Check(FullBlockEmitter.CanInline(i)&&(i.Operation==JitOperation.Nop||FullBlockEmitter.ValidateSystemState(i)),"state native IR");return i;
    }
    static void CheckFullNativeSystemState(byte* machine)
    {
        CheckSystemStateLeaves(machine);
        using(var shared=CreateFullScaffold(out var scaffold))
        {fullOwnerScaffold=scaffold;try{CheckSystemStateLeaves(machine);}finally{fullOwnerScaffold=default;}}
        CheckSystemStateController();
        FullBlockInstruction[] steps=[new(SystemStateClassify([0xf3,0x48,0x0f,0xae,0xd1]))];
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool windows in new[]{false,true})Check(FullBlockEmitter.TryEmit(steps,staging,out int n,windows)&&n>0,"state bothABI encoding");
        for(int n=0;n<32;++n)FullBlockEmitter.Emit(steps,staging);
        long allocated=GC.GetAllocatedBytesForCurrentThread();for(int n=0;n<1000;++n)FullBlockEmitter.Emit(steps,staging);
        Check(allocated==GC.GetAllocatedBytesForCurrentThread(),"state warmed span allocation zero");ResetFull(machine);
    }
    static void CheckSystemStateLeaves(byte* machine)
    {
        var m=(Blink.Machine*)machine;
        foreach(bool gs in new[]{false,true})foreach(bool write in new[]{false,true})foreach(int width in new[]{32,64})
        {
            byte group=(byte)((write?2:0)+(gs?1:0));
            var i=SystemStateClassify(width==64?[0xf3,0x49,0x0f,0xae,(byte)(0xc1|group<<3)]:[0xf3,0x41,0x0f,0xae,(byte)(0xc1|group<<3)]);
            using var code=CreateFullBlock([new(i)]);ResetFull(machine);
            m->fs.@base=0xffff800012345678;m->gs.@base=0xffff800087654321;
            Write64(machine,24+9*8,0x12345678);uint flags=m->flags;
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            f.FsBase=m->fs.@base;f.GsBase=m->gs.@base;
            Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&m->flags==flags,"native base read/write nohostTLSchange/flags");
            if(write)Check((gs?m->gs.@base:m->fs.@base)==0x12345678&&(gs?f.GsBase:f.FsBase)==0x12345678,"base write updates guest and invocation cache");
            else Check(((ulong*)(machine+24))[9]==(width==32?(uint)(gs?m->gs.@base:m->fs.@base):(gs?m->gs.@base:m->fs.@base)),"base read width and highGPR extension");
        }
        foreach(byte[] bytes in new byte[][]{[0x0f,0xae,0xe8],[0x0f,0xae,0xf0],[0x0f,0xae,0xf8],
            [0x0f,0x18,0x03],[0x0f,0x18,0x4b,0x7f],[0x0f,0x0d,0x03]})
        {
            var i=SystemStateClassify(bytes);using var code=CreateFullBlock([new(i)]);ResetFull(machine);
            Write64(machine,48,0x0000800000000000);byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);f.MemoryTlb=(FullMemoryTlbEntry*)1;
            Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&completed==1&&
                new ReadOnlySpan<byte>(machine+12,before.Length-12).SequenceEqual(before.AsSpan(12)),"fences/prefetch do not evaluate hint data or change guest state");
        }
        var invalid=SystemStateClassify([0xf3,0x48,0x0f,0xae,0xd1]);
        using(var code=CreateFullBlock([new(invalid)]))
        {
            ResetFull(machine);Write64(machine,32,0x0000800000000000);m->fs.@base=0x1234;
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);f.FsBase=0x1234;
            Check(FloatRunProbe(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==13&&f.FaultCode==128&&
                m->fs.@base==0x1234&&f.FsBase==0x1234&&completed==0,"noncanonical base write GP before guest/frame updates");
        }
        CheckSystemStateMemory(machine);
    }
    static void CheckSystemStateMemory(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            foreach(bool load in new[]{false,true})foreach(uint value in new uint[]{0,0x1f80,0xffc0,0xffff,0x10000})
            {
                if(!load&&value>0xffff)continue;
                var i=SystemStateClassify([0x0f,0xae,load?(byte)0x13:(byte)0x1b]);using var code=CreateFullBlock([new(i)]);
                ResetFull(machine);Write64(machine,48,0x100fff);((Blink.Machine*)machine)->mxcsr=load?0x1f80u:value;
                new Span<byte>(data,8192).Fill(0xa5);AtomicWrite(data+4095,data+4096,1,0,32,value);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                Check(FloatRunProbe(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0,"MXCSR mapping precedes semantics");
                SetFullMemoryEntry(tlb,0x100000,data);SetFullMemoryEntry(tlb,0x101000,data+4096);FullNativeMemory.ResumeSameIssue(&f);
                bool fault=load&&value>0xffff;
                Check(FloatRunProbe(code,&f)==(fault?FullBlockExit.GuestFault:FullBlockExit.Continue)&&issued==1&&completed==(fault?0:1),"MXCSR same approved issue and reserved-bit GP");
                if(load)Check(((Blink.Machine*)machine)->mxcsr==(fault?0x1f80:value),"LDMXCSR literal control/sticky commit or reservedbit nochange");
                else Check(AtomicRead(data+4095,data+4096,1,0,32)==value,"STMXCSR exact little-endian split-store");
            }
            // Change FS then use it within the SAME native invocation; a stale
            // invocation base would miss the only installed data page.
            var write=SystemStateClassify([0xf3,0x48,0x0f,0xae,0xd1]);
            var read=new JitInstruction(Ip+5,3,JitOperation.LoadMov,8,new(0,0),new(0,0,IsMemory:true),SourceWidth:8,Address:new(2,-1,0,0,Segment:4));
            using var mixed=CreateFullBlock([new(write),new(read)]);ResetFull(machine);
            Write64(machine,32,0x100000);Write64(machine,40,32);data[32]=0xed;
            ulong global=11;long issue=0,done=0;int stopped=0;var header=FullHeader(Ip,mixed.Entry);
            var frame=FullFrame(machine,&header,&global,&issue,&done,&stopped,limit:2);frame.FsBase=0x200000;
            FullMemoryTlbEntry* entries=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(entries,64).Clear();frame.MemoryTlb=entries;frame.MemoryTlbMask=63;
            SetFullMemoryEntry(entries,0x100000,data,FullMemoryAccess.Read);
            Check(FloatRunProbe(mixed,&frame)==FullBlockExit.Continue&&done==2&&issue==2&&((Blink.Machine*)machine)->al==0xed,
                "WRFSBASE updates subsequent generated FS load before dispatch");
        }
        finally{NativeMemory.AlignedFree(data);}
    }
    static void CheckSystemStateController()
    {
        using var f=new ThreadedGuestExecution.FullControllerFixture();const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;
        f.Map(pc,8192);f.Fill(pc,8192);WriteControllerCode(f,pc,[0xf3,0x48,0x0f,0xae,0xd1,0x64,0x8a,0x02,0xeb,0xfe]);
        Write64((byte*)f.Machine(),32,pc+4096);Write64((byte*)f.Machine(),40,16);((byte*)f.Physical(pc+4096))[16]=0x6e;
        f.Counts(2);f.Step(pc);for(int n=0;n<12&&f.Completed<2;++n)f.Continue();
        Check(f.Completed==2&&f.Machine()->al==0x6e&&f.Machine()->fs.@base==pc+4096&&f.Statistics.SemanticHelperInstructions==0,
            "real controller base update survives return/MMU resume and stays generated");
    }
}
