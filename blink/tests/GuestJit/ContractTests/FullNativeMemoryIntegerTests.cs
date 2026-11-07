using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // R77 authored contracts, UNRUN. Parent wires Main and owns serial builds,
    // emission, native execution and real controller/concurrency qualification.
    static void CheckFullNativeMemoryInteger(byte* machine)
    {
        Check(FullBlockFrame.ByteSize == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(TranslationBlockHeader) == 160 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.Temporary3)).ToInt32() == 208,
            "R77 unchanged native ABI and operand staging slot");
        CheckFullMemoryIntegerClassifier();
        using (var fixture = new ThreadedGuestExecution.FullControllerFixture(interpreterOracle: true))
        {
            const ulong guest = ThreadedGuestExecution.FullControllerFixture.Base;
            fixture.Map(guest, 8192); fixture.Fill(guest, 8192);
            CheckFullMemoryIntegerOracle(fixture);
            CheckFullMemoryIntegerIndependent(machine);
            CheckFullMemoryIntegerBoundaries(machine);
            if (!fullOwnerScaffold.IsValid)
            {
                using var common = CreateFullScaffold(out var addresses);
                fullOwnerScaffold = addresses;
                try
                {
                    CheckFullMemoryIntegerOracle(fixture);
                    CheckFullMemoryIntegerIndependent(machine);
                    CheckFullMemoryIntegerBoundaries(machine);
                }
                finally { fullOwnerScaffold = default; }
            }
        }
        CheckFullMemoryIntegerMmu();
        CheckFullMemoryIntegerController();
        CheckFullMemoryIntegerEmission();
    }

    static JitInstruction MemoryIntegerClassify(byte[] bytes)
    {
        Check(FullMemoryIntegerLowering.TryClassify(Ip, bytes, out var i, out string? reason), reason ?? "R77 classification");
        Check(FullBlockEmitter.CanInlineMemoryInteger(i) && FullBlockEmitter.ValidateMemoryInteger(i), "R77 exact typed shape");
        return i;
    }

    static List<byte[]> MemoryIntegerEncodings()
    {
        var cases = new List<byte[]>();
        foreach (int w in new[] { 8, 16, 32, 64 })
        {
            foreach (int group in Enumerable.Range(0, 8))
            {
                cases.Add(IntegerEncoding(w, w == 8 ? (byte)0xc0 : (byte)0xc1, (byte)(3 | group << 3), 17));
                cases.Add(IntegerEncoding(w, w == 8 ? (byte)0xd0 : (byte)0xd1, (byte)(3 | group << 3)));
                cases.Add(IntegerEncoding(w, w == 8 ? (byte)0xd2 : (byte)0xd3, (byte)(3 | group << 3)));
            }
            foreach (int group in new[] { 4, 5, 6, 7 })
                cases.Add(IntegerEncoding(w, w == 8 ? (byte)0xf6 : (byte)0xf7, (byte)(3 | group << 3)));
            if (w == 8) continue;
            foreach (byte ext in new byte[] { 0xa4, 0xac }) cases.Add(IntegerEncoding(w, 0x0f, ext, 0x33, 17));
            foreach (byte ext in new byte[] { 0xa5, 0xad, 0xaf, 0xbc, 0xbd, 0xa3 }) cases.Add(IntegerEncoding(w, 0x0f, ext, 0x33));
            cases.Add(IntegerEncoding(w, 0x0f, 0xba, 0x23, 255));
            cases.Add(IntegerEncoding(w, 0x6b, 0x33, 0x80));
            cases.Add(w == 16 ? [0x66, 0x69, 0x33, 0x01, 0x80] : IntegerEncoding(w, 0x69, 0x33, 0x01, 0, 0, 0x80));
        }
        return cases;
    }

    static void CheckFullMemoryIntegerClassifier()
    {
        foreach (byte[] bytes in MemoryIntegerEncodings()) MemoryIntegerClassify(bytes);
        Check(MemoryIntegerClassify([0x64, 0x67, 0x48, 0x0f, 0xa5, 0x44, 0x8b, 0xf8]).Address ==
            new JitAddress(3, 1, 2, -8, false, 32, 4), "R77 shared FS EA32 SIB grammar");
        Check(MemoryIntegerClassify([0x65, 0x4f, 0x0f, 0xaf, 0x54, 0xcc, 0x80]).Address ==
            new JitAddress(12, 9, 3, -128, false, 64, 5), "R77 GS extended base/index/register");
        Check(MemoryIntegerClassify([0x67, 0x0f, 0xbc, 0x05, 0xf0, 0xff, 0xff, 0xff]).Address ==
            new JitAddress(-1, -1, 0, -16, true, 32), "R77 EIP relative grammar");
        Check(MemoryIntegerClassify([0x48, 0x69, 0x03, 0xff, 0xff, 0xff, 0xff]).Extra.Immediate == ulong.MaxValue &&
            MemoryIntegerClassify([0x66, 0x6b, 0x03, 0x80]).Extra.Immediate == unchecked((ulong)-128L), "R77 signed immediates");
        foreach (byte[] bytes in new byte[][] {
            [], [0x48], [0xf0,0xc1,0x23,1], [0xf2,0xf7,0x23], [0xf3,0x0f,0xbc,0x03],
            [0x66,0x66,0xc1,0x23,1], [0x64,0x65,0xd3,0x23], [0x67,0x67,0xd3,0x23],
            [0x48,0x66,0xd3,0x23], [0x48,0x40,0xd3,0x23], [0x44,0xf7,0x23], [0x44,0xd3,0x23],
            [0xd3,0xe3], [0xc1,0x23], [0xc1,0x23,1,0x90], [0xf7,0x13],
            [0x69,0x03,1], [0x0f,0xa4,0x44], [0x0f,0xa4,0x04,0x25,0,0,0,0] })
            Check(!FullMemoryIntegerLowering.TryClassify(Ip, bytes, out _, out _), "R77 explicit missing prefix/shape/length");
        var good = MemoryIntegerClassify([0x48,0x0f,0xa5,0x33]);
        foreach (var bad in new[] { good with { Width=8 }, good with { SourceWidth=32 }, good with { Target=1 },
            good with { Options=1 }, good with { Source=new(16,0) }, good with { Extra=new(2,0) },
            good with { Extra=new(0,256,true) }, good with { Destination=new(1,0,IsMemory:true) },
            good with { Address=new(-1,-1,4,0) }, good with { Address=new(0,-1,0,0,true) } })
        {
            bool rejected=false; try { FullBlockEmitter.ValidateMemoryInteger(bad); } catch (ArgumentException) { rejected=true; }
            Check(rejected,"R77 malformed IR rejected before native bytes");
        }
        Check(!FullBlockEmitter.CanInlineMemoryInteger(good with { Destination=new(3,0) }), "R77 register admission unchanged");
    }

    static void MemoryIntegerSeed(byte* machine)
    {
        // Keep initialized fixture ownership/bus/MMU fields intact.
        var m=(Blink.Machine*)machine; m->ip=Ip; m->oplen=0; m->attention=false; m->stashaddr=0;
        m->mode.omode=Blink.XED_MODE_LONG; m->mode.genmode=Blink.XED_GEN_MODE_PROTECTED;
        // Active TF/RF/AC now select the precise compatibility context.
        m->flags=0xa52c0e15u & ~0x50100u;
        for (int r=0;r<16;++r) Write64(machine,24+r*8,0xfedcba9876543210UL ^ (0x0101010101010101UL*(uint)r));
        Write64(machine,24,17); Write64(machine,40,0); Write64(machine,32,1);
    }

    static ulong MemoryIntegerAddress(JitInstruction i,byte* machine,ulong fs,ulong gs)
    {
        var a=i.Address;
        ulong value=unchecked((ulong)a.Displacement+(a.RipRelative?i.Ip+(uint)i.Length:0));
        if(a.Base>=0) value=unchecked(value+((ulong*)(machine+24))[a.Base]);
        if(a.Index>=0) value=unchecked(value+(((ulong*)(machine+24))[a.Index]<<a.Scale));
        if(i.Operation is JitOperation.BitTest or JitOperation.BitSet or JitOperation.BitReset or JitOperation.BitComplement)
        {
            if(!i.Source.IsImmediate)
            {
                ulong bits=((ulong*)(machine+24))[i.Source.Register]&IntegerMask(i.Width);
                long signed=i.Width==64?(long)bits:(long)(bits<<(64-i.Width))>>(64-i.Width);
                // BigInteger floor division independently gives word/byte displacement.
                BigInteger index=signed, width=i.Width;
                BigInteger words=BigInteger.DivRem(index,width,out var rem);
                if(rem<0) --words;
                value=unchecked(value+(ulong)(long)(words*(i.Width/8)));
            }
        }
        if(a.AddressWidth==32) value=(uint)value;
        value=unchecked(value+(a.Segment==4?fs:a.Segment==5?gs:0));
        return value;
    }

    static void MemoryIntegerRun(byte* machine,JitInstruction i,byte* first,byte* second,int firstLength,
        ulong fs=0,ulong gs=0,bool divideFault=false)
    {
        using var code=CreateFullBlock([new(i)]);
        ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop); f.FsBase=fs; f.GsBase=gs;
        byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
        ulong expectedAddress=MemoryIntegerAddress(i,machine,fs,gs);
        var access=i.Destination.IsMemory&&i.Operation!=JitOperation.BitTest?FullMemoryAccess.Read|FullMemoryAccess.Write:FullMemoryAccess.Read;
        Check(RunFull(code,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 && f.Executed==0 &&
            f.MemoryAddress==expectedAddress && f.MemoryAccess==access && f.MemoryWidth==i.Width && f.MemoryResume==1 &&
            new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before), "R77 mapping miss has exact request and no effects");
        FullNativeMemory.PublishBarrierTicket(&f,first,second,(uint)firstLength,epoch,123);
        FullNativeMemory.ResumeSameIssue(&f);
        var exit=RunFull(code,&f);
        if(divideFault)
            Check(exit==FullBlockExit.GuestFault && f.FaultTrap==0 && f.FaultCode==0 && f.FaultAddress==i.Ip &&
                issued==1 && completed==0 && f.Executed==0 && f.NativeMemoryComputations==0 &&
                new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before), "R77 staged precise divide fault before outputs");
        else
            Check(exit==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1 &&
                f.NativeMemoryComputations==1 && f.MemoryFlags==FullMemoryFlags.BarrierConsumed && f.MemoryTicket==0 &&
                f.FirstPreissued==0 && f.MemoryResume==0 && ((ulong*)machine)[0]==Ip+(uint)i.Length && machine[8]==0,
                "R77 native computation resumes same issue and commits once");
    }

    static void MemoryIntegerCompare(ThreadedGuestExecution.FullControllerFixture fixture,byte[] encoding,int offset,int count=1)
    {
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        byte* machine=(byte*)fixture.Machine(),p0=(byte*)fixture.Physical(guest),p1=(byte*)fixture.Physical(guest+4096);
        var i=MemoryIntegerClassify(encoding); MemoryIntegerSeed(machine);
        Write64(machine,48,guest+(uint)offset); Write64(machine,32,(uint)count); Write64(machine,72,5);
        for(int b=0;b<4096;++b) { p0[b]=(byte)(b*37+11); p1[b]=(byte)(b*19+7); }
        ulong address=MemoryIntegerAddress(i,machine,0,0);
        Check(address>=guest && address+(uint)(i.Width/8)<=guest+8192,"R77 oracle test address within fixture");
        int at=(int)(address-guest),n=Math.Min(i.Width/8,4096-at%4096);
        byte* first=at<4096?p0+at:p1+at-4096; byte* second=n==i.Width/8?null:p1;
        bool divide=i.Operation is JitOperation.DivideSigned or JitOperation.DivideUnsigned;
        ulong operand=divide?3UL:(1UL<<(i.Width-1))|0x13;
        AtomicWrite(first,second,n,0,i.Width,operand);
        byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
        byte[] b0=new ReadOnlySpan<byte>(p0,4096).ToArray(),b1=new ReadOnlySpan<byte>(p1,4096).ToArray();
        // The actual selected normal GetOp, with authoritative XED decode and
        // CommitStash, is legal only within this initialized fixture DataScope.
        fixture.DataScope(0,()=>IntegerReference(fixture.Program,(byte*)fixture.Machine(),encoding));
        byte[] expected=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
        byte[] e0=new ReadOnlySpan<byte>(p0,4096).ToArray(),e1=new ReadOnlySpan<byte>(p1,4096).ToArray();
        before.CopyTo(new Span<byte>(machine,before.Length)); b0.CopyTo(new Span<byte>(p0,4096)); b1.CopyTo(new Span<byte>(p1,4096));
        MemoryIntegerRun(machine,i,first,second,n);
        Check(new ReadOnlySpan<byte>(machine,9).SequenceEqual(expected.AsSpan(0,9)) &&
            new ReadOnlySpan<byte>(machine+12,4).SequenceEqual(expected.AsSpan(12,4)) &&
            new ReadOnlySpan<byte>(machine+24,128).SequenceEqual(expected.AsSpan(24,128)) &&
            new ReadOnlySpan<byte>(machine+160,256).SequenceEqual(expected.AsSpan(160,256)) &&
            new ReadOnlySpan<byte>(machine+456,236).SequenceEqual(expected.AsSpan(456,236)),
            $"R77 selected normal interpreter state {Convert.ToHexString(encoding)}/{offset}/{count}");
        Check(new ReadOnlySpan<byte>(p0,4096).SequenceEqual(e0) && new ReadOnlySpan<byte>(p1,4096).SequenceEqual(e1),
            "R77 entire memory pages, both sentinels and read-only sources");
    }

    static void CheckFullMemoryIntegerOracle(ThreadedGuestExecution.FullControllerFixture fixture)
    {
        foreach(byte[] encoding in MemoryIntegerEncodings())
        foreach(int offset in new[]{64,65,4095}) MemoryIntegerCompare(fixture,encoding,offset);
        foreach(int w in new[]{8,16,32,64})
        foreach(int group in Enumerable.Range(0,8))
        foreach(int count in new[]{0,2,7,8,9,15,16,17,31,32,33,63,64,65,255})
            MemoryIntegerCompare(fixture,IntegerEncoding(w,w==8?(byte)0xd2:(byte)0xd3,(byte)(3|group<<3)),4095,count);
        foreach(int w in new[]{16,32,64})
        foreach(byte ext in new byte[]{0xa5,0xad})
        foreach(int count in new[]{0,2,16,17,31,32,63,64,255})
            MemoryIntegerCompare(fixture,IntegerEncoding(w,0x0f,ext,0x0b),4095,count); // incoming register=CL register
        // Implicit RAX as EA base; destination aliases EA; extended destination.
        foreach(byte[] encoding in new byte[][] { [0x0f,0xbc,0x1b], [0x48,0x0f,0xaf,0x1b],
            [0x48,0x6b,0x1b,0x80], [0x4c,0x0f,0xbd,0x3b] }) MemoryIntegerCompare(fixture,encoding,65);
        // [RAX] cases set the implicit operand to the mapped address before
        // taking the oracle snapshot; the effective address must survive writes.
        MemoryIntegerAliasCompare(fixture,[0x48,0xd3,0x20]);
        MemoryIntegerAliasCompare(fixture,[0xf6,0x28]);
    }

    static void MemoryIntegerAliasCompare(ThreadedGuestExecution.FullControllerFixture fixture,byte[] encoding)
    {
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        byte* machine=(byte*)fixture.Machine(),data=(byte*)fixture.Physical(guest);
        var i=MemoryIntegerClassify(encoding); MemoryIntegerSeed(machine); Write64(machine,24,guest+65);
        new Span<byte>(data,4096).Fill(0xa5); data[65]=3;
        byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray(),original=new ReadOnlySpan<byte>(data,4096).ToArray();
        fixture.DataScope(0,()=>IntegerReference(fixture.Program,(byte*)fixture.Machine(),encoding));
        byte[] expected=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray(),eData=new ReadOnlySpan<byte>(data,4096).ToArray();
        before.CopyTo(new Span<byte>(machine,before.Length)); original.CopyTo(new Span<byte>(data,4096));
        MemoryIntegerRun(machine,i,data+65,null,i.Width/8);
        Check(new ReadOnlySpan<byte>(machine,9).SequenceEqual(expected.AsSpan(0,9)) &&
            new ReadOnlySpan<byte>(machine+12,4).SequenceEqual(expected.AsSpan(12,4)) &&
            new ReadOnlySpan<byte>(machine+24,128).SequenceEqual(expected.AsSpan(24,128)) &&
            new ReadOnlySpan<byte>(data,4096).SequenceEqual(eData),"R77 implicit accumulator/EA alias and byte high-half fidelity");
    }

    static BigInteger MemoryIntegerSigned(ulong value,int w)
    {
        BigInteger v=value&IntegerMask(w); return (value&(1UL<<(w-1)))!=0?v-(BigInteger.One<<w):v;
    }
    static ulong MemoryIntegerLow(BigInteger value,int w) => (ulong)(value&((BigInteger.One<<w)-1));

    static void CheckFullMemoryIntegerIndependent(byte* machine)
    {
        // Deliberately noncontiguous backing, with an entire sentinel page gap.
        byte* data=(byte*)NativeMemory.AllocZeroed(12288),second=data+8192;
        try
        {
            const ulong guest=0x100000;
            foreach(int w in new[]{8,16,32,64})
            foreach(bool signed in new[]{false,true})
            foreach(ulong operand in new[]{0UL,1UL,3UL,IntegerMask(w),(1UL<<(w-1))})
            {
                MemoryIntegerSeed(machine); Write64(machine,48,guest+4095);
                Write64(machine,24,0xdecafbad34560000UL|IntegerMask(w)); Write64(machine,40,0x876543210fedcba9);
                new Span<byte>(data,12288).Fill(0x5a); AtomicWrite(data+4095,second,1,0,w,operand);
                ulong ax=((ulong*)(machine+24))[0],dx=((ulong*)(machine+40))[0]; uint old=((uint*)(machine+12))[0];
                BigInteger x=signed?MemoryIntegerSigned(ax,w):ax&IntegerMask(w);
                BigInteger y=signed?MemoryIntegerSigned(operand,w):operand;
                BigInteger product=x*y,lo=MemoryIntegerLow(product,w),hi=MemoryIntegerLow(product>>w,w);
                bool overflow=signed?product!=MemoryIntegerSigned((ulong)lo,w):product>IntegerMask(w);
                var i=MemoryIntegerClassify(IntegerEncoding(w,w==8?(byte)0xf6:(byte)0xf7,(byte)(3|(signed?5:4)<<3)));
                MemoryIntegerRun(machine,i,data+4095,w==8?null:second,1);
                ulong expectedAx=w==8?(ax&~65535UL)|MemoryIntegerLow(product,16):w==16?(ax&~65535UL)|(ulong)lo:(ulong)lo;
                ulong expectedDx=w==8?dx:w==16?(dx&~65535UL)|(ulong)hi:w==32&&signed?
                    unchecked((ulong)(long)(int)(uint)hi):(ulong)hi;
                Check(((ulong*)(machine+24))[0]==expectedAx && ((ulong*)(machine+40))[0]==expectedDx &&
                    ((uint*)(machine+12))[0]==((old&~0x801u)|(overflow?0x801u:0)),"R77 independent full signed/unsigned product and CF/OF");
                Check(new ReadOnlySpan<byte>(data+4096,4096).IndexOfAnyExcept((byte)0x5a)<0,"R77 noncontiguous gap untouched");
            }
            foreach(int w in new[]{8,16,32,64})
            foreach(bool signed in new[]{false,true})
            foreach(var input in new (BigInteger Dividend,BigInteger Divisor)[] {
                (17,3),(-17,3),(17,-3),(-17,-3),(0,0),
                (BigInteger.One<<(w-1),-1),(-(BigInteger.One<<(w-1)),-1),
                (-(BigInteger.One<<(w-1)),1),(BigInteger.One<<w,1),
                ((BigInteger.One<<(2*w-1))-1,1),(-(BigInteger.One<<(2*w-1)),1) })
            {
                if(!signed&&(input.Dividend<0||input.Divisor<0)) continue;
                MemoryIntegerSeed(machine); Write64(machine,48,guest+4095);
                Write64(machine,24,0xdecafbad34560011); Write64(machine,40,0x876543210fedcba9);
                ulong oldAx=((ulong*)(machine+24))[0],oldDx=((ulong*)(machine+40))[0]; uint oldFlags=((uint*)(machine+12))[0];
                if(w==8) IntegerRegisterWrite(machine,new(0,0),16,MemoryIntegerLow(input.Dividend,16));
                else { IntegerRegisterWrite(machine,new(0,0),w,MemoryIntegerLow(input.Dividend,w));
                    IntegerRegisterWrite(machine,new(2,0),w,MemoryIntegerLow(input.Dividend>>w,w)); }
                oldAx=((ulong*)(machine+24))[0]; oldDx=((ulong*)(machine+40))[0];
                new Span<byte>(data,12288).Fill(0xa5); AtomicWrite(data+4095,second,1,0,w,MemoryIntegerLow(input.Divisor,w));
                BigInteger dividend=input.Dividend,divisor=input.Divisor;
                BigInteger q=0,r=0; bool fault=divisor==0;
                if(!fault) { q=BigInteger.DivRem(dividend,divisor,out r);
                    fault=q<(signed?-(BigInteger.One<<(w-1)):0)||q>(signed?(BigInteger.One<<(w-1))-1:(BigInteger.One<<w)-1); }
                var i=MemoryIntegerClassify(IntegerEncoding(w,w==8?(byte)0xf6:(byte)0xf7,(byte)(3|(signed?7:6)<<3)));
                byte[] beforeData=new ReadOnlySpan<byte>(data,12288).ToArray();
                MemoryIntegerRun(machine,i,data+4095,w==8?null:second,1,divideFault:fault);
                if(!fault)
                {
                    ulong expectedAx=w==8?(oldAx&~65535UL)|MemoryIntegerLow(q,8)|(MemoryIntegerLow(r,8)<<8):
                        w==16?(oldAx&~65535UL)|MemoryIntegerLow(q,w):MemoryIntegerLow(q,w);
                    ulong expectedDx=w==8?oldDx:w==16?(oldDx&~65535UL)|MemoryIntegerLow(r,w):MemoryIntegerLow(r,w);
                    Check(((ulong*)(machine+24))[0]==expectedAx&&((ulong*)(machine+40))[0]==expectedDx,"R77 independent quotient/remainder/sign/partial width");
                }
                Check(((uint*)(machine+12))[0]==oldFlags && new ReadOnlySpan<byte>(data,12288).SequenceEqual(beforeData),"R77 divide preserves all stored flags and source bytes");
            }
            CheckFullMemoryIntegerShiftMath(machine,data,second);
            CheckFullMemoryIntegerBitAddresses(machine,data);
        }
        finally { NativeMemory.Free(data); }
    }

    static void CheckFullMemoryIntegerShiftMath(byte* machine,byte* data,byte* second)
    {
        foreach(int w in new[]{8,16,32,64})
        foreach(int group in Enumerable.Range(0,8))
        foreach(int count in new[]{0,1,2,7,8,9,15,16,17,31,32,33,63,64,65,255})
        foreach(uint carry in new uint[]{0,1})
        {
            MemoryIntegerSeed(machine); Write64(machine,48,0x100fff); Write64(machine,32,(uint)count);
            uint old=0xa52c0e14u&~0x50100u; old|=carry; ((uint*)(machine+12))[0]=old;
            ulong original=(1UL<<(w-1))|0x13,mask=IntegerMask(w),result=original; uint cf=carry;
            int steps=count&(w==64?63:31); bool rotate=group<4;
            if(group is 2 or 3 && w<32) steps%=w+1;
            for(int n=0;n<steps;++n)
            {
                uint outgoing;
                if(group is 0 or 2 or 4 or 6)
                {
                    outgoing=(uint)(result>>(w-1));
                    result=((result<<1)&mask)|(group==0?outgoing:group==2?cf:0);
                }
                else
                {
                    outgoing=(uint)(result&1);
                    result=(result>>1)|((ulong)(group==1?outgoing:group==3?cf:group==7?(uint)(result>>(w-1)):0)<<(w-1));
                }
                cf=outgoing;
            }
            uint expected=old;
            if(steps!=0)
            {
                uint msb=(uint)(result>>(w-1)),next=(uint)(result>>(w-2))&1;
                uint of=group is 0 or 2 or 4 or 6?msb^cf:group==7?0:msb^next;
                uint replacement=cf|(of<<11);
                if(!rotate) replacement|=(uint)((result&255)<<24)|(result==0?64u:0)|(msb<<7);
                uint bits=rotate?0x801u:0xff0008d1u;
                expected=(old&~bits)|replacement;
            }
            new Span<byte>(data,12288).Fill(0xa5); AtomicWrite(data+4095,second,1,0,w,original);
            var i=MemoryIntegerClassify(IntegerEncoding(w,w==8?(byte)0xd2:(byte)0xd3,(byte)(3|group<<3)));
            MemoryIntegerRun(machine,i,data+4095,w==8?null:second,1);
            Check(AtomicRead(data+4095,second,1,0,w)==result && ((uint*)(machine+12))[0]==expected,
                "R77 independent repeated-bit shift/rotate/carry math and deterministic flags");
        }
        foreach(int w in new[]{16,32,64})
        foreach(bool right in new[]{false,true})
        foreach(int count in new[]{0,1,2,7,15,16,31,32,63,64})
        {
            int steps=count&(w==64?63:31); if(steps>w) continue;
            MemoryIntegerSeed(machine); Write64(machine,48,0x100fff); Write64(machine,32,(uint)count);
            ulong x=(1UL<<(w-1))|0x13,y=0x123456789abcdef0&IntegerMask(w),result=x;
            Write64(machine,72,y); uint old=((uint*)(machine+12))[0],cf=0;
            // Concatenate the two guest words in a BigInteger and take the
            // shifted destination window; this is independent of host encodings.
            if(steps!=0)
            {
                BigInteger pair=right?((BigInteger)y<<w)|x:((BigInteger)x<<w)|y;
                result=MemoryIntegerLow(right?pair>>steps:(pair<<steps)>>w,w);
                cf=(uint)(right?(x>>(steps-1))&1:(x>>(w-steps))&1);
            }
            uint expected=old;
            if(steps!=0)
            {
                uint of=steps==1?(uint)((x^result)>>(w-1)):0;
                expected=(old&~0xff0008d1u)|(uint)((result&255)<<24)|(result==0?64u:0)|
                    ((uint)(result>>(w-1))<<7)|cf|(of<<11);
            }
            new Span<byte>(data,12288).Fill(0xa5); AtomicWrite(data+4095,second,1,0,w,x);
            var i=MemoryIntegerClassify(IntegerEncoding(w,0x0f,right?(byte)0xad:(byte)0xa5,0x33));
            MemoryIntegerRun(machine,i,data+4095,second,1);
            Check(AtomicRead(data+4095,second,1,0,w)==result && ((uint*)(machine+12))[0]==expected,
                "R77 independent concatenated double-shift value/carry/flags");
        }
        foreach(int w in new[]{16,32,64})
        foreach(ulong value in new[]{0UL,1UL,0x8000UL,0x80000000UL,ulong.MaxValue})
        foreach(bool reverse in new[]{false,true})
        {
            MemoryIntegerSeed(machine); Write64(machine,48,0x100fff);
            uint old=((uint*)(machine+12))[0]; ulong input=value&IntegerMask(w); int expected=0;
            if(input!=0)
            {
                if(reverse) { ulong v=input; while((v>>=1)!=0) ++expected; }
                else { ulong v=input; while((v&1)==0) { ++expected; v>>=1; } }
            }
            ulong oldRsi=((ulong*)(machine+72))[0];
            new Span<byte>(data,12288).Fill(0xa5); AtomicWrite(data+4095,second,1,0,w,input);
            var i=MemoryIntegerClassify(IntegerEncoding(w,0x0f,reverse?(byte)0xbd:(byte)0xbc,0x33));
            MemoryIntegerRun(machine,i,data+4095,second,1);
            ulong expectedRsi=w==16?(oldRsi&~65535UL)|(uint)expected:(uint)expected;
            Check(((ulong*)(machine+72))[0]==expectedRsi && ((uint*)(machine+12))[0]==((old&~64u)|(input==0?64u:0)),
                "R77 independent scans, deterministic zero result, ZF-only and destination upper bits");
        }
    }

    static void CheckFullMemoryIntegerBitAddresses(byte* machine,byte* data)
    {
        foreach(int w in new[]{16,32,64})
        foreach(long index in new[]{-129L,-65,-64,-33,-17,-1,0,15,16,31,32,63,64,127})
        foreach(bool address32 in new[]{false,true})
        foreach(bool immediate in new[]{false,true})
        {
            MemoryIntegerSeed(machine); Write64(machine,48,0x100040); Write64(machine,72,unchecked((ulong)index));
            var i=MemoryIntegerClassify(immediate?IntegerEncoding(w,0x0f,0xba,0x23,(byte)index):IntegerEncoding(w,0x0f,0xa3,0x33));
            i=i with { Address=new(3,-1,0,0,AddressWidth:address32?32:64,Segment:4) };
            new Span<byte>(data,12288).Fill(0xa5); uint old=((uint*)(machine+12))[0];
            int bit=(int)(unchecked((ulong)index)&(uint)(w-1));
            ulong expected=(old&~1u)|((0xa5a5a5a5a5a5a5a5UL>>bit)&1);
            MemoryIntegerRun(machine,i,data,null,w/8,fs:0x100000000);
            Check(((uint*)(machine+12))[0]==expected && new ReadOnlySpan<byte>(data,12288).IndexOfAnyExcept((byte)0xa5)<0,
                "readonly BT signed displacement, immediate modulo and architectural EA32 high-FS preservation");
        }
    }

    static void CheckFullMemoryIntegerBoundaries(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AllocZeroed(12288);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(byte[] encoding in new byte[][] { [0x48,0xc1,0x23,0], [0x48,0xc1,0x13,0],
                [0x48,0x0f,0xa4,0x33,0], [0x48,0xf7,0x33], [0x48,0xf7,0x3b], [0x0f,0xbc,0x33], [0x0f,0xa3,0x33] })
            foreach(string failure in new[]{"first","second","readonly","epoch","context"})
            {
                var i=MemoryIntegerClassify(encoding); MemoryIntegerSeed(machine);
                Write64(machine,48,0x100fff); Write64(machine,72,0);
                new Span<byte>(data,12288).Fill(0x5a); new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                SetFullMemoryEntry(tlb,0x100000,data); SetFullMemoryEntry(tlb,0x101000,data+8192);
                ref var entry=ref tlb[FullNativeMemory.TlbIndex(failure=="second"?0x101000UL:0x100000UL,63)];
                if(failure is "first" or "second") entry=default;
                else if(failure=="readonly") entry.Permissions=0;
                else if(failure=="epoch") ++entry.AuthorityEpoch;
                else ++entry.Context;
                using var code=CreateFullBlock([new(i)]);
                ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop); f.MemoryTlb=tlb; f.MemoryTlbMask=63;
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 && f.Executed==0 &&
                    f.NativeMemoryComputations==0 && new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before) &&
                    new ReadOnlySpan<byte>(data,12288).IndexOfAnyExcept((byte)0x5a)<0,"R77 both-page mapping/permission checks precede every effect and #DE");
            }
            // A valid read-only TLB must still reject masked-zero memory RMW.
            foreach(byte[] encoding in new byte[][] { [0x48,0xc1,0x23,0], [0x48,0xd3,0x13], [0x48,0x0f,0xa4,0x33,0] })
            {
                var i=MemoryIntegerClassify(encoding); MemoryIntegerSeed(machine); Write64(machine,48,0x100040); Write64(machine,32,0);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear(); SetFullMemoryEntry(tlb,0x100000,data,FullMemoryAccess.Read);
                using var code=CreateFullBlock([new(i)]);
                ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop); f.MemoryTlb=tlb; f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory && (uint)f.MemoryAccess==3 && completed==0,"R77 count0 still requires selected write permission");
            }
            CheckFullMemoryIntegerMixedSites(machine,data,tlb);
            foreach(byte[] encoding in new byte[][] { [0x48,0xf7,0x23], [0x48,0xf7,0x33], [0x48,0xf7,0x3b],
                [0x0f,0xbc,0x33], [0x0f,0xa3,0x33] })
            {
                var i=MemoryIntegerClassify(encoding); MemoryIntegerSeed(machine); Write64(machine,48,0x100fff); Write64(machine,72,0);
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                SetFullMemoryEntry(tlb,0x100000,data,FullMemoryAccess.Read); SetFullMemoryEntry(tlb,0x101000,data+8192,FullMemoryAccess.Read);
                new Span<byte>(data,12288).Fill(0xa5); AtomicWrite(data+4095,data+8192,1,0,i.Width,3);
                using var code=CreateFullBlock([new(i)]);
                ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop); f.MemoryTlb=tlb; f.MemoryTlbMask=63;
                byte[] beforeData=new ReadOnlySpan<byte>(data,12288).ToArray();
                Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 && f.NativeMemoryComputations==1 &&
                    f.MemoryAccess==FullMemoryAccess.Read && f.MemoryResume==0 &&
                    new ReadOnlySpan<byte>(data,12288).SequenceEqual(beforeData),"R77 ordinary read-only TLB computes without ticket/write intent");
            }
            foreach(string poll in new[]{"budget","stop","attention"})
            {
                var i=MemoryIntegerClassify([0x48,0xf7,0x33]); MemoryIntegerSeed(machine); Write64(machine,48,0x100000);
                using var code=CreateFullBlock([new(i)]);
                ulong epoch=11; long issued=0,completed=0; int stop=poll=="stop"?1:0;
                if(poll=="attention") machine[11]=1;
                var h=FullHeader(Ip,code.Entry); var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:0);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                Check(RunFull(code,&f)==(poll=="stop"?FullBlockExit.Stop:poll=="attention"?FullBlockExit.Attention:FullBlockExit.Budget) &&
                    issued==(poll=="budget"?1:0) && completed==0 && f.MemoryResume==0 && f.NativeMemoryComputations==0 &&
                    new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"R77 cancellation/attention/budget precede memory/#DE");
            }
        }
        finally { NativeMemory.Free(data); }
    }

    static void CheckFullMemoryIntegerMixedSites(byte* machine,byte* data,FullMemoryTlbEntry* tlb)
    {
        // Stage state cannot leak to a following register operation; double
        // shift's184/192 scratch cannot overwrite the memory word at208.
        var first=MemoryIntegerClassify([0x48,0x0f,0xa4,0x33,3]);
        var second=new JitInstruction(Ip+5,3,JitOperation.ShiftLeft,64,new(7,0),new(0,1,true));
        using var code=CreateFullBlock([new(first),new(second)]);
        MemoryIntegerSeed(machine); Write64(machine,48,0x100040); Write64(machine,72,0x1234); Write64(machine,80,7);
        ((ulong*)(data+64))[0]=0x1111111111111111;
        new Span<FullMemoryTlbEntry>(tlb,64).Clear(); SetFullMemoryEntry(tlb,0x100000,data);
        ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop); f.MemoryTlb=tlb; f.MemoryTlbMask=63;
        Check(RunFull(code,&f)==FullBlockExit.Continue && issued==2 && completed==2 && f.NativeMemoryComputations==1 &&
            ((ulong*)(data+64))[0]==0x8888888888888888 && ((ulong*)(machine+80))[0]==14,
            "R77 mixed sites native TLB path, operand scratch and emission-local cleanup");
    }

    static void CheckFullMemoryIntegerMmu()
    {
        using var fixture=new ThreadedGuestExecution.FullControllerFixture();
        const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(guest); FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        var frame=new FullBlockFrame { MemoryTlb=tlb,MemoryTlbMask=63,MemoryResume=1,MemoryAddress=guest,MemoryAccess=FullMemoryAccess.Read,MemoryWidth=64 };
        Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.DemandPage,"R77 real reserved DIV source requests mapping-only materialization");
        fixture.Fill(guest); frame.MemoryAddress=guest+4095;
        foreach(var access in new[]{FullMemoryAccess.Read,FullMemoryAccess.Read|FullMemoryAccess.Write})
        {
            frame.MemoryAccess=access;
            Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.GuestFault && frame.FaultTrap==14 &&
                frame.FaultCode==1 && frame.FaultAddress==guest+4096,"R77 read or zero-count RMW unmapped second page before arithmetic");
        }
        fixture.Map(guest+4096); fixture.Fill(guest+4096);
        Check(fixture.Mutate("mprotect",guest+4096)==0,"R77 second page becomes inaccessible");
        frame.MemoryAddress=guest+4095; frame.MemoryAccess=FullMemoryAccess.Read|FullMemoryAccess.Write;
        Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.GuestFault && frame.FaultTrap==14 &&
            frame.FaultCode==2 && frame.FaultAddress==guest+4096,"R77 count0 shift second-page ACCERR before all outputs");
    }

    static void CheckFullMemoryIntegerEmission()
    {
        var i=MemoryIntegerClassify([0x48,0x0f,0xad,0x33]); FullBlockInstruction[] sites=[new(i)];
        byte[] destination=new byte[FullBlockEmitter.CodeCapacity],shortBuffer=new byte[1];
        foreach(bool windows in new[]{false,true})
        {
            int length=FullBlockEmitter.Emit(sites,destination,windowsAbi:windows);
            Check(length>0 && destination[6]==(windows?(byte)0xca:(byte)0xfa),"R77 SysV/Win64 volatile-register prologue encoding");
            bool rejected=false; try { FullBlockEmitter.Emit(sites,shortBuffer,windowsAbi:windows); } catch(ArgumentException) { rejected=true; }
            Check(rejected,"R77 too-small span rejected");
        }
        for(int n=0;n<8;++n) FullBlockEmitter.Emit(sites,destination);
        long before=GC.GetAllocatedBytesForCurrentThread();
        for(int n=0;n<32;++n) FullBlockEmitter.Emit(sites,destination);
        Check(GC.GetAllocatedBytesForCurrentThread()==before,"R77 warmed span emission allocates zero");
    }

    static void CheckFullMemoryIntegerController()
    {
        foreach(string fault in new[]{"divide-zero","signed-overflow","second-maperr","count0-accerr"})
        foreach(bool tail in new[]{false,true})
        {
            using var f=new ThreadedGuestExecution.FullControllerFixture();
            const ulong source=ThreadedGuestExecution.FullControllerFixture.Base;
            ulong target=tail?source+64:source,data=source+8192,address=data;
            f.Map(source); f.Fill(source); f.Map(data); f.Fill(data);
            if(fault is "second-maperr" or "count0-accerr") address=data+4095;
            if(fault=="count0-accerr")
            {
                f.Map(data+4096); f.Fill(data+4096);
                Check(f.Mutate("mprotect",data+4096)==0,"R77 controller second page protected");
            }
            byte[] body=fault=="count0-accerr"?[0x90,0x48,0xc1,0x23,0]:
                fault=="signed-overflow"?[0x90,0x48,0xf7,0x3b]:[0x90,0x48,0xf7,0x33];
            WriteControllerCode(f,target,body);
            if(tail)
            {
                WriteControllerCode(f,source,[0x90,0xeb,61]);
                Check(f.Acquire(target,0,out var lease,out bool multiple,out int error)&&error==0&&multiple,"R77 memory divide tail compiles");
                using(lease) lock(f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in lease,multiple);
            }
            f.DataScope(0,()=>
            {
                byte* p=f.Program.LookupAddress2(f.Machine(),(long)data,0,0);
                new Span<byte>(p,4096).Clear();
                if(fault=="signed-overflow") ((ulong*)p)[0]=ulong.MaxValue;
            });
            var m=f.Machine(); Write64((byte*)m,48,address);
            Write64((byte*)m,24,fault=="signed-overflow"?0x8000000000000000UL:17);
            Write64((byte*)m,40,fault=="signed-overflow"||fault=="second-maperr"?ulong.MaxValue:0);
            ulong ax=((ulong*)((byte*)m+24))[0],dx=((ulong*)((byte*)m+40))[0]; uint flags=m->flags;
            f.Counts(100);
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var signal=f.StepSynchronousFault(source);
            bool page=fault is "second-maperr" or "count0-accerr";
            Check(signal.Signal==(page?11:8)&&signal.Detail?.EndsWith(page?fault=="count0-accerr"?"signal=11 code=2":"signal=11 code=1":"signal=8 code=1")==true &&
                m->trapno==(page?14:0)&&m->ip==target+1&&m->oplen==0&&m->faultaddr==(long)(page?data+4096:target+1),
                $"R77 controller precise memory/#DE precedence {fault}/tail={tail}: {signal.Detail}");
            ulong done=tail?3UL:1UL;
            Check(f.Issued==done+1&&f.Completed==done&&m->flags==flags&&((ulong*)((byte*)m+24))[0]==ax&&((ulong*)((byte*)m+40))[0]==dx &&
                f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                "R77 actual controller stages faults after native return with exact accounting");
            var store=f.Jit.Store.GetStatistics();
            Check(store.ActiveNativeEpochs==0&&store.ActiveManagedLeases==0&&f.Statistics.ActiveDataBorrows==0,
                "R77 controller fault unwinds all retained/native/physical capabilities");
        }
        using(var f=new ThreadedGuestExecution.FullControllerFixture())
        {
            const ulong code=ThreadedGuestExecution.FullControllerFixture.Base; ulong data=code+4096;
            f.Map(code); f.Fill(code); f.Map(data); // leave the data demand page unmaterialized
            byte[] bytes=[0x48,0xbb,0,0,0,0,0,0,0,0,0x48,0xc1,0x23,1,0x0f,0xbc,0x03,0x31,0xc9,0x75,0];
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(2,8),data); WriteControllerCode(f,code,bytes); f.Counts(100);
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            Check(f.Step(code),"R77 lazy memory-integer initial cap returns");
            for(int n=0;n<8&&f.Machine()->ip!=code+(uint)bytes.Length;++n) Check(f.Continue(),"R77 lazy memory-integer resumes");
            Check(f.Machine()->ip==code+(uint)bytes.Length&&f.Issued==5&&f.Completed==5&&
                f.Statistics.NativeMemoryInstructions==2&&f.Statistics.NativeMemoryAssists>0&&
                f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0&&
                ((ulong*)((byte*)f.Machine()+24))[0]==0&&((ulong*)f.Physical(data))[0]==0,
                "R77 actual lazy mapping assist computes shift and scan only in native instructions");
        }
    }
}
