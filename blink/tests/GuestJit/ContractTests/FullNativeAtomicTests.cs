using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // R69 SOURCE-AUTHORED, UNRUN. Parent owns serial execution and real races.
    // Synthetic tickets below qualify generated semantics, not exclusion.
    static void CheckFullNativeAtomic(byte* machine)
    {
        Check(FullBlockFrame.ByteSize == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(TranslationBlockHeader) == 160, "R69 existing leaf ABI");
        Console.WriteLine("BEGIN atomic existing CheckFullAtomicClassifier()");
        CheckFullAtomicClassifier();
        using (var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle: true))
        {
        const ulong guest = ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(guest, 8192); fixture.Fill(guest, 8192);
        Console.WriteLine("BEGIN atomic existing CheckFullAtomicOracle(fixture)");
        CheckFullAtomicOracle(fixture);
        Console.WriteLine("BEGIN atomic existing CheckFullAtomicIndependent(machine)");
        CheckFullAtomicIndependent(machine);
        Console.WriteLine("BEGIN atomic existing CheckFullAtomicBoundaries(machine)");
        CheckFullAtomicBoundaries(machine);
        if (!fullOwnerScaffold.IsValid)
        {
            using var common = CreateFullScaffold(out var addresses);
            fullOwnerScaffold = addresses;
            try
            {
                Console.WriteLine("BEGIN atomic existing CheckFullAtomicOracle(fixture)");
        CheckFullAtomicOracle(fixture);
                Console.WriteLine("BEGIN atomic existing CheckFullAtomicIndependent(machine)");
        CheckFullAtomicIndependent(machine);
                Console.WriteLine("BEGIN atomic existing CheckFullAtomicBoundaries(machine)");
        CheckFullAtomicBoundaries(machine);
            }
            finally { fullOwnerScaffold = default; }
        }
        Console.WriteLine("BEGIN atomic existing CheckFullAtomicSpanAllocation()");
        CheckFullAtomicSpanAllocation();
        }
        // Separate owners bind distinct C runtime contexts; never nest them.
        Console.WriteLine("BEGIN atomic existing CheckFullAtomicMmu()");
        CheckFullAtomicMmu();
    }

    static JitInstruction AtomicClassify(byte[] bytes)
    {
        Check(FullAtomicLowering.TryClassify(Ip, bytes, out var i, out string? reason), reason ?? "R69 atomic classifier");
        Check(FullBlockEmitter.CanInlineAtomic(i) && FullBlockEmitter.ValidateAtomic(i), "R69 typed native atomic validation");
        return i;
    }

    static byte[] AtomicEncoding(int width, byte[] body, bool locked = true)
    {
        var bytes = new List<byte>();
        if (locked) bytes.Add(0xf0);
        if (width == 16) bytes.Add(0x66);
        if (width == 64) bytes.Add(0x48);
        bytes.AddRange(body); return bytes.ToArray();
    }

    static List<byte[]> AtomicEncodings()
    {
        var cases = new List<byte[]>();
        foreach (int w in new[] { 8, 16, 32, 64 })
        {
            foreach (bool locked in new[] { false, true })
            {
                cases.Add(AtomicEncoding(w, [w == 8 ? (byte)0x86 : (byte)0x87, 0x0b], locked));
                cases.Add(AtomicEncoding(w, [0x0f, w == 8 ? (byte)0xc0 : (byte)0xc1, 0x0b], locked));
                cases.Add(AtomicEncoding(w, [0x0f, w == 8 ? (byte)0xb0 : (byte)0xb1, 0x0b], locked));
            }
            for (int alu = 0; alu < 7; ++alu)
            {
                cases.Add(AtomicEncoding(w, [(byte)(alu * 8 + (w == 8 ? 0 : 1)), 0x0b]));
                cases.Add(AtomicEncoding(w, [w == 8 ? (byte)0x80 : (byte)0x83, (byte)(3 | alu << 3), 0xff]));
            }
            foreach (int group in new[] { 0, 1 })
                cases.Add(AtomicEncoding(w, [w == 8 ? (byte)0xfe : (byte)0xff, (byte)(3 | group << 3)]));
            foreach (int group in new[] { 2, 3 })
                cases.Add(AtomicEncoding(w, [w == 8 ? (byte)0xf6 : (byte)0xf7, (byte)(3 | group << 3)]));
            if (w != 8)
            {
                foreach (byte op in new byte[] { 0xab, 0xb3, 0xbb }) cases.Add(AtomicEncoding(w, [0x0f, op, 0x0b]));
                for (int group = 5; group < 8; ++group) cases.Add(AtomicEncoding(w, [0x0f, 0xba, (byte)(3 | group << 3), 0xff]));
            }
        }
        cases.AddRange(new byte[][] {
            [0x86,0x23], [0xf0,0x0f,0xc0,0x23], [0x0f,0xb0,0x23], // AH
            [0x40,0x86,0x23], [0xf0,0x40,0x0f,0xc0,0x23], // SPL
            [0x0f,0xb0,0x03], [0xf0,0x48,0x0f,0xc1,0x1b], // source aliases address base
            [0xf0,0x4d,0x0f,0xb1,0x13], // high guest registers
            [0x0f,0xc7,0x0b], [0xf0,0x0f,0xc7,0x0b],
            [0x48,0x0f,0xc7,0x0b], [0xf0,0x48,0x0f,0xc7,0x0b] });
        return cases;
    }

    static void CheckFullAtomicClassifier()
    {
        foreach (var bytes in AtomicEncodings()) AtomicClassify(bytes);
        var high = AtomicClassify([0x86,0x23]); var low = AtomicClassify([0x40,0x86,0x23]);
        Check(high.Source == new JitOperand(0,0,ByteOffset:1) && low.Source == new JitOperand(4,0), "R69 AH versus SPL");
        var fs = AtomicClassify([0x64,0x67,0xf0,0x48,0x01,0x44,0x8b,0xf8]);
        Check(fs.Address == new JitAddress(3,1,2,-8,false,32,4), "R69 FS EA32 SIB");
        Check(AtomicClassify([0xf0,0x48,0x81,0x03,0xff,0xff,0xff,0xff]).Source.Immediate == ulong.MaxValue,
            "R69 immediate32 sign extends to64");
        foreach (byte[] bytes in new byte[][] {
            [], [0xf0],
            [0xf0,0x01,0xc3], [0xf0,0x03,0x03], [0xf0,0x39,0x03], [0xf0,0x83,0x3b,1],
            [0xf0,0x0f,0xba,0x23,1], [0xf0,0x0f,0xa3,0x03], [0x0f,0xc7,0xc8], [0x0f,0xc7,0x03],
            [0xf0,0x0f,0xc7,0x13], [0xf0,0x87,0x03,0x90], [0xf0,0x48,0x81,0x03,1],
            [0x01,0x03], [0x0f,0xab,0x03] })
            Check(!FullAtomicLowering.TryClassify(Ip,bytes,out _,out _), "R69 explicit missing/invalid atomic form");
        var valid = AtomicClassify([0xf0,0x48,0x01,0x0b]);
        foreach (var bad in new[] {
            valid with { Width=128 }, valid with { SourceWidth=32 }, valid with { Options=0 },
            valid with { Options=valid.Options | 0x200 }, valid with { Extra=new(1,0) },
            valid with { Destination=new(0,0) }, valid with { Source=new(16,0) },
            valid with { Address=new(-1,-1,4,0) } })
        {
            bool rejected=false; try { FullBlockEmitter.ValidateAtomic(bad); } catch(ArgumentException) { rejected=true; }
            Check(rejected,"R69 malformed typed atomic IR rejected before emission");
        }
    }

    static void AtomicSeed(byte* machine)
    {
        var m=(Blink.Machine*)machine;
        m->ip=Ip; m->oplen=0; m->attention=false; m->stashaddr=0;
        m->mode.omode=Blink.XED_MODE_LONG; m->mode.genmode=Blink.XED_GEN_MODE_PROTECTED;
        m->flags=0xa52c0e15u & ~0x50100u;
        for(int r=0;r<16;++r) Write64(machine,24+r*8,0xfedcba9876543210UL ^ (0x0101010101010101UL*(uint)r));
    }

    // This is the selected NORMAL interpreter, with exact XED bytes. Never
    // call it outside fixture.DataScope: memory resolution owns physical borrows.
    static void AtomicReference(ThreadedGuestExecution.FullControllerFixture fixture, byte[] encoding)
    {
        Blink program=fixture.Program; var m=fixture.Machine(); Blink.XedDecodedInst decoded=default;
        fixed(byte* bytes=encoding)
            Check(program.DecodeInstruction(&decoded,bytes,(ulong)encoding.Length,Blink.XED_MODE_LONG)==0 &&
                decoded.length==encoding.Length,"R69 authoritative oracle decode");
        var previous=m->xedd; m->xedd=&decoded; m->oplen=(byte)encoding.Length; m->ip+=(uint)encoding.Length;
        try
        {
            var op=decoded.op;
            program.GetOp((long)(op.rde>>40&2047))(program,m,op.rde,op.disp,op.uimm0);
            if(m->stashaddr!=0) program.CommitStash(m);
            m->oplen=0;
        }
        finally { m->xedd=previous; }
    }

    static ulong AtomicRead(byte* first,byte* second,int firstLength,int offset,int width)
    {
        ulong value=0;
        for(int b=0;b<width/8;++b) { int n=offset+b; value|=(ulong)(n<firstLength?first[n]:second[n-firstLength])<<(8*b); }
        return value;
    }
    static void AtomicWrite(byte* first,byte* second,int firstLength,int offset,int width,ulong value)
    {
        for(int b=0;b<width/8;++b) { int n=offset+b; if(n<firstLength) first[n]=(byte)(value>>(8*b)); else second[n-firstLength]=(byte)(value>>(8*b)); }
    }

    static void AtomicRunTicket(byte* machine,JitInstruction i,byte* first,byte* second,int firstLength,
        ulong fs=0,ulong gs=0,ulong? expectedAddress=null)
    {
        using var code=CreateFullBlock([new(i)]);
        ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
        f.FsBase=fs; f.GsBase=gs;
        byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
        Check(RunFull(code,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 && f.Executed==0 &&
            f.MemoryWidth==i.Width && (uint)f.MemoryAccess==7 && f.MemoryResume==1 && f.MemoryTicket==0 &&
            new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R69 atomic miss reserves one issue without effects");
        Check(expectedAddress==null || f.MemoryAddress==expectedAddress.Value,"R69 exact selected atomic effective address");
        FullNativeMemory.PublishBarrierTicket(&f,first,second,(uint)firstLength,epoch,123);
        FullNativeMemory.ResumeSameIssue(&f);
        Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1 &&
            f.NativeMemoryComputations==1 && f.MemoryTicket==0 && f.MemoryFlags==FullMemoryFlags.BarrierConsumed &&
            f.FirstPreissued==0 && f.MemoryResume==0 && ((ulong*)machine)[0]==Ip+(uint)i.Length && machine[8]==0,
            "R69 exact one-operation ticket resumes same issue and commits once");
    }

    static void CheckFullAtomicOracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        byte* page0=(byte*)fixture.Physical(guest),page1=(byte*)fixture.Physical(guest+4096),machine=(byte*)fixture.Machine();
        foreach(byte[] encoding in AtomicEncodings())
        // Selected C admits unaligned16B, an architectural fault-domain defect.
        // Compare only aligned16B common-domain rows; 8B retains unaligned/crossing.
        foreach(int offset in AtomicFamilyDataOffsets(AtomicClassify(encoding).Width))
        foreach(bool equal in new[]{false,true})
        {
            var i=AtomicClassify(encoding); var op=FullBlockEmitter.AtomicOperation(i);
            AtomicSeed(machine); Write64(machine,48,guest+(uint)offset); Write64(machine,112,guest+(uint)offset);
            Write64(machine,32,5); // bounded bit index, not a huge signed displacement
            for(int b=0;b<4096;++b) { page0[b]=(byte)(b*37+11); page1[b]=(byte)(b*19+7); }
            int n=Math.Min(i.Width/8,4096-offset); byte* first=page0+offset; byte* second=n==i.Width/8?null:page1;
            if(op==FullAtomicOperation.CompareExchange)
            {
                ulong observed=AtomicRead(first,second,n,0,i.Width);
                IntegerRegisterWrite(machine,new(0,0),i.Width,observed^(equal?0UL:1UL));
                if(i.Width==32) Write64(machine,24,(((ulong*)(machine+24))[0])|0xfedcba9800000000UL);
            }
            else if(op==FullAtomicOperation.CompareExchangePair)
            {
                int half=i.Width/2;
                IntegerRegisterWrite(machine,new(0,0),half,AtomicRead(first,second,n,0,half));
                IntegerRegisterWrite(machine,new(2,0),half,AtomicRead(first,second,n,half/8,half)^(equal?0UL:1UL));
                if(half==32) { ((ulong*)(machine+24))[0]|=0xabcdef1200000000UL; ((ulong*)(machine+40))[0]|=0x8765432100000000UL; }
            }
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            byte[] before0=new ReadOnlySpan<byte>(page0,4096).ToArray(),before1=new ReadOnlySpan<byte>(page1,4096).ToArray();
            Console.WriteLine($"atomic selected oracle BEFORE {Convert.ToHexString(encoding)}/{offset}/{equal}");
            fixture.DataScope(0,()=>AtomicReference(fixture,encoding));
            Console.WriteLine($"atomic selected oracle AFTER {Convert.ToHexString(encoding)}/{offset}/{equal}");
            byte[] expected=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            byte[] expected0=new ReadOnlySpan<byte>(page0,4096).ToArray(),expected1=new ReadOnlySpan<byte>(page1,4096).ToArray();
            // Native4 pair handlers use a read-only resolver and lose crossing
            // stores. This regression intentionally requires the parent's
            // authored producer fix; do not bless the old oracle's lost store.
            if(op==FullAtomicOperation.CompareExchangePair && equal)
            {
                int half=i.Width/2;
                ulong replacement=BitConverter.ToUInt64(before,48);
                Check(AtomicRead(first,second,n,0,half)==(replacement&IntegerMask(half)),"R69 selected pair oracle commits successful low half");
                Check(AtomicRead(first,second,n,half/8,half)==(BitConverter.ToUInt64(before,32)&IntegerMask(half)),"R69 selected pair oracle commits successful high half");
            }
            before.CopyTo(new Span<byte>(machine,before.Length)); before0.CopyTo(new Span<byte>(page0,4096)); before1.CopyTo(new Span<byte>(page1,4096));
            AtomicRunTicket(machine,i,first,second,n,expectedAddress:guest+(uint)offset);
            Check(new ReadOnlySpan<byte>(machine,9).SequenceEqual(expected.AsSpan(0,9)) &&
                new ReadOnlySpan<byte>(machine+12,4).SequenceEqual(expected.AsSpan(12,4)) &&
                new ReadOnlySpan<byte>(machine+24,128).SequenceEqual(expected.AsSpan(24,128)) &&
                new ReadOnlySpan<byte>(machine+160,256).SequenceEqual(expected.AsSpan(160,256)) &&
                new ReadOnlySpan<byte>(machine+456,236).SequenceEqual(expected.AsSpan(456,236)),
                $"R69 normal selected atomic oracle {Convert.ToHexString(encoding)}/{offset}/{equal}");
            Check(new ReadOnlySpan<byte>(page0,4096).SequenceEqual(expected0) && new ReadOnlySpan<byte>(page1,4096).SequenceEqual(expected1),
                "R69 exact memory bytes and both adjacent sentinels");
        }
    }

    static uint AtomicExpectedFlags(FullAtomicOperation op,int w,ulong x,ulong y,uint old)
    {
        ulong mask=IntegerMask(w),sign=1UL<<(w-1),result;
        x&=mask; y&=mask;
        if(op is FullAtomicOperation.Not or FullAtomicOperation.Exchange) return old;
        bool subtract=op is FullAtomicOperation.Sub or FullAtomicOperation.Sbb or FullAtomicOperation.Dec or FullAtomicOperation.Neg;
        bool logical=op is FullAtomicOperation.And or FullAtomicOperation.Or or FullAtomicOperation.Xor;
        ulong carry=op is FullAtomicOperation.Adc or FullAtomicOperation.Sbb ? old&1u : 0;
        if(op is FullAtomicOperation.Inc or FullAtomicOperation.Dec) y=1;
        if(op==FullAtomicOperation.Neg) { y=x; x=0; }
        UInt128 sum=(UInt128)x+y+carry;
        result=op switch { FullAtomicOperation.And=>x&y,FullAtomicOperation.Or=>x|y,FullAtomicOperation.Xor=>x^y,
            _=>subtract ? unchecked(x-y-carry)&mask : (ulong)sum&mask };
        bool cf=!logical && (subtract ? (UInt128)x<(UInt128)y+carry : sum>mask);
        bool of=!logical && (subtract ? ((x^y)&(x^result)&sign)!=0 : ((~(x^y))&(x^result)&sign)!=0);
        uint flags=(uint)((result&255)<<24) | (cf?1u:0) | (result==0?64u:0) | ((result&sign)!=0?128u:0) | (of?2048u:0) |
            (logical?0:(uint)((x^y^result)&16));
        uint replaced=op is FullAtomicOperation.Inc or FullAtomicOperation.Dec ? 0xff0008d0u : 0xff0008d1u;
        return (old&~replaced)|(flags&replaced);
    }

    static void CheckFullAtomicIndependent(byte* machine)
    {
        byte* page0=(byte*)NativeMemory.AllocZeroed(4096),page1=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            const ulong guest=0x100000;
            foreach(int w in new[]{8,16,32,64})
            foreach(int offset in new[]{32,4095})
            foreach(uint carry in new uint[]{0,1})
            foreach(ulong input in new[]{IntegerMask(w),(1UL<<(w-1))-1,1UL<<(w-1),0UL})
            foreach(FullAtomicOperation op in new[]{FullAtomicOperation.Add,FullAtomicOperation.Adc,FullAtomicOperation.Sub,FullAtomicOperation.Sbb,
                FullAtomicOperation.And,FullAtomicOperation.Or,FullAtomicOperation.Xor,FullAtomicOperation.Inc,FullAtomicOperation.Dec,
                FullAtomicOperation.Neg,FullAtomicOperation.Not,FullAtomicOperation.Exchange,FullAtomicOperation.ExchangeAdd})
            {
                bool unary=op is FullAtomicOperation.Inc or FullAtomicOperation.Dec or FullAtomicOperation.Neg or FullAtomicOperation.Not;
                new Span<byte>(page0,4096).Fill(0xa5); new Span<byte>(page1,4096).Fill(0x5a);
                ResetFull(machine); AtomicSeed(machine); ((uint*)(machine+12))[0]=(0xa52c0e14u&~0x50100u)|carry;
                Write64(machine,48,guest+(uint)offset); ulong x=input,y=1;
                Write64(machine,32,0xdeadbeef00000001UL); int n=Math.Min(w/8,4096-offset);
                byte* first=page0+offset,second=n==w/8?null:page1; AtomicWrite(first,second,n,0,w,x);
                var i=new JitInstruction(Ip,4,JitOperation.AtomicMemory,w,new(0,0,IsMemory:true),unary?default:new(1,0),
                    SourceWidth:w,Address:new(3,-1,0,0),Options:(uint)op|FullAtomicLowering.Locked);
                // Narrow source values are1; source64 retains its full input.
                if(w==64) Write64(machine,32,1);
                uint flags=AtomicExpectedFlags(op,w,x,y,((uint*)(machine+12))[0]);
                ulong result=op switch { FullAtomicOperation.And=>x&y,FullAtomicOperation.Or=>x|y,FullAtomicOperation.Xor=>x^y,
                    FullAtomicOperation.Not=>~x,FullAtomicOperation.Neg=>unchecked(0UL-x),FullAtomicOperation.Exchange=>y,
                    FullAtomicOperation.Sub or FullAtomicOperation.Dec=>unchecked(x-y),FullAtomicOperation.Sbb=>unchecked(x-y-carry),
                    FullAtomicOperation.Adc=>unchecked(x+y+carry),_=>unchecked(x+y) };
                AtomicRunTicket(machine,i,first,second,n);
                Check(AtomicRead(first,second,n,0,w)==(result&IntegerMask(w)) && ((uint*)(machine+12))[0]==flags,
                    "R69 independent scalar atomic arithmetic/lazy flags");
                if(op is FullAtomicOperation.Exchange or FullAtomicOperation.ExchangeAdd)
                    Check(((ulong*)(machine+32))[0]==(w==32?x:w==64?x:(0xdeadbeef00000001UL&~IntegerMask(w))|x),
                        "R69 exchange source partial width and upper32 clear");
                Check(page0[offset-1]==0xa5 && (offset+w/8<4096?page0[offset+w/8]==0xa5:page1[offset+w/8-4096]==0x5a),"R69 atomic exact byte count");
            }
            CheckFullAtomicCasIndependent(machine,page0,page1);
            CheckFullAtomicBitIndependent(machine,page0,page1);
        }
        finally { NativeMemory.Free(page0); NativeMemory.Free(page1); }
    }

    static void CheckFullAtomicCasIndependent(byte* machine,byte* page0,byte* page1)
    {
        foreach(int width in new[]{8,16,32,64,128})
        foreach(int offset in width==128?new[]{32,4080}:new[]{32,4095})
        foreach(bool equal in new[]{false,true})
        {
            bool pair=width==128; int half=pair?64:width;
            var op=pair?FullAtomicOperation.CompareExchangePair:FullAtomicOperation.CompareExchange;
            ResetFull(machine); AtomicSeed(machine); new Span<byte>(page0,4096).Fill(0xa5); new Span<byte>(page1,4096).Fill(0x5a);
            const ulong guest=0x100000; Write64(machine,48,guest+(uint)offset); Write64(machine,32,0x8877665544332211);
            int n=Math.Min(width/8,4096-offset); byte* first=page0+offset,second=n==width/8?null:page1;
            ulong old=0x1020304050607080UL&IntegerMask(half),high=0x8070605040302010;
            AtomicWrite(first,second,n,0,half,old); if(pair) AtomicWrite(first,second,n,8,64,high);
            Write64(machine,24,(old^(equal?0UL:1UL))|(width==32?0xaabbccdd00000000UL:0));
            if(pair) Write64(machine,40,high);
            ulong originalAx=((ulong*)(machine+24))[0]; uint previous=((uint*)(machine+12))[0];
            var i=new JitInstruction(Ip,5,JitOperation.AtomicMemory,width,new(0,0,IsMemory:true),pair?default:new(1,0),
                SourceWidth:width,Address:new(3,-1,0,0),Options:(uint)op|FullAtomicLowering.Locked);
            AtomicRunTicket(machine,i,first,second,n);
            Check(AtomicRead(first,second,n,0,half)==(equal?(pair?guest+(uint)offset:0x8877665544332211UL)&IntegerMask(half):old),
                "R69 independent CAS success/failed memory");
            Check(equal?((ulong*)(machine+24))[0]==originalAx:(((ulong*)(machine+24))[0]&IntegerMask(half))==old,"R69 CAS accumulator success/failure");
            if(width==32) Check(((ulong*)(machine+24))[0]==(equal?originalAx:old),"R69 failed CAS32 clears high accumulator; success preserves it");
            if(pair) Check(AtomicRead(first,second,n,8,64)==(equal?0x8877665544332211UL:high) &&
                ((uint*)(machine+12))[0]==((previous&~64u)|(equal?64u:0)),"R69 pair CAS changes only ZF and both halves together");
            else Check(((uint*)(machine+12))[0]==AtomicExpectedFlags(FullAtomicOperation.Sub,width,originalAx,old,previous),"R69 independent CAS subtraction flags");
        }
        // CMPXCHG8B independently covers both32 failure outputs and preserves
        // accumulator upper halves on success, including a crossing operand.
        foreach(bool equal in new[]{false,true})
        {
            ResetFull(machine); AtomicSeed(machine); Write64(machine,48,0x100fff); Write64(machine,32,0x87654321);
            Write64(machine,24,0xaaaaaaaa12345678); Write64(machine,40,equal?0xbbbbbbbb9abcdef0:0xbbbbbbbb9abcdef1);
            AtomicWrite(page0+4095,page1,1,0,32,0x12345678); AtomicWrite(page0+4095,page1,1,4,32,0x9abcdef0);
            var i=AtomicClassify([0xf0,0x0f,0xc7,0x0b]); AtomicRunTicket(machine,i,page0+4095,page1,1);
            Check(((ulong*)(machine+24))[0]==(equal?0xaaaaaaaa12345678UL:0x12345678UL) &&
                ((ulong*)(machine+40))[0]==(equal?0xbbbbbbbb9abcdef0UL:0x9abcdef0UL),"R69 CMPXCHG8B upper32 rules");
            Check(AtomicRead(page0+4095,page1,1,0,32)==(equal?0x100fffUL:0x12345678UL) &&
                AtomicRead(page0+4095,page1,1,4,32)==(equal?0x87654321UL:0x9abcdef0UL),"R69 CMPXCHG8B crosspage all-or-no store");
        }
    }

    static void CheckFullAtomicBitIndependent(byte* machine,byte* page0,byte* page1)
    {
        foreach(int w in new[]{16,32,64})
        foreach(long index in new long[]{-65,-1,0,15,16,31,32,63,64,129})
        foreach(byte opcode in new byte[]{0xab,0xb3,0xbb})
        {
            ResetFull(machine); AtomicSeed(machine); new Span<byte>(page0,4096).Fill(0xaa); new Span<byte>(page1,4096).Fill(0xbb);
            long signed=w==16?(short)index:w==32?(int)index:index;
            long displacement=(signed&-w)>>3; ulong address=unchecked(0x101000UL+(ulong)displacement);
            int off=(int)(address&4095),n=Math.Min(w/8,4096-off); byte* first=address<0x101000?page0+off:page1+off;
            byte* second=n==w/8?null:page1;
            Write64(machine,48,0x101000); Write64(machine,32,unchecked((ulong)index));
            ulong old=AtomicRead(first,second,n,0,w),mask=1UL<<(int)(signed&(w-1)),expected=opcode==0xab?old|mask:opcode==0xb3?old&~mask:old^mask;
            uint flags=((uint*)(machine+12))[0]; var i=AtomicClassify(AtomicEncoding(w,[0x0f,opcode,0x0b]));
            AtomicRunTicket(machine,i,first,second,n,expectedAddress:address);
            Check(AtomicRead(first,second,n,0,w)==expected && ((uint*)(machine+12))[0]==((flags&~1u)|((old&mask)!=0?1u:0)),
                "R69 signed memory bit displacement, modulo bit index and CF-only flags");
        }
        foreach(bool immediate in new[]{false,true})
        foreach(int segment in new[]{4,5})
        {
            ResetFull(machine); AtomicSeed(machine); new Span<byte>(page0,4096).Clear();
            Write64(machine,48,0x100000010); Write64(machine,32,ulong.MaxValue);
            var i=AtomicClassify(immediate ? [0x64,0x67,0xf0,0x48,0x0f,0xba,0x3b,0xff] :
                [0x64,0x67,0xf0,0x48,0x0f,0xbb,0x0b]);
            i=i with { Address=i.Address with { Segment=segment } };
            const ulong basis=0x200100000;
            ulong expected=basis+(immediate?0x10UL:0x8UL);
            AtomicRunTicket(machine,i,page0+(int)(expected&4095),null,8,
                fs:segment==4?basis:0,gs:segment==5?basis:0,expectedAddress:expected);
            Check(AtomicRead(page0+(int)(expected&4095),null,8,0,64)==0x8000000000000000UL,
                "atomic bit EA32 truncates offset before high FS/GS addition, including immediate");
        }
    }

    static void CheckFullAtomicBoundaries(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AllocZeroed(8192);
        try
        {
            var i=AtomicClassify([0xf0,0x48,0x0f,0xc1,0x0b]); using var code=CreateFullBlock([new(i)]);
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(int offset in new[]{32,4095})
            foreach(uint rights in new uint[]{1,2,3,7})
            {
                ResetFull(machine); AtomicSeed(machine); Write64(machine,48,0x100000+(uint)offset);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear(); new Span<byte>(data,8192).Fill(0xa5);
                SetFullMemoryEntry(tlb,0x100000,data,(FullMemoryAccess)rights); SetFullMemoryEntry(tlb,0x101000,data+4096,(FullMemoryAccess)rights);
                // R183 permits exact ordinary RW identity under enrollment;
                // this negative leaf case deliberately forges that identity.
                tlb[FullNativeMemory.TlbIndex(0x100000,63)].Identity ^= 1;
                ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry); var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);
                f.MemoryTlb=tlb; f.MemoryTlbMask=63; byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 && f.NativeMemoryComputations==0 &&
                    f.MemoryAddress==0x100000+(uint)offset && (uint)f.MemoryAccess==7 && f.MemoryResume==1 &&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                    "R183 forged identity/rights and crossing operands cannot enter scalar CAS");
                Check(new ReadOnlySpan<byte>(data,8192).IndexOfAnyExcept((byte)0xa5)<0,"R69 miss does not partially update either page");
            }
            foreach(string poll in new[]{"budget","stop","attention"})
            {
                ResetFull(machine); AtomicSeed(machine); Write64(machine,48,0x100000);
                ulong epoch=11; long issued=0,completed=0; int stop=poll=="stop"?1:0; if(poll=="attention") machine[11]=1;
                var h=FullHeader(Ip,code.Entry); var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:0);
                Check(RunFull(code,&f)==(poll=="stop"?FullBlockExit.Stop:poll=="attention"?FullBlockExit.Attention:FullBlockExit.Budget) &&
                    completed==0 && issued==(poll=="budget"?1:0) && f.MemoryResume==0,"R69 poll/budget precedence before atomic memory work");
            }
            ResetFull(machine); AtomicSeed(machine); Write64(machine,48,0x100000); Write64(machine,32,1);
            ulong ep=11; long iss=0,done=0; int st=0; var header=FullHeader(Ip,code.Entry); var frame=FullFrame(machine,&header,&ep,&iss,&done,&st);
            Check(RunFull(code,&frame)==FullBlockExit.NeedMemory,"R69 saturated ticket initial miss"); frame.Executed=uint.MaxValue;
            ((ulong*)data)[0]=9; FullNativeMemory.PublishBarrierTicket(&frame,data,null,8,ep,789); FullNativeMemory.ResumeSameIssue(&frame);
            Check(RunFull(code,&frame)==FullBlockExit.Continue && iss==1 && done==1 && frame.Executed==(ulong)uint.MaxValue+1 && ((ulong*)data)[0]==10,
                "R69 atomic ticket forces one completion beyond uint cap range");
            foreach(string mismatch in new[]{"address","access","width","site","epoch"})
            {
                ResetFull(machine); AtomicSeed(machine); Write64(machine,48,0x100000); Write64(machine,32,1);
                ep=11; iss=done=0; st=0; header=FullHeader(Ip,code.Entry); frame=FullFrame(machine,&header,&ep,&iss,&done,&st);
                Check(RunFull(code,&frame)==FullBlockExit.NeedMemory,"R69 mismatched ticket initial miss");
                ((ulong*)data)[0]=77; FullNativeMemory.PublishBarrierTicket(&frame,data,null,8,ep,456);
                if(mismatch=="address") ++frame.MemoryAddress;
                if(mismatch=="access") frame.MemoryAccess=FullMemoryAccess.Read|FullMemoryAccess.Write;
                if(mismatch=="width") frame.MemoryWidth=32;
                if(mismatch=="site") ++frame.MemorySite;
                if(mismatch=="epoch") ++frame.MemoryEpoch;
                // ResumeSameIssue authenticates site too, so deliberately
                // emulate a forged native input for that negative leaf case.
                if(mismatch=="site") frame.FirstPreissued=1;
                else FullNativeMemory.ResumeSameIssue(&frame);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                Check(RunFull(code,&frame)==(mismatch=="epoch"?FullBlockExit.NeedMemory:FullBlockExit.Invalidated) &&
                    iss==1 && done==0 && frame.NativeMemoryComputations==0 && ((ulong*)data)[0]==77 &&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R69 ticket exact identity mismatch fails before effects");
            }
            // Single-thread authored exclusion contract only: caller must own
            // real native/store/physical scopes; parent serial tests own races.
        }
        finally { NativeMemory.Free(data); }
    }

    static void CheckFullAtomicMmu()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture();
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(guest); fixture.Fill(guest);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        var f=new FullBlockFrame { MemoryTlb=tlb,MemoryTlbMask=63,MemoryAddress=guest+4095,MemoryWidth=64,MemoryAccess=(FullMemoryAccess)7,MemoryResume=1 };
        Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.GuestFault && f.FaultTrap==14 && f.FaultCode==1 && f.FaultAddress==guest+4096,
            "R69 atomic MMU prechecks unmapped second page even if CAS would fail");
        Check(tlb[FullNativeMemory.TlbIndex(guest,63)].Host==null,"R69 failed second-page precheck publishes no first-page mapping");
        fixture.Map(guest+4096); fixture.Fill(guest+4096);
        fixture.DataScope(0,()=>Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system,(long)(guest+4096),4096,1,false)==0,"R69 second page becomes read-only"));
        Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.GuestFault && f.FaultCode==2 && f.FaultAddress==guest+4096,
            "R69 failed CAS still requires write permission on both pages");
        f.MemoryAddress=guest; f.MemoryWidth=64;
        Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.WriteBarrier,"R69 valid atomic request always requires runtime exclusion ticket");
        f.MemoryAddress=0x0000800000000000;
        Check(FillFullMemoryMmu(fixture,&f)==FullMemoryAssistResult.GuestFault && f.FaultTrap==13 && f.FaultCode==128 && f.FaultAddress==0,"R156 direct atomic assist canonical #GP");
    }

    static void CheckFullAtomicSpanAllocation()
    {
        FullBlockInstruction[] body=[new(AtomicClassify([0xf0,0x48,0x0f,0xc1,0x0b])),
            new(AtomicClassify([0xf0,0x48,0x0f,0xc7,0x0b]) with { Ip=Ip+5 })];
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool windows in new[]{false,true})
        {
            for(int warm=0;warm<10;++warm) Check(FullBlockEmitter.TryEmit(body,staging,out _,windows),"R69 atomic warm emission fits");
            long before=GC.GetAllocatedBytesForCurrentThread(); bool good=true;
            for(int repeat=0;repeat<32;++repeat) good&=FullBlockEmitter.TryEmit(body,staging,out int length,windows)&&length>0;
            Check(good&&GC.GetAllocatedBytesForCurrentThread()==before,"R69 both ABI warmed atomic span emission zero allocation");
            Check(staging.AsSpan(0,7).SequenceEqual(windows?new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xca}:
                new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xfa}),"R69 SysV and Win64 volatile leaf argument encoding");
            Check(!FullBlockEmitter.TryEmit(body,new byte[11],out int shortLength,windows)&&shortLength==0,"R69 oversized atomic output discarded");
        }
    }
}
