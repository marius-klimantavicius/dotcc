using System.Buffers.Binary;
using System.Numerics;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using ControlFlagFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE-AUTHORED, UNRUN. Parent owns Main/core integration and all builds,
    // emitted-code checks and runtime qualification. Only the actual host ABI
    // is invoked; both ABI encodings are authored below. GetOp is test-only.
    static void CheckFullNativeControlFlags(byte* machine)
    {
        Check(sizeof(FullBlockFrame) == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(TranslationBlockHeader) == 160, "R90 unchanged ABI5/frame400/header160");
        Check(!fullOwnerScaffold.IsValid, "R90 begins with standalone scaffold");
        using (var f = new ControlFlagFixture(interpreterOracle: true))
        {
            CheckControlFlagClassifier(f.Program);
            CheckControlFlagOracles(f);
            CheckControlFlagLeaves(machine);
            using var common = CreateFullScaffold(out var addresses);
            fullOwnerScaffold = addresses;
            try { CheckControlFlagOracles(f); CheckControlFlagLeaves(machine); }
            finally { fullOwnerScaffold = default; }
        }
        CheckControlFlagController();
        CheckControlFlagEmission();
        ResetFull(machine);
    }

    static byte[] ControlFlagBytes(byte[] prefix, byte opcode, byte displacement = 0x20)
    {
        var bytes = new byte[prefix.Length + (opcode >= 0xe0 ? 2 : 1)];
        prefix.CopyTo(bytes, 0); bytes[prefix.Length] = opcode;
        if (opcode >= 0xe0) bytes[^1] = displacement;
        return bytes;
    }

    static JitInstruction ControlFlagClassify(Blink p, ulong ip, byte[] bytes)
    {
        Blink.XedDecodedInst decoded = default;
        fixed (byte* b = bytes)
            Check(p.DecodeInstruction(&decoded, b, (ulong)bytes.Length, Blink.XED_MODE_LONG) == 0 &&
                decoded.length == bytes.Length, "R90 exact authoritative long-mode XED span");
        Check(FullControlFlagLowering.TryClassify(ip, bytes, out var i, out string? reason),
            reason ?? "R90 control/flags classify");
        Check(i.Ip == ip && i.Length == bytes.Length && FullBlockEmitter.CanInline(i) &&
            FullBlockEmitter.CanInlineControlFlag(i) && FullBlockEmitter.ValidateControlFlag(i),
            "R90 classified canonical native IR reaches emitter");
        return i;
    }

    static void CheckControlFlagClassifier(Blink p)
    {
        byte[][] prefixes = [[], [0x66], [0x67], [0x66,0x67], [0x67,0x66]];
        foreach (byte[] prefix in prefixes)
        foreach (byte opcode in new byte[] { 0xe0,0xe1,0xe2,0xe3,0x9e,0x9f })
        for (int rex = -1; rex < 16; ++rex)
        {
            byte[] actualPrefix = rex < 0 ? prefix : [..prefix, (byte)(0x40 + rex)];
            var bytes = ControlFlagBytes(actualPrefix, opcode, 0x80);
            var i = ControlFlagClassify(p, Ip, bytes);
            Check(i.Width == (opcode < 0xe0 ? 8 : prefix.Contains((byte)0x67) ? 32 : 64) &&
                i.Target == (opcode < 0xe0 ? 0 : unchecked(Ip + (uint)bytes.Length - 128)),
                "R90 66/REX ignored; only67 selects ECX; rel8 sign extends to full RIP");
            Check(i.Operation == (opcode switch { 0xe0 => JitOperation.LoopNotEqual,
                0xe1 => JitOperation.LoopEqual, 0xe2 => JitOperation.Loop, 0xe3 => JitOperation.CounterZeroJump,
                0x9e => JitOperation.StoreFlagsAh, _ => JitOperation.LoadFlagsAh }), "R90 exact opcode mapping");
            for (int cut = 0; cut < bytes.Length; ++cut)
                Check(!FullControlFlagLowering.TryClassify(Ip, bytes.AsSpan(0,cut), out _, out _),
                    "R90 every truncated accepted instruction rejects");
            Check(!FullControlFlagLowering.TryClassify(Ip, [..bytes,0x90], out _, out _),
                "R90 no trailing instruction may enter exact span");
        }
        foreach (byte segment in new byte[] { 0x26,0x2e,0x36,0x3e,0x64,0x65 })
        foreach (byte[] prefix in new byte[][] { [segment,0x66,0x67], [0x67,segment,0x66],
            [0x66,0x67,segment], [segment,0x67,0x66], [0x66,segment,0x67], [0x67,0x66,segment] })
            Check(!FullControlFlagLowering.TryClassify(Ip,ControlFlagBytes(prefix,0xe1),out _,out _),
                "R90 reserved segment branch prefixes remain named MISSING");
        foreach (byte opcode in new byte[] { 0xe0,0xe1,0xe2,0xe3 })
        foreach (byte[] prefix in new byte[][] { [],[0x66],[0x67],[0x66,0x67,0x48] })
        for (int displacement = -128; displacement < 128; ++displacement)
        foreach (ulong pc in new[] { Ip, ulong.MaxValue - 8, 0xffffffffffff0000UL })
        {
            byte[] bytes = ControlFlagBytes(prefix, opcode, unchecked((byte)displacement));
            var i = ControlFlagClassify(p,pc,bytes);
            Check(i.Target == unchecked(pc + (uint)bytes.Length + (ulong)(long)displacement),
                "R90 all signed rel8 values and full64 RIP wrap independent of counter address width");
        }
        foreach (byte[] bytes in new byte[][] { [], [0x66], [0x67], [0x48], [0xe0], [0xe3],
            [0x9f,0], [0xe1,0,0], [0xf0,0xe2,0], [0xf2,0xe3,0], [0xf3,0xe0,0],
            [0xf0,0x9f], [0xf3,0x9e], [0x66,0x66,0xe2,0], [0x67,0x67,0xe3,0],
            [0x64,0x65,0x9f], [0x48,0x66,0xe1,0], [0x40,0x40,0x9e],
            [0x9c], [0x9d], [0xc8,0,0,0], [0xc9], [0x0f,0x9f,0xc0] })
            Check(!FullControlFlagLowering.TryClassify(Ip,bytes,out var miss,out string? reason) &&
                miss == default && reason != null, "R90 unsupported/invalid forms remain named MISSING");
        Check(!FullControlFlagLowering.TryClassify(Ip,new byte[16],out _,out _), "R90 length over15 rejects");
        Check(FullControlFlagLowering.TryClassify(0x00007ffffffffffeUL,[0xe2,1],out _,out string? missing) &&
            missing == null, "R169 native taken-only noncanonical target fault retains counter site");
        Check(FullControlFlagLowering.TryClassify(0xffff800000000000UL,[0xe3,0x80],out _,out _),
            "R169 upper canonical boundary target retained for taken-only native check");

        var loop = ControlFlagClassify(p,Ip,[0xe2,0x20]);
        var flags = ControlFlagClassify(p,Ip,[0x9f]);
        foreach (var good in new[] { loop,flags })
        foreach (var bad in new[] { good with { Length=0 },good with { Length=16 },good with { Width=16 },
            good with { Destination=new(1,0) },good with { Source=new(0,1,true) },good with { Extra=new(1,0) },
            good with { Source=new(0,0,IsMemory:true) },good with { Address=new(1,-1,0,0) },
            good with { Condition=1 },good with { SourceWidth=8 },good with { Options=1 } })
        {
            bool rejected=false; try { FullBlockEmitter.ValidateControlFlag(bad); } catch (ArgumentException) { rejected=true; }
            Check(rejected && !FullBlockEmitter.CanInlineControlFlag(bad), "R90 malformed family IR rejects before code emission");
        }
        foreach (var bad in new[] { loop with { Length=1 },loop with { Width=32 },
            loop with { Target=Ip+130 },loop with { Target=0x0000800000000000 },flags with { Target=1 } })
        {
            bool rejected=false;try { FullBlockEmitter.Emit([new(bad)],new byte[4096]); } catch(ArgumentException) { rejected=true; }
            Check(rejected,"R90 emitter rejects impossible rel8/width/target metadata");
        }
        bool early=false;
        try { FullBlockEmitter.Emit([new(loop),new(new(Ip+2,1,JitOperation.Nop,64,default,default))],new byte[4096]); }
        catch(ArgumentException) { early=true; }
        Check(early,"R90 native counter branch must end its retained TB");
        Check(!FullBlockEmitter.ValidateControlFlag(flags with { Operation=JitOperation.Nop }),
            "R90 nonfamily validation chains onward");
    }

    static void ControlFlagSeed(byte* m, uint flags = 0xa52c0ed5u)
    {
        m[8]=7; m[11]=0; ((uint*)(m+12))[0]=flags & ~0x50100u;
        for(int r=0;r<16;++r) Write64(m,24+r*8,0xfedcba9876543210UL^(0x0101010101010101UL*(uint)r));
        for(int b=0;b<256;++b)m[160+b]=(byte)(b*37+19);
        ((Blink.Machine*)m)->mxcsr=0x1f80;
    }

    // The independent model uses popcount, not the emitted XOR-fold sequence.
    static byte ControlFlagAh(uint flags) => (byte)((flags & 0xd1u) | 2u |
        ((BitOperations.PopCount(flags >> 24) % 2 == 0) ? 4u : 0u));
    static uint ControlFlagSahf(uint flags, byte ah) =>
        (flags & ~0xff0000d5u) | (uint)(ah & 0xd5) | ((ah & 4) == 0 ? 0x01000000u : 0u);

    static byte[] ControlFlagExpected(byte[] before, JitInstruction i)
    {
        byte[] result=(byte[])before.Clone();
        uint flags=BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12));
        ulong next=unchecked(i.Ip+(uint)i.Length);
        if(FullBlockEmitter.IsNativeCounterBranch(i))
        {
            ulong cx=BinaryPrimitives.ReadUInt64LittleEndian(before.AsSpan(32));
            if(i.Operation == JitOperation.CounterZeroJump)
            {
                if((i.Width==32?(uint)cx:cx)==0)next=i.Target;
            }
            else
            {
                cx=i.Width==32?unchecked((uint)cx-1u):unchecked(cx-1);
                BinaryPrimitives.WriteUInt64LittleEndian(result.AsSpan(32),cx);
                bool zf=(flags&0x40)!=0;
                if(cx!=0 && (i.Operation==JitOperation.Loop || i.Operation==JitOperation.LoopEqual && zf ||
                    i.Operation==JitOperation.LoopNotEqual && !zf))next=i.Target;
            }
        }
        else if(i.Operation == JitOperation.LoadFlagsAh)result[25]=ControlFlagAh(flags);
        else BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(12),ControlFlagSahf(flags,before[25]));
        BinaryPrimitives.WriteUInt64LittleEndian(result,next);result[8]=0;
        return result;
    }

    static void ControlFlagReference(Blink program,byte* machine,byte[] bytes)
    {
        Blink.XedDecodedInst decoded=default;
        fixed(byte* b=bytes)
            Check(program.DecodeInstruction(&decoded,b,(ulong)bytes.Length,Blink.XED_MODE_LONG)==0 && decoded.length==bytes.Length,
                "R90 ordinary GetOp oracle authoritative XED");
        var m=(Blink.Machine*)machine;var previous=m->xedd;
        m->xedd=&decoded;m->oplen=(byte)bytes.Length;m->ip+=(uint)bytes.Length;
        try
        {
            var op=decoded.op;
            program.GetOp((long)((op.rde>>40)&2047))(program,m,op.rde,op.disp,op.uimm0);
            if(m->stashaddr!=0)program.CommitStash(m);
            m->oplen=0;
        }
        finally { m->xedd=previous; }
    }

    static void ControlFlagRunOne(byte* m,NativeCodeAllocation code,JitInstruction i,byte[] expected)
    {
        ulong runtime=11,codeEpoch=11;long issued=0,completed=0;int stopped=0;
        var h=FullHeader(i.Ip,code.Entry);
        var frame=FullFrame(m,&h,&runtime,&issued,&completed,&stopped,1,1,&codeEpoch);
        // This family must be entirely independent of guest-data capabilities.
        frame.MemoryTlb=(FullMemoryTlbEntry*)1;frame.MemoryTlbMask=63;
        frame.DataDescriptors=(JitDataDescriptor*)1;frame.DataDescriptorCount=4;
        frame.StackMemory=(FullStackMemoryDescriptor*)1;
        Check(RunFull(code,&frame)==FullBlockExit.Continue && issued==1 && completed==1 && frame.Executed==1 &&
            frame.HelperIndex==FullBlockFrame.NoHelperIndex && frame.MemoryAssists==0 && frame.NativeMemoryComputations==0 &&
            frame.MemoryResume==0 && frame.TailJumps==0 && frame.FaultTrap==0,
            "R90 one guest instruction issues and completes exactly once without helpers/MMU");
        Check(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(expected),
            $"R90 independent whole-Machine state including all RAX aliases/flags/FP/segments: {i.Operation}/{i.Width}");
    }

    static void CheckControlFlagOracles(ControlFlagFixture f)
    {
        byte* m=(byte*)f.Machine();
        foreach(byte opcode in new byte[]{0xe0,0xe1,0xe2,0xe3,0x9e,0x9f})
        foreach(byte[] prefix in new byte[][]{[],[0x66],[0x67],[0x67,0x66,0x4f]})
        {
            byte[] bytes=ControlFlagBytes(prefix,opcode);
            var i=ControlFlagClassify(f.Program,Ip,bytes);
            using var code=CreateFullBlock([new(i)]);
            foreach(ulong cx in new[]{0UL,1UL,2UL,0xffffffffUL,0x100000000UL,0x100000001UL,
                0x100010000UL,0x8000000000000000UL,ulong.MaxValue})
            foreach(uint status in new uint[]{0xa52c0e02,0x5a2c0ed7})
            {
                f.At(Ip);ControlFlagSeed(m,status);Write64(m,32,cx);
                byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                byte[] expected=ControlFlagExpected(before,i);
                f.DataScope(0,()=>ControlFlagReference(f.Program,(byte*)f.Machine(),bytes));
                byte[] selected=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                if(opcode!=0x9f)
                    Check(selected.AsSpan().SequenceEqual(expected),"R90 exact ordinary selected oracle matches independent branch/SAHF model");
                else
                {
                    // Inherited selected ExportFlags uses FLAGS_F1=3, setting
                    // AH bit3 instead of forcing bit1 and clearing bits3/5.
                    uint flags=BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12));
                    byte selectedAh=(byte)((flags&~4u)|8u|(uint)(ControlFlagAh(flags)&4));
                    byte[] selectedExpected=(byte[])expected.Clone();selectedExpected[25]=selectedAh;
                    Check(selected.AsSpan().SequenceEqual(selectedExpected) && selected[25]!=expected[25],
                        "R90 explicit inherited selected LAHF reserved-bit discrepancy; no false equivalence claim");
                }
                before.AsSpan().CopyTo(new Span<byte>(m,sizeof(Blink.Machine)));
                ControlFlagRunOne(m,code,i,expected);
            }
        }
    }

    static void CheckControlFlagLeaves(byte* m)
    {
        var lahf=new JitInstruction(Ip,1,JitOperation.LoadFlagsAh,8,default,default);
        var sahf=lahf with{Operation=JitOperation.StoreFlagsAh};
        using var load=CreateFullBlock([new(lahf)]);
        using var store=CreateFullBlock([new(sahf)]);
        for(int byteValue=0;byteValue<256;++byteValue)
        foreach(uint seed in new uint[]{0x002c0e00,0x00cc0ed5,0x00540e2a,0x00880e02})
        {
            ResetFull(m);ControlFlagSeed(m,seed|((uint)byteValue<<24));
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            ControlFlagRunOne(m,load,lahf,ControlFlagExpected(before,lahf));
            ResetFull(m);ControlFlagSeed(m,seed|0xa5000000u);m[25]=(byte)byteValue;
            before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            ControlFlagRunOne(m,store,sahf,ControlFlagExpected(before,sahf));
            Check((((uint*)(m+12))[0]&~0xff0000d5u)==(BinaryPrimitives.ReadUInt32LittleEndian(before.AsSpan(12))&~0xff0000d5u),
                "R90 SAHF preserves reserved bits, OF and controls; rawPF and LP encode the new parity");
        }
        // Directly observe SAHF's lazy PF using the existing native condition
        // operation, then load flags back into AH. All are actual inline sites.
        using var combined=CreateFullBlock([new(sahf),
            new(new(Ip+1,3,JitOperation.SetCondition,8,new(3,0),default,Condition:10)),
            new(lahf with{Ip=Ip+4})]);
        for(int ah=0;ah<256;++ah)
        {
            ResetFull(m);ControlFlagSeed(m);m[25]=(byte)ah;
            uint flags=((uint*)(m+12))[0];ulong rax=((ulong*)(m+24))[0];
            ulong epoch=11;long issued=0,completed=0;int stop=0;
            var h=FullHeader(Ip,combined.Entry);var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop,3,3);
            uint expected=ControlFlagSahf(flags,(byte)ah);
            Check(RunFull(combined,&frame)==FullBlockExit.Continue && issued==3 && completed==3 && frame.Executed==3 &&
                m[48]==((ah&4)!=0?1:0) && m[25]==ControlFlagAh(expected) && ((uint*)(m+12))[0]==expected &&
                (((ulong*)(m+24))[0]&~0xff00UL)==(rax&~0xff00UL),"R90 SAHF->SETP->LAHF observes correct lazy parity and RAX upper aliases");
        }
        CheckControlFlagRips(m);
        CheckControlFlagBoundaries(m);
        CheckControlFlagInternal(m);
        CheckControlFlagLinks(m);
        CheckControlFlagLiveGuards(m);
    }

    static void CheckControlFlagRips(byte* m)
    {
        foreach(ulong pc in new[]{0x123450000UL,ulong.MaxValue-8,0xffffffffffff0000UL})
        foreach(int width in new[]{32,64})
        foreach(int displacement in new[]{-128,-1,0,1,127})
        foreach(JitOperation operation in new[]{JitOperation.Loop,JitOperation.LoopEqual,JitOperation.LoopNotEqual,JitOperation.CounterZeroJump})
        {
            int length=width==32?3:2;
            var i=new JitInstruction(pc,length,operation,width,default,default,
                Target:unchecked(pc+(uint)length+(ulong)(long)displacement));
            using var code=CreateFullBlock([new(i)]);
            ResetFull(m,pc);ControlFlagSeed(m,operation==JitOperation.LoopNotEqual?0xa52c0e02u:0xa52c0e42u);
            Write64(m,32,operation==JitOperation.CounterZeroJump?(width==32?0x100000000UL:0UL):2UL);
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            ControlFlagRunOne(m,code,i,ControlFlagExpected(before,i));
        }
    }

    static void CheckControlFlagBoundaries(byte* m)
    {
        foreach(var i in new JitInstruction[]{new(Ip,1,JitOperation.LoadFlagsAh,8,default,default),
            new(Ip,1,JitOperation.StoreFlagsAh,8,default,default),
            new(Ip,2,JitOperation.Loop,64,default,default,Target:Ip+32),
            new(Ip,3,JitOperation.LoopEqual,32,default,default,Target:Ip+32),
            new(Ip,2,JitOperation.LoopNotEqual,64,default,default,Target:Ip+32),
            new(Ip,3,JitOperation.CounterZeroJump,32,default,default,Target:Ip+32)})
        {
            using var code=CreateFullBlock([new(i)]);
            foreach(uint preissued in new uint[]{0,1})
            foreach(string boundary in new[]{"none","stop","attention","invalidated","runtime","code","mode","TF","RF"})
            for(ulong budget=preissued;budget<=2;++budget)
            {
                ResetFull(m);ControlFlagSeed(m);Write64(m,32,0x100000002UL);
                if(boundary=="attention")m[11]=1;
                if(boundary=="mode")((Blink.Machine*)m)->mode.omode=Blink.XED_MODE_LEGACY;
                if(boundary=="TF")((uint*)(m+12))[0]|=0x100;
                if(boundary=="RF")((uint*)(m+12))[0]|=0x10000;
                byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                ulong runtime=11,codeEpoch=11;long issued=preissued,completed=0;int stop=boundary=="stop"?1:0;
                var h=FullHeader(Ip,code.Entry);if(boundary=="invalidated")h.Live=0;
                var frame=FullFrame(m,&h,&runtime,&issued,&completed,&stop,budget,1,&codeEpoch);
                frame.FirstPreissued=preissued;
                if(boundary=="runtime")++runtime;
                if(boundary=="code")++codeEpoch;
                bool blocked=boundary!="none" && !(boundary=="attention" && preissued==1);
                bool runs=!blocked && budget!=0;
                FullBlockExit expected=boundary=="stop"?FullBlockExit.Stop:
                    boundary=="attention" && preissued==0?FullBlockExit.Attention:
                    blocked?FullBlockExit.Invalidated:budget==0?FullBlockExit.Budget:FullBlockExit.Continue;
                Check(RunFull(code,&frame)==expected && issued==(blocked?preissued:1) && completed==(runs?1:0) &&
                    frame.Executed==(runs?1UL:0UL) && frame.FirstPreissued==(blocked?preissued:0u),
                    "R90 exact boundary precedence and approved/rejected issue counts: "+boundary);
                Check(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(runs?ControlFlagExpected(before,i):before),
                    "R90 blocked/rejected branch does not decrement counter or alter AH/flags");
            }
        }
        using var cancellation=new CancellationTokenSource();
        using var stopOwner=new HostExecutionStop(cancellation:cancellation.Token);
        using var poll=stopOwner.AcquireNativePoll();cancellation.Cancel();
        var self=new JitInstruction(Ip,2,JitOperation.Loop,64,default,default,Target:Ip);
        using var loop=CreateFullBlock([new(self)]);
        ResetFull(m);ControlFlagSeed(m);ulong epoch=11;long issue=1,completion=0;
        var header=FullHeader(Ip,loop.Entry);var cancel=FullFrame(m,&header,&epoch,&issue,&completion,poll.Pointer);
        cancel.FirstPreissued=1;byte[] unchanged=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
        Check(RunFull(loop,&cancel)==FullBlockExit.Stop && issue==1 && completion==0 && cancel.Executed==0 &&
            cancel.FirstPreissued==1 && new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(unchanged),
            "R90 external cancellation reaches real native stop poll before approved counter issue");
        // Deterministic completion alias raises stop immediately after one
        // committed branch. Its internal backedge must re-enter the poll.
        ResetFull(m);ControlFlagSeed(m);Write64(m,32,7);issue=completion=0;
        var after=FullFrame(m,&header,&epoch,&issue,&completion,(int*)&completion);
        Check(RunFull(loop,&after)==FullBlockExit.Stop && issue==1 && completion==1 && after.Executed==1 &&
            ((ulong*)m)[0]==Ip && ((ulong*)(m+32))[0]==6,"R90 internal branch polls stop before second decrement/issue");
    }

    static void CheckControlFlagInternal(byte* m)
    {
        foreach(JitOperation operation in new[]{JitOperation.Loop,JitOperation.LoopEqual,JitOperation.LoopNotEqual,
            JitOperation.CounterZeroJump})
        foreach(int width in new[]{32,64})
        {
            int length=width==32?3:2;
            var i=new JitInstruction(Ip,length,operation,width,default,default,Target:Ip);
            using var code=CreateFullBlock([new(i)]);
            foreach(uint cap in new uint[]{0,1,3,512,1025})
            foreach(ulong budget in new[]{0UL,1UL,2UL,33UL,512UL,1025UL})
            {
                ResetFull(m);ControlFlagSeed(m,operation==JitOperation.LoopNotEqual?0x5a2c0e02u:0xa52c0e42u);
                bool jcxz=operation==JitOperation.CounterZeroJump;
                Write64(m,32,jcxz?(width==32?0x100000000UL:0UL):4096UL);
                ulong initial=((ulong*)(m+32))[0];uint flags=((uint*)(m+12))[0];
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop,budget,cap);
                ulong done=cap==0?budget:Math.Min(budget,cap);
                bool capped=cap!=0 && cap<=budget;
                Check(RunFull(code,&frame)==(capped?FullBlockExit.Continue:FullBlockExit.Budget) &&
                    issued==(long)done+(capped?0:1) && completed==(long)done && frame.Executed==done && frame.NativeLimit==cap &&
                    frame.TailJumps==0 && frame.HelperIndex==FullBlockFrame.NoHelperIndex && ((ulong*)m)[0]==Ip &&
                    ((ulong*)(m+32))[0]==(jcxz?initial:initial-done) && ((uint*)(m+12))[0]==flags,
                    "R90 one counter-branch site runs beyond512 instructions; exactly one issue/commit per iteration");
            }
        }
        foreach(int width in new[]{32,64})
        {
            int length=width==32?3:2;
            using var code=CreateFullBlock([new(new(Ip,length,JitOperation.Loop,width,default,default,Target:Ip))]);
            foreach(ulong cx in new[]{0UL,1UL,2UL,0x100000000UL,0x100000001UL})
            foreach(ulong budget in new[]{1UL,2UL,3UL,7UL})
            {
                ResetFull(m);ControlFlagSeed(m);Write64(m,32,cx);
                ulong effective=width==32?(uint)cx:cx;
                ulong done=effective!=0 && effective<=budget?effective:budget;
                bool terminated=effective!=0 && effective<=budget;
                ulong remaining=width==32?unchecked((uint)cx-(uint)done):unchecked(cx-done);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stop,budget);
                Check(RunFull(code,&frame)==(terminated?FullBlockExit.Continue:FullBlockExit.Budget) &&
                    issued==(long)done+(terminated?0:1) && completed==(long)done && frame.Executed==done &&
                    ((ulong*)(m+32))[0]==remaining && ((ulong*)m)[0]==(terminated?Ip+(uint)length:Ip),
                    "R90 zero wraps instead of terminating; ECX high bits clear on first write even if branch falls through");
            }
        }
        // Different internal target label: every branch visit re-executes LAHF
        // through a normal guarded site, while the final counter branch owns
        // one commit. No extra issue for its conditional arithmetic.
        using var mixed=CreateFullBlock([new(new(Ip,1,JitOperation.LoadFlagsAh,8,default,default)),
            new(new(Ip+1,2,JitOperation.Loop,64,default,default,Target:Ip))]);
        ResetFull(m);ControlFlagSeed(m);Write64(m,32,7);uint status=((uint*)(m+12))[0];
        ulong global=11;long issue=0,completion=0;int stopped=0;var header=FullHeader(Ip,mixed.Entry);
        var f=FullFrame(m,&header,&global,&issue,&completion,&stopped,100);
        Check(RunFull(mixed,&f)==FullBlockExit.Continue && issue==14 && completion==14 && f.Executed==14 &&
            ((ulong*)m)[0]==Ip+3 && ((ulong*)(m+32))[0]==0 && m[25]==ControlFlagAh(status) && ((uint*)(m+12))[0]==status,
            "R90 internal backward edge into another site preserves exact instruction boundaries");
    }

    static void CheckControlFlagLinks(byte* m)
    {
        foreach(JitOperation operation in new[]{JitOperation.Loop,JitOperation.LoopEqual,JitOperation.LoopNotEqual,JitOperation.CounterZeroJump})
        foreach(int width in new[]{32,64})
        {
            int length=width==32?3:2;ulong fallPc=Ip+(uint)length,takenPc=Ip+32;
            var i=new JitInstruction(Ip,length,operation,width,default,default,Target:takenPc);
            using var sourceCode=CreateFullBlock([new(i)]);
            using var fallCode=CreateFullBlock([FullBlockInstruction.Helper(fallPc,1)]);
            using var takenCode=CreateFullBlock([FullBlockInstruction.Helper(takenPc,1)]);
            foreach(bool take in new[]{false,true})
            foreach(string mismatch in new[]{"none","other-edge","live","owner","context","generation","link-generation",
                "link-pc","pc","code","validated-code","zero-code","code-epoch","entry","null"})
            {
                ResetFull(m);ControlFlagSeed(m,operation==JitOperation.LoopNotEqual?0x5a2c0e02u:0xa52c0e42u);
                ulong cx=operation==JitOperation.CounterZeroJump?(take?0UL:1UL):(take?2UL:1UL);
                if(width==32)cx|=0xfedcba9800000000UL;
                Write64(m,32,cx);uint flags=((uint*)(m+12))[0];
                ulong global=11,codeEpoch=11;long issued=0,completed=0;int stop=0;
                var source=FullHeader(Ip,sourceCode.Entry);var a=FullHeader(fallPc,fallCode.Entry,18);var b=FullHeader(takenPc,takenCode.Entry,19);
                source.Link0TargetHeader=(nint)(&a);source.Link0TargetGeneration=a.BodyGeneration;source.Link0Pc=fallPc;
                source.Link1TargetHeader=(nint)(&b);source.Link1TargetGeneration=b.BodyGeneration;source.Link1Pc=takenPc;
                TranslationBlockHeader* selected=take?&b:&a;
                switch(mismatch)
                {
                    case "other-edge":(take?&a:&b)->Live=0;break;
                    case "live":selected->Live=0;break;
                    case "owner":++selected->OwnerCookie;break;
                    case "context":++selected->Context;break;
                    case "generation":++selected->BodyGeneration;break;
                    case "pc":++selected->GuestPc;break;
                    case "code":++selected->CodeEpoch;break;
                    case "validated-code":++selected->ValidatedCodeEpoch;break;
                    case "zero-code":selected->CodeEpoch=selected->ValidatedCodeEpoch=0;break;
                    case "code-epoch":++selected->ValidatedGlobalEpoch;break;
                    case "entry":selected->Entry=0;break;
                    case "link-generation":if(take)++source.Link1TargetGeneration;else ++source.Link0TargetGeneration;break;
                    case "link-pc":if(take)++source.Link1Pc;else ++source.Link0Pc;break;
                    case "null":if(take)source.Link1TargetHeader=0;else source.Link0TargetHeader=0;break;
                }
                var frame=FullFrame(m,&source,&global,&issued,&completed,&stop,100,0,&codeEpoch);
                bool linked=mismatch is "none" or "other-edge";
                ulong next=take?takenPc:fallPc;
                Check(RunFull(sourceCode,&frame)==(linked?FullBlockExit.NeedHelper:FullBlockExit.Continue) &&
                    frame.Header==(linked?selected:&source) && frame.TailJumps==(linked?1UL:0UL) &&
                    issued==(linked?2:1) && completed==1 && frame.Executed==1 && ((ulong*)m)[0]==next+(linked?1UL:0UL) &&
                    ((ulong*)(m+32))[0]==(operation==JitOperation.CounterZeroJump?cx:width==32?unchecked((uint)cx-1u):cx-1) &&
                    ((uint*)(m+12))[0]==flags,"R90 selected static edge0/1 independently authenticates target and commits once: "+mismatch);
            }
            foreach(uint cap in new uint[]{0,1,2})
            foreach(ulong budget in new[]{0UL,1UL,2UL})
            foreach(uint approved in new uint[]{0,1})
            {
                if(budget<approved)continue;
                ResetFull(m);ControlFlagSeed(m);Write64(m,32,operation==JitOperation.CounterZeroJump?0UL:2UL);
                // Configure ZF so every branch uses edge1, including LOOPNE.
                if(operation==JitOperation.LoopNotEqual)((uint*)(m+12))[0]&=~0x40u;
                ulong epoch=11;long issued=approved,completed=0;int stop=0;
                var a=FullHeader(Ip,sourceCode.Entry);var b=FullHeader(takenPc,takenCode.Entry,19);
                a.Link1TargetHeader=(nint)(&b);a.Link1TargetGeneration=b.BodyGeneration;a.Link1Pc=takenPc;
                var f=FullFrame(m,&a,&epoch,&issued,&completed,&stop,budget,cap);f.FirstPreissued=approved;
                bool ran=budget!=0,linked=ran && cap!=1,helper=linked && budget>=2;
                FullBlockExit expected=!ran?FullBlockExit.Budget:cap==1?FullBlockExit.Continue:
                    helper?FullBlockExit.NeedHelper:FullBlockExit.Budget;
                Check(RunFull(sourceCode,&f)==expected && completed==(ran?1:0) && f.Executed==(ran?1UL:0UL) &&
                    issued==(!ran?1:cap==1?1:2) && f.TailJumps==(linked?1UL:0UL) &&
                    f.FirstPreissued==0 && ((ulong*)m)[0]==(!ran?Ip:helper?takenPc+1:takenPc),
                    "R90 external branch total cap/budget and approved issue survive target prologue without reset");
            }
        }
    }

    static void CheckControlFlagLiveGuards(byte* m)
    {
        using var code=CreateFullBlock([new(new(Ip,1,JitOperation.LoadFlagsAh,8,default,default)),
            new(new(Ip+1,2,JitOperation.Loop,64,default,default,Target:Ip))]);
        foreach(string changed in new[]{"mode","TF","runtime","code","live"})
        {
            ResetFull(m);ControlFlagSeed(m,changed=="TF"?0xffu:0xa52c0e42u);Write64(m,32,7);
            ulong runtime=11,codeEpoch=11;long issued=0;int stopped=0;var h=FullHeader(Ip,code.Entry);
            long* completed=changed switch{ "mode"=>(long*)(m+9), "TF"=>(long*)(m+12),
                "runtime"=>(long*)&runtime,"code"=>(long*)&codeEpoch,_=>(long*)&h.Live };
            var f=FullFrame(m,&h,&runtime,&issued,completed,&stopped,100,0,&codeEpoch);
            Check(RunFull(code,&f)==FullBlockExit.Invalidated && issued==1 && f.Executed==1 &&
                ((ulong*)m)[0]==Ip+1 && ((ulong*)(m+32))[0]==7 && f.ResumeIndex==1,
                "R90 following counter site rechecks live context and independent code/runtime epochs: "+changed);
        }
        using var loop=CreateFullBlock([new(new(Ip,2,JitOperation.Loop,64,default,default,Target:Ip))]);
        foreach(bool codeMutation in new[]{false,true})
        {
            ResetFull(m);ControlFlagSeed(m);Write64(m,32,7);
            ulong runtime=11,codeEpoch=11;long issued=0;int stop=0;var h=FullHeader(Ip,loop.Entry);
            long* completion=codeMutation?(long*)&codeEpoch:(long*)&runtime;
            var f=FullFrame(m,&h,&runtime,&issued,completion,&stop,100,0,&codeEpoch);
            Check(RunFull(loop,&f)==FullBlockExit.Invalidated && issued==1 && f.Executed==1 &&
                ((ulong*)(m+32))[0]==6 && ((ulong*)m)[0]==Ip,"R90 counter backedge rechecks both authority domains before another issue");
        }
    }

    static void CheckControlFlagController()
    {
        using var f=new ControlFlagFixture();const ulong pc=ControlFlagFixture.Base;
        f.Map(pc);f.Fill(pc);
        foreach(byte opcode in new byte[]{0xe0,0xe1,0xe2,0xe3})
        foreach(bool address32 in new[]{false,true})
        foreach(bool take in new[]{false,true})
        {
            byte[] branch=ControlFlagBytes(address32?new byte[]{0x66,0x67,0x48}:new byte[]{0x66,0x48},opcode);
            byte[] bytes=[0x9e,0x9f,..branch];
            ulong fall=pc+(uint)bytes.Length,target=pc+64;
            bytes[^1]=checked((byte)(target-fall));
            WriteControllerCode(f,pc,bytes);
            WriteControllerCode(f,fall,[0xbb,0x11,0x11,0x11,0x11,0xeb,0xfe]);
            WriteControllerCode(f,target,[0xbb,0x22,0x22,0x22,0x22,0xeb,0xfe]);
            var m=f.Machine();ControlFlagSeed((byte*)m);
            // SAHF establishes ZF as required; LAHF must observe it without
            // clobbering any software flags or redirecting the implicit AH.
            byte ah=opcode==0xe0?(byte)0x15:(byte)0x55;
            ((byte*)m)[25]=ah;
            ulong cx=opcode==0xe3?(take?0UL:1UL):(take?2UL:1UL);
            if(address32)cx|=0xfedcba9800000000UL;
            Write64((byte*)m,32,cx);
            uint expectedFlags=ControlFlagSahf(m->flags,ah);
            ulong initialRax=((ulong*)((byte*)m+24))[0];
            Check(f.Acquire(fall,0,out var a,out bool multiA,out int errorA) && errorA==0 && multiA,
                "R90 controller compiles fallthrough target under real authority");
            using(a)
            {
                Check(f.Acquire(target,0,out var b,out bool multiB,out int errorB) && errorB==0 && multiB,
                    "R90 controller compiles taken target under real authority");
                using(b)
                {
                    Check(f.Acquire(pc,0,out var source,out bool multi,out int error) && error==0 && multi,
                        "R90 controller compiles LAHF/SAHF/counter TB");
                    using(source)
                    {
                        var plans=FullTranslationJit.Plans(f.Jit.Store,in source);
                        Check(plans.Length==3 && plans[0].Helper==null && plans[1].Helper==null && plans[2].Helper==null,
                            "R90 actual compiler selects three native sites and terminates at counter branch");
                        lock(f.Jit.Authority.Gate)
                        {
                            f.Jit.AuthorizeLinks(in a,multiA);f.Jit.AuthorizeLinks(in b,multiB);
                            f.Jit.AuthorizeLinks(in source,multi);
                        }
                        var h=f.Jit.Store.GetHeaderCopy(in source);
                        Check(h.Link0Pc==fall && h.Link1Pc==target && h.Link0TargetHeader!=0 && h.Link1TargetHeader!=0,
                            "R90 AuthorizeLinks recognizes new fixed-target classifier and arms both static edges");
                        Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=4;
                        f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;
                        f.Counts(100);long tails=f.Statistics.NativeTailJumps;
                        using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
                        Check(f.Step(pc),"R90 real native controller executes bounded counter edge");
                        Check(f.Completed==4 && f.Issued==4 && m->ip==(take?target:fall)+5 && m->oplen==0 &&
                            ((ulong*)((byte*)m+48))[0]==(take?0x22222222UL:0x11111111UL) && m->flags==expectedFlags &&
                            ((byte*)m)[25]==ControlFlagAh(expectedFlags) &&
                            (((ulong*)((byte*)m+24))[0]&~0xff00UL)==(initialRax&~0xff00UL) &&
                            ((ulong*)((byte*)m+32))[0]==(opcode==0xe3?cx:address32?unchecked((uint)cx-1u):cx-1) &&
                            f.Statistics.NativeTailJumps>tails && f.Statistics.SemanticHelperInstructions==0 &&
                            f.Statistics.SemanticHelperCalls==0 && f.Statistics.InterpreterAttempts==0 &&
                            f.Statistics.NativeMemoryAssists==0,
                            "R90 actual native controller preserves flags/aliases/counter and selected authenticated edge without fallback");
                    }
                }
            }
            var counts=f.Jit.Store.GetStatistics();
            Check(counts.ActiveNativeEpochs==0 && counts.ActiveManagedLeases==0 && f.Statistics.ActiveDataBorrows==0,
                "R90 controller return releases all native/store/data capabilities");
        }
    }

    static void CheckControlFlagEmission()
    {
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity],tiny=new byte[7],slab=new byte[FullBlockEmitter.ScaffoldCapacity];
        byte[] classifierBytes=[0x66,0x67,0x48,0xe2,0xfe];
        FullBlockInstruction[][] bodies=[
            [new(new(Ip,1,JitOperation.LoadFlagsAh,8,default,default))],
            [new(new(Ip,1,JitOperation.StoreFlagsAh,8,default,default))],
            [new(new(Ip,2,JitOperation.Loop,64,default,default,Target:Ip))],
            [new(new(Ip,3,JitOperation.LoopEqual,32,default,default,Target:Ip+32))],
            [new(new(Ip,2,JitOperation.LoopNotEqual,64,default,default,Target:Ip+32))],
            [new(new(Ip,3,JitOperation.CounterZeroJump,32,default,default,Target:Ip+32))]];
        foreach(bool windows in new[]{false,true})
        {
            FullBlockEmitter.EmitScaffold(slab,out var layout,windows);
            var scaffold=layout.Resolve((nint)0x10000000); // source-authored encoding only; never execute fake address
            foreach(bool shared in new[]{false,true})
            foreach(var body in bodies)
            {
                for(int warm=0;warm<16;++warm)
                    Check(shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows):
                        FullBlockEmitter.TryEmit(body,staging,out _,windows),"R90 warmed both ABI/scaffold source contracts");
                long before=GC.GetAllocatedBytesForCurrentThread();bool good=true;
                for(int n=0;n<32;++n)
                {
                    good&=FullControlFlagLowering.TryClassify(Ip,classifierBytes,out _,out _);
                    good&=shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows):
                        FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                Check(good && GC.GetAllocatedBytesForCurrentThread()==before,
                    "R90 classifier/emitter spans allocate zero on both ABIs and scaffold forms");
                int length=shared?FullBlockEmitter.Emit(body,staging,in scaffold,windows):FullBlockEmitter.Emit(body,staging,windows);
                Check(length>0 && staging.AsSpan(0,7).SequenceEqual(windows?new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xca}:
                    new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xfa}),"R90 host ABI argument and CET entry prefix preserved");
                int emitted;
                bool fits=shared?FullBlockEmitter.TryEmit(body,tiny,in scaffold,out emitted,windows):
                    FullBlockEmitter.TryEmit(body,tiny,out emitted,windows);
                Check(!fits && emitted==0,"R90 short staging span yields no publishable partial counter/flag code");
            }
            // The normal compile-time envelope is512 sites, not an implicit
            // invocation cap. Every branch is final; no generated opcode loop.
            var maximum=new FullBlockInstruction[512];
            for(int i=0;i<511;++i)maximum[i]=new(new(Ip+(uint)i,1,JitOperation.Nop,64,default,default));
            maximum[^1]=new(new(Ip+511,2,JitOperation.Loop,64,default,default,Target:Ip+500));
            Check(FullBlockEmitter.MaximumInstructions==512 &&
                FullBlockEmitter.TryEmit(maximum,staging,out int size,windows) && size>0,
                "R90 full512-site normal envelope retains counter branch without changing native cap0 policy");
        }
    }
}
