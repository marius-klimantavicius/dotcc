using System.Buffers.Binary;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // Parent wires CheckFullRetainedAuthority(machine) into Main. R98 does not
    // build or execute these contracts. Every positive trust prerequisite uses
    // the actual fixture, checked Acquire and product AuthorizeLinks.
    static void CheckFullRetainedAuthority(byte* machine)
    {
        CheckFullRetainedWarm();
        CheckFullRetainedDataPulses();
        CheckFullRetainedPending();
        CheckFullRetainedRemap();
        CheckFullRetainedShared();
        CheckFullRetainedCrossPage();
        CheckFullRetainedMigratedAlias();
        CheckFullRetainedResume();
        CheckFullRetainedContexts();
        CheckFullRetainedControllerCaps();
        CheckFullRetainedBounds();
    }

    static TranslationToken RetainedWarm(ControllerFixture f, ulong pc, uint requestedCount = 0)
    {
        Check(f.Acquire(pc, 0, out var lease, out bool multiple, out int error, requestedCount) &&
            error == 0 && multiple, "R98 checked warm acquisition has real multiple eligibility");
        using (lease)
        {
            lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in lease, multiple);
            var h = f.Jit.Store.GetHeaderCopy(in lease);
            Check(h.Live == 1 && h.Reserved136 == 1 && (h.Context & (1UL << 63)) != 0 &&
                h.CodeEpoch == h.ValidatedCodeEpoch && h.ValidatedGlobalEpoch == f.Jit.Authority.CodeEpoch,
                "R98 positive prerequisite authenticates actual flat-long retained trust");
            return lease.Token;
        }
    }

    static void RetainedClean(ControllerFixture f)
    {
        var s = f.Jit.Store.GetStatistics(); var a = f.Jit.Authority;
        Check(s.ActiveManagedLeases == 0 && s.ActiveNativeEpochs == 0 && f.Statistics.ActiveDataBorrows == 0 &&
            a.ActiveBorrows == 0 && a.CopyWriters == 0 && a.HostWriters == 0 && a.MappingWriters == 0 &&
            a.PendingAtomicWorker == -1 && a.SemanticReadersDrained(), "R98 scopes and capabilities clean up");
    }

    static void RetainedNoValidation(GuestJitStatistics before, GuestJitStatistics after, int acquisitions = 1, int? passiveScopes = null)
    {
        Check(AuthorityCount(after, "RetainedEligibilityHits") == AuthorityCount(before, "RetainedEligibilityHits") + acquisitions &&
            AuthorityCount(after, "TrustedAcquisitions") == AuthorityCount(before, "TrustedAcquisitions") + acquisitions &&
            AuthorityCount(after, "RetainedEligibilityMisses") == AuthorityCount(before, "RetainedEligibilityMisses") &&
            AuthorityCount(after, "ByteValidations") == AuthorityCount(before, "ByteValidations") &&
            AuthorityCount(after, "Restamps") == AuthorityCount(before, "Restamps") &&
            after.ActualBlockCompilations == before.ActualBlockCompilations && after.EmittedCodeBytes == before.EmittedCodeBytes,
            "R98 counters distinguish retained eligibility from Matches, restamping and compilation");
        // This counts lifetime scopes only, not accesses. Actual page-table
        // and backing eligibility calls are observed independently below.
        Check(after.PageTableReadBorrows == before.PageTableReadBorrows + (passiveScopes ?? acquisitions),
            "R98 warm acquisitions retain their passive outer lifetime scope");
    }

    static void CheckFullRetainedWarm()
    {
        foreach (bool helpersOnly in new[] { false, true })
        {
            using var f = new ControllerFixture(helpersOnly);
            ulong pc = ControllerFixture.Base; f.Map(pc); f.Fill(pc);
            var token = RetainedWarm(f, pc); var before = f.Statistics;
            var queries = f.Jit.Authority.FullTestEligibilityQueries;
            long walks = JitPageIdentity.FullTestIdentityReads;
            Check(queries.Calls > 0 && queries.PageIdentity > 0 && queries.Contains > 0 &&
                queries.Available > 0 && queries.Shared > 0 && walks > 0,
                "R98 cold positive prerequisite actually executes observed PTE and backing eligibility queries");
            for (int i = 0; i < 4; ++i)
            {
                Check(f.Acquire(pc, 0, out var lease, out bool multiple, out _) && multiple,
                    "R98 unchanged native/helper resident acquisition remains eligible");
                using (lease) Check(lease.Token == token, "R98 reuses exact resident generation");
            }
            RetainedNoValidation(before, f.Statistics, 4);
            Check(f.Jit.Authority.FullTestEligibilityQueries == queries && JitPageIdentity.FullTestIdentityReads == walks,
                "R98 four warmed native/helper acquisitions perform zero observed PTE/contains/availability/shared/lease eligibility queries");
            Check(f.Issued == 0 && f.Completed == 0, "R98 authority acquisition issues no instruction");
            RetainedClean(f);
        }
    }

    static void CheckFullRetainedDataPulses()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base, data = pc + 4096;
        f.Map(pc, 8192); f.Fill(pc, 8192); nint physical = f.Physical(data);
        var token = RetainedWarm(f, pc); ulong stamp = f.Jit.Authority.CodeEpoch;
        f.DataScope(0, () => f.Expose(data, 0));
        var copies = (IHostGuestMemoryCopies)f.Owner;
        f.At(pc);
        copies.BeginGuestWrite(physical, 1);
        try { ((byte*)physical)[0] = 0x5a; }
        finally { copies.EndGuestWrite(); }
        Check(f.Jit.Authority.CodeEpoch == stamp, "R98 known non-code exposure/copy retains code freshness");
        var before = f.Statistics;
        Check(f.Acquire(pc, 1, out var reused, out bool multiple, out _) && multiple,
            "R98 completed non-code writes retain resident eligibility");
        using (reused) Check(reused.Token == token, "R98 data pulses retain code generation");
        RetainedNoValidation(before, f.Statistics); RetainedClean(f);
    }

    static void CheckFullRetainedPending()
    {
        foreach (string kind in new[] { "mapping", "host", "copy", "reservation", "borrow" })
        {
            using var f = new ControllerFixture();
            ulong pc = ControllerFixture.Base, data = pc + 4096;
            f.Map(pc, 8192); f.Fill(pc, 8192); nint physical = f.Physical(data);
            var token = RetainedWarm(f, pc); var a = f.Jit.Authority;
            var copies = (IHostGuestMemoryCopies)f.Owner;
            var before = f.Statistics;
            void Inspect()
            {
                if (kind is "copy" or "reservation")
                {
                    Check(!f.Acquire(pc, 1, out var deferred, out bool admitted, out int error) &&
                        deferred.Owner == null && !admitted && error == 0,
                        "R205 generic Acquire defers foreign denial without clearing existing source trust");
                    return;
                }
                Check(f.Acquire(pc, 1, out var lease, out bool multiple, out _) && !multiple,
                    "R98 pending scalar/code writer denies retained multiple eligibility: " + kind);
                using (lease)
                {
                    var h = f.Jit.Store.GetHeaderCopy(in lease);
                    Check(lease.Token == token && h.Reserved136 == 0,
                        "R98 Acquire clears old eligibility before returning outside controller: " + kind);
                    if (kind == "copy") Check(h.ValidatedGlobalEpoch == 0,
                        "R98 pending known-data copy retains the R80 non-executable snapshot-zero rule");
                }
            }
            // These are deterministic protocol pauses, not real OS-thread
            // mapping/IO/native races. The borrow uses the real writable resolver.
            if (kind == "borrow") f.DataScope(0, () => { f.Expose(pc, 0); Inspect(); });
            else
            {
                if (kind == "mapping") a.BeginMappingMutation();
                else if (kind == "host") f.Jit.BeginMemoryFileMutation(physical, 4096, false);
                else if (kind == "copy") { f.At(pc); copies.BeginGuestWrite(physical, 1); }
                else lock (a.Gate) Check(a.TryReserveNativeTicket(f.WorkerId(0)), "R98 reserves actual pending ticket");
                try { Inspect(); }
                finally
                {
                    if (kind == "mapping") a.EndMappingMutation();
                    else if (kind == "host") f.Jit.EndMemoryFileMutation();
                    else if (kind == "copy") { f.At(pc); copies.EndGuestWrite(); }
                    else a.AbortNativeReservation(f.WorkerId(0));
                }
            }
            var after = f.Statistics;
            bool preserved = kind is "copy" or "reservation";
            Check(AuthorityCount(after, "RetainedEligibilityHits") == AuthorityCount(before, "RetainedEligibilityHits") &&
                AuthorityCount(after, "RetainedEligibilityMisses") == AuthorityCount(before, "RetainedEligibilityMisses") + (preserved ? 0 : 1) &&
                (preserved ? AuthorityCount(after, "ByteValidations") == AuthorityCount(before, "ByteValidations") :
                    AuthorityCount(after, "ByteValidations") > AuthorityCount(before, "ByteValidations")),
                "R205 only exact trusted foreign denials preserve source; other R98 paths remain checked: " + kind);
            // No AuthorizeLinks(false) was called after the pending Acquire.
            Check(f.Acquire(pc, 0, out var checkedAgain, out bool restored, out _) && restored,
                "R98 completed scope regains eligibility through checked validation");
            using (checkedAgain)
            {
                Check(checkedAgain.Token == token && f.Jit.Store.GetHeaderCopy(in checkedAgain).Reserved136 == (preserved ? 1UL : 0UL),
                    "R205 completion preserves existing trust; R98 restamps never resurrect cleared trust");
                lock (a.Gate) f.Jit.AuthorizeLinks(in checkedAgain, restored);
            }
            RetainedClean(f);
        }
    }

    static void CheckFullRetainedRemap()
    {
        foreach (bool changed in new[] { false, true })
        {
            using var f = new ControllerFixture(); ulong pc = ControllerFixture.Base;
            f.Map(pc); f.Fill(pc); var old = RetainedWarm(f, pc);
            var before = f.Statistics;
            Check(f.Mutate("MAP_FIXED", pc) == 0, "R98 actual MAP_FIXED revokes warmed authority");
            f.Fill(pc); if (changed) f.WriteMov(pc, 9);
            Check(f.Acquire(pc, 0, out var current, out bool multiple, out _) && multiple,
                "R98 remapped backing is checked before multiple admission");
            using (current)
            {
                Check((current.Token != old) == changed &&
                    f.Jit.Store.GetHeaderCopy(in current).Reserved136 == 0,
                    "R98 identical remap restamps; changed bytes replace emission without carrying trust");
                Check(FullTranslationJit.Plans(f.Jit.Store, in current)[0].Decoded.op.uimm0 == (changed ? 9UL : 1UL),
                    "R98 retained decode follows actual remapped source bytes");
            }
            Check(f.Statistics.ContentInvalidations == before.ContentInvalidations + (changed ? 1 : 0),
                "R98 mapping identity and source content have separate outcomes");
            RetainedClean(f);
        }
    }

    static byte[] RetainedPage()
    {
        byte[] code = new byte[4096]; code.AsSpan().Fill(0xcc);
        code[0] = 0xb8; code[1] = 1; code[2] = code[3] = code[4] = 0;
        code[5] = 0xeb; code[6] = 0xf9; return code;
    }

    static void CheckFullRetainedShared()
    {
        using (var f = new ControllerFixture())
        {
            ulong pc = ControllerFixture.Base; f.Map(pc); f.Fill(pc);
            var original = RetainedWarm(f, pc);
            var opened = f.Io.OpenFile("/r98-regular", FileAccessMode.Read | FileAccessMode.Write, create: true);
            Check(opened.Succeeded && f.Io.WriteAt(opened.Value, RetainedPage(), 0).Value == 4096,
                "R98 initializes real mutable regular shared backing");
            f.Owner.FullRetainedTestMapReadOnlyShared(opened.Value, pc);
            for (int i = 0; i < 2; ++i)
            {
                Check(f.Acquire(pc, 0, out var regular, out bool multiple, out _) && !multiple,
                    "R98 regular shared mapping always uses cap-one eligibility");
                using (regular)
                {
                    var h = f.Jit.Store.GetHeaderCopy(in regular);
                    // Regular-shared eligibility returns false before code
                    // registration. Its FIRST Matches snapshot can therefore
                    // pulse runtime admission and deliberately publish stamp0;
                    // the next stable check must restamp, never cache trust.
                    Check(regular.Token == original && h.Reserved136 == 0 && h.Live == 1 &&
                        (h.ValidatedGlobalEpoch == f.Jit.Authority.CodeEpoch || i == 0 && h.ValidatedGlobalEpoch == 0),
                        $"R98 regular remap keeps private trust revoked; first-code stamp={h.ValidatedGlobalEpoch} current={f.Jit.Authority.CodeEpoch}");
                }
            }
            RetainedClean(f);
        }
        using (var f = new ControllerFixture())
        {
            ulong pc = ControllerFixture.Base;
            var opened = f.Io.CreateMemoryFile("r98-local-memfd", 0);
            Check(opened.Succeeded && f.Io.WriteAt(opened.Value, RetainedPage(), 0).Value == 4096,
                "R98 initializes observer-owned local memfd");
            int fd = opened.Value; f.MapShared(fd, pc); var token = RetainedWarm(f, pc);
            var before = f.Statistics;
            Check(f.Acquire(pc, 0, out var hit, out bool multiple, out _) && multiple, "R98 local memfd reuses checked trust");
            hit.Dispose(); RetainedNoValidation(before, f.Statistics);
            ulong stamp = f.Jit.Authority.CodeEpoch;
            Check(f.Io.WriteAt(fd, new byte[] { 2, 0, 0, 0 }, 1).Value == 4 && f.Jit.Authority.CodeEpoch > stamp,
                "R98 actual host memfd writer publishes code freshness");
            Check(f.Acquire(pc, 0, out var changed, out multiple, out _) && multiple,
                "R98 changed memfd source is acquired after host completion");
            using (changed) Check(changed.Token != token &&
                FullTranslationJit.Plans(f.Jit.Store, in changed)[0].Decoded.op.uimm0 == 2,
                "R98 actual host bytes replace the trusted retained generation");
            RetainedWarm(f, pc); stamp = f.Jit.Authority.CodeEpoch;
            Check(f.Io.TruncateFile(fd, 0).Succeeded && f.Jit.Authority.CodeEpoch > stamp,
                "R98 local memfd EOF mutation invalidates cached trust");
            Check(!f.Acquire(pc, 0, out var denied, out multiple, out int error) && !multiple && error != 0,
                "R98 EOF follows the real checked fetch fault path");
            denied.Dispose();
            Check(f.Io.WriteAt(fd, RetainedPage(), 0).Value == 4096, "R98 regrows local memfd backing");
            RetainedWarm(f, pc); RetainedClean(f);
        }
    }

    static void CheckFullRetainedMigratedAlias()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base, oldAlias = pc + 0x10000, newAlias = oldAlias + 0x10000;
        var first = f.Io.CreateMemoryFile("r98-alias-old", 0);
        var second = f.Io.CreateMemoryFile("r98-alias-new", 0);
        Check(first.Succeeded && second.Succeeded && f.Io.WriteAt(first.Value, RetainedPage(), 0).Value == 4096 &&
            f.Io.WriteAt(second.Value, RetainedPage(), 0).Value == 4096, "R98 initializes independent identical backing");
        f.MapShared(first.Value, pc); f.MapShared(first.Value, oldAlias); f.MapShared(second.Value, newAlias);
        f.DataScope(0, () => { f.Expose(oldAlias, 0); f.Expose(newAlias, 0); });
        var token = RetainedWarm(f, pc);
        f.MapShared(second.Value, pc);
        ulong beforeRuntime = f.Jit.Authority.Epoch, beforeCode = f.Jit.Authority.CodeEpoch;
        Check(f.Acquire(pc, 0, out var migrated, out bool firstMultiple, out int migrationError) && migrationError == 0,
            "R98 identical remap performs its first checked acquisition");
        using (migrated)
        {
            Check(migrated.Token == token, "R98 first migration check retains unchanged emitted body");
            if (!firstMultiple)
                Check(f.Jit.Authority.Epoch != beforeRuntime || f.Jit.Authority.CodeEpoch != beforeCode,
                    "R98 first migration cap-one denial requires an actual authority pulse, not stale cached trust");
        }
        Check(RetainedWarm(f, pc) == token, "R98 stable identical remap retains emission after provenance refresh");
        Check(f.Physical(pc) != f.Physical(oldAlias) && f.Physical(pc) == f.Physical(newAlias),
            "R98 actual remap migrates physical identity independently of identical code bytes");
        WriteControllerCode(f, oldAlias, RetainedPage()[..7]);
        Check(f.Jit.Store.TryAcquireToken(token, out var unaffected), "R98 migrated body remains resident");
        using (unaffected) Check(f.Jit.Store.GetHeaderCopy(in unaffected).Live == 1,
            "R98 writing former physical alias does not revoke migrated body provenance");
        RetainedWarm(f, pc);
        WriteControllerCode(f, newAlias, RetainedPage()[..7]);
        Check(f.Jit.Store.TryAcquireToken(token, out var revoked), "R98 revoked migrated body stays retained");
        using (revoked) Check(f.Jit.Store.GetHeaderCopy(in revoked).Live == 0,
            "R98 current physical alias revokes migrated body before identical-byte write");
        RetainedClean(f);
    }

    static void CheckFullRetainedCrossPage()
    {
        using var f = new ControllerFixture();
        ulong page = ControllerFixture.Base, pc = page + 4095; f.Map(page, 8192); f.Fill(page, 8192);
        byte[] bytes = [0xb8, 1, 0, 0, 0, 0xeb, 0xf9];
        // Resolve each page independently; guest adjacency does not imply
        // host adjacency. Both writable lookups use the real callback protocol.
        f.DataScope(0, () =>
        {
            for (int offset = 0; offset < bytes.Length;)
            {
                ulong address = pc + (uint)offset;
                byte* host = f.Program.LookupAddress2(f.Machine(), (long)address, 0, 0);
                Check(host != null, "R98 crossing code write resolves actual backing");
                int count = Math.Min(bytes.Length - offset, 4096 - (int)(address & 4095));
                bytes.AsSpan(offset, count).CopyTo(new Span<byte>(host, count)); offset += count;
            }
        });
        for (int i = 0; i < 2; ++i)
        {
            Check(f.Acquire(pc, 0, out var crossing, out bool multiple, out _) && !multiple,
                "R98 cross-page source keeps checked cap-one eligibility");
            using (crossing)
            {
                lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in crossing, multiple);
                Check(f.Jit.Store.GetHeaderCopy(in crossing).Reserved136 == 0,
                    "R98 crossing source cannot mint cached retained trust");
            }
        }
        Check(AuthorityCount(f.Statistics, "RetainedEligibilityHits") == 0,
            "R98 actual crossing source never uses retained eligibility shortcut");
        RetainedClean(f);
    }

    static void CheckFullRetainedResume()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base, target = pc + 64; f.Map(pc); f.Fill(pc);
        WriteControllerCode(f, pc, [0x90, 0xeb, 61]);
        var source = RetainedWarm(f, pc); var destination = RetainedWarm(f, target);
        f.At(target); var before = f.Statistics;
        Check(f.Owner.FullRetainedTestAcquire(source, uint.MaxValue, out var next, out uint index, out bool multiple, out _) && multiple,
            "R98 predecessor resume outside source range resolves actual target key");
        using (next) Check(next.Token == destination && index == 0, "R98 successor key keeps its own entry boundary");
        RetainedNoValidation(before, f.Statistics);
        Check(AuthorityCount(f.Statistics, "ResumeRangeRejects") == AuthorityCount(before, "ResumeRangeRejects") + 1,
            "R98 source range rejection is counted independently of target authority reuse");
        foreach (uint hint in new[] { 1u, 0u, uint.MaxValue })
        {
            f.At(pc + 1);
            Check(f.Owner.FullRetainedTestAcquire(source, hint, out var inside, out index, out multiple, out _) && multiple,
                "R98 exact resume hint or bounded in-range fallback finds real instruction boundary");
            using (inside) Check(inside.Token == source && index == 1, "R98 inside-range fallback retains actual plan index");
        }
        ulong overlap = pc + 128;
        WriteControllerCode(f, overlap, [0xb8, 0x90, 0x90, 0x90, 0x90, 0xeb, 0xf9]);
        var containing = RetainedWarm(f, overlap); f.At(overlap + 1);
        Check(f.Owner.FullRetainedTestAcquire(containing, 0, out var independent, out index, out _, out _) && index == 0,
            "R98 in-range non-plan target falls through scan to its independent exact key");
        using (independent) Check(independent.Token != containing &&
            f.Jit.Store.GetHeaderCopy(in independent).GuestPc == overlap + 1 &&
            FullTranslationJit.Plans(f.Jit.Store, in independent)[0].Decoded.length == 1,
            "R98 overlapping instruction stream begins at its actual independently decoded NOP");
        RetainedClean(f);
    }

    static void CheckFullRetainedContexts()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base, shifted = pc + 0x10000;
        f.Map(pc); f.Map(shifted); f.Fill(pc); f.Fill(shifted); f.WriteMov(shifted, 2);
        var flat = RetainedWarm(f, pc); var m = f.Machine(); uint flags = m->flags;
        m->flags ^= 1u << Blink.FLAGS_TF;
        try
        {
            f.At(pc);
            Check(f.Owner.FullRetainedTestAcquire(flat, 0, out var tf, out _, out _, out _),
                "R98 TF context goes through its own checked lookup");
            using (tf) Check(tf.Token != flat && f.Jit.Store.GetHeaderCopy(in tf).Reserved136 == 0,
                "R98 exact flags context cannot reuse flat predecessor trust");
        }
        finally { m->flags = flags; }
        ulong cs = m->cs.@base;
        try
        {
            m->cs.@base = shifted - pc; f.At(pc);
            Check(f.Owner.FullRetainedTestAcquire(flat, 0, out var nonflat, out _, out bool multiple, out _),
                "R98 nonflat long context fetches its real shifted source");
            using (nonflat)
            {
                lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in nonflat, multiple);
                var h = f.Jit.Store.GetHeaderCopy(in nonflat);
                Check(nonflat.Token != flat && (h.Context & (1UL << 63)) == 0 && h.Reserved136 == 0 &&
                    FullTranslationJit.Plans(f.Jit.Store, in nonflat)[0].Decoded.op.uimm0 == 2,
                    "R98 eligibility is minted from validated context and source, never current logical PC alone");
            }
        }
        finally { m->cs.@base = cs; }
        RetainedClean(f);
    }

    static void CheckFullRetainedControllerCaps()
    {
        foreach (bool helper in new[] { false, true })
        foreach (bool actor in new[] { false, true })
        {
            using var f = new ControllerFixture(helper); ulong pc = ControllerFixture.Base;
            f.Map(pc); f.Fill(pc); RetainedWarm(f, pc, actor ? 1u : 0u);
            f.Counts(1); f.Preissued = (_, _) => f.Machine()->attention = true;
            Check(f.Step(pc, signalActor: actor) && f.Issued == 1 && f.Completed == 1 &&
                f.Machine()->ip == pc + 5 && FullRax(f) == 1 && f.Statistics.InterpreterAttempts == 0,
                "R98 warmed actual controller preserves first preissue and normal/signal cap across native/helper execution");
            RetainedClean(f);
        }
        using (var f = new ControllerFixture(helpersOnly: true))
        {
            ulong pc = ControllerFixture.Base; f.Map(pc); f.Fill(pc); RetainedWarm(f, pc, 1);
            f.Counts(1);
            f.DataScope(0, () =>
            {
                int depth = f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0));
                f.SignalActor(pc);
                Check(f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0)) == depth,
                    "R98 recursive helper actor restores older counted CPU scope");
            });
            Check(f.Completed == 1 && f.Issued == 2 && f.Stop.Reason == HostExecutionStopReason.Budget &&
                f.Statistics.SemanticHelperInstructions == 1, "R98 recursive helper retains precise rejected-budget issue");
            RetainedClean(f);
        }
    }

    static void CheckFullRetainedBounds()
    {
        using (var f = new ControllerFixture())
        {
            ulong pc = ControllerFixture.Base; f.Map(pc); f.Fill(pc); RetainedWarm(f, pc);
            // Synthetic aligned identities are NEVER dereferenced. They only
            // exhaust the real owner's bounded registry/borrow protocol; the
            // positive eligibility was obtained from actual mapped code above.
            for (int i = 1; i <= 8193 && !f.Jit.Authority.Saturated; ++i)
                f.Jit.PhysicalExposed(f.WorkerId(0), (nint)(0x1000000000UL + (ulong)i * 4096));
            f.Jit.Authority.EndBorrow(f.WorkerId(0));
            Check(f.Jit.Authority.Saturated, "R98 synthetic identities exhaust the real bounded authority");
            Check(f.Acquire(pc, 0, out var denied, out bool multiple, out _) && !multiple,
                "R98 saturated real authority cannot reuse pre-saturation eligibility");
            using (denied) Check(f.Jit.Store.GetHeaderCopy(in denied).Reserved136 == 0, "R98 saturation clears retained trust");
            RetainedClean(f);
        }
        using (var f = new ControllerFixture())
        {
            ulong pc = ControllerFixture.Base; f.Map(pc); f.Fill(pc); RetainedWarm(f, pc);
            var a = f.Jit.Authority;
            // Deliberate exhaustion injection, as in R80. This does not forge
            // positive eligibility or claim an actual 2^64 mutation schedule.
            ulong beforeInjection = a.CodeEpoch;
            try
            {
                a.BeginHostWriter(); a.CodeEpochPointer[0] = ulong.MaxValue; a.EndHostWriter();
                Check(a.SemanticQuarantined, "R98 completion exhaustion quarantines the real authority");
                Check(f.Acquire(pc, 0, out var denied, out bool multiple, out _) && !multiple,
                    "R98 exhausted authority cannot reuse a formerly trusted code body");
                using (denied) Check(f.Jit.Store.GetHeaderCopy(in denied).Reserved136 == 0, "R98 exhaustion cannot retain trust");
                RetainedClean(f);
            }
            finally
            {
                // Restore only the deliberately poisoned test word so the C
                // fixture can free page tables through its mapping callbacks.
                // Quarantine remains set: this does NOT qualify recovery or
                // production cleanup at a genuinely exhausted generation.
                a.CodeEpochPointer[0] = beforeInjection;
            }
            Check(a.SemanticQuarantined, "R98 teardown restoration never clears quarantine");
        }
    }
}
