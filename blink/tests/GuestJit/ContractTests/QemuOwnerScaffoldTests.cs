using Managed.Emulation.Execution;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void CheckQemuOwnerScaffoldLifetime()
    {
        using var f = new ControllerFixture();
        var setup = f.Statistics;
        Check(setup.SharedScaffoldEmittedBytes > 0 && setup.SharedScaffoldEmittedBytes <= FullBlockEmitter.ScaffoldCapacity &&
            setup.SharedScaffoldReservedBytes >= setup.SharedScaffoldEmittedBytes &&
            setup.SharedScaffoldReservedBytes == setup.PeakSharedScaffoldReservedBytes,
            "one owner common slab has an explicit emitted/reserved charge before guest work");
        f.Map(ControllerFixture.Base); f.Fill(ControllerFixture.Base);
        Check(f.Step(ControllerFixture.Base, signalActor: true) && f.Completed == 1 && FullRax(f) == 1,
            "actual controller executes a compact body through its owner common slab");
        Check(f.Jit.Store.TryAcquireToken(f.ResumeToken(), out var held), "owner common lifetime held body lease");
        bool denied = false;
        try { f.Jit.Dispose(); }
        catch (InvalidOperationException) { denied = true; }
        Check(denied && f.Statistics.SharedScaffoldReservedBytes == setup.SharedScaffoldReservedBytes &&
            f.Statistics.CacheBytes > 0, "refused active store disposal cannot release common code");
        held.Dispose();
        long compilations = f.Statistics.ActualBlockCompilations;
        Check(f.Jit.Store.TryFlush(out var failure) && failure == TranslationStoreFailure.None &&
            f.Jit.Store.GetStatistics().ResidentBodies == 0 &&
            f.Statistics.SharedScaffoldReservedBytes == setup.SharedScaffoldReservedBytes,
            "quiescent whole-body flush preserves the single owner slab");
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Check(f.Step(ControllerFixture.Base, signalActor: true) && f.Completed == 2 && FullRax(f) == 1 &&
            f.Statistics.ActualBlockCompilations == compilations + 1 &&
            f.Statistics.SharedScaffoldEmittedBytes == setup.SharedScaffoldEmittedBytes,
            "new body after flush and GC reuses common code without another slab emission");
        f.Dispose();
        var final = f.Statistics;
        Check(final.SharedScaffoldReservedBytes == 0 && final.CacheBytes == 0 && final.RetainedNativeBytes == 0 &&
            final.SharedScaffoldEmittedBytes == setup.SharedScaffoldEmittedBytes &&
            final.PeakSharedScaffoldReservedBytes == setup.SharedScaffoldReservedBytes && final.InterpreterAttempts == 0,
            "quiescent owner cleanup releases common code and retains its separate diagnostic totals");
    }
}
