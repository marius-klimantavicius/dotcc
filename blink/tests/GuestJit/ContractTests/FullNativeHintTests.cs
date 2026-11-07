using System.Buffers.Binary;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using HintFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // R88 SOURCE-AUTHORED, UNRUN. Parent owns classifier/core/Main integration,
    // fresh serial builds, emission and execution. Windows is encoding-only on
    // a non-Windows host. No product hint invokes the selected operation oracle.
    static void CheckFullNativeHints(byte* machine)
    {
        Check(sizeof(FullBlockFrame) == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(TranslationBlockHeader) == 160, "R88 unchanged ABI5/frame400/header160");
        Check(!fullOwnerScaffold.IsValid, "R88 start with standalone leaves");
        using (var f = new HintFixture(interpreterOracle: true))
        {
            CheckFullHintClassifier(f.Program);
            CheckFullHintGuestBaseline(f);
            CheckFullHintOracle(f);
            CheckFullHintLeaves(machine);
            using var common = CreateFullScaffold(out var addresses);
            fullOwnerScaffold = addresses;
            try { CheckFullHintOracle(f); CheckFullHintLeaves(machine); }
            finally { fullOwnerScaffold = default; }
        }
        // Dispose the selected interpreter binding before another owner fixture.
        CheckFullHintController();
        CheckFullHintEmission();
        ResetFull(machine);
    }

    static byte[] HintPrefix(ReadOnlySpan<byte> prefixes, ReadOnlySpan<byte> tail)
    {
        var result = new byte[prefixes.Length + tail.Length];
        prefixes.CopyTo(result); tail.CopyTo(result.AsSpan(prefixes.Length));
        return result;
    }

    static byte[] HintPadding(int length, byte[] tail)
    {
        var result = new byte[length];
        result.AsSpan(0, length - tail.Length).Fill(0x66);
        tail.CopyTo(result, length - tail.Length);
        return result;
    }

    // A corpus independent of service observations, including all recommended
    // 1..9 byte paddings, repeated66 through15, registers and true nonaccess EAs.
    static IEnumerable<byte[]> HintForms()
    {
        byte[][] recommended = [ [0x90], [0x66,0x90], [0x0f,0x1f,0x00],
            [0x0f,0x1f,0x40,0], [0x0f,0x1f,0x44,0,0], [0x66,0x0f,0x1f,0x44,0,0],
            [0x0f,0x1f,0x80,0,0,0,0], [0x0f,0x1f,0x84,0,0,0,0,0],
            [0x66,0x0f,0x1f,0x84,0,0,0,0,0] ];
        foreach (var bytes in recommended) yield return bytes;
        foreach (byte[] tail in new byte[][] { [0x90], [0xf3,0x90], [0x0f,0x1f,0x00],
            [0x0f,0x1f,0x84,0,0,0,0,0] })
            for (int length = tail.Length; length <= 15; ++length) yield return HintPadding(length, tail);
        yield return [0xf3,0x0f,0x1e,0xfa];
        yield return [0xf3,0x0f,0x1e,0xfb];
        foreach (byte prefix in new byte[] { 0x26,0x2e,0x36,0x3e,0x64,0x65,0x67 })
        {
            yield return [prefix,0x90];
            yield return [prefix,0xf3,0x90];
            yield return [prefix,0x0f,0x1f,0x03];
        }
        for (int rex = 0x40; rex <= 0x4f; ++rex)
        {
            yield return [(byte)rex,0x0f,0x1f,0xc7];
            yield return [0x64,0x66,0x67,(byte)rex,0x0f,0x1f,0x84,0x8b,0x80,0,0,0];
            if ((rex & 1) == 0)
            {
                yield return [(byte)rex,0x90];
                yield return [0x66,0xf3,(byte)rex,0x90];
            }
        }
        foreach (byte[] bytes in new byte[][] { [0x0f,0x1f,0x05,0xff,0xff,0xff,0x7f],
            [0x0f,0x1f,0x04,0x25,0,0,0,0x80], [0x0f,0x1f,0x44,0x24,0x80],
            [0x67,0x0f,0x1f,0x05,0xff,0xff,0xff,0xff],
            [0x0f,0x1f,0x45,0x7f], [0x0f,0x1f,0x45,0], [0x0f,0x1f,0x07],
            [0x0f,0x1f,0x47,1], [0x0f,0x1f,0x87,0,0,0,0] }) yield return bytes;
    }

    static JitInstruction HintClassify(Blink program, ReadOnlySpan<byte> bytes)
    {
        Blink.XedDecodedInst decoded = default;
        fixed (byte* p = bytes)
            Check(program.DecodeInstruction(&decoded,p,(ulong)bytes.Length,Blink.XED_MODE_LONG) == 0 &&
                decoded.length == bytes.Length, "R88 authoritative selected XED exact hint length");
        Check(FullHintLowering.TryClassify(Ip,bytes,out var i,out string? reason), reason ?? "R88 hint classify");
        Check(i == new JitInstruction(Ip,bytes.Length,i.Operation,64,default,default) &&
            FullBlockEmitter.CanInlineHint(i) && FullBlockEmitter.ValidateHint(i) && FullBlockEmitter.CanInline(i),
            "R88 canonical operand-free full hint IR and root admission");
        return i;
    }

    static void CheckFullHintClassifier(Blink program)
    {
        foreach (byte[] bytes in HintForms())
        {
            var i = HintClassify(program,bytes);
            bool pause = bytes.Contains((byte)0xf3) && bytes[^1] == 0x90;
            Check(i.Operation == (pause ? JitOperation.Pause : JitOperation.Nop), "R88 preserve PAUSE operation identity");
            for (int length = 0; length < bytes.Length; ++length)
                Check(!FullHintLowering.TryClassify(Ip,bytes.AsSpan(0,length),out _,out string? reason) && reason != null,
                    "R88 proper prefix truncation is never an instruction");
            Check(!FullHintLowering.TryClassify(Ip,HintPrefix(bytes,[0x90]),out _,out _), "R88 exact bytes reject trailing instruction");
        }
        // All /0 ModRM values; all SIB bytes for each memory mod. XED supplies
        // an independent length oracle; displacement bytes carry arbitrary data.
        foreach (byte[] prefix in new byte[][] { [], [0x66], [0x67], [0x65,0x66,0x67,0x4f] })
        for (int mod = 0; mod < 4; ++mod)
        for (int rm = 0; rm < 8; ++rm)
        for (int sib = 0; sib < (mod != 3 && rm == 4 ? 256 : 1); ++sib)
        {
            var bytes = new List<byte>(prefix) { 0x0f,0x1f,(byte)((mod << 6) | rm) };
            if (mod != 3 && rm == 4) bytes.Add((byte)sib);
            int count = mod == 1 ? 1 : mod == 2 || mod == 0 && (rm == 5 || rm == 4 && (sib & 7) == 5) ? 4 : 0;
            for (int d = 0; d < count; ++d) bytes.Add((byte)(0x81 + d));
            Check(HintClassify(program,bytes.ToArray()).Operation == JitOperation.Nop, "R88 all documented /0 addressing lengths");
        }
        // Every ordering of the four legacy groups, followed by a single REX.
        byte[] groups = [0x64,0x66,0x67,0xf3];
        for (int a=0;a<4;++a) for (int b=0;b<4;++b) for (int c=0;c<4;++c) for (int d=0;d<4;++d)
        {
            if (a==b || a==c || a==d || b==c || b==d || c==d) continue;
            HintClassify(program,[groups[a],groups[b],groups[c],groups[d],0x48,0x90]);
        }
        foreach (byte[] bytes in new byte[][] { [], [0x66], [0x0f], [0xf3], [0x40],
            [0x90,0], [0xf3,0x90,0], [0x0f,0x1f], [0x0f,0x1f,0x04], [0x0f,0x1f,0x44,0x24],
            [0x0f,0x1f,0x05,1,2,3], [0x0f,0x1f,0xc0,0], [0x0f,0x19,0], [0x0f,0x1e,0xfa],
            [0xf2,0x90], [0xf3,0x0f,0x1f,0], [0xf2,0x0f,0x1f,0],
            [0xf0,0x90], [0xf0,0xf3,0x90], [0xf0,0x0f,0x1f,0], [0xf0,0xf3,0x0f,0x1e,0xfa],
            [0xf3,0xf3,0x90], [0xf2,0xf3,0x90], [0x64,0x65,0x90], [0x67,0x67,0x90],
            [0x48,0x66,0x90], [0x40,0xf3,0x90], [0x40,0x40,0x90],
            [0x66,0xf3,0x0f,0x1e,0xfa], [0xf3,0x0f,0x1e,0xf9], [0xf3,0x0f,0x1e,0xfc] })
            Check(!FullHintLowering.TryClassify(Ip,bytes,out var missing,out string? reason) &&
                missing == default && reason != null, "R88 invalid/unsupported hint stays a named miss");
        Check(!FullHintLowering.TryClassify(Ip,HintPadding(16,[0x90]),out _,out _), "R88 over15-byte padding rejects");
        for (int rex=0x41;rex<=0x4f;rex+=2)
        foreach (byte[] prefix in new byte[][] { [], [0x66], [0xf3], [0x66,0xf3] })
            Check(!FullHintLowering.TryClassify(Ip,HintPrefix(prefix,[(byte)rex,0x90]),out _,out _),
                "R88 REX.B 90, including F3, is never classified as a hint");
        for (int group=1;group<8;++group)
        foreach (int mod in new[] { 0,1,2,3 })
        {
            byte[] reserved=mod==1?[0x0f,0x1f,(byte)((mod<<6)|(group<<3)),0]:
                mod==2?[0x0f,0x1f,(byte)((mod<<6)|(group<<3)),0,0,0,0]:
                [0x0f,0x1f,(byte)((mod<<6)|(group<<3))];
            Check(!FullHintLowering.TryClassify(Ip,reserved,out _,out _),
                "R88 reserved /1../7 NOP groups reject");
        }

        var good = HintClassify(program,[0xf3,0x90]);
        foreach (var bad in new[] { good with { Width=32 }, good with { Length=1 }, good with { Length=16 },
            good with { Destination=new(1,0) }, good with { Source=new(0,1,true) }, good with { Extra=new(1,0) },
            good with { Destination=new(0,0,IsMemory:true) }, good with { Source=new(0,0,IsMemory:true) },
            good with { Address=new(3,-1,0,0) }, good with { Target=1 }, good with { Condition=1 },
            good with { SourceWidth=8 }, good with { Options=1 } })
        {
            bool rejected=false; try { FullBlockEmitter.ValidateHint(bad); } catch (ArgumentException) { rejected=true; }
            Check(rejected, "R88 malformed Pause metadata rejects before emission");
            var badNop = bad with { Operation=JitOperation.Nop };
            if (bad.Length == 1) continue; // a one-byte NOP is valid
            rejected=false; try { FullBlockEmitter.Emit([new(badNop)],new byte[4096]); } catch (ArgumentException) { rejected=true; }
            Check(rejected, "R88 root hint validator prevents historical NOP bypass");
        }
        Check(!FullBlockEmitter.ValidateHint(good with { Operation=JitOperation.Mov }), "R88 nonhint validation chains onward");
    }

    static void HintReference(Blink program, byte* machine, ReadOnlySpan<byte> bytes)
    {
        Blink.XedDecodedInst decoded=default;
        fixed(byte* p=bytes)
            Check(program.DecodeInstruction(&decoded,p,(ulong)bytes.Length,Blink.XED_MODE_LONG)==0 && decoded.length==bytes.Length,
                "R88 normal GetOp oracle uses actual authoritative XED");
        var m=(Blink.Machine*)machine; var previous=m->xedd;
        m->xedd=&decoded; m->oplen=(byte)bytes.Length; m->ip+=(uint)bytes.Length;
        try
        {
            var op=decoded.op;
            program.GetOp((long)((op.rde>>40)&2047))(program,m,op.rde,op.disp,op.uimm0);
            if(m->stashaddr!=0) program.CommitStash(m);
            m->oplen=0;
        }
        finally { m->xedd=previous; }
    }

    static void HintSeed(byte* m)
    {
        m[8]=7; m[11]=0;
        *(uint*)(m+12)=0xa52c0ed5u & ~0x50100u;
        for(int r=0;r<16;++r) Write64(m,24+r*8,0xfedcba9876543210UL^(0x0101010101010101UL*(uint)r));
        for(int b=0;b<256;++b) m[160+b]=(byte)(b*37+19);
        ((Blink.Machine*)m)->mxcsr=0x1f80;
    }

    static byte[] HintExpected(byte[] before, ulong ip)
    {
        byte[] expected=(byte[])before.Clone();
        BinaryPrimitives.WriteUInt64LittleEndian(expected,ip); expected[8]=0;
        return expected;
    }

    static bool HintInstrumentation(byte[] bytes)
    {
        for(int b=0;b<bytes.Length-2;++b)
            if(bytes[b]==0x0f && bytes[b+1]==0x1f) return bytes[b+2] is 0x45 or 0x07 or 0x47 or 0x87;
        return false;
    }

    static void CheckFullHintGuestBaseline(HintFixture f)
    {
        Check(FullHintLowering.GuestCetDisabledBaseline,"R88 fixed guest CET-disabled policy");
        f.At(Ip); var m=f.Machine();
        Write64((byte*)m,24,7); Write64((byte*)m,32,0);
        f.DataScope(0,()=>HintReference(f.Program,(byte*)f.Machine(),new byte[]{0x0f,0xa2}));
        Check((*(uint*)((byte*)m+32)&(1u<<7))==0 && (*(uint*)((byte*)m+40)&(1u<<20))==0,
            "R88 selected guest CPUID7 excludes CET_SS and CET_IBT independent of host");
    }

    static void CheckFullHintOracle(HintFixture f)
    {
        byte* m=(byte*)f.Machine();
        foreach(byte[] bytes in HintForms())
        {
            f.At(Ip); HintSeed(m);
            var i=HintClassify(f.Program,bytes);
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            byte[] expected=HintExpected(before,Ip+(uint)bytes.Length);
            // BOFRAM changes emulator metadata and BINBASE may invoke a host
            // callback. They cannot be a whole-Machine no-effects oracle. The
            // parent-approved architectural behavior is asserted independently
            // for those forms below, with no claim of interpreter-hook parity.
            if(!HintInstrumentation(bytes))
            {
                f.DataScope(0,()=>HintReference(f.Program,(byte*)f.Machine(),bytes));
                Check(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(expected),
                    "R88 actual normal selected GetOp preserves full state for an ordinary hint");
                before.CopyTo(new Span<byte>(m,sizeof(Blink.Machine)));
            }
            using var code=CreateFullBlock([new(i)]);
            ulong epoch=11;long issued=0,completed=0;int stopped=0;
            var h=FullHeader(Ip,code.Entry);var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stopped,limit:1);
            Check(RunFull(code,&frame)==FullBlockExit.Continue && issued==1 && completed==1 && frame.Executed==1 &&
                frame.HelperIndex==FullBlockFrame.NoHelperIndex && frame.MemoryAssists==0,
                "R88 one native hint issue/completion without helper or MMU assist");
            Check(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(expected),
                "R88 independent whole-Machine no-effects assertion includes RAX high32, flags, FP, XMM and segments");
        }
        // Selected OpNop resolves REX.B before REP3. Preserve that source
        // contract explicitly without calling it a universal CPU-prefix claim.
        foreach(byte[] bytes in new byte[][] { [0x41,0x90], [0x49,0x90], [0xf3,0x41,0x90],
            [0xf3,0x49,0x90], [0x66,0x41,0x90] })
        {
            f.At(Ip); HintSeed(m);
            ulong a=*(ulong*)(m+24),b=*(ulong*)(m+88);
            bool wide=bytes.Contains((byte)0x49),word=bytes[0]==0x66;
            f.DataScope(0,()=>HintReference(f.Program,(byte*)f.Machine(),bytes));
            ulong expectedA=wide?b:word?(a&~65535UL)|(b&65535UL):(uint)b;
            ulong expectedB=wide?a:word?(b&~65535UL)|(a&65535UL):(uint)a;
            Check(*(ulong*)(m+24)==expectedA && *(ulong*)(m+88)==expectedB,
                "R88 selected normal REX.B plus optional F3 really exchanges RAX/R8");
        }
    }

    static void CheckFullHintLeaves(byte* m)
    {
        foreach (byte[] bytes in new byte[][] { [0x90], [0xf3,0x90], [0x0f,0x1f,0x00],
            [0x0f,0x1f,0x04,0x24], [0x64,0x0f,0x1f,0x80,0xff,0xff,0xff,0x7f],
            [0x65,0x67,0x0f,0x1f,0x84,0xcb,0,0,0,0x80],
            [0x0f,0x1f,0x45,0x7f], [0x0f,0x1f,0x07] })
        foreach (ulong address in new[] { 0UL,0x12345000UL,0x0000800000000000UL,0x0100000000000000UL,ulong.MaxValue })
        {
            Check(FullHintLowering.TryClassify(Ip,bytes,out var i,out _),"R88 nonaccess case admitted");
            using var code=CreateFullBlock([new(i)]);
            ResetFull(m);new Span<byte>(m+16,sizeof(Blink.Machine)-16).Fill(0xa5);HintSeed(m);
            for(int r=0;r<16;++r)Write64(m,24+r*8,address);
            byte[] expected=HintExpected(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray(),Ip+(uint)bytes.Length);
            ulong epoch=11;long issued=0,completed=0;int stopped=0;
            var h=FullHeader(Ip,code.Entry);var frame=FullFrame(m,&h,&epoch,&issued,&completed,&stopped,limit:1);
            // Any data-path use would dereference a poisoned table/descriptor.
            frame.MemoryTlb=(FullMemoryTlbEntry*)1;frame.MemoryTlbMask=63;
            frame.DataDescriptors=(JitDataDescriptor*)1;frame.DataDescriptorCount=4;
            frame.StackMemory=(FullStackMemoryDescriptor*)1;
            frame.FsBase=frame.GsBase=0x0100000000000000UL;
            frame.MemoryAddress=0x12345678;frame.MemorySite=99;frame.MemoryWidth=37;
            Check(RunFull(code,&frame)==FullBlockExit.Continue && issued==1 && completed==1 && frame.Executed==1 &&
                frame.MemoryAssists==0 && frame.NativeMemoryComputations==0 && frame.MemoryResume==0 &&
                frame.MemoryAddress==0x12345678 && frame.MemorySite==99 && frame.MemoryWidth==37 &&
                frame.FaultTrap==0 && frame.FaultAddress==0 && frame.HelperIndex==FullBlockFrame.NoHelperIndex &&
                new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(expected),
                "R88 unmapped/noncanonical NOP EA never checks canonicality, touches guest data or requests an MMU assist");
        }
        CheckFullHintBoundaries(m);
        CheckFullHintWarmNative(m);
    }

    static void CheckFullHintBoundaries(byte* m)
    {
        FullBlockInstruction[] body=[new(new(Ip,2,JitOperation.Pause,64,default,default)),
            new(new(Ip+2,3,JitOperation.Nop,64,default,default)),
            new(new(Ip+5,2,JitOperation.Pause,64,default,default))];
        using var code=CreateFullBlock(body);
        foreach(uint preissued in new uint[]{0,1})
        foreach(string boundary in new[]{"none","stop","attention","invalidated"})
        for(ulong budget=preissued;budget<=4;++budget)
        {
            ResetFull(m);HintSeed(m);if(boundary=="attention")m[11]=1;
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=preissued,boundaryCompleted=0;int stopped=boundary=="stop"?1:0;
            var h=FullHeader(Ip,code.Entry);if(boundary=="invalidated")h.Live=0;
            var frame=FullFrame(m,&h,&epoch,&issued,&boundaryCompleted,&stopped,budget,3);frame.FirstPreissued=preissued;
            FullBlockExit exit=RunFull(code,&frame);
            bool blocked=boundary=="stop" || boundary=="invalidated" || boundary=="attention" && preissued==0;
            int done=blocked?0:boundary=="attention"?1:(int)Math.Min(budget,3UL);
            ulong ip=done switch{0=>Ip,1=>Ip+2,2=>Ip+5,_=>Ip+7};
            FullBlockExit expected=boundary=="stop"?FullBlockExit.Stop:
                boundary=="attention"?FullBlockExit.Attention:boundary=="invalidated"?FullBlockExit.Invalidated:
                budget<3?FullBlockExit.Budget:FullBlockExit.Continue;
            long expectedIssued=blocked?preissued:done+(expected==FullBlockExit.Budget?1:0);
            Check(exit==expected && issued==expectedIssued && boundaryCompleted==done && frame.Executed==(ulong)done &&
                frame.FirstPreissued==(blocked?preissued:0u) && frame.HelperIndex==FullBlockFrame.NoHelperIndex,
                "R88 exact stop/attention/authority precedence, approved issue and rejected lock-xadd budget accounting");
            byte[] state=done==0?before:HintExpected(before,ip);
            Check(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(state),"R88 hint boundary has only boundaryCompleted architectural effects");
        }
        // A deterministic completion-store alias sets the stop cell after the
        // first PAUSE: the very next site must poll, with no second issue.
        ResetFull(m);HintSeed(m);
        ulong global=11;long issue=0,completionAndStop=0;
        var header=FullHeader(Ip,code.Entry);
        var stoppedFrame=FullFrame(m,&header,&global,&issue,&completionAndStop,(int*)&completionAndStop);
        Check(RunFull(code,&stoppedFrame)==FullBlockExit.Stop && issue==1 && completionAndStop==1 &&
            stoppedFrame.Executed==1 && *(ulong*)m==Ip+2,"R88 native PAUSE is followed by the existing instruction-boundary stop poll");

        // Real external CancellationToken -> HostExecutionStop -> native poll.
        using var cancellation=new CancellationTokenSource();
        using var owner=new HostExecutionStop(cancellation:cancellation.Token);
        using var poll=owner.AcquireNativePoll();
        cancellation.Cancel();ResetFull(m);HintSeed(m);issue=1;long completed=0;
        var cancel=FullFrame(m,&header,&global,&issue,&completed,poll.Pointer);cancel.FirstPreissued=1;
        byte[] unchanged=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
        Check(RunFull(code,&cancel)==FullBlockExit.Stop && issue==1 && completed==0 && cancel.FirstPreissued==1 &&
            new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(unchanged),"R88 canceled native poll preserves the unused approved hint issue");

        // A generated backedge reexecutes the single PAUSE site hundreds of
        // times in ONE native invocation, with no per-iteration managed call.
        using var loop=CreateFullBlock([new(new(Ip,2,JitOperation.Pause,64,default,default)),
            new(new(Ip+2,2,JitOperation.Jump,64,default,default,Target:Ip))]);
        foreach(ulong budget in new[]{1UL,2UL,3UL,33UL,512UL,1025UL})
        {
            ResetFull(m);HintSeed(m);issue=completed=0;int running=0;
            var loopHeader=FullHeader(Ip,loop.Entry);
            var nativeLoop=FullFrame(m,&loopHeader,&global,&issue,&completed,&running,budget);
            byte[] expected=HintExpected(new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray(),Ip+(budget%2)*2);
            Check(RunFull(loop,&nativeLoop)==FullBlockExit.Budget && issue==(long)budget+1 &&
                completed==(long)budget && nativeLoop.Executed==budget && nativeLoop.NativeLimit==0 &&
                nativeLoop.HelperIndex==FullBlockFrame.NoHelperIndex && nativeLoop.MemoryAssists==0 &&
                new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).SequenceEqual(expected),
                "R88 one native PAUSE site loops past512 with exact per-instruction issue/completion and no semantic trampoline");
        }
    }

    static void CheckFullHintWarmNative(byte* m)
    {
        using var code=CreateFullBlock([new(new(Ip,2,JitOperation.Pause,64,default,default))]);
        ulong epoch=11;long issued=0,completed=0;int stopped=0;var h=FullHeader(Ip,code.Entry);
        for(int warm=0;warm<16;++warm)
        {
            ResetFull(m);var f=FullFrame(m,&h,&epoch,&issued,&completed,&stopped,ulong.MaxValue,1);
            Check(RunFull(code,&f)==FullBlockExit.Continue,"R88 native warm hint");
        }
        long before=GC.GetAllocatedBytesForCurrentThread();bool good=true;
        for(int repeat=0;repeat<64;++repeat)
        {
            ResetFull(m);issued=completed=0;
            var f=FullFrame(m,&h,&epoch,&issued,&completed,&stopped,1,1);
            good&=RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1;
        }
        Check(good && GC.GetAllocatedBytesForCurrentThread()==before,"R88 warmed native PAUSE invocation allocates zero managed bytes");
    }

    static void CheckFullHintController()
    {
        using var f=new HintFixture();const ulong pc=HintFixture.Base;
        f.Map(pc);f.Fill(pc);
        byte[] bytes=[0xf3,0x90,0x0f,0x1f,0x00,0x64,0x0f,0x1f,0x43,0,
            0xf3,0x0f,0x1e,0xfa,0xf3,0x0f,0x1e,0xfb];
        WriteControllerCode(f,pc,bytes);var m=f.Machine();HintSeed((byte*)m);
        Write64((byte*)m,24,0x0100000000000000UL);Write64((byte*)m,48,0x12345000);
        byte[] gpr=new ReadOnlySpan<byte>((byte*)m+24,128).ToArray();uint flags=m->flags;
        // Test-only exact cap; ordinary product defaults stay unbounded.
        Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=5;
        f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;f.Counts(100);
        using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
        Check(f.Step(pc),"R88 real controller hint block returns");
        for(int attempt=0;f.Completed<5 && attempt<8;++attempt)Check(f.Continue(),"R88 bounded controller compilation continuation");
        Check(m->ip==pc+(uint)bytes.Length && m->oplen==0 && m->flags==flags && f.Issued==5 && f.Completed==5 &&
            new ReadOnlySpan<byte>((byte*)m+24,128).SequenceEqual(gpr) && f.Statistics.SemanticHelperInstructions==0 &&
            f.Statistics.SemanticHelperCalls==0 && f.Statistics.InterpreterAttempts==0 &&
            f.Statistics.NativeMemoryAssists==0 && f.Statistics.NativeMemoryInstructions==0,
            "R88 real selected compiler/controller uses generated PAUSE/NOP/ENDBR with no semantic or data-MMU trampoline");
        var s=f.Jit.Store.GetStatistics();Check(s.ActiveNativeEpochs==0 && s.ActiveManagedLeases==0 &&
            f.Statistics.ActiveDataBorrows==0,"R88 controller hint return releases native/store/data capabilities");
    }

    static int HintByteCount(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> pattern)
    {
        int count=0;for(int b=0;b<=bytes.Length-pattern.Length;++b)
            if(bytes.Slice(b,pattern.Length).SequenceEqual(pattern))++count;
        return count;
    }

    static void CheckFullHintEmission()
    {
        byte[] buffer=new byte[FullBlockEmitter.CodeCapacity],other=new byte[FullBlockEmitter.CodeCapacity];
        byte[] slab=new byte[FullBlockEmitter.ScaffoldCapacity],tiny=new byte[7];
        FullBlockInstruction[] pause=[new(new(Ip,2,JitOperation.Pause,64,default,default))];
        FullBlockInstruction[] nop=[new(pause[0].Instruction with{Operation=JitOperation.Nop})];
        byte[] padded=HintPadding(15,[0x90]);
        foreach(bool windows in new[]{false,true})
        {
            FullBlockEmitter.EmitScaffold(slab,out var layout,windows);
            var addresses=layout.Resolve((nint)0x10000000); // encoding only; never execute this address
            foreach(bool shared in new[]{false,true})
            {
                for(int warm=0;warm<16;++warm)
                    Check(shared?FullBlockEmitter.TryEmit(pause,buffer,in addresses,out _,windows):
                        FullBlockEmitter.TryEmit(pause,buffer,out _,windows),"R88 warm both ABI/scaffold spans");
                long before=GC.GetAllocatedBytesForCurrentThread();bool good=true;
                for(int repeat=0;repeat<32;++repeat)
                {
                    good&=FullHintLowering.TryClassify(Ip,padded,out var i,out _) && i.Length==15;
                    good&=shared?FullBlockEmitter.TryEmit(pause,buffer,in addresses,out _,windows):
                        FullBlockEmitter.TryEmit(pause,buffer,out _,windows);
                }
                Check(good && GC.GetAllocatedBytesForCurrentThread()==before,"R88 warmed classifier/emitter span allocates zero for SysV/Win64 and both scaffolds");
                int length=shared?FullBlockEmitter.Emit(pause,buffer,in addresses,windows):FullBlockEmitter.Emit(pause,buffer,windows);
                int nopLength=shared?FullBlockEmitter.Emit(nop,other,in addresses,windows):FullBlockEmitter.Emit(nop,other,windows);
                Check(length==nopLength+2 && HintByteCount(buffer.AsSpan(0,length),[0xf3,0x90])==1 &&
                    HintByteCount(other.AsSpan(0,nopLength),[0xf3,0x90])==0,
                    "R88 exactly one actual baseline F390 for PAUSE; no operation bytes for same-length NOP");
                Check(buffer.AsSpan(0,7).SequenceEqual(windows?new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xca}:
                    new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xfa}),"R88 host CET entry marker and ABI argument stay separate from guest ENDBR");
                int n;
                bool fits=shared?FullBlockEmitter.TryEmit(pause,tiny,in addresses,out n,windows):
                    FullBlockEmitter.TryEmit(pause,tiny,out n,windows);
                Check(!fits && n==0,"R88 tiny span produces no publishable partial hint body");
            }
        }
    }
}
