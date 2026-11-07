using Managed.Emulation.Execution;
using Managed.Emulation.Host;

unsafe partial class Program
{
    // These deterministic protocol schedules use synthetic physical identities
    // and counters. They do NOT execute atomic/native bodies or qualify actual
    // producer copy coverage. Parent owns the real CPU/syscall/native races.
    static void CheckFullSemanticAuthority()
    {
        CheckSemanticAdmissionHandshake();
        CheckSemanticConstructorRollback();
        CheckSemanticReleaseWithoutGate();
        CheckSemanticReaderNeedsGate();
        CheckSemanticRecursiveSuspension();
        CheckSemanticNonCodeExposureDrain();
        CheckSemanticPendingWritersAndCleanup();
        CheckSemanticHelperCleanupUnwind();
        CheckSemanticControllerPendingStop();
        CheckSemanticSnapshotQuota();
    }
    sealed class SemanticTestIo : IDisposable
    {
        internal readonly InstanceIo Value = new(new Dictionary<string, ReadOnlyMemory<byte>>());
        public void Dispose() => Value.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
    static FullCodeAuthority SemanticAuthority(InstanceIo io, long budget = 3L << 20)
        => new(io, 8UL << 20, 3, budget);
    static void CheckSemanticAdmissionHandshake()
    {
        foreach (bool published in new[] { false, true })
        {
            using var ioCleanup = new SemanticTestIo();
        var io = ioCleanup.Value;
            using var a = SemanticAuthority(io);
            Check(a.Charge == FullCodeAuthority.ChargeFor(8UL << 20, 3, 3L << 20) &&
                FullCodeAuthority.SemanticChargeFor(3) == 192 + 20 * 3, "reader state is bounded and precharged");
            a.TestSemanticAdmission = (worker, atPublication) =>
            {
                if (worker != 1 || atPublication != published) return;
                a.TestSemanticAdmission = null;
                lock (a.Gate)
                {
                    Check(a.TryReserveNativeTicket(0), "writer closes admission between reader handshake steps");
                    Check(a.NativeReservationDrained(0) == !published, "writer sees every reader published before its scan");
                }
            };
            Check(!a.TryEnterCpuSemanticRead(1, out var rejected), "reader double-check rolls back after pending publication");
            rejected.Dispose();
            Check(a.CpuSemanticDepth(1) == 0 && !a.SemanticAdmitted(1) && a.NativeReservationDrained(0),
                "failed admission publishes no scope and performs no data read");
            Check(!a.TryBeginHostBackingRead(), "host read attempt under closed admission returns immediately");
            lock (a.Gate)
            {
                Check(a.TryPromoteNativeTicket(0, 0x1000), "writer promotes only after double-check rollback");
                Check(a.TryEnterNativeTicket(0, a.Epoch), "exact ticket alone enters native enrollment");
            }
            a.ExitNative(); a.EndNativeTicket(0);
            Check(a.PendingAtomicWorker == -1 && a.SemanticReadersDrained(), "ticket end unconditionally reopens admission");
            a.TestHostAdmission = atPublication =>
            {
                if (atPublication != published) return;
                a.TestHostAdmission = null;
                lock (a.Gate)
                {
                    Check(a.TryReserveNativeTicket(0), "host backing reader races pending publication");
                    Check(a.NativeReservationDrained(0) == !published, "writer sees counted non-worker backing readers");
                }
            };
            Check(!a.TryBeginHostBackingRead() && a.HostShortReaders == 0,
                "non-worker backing reader rolls back both handshake interleavings");
            a.AbortNativeReservation(0);
            using var outer = a.EnterCpuSemanticRead(1);
            lock (a.Gate) Check(a.TryReserveNativeTicket(0), "writer may reserve behind an existing CPU helper");
            using (var nested = a.EnterCpuSemanticRead(1))
            {
                a.BeginGuestRead(1);
                Check(a.CpuSemanticDepth(1) == 2 && a.ShortSemanticDepth(1) == 1 && !a.NativeReservationDrained(0),
                    "existing CPU and short nesting does not self-wait behind pending");
                a.EndGuestRead(1);
            }
            a.AbortNativeReservation(0);
        }
    }
    static void CheckSemanticConstructorRollback()
    {
        using var ioCleanup = new SemanticTestIo();
        var io = ioCleanup.Value;
        using var a = SemanticAuthority(io);
        var marker = new InvalidOperationException("injected reader admission failure");
        a.TestSemanticAdmission = (_, published) => { if (published) throw marker; };
        try { using var scope = a.EnterCpuSemanticRead(0); Check(false, "injected reader constructor must throw"); }
        catch (InvalidOperationException error) when (ReferenceEquals(error, marker)) { }
        Check(a.SemanticReadersDrained() && a.CpuSemanticDepth(0) == 0, "reader constructor rolls back its actual publication");
        a.TestSemanticAdmission = null;
        a.TestHostAdmission = published => { if (published) throw marker; };
        try { a.TryBeginHostBackingRead(); Check(false, "injected host constructor must throw"); }
        catch (InvalidOperationException error) when (ReferenceEquals(error, marker)) { }
        Check(a.HostShortReaders == 0, "host constructor rolls back its counted publication");
        a.TestHostAdmission = null;
        using (var scope = a.EnterCpuSemanticRead(0))
        {
            a.BeginGuestWrite(0);
            Check(a.CopyWriters == 1 && a.ActiveBorrows == 0, "bounded copy writer does not create a lasting physical borrow");
            a.EndGuestWrite(0);
        }
        Check(a.CopyWriters == 0 && a.SemanticReadersDrained(), "short writer and outer reader release independently");
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        for (int n = 0; n < 64; ++n)
        {
            using var scope = a.EnterCpuSemanticRead(0);
            a.BeginGuestRead(0); a.EndGuestRead(0);
        }
        Check(GC.GetAllocatedBytesForCurrentThread() == allocated, "warm semantic value scopes do not allocate");
        a.EndGuestRead(0); // malformed release is nonthrowing and fails closed
        Check(a.SemanticQuarantined && a.ShortSemanticDepth(0) == 0 && !a.TryEnterNative(a.Epoch),
            "bad release quarantines native admission without wrapping a count");
    }
    static void CheckSemanticReleaseWithoutGate()
    {
        using var ioCleanup = new SemanticTestIo();
        var io = ioCleanup.Value;
        using var a = SemanticAuthority(io);
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        Exception? failure = null;
        var reader = new Thread(() =>
        {
            try
            {
                using var scope = a.EnterCpuSemanticRead(1);
                entered.Set(); Check(release.Wait(TimeSpan.FromSeconds(5)), "reader release schedule arrives");
            }
            catch (Exception error) { failure = error; }
        });
        reader.Start();
        try
        {
            Check(entered.Wait(TimeSpan.FromSeconds(5)), "reader enters before Gate is held by writer");
            lock (a.Gate)
            {
                Check(a.TryReserveNativeTicket(0) && !a.TryPromoteNativeTicket(0, 0x1000), "promotion observes a live foreign reader without waiting");
                release.Set();
                Check(reader.Join(TimeSpan.FromSeconds(5)), "reader release completes while writer retains Gate");
                Check(failure == null && a.NativeReservationDrained(0), "reader release acquires no authority gate");
            }
        }
        finally { release.Set(); reader.Join(TimeSpan.FromSeconds(5)); a.AbortNativeReservation(0); }
    }
    static void CheckSemanticReaderNeedsGate()
    {
        using var ioCleanup = new SemanticTestIo();
        var io = ioCleanup.Value;
        using var a = SemanticAuthority(io);
        using var entered = new ManualResetEventSlim();
        using var finish = new ManualResetEventSlim();
        Exception? failure = null;
        var reader = new Thread(() =>
        {
            try
            {
                using var scope = a.EnterCpuSemanticRead(1);
                entered.Set(); Check(finish.Wait(TimeSpan.FromSeconds(5)), "existing helper receives cleanup schedule");
                lock (a.Gate) a.Expose(1, 0x2000);
                a.EndBorrow(1);
            }
            catch (Exception error) { failure = error; }
        });
        reader.Start();
        try
        {
            Check(entered.Wait(TimeSpan.FromSeconds(5)), "existing helper has published its participation");
            lock (a.Gate)
            {
                Check(a.TryReserveNativeTicket(0) && !a.TryPromoteNativeTicket(0, 0x2000), "pending writer returns from promotion while reader needs Gate");
                finish.Set();
            }
            Check(reader.Join(TimeSpan.FromSeconds(5)) && failure == null, "writer releases Gate so existing helper can expose and clean up");
            lock (a.Gate)
            {
                Check(a.TryPromoteNativeTicket(0, 0x2000), "drained reservation revalidates and promotes");
                // Native has returned, but ticket finalization has not run.
                a.DrainNativeForFiniteGateRead();
                a.BeginGuestRead(2); a.EndGuestRead(2);
                Check(a.TryBeginHostBackingRead(), "finite gate read does not wait for pending/ticket finalization");
                a.EndHostBackingRead();
            }
            a.EndNativeTicket(0);
        }
        finally { finish.Set(); reader.Join(TimeSpan.FromSeconds(5)); a.AbortNativeReservation(0); }
    }
    static void CheckSemanticRecursiveSuspension()
    {
        using var ioCleanup = new SemanticTestIo();
        var io = ioCleanup.Value;
        using var a = SemanticAuthority(io);
        using var outer = a.EnterCpuSemanticRead(0);
        using var nested = a.EnterCpuSemanticRead(0);
        using (var pause = a.PauseCpuSemanticRead(0))
        {
            Check(a.CpuSemanticDepth(0) == 0 && a.SemanticReadersDrained(), "signal actor suspends every older CPU frame");
            lock (a.Gate)
            {
                Check(a.TryReserveNativeTicket(0) && a.TryPromoteNativeTicket(0, 0x3000), "own-worker child ticket does not wait on suspended parent");
                Check(a.TryEnterNativeTicket(0, a.Epoch), "child obtains fresh exclusive admission");
            }
            a.ExitNative(); a.EndNativeTicket(0);
            using var child = a.EnterCpuSemanticRead(0);
            Check(a.CpuSemanticDepth(0) == 1, "child helper starts with fresh logical nesting");
        }
        Check(a.CpuSemanticDepth(0) == 2 && a.SemanticAdmitted(0), "normal recursive return restores exact parent nesting");
        using var waiting = new ManualResetEventSlim();
        Exception? failure = null;
        Thread? writer = null;
        var marker = new InvalidOperationException("recursive callback unwind");
        try
        {
            using var pause = a.PauseCpuSemanticRead(0);
            lock (a.Gate) Check(a.TryReserveNativeTicket(1), "foreign writer reserves while parent C is paused");
            a.TestSemanticPending = worker => { if (worker == 0) waiting.Set(); };
            writer = new Thread(() =>
            {
                try { Check(waiting.Wait(TimeSpan.FromSeconds(5)), "restoration attempts fresh admission before C resumes"); }
                catch (Exception error) { failure = error; }
                finally { a.AbortNativeReservation(1); }
            });
            writer.Start(); throw marker;
        }
        catch (InvalidOperationException error) when (ReferenceEquals(error, marker)) { }
        finally { a.TestSemanticPending = null; a.AbortNativeReservation(1); writer?.Join(TimeSpan.FromSeconds(5)); }
        Check(failure == null && waiting.IsSet && a.CpuSemanticDepth(0) == 2 && a.SemanticAdmitted(0),
            "exceptional recursion waits without Gate and restores before outer CPU unwind");
        a.BeginGuestRead(0);
        bool recursionRejected = false;
        try { a.PauseCpuSemanticRead(0); }
        catch (InvalidOperationException) { recursionRejected = true; }
        finally { a.EndGuestRead(0); }
        Check(recursionRejected, "short copies may not recurse into guest execution");
    }
    static void CheckSemanticNonCodeExposureDrain()
    {
        using var ioCleanup = new SemanticTestIo();
        var io = ioCleanup.Value;
        using var a = SemanticAuthority(io);
        lock (a.Gate)
        {
            Check(a.TryReserveNativeTicket(0) && a.TryPromoteNativeTicket(0, 0x4000), "checked data ticket prepares non-code page");
            Check(a.TryEnterNativeTicket(0, a.Epoch), "synthetic enrolled frame models ticket before RET");
        }
        ulong before = a.Epoch;
        using var finished = new ManualResetEventSlim();
        Exception? failure = null;
        bool codeRevoked = true, enrolled = true;
        var exposure = new Thread(() =>
        {
            try { lock (a.Gate) codeRevoked = a.Expose(1, 0x4000); }
            catch (Exception error) { failure = error; }
            finally { finished.Set(); }
        });
        exposure.Start();
        try
        {
            Check(SpinWait.SpinUntil(() => a.Epoch != before, TimeSpan.FromSeconds(5)), "new non-code exposure advances epoch before pointer escape");
            Check(!finished.IsSet, "new writable exposure drains an active native frame even without code aliases");
            a.ExitNative(); enrolled = false;
            Check(exposure.Join(TimeSpan.FromSeconds(5)) && failure == null && !codeRevoked,
                "non-code pointer escapes only after drain while code revocation remains separate");
        }
        finally
        {
            if (enrolled) a.ExitNative();
            exposure.Join(TimeSpan.FromSeconds(5)); a.EndNativeTicket(0); a.EndBorrow(1);
        }
    }
    static void CheckSemanticPendingWritersAndCleanup()
    {
        using var ioCleanup = new SemanticTestIo();
        var io = ioCleanup.Value;
        using (var a = SemanticAuthority(io, 16))
        {
            lock (a.Gate)
            {
                for (int page = 1; page <= 17; ++page) a.Expose(1, (nint)(page * 4096));
                Check(a.Saturated && a.TryReserveNativeTicket(0) && !a.TryPromoteNativeTicket(0, 0x5000),
                    "saturated unrepresented foreign exposure still blocks ticket promotion");
            }
            a.EndBorrow(1);
            lock (a.Gate)
            {
                Check(a.TryPromoteNativeTicket(0, 0x5000), "saturated exposures end with their exact worker lifetime");
                a.BeginMappingMutation();
            }
            a.EndNativeTicket(0);
            Check(a.PendingAtomicWorker == -1 && a.MappingWriters == 1 && !a.TryEnterNative(a.Epoch),
                "ticket cleanup releases only its own count while foreign mapping scope remains");
            a.EndMappingMutation();
            using (var cpu = a.EnterCpuSemanticRead(1))
            {
                lock (a.Gate) Check(a.TryReserveNativeTicket(0), "writer reserves behind admitted CPU frame");
                a.BeginGuestWrite(1);
                Check(a.CopyWriters == 1 && !a.NativeReservationDrained(0), "existing reader can finish a bounded write behind pending");
                a.EndGuestWrite(1);
            }
            Check(a.NativeReservationDrained(0), "pending writer sees independent short and CPU cleanup");
            a.BeginMappingMutation();
            try
            {
                a.BeginGuestRead(1, mappingOwned: true);
                a.EndGuestRead(1);
                a.BeginGuestWrite(1, mappingOwned: true);
                a.EndGuestWrite(1);
                Check(a.TryBeginHostBackingRead(mappingOwned: true), "mapping-owned backing copy does not wait behind pending mapper drain");
                a.EndHostBackingRead();
                lock (a.Gate) Check(!a.TryPromoteNativeTicket(0, 0x5000), "mapping capability still excludes native promotion during its finite copies");
            }
            finally { a.EndMappingMutation(); }
            a.AbortNativeReservation(0); a.AbortNativeReservation(0);
            Check(a.PendingAtomicWorker == -1 && a.SemanticReadersDrained(), "pending cancellation is balanced and idempotent");
        }
        using (var a = SemanticAuthority(io))
        {
            *a.EpochPointer = ulong.MaxValue;
            bool rejected = false;
            try { lock (a.Gate) a.TryReserveNativeTicket(0); }
            catch (InvalidOperationException) { rejected = true; }
            Check(rejected && a.PendingAtomicWorker == -1 && a.MappingWriters == 0, "failed reservation rolls back pending ownership");
        }
        using (var a = SemanticAuthority(io))
        {
            lock (a.Gate) Check(a.TryReserveNativeTicket(0) && a.TryPromoteNativeTicket(0, 0x6000), "ticket prepares cleanup exception schedule");
            *a.EpochPointer = ulong.MaxValue;
            a.EndNativeTicket(0);
            Check(a.SemanticQuarantined && !a.TryEnterNative(a.Epoch) && a.PendingAtomicWorker == -1 &&
                a.MappingWriters == 0 && a.SemanticReadersDrained(),
                "ticket release quarantines without throwing and clears reservation on epoch exhaustion");
        }
    }
    static void CheckSemanticSnapshotQuota()
    {
        using var f = new ThreadedGuestExecution.FullControllerFixture();
        var copies = (IHostGuestMemoryCopies)f.Owner;
        Check(copies.TryReserveGuestCopyBytes(32) && f.Owner.GuestCopyBytes == 32, "owned snapshot header and payload are charged");
        Check(!copies.TryReserveGuestCopyBytes(ulong.MaxValue) &&
            !copies.TryReserveGuestCopyBytes(ThreadedGuestExecution.FullControllerFixture.MemoryLimit), "snapshot admission rejects overflow and over-budget lifetime");
        Exception? failure = null;
        var cleanup = new Thread(() =>
        {
            try { copies.ReleaseGuestCopyBytes(32); }
            catch (Exception error) { failure = error; }
        });
        cleanup.Start(); Check(cleanup.Join(TimeSpan.FromSeconds(5)) && failure == null && f.Owner.GuestCopyBytes == 0,
            "tagged snapshot release needs no current guest worker");
        Check(copies.TryReserveGuestCopyBytes(ThreadedGuestExecution.FullControllerFixture.MemoryLimit), "entire configured snapshot bound is usable");
        copies.ReleaseGuestCopyBytes(ThreadedGuestExecution.FullControllerFixture.MemoryLimit);
        copies.ReleaseGuestCopyBytes(1);
        Check(f.Owner.GuestCopyBytes == 0 && f.Jit.Authority.SemanticQuarantined && !copies.TryReserveGuestCopyBytes(1),
            "snapshot underflow quarantines admission and never wraps the resource count");
    }
    static void CheckSemanticHelperCleanupUnwind()
    {
        using var f = new ThreadedGuestExecution.FullControllerFixture(helpersOnly: true);
        ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
        f.Map(pc); f.Fill(pc); WriteControllerCode(f, pc, [0x90]);
        var marker = new InvalidOperationException("injected helper cleanup exception");
        nint activeDecoded = 0;
        f.Owner.TestHelperMappingEnded = pointer =>
        {
            activeDecoded = (nint)((Managed.Emulation.BlinkCore.Machine*)pointer)->xedd;
            Check(f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0)) == 1,
                "CPU permit remains held through final helper access and cleanup");
            throw marker;
        };
        try { f.Step(pc); Check(false, "injected helper cleanup exception must propagate"); }
        catch (InvalidOperationException error) when (ReferenceEquals(error, marker)) { }
        finally { f.Owner.TestHelperMappingEnded = null; }
        Check(activeDecoded != 0 && (nint)f.Machine()->xedd != activeDecoded && f.Jit.Authority.SemanticReadersDrained() &&
            f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0)) == 0 && f.Jit.Authority.ActiveBorrows == 0 &&
            f.Statistics.ActiveDataBorrows == 0 && f.Jit.Authority.PendingAtomicWorker == -1,
            "helper cleanup exception restores xedd and releases CPU, physical and passive capabilities");
    }
    static void CheckSemanticControllerPendingStop()
    {
        using var f = new ThreadedGuestExecution.FullControllerFixture();
        ulong pc = ThreadedGuestExecution.FullControllerFixture.Base, destination = pc + 256;
        f.Map(pc); f.Fill(pc);
        byte[] bytes = [0x48,0xbb,0,0,0,0,0,0,0,0, 0xb8,0x78,0x56,0x34,0x12, 0x48,0x89,0x03, 0xeb,0xfe];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(2,8), destination);
        WriteControllerCode(f, pc, bytes);
        nint physical = f.Physical(destination);
        ulong before = ((ulong*)((byte*)physical + (int)(destination & 4095)))[0];
        // Synthetic foreign participation controls the exact reservation wait;
        // native/controller execution is real only when the parent runs this.
        using var reader = f.Jit.Authority.EnterCpuSemanticRead(f.WorkerId(1));
        int reservations = 0;
        f.Jit.Authority.TestReservationPublished = _ =>
        {
            ++reservations;
            Check(!f.Jit.Authority.NativeReservationDrained(f.WorkerId(0)), "controller reservation sees foreign semantic participant");
            f.Stop.RequestStop();
        };
        f.Counts(100);
        using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        try
        {
            f.Step(pc);
            for (int attempt = 0; attempt < 8 && f.Stop.Reason == HostExecutionStopReason.None; ++attempt) f.Continue();
        }
        finally { f.Jit.Authority.TestReservationPublished = null; }
        Check(reservations == 1 && f.Completed == 2 && f.Issued == 3 &&
            ((ulong*)((byte*)physical + (int)(destination & 4095)))[0] == before,
            "stop abandons pending store before effects without charging another issue");
        var store = f.Jit.Store.GetStatistics();
        Check(f.Jit.Authority.PendingAtomicWorker == -1 && f.Jit.Authority.MappingWriters == 0 &&
            store.ActiveManagedLeases == 0 && store.ActiveNativeEpochs == 0 && f.Statistics.ActiveDataBorrows == 0,
            "pending stop releases reservation, fetch pin, translation lease and store epoch");
    }
}
