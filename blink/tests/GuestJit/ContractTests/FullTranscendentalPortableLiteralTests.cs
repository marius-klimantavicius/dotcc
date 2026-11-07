using System;
using System.Collections.Generic;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;
using Raw171=Managed.Emulation.Execution.FullExtended80;
using Op171=Managed.Emulation.Execution.FullExtended80Transcendental;

unsafe partial class Program
{
    readonly record struct Trans171Special(Raw171 X,Raw171 Y,Raw171 Result,ushort Pre,bool Range=false,bool Undefined=false);
    static IEnumerable<Trans171Special> Trans171SpecialRows(Op171 op)
    {
        var one=new Raw171(0x8000000000000000,0x3fff);var inf=new Raw171(0x8000000000000000,0x7fff);
        var indefinite=new Raw171(0xc000000000000000,0xffff);
        yield return new(new(1,0x4000),one,indefinite,1); // unsupported, not subnormal
        yield return new(new(0x8000000000000123,0x7fff),one,new(0xc000000000000123,0x7fff),1);
        yield return new(new(0xc000000000000123,0xffff),new(1,0),new(0xc000000000000123,0xffff),0); // NaN suppresses DE
        if(Trans157Binary(op))
        {
            yield return new(new(0xc000000000000123,0xffff),new(0x8000000000000456,0x7fff),new(0xc000000000000123,0xffff),1);
            yield return new(new(0x8000000000000456,0x7fff),new(0xc000000000000123,0xffff),new(0xc000000000000123,0xffff),1);
            yield return new(new(0xc000000000000123,0xffff),new(0xc000000000000456,0x7fff),new(0xc000000000000456,0x7fff),0);
            yield return new(new(0xc000000000000123,0xffff),new(0xc000000000000123,0x7fff),new(0xc000000000000123,0xffff),0);
            yield return new(new(1,0),new(0xc000000000000456,0x7fff),new(0xc000000000000456,0x7fff),0);
            yield return new(new(0xc000000000000123,0xffff),new(1,0x4000),indefinite,1);
        }
        if(Trans157Trig(op))
        {
            yield return new(inf,one,indefinite,1);
            yield return new(new(0x8000000000000000,0x403e),one,default,0,Range:true);
            yield return new(new(ulong.MaxValue,0xfffe),one,default,0,Range:true);
        }
        if(op==Op171.F2xm1)
        {
            yield return new(inf,one,inf,0,Undefined:true);
            yield return new(new(0x8000000000000001,0xbfff),one,new(0x8000000000000001,0xbfff),0,Undefined:true);
        }
        if(op==Op171.Fyl2xp1)
        {
            yield return new(new(0x95f619980c4336f8,0xbffd),new(1,0),new(1,0),0,Undefined:true);
            yield return new(new(0xd413cccfe7799212,0x3ffd),inf,inf,0,Undefined:true);
            yield return new(new(0,0x8000),one,new(0,0x8000),0);
            yield return new(default,inf,indefinite,1);
        }
        if(op==Op171.Fyl2x)
        {
            yield return new(new(0x8000000000000000,0xbfff),new(1,0),indefinite,1);
            yield return new(default,default,indefinite,1);
            yield return new(default,one,new(0x8000000000000000,0xffff),4);
            yield return new(default,new(1,0),new(0x8000000000000000,0xffff),6);
            yield return new(inf,default,indefinite,1);
            yield return new(one,inf,indefinite,1);
            yield return new(new(0x8000000000000000,0x4000),inf,inf,0);
        }
        if(op==Op171.Fpatan)
        {
            yield return new(one,new(0,0x8000),new(0,0x8000),0);
            yield return new(inf,new(1,0),default,2);
        }
    }
    static void Canon171LiteralSpecials(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach(Op171 op in Enum.GetValues<Op171>())
        {
            var i=Canon157Classify([0xd9,canon157Forms[(int)op]]);using var code=CreateFullBlock([new(i,X87Kernel:calls[69+(int)op])]);
            foreach(var row in Trans171SpecialRows(op))foreach(int rc in new[]{0,1,2,3})foreach(int masks in System.Linq.Enumerable.Range(0,64))
            {
                ushort cw=(ushort)(0x340|rc<<10|masks);int unmasked=row.Pre&~masks&7;bool suppress=unmasked!=0;
                ushort actual=suppress?(ushort)(unmasked&-unmasked):row.Pre;
                var a=new Trans168Rounded(row.Result.Significand,row.Result.SignExponent,actual);
                bool nan=(row.Result.SignExponent&0x7fff)==0x7fff&&row.Result.Significand!=0x8000000000000000UL;
                var b=op==Op171.Fsincos&&nan?a:new Trans168Rounded(0x8000000000000000,0x3fff,0);
                Canon154Seed(fake,state,7,cw);Canon154Input(fake,state,0,row.X);if(Trans157Binary(op))Canon154Input(fake,state,1,row.Y);
                if(Trans157Pair(op))Canon154Input(fake,state,7,default,3);
                byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                Canon171Expected(i,op,a,b,actual,suppress,row.Range,row.Undefined,m,raw);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1,"R171 literal NaN/domain/pre-event/range/undefined completion");
                CanonicalX87Same(fake,state,m,raw);
                byte* work=(byte*)state+192;
                Check(((ushort*)(work+80))[0]==row.Pre&&((ushort*)(work+82))[0]==actual,"R171 literal diagnostic versus selected pre-event priority");
            }
        }
        Canon171PiAxes(fake,state,calls);
    }
    static void Canon171PiAxes(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        var i=Canon157Classify([0xd9,0xf3]);using var code=CreateFullBlock([new(i,X87Kernel:calls[72])]);
        foreach(int shape in new[]{0,1,2,3})foreach(bool negative in new[]{false,true})foreach(int rc in new[]{0,1,2,3})foreach(int masks in new[]{0,31,32,63})
        {
            var x=shape switch{0=>new Raw171(0x8000000000000000,0xbfff),1=>default,2=>new(0x8000000000000000,0x7fff),_=>new(0x8000000000000000,0xffff)};
            var y=shape==0?new Raw171(0,(ushort)(negative?0x8000:0)):new Raw171(0x8000000000000000,(ushort)((shape>=2?0x7fff:0x3fff)|(negative?0x8000:0)));
            ulong floor=shape==3?0x96cbe3f9990e91a7UL:0xc90fdaa22168c234UL;
            int e=shape switch{0=>0x4000,1=>0x3fff,2=>0x3ffe,_=>0x4000};
            bool increment=rc==0||rc==1&&negative||rc==2&&!negative;
            ushort status=(ushort)(increment?0x220:0x20);
            var a=new Trans168Rounded(floor+(increment?1UL:0),(ushort)(e|(negative?0x8000:0)),status);
            Canon154Seed(fake,state,7,(ushort)(0x140|rc<<10|masks));Canon154Input(fake,state,0,x);Canon154Input(fake,state,1,y);
            byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
            Canon171Expected(i,Op171.Fpatan,a,default,status,false,false,false,m,raw);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
            Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1,"R171 independently literal pi axes/infinite quadrants allRC/masks");
            CanonicalX87Same(fake,state,m,raw);
        }
    }
}
