using System.Numerics;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static JitInstruction BitCountClassify(byte[] bytes)
    {
        Check(FullBitCountLowering.TryClassify(Ip,bytes,out var i,out var reason),reason??"native bit-count classify");
        Check(FullBlockEmitter.CanInline(i)&&FullBlockEmitter.ValidateBitCount(i),"native bit-count IR validation");
        return i;
    }

    static byte[] BitCountEncoding(int width,byte op,byte modrm=0xc1)
        =>width==16?[0xf3,0x66,0x0f,op,modrm]:width==64?[0xf3,0x48,0x0f,op,modrm]:[0xf3,0x0f,op,modrm];

    static ulong BitCountExpected(ulong input,int width,byte opcode)
    {
        input&=IntegerMask(width);
        return opcode==0xb8?(ulong)BitOperations.PopCount(input):input==0?(ulong)width:
            opcode==0xbc?(ulong)BitOperations.TrailingZeroCount(input):
                (ulong)(BitOperations.LeadingZeroCount(input)-(64-width));
    }

    static uint BitCountFlags(uint before,ulong input,ulong result,byte opcode)
        =>opcode==0xb8?(before&~0xff0008d5u)|0x01000000u|(input==0?64u:0u):
            (before&~0x41u)|(input==0?1u:0u)|(result==0?64u:0u);

    static void CheckFullNativeBitCounts(byte* machine)
    {
        foreach(int width in new[]{16,32,64})
        foreach(byte op in new byte[]{0xb8,0xbc,0xbd})
        {
            var i=BitCountClassify(BitCountEncoding(width,op));
            Check(i.Width==width&&i.SourceWidth==width,"count operand width");
        }
        foreach(byte[] bytes in new byte[][]{[0x66,0x67,0x64,0xf3,0x4c,0x0f,0xb8,0x44,0x8b,0x80],
            [0xf3,0x66,0x66,0x48,0x0f,0xbd,0xc1],[0x48,0x66,0xf3,0x0f,0xbc,0xc1],
            [0x64,0x65,0xf3,0x67,0x0f,0xb8,0x05,0,0,0,0]})BitCountClassify(bytes);
        foreach(byte[] bytes in new byte[][]{[],[0xf3],[0x0f,0xb8,0xc1],[0xf2,0x0f,0xbc,0xc1],
            [0xf0,0xf3,0x0f,0xbd,0xc1],[0xf3,0x0f,0xbc,0xc1,0]})
            Check(!FullBitCountLowering.TryClassify(Ip,bytes,out _,out _),"invalid/excluded count prefixes/length");
        CheckBitCountLeaves(machine);
        using(var common=CreateFullScaffold(out var shared))
        {fullOwnerScaffold=shared;try{CheckBitCountLeaves(machine);}finally{fullOwnerScaffold=default;}}
        CheckBitCountOracle();
        FullBlockInstruction[] steps=[new(BitCountClassify([0xf3,0x48,0x0f,0xb8,0xc1]))];
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool windows in new[]{false,true})Check(FullBlockEmitter.TryEmit(steps,staging,out int n,windows)&&n>0,"count both ABI encoding");
        for(int n=0;n<32;++n)FullBlockEmitter.Emit(steps,staging);
        long start=GC.GetAllocatedBytesForCurrentThread();
        for(int n=0;n<1000;++n)FullBlockEmitter.Emit(steps,staging);
        Check(GC.GetAllocatedBytesForCurrentThread()==start,"software count warmed span emission zero allocation");
        ResetFull(machine);
    }

    static void CheckBitCountLeaves(byte* machine)
    {
        foreach(int width in new[]{16,32,64})
        foreach(byte opcode in new byte[]{0xb8,0xbc,0xbd})
        foreach(bool alias in new[]{false,true})
        {
            var i=BitCountClassify(BitCountEncoding(width,opcode,alias?(byte)0xc9:(byte)0xc1));
            using var code=CreateFullBlock([new(i)]);
            var values=new List<ulong>{0,1,2,3,ulong.MaxValue,0x5555555555555555,0x8000000000000000,0x0123456789abcdef};
            for(int bit=0;bit<width;++bit){values.Add(1UL<<bit);values.Add(~(1UL<<bit));}
            foreach(ulong raw in values)
            {
                ResetFull(machine);Write64(machine,24,0xdeadbeefcafebabe);Write64(machine,32,raw);
                uint flags=0xa5000ed5u&~0x100u;((Blink.Machine*)machine)->flags=flags;
                ulong before=((ulong*)(machine+24))[i.Destination.Register];
                ulong input=raw&IntegerMask(width),result=BitCountExpected(input,width,opcode);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1,
                    "software count one native issue/completion");
                ulong expected=width==16?(before&~65535UL)|result:result;
                Check(((ulong*)(machine+24))[i.Destination.Register]==expected&&
                    ((Blink.Machine*)machine)->flags==BitCountFlags(flags,input,result,opcode),
                    "independent popcount/leading/trailing result, alias, width and defined flags");
            }
        }
        byte* data=(byte*)NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            foreach(int width in new[]{16,32,64})
            foreach(byte opcode in new byte[]{0xb8,0xbc,0xbd})
            {
                var i=BitCountClassify(BitCountEncoding(width,opcode,0x03));
                using var code=CreateFullBlock([new(i)]);ResetFull(machine);
                Write64(machine,48,0x100fff);Write64(machine,24,0xdeadbeefcafebabe);
                ulong input=(0x8000000080008001UL&IntegerMask(width));
                AtomicWrite(data+4095,data+4096,1,0,width,input);
                uint flags=((Blink.Machine*)machine)->flags;
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&
                    f.MemoryAddress==0x100fff&&f.MemoryAccess==FullMemoryAccess.Read&&f.MemoryWidth==width&&
                    ((ulong*)(machine+24))[0]==0xdeadbeefcafebabe&&((Blink.Machine*)machine)->flags==flags,
                    "count missing mapping has no flags/destination effects");
                SetFullMemoryEntry(tlb,0x100000,data,FullMemoryAccess.Read);
                SetFullMemoryEntry(tlb,0x101000,data+4096,FullMemoryAccess.Read);
                FullNativeMemory.ResumeSameIssue(&f);
                ulong result=BitCountExpected(input,width,opcode);
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                    ((ulong*)(machine+24))[0]==(width==16?0xdeadbeefcafe0000UL|result:result)&&
                    ((Blink.Machine*)machine)->flags==BitCountFlags(flags,input,result,opcode),
                    "count readonly split mapping resumes exact approved issue and generated computation");
            }
        }
        finally{NativeMemory.AlignedFree(data);}
    }

    static void CheckBitCountOracle()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture(interpreterOracle:true);
        byte* m=(byte*)fixture.Machine();
        foreach(int width in new[]{16,32,64})
        foreach(byte opcode in new byte[]{0xb8,0xbc})
        foreach(ulong input in new ulong[]{0,1,2,0x8000,0x80000000,ulong.MaxValue})
        {
            var bytes=BitCountEncoding(width,opcode);var i=BitCountClassify(bytes);MemoryIntegerSeed(m);
            ((Blink.Machine*)m)->flags=0x01000881; // source-selected PF/AF representation already canonical
            Write64(m,32,input);var before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            fixture.DataScope(0,()=>IntegerReference(fixture.Program,(byte*)fixture.Machine(),bytes));
            var expected=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            before.CopyTo(new Span<byte>(m,before.Length));using var code=CreateFullBlock([new(i)]);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop,limit:1);
            Check(RunFull(code,&f)==FullBlockExit.Continue&&new ReadOnlySpan<byte>(m+24,128).SequenceEqual(expected.AsSpan(24,128))&&
                ((Blink.Machine*)m)->flags==BinaryPrimitives.ReadUInt32LittleEndian(expected.AsSpan(12,4)),
                "selected normal POPCNT/TZCNT oracle in canonical undefined-flag domain");
        }
        // Selected F3 0F BD uses BSR's index rather than architectural leading
        // zero count. Keep a discriminating inherited-source witness.
        MemoryIntegerSeed(m);Write64(m,32,1);
        fixture.DataScope(0,()=>IntegerReference(fixture.Program,(byte*)fixture.Machine(),new byte[]{0xf3,0x48,0x0f,0xbd,0xc1}));
        Check(((ulong*)(m+24))[0]==0&&BitCountExpected(1,64,0xbd)==63,
            "selected LZCNT BSR-index defect retained; native independent architectural result is63");
    }
}
