using System.Numerics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static JitInstruction FlagStackClassify(byte[] bytes, ulong ip = Ip)
    {
        Check(FullFlagStackLowering.TryClassify(ip, bytes, out var i, out var reason), reason ?? "flag stack decode");
        Check(FullBlockEmitter.CanInline(i) && FullBlockEmitter.ValidateFlagStack(i), "native flag stack IR");
        return i;
    }

    static uint ExpectedPushedFlags(uint flags) =>
        (flags & 0x003c7fd1u) | 2u | ((BitOperations.PopCount(flags >> 24) & 1) == 0 ? 4u : 0u);

    static uint ExpectedPoppedFlags(uint before, ulong value, int width)
    {
        uint mask = width == 16 ? 0x4dd5u : 0x244dd5u;
        if ((before & 0x3000) == 0x3000) mask |= 0x200;
        return (before & ~(mask | 0xff010000u)) | ((uint)value & mask) |
            (((uint)value & 4) == 0 ? 0x01000000u : 0);
    }

    static void CheckFullNativeFlagStack(byte* machine)
    {
        CheckFlagStackLeaves(machine);
        using (var shared = CreateFullScaffold(out var scaffold))
        {
            fullOwnerScaffold = scaffold;
            try { CheckFlagStackLeaves(machine); }
            finally { fullOwnerScaffold = default; }
        }
        CheckFlagStackLinkedLeaves(machine);
        using (var owner = CreateFullScaffold(out var linkedScaffold))
        { fullOwnerScaffold = linkedScaffold; try { CheckFlagStackLinkedLeaves(machine); } finally { fullOwnerScaffold = default; } }
        foreach (byte[] bytes in new byte[][] { [0xf0,0x9c], [0xf3,0x9d], [0x9c,0x90], [] })
            Check(!FullFlagStackLowering.TryClassify(Ip, bytes, out _, out _), "flag stack rejected form remains explicit");
        foreach (byte[] bytes in new byte[][] { [0x9c], [0x66,0x9c], [0x67,0x64,0x9c], [0x66,0x48,0x9d], [0x48,0x66,0x9d] })
        {
            var i = FlagStackClassify(bytes);
            Check(i.Width == (bytes.AsSpan().SequenceEqual(new byte[]{0x66,0x9c}) ||
                bytes.AsSpan().SequenceEqual(new byte[]{0x48,0x66,0x9d}) ? 16 : 64), "flag stack effective operand prefix");
            FullBlockInstruction[] block = [new(i)]; byte[] scratch = new byte[FullBlockEmitter.CodeCapacity];
            foreach (bool win in new[] { false, true })
                Check(FullBlockEmitter.TryEmit(block, scratch, out int length, win) && length > 0, "flag stack both ABI emission");
            for (int n = 0; n < 32; ++n) FullBlockEmitter.Emit(block, scratch);
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            for (int n = 0; n < 500; ++n) FullBlockEmitter.Emit(block, scratch);
            Check(allocated == GC.GetAllocatedBytesForCurrentThread(), "flag stack warm emission zero allocation");
        }
        CheckFlagStackOracle(); CheckFlagStackController(); CheckFlagStackLinkedController(); ResetFull(machine);
    }

    static void CheckFlagStackLeaves(byte* machine)
    {
        byte* data = (byte*)NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach (int width in new[] {16,64}) foreach (bool push in new[] {true,false})
            {
                var i = FlagStackClassify(width == 16 ? [0x66,push?(byte)0x9c:(byte)0x9d] : [push?(byte)0x9c:(byte)0x9d]);
                using var code = CreateFullBlock([new(i)]);
                for (int parity = 0; parity < 256; ++parity) foreach (uint iopl in new uint[] {0,0x1000,0x2000,0x3000})
                {
                    ResetFull(machine);
                    uint flags = (uint)parity << 24 | iopl | 0x00284ed5u; // no initial TF/RF/AC
                    ((Blink.Machine*)machine)->flags = flags;
                    ulong rsp = 0x101001; Write64(machine,56,push ? rsp + (uint)(width/8) : rsp);
                    ulong value = 0xfffffffffffffffful ^ ((ulong)(parity & 1) << 2);
                    new Span<byte>(data,8192).Fill(0x5a);
                    AtomicWrite(data+4097,data+8192,width/8,0,width,value);
                    ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                    var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                    new Span<FullMemoryTlbEntry>(tlb,64).Clear();f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                    SetFullMemoryEntry(tlb,0x100000,data);SetFullMemoryEntry(tlb,0x101000,data+4096);
                    Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1,"native flags one approved issue/commit");
                    if(push)
                        Check(((Blink.Machine*)machine)->flags==flags&&AtomicRead(data+4097,data+8192,width/8,0,width)==
                            (ExpectedPushedFlags(flags)&(width==16?65535ul:ulong.MaxValue)),"PUSHF architectural image and lazy parity without flag mutation");
                    else Check(((Blink.Machine*)machine)->flags==ExpectedPoppedFlags(flags,value,width),"POPF width/CPL3/IOPL/lazy parity/control flags");
                    Check(((ulong*)(machine+24))[4]==(push?rsp:rsp+(uint)(width/8)),"flags stack exact RSP commit");
                }
                // Force a real two-page miss. No stack, flag, or byte effect
                // may occur before the destination second-page write permit.
                ResetFull(machine);new Span<byte>(data,8192).Fill(0xa5);
                ulong start=0x100fff;Write64(machine,56,push?start+(uint)(width/8):start);
                uint oldFlags=((Blink.Machine*)machine)->flags;
                ulong global=11;long issue=0,done=0;int stopped=0;var header=FullHeader(Ip,code.Entry);
                var frame=FullFrame(machine,&header,&global,&issue,&done,&stopped,limit:1);
                FullMemoryTlbEntry* entries=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(entries,64).Clear();
                frame.MemoryTlb=entries;frame.MemoryTlbMask=63;SetFullMemoryEntry(entries,0x100000,data);
                Check(FloatRunProbe(code,&frame)==FullBlockExit.NeedMemory&&done==0&&issue==1&&
                    ((Blink.Machine*)machine)->flags==oldFlags&&((ulong*)(machine+24))[4]==(push?start+(uint)(width/8):start)&&
                    data[4095]==0xa5&&data[4096]==0xa5,"flags stack crosspage no partial effects");
                SetFullMemoryEntry(entries,0x101000,data+4096);FullNativeMemory.ResumeSameIssue(&frame);
                Check(FloatRunProbe(code,&frame)==FullBlockExit.Continue&&done==1&&issue==1,"flags stack exact same-issue mapping resume");
                ResetFull(machine);Write64(machine,56,0x0000800000000000ul+(push?(uint)(width/8):0));
                frame=FullFrame(machine,&header,&global,&issue,&done,&stopped,limit:1);frame.MemoryTlb=(FullMemoryTlbEntry*)1;
                Check(FloatRunProbe(code,&frame)==FullBlockExit.GuestFault&&frame.FaultTrap==12&&frame.FaultCode==128,"flags stack SS before MMU dereference");
            }
            foreach(uint changed in new uint[]{0x100,0x40000})
            {
                var pop=FlagStackClassify([0x9d]);var next=new JitInstruction(Ip+1,1,JitOperation.Nop,64,default,default);
                using var code=CreateFullBlock([new(pop),new(next)]);ResetFull(machine);Write64(machine,56,0x100000);
                *(ulong*)data=changed;
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:0);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                f.MemoryTlb=tlb;f.MemoryTlbMask=63;SetFullMemoryEntry(tlb,0x100000,data);
                Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.HelperIndex==FullBlockFrame.NoHelperIndex&&
                    ((Blink.Machine*)machine)->ip==Ip+1,"POPF TF/AC context change stops before another guest issue");
            }
        }
        finally{NativeMemory.AlignedFree(data);}
    }

    static void CheckFlagStackOracle()
    {
        // The selected producer deliberately exports IOPL=3 and bit3=1 on
        // PUSHF, and permits IF changes on POPF even with stored IOPL<3.
        // Compare only the matching POPF domain; independent tests cover the
        // architectural corrections rather than asserting false equivalence.
        using var fixture=new ThreadedGuestExecution.FullControllerFixture(interpreterOracle:true);
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(guest,8192);fixture.Fill(guest,8192);
        foreach(int width in new[]{16,64})foreach(ulong value in new[]{0ul,0x204ed5ul,ulong.MaxValue & ~0x40100ul})
        {
            byte[] bytes=width==16?[0x66,0x9d]:[0x9d];var i=FlagStackClassify(bytes);
            var m=fixture.Machine();m->ip=Ip;m->oplen=0;m->attention=false;
            m->flags=0x01003ed5;m->flags&=~0x50100u;
            Write64((byte*)m,56,guest+4096+128);
            byte* data=(byte*)fixture.Physical(guest+4096);
            for(int b=0;b<width/8;++b)data[128+b]=(byte)(value>>(8*b));
            byte[] before=new ReadOnlySpan<byte>((byte*)m,sizeof(Blink.Machine)).ToArray();
            fixture.DataScope(0,()=>IntegerReference(fixture.Program,(byte*)m,bytes));
            uint expectedFlags=m->flags;ulong expectedRsp=((ulong*)((byte*)m+24))[4];ulong expectedIp=m->ip;
            before.CopyTo(new Span<byte>((byte*)m,before.Length));
            using var code=CreateFullBlock([new(i)]);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame((byte*)m,&h,&epoch,&issued,&completed,&stop,limit:1);
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            f.MemoryTlb=tlb;f.MemoryTlbMask=63;SetFullMemoryEntry(tlb,guest+4096,data);
            Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&m->flags==expectedFlags&&m->ip==expectedIp&&
                ((ulong*)((byte*)m+24))[4]==expectedRsp&&issued==1&&completed==1,
                "POPF actual selected normal oracle in common CPL3/IOPL3 domain");
        }
    }

    static void CheckFlagStackLinkedLeaves(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AlignedAlloc(4096,4096);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(bool helper in new[]{false,true})foreach(uint changed in new uint[]{0,0x100,0x40000})foreach(uint cap in new uint[]{0,1})
            {
                var pop=FlagStackClassify([0x9d]);
                using var source=CreateFullBlock([new(pop)]);
                using var target=CreateFullBlock(helper?[FullBlockInstruction.Helper(Ip+1,2)]:
                    [new(new(Ip+1,1,JitOperation.Nop,64,default,default))]);
                ResetFull(machine);Write64(machine,56,0x100000);*(ulong*)data=changed;
                ulong epoch=11;long issued=0,completed=0;int stop=0;
                var a=FullHeader(Ip,source.Entry);var b=FullHeader(Ip+1,target.Entry,18);
                a.Link0TargetHeader=(nint)(&b);a.Link0TargetGeneration=b.BodyGeneration;a.Link0Pc=Ip+1;
                var frame=FullFrame(machine,&a,&epoch,&issued,&completed,&stop,limit:cap);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
                SetFullMemoryEntry(tlb,0x100000,data,FullMemoryAccess.Read);
                Check(FloatRunProbe(source,&frame)==FullBlockExit.Continue&&issued==1&&completed==1&&frame.Executed==1&&
                    frame.HelperIndex==FullBlockFrame.NoHelperIndex&&frame.Header==&a&&frame.TailJumps==0&&
                    ((Blink.Machine*)machine)->ip==Ip+1&&((ulong*)(machine+24))[4]==0x100008,
                    "R119 POPF returns before a prelinked helper/inline target for changed or unchanged flags, unbounded/cap1");
            }
        }
        finally{NativeMemory.AlignedFree(data);}
    }

    static void CheckFlagStackLinkedController()
    {
        foreach(bool helper in new[]{false,true})foreach(uint changed in new uint[]{0x100,0x40000})
        {
            using var f=new ThreadedGuestExecution.FullControllerFixture();const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;
            f.Map(pc,8192);f.Fill(pc,8192);
            WriteControllerCode(f,pc,helper?[0x9d,0x0f,0xa2,0xeb,0xfe]:[0x9d,0x90,0xeb,0xfe]);
            var target=WarmLastExit(f,pc+1);var source=WarmLastExit(f,pc);
            Check(LastExitHeader(f,source).Link0TargetHeader==0,"R119 POPF publication has no static fallthrough link");
            // Deliberately install an otherwise authenticated old link using
            // the real store; the native state boundary must still protect it.
            Check(f.Jit.Store.TryLink(source,0,target),"R119 real retained prelinked target");
            *(ulong*)((byte*)f.Physical(pc+4096)+128)=changed;
            Write64((byte*)f.Machine(),56,pc+4096+128);f.Counts(100);
            Check(f.Step(pc)&&f.Completed==1&&f.Issued==1&&f.Machine()->ip==pc+1&&
                f.Statistics.SemanticHelperInstructions==0&&f.Statistics.NativeTailJumps==0,
                "R119 controller leaves prelinked helper-first/inline-first target untouched after POPF TF/AC");
            Check(f.Acquire(pc+1,0,out var next,out _,out int error)&&error==0,"R119 reacquire changed execution context");
            using(next)
            {
                var header=f.Jit.Store.GetHeaderCopy(in next);
                Check(FullTranslationJit.Plans(f.Jit.Store,in next)[0].Helper!=null&&
                    header.Context!=LastExitHeader(f,target).Context,
                    "R119 separately acquired TF/AC target is explicitly a non-native compatibility plan");
            }
            LastExitClean(f);
        }
    }

    static void CheckFlagStackController()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture();const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(pc,8192);fixture.Fill(pc,8192);WriteControllerCode(fixture,pc,[0x9c,0x58,0x50,0x9d,0xeb,0xfe]);
        fixture.Machine()->flags=0x01000ad5;Write64((byte*)fixture.Machine(),56,pc+8192);
        uint flags=fixture.Machine()->flags;fixture.Counts(4);fixture.Step(pc);
        for(int n=0;n<16&&fixture.Completed<4;++n)fixture.Continue();
        Check(fixture.Completed==4&&((ulong*)((byte*)fixture.Machine()+24))[0]==ExpectedPushedFlags(flags)&&
            fixture.Machine()->flags==ExpectedPoppedFlags(flags,ExpectedPushedFlags(flags),64)&&
            ((ulong*)((byte*)fixture.Machine()+24))[4]==pc+8192&&fixture.Statistics.SemanticHelperInstructions==0,
            "controller PUSHF/POP/POPF mixed native paths and precise budget");
    }
}
