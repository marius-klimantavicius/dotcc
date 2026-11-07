using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using E = Managed.Emulation.Execution.FullBlockEmitter.FullVectorExtendedOperation;

unsafe partial class Program
{
    // R111: ALL AUTHORED_UNRUN. Parent alone builds/emits/executes/qualifies.
    static void CheckFullNativeVectorExtended(byte* machine)
    {
        Check(sizeof(FullBlockFrame) == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(TranslationBlockHeader) == 160 && FullBlockEmitter.HasExpectedVectorLayout(), "R111 ABI6 immutable layout");
        CheckFullVectorExtendedClassifier();
        Check(!fullOwnerScaffold.IsValid, "R111 standalone scaffold starts cleared");
        using (var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle: true))
        {
            const ulong guest = ThreadedGuestExecution.FullControllerFixture.Base;
            fixture.Map(guest, 8192); fixture.Fill(guest, 8192);
            CheckFullVectorExtendedOracle(fixture);
            CheckFullVectorExtendedIndependent(machine);
            CheckFullVectorExtendedMemory(machine);
            CheckFullVectorExtendedIssues(machine);
            using var common = CreateFullScaffold(out var addresses);
            fullOwnerScaffold = addresses;
            try
            {
                CheckFullVectorExtendedOracle(fixture);
                CheckFullVectorExtendedIndependent(machine);
                CheckFullVectorExtendedMemory(machine);
                CheckFullVectorExtendedIssues(machine);
            }
            finally { fullOwnerScaffold = default; }
        }
        // No oracle DataScope/thread binding survives into these MMU fixtures.
        CheckFullVectorExtendedMmu();
        CheckFullVectorExtendedController();
        CheckFullVectorExtendedEmission();
    }

    sealed record VectorExtendedForm(string Name, byte Map, byte Opcode, E Operation, bool Immediate = false);
    static VectorExtendedForm[] VectorExtendedForms() => [
        new("pshufb",0x38,0x00,E.ShuffleByte), new("phaddw",0x38,0x01,E.HorizontalAddWord),
        new("phaddd",0x38,0x02,E.HorizontalAddDword), new("phaddsw",0x38,0x03,E.HorizontalAddSignedWord),
        new("pmaddubsw",0x38,0x04,E.MultiplyAddUnsignedSignedByte), new("phsubw",0x38,0x05,E.HorizontalSubWord),
        new("phsubd",0x38,0x06,E.HorizontalSubDword), new("phsubsw",0x38,0x07,E.HorizontalSubSignedWord),
        new("psignb",0x38,0x08,E.SignByte), new("psignw",0x38,0x09,E.SignWord),
        new("psignd",0x38,0x0a,E.SignDword), new("pmulhrsw",0x38,0x0b,E.MultiplyRoundedSignedWord),
        new("pabsb",0x38,0x1c,E.AbsoluteByte), new("pabsw",0x38,0x1d,E.AbsoluteWord),
        new("pabsd",0x38,0x1e,E.AbsoluteDword), new("pmulld",0x38,0x40,E.MultiplyLowDword),
        new("palignr",0x3a,0x0f,E.AlignRightBytes,true), new("pclmulqdq",0x3a,0x44,E.CarrylessMultiplyQword,true)
    ];

    static byte[] VectorExtendedEncoding(VectorExtendedForm form, bool memory = false, bool high = false, bool alias = false, byte control = 0)
    {
        var bytes = new List<byte>(7) { 0x66 };
        if (high) bytes.Add(memory ? (byte)0x44 : (byte)0x45);
        bytes.Add(0x0f); bytes.Add(form.Map); bytes.Add(form.Opcode);
        bytes.Add(memory ? (byte)0x03 : alias ? (byte)0xc0 : (byte)0xc1);
        if (form.Immediate) bytes.Add(control);
        return bytes.ToArray();
    }

    static JitInstruction VectorExtendedClassify(ReadOnlySpan<byte> bytes)
    {
        Check(FullVectorExtendedLowering.TryClassify(Ip, bytes, out var i, out var reason), reason ?? "R111 classify");
        Check(FullBlockEmitter.CanInlineVectorExtended(i) && FullBlockEmitter.ValidateVectorExtended(i), "R111 typed extended shape");
        return i;
    }

    static void CheckFullVectorExtendedClassifier()
    {
        foreach (var form in VectorExtendedForms())
        foreach (bool memory in new[] { false, true }) foreach (bool high in new[] { false, true })
        {
            var i = VectorExtendedClassify(VectorExtendedEncoding(form, memory, high, control: 0xff));
            Check(FullBlockEmitter.VectorExtendedOperation(i) == form.Operation && i.Width == 128 && i.SourceWidth == 128 &&
                i.Source.IsMemory == memory && !i.Destination.IsMemory && i.Destination.Register == (high ? 8 : 0) &&
                (memory || i.Source.Register == (high ? 9 : 1)) &&
                ((i.Options & FullBlockEmitter.VectorAlignedMemory) != 0) == memory &&
                (form.Immediate ? i.Extra == new JitOperand(0,255,true) : i.Extra == default), "R111 exact operation/register/memory/imm domain");
        }
        Check(VectorExtendedClassify([0x66,0x64,0x67,0x44,0x0f,0x38,0x00,0x54,0x8b,0xf8]).Address ==
            new JitAddress(3,1,2,-8,false,32,4), "R111 arbitrary legacy order FS/EA32/SIB/disp8");
        Check(VectorExtendedClassify([0x65,0x66,0x67,0x4f,0x0f,0x38,0x00,0x54,0x8b,0xf8]).Address ==
            new JitAddress(11,9,2,-8,false,32,5), "R111 GS/EA32/REX.BXR/ignoredW");
        Check(VectorExtendedClassify([0x67,0x66,0x41,0x0f,0x38,0x00,0x05,1,0,0,0]).Address ==
            new JitAddress(-1,-1,0,1,true,32), "R111 EIP-relative ignores REX.B");
        Check(VectorExtendedClassify([0x66,0x41,0x0f,0x38,0x00,0x04,0x25,0xf0,0xff,0xff,0xff]).Address ==
            new JitAddress(-1,-1,0,-16), "R111 SIB no-base ignores REX.B; disp32 sign extension");
        Check(VectorExtendedClassify([0x66,0x66,0x67,0x67,0x64,0x65,0x40,0x4f,0x0f,0x38,0x00,0xc1]).Source.Register == 9,
            "R111 redundant groups/last segment/last REX/ignored reg address prefix");
        Check(VectorExtendedClassify([0x45,0x66,0x0f,0x38,0x00,0xc1]).Destination.Register == 0,
            "R111 legacy prefix after REX cancels earlier REX");
        foreach (var form in VectorExtendedForms())
        {
            byte[] bytes = VectorExtendedEncoding(form);
            for (int length = 0; length < bytes.Length; ++length)
                Check(!FullVectorExtendedLowering.TryClassify(Ip, bytes.AsSpan(0,length),out _,out var why) && why != null,"R111 truncations fail closed");
            Check(!FullVectorExtendedLowering.TryClassify(Ip,bytes.Concat(new byte[]{0x90}).ToArray(),out _,out _),"R111 trailing bytes rejected");
        }
        foreach (byte[] bad in new byte[][] {
            [0x0f,0x38,0x00,0xc1], [0xf0,0x66,0x0f,0x38,0x00,0xc1],
            [0xf2,0x66,0x0f,0x38,0x00,0xc1], [0x66,0xf3,0x0f,0x38,0x00,0xc1],
            [0xc4,0xe2,0x79,0x00,0xc1], [0x62,0xf2,0x7d,0x08,0x00,0xc1],
            [0x66,0x0f,0x38,0x28,0xc1], [0x66,0x0f,0x38,0x20,0xc1], [0x66,0x0f,0x3a,0x0e,0xc1,0] })
            Check(!FullVectorExtendedLowering.TryClassify(Ip,bad,out _,out var why) && why != null,"R111 explicit MMX/group1/VEX/EVEX/unselected opcode MISSING");
        var good = VectorExtendedClassify([0x66,0x0f,0x38,0x00,0xc1]);
        foreach (var bad in new[] {
            good with { Width=64 },good with { SourceWidth=64 },good with { Length=0 },good with { Target=1 },
            good with { Condition=1 },good with { Options=0x2000 },good with { Options=0x2013 },
            good with { Options=good.Options|0x20000 },good with { Options=good.Options|FullBlockEmitter.VectorAlignedMemory },
            good with { Destination=new(16,0) },good with { Source=new(1,1) },good with { Extra=new(0,1,true) },
            good with { Source=new(0,0,IsMemory:true),Address=new(3,-1,0,0) },
            good with { Address=new(3,-1,0,0) },good with { Destination=new(0,0,IsMemory:true) },
            VectorExtendedClassify([0x66,0x0f,0x3a,0x44,0xc1,0]) with { Extra=default },
            VectorExtendedClassify([0x66,0x0f,0x38,0x00,0x03]) with { Address=new(16,-1,0,0) } })
        {
            bool rejected=false;
            try { FullBlockEmitter.ValidateVectorExtended(bad); } catch (ArgumentException) { rejected=true; }
            Check(rejected,"R111 invalid IR rejected before publication");
        }
        Check(!FullBlockEmitter.CanInlineVectorExtended(VectorClassify([0x66,0x0f,0xfc,0xc1])),"R111 separate R66 namespace");
    }

    // Architectural lane model, independent of generated extraction/append
    // sequences. Polynomial convolution uses parity per degree (no shared
    // shift/XOR recurrence). Rounding uses floor division rather than shifts.
    static long VectorExtendedLane(ReadOnlySpan<byte> bytes, int lane, int width, bool signed)
    {
        int at=lane*width/8;
        return width switch {
            8 => signed ? (sbyte)bytes[at] : bytes[at],
            16 => signed ? BinaryPrimitives.ReadInt16LittleEndian(bytes[at..]) : BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]),
            _ => signed ? (long)BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]) : BinaryPrimitives.ReadUInt32LittleEndian(bytes[at..]) };
    }
    static void VectorExtendedPut(Span<byte> bytes,int lane,int width,long value)
    {
        int at=lane*width/8;
        if(width==8) bytes[at]=unchecked((byte)value);
        else if(width==16) BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..],unchecked((ushort)value));
        else BinaryPrimitives.WriteUInt32LittleEndian(bytes[at..],unchecked((uint)value));
    }
    static byte[] VectorExtendedBits(E op, ReadOnlySpan<byte> destination, ReadOnlySpan<byte> source, byte control)
    {
        byte[] result=new byte[16];
        if(op==E.ShuffleByte)
        {
            for(int b=0;b<16;++b) result[b]=(source[b]&128)!=0?(byte)0:destination[source[b]%16];
        }
        else if(op is E.HorizontalAddWord or E.HorizontalAddDword or E.HorizontalAddSignedWord or
            E.HorizontalSubWord or E.HorizontalSubDword or E.HorizontalSubSignedWord)
        {
            bool sub=op is E.HorizontalSubWord or E.HorizontalSubDword or E.HorizontalSubSignedWord;
            bool sat=op is E.HorizontalAddSignedWord or E.HorizontalSubSignedWord;
            int width=op is E.HorizontalAddDword or E.HorizontalSubDword?32:16, half=64/width;
            for(int halfIndex=0;halfIndex<2;++halfIndex)
            {
                ReadOnlySpan<byte> input=halfIndex==0?destination:source;
                for(int pair=0;pair<half;++pair)
                {
                    long a=VectorExtendedLane(input,pair*2,width,sat),b=VectorExtendedLane(input,pair*2+1,width,sat);
                    long value=sub?a-b:a+b;
                    VectorExtendedPut(result,halfIndex*half+pair,width,sat?Math.Clamp(value,-32768,32767):value);
                }
            }
        }
        else if(op==E.MultiplyAddUnsignedSignedByte)
        {
            for(int pair=0;pair<8;++pair)
                VectorExtendedPut(result,pair,16,Math.Clamp(destination[pair*2]*(sbyte)source[pair*2]+
                    destination[pair*2+1]*(sbyte)source[pair*2+1],-32768,32767));
        }
        else if(op is E.SignByte or E.SignWord or E.SignDword or E.AbsoluteByte or E.AbsoluteWord or E.AbsoluteDword)
        {
            bool abs=op is E.AbsoluteByte or E.AbsoluteWord or E.AbsoluteDword;
            int width=op is E.SignByte or E.AbsoluteByte?8:op is E.SignWord or E.AbsoluteWord?16:32;
            for(int lane=0;lane<128/width;++lane)
            {
                long value=VectorExtendedLane(abs?source:destination,lane,width,true);
                long sign=VectorExtendedLane(source,lane,width,true);
                VectorExtendedPut(result,lane,width,abs?Math.Abs(value):sign<0?-value:sign==0?0:value);
            }
        }
        else if(op is E.MultiplyRoundedSignedWord or E.MultiplyLowDword)
        {
            int width=op==E.MultiplyRoundedSignedWord?16:32;
            for(int lane=0;lane<128/width;++lane)
            {
                long value=VectorExtendedLane(destination,lane,width,true)*VectorExtendedLane(source,lane,width,true);
                if(width==16)
                {
                    long dividend=value+16384;
                    value=Math.DivRem(dividend,32768,out long remainder);
                    if(remainder<0) --value;
                }
                VectorExtendedPut(result,lane,width,value);
            }
        }
        else if(op==E.AlignRightBytes)
        {
            byte[] concatenation=new byte[48]; source.CopyTo(concatenation); destination.CopyTo(concatenation.AsSpan(16));
            concatenation.AsSpan(Math.Min((int)control,32),16).CopyTo(result);
        }
        else if(op==E.CarrylessMultiplyQword)
        {
            ulong a=BinaryPrimitives.ReadUInt64LittleEndian(destination[((control&1)*8)..]);
            ulong b=BinaryPrimitives.ReadUInt64LittleEndian(source[(((control>>4)&1)*8)..]);
            BigInteger polynomial=BigInteger.Zero;
            for(int degree=0;degree<=126;++degree)
            {
                int parity=0;
                for(int j=Math.Max(0,degree-63);j<=Math.Min(63,degree);++j)
                    parity^=(int)(((a>>j)&1)&((b>>(degree-j))&1));
                if(parity!=0) polynomial|=BigInteger.One<<degree;
            }
            BinaryPrimitives.WriteUInt64LittleEndian(result,(ulong)(polynomial&ulong.MaxValue));
            BinaryPrimitives.WriteUInt64LittleEndian(result.AsSpan(8),(ulong)(polynomial>>64));
        }
        else throw new ArgumentException("R111 missing independent model");
        return result;
    }

    static byte[] VectorExtendedExpected(JitInstruction i,byte[] before,ReadOnlySpan<byte> memory)
    {
        byte[] expected=(byte[])before.Clone();
        ReadOnlySpan<byte> source=i.Source.IsMemory?memory:before.AsSpan(160+i.Source.Register*16,16);
        VectorExtendedBits(FullBlockEmitter.VectorExtendedOperation(i),before.AsSpan(160+i.Destination.Register*16,16),
            source,(byte)i.Extra.Immediate).CopyTo(expected,160+i.Destination.Register*16);
        BinaryPrimitives.WriteUInt64LittleEndian(expected,Ip+(uint)i.Length); expected[8]=0;
        return expected;
    }
    static void VectorExtendedRun(byte* machine,JitInstruction i,FullMemoryTlbEntry* tlb=null,ulong fs=0,ulong gs=0)
    {
        using var code=CreateFullBlock([new(i)]);
        ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=tlb;f.MemoryTlbMask=tlb==null?0u:63u;f.FsBase=fs;f.GsBase=gs;
        Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1&&
            f.NativeMemoryComputations==(i.Source.IsMemory?1UL:0UL),"R111 one issue/commit/generated computation");
    }

    static void CheckFullVectorExtendedOracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        byte* machine=(byte*)fixture.Machine(),page=(byte*)fixture.Physical(guest);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach(var form in VectorExtendedForms()) foreach(bool memory in new[]{false,true})
        foreach(bool high in new[]{false,true}) foreach(bool alias in memory?new[]{false}:new[]{false,true})
        foreach(byte control in form.Immediate?new byte[]{0,1,7,8,15,16,17,31,32,255}:new byte[]{0})
        {
            byte[] bytes=VectorExtendedEncoding(form,memory,high,alias,control);var i=VectorExtendedClassify(bytes);
            VectorSeed(machine);if(memory) Write64(machine,48,guest+4080);
            for(int b=0;b<4096;++b) page[b]=(byte)(b*39+0x81);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            byte[] memoryBefore=new ReadOnlySpan<byte>(page,4096).ToArray();
            byte[] expected=VectorExtendedExpected(i,before,new ReadOnlySpan<byte>(page+4080,16));
            // Actual authoritative XED + normal selected GetOp, not a typed
            // clone or a managed model. Scope owns source read and stash use.
            fixture.DataScope(0,()=>VectorReference(fixture.Program,(byte*)fixture.Machine(),bytes));
            VectorCheckArchitecture(machine,expected,$"R111 normal selected GetOp {form.Name}/mem={memory}/high={high}/alias={alias}/imm={control}");
            before.CopyTo(new Span<byte>(machine,before.Length));
            new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,guest,page,FullMemoryAccess.Read);
            VectorExtendedRun(machine,i,memory?tlb:null);
            VectorCheckArchitecture(machine,expected,"R111 independent native result, all GPR/XMM/flags/MXCSR retained outside destination");
            Check(new ReadOnlySpan<byte>(page,4096).SequenceEqual(memoryBefore),"R111 m128 read never writes guest memory");
        }
        foreach(var form in VectorExtendedForms().Where(x=>x.Operation is E.AlignRightBytes or E.CarrylessMultiplyQword))
        {
            // Exact selected semantic discrepancy: unaligned valid mapping
            // succeeds through Read16; architecture requires #GP(0). The native
            // fault assertion is below, never hidden by comparing this oracle.
            VectorSeed(machine);Write64(machine,48,guest+65);
            byte[] bytes=VectorExtendedEncoding(form,true,control:17);
            fixture.DataScope(0,()=>VectorReference(fixture.Program,(byte*)fixture.Machine(),bytes));
            Check(fixture.Machine()->ip==Ip+(uint)bytes.Length,"R111 EXPECTED SELECTED GAP: unaligned PALIGNR/PCLMULQDQ succeeds");
        }
    }

    static void CheckFullVectorExtendedIndependent(byte* machine)
    {
        foreach(var form in VectorExtendedForms()) foreach(bool alias in new[]{false,true})
        for(int sample=0;sample<(form.Immediate?256:20);++sample)
        {
            ResetFull(machine);VectorSeed(machine);
            var i=VectorExtendedClassify(VectorExtendedEncoding(form,high:true,alias:alias,control:(byte)sample));
            Span<byte> destination=new(machine+160+i.Destination.Register*16,16);
            Span<byte> source=new(machine+160+i.Source.Register*16,16);
            for(int b=0;b<16;++b){destination[b]=(byte)(sample*17+b*13);source[b]=(byte)(sample*29+255-b*17);}
            // Explicit saturation, sign/zero, INT_MIN, rounding and overflow
            // edges; all lane writes finish before either operand is sampled.
            if(sample<4)
            {
                Write64(machine,160+i.Destination.Register*16,sample switch {0=>0x8000800080008000UL,1=>0x7fff7fff7fff7fffUL,2=>ulong.MaxValue,_=>0UL});
                Write64(machine,160+i.Source.Register*16,sample switch {0=>0x8000800080008000UL,1=>0x7fff7fff7fff7fffUL,2=>0UL,_=>ulong.MaxValue});
            }
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            byte[] expected=VectorExtendedExpected(i,before,ReadOnlySpan<byte>.Empty);
            VectorExtendedRun(machine,i);VectorCheckArchitecture(machine,expected,"R111 independent lane/saturation/rounding/alias/ignored-imm-bit math");
        }
        var carry=VectorExtendedForms().Single(x=>x.Operation==E.CarrylessMultiplyQword);
        // Polynomial basis X^63*X^j traverses both 64-bit halves, degree126;
        // all-ones products above separately distinguish XOR from integer carry.
        for(int bit=0;bit<64;++bit)
        {
            ResetFull(machine);VectorSeed(machine);Write64(machine,160,1UL<<63);Write64(machine,176,1UL<<bit);
            VectorExtendedRun(machine,VectorExtendedClassify(VectorExtendedEncoding(carry)));
            Check(BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(machine+160,8))==(bit==0?1UL<<63:0UL)&&
                BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(machine+168,8))==(bit==0?0UL:1UL<<(bit-1)),
                "R111 independent carryless basis and high-half carries forbidden");
        }
    }

    static void CheckFullVectorExtendedMemory(byte* machine)
    {
        const ulong guest=0x400000;
        byte* allocation=(byte*)NativeMemory.AllocZeroed(12288);
        try
        {
            byte* page=allocation,second=allocation+8192;
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(var form in VectorExtendedForms())
            foreach(int offset in new[]{64,4080}) foreach(string mapping in new[]{"read","write-only","missing","missing-next"})
            {
                ResetFull(machine);VectorSeed(machine);Write64(machine,48,guest+(uint)offset);
                for(int b=0;b<12288;++b)allocation[b]=(byte)(b*23+0x55);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                byte[] memoryBefore=new ReadOnlySpan<byte>(allocation,12288).ToArray();
                var i=VectorExtendedClassify(VectorExtendedEncoding(form,true,control:255));
                var expected=VectorExtendedExpected(i,before,new ReadOnlySpan<byte>(page+offset,16));
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                if(mapping!="missing")SetFullMemoryEntry(tlb,guest,page,mapping=="write-only"?FullMemoryAccess.Write:FullMemoryAccess.Read);
                if(mapping!="missing-next")SetFullMemoryEntry(tlb,guest+4096,second,FullMemoryAccess.Read);
                using var code=CreateFullBlock([new(i)]);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                if(mapping is "missing" or "write-only")
                {
                    Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.Executed==0&&f.MemoryResume==1&&
                        f.MemoryAddress==guest+(uint)offset&&f.MemoryWidth==128&&f.MemoryAccess==FullMemoryAccess.Read&&
                        f.NativeMemoryComputations==0&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before)&&
                        new ReadOnlySpan<byte>(allocation,12288).SequenceEqual(memoryBefore),"R111 precise full-width permission/mapping precheck before any destination effects");
                    SetFullMemoryEntry(tlb,guest,page,FullMemoryAccess.Read);FullNativeMemory.ResumeSameIssue(&f);
                }
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1&&f.NativeMemoryComputations==1,
                    "R111 same issue resume exact m128 read; zero result PALIGNR still reads source; next page not needed at4080");
                VectorCheckArchitecture(machine,expected,"R111 exact memory output and high guest XMM lanes");
                Check(new ReadOnlySpan<byte>(allocation,12288).SequenceEqual(memoryBefore),"R111 read-only operation preserves both pages and noncontiguous gap");
            }
            foreach(var form in VectorExtendedForms()) foreach(int offset in new[]{1,65,4081,4095})
            {
                ResetFull(machine);VectorSeed(machine);Write64(machine,48,guest+(uint)offset);
                ((Blink.Machine*)machine)->segvcode=2;
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                using var code=CreateFullBlock([new(VectorExtendedClassify(VectorExtendedEncoding(form,true,control:255)))]);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
                Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==13&&f.FaultCode==128&&f.FaultAddress==0&&
                    issued==1&&completed==0&&f.Executed==0&&f.NativeMemoryComputations==0&&f.MemoryResume==0&&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R111 unaligned/cross-page #GP precedes missing mappings and never partially changes destination");
            }
            // Actual EA32 wrap before FS/GS, high REX operands and SIB scales;
            // high bits on both address inputs must be discarded before base.
            foreach(byte segment in new byte[]{0x64,0x65})
            {
                ResetFull(machine);VectorSeed(machine);Write64(machine,48,0x100000010);Write64(machine,32,0x100000002);
                var i=VectorExtendedClassify(new byte[]{0x66,segment,0x67,0x44,0x0f,0x38,0x00,0x54,0x8b,0xf8});
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                var expected=VectorExtendedExpected(i,before,new ReadOnlySpan<byte>(page+16,16));
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,guest,page,FullMemoryAccess.Read);
                VectorExtendedRun(machine,i,tlb,segment==0x64?guest:0,segment==0x65?guest:0);
                VectorCheckArchitecture(machine,expected,"R111 AS32/SIB/FS/GS/REX exact computation");
            }
            foreach(var form in VectorExtendedForms())
            foreach(string shape in new[]{"rip64","rip32","sib64","sib32","sib-rex","sib-nobase","rex-cleared"})
            {
                ResetFull(machine);VectorSeed(machine);
                var bytes=new List<byte>();
                if(shape=="rex-cleared")bytes.Add(0x45);
                bytes.Add(0x66);if(shape is "rip32" or "sib32")bytes.Add(0x67);
                if(shape=="sib-rex")bytes.Add(0x4f);
                bytes.Add(0x0f);bytes.Add(form.Map);bytes.Add(form.Opcode);
                if(shape is "rip64" or "rip32")
                {
                    bytes.Add(0x05);
                    int length=bytes.Count+4+(form.Immediate?1:0);
                    int displacement=checked((int)((long)guest+64-(long)Ip-length));
                    byte[] disp=new byte[4];BinaryPrimitives.WriteInt32LittleEndian(disp,displacement);bytes.AddRange(disp);
                }
                else if(shape=="sib-nobase")
                {
                    bytes.Add(0x04);bytes.Add(0x25);
                    byte[] disp=new byte[4];BinaryPrimitives.WriteUInt32LittleEndian(disp,(uint)guest+64);bytes.AddRange(disp);
                }
                else if(shape=="rex-cleared") {bytes.Add(0x03);Write64(machine,48,guest+64);}
                else
                {
                    // [base + index*4 - 8] with base=guest+64, index=2.
                    bytes.Add(0x44);bytes.Add(0x8b);bytes.Add(0xf8);
                    bool rex=shape=="sib-rex";bool address32=shape=="sib32";
                    Write64(machine,24+(rex?11:3)*8,guest+64+(address32?0x100000000UL:0));
                    Write64(machine,24+(rex?9:1)*8,2+(address32?0x100000000UL:0));
                }
                if(form.Immediate)bytes.Add(0x11);
                var i=VectorExtendedClassify(bytes.ToArray());byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                byte[] expected=VectorExtendedExpected(i,before,new ReadOnlySpan<byte>(page+64,16));
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,guest,page,FullMemoryAccess.Read);
                VectorExtendedRun(machine,i,tlb);VectorCheckArchitecture(machine,expected,$"R111 actual {shape} full-width memory computation");
            }
        }
        finally {NativeMemory.Free(allocation);}
    }

    static void CheckFullVectorExtendedIssues(byte* machine)
    {
        foreach(var form in VectorExtendedForms())
        {
            var i=VectorExtendedClassify(VectorExtendedEncoding(form,high:true,control:255));using var code=CreateFullBlock([new(i)]);
            foreach(string boundary in new[]{"stop","attention","budget","approved-attention"})
            {
                ResetFull(machine);VectorSeed(machine);machine[11]=(byte)(boundary.Contains("attention")?1:0);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                ulong epoch=11;long issued=boundary=="approved-attention"?1:0,completed=0;int stop=boundary=="stop"?1:0;
                var h=FullHeader(Ip,code.Entry);var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:boundary=="budget"?0UL:1UL);
                f.FirstPreissued=(uint)(boundary=="approved-attention"?1:0);var exit=RunFull(code,&f);
                if(boundary=="approved-attention")Check(exit==FullBlockExit.Continue&&issued==1&&completed==1,"R111 already approved operation precedes late attention");
                else Check(exit==(boundary=="stop"?FullBlockExit.Stop:boundary=="attention"?FullBlockExit.Attention:FullBlockExit.Budget)&&
                    completed==0&&issued==(boundary=="budget"?1:0)&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R111 same issue budget/stop/poll no destination effects");
            }
        }
    }

    static void CheckFullVectorExtendedMmu()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture();const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(guest);fixture.Fill(guest);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        var f=new FullBlockFrame{MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=guest+4080,MemoryAccess=FullMemoryAccess.Read,MemoryWidth=128,MemoryResume=1};
        Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.Resolved,"R111 exact final m128 does not access next unmapped page");
        f.MemoryAddress=guest+4095;f.MemoryResume=1;
        Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.GuestFault&&f.FaultTrap==14&&f.FaultAddress==guest+4096,
            "R111 generic m128 MMU authenticates second page; actual legacy instruction alignment faults earlier");
    }

    static void CheckFullVectorExtendedController()
    {
        foreach(var form in VectorExtendedForms())
        {
            using var fixture=new ThreadedGuestExecution.FullControllerFixture();const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;
            fixture.Map(pc);fixture.Fill(pc);
            byte[] operation=VectorExtendedEncoding(form,true,control:255),body=new byte[operation.Length+1];body[0]=0x90;operation.CopyTo(body,1);
            WriteControllerCode(fixture,pc,body);var m=fixture.Machine();Write64((byte*)m,48,pc+8192+4095);
            byte[] vectors=new ReadOnlySpan<byte>((byte*)m+160,256).ToArray();uint flags=m->flags,mxcsr=m->mxcsr;
            fixture.Counts(100);using var deadline=new Timer(_=>fixture.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var signal=fixture.StepSynchronousFault(pc);
            Check(signal.Signal==11&&signal.Detail?.EndsWith("signal=11 code=128")==true&&m->trapno==13&&m->faultaddr==0&&
                m->ip==pc+1&&m->oplen==0&&m->flags==flags&&m->mxcsr==mxcsr&&new ReadOnlySpan<byte>((byte*)m+160,256).SequenceEqual(vectors),
                "R111 controller architecture-first #GP precise signal and no partial destination");
            Check(fixture.Issued==2&&fixture.Completed==1&&fixture.Statistics.SemanticHelperInstructions==0&&fixture.Statistics.InterpreterAttempts==0&&
                fixture.Statistics.ActiveDataBorrows==0&&fixture.Jit.Store.GetStatistics().ActiveNativeEpochs==0&&fixture.Jit.Store.GetStatistics().ActiveManagedLeases==0,
                "R111 generated native path releases ownership on fault without Op trampoline");
        }
    }

    static void CheckFullVectorExtendedEmission()
    {
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity],scaffoldBytes=new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach(bool windows in new[]{false,true}) foreach(bool shared in new[]{false,true})
        {
            FullBlockEmitter.EmitScaffold(scaffoldBytes,out var layout,windows);
            // Never executed: deterministic synthetic RX address for shared
            // encoding/allocation contracts on either ABI from either host.
            var scaffold=layout.Resolve((nint)0x10000000);
            foreach(var form in VectorExtendedForms()) foreach(bool memory in new[]{false,true})
            {
                var i=VectorExtendedClassify(VectorExtendedEncoding(form,memory,high:true,control:17));FullBlockInstruction[] instructions=[new(i)];
                for(int warm=0;warm<8;++warm)
                    Check(shared?FullBlockEmitter.TryEmit(instructions,staging,in scaffold,out _,windows):FullBlockEmitter.TryEmit(instructions,staging,out _,windows),"R111 warm span emission");
                long before=GC.GetAllocatedBytesForCurrentThread();bool good=true;
                for(int repeat=0;repeat<16;++repeat)
                    good&=shared?FullBlockEmitter.TryEmit(instructions,staging,in scaffold,out _,windows):FullBlockEmitter.TryEmit(instructions,staging,out _,windows);
                Check(good&&GC.GetAllocatedBytesForCurrentThread()==before,"R111 both ABIs/scaffolds warmed emission zero allocation");
                int tiny;
                Check(!(shared?FullBlockEmitter.TryEmit(instructions,new byte[7],in scaffold,out tiny,windows):FullBlockEmitter.TryEmit(instructions,new byte[7],out tiny,windows))&&tiny==0,
                    "R111 no partial publication on short destination");
            }
        }
    }
}
