using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using Managed.Emulation.Execution;
using Transform80Value = Managed.Emulation.Execution.FullExtended80;
using Transform80Op = Managed.Emulation.Execution.FullExtended80Transform;

unsafe partial class Program
{
    // Parent wires this entry. ALL tests/emissions/native calls AUTHORED_UNRUN.
    static void CheckFullExtended80Transforms()
    {
        CheckTransform80Literals();CheckTransform80Emission();
        foreach(Transform80Op op in Enum.GetValues<Transform80Op>())
        foreach(bool windows in new[]{false,true})foreach(bool continuation in new[]{false,true})
        {
            using var code=Transform80Code(op,windows,continuation);
            using var ret=Transform80Publish([0xf3,0x0f,0x1e,0xfa,0xc3]);
            using var entry=Transform80Adapter(code.Entry,windows,continuation);
            nint tail=continuation?ret.Entry:0;
            foreach(var x in Transform80Cases())
            foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})foreach(int masks in new[]{0,63})
            {
                ushort cw=(ushort)(0x40|masks|(pc<<8)|(rc<<10));
                Transform80Invoke(entry.Entry,op,x,cw,Transform80Reference(op,x,cw),tail);
            }
            Transform80NativeLiterals(entry.Entry,op,tail);
            Transform80Protocol(entry.Entry,op,tail);Transform80Warm(entry.Entry,tail);
            using var probe=Transform80PreservationProbe(entry.Entry);
            var f=Transform80Frame(new(0x8000000000000001,0x3fff),default,0x37f,tail);
            Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)probe.Entry)(&f)==0,
                "R136 native nonvolatile GPR union preserved");
            Transform80ExponentSweep(entry.Entry,op,tail);
        }
    }
    readonly record struct Transform80Expected(Transform80Value Normal,Transform80Value Exponent,
        ushort Masked,ushort Events,uint Metadata,uint Admission=0,bool Root=false);
    static NativeCodeAllocation Transform80Publish(ReadOnlySpan<byte> b)
    {
        Check(NativeCodeAllocation.TryCreate(b,out var owner,out string why,requireNativeFlags:false),why);
        return owner!;
    }
    static NativeCodeAllocation Transform80Code(Transform80Op op,bool windows,bool continuation)
    {
        byte[] b=new byte[FullExtended80TransformEmitter.CodeCapacity];int n;
        bool ok=continuation?FullExtended80TransformEmitter.TryEmitContinuation(op,b,out n):
            FullExtended80TransformEmitter.TryEmit(op,b,out n,windows);
        Check(ok&&n>0&&n<=b.Length,"R136 bounded specialized transform emission");
        return Transform80Publish(b.AsSpan(0,n));
    }
    static NativeCodeAllocation Transform80Adapter(nint target,bool windows,bool continuation)
    {
        var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};bool hostWin=OperatingSystem.IsWindows();
        if(continuation)
        {b.AddRange(hostWin?new byte[]{0x49,0x89,0xcb}:new byte[]{0x49,0x89,0xfb});}
        else if(windows!=hostWin)
        {
            if(windows)b.AddRange(new byte[]{0x48,0x89,0xf9,0x48,0x83,0xec,0x28}); // RCX=RDI; Win64 home space/alignment
            else b.AddRange(new byte[]{0x57,0x48,0x89,0xcf,0x48,0x83,0xec,0x20});
        }
        b.AddRange(new byte[]{0x48,0xb8});Transform80U64(b,(ulong)target);
        if(!continuation&&hostWin!=windows)
        {
            b.AddRange(new byte[]{0xff,0xd0,0x48,0x83,0xc4,(byte)(windows?0x28:0x20)});
            if(hostWin)b.Add(0x5f);
            b.Add(0xc3);
        }
        else b.AddRange(new byte[]{0xff,0xe0});
        return Transform80Publish(b.ToArray());
    }
    static void Transform80U64(List<byte> b,ulong v){for(int i=0;i<8;i++)b.Add((byte)(v>>(8*i)));}
    static void Transform80I32(List<byte> b,int v){for(int i=0;i<4;i++)b.Add((byte)(v>>(8*i)));}

    // Test-only synchronous machine-code wrapper, with normal native CALL/RET.
    // Saves/restores the SysV/Win64 union of nonvolatile GPRs, poisons each,
    // then checks after kernel. SysV RDI is checked against its frame argument.
    // No async stack walking, host FP or semantic helper. Does not qualify
    // asynchronous unwind or Windows execution until actually run there.
    static NativeCodeAllocation Transform80PreservationProbe(nint target)
    {
        bool win=OperatingSystem.IsWindows();int[] regs=new[]{3,5,6,7,12,13,14,15};
        var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};
        foreach(int r in regs){if(r>=8)b.Add(0x41);b.Add((byte)(0x50+(r&7)));}
        b.Add((byte)(win?0x51:0x57)); // save incoming frame; even regs + one push aligns CALL
        b.AddRange(new byte[]{0x48,0x83,0xec,0x20});
        for(int i=0;i<regs.Length;i++)
        {int r=regs[i];b.Add((byte)(r>=8?0x49:0x48));b.Add((byte)(0xb8+(r&7)));Transform80U64(b,0x123456789abc0000UL+(uint)i);}
        // Reload argument from above shadow space after poisoning RDI on Win64.
        b.AddRange(win?new byte[]{0x48,0x8b,0x4c,0x24,0x20}:new byte[]{0x48,0x8b,0x7c,0x24,0x20});
        b.AddRange(new byte[]{0x48,0xb8});Transform80U64(b,(ulong)target);b.AddRange(new byte[]{0xff,0xd0,0x45,0x31,0xd2});
        for(int i=0;i<regs.Length;i++)
        {
            int r=regs[i];
            if(!win&&r==7)b.AddRange(new byte[]{0x48,0x8b,0x44,0x24,0x20});
            else {b.AddRange(new byte[]{0x48,0xb8});Transform80U64(b,0x123456789abc0000UL+(uint)i);}
            b.Add((byte)(r>=8?0x49:0x48));b.Add(0x39);b.Add((byte)(0xc0|(r&7)));
            b.AddRange(new byte[]{0x74,0x07,0x41,0x81,0xca});Transform80I32(b,1<<i);
        }
        b.AddRange(new byte[]{0x44,0x89,0xd0,0x48,0x83,0xc4,0x28});
        for(int i=regs.Length-1;i>=0;i--){int r=regs[i];if(r>=8)b.Add(0x41);b.Add((byte)(0x58+(r&7)));}
        b.Add(0xc3);return Transform80Publish(b.ToArray());
    }
    static FullExtended80WorkFrame Transform80Frame(Transform80Value left,Transform80Value right,ushort cw,nint continuation)=>new()
    {Version=1,Size=384,ControlWord=cw,Left=left,Right=right,Continuation=(ulong)continuation};
    static bool Transform80Same(Transform80Value a,Transform80Value b)=>new ReadOnlySpan<byte>(&a,16).SequenceEqual(new ReadOnlySpan<byte>(&b,16));
    static void CheckTransform80Emission()
    {
        byte[] b=new byte[FullExtended80TransformEmitter.CodeCapacity+16];
        foreach(Transform80Op op in Enum.GetValues<Transform80Op>())foreach(bool win in new[]{false,true})foreach(bool cont in new[]{false,true})
        {
            int n;bool ok=cont?FullExtended80TransformEmitter.TryEmitContinuation(op,b,out n):FullExtended80TransformEmitter.TryEmit(op,b,out n,win);
            Check(ok&&n>0&&n<=FullExtended80TransformEmitter.CodeCapacity,"R136 bounded specialized transforms");
            byte[] image=b.AsSpan(0,n).ToArray();
            Check(image.AsSpan(0,4).SequenceEqual(new byte[]{0xf3,0x0f,0x1e,0xfa}),"R136 CET leaf prefix");
            Check(cont?image.AsSpan(n-7).SequenceEqual(new byte[]{0x41,0xff,0xa3,88,0,0,0}):image[n-1]==0xc3,
                "R136 literal RET or R11 JMP[88] tail");
            for(int size=0;size<n;size++)
            {
                Array.Fill(b,(byte)0xb7);Span<byte> dst=b.AsSpan(8,size);
                bool shortOk=cont?FullExtended80TransformEmitter.TryEmitContinuation(op,dst,out int m):FullExtended80TransformEmitter.TryEmit(op,dst,out m,win);
                Check(!shortOk&&m==0&&b.AsSpan(0,8).IndexOfAnyExcept((byte)0xb7)<0&&
                    b.AsSpan(8+size).IndexOfAnyExcept((byte)0xb7)<0,"R136 EVERY short span, zero length, guards");
            }
            Array.Fill(b,(byte)0xb7);
            bool full=cont?FullExtended80TransformEmitter.TryEmitContinuation(op,b,out int count):FullExtended80TransformEmitter.TryEmit(op,b,out count,win);
            Check(full&&count==n&&b.AsSpan(n).IndexOfAnyExcept((byte)0xb7)<0,"R136 deterministic bounded emission and untouched tail");
            for(int i=0;i<16;i++)if(cont)FullExtended80TransformEmitter.TryEmitContinuation(op,b,out _);else FullExtended80TransformEmitter.TryEmit(op,b,out _,win);
            long baseline=GC.GetAllocatedBytesForCurrentThread();bool all=true;
            for(int i=0;i<128;i++)all&=cont?FullExtended80TransformEmitter.TryEmitContinuation(op,b,out _):FullExtended80TransformEmitter.TryEmit(op,b,out _,win);
            Check(all&&GC.GetAllocatedBytesForCurrentThread()==baseline,"R136 warm zero-allocation emitter");
        }
        Array.Fill(b,(byte)0xb7);
        Check(!FullExtended80TransformEmitter.TryEmit((Transform80Op)255,b,out int bad)&&bad==0&&b.AsSpan().IndexOfAnyExcept((byte)0xb7)<0&&
            !FullExtended80TransformEmitter.TryEmitContinuation((Transform80Op)255,b,out bad)&&bad==0,"R136 unknown selector false/zero/no writes");
    }
    static void Transform80Warm(nint entry,nint tail)
    {
        var f=Transform80Frame(new(0xc000000000000001,0x3fff),new(1,0),0x37f,tail);
        for(int i=0;i<16;i++)((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        long before=GC.GetAllocatedBytesForCurrentThread();uint result=0;
        for(int i=0;i<256;i++)result|=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        Check(result==0&&GC.GetAllocatedBytesForCurrentThread()==before,"R136 warm zero-allocation native kernel");
    }

    static void Transform80Invoke(nint entry,Transform80Op op,Transform80Value x,ushort cw,
        Transform80Expected expected,nint continuation)
    {
        byte* guarded=stackalloc byte[416];new Span<byte>(guarded,416).Fill(0xad);
        var f=(FullExtended80WorkFrame*)(guarded+16);
        *f=Transform80Frame(x,new(0x4000000000000001,0xffff),cw,continuation);
        f->Reserved10=0x1133;f->Left.Reserved10=0x2468;f->Left.Reserved12=0xabcdef01;
        f->Right.Reserved10=0x1357;f->Right.Reserved12=0x12345678;
        new Span<byte>((byte*)f+96,288).Fill(0xad);
        byte* before=stackalloc byte[384];new ReadOnlySpan<byte>(f,384).CopyTo(new Span<byte>(before,384));
        uint admission=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(f);
        Check(admission==expected.Admission&&(uint)f->Admission==expected.Admission,"R136 EAX/frame admission");
        Check(Transform80Same(f->Normal,expected.Normal)&&Transform80Same(f->Wrapped,expected.Exponent)&&
            f->NormalStatus==expected.Masked&&f->WrappedStatus==expected.Events&&(uint)f->Classification==expected.Metadata,
            "R136 independent rational square-root / exact extract, status priority and typed metadata");
        Check(new ReadOnlySpan<byte>(f,12).SequenceEqual(new ReadOnlySpan<byte>(before,12))&&
            new ReadOnlySpan<byte>((byte*)f+16,32).SequenceEqual(new ReadOnlySpan<byte>(before+16,32))&&
            new ReadOnlySpan<byte>((byte*)f+88,8).SequenceEqual(new ReadOnlySpan<byte>(before+88,8))&&
            new ReadOnlySpan<byte>((byte*)f+228,156).SequenceEqual(new ReadOnlySpan<byte>(before+228,156)),
            "R136 all input/control/padding/Right/continuation/reserved and unused scratch immutable");
        if(expected.Root)Check(BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>((byte*)f+124,4))==128,
            "R136 finite positive sqrt uses exactly128 iterations, independent of exponent distance");
        if(admission!=0)Check(new ReadOnlySpan<byte>((byte*)f+96,160).SequenceEqual(new ReadOnlySpan<byte>(before+96,160)),
            "R136 protocol rejects without scratch writes");
        Check(new ReadOnlySpan<byte>(guarded,16).IndexOfAnyExcept((byte)0xad)<0&&
            new ReadOnlySpan<byte>(guarded+400,16).IndexOfAnyExcept((byte)0xad)<0,"R136 frame guards");
        // Repeat after replacing ignored Right with a distinct class/padding.
        f->Right=new(0xcfffffffffffffff,0x7fff);f->Right.Reserved10=0xffff;f->Right.Reserved12=uint.MaxValue;
        admission=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(f);
        Check(admission==expected.Admission&&Transform80Same(f->Normal,expected.Normal)&&
            Transform80Same(f->Wrapped,expected.Exponent)&&f->NormalStatus==expected.Masked&&
            f->WrappedStatus==expected.Events&&(uint)f->Classification==expected.Metadata,
            "R136 Right ignored even if unsupported/NaN");
    }
    static void Transform80Protocol(nint entry,Transform80Op op,nint continuation)
    {
        foreach((uint version,uint size,uint reason) in new[]{(0u,384u,1u),(2u,384u,1u),
            (1u,0u,2u),(1u,383u,2u),(1u,385u,2u),(1u,uint.MaxValue,2u)})
        {
            var f=Transform80Frame(default,default,0x17f,continuation);f.Version=version;f.Size=size;
            new Span<byte>((byte*)&f+48,40).Fill(0xef);new Span<byte>((byte*)&f+96,288).Fill(0xa7);
            Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f)==reason&&(uint)f.Admission==reason&&
                new ReadOnlySpan<byte>((byte*)&f+48,40).IndexOfAnyExcept((byte)0)<0&&
                new ReadOnlySpan<byte>((byte*)&f+96,288).IndexOfAnyExcept((byte)0xa7)<0,
                "R136 version before size rejection, cleared outputs, untouched scratch/reserved");
        }
        Transform80Invoke(entry,op,new(0x8000000000000000,0x3fff),0x17f,
            Transform80Reference(op,new(0x8000000000000000,0x3fff),0x17f),continuation);
    }
    enum Transform80Kind { Finite, Infinity, QuietNaN, SignalingNaN, Unsupported }
    static Transform80Kind Transform80Class(Transform80Value x)
    {
        int biased=x.SignExponent&0x7fff;
        if(biased!=0&&(x.Significand&0x8000000000000000)==0)return Transform80Kind.Unsupported;
        if(biased!=0x7fff)return Transform80Kind.Finite;
        if((x.Significand&0x7fffffffffffffff)==0)return Transform80Kind.Infinity;
        return (x.Significand&0x4000000000000000)==0?Transform80Kind.SignalingNaN:Transform80Kind.QuietNaN;
    }
    // Test oracle: independent exact rational binary search. No native digit-pair
    // recurrence, remainder jam, Math.Sqrt, double or old arithmetic reference.
    static (Transform80Value Value,ushort Status) Transform80SqrtRational(Transform80Value x,int p,int rc)
    {
        BigInteger s=x.Significand;
        int scale=Math.Max(1,x.SignExponent&0x7fff)-16383-63;
        int log=(int)s.GetBitLength()-1+scale;
        int e=log>=0?log/2:(log-1)/2;
        int q=scale-2*(e-(p-1));
        BigInteger numerator=s,denominator=BigInteger.One;
        if(q>=0)numerator<<=q;else denominator<<=-q;
        BigInteger low=BigInteger.Zero,high=BigInteger.One<<p;
        while(high-low>1)
        {
            BigInteger middle=(low+high)/2;
            if(middle*middle*denominator<=numerator)low=middle;else high=middle;
        }
        bool inexact=low*low*denominator!=numerator,up=false;
        if(inexact)
        {
            if(rc==2)up=true;
            else if(rc==0)
            {
                BigInteger twice=2*low+1;
                int cmp=(4*numerator).CompareTo(twice*twice*denominator);
                up=cmp>0||(cmp==0&&!low.IsEven);
            }
        }
        if(up)low++;
        if(low==(BigInteger.One<<p)){low>>=1;e++;}
        return(new((ulong)(low<<(64-p)),(ushort)(e+16383)),(ushort)((inexact?32:0)|(up?512:0)));
    }
    static Transform80Value Transform80Integer(BigInteger integer)
    {
        if(integer.IsZero)return default;
        ushort sign=(ushort)(integer.Sign<0?0x8000:0);
        integer=BigInteger.Abs(integer);int bits=(int)integer.GetBitLength();
        return new((ulong)(integer<<(64-bits)),(ushort)(sign|(bits-1+16383)));
    }
    static Transform80Expected Transform80Reference(Transform80Op op,Transform80Value x,ushort cw)
    {
        Transform80Kind kind=Transform80Class(x);
        Transform80Value n=default,e=default;ushort sw=0;bool root=false;
        bool negative=(x.SignExponent&0x8000)!=0,finite=kind==Transform80Kind.Finite;
        bool denormal=finite&&(x.SignExponent&0x7fff)==0&&x.Significand!=0;
        if(kind==Transform80Kind.Unsupported)
        {n=new(0xc000000000000000,0xffff);e=n;sw=1;}
        else if(kind is Transform80Kind.SignalingNaN or Transform80Kind.QuietNaN)
        {n=new(x.Significand|0x4000000000000000,x.SignExponent);e=n;sw=(ushort)(kind==Transform80Kind.SignalingNaN?1:0);}
        else if(op==Transform80Op.Sqrt)
        {
            if(x.Significand==0)n=new(0,(ushort)(negative?0x8000:0));
            else if(negative){n=new(0xc000000000000000,0xffff);sw=1;}
            else if(kind==Transform80Kind.Infinity)n=new(x.Significand,x.SignExponent);
            else
            {
                int p=((cw>>8)&3) switch{0=>24,2=>53,_=>64};
                (n,sw)=Transform80SqrtRational(x,p,(cw>>10)&3);root=true;
                if(denormal)sw|=2;
            }
        }
        else if(kind==Transform80Kind.Infinity)
        {n=new(x.Significand,x.SignExponent);e=new(0x8000000000000000,0x7fff);}
        else if(x.Significand==0)
        {n=new(0,(ushort)(negative?0x8000:0));e=new(0x8000000000000000,0xffff);sw=4;}
        else
        {
            int bits=(int)((BigInteger)x.Significand).GetBitLength();
            int exponent=bits-1+Math.Max(1,x.SignExponent&0x7fff)-16383-63;
            n=new((ulong)((BigInteger)x.Significand<<(64-bits)),(ushort)(0x3fff|(negative?0x8000:0)));
            e=Transform80Integer(exponent);if(denormal)sw=2;
        }
        if(op==Transform80Op.Sqrt)e=default;
        return Transform80Finish(op,n,e,sw,cw,finite,root);
    }
    static Transform80Expected Transform80Finish(Transform80Op op,Transform80Value n,Transform80Value e,
        ushort masked,ushort cw,bool finite,bool root=false)
    {
        bool extract=op==Transform80Op.Extract;uint meta=extract?2u:1u;
        ushort pre=(ushort)((masked&1)!=0&&(cw&1)==0?1:
            (masked&2)!=0&&(cw&2)==0?2:(masked&4)!=0&&(cw&4)==0?4:0);
        ushort events=pre!=0?pre:masked;
        if(pre!=0){n=default;e=default;meta|=1024u|(extract?2048u:0u);}
        else
        {
            meta|=4u|(extract?8u:8192u);
            if(finite&&(masked&33)==0)meta|=16;
            if(!extract&&(masked&32)!=0&&(cw&32)==0)meta|=4096;
        }
        if((events&1)!=0)meta|=32;if((events&2)!=0)meta|=64;if((events&4)!=0)meta|=128;
        if((events&32)!=0)meta|=256;if((events&512)!=0)meta|=512;
        return new(n,e,masked,events,meta,Root:root);
    }
    readonly record struct Transform80Literal(Transform80Op Op,Transform80Value Input,
        Transform80Value Normal,Transform80Value Exponent,ushort Masked,int Pc=-1,int Rc=-1);
    static IEnumerable<Transform80Literal> Transform80Literals()
    {
        const ulong J=0x8000000000000000;
        yield return new(Transform80Op.Sqrt,new(J,0x4000),new(0xb504f30000000000,0x3fff),default,32,0,0);
        yield return new(Transform80Op.Sqrt,new(J,0x4000),new(0xb504f333f9de6800,0x3fff),default,544,2,0);
        yield return new(Transform80Op.Sqrt,new(J,0x4000),new(0xb504f333f9de6484,0x3fff),default,32,3,0);
        yield return new(Transform80Op.Sqrt,new(J,0x4000),new(0xb504f333f9de6485,0x3fff),default,544,3,2);
        yield return new(Transform80Op.Sqrt,new(0x9000000000000000,0x4002),new(0xc000000000000000,0x4000),default,0);
        yield return new(Transform80Op.Sqrt,new(J,0x3ffd),new(J,0x3ffe),default,0);
        yield return new(Transform80Op.Sqrt,new(0x8000010000008000,0x3fff),new(J,0x3fff),default,32,0,0);
        yield return new(Transform80Op.Sqrt,new(0x8000010000007fff,0x3fff),new(J,0x3fff),default,32,0,0);
        yield return new(Transform80Op.Sqrt,new(0x8000010000008001,0x3fff),new(0x8000010000000000,0x3fff),default,544,0,0);
        yield return new(Transform80Op.Sqrt,new(0x8000030000048000,0x3fff),new(0x8000020000000000,0x3fff),default,544,0,0);
        yield return new(Transform80Op.Sqrt,new(J+1,0x3fff),new(J,0x3fff),default,32,3,0);
        yield return new(Transform80Op.Sqrt,new(J+1,0x3fff),new(J+1,0x3fff),default,544,3,2);
        yield return new(Transform80Op.Sqrt,new(ulong.MaxValue,0x4000),new(J,0x4000),default,544,3,2);
        yield return new(Transform80Op.Sqrt,new(1,0),new(0xb504f333f9de6484,0x1fe0),default,34,3,0);
        yield return new(Transform80Op.Sqrt,new(J,0),new(J,0x2000),default,2);
        yield return new(Transform80Op.Sqrt,new(0,0x8000),new(0,0x8000),default,0);
        yield return new(Transform80Op.Sqrt,new(0,0),default,default,0);
        yield return new(Transform80Op.Sqrt,new(J,0x7fff),new(J,0x7fff),default,0);
        yield return new(Transform80Op.Sqrt,new(J,0xffff),new(0xc000000000000000,0xffff),default,1);
        yield return new(Transform80Op.Sqrt,new(1,0x8000),new(0xc000000000000000,0xffff),default,1);
        yield return new(Transform80Op.Sqrt,new(J+1,0x8000),new(0xc000000000000000,0xffff),default,1);
        yield return new(Transform80Op.Sqrt,new(0xc000000000000123,0xffff),new(0xc000000000000123,0xffff),default,0);
        yield return new(Transform80Op.Sqrt,new(J+0x123,0xffff),new(0xc000000000000123,0xffff),default,1);
        yield return new(Transform80Op.Sqrt,new(0x4000000000000000,0x4000),new(0xc000000000000000,0xffff),default,1);
        // Parent's 40-row AuthenticAMD control anchors; no guest credit.
        yield return new(Transform80Op.Extract,new(J,0x7fff),new(J,0x7fff),new(J,0x7fff),0);
        yield return new(Transform80Op.Extract,new(J,0xffff),new(J,0xffff),new(J,0x7fff),0);
        yield return new(Transform80Op.Extract,new(0xc000000000000123,0xffff),new(0xc000000000000123,0xffff),new(0xc000000000000123,0xffff),0);
        yield return new(Transform80Op.Extract,new(J+0x123,0x7fff),new(0xc000000000000123,0x7fff),new(0xc000000000000123,0x7fff),1);
        yield return new(Transform80Op.Extract,new(0x4000000000000000,0x4000),new(0xc000000000000000,0xffff),new(0xc000000000000000,0xffff),1);
        yield return new(Transform80Op.Extract,new(0,0),default,new(J,0xffff),4);
        yield return new(Transform80Op.Extract,new(0,0x8000),new(0,0x8000),new(J,0xffff),4);
        yield return new(Transform80Op.Extract,new(1,0),new(J,0x3fff),new(0x807a000000000000,0xc00d),2);
        yield return new(Transform80Op.Extract,new(J+1,0),new(J+1,0x3fff),new(0xfff8000000000000,0xc00c),2);
        yield return new(Transform80Op.Extract,new(0xc000000000000001,0x3fff),new(0xc000000000000001,0x3fff),default,0);
        yield return new(Transform80Op.Extract,new(0xc000000000000001,0xbfff),new(0xc000000000000001,0xbfff),default,0);
        yield return new(Transform80Op.Extract,new(ulong.MaxValue,0x7ffe),new(ulong.MaxValue,0x3fff),new(0xfffc000000000000,0x400c),0);
    }
    static Transform80Expected Transform80LiteralExpected(Transform80Literal row,ushort cw)
    {
        bool finite=Transform80Class(row.Input)==Transform80Kind.Finite;
        bool root=row.Op==Transform80Op.Sqrt&&finite&&row.Input.Significand!=0&&(row.Input.SignExponent&0x8000)==0;
        return Transform80Finish(row.Op,row.Normal,row.Exponent,row.Masked,cw,finite,root);
    }
    static void CheckTransform80Literals()
    {
        foreach(var row in Transform80Literals())
        foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})
        {
            if((row.Pc>=0&&row.Pc!=pc)||(row.Rc>=0&&row.Rc!=rc))continue;
            ushort cw=(ushort)(0x7f|(pc<<8)|(rc<<10));
            Check(Transform80Reference(row.Op,row.Input,cw)==Transform80LiteralExpected(row,cw),
                "R136 independent oracle anchored to literal binary80 outputs/status");
        }
        // Explicit actual-mask priorities, independent literal event assertions.
        var de=Transform80Reference(Transform80Op.Sqrt,new(1,0),0x35d);
        Check(de.Masked==34&&de.Events==2&&de.Normal.Significand==0&&de.Metadata==(1|64|1024),
            "R136 unmasked DE blocks lower PE/C1 and destination");
        var pe=Transform80Reference(Transform80Op.Sqrt,new(0x8000000000000000,0x4000),0x35f);
        Check(pe.Events==32&&pe.Normal.Significand==0xb504f333f9de6484&&
            (pe.Metadata&(4|256|4096|8192))==(4|256|4096|8192),"R136 PE alone completes rounded result");
        var ie=Transform80Reference(Transform80Op.Sqrt,new(1,0x8000),0x37e);
        Check(ie.Masked==1&&ie.Events==1&&ie.Metadata==(1|32|1024),"R136 negative subnormal IE precedes DE");
        var ze=Transform80Reference(Transform80Op.Extract,new(0,0x8000),0x37b);
        Check(ze.Events==4&&ze.Metadata==(2|128|1024|2048)&&Transform80Same(ze.Normal,default)&&Transform80Same(ze.Exponent,default),
            "R136 unmasked Extract ZE suppresses BOTH result images and push");
    }
    static void Transform80NativeLiterals(nint entry,Transform80Op op,nint tail)
    {
        foreach(var row in Transform80Literals())
        {
            if(row.Op!=op)continue;
            foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})
            {
                if((row.Pc>=0&&row.Pc!=pc)||(row.Rc>=0&&row.Rc!=rc))continue;
                for(int masks=0;masks<64;masks++)
                {
                    ushort cw=(ushort)(0x40|masks|(pc<<8)|(rc<<10));
                    Transform80Invoke(entry,op,row.Input,cw,Transform80LiteralExpected(row,cw),tail);
                }
            }
        }
    }
    static IEnumerable<Transform80Value> Transform80Cases()
    {
        foreach(ushort exponent in new ushort[]{0,1,2,0x1fff,0x3ffe,0x3fff,0x4000,0x5fff,0x7ffd,0x7ffe,0x7fff})
        foreach(ulong sig in new ulong[]{0,1,0x3fffffffffffffff,0x4000000000000000,0x7fffffffffffffff,
            0x8000000000000000,0x8000000000000001,0x80000000000007ff,0x8000000000000800,
            0x8000000000000801,0xc000000000000001,ulong.MaxValue})
        foreach(int sign in new[]{0,0x8000})yield return new(sig,(ushort)(exponent|sign));
        foreach(ulong sig in new ulong[]{0x8000010000007fff,0x8000010000008000,0x8000010000008001,
            0x8000030000047fff,0x8000030000048000,0x8000030000048001})yield return new(sig,0x3fff);
        // Every leading position in the subnormal significand, +/- its LSB.
        for(int bit=0;bit<64;bit++)foreach(int sign in new[]{0,0x8000})
        {
            ulong sig=1UL<<bit;yield return new(sig,(ushort)sign);
            if(sig>1)yield return new(sig-1,(ushort)sign);
            if(sig<ulong.MaxValue)yield return new(sig+1,(ushort)sign);
        }
    }
    static void Transform80ExponentSweep(nint entry,Transform80Op op,nint tail)
    {
        // Exactly32829 exponent magnitudes (-16445..16383), two input signs.
        // Extract covers all; Sqrt uses only positive finite roots and negative IE.
        // One deterministic CW per sign/exponent; other grids cross all PC/RC/masks.
        for(int exponent=-16445;exponent<=16383;exponent++)foreach(int sign in new[]{0,0x8000})
        {
            Transform80Value x=exponent< -16382?new(1UL<<(exponent+16445),(ushort)sign):
                new(0x8000000000000001,(ushort)((exponent+16383)|sign));
            int ordinal=exponent+16445,pc=new[]{0,2,3}[ordinal%3],rc=ordinal%4,masks=(ordinal%2==0?63:0);
            ushort cw=(ushort)(0x40|masks|(pc<<8)|(rc<<10));
            Transform80Invoke(entry,op,x,cw,Transform80Reference(op,x,cw),tail);
        }
    }
}
