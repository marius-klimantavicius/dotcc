using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // AUTHORED_UNRUN. Parent owns integration, builds, emission and execution.
    // Production Active0/disabled classifier is never changed by these tests.
    static void CheckFullCanonicalX87Memory(byte* machine)
    {
        _ = machine;
        Check(sizeof(FullBlockFrame) == 416 && sizeof(FullX87NativeStorage) == 576 &&
            sizeof(FullX87RegisterFile) == 192 && sizeof(FullExtended80WorkFrame) == 384 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.X87)).ToInt32() == 400 &&
            FullBlockFrame.AbiVersion == 0x46420007 &&
            !RuntimeHelpers.IsReferenceOrContainsReferences<FullX87KernelCall>() &&
            !RuntimeHelpers.IsReferenceOrContainsReferences<FullBlockInstruction>(), "R150 unchanged ABI7 and pure-value IR");
        MemoryX87Decode(); MemoryX87ReferenceAnchors(); MemoryX87ProbeMmu();
        using var io = new SemanticTestIo();
        using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        using var catalog = new FullX87KernelCatalog(authority, store, true);
        var calls = new FullX87KernelCall[32];
        FullX87KernelBinding saved;
        lock (authority.Gate)
        {
            Check(catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes], out _) && catalog.Count == FullX87KernelCatalog.MaximumKernels,
                "R150 all32 eager native catalog publication");
            foreach (var form in MemoryX87Forms())
            {
                var i = MemoryX87Classify([form.Opcode, (byte)(form.Group << 3)]);
                var key = FullBlockEmitter.CanonicalX87MemoryKernel(i);
                var binding = catalog.Find(key);
                Check(catalog.IsValid(in binding), "R150 same-Gate authenticate typed RX binding before embedding Entry");
                calls[(int)key] = new((byte)key, binding.Entry, 73, binding.CodeBytes);
            }
            saved = catalog.Find(FullX87KernelKey.LoadRaw80);
        }
        var token = RetainedPublish(store, 0x500000);
        Check(store.TryAcquireToken(token, out var root),"R150 original TB root retained through all continuations");
        Check(store.TryBeginNativeEpoch(out var epochLease),"R150 original TB native epoch retained through all continuations");
        bool enrolled;
        lock (authority.Gate) enrolled = catalog.AdmissionOpen && authority.TryEnterNative(authority.Epoch);
        Check(enrolled, "R150 original authority native enrollment spans kernel and continuation");
        byte* fake = (byte*)NativeMemory.AlignedAlloc((nuint)((sizeof(Blink.Machine) + 15) & ~15), 16);
        var state = (FullX87RegisterFile*)NativeMemory.AlignedAlloc(576, 16);
        byte* pages = (byte*)NativeMemory.AllocZeroed(12288);
        try
        {
            Check(fake != null && state != null && pages != null && !fullOwnerScaffold.IsValid,
                "R150 isolated576 canonical owner and discontiguous test pages");
            MemoryX87Suite(fake, state, pages, calls);
            using (var common = CreateFullScaffold(out var addresses))
            {
                fullOwnerScaffold = addresses;
                try { MemoryX87Suite(fake, state, pages, calls); }
                finally { fullOwnerScaffold = default; }
            }
            MemoryX87Encodings(calls);
        }
        finally
        {
            NativeMemory.Free(pages); NativeMemory.AlignedFree(state); NativeMemory.AlignedFree(fake);
            if (enrolled) authority.ExitNative();
            epochLease.Dispose(); root.Dispose();
        }
        lock (authority.Gate)
        {
            Check(catalog.IsValid(in saved), "R150 body disposal never releases catalog code");
            catalog.CloseAdmission();
            Check(!catalog.IsValid(in saved), "R150 no closed binding may authorize a later native call");
        }
        store.Dispose(); catalog.Dispose();
        Check(catalog.ChargedBytes == 0, "R150 successful Store/native lifetime barrier precedes final catalog unmap");
        bool dead = false; try { lock (authority.Gate) _ = saved.Entry; } catch (InvalidOperationException) { dead = true; }
        Check(dead, "R150 no Entry recovery/call after final unmap");
    }

    static void MemoryX87ProbeMmu()
    {
        foreach (int width in new[] {16,32,64,80})
        foreach (string kind in new[] {"ok","firstmissing","secondmissing","readonly","eof","lazy"})
        {
            using var fixture=new ThreadedGuestExecution.FullControllerFixture();
            const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
            if (kind!="firstmissing") { fixture.Map(guest);fixture.Fill(guest); }
            if (kind is "ok" or "readonly") { fixture.Map(guest+4096);fixture.Fill(guest+4096); }
            if (kind=="readonly") Check(fixture.Mutate("mprotect",guest+4096)==0,"R150 probe readonly second page setup");
            if (kind=="lazy") fixture.Map(guest+4096);
            if (kind=="eof")
            {
                var file=fixture.Io.CreateMemoryFile("r150-probe-eof",0);
                Check(file.Succeeded&&fixture.Io.TruncateFile(file.Value,4096).Succeeded,"R150 file-backed probe setup");
                fixture.MapShared(file.Value,guest+4096);fixture.Fill(guest+4096);
                Check(fixture.Io.TruncateFile(file.Value,0).Succeeded,"R150 probe EOF withdrawal");
            }
            var descriptor=new FullStackMemoryDescriptor {
                Version=FullCanonicalX87MemoryLowering.MemoryProbeTag,Count=1,State=FullStackMemoryState.Request,
                Request0=new(){Address=guest+4095,Access=FullMemoryAccess.Write,Width=(uint)width}};
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            // Poisoned entries prove that a successful proof publishes no
            // native WRITE authority or mapping pointer into the ordinary TLB.
            tlb[7].Host=(byte*)1;tlb[7].Permissions=FullMemoryAccess.Read;
            byte[] tlbBefore=CanonicalX87Bytes((byte*)tlb,64*64);
            var frame=StackMemoryMmuRequest(&descriptor,tlb);frame.MemoryAccess=FullMemoryAccess.Write;
            frame.Machine=fixture.Machine();
            Check(FullNativeMemory.IsCanonicalX87MemoryProbe(&frame),"R150 parent exact tagged permission-only dispatch");
            var result=FillFullMemoryMmu(fixture,&frame);
            var expected=kind=="ok"?FullMemoryAssistResult.Resolved:kind=="lazy"?FullMemoryAssistResult.DemandPage:FullMemoryAssistResult.GuestFault;
            Check(result==expected,"R150 actual parent probe PTE/RW/both-page/EOF/lazy resolver");
            Check(descriptor.Request0.Host0==null&&descriptor.Request0.Host1==null&&descriptor.Request0.Length0==0&&
                descriptor.Request0.Reserved36==0&&descriptor.Request0.Reserved40==0&&descriptor.Request1.Address==0&&
                descriptor.Request1.Host0==null&&descriptor.Request1.Host1==null&&descriptor.Ticket==0&&
                new ReadOnlySpan<byte>(tlb,64*64).SequenceEqual(tlbBefore)&&frame.MemoryFlags==FullMemoryFlags.None,
                "R150 probe publishes no host pointers/TLB write permission/ticket and never requests code-alias revocation");
            if (kind=="ok") Check(descriptor.State==FullStackMemoryState.Ready&&descriptor.Epoch==frame.ObservedGlobalEpoch,
                "R150 only Ready epoch is published as mapping proof");
            else if (kind!="lazy") Check(frame.FaultTrap==14&&frame.FaultAddress==(kind=="firstmissing"?guest+4095:guest+4096)&&
                frame.FaultCode==(kind=="readonly"?2u:kind=="eof"?unchecked((uint)-2):1u)&&descriptor.State!=FullStackMemoryState.Ready,
                "R150 exact failing permission/EOF page wins over deferred x87 exception");
        }
    }

    readonly record struct MemoryX87Form(byte Opcode, int Group, int Width, bool Load, bool Integer, bool Pop, bool Truncate = false);
    static IEnumerable<MemoryX87Form> MemoryX87Forms()
    {
        // Independent documented encoding rows, not product enum arithmetic.
        yield return new(0xd9,0,32,true,false,false); yield return new(0xdd,0,64,true,false,false);
        yield return new(0xdb,5,80,true,false,false);
        yield return new(0xdf,0,16,true,true,false); yield return new(0xdb,0,32,true,true,false);
        yield return new(0xdf,5,64,true,true,false);
        yield return new(0xd9,2,32,false,false,false); yield return new(0xdd,2,64,false,false,false);
        yield return new(0xd9,3,32,false,false,true); yield return new(0xdd,3,64,false,false,true);
        yield return new(0xdb,7,80,false,false,true);
        yield return new(0xdf,2,16,false,true,false); yield return new(0xdb,2,32,false,true,false);
        yield return new(0xdf,3,16,false,true,true); yield return new(0xdb,3,32,false,true,true);
        yield return new(0xdf,7,64,false,true,true);
        yield return new(0xdf,1,16,false,true,true,true); yield return new(0xdb,1,32,false,true,true,true);
        yield return new(0xdd,1,64,false,true,true,true);
    }
    static JitInstruction MemoryX87Classify(ReadOnlySpan<byte> bytes)
    {
        Check(FullCanonicalX87MemoryLowering.TryClassify(Ip,bytes,out var i,out var why),why ?? "R150 exact memory grammar");
        Check(FullBlockEmitter.ValidateCanonicalX87Memory(i),"R150 typed memory IR admission");
        return i;
    }
    static FullX87KernelCall MemoryX87Call(JitInstruction i, FullX87KernelCall[] calls) =>
        calls[(int)FullBlockEmitter.CanonicalX87MemoryKernel(i)];

    static void MemoryX87Decode()
    {
        int rows = 0;
        foreach (var form in MemoryX87Forms())
        {
            ++rows;
            foreach (byte[] prefix in new byte[][] { [], [0x66], [0x67], [0x64,0x4f], [0x65,0x67,0x4f],
                [0x4f,0x66,0x40], [0x64,0x65,0x66,0x66,0x67,0x67,0x4f,0x40] })
            for (int mod = 0; mod < 3; ++mod)
            for (int rm = 0; rm < 8; ++rm)
            {
                var bytes = new List<byte>(prefix) { form.Opcode,(byte)(mod << 6 | form.Group << 3 | rm) };
                if (rm == 4) bytes.Add(0xcc); // [rsp/r12 + rcx/r9*8]
                if (mod == 1) bytes.Add(0x80);
                if (mod == 2 || mod == 0 && rm == 5) bytes.AddRange([0x78,0x56,0x34,0x12]);
                var i = MemoryX87Classify(bytes.ToArray());
                Check(i.SourceWidth == form.Width && i.Width == 80 && i.Source.IsMemory == form.Load &&
                    i.Destination.IsMemory != form.Load && ((i.Options >> 16 & 1) != 0) == form.Pop &&
                    i.Extra.Immediate == (ulong)((form.Opcode & 7) << 8 | mod << 6 | form.Group << 3 | rm),
                    "R150 all19 rows/full FOP/ignored66+REX.W/address modes");
                foreach (var bad in new[] { i with { Width=64 }, i with { SourceWidth=128 }, i with { Options=i.Options|0x20000 },
                    i with { Extra=i.Extra with { Immediate=i.Extra.Immediate|0x800 } }, i with { Target=1 },
                    i with { Source=form.Load?default:new(1,0) }, i with { Condition=1 },
                    i with { Address=i.Address with { Scale=4 } } })
                {
                    bool rejected=false; try { FullBlockEmitter.ValidateCanonicalX87Memory(bad); } catch (ArgumentException) { rejected=true; }
                    Check(rejected,"R150 malformed recognized IR cannot publish");
                }
            }
        }
        Check(rows == 19,"R150 documented19 encoding rows, not15 kernel domains conflated with encodings");
        Check(MemoryX87Classify([0x65,0x67,0x4f,0xdb,0x6c,0xcc,0x80]).Address == new JitAddress(12,9,3,-128,false,32,5),
            "R150 GS AS32 extended base/index SIB");
        Check(MemoryX87Classify([0x4f,0x66,0xdb,0x6c,0xcc,0x80]).Address == new JitAddress(4,1,3,-128),
            "R150 legacy after REX cancels B/X/W");
        Check(MemoryX87Classify([0x67,0xd9,0x05,0xfc,0xff,0xff,0xff]).Address == new JitAddress(-1,-1,0,-4,true,32),
            "R150 EIP-relative, not absolute disp32");
        foreach (byte[] bad in new byte[][] { [], [0xdb], [0xd9,0x04], [0xdb,0x6c,0xcc], [0xdb,0x2b,0],
            [0xf0,0xdb,0x2b], [0xf2,0xdb,0x2b], [0xf3,0xdb,0x2b], [0x9b,0xdb,0x2b],
            [0xd9,0xc0], [0xdf,0xc0], [0xdb,0x20], [0xdb,0x30], [0xdd,0x28], [0xdf,0x20],
            [0xdf,0x30], [0xd8,0x00], [0xda,0x00], [0xdc,0x00], [0xde,0x00], [0xd9,0x20] })
            Check(!FullCanonicalX87MemoryLowering.TryClassify(Ip,bad,out _,out var why) && why != null,
                "R150 named prefix/alias/BCD/arithmetic/environment/length exclusions");
    }

    readonly record struct MemoryX87Numeric(byte[] Image, uint Delta, bool Suppress);
    static byte[] MemoryX87Raw(ulong significand, ushort exponent)
    {
        byte[] result = new byte[10]; BinaryPrimitives.WriteUInt64LittleEndian(result,significand);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(8),exponent); return result;
    }
    static byte[] MemoryX87Packed(ulong value, int width)
    {
        byte[] result = new byte[10]; BinaryPrimitives.WriteUInt64LittleEndian(result,value);
        if (width < 64) Array.Clear(result,width/8,10-width/8); return result;
    }
    // Exact rational oracle. It uses DivRem on numerator/denominator, no host
    // float/double, product numeric helper, emitter algorithm or native kernel.
    static BigInteger MemoryX87Round(BigInteger n, int power, int quantum, int rc, bool negative, out bool inexact, out bool up)
    {
        int shift = power-quantum;
        BigInteger numerator = shift >= 0 ? n << shift : n;
        BigInteger denominator = shift >= 0 ? BigInteger.One : BigInteger.One << -shift;
        BigInteger q = BigInteger.DivRem(numerator,denominator,out var remainder);
        inexact = !remainder.IsZero;
        up = inexact && (rc == 0 ? remainder*2 > denominator || remainder*2 == denominator && !q.IsEven :
            rc == 1 ? negative : rc == 2 && !negative);
        return up ? q+1 : q;
    }
    static MemoryX87Numeric MemoryX87Reference(MemoryX87Form form, byte[] input, ushort cw)
    {
        if (form.Width == 80) return new(input.AsSpan(0,10).ToArray(),0,false);
        int rc = form.Truncate ? 3 : cw >> 10 & 3;
        if (form.Load && form.Integer)
        {
            BigInteger v = new(input.AsSpan(0,form.Width/8),isUnsigned:false,isBigEndian:false);
            bool sign = v.Sign < 0; v = BigInteger.Abs(v);
            if (v.IsZero) return new(MemoryX87Raw(0,0),0,false);
            int bits = (int)v.GetBitLength();
            return new(MemoryX87Raw((ulong)(v << (64-bits)),(ushort)(16382+bits+(sign?0x8000:0))),0,false);
        }
        if (form.Load)
        {
            ulong b = BinaryPrimitives.ReadUInt64LittleEndian(input);
            int f = form.Width == 32 ? 23 : 52, bias = form.Width == 32 ? 127 : 1023;
            ulong max = form.Width == 32 ? 255UL : 2047UL;
            bool sign = (b >> (form.Width-1) & 1) != 0;
            int e = (int)(b >> f & max); ulong frac = b & ((1UL << f)-1); uint delta = 0;
            ulong sig; int exponent;
            if (e == (int)max)
            {
                if (frac != 0 && (frac & (1UL << (f-1))) == 0) { delta=1; frac |= 1UL << (f-1); }
                sig = 0x8000000000000000UL | frac << (63-f); exponent=0x7fff;
            }
            else if (e == 0 && frac == 0) { sig=0; exponent=0; }
            else
            {
                BigInteger n = e == 0 ? frac : (1UL << f)|frac;
                int bits=(int)n.GetBitLength(); sig=(ulong)(n << (64-bits));
                exponent=16383+(e==0?1-bias:e-bias)-f+bits-1;
                if (e == 0) delta=2;
            }
            return new(MemoryX87Raw(sig,(ushort)(exponent|(sign?0x8000:0))),delta,delta==1&&(cw&1)==0);
        }
        ulong s=BinaryPrimitives.ReadUInt64LittleEndian(input); ushort se=BinaryPrimitives.ReadUInt16LittleEndian(input.AsSpan(8));
        bool negative=(se&0x8000)!=0; int exp=se&0x7fff;
        bool unsupported=exp!=0&&(s&0x8000000000000000UL)==0;
        bool special=exp==0x7fff, invalid=unsupported || form.Integer&&special;
        int fraction=form.Width==32?23:52, exponentBits=form.Width==32?8:11;
        ulong signBit=negative?1UL<<(form.Width-1):0;
        if (invalid)
        {
            ulong indefinite=form.Integer?1UL<<(form.Width-1):
                form.Width==32?0xffc00000UL:0xfff8000000000000UL;
            return new(MemoryX87Packed(indefinite,form.Width),1,(cw&1)==0);
        }
        if (special)
        {
            ulong fraction80=s&0x7fffffffffffffffUL;
            uint delta=fraction80!=0&&(s&0x4000000000000000UL)==0?1u:0u;
            ulong bits=signBit|((1UL<<exponentBits)-1)<<fraction;
            if (fraction80!=0) bits|=(s>>(63-fraction)&((1UL<<fraction)-1))|1UL<<(fraction-1);
            return new(MemoryX87Packed(bits,form.Width),delta,delta!=0&&(cw&1)==0);
        }
        if (s==0) return new(MemoryX87Packed(form.Integer?0:signBit,form.Width),0,false);
        BigInteger n80=s; int power=(exp==0?1-16383:exp-16383)-63;
        if (form.Integer)
        {
            BigInteger q=MemoryX87Round(n80,power,0,rc,negative,out bool lost,out bool up);
            BigInteger signed=negative?-q:q, bound=BigInteger.One<<(form.Width-1);
            if (signed < -bound || signed >= bound)
                return new(MemoryX87Packed(1UL<<(form.Width-1),form.Width),1,(cw&1)==0);
            if (signed.Sign<0) signed+=BigInteger.One<<form.Width;
            return new(MemoryX87Packed((ulong)signed,form.Width),lost?32u|(up?512u:0):0,false);
        }
        int emin=form.Width==32?-126:-1022, emax=form.Width==32?127:1023, bias2=emax;
        int unbiased=(int)n80.GetBitLength()-1+power;
        BigInteger rounded=MemoryX87Round(n80,power,unbiased-fraction,rc,negative,out bool inexact,out bool increase);
        if (rounded == BigInteger.One<<(fraction+1)) { rounded>>=1; ++unbiased; }
        uint status; ulong output; bool tiny=unbiased<emin;
        if (unbiased>emax)
        {
            bool infinity=rc==0||rc==1&&negative||rc==2&&!negative;
            output=signBit|(infinity?((1UL<<exponentBits)-1)<<fraction:
                (((1UL<<exponentBits)-2)<<fraction)|((1UL<<fraction)-1));
            status=40u|(infinity?512u:0);
            return (cw&8)==0 ? new(MemoryX87Packed(output,form.Width),8,true):
                new(MemoryX87Packed(output,form.Width),status,false);
        }
        if (tiny)
        {
            rounded=MemoryX87Round(n80,power,emin-fraction,rc,negative,out inexact,out increase);
            output=signBit|(ulong)rounded; status=inexact?48u|(increase?512u:0):0;
            if ((cw&16)==0) return new(MemoryX87Packed(output,form.Width),16,true);
        }
        else
        {
            output=signBit|(ulong)(unbiased+bias2)<<fraction|((ulong)rounded&((1UL<<fraction)-1));
            status=inexact?32u|(increase?512u:0):0;
        }
        return new(MemoryX87Packed(output,form.Width),status,false);
    }

    static void MemoryX87ReferenceAnchors()
    {
        var fld32=new MemoryX87Form(0xd9,0,32,true,false,false);
        var fild64=new MemoryX87Form(0xdf,5,64,true,true,false);
        var fst32=new MemoryX87Form(0xd9,3,32,false,false,true);
        var fist64=new MemoryX87Form(0xdf,7,64,false,true,true);
        Check(MemoryX87Reference(fild64,MemoryX87Packed(0x0020000000000001,64),0x37f).Image.AsSpan().SequenceEqual(
            MemoryX87Raw(0x8000000000000400,0x4034)),"R150 exact >binary64 integer load literal");
        var direct=MemoryX87Reference(fst32,MemoryX87Raw(0x8000008000000001,0x3fff),0x37f);
        Check(BinaryPrimitives.ReadUInt32LittleEndian(direct.Image)==0x3f800001 && direct.Delta==544 && !direct.Suppress,
            "R150 direct80-to32 double-rounding counterexample literal");
        var tiny=MemoryX87Reference(fst32,MemoryX87Raw(0x8000000000000000,0x3f6a),0x36f);
        Check(tiny.Delta==16&&tiny.Suppress,"R150 exact tiny unmasked UE suppresses store literal");
        var denormal=MemoryX87Reference(fld32,MemoryX87Packed(1,32),0x37d);
        Check(denormal.Delta==2&&!denormal.Suppress,"R150 FLD DM-clear denormal still pushes literal");
        var invalid=MemoryX87Reference(fist64,MemoryX87Raw(0xffffffffffffffff,0x403d),0x37f);
        Check(invalid.Delta==1&&BinaryPrimitives.ReadUInt64LittleEndian(invalid.Image)==0x8000000000000000,
            "R150 integer post-round range IE replaces provisional PE/C1 literal");
    }

    static FullExtended80WorkFrame* MemoryX87Work(FullX87RegisterFile* state) => (FullExtended80WorkFrame*)((byte*)state+192);
    static void MemoryX87Seed(byte* fake,FullX87RegisterFile* state,int top,ushort cw)
    {
        new Span<byte>(state,576).Fill(0xa5); CanonicalX87Seed(fake,state,top,cw);
        ((Blink.Machine*)fake)->fpu.sw=0x5a004740u|(uint)top<<11;
        ((Blink.Machine*)fake)->fpu.tw=unchecked((int)0x12340000);
        for (int selector=0;selector<6;++selector) SetScalarMiscSelector((Blink.Machine*)fake,selector,(ushort)(0x9001+selector*0x111));
        MemoryX87Work(state)->Continuation=0;
        ((uint*)((byte*)MemoryX87Work(state)+280))[0]=0;
    }
    static void MemoryX87Tag(byte* fake,FullX87RegisterFile* state,int logical,int tag,byte[]? raw=null)
    {
        int physical=((int)(((Blink.Machine*)fake)->fpu.sw>>11)+logical)&7;
        uint tw=unchecked((uint)((Blink.Machine*)fake)->fpu.tw);
        ((Blink.Machine*)fake)->fpu.tw=unchecked((int)((tw&~(3u<<(physical*2)))|(uint)tag<<(physical*2)));
        if (raw!=null) raw.AsSpan(0,10).CopyTo(new Span<byte>(FullX87RegisterFile.Physical(state,physical),10));
    }
    static int MemoryX87TagFor(byte[] raw)
    {
        ulong s=BinaryPrimitives.ReadUInt64LittleEndian(raw); int e=BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(8))&0x7fff;
        return e==0&&s==0?1:e>0&&e<0x7fff&&(s>>63)!=0?0:2;
    }

    static (uint Delta,bool Write) MemoryX87Expected(MemoryX87Form form,JitInstruction i,byte[] input,byte[] m,byte[] raw,
        byte[] pages,int offset,ulong address)
    {
        uint sw=CanonicalX87U32(m,648),tw=CanonicalX87U32(m,652); ushort cw=(ushort)CanonicalX87U32(m,672);
        int top=(int)(sw>>11)&7, physical=form.Load?(top+7)&7:top;
        bool empty=((tw>>(physical*2))&3)==3;
        bool stack=form.Load?!empty:empty;
        MemoryX87Numeric outcome;
        if (stack)
        {
            byte[] indefinite=form.Load||form.Width==80?MemoryX87Raw(0xc000000000000000,0xffff):
                form.Integer?MemoryX87Packed(1UL<<(form.Width-1),form.Width):
                MemoryX87Packed(form.Width==32?0xffc00000UL:0xfff8000000000000UL,form.Width);
            outcome=new(indefinite,form.Load?0x241u:0x41u,(cw&1)==0);
        }
        else outcome=MemoryX87Reference(form,input,cw);
        CanonicalX87W64(m,664,i.Ip); CanonicalX87W32(m,656,(uint)i.Extra.Immediate); CanonicalX87W64(m,680,address);
        BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40),BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(472)));
        int seg=i.Address.Segment>=0?i.Address.Segment:FullScalarMiscLowering.UsesStackSegment(i.Address)?2:3;
        BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(42),BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(456+seg*16)));
        sw=(sw&~0x200u)|outcome.Delta;
        if ((outcome.Delta&~cw&63)!=0) sw|=0x8080;
        if (!outcome.Suppress)
        {
            if (form.Load)
            {
                outcome.Image.AsSpan(0,10).CopyTo(raw.AsSpan(64+physical*16,10));
                tw=(tw&~(3u<<(physical*2)))|(uint)MemoryX87TagFor(outcome.Image)<<(physical*2);
                sw=(sw&~0x3800u)|(uint)physical<<11;
            }
            else
            {
                outcome.Image.AsSpan(0,form.Width/8).CopyTo(pages.AsSpan(offset,form.Width/8));
                if (form.Pop) { tw|=3u<<(top*2); sw=(sw&~0x3800u)|(uint)((top+1)&7)<<11; }
            }
        }
        CanonicalX87W32(m,648,sw); CanonicalX87W32(m,652,tw);
        CanonicalX87W64(m,0,i.Ip+(uint)i.Length); m[8]=0;
        return (outcome.Delta,!outcome.Suppress);
    }

    static void MemoryX87Suite(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        MemoryX87Numbers(fake,state,pages,calls);
        MemoryX87Addresses(fake,state,pages,calls);
        MemoryX87Misses(fake,state,pages,calls);
        MemoryX87Canonical(fake,state,calls);
        MemoryX87ProofFailures(fake,state,calls);
        MemoryX87Boundaries(fake,state,pages,calls);
        MemoryX87Deferred(fake,state,pages,calls);
        MemoryX87BadProtocol(fake,state);
    }

    static void MemoryX87BadProtocol(byte* fake,FullX87RegisterFile* state)
    {
        foreach ((uint admission,uint returned,uint metadata,uint status) in new (uint,uint,uint,uint)[] {
            (1,1,0,0),(0,1,1,0),(1,0,1,0),(0,0,0,0),(0,0,0x1001,0),(0,0,1,0x1000) })
        {
            // Intentional invalid TEST kernel; never published in real catalog.
            // It destroys R10, preserves the protocol's R11=Work return base.
            var bytes=new List<byte>{0xf3,0x0f,0x1e,0xfa};
            void Put(int offset,uint value)
            {
                bytes.AddRange([0x41,0xc7,0x83]);
                for (int k=0;k<4;++k) bytes.Add((byte)((uint)offset>>(8*k)));
                for (int k=0;k<4;++k) bytes.Add((byte)(value>>(8*k)));
            }
            Put(12,admission);Put(80,status);Put(84,metadata);
            bytes.AddRange([0x49,0xba,0,0,0,0,0,0,0,0,0xb8]);
            for (int k=0;k<4;++k) bytes.Add((byte)(returned>>(8*k)));
            bytes.AddRange([0x41,0xff,0x63,88]);
            Check(NativeCodeAllocation.TryCreate(bytes.ToArray(),out var kernel,out var why),why);
            using (kernel)
            foreach (var form in MemoryX87Forms())
            {
                if (form.Load) continue;
                var i=MemoryX87Classify([form.Opcode,(byte)(form.Group<<3|3)]);
                var call=new FullX87KernelCall((byte)FullBlockEmitter.CanonicalX87MemoryKernel(i),kernel!.Entry,73,bytes.Count);
                using var code=CreateFullBlock([new(i,X87Kernel:call)]);
                MemoryX87Seed(fake,state,2,0x37f);MemoryX87Tag(fake,state,0,0,MemoryX87Raw(0x8000000000000000,0x3fff));
                Write64(fake,48,0x100080);
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                ulong epoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);f.X87=state;f.FirstPreissued=1;
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;f.StackMemory=(FullStackMemoryDescriptor*)1;
                Check(FloatRunProbe(code,&f)==FullBlockExit.Invalidated&&issued==1&&completed==0&&f.Executed==0&&
                    f.FirstPreissued==1&&f.FaultTrap==0&&((uint*)((byte*)MemoryX87Work(state)+280))[0]==0x58383746&&
                    ((uint*)((byte*)MemoryX87Work(state)+284))[0]!=0&&MemoryX87Work(state)->Continuation==0,
                    "R150 malformed conversion protocol restores caller then returns internal fatal with SAME issue; no memory/CPU fault/fallback");
                CanonicalX87Same(fake,state,before,raw);
            }
        }
    }

    static void MemoryX87Canonical(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryX87Forms())
        foreach (int register in new[] {3,4,5,12,13})
        foreach (int segment in new[] {-1,2,3,4,5})
        {
            var bytes=new List<byte>();
            if (segment>=0) bytes.Add(segment switch {2=>(byte)0x36,3=>(byte)0x3e,4=>(byte)0x64,_=>(byte)0x65});
            if (register>=8) bytes.Add(0x41);
            bytes.Add(form.Opcode);int rm=register&7;
            bytes.Add((byte)((rm==5?0x40:0)|form.Group<<3|rm));if (rm==4) bytes.Add(0x24);if (rm==5) bytes.Add(0);
            var i=MemoryX87Classify(bytes.ToArray());var call=MemoryX87Call(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (int masks in new[] {0,63})
            foreach (ulong address in new[] {0x0100000000000000UL,0x7fffffffffffUL,ulong.MaxValue})
            {
                MemoryX87Seed(fake,state,4,(ushort)(0x300|masks));Write64(fake,24+register*8,address);
                MemoryX87Tag(fake,state,form.Load?7:0,form.Load?0:3); // even suppressed empty/full cannot win GP/SS
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;f.StackMemory=(FullStackMemoryDescriptor*)1;
                Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==(FullScalarMiscLowering.UsesStackSegment(i.Address)?12u:13u)&&
                    f.FaultCode==128&&f.FaultAddress==0&&issued==1&&completed==0&&f.Executed==0,
                    "R150 canonical endpoint/wrap SS versus GP precedes memory, stack fault and status even no-write");
                CanonicalX87Same(fake,state,before,raw);
            }
        }
    }

    static void MemoryX87ProofFailures(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryX87Forms())
        {
            if (form.Load) continue;
            var i=MemoryX87Classify([form.Opcode,(byte)(form.Group<<3|3)]);var call=MemoryX87Call(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (string mutation in new[] {"fault","site","address","width","access","host","length","reserved","request1","ticket","epoch","stop","attention"})
            {
                MemoryX87Seed(fake,state,0,0x37e);Write64(fake,48,0x100fff);MemoryX87Tag(fake,state,0,3);
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                FullStackMemoryDescriptor descriptor=default;f.StackMemory=&descriptor;
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0,
                    "R150 unmasked empty store stages permission proof before flags/metadata");
                Check(FullNativeMemory.AuthenticateCanonicalX87MemoryProbe(&f,in i,(Blink.Machine*)fake),
                    "R150 parent mapping-only request authenticated against exact current typed store");
                CanonicalX87Same(fake,state,before,raw);
                if (mutation=="fault")
                {
                    Check(FullNativeMemory.Fault(&f,0x101000,2)==FullMemoryAssistResult.GuestFault&&f.FaultTrap==14,
                        "R150 readonly/unmapped suppressed destination still gets PF");
                    CanonicalX87Same(fake,state,before,raw);continue;
                }
                descriptor.Epoch=epoch;descriptor.State=FullStackMemoryState.Ready;
                switch (mutation)
                {
                    case "site": ++descriptor.Site;break;case "address":++descriptor.Request0.Address;break;
                    case "width":descriptor.Request0.Width=8;break;case "access":descriptor.Request0.Access=FullMemoryAccess.Read;break;
                    case "host":descriptor.Request0.Host0=(byte*)1;break;case "length":descriptor.Request0.Length0=1;break;
                    case "reserved":descriptor.Request0.Reserved40=1;break;case "request1":descriptor.Request1.Width=16;break;
                    case "ticket":descriptor.Ticket=1;break;case "epoch":descriptor.Epoch=10;break;
                }
                FullNativeMemory.ResumeSameIssue(&f);
                if (mutation is "stop" or "attention")
                {
                    if (mutation=="attention")
                    {
                        ((Blink.Machine*)fake)->attention=true;f.Budget=0;
                        // Existing Poll deliberately honors the approved first
                        // issue before attention. This SAME memory instruction
                        // completes; pending attention remains for the next one.
                        Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                            f.Executed==1&&f.FirstPreissued==0&&descriptor.State==FullStackMemoryState.Consumed&&
                            (int)((Blink.Machine*)fake)->attention!=0&&f.MemoryAssists==0,
                            "R150 approved memory resume completes once before attention without new issue or mapping");
                        continue;
                    }
                    stop=1;
                    byte[] pending=CanonicalX87Bytes(fake,sizeof(Blink.Machine));
                    Check(RunFull(code,&f)==FullBlockExit.Stop&&
                        issued==1&&completed==0&&f.FirstPreissued==1&&descriptor.State==FullStackMemoryState.Ready,
                        "R150 stop preserves proof and SAME preissued instruction");
                    CanonicalX87Same(fake,state,pending,raw);
                    stop=0;((Blink.Machine*)fake)->attention=false;f.Budget=0;
                    Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.FirstPreissued==0&&
                        descriptor.State==FullStackMemoryState.Consumed,"R150 resumed approved issue completes once despite exhausted later budget");
                    continue;
                }
                Check(RunFull(code,&f)==(mutation=="epoch"?FullBlockExit.NeedMemory:FullBlockExit.Invalidated)&&
                    issued==1&&completed==0&&f.Executed==0,
                    "R150 proof is bound to exact site/EA/width/zero pointer payload and current epoch");
                CanonicalX87Same(fake,state,before,raw);
                if (mutation=="epoch") Check(descriptor.State==FullStackMemoryState.Request&&f.MemoryResume==1,
                    "R150 stale proof restages exact Request before returning NeedMemory");
            }
        }
    }

    static void MemoryX87One(NativeCodeAllocation code,MemoryX87Form form,JitInstruction i,byte* fake,
        FullX87RegisterFile* state,byte* pages,int offset,ulong fs=0,ulong gs=0,bool ticket=false)
    {
        ulong address=MemoryIntegerAddress(i,fake,fs,gs);
        Check((address&~4095UL)==0x100000 || (address&~4095UL)==0x101000,$"R150 fixture guest span EA={address:x16}, offset={offset}, fs={fs:x16}, gs={gs:x16}, IR={i}");
        byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
        byte[] expected=new byte[8192]; new ReadOnlySpan<byte>(pages,4096).CopyTo(expected);
        new ReadOnlySpan<byte>(pages+8192,4096).CopyTo(expected.AsSpan(4096));
        byte[] input;
        if (form.Load) input=expected.AsSpan(offset,form.Width/8).ToArray();
        else
        {
            int top=(int)(((Blink.Machine*)fake)->fpu.sw>>11)&7;
            input=new ReadOnlySpan<byte>(FullX87RegisterFile.Physical(state,top),10).ToArray();
        }
        if (input.Length<10) Array.Resize(ref input,10);
        var completion=MemoryX87Expected(form,i,input,m,raw,expected,offset,address);
        ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(i.Ip,code.Entry);
        var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87=state; f.FsBase=fs; f.GsBase=gs;
        FullStackMemoryDescriptor descriptor=default; f.StackMemory=&descriptor;
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        f.MemoryTlb=tlb; f.MemoryTlbMask=63;
        if (!ticket)
        {
            SetFullMemoryEntry(tlb,0x100000,pages); SetFullMemoryEntry(tlb,0x101000,pages+8192);
        }
        else
        {
            byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),beforeRaw=CanonicalX87Bytes((byte*)state,192);
            bool accessNeeded=form.Load||completion.Write;
            if (accessNeeded)
            {
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryWidth==form.Width&&
                    f.MemoryAddress==address&&f.MemoryAccess==(form.Load?FullMemoryAccess.Read:FullMemoryAccess.Write),
                    "R150 exact fullspan mapping request before guest effects");
                CanonicalX87Same(fake,state,before,beforeRaw);
                int first=Math.Min(form.Width/8,4096-(int)(address&4095));
                byte* firstPointer=offset<4096?pages+offset:pages+8192+offset-4096;
                FullNativeMemory.PublishBarrierTicket(&f,firstPointer,first<form.Width/8?pages+8192:null,(uint)first,epoch,123);
                FullNativeMemory.ResumeSameIssue(&f);
            }
        }
        if (!form.Load&&!completion.Write)
        {
            byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),beforeRaw=CanonicalX87Bytes((byte*)state,192);
            Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryWidth==form.Width&&
                f.MemoryAddress==address&&descriptor.Version==FullCanonicalX87MemoryLowering.MemoryProbeTag&&
                descriptor.Count==1&&descriptor.State==FullStackMemoryState.Request&&descriptor.Ticket==0&&
                descriptor.Request0.Host0==null&&descriptor.Request0.Host1==null,
                "R150 suppressed store still requests fullspan RW proof, no writer pointer/ticket");
            CanonicalX87Same(fake,state,before,beforeRaw);
            // Isolated proof publication models parent mapping-only resolver.
            // Parent integration tests must prove real PTE/EOF/authority checks.
            descriptor.Epoch=epoch; descriptor.State=FullStackMemoryState.Ready;
            FullNativeMemory.ResumeSameIssue(&f);
        }
        Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1&&
            f.HelperIndex==FullBlockFrame.NoHelperIndex&&f.HostMxcsrSaved==0&&f.FaultTrap==0,
            "R150 single native issue/complete, all nonvolatile GPRs, no semantic callback");
        CanonicalX87Same(fake,state,m,raw);
        Check(new ReadOnlySpan<byte>(pages,4096).SequenceEqual(expected.AsSpan(0,4096))&&
            new ReadOnlySpan<byte>(pages+8192,4096).SequenceEqual(expected.AsSpan(4096)),
            "R150 whole pages exactly match independent numeric+instruction completion oracle");
        Check(new ReadOnlySpan<byte>(pages+4096,4096).IndexOfAnyExcept((byte)0xa5)<0,"R150 cross-page transfer never touches unmapped host gap");
        Check(((uint*)((byte*)MemoryX87Work(state)+280))[0]==0&&MemoryX87Work(state)->Continuation==0,
            "R150 no stale live continuation or protocol failure survives RET");
        Check(f.NativeMemoryComputations==(form.Load||completion.Write?1UL:0UL),"R150 suppressed memory store performs no data access");
    }

    static IEnumerable<byte[]> MemoryX87Inputs(MemoryX87Form form)
    {
        if (form.Load && form.Width<80)
        {
            ulong[] values=form.Integer?[0,1,ulong.MaxValue,1UL<<(form.Width-1),(1UL<<(form.Width-1))-1,0x0020000000000001]:
                form.Width==32?[0,0x80000000,1,0x007fffff,0x00800000,0x3f800001,0x7f800000,0xff800000,0x7fc12345,0xff812345]:
                [0,0x8000000000000000,1,0xfffffffffffff,0x10000000000000,0x3ff0000000000001,
                    0x7ff0000000000000,0xfff0000000000000,0x7ff8000000000001,0xfff0000000000001];
            foreach (ulong v in values) yield return MemoryX87Packed(v,form.Width);
            yield break;
        }
        foreach ((ulong sig,ushort exp) in new (ulong,ushort)[] {
            (0,0),(0,0x8000),(0x8000000000000000,0x3fff),(0x8000008000000001,0x3fff),
            (0x8000000000000000,0x3ffe),(0xc000000000000000,0xbfff),(0xffffffffffffffff,0x403d),
            (0x8000000000000000,0xc03e),(0x8000000000000000,0x3f6a),(0x8000000000000001,0x3f6a),
            (0x8000000000000000,0x7ffe),(1,0),(0x8000000000000001,0),(0x4000000000000000,1),
            (0,0x4000),(0x8000000000000000,0x7fff),(0x8000000000000000,0xffff),
            (0xc123456789abcdef,0x7fff),(0x8123456789abcdef,0xffff),(0x4123456789abcdef,0x7fff) })
            yield return MemoryX87Raw(sig,exp);
    }

    static void MemoryX87Numbers(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryX87Forms())
        {
            var i=MemoryX87Classify([form.Opcode,(byte)(form.Group<<3|3)]); var call=MemoryX87Call(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (byte[] value in MemoryX87Inputs(form))
            foreach (int rc in new[] {0,1,2,3})
            foreach (int pc in new[] {0,1,2,3})
            foreach (int masks in new[] {0,1,2,8,16,32,63})
            {
                ushort cw=(ushort)(0x40|pc<<8|rc<<10|masks);
                MemoryX87Seed(fake,state,(rc+pc)&7,cw); new Span<byte>(pages,12288).Fill(0xa5);
                Write64(fake,48,0x100ffb); // RBX, all selected widths may cross
                int top=(int)(((Blink.Machine*)fake)->fpu.sw>>11)&7;
                MemoryX87Tag(fake,state,form.Load?7:0,form.Load?3:MemoryX87TagFor(value),form.Load?null:value);
                if (form.Load)
                    for (int b=0;b<form.Width/8;++b) (4091+b<4096?pages+4091+b:pages+8192+4091+b-4096)[0]=value[b];
                MemoryX87One(code,form,i,fake,state,pages,4091);
            }
            // Every mask, every TOP, full/empty and all pop forms. This separately
            // constrains stack priority and raw80 empty integer/FP indefinites.
            for (int masks=0;masks<64;++masks)
            for (int top=0;top<8;++top)
            {
                MemoryX87Seed(fake,state,top,(ushort)(0x300|masks)); new Span<byte>(pages,12288).Fill(0xa5);
                Write64(fake,48,0x100080);
                MemoryX87Tag(fake,state,form.Load?7:0,form.Load?0:3);
                MemoryX87One(code,form,i,fake,state,pages,128,ticket:true);
            }
        }
    }

    static void MemoryX87Addresses(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryX87Forms())
        foreach (byte[] addressBytes in new byte[][] { [0], [4,0x24], [0x45,0], [0x84,0xcc,0x80,0,0,0],
            [5,0,0,0,0], [4,0x25,0,0,0,0] })
        foreach (byte[] prefix in new byte[][] { [],[0x67],[0x64],[0x65,0x67],[0x4b],[0x65,0x67,0x4b] })
        {
            byte[] encoding=[..prefix,form.Opcode,(byte)(addressBytes[0]|form.Group<<3),..addressBytes.AsSpan(1).ToArray()];
            var i=MemoryX87Classify(encoding); var call=MemoryX87Call(i,calls);
            var original=i;
            foreach (int offset in new[] {0,1,4096-form.Width/8,4095,4097})
            {
                i=original;
                MemoryX87Seed(fake,state,7,0x37f); new Span<byte>(pages,12288).Fill(0xa5);
                for (int r=0;r<16;++r) Write64(fake,24+r*8,0);
                ulong fs=0,gs=0; var a=i.Address;
                ulong target=0x100000+(uint)offset;
                ulong current=MemoryIntegerAddress(i,fake,0,0);
                if (a.Base>=0)
                {
                    ulong b=unchecked(target-current);
                    if (a.Index==a.Base)
                    {
                        // REX.X can make the SIB no-index code a real R12
                        // index, equal to the base. Solve BOTH occurrences;
                        // offsets outside this finite encoding's lattice are
                        // unrepresentable, rather than a native failure.
                        ulong factor=1UL+(1UL<<a.Scale);
                        if (b%factor!=0) continue;
                        b/=factor;
                    }
                    if (a.AddressWidth==32) b|=0xfedcba9800000000;
                    Write64(fake,24+a.Base*8,b);
                }
                else if (a.Segment==4) fs=unchecked(target-current);
                else if (a.Segment==5) gs=unchecked(target-current);
                else
                {
                    // RIP/EIP and no-base displacement adjusted at translation.
                    long disp=unchecked((long)target-(a.RipRelative?(long)(i.Ip+(uint)i.Length):0));
                    if (disp is < int.MinValue or > int.MaxValue) continue;
                    i=i with { Address=a with { Displacement=disp } };
                    // Body with updated compile-time displacement belongs to a
                    // separate allocation; ownership stays synchronous.
                    using var adjusted=CreateFullBlock([new(i,X87Kernel:call)]);
                    MemoryX87Tag(fake,state,form.Load?7:0,form.Load?3:0,form.Load?null:MemoryX87Raw(0x8000000000000001,0x3fff));
                    MemoryX87One(adjusted,form,i,fake,state,pages,offset); continue;
                }
                using var code=CreateFullBlock([new(i,X87Kernel:call)]);
                MemoryX87Tag(fake,state,form.Load?7:0,form.Load?3:0,form.Load?null:MemoryX87Raw(0x8000000000000001,0x3fff));
                MemoryX87One(code,form,i,fake,state,pages,offset,fs,gs,ticket:offset==4095);
            }
        }
    }

    static void MemoryX87Misses(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach (var form in MemoryX87Forms())
        {
            var i=MemoryX87Classify([form.Opcode,(byte)(form.Group<<3|3)]);var call=MemoryX87Call(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (string failure in new[] {"first","second","permission","eof","epoch","barrier"})
            {
                MemoryX87Seed(fake,state,0,0x37f); new Span<byte>(pages,12288).Fill(0xa5); Write64(fake,48,0x100fff);
                MemoryX87Tag(fake,state,form.Load?7:0,form.Load?3:0,form.Load?null:MemoryX87Raw(0x8000000000000000,0x3fff));
                new Span<FullMemoryTlbEntry>(tlb,64).Clear(); SetFullMemoryEntry(tlb,0x100000,pages);SetFullMemoryEntry(tlb,0x101000,pages+8192);
                ref var e=ref tlb[FullNativeMemory.TlbIndex(failure=="first"?0x100000UL:0x101000UL,63)];
                if (failure is "first" or "second") e=default;
                if (failure=="permission") e.Permissions=form.Load?FullMemoryAccess.Write:FullMemoryAccess.Read;
                if (failure=="eof") e.ValidEnd=4095;
                if (failure=="epoch") e.AuthorityEpoch=10;
                if (failure=="barrier") { if (form.Load) continue; e.Flags=FullMemoryFlags.WriteBarrier; }
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192),data=CanonicalX87Bytes(pages,12288);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryWidth==form.Width&&f.MemoryResume==1,
                    "R150 first/second/permission/EOF/epoch/barrier miss before any architectural effect");
                CanonicalX87Same(fake,state,before,raw);
                Check(new ReadOnlySpan<byte>(pages,12288).SequenceEqual(data),"R150 both pages unchanged on partial-span miss");
                Check(FullNativeMemory.Fault(&f,f.MemoryAddress,failure=="eof"?unchecked((uint)-2):2)==FullMemoryAssistResult.GuestFault,
                    "R150 mapping assist delivers precise guest fault, no arithmetic fallback");
                CanonicalX87Same(fake,state,before,raw);
                Check(completed==0&&f.Executed==0,"R150 fault preserves RSP and withholds completion");
            }
        }
    }

    static void MemoryX87Boundaries(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryX87Forms())
        {
            var i=MemoryX87Classify([form.Opcode,(byte)(form.Group<<3|3)]); var call=MemoryX87Call(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (string boundary in new[] {"stop","attention","budget","epoch","null","owner","mf1","mf2","mf4","mf8","mf16","mf32"})
            {
                MemoryX87Seed(fake,state,3,0x37f); Write64(fake,48,0x800000000000); // poison noncanonical EA
                bool pending=boundary.StartsWith("mf",StringComparison.Ordinal);
                if (pending) { uint bit=uint.Parse(boundary.AsSpan(2));((Blink.Machine*)fake)->fpu.sw|=bit;((Blink.Machine*)fake)->fpu.cw&=~bit; }
                if (boundary=="attention") ((Blink.Machine*)fake)->attention=true;
                byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192),work=CanonicalX87Bytes((byte*)MemoryX87Work(state),384);
                ulong epoch=11;long issued=0,completed=0;int stop=boundary=="stop"?1:0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:boundary=="budget"?0UL:1UL,limit:1);
                f.X87=boundary=="null"?null:state;
                if (boundary=="owner") { h.OwnerCookie=f.OwnerCookie=74;state->OwnerToken=74;raw=CanonicalX87Bytes((byte*)state,192); }
                if (boundary=="epoch") ++epoch;
                f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;f.StackMemory=(FullStackMemoryDescriptor*)1;
                var want=boundary switch {"stop"=>FullBlockExit.Stop,"attention"=>FullBlockExit.Attention,
                    "budget"=>FullBlockExit.Budget,_=>pending?FullBlockExit.GuestFault:FullBlockExit.Invalidated};
                Check(RunFull(code,&f)==want&&completed==0&&f.Executed==0&&issued==(pending||boundary=="budget"?1:0),
                    "R150 pre-Issue/previous-MF priority before poisoned memory/kernel");
                CanonicalX87Same(fake,state,before,raw);
                Check(new ReadOnlySpan<byte>(MemoryX87Work(state),384).SequenceEqual(work),"R150 denied/prior-MF never enters Work or kernel");
                if (pending) Check(f.FaultTrap==16,"R150 exactly six pending masks, prior #MF wins noncanonical memory");
            }
            foreach (byte[] raw in new[] {MemoryX87Raw(0xc123456789abcdef,0x7fff),MemoryX87Raw(0x8000000000000000,0x7ffe)})
            {
                if (form.Load) continue;
                MemoryX87Seed(fake,state,3,0x340); new Span<byte>(pages,12288).Fill(0xa5);Write64(fake,48,0x100fff);
                MemoryX87Tag(fake,state,0,MemoryX87TagFor(raw),raw);
                // Suppressed writes require mapping-only RW proof even though
                // no writable data pointer or revocation ticket is consumed.
                MemoryX87One(code,form,i,fake,state,pages,4095,ticket:true);
            }
        }
    }

    static void MemoryX87Deferred(byte* fake,FullX87RegisterFile* state,byte* pages,FullX87KernelCall[] calls)
    {
        foreach (var form in MemoryX87Forms())
        {
            var i=MemoryX87Classify([form.Opcode,(byte)(form.Group<<3|3)]);var call=MemoryX87Call(i,calls);
            var wait=new JitInstruction(i.Ip+(uint)i.Length,1,JitOperation.X87,80,default,default,
                Extra:new(0,0,IsImmediate:true),Options:(uint)FullCanonicalX87StackOperation.Fwait);
            using var code=CreateFullBlock([new(i,X87Kernel:call),new(wait)]);
            MemoryX87Seed(fake,state,7,0x37e);new Span<byte>(pages,12288).Fill(0xa5);Write64(fake,48,0x100080);
            MemoryX87Tag(fake,state,form.Load?7:0,form.Load?0:3);
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,0x100000,pages);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop);f.X87=state;f.MemoryTlb=tlb;f.MemoryTlbMask=63;
            FullStackMemoryDescriptor descriptor=default;f.StackMemory=&descriptor;
            if (!form.Load)
            {
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0,
                    "R150 newly unmasked empty store first requests RW proof");
                descriptor.Epoch=epoch;descriptor.State=FullStackMemoryState.Ready;FullNativeMemory.ResumeSameIssue(&f);
            }
            Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==16&&issued==2&&completed==1&&f.Executed==1&&
                ((Blink.Machine*)fake)->ip==wait.Ip&&(((Blink.Machine*)fake)->fpu.sw&0x8081)==0x8081,
                "R150 newly unmasked stack exception completes its instruction; following waiting instruction raises MF");
        }
    }

    static void MemoryX87Encodings(FullX87KernelCall[] calls)
    {
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity],common=new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windows in new[] {false,true})
        foreach (bool shared in new[] {false,true})
        {
            FullBlockEmitter.EmitScaffold(common,out var layout,windows);var scaffold=layout.Resolve((nint)0x10000000);
            foreach (var form in MemoryX87Forms())
            {
                byte[] bytes=[0x65,0x67,0x4b,form.Opcode,(byte)(0x44|form.Group<<3),0xcc,0xf8];
                var i=MemoryX87Classify(bytes);var call=MemoryX87Call(i,calls);FullBlockInstruction[] body=[new(i,X87Kernel:call)];
                int n; bool good=shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out n,windows):FullBlockEmitter.TryEmit(body,staging,out n,windows);
                Check(good&&n>0,"R150 both ABIs/scaffolds/all19 actual memory native bodies");
                int continuationCount=0;
                for (int p=0;p+7<n;++p)
                {
                    if (!staging.AsSpan(p,3).SequenceEqual(new byte[] {0x48,0x8d,0x05})) continue;
                    int target=p+7+BinaryPrimitives.ReadInt32LittleEndian(staging.AsSpan(p+3));
                    if (target<0||target+7>n||!staging.AsSpan(target,7).SequenceEqual(new byte[] {0xf3,0x0f,0x1e,0xfa,0x4d,0x89,0xd9})) continue;
                    ++continuationCount;
                    Check(BinaryPrimitives.ReadUInt64LittleEndian(staging.AsSpan(target-10))==(ulong)call.Entry,
                        "R150 literal typed catalog Entry, exact continuation with restored frame+Machine");
                }
                Check(continuationCount==1,"R150 one specialized conversion continuation, no runtime selector");
                foreach (int cut in new[] {0,1,7,n-1})
                {
                    int size; bool partial=shared?FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),in scaffold,out size,windows):
                        FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),out size,windows);
                    Check(!partial&&size==0,"R150 short span cannot publish partial native body");
                }
                for (int warm=0;warm<12;++warm)
                {
                    FullCanonicalX87MemoryLowering.TryClassify(Ip,bytes,out _,out _);
                    if (shared) FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows);else FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                long allocated=GC.GetAllocatedBytesForCurrentThread();
                for (int warm=0;warm<16;++warm)
                {
                    good&=FullCanonicalX87MemoryLowering.TryClassify(Ip,bytes,out _,out _);
                    good&=shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows):FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                Check(good&&allocated==GC.GetAllocatedBytesForCurrentThread(),"R150 warm exact parser/caller-owned emission allocates zero");
            }
        }
    }
}
