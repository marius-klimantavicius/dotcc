using System;
using System.Collections.Generic;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;
using Raw165=Managed.Emulation.Execution.FullExtended80;
using Op165=Managed.Emulation.Execution.FullExtended80Transcendental;
unsafe partial class Program
{
    static void Canon171RetainedProductPoison(byte* fake,FullX87RegisterFile* state)
    {
        var op=Op165.Fyl2x;var i=Canon157Classify([0xd9,0xf1]);
        foreach(int rc in new[]{0,1,2,3})foreach(int masks in new[]{0,31,32,63})
        foreach(int mutation in new[]{0,1,2,3,4,5,6,7})
        {
            ushort cw=(ushort)(0x140|rc<<10|masks);
            var x=new Raw165(0x8000000000000000,0x4002); // k=3
            var y=new Raw165(0xaaaaaaaaaaaaaaab,0x3fff);
            var result=Trans165IntegerOracle(op,x,y,rc);
            uint metadata=0x10102u|((result.Status&32)!=0&&(cw&32)==0?0x1000u:0);
            ulong sig=result.Value.Significand;uint se=result.Value.SignExponent;
            uint status=(uint)(result.Status|(uint)result.Status<<16),cc=(uint)(0x02000000|result.Status&0x200);
            switch(mutation)
            {
                case 0:sig^=1;break;case 1:se^=1;break;case 2:se^=0x8000;break;
                case 3:status^=32;break;case 4:status^=0x2000000;break;
                case 5:metadata^=0x1000;break;case 6:cc^=0x200;break;case 7:metadata|=0x4000;break;
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
                "R171 retains R165 independent product poison0..7 on the exact covered anchor");
            CanonicalX87Same(fake,state,m,raw);
        }
    }
}

unsafe partial class Program
{
    static void Canon171ResponsePoison(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        _=calls;
        foreach(Op165 op in Enum.GetValues<Op165>())
        {
            var row=Array.Find(trans168Vectors,v=>v.Op==(int)op&&v.X.Significand==0xa000000000000001UL&&
                v.X.SignExponent==(op==Op165.Fyl2x?0x3fff:0x3ffd));
            Check(Trans168Certify(row.Low,row.High,row.Exponent,0,63,!row.Exact,out var a),"R171 independent poison base normal value");
            var second=default(Trans168Rounded);
            if(Trans157Pair(op))Check(Trans168Certify(row.SecondLow,row.SecondHigh,row.SecondExponent,0,63,op!=Op165.Fptan,out second),"R171 independent poison base second value");
            uint status=(uint)(op==Op165.Fsincos?a.Status|(second.Status&~0x200):a.Status);
            uint meta=(uint)((int)op+1|0x10100|(Trans157Pair(op)?0x200:0));
            foreach(int mutation in System.Linq.Enumerable.Range(0,33))
            {
                var bytes=new List<byte>{0xf3,0x0f,0x1e,0xfa};
                void D(uint n){for(int j=0;j<4;++j)bytes.Add((byte)(n>>(j*8)));}
                void Put(int off,uint v){bytes.AddRange([0x41,0xc7,0x83]);D((uint)off);D(v);}
                void Q(int off,ulong v){Put(off,(uint)v);Put(off+4,(uint)(v>>32));}
                for(int p=48;p<88;p+=4)Put(p,0);for(int p=96;p<256;p+=4)Put(p,0);
                Q(48,a.Sig);Put(56,a.Se);Q(64,second.Sig);Put(72,second.Se);
                Put(80,status|status<<16);Put(84,meta);Put(232,(Trans157Trig(op)?0x06000000u:0x02000000u)|(status&0x200));Put(12,0);
                switch(mutation)
                {
                    case 0:Put(84,meta|0x2000);break;case 1:Put(84,meta|0x4000);break;
                    case 2:Put(84,meta|0x1000);break;case 3:Put(84,meta|0x400);break;
                    case 4:Put(84,meta|0x800);break;case 5:Put(84,meta|0x8000);break;
                    case 6:Put(84,meta^0x100);break;case 7:Put(84,meta^1);break;
                    case 8:Put(80,status&~32u|status<<16);break;
                    case 9:Put(80,status|(status&~32u)<<16);break;
                    case 10:Put(80,(status|1)|(status|1)<<16);break;
                    case 11:Put(80,(status|2)|(status|2)<<16);break;
                    case 12:Put(80,(status|4)|(status|4)<<16);break;
                    case 13:Put(232,(Trans157Trig(op)?0x06000000u:0x02000000u)|((status^0x200)&0x200));break;
                    case 14:Put(234,0);break;case 15:Put(96,1);break;case 16:Put(208,1);break;
                    case 17:Put(236,1);break;case 18:Put(244,1);break;case 19:Put(248,1);break;
                    case 20:Put(58,1);break;case 21:Put(60,1);break;
                    case 22:Put(26,1);break;case 23:Put(28,1);break;
                    case 24:Put(42,1);break;case 25:Put(44,1);break;
                    case 26:Put(16,(uint)row.X.Significand^1);break; // rewritten raw witness
                    case 27:Put(8,0x37e);break;case 28:Put(0,2);break;case 29:Put(4,383);break;
                    case 30:Put(56,0x7fff);break; // ordinary finite cannot return NaN/infinity
                    case 31:Put(64,1);Put(68,0);Put(72,0x3fff);break; // wrong pair or unused destination
                    case 32:Put(12,1);break; // actual protocol violation, never an ordinary refusal
                }
                bytes.AddRange([0x31,0xc0,0x41,0xff,0xa3,88,0,0,0]);
                using var adversary=Transform80Publish(bytes.ToArray());var i=Canon157Classify([0xd9,canon157Forms[(int)op]]);
                var call=new FullX87KernelCall((byte)(69+(int)op),adversary.Entry,73,bytes.Count);using var body=CreateFullBlock([new(i,X87Kernel:call)]);
                Canon154Seed(fake,state,5,0x37f);Canon154Input(fake,state,0,row.X);
                if(Trans157Binary(op))Canon154Input(fake,state,1,row.Y);if(Trans157Pair(op))Canon154Input(fake,state,7,default,3);
                byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,body.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                Check(RunFull(body,&f)==FullBlockExit.Invalidated&&issued==1&&completed==0&&f.FirstPreissued==1,
                    $"R171 malformed ordinary response preserves SAME issue k={op} poison={mutation}");
                CanonicalX87Same(fake,state,m,raw);
            }
        }
    }
}
