using System.Buffers.Binary;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // Authored controller witnesses; the parent owns Main wiring and runtime
    // receipts. All mappings, fetches and write admissions use the real owner.
    static void CheckQemuProvenanceMigration()
    {
        foreach (bool helpersOnly in new[] { false, true })
        {
            CheckQemuProvenanceResumeMigration(helpersOnly);
            CheckQemuProvenanceSecondPageMigration(helpersOnly);
        }
    }

    static void CheckQemuProvenanceResumeMigration(bool helpersOnly)
    {
        using var f = new ControllerFixture(helpersOnly);
        int oldFd = -1, newFd = -1;
        try
        {
            byte[] bytes = new byte[4096]; bytes.AsSpan().Fill(0xcc);
            QemuProvenanceMovLoop(bytes, 0);
            oldFd = QemuProvenanceFile(f, "provenance-resume-old", bytes);
            newFd = QemuProvenanceFile(f, "provenance-resume-new", bytes);
            ulong first = ControllerFixture.Base, oldAlias = first + 0x10000, newAlias = first + 0x20000;
            ulong root = f.Machine()->blink_host_system->cr3;
            f.MapShared(oldFd, first); f.MapShared(oldFd, oldAlias); f.MapShared(newFd, newAlias);
            nint oldPage = QemuProvenanceBacking(f, oldAlias), newPage = QemuProvenanceBacking(f, newAlias);
            Check(oldPage != newPage && QemuProvenanceBacking(f, first) == oldPage,
                "resume provenance starts with distinct live memfd backings and an actual old root PTE");

            f.Counts(16); FullRax(f, 0xfeedfacefeedfaceUL);
            int approvals = 0;
            f.Preissued = (machine, actor) =>
            {
                ++approvals;
                f.Machine()->attention = true;
                Check(machine == (nint)f.Machine() && !actor && f.Issued == (ulong)approvals &&
                    f.Completed == (ulong)approvals - 1,
                    "resume migration approves exactly one outstanding ordinary late-attention issue");
            };
            Check(f.Step(first) && f.Issued == 1 && f.Completed == 1 &&
                f.Machine()->ip == first + 5 && f.Machine()->oplen == 0 && FullRax(f) == 1,
                "late-attention resume migration executes exactly MOV1 and leaves its real suffix continuation");
            TranslationToken token = f.ResumeToken();
            Check(token.Generation != 0, "resume migration has a controller-issued token");
            Check(f.Jit.Store.TryAcquireToken(token, out var originalLease),
                "resume migration controller token is resident");
            TranslationBlockHeader original;
            using (originalLease)
            {
                original = f.Jit.Store.GetHeaderCopy(in originalLease);
                var plans = FullTranslationJit.Plans(f.Jit.Store, in originalLease);
                Check(original.GuestPc == first && original.GuestLength == 7 && plans.Length == 2 &&
                    plans[0].Ip == first && plans[0].Decoded.length == 5 && plans[0].Decoded.op.uimm0 == 1 &&
                    plans[1].Ip == first + 5 && plans[1].Decoded.length == 2,
                    "real resume token owns both MOV1 and the retained JMP suffix");
            }
            long compiles = f.Statistics.ActualBlockCompilations, publications = f.Jit.Store.Publications;
            long emitted = f.Statistics.EmittedCodeBytes;
            Check(compiles == 1 && publications == 1, "resume witness begins with one actual compilation");

            // MapShared selects the owner without At/Acquire, so MAP_FIXED and
            // the checked PTE walk must preserve the actual FullResume token.
            f.MapShared(newFd, first);
            Check(f.ResumeToken() == token && f.Machine()->ip == first + 5 &&
                f.Machine()->blink_host_system->cr3 == root && QemuProvenanceBacking(f, first) == newPage &&
                QemuProvenanceBacking(f, oldAlias) == oldPage && QemuProvenanceBacking(f, newAlias) == newPage,
                "identical-byte MAP_FIXED changes the real leaf backing while preserving root, old alias and resume");
            int resumedFrames = 0;
            f.NativePrepared = pointer =>
            {
                ++resumedFrames;
                var frame = (FullBlockFrame*)pointer;
                // First code registration on the replacement physical page
                // advances authority during acquisition, imposing checked cap1.
                Check(frame->ResumeIndex == 1 && frame->NativeLimit == 1 && frame->FirstPreissued == 1 &&
                    frame->Header->Entry == original.Entry && frame->BodyGeneration == token.Generation &&
                    frame->Header->Live == 1 && frame->Header->CodeEpoch == frame->Header->ValidatedCodeEpoch &&
                    frame->ObservedGlobalEpoch == f.Jit.Authority.Epoch &&
                    f.Machine()->ip == first + 5 && f.Issued == 2 && f.Completed == 1,
                    $"real remap resumes original suffix with cap1/current authority/one issue: resume={frame->ResumeIndex} cap={frame->NativeLimit} preissued={frame->FirstPreissued} entrySame={frame->Header->Entry == original.Entry} generation={frame->BodyGeneration} epoch={frame->ObservedGlobalEpoch}/{f.Jit.Authority.Epoch} issued={f.Issued} completed={f.Completed}");
            };
            Check(f.Continue() && resumedFrames == 1 && approvals == 2 &&
                f.Issued == 2 && f.Completed == 2 && f.Machine()->ip == first && FullRax(f) == 1 &&
                f.ResumeToken() == token,
                "identical replacement resumes JMP once from the original token rather than compiling the suffix");
            var migrated = QemuProvenanceSameEmission(f, token, original, compiles, publications, emitted);
            Check(migrated.Live == 1 && migrated.CodeEpoch > original.CodeEpoch &&
                migrated.CodeEpoch == migrated.ValidatedCodeEpoch && migrated.ValidatedGlobalEpoch == f.Jit.Authority.CodeEpoch,
                "byte-valid resume reauthorizes the retained emission after real mapping revocation");

            QemuProvenanceAliasExposure(f, token, migrated, oldAlias, revoke: false);
            QemuProvenanceAliasExposure(f, token, migrated, newAlias, revoke: true);
            Check(f.ResumeToken() == token && f.Machine()->ip == first,
                "old and new alias admissions preserve the real continuation and guest IP");
            FullRax(f, 0xfeedfacefeedfaceUL);
            Check(f.Continue() && approvals == 3 && f.Issued == 3 && f.Completed == 3 &&
                FullRax(f) == 1 && f.Machine()->ip == first + 5 && f.ResumeToken() == token,
                "unchanged new-alias exposure revalidates the same real resume and executes MOV1 exactly once");
            var revalidated = QemuProvenanceSameEmission(f, token, original, compiles, publications, emitted);
            Check(revalidated.Live == 1 && revalidated.CodeEpoch == revalidated.ValidatedCodeEpoch &&
                f.Jit.Store.GetStatistics().ContentRetirements == 0,
                "mapping and pointer exposure alone never retire identical retained content");

            long mutations = f.Statistics.HostBackingMutationScopes;
            var write = f.Io.WriteAt(newFd, new byte[] { 2, 0, 0, 0 }, 1);
            Check(write.Succeeded && write.Value == 4 && f.Statistics.HostBackingMutationScopes > mutations &&
                QemuProvenanceHeader(f, token).Live == 0 && f.Jit.Store.GetStatistics().ContentRetirements == 0,
                "actual observed IO write revokes the migrated resident before changed bytes are executed");
            Check(f.Step(first) && approvals == 4 && f.Issued == 4 && f.Completed == 4 &&
                f.Machine()->ip == first + 5 && f.Machine()->oplen == 0 && FullRax(f) == 2 && f.ResumeToken() != token,
                "changed new backing executes exact MOV2 once through a replacement controller translation");
            var statistics = f.Statistics;
            var store = f.Jit.Store.GetStatistics();
            Check(statistics.ActualBlockCompilations == compiles + 1 && statistics.CompileAttempts == compiles + 1 &&
                f.Jit.Store.Publications == publications + 1 && store.ContentRetirements == 1 &&
                store.ResidentBodies == 1 && store.TotalFlushes == 0 && store.CapacityRetirements == 0 &&
                statistics.InlineNativeInstructions == (helpersOnly ? 0 : 4) &&
                statistics.SemanticHelperInstructions == (helpersOnly ? 4 : 0) && statistics.InterpreterAttempts == 0 &&
                f.Stop.Reason == HostExecutionStopReason.None,
                "resume migration compiles only changed content with four capped native or typed-helper completions");
            QemuProvenanceDrained(f);
        }
        finally
        {
            // The fixture's using scope releases every real mapping and keeps
            // the original IO mutation observer bound through mapping cleanup.
            if (newFd >= 0) f.Io.Close(newFd);
            if (oldFd >= 0) f.Io.Close(oldFd);
        }
    }

    static void CheckQemuProvenanceSecondPageMigration(bool helpersOnly)
    {
        using var f = new ControllerFixture(helpersOnly);
        int oldFd = -1, newFd = -1;
        try
        {
            byte[] bytes = new byte[8192]; bytes.AsSpan().Fill(0xcc);
            bytes[4094] = 0xb8; bytes[4095] = 1;
            bytes.AsSpan(4096, 3).Clear(); // MOV eax,1 spans exactly 2 + 3 bytes.
            QemuProvenanceMovLoop(bytes, 4096 + 16);
            oldFd = QemuProvenanceFile(f, "provenance-cross-old", bytes);
            newFd = QemuProvenanceFile(f, "provenance-cross-new", bytes.AsSpan(4096));
            ulong first = ControllerFixture.Base, second = first + 4096, pc = first + 4094;
            ulong oldAlias = first + 0x10000, newAlias = first + 0x20000;
            ulong root = f.Machine()->blink_host_system->cr3;
            f.MapShared(oldFd, first);
            QemuProvenanceMapOffset(f, oldFd, second, 4096);
            QemuProvenanceMapOffset(f, oldFd, oldAlias, 4096);
            f.MapShared(newFd, newAlias);
            nint firstPage = QemuProvenanceBacking(f, first);
            nint oldPage = QemuProvenanceBacking(f, oldAlias), newPage = QemuProvenanceBacking(f, newAlias);
            Check(firstPage != oldPage && firstPage != newPage && oldPage != newPage &&
                QemuProvenanceBacking(f, second) == oldPage,
                "crossing instruction has three distinct actual backings and an old second-page alias");

            // Crossing bodies are deliberately single-instruction admissions.
            // Compile real in-page alias loops so both physical pages have
            // code authority and Expose exercises the actual write callback.
            foreach (ulong alias in new[] { oldAlias, newAlias })
            {
                Check(f.Acquire(alias + 16, 1, out var marker, out bool multiple, out int error),
                    "second-page alias compiles through the actual controller");
                using (marker)
                    Check(multiple && error == 0, "second-page alias has actual executable in-page code authority");
            }
            f.Counts(16); FullRax(f, 0xfeedfacefeedfaceUL);
            Check(f.Step(pc, signalActor: true) && f.Issued == 1 && f.Completed == 1 && FullRax(f) == 1 &&
                f.Machine()->ip == pc + 5 && f.Machine()->oplen == 0,
                "actual crossing MOV1 completes once with the exact five-byte next IP");
            TranslationToken token = f.ResumeToken();
            Check(f.Jit.Store.TryAcquireToken(token, out var originalLease), "crossing controller token is resident");
            TranslationBlockHeader original;
            using (originalLease)
            {
                original = f.Jit.Store.GetHeaderCopy(in originalLease);
                var plans = FullTranslationJit.Plans(f.Jit.Store, in originalLease);
                Check(original.GuestPc == pc && original.GuestLength == 5 && plans.Length == 1 &&
                    plans[0].Ip == pc && plans[0].Decoded.length == 5 && plans[0].Decoded.op.uimm0 == 1 &&
                    original.Reserved136 == 0,
                    "crossing root owns the exact two-page MOV plan without trusted multiple authorization");
            }
            long compiles = f.Statistics.ActualBlockCompilations, publications = f.Jit.Store.Publications;
            long emitted = f.Statistics.EmittedCodeBytes;
            Check(compiles == 3 && publications == 3, "crossing witness retains two alias loops and one root body");

            f.MapShared(newFd, second);
            Check(f.ResumeToken() == token && f.Machine()->blink_host_system->cr3 == root &&
                QemuProvenanceBacking(f, first) == firstPage && QemuProvenanceBacking(f, second) == newPage &&
                QemuProvenanceBacking(f, oldAlias) == oldPage && QemuProvenanceBacking(f, newAlias) == newPage,
                "only the actual second-page PTE changes while the first page and old backing remain alive");
            // Acquire intentionally resets FullResume here; the separate
            // witness above exercises continuation without that fixture reset.
            Check(f.Acquire(pc, 0, out var refreshed, out bool crossingMultiple, out int fetchError, requestedCount: 1),
                "byte-valid crossing root is reacquired through actual controller fetch and revalidation");
            TranslationToken refreshedToken;
            using (refreshed)
            {
                Check(!crossingMultiple && fetchError == 0, "crossing root retains checked single-instruction admission");
                refreshedToken = refreshed.Token;
            }
            Check(refreshedToken == token, "identical second-page replacement retains the crossing root token");
            var migrated = QemuProvenanceSameEmission(f, token, original, compiles, publications, emitted);
            Check(migrated.Live == 1 && migrated.CodeEpoch > original.CodeEpoch &&
                migrated.CodeEpoch == migrated.ValidatedCodeEpoch && migrated.ValidatedGlobalEpoch == f.Jit.Authority.CodeEpoch,
                "checked crossing revalidation restores the original body after second-page replacement");
            QemuProvenanceAliasExposure(f, token, migrated, oldAlias, revoke: false);
            QemuProvenanceAliasExposure(f, token, migrated, newAlias, revoke: true);
            Check(f.Acquire(pc, 0, out var revalidated, out _, out fetchError, requestedCount: 1),
                "unchanged crossing body revalidates after new second-page pointer exposure");
            TranslationToken revalidatedToken;
            using (revalidated)
            {
                Check(fetchError == 0, "crossing revalidation retains checked executable fetch access");
                revalidatedToken = revalidated.Token;
            }
            Check(revalidatedToken == token, "second-page pointer exposure preserves the original crossing token");
            var live = QemuProvenanceSameEmission(f, token, original, compiles, publications, emitted);
            Check(live.Live == 1 && live.CodeEpoch == live.ValidatedCodeEpoch &&
                f.Jit.Store.GetStatistics().ContentRetirements == 0,
                "crossing root keeps identical emission through both alias admissions");

            // Alter only immediate bytes on page two; the opcode and low byte
            // on the unchanged first page now decode as MOV eax,257.
            long mutations = f.Statistics.HostBackingMutationScopes;
            var write = f.Io.WriteAt(newFd, new byte[] { 1 }, 0);
            Check(write.Succeeded && write.Value == 1 && f.Statistics.HostBackingMutationScopes > mutations &&
                QemuProvenanceHeader(f, token).Live == 0,
                "real second-page IO mutation revokes the migrated crossing root");
            FullRax(f, 0xfeedfacefeedfaceUL);
            Check(f.Step(pc, signalActor: true) && f.Issued == 2 && f.Completed == 2 && FullRax(f) == 257 &&
                f.Machine()->ip == pc + 5 && f.Machine()->oplen == 0 && f.ResumeToken() != token &&
                QemuProvenanceBacking(f, first) == firstPage && QemuProvenanceBacking(f, second) == newPage,
                "changed second page executes exact MOV257 once with unchanged first-page backing and precise next IP");
            var statistics = f.Statistics;
            var store = f.Jit.Store.GetStatistics();
            Check(statistics.ActualBlockCompilations == compiles + 1 && statistics.CompileAttempts == compiles + 1 &&
                f.Jit.Store.Publications == publications + 1 && store.ContentRetirements == 1 && store.ResidentBodies == 3 &&
                store.TotalFlushes == 0 && store.CapacityRetirements == 0 && statistics.InterpreterAttempts == 0 &&
                statistics.InlineNativeInstructions == (helpersOnly ? 0 : 2) &&
                statistics.SemanticHelperInstructions == (helpersOnly ? 2 : 0) && f.Stop.Reason == HostExecutionStopReason.None,
                "crossing migration retires only the changed root and completes two exact native or typed-helper MOVs");
            QemuProvenanceDrained(f);
        }
        finally
        {
            if (newFd >= 0) f.Io.Close(newFd);
            if (oldFd >= 0) f.Io.Close(oldFd);
        }
    }

    static void QemuProvenanceMovLoop(Span<byte> bytes, int offset)
    {
        bytes[offset] = 0xb8;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.Slice(offset + 1, 4), 1);
        bytes[offset + 5] = 0xeb; bytes[offset + 6] = 0xf9;
    }

    static int QemuProvenanceFile(ControllerFixture f, string name, ReadOnlySpan<byte> bytes)
    {
        var opened = f.Io.CreateMemoryFile(name, 0);
        Check(opened.Succeeded, "provenance witness creates an observer-enrolled memfd");
        try
        {
            var written = f.Io.WriteAt(opened.Value, bytes, 0);
            Check(written.Succeeded && written.Value == bytes.Length, "provenance memfd receives exact initial bytes");
            return opened.Value;
        }
        catch { f.Io.Close(opened.Value); throw; }
    }

    static void QemuProvenanceMapOffset(ControllerFixture f, int fd, ulong address, long offset)
        => f.DataScope(0, () => Check(f.Program.ReserveVirtual(f.Machine()->blink_host_system,
            (long)address, 4096, f.Program.SetProtection(7), fd, offset, true, true) == (long)address,
            "provenance second-page mapping uses actual fixed ReserveVirtual with file offset"));

    static nint QemuProvenanceBacking(ControllerFixture f, ulong address)
    {
        // Demand-commit a real mapping through an audited read, then inspect a
        // fresh root-table walk; no synthetic PTEs or cached TLB identities.
        f.DataScope(0, () =>
        {
            byte scratch = 0;
            Check(f.Program.Load(f.Machine(), (long)address, 1, &scratch) != null,
                "provenance backing is readable through the actual mapped page");
        });
        nint physical = f.Physical(address);
        Check(physical != 0 && JitPageIdentity.TryRead(f.Program, f.Machine(), address, out ulong entry) &&
            (entry & (ulong)(Blink.PAGE_V | Blink.PAGE_U | Blink.PAGE_HOST)) ==
                (ulong)(Blink.PAGE_V | Blink.PAGE_U | Blink.PAGE_HOST) &&
            (entry & (ulong)(Blink.PAGE_XD | Blink.PAGE_RSRV)) == 0 && (nint)f.Program.FindHostPage(entry) == physical,
            "fresh executable root PTE names the actual committed physical backing");
        return physical;
    }

    static TranslationBlockHeader QemuProvenanceHeader(ControllerFixture f, TranslationToken token)
    {
        Check(f.Jit.Store.TryAcquireToken(token, out var lease), "provenance token remains resident for inspection");
        using (lease) return f.Jit.Store.GetHeaderCopy(in lease);
    }

    static TranslationBlockHeader QemuProvenanceSameEmission(ControllerFixture f, TranslationToken token,
        TranslationBlockHeader original, long compiles, long publications, long emitted)
    {
        var current = QemuProvenanceHeader(f, token);
        Check(current.Entry == original.Entry && current.BodyGeneration == original.BodyGeneration &&
            current.Slot == original.Slot && current.Payload == original.Payload && current.PayloadBytes == original.PayloadBytes &&
            current.GuestPc == original.GuestPc && current.GuestLength == original.GuestLength && current.Context == original.Context &&
            f.Statistics.ActualBlockCompilations == compiles && f.Statistics.CompileAttempts == compiles &&
            f.Jit.Store.Publications == publications &&
            f.Statistics.EmittedCodeBytes == emitted,
            "provenance migration preserves exact entry, generation, plans and emission without compilation or publication");
        return current;
    }

    static void QemuProvenanceAliasExposure(ControllerFixture f, TranslationToken token,
        TranslationBlockHeader live, ulong alias, bool revoke)
    {
        f.DataScope(1, () =>
        {
            f.Expose(alias, 1);
            var current = QemuProvenanceHeader(f, token);
            Check(f.Jit.Authority.ActiveBorrows == 1 && current.Entry == live.Entry &&
                current.BodyGeneration == live.BodyGeneration && current.Payload == live.Payload,
                "actual alias pointer admission retains resident emission while its physical write borrow is active");
            // Global mapping/authority epochs may advance for either alias.
            // Per-body Live/CodeEpoch alone establish which backing revoked it.
            Check(revoke ? current.Live == 0 && current.CodeEpoch > live.CodeEpoch :
                current.Live == 1 && current.CodeEpoch == live.CodeEpoch,
                revoke ? "new physical alias revokes the migrated root before caller stores can occur" :
                    "old physical alias no longer revokes the migrated root");
        });
        QemuProvenanceDrained(f);
    }

    static void QemuProvenanceDrained(ControllerFixture f)
    {
        var store = f.Jit.Store.GetStatistics();
        Check(store.ActiveManagedLeases == 0 && store.ActiveNativeEpochs == 0 && store.ActiveDrafts == 0 &&
            f.Jit.Authority.ActiveBorrows == 0 && f.Statistics.ActiveDataBorrows == 0 &&
            !f.MappingBorrow(0) && !f.MappingBorrow(1),
            "provenance witness drains inspection, native, data, physical-write and mapping leases");
    }
}
