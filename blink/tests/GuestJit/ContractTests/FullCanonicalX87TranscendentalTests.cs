using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;
using Raw157=Managed.Emulation.Execution.FullExtended80;

unsafe partial class Program
{
    // The ONE entry for the whole8-form family. Parent alone adopts/wires this
    // after one catalog77 batch. AUTHORED_UNRUN; no production activation here.
    static void CheckFullCanonicalX87Transcendentals(byte* machine, bool integerOnly = false)
    {
        _=machine;Canon157Grammar();if(!integerOnly)Trans157ComputeSuite();Trans165IntegerNativeSuite();
        Check(!FullExtended80TranscendentalProtocol.HostBackendActivationReady,"R157 historical private-host backend remains unqualified");
        Check(FullX87KernelCatalog.MaximumKernels>=77&&sizeof(FullExtended80WorkFrame)==384&&
            sizeof(FullX87NativeStorage)==576&&sizeof(FullBlockFrame)==416,"R157 parent77 catalog, unchanged ABI7");
        using var io=new SemanticTestIo();using var authority=new FullCodeAuthority(io.Value,4096,2,4L<<20);
        using var store=new RetainedTranslationStore(RetainedOptions() with {PayloadBytes=0});
        using var catalog=new FullX87KernelCatalog(authority,store,true);
        var calls=new FullX87KernelCall[FullX87KernelCatalog.MaximumKernels];
        lock(authority.Gate)
        {
            Check(catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes],out _)&&catalog.Count==FullX87KernelCatalog.MaximumKernels,
                "R157 ONE real eager complete catalog");
            for(int k=0;k<calls.Length;++k)
            {var b=catalog.Find((FullX87KernelKey)k);Check(catalog.IsValid(in b),"R157 exact authority binding");calls[k]=new((byte)k,b.Entry,73,b.CodeBytes);}
        }
        var token=RetainedPublish(store,0x570000);Check(store.TryAcquireToken(token,out var root),"R157 actual root lease");
        Check(store.TryBeginNativeEpoch(out var epoch),"R157 native epoch");
        bool enrolled;lock(authority.Gate)enrolled=catalog.AdmissionOpen&&authority.TryEnterNative(authority.Epoch);
        Check(enrolled,"R157 SAME gate enrollment held through continuation/RET");
        byte* fake=(byte*)NativeMemory.AlignedAlloc((nuint)((sizeof(Blink.Machine)+15)&~15),16);
        var state=(FullX87RegisterFile*)NativeMemory.AlignedAlloc(576,16);
        try
        {
            Check(fake!=null&&state!=null&&!fullOwnerScaffold.IsValid,"R157 isolated canonical storage");
            if(!integerOnly)
            { Canon157Completion(fake,state,calls);Canon157Boundaries(fake,state,calls);Canon157Deferred(fake,state,calls);Canon157Poison(fake,state); }
            Canon165IntegerComplete(fake,state,calls);Canon165IntegerPoison(fake,state);Canon165ProductPoison(fake,state);
            using(var common=CreateFullScaffold(out var addresses))
            {fullOwnerScaffold=addresses;try{if(!integerOnly)Canon157Completion(fake,state,calls);Canon165IntegerComplete(fake,state,calls);}finally{fullOwnerScaffold=default;}}
            lock(authority.Gate)catalog.CloseAdmission();
        }
        catch(Exception error)
        { Console.Error.WriteLine("PRIMARY R157 family failure: "+error); throw; }
        finally
        {
            NativeMemory.AlignedFree(state);NativeMemory.AlignedFree(fake);
            if(enrolled)authority.ExitNative();epoch.Dispose();root.Dispose();
            store.Dispose();catalog.Dispose();
        }
        Check(catalog.ChargedBytes==0,"R157 store barrier before final native unmap");
    }
    static readonly byte[] canon157Forms=[0xf0,0xf1,0xf2,0xf3,0xf9,0xfb,0xfe,0xff];
    static JitInstruction Canon157Classify(ReadOnlySpan<byte> bytes,ulong ip=Ip)
    {
        Check(FullCanonicalX87TranscendentalLowering.TryClassify(ip,bytes,out var i,out var why),why??"R157 documented form");
        Check(FullBlockEmitter.ValidateCanonicalX87Transcendental(i),"R157 strict typed family IR");return i;
    }
    static void Canon157Grammar()
    {
        for(int k=0;k<8;++k)
        foreach(byte[] prefix in new byte[][]{[],[0x66],[0x67,0x64,0x4f],[0x4f,0x66,0x40],
            [0x26,0x2e,0x36,0x3e,0x64,0x65,0x66,0x67,0x40,0x4f,0x66,0x67,0x40]})
        {
            byte[] b=new byte[prefix.Length+2];prefix.CopyTo(b,0);b[^2]=0xd9;b[^1]=canon157Forms[k];
            var i=Canon157Classify(b);bool binary=k is 1 or 3 or 4;
            Check(i.Options==0xa001u+(uint)k&&i.Source==new JitOperand(binary?1:0,0)&&i.Extra.Immediate==(ulong)(0x100|b[^1])&&
                (byte)FullBlockEmitter.CanonicalX87TranscendentalKernel(i)==69+k&&!FullBlockEmitter.CanonicalX87TranscendentalRequiresArithmeticPrecision(i),
                "R157 all8 exact IR/FOP/source/key, PC01 admissible");
            foreach(var bad in new[]{i with {Width=64},i with {Options=i.Options|0x10000},i with {Length=1},
                i with {Source=new(7,0)},i with {Destination=new(1,0)},i with {Extra=default},i with {SourceWidth=80},
                i with {Target=1},i with {Condition=1},i with {Address=new(1,2,0,0)}})
            {bool reject=false;try{FullBlockEmitter.ValidateCanonicalX87Transcendental(bad);}catch(ArgumentException){reject=true;}Check(reject,"R157 malformed recognized IR refuses");}
            var call=new FullX87KernelCall((byte)(69+k),(nint)0x1000,73,32);
            Check(FullBlockEmitter.ValidateCanonicalX87Transcendental(i,in call),"R157 typed value tuple");
            foreach(var bad in new[]{call with {Entry=0},call with {OwnerCookie=0},call with {Key=(byte)(call.Key^1)},call with {CodeBytes=0}})
            {bool reject=false;try{FullBlockEmitter.ValidateCanonicalX87Transcendental(i,in bad);}catch(ArgumentException){reject=true;}Check(reject,"R157 malformed native tuple refuses");}
        }
        foreach(byte[] bad in new byte[][]{[],[0x9b],[0xf0,0xd9,0xf0],[0xf2,0xd9,0xfe],[0xf3,0xd9,0xff],[0xd9,0xfa],[0xd9,0xf0,0],[0xd8,0xf0]})
            Check(!FullCanonicalX87TranscendentalLowering.TryClassify(Ip,bad,out _,out _),"R157 exact documented domain/LOCK/REP/WAIT policy");
    }
    static void Canon157Completion(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        for(int k=0;k<8;++k)
        {
            var i=Canon157Classify([0xd9,canon157Forms[k]]);var call=calls[69+k];
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            foreach(int top in new[]{0,1,2,3,4,5,6,7})foreach(int pc in new[]{0,1,2,3})foreach(int rc in new[]{0,1,2,3})
            foreach(bool masked in new[]{false,true})foreach(string mode in new[]{"zero+","zero-","empty0","empty1","full","both","range","range-full","snan","denormal","zero-divide"})
            {
                bool binary=k is 1 or 3 or 4,pair=k is 2 or 5,trig=k is 2 or 5 or 6 or 7;
                if(mode=="empty1"&&!binary||(mode is "full" or "both" or "range-full")&&!pair||mode.StartsWith("range",StringComparison.Ordinal)&&!trig||mode=="zero-divide"&&k!=1)continue;
                ushort cw=(ushort)(0x40|pc<<8|rc<<10|(masked?63:0));
                Canon154Seed(fake,state,top,cw);
                Raw157 one=new(0x8000000000000000,0x3fff),zero=new(0,mode=="zero-"?(ushort)0x8000:(ushort)0);
                Raw157 x=k is 1 or 3?one:zero,y=k is 1 or 3?zero:one;
                if(mode.StartsWith("range",StringComparison.Ordinal))x=new(0x8000000000000000,0x403e);
                if(mode=="snan")x=new(0x8000000000000123,0x7fff);
                if(mode=="denormal")x=new(1,0);
                if(mode=="zero-divide"){x=default;y=one;}
                Canon154Input(fake,state,0,x);if(binary)Canon154Input(fake,state,1,y);
                if(pair&&mode is not ("full" or "both" or "range-full"))Canon154Input(fake,state,7,default,3);
                if(mode is "empty0" or "both")Canon154Input(fake,state,0,default,3);
                if(mode=="empty1")Canon154Input(fake,state,1,default,3);
                // Generic masked NaN/DE/ZE values are covered numerically. This
                // whole-image literal oracle targets exact zero/range/stack or
                // unmasked pre-operation suppression, without a host oracle.
                if(masked&&(mode is "snan" or "denormal" or "zero-divide"))continue;
                byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                Canon157Expected(i,k,mode,masked,m,raw);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                try
                {
                Check(FloatRunProbe(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1&&
                    f.HelperIndex==FullBlockFrame.NoHelperIndex&&f.MemoryAssists==0&&f.HostMxcsrSaved==0,"R157 one native family issue/completion, no helper");
                CanonicalX87Same(fake,state,m,raw);Check(*(uint*)((byte*)state+192+280)==0,"R157 valid family response never protocol-fatal");
                }
                catch(Exception error)
                { Console.Error.WriteLine($"R157 completion k={k} top={top} pc={pc} rc={rc} masked={masked} mode={mode} fatal={*(uint*)((byte*)state+192+280):x} workStatus={*(uint*)((byte*)state+192+80):x}: {error}"); throw; }
            }
        }
    }
    static void Canon157Expected(JitInstruction i,int k,string mode,bool masked,byte[] m,byte[] raw)
    {
        uint sw=CanonicalX87U32(m,648),tw=CanonicalX87U32(m,652);ushort cw=(ushort)CanonicalX87U32(m,672);
        int top=(int)(sw>>11)&7;bool binary=k is 1 or 3 or 4,pair=k is 2 or 5,trig=k is 2 or 5 or 6 or 7;
        void Write(int physical,Raw157 v){Canon154Write(raw,physical,v);tw=(tw&~(3u<<(2*physical)))|(uint)Canon154Tag(v)<<(2*physical);}
        void Pop(){tw|=3u<<(2*top);top=(top+1)&7;}
        bool stack=mode is "empty0" or "empty1" or "full" or "both";
        uint delta=stack?(mode=="full"?0x241u:0x41u):mode=="snan"?1u:mode=="denormal"?2u:mode=="zero-divide"?4u:0;
        if(mode.StartsWith("range",StringComparison.Ordinal))sw|=0x400;
        else
        {
            sw&=~0x200u;if(trig&&!stack&&delta==0)sw&=~0x400u;
            sw|=delta;if((delta&~cw&63)!=0)sw|=0x8080;
            bool suppress=(delta&~cw&7)!=0;
            if(!suppress)
            {
                Raw157 result=stack?new(0xc000000000000000,0xffff):new(0,mode=="zero-"?(ushort)0x8000:(ushort)0);
                if(k==7&&!stack)result=new(0x8000000000000000,0x3fff);
                Write(binary?(top+1)&7:top,result);
                if(binary)Pop();
                else if(pair){top=(top+7)&7;Write(top,stack?new(0xc000000000000000,0xffff):new(0x8000000000000000,0x3fff));}
            }
        }
        sw=(sw&~0x3800u)|(uint)top<<11;CanonicalX87W32(m,648,sw);CanonicalX87W32(m,652,tw);
        CanonicalX87W64(m,664,i.Ip);CanonicalX87W32(m,656,(uint)i.Extra.Immediate);
        BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40),BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(472)));
        CanonicalX87W64(m,0,i.Ip+(uint)i.Length);m[8]=0;
    }
    static void Canon157Boundaries(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        for(int k=0;k<8;++k)
        {
            var i=Canon157Classify([0xd9,canon157Forms[k]]);using var code=CreateFullBlock([new(i,X87Kernel:calls[69+k])]);
            foreach(string boundary in new[]{"stop","attention","budget","epoch","null","null-header","inactive","owner","pc01",
                "mf1","mf2","mf4","mf8","mf16","mf32"})
            {
                Canon154Seed(fake,state,3,0x37f);Canon154Input(fake,state,0,new(0x8000000000000000,0x3ffd));
                if(k is 1 or 3 or 4)Canon154Input(fake,state,1,new(0x8000000000000000,0x3fff));
                if(k is 2 or 5)Canon154Input(fake,state,7,default,3);
                bool pending=boundary.StartsWith("mf",StringComparison.Ordinal);
                if(pending){uint bit=uint.Parse(boundary.AsSpan(2));((Blink.Machine*)fake)->fpu.sw|=bit;((Blink.Machine*)fake)->fpu.cw&=~bit;Canon154Input(fake,state,0,default,3);}
                if(boundary=="attention")((Blink.Machine*)fake)->attention=true;
                if(boundary=="inactive")state->ProfileActive=0;
                if(boundary=="pc01")((Blink.Machine*)fake)->fpu.cw=0x17f;
                ulong epoch=11;long issued=0,completed=0;int stop=boundary=="stop"?1:0;var h=FullHeader(i.Ip,code.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:boundary=="budget"?0UL:1UL,limit:1);
                f.X87=boundary=="null"?null:state;if(boundary=="null-header")f.Header=null;
                if(boundary=="owner"){h.OwnerCookie=f.OwnerCookie=74;state->OwnerToken=74;}
                if(boundary=="epoch")++epoch;
                byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                byte[] work=CanonicalX87Bytes((byte*)state+192,384);
                var want=boundary switch {"stop"=>FullBlockExit.Stop,"attention"=>FullBlockExit.Attention,"budget"=>FullBlockExit.Budget,
                    "pc01"=>FullBlockExit.Continue,_=>pending?FullBlockExit.GuestFault:FullBlockExit.Invalidated};
                Check(FloatRunProbe(code,&f)==want&&completed==(boundary=="pc01"?1:0)&&issued==(pending||boundary is "pc01" or "budget"?1:0),
                    $"R157 boundary k={k} kind={boundary} actual={f.Exit} expected={want} issued={issued} completed={completed}; guards precede effects, PC01 admitted");
                if(boundary!="pc01")
                {CanonicalX87Same(fake,state,m,raw);Check(new ReadOnlySpan<byte>((byte*)state+192,384).SequenceEqual(work),"R157 denied/pending state and Work unchanged");}
                if(pending)Check(f.FaultTrap==16&&f.FaultCode==0&&f.FaultAddress==i.Ip,"R157 pendingMF outranks simultaneous empty input");
            }
        }
    }
    static void Canon157Deferred(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        for(int k=0;k<8;++k)
        {
            var i=Canon157Classify([0xd9,canon157Forms[k]]);var wait=CanonicalX87Classify([0x9b]) with {Ip=i.Ip+2};
            using var code=CreateFullBlock([new(i,X87Kernel:calls[69+k]),new(wait)]);
            Canon154Seed(fake,state,7,0x37d);Canon154Input(fake,state,0,new(1,0));
            if(k is 1 or 3 or 4)Canon154Input(fake,state,1,new(0x8000000000000000,0x3fff));
            if(k is 2 or 5)Canon154Input(fake,state,7,default,3);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:2);f.X87=state;
            Check(FloatRunProbe(code,&f)==FullBlockExit.GuestFault&&issued==2&&completed==1&&f.Executed==1&&
                f.FaultTrap==16&&f.FaultAddress==wait.Ip&&((ulong*)fake)[0]==wait.Ip,"R157 complete preexception status then next waiting MF, no second completion");
        }
    }
    static void Canon157Poison(byte* fake,FullX87RegisterFile* state)
    {
        // Forge an admitted response without doing semantic computation. Every
        // malformed image/metadata/condition/capture discrepancy must be rejected
        // BEFORE architectural writes. This code is solely a protocol adversary.
        foreach((int offset,uint value) in new[]{(84,0u),(84,0x8101u),(80,0x00400000u),(232,0x47000000u),
            (236,5u),(244,1u),(248,1u),(58,1u),(216,0x10000u)})
        {
            var b=new System.Collections.Generic.List<byte>{0xf3,0x0f,0x1e,0xfa};
            void D(int n){for(int j=0;j<4;++j)b.Add((byte)(n>>(8*j)));}
            void Put(int off,uint v){b.AddRange(new byte[]{0x41,0xc7,0x83});D(off);D(unchecked((int)v));}
            for(int p=48;p<88;p+=4)Put(p,0);Put(84,0x2101);Put(232,0x02000000);Put(236,0);Put(244,0);Put(216,0);Put(248,0);Put(252,0);
            Put(offset,value);Put(12,0);b.AddRange(new byte[]{0x31,0xc0,0x41,0xff,0xa3,88,0,0,0});
            using var bad=Transform80Publish(b.ToArray());var i=Canon157Classify([0xd9,0xf0]);
            var call=new FullX87KernelCall(69,bad.Entry,73,b.Count);using var body=CreateFullBlock([new(i,X87Kernel:call)]);
            Canon154Seed(fake,state,0,0x37f);Canon154Input(fake,state,0,default);
            byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,body.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
            Check(RunFull(body,&f)==FullBlockExit.Invalidated&&issued==1&&completed==0&&f.FirstPreissued==1&&
                *(uint*)((byte*)state+192+280)==0x58383746,"R157 protocol poison fatal preserves SAME reserved issue");
            CanonicalX87Same(fake,state,m,raw);
        }
    }
}
