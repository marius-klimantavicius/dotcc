using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void CheckQemuPublicationProgress()
    {
        using var f = new ControllerFixture(workers: 3, retentionBudget: 4L << 20);
        ulong pc = ControllerFixture.Base, missing = pc + 0x10000;
        f.Map(pc); f.Fill(pc); f.Map(missing); f.Fill(missing);
        Check(f.Acquire(pc, 0, out var resident, out bool multiple, out _) && multiple,
            "publication witness resident native loop");
        using (resident)
        {
            var header = f.Jit.Store.GetHeaderCopy(in resident);
            nint headerPointer = f.Jit.Store.GetHeader(in resident);
            f.At(pc, 0); f.At(pc, 1);
            Check(f.Jit.Store.TryBeginNativeEpoch(out var firstEpoch), "publication reader A epoch");
            Check(f.Jit.Store.TryBeginNativeEpoch(out var secondEpoch), "publication reader B epoch");
            using var poll = f.Stop.AcquireNativePoll();
            long* counters = (long*)NativeMemory.AllocZeroed(4 * (nuint)sizeof(long));
            if (counters == null) throw new OutOfMemoryException();
            var failures = new Exception?[2]; var exits = new FullBlockExit[2];
            Thread[] readers = new Thread[2];
            using var watchdog = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
            for (int i = 0; i < readers.Length; ++i)
            {
                int reader = i;
                readers[i] = new Thread(() =>
                {
                    bool enrolled = false;
                    try
                    {
                        lock (f.Jit.Authority.Gate)
                        {
                            enrolled = f.Jit.Authority.TryEnterNative(f.Jit.Authority.Epoch);
                            if (!enrolled) throw new InvalidOperationException("Reader enrollment denied before compiler phase.");
                        }
                        FullBlockFrame frame = new()
                        {
                            Version = FullBlockFrame.AbiVersion, Size = (uint)sizeof(FullBlockFrame),
                            Machine = f.Machine(reader), Header = (TranslationBlockHeader*)headerPointer,
                            OwnerCookie = header.OwnerCookie, BodyGeneration = header.BodyGeneration, Context = header.Context,
                            GlobalEpochPointer = f.Jit.Authority.EpochPointer, ObservedGlobalEpoch = f.Jit.Authority.Epoch,
                            CodeAuthorityEpochPointer = f.Jit.Authority.CodeEpochPointer, ObservedCodeAuthorityEpoch = header.ValidatedGlobalEpoch,
                            Stop = poll.Pointer, Issued = counters + reader * 2, Completed = counters + reader * 2 + 1,
                            NativeLimit = 0, Budget = 1_000_000_000, HelperIndex = FullBlockFrame.NoHelperIndex
                        };
                        ((delegate* unmanaged<FullBlockFrame*, void>)header.Entry)(&frame);
                        exits[reader] = frame.Exit;
                        f.Jit.NativeCompleted(in frame);
                    }
                    catch (Exception error) { failures[reader] = error; f.Stop.RequestStop(); }
                    finally { if (enrolled) f.Jit.Authority.ExitNative(); }
                }) { IsBackground = true };
                readers[i].Start();
            }
            try
            {
                Check(SpinWait.SpinUntil(() => Volatile.Read(ref counters[0]) > 1024 && Volatile.Read(ref counters[2]) > 1024,
                    TimeSpan.FromSeconds(5)), "two actual native readers overlap before compiler request");
                long attempts = f.Statistics.CompileAttempts;
                Check(!f.Acquire(missing, 2, out var unavailable, out _, out int error) && error == 0,
                    "third worker designates its missing translation without consuming a guest issue");
                unavailable.Dispose();
                Check(!f.Jit.Store.TryBeginNativeEpoch(out var denied),
                    "publication phase closes store admission for all replacement native readers");
                denied.Dispose();
                foreach (var reader in readers) Check(reader.Join(TimeSpan.FromSeconds(5)), "epoch kick drains actual native reader");
                for (int i = 0; i < 2; ++i)
                {
                    if (failures[i] != null) throw new InvalidOperationException("native publication reader", failures[i]);
                    Check(exits[i] == FullBlockExit.Invalidated && counters[i * 2] == counters[i * 2 + 1],
                        "native reader exits on publication epoch with exact issuance");
                }
                firstEpoch.Dispose();
                Check(!f.Jit.Store.TryBeginNativeEpoch(out denied), "first release cannot admit a replacement overlapping reader");
                denied.Dispose(); secondEpoch.Dispose();
                Check(!f.Jit.Store.TryBeginNativeEpoch(out denied), "quiescent phase remains reserved for designated compiler");
                denied.Dispose();
                for (int i = 0; i < 8; ++i)
                {
                    Check(!f.Acquire(pc, i & 1, out var blocked, out _, out error) && error == 0,
                        "resident worker cannot steal designated compiler's publication phase");
                    blocked.Dispose();
                }
                Check(f.Statistics.CompileAttempts == attempts && f.Issued == 0 && f.Completed == 0,
                    "waiting phase does not regenerate bodies or reserve guest instructions");
                Check(f.Acquire(missing, 2, out var published, out _, out error) && error == 0,
                    "designated third worker publishes after both old readers drain");
                published.Dispose();
                Check(f.Jit.Store.TryBeginNativeEpoch(out var reopened), "publication reopens native admission");
                reopened.Dispose();
                Check(f.Statistics.ActualBlockCompilations == 2 && f.Statistics.PublicationQuiescenceRequests == 1 &&
                    f.Statistics.NativePublicationAdmissionDenials >= 8 && f.Statistics.InterpreterAttempts == 0,
                    "publication phase records one actual compile and bounded nonallocating admission denials");

                // Actual controller cancellation owns finally-based reopening.
                Check(f.Jit.Store.TryBeginNativeEpoch(out var held), "cancellation witness old epoch");
                long requests = f.Statistics.PublicationQuiescenceRequests;
                var stopper = new Thread(() =>
                {
                    if (SpinWait.SpinUntil(() => f.Statistics.PublicationQuiescenceRequests > requests, TimeSpan.FromSeconds(5)))
                        f.Stop.RequestStop();
                    else f.Stop.RequestStop();
                }) { IsBackground = true };
                stopper.Start();
                try
                {
                    Check(f.Step(missing + 16, 2), "stopped compiler leaves approved issue without executing a guest opcode");
                    Check(stopper.Join(TimeSpan.FromSeconds(5)), "compiler cancellation thread terminates");
                    Check(f.Jit.Store.TryBeginNativeEpoch(out reopened), "controller finally reopens admission on cancelled compiler");
                    reopened.Dispose();
                }
                finally { f.Stop.RequestStop(); stopper.Join(TimeSpan.FromSeconds(5)); held.Dispose(); }
            }
            finally
            {
                f.Stop.RequestStop();
                foreach (var reader in readers)
                    if (!reader.Join(TimeSpan.FromSeconds(5))) throw new TimeoutException("Publication native reader did not stop.");
                firstEpoch.Dispose(); secondEpoch.Dispose(); f.Jit.CancelPublication(2);
                NativeMemory.Free(counters);
            }
        }
    }
}
