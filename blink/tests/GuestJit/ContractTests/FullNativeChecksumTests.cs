using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static JitInstruction ChecksumClassify(byte[] bytes)
    {
        Check(FullChecksumLowering.TryClassify(Ip,bytes,out var i,out var reason),reason??"CRC32 classify");
        Check(FullBlockEmitter.CanInline(i)&&FullBlockEmitter.ValidateChecksum(i),"native CRC32 validated IR");return i;
    }
    static byte[] ChecksumEncoding(int sourceWidth,int destination,int source,bool memory=false,bool wideByte=false)
    {
        List<byte> bytes=[0xf2];if(sourceWidth==16)bytes.Add(0x66);
        byte rex=(byte)(0x40|(sourceWidth==64||wideByte?8:0)|((destination>>3)<<2)|(source>>3));
        if(rex!=0x40||sourceWidth==8&&source is >=4 and <=7&&!memory)bytes.Add(rex);
        bytes.AddRange([0x0f,0x38,sourceWidth==8?(byte)0xf0:(byte)0xf1,
            (byte)((memory?0:0xc0)|(destination&7)<<3|(source&7))]);return bytes.ToArray();
    }
    static uint ReverseChecksum(uint x)
    {
        uint result=0;for(int bit=0;bit<32;++bit){result=result<<1|(x&1);x>>=1;}return result;
    }
    static uint ChecksumExpected(uint seed,ulong input,int width)
    {
        // Independent MSB-first polynomial remainder with the normal (not
        // reflected) Castagnoli polynomial; emitter uses reflected LSB steps.
        uint remainder=ReverseChecksum(seed);
        for(int bit=0;bit<width;++bit)
        {
            bool subtract=((remainder>>31)^((uint)(input>>bit)&1))!=0;
            remainder<<=1;if(subtract)remainder^=0x1edc6f41;
        }
        return ReverseChecksum(remainder);
    }
    static void CheckFullNativeChecksum(byte* machine)
    {
        uint known=uint.MaxValue;foreach(byte b in "123456789"u8)known=ChecksumExpected(known,b,8);
        Check((known^uint.MaxValue)==0xe3069283,"independent CRC32C known polynomial vector");
        CheckChecksumLeaves(machine);
        using(var owner=CreateFullScaffold(out var scaffold))
        {fullOwnerScaffold=scaffold;try{CheckChecksumLeaves(machine);}finally{fullOwnerScaffold=default;}}
        CheckChecksumOracle();CheckChecksumController();
        foreach(byte[] bytes in new byte[][]{[0x0f,0x38,0xf0,0xc1],[0xf3,0x0f,0x38,0xf0,0xc1],
            [0xf0,0xf2,0x0f,0x38,0xf1,0x03],[0xf2,0x0f,0x38,0xf0,0xc1,0],[]})
            Check(!FullChecksumLowering.TryClassify(Ip,bytes,out _,out _),"CRC32 missing/invalid mandatory prefixes and exact span");
        FullBlockInstruction[] block=[new(ChecksumClassify([0xf2,0x48,0x0f,0x38,0xf1,0xc1]))];
        byte[] scratch=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool win in new[]{false,true})Check(FullBlockEmitter.TryEmit(block,scratch,out int n,win)&&n>0,"CRC32 both ABI encoding");
        for(int n=0;n<32;++n)FullBlockEmitter.Emit(block,scratch);
        long allocated=GC.GetAllocatedBytesForCurrentThread();for(int n=0;n<500;++n)FullBlockEmitter.Emit(block,scratch);
        Check(allocated==GC.GetAllocatedBytesForCurrentThread(),"CRC32 warm bounded compiler allocation");ResetFull(machine);
    }
    static void CheckChecksumLeaves(byte* machine)
    {
        foreach(int width in new[]{8,16,32,64})foreach(int destination in Enumerable.Range(0,16))foreach(int source in Enumerable.Range(0,16))
        {
            var i=ChecksumClassify(ChecksumEncoding(width,destination,source));using var code=CreateFullBlock([new(i)]);
            foreach(ulong value in new[]{0ul,0x123456789abcdef0ul,ulong.MaxValue})
            {
                IntegerSeed(machine);Write64(machine,24+destination*8,0xfedcba9887654321);
                IntegerRegisterWrite(machine,i.Source,width,value);
                uint seed=(uint)((ulong*)(machine+24))[destination];
                ulong operand=(*(ulong*)(machine+24+i.Source.Register*8)>>(8*i.Source.ByteOffset))&IntegerMask(width);
                uint flags=((Blink.Machine*)machine)->flags;
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                    ((ulong*)(machine+24))[destination]==ChecksumExpected(seed,operand,width)&&((Blink.Machine*)machine)->flags==flags,
                    "software CRC32 all register aliases, zero extension and preserved guest flags");
            }
        }
        foreach(byte[] bytes in new byte[][]{[0xf2,0x0f,0x38,0xf0,0xc4],[0xf2,0x48,0x0f,0x38,0xf0,0xc1]})
        {
            var i=ChecksumClassify(bytes);using var code=CreateFullBlock([new(i)]);IntegerSeed(machine);
            Write64(machine,24,0x112233448765ab21);Write64(machine,32,0x42);
            uint seed=*(uint*)(machine+24);
            ulong value=*(ulong*)(machine+24+i.Source.Register*8)>>(8*i.Source.ByteOffset)&255;
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&*(ulong*)(machine+24)==ChecksumExpected(seed,value,8),"CRC32 AH alias and 64-bit byte destination");
        }
        byte* data=(byte*)NativeMemory.AlignedAlloc(8192,4096);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(int width in new[]{8,16,32,64})
            {
                var i=ChecksumClassify(ChecksumEncoding(width,0,3,true));using var code=CreateFullBlock([new(i)]);
                IntegerSeed(machine);Write64(machine,48,0x100fff);Write64(machine,24,0xabcdef0187654321);
                new Span<byte>(data,8192).Fill(0xa5);for(int b=0;b<width/8;++b)data[4095+b]=(byte)(b*37+11);
                ulong value=0;for(int b=0;b<width/8;++b)value|=(ulong)data[4095+b]<<(b*8);
                uint flags=((Blink.Machine*)machine)->flags;
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                Check(FloatRunProbe(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&
                    *(ulong*)(machine+24)==0xabcdef0187654321&&((Blink.Machine*)machine)->flags==flags,"CRC32 mappings before accumulator/flag effects");
                SetFullMemoryEntry(tlb,0x100000,data,FullMemoryAccess.Read);SetFullMemoryEntry(tlb,0x101000,data+4096,FullMemoryAccess.Read);
                FullNativeMemory.ResumeSameIssue(&f);
                Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                    *(ulong*)(machine+24)==ChecksumExpected(0x87654321,value,width)&&f.NativeMemoryComputations==1,"CRC32 exact read width and same issue resume");
                foreach(int segment in new[]{2,3})
                {
                    var bad=i with{Address=i.Address with{Segment=segment}};using var invalid=CreateFullBlock([new(bad)]);
                    IntegerSeed(machine);Write64(machine,48,0x0000800000000000);
                    h=FullHeader(Ip,invalid.Entry);issued=completed=0;f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                    f.MemoryTlb=(FullMemoryTlbEntry*)1;
                    Check(FloatRunProbe(invalid,&f)==FullBlockExit.GuestFault&&f.FaultTrap==(segment==2?12u:13u)&&
                        completed==0&&issued==1,"CRC32 canonical SS/GP priority before poisoned MMU");
                }
            }
        }
        finally{NativeMemory.AlignedFree(data);}
    }
    static void CheckChecksumOracle()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture(interpreterOracle:true);
        foreach(int width in new[]{8,32,64})foreach(ulong value in new[]{0ul,1ul,0x123456789abcdef0ul,ulong.MaxValue})
        {
            var bytes=ChecksumEncoding(width,0,1);var i=ChecksumClassify(bytes);byte* m=(byte*)fixture.Machine();
            MemoryIntegerSeed(m);Write64(m,24,0x87654321);IntegerRegisterWrite(m,i.Source,width,value);
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            fixture.DataScope(0,()=>IntegerReference(fixture.Program,(byte*)fixture.Machine(),bytes));
            ulong expected=*(ulong*)(m+24);uint flags=((Blink.Machine*)m)->flags;
            before.CopyTo(new Span<byte>(m,before.Length));using var code=CreateFullBlock([new(i)]);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop,limit:1);
            Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&*(ulong*)(m+24)==expected&&((Blink.Machine*)m)->flags==flags,"CRC32 selected normal GetOp table oracle");
        }
        // The selected C explicitly rejects 66 CRC32 (and MOVBE); 16-bit CRC
        // uses independent architectural polynomial evidence, not that oracle.
    }
    static void CheckChecksumController()
    {
        using var f=new ThreadedGuestExecution.FullControllerFixture();const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;
        f.Map(pc,8192);f.Fill(pc,8192);byte[] bytes=ChecksumEncoding(64,0,3,true);
        WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);byte* m=(byte*)f.Machine();
        Write64(m,24,0x87654321);Write64(m,48,pc+4095);byte* first=(byte*)f.Physical(pc);byte* second=(byte*)f.Physical(pc+4096);
        AtomicWrite(first+4095,second,1,0,64,0xfedcba9876543210);
        f.Counts(1);f.Step(pc);for(int n=0;n<16&&f.Completed==0;++n)f.Continue();
        Check(f.Completed==1&&*(ulong*)(m+24)==ChecksumExpected(0x87654321,0xfedcba9876543210,64)&&
            f.Statistics.SemanticHelperInstructions==0,"actual controller crosspage CRC32 computation has no semantic helper");
    }
}
