using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using AtomicAlignmentFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // Part of the SAME R156 family entry and build. AUTHORED_UNRUN.
    // The selected unaligned CMPXCHG16B result is a known oracle defect: no
    // interpreter equality is used for fault-domain expectations below.
    static void CheckFullAtomicAlignmentR156R1(byte* machine)
    {
        foreach(bool common in new[]{false,true})
        {
            using var scaffold=CreateFullScaffold(out var addresses);
            var previous=fullOwnerScaffold;fullOwnerScaffold=common?addresses:default;
            try { AtomicPairAlignmentLeafR156R1(machine); }
            finally { fullOwnerScaffold=previous; }
        }
        AtomicPairAlignmentMmuR156R1();
        AtomicPairAlignmentControllerR156R1();
        AtomicPairAliasesR156R1();
    }

    static void AtomicPairAlignmentLeafR156R1(byte* machine)
    {
        byte* memory=(byte*)NativeMemory.AllocZeroed(8192);
        try
        {
            foreach(byte[] bytes in new byte[][]{
                [0x48,0x0f,0xc7,0x0b],[0xf0,0x48,0x0f,0xc7,0x0b],
                [0x66,0xf0,0x48,0x0f,0xc7,0x0b],[0x36,0xf0,0x48,0x0f,0xc7,0x0b],
                [0x64,0x67,0xf0,0x48,0x0f,0xc7,0x0b],[0x65,0x67,0xf0,0x48,0x0f,0xc7,0x0b]})
            foreach(bool equal in new[]{false,true})foreach(bool forgedTicket in new[]{false,true})
            foreach(int residue in new[]{1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,4095})
            {
                var i=AtomicClassify(bytes);using var code=CreateFullBlock([new(i)]);
                AtomicSeed(machine);new Span<byte>(memory,8192).Fill(0xa5);
                // AS32 discards the high register bits before the full segment
                // base. All residues are in the FINAL linear address.
                bool segmented=i.Address.Segment is 4 or 5;
                Write64(machine,48,segmented?0x100100000UL:0x100000UL+(uint)residue);
                Write64(machine,24,0xa5a5a5a5a5a5a5a5UL^(equal?0UL:1UL));Write64(machine,40,0xa5a5a5a5a5a5a5a5UL);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
                if(i.Address.Segment==4)f.FsBase=(uint)residue;
                if(i.Address.Segment==5)f.GsBase=(uint)residue;
                // Poisoned hints/pointers prove fault precedes preparation,
                // and even an otherwise forged ticket is not consumed.
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
                if(forgedTicket)
                {
                    f.MemoryFlags=FullMemoryFlags.BarrierTicket;f.MemoryTicket=123;f.MemoryResume=1;
                    f.MemoryAddress=0x100000+(uint)residue;f.MemoryWidth=128;f.MemoryAccess=FullNativeMemory.AtomicReadWrite;
                    f.MemoryHost0=(byte*)1;f.MemoryHost1=(byte*)1;f.MemoryLength0=1;f.MemoryEpoch=epoch;
                }
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==13&&f.FaultCode==128&&f.FaultAddress==0&&
                    issued==1&&completed==0&&f.Executed==0&&f.NativeMemoryComputations==0&&f.MemoryAssists==0&&
                    f.MemoryFlags==(forgedTicket?FullMemoryFlags.BarrierTicket:FullMemoryFlags.None)&&f.MemoryTicket==(forgedTicket?123UL:0)&&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before)&&
                    new ReadOnlySpan<byte>(memory,8192).IndexOfAnyExcept((byte)0xa5)<0,
                    "R156 R1 unaligned16B #GP before MMU/ticket/compare/flags/register/memory effects on either CAS result");
            }
            foreach(bool stack in new[]{false,true})foreach(ulong address in new[]{0x7ffffffffff8UL,0x800000000001UL,ulong.MaxValue-7})
            {
                var i=AtomicClassify(stack?[0xf0,0x48,0x0f,0xc7,0x4d,0]:[0xf0,0x48,0x0f,0xc7,0x0b]);
                using var code=CreateFullBlock([new(i)]);AtomicSeed(machine);
                Write64(machine,24+i.Address.Base*8,address);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==(stack?12u:13u)&&f.FaultCode==128&&f.FaultAddress==0&&
                    issued==1&&completed==0&&f.Executed==0&&f.MemoryAssists==0&&f.MemoryResume==0&&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                    "R156 R1 noncanonical start/end/wrap #SS/#GP precedes16B alignment rejection");
            }
        }
        finally { NativeMemory.Free(memory); }
    }

    static void AtomicPairAlignmentMmuR156R1()
    {
        using var f=new AtomicAlignmentFixture();using var deadline=f.AtomicDeadline();var a=f.Jit.Authority;
        foreach(ulong address in new[]{0x400001UL,0x400008UL,0x40000fUL,0x400fffUL})
        foreach(bool inspection in new[]{false,true})
        {
            var request=new FullBlockFrame{MemoryAddress=address,MemoryWidth=128,MemoryAccess=FullNativeMemory.AtomicReadWrite,
                MemoryResume=1,ObservedGlobalEpoch=a.Epoch+1,MemoryTlb=(FullMemoryTlbEntry*)1,MemoryTlbMask=63};
            var instruction=AtomicClassify([0xf0,0x48,0x0f,0xc7,0x0b]);
            var machine=f.Machine();AtomicSeed((byte*)machine);Write64((byte*)machine,48,address);
            request.Machine=machine;request.FsBase=machine->fs.@base;request.GsBase=machine->gs.@base;
            Check(FullNativeMemory.AuthenticateAtomicRequest(&request,in instruction,machine),
                "R156 R1 unaligned128 is exact authenticated typed request, not malformed plan/IR");
            f.Jit.BeginRead(f.Program,false);
            try
            {
                lock(a.Gate)
                {
                    FullMemoryAssistResult result;
                    // TryFill transports the authenticated actual Machine;
                    // direct inspection additionally poisons its walk pointer.
                    // No TLB/PTE read, stale epoch or missing backing hides #GP.
                    if(inspection)
                    {
                        result=FullNativeMemory.InspectAtomicTicket(f.Program,(Blink.Machine*)1,&request,a,
                            out byte* first,out byte* second,out uint length,out nint page0,out nint page1);
                        Check(first==null&&second==null&&length==0&&page0==0&&page1==0,"R156 R1 rejected atomic inspection publishes no pointers");
                    }
                    else result=FullNativeMemory.TryFill(f.Program,machine,&request,a);
                    Check(result==FullMemoryAssistResult.GuestFault&&request.FaultTrap==13&&request.FaultCode==128&&request.FaultAddress==0&&
                        request.MemoryHost0==null&&request.MemoryHost1==null&&request.MemoryTicket==0&&request.MemoryResume==1,
                        "R156 R1 direct access7 width128 rejects unaligned exact request before any PTE query");
                }
            }
            finally { f.Jit.EndRead(f.Program); }
            AtomicFamilyAssertClean(f);
        }
    }

    static void AtomicPairAlignmentControllerR156R1()
    {
        foreach(string backing in new[]{"mapped","readonly","unmapped"})
        foreach(bool equal in new[]{false,true})foreach(bool stackOverride in new[]{false,true})
        foreach(int offset in new[]{65,4095})
        {
            using var f=new AtomicAlignmentFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicAlignmentFixture.Base,data=pc+8192;
            f.Map(pc);f.Fill(pc);
            byte* a=null;byte* b=null;byte[] before0=[],before1=[];
            if(backing!="unmapped")
            {
                f.Map(data,8192);f.Fill(data,8192);a=(byte*)f.Physical(data);b=(byte*)f.Physical(data+4096);
                f.DataScope(0,()=>AtomicFamilyWrite(a+offset,b,Math.Min(16,4096-offset),128,0));
                before0=new ReadOnlySpan<byte>(a,4096).ToArray();before1=new ReadOnlySpan<byte>(b,4096).ToArray();
                if(backing=="readonly")Check(f.Mutate("mprotect",data)==0,"R156 R1 alignment precedes read-only page fault");
            }
            byte[] encoding=stackOverride?[0x36,0xf0,0x48,0x0f,0xc7,0x0b]:[0xf0,0x48,0x0f,0xc7,0x0b];
            WriteControllerCode(f,pc,encoding);AtomicFamilySeed(f,data+(uint)offset);
            Write64((byte*)f.Machine(),24,equal?0UL:1UL);Write64((byte*)f.Machine(),40,0);f.At(pc);f.Counts(100);
            uint flags=f.Machine()->flags;byte[] registers=new ReadOnlySpan<byte>((byte*)f.Machine()+24,128).ToArray();
            var signal=f.StepSynchronousFault(pc);
            Check(signal.Signal==11&&signal.Detail?.EndsWith("signal=11 code=128")==true&&f.Machine()->trapno==13&&
                f.Machine()->faultaddr==0&&f.Machine()->ip==pc&&f.Completed==0&&f.Issued==1&&
                f.Machine()->flags==flags&&new ReadOnlySpan<byte>((byte*)f.Machine()+24,128).SequenceEqual(registers)&&
                f.Statistics.NativeMemoryAssists==0&&f.Statistics.NativeMemoryInstructions==0&&f.Statistics.SemanticHelperInstructions==0,
                "R156 R1 actual controller delivers alignment #GP(0)/SI_KERNEL, including false CAS and SS override, without partial effects");
            if(a!=null)Check(new ReadOnlySpan<byte>(a,4096).SequenceEqual(before0)&&new ReadOnlySpan<byte>(b,4096).SequenceEqual(before1),
                "R156 R1 both operand pages remain byte-exact on alignment fault");
            AtomicFamilyAssertClean(f);
        }
    }

    static void AtomicPairAliasesR156R1()
    {
        foreach(int width in new[]{64,128})
        {
            using var f=new AtomicAlignmentFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicAlignmentFixture.Base,data=pc+8192,alias0=pc+16384,alias1=alias0+4096;
            int offset=width==128?64:4095;ulong address=data+(uint)offset;
            f.Map(pc);f.Fill(pc);int fd=f.Io.CreateMemoryFile("r156-r1-pair-alias",0).Value;
            Check(f.Io.WriteAt(fd,new byte[8192],0).Value==8192,"R156 R1 shared two-page alias backing");
            f.AtomicMapSharedPage(data,fd,0);f.AtomicMapSharedPage(data+4096,fd,4096);f.Fill(data,8192);
            f.AtomicMapSharedPage(alias0,fd,0);f.AtomicMapSharedPage(alias1,fd,width==128?0:4096);
            f.WriteMov(alias0+128,17);f.WriteMov(alias1+128,19);
            byte* a=(byte*)f.Physical(data)+offset,b=(byte*)f.Physical(data+4096);int length=Math.Min(width/8,4096-offset);
            f.DataScope(0,()=>AtomicFamilyWrite(a,b,length,width,0));
            WriteControllerCode(f,pc,AtomicEncoding(width==128?64:32,[0x0f,0xc7,0x0b]));AtomicFamilySeed(f,address);
            Write64((byte*)f.Machine(),24,0);Write64((byte*)f.Machine(),40,0);f.Counts(100);int tickets=0;
            Check(f.Acquire(alias0+128,0,out var lease0,out _,out int error0,1)&&error0==0,"R156 R1 first watched physical alias");
            using(lease0)
            {
                Check(f.Acquire(alias1+128,0,out var lease1,out _,out int error1,1)&&error1==0,"R156 R1 second watched physical alias");
                using(lease1)
                {
                    f.NativeReturned=p=>{if(((FullBlockFrame*)p)->MemoryFlags==FullMemoryFlags.BarrierConsumed)++tickets;};
                    try {Check(f.Step(pc,signalActor:true)&&f.Issued==1&&f.Completed==1&&tickets==1,"R156 R1 pair consumes one exact native ticket");}
                    finally {f.NativeReturned=null;}
                    UInt128 expected=width==128?((UInt128)5<<64)|address:((UInt128)5<<32)|(uint)address;
                    Check(AtomicFamilyRead(a,b,length,width)==expected&&f.Jit.Store.GetHeaderCopy(in lease0).Live==0&&
                        f.Jit.Store.GetHeaderCopy(in lease1).Live==0,"R156 R1 crossing8B/aligned16B native pair revokes every watched physical alias");
                }
            }
            AtomicFamilyAssertClean(f);Check(f.Io.Close(fd).Succeeded,"R156 R1 shared alias descriptor cleanup");
        }
    }
}
