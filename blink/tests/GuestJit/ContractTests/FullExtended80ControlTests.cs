using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using Managed.Emulation.Execution;
using C80 = Managed.Emulation.Execution.FullExtended80;
using Ctrl80 = Managed.Emulation.Execution.FullExtended80Control;
using CM80 = Managed.Emulation.Execution.FullExtended80ControlMetadata;
using Rel80 = Managed.Emulation.Execution.FullExtended80Relation;

unsafe partial class Program
{
    // Parent wires this entry. SOURCE ONLY: every emission, publish, allocation
    // check and native invocation authored below is UNRUN by implementation.
    static void CheckFullExtended80Controls()
    {
        CheckControl80Literals();CheckControl80Emission();
        foreach(Ctrl80 op in Enum.GetValues<Ctrl80>())
        foreach(bool windows in new[]{false,true})foreach(bool continuation in new[]{false,true})
        {
            using var code=Control80Code(op,windows,continuation);
            using var ret=Control80Publish([0xf3,0x0f,0x1e,0xfa,0xc3]);
            using var entry=Control80Adapter(code.Entry,windows,continuation);
            nint tail=continuation?ret.Entry:0;
            foreach(var pair in Control80Cases(op))
            foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})foreach(int masks in new[]{0,63})
            {
                ushort cw=(ushort)(0x40|masks|(pc<<8)|(rc<<10));
                Control80Invoke(entry.Entry,pair.Left,pair.Right,cw,Control80Reference(op,pair.Left,pair.Right,cw),tail);
            }
            // Literal ISA anchors cross every mask, PC and RC on BOTH ABIs and
            // entry forms. Expectations include literal bits independent of oracle.
            Control80LiteralNative(entry.Entry,op,tail);
            Control80Protocol(entry.Entry,tail);Control80Warm(entry.Entry,tail);
            using var probe=Control80PreservationProbe(entry.Entry);
            var f=Control80Frame(new(0x8000000000000001,0x3fff),new(0x8000000000000000,0x3fff),0x37f,tail);
            Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)probe.Entry)(&f)==0,
                "R134 native nonvolatile GPR union preserved");
            if(op==Ctrl80.RoundToIntegral)Control80ExponentSweep(entry.Entry,tail);
        }
    }
    readonly record struct Control80Expected(C80 Image,ushort Masked,ushort Events,uint Metadata);
    readonly record struct Control80Pair(C80 Left,C80 Right);
    static NativeCodeAllocation Control80Publish(ReadOnlySpan<byte> b)
    {
        Check(NativeCodeAllocation.TryCreate(b,out var owner,out string why,requireNativeFlags:false),why);
        return owner!;
    }
    static NativeCodeAllocation Control80Code(Ctrl80 op,bool windows,bool continuation)
    {
        byte[] b=new byte[FullExtended80ControlEmitter.CodeCapacity];int n;
        bool ok=continuation?FullExtended80ControlEmitter.TryEmitContinuation(op,b,out n):
            FullExtended80ControlEmitter.TryEmit(op,b,out n,windows);
        Check(ok&&n>0&&n<=b.Length,"R134 bounded specialized conversion emission");
        return Control80Publish(b.AsSpan(0,n));
    }
    static NativeCodeAllocation Control80Adapter(nint target,bool windows,bool continuation)
    {
        var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};bool hostWin=OperatingSystem.IsWindows();
        if(continuation)
        {b.AddRange(hostWin?new byte[]{0x49,0x89,0xcb}:new byte[]{0x49,0x89,0xfb});}
        else if(windows!=hostWin)
        {
            if(windows)b.AddRange(new byte[]{0x48,0x89,0xf9,0x48,0x83,0xec,0x28}); // RCX=RDI; Win64 home space/alignment
            else b.AddRange(new byte[]{0x57,0x48,0x89,0xcf,0x48,0x83,0xec,0x20});
        }
        b.AddRange(new byte[]{0x48,0xb8});Control80U64(b,(ulong)target);
        if(!continuation&&hostWin!=windows)
        {
            b.AddRange(new byte[]{0xff,0xd0,0x48,0x83,0xc4,(byte)(windows?0x28:0x20)});
            if(hostWin)b.Add(0x5f);
            b.Add(0xc3);
        }
        else b.AddRange(new byte[]{0xff,0xe0});
        return Control80Publish(b.ToArray());
    }
    static void Control80U64(List<byte> b,ulong v){for(int i=0;i<8;i++)b.Add((byte)(v>>(8*i)));}
    static void Control80I32(List<byte> b,int v){for(int i=0;i<4;i++)b.Add((byte)(v>>(8*i)));}

    // Test-only synchronous machine-code wrapper, with normal native CALL/RET.
    // Saves/restores the SysV/Win64 union of nonvolatile GPRs, poisons each,
    // then checks after kernel. SysV RDI is checked against its frame argument.
    // No async stack walking, host FP or semantic helper. Does not qualify
    // asynchronous unwind or Windows execution until actually run there.
    static NativeCodeAllocation Control80PreservationProbe(nint target)
    {
        bool win=OperatingSystem.IsWindows();int[] regs=new[]{3,5,6,7,12,13,14,15};
        var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};
        foreach(int r in regs){if(r>=8)b.Add(0x41);b.Add((byte)(0x50+(r&7)));}
        b.Add((byte)(win?0x51:0x57)); // save incoming frame; even regs + one push aligns CALL
        b.AddRange(new byte[]{0x48,0x83,0xec,0x20});
        for(int i=0;i<regs.Length;i++)
        {int r=regs[i];b.Add((byte)(r>=8?0x49:0x48));b.Add((byte)(0xb8+(r&7)));Control80U64(b,0x123456789abc0000UL+(uint)i);}
        // Reload argument from above shadow space after poisoning RDI on Win64.
        b.AddRange(win?new byte[]{0x48,0x8b,0x4c,0x24,0x20}:new byte[]{0x48,0x8b,0x7c,0x24,0x20});
        b.AddRange(new byte[]{0x48,0xb8});Control80U64(b,(ulong)target);b.AddRange(new byte[]{0xff,0xd0,0x45,0x31,0xd2});
        for(int i=0;i<regs.Length;i++)
        {
            int r=regs[i];
            if(!win&&r==7)b.AddRange(new byte[]{0x48,0x8b,0x44,0x24,0x20});
            else {b.AddRange(new byte[]{0x48,0xb8});Control80U64(b,0x123456789abc0000UL+(uint)i);}
            b.Add((byte)(r>=8?0x49:0x48));b.Add(0x39);b.Add((byte)(0xc0|(r&7)));
            b.AddRange(new byte[]{0x74,0x07,0x41,0x81,0xca});Control80I32(b,1<<i);
        }
        b.AddRange(new byte[]{0x44,0x89,0xd0,0x48,0x83,0xc4,0x28});
        for(int i=regs.Length-1;i>=0;i--){int r=regs[i];if(r>=8)b.Add(0x41);b.Add((byte)(0x58+(r&7)));}
        b.Add(0xc3);return Control80Publish(b.ToArray());
    }
    static FullExtended80WorkFrame Control80Frame(C80 left,C80 right,ushort cw,nint continuation)=>new()
    {Version=1,Size=384,ControlWord=cw,Left=left,Right=right,Continuation=(ulong)continuation};
    static bool Control80Same(C80 a,C80 b)=>new ReadOnlySpan<byte>(&a,16).SequenceEqual(new ReadOnlySpan<byte>(&b,16));
    static void Control80Invoke(nint entry,C80 left,C80 right,ushort cw,Control80Expected expected,nint continuation)
    {
        byte* guarded=stackalloc byte[416];new Span<byte>(guarded,416).Fill(0xad);
        var f=(FullExtended80WorkFrame*)(guarded+16);*f=Control80Frame(left,right,cw,continuation);
        f->Reserved10=0x1133;f->Left.Reserved10=0x2468;f->Left.Reserved12=0xabcdef01;
        f->Right.Reserved10=0x1357;f->Right.Reserved12=0x12345678;
        new Span<byte>((byte*)f+96,288).Fill(0xad);
        byte* before=stackalloc byte[384];new ReadOnlySpan<byte>(f,384).CopyTo(new Span<byte>(before,384));
        uint admission=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(f);
        Check(admission==0&&f->Admission==0,"R134 EAX/frame Complete, numeric foundation only");
        Check(Control80Same(f->Normal,expected.Image)&&new ReadOnlySpan<byte>((byte*)f+64,16).IndexOfAnyExcept((byte)0)<0&&
            f->NormalStatus==expected.Masked&&f->WrappedStatus==expected.Events&&(uint)f->Classification==expected.Metadata,
            "R134 rational/literal value, relation/masks, masked SW, event priority, pop/destination metadata");
        Check(new ReadOnlySpan<byte>(f,12).SequenceEqual(new ReadOnlySpan<byte>(before,12))&&
            new ReadOnlySpan<byte>((byte*)f+16,32).SequenceEqual(new ReadOnlySpan<byte>(before+16,32))&&
            new ReadOnlySpan<byte>((byte*)f+88,8).SequenceEqual(new ReadOnlySpan<byte>(before+88,8))&&
            new ReadOnlySpan<byte>((byte*)f+152,232).SequenceEqual(new ReadOnlySpan<byte>(before+152,232)),
            "R134 input/padding/continuation/reserved and unused scratch immutable");
        Check(new ReadOnlySpan<byte>(guarded,16).IndexOfAnyExcept((byte)0xad)<0&&
            new ReadOnlySpan<byte>(guarded+400,16).IndexOfAnyExcept((byte)0xad)<0,"R134 frame guards");
    }
    static void Control80Protocol(nint entry,nint continuation)
    {
        foreach((uint version,uint size,uint reason) in new[]{(0u,384u,1u),(2u,384u,1u),(1u,0u,2u),(1u,383u,2u),(1u,385u,2u),(1u,uint.MaxValue,2u)})
        {
            var f=Control80Frame(default,default,0x17f,continuation);f.Version=version;f.Size=size;
            new Span<byte>((byte*)&f+48,40).Fill(0xef);
            Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f)==reason&&(uint)f.Admission==reason&&
                new ReadOnlySpan<byte>((byte*)&f+48,40).IndexOfAnyExcept((byte)0)<0,"R134 protocol rejects clear outputs");
        }
    }
    static void CheckControl80Emission()
    {
        byte[] b=new byte[FullExtended80ControlEmitter.CodeCapacity+16];
        foreach(Ctrl80 op in Enum.GetValues<Ctrl80>())foreach(bool win in new[]{false,true})foreach(bool cont in new[]{false,true})
        {
            int n;bool ok=cont?FullExtended80ControlEmitter.TryEmitContinuation(op,b,out n):FullExtended80ControlEmitter.TryEmit(op,b,out n,win);
            Check(ok&&n>0&&n<=FullExtended80ControlEmitter.CodeCapacity,"R134 bounded specialized controls");
            byte[] image=b.AsSpan(0,n).ToArray();
            Check(image.AsSpan(0,4).SequenceEqual(new byte[]{0xf3,0x0f,0x1e,0xfa}),"R134 CET leaf prefix");
            Check(cont?image.AsSpan(n-7).SequenceEqual(new byte[]{0x41,0xff,0xa3,88,0,0,0}):image[n-1]==0xc3,
                "R134 literal RET or R11 JMP[88] tail");
            for(int size=0;size<n;size++)
            {
                Array.Fill(b,(byte)0xb7);Span<byte> dst=b.AsSpan(8,size);
                bool shortOk=cont?FullExtended80ControlEmitter.TryEmitContinuation(op,dst,out int m):FullExtended80ControlEmitter.TryEmit(op,dst,out m,win);
                Check(!shortOk&&m==0&&b.AsSpan(0,8).IndexOfAnyExcept((byte)0xb7)<0&&
                    b.AsSpan(8+size).IndexOfAnyExcept((byte)0xb7)<0,"R134 EVERY short span, zero length, guards");
            }
            Array.Fill(b,(byte)0xb7);
            bool full=cont?FullExtended80ControlEmitter.TryEmitContinuation(op,b,out int count):FullExtended80ControlEmitter.TryEmit(op,b,out count,win);
            Check(full&&count==n&&b.AsSpan(n).IndexOfAnyExcept((byte)0xb7)<0,"R134 deterministic bounded emission and untouched tail");
            for(int i=0;i<16;i++)if(cont)FullExtended80ControlEmitter.TryEmitContinuation(op,b,out _);else FullExtended80ControlEmitter.TryEmit(op,b,out _,win);
            long baseline=GC.GetAllocatedBytesForCurrentThread();bool all=true;
            for(int i=0;i<128;i++)all&=cont?FullExtended80ControlEmitter.TryEmitContinuation(op,b,out _):FullExtended80ControlEmitter.TryEmit(op,b,out _,win);
            Check(all&&GC.GetAllocatedBytesForCurrentThread()==baseline,"R134 warm zero-allocation emitter");
        }
        Array.Fill(b,(byte)0xb7);
        Check(!FullExtended80ControlEmitter.TryEmit((Ctrl80)255,b,out int bad)&&bad==0&&b.AsSpan().IndexOfAnyExcept((byte)0xb7)<0&&
            !FullExtended80ControlEmitter.TryEmitContinuation((Ctrl80)255,b,out bad)&&bad==0,"R134 unknown selector false/zero/no writes");
    }
    static void Control80Warm(nint entry,nint tail)
    {
        var f=Control80Frame(new(0xc000000000000001,0xbfff),new(1,0),0x17f,tail);
        for(int i=0;i<16;i++)((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        long before=GC.GetAllocatedBytesForCurrentThread();uint result=0;
        for(int i=0;i<256;i++)result|=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        Check(result==0&&GC.GetAllocatedBytesForCurrentThread()==before,"R134 warm zero-allocation native kernel");
    }

    // Independent oracle: finite values are exact integer multiples of 2^-16445.
    // No BSR/limb alignment or emitter round algorithm, host float or prior oracle.
    enum Control80Kind { Finite, Infinity, QuietNaN, SignalingNaN, Unsupported }
    static Control80Kind Control80Class(C80 x)
    {
        int e=x.SignExponent&0x7fff;bool j=(x.Significand>>63)!=0;
        if(e!=0&&!j)return Control80Kind.Unsupported;
        if(e!=0x7fff)return Control80Kind.Finite;
        if(x.Significand==0x8000000000000000)return Control80Kind.Infinity;
        return (x.Significand&0x4000000000000000)!=0?Control80Kind.QuietNaN:Control80Kind.SignalingNaN;
    }
    static bool Control80Denormal(C80 x)=>(x.SignExponent&0x7fff)==0&&x.Significand!=0;
    static BigInteger Control80Units(C80 x)
    {
        int exponent=x.SignExponent&0x7fff;
        BigInteger units=(BigInteger)x.Significand<<Math.Max(0,exponent-1);
        return (x.SignExponent&0x8000)!=0?-units:units;
    }
    static bool Control80EFlags(Ctrl80 op)=>op is Ctrl80.Fcomi or Ctrl80.Fcomip or Ctrl80.Fucomi or Ctrl80.Fucomip;
    static bool Control80Quiet(Ctrl80 op)=>op is Ctrl80.Fucom or Ctrl80.Fucomp or Ctrl80.Fucompp or Ctrl80.Fucomi or Ctrl80.Fucomip;
    static bool Control80Pops(Ctrl80 op)=>op is Ctrl80.Fcomp or Ctrl80.Fcompp or Ctrl80.Fucomp or Ctrl80.Fucompp or Ctrl80.Fcomip or Ctrl80.Fucomip;
    static C80 Control80CompareImage(Rel80 rel,bool flags,bool hold)
    {
        C80 image=default;var p=(FullExtended80CompareResult*)&image;p->Relation=rel;
        if(flags)
        {
            p->StatusWriteMask=0x200;
            p->EFlagsBits=rel switch{Rel80.Greater=>0u,Rel80.Less=>1u,Rel80.Equal=>0x40u,_=>0x45u};
            p->EFlagsWriteMask=hold?0x890u:0x8d5u;
        }
        else
        {
            p->StatusBits=rel switch{Rel80.Greater=>0,Rel80.Less=>0x100,Rel80.Equal=>0x4000,_=>0x4500};
            p->StatusWriteMask=(ushort)(hold?0x200:0x4700);
        }
        return image;
    }
    static int Control80CompareNumeric(C80 a,C80 b)
    {
        bool ai=Control80Class(a)==Control80Kind.Infinity,bi=Control80Class(b)==Control80Kind.Infinity;
        int sa=(a.SignExponent&0x8000)!=0?-1:1,sb=(b.SignExponent&0x8000)!=0?-1:1;
        if(ai&&bi)return sa.CompareTo(sb);
        if(ai)return sa;if(bi)return -sb;
        return Control80Units(a).CompareTo(Control80Units(b));
    }
    static Control80Expected Control80Reference(Ctrl80 op,C80 a,C80 b,ushort cw)
    {
        if(op==Ctrl80.RoundToIntegral)return Control80RoundReference(a,cw);
        var ac=Control80Class(a);var bc=Control80Class(b);
        bool invalid=ac is Control80Kind.Unsupported or Control80Kind.SignalingNaN||bc is Control80Kind.Unsupported or Control80Kind.SignalingNaN;
        bool qnan=ac==Control80Kind.QuietNaN||bc==Control80Kind.QuietNaN;
        invalid|=qnan&&!Control80Quiet(op);
        bool unordered=invalid||qnan,hold=invalid&&(cw&1)==0;
        Rel80 rel=unordered?Rel80.Unordered:Control80CompareNumeric(a,b) switch{<0=>Rel80.Less,0=>Rel80.Equal,_=>Rel80.Greater};
        bool denormal=!unordered&&(Control80Denormal(a)||Control80Denormal(b));
        ushort sw=(ushort)(invalid?1:denormal?2:0);
        uint meta=1u|(invalid?8u:unordered?0u:4u)|(denormal?16u|1024u:0u)|(hold?128u:0u);
        if(Control80Pops(op)&&(hold||(denormal&&(cw&2)==0)))meta|=2048;
        return new(Control80CompareImage(rel,Control80EFlags(op),hold),sw,sw,meta);
    }
    static C80 Control80EncodeInteger(BigInteger magnitude,bool negative)
    {
        if(magnitude.IsZero)return new(0,negative?(ushort)0x8000:(ushort)0);
        // Independent exact integer encoding via bit length, not emitter BSR.
        int bits=checked((int)magnitude.GetBitLength());
        BigInteger significand=bits<=64?magnitude<<(64-bits):magnitude>>(bits-64);
        // FRNDINT's exact integer result has <=64 significant bits even at huge
        // exponent. Verify that this encoding did not silently round anything.
        Check(bits<=64||(significand<<(bits-64))==magnitude,"R134 oracle integral exact representability");
        return new((ulong)significand,(ushort)((bits-1+16383)|(negative?0x8000:0)));
    }
    static Control80Expected Control80RoundReference(C80 a,ushort cw)
    {
        var kind=Control80Class(a);C80 output;ushort status=0;uint meta=2;
        if(kind==Control80Kind.Unsupported)
        {output=new(0xc000000000000000,0xffff);status=1;meta|=8;}
        else if(kind==Control80Kind.SignalingNaN)
        {output=new(a.Significand|0x4000000000000000,a.SignExponent);status=1;meta|=8;}
        else if(kind is Control80Kind.Infinity or Control80Kind.QuietNaN)
        {output=new(a.Significand,a.SignExponent);meta|=4;}
        else
        {
            bool negative=(a.SignExponent&0x8000)!=0;
            BigInteger numerator=BigInteger.Abs(Control80Units(a)),denominator=BigInteger.One<<16445;
            BigInteger integer=BigInteger.DivRem(numerator,denominator,out BigInteger remainder);
            bool up=false,inexact=!remainder.IsZero;int rc=(cw>>10)&3;
            if(inexact)
            {
                if(rc==0){int half=(remainder*2).CompareTo(denominator);up=half>0||(half==0&&!integer.IsEven);}
                else if(rc==1)up=negative;else if(rc==2)up=!negative;
                if(up)integer++;
                status|=32;if(up)status|=512;meta|=32;if(up)meta|=64;
            }
            else meta|=4;
            if(Control80Denormal(a)){status|=2;meta|=16;}
            output=Control80EncodeInteger(integer,negative);
        }
        ushort events=status;
        if((status&1)!=0&&(cw&1)==0){events=1;meta|=256;}
        else if((status&2)!=0&&(cw&2)==0){events=2;meta|=256;}
        else if((status&32)!=0&&(cw&32)==0)meta|=512;
        return new(output,status,events,meta);
    }
    static IEnumerable<Control80Pair> Control80Cases(Ctrl80 op)
    {
        if(op==Ctrl80.RoundToIntegral)
        {
            foreach(ushort sign in new ushort[]{0,0x8000})
            foreach(int exponent in new[]{0,1,2,16318,16319,16320,16329,16330,16381,16382,16383,16384,16385,16434,16435,16436,16445,16446,16447,32765,32766,32767})
            foreach(ulong sig in new[]{0UL,1UL,0x3fffffffffffffffUL,0x4000000000000000UL,0x7fffffffffffffffUL,
                0x8000000000000000UL,0x8000000000000001UL,0x80000000000007ffUL,0xbfffffffffffffffUL,0xc000000000000000UL,0xc000000000000001UL,ulong.MaxValue})
                yield return new(new(sig,(ushort)(sign|exponent)),new(0x123456789abcdef0,0x8765));
            yield break;
        }
        C80[] atoms={new(0,0),new(0,0x8000),new(1,0),new(1,0x8000),new(0x7fffffffffffffff,0),
            new(0x8000000000000000,0),new(0x8000000000000001,0),new(ulong.MaxValue,0),
            new(0x8000000000000000,1),new(0x8000000000000000,0x8001),new(0x8000000000000001,1),
            new(0x8000000000000000,0x3fff),new(0x8000000000000001,0x3fff),new(0x8000000000000000,0xbfff),
            new(0x8000000000000001,0xbfff),new(ulong.MaxValue,0x7ffe),new(ulong.MaxValue,0xfffe),
            new(0x8000000000000000,0x7fff),new(0x8000000000000000,0xffff),
            new(0xc000000000000001,0x7fff),new(0xffffffffffffffff,0xffff),new(0x8000000000000001,0x7fff),
            new(0xbfffffffffffffff,0xffff),new(0,1),new(0x7fffffffffffffff,0x7fff),new(1,0x4000)};
        // Full Cartesian inputs includes sign reversal, operand reversal,
        // unsupported-vs-NaN-vs-denormal priority, zero-vs-normalized denormal.
        foreach(C80 a in atoms)foreach(C80 b in atoms)yield return new(a,b);
        // Additional full-exponent and trailing-bit neighbors. Oracle compares
        // integers in a common exact quantum, no conversion via binary64.
        foreach(int e in new[]{1,2,1000,16319,16382,16383,16435,16446,30000,32766})
        foreach(ushort sign in new ushort[]{0,0x8000})
        {
            var a=new C80(0x8000000000000000,(ushort)(e|sign));var b=new C80(0x8000000000000001,(ushort)(e|sign));
            yield return new(a,b);yield return new(b,a);
        }
    }
    static void Control80ExponentSweep(nint entry,nint tail)
    {
        // Every unbiased exponent in the requested range, including the minimum
        // true80 subnormal. PC/RC/masks cycle; literal grid separately crosses
        // all of them. This is bounded (32829 cases per native entry).
        for(int e=-16445;e<=16383;e++)
        {
            bool negative=(e&1)!=0;C80 a;
            if(e< -16382)a=new(1UL<<(e+16445),(ushort)(negative?0x8000:0));
            else a=new((e&7)==0?ulong.MaxValue:0x8000000000000001,(ushort)((e+16383)|(negative?0x8000:0)));
            ushort cw=(ushort)(0x40|((e+16445)&63)|(((e+16445)&3)<<10)|((((e+16445)>>2)&3)<<8));
            Control80Invoke(entry,a,default,cw,Control80RoundReference(a,cw),tail);
        }
    }

    readonly record struct Control80CompareLiteral(C80 A,C80 B,Rel80 Relation,bool OrderedInvalid,bool QuietInvalid,bool Denormal);
    static readonly Control80CompareLiteral[] Control80CompareLiterals={
        new(new(0,0),new(0,0x8000),Rel80.Equal,false,false,false),
        new(new(1,0),new(0,0),Rel80.Greater,false,false,true),
        new(new(1,0x8000),new(0,0),Rel80.Less,false,false,true),
        new(new(0x8000000000000000,0),new(0x8000000000000000,1),Rel80.Equal,false,false,true),
        new(new(0x7fffffffffffffff,0),new(0x8000000000000000,1),Rel80.Less,false,false,true),
        new(new(0x8000000000000000,0),new(1,0),Rel80.Greater,false,false,true),
        new(new(0x8000000000000001,0x3fff),new(0x8000000000000000,0x3fff),Rel80.Greater,false,false,false),
        new(new(0x8000000000000001,0xbfff),new(0x8000000000000000,0xbfff),Rel80.Less,false,false,false),
        new(new(0x8000000000000000,0x7fff),new(ulong.MaxValue,0x7ffe),Rel80.Greater,false,false,false),
        new(new(0x8000000000000000,0xffff),new(ulong.MaxValue,0xfffe),Rel80.Less,false,false,false),
        new(new(0x8000000000000000,0x7fff),new(0x8000000000000000,0x7fff),Rel80.Equal,false,false,false),
        new(new(0xc000000000000001,0x7fff),new(1,0),Rel80.Unordered,true,false,false),
        new(new(0xc000000000000001,0xffff),new(0x8000000000000000,0),Rel80.Unordered,true,false,false),
        new(new(0x8000000000000001,0x7fff),new(1,0),Rel80.Unordered,true,true,false),
        new(new(0,1),new(1,0),Rel80.Unordered,true,true,false),
        new(new(0x4000000000000000,0x7fff),new(0xc000000000000001,0x7fff),Rel80.Unordered,true,true,false),
        new(new(0x8000000000000001,0x7fff),new(0xc000000000000001,0xffff),Rel80.Unordered,true,true,false)
    };
    readonly record struct Control80RoundLiteral(C80 Input,C80 Nearest,C80 Down,C80 Up,C80 Truncate,ushort NearestSW,ushort DownSW,ushort UpSW,ushort TruncateSW);
    static readonly Control80RoundLiteral[] Control80RoundLiterals={
        new(new(0x8000000000000000,0x3ffe),new(0,0),new(0,0),new(0x8000000000000000,0x3fff),new(0,0),32,32,544,32), // +0.5
        new(new(0xc000000000000000,0x3fff),new(0x8000000000000000,0x4000),new(0x8000000000000000,0x3fff),new(0x8000000000000000,0x4000),new(0x8000000000000000,0x3fff),544,32,544,32), // +1.5
        new(new(0xc000000000000000,0xbfff),new(0x8000000000000000,0xc000),new(0x8000000000000000,0xc000),new(0x8000000000000000,0xbfff),new(0x8000000000000000,0xbfff),544,544,32,32), // -1.5
        new(new(0xa000000000000000,0x4000),new(0x8000000000000000,0x4000),new(0x8000000000000000,0x4000),new(0xc000000000000000,0x4000),new(0x8000000000000000,0x4000),32,32,544,32), // +2.5
        new(new(ulong.MaxValue,0x3ffe),new(0x8000000000000000,0x3fff),new(0,0),new(0x8000000000000000,0x3fff),new(0,0),544,32,544,32), // just below1
        new(new(0x8000000000000001,0x3fff),new(0x8000000000000000,0x3fff),new(0x8000000000000000,0x3fff),new(0x8000000000000000,0x4000),new(0x8000000000000000,0x3fff),32,32,544,32), // beyond53bits
        new(new(1,0),new(0,0),new(0,0),new(0x8000000000000000,0x3fff),new(0,0),34,34,546,34), // minDN
        new(new(1,0x8000),new(0,0x8000),new(0x8000000000000000,0xbfff),new(0,0x8000),new(0,0x8000),34,546,34,34),
        new(new(0x8000000000000000,0),new(0,0),new(0,0),new(0x8000000000000000,0x3fff),new(0,0),34,34,546,34), // pseudoDN
        new(new(ulong.MaxValue,0x403d),new(0x8000000000000000,0x403e),new(0xfffffffffffffffe,0x403d),new(0x8000000000000000,0x403e),new(0xfffffffffffffffe,0x403d),544,32,544,32), // 2^63-0.5
        new(new(0x8000000000000001,0x403e),new(0x8000000000000001,0x403e),new(0x8000000000000001,0x403e),new(0x8000000000000001,0x403e),new(0x8000000000000001,0x403e),0,0,0,0),
        new(new(ulong.MaxValue,0x7ffe),new(ulong.MaxValue,0x7ffe),new(ulong.MaxValue,0x7ffe),new(ulong.MaxValue,0x7ffe),new(ulong.MaxValue,0x7ffe),0,0,0,0),
        new(new(0,0x8000),new(0,0x8000),new(0,0x8000),new(0,0x8000),new(0,0x8000),0,0,0,0),
        new(new(0x8000000000000000,0xffff),new(0x8000000000000000,0xffff),new(0x8000000000000000,0xffff),new(0x8000000000000000,0xffff),new(0x8000000000000000,0xffff),0,0,0,0),
        new(new(0xc000000000000001,0xffff),new(0xc000000000000001,0xffff),new(0xc000000000000001,0xffff),new(0xc000000000000001,0xffff),new(0xc000000000000001,0xffff),0,0,0,0),
        new(new(0x8000000000000001,0x7fff),new(0xc000000000000001,0x7fff),new(0xc000000000000001,0x7fff),new(0xc000000000000001,0x7fff),new(0xc000000000000001,0x7fff),1,1,1,1),
        new(new(0,1),new(0xc000000000000000,0xffff),new(0xc000000000000000,0xffff),new(0xc000000000000000,0xffff),new(0xc000000000000000,0xffff),1,1,1,1)
    };
    static Control80Expected Control80LiteralRound(Control80RoundLiteral row,ushort cw)
    {
        int rc=(cw>>10)&3;C80 value=rc switch{0=>row.Nearest,1=>row.Down,2=>row.Up,_=>row.Truncate};
        ushort sw=rc switch{0=>row.NearestSW,1=>row.DownSW,2=>row.UpSW,_=>row.TruncateSW};
        uint meta=2u|((sw&1)!=0?8u:(sw&32)==0?4u:32u)|((sw&2)!=0?16u:0u)|((sw&512)!=0?64u:0u);
        ushort events=sw;
        if((sw&1)!=0&&(cw&1)==0){events=1;meta|=256;}
        else if((sw&2)!=0&&(cw&2)==0){events=2;meta|=256;}
        else if((sw&32)!=0&&(cw&32)==0)meta|=512;
        return new(value,sw,events,meta);
    }
    static Control80Expected Control80LiteralCompare(Ctrl80 op,Control80CompareLiteral row,ushort cw,bool reverse)
    {
        bool invalid=Control80Quiet(op)?row.QuietInvalid:row.OrderedInvalid,hold=invalid&&(cw&1)==0;
        Rel80 rel=row.Relation;if(reverse)rel=rel==Rel80.Less?Rel80.Greater:rel==Rel80.Greater?Rel80.Less:rel;
        uint meta=1u|(invalid?8u:rel==Rel80.Unordered?0u:4u)|(row.Denormal?16u|1024u:0u)|(hold?128u:0u);
        if(Control80Pops(op)&&(hold||(row.Denormal&&(cw&2)==0)))meta|=2048;
        ushort sw=(ushort)(invalid?1:row.Denormal?2:0);
        return new(Control80CompareImage(rel,Control80EFlags(op),hold),sw,sw,meta);
    }
    static void Control80LiteralNative(nint entry,Ctrl80 op,nint tail)
    {
        for(int pc=0;pc<4;pc++)for(int rc=0;rc<4;rc++)for(int masks=0;masks<64;masks++)
        {
            ushort cw=(ushort)(0x40|masks|(pc<<8)|(rc<<10));
            if(op==Ctrl80.RoundToIntegral)
                foreach(var row in Control80RoundLiterals)Control80Invoke(entry,row.Input,default,cw,Control80LiteralRound(row,cw),tail);
            else foreach(var row in Control80CompareLiterals)
            {
                Control80Invoke(entry,row.A,row.B,cw,Control80LiteralCompare(op,row,cw,false),tail);
                Control80Invoke(entry,row.B,row.A,cw,Control80LiteralCompare(op,row,cw,true),tail);
            }
        }
    }
    static void CheckControl80Literals()
    {
        // Test the independent oracle against every literal before any native
        // publication. This cannot establish emitter correctness on its own.
        foreach(Ctrl80 op in Enum.GetValues<Ctrl80>())for(int rc=0;rc<4;rc++)for(int masks=0;masks<64;masks++)
        {
            ushort cw=(ushort)(0x140|masks|(rc<<10));
            if(op==Ctrl80.RoundToIntegral)foreach(var row in Control80RoundLiterals)
            {
                var a=Control80RoundReference(row.Input,cw);var b=Control80LiteralRound(row,cw);
                Check(Control80Same(a.Image,b.Image)&&a.Masked==b.Masked&&a.Events==b.Events&&a.Metadata==b.Metadata,"R134 oracle vs literal integral anchors");
            }
            else foreach(var row in Control80CompareLiterals)foreach(bool reverse in new[]{false,true})
            {
                var a=Control80Reference(op,reverse?row.B:row.A,reverse?row.A:row.B,cw);var b=Control80LiteralCompare(op,row,cw,reverse);
                Check(Control80Same(a.Image,b.Image)&&a.Masked==b.Masked&&a.Events==b.Events&&a.Metadata==b.Metadata,"R134 oracle vs literal compare anchors");
            }
        }
        // Literal condition-merge anchors, deliberately seed CF/PF=0 and ZF=1
        // so unordered updates cannot masquerade as unmasked IE flag holding.
        C80 held=Control80CompareImage(Rel80.Unordered,true,true);var h=(FullExtended80CompareResult*)&held;
        Check(h->EFlagsWriteMask==0x890&&((0x8d0u&~h->EFlagsWriteMask)|(h->EFlagsBits&h->EFlagsWriteMask))==0x40,
            "R134 Intel unmasked IE: clear OF/SF/AF, hold CF/PF/ZF");
        C80 less=Control80CompareImage(Rel80.Less,true,false);var l=(FullExtended80CompareResult*)&less;
        Check(((0x8d0u&~l->EFlagsWriteMask)|(l->EFlagsBits&l->EFlagsWriteMask))==1&&l->StatusWriteMask==0x200,
            "R134 DE completes less EFLAGS, x87 C0/C2/C3 unaffected");
        C80 status=Control80CompareImage(Rel80.Less,false,false);var s=(FullExtended80CompareResult*)&status;
        Check(((0x4700u&~s->StatusWriteMask)|(uint)(s->StatusBits&s->StatusWriteMask))==0x100,
            "R134 comparison C1 clear, less status replaces unordered/equal seed");
    }
}
