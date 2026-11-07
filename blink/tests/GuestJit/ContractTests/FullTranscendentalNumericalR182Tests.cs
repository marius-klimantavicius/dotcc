using System;
using System.Numerics;
using System.Buffers.Binary;
using Managed.Emulation.Execution;

unsafe partial class Program
{
    readonly record struct Trans182Endpoint(string Wide,int WideExponent,string Low,string High,int Exponent);
    readonly record struct Trans182Vector(int Op,FullExtended80 X,FullExtended80 Y,
        Trans182Endpoint First,Trans182Endpoint Second);
    static void Trans182RecordImage(FullExtended80Transcendental op,bool windows,bool tail,ReadOnlySpan<byte> bytes)
    {
        // Optional evidence capture during the parent's EXISTING run. Does not
        // invoke an emitter, native entry or extra test queue. Directory is set
        // explicitly by the parent for each runtime; unset means no file writes.
        string? directory=Environment.GetEnvironmentVariable("BLINK_R182_IMAGES");
        if(string.IsNullOrEmpty(directory))return;
        string form=tail?"r11-tail":windows?"windows-ret":"sysv-ret";
        string path=System.IO.Path.Combine(directory,$"{69+(int)op}-{form}.bin");
        System.IO.Directory.CreateDirectory(directory);
        byte[] body=bytes.ToArray();
        if(System.IO.File.Exists(path))Check(System.IO.File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes),"R182 immutable same-key native image equality");
        else System.IO.File.WriteAllBytes(path,body);
        Console.WriteLine($"R182-IMAGE key={69+(int)op} entry={form} bytes={bytes.Length} sha256={Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant()} path={path}");
    }
    static void Trans182NumericalClosure(nint entry,FullExtended80Transcendental op,nint continuation)
    {
        int rows=0,calls=0;
        foreach(var v in trans182Vectors)
        {
            if(v.Op!=(int)op)continue;
            ++rows;
            foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})
            for(int masks=0;masks<64;++masks)
            {
                ushort cw=(ushort)(0x40|pc<<8|rc<<10|masks);
                var f=Trans157Frame(v.X,v.Y,cw,continuation);Trans157Invoke(entry,ref f);
                ++calls;
                int de=Trans168DE(v.X)|(Trans157Binary(op)?Trans168DE(v.Y):0);
                bool suppressed=(de&~masks)!=0;
                Check(f.Admission==0&&f.ControlWord==cw,"R182 ordinary cell all PC/RC/masks admission0, original CW");
                if(suppressed)
                {
                    Check(f.WrappedStatus==2&&(uint)f.Classification==(uint)((int)op+1|0x10400),"R182 unmasked DE precedes numerical completion");
                    Check(f.Normal.Significand==0&&f.Normal.SignExponent==0&&f.Wrapped.Significand==0&&f.Wrapped.SignExponent==0,"R182 both destinations suppressed");
                }
                else
                {
                    // Two independent obligations: exact native correspondence
                    // to the source candidate AND containment near an oracle
                    // that uses different transcendental algorithms.
                    bool pair=Trans157Pair(op);
                    var actual=Trans168Round(BigInteger.Parse(v.First.Wide),v.First.WideExponent,rc,masks,true);
                    var diagnostic=Trans168Round(BigInteger.Parse(v.First.Wide),v.First.WideExponent,rc,63,true);
                    int status=actual.Status|de,normalStatus=diagnostic.Status|de;
                    if(f.Normal.Significand!=actual.Sig||f.Normal.SignExponent!=actual.Se)
                        throw new InvalidOperationException($"R182 raw mismatch op={op} x={v.X.Significand:x16}:{v.X.SignExponent:x4} y={v.Y.Significand:x16}:{v.Y.SignExponent:x4} cw={cw:x4} actual={f.Normal.Significand:x16}:{f.Normal.SignExponent:x4} expected={actual.Sig:x16}:{actual.Se:x4} status={f.NormalStatus:x4}/{f.WrappedStatus:x4} row={rows}");
                    Trans168Raw(f.Normal,actual,"R182 actual unbounded candidate, exact RC/range/wrap");
                    Trans182Interval(v.First,op);
                    if(pair)
                    {
                        bool approximate=op!=FullExtended80Transcendental.Fptan;
                        var b=Trans168Round(BigInteger.Parse(v.Second.Wide),v.Second.WideExponent,rc,masks,approximate);
                        var bm=Trans168Round(BigInteger.Parse(v.Second.Wide),v.Second.WideExponent,rc,63,approximate);
                        Trans168Raw(f.Wrapped,b,"R182 paired cosine/one rounded independently");
                        status|=b.Status&~0x200;normalStatus|=bm.Status&~0x200;
                        Trans182Interval(v.Second,op);
                    }
                    else Check(f.Wrapped.Significand==0&&f.Wrapped.SignExponent==0,"R182 unary/binary output shape");
                    Check(f.NormalStatus==normalStatus&&f.WrappedStatus==status,"R182 exact diagnostic/actual PE/C1/OE/UE, sine C1 on FSINCOS");
                    uint meta=(uint)((int)op+1|0x10100|(pair?0x200:0)|((status&32)!=0&&(masks&32)==0?0x1000:0));
                    Check((uint)f.Classification==meta,"R182 typed metadata exact equality and precision completion");
                }
                byte* p=(byte*)&f;
                uint cc=BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(p+232,4));
                Check(cc==(uint)(suppressed?0x02000000:((Trans157Trig(op)?0x600:0x200)<<16)|(f.WrappedStatus&0x200)),"R182 defined condition mask");
                Check(new ReadOnlySpan<byte>(p+96,136).IndexOfAnyExcept((byte)0)<0&&new ReadOnlySpan<byte>(p+236,20).IndexOfAnyExcept((byte)0)<0,"R182 scratch scrub before typed precommit");
            }
        }
        if(op==FullExtended80Transcendental.Fyl2x)Trans182DyadicEdges(entry,continuation);
        Console.WriteLine($"R182-NUMERICAL op={(int)op} rows={rows} calls={calls} pc=4 rc=4 masks=64 dyadicEdges={(op==FullExtended80Transcendental.Fyl2x?8:0)} PASS");
    }
    static void Trans182Interval(Trans182Endpoint v,FullExtended80Transcendental op)
    {
        BigInteger n=BigInteger.Parse(v.Wide),lo=BigInteger.Parse(v.Low),hi=BigInteger.Parse(v.High);
        int bits=op==FullExtended80Transcendental.Fptan?90:Trans157Trig(op)?180:230;
        BigInteger m=BigInteger.Max(BigInteger.Abs(lo),BigInteger.Abs(hi));
        int scale=m.IsZero?0:checked((int)m.GetBitLength()-1+v.Exponent);
        if(op is FullExtended80Transcendental.Fsin or FullExtended80Transcendental.Fcos or FullExtended80Transcendental.Fsincos)scale=Math.Max(scale,0);
        // Using the next higher power of two only widens the explicit bound.
        int allowance=scale+1-bits,unit=Math.Min(Math.Min(v.WideExponent,v.Exponent),allowance);
        n<<=v.WideExponent-unit;lo<<=v.Exponent-unit;hi<<=v.Exponent-unit;
        BigInteger error=BigInteger.One<<(allowance-unit);
        Check(lo-error<=n&&n<=hi+error,"R182 independently enclosed mathematical target, no skipped interval/midpoint cell");
    }
    static void Trans182DyadicEdges(nint entry,nint continuation)
    {
        // Independent exact k*significand*2^unit, including y beyond R165's
        // [-16000,16000] bound, pseudo-denormals and <=79-bit inexact products.
        var rows=new (FullExtended80 X,FullExtended80 Y)[]{
            (new(0x8000000000000000,0x0001),new(0xffffffffffffffff,0x7ffe)),
            (new(0x8000000000000000,0x7ffe),new(0xffffffffffffffff,0xfffe)),
            (new(0x8000000000000000,0x3ffe),new(1,0)),
            (new(0x8000000000000000,0x4001),new(0x8000000000000000,0x7ffe)),
            (new(0x8000000000000000,0x3fff),new(0xffffffffffffffff,0xfffe)),
            (new(1,0),new(0xffffffffffffffff,0x0001)),
            (new(0x8000000000000000,0),new(0x8000000000000001,0x8000)),
            (new(0x8000000000000000,0x4002),new(0x8000000000000001,0x0001)),
        };
        foreach(var r in rows)foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})for(int masks=0;masks<64;++masks)
        {
            int xe=(r.X.SignExponent&0x7fff)==0?-16382:(r.X.SignExponent&0x7fff)-16383;
            int k=xe+checked((int)new BigInteger(r.X.Significand).GetBitLength())-64;
            int ye=(r.Y.SignExponent&0x7fff)==0?-16382:(r.Y.SignExponent&0x7fff)-16383;
            BigInteger n=new BigInteger(r.Y.Significand)*k;
            if((r.Y.SignExponent&0x8000)!=0)n=-n;
            var a=Trans168Round(n,ye-63,rc,masks,false);var d=Trans168Round(n,ye-63,rc,63,false);
            if(n.IsZero){a=a with {Se=(ushort)(r.Y.SignExponent&0x8000)};d=a;}
            int de=Trans168DE(r.X)|Trans168DE(r.Y);
            var f=Trans157Frame(r.X,r.Y,(ushort)(0x40|pc<<8|rc<<10|masks),continuation);Trans157Invoke(entry,ref f);
            Check(f.Admission==0,"R182 exact full-domain dyadic admission0");
            if((de&~masks)!=0){Check(f.WrappedStatus==2,"R182 dyadic DE precedence");continue;}
            Trans168Raw(f.Normal,a,"R182 independent full-domain dyadic exact RC/wrapped value");
            Check(f.WrappedStatus==(a.Status|de)&&f.NormalStatus==(d.Status|de),"R182 dyadic PE iff discarded bits/saturation; C1 iff increment");
        }
    }
}
