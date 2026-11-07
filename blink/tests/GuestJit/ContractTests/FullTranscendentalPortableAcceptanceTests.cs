using System;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using Raw171 = Managed.Emulation.Execution.FullExtended80;
using Op171 = Managed.Emulation.Execution.FullExtended80Transcendental;

unsafe partial class Program
{
    // ONE family qualification entry. AUTHORED_UNRUN. Parent wires this once;
    // production activation remains false. No old private host numeric entry.
    static void CheckFullCanonicalX87PortableFamily(byte* machine)
    {
        _=machine;Canon157Grammar();Trans168SoftwareFamilySuite();
        Check(!FullExtended80TranscendentalProtocol.GuestActivationReady || FullExtended80TranscendentalSoftwareProfile.Qualified,
            "portable production activation cannot bypass numerical acceptance");
        Check(FullX87KernelCatalog.MaximumKernels==77 && FullX87KernelCatalog.StagingBytes==65536 &&
            FullX87KernelCatalog.RegionBytes==65536 && FullX87KernelCatalog.MaximumCodeBytes==77L*65536 &&
            sizeof(FullExtended80WorkFrame)==384 && sizeof(FullBlockFrame)==416 && sizeof(FullX87NativeStorage)==576,
            "R171 bounded charged catalog keeps77 keys, Work384/frame416/ABI7");
        using var io=new SemanticTestIo();using var authority=new FullCodeAuthority(io.Value,4096,2,4L<<20);
        using var store=new RetainedTranslationStore(RetainedOptions() with {PayloadBytes=0});
        using var catalog=new FullX87KernelCatalog(authority,store,true);
        var calls=new FullX87KernelCall[77];
        lock(authority.Gate)
        {
            Check(catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes],out _),"R171 eager quota-backed software catalog");
            for(int key=0;key<77;++key)
            {
                var binding=catalog.Find((FullX87KernelKey)key);
                var owned=new FullX87KernelCall((byte)key,binding.Entry,store.OwnerCookie,binding.CodeBytes);
                Check(catalog.Authenticates(in owned),"R171 same-Gate immutable typed entry/cookie/length authentication");
                foreach(var forged in new[]{owned with{Key=255},owned with{Entry=owned.Entry+1},owned with{OwnerCookie=owned.OwnerCookie+1},owned with{CodeBytes=owned.CodeBytes-1}})
                    Check(!catalog.Authenticates(in forged),"R171 foreign/stale/interior/wrong-length tuple rejected before compile");
                // Synthetic leaf harness uses73; the real compiler/controller
                // below obtains its actual cookie from its own owner catalog.
                calls[key]=owned with{OwnerCookie=73};
            }
            foreach(Op171 op in Enum.GetValues<Op171>())
            {
                byte[] portable=new byte[65536];
                Check(FullExtended80TranscendentalSoftwareEmitter.TryEmitContinuation(op,portable,out int length),"R171 portable continuation fits approved staging");
                var call=calls[69+(int)op];
                Check(length==call.CodeBytes && new ReadOnlySpan<byte>((void*)call.Entry,length).SequenceEqual(portable.AsSpan(0,length)),
                    "R171 catalog69..76 are exact portable code, no historical host binding");
            }
        }
        var token=RetainedPublish(store,0x571000);Check(store.TryAcquireToken(token,out var root),"R171 original TB root lease");
        Check(store.TryBeginNativeEpoch(out var epoch),"R171 original native epoch");
        bool enrolled;lock(authority.Gate)enrolled=catalog.AdmissionOpen&&authority.TryEnterNative(authority.Epoch);
        Check(enrolled,"R171 same-Gate native enrollment spans entry/continuation/RET");
        byte* fake=(byte*)NativeMemory.AlignedAlloc((nuint)((sizeof(Blink.Machine)+15)&~15),16);
        var state=(FullX87RegisterFile*)NativeMemory.AlignedAlloc(576,16);
        try
        {
            Check(fake!=null&&state!=null&&!fullOwnerScaffold.IsValid,"R171 isolated complete canonical storage");
            for(int scaffold=0;scaffold<2;++scaffold)
            {
                using var shared=CreateFullScaffold(out var addresses);
                if(scaffold!=0)fullOwnerScaffold=addresses;
                try
                {
                    Canon171GeneralImages(fake,state,calls);Canon171LiteralSpecials(fake,state,calls);
                    Canon157Completion(fake,state,calls);Canon157Boundaries(fake,state,calls);Canon157Deferred(fake,state,calls);
                    Canon165IntegerComplete(fake,state,calls);Canon165IntegerPoison(fake,state);Canon171RetainedProductPoison(fake,state);
                    Canon171ResponsePoison(fake,state,calls);Canon171Preissued(fake,state,calls);
                }
                finally{fullOwnerScaffold=default;}
            }
            Canon171AbiScaffolds(fake,state,calls);
            lock(authority.Gate)catalog.CloseAdmission();
        }
        finally
        {
            NativeMemory.AlignedFree(state);NativeMemory.AlignedFree(fake);
            if(enrolled)authority.ExitNative();epoch.Dispose();root.Dispose();store.Dispose();catalog.Dispose();
        }
        Check(catalog.ChargedBytes==0,"R171 all quota ownership released only after original store barrier");
        Canon171ControllerFamily();
    }
    static bool Trans171Undefined(int op,Raw171 x)
    {
        int e=x.SignExponent&0x7fff;
        if(op==0)return e>0x3fff||e==0x3fff&&x.Significand>0x8000000000000000UL;
        if(op!=4)return false;
        ulong limit=(x.SignExponent&0x8000)!=0?0x95f619980c4336f7UL:0xd413cccfe7799211UL;
        return e>0x3ffd||e==0x3ffd&&x.Significand>limit;
    }
    static void Canon171GeneralImages(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        var coverage=new int[8];
        foreach(Op171 op in Enum.GetValues<Op171>())
        {
            int k=(int)op;var i=Canon157Classify([0xd9,canon157Forms[k]]);
            using var code=CreateFullBlock([new(i,X87Kernel:calls[69+k])]);
            foreach(var row in System.Linq.Enumerable.Concat(trans168Vectors,trans171BoundaryVectors))
            {
                if(row.Op!=k)continue;
                foreach(int rc in new[]{0,1,2,3})foreach(int pc in new[]{0,1,2,3})
                foreach(int masks in new[]{0,7,15,23,31,39,47,55,63})
                {
                    ushort cw=(ushort)(0x40|pc<<8|rc<<10|masks);
                    bool undefined=Trans171Undefined(k,row.X),pair=Trans157Pair(op);
                    int de=(row.X.SignExponent&0x7fff)==0&&row.X.Significand!=0?2:0;
                    if(Trans157Binary(op)&&(row.Y.SignExponent&0x7fff)==0&&row.Y.Significand!=0)de=2;
                    bool suppressed=!undefined&&de!=0&&(masks&2)==0;
                    Trans168Rounded a=default,b=default,am=default,bm=default;
                    bool known=true;
                    if(undefined){var copied=k==0?row.X:row.Y;a=new(copied.Significand,copied.SignExponent,0);}
                    else
                    {
                        known=Trans168Certify(row.Low,row.High,row.Exponent,rc,masks,!row.Exact,out a);
                        known&=Trans168Certify(row.Low,row.High,row.Exponent,rc,63,!row.Exact,out am);
                        if(pair)
                        {
                            known&=Trans168Certify(row.SecondLow,row.SecondHigh,row.SecondExponent,rc,masks,op!=Op171.Fptan,out b);
                            known&=Trans168Certify(row.SecondLow,row.SecondHigh,row.SecondExponent,rc,63,op!=Op171.Fptan,out bm);
                        }
                    }
                    // Midpoint-straddling reference intervals are covered by the
                    // standalone envelope tests, not mislabeled as literal oracles.
                    if(!known)continue;
                    int actual=undefined?0:op==Op171.Fsincos?(a.Status|(b.Status&~0x200)|de):(a.Status|de);
                    int diagnostic=undefined?0:op==Op171.Fsincos?(am.Status|(bm.Status&~0x200)|de):(am.Status|de);
                    if(suppressed)actual=2;
                    int top=(coverage[k]++)&7;Canon154Seed(fake,state,top,cw);
                    Canon154Input(fake,state,0,row.X);if(Trans157Binary(op))Canon154Input(fake,state,1,row.Y);
                    if(pair)Canon154Input(fake,state,7,default,3);
                    byte[] before=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                    byte[] context=CanonicalX87Bytes((byte*)state+192+320,64);
                    Canon171Expected(i,op,a,b,(ushort)actual,suppressed,false,undefined,before,raw);
                    ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,code.Entry);
                    var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                    Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1 &&
                        f.HelperIndex==FullBlockFrame.NoHelperIndex && f.MemoryAssists==0 && f.HostMxcsrSaved==0,
                        $"R171 all8 positive canonical general commit k={k} rc={rc} masks={masks}");
                    CanonicalX87Same(fake,state,before,raw);
                    byte* work=(byte*)state+192;
                    Check(((ushort*)(work+80))[0]==diagnostic&&((ushort*)(work+82))[0]==actual,"R171 independent masked and actual status oracles");
                    Check(((uint*)(work+280))[0]==0&&new ReadOnlySpan<byte>(work+288,32).IndexOfAnyExcept((byte)0)<0&&
                        new ReadOnlySpan<byte>(work+320,64).SequenceEqual(context),"R171 verifier charge scrubbed, remaining caller context preserved");
                }
            }
        }
        for(int k=0;k<8;++k)Check(coverage[k]>0,"R171 each operation has certified general full-image positive commits");
    }
    static void Canon171Expected(JitInstruction i,Op171 op,Trans168Rounded a,Trans168Rounded b,ushort delta,
        bool suppressed,bool range,bool undefined,byte[] m,byte[] raw)
    {
        int top=(int)(CanonicalX87U32(m,648)>>11)&7;uint sw=CanonicalX87U32(m,648),tw=CanonicalX87U32(m,652);
        ushort cw=(ushort)CanonicalX87U32(m,672);
        uint replace=range?0x400u:Trans157Trig(op)&&!suppressed&&!undefined?0x600u:0x200u;
        sw=(sw&~replace)|(range?0x400u:(uint)delta&0x200u);sw|=(uint)delta&63;
        if((delta&~cw&63)!=0)sw|=0x8080;
        void Write(int physical,Trans168Rounded v)
        {var value=new Raw171(v.Sig,v.Se);Canon154Write(raw,physical,value);tw=(tw&~(3u<<(2*physical)))|(uint)Canon154Tag(value)<<(2*physical);}
        if(!suppressed&&!range)
        {
            bool binary=Trans157Binary(op),pair=Trans157Pair(op);Write(binary?(top+1)&7:top,a);
            if(binary){tw|=3u<<(2*top);top=(top+1)&7;}
            else if(pair){top=(top+7)&7;Write(top,b);}
        }
        sw=(sw&~0x3800u)|(uint)top<<11;CanonicalX87W32(m,648,sw);CanonicalX87W32(m,652,tw);
        CanonicalX87W64(m,664,i.Ip);CanonicalX87W32(m,656,(uint)i.Extra.Immediate);
        BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40),BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(472)));
        CanonicalX87W64(m,0,i.Ip+(uint)i.Length);m[8]=0;
    }
    static void Canon171Preissued(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach(Op171 op in Enum.GetValues<Op171>())foreach(bool stopNow in new[]{false,true})
        {
            var i=Canon157Classify([0xd9,canon157Forms[(int)op]]);using var code=CreateFullBlock([new(i,X87Kernel:calls[69+(int)op])]);
            Canon154Seed(fake,state,0,0x37f);Canon154Input(fake,state,0,op is Op171.Fyl2x or Op171.Fpatan?new(0x8000000000000000,0x3fff):default);
            if(Trans157Binary(op))Canon154Input(fake,state,1,default);if(Trans157Pair(op))Canon154Input(fake,state,7,default,3);
            byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),r=CanonicalX87Bytes((byte*)state,192);
            ulong epoch=11;long issued=1,completed=0;int stop=stopNow?1:0;var h=FullHeader(i.Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);f.X87=state;f.FirstPreissued=1;
            Check(RunFull(code,&f)==(stopNow?FullBlockExit.Stop:FullBlockExit.Continue)&&issued==1&&completed==(stopNow?0:1),
                "R171 stop wins; otherwise preissued instruction completes without budget/reissue");
            if(stopNow)CanonicalX87Same(fake,state,m,r);
        }
    }
    static void Canon171ControllerFamily()
    {
        foreach(Op171 op in Enum.GetValues<Op171>())
        {
            using var fixture=new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base;
            fixture.Map(pc,4096);fixture.Fill(pc,4096);WriteControllerCode(fixture,pc,[0xd9,canon157Forms[(int)op],0xeb,0xfe]);
            fixture.StartCanonicalFamily();
            try
            {
                var machine=fixture.Machine();var file=fixture.CanonicalFile();
                machine->fpu.cw=0x37f;machine->fpu.sw=0;machine->fpu.tw=0xfff0;
                // Ordinary non-anchor inputs for all8 (positive log1p in-domain).
                var x=new Raw171(0xa000000000000001,op==Op171.Fyl2x?(ushort)0x3fff:(ushort)0x3ffd);
                FullX87RegisterFile.Physical(file,0)[0]=x;FullX87RegisterFile.Physical(file,1)[0]=new(0xc000000000000001,0x3fff);
                if(Trans157Pair(op))machine->fpu.tw|=3<<14;
                fixture.Counts(100);Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;
                fixture.NativePrepared=cap;fixture.NativeReturned=_=>fixture.NativePrepared=cap;
                Check(fixture.Step(pc),"R171 actual compiler/controller all8 general entry");
                for(int retry=0;fixture.Completed<1&&retry<16;++retry)Check(fixture.Continue(),"R171 bounded same-issue controller continuation");
                Check(fixture.Issued==1&&fixture.Completed==1&&machine->ip==pc+2&&machine->oplen==0&&
                    fixture.Statistics.SemanticHelperInstructions==0&&fixture.Statistics.InterpreterAttempts==0,
                    "R171 actual typed compiler binding executes every general form once, no managedOp");
                int destination=Trans157Binary(op)?1:0;var value=FullX87RegisterFile.Physical(file,destination);
                var row=Array.Find(trans168Vectors,v=>v.Op==(int)op&&v.X.Significand==x.Significand&&v.X.SignExponent==x.SignExponent);
                Check(row.Low!=null,"R171 independent controller reference row exists");
                Check(Trans168Certify(row.Low,row.High,row.Exponent,0,63,!row.Exact,out var expected),"R171 controller reference interval certifies raw80");
                Check(value->Significand==expected.Sig&&value->SignExponent==expected.Se,"R171 actual compiler/controller literal interval raw result");
                Check((machine->fpu.sw&0x23f)==expected.Status,"R171 controller independent status including sine pairC1");
                if(Trans157Pair(op))
                {
                    var pushed=FullX87RegisterFile.Physical(file,7);
                    Check(Trans168Certify(row.SecondLow,row.SecondHigh,row.SecondExponent,0,63,op!=Op171.Fptan,out var second)&&
                        pushed->Significand==second.Sig&&pushed->SignExponent==second.Se,"R171 controller exact pair order and second value");
                }
            }
            finally{fixture.EndCanonicalFamily();}
        }
    }
}

unsafe partial class Program
{
    // Execute the whole wrapper under both argument-register conventions and
    // both scaffold forms, in addition to the standalone RET/R11 ABI matrix.
    // This remains synchronous ABI evidence; Windows OS/unwind is unqualified.
    static void Canon171AbiScaffolds(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach(bool windows in new[]{false,true})foreach(bool shared in new[]{false,true})
        {
            byte[] slab=new byte[FullBlockEmitter.ScaffoldCapacity];
            int slabLength=FullBlockEmitter.EmitScaffold(slab,out var layout,windows);
            using var scaffold=Transform80Publish(slab.AsSpan(0,slabLength));var addresses=layout.Resolve(scaffold.Entry);
            foreach(Op171 op in Enum.GetValues<Op171>())
            {
                var row=Array.Find(trans168Vectors,v=>v.Op==(int)op&&v.X.Significand==0xa000000000000001UL&&v.X.SignExponent==(op==Op171.Fyl2x?0x3fff:0x3ffd));
                Check(Trans168Certify(row.Low,row.High,row.Exponent,0,63,!row.Exact,out var a),"R171 ABI independent normal oracle");
                var b=default(Trans168Rounded);
                if(Trans157Pair(op))Check(Trans168Certify(row.SecondLow,row.SecondHigh,row.SecondExponent,0,63,op!=Op171.Fptan,out b),"R171 ABI independent second oracle");
                ushort status=(ushort)(op==Op171.Fsincos?a.Status|(b.Status&~0x200):a.Status);
                var i=Canon157Classify([0xd9,canon157Forms[(int)op]]);FullBlockInstruction[] instructions=[new(i,X87Kernel:calls[69+(int)op])];
                byte[] bytes=new byte[FullBlockEmitter.CodeCapacity];
                int length=shared?FullBlockEmitter.Emit(instructions,bytes,in addresses,windows):FullBlockEmitter.Emit(instructions,bytes,windows);
                using var body=Transform80Publish(bytes.AsSpan(0,length));using var adapter=Transform80Adapter(body.Entry,windows,false);
                Canon154Seed(fake,state,3,0x37f);Canon154Input(fake,state,0,row.X);
                if(Trans157Binary(op))Canon154Input(fake,state,1,row.Y);if(Trans157Pair(op))Canon154Input(fake,state,7,default,3);
                byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
                Canon171Expected(i,op,a,b,status,false,false,false,m,raw);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(i.Ip,body.Entry);
                var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1);f.X87=state;
                Check(RunFull(adapter,&f)==FullBlockExit.Continue&&issued==1&&completed==1,"R171 all8 whole canonical wrapper both ABIs/scaffolds");
                CanonicalX87Same(fake,state,m,raw);
            }
        }
    }
}
