using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;
using Raw165=Managed.Emulation.Execution.FullExtended80;
using Op165=Managed.Emulation.Execution.FullExtended80Transcendental;

unsafe partial class Program
{
    // One coalesced INTEGER family seam. No host-numeric benchmark or host
    // qualification is implied. All native tests in this file AUTHORED_UNRUN.
    static IEnumerable<(Raw165 X,Raw165 Y)> Trans165IntegerRows(Op165 op)
    {
        var one=new Raw165(0x8000000000000000,0x3fff);
        foreach(ushort sign in new ushort[]{0,0x8000})
        {
            var z=new Raw165(0,sign);
            yield return op switch
            {
                Op165.Fyl2x => (one,z), Op165.Fpatan => (one,z),
                Op165.Fyl2xp1 => (z,one), _ => (z,one)
            };
        }
        if(op==Op165.F2xm1){yield return (one,default);yield return (new(0x8000000000000000,0xbfff),default);}
        if(op==Op165.Fpatan){yield return (default,default);yield return (new(0x8000000000000001,1),new(0,0x8000));}
        if(op==Op165.Fyl2xp1)
            foreach(ushort sx in new ushort[]{0,0x8000})foreach(ushort sy in new ushort[]{0,0x8000})
                yield return (new(0,sx),new(0,(ushort)sy));
        if(op==Op165.Fyl2x)
        {
            int row=0;
            foreach(int k in new[]{-16382,-37,-3,-1,0,1,3,37,16383})
            foreach(ulong sig in new[]{0x8000000000000000UL,0x8000000000000001UL,ulong.MaxValue})
            foreach(ushort sign in new ushort[]{0,0x8000})
            {
                int e=new[]{-16000,-123,0,57,16000}[row++%5];
                yield return (new(0x8000000000000000,(ushort)(k+16383)),new(sig,(ushort)(e+16383|sign)));
            }
            // Halfway-even, halfway-odd and carry after rounding by factor3.
            foreach(ulong sig in new[]{0xaaaaaaaaaaaaaaabUL,0xaaaaaaaaaaaaaaadUL,ulong.MaxValue})
                yield return (new(0x8000000000000000,0x4002),new(sig,0x3fff));
        }
    }
    static (Raw165 Value,ushort Status) Trans165IntegerOracle(Op165 op,Raw165 x,Raw165 y,int rc)
    {
        bool negX=(x.SignExponent&0x8000)!=0,negY=(y.SignExponent&0x8000)!=0;
        if(op==Op165.Fyl2x)
        {
            int k=(x.SignExponent&0x7fff)-16383;
            ushort sign=(ushort)((negY^(k<0))?0x8000:0);
            if(k==0||y.Significand==0)return(new(0,sign),0);
            // Independent mathematical dyadic integer oracle, no Op/libm/FX.
            BigInteger p=new BigInteger(y.Significand)*Math.Abs(k);
            int discarded=(int)p.GetBitLength()-64;
            BigInteger q=BigInteger.DivRem(p,BigInteger.One<<discarded,out var remainder);
            bool inc=remainder!=0&&(rc switch
            {0 => remainder*2>(BigInteger.One<<discarded)||remainder*2==(BigInteger.One<<discarded)&&!q.IsEven,
             1 => sign!=0,2 => sign==0,_ => false});
            if(inc)++q;
            int exponent=(y.SignExponent&0x7fff)+discarded;
            if(q==(BigInteger.One<<64)){q>>=1;++exponent;}
            return(new((ulong)q,(ushort)(exponent|sign)),(ushort)((remainder!=0?32:0)|(inc?0x200:0)));
        }
        if(op==Op165.Fyl2xp1)return(new(0,(ushort)((negX^negY)?0x8000:0)),0);
        if(op==Op165.Fpatan)return(new(0,(ushort)(negY?0x8000:0)),0);
        if(op==Op165.Fcos)return(new(0x8000000000000000,0x3fff),0);
        if(op==Op165.F2xm1&&x.Significand!=0)return(new(0x8000000000000000,negX?(ushort)0xbffe:(ushort)0x3fff),0);
        return(new(x.Significand,x.SignExponent),0);
    }
    static void Trans165IntegerNativeSuite()
    {
        Check(!FullExtended80TranscendentalProtocol.HostBackendActivationReady&&
            FullExtended80TranscendentalProtocol.HostStateBoundaryBlocker=="R165UniversalHostX87StateRestorationMISSING",
            "R165 partial replacement never clears universal host/async production blocker");
        foreach(Op165 op in Enum.GetValues<Op165>())foreach(bool windows in new[]{false,true})foreach(bool continuation in new[]{false,true})
        {
            using var code=Trans157Code(op,windows,continuation);
            using var ret=Transform80Publish([0xf3,0x0f,0x1e,0xfa,0xc3]);
            using var adapter=Transform80Adapter(code.Entry,windows,continuation);
            nint tail=continuation?ret.Entry:0;
            foreach(var row in Trans165IntegerRows(op))foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})
            foreach(int masks in System.Linq.Enumerable.Range(0,64))
            {
                var f=Trans157Frame(row.X,row.Y,(ushort)(0x40|pc<<8|rc<<10|masks),tail);
                var expected=Trans165IntegerOracle(op,row.X,row.Y,rc);
                Trans157Invoke(adapter.Entry,ref f);
                uint metadata=(uint)((int)op+1|0x10100|(Trans157Pair(op)?0x200:0)|
                    ((expected.Status&32)!=0&&(masks&32)==0?0x1000:0));
                Check((uint)f.Admission==0&&(uint)f.Classification==metadata&&f.NormalStatus==expected.Status&&f.WrappedStatus==expected.Status,
                    "R165 all8 exact integer kind/metadata/status; all64 masks and allPC/RC");
                Check(f.Normal.Significand==expected.Value.Significand&&f.Normal.SignExponent==expected.Value.SignExponent,
                    "R165 raw80 equals independent dyadic oracle");
                Check(Trans157Pair(op)?f.Wrapped.Significand==0x8000000000000000&&f.Wrapped.SignExponent==0x3fff:
                    f.Wrapped.Significand==0&&f.Wrapped.SignExponent==0,"R165 exact pair completion");
                Check(*(uint*)((byte*)&f+232)==(uint)((expected.Status&0x200)|((Trans157Trig(op)?0x600:0x200)<<16)),
                    "R165 exact defined C1/C2 replacement mask");
                Check(new ReadOnlySpan<byte>((byte*)&f+96,136).IndexOfAnyExcept((byte)0)<0&&
                    new ReadOnlySpan<byte>((byte*)&f+236,20).IndexOfAnyExcept((byte)0)<0,
                    "R165 whole capture/classification/arithmetic scratch is zero, including216/244/248");
            }
        }
    }

    static void Canon165IntegerComplete(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        var i=Canon157Classify([0xd9,0xf1]);
        using var code=CreateFullBlock([new(i,X87Kernel:calls[70])]);
        foreach(var row in Trans165IntegerRows(Op165.Fyl2x))foreach(int rc in new[]{0,1,2,3})foreach(int masks in new[]{0,31,32,63})
        {
            ushort cw=(ushort)(0x140|rc<<10|masks);Canon154Seed(fake,state,7,cw);
            Canon154Input(fake,state,0,row.X);Canon154Input(fake,state,1,row.Y);
            var expected=Trans165IntegerOracle(Op165.Fyl2x,row.X,row.Y,rc);
            byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
            uint sw=CanonicalX87U32(m,648),tw=CanonicalX87U32(m,652); // initial TOP7, destination physical0
            Canon154Write(raw,0,expected.Value);tw=(tw&~3u)|(uint)Canon154Tag(expected.Value);tw|=3u<<14;
            sw=(sw&~0x3a00u)|expected.Status;if((expected.Status&~cw&63)!=0)sw|=0x8080;
            CanonicalX87W32(m,648,sw);CanonicalX87W32(m,652,tw);CanonicalX87W64(m,664,i.Ip);
            CanonicalX87W32(m,656,(uint)i.Extra.Immediate);CanonicalX87W64(m,0,i.Ip+2);m[8]=0;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40),
                System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(472)));
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
            Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1,
                "R165 nontrivial integer log independent validator accepts and commits once");
            CanonicalX87Same(fake,state,m,raw);
        }
    }

    static void Canon165IntegerPoison(byte* fake,FullX87RegisterFile* state)
    {
        foreach(Op165 op in Enum.GetValues<Op165>())
        foreach((int off,uint value) in new[]{(84,0x2000u),(84,0x4000u),(84,0x1000u),(84,0x800u),(84,0x400u),(84,0x8000u),
            (80,32u),(82,32u),(232,0x200u),(234,0x4700u),(244,1u),(216,1u),(248,1u),(96,1u),(208,1u),
            (58,1u),(60,1u),(48,1u),(56,1u),(236,1u),(-1,0u),(-2,0u),(-3,0u),(-4,0u),(-5,0u),(-6,0u)})
        {
            // Start with an independently literal-valid zero cell. Off=-1 forges
            // IntegerNative for an uncovered finite .25 input, even with all
            // other response bits plausible. No producer/validator oracle reuse.
            uint metadata=(uint)((int)op+1|0x10100|(Trans157Pair(op)?0x200:0));
            var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};
            void D(int v){for(int n=0;n<4;++n)b.Add((byte)(v>>(n*8)));}
            void Put(int p,uint v){b.AddRange([0x41,0xc7,0x83]);D(p);D(unchecked((int)v));}
            for(int p=48;p<88;p+=4)Put(p,0);for(int p=96;p<256;p+=4)Put(p,0);
            if(op==Op165.Fcos){Put(52,0x80000000);Put(56,0x3fff);}
            if(Trans157Pair(op)){Put(68,0x80000000);Put(72,0x3fff);}
            Put(84,metadata);Put(232,Trans157Trig(op)?0x06000000u:0x02000000u);Put(12,0);
            if(off>=0)Put(off,off==84?metadata|value:value);
            if(off==-6)
            {
                Put(16,0);Put(20,op is Op165.Fyl2x or Op165.Fpatan?0x80000000u:0);Put(24,op is Op165.Fyl2x or Op165.Fpatan?0x3fffu:0);
                Put(32,0);Put(36,0);Put(40,0); // coherent forged domain witness
            }
            b.AddRange([0x31,0xc0,0x41,0xff,0xa3,88,0,0,0]);
            using var bad=Transform80Publish(b.ToArray());var i=Canon157Classify([0xd9,canon157Forms[(int)op]]);
            var call=new FullX87KernelCall((byte)(69+(int)op),bad.Entry,73,b.Count);using var body=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach(ushort cw in new ushort[]{0x40,0x15f,0x960,0xf7f})
            {
                Canon154Seed(fake,state,0,cw);
                Raw165 x=op is Op165.Fyl2x or Op165.Fpatan?new(0x8000000000000000,0x3fff):default;
                Raw165 y=default;
                if(off<0)
                {
                    x=off switch {-2=>new(1,0),-3=>new(0xc000000000000123,0x7fff),
                        -4=>new(0x8000000000000123,0x7fff),-5=>new(1,0x4000),
                        _=>op==Op165.Fyl2x?new(0xa000000000000000,0x3fff):new(0x8000000000000000,0x3ffd)};
                    y=new(0x8000000000000000,0x3fff);
                }
                Canon154Input(fake,state,0,x);if(Trans157Binary(op))Canon154Input(fake,state,1,y);
                if(Trans157Pair(op))Canon154Input(fake,state,7,default,3);
                byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,body.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                Check(RunFull(body,&f)==FullBlockExit.Invalidated&&issued==1&&completed==0&&f.FirstPreissued==1,
                    "R165 integer bit cannot launder forged raw/status/CC/capture/metadata or uncovered finite domain");
                CanonicalX87Same(fake,state,m,raw);
            }
        }
    }
    static void Canon165ProductPoison(byte* fake,FullX87RegisterFile* state)
    {
        var op=Op165.Fyl2x;var i=Canon157Classify([0xd9,0xf1]);
        foreach(int rc in new[]{0,1,2,3})foreach(int masks in new[]{0,31,32,63})
        foreach(int mutation in new[]{0,1,2,3,4,5,6,7,8,9})
        {
            ushort cw=(ushort)(0x140|rc<<10|masks);
            var x=new Raw165(0x8000000000000000,0x4002); // k=3
            var y=new Raw165(0xaaaaaaaaaaaaaaab,0x3fff);
            if(mutation>=8){x=new(0x8000000000000000,0x4000);y=new(0x8000000000000000,(ushort)(16383+(mutation==8?16001:-16001)));}
            var result=Trans165IntegerOracle(op,x,y,rc);
            uint metadata=0x10102u|((result.Status&32)!=0&&(cw&32)==0?0x1000u:0);
            ulong sig=result.Value.Significand;uint se=result.Value.SignExponent;
            uint status=(uint)(result.Status|(uint)result.Status<<16),cc=(uint)(0x02000000|result.Status&0x200);
            switch(mutation)
            {
                case 0:sig^=1;break;case 1:se^=1;break;case 2:se^=0x8000;break;
                case 3:status^=32;break;case 4:status^=0x2000000;break;
                case 5:metadata^=0x1000;break;case 6:cc^=0x200;break;case 7:metadata|=0x4000;break;
                // 8/9 are mathematically CORRECT finite responses with y one
                // exponent step outside the approved conservative domain.
            }
            var b=new List<byte>{0xf3,0x0f,0x1e,0xfa};
            void D(int v){for(int n=0;n<4;++n)b.Add((byte)(v>>(8*n)));}
            void Put(int p,uint v){b.AddRange([0x41,0xc7,0x83]);D(p);D(unchecked((int)v));}
            for(int p=48;p<88;p+=4)Put(p,0);for(int p=96;p<256;p+=4)Put(p,0);
            Put(48,(uint)sig);Put(52,(uint)(sig>>32));Put(56,se);Put(80,status);Put(84,metadata);Put(232,cc);Put(12,0);
            b.AddRange([0x31,0xc0,0x41,0xff,0xa3,88,0,0,0]);
            using var bad=Transform80Publish(b.ToArray());var call=new FullX87KernelCall(70,bad.Entry,73,b.Count);
            using var body=CreateFullBlock([new(i,X87Kernel:call)]);
            Canon154Seed(fake,state,7,cw);Canon154Input(fake,state,0,x);Canon154Input(fake,state,1,y);
            byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,body.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
            Check(RunFull(body,&f)==FullBlockExit.Invalidated&&issued==1&&completed==0&&f.FirstPreissued==1,
                "R165 independent product proof rejects wrong rounding/C1/PE/RC/mask/completion and truthful out-of-domain responses");
            CanonicalX87Same(fake,state,m,raw);
        }
    }

}
