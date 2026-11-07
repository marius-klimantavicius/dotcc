using Managed.Emulation.Execution;

unsafe partial class Program
{
    static void CheckFullNativeDenseMemory()
    {
        foreach(uint bits in new uint[]{8,16,24,32,64,80,112,128,224,752,864,4096})
            Check(FullNativeMemory.ValidMemoryWidth(bits),"bounded byte-aligned dense mapping width");
        foreach(uint bits in new uint[]{0,1,7,9,127,4097,4104,uint.MaxValue})
            Check(!FullNativeMemory.ValidMemoryWidth(bits),"invalid/unbounded dense width rejected");
        using var fixture=new ThreadedGuestExecution.FullControllerFixture();
        const ulong page=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(page,8192);fixture.Fill(page,8192);
        byte* a=(byte*)fixture.Physical(page),b=(byte*)fixture.Physical(page+4096);
        byte[] beforeA=new ReadOnlySpan<byte>(a,4096).ToArray(),beforeB=new ReadOnlySpan<byte>(b,4096).ToArray();
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach(uint width in new uint[]{80,112,224,752,864,4096})
        foreach(int offset in new[]{64,4095})
        foreach(FullMemoryAccess access in new[]{FullMemoryAccess.Read,FullMemoryAccess.Write})
        {
            new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            var frame=new FullBlockFrame{MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=page+(uint)offset,
                MemoryAccess=access,MemoryWidth=width,MemoryResume=1};
            Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.Resolved&&
                tlb[FullNativeMemory.TlbIndex(page,63)].Host==a&&
                (offset+width/8<=4096||tlb[FullNativeMemory.TlbIndex(page+4096,63)].Host==b),
                "actual dense MMU resolves all required pages, no guest computation");
            Check(new ReadOnlySpan<byte>(a,4096).SequenceEqual(beforeA)&&new ReadOnlySpan<byte>(b,4096).SequenceEqual(beforeB),
                "dense read/write permission assist never performs semantic data access");
        }
        fixture.DataScope(0,()=>Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system,
            (long)(page+4096),4096,1,false)==0,"dense second page readonly"));
        foreach(uint width in new uint[]{80,112,224,752,864,4096})
        {
            new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            var frame=new FullBlockFrame{MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=page+4095,
                MemoryAccess=FullMemoryAccess.Write,MemoryWidth=width,MemoryResume=1};
            Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.GuestFault&&frame.FaultTrap==14&&
                frame.FaultCode==2&&frame.FaultAddress==page+4096&&
                tlb[FullNativeMemory.TlbIndex(page,63)].Host==null,
                "dense store denied second page publishes no partial first-page capability");
        }
    }
}
