using System;
using System.Collections.Concurrent;
using System.Threading;

internal static unsafe class Program
{
    private static Barrier? rendezvous;
    private static readonly ManualResetEventSlim entered = new();
    private static readonly ManualResetEventSlim release = new();
    private static int Rendezvous(ConcurrencyCore owner, int marker)
    {
        Require(rendezvous!.SignalAndWait(TimeSpan.FromSeconds(30)), "concurrent rendezvous timed out");
        return marker;
    }
    private static int Wait(ConcurrencyCore owner, int marker)
    {
        entered.Set();
        Require(release.Wait(TimeSpan.FromSeconds(30)), "held call timed out");
        return marker;
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private static void ParallelHeap(ConcurrencyCore first, ConcurrencyCore second, bool independent)
    {
        const int count = 8;
        using var barrier = new Barrier(count);
        rendezvous = barrier;
        var failures = new ConcurrentQueue<Exception>();
        var workers = new Thread[count];
        for (int i = 0; i < count; ++i)
        {
            int marker = 100 + i;
            ConcurrencyCore owner = independent && (i & 1) != 0 ? second : first;
            workers[i] = new Thread(() =>
            {
                try
                {
                    int result = owner.owner_concurrency(marker, 4000, &Rendezvous);
                    Require(result == 0, "heap/errno regression: " + result);
                    Require(owner.owner_errno() == marker, "errno persists on calling thread");
                    owner.ReleaseThread();
                    Require(owner.owner_errno() == 0, "released errno slot is fresh");
                    owner.ReleaseThread();
                }
                catch (Exception error) { failures.Enqueue(error); }
            });
            workers[i].Start();
        }
        foreach (Thread worker in workers)
            Require(worker.Join(TimeSpan.FromSeconds(60)), "parallel allocation worker timed out");
        if (!failures.IsEmpty) throw new AggregateException(failures);
        Require(first.owner_errno() == 11 && second.owner_errno() == 22, "owner/thread errno aliases");
    }
    public static void Main()
    {
        using (var first = new ConcurrencyCore())
        using (var second = new ConcurrencyCore())
        {
            first.owner_set_errno(11);
            second.owner_set_errno(22);
            ParallelHeap(first, second, false);
            ParallelHeap(first, second, true);
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            Require(first.owner_errno() == 11 && second.owner_errno() == 22, "pinned errno storage moved");
        }
        using var closing = new ConcurrencyCore();
        var failures = new ConcurrentQueue<Exception>();
        var caller = new Thread(() =>
        {
            try { Require(closing.owner_wait(&Wait) == 17, "held callback result"); }
            catch (Exception error) { failures.Enqueue(error); }
        });
        using var shutdownStarted = new ManualResetEventSlim();
        var disposer = new Thread(() =>
        {
            try { shutdownStarted.Set(); closing.Shutdown(); }
            catch (Exception error) { failures.Enqueue(error); }
        });
        caller.Start();
        Require(entered.Wait(TimeSpan.FromSeconds(30)), "translated call did not enter");
        disposer.Start();
        Require(shutdownStarted.Wait(TimeSpan.FromSeconds(30)), "shutdown thread did not start");
        Require(!disposer.Join(100), "shutdown reclaimed storage during an active callback");
        release.Set();
        Require(caller.Join(TimeSpan.FromSeconds(30)), "held call did not finish");
        Require(disposer.Join(TimeSpan.FromSeconds(30)), "shutdown did not finish");
        if (!failures.IsEmpty) throw new AggregateException(failures);
        try { closing.owner_errno(); throw new InvalidOperationException("disposed owner accepted a call"); }
        catch (ObjectDisposedException) { }
        Console.WriteLine("PASS: parallel owner heaps, thread/owner errno, release and active-call shutdown");
    }
}
