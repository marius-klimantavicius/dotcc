using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // SOURCE ONLY / AUTHORED_UNRUN. One selector, one accumulating table. Both
    // ABI encodings are checked; native calls use ONLY the host's real ABI.
    // The new owner rows hold genuine root leases, the SAME store's native
    // epoch, and real SC authority enrollment through RET, with private counters.
    static int CheckFullImmutableGuardR202(byte* machine)
    {
        (string Name, Action Run)[] rows =
        [
            ("both ABI encodings / byte pin / standalone contract", Guard202Encoding),
            ("retained entries / every resume / malformed frames", () => Guard202Entries(machine)),
            ("actual helper RET and fresh authenticated interior resume", () => Guard202Resume(machine)),
            ("actual retained static and dynamic links", () => Guard202Links(machine)),
            ("actual store revocation and enrolled writer after first commit", () => Guard202Races(machine)),
            ("conservative standalone and scaffold boundary contracts", () => Guard202Conservative(machine)),
            ("R195 controller cold/resume / stop / attention / budget", Entry195Native),
            ("R195 actual enrollment races and revocations", Entry195Races),
            ("R195 context fallback and catalog closure", () => { Entry195Fallback(); Entry195Closure(); }),
            ("production ACTIVE x87 / literal catalog continuation", Entry195Active),
            ("selected machine contexts / actual POPF TF-AC-RF effects", CheckCanonicalX87SelectedContexts),
            ("owner shared memory continuations", Entry195Memory),
            ("shared guard canaries / roots / tickets", () => CheckFullSharedMemoryGuardsR188(machine)),
            ("ENV Ready authentication and deferred commit", Completion189Environment),
            ("REP yield / completion / fault cleanup", Completion189Rep),
            ("atomic same-issue admission and mapping discard", Mapping193AtomicDiscard),
            ("real controller static tail authentication", () => { CheckFullLastExitTailPredecessor(); CheckFullLastExitAuthentication(); })
        ];
        var failures = new List<Exception>();
        foreach (var row in rows)
        {
            try { row.Run(); Console.WriteLine("PASS R202 " + row.Name); }
            catch (Exception error)
            { failures.Add(new InvalidOperationException(row.Name, error)); Console.Error.WriteLine(error); }
        }
        if (failures.Count != 0) throw new AggregateException("R202 immutable guard family", failures);
        Console.WriteLine($"PASS immutable guard family checks={checks}");
        return 0;
    }

    static int Guard202Count(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> needle)
    {
        int count = 0;
        for (int i = 0; i <= bytes.Length - needle.Length; ++i)
            if (bytes.Slice(i, needle.Length).SequenceEqual(needle)) ++count;
        return count;
    }

    static void Guard202Encoding()
    {
        foreach (bool windows in new[] { false, true })
        {
            byte[] old = new byte[FullBlockEmitter.ScaffoldCapacity], owner = new byte[old.Length];
            int conservative = FullBlockEmitter.EmitScaffold(old, out var oldLayout, windows);
            int retained = FullBlockEmitter.EmitOwnerScaffold(owner, out var layout, windows);
            Check(retained == conservative - 42 && layout.EntryOffset == oldLayout.EntryOffset &&
                retained == layout.CodeLength && retained <= FullBlockEmitter.ScaffoldCapacity,
                "R202 numerical pin: 2*55 boundary/link bytes removed, 68 entry bytes added, SAME ABIs");
            ReadOnlySpan<byte> identity = [0x48,0x8b,0x08,0x49,0x3b,0x4a,104];
            Check(Guard202Count(old.AsSpan(0,conservative),identity) == 2 &&
                Guard202Count(owner.AsSpan(0,retained),identity) == 1 &&
                Guard202Count(owner.AsSpan(0,layout.LinkOffset),identity) == 1 &&
                Guard202Count(owner.AsSpan(layout.BoundaryOffset,layout.MemoryReadOffset-layout.BoundaryOffset),identity) == 0,
                "R202 identity occurs at owner entry, never in its instruction boundary or source link guard");
            foreach (byte[] mutable in new byte[][] {
                [0x83,0x78,40,1], [0x48,0x8b,0x48,112,0x48,0x85,0xc9],
                [0x49,0x8b,0x8a,0x78,1,0,0], [0x49,0x8b,0x8a,0x88,0,0,0] })
                Check(Guard202Count(owner.AsSpan(0,retained),mutable) == Guard202Count(old.AsSpan(0,conservative),mutable),
                    "R202 Live/code/header/runtime polls are not hoisted or deleted");
            // Both internal JMP ABIs/layouts are unchanged. Fake addresses are
            // used ONLY for emission; no bytes from this encoding table execute.
            var previous = oldLayout.Resolve((nint)0x10000000);
            var current = layout.Resolve((nint)0x10000000);
            foreach (int count in new[] { 1, 16, 256 })
            {
                var sites = new FullBlockInstruction[count];
                for (int i = 0; i < count; ++i) sites[i] = new(new(Ip+(uint)i,1,JitOperation.Nop,64,default,default));
                byte[] a = new byte[FullBlockEmitter.CodeCapacity], b = new byte[a.Length];
                int standalone = FullBlockEmitter.Emit(sites,a,windows);
                Check(Guard202Count(a.AsSpan(0,standalone),identity) == 2,
                    "R202 standalone retains identity checks in boundary AND source link guards");
                int n = FullBlockEmitter.Emit(sites,a,in previous,windows);
                Check(FullBlockEmitter.Emit(sites,b,in current,windows) == n,
                    "R202 body bytes stay the same length; savings occur in executed common guard work");
            }
        }
    }

    delegate void Guard202Prepare(FullBlockFrame* frame, TranslationBlockHeader* header);

    sealed class Guard202Owner : IDisposable
    {
        internal readonly RetainedTranslationStore Store;
        internal readonly FullCodeAuthority Authority;
        readonly NativeCodeAllocation? common;
        readonly FullSharedScaffold scaffold;
        internal Guard202Owner(int kind)
        {
            var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
            Authority = new(io, 8UL << 20, 2, 4L << 20);
            Store = new(new() { BudgetBytes = 4L << 20, MaximumWorkers = 2,
                RegionBytes = 65536, AllowUnverifiedWindows = true, RequireNativeFlags = false });
            if (kind == 0) return; // standalone contract
            byte[] staging = new byte[FullBlockEmitter.ScaffoldCapacity];
            FullSharedScaffoldLayout layout;
            int n = kind == 1 ? FullBlockEmitter.EmitScaffold(staging,out layout) :
                FullBlockEmitter.EmitOwnerScaffold(staging,out layout);
            Check(NativeCodeAllocation.TryCreate(staging.AsSpan(0,n),out var allocation,out string why,
                requireNativeFlags:false),why);
            common = allocation!; scaffold = layout.Resolve(common.Entry);
        }
        internal TranslationToken Publish(FullBlockInstruction[] sites, ulong context = 0x1234)
        {
            uint bytes = checked((uint)(sites[^1].Instruction.Ip+(uint)sites[^1].Instruction.Length-sites[0].Instruction.Ip));
            Check(Store.TryReserve(new(sites[0].Instruction.Ip,context),FullBlockEmitter.CodeCapacity,0,bytes,
                out var draft,out var why),"R202 reserve: "+why);
            var token = draft.Token;
            try
            {
                // This complete-writer-protocol mark is established ONLY while
                // the unique draft is writable, never by changing a live header.
                ((TranslationBlockHeader*)draft.Header)->Reserved136 = 1;
                int n = scaffold.IsValid ? FullBlockEmitter.Emit(sites,draft.Code,in scaffold) : FullBlockEmitter.Emit(sites,draft.Code);
                Check(Store.Publish(ref draft,n,0,Authority.CodeEpoch,Authority.CodeEpoch,out why),"R202 publish: "+why);
                return token;
            }
            finally { Store.Abort(ref draft); }
        }
        internal FullBlockExit Run(TranslationToken token, byte* machine, FullBlockFrame* frame,
            long* issued, long* completed, int* stop, uint resume = 0,
            Guard202Prepare? prepare = null, nint entryOverride = 0)
        {
            Check(Store.TryAcquireToken(token,out var lease),"R202 genuine root lease");
            NativeEpochLease epoch = default; bool enrolled = false;
            try
            {
                var h = (TranslationBlockHeader*)Store.GetHeader(in lease);
                *frame = FullFrame(machine,h,Authority.EpochPointer,issued,completed,stop,codeEpoch:Authority.CodeEpochPointer);
                frame->ResumeIndex = resume;
                frame->StoreGenerationPointer = Store.GenerationPointer;
                frame->ObservedStoreGeneration = Store.Generation;
                Check(Store.TryBeginNativeEpoch(out epoch),"R202 actual root store epoch");
                enrolled = Authority.TryEnterNative(frame->ObservedGlobalEpoch);
                Check(enrolled,"R202 actual SC enrollment required before native entry");
                prepare?.Invoke(frame,h); // test-only: no waiting callbacks
                ulong executed = frame->Executed;
                int result = ((delegate* unmanaged<FullBlockFrame*,int>)(entryOverride == 0 ? h->Entry : entryOverride))(frame);
                RecordQualificationNative(frame,executed);
                Check(result == (int)frame->Exit && frame->NextIp == *(ulong*)machine,
                    "R202 RET materializes typed exit and architectural IP");
                Check(Store.TestNativeEpochs195 > 0 && Authority.FullTestR185NativeFrames > 0,
                    "R202 genuine epoch and runtime enrollment still held AFTER RET");
                return frame->Exit;
            }
            finally
            {
                if (enrolled) Authority.ExitNative();
                try { epoch.Dispose(); } finally { lease.Dispose(); }
            }
        }
        public void Dispose()
        {
            var s = Store.GetStatistics();
            Check(s.ActiveNativeEpochs == 0 && s.ActiveManagedLeases == 0 && Authority.FullTestR185NativeFrames == 0,
                "R202 native return and denials release all capabilities");
            Store.Dispose(); common?.Dispose(); Authority.Dispose();
        }
    }

    static void Guard202Entries(byte* machine)
    {
        foreach (int kind in new[] { 0, 1, 2 })
        using (var o = new Guard202Owner(kind))
        {
            var token = o.Publish([new(new(Ip,1,JitOperation.Nop,64,default,default)),
                new(new(Ip+1,1,JitOperation.Nop,64,default,default)), FullBlockInstruction.Helper(Ip+2,1)]);
            var other = o.Publish([new(new(Ip+32,1,JitOperation.Nop,64,default,default))]);
            Check(o.Store.TryAcquireToken(other,out var otherLease),"R202 other body lease");
            nint wrongEntry;
            try { wrongEntry = o.Store.GetHeaderCopy(in otherLease).Entry; }
            finally { otherLease.Dispose(); }
            foreach (uint resume in new uint[] { 0, 1, 2 })
            foreach (string fault in new[] { "none", "owner", "generation", "context", "wrong-body", "null-header",
                "abi", "size", "preissued", "x87-reserved", "resume", "pc", "runtime", "code-global",
                "null-runtime", "null-code", "stop", "attention", "budget", "cap", "mode", "TF", "RF" })
            {
                ResetFull(machine,Ip+resume);
                byte[] before = new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                long issued = 0, completed = 0; int stop = fault == "stop" ? 1 : 0;
                FullBlockFrame f = default;
                var exit = o.Run(token,machine,&f,&issued,&completed,&stop,resume,(p,h) =>
                {
                    switch (fault)
                    {
                        case "owner": ++p->OwnerCookie; break;
                        case "generation": ++p->BodyGeneration; break;
                        case "context": ++p->Context; break;
                        case "null-header": p->Header = null; break;
                        case "abi": p->Version = 0; break;
                        case "size": p->Size = 96; break;
                        case "preissued": p->FirstPreissued = 2; break;
                        case "x87-reserved": p->X87Reserved = 1; break;
                        case "resume": p->ResumeIndex = 512; break;
                        case "pc": ++*(ulong*)p->Machine; break;
                        case "runtime": ++p->ObservedGlobalEpoch; break;
                        case "code-global": ++p->ObservedCodeAuthorityEpoch; break;
                        case "null-runtime": p->GlobalEpochPointer = null; break;
                        case "null-code": p->CodeAuthorityEpochPointer = null; break;
                        case "attention": ((Blink.Machine*)p->Machine)->attention = true; break;
                        case "budget": p->Budget = 0; break;
                        case "cap": p->NativeLimit = 1; p->Executed = 1; break;
                        case "mode": ((Blink.Machine*)p->Machine)->mode.omode = 0; break;
                        case "TF": ((Blink.Machine*)p->Machine)->flags |= 1u << Blink.FLAGS_TF; break;
                        case "RF": ((Blink.Machine*)p->Machine)->flags |= 1u << Blink.FLAGS_RF; break;
                    }
                },fault == "wrong-body" ? wrongEntry : 0);
                bool helperContext = resume == 2 && (fault is "mode" or "TF" or "RF");
                bool success = fault == "none" || helperContext;
                var expected = success ? FullBlockExit.NeedHelper : fault switch {
                    "stop" => FullBlockExit.Stop, "attention" => FullBlockExit.Attention,
                    "budget" => FullBlockExit.Budget, "cap" => FullBlockExit.Continue, _ => FullBlockExit.Invalidated };
                Check(exit == expected && issued == (success ? 3-resume : fault == "budget" ? 1 : 0) &&
                    completed == (success ? 2-resume : 0) && f.Executed == (success ? 2-resume : fault == "cap" ? 1UL : 0UL),
                    $"R202 entry/resume table kind={kind} index={resume} fault={fault}");
                if (success) Check(f.HelperIndex == 2 && f.ResumeIndex == 3 && f.FirstPreissued == 0 && machine[8] == 1,
                    "R202 helper staging remains after exact issue, without helper completion");
                if (!success && fault is not ("pc" or "attention" or "mode" or "TF" or "RF"))
                    Check(new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(before),
                        "R202 malformed entry has no architectural effect");
            }
        }
    }

    static void Guard202Resume(byte* machine)
    {
        foreach (int kind in new[] { 0, 1, 2 })
        using (var o = new Guard202Owner(kind))
        {
            var token = o.Publish([new(new(Ip,1,JitOperation.Nop,64,default,default)),
                FullBlockInstruction.Helper(Ip+1,2), new(new(Ip+3,1,JitOperation.Nop,64,default,default))]);
            ResetFull(machine); long issued = 1, completed = 0; int stop = 0; FullBlockFrame f = default;
            Check(o.Run(token,machine,&f,&issued,&completed,&stop,prepare:(p,_) => p->FirstPreissued=1) ==
                FullBlockExit.NeedHelper && issued == 2 && completed == 1 && f.FirstPreissued == 0 &&
                f.ResumeIndex == 2 && f.HelperIndex == 1 && f.NextIp == Ip+3,
                "R202 cold approved issue is consumed once before real helper RET");
            // Model ONLY the already-staged helper's commit outside native.
            // The next Run reacquires lease/epoch/enrollment and BODY Entry.
            machine[8] = 0; ++completed;
            Check(o.Run(token,machine,&f,&issued,&completed,&stop,resume:2) == FullBlockExit.Continue &&
                issued == 3 && completed == 3 && f.Executed == 1 && f.NextIp == Ip+4,
                "R202 authenticated helper resume skips completed prefix and preserves exact issue/commit totals");
        }
    }

    static void Guard202Links(byte* machine)
    {
        foreach (int kind in new[] { 0, 1, 2 })
        foreach (bool dynamic in new[] { false, true })
        foreach (bool revoke in new[] { false, true })
        {
            using var o = new Guard202Owner(kind);
            const ulong targetPc = Ip+64;
            JitInstruction transfer = dynamic ? new(Ip,2,JitOperation.IndirectJump,64,default,new(0,0)) :
                new(Ip,5,JitOperation.Jump,64,default,default,Target:targetPc);
            var source = o.Publish([new(transfer)]);
            var target = o.Publish([new(new(targetPc,1,JitOperation.Nop,64,default,default))]);
            Check(o.Store.TryLink(source,1,target),"R202 real RW source link");
            Check(o.Store.TryAcquireToken(target,out var targetLease),"R202 target lease for cache publication");
            var cache = (FullNativeJumpEntry*)NativeMemory.AllocZeroed((nuint)(256*sizeof(FullNativeJumpEntry)));
            try
            {
                var h = (TranslationBlockHeader*)o.Store.GetHeader(in targetLease);
                FullNativeMemory.PublishJump(cache,255,o.Store.Generation,h,in *h);
                ResetFull(machine); *(ulong*)(machine+24) = targetPc;
                long issued = 0, completed = 0; int stop = 0; FullBlockFrame f = default;
                // Legal target revocation before entry, with a retained lease
                // keeping the populated nonowning cache pointer alive. Native
                // traversal then additionally holds the actual global epoch.
                if (revoke) o.Store.MarkAuthorityChanged(targetPc,1);
                var exit = o.Run(source,machine,&f,&issued,&completed,&stop,prepare:(p,_) =>
                { p->NativeLimit = 2; p->JumpCache = cache; p->JumpCacheMask = 255; });
                Check(exit == FullBlockExit.Continue && issued == (revoke ? 1 : 2) && completed == issued &&
                    f.TailJumps == (revoke ? 0UL : 1UL) && f.NextIp == (revoke ? targetPc : targetPc+1),
                    "R202 actual static/dynamic target admission, revocation and exact commit");
            }
            finally { NativeMemory.Free(cache); targetLease.Dispose(); }
        }
    }

    static void Guard202Races(byte* machine)
    {
        foreach (int kind in new[] { 0, 1, 2 })
        foreach (string change in new[] { "live", "runtime", "writer" })
        {
            using var o = new Guard202Owner(kind);
            var token = o.Publish([new(new(Ip,1,JitOperation.Inc,64,new(0,0),default)),
                new(new(Ip+1,5,JitOperation.Jump,64,default,default,Target:Ip))]);
            // Native owns private unmanaged counters; the writer observes only
            // the published first completion, then uses the real revoke/pulse.
            long* counts = (long*)NativeMemory.AllocZeroed(16);
            int* stop = (int*)NativeMemory.AllocZeroed(4);
            using var done = new ManualResetEventSlim();
            Exception? writerError = null;
            var writer = new Thread(() =>
            {
                try
                {
                    var until = DateTime.UtcNow + TimeSpan.FromSeconds(5);
                    while (Volatile.Read(ref counts[1]) == 0 && !done.IsSet)
                    {
                        if (DateTime.UtcNow >= until) throw new TimeoutException("R202 first native completion");
                        Thread.Yield();
                    }
                    if (done.IsSet) throw new InvalidOperationException("R202 native stopped before the writer");
                    if (change == "live") o.Store.MarkAuthorityChanged(Ip,6);
                    else if (change == "runtime") lock (o.Authority.Gate) o.Authority.AdvanceCodePublicationRuntime();
                    else { o.Authority.BeginHostWriter(); o.Authority.EndHostWriter(); }
                }
                catch (Exception error) { writerError = error; Volatile.Write(ref *stop,1); }
            });
            using var deadline = new Timer(_ => Volatile.Write(ref *stop,1),null,TimeSpan.FromSeconds(10),Timeout.InfiniteTimeSpan);
            ResetFull(machine); FullBlockFrame f = default;
            try
            {
                var exit = o.Run(token,machine,&f,counts,counts+1,stop,prepare:(p,_) =>
                { p->Budget = ulong.MaxValue; writer.Start(); });
                done.Set();
                Check(writer.Join(TimeSpan.FromSeconds(5)) && writerError == null,"R202 revoking writer joins");
                Check(exit == FullBlockExit.Invalidated && counts[1] > 0 &&
                    (ulong)counts[1] == f.Executed && counts[0] == counts[1],
                    "R202 later instruction boundary rejects actual mutable revocation after one or more precise commits");
            }
            finally
            {
                done.Set(); Volatile.Write(ref *stop,1);
                if (writer.IsAlive) Check(writer.Join(TimeSpan.FromSeconds(5)),"R202 writer cleanup joins before storage release");
                NativeMemory.Free(counts); NativeMemory.Free(stop);
            }
        }
    }

    static void Guard202Conservative(byte* machine)
    {
        Check(!fullOwnerScaffold.IsValid,"R202 conservative fixture starts clear");
        foreach (bool shared in new[] { false, true })
        {
            using var code = CreateFullScaffold(out var conservative);
            fullOwnerScaffold = shared ? conservative : default;
            try
            {
                CheckFullInstructionBoundaryGuards(machine); // includes forbidden-for-owner counter aliases
                CheckFullLinkAuthentication(machine);
                CheckFullSharedHelperResume(machine);
                CheckFullBoundaryPrecedence(machine);
                CheckFullLargeInternalLoop(machine);
            }
            finally { fullOwnerScaffold = default; }
        }
    }
}
