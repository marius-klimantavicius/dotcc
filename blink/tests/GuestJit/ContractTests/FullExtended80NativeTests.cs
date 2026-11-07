using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using E80 = Managed.Emulation.Execution.FullExtended80;
using E80Op = Managed.Emulation.Execution.FullExtended80Operation;

unsafe partial class Program
{
    // R126 SOURCE_READY_UNEXECUTED. Parent wires this entry; it has not been compiled,
    // its emitter has not been called, and none of its native code has run.
    // Actual repository publisher is NativeCodeAllocation.TryCreate (RW->RX);
    // no NativeCodeRegion type was found in this checkout.
    static void CheckFullExtended80Native()
    {
        Check(sizeof(E80) == 16 && sizeof(FullExtended80WorkFrame) == 384,
            "R123 separate blittable binary80 element/workframe sizes");
        foreach ((string field, int offset) in new (string, int)[] {
            (nameof(FullExtended80WorkFrame.Version),0),(nameof(FullExtended80WorkFrame.Size),4),
            (nameof(FullExtended80WorkFrame.ControlWord),8),(nameof(FullExtended80WorkFrame.Admission),12),
            (nameof(FullExtended80WorkFrame.Left),16),(nameof(FullExtended80WorkFrame.Right),32),
            (nameof(FullExtended80WorkFrame.Normal),48),(nameof(FullExtended80WorkFrame.Wrapped),64),
            (nameof(FullExtended80WorkFrame.NormalStatus),80),(nameof(FullExtended80WorkFrame.WrappedStatus),82),
            (nameof(FullExtended80WorkFrame.Classification),84),(nameof(FullExtended80WorkFrame.Continuation),88),
            (nameof(FullExtended80WorkFrame.NativeScratch),96),(nameof(FullExtended80WorkFrame.Reserved256),256) })
            Check(Marshal.OffsetOf<FullExtended80WorkFrame>(field).ToInt32() == offset, "R123 workframe field offsets");
        Check(Marshal.OffsetOf<E80>(nameof(E80.Significand)).ToInt32() == 0 &&
            Marshal.OffsetOf<E80>(nameof(E80.SignExponent)).ToInt32() == 8, "R123 architectural ten-byte prefix");
        CheckE80ReferenceAnchors();
        CheckE80Emission();
        foreach (E80Op op in new[] { E80Op.Add, E80Op.Subtract, E80Op.Multiply, E80Op.Divide })
        {
            using var standalone = CreateE80Code(op, false);
            using var kernel = CreateE80Code(op, true);
            // Continuation is a native leaf RET, no semantic helper/trampoline.
            Check(NativeCodeAllocation.TryCreate([0xf3,0x0f,0x1e,0xfa,0xc3], out var ret,
                out string why, requireNativeFlags: false), why);
            using var returnCode = ret!;
            using var bridge = CreateE80Bridge(kernel.Entry);
            foreach (int pc in new[] { 0, 2, 3 }) foreach (int rc in new[] { 0, 1, 2, 3 })
            foreach (int masks in new[] { 63, 0, 61, 31, 55, 47, 62, 59 })
            {
                ushort cw = (ushort)(0x40 | masks | pc << 8 | rc << 10);
                foreach ((E80 a, E80 b) in E80CasesForOperation(op))
                {
                    E80Expected expected = ReferenceE80(op, a, b, cw);
                    CheckE80Invocation(standalone.Entry, op, a, b, cw, expected, 0);
                    CheckE80Invocation(bridge.Entry, op, a, b, cw, expected, returnCode.Entry);
                }
            }
            CheckE80Protocol(standalone.Entry);
            CheckE80Protocol(bridge.Entry, returnCode.Entry);
            CheckE80NaNPolicy(standalone.Entry, op);
            CheckE80NaNPolicy(bridge.Entry, op, returnCode.Entry);
            if(op==E80Op.Divide)
            {
                CheckE80DivisionPriority(standalone.Entry);
                CheckE80DivisionPriority(bridge.Entry, returnCode.Entry);
                CheckE80DivisionSingleRounding(standalone.Entry);
                CheckE80DivisionSingleRounding(bridge.Entry, returnCode.Entry);
            }
            CheckE80NativeWarm(standalone.Entry);
            CheckE80NativeWarm(bridge.Entry, returnCode.Entry);
        }
    }

    static NativeCodeAllocation CreateE80Code(E80Op op, bool continuation)
    {
        byte[] staging = new byte[FullExtended80Emitter.CodeCapacity];
        bool emitted = continuation ? FullExtended80Emitter.TryEmitContinuation(op, staging, out int n) :
            FullExtended80Emitter.TryEmit(op, staging, out n);
        Check(emitted, "R123 specialized binary80 emission");
        Check(NativeCodeAllocation.TryCreate(staging.AsSpan(0,n), out var allocation, out string why,
            requireNativeFlags: false), why);
        return allocation!;
    }

    // Shared-owner scaffold model: one bridge per immutable arithmetic routine,
    // and caller-selected native continuation. JMP chains never add CLR frames.
    static NativeCodeAllocation CreateE80Bridge(nint internalEntry)
    {
        byte[] b = new byte[19]; b[0]=0xf3; b[1]=0x0f; b[2]=0x1e; b[3]=0xfa;
        b[4]=0x49; b[5]=0x89; b[6]=OperatingSystem.IsWindows() ? (byte)0xcb : (byte)0xfb; // R11=arg
        b[7]=0x48; b[8]=0xb8; BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(9), (ulong)internalEntry);
        b[17]=0xff; b[18]=0xe0; // JMP RAX
        Check(NativeCodeAllocation.TryCreate(b, out var allocation, out string why, requireNativeFlags:false),why);
        return allocation!;
    }

    static FullExtended80WorkFrame E80Frame(E80 a, E80 b, ushort cw) => new()
    { Version=1, Size=384, Left=a, Right=b, ControlWord=cw };

    static void CheckE80Invocation(nint entry, E80Op op, E80 a, E80 b, ushort cw,
        E80Expected expected, nint continuation)
    {
        // Guard both sides, caller-reserved bytes and input padding. Native
        // scratch is intentionally poisoned to expose missing initialization.
        byte* guarded=stackalloc byte[416]; new Span<byte>(guarded,416).Fill(0xa5);
        var f=(FullExtended80WorkFrame*)(guarded+16); *f=E80Frame(a,b,cw);
        new Span<byte>((byte*)f+96,288).Fill(0xa5);
        f->Left.Reserved10=0x1234; f->Left.Reserved12=0xabcdef01;
        f->Right.Reserved10=0x5678; f->Right.Reserved12=0xabcdef02;
        f->Continuation=(ulong)continuation;
        uint result=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(f);
        Check(result==(uint)expected.Admission && f->Admission==expected.Admission, "R123 admission returned in EAX and frame");
        Check(new ReadOnlySpan<byte>(guarded,16).IndexOfAnyExcept((byte)0xa5)<0 &&
            new ReadOnlySpan<byte>(guarded+400,16).IndexOfAnyExcept((byte)0xa5)<0 &&
            new ReadOnlySpan<byte>((byte*)f+256,128).IndexOfAnyExcept((byte)0xa5)<0,
            "R123 all native stores remain within declared workframe scratch/output bounds");
        Check(E80Equal(f->Left,a) && E80Equal(f->Right,b) && f->Left.Reserved10==0x1234 &&
            f->Left.Reserved12==0xabcdef01 && f->Right.Reserved10==0x5678 &&
            f->Right.Reserved12==0xabcdef02 && f->Version==1 && f->Size==384 && f->ControlWord==cw &&
            f->Continuation==(ulong)continuation, "R123 input values/padding/control/continuation immutable");
        Check(E80Equal(f->Normal,expected.Normal) && E80Equal(f->Wrapped,expected.Wrapped) &&
            f->NormalStatus==expected.NormalStatus && f->WrappedStatus==expected.WrappedStatus &&
            (uint)f->Classification==expected.Classification && f->Normal.Reserved10==0 &&
            f->Normal.Reserved12==0 && f->Wrapped.Reserved10==0 && f->Wrapped.Reserved12==0,
            "R125 exact BigInteger dyadic/rational result/status/C1/wrap/classification (all PC/RC/masks)");
        // Reuse output as next input, including ST(i)==ST(0) source values.
        if (expected.Admission==0 && expected.Classification==17)
        {
            E80 previous=f->Normal; f->Left=previous; f->Right=previous;
            var again=ReferenceE80(op,previous,previous,cw);
            ((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(f);
            Check(E80Equal(f->Normal,again.Normal) && f->NormalStatus==again.NormalStatus,
                "R123 output/input reuse and identical physical source values");
        }
    }

    static void CheckE80Protocol(nint entry, nint continuation = 0)
    {
        foreach ((uint version,uint size,ushort cw,FullExtended80Admission reason) in new[] {
            (2u,384u,(ushort)0x37f,FullExtended80Admission.BadVersion),
            (1u,383u,(ushort)0x37f,FullExtended80Admission.BadSize) })
        {
            var f=E80Frame(new(0x8000000000000000,0x3fff),default,cw); f.Version=version; f.Size=size;
            f.Continuation=(ulong)continuation;
            Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f)==(uint)reason &&
                f.Admission==reason && f.Classification==0 && f.NormalStatus==0 && f.WrappedStatus==0,
                "R123 protocol reject before result validity");
        }
    }

    static void CheckE80NativeWarm(nint entry, nint continuation = 0)
    {
        var f=E80Frame(new(0x8000000000000000,0x3fff),new(0x8000000000000000,0x3fc0),0x37f);
        f.Continuation=(ulong)continuation;
        for(int i=0;i<16;++i)((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        long before=GC.GetAllocatedBytesForCurrentThread(); uint admission=0;
        for(int i=0;i<256;++i)admission|=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        Check(admission==0 && GC.GetAllocatedBytesForCurrentThread()==before,"R123 warm native arithmetic allocates zero managed bytes");
    }

    // Literal architectural anchors independent of ReferenceE80. CW masks
    // deliberately do not change this API's fully-masked result/status: the
    // future instruction-completion layer must select/suppress by priority.
    static void CheckE80NaNPolicy(nint entry, E80Op op, nint continuation = 0)
    {
        // Independently specified selections: q over s, otherwise larger raw
        // significand; SAME-CLASS TIES select Left, preserving its raw sign.
        // selectLeft/ie below are literal expectations, not oracle decisions.
        foreach(int pc in new[]{0,1,2,3}) foreach(int rc in new[]{0,1,2,3})
        foreach(int masks in new[]{0,1,62,63}) // both IM states, other masks both ways
        foreach(int signA in new[]{0,0x8000}) foreach(int signB in new[]{0,0x8000})
        foreach((ulong aSig,ulong bSig,bool selectLeft,bool ie) in new[] {
            (0xc000000000000001UL,0xc000000000000001UL,true,false),
            (0x8000000000000001UL,0x8000000000000001UL,true,true),
            (0xc000000000000001UL,0xffffffffffffffffUL,false,false),
            (0xffffffffffffffffUL,0xc000000000000001UL,true,false),
            (0x8000000000000001UL,0xbfffffffffffffffUL,false,true),
            (0xbfffffffffffffffUL,0x8000000000000001UL,true,true),
            (0xc000000000000001UL,0xbfffffffffffffffUL,true,true),
            (0xbfffffffffffffffUL,0xc000000000000001UL,false,true) })
        {
            ushort cw=(ushort)(0x40|masks|pc<<8|rc<<10);
            E80 a=new(aSig,(ushort)(0x7fff|signA)), b=new(bSig,(ushort)(0x7fff|signB));
            E80 selected=selectLeft?a:b; selected.Significand|=0x4000000000000000;
            E80Expected expected=new(selected,default,(ushort)(ie?1:0),0,ie?97u:65u);
            var oracle=ReferenceE80(op,a,b,cw);
            Check(oracle.Admission==FullExtended80Admission.Complete &&
                E80Equal(oracle.Normal,expected.Normal) && E80Equal(oracle.Wrapped,default) &&
                oracle.NormalStatus==expected.NormalStatus && oracle.WrappedStatus==0 &&
                oracle.Classification==expected.Classification,
                "R126 independent NaN literals: Left tie, larger same class, q over s, IE/pre and sign");
            CheckE80Invocation(entry,op,a,b,cw,expected,continuation);
        }
    }

    static void CheckE80DivisionPriority(nint entry, nint continuation = 0)
    {
        E80 zero=default, one=E80Power(0), sub=new(1,0), inf=new(0x8000000000000000,0x7fff);
        E80 indefinite=new(0xc000000000000000,0xffff), qnan=new(0xc000000000000001,0x7fff);
        foreach(int pc in new[]{0,1,2,3}) foreach(int rc in new[]{0,1,2,3})
        foreach(int masks in new[]{0,63,61,59,62})
        foreach(int signA in new[]{0,0x8000}) foreach(int signB in new[]{0,0x8000})
        {
            ushort cw=(ushort)(0x40|masks|pc<<8|rc<<10), sign=(ushort)(signA^signB);
            E80 Signed(E80 x,int signBit) => new(x.Significand,(ushort)(x.SignExponent|signBit));
            foreach((E80 a,E80 b,E80Expected expected) in new (E80,E80,E80Expected)[]{
                (Signed(sub,signA),Signed(zero,signB),new(new(0x8000000000000000,(ushort)(0x7fff|sign)),default,4,0,161)),
                (Signed(new(0x8000000000000000,0),signA),Signed(zero,signB),new(new(0x8000000000000000,(ushort)(0x7fff|sign)),default,4,0,161)),
                (Signed(one,signA),Signed(zero,signB),new(new(0x8000000000000000,(ushort)(0x7fff|sign)),default,4,0,161)),
                (Signed(inf,signA),Signed(zero,signB),new(new(0x8000000000000000,(ushort)(0x7fff|sign)),default,0,0,145)),
                (Signed(zero,signA),Signed(zero,signB),new(indefinite,default,1,0,97)),
                (Signed(inf,signA),Signed(inf,signB),new(indefinite,default,1,0,97)),
                (Signed(sub,signA),Signed(inf,signB),new(new(0,sign),default,2,0,49)),
                (Signed(zero,signA),Signed(sub,signB),new(new(0,sign),default,2,0,49)),
                (Signed(zero,signA),Signed(one,signB),new(new(0,sign),default,0,0,17)),
                (qnan,Signed(zero,signB),new(qnan,default,0,0,65)),
                (qnan,Signed(sub,signB),new(qnan,default,0,0,65)),
                (Signed(zero,signA),qnan,new(qnan,default,0,0,65)),
                (Signed(sub,signA),qnan,new(qnan,default,0,0,65)),
                (new(0x8000000000000001,0x7fff),Signed(sub,signB),new(qnan,default,1,0,97)),
                (Signed(sub,signA),new(0x8000000000000001,0x7fff),new(qnan,default,1,0,97)),
                (new(0x8000000000000001,0x7fff),Signed(zero,signB),new(qnan,default,1,0,97)),
                (Signed(zero,signA),new(0x8000000000000001,0x7fff),new(qnan,default,1,0,97)),
                (new(0,0x4000),qnan,new(indefinite,default,1,0,97)),
                (qnan,new(0,0x4000),new(indefinite,default,1,0,97)),
                (new(0x4000000000000001,0x7fff),new(0x8000000000000001,0x7fff),new(indefinite,default,1,0,97)),
                (new(0x8000000000000001,0x7fff),new(0x4000000000000001,0x7fff),new(indefinite,default,1,0,97)) })
                CheckE80Invocation(entry,E80Op.Divide,a,b,cw,expected,continuation);
        }
    }

    static void CheckE80DivisionSingleRounding(nint entry, nint continuation = 0)
    {
        // Each non-dyadic ratio gives a different masked result if its PC-
        // rounded wrapped significand is rounded a second time for underflow.
        // Literal values came from exact arbitrary-precision rational rounding.
        foreach((int pc,ulong a,ulong b,ulong normal,ushort ns,ulong wrapped,ushort ws) in new[] {
            (0,0x839c73b5a965ffdaUL,0xe1dc6c2abef35a44UL,0x254b190000000000UL,(ushort)560,0x952c620000000000UL,(ushort)48),
            (2,0x8967ddc1693bbd38UL,0xd218ea34dcb0cac9UL,0x29db4c4bb5726800UL,(ushort)48,0xa76d312ed5c9b000UL,(ushort)560),
            (3,0xb6bf35839a88fb6fUL,0xfb11668aaa967cebUL,0x2e958e9278cc4ca9UL,(ushort)48,0xba563a49e33132a6UL,(ushort)560) })
        foreach(int masks in new[]{0,63,47,31})
            CheckE80Invocation(entry,E80Op.Divide,new(a,1),new(b,0x4000),(ushort)(0x40|masks|pc<<8),
                new(new(normal,0),new(wrapped,0x5fff),ns,ws,7),continuation);
    }

    static void CheckE80Emission()
    {
        byte[] buffer=new byte[FullExtended80Emitter.CodeCapacity+16];
        foreach(E80Op op in new[]{E80Op.Add,E80Op.Subtract,E80Op.Multiply,E80Op.Divide})
        foreach(bool windows in new[]{false,true}) foreach(bool continuation in new[]{false,true})
        {
            bool good=continuation ? FullExtended80Emitter.TryEmitContinuation(op,buffer,out int n) :
                FullExtended80Emitter.TryEmit(op,buffer,out n,windows);
            Check(good && n>0 && n<=FullExtended80Emitter.CodeCapacity,"R123 both ABI / internal-entry code capacity");
            if(continuation)Check(buffer.AsSpan(n-7,7).SequenceEqual(new byte[]{0x41,0xff,0xa3,88,0,0,0}),"R123 internal native continuation tail");
            else Check(buffer.AsSpan(4,3).SequenceEqual(windows ? new byte[]{0x49,0x89,0xcb} : new byte[]{0x49,0x89,0xfb}) &&
                buffer[n-1]==0xc3,"R123 ABI pointer register and RET form");
            foreach(int capacity in new[]{0,1,4,n/2,n-1,n})
            {
                buffer.AsSpan().Fill(0x5a);
                bool emitted=continuation ? FullExtended80Emitter.TryEmitContinuation(op,buffer.AsSpan(0,capacity),out int length) :
                    FullExtended80Emitter.TryEmit(op,buffer.AsSpan(0,capacity),out length,windows);
                Check(emitted==(capacity==n) && length==(emitted?n:0) &&
                    buffer.AsSpan(capacity).IndexOfAnyExcept((byte)0x5a)<0,"R123 bounded overflow false/length0, no destination overrun");
            }
            for(int i=0;i<8;++i)good&=continuation ? FullExtended80Emitter.TryEmitContinuation(op,buffer,out _) :
                FullExtended80Emitter.TryEmit(op,buffer,out _,windows);
            long before=GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<64;++i)good&=continuation ? FullExtended80Emitter.TryEmitContinuation(op,buffer,out _) :
                FullExtended80Emitter.TryEmit(op,buffer,out _,windows);
            Check(good && GC.GetAllocatedBytesForCurrentThread()==before,"R123 warm both ABI/internal span emission allocates zero");
        }
        Check(!FullExtended80Emitter.TryEmit((E80Op)255,buffer,out int missing) && missing==0 &&
            !FullExtended80Emitter.TryEmitContinuation((E80Op)255,buffer,out missing) && missing==0,
            "R125 unknown operation explicitly rejected by both entries");
    }

    static bool E80Equal(E80 a,E80 b) => a.Significand==b.Significand && a.SignExponent==b.SignExponent;
    static E80 E80Power(int exponent,bool negative=false) => new(0x8000000000000000,
        (ushort)(exponent+16383|(negative?0x8000:0)));
    static IEnumerable<(E80,E80)> E80CasesForOperation(E80Op op)
    {
        foreach(var pair in E80Cases())yield return pair;
        if(op!=E80Op.Divide)yield break;
        E80 one=E80Power(0), three=new(0xc000000000000000,0x4000);
        yield return(one,three); yield return(one,E80Power(1));
        yield return(new(0x839c73b5a965ffda,1),new(0xe1dc6c2abef35a44,0x4000));
        yield return(new(0x8967ddc1693bbd38,1),new(0xd218ea34dcb0cac9,0x4000));
        yield return(new(0xb6bf35839a88fb6f,1),new(0xfb11668aaa967ceb,0x4000));
        // All normalized-significand extrema/near-equality ratios, including
        // remainders requiring the 65th bit when doubled in the native loop.
        foreach(ulong a in new ulong[]{0x8000000000000000,0x8000000000000001,
            0xaaaaaaaaaaaaaaaa,0xc000000000000000,ulong.MaxValue-1,ulong.MaxValue})
        foreach(ulong b in new ulong[]{0x8000000000000000,0x8000000000000001,
            0xaaaaaaaaaaaaaaab,0xc000000000000000,ulong.MaxValue-1,ulong.MaxValue})
        foreach(int sign in new[]{0,0x8000})
        { yield return(new(a,(ushort)(0x3fff|sign)),new(b,0x3fff)); }
        foreach(E80 a in new E80[]{new(1,0),new(3,0),new(0x7fffffffffffffff,0),
            E80Power(-16382),new(0x8000000000000001,1),new(ulong.MaxValue,1),
            E80Power(16383),new(ulong.MaxValue,0x7ffe)})
        foreach(E80 b in new E80[]{new(1,0),new(3,0),E80Power(-16382),
            E80Power(-1),E80Power(1),three,E80Power(16383),new(ulong.MaxValue,0x7ffe)})
        {
            yield return(a,b);
            E80 negative=a; negative.SignExponent^=0x8000; yield return(negative,b);
        }
        // Exact PC24/53 halfways with both retained parities, plus rational
        // tails on each side. PC64 normal dyadic quotients have <=64 bits;
        // its halfway cases instead occur at denormal range rounding.
        foreach(int p in new[]{24,53}) foreach(ulong parity in new ulong[]{0,1})
        {
            ulong middle=0x8000000000000000+(parity<<(64-p))+(1UL<<(63-p));
            foreach(ulong n in new[]{middle-1,middle,middle+1})
            { yield return(new(n,0x3fff),one); yield return(new(n,1),E80Power(1)); }
        }
        foreach(ulong n in new ulong[]{1,2,3,5,ulong.MaxValue})
            yield return(new(n,0),E80Power(1));
    }
    static IEnumerable<(E80,E80)> E80Cases()
    {
        E80 one=E80Power(0), zero=default;
        // Discriminators the binary64 backing cannot represent.
        yield return(one,E80Power(-63)); yield return(new(0x8000000000000001,0x3fff),one);
        yield return(one,new(0x8000000000000001,0x3fff));
        foreach(int power in new[]{-32800,-16446,-16445,-16383,-16382,-65,-64,-63,-54,-53,-24,-23,0,1,63,16382,16383})
        {
            // Only construct representable operands; below -16382 use explicit subnormal.
            E80 x=power < -16445 ? zero : power < -16382 ? new(1UL<<(power+16445),0) : E80Power(power);
            foreach(bool negative in new[]{false,true})
            {
                x.SignExponent=(ushort)((x.SignExponent&0x7fff)|(negative?0x8000:0));
                yield return(x,one); yield return(one,x); yield return(x,x);
                E80 opposite=x; opposite.SignExponent^=0x8000; yield return(x,opposite);
            }
        }
        foreach(ulong sig in new ulong[]{1,2,3,0x3fffffffffffffff,0x7ffffffffffffffe,0x7fffffffffffffff,
            0x8000000000000000,0x8000000000000001,0xbfffffffffffffff,0xc000000000000000,ulong.MaxValue})
        foreach(int exp in new[]{0,1,0x3ffe,0x3fff,0x4000,0x7ffe,0x7fff})
        {
            E80 x=new(sig,(ushort)exp); yield return(x,one); yield return(x,x);
            yield return(x,E80Power(-1)); yield return(x,E80Power(1));
            E80 opposite=x; opposite.SignExponent^=0x8000; yield return(x,opposite);
        }
        foreach(int p in new[]{24,53,64})
        {
            yield return(one,E80Power(-p)); // exact halfway, even low significand
            yield return(new(0x8000000000000000UL+(1UL<<(64-p)),0x3fff),E80Power(-p));
            yield return(new(ulong.MaxValue,0x7ffe),E80Power(16383));
            yield return(new(1,0),E80Power(-1)); // half minimum subnormal
        }
        E80[] specials=[zero,new(0,0x8000),new(0x8000000000000000,0x7fff),new(0x8000000000000000,0xffff),
            new(0xc000000000000001,0x7fff),new(0xc000000000000002,0xffff),
            new(0x8000000000000001,0x7fff),new(0x8000000000000002,0xffff),
            new(0xc000000000000001,0xffff),new(0x8000000000000001,0xffff),
            new(0x7fffffffffffffff,0x7fff),new(0,0x4000),new(1,0),one];
        foreach(E80 a in specials)foreach(E80 b in specials)yield return(a,b);
        var rng=new Random(123);
        byte[] random=new byte[20];
        for(int i=0;i<512;++i)
        {
            rng.NextBytes(random);
            ulong a=BinaryPrimitives.ReadUInt64LittleEndian(random)|0x8000000000000000;
            ulong b=BinaryPrimitives.ReadUInt64LittleEndian(random.AsSpan(8))|0x8000000000000000;
            ushort ea=(ushort)(BinaryPrimitives.ReadUInt16LittleEndian(random.AsSpan(16))&0x7ffe);
            ushort eb=(ushort)(BinaryPrimitives.ReadUInt16LittleEndian(random.AsSpan(18))&0x7ffe);
            yield return(new(a,ea),new(b,(ushort)(eb|((i&1)<<15))));
        }
    }

    readonly record struct E80Expected(E80 Normal,E80 Wrapped,ushort NormalStatus,ushort WrappedStatus,
        uint Classification,FullExtended80Admission Admission=FullExtended80Admission.Complete);

    // Independent test-only exact dyadic/rational oracle. It uses unbounded BigInteger
    // arithmetic over original encodings, never the product's two-limb/sticky
    // algorithm or restoring division, never double/decimal/host FP or interpreter/GetOp results.
    static E80Expected ReferenceE80(E80Op op,E80 a,E80 b,ushort cw)
    {
        int ca=E80Class(a), cb=E80Class(b), sa=a.SignExponent>>15, sb=b.SignExponent>>15;
        E80Expected Reject(FullExtended80Admission why) => new(default,default,0,0,0,why);
        if(ca==5||cb==5)return new(new(0xc000000000000000,0xffff),default,1,0,97);
        if(ca is 3 or 4 || cb is 3 or 4)
        {
            bool ie=ca==4||cb==4;
            // Stable maximum over original source encodings: class rank q>s>
            // non-NaN, then significand. Strict replacement preserves LEFT
            // on equality without consulting sign or the arithmetic operation.
            int rankA=ca==3?2:ca==4?1:0, rankB=cb==3?2:cb==4?1:0;
            E80 chosen=rankB>rankA || rankB==rankA && b.Significand>a.Significand ? b:a;
            chosen.Significand|=0x4000000000000000;
            return new(chosen,default,(ushort)(ie?1:0),0,(uint)(65|(ie?32:0)));
        }
        // IEEE/Intel093 4.9.2: zero-divide preempts denormal DE. Infinity/zero
        // is exact infinity and does not report ZE (8.5.3 finite-nonzero rule).
        if(op==E80Op.Divide && cb==0)
        {
            if(ca==0)return new(new(0xc000000000000000,0xffff),default,1,0,97);
            bool infinite=ca==2;
            return new(new(0x8000000000000000,(ushort)(0x7fff|(sa^sb)<<15)),default,
                (ushort)(infinite?0:4),0,(uint)(infinite?145:161));
        }
        ushort de=(ushort)(ca==6||cb==6?2:0); uint pre=(uint)(de==0?0:32);
        if(op==E80Op.Subtract)sb^=1;
        if(ca==2||cb==2)
        {
            bool invalid=op==E80Op.Divide ? ca==2&&cb==2 :
                op==E80Op.Multiply ? ca==0||cb==0 : ca==2&&cb==2&&sa!=sb;
            if(invalid)return new(new(0xc000000000000000,0xffff),default,1,0,97);
            if(op==E80Op.Divide && ca!=2)
                return new(new(0,(ushort)((sa^sb)<<15)),default,de,0,17|pre);
            int sign=op is E80Op.Multiply or E80Op.Divide?sa^sb:ca==2?sa:sb;
            return new(new(0x8000000000000000,(ushort)(0x7fff|sign<<15)),default,de,0,145|pre);
        }
        int ea=Math.Max(a.SignExponent&0x7fff,1)-16446;
        int eb=Math.Max(b.SignExponent&0x7fff,1)-16446;
        BigInteger magnitude, denominator=BigInteger.One; int scale; bool negative;
        if(op==E80Op.Divide)
        { magnitude=a.Significand; denominator=b.Significand; scale=ea-eb; negative=(sa^sb)!=0; }
        else if(op==E80Op.Multiply)
        { magnitude=(BigInteger)a.Significand*b.Significand; scale=ea+eb; negative=(sa^sb)!=0; }
        else
        {
            scale=Math.Min(ea,eb);
            BigInteger sum=((BigInteger)a.Significand<<(ea-scale))*(sa==0?1:-1)+
                ((BigInteger)b.Significand<<(eb-scale))*(sb==0?1:-1);
            negative=sum.Sign<0; magnitude=BigInteger.Abs(sum);
            if(magnitude.IsZero)negative=ca==0&&cb==0&&sa==sb ? sa!=0 : (cw>>10&3)==1;
        }
        if(magnitude.IsZero)return new(new(0,(ushort)(negative?0x8000:0)),default,de,0,17|pre);
        int precision=(cw>>8&3) switch{0=>24,2=>53,_=>64}, rc=cw>>10&3;
        int e=E80RatioExponent(magnitude,denominator)+scale, quantum=e-precision+1;
        BigInteger pc=E80QuantizeRational(magnitude,denominator,scale,quantum,rc,negative,out bool pcInexact,out bool pcUp);
        int roundedExponent=checked((int)pc.GetBitLength())-1+quantum;
        ushort pcStatus=(ushort)(de|(pcInexact?32:0)|(pcUp?512:0));
        E80 wrapped=default; ushort wrappedStatus=0; uint classification=1|pre;
        if(roundedExponent>16383)
        {
            classification|=10;
            wrapped=E80PackExact(pc,quantum-24576,negative); wrappedStatus=(ushort)(pcStatus|8);
            bool inf=rc==0 || rc==1&&negative || rc==2&&!negative;
            E80 normal=inf ? new(0x8000000000000000,(ushort)(0x7fff|(negative?0x8000:0))) :
                new(ulong.MaxValue<<(64-precision),(ushort)(0x7ffe|(negative?0x8000:0)));
            return new(normal,wrapped,(ushort)(de|8|32|(inf?512:0)),wrappedStatus,classification);
        }
        bool tiny=roundedExponent < -16382;
        if(tiny)
        {
            classification|=6; wrapped=E80PackExact(pc,quantum+24576,negative);
            wrappedStatus=(ushort)(pcStatus|16);
        }
        int maskedQuantum=Math.Max(e,-16382)-precision+1;
        BigInteger masked=E80QuantizeRational(magnitude,denominator,scale,maskedQuantum,rc,negative,out bool inexact,out bool up);
        ushort status=(ushort)(de|(inexact?32:0)|(up?512:0)|(tiny&&inexact?16:0));
        if(!inexact)classification|=16;
        return new(E80PackExact(masked,maskedQuantum,negative),wrapped,status,wrappedStatus,classification);
    }

    static int E80Class(E80 x)
    {
        int exp=x.SignExponent&0x7fff; ulong sig=x.Significand;
        if(exp==0)return sig==0?0:6;
        if((sig&0x8000000000000000)==0)return 5;
        if(exp!=0x7fff)return 1;
        return sig==0x8000000000000000?2:(sig&0x4000000000000000)!=0?3:4;
    }
    static BigInteger E80Quantize(BigInteger n,int scale,int quantum,int rc,bool negative,
        out bool inexact,out bool up)
    {
        int cut=quantum-scale; inexact=up=false;
        if(cut<=0)return n<<-cut;
        BigInteger q=n>>cut, remainder=n-(q<<cut); inexact=!remainder.IsZero;
        if(!inexact)return q;
        if(rc==0)
        { BigInteger half=BigInteger.One<<(cut-1); up=remainder>half || remainder==half&&!q.IsEven; }
        else up=rc==1&&negative || rc==2&&!negative;
        return up?q+1:q;
    }
    // Exact rational rounding at a selected power-of-two quantum, using a single
    // arbitrary-precision DivRem. Independent of product quotient-bit generation.
    static BigInteger E80QuantizeRational(BigInteger n,BigInteger denominator,int scale,int quantum,
        int rc,bool negative,out bool inexact,out bool up)
    {
        if(denominator.IsOne)return E80Quantize(n,scale,quantum,rc,negative,out inexact,out up);
        int shift=scale-quantum;
        if(shift>=0)n<<=shift; else denominator<<=-shift;
        BigInteger q=BigInteger.DivRem(n,denominator,out BigInteger remainder);
        inexact=!remainder.IsZero; up=false;
        if(inexact)
            up=rc==0 ? remainder*2>denominator || remainder*2==denominator&&!q.IsEven :
                rc==1&&negative || rc==2&&!negative;
        return up?q+1:q;
    }
    static int E80RatioExponent(BigInteger n,BigInteger denominator)
    {
        int e=checked((int)n.GetBitLength()-(int)denominator.GetBitLength());
        if(e>=0 ? n<(denominator<<e) : (n<<-e)<denominator)--e;
        return e;
    }
    static E80 E80PackExact(BigInteger n,int scale,bool negative)
    {
        ushort sign=(ushort)(negative?0x8000:0); if(n.IsZero)return new(0,sign);
        int bits=checked((int)n.GetBitLength()), exponent=bits-1+scale;
        if(exponent < -16382)
        {
            int shift=scale+16445;
            return new(checked((ulong)(shift>=0?n<<shift:n>>-shift)),sign);
        }
        BigInteger sig=bits<=64?n<<(64-bits):n>>(bits-64);
        return new(checked((ulong)sig),(ushort)(sign|exponent+16383));
    }
    static void CheckE80ReferenceAnchors()
    {
        E80 one=E80Power(0), quantum=E80Power(-63);
        var r=ReferenceE80(E80Op.Add,one,quantum,0x37f);
        Check(E80Equal(r.Normal,new(0x8000000000000001,0x3fff)) && r.NormalStatus==0,
            "R123 oracle exact 1+2^-63 distinguishes true64 significand from double");
        r=ReferenceE80(E80Op.Subtract,new(0x8000000000000001,0x3fff),one,0x37f);
        Check(E80Equal(r.Normal,quantum) && r.NormalStatus==0,"R123 oracle cancellation retains the 64th bit");
        r=ReferenceE80(E80Op.Multiply,E80Power(-16382),E80Power(-1),0x37f);
        Check(E80Equal(r.Normal,new(0x4000000000000000,0)) && r.NormalStatus==0 &&
            r.WrappedStatus==16 && (r.Classification&6)==6,"R123 tiny exact masked noUE but unmasked wrap UE");
        r=ReferenceE80(E80Op.Multiply,new(1,0),E80Power(-1),0x37f);
        Check(E80Equal(r.Normal,default) && r.NormalStatus==50 && r.WrappedStatus==18,
            "R123 half-minimum subnormal ties-even: masked DE/UE/PE, wrapped exact DE/UE");
        r=ReferenceE80(E80Op.Add,one,E80Power(-64),0x37f);
        Check(E80Equal(r.Normal,one) && r.NormalStatus==32,"R123 oracle nearest-even tie no C1");
        r=ReferenceE80(E80Op.Add,new(0x8000000000000001,0x3fff),E80Power(-64),0x37f);
        Check(E80Equal(r.Normal,new(0x8000000000000002,0x3fff)) && r.NormalStatus==544,
            "R123 oracle nearest-even odd tie increments magnitude and C1");
        r=ReferenceE80(E80Op.Divide,one,new(0xc000000000000000,0x4000),0x37f);
        Check(E80Equal(r.Normal,new(0xaaaaaaaaaaaaaaab,0x3ffd)) && r.NormalStatus==544 &&
            r.Classification==1,"R125 rational 1/3: true PC64 recurring quotient rounds upward");
        r=ReferenceE80(E80Op.Divide,one,E80Power(1),0x37f);
        Check(E80Equal(r.Normal,E80Power(-1)) && r.NormalStatus==0 && r.Classification==17,
            "R125 rational 1/2 is exact");
        r=ReferenceE80(E80Op.Divide,new(1,0),E80Power(-16382),0x37f);
        Check(E80Equal(r.Normal,E80Power(-63)) && r.NormalStatus==2 && r.Classification==49,
            "R125 minimum subnormal / minimum normal retains exponent and masked DE");
        r=ReferenceE80(E80Op.Divide,E80Power(-16382),E80Power(1),0x37f);
        Check(E80Equal(r.Normal,new(0x4000000000000000,0)) && r.NormalStatus==0 &&
            r.WrappedStatus==16 && (r.Classification&6)==6,
            "R125 tiny exact quotient: masked noUE, unmasked wrapped UE");
        r=ReferenceE80(E80Op.Divide,new(1,0),E80Power(1),0x37f);
        Check(E80Equal(r.Normal,default) && r.NormalStatus==50 && r.WrappedStatus==18,
            "R125 quotient half-minimum subnormal single rounding and exact wrapped result");
        r=ReferenceE80(E80Op.Divide,new(1,0),default,0x37f);
        Check(E80Equal(r.Normal,new(0x8000000000000000,0x7fff)) && r.NormalStatus==4 &&
            r.Classification==161,"R125 zero-divide preempts denormal DE even when masked");
        r=ReferenceE80(E80Op.Divide,new(0x8000000000000000,0xffff),new(0,0x8000),0x37f);
        Check(E80Equal(r.Normal,new(0x8000000000000000,0x7fff)) && r.NormalStatus==0 &&
            r.Classification==145,"R125 infinity / zero: XOR sign, no zero-divide exception");
        r=ReferenceE80(E80Op.Divide,new(0,0x8000),E80Power(0),0x37f);
        Check(E80Equal(r.Normal,new(0,0x8000)) && r.NormalStatus==0 && r.Classification==17,
            "R125 signed zero quotient uses XOR sign in every RC");
        r=ReferenceE80(E80Op.Divide,new(ulong.MaxValue,0x7ffe),new(1,0),0x37f);
        Check(E80Equal(r.Normal,new(0x8000000000000000,0x7fff)) && r.NormalStatus==554 &&
            E80Equal(r.Wrapped,new(ulong.MaxValue,0x603b)) && r.WrappedStatus==10 && r.Classification==43,
            "R125 largest finite / minimum subnormal: exact significand, exponent wraps down24576");
        r=ReferenceE80(E80Op.Divide,new(1,0),new(ulong.MaxValue,0x7ffe),0x37f);
        Check(E80Equal(r.Normal,default) && r.NormalStatus==50 &&
            E80Equal(r.Wrapped,new(0x8000000000000001,0x1fc2)) && r.WrappedStatus==562 && r.Classification==39,
            "R125 minimum subnormal / largest finite: sticky rational quotient, exponent wraps up24576");
    }
}
