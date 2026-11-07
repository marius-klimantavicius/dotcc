using Managed.Emulation.Execution;

namespace Managed.Emulation.Execution
{
    internal sealed partial class FullTranslationJit
    {
        internal void TestCleanupPublicationOwner(int worker)
        {
            lock (Authority.Gate) RequestPublication(worker);
        }
    }
}

unsafe partial class Program
{
    static void CheckFullPublicationCleanupFastPath()
    {
        using var fixture = new ThreadedGuestExecution.FullControllerFixture();
        foreach (bool foreignPublication in new[] { false, true })
        {
            if (foreignPublication) fixture.Jit.TestCleanupPublicationOwner(1);
            Exception? failure = null;
            using var done = new ManualResetEventSlim();
            var cleanup = new Thread(() =>
            {
                try { fixture.Jit.CancelPublication(0); }
                catch (Exception e) { failure = e; }
                finally { done.Set(); }
            });
            bool completed;
            lock (fixture.Jit.Authority.Gate)
            {
                cleanup.Start();
                completed = done.Wait(TimeSpan.FromSeconds(2));
            }
            Check(cleanup.Join(TimeSpan.FromSeconds(2)) && completed && failure == null,
                "unowned publication cleanup requires no global authority gate");
            if (foreignPublication)
            {
                Check(!fixture.Jit.Store.TryBeginNativeEpoch(out _),
                    "foreign cleanup cannot reopen another compiler's publication");
                fixture.Jit.CancelPublication(1);
            }
            Check(fixture.Jit.Store.TryBeginNativeEpoch(out var reopened),
                "owning compiler cleanup reopens publication admission");
            reopened.Dispose();
        }
    }
}
