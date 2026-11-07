using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using MaskedFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // R120 ALL AUTHORED_UNRUN. Parent alone integrates/builds/emits/runs.
    // Synthetic leaf Ready/TicketReady checks do NOT qualify MMU/controller ownership.
    static void CheckFullNativeMaskedStore(byte* machine)
    {
        Check(FullBlockEmitter.HasExpectedMaskedStoreLayout() && sizeof(FullMaskedStoreDescriptor)==128 &&
            sizeof(FullBlockFrame)==416 && FullBlockFrame.AbiVersion==0x46420007 && sizeof(TranslationBlockHeader)==160 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.StackMemory)).ToInt32()==384,
            "R120 compatible union ABI6/frame400/header160 SW648/TW652");
        CheckFullMaskedStoreClassifier();
        CheckFullMaskedStoreLeaves(machine); CheckFullMaskedStoreIdentity(machine); CheckFullMaskedStoreFaults(machine);
        CheckFullMaskedStoreBoundaries(machine);
        if(!fullOwnerScaffold.IsValid)
        {
            using var common=CreateFullScaffold(out var addresses);fullOwnerScaffold=addresses;
            try { CheckFullMaskedStoreLeaves(machine);CheckFullMaskedStoreIdentity(machine);CheckFullMaskedStoreFaults(machine);CheckFullMaskedStoreBoundaries(machine); }
            finally { fullOwnerScaffold=default; }
        }
        using(var fixture=new MaskedFixture(interpreterOracle:true)) CheckFullMaskedStoreOracle(fixture);
        CheckFullMaskedStoreMmu();CheckFullMaskedStoreEof();CheckFullMaskedStoreChangedMapping();CheckFullMaskedStoreController();CheckFullMaskedStoreAlias();CheckFullMaskedStoreEmission();
    }

    static byte[] MaskedEncoding(bool xmm,bool high=false,bool alias=false,byte segment=0,bool ea32=false)
    {
        var bytes=new List<byte>();if(segment!=0)bytes.Add(segment);if(ea32)bytes.Add(0x67);
        if(xmm)bytes.Add(0x66);if(high)bytes.Add(0x4f);
        bytes.AddRange([0x0f,0xf7,alias?(byte)0xc0:(byte)0xc1]);return bytes.ToArray();
    }
    static JitInstruction MaskedClassify(byte[] bytes)
    {
        Check(FullMaskedStoreLowering.TryClassify(Ip,bytes,out var i,out var why),why??"R120 classify");
        Check(FullBlockEmitter.CanInlineMaskedStore(i)&&FullBlockEmitter.ValidateMaskedStore(i),"R120 dispatch before broad MMX/Vector");return i;
    }
    static void CheckFullMaskedStoreClassifier()
    {
        foreach(bool xmm in new[]{false,true})foreach(bool high in new[]{false,true})foreach(bool alias in new[]{false,true})
        foreach(byte segment in new byte[]{0,0x26,0x2e,0x36,0x3e,0x64,0x65})foreach(bool ea32 in new[]{false,true})
        {
            byte[] bytes=MaskedEncoding(xmm,high,alias,segment,ea32);var i=MaskedClassify(bytes);
            Check(i.Options==(xmm?0x5001u:0x3007u)&&i.Width==(xmm?128:64)&&i.SourceWidth==i.Width&&
                i.Source.Register==(xmm&&high?8:0)&&i.Extra.Register==(alias?i.Source.Register:xmm&&high?9:1)&&
                i.Address.Base==7&&i.Address.Index==-1&&i.Address.AddressWidth==(ea32?32:64)&&
                (FullBlockEmitter.MaskedStoreShape(i)&(1u<<24))!=0,"R120 exact form REX masked MMX/extended XMM/implicit DI fixed policy");
            for(int n=0;n<bytes.Length;++n)Check(!FullMaskedStoreLowering.TryClassify(Ip,bytes.AsSpan(0,n),out _,out _),"R120 exact-span truncation");
            Check(!FullMaskedStoreLowering.TryClassify(Ip,[..bytes,0x90],out _,out _),"R120 trailing byte rejected");
        }
        foreach(bool xmm in new[]{false,true})for(int rex=0;rex<16;++rex)for(int source=0;source<8;++source)for(int mask=0;mask<8;++mask)
        {
            byte[] bytes=xmm?new byte[]{0x66,(byte)(0x40|rex),0x0f,0xf7,(byte)(0xc0|source<<3|mask)}:
                new byte[]{(byte)(0x40|rex),0x0f,0xf7,(byte)(0xc0|source<<3|mask)};
            var i=MaskedClassify(bytes);
            Check(i.Source.Register==(source|(xmm?(rex&4)<<1:0))&&i.Extra.Register==(mask|(xmm?(rex&1)<<3:0))&&i.Address.Base==7,
                "R120 exhaustive MMX0..7/XMM0..15 ModRM and all REX bits preserve implicit DI");
        }
        Check(MaskedClassify([0x4f,0x66,0x0f,0xf7,0xc1]).Source.Register==0&&
            MaskedClassify([0x66,0x66,0x67,0x67,0x64,0x65,0x40,0x4f,0x0f,0xf7,0xc1]).Address==new JitAddress(7,-1,0,0,false,32,5),
            "R120 final REX and segment groups; ignored W/X/B never extend DI");
        foreach(byte[] bytes in new byte[][]{[0xf0,0x0f,0xf7,0xc1],[0xf2,0x0f,0xf7,0xc1],[0xf3,0x66,0x0f,0xf7,0xc1],
            [0x0f,0xf7,0x01],[0x66,0x0f,0xf7,0x05],[0xc5,0xf9,0xf7,0xc1],[0x62,0xf1,0x7d,8,0xf7,0xc1]})
            Check(!FullMaskedStoreLowering.TryClassify(Ip,bytes,out _,out var reason)&&reason!=null,"R120 named LOCK illegal / REP excluded / memory ModRM / VEX / EVEX excluded");
        var good=MaskedClassify([0x0f,0xf7,0xc1]);
        foreach(var bad in new[]{good with{Width=128},good with{SourceWidth=128},good with{Length=2},good with{Source=new(8,0)},
            good with{Extra=new(8,0)},good with{Source=new(0,1)},good with{Extra=new(1,0,IsImmediate:true)},
            good with{Destination=default},good with{Target=1},good with{Condition=1},good with{Address=new(15,-1,0,0)},
            good with{Address=new(7,-1,0,1)},good with{Address=new(7,-1,0,0,false,16)},good with{Address=new(7,-1,0,0,false,64,6)}})
        {bool rejected=false;try{FullBlockEmitter.ValidateMaskedStore(bad);}catch(ArgumentException){rejected=true;}Check(rejected,"R120 malformed canonical IR fails before emission");}
        Check(!FullBlockEmitter.CanInlineMaskedStore(good with{Options=0x5002})&&!FullBlockEmitter.CanInlineMaskedStore(good with{Options=0x13007}),"R120 finite options only");
    }

    static void MaskedSeed(byte* machine,JitInstruction i,ulong di,uint enabled)
    {
        MmxSeed(machine);var m=(Blink.Machine*)machine;m->fpu.sw=0x7f00;m->fpu.cw=0x37f;m->fpu.tw=0x12345678;
        Write64(machine,80,di); // guest RDI; high R15 deliberately distinct
        Write64(machine,144,0x8877665544332211);
        for(int b=0;b<16;++b)machine[160+i.Source.Register*16+b]=(byte)(0x41+b*7);
        for(int b=0;b<16;++b)machine[160+i.Extra.Register*16+b]=(byte)((enabled>>b&1)!=0?0x80|(b&0x7f):b&0x7f);
    }
    static uint MaskedBits(byte* machine,JitInstruction i)
    {
        uint result=0;for(int b=0;b<i.Width/8;++b)result|=(uint)(machine[160+i.Extra.Register*16+b]>>7)<<b;return result;
    }
    static void MaskedGuestState(byte* machine,byte[] before,JitInstruction i,bool success)
    {
        byte[] expected=(byte[])before.Clone();
        if(success)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(expected,Ip+(uint)i.Length);expected[8]=0;
            if(i.Width==64)
            {
                uint sw=BinaryPrimitives.ReadUInt32LittleEndian(expected.AsSpan(648));
                BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(648),sw&~0x3800u);
                BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(652),0);
            }
        }
        foreach((int at,int count) in new[]{(0,9),(12,4),(24,128),(160,256),(456,236)})
            Check(new ReadOnlySpan<byte>(machine+at,count).SequenceEqual(expected.AsSpan(at,count)),
                "R120 precise guest state: only success IP and MASKMOVQ TOP/tag transition; all flags/GPR/vector/CW/other SW preserved");
    }
    static void MaskedPublishSynthetic(ref FullMaskedStoreDescriptor d,byte* p,byte* q,ulong guestPage,ulong epoch,bool ticket=false)
    {
        d.HostPage0=d.Page0==guestPage?p:q;
        d.HostPage1=d.Selected1==0?null:d.Page1==guestPage?p:q;
        d.Epoch=epoch;d.Ticket=ticket?0x123456789abcdef0ul:0ul;d.State=ticket?FullMaskedStoreState.TicketReady:FullMaskedStoreState.Ready;
    }
    static void CheckFullMaskedStoreLeaves(byte* machine)
    {
        byte* p=(byte*)NativeMemory.Alloc(4096),q=(byte*)NativeMemory.Alloc(4096);
        try
        {
            foreach(bool xmm in new[]{false,true})foreach(bool high in new[]{false,true})foreach(bool alias in new[]{false,true})
            foreach(bool lastRegister in new[]{false,true})foreach(int offset in new[]{0,1,4081,4088,4095})foreach(uint bits in new uint[]{0,1,2,0x80,0x100,0x8000,0x5555,0xaaaa,0xffff})
            foreach(bool ticket in new[]{false,true})
            {
                var i=MaskedClassify(MaskedEncoding(xmm,high,alias));
                if(lastRegister)i=i with{Source=new(xmm&&high?15:7,0),Extra=new(alias?(xmm&&high?15:7):xmm&&high?14:6,0)};
                const ulong guest=0x100000;ulong ea=guest+(uint)offset;
                MaskedSeed(machine,i,ea,bits);uint enabled=MaskedBits(machine,i);
                new Span<byte>(p,4096).Fill(0xa5);new Span<byte>(q,4096).Fill(0x5a);
                byte[] expected0=new ReadOnlySpan<byte>(p,4096).ToArray(),expected1=new ReadOnlySpan<byte>(q,4096).ToArray();
                for(int b=0;b<i.Width/8;++b)if((enabled>>b&1)!=0)
                {int at=offset+b;byte value=machine[160+i.Source.Register*16+b];if(at<4096)expected0[at]=value;else expected1[at-4096]=value;}
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullMaskedStoreDescriptor();var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                f.StackMemory=(FullStackMemoryDescriptor*)&d;
                var first=RunFull(code,&f);
                if(enabled!=0)
                {
                    Check(first==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryResume==1&&f.MemoryWidth==8&&
                        d.TagVersion==0x53500120&&d.Kind==0x53500002&&d.State==FullMaskedStoreState.Request&&d.Ticket==0&&
                        d.HostPage0==null&&d.HostPage1==null&&d.BaseEA==ea&&d.Ip==Ip&&d.EnabledBits==enabled&&
                        (d.Selected0|d.Selected1)==enabled&&(d.Selected0&d.Selected1)==0&&d.Selected0!=0,
                        "R120 native complete compact request before any effect");
                    MaskedGuestState(machine,before,i,false);
                    if(offset==4095&&(enabled&1)==0)Check(d.Page0==guest+4096&&d.Page1==0&&d.Selected0==enabled&&d.Selected1==0,
                        "R120 second-only page compacts slot0; disabled first page unmapped");
                    MaskedPublishSynthetic(ref d,p,q,guest,epoch,ticket);f.Budget=0;FullNativeMemory.ResumeSameIssue(&f);
                    Check(RunFull(code,&f)==FullBlockExit.Continue,"R120 native resumed sparse store");
                    Check(d.State==FullMaskedStoreState.Consumed&&d.Ticket==0&&
                        f.MemoryFlags==(ticket?FullMemoryFlags.BarrierConsumed:FullMemoryFlags.None),"R120 one-shot Ready/ticket consume");
                }
                else Check(first==FullBlockExit.Continue&&d.Kind==0,"R120 direct zero mask needs no mapper");
                Check(issued==1&&completed==1&&f.Executed==1&&f.MemoryResume==0&&f.FirstPreissued==0&&f.NativeMemoryComputations==1,
                    "R120 exact one issue/one commit/native computation including zero mask");
                MaskedGuestState(machine,before,i,true);
                Check(new ReadOnlySpan<byte>(p,4096).SequenceEqual(expected0)&&new ReadOnlySpan<byte>(q,4096).SequenceEqual(expected1),
                    "R120 each enabled MSB byte only; no dense stores/RMW or collateral disabled-byte changes");
            }
        }
        finally{NativeMemory.Free(p);NativeMemory.Free(q);}
    }

    static void CheckFullMaskedStoreIdentity(byte* machine)
    {
        byte* p=(byte*)NativeMemory.Alloc(4096),q=(byte*)NativeMemory.Alloc(4096);
        try
        {
            foreach(bool xmm in new[]{false,true})foreach(string stale in new[]{"tag","kind","site","state","consumed","shape","ip","base","data","mask",
                "enabled","selected0","selected1","page0","page1","guestData","guestMask","guestDI","observed","current","pointer0","pointer1","ticket0","ticketReady","extraPointer"})
            {
                var i=MaskedClassify(MaskedEncoding(xmm));const ulong guest=0x100000;
                MaskedSeed(machine,i,guest+4095,3);new Span<byte>(p,4096).Fill(0xa5);new Span<byte>(q,4096).Fill(0x5a);
                byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullMaskedStoreDescriptor();var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=(FullStackMemoryDescriptor*)&d;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R120 identity setup");MaskedPublishSynthetic(ref d,p,q,guest,epoch);
                switch(stale)
                {
                    case "tag":d.TagVersion=1;break;case "kind":d.Kind=0;break;case "site":d.Site++;break;
                    case "state":d.State=FullMaskedStoreState.Request;break;case "consumed":d.State=FullMaskedStoreState.Consumed;break;
                    case "shape":d.Shape^=1u<<24;break;case "ip":d.Ip++;break;case "base":d.BaseEA++;break;
                    case "data":d.DataLo^=1;break;case "mask":d.MaskLo^=1;break;case "enabled":d.EnabledBits^=1;break;
                    case "selected0":d.Selected0^=1;break;case "selected1":d.Selected1^=2;break;
                    case "page0":d.Page0+=4096;break;case "page1":d.Page1+=4096;break;
                    case "guestData":machine[160+i.Source.Register*16]^=1;before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();break;
                    case "guestMask":machine[160+i.Extra.Register*16]^=1;before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();break;
                    case "guestDI":Write64(machine,80,guest+4094);before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();break;
                    case "observed":d.Epoch--;d.HostPage0=(byte*)1;d.HostPage1=(byte*)1;break;
                    case "current":++epoch;d.HostPage0=(byte*)1;d.HostPage1=(byte*)1;break;
                    case "pointer0":d.HostPage0=null;break;case "pointer1":d.HostPage1=null;break;
                    case "ticket0":d.State=FullMaskedStoreState.TicketReady;d.Ticket=0;break;
                    case "ticketReady":d.Ticket=1;break;
                    case "extraPointer":
                        // Rebuild first-only request then poison unused second pointer.
                        MaskedSeed(machine,i,guest,1);FullNativeMemory.ResumeSameIssue(&f);
                        Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R120 changed identity restaged same issued op");
                        MaskedPublishSynthetic(ref d,p,q,guest,epoch);d.HostPage1=q;break;
                }
                FullNativeMemory.ResumeSameIssue(&f);
                if(stale=="current")
                {
                    // Reacquired entry is current, but the old descriptor/pointers
                    // retain the previous epoch and must restage the same issue.
                    f.ObservedGlobalEpoch=epoch;f.ObservedCodeAuthorityEpoch=epoch;h.ValidatedGlobalEpoch=epoch;
                }
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&d.State==FullMaskedStoreState.Request&&
                    d.Ticket==0&&d.Epoch==0&&d.HostPage0==null&&d.HostPage1==null&&f.MemoryResume==1,
                    "R120 stale state/identity/epoch/pointer/ticket restages same issue; poison pointers never dereferenced");
                Check(new ReadOnlySpan<byte>(p,4096).IndexOfAnyExcept((byte)0xa5)<0&&new ReadOnlySpan<byte>(q,4096).IndexOfAnyExcept((byte)0x5a)<0,
                    "R120 EVERY selected page validated before first store");
                if(stale!="extraPointer")MaskedGuestState(machine,before,i,false);
            }
        }
        finally{NativeMemory.Free(p);NativeMemory.Free(q);}
    }

    static void CheckFullMaskedStoreFaults(byte* machine)
    {
        foreach(bool xmm in new[]{false,true})foreach(byte segment in new byte[]{0,0x36,0x64,0x65})
        foreach(ulong address in new ulong[]{0x800000000000,0x7fffffffffff,ulong.MaxValue})foreach(uint bits in new uint[]{0,1,2})
        {
            var i=MaskedClassify(MaskedEncoding(xmm,segment:segment));MaskedSeed(machine,i,address,bits);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullMaskedStoreDescriptor();var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=(FullStackMemoryDescriptor*)&d;
            if(segment==0x64)f.FsBase=0;if(segment==0x65)f.GsBase=0;
            var exit=RunFull(code,&f);
            bool fault=bits!=0&&(address==0x800000000000||bits==2);
            Check(exit==(bits==0?FullBlockExit.Continue:fault?FullBlockExit.GuestFault:FullBlockExit.NeedMemory)&&
                issued==1&&completed==(bits==0?1:0),"R120 zero profile bypasses noncanonical/overflow EA; only selected bytes checked");
            if(fault)Check(f.FaultTrap==(segment==0x36?12u:13u)&&f.FaultCode==128&&f.FaultAddress==0,"R120 typed SS/GP canonical metadata");
            MaskedGuestState(machine,before,i,bits==0);
        }
        foreach(bool zero in new[]{false,true})foreach(bool pending in new[]{false,true})
        {
            var i=MaskedClassify([0x0f,0xf7,0xc1]);MaskedSeed(machine,i,0x800000000000,zero?0u:1u);
            var m=(Blink.Machine*)machine;m->fpu.sw=4;m->fpu.cw=pending?0x37bu:0x37fu;
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullMaskedStoreDescriptor{Kind=0x11223344};var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=(FullStackMemoryDescriptor*)&d;
            Check(RunFull(code,&f)==(pending||!zero?FullBlockExit.GuestFault:FullBlockExit.Continue),"R120 MMX pending MF before zero/path EA");
            if(pending)Check(f.FaultTrap==16&&f.FaultAddress==Ip&&d.Kind==0x11223344&&completed==0,"R120 waiting MF precedes every descriptor/CPU/memory effect");
            MaskedGuestState(machine,before,i,!pending&&zero);
        }
        foreach(bool xmm in new[]{false,true})foreach(byte segment in new byte[]{0x64,0x65})
        {
            var i=MaskedClassify(MaskedEncoding(xmm,high:true,segment:segment,ea32:true));MaskedSeed(machine,i,0xffffffff00000fff,1);
            using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullMaskedStoreDescriptor();var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=(FullStackMemoryDescriptor*)&d;
            if(segment==0x64)f.FsBase=0x100000;else f.GsBase=0x100000;
            Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&d.BaseEA==0x100fff&&d.Page0==0x100000&&d.Page1==0,
                "R120 EA32 truncates EDI before FS/GS; high MMX/XMM source REX never extends implicit DI");
        }
    }

    static void CheckFullMaskedStoreBoundaries(byte* machine)
    {
        var i=MaskedClassify([0x66,0x0f,0xf7,0xc1]);
        foreach(string boundary in new[]{"budget","stop","attention"})
        {
            MaskedSeed(machine,i,0x100fff,3);using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=boundary=="stop"?1:0;
            var h=FullHeader(Ip,code.Entry);var d=new FullMaskedStoreDescriptor();var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:boundary=="budget"?0ul:100ul);
            f.StackMemory=(FullStackMemoryDescriptor*)&d;if(boundary=="attention")((Blink.Machine*)machine)->attention=true;
            var exit=RunFull(code,&f);
            Check(exit==(boundary=="budget"?FullBlockExit.Budget:boundary=="stop"?FullBlockExit.Stop:FullBlockExit.Attention)&&completed==0&&d.Kind==0,
                "R120 budget/stop/attention boundary precedes sparse request or effects");((Blink.Machine*)machine)->attention=false;
        }
        byte* p=(byte*)NativeMemory.Alloc(4096);
        try
        {
            MaskedSeed(machine,i,0x100000,1);using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;
            var h=FullHeader(Ip,code.Entry);var d=new FullMaskedStoreDescriptor();var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=(FullStackMemoryDescriptor*)&d;
            Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R120 pending issue stop setup");MaskedPublishSynthetic(ref d,p,null,0x100000,epoch);
            FullNativeMemory.ResumeSameIssue(&f);stop=1;
            Check(RunFull(code,&f)==FullBlockExit.Stop&&issued==1&&completed==0&&f.FirstPreissued==1&&d.State==FullMaskedStoreState.Ready,"R120 stop retains approved same issue and Ready state");
            stop=0;f.Budget=0;
            Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1,"R120 later exhausted budget does not reissue approved store");
        }
        finally{NativeMemory.Free(p);}
    }

    static void CheckFullMaskedStoreOracle(MaskedFixture fixture)
    {
        // Restricted COMMON domain: dense destination fully writable, ESbase0,
        // default DS, no ignored-MMX REX extensions; MMX TOP/tag already transitioned.
        // Same-page only: selected BeginStore crossing uses copy=false then
        // EndStore copies the WHOLE staging span, corrupting disabled bytes.
        // Faults/crossing/FS-GS/ignored-MMX REX/transition use independent expectations.
        const ulong guest=MaskedFixture.Base;fixture.Map(guest,8192);fixture.Fill(guest,8192);
        foreach(bool xmm in new[]{false,true})foreach(bool alias in new[]{false,true})foreach(uint bits in new uint[]{0,1,0x55,0xffff})
        {
            byte[] bytes=MaskedEncoding(xmm,alias:alias);var i=MaskedClassify(bytes);byte* machine=(byte*)fixture.Machine();
            MaskedSeed(machine,i,guest+64,bits);fixture.Machine()->fpu.sw&=~0x3800u;fixture.Machine()->fpu.tw=0;fixture.Machine()->es.@base=0;
            byte* p=(byte*)fixture.Physical(guest),q=(byte*)fixture.Physical(guest+4096);
            new Span<byte>(p,4096).Fill(0xa5);new Span<byte>(q,4096).Fill(0x5a);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            fixture.DataScope(0,()=>IntegerReference(fixture.Program,(byte*)fixture.Machine(),bytes));
            byte[] after=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray(),a=new ReadOnlySpan<byte>(p,4096).ToArray(),b=new ReadOnlySpan<byte>(q,4096).ToArray();
            MmxRestoreGuest(machine,before);new Span<byte>(p,4096).Fill(0xa5);new Span<byte>(q,4096).Fill(0x5a);
            using var code=CreateFullBlock([new(i)]);ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullMaskedStoreDescriptor();var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop);f.StackMemory=(FullStackMemoryDescriptor*)&d;
            var exit=RunFull(code,&f);if(MaskedBits(machine,i)!=0)
            {Check(exit==FullBlockExit.NeedMemory,"R120 oracle native request");MaskedPublishSynthetic(ref d,p,q,guest,epoch);FullNativeMemory.ResumeSameIssue(&f);exit=RunFull(code,&f);}
            Check(exit==FullBlockExit.Continue&&new ReadOnlySpan<byte>(p,4096).SequenceEqual(a)&&new ReadOnlySpan<byte>(q,4096).SequenceEqual(b),"R120 actual ordinary GetOp common-domain bytes");
            foreach((int at,int count) in new[]{(0,9),(12,4),(24,128),(160,256),(456,236)})
                Check(new ReadOnlySpan<byte>(machine+at,count).SequenceEqual(after.AsSpan(at,count)),"R120 common-domain GetOp guest state (no false discrepancy oracle)");
        }
    }

    static FullBlockFrame MaskedMmuFrame(FullMaskedStoreDescriptor* d,FullMemoryTlbEntry* tlb,byte* machine) => new()
    {Version=FullBlockFrame.AbiVersion,Size=400,Machine=machine,MemoryResume=1,MemoryWidth=8,MemoryAccess=FullMemoryAccess.Write,
        MemoryAddress=d->BaseEA+(uint)System.Numerics.BitOperations.TrailingZeroCount(d->EnabledBits),Context=0x1234,StackMemory=(FullStackMemoryDescriptor*)d,MemoryTlb=tlb,MemoryTlbMask=63};

    static FullMaskedStoreDescriptor MaskedMmuDescriptor(byte* machine,JitInstruction i,ulong ea,uint bits)
    {
        // Test reference only. Runtime assist may VALIDATE this arithmetic;
        // it must never use it to perform the instruction's data writes.
        var d=new FullMaskedStoreDescriptor{TagVersion=0x53500120,Kind=0x53500002,State=FullMaskedStoreState.Request,
            BaseEA=ea,Ip=i.Ip,Shape=FullBlockEmitter.MaskedStoreShape(i),EnabledBits=bits};
        d.DataLo=BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(machine+160+i.Source.Register*16,8));
        d.MaskLo=BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(machine+160+i.Extra.Register*16,8));
        if(i.Width==128){d.DataHi=BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(machine+168+i.Source.Register*16,8));d.MaskHi=BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(machine+168+i.Extra.Register*16,8));}
        for(int b=0;b<i.Width/8;++b)if((bits>>b&1)!=0)
        {
            ulong page=(ea+(uint)b)&~4095UL;
            if(d.Selected0==0||page==d.Page0){d.Page0=page;d.Selected0|=1u<<b;}
            else{d.Page1=page;d.Selected1|=1u<<b;}
        }
        return d;
    }
    static void CheckFullMaskedStoreMmu()
    {
        foreach(bool xmm in new[]{false,true})foreach(string mode in new[]{"first-only-unmapped-second","second-only-unmapped-first","both-valid","both-fault","disabled-readonly","selected-readonly"})
        {
            using var fixture=new MaskedFixture();const ulong guest=MaskedFixture.Base;
            bool secondOnly=mode=="second-only-unmapped-first";
            if(!secondOnly){fixture.Map(guest);fixture.Fill(guest);}
            if(mode!="first-only-unmapped-second"&&mode!="both-fault"){fixture.Map(guest+4096);fixture.Fill(guest+4096);}
            bool readOnly=mode is "disabled-readonly" or "selected-readonly";
            if(readOnly)fixture.DataScope(0,()=>Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system,(long)(guest+4096),4096,1,false)==0,"R120 actual readonly second page"));
            uint bits=secondOnly?2u:mode is "first-only-unmapped-second" or "disabled-readonly"?1u:3u;
            var i=MaskedClassify(MaskedEncoding(xmm));byte* machine=(byte*)fixture.Machine();MaskedSeed(machine,i,guest+4095,bits);
            var d=MaskedMmuDescriptor(machine,i,guest+4095,bits);FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            var frame=MaskedMmuFrame(&d,tlb,machine);
            Check(FullNativeMemory.IsMaskedStoreRequest(&frame),"R120 tag-first sparse grammar");
            Check(FullNativeMemory.AuthenticateMaskedStore(&frame,in i,(Blink.Machine*)machine),"R120 exact instruction/snapshot identity before mapping");
            var saved=d;
            foreach(string field in new[]{"tag","kind","shape","ip","base","data","mask","bits","page","selector","register"})
            {
                d=saved;var candidate=i;
                switch(field)
                {
                    case "tag":d.TagVersion=1;break;case "kind":d.Kind=2;break;case "shape":d.Shape^=1u<<24;break;
                    case "ip":d.Ip++;break;case "base":d.BaseEA++;break;case "data":d.DataLo^=1;break;
                    case "mask":d.MaskLo^=1;break;case "bits":d.EnabledBits^=1;break;case "page":d.Page0+=4096;break;
                    case "selector":d.Selected0^=1;break;case "register":candidate=i with{Source=new(2,0)};break;
                }
                Check(!FullNativeMemory.AuthenticateMaskedStore(&frame,in candidate,(Blink.Machine*)machine),"R120 parent authentication rejects exact snapshot/plan mismatch before mapping");
            }
            d=saved;
            var result=FillFullMemoryMmu(fixture,&frame);bool fault=mode is "both-fault" or "selected-readonly";
            Check(result==(fault?FullMemoryAssistResult.GuestFault:FullMemoryAssistResult.Resolved),"R120 actual MMU touches selected pages only");
            if(fault)Check(frame.FaultTrap==14&&frame.FaultAddress==guest+4096&&frame.FaultCode==(mode=="selected-readonly"?2u:1u)&&
                d.State!=FullMaskedStoreState.Ready,"R120 selected second fault precedes every data store");
            else Check(d.State==FullMaskedStoreState.Ready&&d.HostPage0!=null&&
                (d.Selected1==0?d.HostPage1==null:d.HostPage1!=null)&&d.Page0==(secondOnly?guest+4096:guest),"R120 checked compact independent page bases");
            if(!secondOnly)Check(*((byte*)fixture.Physical(guest)+4095)==0xcc,"R120 mapper/permission fault does not implement semantic byte write");
            d.Kind=0;Check(!FullNativeMemory.IsMaskedStoreRequest(&frame),"R120 ordinary Count0 cancels sparse kind");
            d.Kind=2;d.TagVersion=1;Check(!FullNativeMemory.IsMaskedStoreRequest(&frame),"R120 dense Count2 cannot be sparse");
        }
    }

    static void CheckFullMaskedStoreEof()
    {
        using var fixture=new MaskedFixture();const ulong guest=MaskedFixture.Base;
        fixture.Map(guest);fixture.Fill(guest);
        var file=fixture.Io.CreateMemoryFile("r120-selected-eof",0);Check(file.Succeeded,"R120 EOF backing opens");
        Check(fixture.Io.TruncateFile(file.Value,4096).Succeeded,"R120 EOF initial extent");
        fixture.MapShared(file.Value,guest+4096);fixture.Fill(guest+4096);
        Check(fixture.Io.TruncateFile(file.Value,0).Succeeded,"R120 actual EOF withdrawal");
        foreach(bool xmm in new[]{false,true})foreach(uint bits in new uint[]{1,2,3})
        {
            var i=MaskedClassify(MaskedEncoding(xmm));byte* machine=(byte*)fixture.Machine();MaskedSeed(machine,i,guest+4095,bits);
            var d=MaskedMmuDescriptor(machine,i,guest+4095,bits);FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            var frame=MaskedMmuFrame(&d,tlb,machine);var result=FillFullMemoryMmu(fixture,&frame);
            Check(result==(bits==1?FullMemoryAssistResult.Resolved:FullMemoryAssistResult.GuestFault),"R120 disabled EOF page ignored, selected EOF faults");
            if(bits!=1)Check(frame.FaultTrap==14&&unchecked((int)frame.FaultCode)==-2&&frame.FaultAddress==guest+4096&&
                d.State!=FullMaskedStoreState.Ready,"R120 exact first selected EOF byte uses BUS_ADRERR before any store");
            Check(*((byte*)fixture.Physical(guest)+4095)==0xcc,"R120 valid earlier selected byte unchanged on EOF failure");
        }
    }

    static void CheckFullMaskedStoreChangedMapping()
    {
        using var fixture=new MaskedFixture();const ulong guest=MaskedFixture.Base;
        fixture.Map(guest,8192);fixture.Fill(guest,8192);
        var i=MaskedClassify([0x66,0x0f,0xf7,0xc1]);byte* machine=(byte*)fixture.Machine();MaskedSeed(machine,i,guest+4095,3);
        using var code=CreateFullBlock([new(i)]);var h=FullHeader(Ip,code.Entry);h.ValidatedGlobalEpoch=fixture.Jit.Authority.CodeEpoch;
        long issued=0,completed=0;int stop=0;FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        var d=new FullMaskedStoreDescriptor();var frame=FullFrame(machine,&h,fixture.Jit.Authority.EpochPointer,&issued,&completed,&stop,limit:1,
            codeEpoch:fixture.Jit.Authority.CodeEpochPointer);frame.StackMemory=(FullStackMemoryDescriptor*)&d;frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
        Check(RunFull(code,&frame)==FullBlockExit.NeedMemory&&FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.Resolved,"R120 checked sparse real mapping setup");
        ulong old=d.Epoch;
        Check(fixture.Mutate("MAP_FIXED",guest+4096)==0,"R120 selected destination actual mapping replacement");fixture.Fill(guest+4096);
        frame.ObservedGlobalEpoch=fixture.Jit.Authority.Epoch;frame.ObservedCodeAuthorityEpoch=fixture.Jit.Authority.CodeEpoch;h.ValidatedGlobalEpoch=frame.ObservedCodeAuthorityEpoch;
        // The pointer may already be unmapped. Stale epoch MUST precede its use.
        d.HostPage0=(byte*)1;d.HostPage1=(byte*)1;FullNativeMemory.ResumeSameIssue(&frame);
        Check(old!=frame.ObservedGlobalEpoch&&RunFull(code,&frame)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&d.State==FullMaskedStoreState.Request&&
            *((byte*)fixture.Physical(guest)+4095)==0xcc&&*((byte*)fixture.Physical(guest+4096))==0xb8,
            "R120 remap invalidates ALL saved sparse pointers before any selected store; no new issue");
        Check(FillFullMemoryMmu(fixture,&frame)==FullMemoryAssistResult.Resolved,"R120 real remap rechecks every selected page");
        frame.ObservedGlobalEpoch=fixture.Jit.Authority.Epoch;frame.ObservedCodeAuthorityEpoch=fixture.Jit.Authority.CodeEpoch;h.ValidatedGlobalEpoch=frame.ObservedCodeAuthorityEpoch;
        FullNativeMemory.ResumeSameIssue(&frame);
        Check(RunFull(code,&frame)==FullBlockExit.Continue&&issued==1&&completed==1&&*((byte*)fixture.Physical(guest)+4095)==0x41&&
            *((byte*)fixture.Physical(guest+4096))==0x48,"R120 remapped selected pointers produce exact generated bytes once");
    }

    static void CheckFullMaskedStoreController()
    {
        foreach(bool xmm in new[]{false,true})foreach(string mode in new[]{"zero-noncanonical","first-only","second-only","lazy-both","selected-fault","watched-code"})
        {
            using var fixture=new MaskedFixture();const ulong pc=MaskedFixture.Base,destination=pc+8192;
            fixture.Map(pc);fixture.Fill(pc);bool zero=mode=="zero-noncanonical",watched=mode=="watched-code",fault=mode=="selected-fault";
            byte[] operation=MaskedEncoding(xmm);WriteControllerCode(fixture,pc,[0x90,..operation,0xeb,0xfe]);
            ulong ea=zero?0x800000000000:watched?pc+128:destination+4095;
            uint bits=zero?0u:mode=="second-only"?2u:mode is "lazy-both" or "selected-fault"?3u:1u;
            if(!zero&&!watched)
            {
                if(mode!="second-only"){fixture.Map(destination);fixture.Fill(destination);}
                if(mode is "second-only" or "lazy-both"){fixture.Map(destination+4096);if(mode=="second-only")fixture.Fill(destination+4096);}
            }
            var i=MaskedClassify(operation);MaskedSeed((byte*)fixture.Machine(),i,ea,bits);
            byte[] before=new ReadOnlySpan<byte>((byte*)fixture.Machine()+160,256).ToArray();
            Action<nint> cap=address=>((FullBlockFrame*)address)->NativeLimit=1;
            fixture.NativePrepared=cap;bool consumed=false;
            fixture.NativeReturned=address=>{consumed|=((FullBlockFrame*)address)->MemoryFlags==FullMemoryFlags.BarrierConsumed;fixture.NativePrepared=cap;};
            fixture.Counts(2);using var deadline=new Timer(_=>fixture.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            if(fault)
            {
                var signal=fixture.StepSynchronousFault(pc);
                Check(signal.Signal==11&&fixture.Machine()->trapno==14&&fixture.Machine()->faultaddr==(long)(destination+4096)&&
                    fixture.Issued==2&&fixture.Completed==1&&*((byte*)fixture.Physical(destination)+4095)==0xcc,
                    "R120 actual controller selected page fault leaves earlier selected byte unmodified");
            }
            else
            {
                Check(fixture.Step(pc),"R120 actual controller initial cap");
                for(int attempt=0;fixture.Completed<2&&attempt<8;++attempt)Check(fixture.Continue(),"R120 bounded mapping/ticket same-issue continuation");
                Check(fixture.Issued==2&&fixture.Completed==2&&fixture.Machine()->ip==pc+1+(uint)operation.Length&&fixture.Statistics.NativeMemoryInstructions==1,
                    "R120 controller maps only selected pages; watched code ticket consumed then postcommit return");
                if(watched)Check(consumed,"R120 watched write requires actual exclusive sparse TicketReady and BarrierConsumed postcommit return");
                if(!zero)
                {
                    byte first=(byte)(0x41+(bits==2?7:0));ulong address=ea+(bits==2?1u:0u);
                    Check(*((byte*)fixture.Physical(address&~4095UL)+(int)(address&4095))==first,"R120 real MMU/controller generated result byte");
                }
            }
            fixture.NativeReturned=null;
            Check(fixture.Statistics.SemanticHelperInstructions==0&&fixture.Statistics.InterpreterAttempts==0&&
                fixture.Statistics.ActiveDataBorrows==0&&fixture.Jit.Store.GetStatistics().ActiveNativeEpochs==0&&fixture.Jit.Store.GetStatistics().ActiveManagedLeases==0&&
                new ReadOnlySpan<byte>((byte*)fixture.Machine()+160,256).SequenceEqual(before),"R120 no Op wrappers; actual controller releases epochs/read lifetimes/exclusive ticket through RET");
        }
    }

    static void CheckFullMaskedStoreAlias()
    {
        foreach(bool xmm in new[]{false,true})
        {
            using var fixture=new MaskedFixture();const ulong pc=MaskedFixture.Base,alias=pc+0x20000;
            var file=fixture.Io.CreateMemoryFile("r120-physical-code-alias",0);Check(file.Succeeded,"R120 code alias file");
            Check(fixture.Io.TruncateFile(file.Value,4096).Succeeded,"R120 alias extent");
            fixture.MapShared(file.Value,pc);fixture.Fill(pc);fixture.MapShared(file.Value,alias);
            byte[] operation=MaskedEncoding(xmm);WriteControllerCode(fixture,pc,[..operation,0xeb,0xfe]);
            // Enroll BOTH virtual code aliases before writing the shared physical page.
            Check(fixture.Acquire(alias+256,0,out var lease,out _,out _),"R120 retained second virtual code alias");lease.Dispose();
            var i=MaskedClassify(operation);MaskedSeed((byte*)fixture.Machine(),i,alias+128,0x81);
            bool consumed=false;Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;
            fixture.NativePrepared=cap;fixture.NativeReturned=p=>{consumed|=((FullBlockFrame*)p)->MemoryFlags==FullMemoryFlags.BarrierConsumed;fixture.NativePrepared=cap;};
            fixture.Counts(1);using var deadline=new Timer(_=>fixture.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            Check(fixture.Step(pc),"R120 actual shared alias controller step");
            for(int attempt=0;fixture.Completed<1&&attempt<8;++attempt)Check(fixture.Continue(),"R120 alias bounded same issue");
            fixture.NativeReturned=null;
            Check(consumed&&fixture.Issued==1&&fixture.Completed==1&&fixture.Machine()->ip==pc+(uint)operation.Length&&
                *((byte*)fixture.Physical(pc)+128)==0x41&&*((byte*)fixture.Physical(alias)+135)==0x72&&
                fixture.Physical(pc)==fixture.Physical(alias)&&fixture.Statistics.SemanticHelperInstructions==0&&fixture.Statistics.InterpreterAttempts==0,
                "R120 all physical aliases revoked before selected byte effects; actual exclusive ticket consumed and returned postcommit");
            Check(fixture.Statistics.ActiveDataBorrows==0&&fixture.Jit.Store.GetStatistics().ActiveNativeEpochs==0&&fixture.Jit.Store.GetStatistics().ActiveManagedLeases==0,
                "R120 alias exclusive ticket and backing read lifetimes released only after RET");
        }
    }

    static void CheckFullMaskedStoreEmission()
    {
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity],scaffoldBytes=new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach(bool windows in new[]{false,true})foreach(bool shared in new[]{false,true})foreach(bool xmm in new[]{false,true})
        {
            FullBlockEmitter.EmitScaffold(scaffoldBytes,out var layout,windows);var scaffold=layout.Resolve((nint)0x10000000);
            var i=MaskedClassify(MaskedEncoding(xmm,high:true,segment:0x65,ea32:true));FullBlockInstruction[] sites=[new(i)];
            for(int warm=0;warm<8;++warm)Check(shared?FullBlockEmitter.TryEmit(sites,staging,in scaffold,out _,windows):FullBlockEmitter.TryEmit(sites,staging,out _,windows),"R120 both ABI/scaffold source-authored emission");
            long before=GC.GetAllocatedBytesForCurrentThread();bool good=true;
            for(int repeat=0;repeat<16;++repeat)good&=shared?FullBlockEmitter.TryEmit(sites,staging,in scaffold,out _,windows):FullBlockEmitter.TryEmit(sites,staging,out _,windows);
            Check(good&&GC.GetAllocatedBytesForCurrentThread()==before,"R120 warmed span emission allocates zero");
            int shortLength;Check(!(shared?FullBlockEmitter.TryEmit(sites,new byte[7],in scaffold,out shortLength,windows):FullBlockEmitter.TryEmit(sites,new byte[7],out shortLength,windows))&&shortLength==0,
                "R120 short buffer never publishes incomplete generated sparse operation");
        }
    }
}
