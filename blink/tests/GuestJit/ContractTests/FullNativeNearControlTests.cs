using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using NearFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // AUTHORED_UNRUN. One family gate; parent owns Main, builds, emitted-code,
    // controller, signal, checked-mapper and ticket execution. No CPU probe.
    static void CheckFullNativeNearControl(byte* machine)
    {
        Check(sizeof(FullBlockFrame)==416 && FullBlockFrame.AbiVersion==0x46420007 &&
            sizeof(TranslationBlockHeader)==160 && sizeof(FullStackMemoryDescriptor)==128 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.StackMemory)).ToInt32()==384,
            "R169 unchanged ABI7/frame416/header160/descriptor128");
        using(var oracle=new NearFixture(interpreterOracle:true)) { NearClassifier(oracle.Program); NearCommonOracle(oracle); }
        NearBranchLeaves(machine);
        using(var common=CreateFullScaffold(out var addresses))
        {
            fullOwnerScaffold=addresses;
            try { NearBranchLeaves(machine); }
            finally { fullOwnerScaffold=default; }
        }
        CheckFullStackMemoryComputed(machine); CheckFullStackMemoryAuthentication(machine); CheckFullStackMemoryTicketTuples(machine);
        NearMemoryController(); NearFaultController(); NearControllerAdmission();
        NearPairMapper(); NearMappedFaultLeaves(); NearTicketController(); NearEmission(); ResetFull(machine);
    }

    static JitInstruction NearClassify(ulong ip,byte[] bytes)
    {
        Check(FullNearControlLowering.TryClassify(ip,bytes,out var i,out string? reason),reason??"R169 near classify");
        Check(FullBlockEmitter.CanInlineNearControl(i)&&FullBlockEmitter.ValidateNearControl(i),"R169 strict near IR");
        return i;
    }
    static byte[][] NearEncodings() => [
        [0xeb,0x20],[0xe9,0x20,0,0,0],[0xe8,0x20,0,0,0],
        [0x66,0xe8,0x20,0,0,0],[0x66,0xe9,0x20,0,0,0],[0x66,0x0f,0x84,0x20,0,0,0],
        [0x66,0x48,0xe8,0x20,0,0,0],[0x66,0x48,0xe9,0x20,0,0,0],
        [0x66,0x48,0x0f,0x85,0x20,0,0,0],[0xc3],[0xc2,0xff,0xff],
        [0x66,0xc3],[0x66,0xc2,0x34,0x12],[0x66,0x48,0xc3],[0x66,0x48,0xc2,0x34,0x12],[0xf3,0xc3],[0xf3,0xc2,8,0],
        [0xff,0xe0],[0xff,0xd0],[0xff,0x23],[0xff,0x13],[0xff,0x14,0x24],
        [0x66,0xff,0x13],[0x67,0xff,0x13],[0x64,0x67,0xff,0x54,0x8b,0xf8],
        [0x65,0x4b,0xff,0x54,0xcc,0x80],[0x67,0xff,0x15,0xf0,0xff,0xff,0xff],
        [0xff,0x14,0x25,0,0x10,0,0],[0xff,0x55,0x80],
        [0x74,0x20],[0x0f,0x84,0x20,0,0,0],[0xe0,0x20],[0xe1,0x20],[0xe2,0x20],[0xe3,0x20],
        [0x66,0x67,0x4f,0xe0,0x80],[0x67,0xe3,0x20]];

    static void NearClassifier(Blink p)
    {
        foreach(var bytes in NearEncodings())
        {
            Blink.XedDecodedInst x=default;
            fixed(byte* b=bytes) Check(p.DecodeInstruction(&x,b,(ulong)bytes.Length,Blink.XED_MODE_LONG)==0&&
                x.length==bytes.Length,"R169 authoritative exact long-mode span");
            var i=NearClassify(Ip,bytes);
            for(int cut=0;cut<bytes.Length;++cut)
                Check(!FullNearControlLowering.TryClassify(Ip,bytes.AsSpan(0,cut),out _,out _),"R169 truncated span rejects");
            Check(!FullNearControlLowering.TryClassify(Ip,[..bytes,0x90],out _,out _),"R169 trailing bytes reject");
            foreach(var bad in new[]{i with{Length=0},i with{Length=16},i with{Width=8},i with{Options=1},
                i with{Destination=new(1,0)},i with{Extra=new(1,0)},i with{SourceWidth=8}})
            {
                bool rejected=false;try{FullBlockEmitter.ValidateNearControl(bad);}catch(ArgumentException){rejected=true;}
                Check(rejected,"R169 malformed family IR cannot fall through an older validator");
            }
        }
        foreach(byte[] prefix in new byte[][]{[],[0x66],[0x67],[0x2e],[0x64,0x67],[0x67,0x65,0x66]})
        for(int rex=-1;rex<16;++rex)
        foreach(byte opcode in new byte[]{0xe0,0xe1,0xe2,0xe3,0x74,0xeb})
        {
            byte[] b=[..prefix,..(rex<0?Array.Empty<byte>():new[]{(byte)(0x40+rex)}),opcode,0x80];
            var i=NearClassify(Ip,b);
            Check(i.Target==unchecked(Ip+(uint)b.Length-128)&&
                i.Width==((opcode is >=0xe0 and <=0xe3)&&prefix.Contains((byte)0x67)?32:64),
                "R169 full RIP rel8 sign extension; ignored66/segment/REX; only67 selects counter32");
        }
        for(int r=0;r<16;++r)foreach(int extension in new[]{2,4})foreach(int ignoredRex in new[]{0,4,8,12})
        {
            byte[] b=[(byte)(0x40|ignoredRex|(r>>3)),0xff,(byte)(0xc0|(extension<<3)|(r&7))];
            var i=NearClassify(Ip,b);Check(i.Source.Register==r&&i.Width==64,
                "R169 all register aliases and ignored opcode-extension REX.R/W");
        }
        foreach(byte[] b in new byte[][]{[],[0x48],[0xff],[0xff,0xd8],[0xff,0xe8],[0xff,0xf8],
            [0x9a,0,0,0,0,0,0],[0xea,0,0,0,0,0,0],[0xcb],[0xca,0,0],[0xcf],[0xcd,0x80],
            [0x0f,0x05],[0xf0,0xeb,0],[0xf2,0xff,0xd0],[0xf3,0xeb,0],[0x66,0x66,0xeb,0],
            [0x67,0x67,0xe2,0],[0x64,0x65,0xff,0x13],[0x48,0x66,0xeb,0],[0x40,0x40,0xeb,0],
            [0x66,0xe8,0,0],[0x66,0xe9,0,0],[0x66,0x0f,0x84,0,0]})
            Check(!FullNearControlLowering.TryClassify(Ip,b,out _,out string? reason)&&reason!=null,
                "R169 far/system/LOCK/BND/undefined aliases/vendor-rel16 spans remain explicitly outside current profile");
        Check(NearClassify(0x7ffffffffffe,[0xe2,1]).Target==0x800000000001,
            "R169 noncanonical static target is native fault data, never compile-time MISSING");
        for(int cc=0;cc<16;++cc)
        {
            Check(NearClassify(Ip,[(byte)(0x70+cc),0x80]).Condition==cc&&
                NearClassify(Ip,[0x0f,(byte)(0x80+cc),0xff,0xff,0xff,0xff]).Condition==cc,
                "R169 every rel8/rel32 condition encoding");
        }
    }

    // Independent mathematical model: unsigned wrapping and direct condition
    // Boolean algebra. Parity uses popcount of the software parity input byte.
    static bool NearConditionModel(int cc,uint flags)
    {
        bool c=(flags&1)!=0,z=(flags&0x40)!=0,s=(flags&0x80)!=0,o=(flags&0x800)!=0;
        bool p=(BitOperations.PopCount(flags>>24)&1)==0;
        return cc switch{0=>o,1=>!o,2=>c,3=>!c,4=>z,5=>!z,6=>c||z,7=>!c&&!z,
            8=>s,9=>!s,10=>p,11=>!p,12=>s!=o,13=>s==o,14=>z||s!=o,_=>!z&&s==o};
    }
    static bool NearCanonicalModel(ulong target) => target<=0x7fffffffffffUL||target>=0xffff800000000000UL;
    static (byte[] Machine,bool Fault) NearBranchModel(byte[] before,JitInstruction i)
    {
        var after=(byte[])before.Clone();ulong next=unchecked(i.Ip+(uint)i.Length);
        ulong cx=BinaryPrimitives.ReadUInt64LittleEndian(before.AsSpan(32));
        uint flags=BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12));
        bool take=true,writeCounter=false;
        ulong result=cx;
        if(i.Operation==JitOperation.ConditionalJump)take=NearConditionModel(i.Condition,flags);
        else if(i.Operation==JitOperation.CounterZeroJump)take=(i.Width==32?(uint)cx:cx)==0;
        else if(FullBlockEmitter.IsNativeCounterBranch(i))
        {
            writeCounter=true;result=i.Width==32?unchecked((uint)cx-1):unchecked(cx-1);
            take=result!=0&&(i.Operation==JitOperation.Loop||
                i.Operation==JitOperation.LoopEqual&&(flags&0x40)!=0||
                i.Operation==JitOperation.LoopNotEqual&&(flags&0x40)==0);
        }
        ulong target=i.Operation==JitOperation.IndirectJump?
            BinaryPrimitives.ReadUInt64LittleEndian(before.AsSpan(24+i.Source.Register*8)):i.Target;
        if(take&&!NearCanonicalModel(target))return(after,true); // precise fault: counter beforeimage
        if(writeCounter)BinaryPrimitives.WriteUInt64LittleEndian(after.AsSpan(32),result);
        BinaryPrimitives.WriteUInt64LittleEndian(after,take?target:next);after[8]=0;
        return(after,false);
    }
    static void NearCommonOracle(NearFixture f)
    {
        // Proven common domain only: canonical targets, ordinary64 branches,
        // current-profile ignored66 rel32, no faults, no stack effects, no TF/RF/AC.
        foreach(byte[] b in new byte[][]{[0xeb,0x20],[0xe9,0x20,0,0,0],[0x74,0x20],
            [0x0f,0x85,0x20,0,0,0],[0xe0,0x20],[0xe1,0x20],[0xe2,0x20],[0xe3,0x20],
            [0x67,0xe0,0x20],[0x67,0xe2,0x20],[0x67,0xe3,0x20],
            [0x66,0xe9,0x20,0,0,0],[0x66,0x0f,0x84,0x20,0,0,0]})
        foreach(ulong cx in new[]{0UL,1UL,2UL,0xfeed000000000002UL})
        foreach(uint flags in new[]{0u,0x40u})
        {
            f.At(Ip);var m=f.Machine();ControlFlagSeed((byte*)m,flags);m->oplen=0;Write64((byte*)m,32,cx);
            var i=NearClassify(Ip,b);byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            var expected=NearBranchModel(before,i);Check(!expected.Fault,"R169 oracle common-domain guard");
            f.DataScope(0,()=>ControlFlagReference(f.Program,(byte*)f.Machine(),b));
            Check(m->ip==BinaryPrimitives.ReadUInt64LittleEndian(expected.Machine)&&m->oplen==0&&
                new ReadOnlySpan<byte>((byte*)m+12,4).SequenceEqual(expected.Machine.AsSpan(12,4))&&
                new ReadOnlySpan<byte>((byte*)m+24,128).SequenceEqual(expected.Machine.AsSpan(24,128)),
                "R169 selected GetOp only on established canonical branch common domain");
        }
    }

    static void NearBranchLeaves(byte* m)
    {
        foreach(ulong ip in new[]{Ip,0x7ffffffffffeUL,0xffff800000000000UL})
        foreach(byte op in new byte[]{0xe0,0xe1,0xe2,0xe3,0x74,0xeb})
        foreach(bool a32 in new[]{false,true})foreach(byte rel in new byte[]{0,1,0x7f,0x80,0xff})
        {
            var b=a32?new byte[]{0x67,op,rel}:new byte[]{op,rel};var i=NearClassify(ip,b);
            using var code=CreateFullBlock([new(i)]);
            foreach(ulong cx in new[]{0UL,1UL,2UL,0x100000000UL,0xfeed000000000002UL,ulong.MaxValue})
            foreach(uint flags in new uint[]{0,0x40,0x800,0x01000081})
                NearLeaf(m,code,i,cx,flags);
        }
        for(int cc=0;cc<16;++cc)foreach(bool longForm in new[]{false,true})foreach(bool word in new[]{false,true})
        {
            byte[] raw=longForm?[0x0f,(byte)(0x80+cc),0x7f,0,0,0]:[(byte)(0x70+cc),0x7f];
            byte[] b=word?[0x66,..raw]:raw;
            var i=NearClassify(0x7ffffffffffe,b);using var code=CreateFullBlock([new(i)]);
            for(uint mask=0;mask<32;++mask)
            {
                uint flags=(mask&1)|((mask&2)<<5)|((mask&4)<<5)|((mask&8)<<8)|((mask&16)<<20);
                NearLeaf(m,code,i,0xfedcba9876543210,flags);
            }
        }
        for(int r=0;r<16;++r)foreach(ulong target in new[]{0UL,Ip+32,0x7fffffffffffUL,
            0xffff800000000000UL,0x800000000000UL,0xffff7fffffffffffUL,ulong.MaxValue})
        {
            var i=NearClassify(Ip,[(byte)(0x40|(r>>3)),0xff,(byte)(0xe0|(r&7))]);
            using var code=CreateFullBlock([new(i)]);ResetFull(m);Write64(m,24+r*8,target);
            NearLeafPrepared(m,code,i);
        }
        NearLeafAdmission(m);
    }
    static void NearLeaf(byte* m,NativeCodeAllocation code,JitInstruction i,ulong cx,uint flags)
    {
        ResetFull(m,i.Ip);m[8]=0;
        ((Blink.Machine*)m)->flags=flags&~0x50100u;Write64(m,32,cx);
        NearLeafPrepared(m,code,i);
    }
    static void NearLeafPrepared(byte* m,NativeCodeAllocation code,JitInstruction i)
    {
        byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
        var expected=NearBranchModel(before,i);ulong epoch=11;long issued=0,completed=0;int stop=0;
        var h=FullHeader(i.Ip,code.Entry);var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop,limit:1);
        var exit=RunFull(code,&f);
        Check(exit==(expected.Fault?FullBlockExit.GuestFault:FullBlockExit.Continue)&&issued==1&&
            completed==(expected.Fault?0:1)&&f.Executed==(expected.Fault?0UL:1UL)&&f.MemoryAssists==0&&
            f.HelperIndex==FullBlockFrame.NoHelperIndex,"R169 exactly one issue; precise completion and no semantic helper");
        Check(!expected.Fault||(f.FaultTrap==13&&f.FaultCode==128&&f.FaultAddress==0),"R169 target GP13/code128/address0");
        Check(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(expected.Machine),
            "R169 whole machine beforeimage on faults; independent full-machine successful branch model");
    }
    static void NearLeafAdmission(byte* m)
    {
        var i=NearClassify(Ip,[0x75,0x20]);using var code=CreateFullBlock([new(i)]);
        foreach(int scenario in new[]{0,1,2,3,4})foreach(uint preissue in new[]{0u,1u})
        {
            ResetFull(m);m[8]=0;byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=preissue,completed=0;int stopped=scenario==1?1:0;
            var h=FullHeader(Ip,code.Entry);var f=FullFrame(m,&h,&epoch,&issued,&completed,&stopped,
                budget:scenario==0?0UL:100UL,limit:1);f.FirstPreissued=preissue;
            if(scenario==2)((Blink.Machine*)m)->attention=true;
            if(scenario==3){f.Header->Live=0;}
            if(scenario==4){f.Context^=1;}
            var exit=RunFull(code,&f);
            bool runs=(scenario==0||scenario==2)&&preissue==1; // approved cold issue runs before attention; stop still wins
            Check(completed==(runs?1:0)&&issued==(scenario==0?1:preissue),
                $"R169 boundary scenario={scenario} preissued={preissue} exit={exit} issued={issued} completed={completed} runs={runs}");
            Check(runs||(((ulong*)m)[0]==Ip&&f.Executed==0),"R169 polls precede native target and all effects");
            Check(runs||exit is FullBlockExit.Stop or FullBlockExit.Attention or FullBlockExit.Invalidated or FullBlockExit.Budget,
                "R169 actual boundary exit class");
        }
    }

    static byte[] NearReadPages(NearFixture f,params ulong[] pages)
    {
        var bytes=new byte[pages.Length*4096];
        for(int n=0;n<pages.Length;++n)new ReadOnlySpan<byte>((void*)f.Physical(pages[n]),4096).CopyTo(bytes.AsSpan(n*4096));
        return bytes;
    }
    static void NearWrite64(NearFixture f,ulong ea,ulong value)
    {
        f.DataScope(0,()=>
        {
            for(int n=0;n<8;++n)
            {
                byte* p=f.Program.LookupAddress2(f.Machine(),unchecked((long)(ea+(uint)n)),0,0);
                Check(p!=null,"R169 source seed checked mapping");*p=(byte)(value>>(n*8));
            }
        });
    }
    static void NearArchitecturalBeforeimage(byte[] before,Blink.Machine* m)
    {
        byte* b=(byte*)m;
        foreach((int Start,int Length) r in new[]{(12,4),(24,128),(160,256),(456,236)})
            Check(new ReadOnlySpan<byte>(b+r.Start,r.Length).SequenceEqual(before.AsSpan(r.Start,r.Length)),
                "R169 controller fault preserves complete GPR/flags/vector/x87 architectural beforeimage");
    }
    static void NearCap(NearFixture f)
    {
        Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;
        f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;
    }
    static void NearNoCapabilities(NearFixture f)
    {
        var s=f.Jit.Store.GetStatistics();
        Check(s.ActiveNativeEpochs==0&&s.ActiveManagedLeases==0&&f.Statistics.ActiveDataBorrows==0&&
            f.Statistics.SemanticHelperInstructions==0&&f.Statistics.SemanticHelperCalls==0&&f.Statistics.InterpreterAttempts==0,
            "R169 no native/store/passive capabilities survive return; no semantic fallback");
    }
    static void NearMemoryController()
    {
        // All supported target source forms, EA widths, RSP aliases, crosspage,
        // overlap, unsigned RETimm16 and independent stack64 expectations.
        foreach(byte[] bytes in NearEncodings().Where(b=>b.Contains((byte)0xff)||b[0] is 0xc2 or 0xc3||
            b.Length>1&&b[0]==0x66&&b[1] is 0xc2 or 0xc3 or 0xe8||
            b.Length>2&&b[0]==0x66&&b[1]==0x48&&b[2] is 0xc2 or 0xc3 or 0xe8||b[0]==0xe8))
        foreach(bool cross in new[]{false,true})
        {
            using var f=new NearFixture();ulong pc=NearFixture.Base,source=pc+0x20000,stack=pc+0x60000;
            f.Map(pc);f.Fill(pc);f.Map(source,8192);f.Fill(source,8192);f.Map(stack,8192);f.Fill(stack,8192);
            WriteControllerCode(f,pc,bytes);var i=NearClassify(pc,bytes);var m=f.Machine();
            ulong target=pc+128,oldRsp=stack+(cross?4103UL:256UL),ea=source+(cross?4095UL:64UL);
            // For arbitrary encodings recompute source EA from an independently
            // chosen register seed; adjust mapping by using only encodable rows.
            for(int r=0;r<16;++r)Write64((byte*)m,24+r*8,0);
            Write64((byte*)m,56,oldRsp);
            if(i.Source.IsMemory)
            {
                if(i.Address.RipRelative||i.Address.Base<0||i.Address.Index==4||i.Address.Base==4) continue;
                ulong baseValue=unchecked(ea-(ulong)i.Address.Displacement);
                Write64((byte*)m,24+i.Address.Base*8,baseValue);
                if(i.Address.Index>=0)Write64((byte*)m,24+i.Address.Index*8,0);
                if(i.Address.Segment==4)m->fs.@base=0;
                if(i.Address.Segment==5)m->gs.@base=0;
                NearWrite64(f,ea,target);
            }
            else if(i.Operation is JitOperation.IndirectCall or JitOperation.IndirectJump)
            { if(i.Source.Register==4)target=oldRsp;else Write64((byte*)m,24+i.Source.Register*8,target); }
            else if(i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust)
            { target=i.Width==16?target&0xffff:target;oldRsp=stack+(cross?4095UL:256UL);Write64((byte*)m,56,oldRsp);NearWrite64(f,oldRsp,target); }
            else if(i.Operation==JitOperation.Call)target=i.Target;
            byte[] pages=NearReadPages(f,source,source+4096,stack,stack+4096);
            uint flags=m->flags;NearCap(f);f.Counts(100);
            Check(f.Step(pc),"R169 controller canonical near operation");
            for(int n=0;n<8&&f.Completed==0;++n)Check(f.Continue(),"R169 same-issued checked memory continuation");
            ulong expectedRsp=i.Operation is JitOperation.Call or JitOperation.IndirectCall?oldRsp-8:
                i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust?oldRsp+(uint)(i.Width/8)+
                (i.Operation==JitOperation.ReturnAdjust?i.Source.Immediate:0):oldRsp;
            Check(f.Issued==1&&f.Completed==1&&m->ip==target&&m->oplen==0&&m->flags==flags&&
                ((ulong*)((byte*)m+56))[0]==expectedRsp,"R169 independent CALL/RET/JMP target, stack64 and pop/adjust math");
            byte[] expected=(byte[])pages.Clone();
            if(i.Operation is JitOperation.Call or JitOperation.IndirectCall)
                BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(8192+(int)(oldRsp-stack)-8),pc+(uint)bytes.Length);
            Check(NearReadPages(f,source,source+4096,stack,stack+4096).AsSpan().SequenceEqual(expected),
                "R169 complete source/destination beforeimage with sole exact8 return-address store");
            NearNoCapabilities(f);
        }
        NearRspOverlapController(); NearAddressController();
    }

    static void NearRspOverlapController()
    {
        foreach(byte[] bytes in new byte[][]{[0xff,0x14,0x24],[0xff,0x54,0x24,0xf8],[0xff,0xd4]})
        {
            using var f=new NearFixture();ulong pc=NearFixture.Base,stack=pc+0x20000,oldRsp=stack+4095;
            f.Map(pc);f.Fill(pc);f.Map(stack,8192);f.Fill(stack,8192);WriteControllerCode(f,pc,bytes);
            Write64((byte*)f.Machine(),56,oldRsp);ulong target=bytes[^1]==0xd4?oldRsp:pc+128;
            if(bytes[^1]!=0xd4)NearWrite64(f,bytes.Length==4?oldRsp-8:oldRsp,target);
            byte[] before=NearReadPages(f,stack,stack+4096);NearCap(f);f.Counts(100);
            Check(f.Step(pc),"R169 CALL RSP/[RSP]/[RSP-8] checked controller");
            for(int n=0;n<8&&f.Completed==0;++n)Check(f.Continue(),"R169 overlap same-issue resume");
            var expected=(byte[])before.Clone();BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(4087),pc+(uint)bytes.Length);
            Check(f.Completed==1&&f.Issued==1&&f.Machine()->ip==target&&((ulong*)((byte*)f.Machine()+56))[0]==oldRsp-8&&
                NearReadPages(f,stack,stack+4096).AsSpan().SequenceEqual(expected),
                "R169 target source captured before overlapping return store; CALL RSP uses original alias");
            NearNoCapabilities(f);
        }
    }

    static void NearAddressController()
    {
        foreach(int shape in new[]{0,1,2,3,4,5,6})foreach(bool call in new[]{false,true})
        {
            using var f=new NearFixture();ulong pc=NearFixture.Base,source=pc+0x20000,stack=pc+0x60000;
            f.Map(pc);f.Fill(pc);f.Map(source,8192);f.Fill(source,8192);f.Map(stack,8192);f.Fill(stack,8192);
            byte extension=call?(byte)0x10:(byte)0x20;
            byte[] bytes=shape switch{
                0=>[0x64,0x67,0xff,(byte)(extension|3)],
                1=>[0x65,0x67,0xff,(byte)(extension|3)],
                2=>[0xff,(byte)(extension|5),0,0,0,0],
                3=>[0x67,0xff,(byte)(extension|5),0,0,0,0],
                4=>[0x43,0xff,(byte)(extension|4),0x24], // [r12+r12], real REX.X index12
                5=>[0xff,(byte)(extension|4),0x25,0,0,0,0],
                _=>[0x67,0xff,(byte)(extension|4),0x24]}; // [esp], implicit stack stillRSP64
            var m=f.Machine();ulong oldRsp=stack+256,ea=source+4095,target=pc+128;
            Write64((byte*)m,56,oldRsp);
            if(shape is 0 or 1)
            {
                Write64((byte*)m,48,0xfedcba9800001000UL);
                if(shape==0)m->fs.@base=ea-0x1000;else m->gs.@base=ea-0x1000;
            }
            else if(shape is 2 or 3)
                BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length-4),checked((int)(ea-(pc+(uint)bytes.Length))));
            else if(shape==4)
            {ea=source+4094;Write64((byte*)m,24+12*8,ea/2);}
            else if(shape==5)BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length-4),checked((int)ea));
            else
            { oldRsp=stack+4095;Write64((byte*)m,56,oldRsp);ea=oldRsp; }
            NearWrite64(f,ea,target);WriteControllerCode(f,pc,bytes);NearClassify(pc,bytes);
            byte[] before=NearReadPages(f,source,source+4096,stack,stack+4096);uint flags=m->flags;
            NearCap(f);f.Counts(100);Check(f.Step(pc),"R169 FS/GS EA32, RIP/EIP, SIB R12, absolute, [ESP] controller");
            for(int n=0;n<8&&f.Completed==0;++n)Check(f.Continue(),"R169 address-form same-issue checked resume");
            Check(f.Issued==1&&f.Completed==1&&m->ip==target&&m->flags==flags&&
                ((ulong*)((byte*)m+56))[0]==(call?oldRsp-8:oldRsp),
                "R169 independent EA32 truncation before full FS/GS and implicit stack64 arithmetic");
            var expected=(byte[])before.Clone();if(call)BinaryPrimitives.WriteUInt64LittleEndian(
                expected.AsSpan(8192+(int)(oldRsp-stack)-8),pc+(uint)bytes.Length);
            Check(NearReadPages(f,source,source+4096,stack,stack+4096).AsSpan().SequenceEqual(expected),
                "R169 all source/address forms preserve all memory except exact CALL return slot");
            NearNoCapabilities(f);
        }
    }

    static void NearFaultController()
    {
        // Parent AMD matrix v2: read source > stack permissions/canonical >
        // target canonical. This is a finite software profile, no Intel claim.
        foreach(byte[] bytes in new byte[][]{[0xff,0xd0],[0xff,0x13],[0x36,0xff,0x13],
            [0xc3],[0xc2,0xff,0xff],[0xff,0x23],[0x36,0xff,0x23]})
        foreach(int scenario in new[]{0,1,2,3,4,5,6,7})
        {
            using var f=new NearFixture();ulong pc=NearFixture.Base,source=pc+0x20000,stack=pc+0x60000;
            f.Map(pc);f.Fill(pc);f.Map(source,8192);f.Fill(source,8192);f.Map(stack,8192);f.Fill(stack,8192);
            WriteControllerCode(f,pc,bytes);var i=NearClassify(pc,bytes);var m=f.Machine();
            bool call=i.Operation==JitOperation.IndirectCall,ret=i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust;
            ulong target=0x800000000000UL,read=source+4095,oldRsp=stack+256;
            Write64((byte*)m,24,target);Write64((byte*)m,48,read);Write64((byte*)m,56,oldRsp);
            NearWrite64(f,read,target);NearWrite64(f,oldRsp,target);
            byte[] pages=NearReadPages(f,source,source+4096,stack,stack+4096);
            uint trap=13,code=128;ulong address=0;
            if(scenario==1) // readonly stack: CALL PFwrite wins, RET still targetGP
            {f.DataScope(0,()=>Check(f.Program.ProtectVirtual(f.Machine()->blink_host_system,(long)stack,4096,1,false)==0,
                "R169 stack PROT_READ preserves RET reads and denies CALL writes"));if(call){trap=14;code=2;address=oldRsp-8;}}
            if(scenario==2) // unmapped stack: RET read PF; CALL write PF
            {Check(f.Mutate("munmap",stack)==0,"R169 absent stack setup");if(call||ret){trap=14;code=1;address=ret?oldRsp:oldRsp-8;}}
            if(scenario==3)
            {oldRsp=0x800000000008UL;Write64((byte*)m,56,oldRsp);if(call||ret){trap=12;code=128;address=0;}}
            if(scenario==4&&i.Source.IsMemory)
            {read=source+8192;Write64((byte*)m,48,read);trap=14;code=1;address=read;}
            if(scenario==5&&i.Source.IsMemory)
            {read=0x800000000000UL;Write64((byte*)m,48,read);trap=i.Address.Segment==2?12u:13u;code=128;address=0;}
            if(scenario==6&&i.Source.IsMemory)
            {
                read=source+8192;Write64((byte*)m,48,read);Write64((byte*)m,56,0x800000000008UL);
                oldRsp=0x800000000008UL;trap=14;code=1;address=read;
            }
            if(scenario==7&&i.Source.IsMemory)
                f.DataScope(0,()=>Check(f.Program.ProtectVirtual(f.Machine()->blink_host_system,(long)(source+4096),4096,1,false)==0,
                    "R169 crosspage source PROT_READ is valid for target read"));
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();f.Counts(100);
            var signal=f.StepSynchronousFault(pc);
            Check(m->trapno==trap&&m->faultaddr==(long)address&&m->ip==pc&&m->oplen==0&&
                signal.Signal==(trap==12?7:11)&&signal.Detail?.EndsWith($"signal={(trap==12?7:11)} code={code}")==true,
                $"R169 controller bytes={Convert.ToHexString(bytes)} scenario={scenario} expected={trap}/{code}/{address:x} actual={m->trapno}/{m->faultaddr:x} ip={m->ip:x} oplen={m->oplen} signal={signal.Signal} detail={signal.Detail}");
            Check(f.Issued==1&&f.Completed==0&&((ulong*)((byte*)m+56))[0]==oldRsp,
                "R169 fault consumes one issue, no completion, no premature RSP update");
            NearArchitecturalBeforeimage(before,m);
            // Missing stack page cannot be read back; all mapped source pages
            // and the still-mapped second stack page remain exact beforeimages.
            Check(NearReadPages(f,source,source+4096).AsSpan().SequenceEqual(pages.AsSpan(0,8192))&&
                NearReadPages(f,stack+4096).AsSpan().SequenceEqual(pages.AsSpan(12288,4096)),
                "R169 full accessible memory beforeimage on architectural faults");
            if(scenario!=2)Check(NearReadPages(f,stack).AsSpan().SequenceEqual(pages.AsSpan(8192,4096)),
                "R169 invalid target never writes any return slot bytes");
            NearNoCapabilities(f);
        }
    }

    static void NearControllerAdmission()
    {
        foreach(byte[] bytes in new byte[][]{[0xff,0xe0],[0xff,0xd0],[0xc3],[0xc2,8,0],[0xe2,0x20],[0x75,0x20]})
        foreach(int boundary in new[]{0,1,2})
        {
            using var f=new NearFixture();ulong pc=NearFixture.Base,stack=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(stack);f.Fill(stack);WriteControllerCode(f,pc,bytes);
            Write64((byte*)f.Machine(),24,pc+128);Write64((byte*)f.Machine(),32,2);Write64((byte*)f.Machine(),56,stack+128);
            NearWrite64(f,stack+128,pc+128);byte[] before=NearReadPages(f,stack);
            NearCap(f);f.Counts(1);
            f.Preissued=(_,_)=>{if(boundary==1)f.Stop.RequestStop();if(boundary==2)f.Machine()->attention=true;};
            Check(f.Step(pc),"R169 real controller preissue/budget/stop/attention boundary");
            Check(f.Issued==1&&f.Completed==(boundary==1?0UL:1UL),"R169 actual preissued operation executes at most once");
            if(boundary==1)Check(f.Machine()->ip==pc&&NearReadPages(f,stack).AsSpan().SequenceEqual(before),
                "R169 stopped preissued CALL/RET makes no target or stack effects");
            NearNoCapabilities(f);
        }
    }

    static void NearPairMapper()
    {
        using var f=new NearFixture();ulong source=NearFixture.Base,stack=source+0x40000;
        f.Map(source,8192);f.Fill(source,8192);f.Map(stack,8192);f.Fill(stack,8192);
        var i=NearClassify(Ip,[0xff,0x13]);var m=f.Machine();m->ip=Ip;
        Write64((byte*)m,48,source+4095);Write64((byte*)m,56,stack+4103);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        var d=StackMemoryRequest(source+4095,stack+4095);var frame=StackMemoryMmuRequest(&d,tlb);
        frame.Machine=m;frame.FsBase=m->fs.@base;frame.GsBase=m->gs.@base;
        byte[] before=NearReadPages(f,source,source+4096,stack,stack+4096);
        f.Jit.BeginRead(f.Program,false);
        try
        {
            lock(f.Jit.Authority.Gate)
            {
                frame.ObservedGlobalEpoch=f.Jit.Authority.Epoch;
                Check(FullNativeMemory.AuthenticateNearCall(&frame,in i,m)&&
                    FullNativeMemory.TryFillNearCall(f.Program,m,&frame,f.Jit.Authority,in i)==FullMemoryAssistResult.Resolved&&
                    d.Request0.Host0!=null&&d.Request0.Host1!=null&&d.Request1.Host0!=null&&d.Request1.Host1!=null,
                    "R169 checked mapper independently retains four collision-safe pointers");
                var good=d;d.State=FullStackMemoryState.Request;d.Request0.Address++;
                Check(!FullNativeMemory.AuthenticateNearCall(&frame,in i,m),"R169 mutated source EA fails exact-plan authentication");
                d=good;d.State=FullStackMemoryState.Request;frame.Context^=1;
                // Header/context are authenticated by controller/native guard,
                // never stamped into a mapping pointer as an invented TLB proof.
                frame.Context^=1;d.Request1.Address++;
                Check(!FullNativeMemory.AuthenticateNearCall(&frame,in i,m),"R169 mutated stack EA fails exact-plan authentication");
                d=good;
            }
        }
        finally{f.Jit.EndRead(f.Program);}
        Check(NearReadPages(f,source,source+4096,stack,stack+4096).AsSpan().SequenceEqual(before),
            "R169 checked mapping adapter never loads target or writes return bytes");
    }

    static void NearMappedFaultLeaves()
    {
        // Whole Machine + all mapped bytes, with REAL checked mapper entries.
        // Dummy standalone header is the usual leaf-ABI fixture, never a fake
        // TLB. Passive lifetime, store epoch and native admission cover access.
        foreach(byte[] bytes in new byte[][]{[0xff,0xd0],[0xff,0x13],[0xc3],[0xc2,0xff,0xff],[0xff,0x23]})
        {
            using var f=new NearFixture();ulong source=NearFixture.Base,stack=source+0x40000;
            f.Map(source,8192);f.Fill(source,8192);f.Map(stack,8192);f.Fill(stack,8192);
            f.At(Ip);var m=f.Machine();ControlFlagSeed((byte*)m,0x5a0008d5);m->oplen=0;
            Write64((byte*)m,24,0x800000000000UL);Write64((byte*)m,48,source+4095);
            bool returns=bytes[0] is 0xc2 or 0xc3;
            ulong oldRsp=stack+(returns?4095UL:4103UL);Write64((byte*)m,56,oldRsp);
            NearWrite64(f,source+4095,0x800000000000UL);NearWrite64(f,oldRsp,0x800000000000UL);
            var i=NearClassify(Ip,bytes);using var code=CreateFullBlock([new(i)]);
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            byte[] memory=NearReadPages(f,source,source+4096,stack,stack+4096);
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            var d=new FullStackMemoryDescriptor{Version=1};long issued=0,completed=0;int stop=0;
            ulong runtime=f.Jit.Authority.Epoch,codeEpoch=runtime;var h=FullHeader(Ip,code.Entry);h.ValidatedGlobalEpoch=codeEpoch;
            var frame=FullFrame((byte*)m,&h,&runtime,&issued,&completed,&stop,limit:1,codeEpoch:&codeEpoch);
            frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;frame.StackMemory=&d;
            frame.FsBase=m->fs.@base;frame.GsBase=m->gs.@base;
            f.Jit.BeginRead(f.Program,false);
            try
            {
                Check(f.Jit.Store.TryBeginNativeEpoch(out var nativeEpoch),"R169 fault-leaf real store epoch");
                using(nativeEpoch)
                {
                    bool fault=false;
                    for(int attempt=0;attempt<4;++attempt)
                    {
                        Check(f.Jit.Authority.TryEnterNative(runtime),"R169 fault-leaf real native admission");
                        FullBlockExit exit;
                        byte[] finalBefore=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                        try{exit=RunFull(code,&frame);}finally{f.Jit.Authority.ExitNative();}
                        if(exit==FullBlockExit.GuestFault)
                        {
                            Check(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(finalBefore),
                                "R169 exact full-Machine beforeimage of final faulting native attempt (after mapping housekeeping)");
                            fault=true;break;
                        }
                        Check(exit==FullBlockExit.NeedMemory,"R169 target-fault leaf only maps before fault");
                        lock(f.Jit.Authority.Gate)
                        {
                            f.Program.ResetTlb(m);
                            var result=i.Operation==JitOperation.IndirectCall&&i.Source.IsMemory?
                                FullNativeMemory.TryFillNearCall(f.Program,m,&frame,f.Jit.Authority,in i):
                                FullNativeMemory.TryFill(f.Program,m,&frame,f.Jit.Authority);
                            Check(result==FullMemoryAssistResult.Resolved,"R169 actual checked mapper permission before target fault");
                        }
                        FullNativeMemory.ResumeSameIssue(&frame);
                    }
                    Check(fault&&issued==1&&completed==0&&frame.FaultTrap==13&&frame.FaultCode==128&&
                        frame.FaultAddress==0&&m->ip==Ip&&m->oplen==0&&
                        NearReadPages(f,source,source+4096,stack,stack+4096).AsSpan().SequenceEqual(memory),
                        "R169 CALL/RET/RETimm/memoryJMP exact full-memory target-fault beforeimages");
                    NearArchitecturalBeforeimage(before,m);
                }
            }
            finally{f.Jit.EndRead(f.Program);}
        }
    }

    static void NearTicketController()
    {
        foreach(bool invalid in new[]{false,true})
        {
            using var f=new NearFixture();ulong pc=NearFixture.Base,source=pc+8192,slot=pc+256;
            f.Map(pc);f.Fill(pc);f.Map(source,8192);f.Fill(source,8192);
            WriteControllerCode(f,pc,[0xff,0x13]);Write64((byte*)f.Machine(),48,source+4095);
            Write64((byte*)f.Machine(),56,slot+8);NearWrite64(f,source+4095,invalid?0x800000000000UL:pc+128);
            byte[] before=NearReadPages(f,pc,source,source+4096);bool consumed=false;
            NearCap(f);f.NativeReturned=pointer=>
            {
                var frame=(FullBlockFrame*)pointer;
                if(frame->MemoryFlags==FullMemoryFlags.BarrierConsumed)
                {
                    consumed=true;Check(f.Jit.Store.GetStatistics().ActiveNativeEpochs>0&&
                        f.Statistics.ActiveDataBorrows>0,"R169 real ticket/exclusive/source lifetime retained through native RET");
                }
                f.NativePrepared=p=>((FullBlockFrame*)p)->NativeLimit=1;
            };
            f.Counts(100);
            if(invalid)
            {
                var signal=f.StepSynchronousFault(pc);
                Check(signal.Signal==11&&f.Machine()->trapno==13&&f.Issued==1&&f.Completed==0&&
                    NearReadPages(f,pc,source,source+4096).AsSpan().SequenceEqual(before),
                    "R169 target-fault after real watched-destination ticket withholds all stores");
            }
            else
            {
                Check(f.Step(pc),"R169 watched destination CALL controller");
                for(int n=0;n<8&&f.Completed==0;++n)Check(f.Continue(),"R169 watched CALL same-issue resume");
                Check(f.Completed==1&&f.Issued==1&&((ulong*)((byte*)f.Machine()+56))[0]==slot,
                    "R169 watched ticket CALL commits once after target proof");
            }
            Check(consumed,"R169 test exercised actual controller writer ticket");NearNoCapabilities(f);
        }
    }

    static void NearEmission()
    {
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity],slab=new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach(bool windows in new[]{false,true})
        {
            FullBlockEmitter.EmitScaffold(slab,out var layout,windows);var common=layout.Resolve((nint)0x10000000);
            foreach(bool shared in new[]{false,true})foreach(byte[] bytes in NearEncodings())
            {
                var i=NearClassify(Ip,bytes);FullBlockInstruction[] body=[new(i)];
                Check(shared?FullBlockEmitter.TryEmit(body,staging,in common,out _,windows):
                    FullBlockEmitter.TryEmit(body,staging,out _,windows),"R169 both ABI/scaffold emission only");
                bool early=false;
                try{FullBlockEmitter.Emit([new(i),new(new(Ip+(uint)i.Length,1,JitOperation.Nop,64,default,default))],staging);}
                catch(ArgumentException){early=true;}
                Check(early,"R169 every family form terminates TB");
            }
        }
    }
}
