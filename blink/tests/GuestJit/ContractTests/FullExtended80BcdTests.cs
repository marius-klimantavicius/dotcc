using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using Managed.Emulation.Execution;
using C80 = Managed.Emulation.Execution.FullExtended80;
using BcdOp = Managed.Emulation.Execution.FullExtended80BcdOperation;

unsafe partial class Program
{
    // Parent alone wires, builds, emits and executes this suite. ALL UNRUN.
    static void CheckFullExtended80Bcd()
    {
        CheckBcd80Literals();CheckBcd80Emission();
        foreach(BcdOp op in Enum.GetValues<BcdOp>())
        foreach(bool windows in new[]{false,true})foreach(bool continuation in new[]{false,true})
        {
            using var code=Bcd80Code(op,windows,continuation);
            using var ret=Bcd80Publish([0xf3,0x0f,0x1e,0xfa,0xc3]);
            using var entry=Bcd80Adapter(code.Entry,windows,continuation);
            nint tail=continuation?ret.Entry:0;
            foreach(C80 input in Bcd80Cases(op))
            foreach(int rc in new[]{0,1,2,3})foreach(int pc in new[]{0,1,2,3})foreach(int masks in new[]{0,63})
            {
                ushort cw=(ushort)(0x40|masks|(pc<<8)|(rc<<10));
                Bcd80Invoke(entry.Entry,input,cw,Bcd80Reference(op,input,cw),tail);
            }
            // Literal images under every actual mask, including irrelevant DE/
            // ZE/OE/UE masks, and every PC including reserved01.
            foreach(var row in Bcd80Literals())if(row.Op==op)
            {
                C80 input=Bcd80Image(row.Input);
                Bcd80Invoke(entry.Entry,input,row.Cw,
                    new(Bcd80Image(row.Output),row.Masked,row.Events,row.Metadata,row.Admission),tail);
                for(int masks=0;masks<64;masks++)for(int pc=0;pc<4;pc++)
                {
                    ushort cw=(ushort)(0x40|masks|(pc<<8)|(row.Cw&0xc00));
                    Bcd80Invoke(entry.Entry,input,cw,Bcd80Reference(op,input,cw),tail);
                }
            }
            Bcd80Protocol(entry.Entry,tail);Bcd80Warm(entry.Entry,tail);
            using var probe=Bcd80PreservationProbe(entry.Entry);
            var frame=Bcd80Frame(op==BcdOp.LoadPackedBcd?Bcd80Packed(999999999999999999,false):new(0xde0b6b3a763ffff8,0x403a),0x17f,tail);
            Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)probe.Entry)(&frame)==0,
                "R152 callee-save GPR union preserved, both target ABIs/entries (UNRUN)");
        }
    }
    readonly record struct Bcd80Expected(C80 Value,ushort Masked,ushort Events,uint Metadata,uint Admission=0);
    static NativeCodeAllocation Bcd80Publish(ReadOnlySpan<byte> b)
    {
        Check(NativeCodeAllocation.TryCreate(b,out var owner,out string why,requireNativeFlags:false),why);
        return owner!;
    }
    static NativeCodeAllocation Bcd80Code(BcdOp op,bool windows,bool continuation)
    {
        byte[] b=new byte[FullExtended80BcdEmitter.CodeCapacity];int n;
        bool ok=continuation?FullExtended80BcdEmitter.TryEmitContinuation(op,b,out n):
            FullExtended80BcdEmitter.TryEmit(op,b,out n,windows);
        Check(ok&&n>0&&n<=b.Length,"R152 bounded specialized conversion emission");
        return Bcd80Publish(b.AsSpan(0,n));
    }
    static NativeCodeAllocation Bcd80Adapter(nint target,bool windows,bool continuation)
    {
        var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};bool hostWin=OperatingSystem.IsWindows();
        if(continuation)
        {b.AddRange(hostWin?new byte[]{0x49,0x89,0xcb}:new byte[]{0x49,0x89,0xfb});}
        else if(windows!=hostWin)
        {
            if(windows)b.AddRange(new byte[]{0x48,0x89,0xf9,0x48,0x83,0xec,0x28}); // RCX=RDI; Win64 home space/alignment
            else b.AddRange(new byte[]{0x57,0x48,0x89,0xcf,0x48,0x83,0xec,0x20});
        }
        b.AddRange(new byte[]{0x48,0xb8});Bcd80U64(b,(ulong)target);
        if(!continuation&&hostWin!=windows)
        {
            b.AddRange(new byte[]{0xff,0xd0,0x48,0x83,0xc4,(byte)(windows?0x28:0x20)});
            if(hostWin)b.Add(0x5f);
            b.Add(0xc3);
        }
        else b.AddRange(new byte[]{0xff,0xe0});
        return Bcd80Publish(b.ToArray());
    }
    static void Bcd80U64(List<byte> b,ulong v){for(int i=0;i<8;i++)b.Add((byte)(v>>(8*i)));}
    static void Bcd80I32(List<byte> b,int v){for(int i=0;i<4;i++)b.Add((byte)(v>>(8*i)));}

    // Test-only synchronous machine-code wrapper, with normal native CALL/RET.
    // Saves/restores the SysV/Win64 union of nonvolatile GPRs, poisons each,
    // then checks after kernel. SysV RDI is checked against its frame argument.
    // No async stack walking, host FP or semantic helper. Does not qualify
    // asynchronous unwind or Windows execution until actually run there.
    static NativeCodeAllocation Bcd80PreservationProbe(nint target)
    {
        bool win=OperatingSystem.IsWindows();int[] regs=new[]{3,5,6,7,12,13,14,15};
        var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};
        foreach(int r in regs){if(r>=8)b.Add(0x41);b.Add((byte)(0x50+(r&7)));}
        b.Add((byte)(win?0x51:0x57)); // save incoming frame; even regs + one push aligns CALL
        b.AddRange(new byte[]{0x48,0x83,0xec,0x20});
        for(int i=0;i<regs.Length;i++)
        {int r=regs[i];b.Add((byte)(r>=8?0x49:0x48));b.Add((byte)(0xb8+(r&7)));Bcd80U64(b,0x123456789abc0000UL+(uint)i);}
        // Reload argument from above shadow space after poisoning RDI on Win64.
        b.AddRange(win?new byte[]{0x48,0x8b,0x4c,0x24,0x20}:new byte[]{0x48,0x8b,0x7c,0x24,0x20});
        b.AddRange(new byte[]{0x48,0xb8});Bcd80U64(b,(ulong)target);b.AddRange(new byte[]{0xff,0xd0,0x45,0x31,0xd2});
        for(int i=0;i<regs.Length;i++)
        {
            int r=regs[i];
            if(!win&&r==7)b.AddRange(new byte[]{0x48,0x8b,0x44,0x24,0x20});
            else {b.AddRange(new byte[]{0x48,0xb8});Bcd80U64(b,0x123456789abc0000UL+(uint)i);}
            b.Add((byte)(r>=8?0x49:0x48));b.Add(0x39);b.Add((byte)(0xc0|(r&7)));
            b.AddRange(new byte[]{0x74,0x07,0x41,0x81,0xca});Bcd80I32(b,1<<i);
        }
        b.AddRange(new byte[]{0x44,0x89,0xd0,0x48,0x83,0xc4,0x28});
        for(int i=regs.Length-1;i>=0;i--){int r=regs[i];if(r>=8)b.Add(0x41);b.Add((byte)(0x58+(r&7)));}
        b.Add(0xc3);return Bcd80Publish(b.ToArray());
    }
    static FullExtended80WorkFrame Bcd80Frame(C80 input,ushort cw,nint continuation)=>new()
    {Version=1,Size=384,ControlWord=cw,Left=input,Continuation=(ulong)continuation};
    static bool Bcd80Same(C80 a,C80 b)=>a.Significand==b.Significand&&a.SignExponent==b.SignExponent;
    static void Bcd80Invoke(nint entry,C80 input,ushort cw,Bcd80Expected expected,nint continuation)
    {
        byte* guarded=stackalloc byte[416];new Span<byte>(guarded,416).Fill(0xad);
        var f=(FullExtended80WorkFrame*)(guarded+16);*f=Bcd80Frame(input,cw,continuation);
        f->Reserved10=0x1133;f->Left.Reserved10=0x2468;f->Left.Reserved12=0xabcdef01;
        new Span<byte>((byte*)f+32,16).Fill(0x69);new Span<byte>((byte*)f+48,40).Fill(0xce);
        new Span<byte>((byte*)f+96,288).Fill(0xad);
        byte* before=stackalloc byte[384];new ReadOnlySpan<byte>(f,384).CopyTo(new Span<byte>(before,384));
        uint result=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(f);
        Check(result==expected.Admission&&(uint)f->Admission==expected.Admission,"R152 EAX/frame Complete");
        Check(Bcd80Same(f->Normal,expected.Value)&&f->Normal.Reserved10==0&&f->Normal.Reserved12==0&&
            new ReadOnlySpan<byte>((byte*)f+64,16).IndexOfAnyExcept((byte)0)<0&&
            f->NormalStatus==expected.Masked&&f->WrappedStatus==expected.Events&&(uint)f->Classification==expected.Metadata,
            "R152 independent rational output/masked SW/event priority/metadata");
        Check(new ReadOnlySpan<byte>(f,12).SequenceEqual(new ReadOnlySpan<byte>(before,12))&&
            new ReadOnlySpan<byte>((byte*)f+16,32).SequenceEqual(new ReadOnlySpan<byte>(before+16,32))&&
            new ReadOnlySpan<byte>((byte*)f+88,8).SequenceEqual(new ReadOnlySpan<byte>(before+88,8))&&
            new ReadOnlySpan<byte>((byte*)f+256,128).SequenceEqual(new ReadOnlySpan<byte>(before+256,128)),
            "R152 input/padding/right/continuation/reserved immutable");
        Check(new ReadOnlySpan<byte>(guarded,16).IndexOfAnyExcept((byte)0xad)<0&&
            new ReadOnlySpan<byte>(guarded+400,16).IndexOfAnyExcept((byte)0xad)<0,"R152 frame guard bounds");
    }
    static void Bcd80Protocol(nint entry,nint continuation)
    {
        foreach((uint version,uint size,uint reason) in new[]{(2u,384u,1u),(0u,384u,1u),(1u,0u,2u),(1u,383u,2u),(1u,385u,2u),(1u,uint.MaxValue,2u)})
        {
            var f=Bcd80Frame(default,0x17f,continuation);f.Version=version;f.Size=size;
            new Span<byte>((byte*)&f+48,40).Fill(0xce);
            Check(((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f)==reason&&
                (uint)f.Admission==reason&&new ReadOnlySpan<byte>((byte*)&f+48,40).IndexOfAnyExcept((byte)0)<0,
                "R152 strict version/size; no output validity on protocol reject");
        }
    }
    static C80 Bcd80Image(string hex)
    {
        byte[] b=Convert.FromHexString(hex);
        if(b.Length!=10)throw new InvalidOperationException("ten-byte literal required");
        return new(BinaryPrimitives.ReadUInt64LittleEndian(b),BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(8)));
    }
    static void CheckBcd80Emission()
    {
        byte[] b=new byte[FullExtended80BcdEmitter.CodeCapacity+32];
        foreach(BcdOp op in Enum.GetValues<BcdOp>())foreach(bool win in new[]{false,true})foreach(bool cont in new[]{false,true})
        {
            bool ok=cont?FullExtended80BcdEmitter.TryEmitContinuation(op,b,out int n):FullExtended80BcdEmitter.TryEmit(op,b,out n,win);
            Check(ok&&n>0&&n<=FullExtended80BcdEmitter.CodeCapacity,"R152 bounded code capacity");
            var image=b.AsSpan(0,n);
            Check(image[..4].SequenceEqual(new byte[]{0xf3,0x0f,0x1e,0xfa}),"R152 ENDBR64 prefix");
            Check(cont||image.Slice(4,3).SequenceEqual(win?new byte[]{0x49,0x89,0xcb}:new byte[]{0x49,0x89,0xfb}),
                "R152 literal SysV RDI / Win64 RCX ingress");
            Check(cont?image[^7..].SequenceEqual(new byte[]{0x41,0xff,0xa3,88,0,0,0}):image[n-1]==0xc3,
                "R152 RET / JMP[R11+88] tail");
            for(int size=0;size<n;size++)
            {
                Array.Fill(b,(byte)0xb7);
                bool shortOk=cont?FullExtended80BcdEmitter.TryEmitContinuation(op,b.AsSpan(8,size),out int m):
                    FullExtended80BcdEmitter.TryEmit(op,b.AsSpan(8,size),out m,win);
                Check(!shortOk&&m==0&&b.AsSpan(0,8).IndexOfAnyExcept((byte)0xb7)<0&&
                    b.AsSpan(8+size).IndexOfAnyExcept((byte)0xb7)<0,"R152 every short span false/length0 with guards");
            }
            for(int i=0;i<16;i++)if(cont)FullExtended80BcdEmitter.TryEmitContinuation(op,b,out _);else FullExtended80BcdEmitter.TryEmit(op,b,out _,win);
            long before=GC.GetAllocatedBytesForCurrentThread();bool all=true;
            for(int i=0;i<128;i++)all&=cont?FullExtended80BcdEmitter.TryEmitContinuation(op,b,out _):FullExtended80BcdEmitter.TryEmit(op,b,out _,win);
            Check(all&&GC.GetAllocatedBytesForCurrentThread()==before,"R152 zero warm emission allocation, UNRUN");
        }
        foreach(byte unknown in new byte[]{2,3,127,255})
        {
            Array.Fill(b,(byte)0xb7);
            Check(!FullExtended80BcdEmitter.TryEmit((BcdOp)unknown,b,out int n)&&n==0&&
                !FullExtended80BcdEmitter.TryEmitContinuation((BcdOp)unknown,b,out n)&&n==0&&
                b.AsSpan().IndexOfAnyExcept((byte)0xb7)<0,"R152 unknown selector rejects before writes");
        }
    }
    static void Bcd80Warm(nint entry,nint continuation)
    {
        var f=Bcd80Frame(new(0,0x8000),0x17f,continuation);
        for(int i=0;i<16;i++)((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        long before=GC.GetAllocatedBytesForCurrentThread();uint result=0;
        for(int i=0;i<256;i++)result|=((delegate* unmanaged<FullExtended80WorkFrame*,uint>)entry)(&f);
        Check(result==0&&GC.GetAllocatedBytesForCurrentThread()==before,"R152 native warm invocation allocation, PC01, UNRUN");
    }

    static C80 Bcd80Packed(BigInteger magnitude,bool negative,int padding=0)
    {
        // Deliberately decimal string reference, independent of native Horner
        // and repeated DIV. Test-only BigInteger and allocations are allowed.
        string digits=magnitude.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(18,'0');
        byte[] b=new byte[10];
        for(int i=0;i<18;i++)b[i/2]|=(byte)((digits[17-i]-'0')<<((i&1)*4));
        b[9]=(byte)((negative?128:0)|padding);
        return new(BinaryPrimitives.ReadUInt64LittleEndian(b),BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(8)));
    }
    static C80 Bcd80Exact(BigInteger n,bool negative)
    {
        if(n.IsZero)return new(0,(ushort)(negative?0x8000:0));
        int top=(int)n.GetBitLength()-1;
        return new((ulong)(n<<(63-top)),(ushort)((negative?0x8000:0)|(top+16383)));
    }
    static IEnumerable<C80> Bcd80Cases(BcdOp op)
    {
        if(op==BcdOp.LoadPackedBcd)
        {
            foreach(bool sign in new[]{false,true})
            {
                foreach(BigInteger n in new BigInteger[]{0,1,9,10,99,100,9007199254740993,999999999999999999})
                foreach(int pad in new[]{0,1,63,127})yield return Bcd80Packed(n,sign,pad);
                // Every decimal position/digit, with padding isolated from digits.
                for(int position=0;position<18;position++)for(int digit=0;digit<10;digit++)
                    yield return Bcd80Packed(digit*BigInteger.Pow(10,position),sign,127);
                // Every invalid nibble at every position rejects distinctly,
                // never silently substitutes a parent helper or numeric IE.
                for(int position=0;position<18;position++)for(int digit=10;digit<16;digit++)
                {
                    BigInteger bits=(BigInteger)digit<<(position*4);
                    yield return new((ulong)(bits&ulong.MaxValue),(ushort)((ulong)(bits>>64)|(sign?0x8000UL:0)));
                }
                // Every don't-care padding pattern is accepted for a valid value.
                for(int pad=0;pad<128;pad++)yield return Bcd80Packed(123456789012345678,sign,pad);
            }
            yield break;
        }
        foreach(int sign in new[]{0,0x8000})
        {
            foreach(int exponent in new[]{0,1,2,0x3fbd,0x3fbe,0x3ffd,0x3ffe,0x3fff,0x4000,0x4038,0x4039,0x403a,0x403b,0x403e,0x7ffe,0x7fff})
            foreach(ulong sig in new[]{0UL,1UL,2UL,0x3fffffffffffffffUL,0x4000000000000000UL,0x7fffffffffffffffUL,
                0x8000000000000000UL,0x8000000000000001UL,0xbfffffffffffffffUL,0xc000000000000000UL,
                0xc000000000000001UL,0xffffffffffffffffUL})
                yield return new(sig,(ushort)(exponent|sign));
            // Binary80 units here are1/16. Sweep both sides of the18-digit
            // boundary, including half ties and rounded-magnitude overflow.
            BigInteger edge=(BigInteger)999999999999999999*16;
            for(int delta=-33;delta<=33;delta++)yield return new((ulong)(edge+delta),(ushort)(0x403a|sign));
            foreach(ulong n in new[]{0UL,1UL,2UL,3UL,9UL,10UL,9007199254740993UL,999999999999999999UL,1000000000000000000UL})
                yield return Bcd80Exact(n,sign!=0);
        }
    }

    // Independent exact rational oracle. No product numeric code, float/double/
    // decimal, host x87, interpreter, arithmetic conversion oracle or GPR model.
    static Bcd80Expected Bcd80Reference(BcdOp op,C80 input,ushort cw)
    {
        if(op==BcdOp.LoadPackedBcd)
        {
            BigInteger bits=input.Significand|((BigInteger)(input.SignExponent&255)<<64),n=0;
            for(int position=0;position<18;position++)
            {
                int digit=(int)((bits>>(4*position))&15);
                if(digit>9)return new(default,0,0,0,3);
                n+=digit*BigInteger.Pow(10,position);
            }
            return Bcd80Finish(Bcd80Exact(n,(input.SignExponent&0x8000)!=0),0,cw);
        }
        int exponent=input.SignExponent&0x7fff;bool negative=(input.SignExponent&0x8000)!=0;
        if((exponent!=0&&(input.Significand>>63)==0)||exponent==0x7fff)return Bcd80Invalid(cw);
        BigInteger numerator=input.Significand,denominator=BigInteger.One;
        int power=(exponent==0?1:exponent)-16383-63;
        if(power>=0)numerator<<=power;else denominator<<=-power;
        BigInteger quotient=BigInteger.DivRem(numerator,denominator,out BigInteger remainder);
        bool lost=!remainder.IsZero;int rc=(cw>>10)&3;
        bool up=lost&&(rc==0?(2*remainder>denominator||(2*remainder==denominator&&!quotient.IsEven)):
            rc==1?negative:rc==2&&!negative);
        if(up)quotient++;
        if(quotient>999999999999999999)return Bcd80Invalid(cw);
        return Bcd80Finish(Bcd80Packed(quotient,negative),(lost?32:0)|(up?512:0),cw);
    }
    static Bcd80Expected Bcd80Invalid(ushort cw)=>Bcd80Finish(new(0xc000000000000000,0xffff),1,cw);
    static Bcd80Expected Bcd80Finish(C80 output,int status,ushort cw)
    {
        uint metadata=1;
        if((status&1)!=0)metadata|=8;
        else if((status&32)==0)metadata|=2;
        if((status&32)!=0)metadata|=128;
        if((status&512)!=0)metadata|=256;
        int events=status;
        if((status&1)!=0&&(cw&1)==0){metadata|=512;events=1;}
        else if((status&32)!=0&&(cw&32)==0)metadata|=1024;
        return new(output,(ushort)status,(ushort)events,metadata);
    }
    readonly record struct Bcd80Literal(BcdOp Op,string Input,ushort Cw,string Output,ushort Masked,ushort Events,uint Metadata,uint Admission=0);
    // Literal10-byte memory images: 037f nearest,077f down,0b7f up,0f7f chop.
    // Anchors specify ISA/mathematical outcomes independently of both algorithms.
    static IEnumerable<Bcd80Literal> Bcd80Literals()
    {
        yield return new(BcdOp.LoadPackedBcd,"00000000000000000000",0x017f,"00000000000000000000",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"00000000000000000080",0x017f,"00000000000000000080",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"0000000000000000007F",0x017f,"00000000000000000000",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"000000000000000000FF",0x017f,"00000000000000000080",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"01000000000000000000",0x017f,"0000000000000080FF3F",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"010000000000000000FF",0x017f,"0000000000000080FFBF",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"09000000000000000000",0x017f,"00000000000000900240",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"10000000000000000080",0x017f,"00000000000000A002C0",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"99000000000000000000",0x017f,"00000000000000C60540",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"7856341290785634127F",0x017f,"00A77918D3A54DDB3740",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"93097454921907900000",0x017f,"00040000000000803440",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"99999999999999999900",0x017f,"F0FF3F763A6B0BDE3A40",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"999999999999999999FF",0x017f,"F0FF3F763A6B0BDE3AC0",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"00000000000000009000",0x017f,"0000A09DB413D7C73A40",0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"0A000000000000000000",0x037f,"00000000000000000000",0,0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"0000000000000000F000",0x037f,"00000000000000000000",0,0,0,3);
        yield return new(BcdOp.LoadPackedBcd,"00000000000000C0FFFF",0x037f,"00000000000000000000",0,0,0,3);
        yield return new(BcdOp.StorePackedBcd,"00000000000000000000",0x017f,"00000000000000000000",0,0,3);
        yield return new(BcdOp.StorePackedBcd,"00000000000000000080",0x017f,"00000000000000000080",0,0,3);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FF3F",0x017f,"01000000000000000000",0,0,3);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FFBF",0x017f,"01000000000000000080",0,0,3);
        yield return new(BcdOp.StorePackedBcd,"00040000000000803440",0x017f,"93097454921907900000",0,0,3);
        yield return new(BcdOp.StorePackedBcd,"F0FF3F763A6B0BDE3A40",0x017f,"99999999999999999900",0,0,3);
        yield return new(BcdOp.StorePackedBcd,"F0FF3F763A6B0BDE3AC0",0x017f,"99999999999999999980",0,0,3);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FE3F",0x037f,"00000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FE3F",0x077f,"00000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FE3F",0x0b7f,"01000000000000000000",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FE3F",0x0f7f,"00000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000C0FF3F",0x037f,"02000000000000000000",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"00000000000000C0FF3F",0x077f,"01000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000C0FF3F",0x0b7f,"02000000000000000000",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"00000000000000C0FF3F",0x0f7f,"01000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000A00040",0x037f,"02000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000A00040",0x077f,"02000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000A00040",0x0b7f,"03000000000000000000",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"00000000000000A00040",0x0f7f,"02000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FEBF",0x037f,"00000000000000000080",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FEBF",0x077f,"01000000000000000080",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FEBF",0x0b7f,"00000000000000000080",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FEBF",0x0f7f,"00000000000000000080",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000C0FFBF",0x037f,"02000000000000000080",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"00000000000000C0FFBF",0x077f,"02000000000000000080",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"00000000000000C0FFBF",0x0b7f,"01000000000000000080",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000C0FFBF",0x0f7f,"01000000000000000080",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000A000C0",0x037f,"02000000000000000080",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000A000C0",0x077f,"03000000000000000080",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"00000000000000A000C0",0x0b7f,"02000000000000000080",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000A000C0",0x0f7f,"02000000000000000080",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"0100000000000080FE3F",0x037f,"01000000000000000000",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"FFFFFFFFFFFFFFFFFD3F",0x037f,"00000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"01000000000000000000",0x037d,"00000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"01000000000000000000",0x0b7d,"01000000000000000000",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"01000000000000000080",0x077d,"01000000000000000080",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"00000000000000800000",0x037d,"00000000000000000000",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"00000000000000800080",0x077d,"01000000000000000080",544,544,385);
        yield return new(BcdOp.StorePackedBcd,"00000000000000C0FF3F",0x035f,"02000000000000000000",544,544,1409);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FF7F",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FFFF",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"01000000000000C0FF7F",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"01000000000000C0FFFF",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"0100000000000080FF7F",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"0100000000000080FFFF",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"0000000000000040FF3F",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"0000000000000000FF3F",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"0000000000000000FF7F",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"0000000000000080FE7F",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"01000000000000C0FF7F",0x037e,"00000000000000C0FFFF",1,1,521);
        yield return new(BcdOp.StorePackedBcd,"F8FF3F763A6B0BDE3A40",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"F8FF3F763A6B0BDE3A40",0x037e,"00000000000000C0FFFF",1,1,521);
        yield return new(BcdOp.StorePackedBcd,"F8FF3F763A6B0BDE3A40",0x0f7f,"99999999999999999900",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"E8FF3F763A6B0BDE3A40",0x037f,"98999999999999999900",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"FFFF3F763A6B0BDE3A40",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"FFFF3F763A6B0BDE3A40",0x0f5f,"99999999999999999900",32,32,1153);
        yield return new(BcdOp.StorePackedBcd,"000040763A6B0BDE3A40",0x0f7f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"F8FF3F763A6B0BDE3AC0",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"F8FF3F763A6B0BDE3AC0",0x037e,"00000000000000C0FFFF",1,1,521);
        yield return new(BcdOp.StorePackedBcd,"F8FF3F763A6B0BDE3AC0",0x0f7f,"99999999999999999980",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"E8FF3F763A6B0BDE3AC0",0x037f,"98999999999999999980",32,32,129);
        yield return new(BcdOp.StorePackedBcd,"FFFF3F763A6B0BDE3AC0",0x037f,"00000000000000C0FFFF",1,1,9);
        yield return new(BcdOp.StorePackedBcd,"FFFF3F763A6B0BDE3AC0",0x0f5f,"99999999999999999980",32,32,1153);
        yield return new(BcdOp.StorePackedBcd,"000040763A6B0BDE3AC0",0x0f7f,"00000000000000C0FFFF",1,1,9);
    }
    static void CheckBcd80Literals()
    {
        foreach(var row in Bcd80Literals())
        {
            var actual=Bcd80Reference(row.Op,Bcd80Image(row.Input),row.Cw);
            Check(Bcd80Same(actual.Value,Bcd80Image(row.Output))&&actual.Masked==row.Masked&&
                actual.Events==row.Events&&actual.Metadata==row.Metadata&&actual.Admission==row.Admission,
                "R152 literal byte images constrain independent BigInteger rational oracle (UNRUN)");
        }
    }
}
