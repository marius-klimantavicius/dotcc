using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using Managed.Emulation.Execution;
using C80 = Managed.Emulation.Execution.FullExtended80;
using Conv80 = Managed.Emulation.Execution.FullExtended80Conversion;
using Meta80 = Managed.Emulation.Execution.FullExtended80ConversionMetadata;

unsafe partial class Program
{
    // Parent wires this method into a standalone suite. R129 does not compile,
    // emit, publish or execute anything. All native invocations below UNRUN.
    static void CheckFullExtended80Conversions()
    {
        CheckConversion80Literals();
        CheckConversion80Emission();
        foreach (Conv80 op in Enum.GetValues<Conv80>())
        {
            if((uint)op>(uint)Conv80.StoreRaw80)continue;
            foreach(bool windows in new[]{false,true}) foreach(bool continuation in new[]{false,true})
            {
                using var code=Conversion80Code(op,windows,continuation);
                using var ret=Conversion80Publish([0xf3,0x0f,0x1e,0xfa,0xc3]);
                using var entry=Conversion80Adapter(code.Entry,windows,continuation);
                foreach(C80 input in Conversion80Cases(op))
                foreach(int rc in new[]{0,1,2,3}) foreach(int pc in new[]{0,1,2,3})
                foreach(int masks in new[]{0,63})
                {
                    ushort cw=(ushort)(0x40|masks|(pc<<8)|(rc<<10));
                    Conversion80Expected expected=Conversion80Reference(op,input,cw);
                    Conversion80Invoke(entry.Entry,input,cw,expected,continuation?ret.Entry:0);
                }
                Conversion80Protocol(entry.Entry,continuation?ret.Entry:0);
                Conversion80LiteralNative(entry.Entry,op,continuation?ret.Entry:0);
                Conversion80Warm(entry.Entry,op,continuation?ret.Entry:0);
                using var probe=Conversion80PreservationProbe(entry.Entry);
                var frame=Conversion80Frame(new(0x8000000000000001,0x3fff),0x37f,continuation?ret.Entry:0);
                Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)probe.Entry)(&frame)==0,
                    "R129 native callee-save GPR preservation, both target ABIs/entries");
            }
        }
    }
    static readonly int[] Conversion80Masks=BuildConversion80Masks();
    static int[] BuildConversion80Masks(){var a=new int[64];for(int i=0;i<64;i++)a[i]=i;return a;}
    readonly record struct Conversion80Expected(C80 Value,ushort Masked,ushort Events,uint Metadata);

    static NativeCodeAllocation Conversion80Publish(ReadOnlySpan<byte> b)
    {
        Check(NativeCodeAllocation.TryCreate(b,out var owner,out string why,requireNativeFlags:false),why);
        return owner!;
    }
    static NativeCodeAllocation Conversion80Code(Conv80 op,bool windows,bool continuation)
    {
        byte[] b=new byte[FullExtended80ConversionEmitter.CodeCapacity];int n;
        bool ok=continuation?FullExtended80ConversionEmitter.TryEmitContinuation(op,b,out n):
            FullExtended80ConversionEmitter.TryEmit(op,b,out n,windows);
        Check(ok&&n>0&&n<=b.Length,"R129 bounded specialized conversion emission");
        return Conversion80Publish(b.AsSpan(0,n));
    }
    static NativeCodeAllocation Conversion80Adapter(nint target,bool windows,bool continuation)
    {
        var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};bool hostWin=OperatingSystem.IsWindows();
        if(continuation)
        {b.AddRange(hostWin?new byte[]{0x49,0x89,0xcb}:new byte[]{0x49,0x89,0xfb});}
        else if(windows!=hostWin)
        {
            if(windows)b.AddRange(new byte[]{0x48,0x89,0xf9,0x48,0x83,0xec,0x28}); // RCX=RDI; Win64 home space/alignment
            else b.AddRange(new byte[]{0x57,0x48,0x89,0xcf,0x48,0x83,0xec,0x20});
        }
        b.AddRange(new byte[]{0x48,0xb8});Conversion80U64(b,(ulong)target);
        if(!continuation&&hostWin!=windows)
        {
            b.AddRange(new byte[]{0xff,0xd0,0x48,0x83,0xc4,(byte)(windows?0x28:0x20)});
            if(hostWin)b.Add(0x5f);
            b.Add(0xc3);
        }
        else b.AddRange(new byte[]{0xff,0xe0});
        return Conversion80Publish(b.ToArray());
    }
    static void Conversion80U64(List<byte> b,ulong v){for(int i=0;i<8;i++)b.Add((byte)(v>>(8*i)));}
    static void Conversion80I32(List<byte> b,int v){for(int i=0;i<4;i++)b.Add((byte)(v>>(8*i)));}

    // Test-only synchronous machine-code wrapper, with normal native CALL/RET.
    // Saves/restores the SysV/Win64 union of nonvolatile GPRs, poisons each,
    // then checks after kernel. SysV RDI is checked against its frame argument.
    // No async stack walking, host FP or semantic helper. Does not qualify
    // asynchronous unwind or Windows execution until actually run there.
    static NativeCodeAllocation Conversion80PreservationProbe(nint target)
    {
        bool win=OperatingSystem.IsWindows();int[] regs=new[]{3,5,6,7,12,13,14,15};
        var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};
        foreach(int r in regs){if(r>=8)b.Add(0x41);b.Add((byte)(0x50+(r&7)));}
        b.Add((byte)(win?0x51:0x57)); // save incoming frame; even regs + one push aligns CALL
        b.AddRange(new byte[]{0x48,0x83,0xec,0x20});
        for(int i=0;i<regs.Length;i++)
        {int r=regs[i];b.Add((byte)(r>=8?0x49:0x48));b.Add((byte)(0xb8+(r&7)));Conversion80U64(b,0x123456789abc0000UL+(uint)i);}
        // Reload argument from above shadow space after poisoning RDI on Win64.
        b.AddRange(win?new byte[]{0x48,0x8b,0x4c,0x24,0x20}:new byte[]{0x48,0x8b,0x7c,0x24,0x20});
        b.AddRange(new byte[]{0x48,0xb8});Conversion80U64(b,(ulong)target);b.AddRange(new byte[]{0xff,0xd0,0x45,0x31,0xd2});
        for(int i=0;i<regs.Length;i++)
        {
            int r=regs[i];
            if(!win&&r==7)b.AddRange(new byte[]{0x48,0x8b,0x44,0x24,0x20});
            else {b.AddRange(new byte[]{0x48,0xb8});Conversion80U64(b,0x123456789abc0000UL+(uint)i);}
            b.Add((byte)(r>=8?0x49:0x48));b.Add(0x39);b.Add((byte)(0xc0|(r&7)));
            b.AddRange(new byte[]{0x74,0x07,0x41,0x81,0xca});Conversion80I32(b,1<<i);
        }
        b.AddRange(new byte[]{0x44,0x89,0xd0,0x48,0x83,0xc4,0x28});
        for(int i=regs.Length-1;i>=0;i--){int r=regs[i];if(r>=8)b.Add(0x41);b.Add((byte)(0x58+(r&7)));}
        b.Add(0xc3);return Conversion80Publish(b.ToArray());
    }
    static FullExtended80WorkFrame Conversion80Frame(C80 input,ushort cw,nint continuation)=>new()
    {Version=1,Size=384,ControlWord=cw,Left=input,Continuation=(ulong)continuation};
    static bool Conversion80Same(C80 a,C80 b)=>a.Significand==b.Significand&&a.SignExponent==b.SignExponent;
    static void Conversion80Invoke(nint entry,C80 input,ushort cw,Conversion80Expected expected,nint continuation)
    {
        byte* guarded=stackalloc byte[416];new Span<byte>(guarded,416).Fill(0xad);
        var f=(FullExtended80WorkFrame*)(guarded+16);*f=Conversion80Frame(input,cw,continuation);
        f->Reserved10=0x1133;f->Left.Reserved10=0x2468;f->Left.Reserved12=0xabcdef01;
        new Span<byte>((byte*)f+32,16).Fill(0x69);new Span<byte>((byte*)f+96,288).Fill(0xad);
        byte* before=stackalloc byte[384];new ReadOnlySpan<byte>(f,384).CopyTo(new Span<byte>(before,384));
        uint result=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(f);
        Check(result==0&&f->Admission==0,"R129 EAX/frame Complete");
        Check(Conversion80Same(f->Normal,expected.Value)&&f->Normal.Reserved10==0&&f->Normal.Reserved12==0&&
            new ReadOnlySpan<byte>((byte*)f+64,16).IndexOfAnyExcept((byte)0)<0&&
            f->NormalStatus==expected.Masked&&f->WrappedStatus==expected.Events&&(uint)f->Classification==expected.Metadata,
            "R129 independent rational output/masked SW/event priority/metadata");
        Check(new ReadOnlySpan<byte>(f,12).SequenceEqual(new ReadOnlySpan<byte>(before,12))&&
            new ReadOnlySpan<byte>((byte*)f+16,32).SequenceEqual(new ReadOnlySpan<byte>(before+16,32))&&
            new ReadOnlySpan<byte>((byte*)f+88,8).SequenceEqual(new ReadOnlySpan<byte>(before+88,8))&&
            new ReadOnlySpan<byte>((byte*)f+256,128).SequenceEqual(new ReadOnlySpan<byte>(before+256,128)),
            "R129 input/padding/right/continuation/reserved immutable");
        Check(new ReadOnlySpan<byte>(guarded,16).IndexOfAnyExcept((byte)0xad)<0&&
            new ReadOnlySpan<byte>(guarded+400,16).IndexOfAnyExcept((byte)0xad)<0,"R129 frame guard bounds");
    }
    static void Conversion80Protocol(nint entry,nint continuation)
    {
        foreach((uint version,uint size,uint reason) in new[]{(2u,384u,1u),(0u,384u,1u),(1u,0u,2u),(1u,383u,2u),(1u,385u,2u),(1u,uint.MaxValue,2u)})
        {
            var f=Conversion80Frame(default,0x17f,continuation);f.Version=version;f.Size=size;
            Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f)==reason&&
                (uint)f.Admission==reason&&f.NormalStatus==0&&f.WrappedStatus==0&&(uint)f.Classification==0,
                "R129 strict version/size; no output validity on protocol reject");
        }
    }
    static void CheckConversion80Emission()
    {
        byte[] b=new byte[FullExtended80ConversionEmitter.CodeCapacity+16];
        foreach(Conv80 op in Enum.GetValues<Conv80>())foreach(bool win in new[]{false,true})foreach(bool cont in new[]{false,true})
        {
            int n;bool ok=cont?FullExtended80ConversionEmitter.TryEmitContinuation(op,b,out n):FullExtended80ConversionEmitter.TryEmit(op,b,out n,win);
            if((uint)op>(uint)Conv80.StoreRaw80){Check(!ok&&n==0,"R129 packed BCD explicitly MISSING");continue;}
            Check(ok&&n>0&&n<=FullExtended80ConversionEmitter.CodeCapacity,"R129 full emission bounded");
            byte[] image=b.AsSpan(0,n).ToArray();
            Check(image[0]==0xf3&&image[1]==0x0f&&image[2]==0x1e&&image[3]==0xfa,"R129 CET-compatible leaf prefix");
            Check(cont?image.AsSpan(n-7).SequenceEqual(new byte[]{0x41,0xff,0xa3,88,0,0,0}):image[n-1]==0xc3,
                "R129 literal RET / JMP[frame+88] native tails");
            // EVERY shorter destination length, guards on both sides; writer
            // may modify partial payload but must never claim successful code.
            for(int size=0;size<n;size++)
            {
                Array.Fill(b,(byte)0xb7);Span<byte> dst=b.AsSpan(8,size);
                bool shortOk=cont?FullExtended80ConversionEmitter.TryEmitContinuation(op,dst,out int m):
                    FullExtended80ConversionEmitter.TryEmit(op,dst,out m,win);
                Check(!shortOk&&m==0&&b.AsSpan(0,8).IndexOfAnyExcept((byte)0xb7)<0&&
                    b.AsSpan(8+size).IndexOfAnyExcept((byte)0xb7)<0,"R129 all short spans false/zero with guards");
            }
            for(int i=0;i<16;i++)if(cont)FullExtended80ConversionEmitter.TryEmitContinuation(op,b,out _);else FullExtended80ConversionEmitter.TryEmit(op,b,out _,win);
            long baseline=GC.GetAllocatedBytesForCurrentThread();bool all=true;
            for(int i=0;i<128;i++)all&=cont?FullExtended80ConversionEmitter.TryEmitContinuation(op,b,out _):FullExtended80ConversionEmitter.TryEmit(op,b,out _,win);
            Check(all&&GC.GetAllocatedBytesForCurrentThread()==baseline,"R129 zero managed allocation warm emission, both ABIs/entries");
        }
        Array.Fill(b,(byte)0xb7);
        Check(!FullExtended80ConversionEmitter.TryEmit((Conv80)255,b,out int bad)&&bad==0&&b.AsSpan().IndexOfAnyExcept((byte)0xb7)<0&&
            !FullExtended80ConversionEmitter.TryEmitContinuation((Conv80)255,b,out bad)&&bad==0,"R129 unknown selector rejected before writes");
    }
    static void Conversion80Warm(nint entry,Conv80 op,nint continuation)
    {
        var f=Conversion80Frame(new(0x8000000000000001,0x3fff),0x17f,continuation);
        for(int i=0;i<16;i++)((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        long before=GC.GetAllocatedBytesForCurrentThread();uint result=0;
        for(int i=0;i<256;i++)result|=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        Check(result==0&&GC.GetAllocatedBytesForCurrentThread()==before,"R129 zero-allocation native warm invocation, PC01");
    }

    static IEnumerable<C80> Conversion80Cases(Conv80 op)
    {
        if(op is Conv80.LoadBinary32 or Conv80.LoadBinary64)
        {
            int f=op==Conv80.LoadBinary32?23:52,width=op==Conv80.LoadBinary32?32:64,e=width==32?8:11;
            ulong max=(1UL<<e)-1;
            foreach(ulong sign in new[]{0UL,1UL<<(width-1)})
            foreach(ulong exponent in new[]{0UL,1UL,2UL,max/2,max-1,max})
            foreach(ulong fraction in new[]{0UL,1UL,2UL,(1UL<<(f-1))-1,1UL<<(f-1),(1UL<<f)-1})
                yield return new(sign|(exponent<<f)|fraction,0xaaaa); // ignored input suffix
            yield break;
        }
        if(((uint)op >= (uint)Conv80.LoadInt16 && (uint)op <= (uint)Conv80.LoadInt64))
        {
            int width=op==Conv80.LoadInt16?16:op==Conv80.LoadInt32?32:64;
            foreach(ulong n in new[]{0UL,1UL,2UL,3UL,(1UL<<(width-1))-1,1UL<<(width-1),(1UL<<(width-1))+1,ulong.MaxValue,0x0020000000000001UL})
                yield return new(n,0xaaaa);
            yield break;
        }
        foreach(int sign in new[]{0,0x8000})
        {
            // Exponent extremes, ordinary precision, each IEEE boundary, integer
            // range edges and halfway neighbors, canonical/pseudo denormals.
            foreach(int exponent in new[]{0,1,2,0x3fff-1075,0x3fff-1074,0x3fff-1023,0x3fff-1022,
                0x3fff-150,0x3fff-149,0x3fff-127,0x3fff-126,0x3ffe,0x3fff,0x4000,
                0x400d,0x400e,0x401d,0x401e,0x403d,0x403e,0x407e,0x407f,0x43fe,0x43ff,0x7ffe,0x7fff})
            foreach(ulong sig in new[]{0UL,1UL,0x4000000000000000UL,0x7fffffffffffffffUL,0x8000000000000000UL,
                0x8000000000000001UL,0x80000000000003ffUL,0x8000000000000400UL,0x8000000000000401UL,
                0x8000007fffffffffUL,0x8000008000000000UL,0x8000008000000001UL,0xbfffffffffffffffUL,
                0xc000000000000000UL,0xfffffefffffffffeUL,0xffffff0000000000UL,0xffffffffffffffffUL})
                yield return new(sig,(ushort)(sign|exponent));
            foreach(int exponent in new[]{0x400d,0x400e,0x401d,0x401e,0x403d,0x403e})
                foreach(ulong sig in new[]{0xffff000000000000UL,0xffff800000000000UL,0xfffffffe00000000UL,
                    0xffffffff00000000UL,0xfffffffffffffff8UL,0xffffffffffffffffUL})
                    yield return new(sig,(ushort)(sign|exponent));
        }
    }

    // Independent reference: exact nonnegative rational N/D, quantized by
    // BigInteger.DivRem in destination units. No GPR shift/jam simulation.
    static BigInteger Conversion80Round(BigInteger n,BigInteger d,int quantum,int rc,bool negative,out bool inexact,out bool up)
    {
        if(quantum>=0)d<<=quantum;else n<<=-quantum;
        BigInteger q=BigInteger.DivRem(n,d,out var r);inexact=!r.IsZero;
        up=inexact&&(rc==0?(r*2>d||(r*2==d&&!q.IsEven)):rc==1?negative:rc==2&&!negative);
        return up?q+1:q;
    }
    static C80 Conversion80Exact(BigInteger n,int power,bool negative)
    {
        if(n.IsZero)return new(0,(ushort)(negative?0x8000:0));
        int top=(int)n.GetBitLength()-1;
        return new((ulong)(n<<(63-top)),(ushort)((negative?0x8000:0)|top+power+16383));
    }
    static Conversion80Expected Conversion80Reference(Conv80 op,C80 input,ushort cw)
    {
        int status=0,metadata=1;C80 output;bool sign;
        if(op is Conv80.LoadRaw80 or Conv80.StoreRaw80)
        {output=new(input.Significand,input.SignExponent);metadata|=4;return Conversion80Finish(op,output,0,metadata,cw);}
        if(op is Conv80.LoadBinary32 or Conv80.LoadBinary64)
        {
            int f=op==Conv80.LoadBinary32?23:52,width=op==Conv80.LoadBinary32?32:64,bias=width==32?127:1023;
            ulong raw=width==32?(uint)input.Significand:input.Significand,frac=raw&((1UL<<f)-1);
            int exp=(int)((raw>>f)&(width==32?255UL:2047UL));sign=(raw>>(width-1))!=0;
            if(exp==(width==32?255:2047))
            {
                bool signaling=frac!=0&&(frac&(1UL<<(f-1)))==0;
                if(signaling){status=1;metadata|=8;frac|=1UL<<(f-1);}
                output=new(0x8000000000000000UL|(frac<<(63-f)),(ushort)((sign?0x8000:0)|0x7fff));
            }
            else
            {
                BigInteger n=exp==0?frac:(BigInteger.One<<f)+frac;
                output=Conversion80Exact(n,(exp==0?1:exp)-bias-f,sign);
                if(exp==0&&frac!=0){status=2;metadata|=16;}
            }
            return Conversion80Finish(op,output,status,metadata,cw);
        }
        if(((uint)op >= (uint)Conv80.LoadInt16 && (uint)op <= (uint)Conv80.LoadInt64))
        {
            int width=op==Conv80.LoadInt16?16:op==Conv80.LoadInt32?32:64;
            BigInteger n=new BigInteger(input.Significand)&((BigInteger.One<<width)-1);
            if(((n>>(width-1))&BigInteger.One)!=0)n-=BigInteger.One<<width;
            output=Conversion80Exact(BigInteger.Abs(n),0,n.Sign<0);
            return Conversion80Finish(op,output,0,metadata,cw);
        }
        bool ieee=op is Conv80.StoreBinary32 or Conv80.StoreBinary64;
        int w=op==Conv80.StoreBinary32?32:op==Conv80.StoreBinary64?64:op is Conv80.StoreInt16 or Conv80.TruncateInt16?16:op is Conv80.StoreInt32 or Conv80.TruncateInt32?32:64;
        int exponent=input.SignExponent&0x7fff;sign=(input.SignExponent&0x8000)!=0;
        ulong sig=input.Significand;bool unsupported=exponent!=0&&(sig>>63)==0;
        bool special=exponent==0x7fff;
        int rc=((uint)op >= (uint)Conv80.TruncateInt16 && (uint)op <= (uint)Conv80.TruncateInt64)?3:(cw>>10)&3;
        if(unsupported||(!ieee&&special))
            return Conversion80Invalid(op,w,cw);
        if(ieee&&special)
        {
            int f=w==32?23:52;ulong fraction=sig&0x7fffffffffffffffUL;
            ulong bits=(w==32?255UL:2047UL)<<f;
            if(fraction!=0)
            {
                if((fraction&0x4000000000000000UL)==0){status=1;metadata|=8;}
                bits|=(fraction>>(63-f))|(1UL<<(f-1));
            }
            if(sign)bits|=1UL<<(w-1);
            return Conversion80Finish(op,new(bits,0),status,metadata,cw);
        }
        BigInteger numerator=sig,denominator=BigInteger.One;
        int power=(exponent==0?1:exponent)-16383-63;
        if(power>=0)numerator<<=power;else denominator<<=-power;
        if(!ieee)
        {
            BigInteger rounded=Conversion80Round(numerator,denominator,0,rc,sign,out bool lost,out bool up);
            if(sign)rounded=-rounded;
            if(rounded<-(BigInteger.One<<(w-1))||rounded>=(BigInteger.One<<(w-1)))return Conversion80Invalid(op,w,cw);
            if(lost)status|=32;if(up)status|=512;
            output=new((ulong)(rounded&((BigInteger.One<<w)-1)),0);
        }
        else
        {
            int p=w==32?24:53,bias=w==32?127:1023,emin=1-bias;
            if(sig==0)output=new(sign?1UL<<(w-1):0,0);
            else
            {
                int e=(int)new BigInteger(sig).GetBitLength()-1+power;
                BigInteger unbounded=Conversion80Round(numerator,denominator,e-(p-1),rc,sign,out _,out _);
                if(unbounded==(BigInteger.One<<p))e++;
                if(e>bias)
                {
                    bool infinity=rc==0||(rc==1&&sign)||(rc==2&&!sign);
                    ulong bits=infinity?((w==32?255UL:2047UL)<<(p-1)):
                        (((w==32?254UL:2046UL)<<(p-1))|((1UL<<(p-1))-1));
                    output=new(bits|(sign?1UL<<(w-1):0),0);status=40|(infinity?512:0);metadata|=64;
                }
                else
                {
                    bool tiny=e<emin;if(tiny)metadata|=32;
                    int quantum=Math.Max(e,emin)-(p-1);
                    BigInteger q=Conversion80Round(numerator,denominator,quantum,rc,sign,out bool lost,out bool up);
                    if(lost)status|=32;if(up)status|=512;if(tiny&&lost)status|=16;
                    // Reconstruct exponent/fraction by the rounded integer's
                    // bit length, including subnormal-to-normal carry.
                    int top=q.IsZero?0:(int)q.GetBitLength()-1;
                    ulong bits;
                    if(q<(BigInteger.One<<(p-1)))bits=(ulong)q;
                    else
                    {
                        int encoded=quantum+top+bias;
                        BigInteger frac=q-(BigInteger.One<<top);
                        bits=((ulong)encoded<<(p-1))|(ulong)(frac<<(p-1-top));
                    }
                    output=new(bits|(sign?1UL<<(w-1):0),0);
                }
            }
        }
        return Conversion80Finish(op,output,status,metadata,cw);
    }
    static Conversion80Expected Conversion80Invalid(Conv80 op,int width,ushort cw)
    {
        ulong bits=op==Conv80.StoreBinary32?0xffc00000UL:op==Conv80.StoreBinary64?0xfff8000000000000UL:1UL<<(width-1);
        return Conversion80Finish(op,new(bits,0),1,9,cw);
    }
    static Conversion80Expected Conversion80Finish(Conv80 op,C80 output,int status,int metadata,ushort cw)
    {
        if((status&32)!=0)metadata|=128;else if((status&1)==0)metadata|=2;
        if((status&512)!=0)metadata|=256;
        int events=status;
        if((status&1)!=0&&(cw&1)==0){events=1;metadata|=512;}
        else if((op is Conv80.StoreBinary32 or Conv80.StoreBinary64)&&(status&8)!=0&&(cw&8)==0)
        {events=8;metadata|=512;}
        else if((op is Conv80.StoreBinary32 or Conv80.StoreBinary64)&&(metadata&32)!=0&&(cw&16)==0)
        {events=16;metadata|=512;}
        else if((op is Conv80.LoadBinary32 or Conv80.LoadBinary64)&&(status&2)!=0&&(cw&2)==0)
            metadata|=2048;
        else if((status&32)!=0&&(cw&32)==0)metadata|=1024;
        return new(output,(ushort)status,(ushort)events,(uint)metadata);
    }

    readonly record struct Conversion80Literal(Conv80 Op,C80 Input,ushort Cw,C80 Output,ushort Masked,ushort Events,uint Metadata);
    // Hex values/statuses are ISA/mathematical literals, not generated by either
    // oracle or emitted shift path. CW: 037f nearest, 077f down, 0b7f up, 0f7f chop.
    static IEnumerable<Conversion80Literal> Conversion80Literals()
    {
        yield return new(Conv80.LoadBinary32,new(0x80000000,0),0x037f,new(0,0x8000),0,0,3);
        yield return new(Conv80.LoadBinary32,new(1,0),0x037f,new(0x8000000000000000,0x3f6a),2,2,19);
        yield return new(Conv80.LoadBinary32,new(1,0),0x037d,new(0x8000000000000000,0x3f6a),2,2,2067);
        yield return new(Conv80.LoadBinary32,new(0x7f800001,0),0x037e,new(0xc000010000000000,0x7fff),1,1,521);
        yield return new(Conv80.LoadBinary64,new(1,0),0x017f,new(0x8000000000000000,0x3bcd),2,2,19);
        yield return new(Conv80.LoadBinary64,new(0xfff0000000000001,0),0x037f,new(0xc000000000000800,0xffff),1,1,9);
        yield return new(Conv80.LoadBinary64,new(0x3ff0000000000001,0),0x007f,new(0x8000000000000800,0x3fff),0,0,3);
        yield return new(Conv80.LoadInt16,new(0x8000,0),0x017f,new(0x8000000000000000,0xc00e),0,0,3);
        yield return new(Conv80.LoadInt32,new(0x80000000,0),0x007f,new(0x8000000000000000,0xc01e),0,0,3);
        yield return new(Conv80.LoadInt64,new(0x8000000000000000,0),0x037f,new(0x8000000000000000,0xc03e),0,0,3);
        yield return new(Conv80.LoadInt64,new(0x7fffffffffffffff,0),0x007f,new(0xfffffffffffffffe,0x403d),0,0,3);
        yield return new(Conv80.LoadInt64,new(0x0020000000000001,0),0x017f,new(0x8000000000000400,0x4034),0,0,3);
        yield return new(Conv80.StoreBinary64,new(0x8000000000000001,0x3fff),0x037f,new(0x3ff0000000000000,0),32,32,129);
        yield return new(Conv80.StoreBinary64,new(0x8000000000000001,0x3fff),0x0b7f,new(0x3ff0000000000001,0),544,544,385);
        yield return new(Conv80.StoreBinary64,new(0x8000000000000001,0x3fff),0x0b5f,new(0x3ff0000000000001,0),544,544,1409);
        yield return new(Conv80.StoreBinary32,new(0x8000008000000000,0x3fff),0x037f,new(0x3f800000,0),32,32,129);
        yield return new(Conv80.StoreBinary32,new(0x8000008000000001,0x3fff),0x037f,new(0x3f800001,0),544,544,385);
        yield return new(Conv80.StoreBinary32,new(0x8000008000000001,0xbfff),0x077f,new(0xbf800001,0),544,544,385);
        yield return new(Conv80.StoreBinary32,new(0x8000000000000000,0x3f6a),0x037f,new(1,0),0,0,35);
        yield return new(Conv80.StoreBinary32,new(0x8000000000000000,0x3f6a),0x036f,new(1,0),0,16,547);
        yield return new(Conv80.StoreBinary64,new(0x8000000000000000,0x3bcd),0x037f,new(1,0),0,0,35);
        yield return new(Conv80.StoreBinary32,new(0x8000000000000000,0x3f69),0x037f,new(0,0),48,48,161);
        yield return new(Conv80.StoreBinary32,new(0x8000000000000000,0x3f69),0x0b7f,new(1,0),560,560,417);
        yield return new(Conv80.StoreBinary32,new(0x8000000000000000,0x3f69),0x0b4f,new(1,0),560,16,929);
        yield return new(Conv80.StoreBinary32,new(0xffffff0000000000,0x3f80),0x037f,new(0x00800000,0),560,560,417);
        yield return new(Conv80.StoreBinary32,new(0xffffff8000000000,0x3f80),0x037f,new(0x00800000,0),544,544,385);
        yield return new(Conv80.StoreBinary32,new(0x8000000000000000,0x407f),0x037f,new(0x7f800000,0),552,552,449);
        yield return new(Conv80.StoreBinary32,new(0x8000000000000000,0x407f),0x0377,new(0x7f800000,0),552,8,961);
        yield return new(Conv80.StoreBinary32,new(0x8000000000000000,0x407f),0x0f7f,new(0x7f7fffff,0),40,40,193);
        yield return new(Conv80.StoreBinary64,new(0xc000000000000001,0xffff),0x037f,new(0xfff8000000000000,0),0,0,3);
        yield return new(Conv80.StoreBinary64,new(0x8000000000000001,0x7fff),0x037e,new(0x7ff8000000000000,0),1,1,521);
        yield return new(Conv80.StoreBinary32,new(0x4000000000000000,0x3fff),0x037f,new(0xffc00000,0),1,1,9);
        yield return new(Conv80.StoreInt16,new(0xc000000000000000,0x3fff),0x037f,new(2,0),544,544,385);
        yield return new(Conv80.StoreInt16,new(0xc000000000000000,0xbfff),0x037f,new(0xfffe,0),544,544,385);
        yield return new(Conv80.TruncateInt16,new(0xc000000000000000,0xbfff),0x077f,new(0xffff,0),32,32,129);
        yield return new(Conv80.StoreInt16,new(0xfffe000000000000,0x400d),0x037f,new(0x7fff,0),0,0,3);
        yield return new(Conv80.StoreInt16,new(0x8000000000000000,0xc00e),0x037f,new(0x8000,0),0,0,3);
        yield return new(Conv80.StoreInt16,new(0xffff000000000000,0x400d),0x037f,new(0x8000,0),1,1,9);
        yield return new(Conv80.StoreInt16,new(0xffff000000000000,0x400d),0x037e,new(0x8000,0),1,1,521);
        yield return new(Conv80.StoreInt16,new(0xffff000000000000,0x400d),0x0f7f,new(0x7fff,0),32,32,129);
        yield return new(Conv80.StoreInt16,new(0x8000800000000000,0xc00e),0x037f,new(0x8000,0),32,32,129);
        yield return new(Conv80.StoreInt16,new(0x8000800000000000,0xc00e),0x077f,new(0x8000,0),1,1,9);
        yield return new(Conv80.StoreInt32,new(0xfffffffe00000000,0x401d),0x037f,new(0x7fffffff,0),0,0,3);
        yield return new(Conv80.StoreInt32,new(0x8000000000000000,0xc01e),0x037f,new(0x80000000,0),0,0,3);
        yield return new(Conv80.TruncateInt32,new(0xc000000000000000,0xbfff),0x0b7f,new(0xffffffff,0),32,32,129);
        yield return new(Conv80.StoreInt64,new(0xfffffffffffffffe,0x403d),0x037f,new(0x7fffffffffffffff,0),0,0,3);
        yield return new(Conv80.StoreInt64,new(0xffffffffffffffff,0x403d),0x037f,new(0x8000000000000000,0),1,1,9);
        yield return new(Conv80.StoreInt64,new(0xffffffffffffffff,0xc03d),0x037f,new(0x8000000000000000,0),544,544,385);
        yield return new(Conv80.StoreInt64,new(0x8000000000000000,0xc03e),0x037f,new(0x8000000000000000,0),0,0,3);
        yield return new(Conv80.StoreInt64,new(0x8000000000000000,0x403e),0x037f,new(0x8000000000000000,0),1,1,9);
        yield return new(Conv80.TruncateInt64,new(0x8000000000000001,0xc03e),0x037f,new(0x8000000000000000,0),1,1,9);
        yield return new(Conv80.StoreInt32,new(1,0),0x0b7f,new(1,0),544,544,385); // no DE
        yield return new(Conv80.StoreBinary32,new(0x8000000000000000,0),0x037f,new(0,0),48,48,161); // pseudo, no DE
        yield return new(Conv80.LoadRaw80,new(0x8000000000000001,0x7fff),0x0100,new(0x8000000000000001,0x7fff),0,0,7);
        yield return new(Conv80.LoadRaw80,new(0x8000000000000000,0),0x0100,new(0x8000000000000000,0),0,0,7);
        yield return new(Conv80.StoreRaw80,new(0x4000000000000000,0x7fff),0x0100,new(0x4000000000000000,0x7fff),0,0,7);
    }
    static void CheckConversion80Literals()
    {
        foreach(var row in Conversion80Literals())
        {
            var actual=Conversion80Reference(row.Op,row.Input,row.Cw);
            Check(Conversion80Same(actual.Value,row.Output)&&actual.Masked==row.Masked&&actual.Events==row.Events&&actual.Metadata==row.Metadata,
                "R129 independently specified ISA/literal anchor constrains BigInteger oracle");
        }
    }
    static void Conversion80LiteralNative(nint entry,Conv80 op,nint continuation)
    {
        foreach(var row in Conversion80Literals())if(row.Op==op)
        {
            Conversion80Invoke(entry,row.Input,row.Cw,new(row.Output,row.Masked,row.Events,row.Metadata),continuation);
            // Every mask and PC, retaining the row's RC. This independently tests
            // mask-sensitive completion without multiplying the broad dyadic grid.
            foreach(int masks in Conversion80Masks)foreach(int pc in new[]{0,1,2,3})
            {
                ushort cw=(ushort)(0x40|masks|(pc<<8)|(row.Cw&0xc00));
                Conversion80Invoke(entry,row.Input,cw,Conversion80Reference(op,row.Input,cw),continuation);
            }
        }
    }
}
