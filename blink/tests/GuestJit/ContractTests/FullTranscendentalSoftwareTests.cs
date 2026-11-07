using System;
using System.Numerics;
using Managed.Emulation.Execution;
using Raw168=Managed.Emulation.Execution.FullExtended80;
using Op168=Managed.Emulation.Execution.FullExtended80Transcendental;

unsafe partial class Program
{
    // AUTHORED_UNRUN. A single family entry, deliberately not auto-wired into
    // the parent's runner. Literal intervals come from independently outward
    // rational references in math/model.py, not the candidate arithmetic.
    readonly record struct Trans168Vector(int Op,Raw168 X,Raw168 Y,
        string Low,string High,int Exponent,string SecondLow,string SecondHigh,int SecondExponent,bool Exact);
    readonly record struct Trans168Rounded(ulong Sig,ushort Se,ushort Status);
    static NativeCodeAllocation Trans168Code(Op168 op,bool windows,bool continuation,out int length)
    {
        byte[] b=new byte[FullExtended80TranscendentalSoftwareEmitter.CodeCapacity];
        bool ok=continuation?FullExtended80TranscendentalSoftwareEmitter.TryEmitContinuation(op,b,out length):
            FullExtended80TranscendentalSoftwareEmitter.TryEmit(op,b,out length,windows);
        Check(ok&&length>0&&length<=b.Length,"R168 full software family emits in charged capacity");
        Trans182RecordImage(op,windows,continuation,b.AsSpan(0,length));
        return Transform80Publish(b.AsSpan(0,length));
    }
    // Independent exact rational rounding: integer N times 2^exponent.
    // Intervals retain their own exponent, including 2^-32890 products.
    static Trans168Rounded Trans168Round(BigInteger n,int exponent,int rc,int masks,bool approximate)
    {
        bool negative=n.Sign<0;n=BigInteger.Abs(n);
        if(n.IsZero)return new(0,(ushort)(negative?0x8000:0),(ushort)(approximate?32:0));
        int e=checked((int)n.GetBitLength()-1+exponent);
        (BigInteger Q,bool Inexact,bool Increment,int E) RoundAt(int unit,int normalExponent)
        {
            int shift=exponent-unit;BigInteger q,remainder,denominator;
            if(shift>=0){q=n<<shift;remainder=0;denominator=1;}
            else{denominator=BigInteger.One<<-shift;q=BigInteger.DivRem(n,denominator,out remainder);}
            bool inc=!remainder.IsZero&&(rc==0?(remainder*2>denominator||remainder*2==denominator&&!q.IsEven):rc==1?negative:rc==2&&!negative);
            if(inc)++q;
            if(q==(BigInteger.One<<64)){q>>=1;++normalExponent;}
            return(q,!remainder.IsZero,inc,normalExponent);
        }
        var r=RoundAt(e-63,e);int range=0;
        if(r.E>16383)
        {
            if((masks&8)==0){e=r.E-24576;range=8;}
            else
            {
                bool inf=rc==0||rc==1&&negative||rc==2&&!negative;
                return new(inf?0x8000000000000000:ulong.MaxValue,(ushort)((inf?0x7fff:0x7ffe)|(negative?0x8000:0)),(ushort)(inf?0x228:0x28));
            }
        }
        else if(r.E<-16382)
        {
            if((masks&16)==0){e=r.E+24576;range=16;}
            else
            {
                r=RoundAt(-16445,-16382);
                int status=(r.Inexact||approximate?0x30:0)|(r.Increment?0x200:0);
                return new((ulong)r.Q,(ushort)((r.Q>=(BigInteger.One<<63)?1:0)|(negative?0x8000:0)),(ushort)status);
            }
        }
        else e=r.E;
        return new((ulong)r.Q,(ushort)((e+16383)|(negative?0x8000:0)),
            (ushort)(range|(r.Inexact||approximate?32:0)|(r.Increment?0x200:0)));
    }
    static bool Trans168Certify(string lo,string hi,int exponent,int rc,int masks,bool approximate,out Trans168Rounded result)
    {
        result=Trans168Round(BigInteger.Parse(lo),exponent,rc,masks,approximate);
        return result==Trans168Round(BigInteger.Parse(hi),exponent,rc,masks,approximate);
    }
    static int Trans168DE(Raw168 x)=>((x.SignExponent&0x7fff)==0&&x.Significand!=0)?2:0;
    static void Trans168Raw(Raw168 actual,Trans168Rounded expected,string label)
        =>Check(actual.Significand==expected.Sig&&actual.SignExponent==expected.Se,label);
    static void Trans168Near(Raw168 value,string low,string high,int exp,Op168 op,ushort status,int masks)
    {
        if((value.SignExponent&0x7fff)==0x7fff)return; // checked by certified range/literal rows
        BigInteger lo=BigInteger.Parse(low),hi=BigInteger.Parse(high);
        BigInteger mag=BigInteger.Max(BigInteger.Abs(lo),BigInteger.Abs(hi));
        if(mag.IsZero)return;
        int referenceE=checked((int)mag.GetBitLength()-1+exp);
        int actualE=(value.SignExponent&0x7fff)==0?-16445:(value.SignExponent&0x7fff)-16383-63;
        if(referenceE>16383&&(status&8)!=0&&(masks&8)==0)actualE+=24576;
        if(referenceE<-16382&&(status&16)!=0&&(masks&16)==0)actualE-=24576;
        int ulp=referenceE-63;
        if(referenceE<-16382&&(masks&16)!=0)ulp=-16445;
        int internalError=(Trans157Trig(op)?Math.Max(referenceE,0):referenceE)-170;
        int common=Math.Min(Math.Min(actualE,exp),Math.Min(ulp,internalError));
        BigInteger actual=new BigInteger(value.Significand)<<(actualE-common);
        if((value.SignExponent&0x8000)!=0)actual=-actual;
        lo<<=exp-common;hi<<=exp-common;
        BigInteger error=BigInteger.Max(BigInteger.Zero,BigInteger.Max(lo-actual,actual-hi));
        BigInteger allowance=(BigInteger.One<<(ulp-common))+(BigInteger.One<<(internalError-common));
        Check(error<=allowance,"R168 explicit per-row portable finite envelope, retained tiny exponent, one final rounding unit");
    }
    static bool Trans168Undefined(Op168 op,Raw168 x)
    {
        int e=x.SignExponent&0x7fff;
        if(op==Op168.F2xm1)return e>0x3fff||e==0x3fff&&x.Significand>0x8000000000000000UL;
        if(op!=Op168.Fyl2xp1)return false;
        ulong edge=(x.SignExponent&0x8000)!=0?0x95f619980c4336f7UL:0xd413cccfe7799211UL;
        return e>0x3ffd||e==0x3ffd&&x.Significand>edge;
    }
    static void Trans168SoftwareCodeSizes()
    {
        foreach(Op168 op in Enum.GetValues<Op168>())
        {
            int maximum=0;byte[] bytes=new byte[FullExtended80TranscendentalSoftwareEmitter.CodeCapacity];
            foreach(bool windows in new[]{false,true})foreach(bool tail in new[]{false,true})
            {
                bool ok=tail?FullExtended80TranscendentalSoftwareEmitter.TryEmitContinuation(op,bytes,out int length):
                    FullExtended80TranscendentalSoftwareEmitter.TryEmit(op,bytes,out length,windows);
                Check(ok,"R168 family emission size proof before any native call");maximum=Math.Max(maximum,length);
            }
            Console.WriteLine($"R168-EMITTED-SIZE op={op} maxBytes={maximum}");
        }
    }
    static void Trans168SoftwareFamilySuite(bool exhaustiveControls=false)
    {
        Check(!FullExtended80TranscendentalProtocol.GuestActivationReady||FullExtended80TranscendentalSoftwareProfile.Qualified,
            "portable production readiness requires source-bound numerical acceptance");
        foreach(Op168 op in Enum.GetValues<Op168>())
        foreach(bool windows in new[]{false,true})foreach(bool tail in new[]{false,true})
        {
            using var code=Trans168Code(op,windows,tail,out _);
            using var ret=Transform80Publish([0xf3,0x0f,0x1e,0xfa,0xc3]);
            using var adapter=Transform80Adapter(code.Entry,windows,tail);
            nint continuation=tail?ret.Entry:0;
            foreach(var v in trans168Vectors)
            {
                if(v.Op!=(int)op)continue;
                foreach(int rc in new[]{0,1,2,3})foreach(int pc in new[]{0,1,2,3})
                foreach(int masks in exhaustiveControls?System.Linq.Enumerable.Range(0,64):new[]{0,7,15,23,31,39,47,55,63})
                {
                    var f=Trans157Frame(v.X,v.Y,(ushort)(0x40|pc<<8|rc<<10|masks),continuation);
                    Trans157Invoke(adapter.Entry,ref f); // existing whole-Work guards/padding/input checks
                    // These mathematical reference rows also cross named ISA
                    // undefined domains. Assert the explicit CopyInput profile,
                    // never compare those results with log/exp mathematics.
                    if(Trans168Undefined(op,v.X))
                    {
                        var expected=op==Op168.F2xm1?v.X:v.Y;
                        Check((uint)f.Admission==0&&(uint)f.Classification==(uint)((int)op+1|0x8100)&&
                            f.Normal.Significand==expected.Significand&&f.Normal.SignExponent==expected.SignExponent&&
                            f.Wrapped.Significand==0&&f.Wrapped.SignExponent==0&&f.NormalStatus==0&&f.WrappedStatus==0,
                            "R168 undefined-domain CopyInput software profile is separate from numerical intervals");
                        Check(new ReadOnlySpan<byte>((byte*)&f+96,136).IndexOfAnyExcept((byte)0)<0&&
                            new ReadOnlySpan<byte>((byte*)&f+236,20).IndexOfAnyExcept((byte)0)<0&&
                            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>((byte*)&f+232,4))==0x02000000,
                            "R168 undefined-domain profile scrubs scratch and changes only C1");
                        continue;
                    }
                    int de=Trans168DE(v.X)+(Trans157Binary(op)?Trans168DE(v.Y):0);de=de!=0?2:0;
                    bool pre=de!=0&&(masks&2)==0;
                    uint m=(uint)f.Classification;
                    Check((uint)f.Admission==0&&(m&0x6000)==0&&(m&0x10000)!=0,
                        $"R168 response op={op} X={v.X.Significand:x16}:{v.X.SignExponent:x4} Y={v.Y.Significand:x16}:{v.Y.SignExponent:x4} rc={rc} pc={pc} masks={masks:x} admission={(uint)f.Admission:x} metadata={m:x} statuses={f.NormalStatus:x}/{f.WrappedStatus:x}");
                    if(pre)
                    {
                        Check(m==(uint)((int)op+1|0x10400)&&f.WrappedStatus==2,
                            "R168 unmasked DE priority suppresses both results/pop/push");
                        Check(f.Normal.Significand==0&&f.Normal.SignExponent==0&&f.Wrapped.Significand==0&&f.Wrapped.SignExponent==0,
                            "R168 pre-event has no usable raw destinations");
                    }
                    else
                    {
                        bool known=Trans168Certify(v.Low,v.High,v.Exponent,rc,masks,!v.Exact,out var a);
                        bool maskedKnown=Trans168Certify(v.Low,v.High,v.Exponent,rc,63,!v.Exact,out var am);
                        bool pair=Trans157Pair(op);
                        bool secondKnown=!pair;var b=default(Trans168Rounded);var bm=default(Trans168Rounded);
                        if(pair)
                        {
                            secondKnown=Trans168Certify(v.SecondLow,v.SecondHigh,v.SecondExponent,rc,masks,op!=Op168.Fptan,out b);
                            maskedKnown&=Trans168Certify(v.SecondLow,v.SecondHigh,v.SecondExponent,rc,63,op!=Op168.Fptan,out bm);
                        }
                        Trans168Near(f.Normal,v.Low,v.High,v.Exponent,op,f.WrappedStatus,masks);
                        if(pair)Trans168Near(f.Wrapped,v.SecondLow,v.SecondHigh,v.SecondExponent,op,f.WrappedStatus,masks);
                        if(known)Trans168Raw(f.Normal,a,"R168 normal/adjusted image equals independently certified rational rounding");
                        if(pair&&secondKnown)Trans168Raw(f.Wrapped,b,"R168 second destination order and rounding");
                        if(known&&secondKnown)
                        {
                            int status=a.Status|(op==Op168.Fsincos?b.Status&~0x200:b.Status)|de;
                            Check(f.WrappedStatus==status,"R168 actual PE/C1/OE/UE and exponent-wrap status against independent interval");
                            uint expected=(uint)((int)op+1|0x10100|(pair?0x200:0)|((status&32)!=0&&(masks&32)==0?0x1000:0));
                            Check(m==expected,"R168 precision completion metadata is computed from actual masks");
                        }
                        if(maskedKnown)Check(f.NormalStatus==(am.Status|(op==Op168.Fsincos?bm.Status&~0x200:bm.Status)|de),"R168 fully masked diagnostic computed separately");
                    }
                    Check(new ReadOnlySpan<byte>((byte*)&f+96,136).IndexOfAnyExcept((byte)0)<0&&
                        new ReadOnlySpan<byte>((byte*)&f+236,20).IndexOfAnyExcept((byte)0)<0,"R168 all160 scratch bytes scrubbed except documented condition word");
                    uint cc=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>((byte*)&f+232,4));
                    Check(cc==(uint)(pre?0x02000000:((Trans157Trig(op)?0x600:0x200)<<16)|(f.WrappedStatus&0x200)),
                        "R168 defined C1/C2 mask and no leaked reduction quadrant/counter");
                }
            }
            Trans168LiteralAnchors(adapter.Entry,op,continuation);
            Trans182NumericalClosure(adapter.Entry,op,continuation);
            Trans157Protocol(adapter.Entry,continuation);
            Trans157Specials(adapter.Entry,op,continuation);
            Trans157RangeExceptions(adapter.Entry,op,continuation);
            Trans157Ranges(adapter.Entry,op,continuation);
            if(op==Op168.Fyl2xp1)Trans157Log1pEndpoints(adapter.Entry,continuation);
            // No new host media probe is queued. The generated-code allowlist
            // and complete Work preservation are separate checks; parent retains
            // its existing independent caller-media/legacy-state gate.

        }
    }
    static void Trans168LiteralAnchors(nint entry,Op168 op,nint continuation)
    {
        Trans179ExactDyadicLog(entry,op,continuation);
        // Literal raw/status/mask/CC anchors, independent of both model kernels.
        foreach(ushort sign in new ushort[]{0,0x8000})foreach(int pc in new[]{0,1,2,3})
        foreach(int rc in new[]{0,1,2,3})foreach(int masks in System.Linq.Enumerable.Range(0,64))
        {
            var x=op is Op168.Fyl2x or Op168.Fpatan?new Raw168(0x8000000000000000,0x3fff):new Raw168(0,sign);
            var y=op==Op168.Fyl2xp1?new Raw168(0x8000000000000000,0x3fff):new Raw168(0,sign);
            var f=Trans157Frame(x,y,(ushort)(0x40|pc<<8|rc<<10|masks),continuation);Trans157Invoke(entry,ref f);
            Check(f.NormalStatus==0&&f.WrappedStatus==0&&(uint)f.Admission==0,"R168 exact signed zero has no forced PE");
            Check((uint)f.Classification==(uint)((int)op+1|0x10100|(Trans157Pair(op)?0x200:0)),"R168 literal exact completion kind/valid bits, no PE trap");
            uint cc=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>((byte*)&f+232,4));
            Check(cc==(uint)((Trans157Trig(op)?0x600:0x200)<<16),"R168 literal zero clears defined C1/C2 only");
            Trans168Raw(f.Normal,new(op==Op168.Fcos?0x8000000000000000:0,op==Op168.Fcos?(ushort)0x3fff:sign,0),"R168 literal signed zero/one");
        }
        if(op==Op168.Fyl2x)foreach(int rc in new[]{0,1,2,3})
        {
            var f=Trans157Frame(new(0x8000000000000000,0x4001),new(0x8000000000000000,0x7ffe),(ushort)(0x377|rc<<10),continuation);
            Trans157Invoke(entry,ref f);Trans168Raw(f.Normal,new(0x8000000000000000,0x1fff,8),"R168 literal exact overflow adjusted exponent -24576");
            Check(f.WrappedStatus==8,"R168 exact overflow does not invent PE");
            f=Trans157Frame(new(0x8000000000000000,0x3ffe),new(1,0),(ushort)(0x36f|rc<<10),continuation);
            Trans157Invoke(entry,ref f);Trans168Raw(f.Normal,new(0x8000000000000000,0xdfc2,0x12),"R168 literal exact tiny adjusted exponent +24576");
            Check(f.WrappedStatus==0x12,"R168 exact unmasked tiny UE retains DE without inventing PE");
        }
        if(op==Op168.F2xm1)foreach(var r in new[]{(Se:(ushort)0x3fff,Out:(ushort)0x3fff),(Se:(ushort)0xbfff,Out:(ushort)0xbffe)})
        foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})foreach(int masks in System.Linq.Enumerable.Range(0,64))
        {
            var f=Trans157Frame(new(0x8000000000000000,r.Se),default,(ushort)(0x40|pc<<8|rc<<10|masks),continuation);Trans157Invoke(entry,ref f);
            Trans168Raw(f.Normal,new(0x8000000000000000,r.Out,0),"R168 F2XM1 +/-1 exact anchors");Check(f.WrappedStatus==0,"R168 exact endpoints no PE");
        }
    }
    // Exact certifiable domain: log2(2^k)*signed2^e is a finite exact dyadic
    // with <=7 significand bits here. Independent integer construction, no
    // transcendental model or host FP. Nonzero approximation cells remain a
    // separate, unqualified error/refinement/native correspondence obligation.
    static void Trans179ExactDyadicLog(nint entry,Op168 op,nint continuation)
    {
        if(op!=Op168.Fyl2x)return;
        foreach(int k in new[]{-64,-1,0,1,63})foreach(int e in new[]{-100,-1,0,100})foreach(bool negative in new[]{false,true})
        foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})foreach(int masks in System.Linq.Enumerable.Range(0,64))
        {
            int magnitude=Math.Abs(k),bits=magnitude==0?0:(int)new BigInteger(magnitude).GetBitLength();
            bool sign=negative^(k<0);
            var expected=magnitude==0?new Trans168Rounded(0,(ushort)(negative?0x8000:0),0):
                new Trans168Rounded((ulong)magnitude<<(64-bits),(ushort)((e+bits-1+16383)|(sign?0x8000:0)),0);
            ushort cw=(ushort)(0x40|pc<<8|rc<<10|masks);
            var f=Trans157Frame(new(0x8000000000000000,(ushort)(k+16383)),
                new(0x8000000000000000,(ushort)((e+16383)|(negative?0x8000:0))),cw,continuation);
            Trans157Invoke(entry,ref f);
            Trans168Raw(f.Normal,expected,"R179r1 exact dyadic logarithm domain independent of approximate backend");
            Check(f.ControlWord==cw && f.Admission==0 && f.NormalStatus==0 && f.WrappedStatus==0,
                "R179r1 exact dyadic log all PC/masks/RC preserves raw CW and has no fabricated PE");
        }
    }

    // The old finite validator MUST stay closed for every new ordinary row.
    // This test proves neither general acceptance nor a new commit validator.
    static void Canon168SoftwareValidatorRemainsClosed(byte* fake,FullX87RegisterFile* state)
    {
        foreach(Op168 op in Enum.GetValues<Op168>())
        {
            using var kernel=Trans168Code(op,false,true,out int n);
            var i=Canon157Classify([0xd9,canon157Forms[(int)op]]);
            var call=new FullX87KernelCall((byte)(69+(int)op),kernel.Entry,73,n);
            using var body=CreateFullBlock([new(i,X87Kernel:call)]);Canon154Seed(fake,state,0,0x37f);
            Canon154Input(fake,state,0,new(0xa000000000000001,op==Op168.Fyl2x?(ushort)0x3fff:(ushort)0x3ffd));
            if(Trans157Binary(op))Canon154Input(fake,state,1,new(0x8000000000000000,0x3fff));
            if(Trans157Pair(op))Canon154Input(fake,state,7,default,3);
            byte[] m=CanonicalX87Bytes(fake,sizeof(Managed.Emulation.BlinkCore.Machine)),r=CanonicalX87Bytes((byte*)state,192);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,body.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
            Check(RunFull(body,&f)==FullBlockExit.Invalidated&&issued==1&&completed==0&&f.FirstPreissued==1,
                "R168 candidate general IntegerNative marker cannot bypass finite validator");
            CanonicalX87Same(fake,state,m,r);
        }
    }
}
