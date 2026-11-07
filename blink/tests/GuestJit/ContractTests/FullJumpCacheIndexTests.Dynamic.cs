using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using JumpIndexFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // Completes the stopped R109 family. All emission/native/controller checks
    // here are AUTHORED_UNRUN; only the parent's serial gate may execute them.
    static (string Name, byte[] Bytes)[] JumpIndexForms() => [
        ("JMP RAX", [0xff,0xe0]), ("JMP RSP", [0xff,0xe4]),
        ("JMP R12", [0x41,0xff,0xe4]), ("JMP [RBX]", [0xff,0x23]),
        ("JMP [RSP]", [0xff,0x24,0x24]), ("JMP [RBX+RCX*4-8]", [0xff,0x64,0x8b,0xf8]),
        ("CALL RAX", [0xff,0xd0]), ("CALL RSP", [0xff,0xd4]),
        ("CALL R12", [0x41,0xff,0xd4]), ("CALL [RBX]", [0xff,0x13]),
        ("CALL [RSP]", [0xff,0x14,0x24]), ("CALL [RSP-8]", [0xff,0x54,0x24,0xf8]),
        ("RET", [0xc3]), ("RET imm16", [0xc2,0xff,0xff]),
        ("REP RET", [0xf3,0xc3]), ("RET16", [0x66,0xc3]),
        ("RET16 imm16", [0x66,0xc2,0xff,0xff]) ];

    static JitInstruction JumpIndexClassify(ulong pc, byte[] bytes)
    {
        Check(FullNearControlLowering.TryClassify(pc, bytes, out var i, out string? reason),
            reason ?? "R109 exact dynamic guest span classifies");
        Check(FullBlockEmitter.CanInlineNearControl(i) && FullBlockEmitter.ValidateNearControl(i),
            "R109 current strict R169 IR admits the classified source");
        return i;
    }

    static void CheckJumpIndexLayout()
    {
        Check(FullBlockEmitter.HasExpectedLayout() && sizeof(FullNativeJumpEntry) == 64 &&
            Marshal.OffsetOf<FullNativeJumpEntry>(nameof(FullNativeJumpEntry.GuestPc)).ToInt32() == 0 &&
            Marshal.OffsetOf<FullNativeJumpEntry>(nameof(FullNativeJumpEntry.Context)).ToInt32() == 8 &&
            Marshal.OffsetOf<FullNativeJumpEntry>(nameof(FullNativeJumpEntry.StoreGeneration)).ToInt32() == 16 &&
            Marshal.OffsetOf<FullNativeJumpEntry>(nameof(FullNativeJumpEntry.OwnerCookie)).ToInt32() == 24 &&
            Marshal.OffsetOf<FullNativeJumpEntry>(nameof(FullNativeJumpEntry.BodyGeneration)).ToInt32() == 32 &&
            Marshal.OffsetOf<FullNativeJumpEntry>(nameof(FullNativeJumpEntry.Header)).ToInt32() == 40 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.StoreGenerationPointer)).ToInt32() == 296 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.JumpCache)).ToInt32() == 312 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.JumpCacheMask)).ToInt32() == 320,
            "R109 fixed hint/frame offsets, ABI7/frame416/header160; no layout/capacity change");
        Check(FullNativeMemory.WorkerNativeStorageBytes == 33472 &&
            FullNativeMemory.WorkerCharge == 33600 + FullWorkerNativeState.MappingCacheCharge && FullNativeMemory.WorkerChargeFor(4096) == 279360 + FullWorkerNativeState.MappingCacheCharge,
            "R109 current c139 storage/worker charges retain x87 storage and configured TLB charges");

        foreach (ulong upper in new[] { 0UL, 0x123456780000UL, 0xffff800000000000UL })
        {
            var oldSlots = new HashSet<int>(); var newSlots = new HashSet<int>();
            for (ulong n = 0; n < 16; ++n)
            {
                ulong pc = upper | 0x5000 | n;
                oldSlots.Add((int)((pc >> 4) & 255));
                newSlots.Add(FullNativeMemory.JumpIndex(pc, 255));
                // Model the actual emitted 64-bit logical shift/XOR, then
                // EAX AND [frame+320] (zero extension), then byte stride64.
                ulong r8 = pc, rax = pc >> 8;
                rax ^= r8; rax = (uint)rax & 255u;
                Check(rax * 64 == (ulong)FullNativeMemory.JumpIndex(pc, 255) * 64,
                    "R109 unsigned native register model matches managed slot and byte offset without ISA execution");
            }
            Check(oldSlots.Count == 1 && newSlots.Count == 16,
                "R109 adversarial low-nibble cluster: old one slot, candidate sixteen slots");
            oldSlots.Clear(); newSlots.Clear();
            for (ulong n = 0; n < 16; ++n)
            {
                ulong pc = upper | (n << 12) | 1;
                oldSlots.Add((int)((pc >> 4) & 255)); newSlots.Add(FullNativeMemory.JumpIndex(pc, 255));
            }
            Check(oldSlots.Count == 1 && newSlots.Count == 16,
                "R109 page-stride cluster: old one slot, candidate sixteen slots");
        }
        const ulong a = 0x12345003, b = a ^ 0x101;
        Check(FullNativeMemory.JumpIndex(a,255) == FullNativeMemory.JumpIndex(b,255) &&
            ((a >> 4) & 255) != ((b >> 4) & 255) &&
            FullNativeMemory.JumpIndex(a,255) == FullNativeMemory.JumpIndex(a | 0xffff800000000000,255),
            "R109 candidate also creates collisions the old index separates; upper PCs require full tags");
    }

    // Architectural inputs only. The controller owns its Machine metadata;
    // this helper never resets it or invokes a managed instruction semantic.
    static (ulong Target, ulong Rsp, ulong Read) JumpIndexSeed(byte* machine, byte* data,
        ulong page, JitInstruction i, bool cross, ulong target)
    {
        bool call = i.Operation == JitOperation.IndirectCall;
        bool ret = i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust;
        ulong rsp = page + (cross ? (call ? 4103UL : 4095UL) : 384UL);
        ulong read = page + (cross ? 4095UL : 64UL);
        if (i.Source.IsMemory && i.Address.Base == 4)
        {
            rsp = page + (cross ? (i.Address.Displacement == -8 ? 4103UL : 4095UL) : 384UL);
            read = unchecked(rsp + (ulong)i.Address.Displacement);
        }
        Write64(machine,56,rsp);
        if (ret) { read = rsp; if (i.Width == 16) target &= 65535; }
        else if (i.Source.IsMemory)
        {
            if (i.Address.Base != 4)
                Write64(machine,24+i.Address.Base*8,unchecked(read-(ulong)i.Address.Displacement));
            if (i.Address.Index >= 0) Write64(machine,24+i.Address.Index*8,0);
        }
        else if (i.Source.Register == 4) target = rsp;
        else Write64(machine,24+i.Source.Register*8,target);
        if (data != null && (ret || i.Source.IsMemory))
            for (int n=0; n<(ret ? i.Width/8 : 8); ++n) data[(int)(read-page)+n]=(byte)(target>>(n*8));
        return (target,rsp,read);
    }

    static byte[] JumpIndexExpectedMachine(byte* machine, JitInstruction i,
        ulong target, ulong rsp, int done)
    {
        byte[] expected = new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
        if (done == 0) return expected;
        BinaryPrimitives.WriteUInt64LittleEndian(expected,target+(done==2 ? 1UL : 0)); expected[8]=0;
        ulong after = i.Operation == JitOperation.IndirectCall ? rsp-8 :
            i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust ? rsp+(uint)(i.Width/8)+
            (i.Operation==JitOperation.ReturnAdjust ? i.Source.Immediate : 0) : rsp;
        BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(56),after);
        return expected;
    }

    static byte[] JumpIndexExpectedMemory(byte* data, JitInstruction i, ulong page, ulong rsp, bool committed)
    {
        byte[] expected = new ReadOnlySpan<byte>(data,8192).ToArray();
        if (committed && i.Operation==JitOperation.IndirectCall)
            BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan((int)(rsp-page-8)),i.Ip+(uint)i.Length);
        return expected;
    }

    static void CheckJumpIndexDynamicForms(byte* machine)
    {
        byte* data = stackalloc byte[8192];
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        FullNativeJumpEntry* cache = stackalloc FullNativeJumpEntry[256];
        foreach (var form in JumpIndexForms())
        foreach (bool cross in new[] { false,true })
        {
            var i = JumpIndexClassify(Ip,form.Bytes);
            using var source = CreateFullBlock([new(i)]);
            // Include negative canonical data/target addresses with exact tags.
            ulong page = cross ? 0xffff800000300000UL : 0x300000UL;
            ResetFull(machine); new Span<byte>(data,8192).Fill(0xa5);
            var seed = JumpIndexSeed(machine,data,page,i,cross,cross ? 0xffff800012345003UL : 0x12345003UL);
            using var target = CreateFullBlock([new(new(seed.Target,1,JitOperation.Nop,64,default,default))]);
            foreach (uint limit in new[] { 0u,1u,2u })
            foreach (ulong budget in new[] { 0UL,1UL,2UL,3UL })
            foreach (int poll in new[] { 0,1,2 })
            foreach (uint preissued in new[] { 0u,1u })
            {
                ResetFull(machine); new Span<byte>(data,8192).Fill(0xa5);
                JumpIndexSeed(machine,data,page,i,cross,seed.Target);
                if (poll==2) machine[11]=1;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                SetFullMemoryEntry(tlb,page,data); SetFullMemoryEntry(tlb,page+4096,data+4096);
                new Span<FullNativeJumpEntry>(cache,256).Clear();
                ulong global=11, generation=123; long issued=preissued, completed=0; int stop=poll==1 ? 1 : 0;
                var h=FullHeader(Ip,source.Entry); var t=FullHeader(seed.Target,target.Entry,18); t.Reserved136=1;
                FullNativeMemory.PublishJump(cache,255,generation,&t,in t);
                var d=new FullStackMemoryDescriptor { Version=FullStackMemoryDescriptor.CurrentVersion };
                var f=FullFrame(machine,&h,&global,&issued,&completed,&stop,budget,limit);
                f.FirstPreissued=preissued; f.StackMemory=&d; f.MemoryTlb=tlb; f.MemoryTlbMask=63;
                f.JumpCache=cache; f.JumpCacheMask=255; f.StoreGenerationPointer=&generation; f.ObservedStoreGeneration=generation;
                bool polled=poll==1 || poll==2 && preissued==0;
                bool commit=!polled && (preissued==1 || budget!=0);
                ulong tails=commit && limit!=1 ? 1UL : 0;
                int done=commit ? tails!=0 && poll!=2 && budget>1 ? 2 : 1 : 0;
                FullBlockExit exit=poll==1 ? FullBlockExit.Stop : polled ? FullBlockExit.Attention :
                    !commit ? FullBlockExit.Budget : limit==1 ? FullBlockExit.Continue :
                    poll==2 ? FullBlockExit.Attention : budget<=1 ? FullBlockExit.Budget : FullBlockExit.Continue;
                long issues=polled ? preissued : !commit ? 1 : tails!=0 && poll!=2 ? 2 : 1;
                byte[] expected=JumpIndexExpectedMachine(machine,i,seed.Target,seed.Rsp,done);
                byte[] memory=JumpIndexExpectedMemory(data,i,page,seed.Rsp,commit);
                Check(RunFull(source,&f)==exit && issued==issues && completed==done && f.Executed==(ulong)done &&
                    f.TailJumps==tails && f.NativeLimit==limit && f.FirstPreissued==(polled ? preissued : 0) &&
                    f.MemoryAssists==0 && f.HelperIndex==FullBlockFrame.NoHelperIndex &&
                    new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(expected) &&
                    new ReadOnlySpan<byte>(data,8192).SequenceEqual(memory),
                    "R109 grouped "+form.Name+": exact issue/budget/cap/stop/attention, full state, aliases and memory effects");
            }
            CheckJumpIndexFormGuards(machine,data,tlb,cache,i,source,target,page,cross,seed.Target);
            if (i.Source.IsMemory || i.Operation is JitOperation.IndirectCall or JitOperation.Ret or JitOperation.ReturnAdjust)
                CheckJumpIndexFormResume(machine,data,tlb,cache,i,source,target,page,cross,seed.Target);
        }
    }

    static void CheckJumpIndexFormGuards(byte* machine, byte* data, FullMemoryTlbEntry* tlb,
        FullNativeJumpEntry* cache, JitInstruction i, NativeCodeAllocation source, NativeCodeAllocation target,
        ulong page, bool cross, ulong targetPc)
    {
        foreach (string fault in new[] { "collision","generation","entry-generation","context","owner","body","live","code","trust" })
        {
            ResetFull(machine); new Span<byte>(data,8192).Fill(0xa5);
            var seed=JumpIndexSeed(machine,data,page,i,cross,targetPc);
            new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            SetFullMemoryEntry(tlb,page,data); SetFullMemoryEntry(tlb,page+4096,data+4096);
            new Span<FullNativeJumpEntry>(cache,256).Clear();
            ulong global=11,generation=123; long issued=0,completed=0; int stop=0;
            var h=FullHeader(Ip,source.Entry); var t=FullHeader(seed.Target,target.Entry,18); t.Reserved136=1;
            FullNativeMemory.PublishJump(cache,255,generation,&t,in t);
            var f=FullFrame(machine,&h,&global,&issued,&completed,&stop,limit:2);
            var d=new FullStackMemoryDescriptor { Version=FullStackMemoryDescriptor.CurrentVersion };
            f.StackMemory=&d; f.MemoryTlb=tlb; f.MemoryTlbMask=63;
            f.JumpCache=cache; f.JumpCacheMask=255; f.StoreGenerationPointer=&generation; f.ObservedStoreGeneration=123;
            ref var e=ref cache[FullNativeMemory.JumpIndex(seed.Target,255)];
            switch (fault)
            {
                case "collision": e.GuestPc^=0x101; e.Header=(TranslationBlockHeader*)1; break;
                case "generation": ++generation; e.Header=(TranslationBlockHeader*)1; break;
                case "entry-generation": --e.StoreGeneration; e.Header=(TranslationBlockHeader*)1; break;
                case "context": ++e.Context; e.Header=(TranslationBlockHeader*)1; break;
                case "owner": ++e.OwnerCookie; e.Header=(TranslationBlockHeader*)1; break;
                case "body": ++e.BodyGeneration; break;
                case "live": t.Live=0; break;
                case "code": ++t.ValidatedCodeEpoch; break;
                case "trust": t.Reserved136=0; break;
            }
            byte[] expected=JumpIndexExpectedMachine(machine,i,seed.Target,seed.Rsp,1);
            byte[] memory=JumpIndexExpectedMemory(data,i,page,seed.Rsp,true);
            Check(RunFull(source,&f)==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1 && f.TailJumps==0 &&
                new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(expected) &&
                new ReadOnlySpan<byte>(data,8192).SequenceEqual(memory),
                "R109 every dynamic form commits once before denying a colliding/stale/context/owner/body/code/provenance hint");
        }
    }

    static void CheckJumpIndexFormResume(byte* machine, byte* data, FullMemoryTlbEntry* tlb,
        FullNativeJumpEntry* cache, JitInstruction i, NativeCodeAllocation source, NativeCodeAllocation target,
        ulong page, bool cross, ulong targetPc)
    {
        foreach (int changed in new[] { 0,1,2,3,4 }) // unchanged, stop, attention, runtime epoch, code epoch
        {
            ResetFull(machine); new Span<byte>(data,8192).Fill(0xa5);
            var seed=JumpIndexSeed(machine,data,page,i,cross,targetPc);
            new Span<FullMemoryTlbEntry>(tlb,64).Clear(); new Span<FullNativeJumpEntry>(cache,256).Clear();
            ulong global=11,code=11,generation=123; long issued=0,completed=0; int stop=0;
            var h=FullHeader(Ip,source.Entry); var t=FullHeader(seed.Target,target.Entry,18); t.Reserved136=1;
            FullNativeMemory.PublishJump(cache,255,generation,&t,in t);
            var d=new FullStackMemoryDescriptor { Version=FullStackMemoryDescriptor.CurrentVersion };
            var f=FullFrame(machine,&h,&global,&issued,&completed,&stop,budget:1,limit:1,codeEpoch:&code);
            f.StackMemory=&d; f.MemoryTlb=tlb; f.MemoryTlbMask=63;
            f.JumpCache=cache; f.JumpCacheMask=255; f.StoreGenerationPointer=&generation; f.ObservedStoreGeneration=123;
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            byte[] memoryBefore=new ReadOnlySpan<byte>(data,8192).ToArray();
            Check(RunFull(source,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 && f.Executed==0 &&
                f.MemoryResume==1 && f.TailJumps==0 &&
                new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(before) &&
                new ReadOnlySpan<byte>(data,8192).SequenceEqual(memoryBefore),
                "R109 dynamic memory miss reserves one issue before every guest/cache/alias effect");
            SetFullMemoryEntry(tlb,page,data); SetFullMemoryEntry(tlb,page+4096,data+4096);
            f.Budget=0; FullNativeMemory.ResumeSameIssue(&f);
            if (changed==1) stop=1; if (changed==2) machine[11]=1;
            if (changed==3) ++global; if (changed==4) ++code;
            bool commit=changed is 0 or 2;
            byte[] expected=JumpIndexExpectedMachine(machine,i,seed.Target,seed.Rsp,commit ? 1 : 0);
            byte[] memory=JumpIndexExpectedMemory(data,i,page,seed.Rsp,commit);
            Check(RunFull(source,&f)==(changed==1 ? FullBlockExit.Stop : changed>=3 ? FullBlockExit.Invalidated : FullBlockExit.Continue) &&
                issued==1 && completed==(commit ? 1 : 0) && f.Executed==(commit ? 1UL : 0) && f.TailJumps==0 &&
                f.FirstPreissued==(commit ? 0u : 1u) &&
                new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(expected) &&
                new ReadOnlySpan<byte>(data,8192).SequenceEqual(memory),
                "R109 same issue survives exhausted budget; stop/authority deny and approved attention commits exactly once");
        }
    }

    static void JumpIndexControllerWrite(JumpIndexFixture f, ulong address, ulong value, int width)
    {
        f.DataScope(0,()=>
        {
            for (int n=0;n<width/8;++n)
            {
                byte* at=f.Program.LookupAddress2(f.Machine(),unchecked((long)(address+(uint)n)),0,0);
                Check(at!=null,"R109 checked controller input backing"); *at=(byte)(value>>(n*8));
            }
        });
    }

    static byte[] JumpIndexControllerMemory(JumpIndexFixture f, ulong page)
    {
        byte[] bytes=new byte[8192];
        new ReadOnlySpan<byte>((void*)f.Physical(page),4096).CopyTo(bytes);
        new ReadOnlySpan<byte>((void*)f.Physical(page+4096),4096).CopyTo(bytes.AsSpan(4096));
        return bytes;
    }

    static void JumpIndexControllerCode(JumpIndexFixture f, ulong pc, byte[] bytes)
    {
        // JMP RSP can put the two-byte target across a guest page boundary;
        // never assume that two independently mapped host pages are contiguous.
        f.DataScope(0,()=>
        {
            for (int n=0;n<bytes.Length;++n)
            {
                byte* at=f.Program.LookupAddress2(f.Machine(),unchecked((long)(pc+(uint)n)),0,0);
                Check(at!=null,"R109 checked controller code byte backing"); *at=bytes[n];
            }
        });
    }

    static void JumpIndexNoCapabilities(JumpIndexFixture f)
    {
        var s=f.Jit.Store.GetStatistics();
        Check(s.ActiveNativeEpochs==0 && s.ActiveManagedLeases==0 && f.Statistics.ActiveDataBorrows==0 &&
            f.Statistics.SemanticHelperInstructions==0 && f.Statistics.SemanticHelperCalls==0 && f.Statistics.InterpreterAttempts==0,
            "R109 actual controller releases every capability without semantic helpers/interpreter fallback");
    }

    static void CheckJumpIndexDynamicController()
    {
        foreach (var form in JumpIndexForms())
        foreach (bool cross in new[] { false,true })
        {
            using var f=new JumpIndexFixture();
            ulong pc=JumpIndexFixture.Base+1, page=JumpIndexFixture.Base+0x20000;
            f.Map(JumpIndexFixture.Base); f.Fill(JumpIndexFixture.Base);
            f.Map(page,8192); f.Fill(page,8192); JumpIndexControllerCode(f,pc,form.Bytes);
            var i=JumpIndexClassify(pc,form.Bytes); var m=f.Machine();
            var seed=JumpIndexSeed((byte*)m,null,page,i,cross,i.Width==16 ? 0x8003UL : pc+128);
            if (i.Width==16) { f.Map(seed.Target&~4095UL); f.Fill(seed.Target&~4095UL); }
            JumpIndexControllerCode(f,seed.Target,[0xeb,0xfe]); // one terminating guest instruction
            JumpIndexWarm(f,pc); JumpIndexWarm(f,seed.Target);
            Action<nint> cap1=p=>((FullBlockFrame*)p)->NativeLimit=1;
            Action<nint> cap2=p=>((FullBlockFrame*)p)->NativeLimit=2;
            f.NativePrepared=cap1; f.NativeReturned=_=>f.NativePrepared=cap1;
            f.Counts(10000);
            try
            {
                // Actual source admission/refill, then actual target publication.
                if (i.Source.IsMemory || i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust)
                    JumpIndexControllerWrite(f,seed.Read,seed.Target,i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust ? i.Width : 64);
                Check(f.Step(pc),"R109 actual controller source warm");
                for (int retry=0;retry<8 && f.Completed==0;++retry) Check(f.Continue(),"R109 source same-issue warm resume");
                Check(f.Completed==1 && f.Issued==1,"R109 every decoded source warms with one issue/commit");
                Check(f.Step(seed.Target),"R109 target publication uses the actual retained controller");
                JumpIndexSeed((byte*)m,null,page,i,cross,seed.Target);
                if (i.Source.IsMemory || i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust)
                    JumpIndexControllerWrite(f,seed.Read,seed.Target,i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust ? i.Width : 64);
                // CALL RSP writes the page containing its own target body. The
                // existing code-writer ticket MUST force a return after CALL.
                bool writerAlias=i.Operation==JitOperation.IndirectCall && !i.Source.IsMemory && i.Source.Register==4;
                // Existing eligibility deliberately keeps a two-byte target straddling pages cap1.
                // Its nonowning hint must not grant a native tail into that body.
                bool cappedTarget=(seed.Target&4095)==4095;
                int done=writerAlias || cappedTarget ? 1 : 2;
                ulong completed=f.Completed, issued=f.Issued;
                var storeBefore=f.Jit.Store.GetStatistics();
                byte[] memory=JumpIndexControllerMemory(f,page);
                if (i.Operation==JitOperation.IndirectCall)
                    BinaryPrimitives.WriteUInt64LittleEndian(memory.AsSpan((int)(seed.Rsp-page-8)),pc+(uint)i.Length);
                byte[] registers=new ReadOnlySpan<byte>((byte*)m+24,128).ToArray(); uint flags=m->flags;
                ulong expectedRsp=i.Operation==JitOperation.IndirectCall ? seed.Rsp-8 :
                    i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust ? seed.Rsp+(uint)(i.Width/8)+
                    (i.Operation==JitOperation.ReturnAdjust ? i.Source.Immediate : 0) : seed.Rsp;
                BinaryPrimitives.WriteUInt64LittleEndian(registers.AsSpan(32),expectedRsp);
                bool chain=false, ticketReturn=false;
                f.NativePrepared=cap2;
                f.NativeReturned=p=>
                {
                    var frame=(FullBlockFrame*)p; f.NativePrepared=cap2;
                    chain|=frame->Executed==2 && frame->TailJumps==1;
                    ticketReturn|=frame->Executed==1 && frame->TailJumps==0 &&
                        (frame->MemoryFlags&FullMemoryFlags.BarrierConsumed)!=0;
                };
                Check(f.Step(pc),"R109 grouped actual dynamic controller entry");
                for (int retry=0;retry<8 && f.Completed==completed;++retry)
                    Check(f.Continue(),"R109 grouped actual dynamic controller same-issue resume");
                Check(f.Completed==completed+(uint)done && f.Issued==issued+(uint)done &&
                    m->ip==seed.Target && m->oplen==0 && m->flags==flags &&
                    new ReadOnlySpan<byte>((byte*)m+24,128).SequenceEqual(registers) &&
                    JumpIndexControllerMemory(f,page).AsSpan().SequenceEqual(memory) &&
                    (writerAlias ? ticketReturn && !chain : cappedTarget ? !chain && !ticketReturn : chain),
                    $"R109 {form.Name} cross={cross}: completed={f.Completed-completed}/{done} issued={f.Issued-issued} ip={m->ip:x}/{seed.Target:x} oplen={m->oplen} flags={m->flags:x}/{flags:x} regs={new ReadOnlySpan<byte>((byte*)m+24,128).SequenceEqual(registers)} memory={JumpIndexControllerMemory(f,page).AsSpan().SequenceEqual(memory)} chain={chain} ticket={ticketReturn} alias={writerAlias}");
                var storeAfter=f.Jit.Store.GetStatistics();
                Check(storeAfter.ChargedBytes<=JumpIndexFixture.Budget && storeAfter.BodyCapacity==storeBefore.BodyCapacity &&
                    storeAfter.CapacityRetirements==storeBefore.CapacityRetirements && storeAfter.TotalFlushes==storeBefore.TotalFlushes,
                    "R109 dynamic family fits the same retained capacity/budget without a cache-policy change");
                JumpIndexNoCapabilities(f);
            }
            finally { f.NativePrepared=null; f.NativeReturned=null; }
        }
    }

    static void CheckJumpIndexControllerAdmission()
    {
        foreach (var form in JumpIndexForms())
        foreach (int boundary in new[] { 0,1,2 }) // last budget issue, late stop, late attention
        {
            using var f=new JumpIndexFixture(); ulong pc=JumpIndexFixture.Base+1, page=pc+0x1ffff;
            f.Map(JumpIndexFixture.Base); f.Fill(JumpIndexFixture.Base);
            f.Map(page,8192); f.Fill(page,8192); JumpIndexControllerCode(f,pc,form.Bytes);
            var i=JumpIndexClassify(pc,form.Bytes); var m=f.Machine();
            var seed=JumpIndexSeed((byte*)m,null,page,i,true,i.Width==16 ? 0x8003UL : pc+128);
            if (i.Source.IsMemory || i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust)
                JumpIndexControllerWrite(f,seed.Read,seed.Target,i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust ? i.Width : 64);
            byte[] memory=JumpIndexControllerMemory(f,page);
            if (boundary!=1 && i.Operation==JitOperation.IndirectCall)
                BinaryPrimitives.WriteUInt64LittleEndian(memory.AsSpan((int)(seed.Rsp-page-8)),pc+(uint)i.Length);
            byte[] registers=new ReadOnlySpan<byte>((byte*)m+24,128).ToArray(); uint flags=m->flags;
            ulong rsp=boundary==1 ? seed.Rsp : i.Operation==JitOperation.IndirectCall ? seed.Rsp-8 :
                i.Operation is JitOperation.Ret or JitOperation.ReturnAdjust ? seed.Rsp+(uint)(i.Width/8)+
                (i.Operation==JitOperation.ReturnAdjust ? i.Source.Immediate : 0) : seed.Rsp;
            BinaryPrimitives.WriteUInt64LittleEndian(registers.AsSpan(32),rsp);
            Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;
            f.NativePrepared=cap; f.NativeReturned=_=>f.NativePrepared=cap;
            f.Preissued=(_,_)=>
            {
                if (boundary==1) f.Stop.RequestStop();
                if (boundary==2) f.Machine()->attention=true;
            };
            f.Counts(1);
            try
            {
                Check(f.Step(pc),"R109 grouped controller last issue/late stop/late attention");
                for (int retry=0;retry<8 && boundary!=1 && f.Completed==0;++retry)
                    Check(f.Continue(),"R109 checked memory retry retains the final approved issue");
                Check(f.Issued==1 && f.Completed==(boundary==1 ? 0UL : 1UL) &&
                    m->ip==(boundary==1 ? pc : seed.Target) && m->oplen==0 && m->flags==flags &&
                    new ReadOnlySpan<byte>((byte*)m+24,128).SequenceEqual(registers) &&
                    JumpIndexControllerMemory(f,page).AsSpan().SequenceEqual(memory),
                    "R109 "+form.Name+": actual controller preserves issue/stop/budget/aliases and precise memory effects");
                JumpIndexNoCapabilities(f);
            }
            finally { f.Preissued=null; f.NativePrepared=null; f.NativeReturned=null; }
        }
    }

    static void CheckJumpIndexControllerGuards()
    {
        using var f=new JumpIndexFixture(); ulong pc=JumpIndexFixture.Base+1, target=pc+128;
        f.Map(JumpIndexFixture.Base); f.Fill(JumpIndexFixture.Base);
        JumpIndexControllerCode(f,pc,[0xff,0xe0]); JumpIndexControllerCode(f,target,[0xeb,0xfe]);
        JumpIndexWarm(f,pc); JumpIndexWarm(f,target); f.Counts(10000);
        foreach (string fault in new[] { "collision","generation","context","owner" })
        {
            f.NativePrepared=p=>((FullBlockFrame*)p)->NativeLimit=1;
            Check(f.Step(target),"R109 controller publishes a fresh real target hint");
            Write64((byte*)f.Machine(),24,target);
            ulong done=f.Completed, issued=f.Issued; long tails=f.Statistics.NativeTailJumps;
            bool poisoned=false;
            f.NativePrepared=p=>
            {
                var frame=(FullBlockFrame*)p; frame->NativeLimit=2;
                ref var e=ref frame->JumpCache[FullNativeMemory.JumpIndex(target,255)];
                Check(e.GuestPc==target,"R109 actual worker hint is present before adversarial mutation");
                if (fault=="collision") e.GuestPc^=0x101;
                else if (fault=="generation") --e.StoreGeneration;
                else if (fault=="context") ++e.Context;
                else ++e.OwnerCookie;
                e.Header=(TranslationBlockHeader*)1; poisoned=true;
            };
            try
            {
                Check(f.Step(pc) && poisoned && f.Completed==done+1 && f.Issued==issued+1 &&
                    f.Machine()->ip==target && f.Statistics.NativeTailJumps==tails,
                    "R109 actual worker full-tag/generation/context/owner denial precedes poisoned retained-header dereference");
            }
            finally { f.NativePrepared=null; }
            JumpIndexNoCapabilities(f);
        }
        // Replace the real target after its hint was published. Do not forge
        // epochs or preserve a raw pointer outside the native lifetime scope.
        f.NativePrepared=p=>((FullBlockFrame*)p)->NativeLimit=1;
        Check(f.Step(target),"R109 target hint before real content invalidation");
        var before=f.Statistics;
        JumpIndexControllerCode(f,target,[0xba,0x78,0x56,0x34,0x12,0xeb,0xfe]);
        Write64((byte*)f.Machine(),24,target); Write64((byte*)f.Machine(),40,0);
        ulong completed=f.Completed, attempts=f.Issued;
        f.NativePrepared=p=>((FullBlockFrame*)p)->NativeLimit=2;
        Check(f.Step(pc) && f.Completed==completed+1 && f.Issued==attempts+1 &&
            f.Machine()->ip==target && ((ulong*)((byte*)f.Machine()+40))[0]==0 &&
            f.Statistics.NativeTailJumps==before.NativeTailJumps,
            "R109 real invalidation rejects the old target hint after source commit");
        f.NativePrepared=p=>((FullBlockFrame*)p)->NativeLimit=1;
        Check(f.Step(target) && ((ulong*)((byte*)f.Machine()+40))[0]==0x12345678 &&
            f.Statistics.ContentInvalidations>before.ContentInvalidations,
            "R109 real target recompilation executes only the replacement body");
        f.NativePrepared=null; JumpIndexNoCapabilities(f);
    }
}
