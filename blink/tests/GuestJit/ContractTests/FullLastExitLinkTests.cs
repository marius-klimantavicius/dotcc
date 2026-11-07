using System.Buffers.Binary;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using LinkFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE-AUTHORED, UNRUN by R92. Parent owns Main wiring, all emission,
    // builds and native/controller execution against its frozen source set.
    static void CheckFullLastExitLinks(byte* machine)
    {
        Check(sizeof(FullBlockFrame) == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(TranslationBlockHeader) == 160, "R92 retains ABI5/frame400/header160");
        CheckFullLastExitTailPredecessor();
        CheckFullLastExitStaticEdges();
        CheckFullLastExitOrdinaryFallthrough();
        CheckFullLastExitCrossPageRepetition();
        CheckFullLastExitDeniedReturns();
        CheckFullLastExitHelperResume();
        CheckFullLastExitAuthentication();
        CheckFullLastExitRevocation();
        CheckFullLastExitAlias();
        CheckFullLastExitLateBoundaries();
        ResetFull(machine);
    }

    static byte[] LastExitJump(ulong from, ulong to) =>
        [0xe9, .. BitConverter.GetBytes(checked((int)((long)to - (long)from - 5)))];

    static byte[] LastExitMarker(uint value) =>
        [0xb8, .. BitConverter.GetBytes(value), 0xeb, 0xf9]; // MOV; internal JMP to MOV

    static TranslationToken WarmLastExit(LinkFixture f, ulong pc, uint requestedCount = 0)
    {
        Check(f.Acquire(pc, 0, out var lease, out bool multiple, out int error, requestedCount) &&
            multiple && error == 0, "R92 acquires a real stable retained body");
        using (lease)
        {
            lock (f.Jit.Authority.Gate)
                f.Jit.AuthorizeLinks(in lease, multiple, nativeLimit: requestedCount == 0 ? 0u : 1u);
            return lease.Token;
        }
    }

    static TranslationBlockHeader LastExitHeader(LinkFixture f, TranslationToken token)
    {
        Check(f.Jit.Store.TryAcquireToken(token, out var lease), "R92 inspect resident source by token");
        using (lease) return f.Jit.Store.GetHeaderCopy(in lease);
    }

    static void LastExitClean(LinkFixture f)
    {
        var store = f.Jit.Store.GetStatistics();
        Check(store.ActiveNativeEpochs == 0 && store.ActiveManagedLeases == 0 &&
            f.Statistics.ActiveDataBorrows == 0 && f.Statistics.ActivePhysicalCodeBorrows == 0,
            "R92 controller return releases store epochs, leases and physical/passive pins");
    }

    static void CheckFullLastExitTailPredecessor()
    {
        using var f = new LinkFixture();
        ulong a = LinkFixture.Base, b = a + 32, c = a + 64;
        f.Map(a); f.Fill(a);
        WriteControllerCode(f, a, [0x90, .. LastExitJump(a + 1, b)]);
        WriteControllerCode(f, b, [0x90, .. LastExitJump(b + 1, c)]);
        WriteControllerCode(f, c, [0x90, .. LastExitJump(c + 1, a)]);
        var source = WarmLastExit(f, b); // C is absent when B is authorized.
        WarmLastExit(f, a);             // A may already chain natively into B.
        var before = LastExitHeader(f, source);
        Check(before.Link1TargetHeader == 0 && before.Link1Pc == 0,
            "R92 counterexample begins with absent target AND absent stored linkPc");
        f.Counts(1000);
        Check(f.Step(a) && f.Machine()->ip == c && f.Completed == 4 && f.Issued == 4 &&
            f.ResumeToken() == source && f.Statistics.NativeEntries == 1 && f.Statistics.NativeTailJumps == 1,
            "R92 predecessor B is reached by native A->B, never by managed Acquire(B)");
        var candidate = f.Owner.FullTestLastExit;
        Check(candidate.Source == source && candidate.SourcePc == b && candidate.SitePc == b + 1 &&
            candidate.TargetPc == c && candidate.Edge == 1 && candidate.SiteIndex == 1,
            "R92 copies exact terminal source/edge/target evidence before epoch release");
        LastExitClean(f);
        Check(f.Continue() && f.Completed == 1000 && f.Issued == 1001 &&
            f.Stop.Reason == HostExecutionStopReason.Budget && f.Statistics.NativeEntries == 2 &&
            f.Statistics.NativeTailJumps > 400,
            "R92 target resolution backpatches B; warm A/B/C chain has no repeated managed returns");
        var after = LastExitHeader(f, source);
        Check(after.Link1TargetHeader != 0 && after.Link1Pc == c && after.Link0TargetHeader == 0 &&
            f.Owner.FullTestLastExit.Source.Generation == 0 && f.Statistics.SemanticHelperInstructions == 0 &&
            f.Statistics.InterpreterAttempts == 0 && f.Statistics.LastExitLinkAttempts == 1 &&
            f.Statistics.LastExitLinksCompleted == 1, "R92 warm chain records exactly one real backpatch and clears budget candidate");
        LastExitClean(f);
    }

    static void CheckFullLastExitStaticEdges()
    {
        foreach (bool taken in new[] { false, true })
            CheckFullLastExitBranch(0x75, false, taken);
        foreach (byte opcode in new byte[] { 0xe0, 0xe1, 0xe2, 0xe3 })
        foreach (bool address32 in new[] { false, true })
        foreach (bool taken in new[] { false, true }) CheckFullLastExitBranch(opcode, address32, taken);
    }

    static void CheckFullLastExitBranch(byte opcode, bool address32, bool taken)
    {
        using var f = new LinkFixture();
        ulong sourcePc = LinkFixture.Base, targetPc = sourcePc + 64;
        int length = address32 ? 3 : 2;
        ulong selectedPc = taken ? targetPc : sourcePc + (uint)length;
        f.Map(sourcePc); f.Fill(sourcePc);
        byte displacement = checked((byte)(64 - length));
        WriteControllerCode(f, sourcePc, address32 ? [0x67, opcode, displacement] : [opcode, displacement]);
        WriteControllerCode(f, sourcePc + (uint)length, LastExitMarker(0x13579bdf));
        WriteControllerCode(f, targetPc, LastExitMarker(0x2468ace0));
        var source = WarmLastExit(f, sourcePc);
        ulong low = opcode == 0xe3 ? (taken ? 0UL : 1UL) : opcode == 0xe2 ? (taken ? 2UL : 1UL) : 2UL;
        ulong counter = low | (address32 ? 1UL << 32 : 0UL);
        BinaryPrimitives.WriteUInt64LittleEndian(new((byte*)f.Machine() + 32, 8), counter);
        bool zf = opcode switch { 0x75 => !taken, 0xe0 => !taken, 0xe1 => taken, _ => false };
        f.Machine()->flags = (f.Machine()->flags & ~(1u << Blink.FLAGS_ZF)) | (zf ? 1u << Blink.FLAGS_ZF : 0);
        uint flags = f.Machine()->flags;
        f.Counts(101);
        Check(f.Step(sourcePc) && f.Machine()->ip == selectedPc && f.Completed == 1 && f.Issued == 1,
            $"R92 generated selected branch exit opcode={opcode:x2} ea32={address32} taken={taken}");
        var candidate = f.Owner.FullTestLastExit;
        Check(candidate.Source == source && candidate.Edge == (taken ? 1 : 0) && candidate.TargetPc == selectedPc,
            "R92 Jcc/counter selected static edge matches generated result");
        ulong expectedCounter = opcode is 0x75 or 0xe3 ? counter : address32 ? unchecked((uint)(low - 1)) : counter - 1;
        Check(BinaryPrimitives.ReadUInt64LittleEndian(new((byte*)f.Machine() + 32, 8)) == expectedCounter &&
            f.Machine()->flags == flags, "R92 linking never recomputes counters or changes guest flags");
        Check(f.Continue() && f.Completed == 101 && f.Issued == 102 && f.Statistics.NativeEntries == 2,
            "R92 conditional target compiles later and executes through one native entry");
        var header = LastExitHeader(f, source);
        Check((taken ? header.Link1TargetHeader : header.Link0TargetHeader) != 0 &&
            (taken ? header.Link1Pc : header.Link0Pc) == selectedPc &&
            (taken ? header.Link0TargetHeader : header.Link1TargetHeader) == 0 &&
            FullRax(f) == (taken ? 0x2468ace0UL : 0x13579bdfUL) &&
            f.Statistics.LastExitLinkAttempts == 1 && f.Statistics.LastExitLinksCompleted == 1,
            "R92 patches only the executed branch edge and preserves selected marker");
        LastExitClean(f);
    }

    static void CheckFullLastExitOrdinaryFallthrough()
    {
        using var f = new LinkFixture();
        ulong pc = LinkFixture.Base;
        f.Map(pc); f.Fill(pc);
        WriteControllerCode(f, pc, Enumerable.Repeat((byte)0x90, 512).ToArray());
        WriteControllerCode(f, pc + 512, LastExitMarker(0x55aa));
        var source = WarmLastExit(f, pc);
        var header = LastExitHeader(f, source);
        ulong target = pc + header.GuestLength;
        uint sites = header.PayloadBytes / (uint)sizeof(FullTranslationJit.Plan);
        f.Counts(1000);
        Check(sites > 1 && f.Step(pc) && f.Completed == sites && f.Machine()->ip == target &&
            f.Owner.FullTestLastExit.Source == source && f.Owner.FullTestLastExit.SiteIndex == sites - 1 &&
            f.Owner.FullTestLastExit.Edge == 0, "R92 terminal ordinary fallthrough supplies actual last-site evidence");
        Check(f.Continue() && LastExitHeader(f, source).Link0Pc == target && f.Completed == 1000 && f.Issued == 1001,
            "R92 ordinary fallthrough backpatches a later compiled entry0 target");
        LastExitClean(f);
    }

    static void CheckFullLastExitCrossPageRepetition()
    {
        using var f = new LinkFixture();
        ulong page = LinkFixture.Base, a = page + 4064, b = page + 4080, target = page + 4096;
        f.Map(page, 8192); f.Fill(page, 8192);
        WriteControllerCode(f, a, LastExitJump(a, b));
        WriteControllerCode(f, b, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90]);
        WriteControllerCode(f, target, LastExitMarker(0x1234));
        // Start sixteen bytes before the end: compiling at the final byte
        // speculatively registers an already committed possible second page.
        // A sixteen-NOP suffix reaches the same page edge without that first
        // snapshot lookahead, leaving target genuinely unregistered as code.
        var source = WarmLastExit(f, b); WarmLastExit(f, a);
        f.Counts(1000);
        Check(f.Step(a) && f.Machine()->ip == target && f.Owner.FullTestLastExit.Source == source,
            "R92 native predecessor exits to not-yet-code target page");
        Check(f.Continue() && f.Completed == 18 && f.Machine()->ip == target + 5 &&
            LastExitHeader(f, source).Link0TargetHeader == 0 && f.Owner.FullTestLastExit.Source.Generation == 0,
            "R92 first-code runtime pulse keeps target cap1 and refuses predecessor patch");
        // A stable ordinary target acquisition can now grant trust. Attention
        // bounds this real controller call after its first approved MOV.
        f.Machine()->attention = true;
        Check(f.Step(target) && f.Completed == 19 && f.Machine()->ip == target + 5,
            "R92 stable target is acquired through actual ordinary controller");
        f.Machine()->attention = false;
        Check(LastExitHeader(f, source).Link0TargetHeader == 0,
            "R92 target trust alone did not retroactively patch any predecessor");
        Check(f.Step(a) && f.Completed == 36 && f.Machine()->ip == target &&
            f.ResumeToken() == source && f.Owner.FullTestLastExit.Source == source &&
            LastExitHeader(f, source).Link0TargetHeader == 0,
            "R92 stable repetition reencounters B by native chain with missing edge");
        Check(f.Continue() && LastExitHeader(f, source).Link0Pc == target &&
            f.Completed == 1000 && f.Issued == 1001 && f.Statistics.NativeEntries == 5 &&
            f.Statistics.LastExitLinkAttempts == 2 && f.Statistics.LastExitLinksCompleted == 1,
            "R92 later stable repetition backpatches cross-page edge without a trust shortcut");
        LastExitClean(f);
    }

    static void CheckFullLastExitDeniedReturns()
    {
        foreach (string kind in new[] { "signal", "cap", "helper", "dynamic", "ambiguous", "internal-cap", "nonterminal-cap", "invalidated-entry" })
        {
            using var f = new LinkFixture(helpersOnly: kind == "helper");
            ulong pc = LinkFixture.Base, target = pc + 64;
            f.Map(pc); f.Fill(pc);
            byte[] sourceBytes = kind switch
            {
                "dynamic" => [0xff, 0xe0], // JMP RAX goes through guarded jump cache
                "ambiguous" => [0x75, 0], // both static edges name pc+2
                "internal-cap" => [0x90, 0xeb, 0xfd],
                "nonterminal-cap" => [0x90, .. LastExitJump(pc + 1, target)],
                _ => LastExitJump(pc, target)
            };
            WriteControllerCode(f, pc, sourceBytes);
            if (kind == "ambiguous") target = pc + 2;
            WriteControllerCode(f, target, LastExitMarker(7));
            if (kind != "helper") WarmLastExit(f, pc, kind == "signal" ? 1u : 0u);
            f.Counts(100);
            if (kind is "cap" or "internal-cap" or "nonterminal-cap")
                f.NativePrepared = pointer => ((FullBlockFrame*)pointer)->NativeLimit = kind == "internal-cap" ? 2u : 1u;
            if (kind == "invalidated-entry") f.NativePrepared = pointer =>
            {
                // A real pre-entry epoch mismatch must retry the SAME approved
                // issue. Attention then bounds execution at the native branch.
                f.Jit.Authority.AdvanceCode();
                f.Machine()->attention = true;
            };
            if (kind == "helper") f.Machine()->attention = true;
            if (kind == "dynamic") FullRax(f, target);
            Check(f.Step(pc, signalActor: kind == "signal") && f.Completed > 0 &&
                f.Owner.FullTestLastExit.Source.Generation == 0,
                $"R92 {kind} return cannot mint a last-exit candidate");
            Check(f.Statistics.InterpreterAttempts == 0, "R92 rejected candidate does not fall back to interpreter");
            Check(f.Statistics.LastExitLinkAttempts == 0 && f.Statistics.LastExitLinksCompleted == 0,
                "R92 default/rejected returns do not inflate candidate consumption counters");
            LastExitClean(f);
        }
    }

    static bool CompleteLastExit(LinkFixture f, in FullTranslationJit.LastExitLink candidate,
        in TranslationLease target, uint index = 0, bool multiple = true, uint nativeLimit = 0, ulong? context = null)
    {
        lock (f.Jit.Authority.Gate)
            return f.Jit.CompleteLastExitLink(in candidate, in target, index, multiple, nativeLimit,
                context ?? f.Jit.Store.GetHeaderCopy(in target).Context, f.Owner.FullTestLastExitInstructions);
    }

    static void CheckFullLastExitHelperResume()
    {
        foreach (bool cap in new[] { false, true })
        {
            using var f = new LinkFixture();
            ulong pc = LinkFixture.Base, target = pc + 64, stack = pc + 4096;
            f.Map(pc, 8192); f.Fill(pc);
            WriteControllerCode(f, pc, [0x9c, 0x90, .. LastExitJump(pc + 2, target)]); // typed PUSHF; native NOP/JMP
            WriteControllerCode(f, target, LastExitMarker(13));
            BinaryPrimitives.WriteUInt64LittleEndian(new((byte*)f.Machine() + 24 + 4 * 8, 8), stack + 128);
            var source = WarmLastExit(f, pc);
            f.Counts(101);
            bool helperExit = false;
            f.NativeReturned = pointer =>
            {
                if (((FullBlockFrame*)pointer)->Exit == FullBlockExit.NeedHelper)
                {
                    helperExit = true;
                    Check(f.Owner.FullTestLastExit.Source.Generation == 0,
                        "R92 typed helper return has no last-exit candidate");
                }
            };
            if (cap) f.NativePrepared = pointer =>
            {
                Check(((FullBlockFrame*)pointer)->ResumeIndex == 0, "R92 helper starts at site0");
                f.NativePrepared = resumed =>
                {
                    Check(((FullBlockFrame*)resumed)->ResumeIndex == 1, "R92 actual controller resumes native site1 after helper");
                    ((FullBlockFrame*)resumed)->NativeLimit = 1;
                };
            };
            Check(f.Step(pc) && helperExit && f.Statistics.SemanticHelperInstructions == 1 &&
                f.Completed == (cap ? 2UL : 3UL) && f.Issued == f.Completed,
                "R92 helper resume preserves one helper plus exactly the reached native work");
            if (cap) Check(f.Owner.FullTestLastExit.Source.Generation == 0 &&
                f.Machine()->ip == pc + 2 && LastExitHeader(f, source).Link1TargetHeader == 0,
                "R92 helper-resumed NONTERMINAL Continue cannot mint an edge");
            else
            {
                Check(f.Owner.FullTestLastExit.Source == source && f.Owner.FullTestLastExit.SiteIndex == 2 &&
                    f.Owner.FullTestLastExit.SitePc == pc + 2 && f.Machine()->ip == target,
                    "R92 helper-resumed TERMINAL native JMP supplies its own executed-site evidence");
                Check(f.Continue() && LastExitHeader(f, source).Link1Pc == target &&
                    f.Completed == 101 && f.Issued == 102 && f.Statistics.NativeEntries == 3,
                    "R92 only the later native terminal exit can backpatch after a typed helper");
            }
            LastExitClean(f);
        }
    }

    static void CheckFullLastExitAuthentication()
    {
        using var f = new LinkFixture();
        ulong pc = LinkFixture.Base, targetPc = pc + 64;
        f.Map(pc); f.Fill(pc);
        WriteControllerCode(f, pc, LastExitJump(pc, targetPc));
        WriteControllerCode(f, targetPc, LastExitMarker(9));
        var source = WarmLastExit(f, pc);
        f.Counts(100);
        Check(f.Step(pc), "R92 authentication witness executes its predecessor");
        var original = f.Owner.FullTestLastExit;
        Check(original.Source == source, "R92 authentication starts from a real generated candidate");
        Check(f.Acquire(targetPc, 0, out var target, out bool multiple, out int error) && multiple && error == 0,
            "R92 authentication target compiles later");
        using (target)
        {
            lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in target, multiple);
            FullTranslationJit.LastExitLink[] denied = [default,
                original with { Source = new(source.Slot, source.Generation + 1) },
                original with { StoreGeneration = original.StoreGeneration + 1 },
                original with { OwnerCookie = original.OwnerCookie + 1 },
                original with { Context = original.Context ^ 1 },
                original with { SourcePc = original.SourcePc + 1 },
                original with { SitePc = original.SitePc + 1 },
                original with { TargetPc = original.TargetPc + 1 },
                original with { SourceCodeEpoch = original.SourceCodeEpoch + 1 },
                original with { CodeAuthorityEpoch = original.CodeAuthorityEpoch + 1 },
                original with { WorkerInstructions = original.WorkerInstructions + 1 },
                original with { SiteIndex = original.SiteIndex + 1 },
                original with { Edge = 0 }, original with { Edge = -1 }, original with { Edge = 2 }];
            foreach (var candidate in denied)
                Check(!CompleteLastExit(f, in candidate, in target) && LastExitHeader(f, source).Link1TargetHeader == 0,
                    "R92 stale/forged candidate is rejected before Store.TryLink");
            Check(!CompleteLastExit(f, in original, in target, index: 1) &&
                !CompleteLastExit(f, in original, in target, multiple: false) &&
                !CompleteLastExit(f, in original, in target, nativeLimit: 1) &&
                !CompleteLastExit(f, in original, in target, context: original.Context ^ 1),
                "R92 only exact acquired entry0 and matching stable context may consume candidate");
            // Trust and code stamps are separate from an otherwise valid lease.
            var h = (TranslationBlockHeader*)f.Jit.Store.GetHeader(in target);
            ulong trust = h->Reserved136; h->Reserved136 = 0;
            Check(!CompleteLastExit(f, in original, in target), "R92 untrusted acquired target is denied");
            h->Reserved136 = trust;
            Check(CompleteLastExit(f, in original, in target) && LastExitHeader(f, source).Link1Pc == targetPc,
                "R92 authentic candidate patches only via reverse-adjacency Store.TryLink");
            Check(f.Statistics.LastExitLinkAttempts == denied.Length - 1 + 6 && f.Statistics.LastExitLinksCompleted == 1,
                "R92 scalar counters count each nondefault consumption attempt and only successful Store.TryLink");
            Check(f.Jit.Store.RetireContentChanged(target.Token) && LastExitHeader(f, source).Link1TargetHeader == 0,
                "R92 target retirement clears incoming source cell before target reuse");
        }
        LastExitClean(f);
    }

    static void CheckFullLastExitRevocation()
    {
        foreach (string kind in new[] { "code-epoch", "runtime-epoch", "retired-source", "replacement-source", "flush", "context", "signal" })
        {
            using var f = new LinkFixture();
            ulong pc = LinkFixture.Base, targetPc = pc + 64;
            f.Map(pc); f.Fill(pc);
            WriteControllerCode(f, pc, LastExitJump(pc, targetPc));
            WriteControllerCode(f, targetPc, LastExitMarker(3));
            var source = WarmLastExit(f, pc);
            f.Counts(101);
            Check(f.Step(pc) && f.Owner.FullTestLastExit.Source == source, "R92 revocation captures actual predecessor");
            var candidate = f.Owner.FullTestLastExit;
            TranslationToken replacement = default;
            if (kind == "code-epoch") lock (f.Jit.Authority.Gate) f.Jit.Authority.AdvanceCode();
            if (kind == "runtime-epoch") lock (f.Jit.Authority.Gate) f.Jit.Authority.Advance();
            if (kind == "retired-source") Check(f.Jit.Store.RetireContentChanged(source), "R92 source retirement");
            if (kind == "replacement-source")
            {
                Check(f.Jit.Store.RetireContentChanged(source), "R92 retires the exact captured source generation");
                replacement = WarmLastExit(f, pc);
                Check(replacement.Generation != source.Generation, "R92 identical-key source replacement gets fresh generation");
                f.At(targetPc);
            }
            if (kind == "flush") Check(f.Jit.Store.TryFlush(out _), "R92 flush between predecessor and target resolution");
            if (kind == "context") f.Machine()->flags |= 1u << Blink.FLAGS_TF;
            // Runtime-only pulses do not change code authority: publication and
            // unrelated data revocation must not permanently stale this value.
            // Other mutations cannot patch, even if bytes are still identical.
            if (kind == "context")
            {
                Check(f.Acquire(targetPc, 0, out var different, out bool multiple, out _) && multiple,
                    "R92 distinct TF context acquires its own retained target");
                using (different)
                {
                    lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in different, multiple);
                    Check(!CompleteLastExit(f, in candidate, in different), "R92 current context mismatch refuses old edge");
                }
            }
            else if (kind == "signal")
            {
                Check(f.Step(targetPc, signalActor: true) && f.Completed == 2 && f.Issued == 2 &&
                    f.Owner.FullTestLastExit.Source.Generation == 0 && LastExitHeader(f, source).Link1TargetHeader == 0,
                    "R92 recursive cap1 entry discards an already pending ordinary predecessor");
            }
            else
            {
                Check(f.Continue() && f.Completed == 101 && f.Issued == 102,
                    "R92 stale predecessor never loses/doubles an approved target issue");
                if (kind == "runtime-epoch") Check(LastExitHeader(f, source).Link1Pc == targetPc,
                    "R92 code-stable runtime pulse alone does not deny later safe completion");
                else if (kind == "code-epoch") Check(LastExitHeader(f, source).Link1TargetHeader == 0,
                    "R92 old code-authority candidate cannot patch restamped target");
                else if (kind == "replacement-source") Check(LastExitHeader(f, replacement).Link1TargetHeader == 0,
                    "R92 old executed generation cannot authorize a same-key replacement source");
                else Check(!f.Jit.Store.TryAcquireToken(source, out _), "R92 stale retired/flushed source token stays unusable");
            }
            LastExitClean(f);
        }
    }

    static void CheckFullLastExitAlias()
    {
        using var f = new LinkFixture();
        var opened = f.Io.CreateMemoryFile("r92-last-exit-alias", 0);
        Check(opened.Succeeded, "R92 creates owned shared alias backing");
        ulong pc = LinkFixture.Base, alias = pc + 0x10000, targetPc = pc + 64;
        byte[] bytes = new byte[4096]; bytes.AsSpan().Fill(0xcc);
        LastExitJump(pc, targetPc).CopyTo(bytes, 0); LastExitMarker(11).CopyTo(bytes, 64);
        Check(f.Io.WriteAt(opened.Value, bytes, 0).Value == bytes.Length, "R92 shared alias backing initialized");
        f.MapShared(opened.Value, pc); f.MapShared(opened.Value, alias);
        var source = WarmLastExit(f, pc);
        Check(f.Physical(pc) == f.Physical(alias), "R92 aliases share actual physical code backing");
        f.Counts(101);
        Check(f.Step(pc) && f.Owner.FullTestLastExit.Source == source, "R92 alias witness captures predecessor");
        // An identical-byte store still revokes source authority through its
        // physical alias. It must not be repaired using the old code stamp.
        WriteControllerCode(f, alias, LastExitJump(pc, targetPc));
        Check(f.Continue() && f.Completed == 101 && f.Issued == 102 &&
            LastExitHeader(f, source).Link1TargetHeader == 0,
            "R92 physical-alias revocation rejects pending predecessor while target executes exactly");
        LastExitClean(f);
    }

    static void CheckFullLastExitLateBoundaries()
    {
        foreach (string kind in new[] { "attention", "stop", "budget", "invalidated", "helper" })
        {
            using var f = new LinkFixture(helpersOnly: kind == "helper");
            ulong pc = LinkFixture.Base, target = pc + 64;
            f.Map(pc); f.Fill(pc);
            WriteControllerCode(f, pc, LastExitJump(pc, target));
            WriteControllerCode(f, target, LastExitMarker(5));
            if (kind != "helper") WarmLastExit(f, pc);
            f.Counts(kind == "budget" ? 1UL : 100UL);
            if (kind is "attention" or "helper")
                f.Preissued = (_, _) => f.Machine()->attention = true;
            if (kind == "stop") f.Preissued = (_, _) => f.Stop.RequestStop();
            if (kind == "invalidated") f.NativeReturned = pointer =>
            {
                var frame = (FullBlockFrame*)pointer;
                Check(frame->Exit == FullBlockExit.Continue, "R92 invalidation races after committed static exit");
                lock (f.Jit.Authority.Gate) f.Jit.Authority.AdvanceCode();
            };
            Check(f.Step(pc), "R92 late boundary first controller call returns");
            Check(f.Completed == (kind == "stop" ? 0UL : 1UL) && f.Issued == 1,
                "R92 late stop/attention/helper preserves approved issue accounting");
            if (kind != "budget") Check(f.Owner.FullTestLastExit.Source.Generation == 0,
                "R92 late boundary cannot preserve an eligible predecessor candidate");
            if (kind == "budget")
            {
                // A true completed static exit can supply a candidate at the
                // last allowance; the next rejected issue clears it BEFORE
                // any target fetch/compile/link.
                long compilations = f.Statistics.ActualBlockCompilations;
                Check(!f.Continue() && f.Completed == 1 && f.Issued == 2 &&
                    f.Stop.Reason == HostExecutionStopReason.Budget &&
                    f.Statistics.ActualBlockCompilations == compilations && f.Owner.FullTestLastExit.Source.Generation == 0,
                    "R92 exhausted outer budget clears candidate before target demand fetch");
            }
            LastExitClean(f);
        }
    }
}
