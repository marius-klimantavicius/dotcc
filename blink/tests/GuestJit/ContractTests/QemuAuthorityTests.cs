using Managed.Emulation.Execution;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void CheckQemuAuthorityScopes()
    {
        CheckQemuPagedVirtualWrites();
        using var f = new ControllerFixture();
        ulong first = ControllerFixture.Base, other = first + 0x10000;
        f.Map(first); f.Map(other); f.Fill(first); f.Fill(other);
        f.DataScope(0, () =>
        {
            byte* text = f.Program.LookupAddress2(f.Machine(), (long)(first + 256), 0, 0);
            text[0] = (byte)'a'; text[1] = 0;
        });
        Check(f.Acquire(first, 0, out var a, out _, out _), "arena authority first compiled body");
        TranslationToken firstToken = a.Token;
        var firstHeader = f.Jit.Store.GetHeaderCopy(in a); a.Dispose();
        Check(f.Acquire(other, 0, out var b, out _, out _), "arena authority unrelated compiled body");
        TranslationToken otherToken = b.Token;
        var otherHeader = f.Jit.Store.GetHeaderCopy(in b); b.Dispose();
        long publications = f.Jit.Store.Publications;
        var live = f.Statistics;
        Check(live.FixedChargedBytes > f.Jit.Store.GetStatistics().FixedChargedBytes &&
            live.CacheBytes == live.FixedChargedBytes + live.ArenaReservedBytes,
            "public fixed charge includes owner registries, worker scratch and store metadata");
        ulong epoch = f.Jit.Authority.Epoch;
        long revocations = f.Jit.Store.GetStatistics().AuthorityRevocations;
        foreach (int path in new[] { 0, 1, 2, 3, 4 })
        {
            f.DataScope(0, () =>
            {
                byte* scratch = stackalloc byte[16];
                bool success = path switch
                {
                    0 => f.Program.Load(f.Machine(), (long)first, 1, scratch) != null,
                    1 => f.Program.ReserveAddress(f.Machine(), (long)first, 1, false) != null,
                    2 => f.Program.CopyFromUser(f.Machine(), scratch, (long)first, 1) == 0,
                    3 => f.Program.SchlepR(f.Machine(), (long)first, 1) != null,
                    _ => f.Program.LoadStr(f.Machine(), (long)(first + 256)) != null
                };
                Check(success && f.Jit.Authority.ActiveBorrows == 0,
                    "audited read path retains memory lifetime without acquiring write authority");
            });
            Check(f.Jit.Authority.Epoch == epoch &&
                f.Jit.Store.GetStatistics().AuthorityRevocations == revocations,
                "reading a translated physical page does not revoke or advance code authority");
        }
        f.DataScope(0, () =>
        {
            // LookupAddress deliberately permits unchecked stores. Its public
            // pointer result must remain a conservative write admission.
            Check(f.Program.LookupAddress(f.Machine(), (long)first) != null &&
                f.Jit.Authority.ActiveBorrows == 1, "unknown pointer result retains conservative write authority");
            Check(f.Jit.Store.TryAcquireToken(firstToken, out var changed), "revoked physical body remains resident");
            using (changed)
            {
                var h = f.Jit.Store.GetHeaderCopy(in changed);
                Check(h.Live == 0 && h.CodeEpoch > firstHeader.CodeEpoch,
                    "possible store revokes its actual code backing before pointer escape");
            }
            Check(f.Jit.Store.TryAcquireToken(otherToken, out var unchanged), "unrelated physical body remains resident");
            using (unchanged)
            {
                var h = f.Jit.Store.GetHeaderCopy(in unchanged);
                Check(h.Live == 1 && h.CodeEpoch == otherHeader.CodeEpoch,
                    "physical-page index preserves unrelated body authority");
            }
        });
        Check(f.Acquire(first, 0, out var revalidated, out _, out _), "unchanged written-page plan revalidates");
        using (revalidated)
            Check(revalidated.Token == firstToken && f.Jit.Store.Publications == publications,
                "unchanged page revalidation reuses token and emitted body");
        Check(f.Jit.Authority.ActiveBorrows == 0 && f.Statistics.InterpreterAttempts == 0,
            "read/write authority contracts drain scopes without interpreter entry");
        long fixedPeak = f.Statistics.PeakFixedChargedBytes;
        f.Dispose();
        var disposed = f.Statistics;
        Check(disposed.FixedChargedBytes == 0 && disposed.ArenaReservedBytes == 0 && disposed.CacheBytes == 0 &&
            disposed.PeakFixedChargedBytes == fixedPeak && disposed.PeakCacheBytes >= fixedPeak && fixedPeak > 0,
            "public cleanup zeroes current charges and preserves the complete fixed high-water charge");
    }
    static void CheckQemuPagedVirtualWrites()
    {
        foreach (bool codeWrite in new[] { false, true })
        {
            using var f = new ControllerFixture();
            ulong pc = ControllerFixture.Base, data = pc + 0x10000;
            f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
            Check(f.Acquire(pc, 0, out var body, out bool multiple, out _) && multiple,
                "virtual-write witness has trusted resident code");
            using (body)
            {
                var header = f.Jit.Store.GetHeaderCopy(in body);
                nint headerPointer = f.Jit.Store.GetHeader(in body);
                f.At(pc, 1);
                Check(f.Jit.Store.TryBeginNativeEpoch(out var epoch), "virtual-write native lifetime epoch");
                using var poll = f.Stop.AcquireNativePoll();
                long* counts = (long*)System.Runtime.InteropServices.NativeMemory.AllocZeroed(2 * (nuint)sizeof(long));
                if (counts == null) throw new OutOfMemoryException();
                int nativeReturned = 0; FullBlockExit exit = default; Exception? error = null;
                using var watchdog = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
                var reader = new Thread(() =>
                {
                    bool enrolled = false;
                    try
                    {
                        lock (f.Jit.Authority.Gate)
                        {
                            enrolled = f.Jit.Authority.TryEnterNative(f.Jit.Authority.Epoch);
                            if (!enrolled) throw new InvalidOperationException("virtual-write reader admission");
                        }
                        FullBlockFrame frame = new()
                        {
                            Version = FullBlockFrame.AbiVersion, Size = (uint)sizeof(FullBlockFrame),
                            Machine = f.Machine(1), Header = (TranslationBlockHeader*)headerPointer,
                            OwnerCookie = header.OwnerCookie, BodyGeneration = header.BodyGeneration, Context = header.Context,
                            GlobalEpochPointer = f.Jit.Authority.EpochPointer, ObservedGlobalEpoch = f.Jit.Authority.Epoch,
                            CodeAuthorityEpochPointer = f.Jit.Authority.CodeEpochPointer, ObservedCodeAuthorityEpoch = header.ValidatedGlobalEpoch,
                            Issued = counts, Completed = counts + 1, Stop = poll.Pointer, Budget = 1_000_000_000,
                            HelperIndex = FullBlockFrame.NoHelperIndex, NativeLimit = 0
                        };
                        ((delegate* unmanaged<FullBlockFrame*, void>)header.Entry)(&frame);
                        exit = frame.Exit;
                        Volatile.Write(ref nativeReturned, 1);
                        f.Jit.NativeCompleted(in frame);
                    }
                    catch (Exception failure) { error = failure; f.Stop.RequestStop(); }
                    finally { if (enrolled) f.Jit.Authority.ExitNative(); }
                }) { IsBackground = true };
                reader.Start();
                try
                {
                    Check(SpinWait.SpinUntil(() => Volatile.Read(ref counts[0]) > 1024, TimeSpan.FromSeconds(5)),
                        "actual native loop overlaps guest writable lookup");
                    ulong beforeEpoch = f.Jit.Authority.Epoch;
                    ulong beforeCodeEpoch = f.Jit.Authority.CodeEpoch;
                    long beforeNotifications = f.Statistics.VirtualWriteNotifications;
                    f.DataScope(0, () =>
                    {
                        byte* pointer = f.Program.LookupAddress2(f.Machine(), (long)(codeWrite ? pc : data), 0, 0);
                        Check(pointer != null && f.Jit.Authority.ActiveBorrows > 0,
                            "actual writable lookup retains physical authority scope");
                        if (codeWrite)
                        {
                            Check(f.Jit.Authority.Epoch > beforeEpoch && f.Jit.Authority.CodeEpoch > beforeCodeEpoch && Volatile.Read(ref nativeReturned) == 1 &&
                                exit == FullBlockExit.Invalidated && f.Jit.Store.GetHeaderCopy(in body).Live == 0,
                                "physical code exposure drains native before returning the guest pointer");
                            pointer[1] = 2; // change MOV eax,1 to MOV eax,2 only AFTER its reader drained
                        }
                        else
                        {
                            Check(f.Jit.Authority.Epoch > beforeEpoch && f.Jit.Authority.CodeEpoch == beforeCodeEpoch && Volatile.Read(ref nativeReturned) == 1 &&
                                exit == FullBlockExit.Invalidated &&
                                f.Jit.Store.GetHeaderCopy(in body).Live == 1,
                                "new non-code writable exposure drains native frames while retaining unchanged code content");
                            pointer[0] = 0x5a;
                        }
                    });
                    Check(f.Statistics.VirtualWriteNotifications > beforeNotifications &&
                        f.Statistics.ConservativeVirtualWriteBarriers == 0 && f.Jit.Authority.ActiveBorrows == 0,
                        "paged write notification uses physical callback and releases its borrow");
                    if (!codeWrite) f.Stop.RequestStop();
                    Check(reader.Join(TimeSpan.FromSeconds(5)), "virtual-write native reader completes");
                    if (error != null) throw new InvalidOperationException("virtual-write reader", error);
                    Check(counts[0] == counts[1] && exit == FullBlockExit.Invalidated,
                        "data/code write paths preserve exact native issuance and their real exit causes");
                }
                finally
                {
                    f.Stop.RequestStop();
                    if (!reader.Join(TimeSpan.FromSeconds(5))) throw new TimeoutException("virtual-write reader did not stop");
                    epoch.Dispose(); System.Runtime.InteropServices.NativeMemory.Free(counts);
                }
            }
        }
        using (var f = new ControllerFixture())
        {
            var machine = f.Machine(); var mode = machine->mode; var metal = machine->metal;
            try
            {
                machine->metal = true; machine->mode.omode = Managed.Emulation.BlinkCore.XED_MODE_LEGACY;
                machine->mode.genmode = Managed.Emulation.BlinkCore.XED_GEN_MODE_REAL;
                ulong before = f.Jit.Authority.Epoch;
                f.Jit.NotifyVirtualWrite(machine, (long)ControllerFixture.Base, 4096);
                Check(f.Jit.Authority.Epoch > before && f.Statistics.ConservativeVirtualWriteBarriers == 1,
                    "direct nonpaged context keeps conservative early barrier without touching real memory");
            }
            finally { machine->mode = mode; machine->metal = metal; }
        }
    }
}
