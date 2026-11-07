using Managed.Emulation.Execution;
using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static void CheckFullNativeUnlockedBits(byte* machine)
    {
        using (var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle:true))
        {
            const ulong page = ThreadedGuestExecution.FullControllerFixture.Base;
            fixture.Map(page,8192); fixture.Fill(page,8192);
            CheckUnlockedBitOracles(fixture);
            using var common = CreateFullScaffold(out var shared);
            fullOwnerScaffold = shared;
            try { CheckUnlockedBitOracles(fixture); }
            finally { fullOwnerScaffold = default; }
        }
        byte* data = (byte*)NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            foreach (int width in new[] {16,32,64})
            foreach (byte opcode in new byte[] {0xab,0xb3,0xbb})
            foreach (long index in new long[] {-129,-65,-33,-17,-1,0,1,15,16,31,32,63,64,129})
            {
                var bytes = IntegerEncoding(width,0x0f,opcode,0x0b);
                var i = MemoryIntegerClassify(bytes); ResetFull(machine);
                ulong old = 0x85319ca7f012e64b & IntegerMask(width);
                Write64(machine,48,0x100800); Write64(machine,32,unchecked((ulong)index));
                uint flags = 0x55000ed4 & ~0x100u; ((Blink.Machine*)machine)->flags = flags;
                ulong address = MemoryIntegerAddress(i,machine,0,0);
                int at = checked((int)(address-0x100000));
                AtomicWrite(data+at,null,width/8,0,width,old);
                MemoryIntegerRun(machine,i,data+at,null,width/8);
                int bit = (int)(unchecked((ulong)index) & (uint)(width-1));
                ulong mask = 1UL << bit;
                ulong expected = opcode == 0xab ? old | mask : opcode == 0xb3 ? old & ~mask : old ^ mask;
                Check(AtomicRead(data+at,null,width/8,0,width) == (expected & IntegerMask(width)) &&
                    ((Blink.Machine*)machine)->flags == ((flags & ~1u) | (uint)((old >> bit)&1)),
                    "unlocked bit signed word displacement, exact bit modification and only CF changes");
            }
            CheckUnlockedBitTlb(machine,data);
        }
        finally { NativeMemory.AlignedFree(data); }
        var sample = MemoryIntegerClassify([0x48,0x0f,0xab,0x0b]);
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity]; FullBlockInstruction[] steps = [new(sample)];
        foreach (bool windows in new[] {false,true})
            Check(FullBlockEmitter.TryEmit(steps,staging,out int n,windows) && n > 0,"unlocked bit both ABI encodings");
        for (int k=0;k<32;++k) FullBlockEmitter.Emit(steps,staging);
        long allocated=GC.GetAllocatedBytesForCurrentThread();
        for (int k=0;k<1000;++k) FullBlockEmitter.Emit(steps,staging);
        Check(allocated==GC.GetAllocatedBytesForCurrentThread(),"unlocked bit warmed span emission has zero allocation");
        ResetFull(machine);
    }

    static void CheckUnlockedBitOracles(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        foreach (int width in new[] {16,32,64})
        foreach (byte opcode in new byte[] {0xab,0xb3,0xbb})
        foreach (int offset in new[] {64,65,4095})
        {
            var encoding=IntegerEncoding(width,0x0f,opcode,0x0b);
            var i=MemoryIntegerClassify(encoding);
            Check(i.Options==0 && i.Operation is JitOperation.BitSet or JitOperation.BitReset or JitOperation.BitComplement,
                "unlocked bit is ordinary native RMW, not AtomicMemory/access7");
            MemoryIntegerCompare(fixture,encoding,offset,1);
            int group=opcode==0xab?5:opcode==0xb3?6:7;
            foreach(byte immediate in new byte[] {0,1,15,16,31,32,63,64,255})
                MemoryIntegerCompare(fixture,IntegerEncoding(width,0x0f,0xba,(byte)(3|group<<3),immediate),offset);
        }
        foreach(byte opcode in new byte[] {0xab,0xb3,0xbb})
            Check(!FullMemoryIntegerLowering.TryClassify(Ip,[0xf0,0x48,0x0f,opcode,0x0b],out _,out _),
                "LOCK remains disabled rather than accidentally entering ordinary RMW");
    }

    static void CheckUnlockedBitTlb(byte* machine,byte* data)
    {
        var i=MemoryIntegerClassify([0x48,0x0f,0xbb,0x0b]);
        using var code=CreateFullBlock([new(i)]); ResetFull(machine);
        Write64(machine,48,0x100fff); Write64(machine,32,5);
        new Span<byte>(data,8192).Fill(0xa5);
        uint flags=((Blink.Machine*)machine)->flags;
        ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        f.MemoryTlb=tlb;f.MemoryTlbMask=63;
        SetFullMemoryEntry(tlb,0x100000,data);SetFullMemoryEntry(tlb,0x101000,data+4096,FullMemoryAccess.Read);
        Check(RunFull(code,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 &&
            f.MemoryAccess==(FullMemoryAccess.Read|FullMemoryAccess.Write) &&
            ((Blink.Machine*)machine)->flags==flags && data[4095]==0xa5 && data[4096]==0xa5,
            "readonly second page rejects ordinary RMW before CF and bytes");
        SetFullMemoryEntry(tlb,0x101000,data+4096);
        FullNativeMemory.ResumeSameIssue(&f);
        Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 &&
            data[4095]==0x85 && data[4096]==0xa5 && f.MemoryFlags!=FullMemoryFlags.BarrierConsumed,
            "ordinary checked TLB path computes native bit modification, no exclusive atomic ticket");
    }
}
