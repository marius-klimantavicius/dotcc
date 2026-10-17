using Application = Translated.Program;

internal static unsafe class ThreadChecks
{
    private static readonly int[][] before = new int[4][];
    private static readonly int[][] after = new int[4][];
    private static readonly nint[,] addresses = new nint[4, 6];
    private static readonly Barrier barrier = new(4);

    public static int Main()
    {
        var threads = Enumerable.Range(0, 3).Select(index => new Thread(() => Worker(index)) { IsBackground = true }).ToArray();
        foreach (var thread in threads) thread.Start();
        // First translated entry occurs on a worker. The type constructor cannot
        // supply the main thread or the other workers' TLS initial values.
        Wait();
        Capture(3);
        Collect();
        for (var thread = 0; thread < 4; ++thread)
        {
            Require(before[thread].SequenceEqual(new[] { 0, 7, 11, 13, 17, 29, 23, 31 }), "per-thread constant values");
            Require((nuint)addresses[thread, 3] % 64 == 0, "explicit thread alignment");
            for (var item = 0; item < 5; ++item)
            {
                Require(addresses[thread, item] != 0, "live thread address");
                for (var other = 0; other < thread; ++other)
                    Require(addresses[thread, item] != addresses[other, item], "distinct live thread identity");
            }
            Require(addresses[thread, 5] == addresses[0, 5] && *(int*)addresses[thread, 5] == 29, "shared process address fixup");
            CheckPhysical(thread, 0);
        }
        Wait();
        Wait();
        Application.thread_change(400);
        after[3] = Read();
        Collect();
        for (var thread = 0; thread < 4; ++thread)
        {
            var salt = (thread + 1) * 100;
            Require(after[thread].SequenceEqual(new[] { salt, 7 + salt, 11, 13 + salt, 17, 29, 23 + salt, 31 + salt }), "independent thread mutation");
            CheckPhysical(thread, salt);
        }
        CheckAddresses(3);
        Wait();
        foreach (var thread in threads) thread.Join();
        for (var thread = 0; thread < 4; ++thread)
        {
            Console.WriteLine($"thread{thread}-before={string.Join(',', before[thread])}");
            Console.WriteLine($"thread{thread}-after={string.Join(',', after[thread])}");
        }
        Console.WriteLine("thread-addresses=stable-aligned-distinct-shared-process");
        return 0;
    }

    private static void Worker(int index)
    {
        Capture(index);
        Wait();
        Wait();
        Application.thread_change((index + 1) * 100);
        after[index] = Read();
        CheckAddresses(index);
        Wait();
        // Keep the owning thread alive while main reads its pointer and collects.
        Wait();
    }

    private static int[] Read() => Enumerable.Range(0, 8).Select(Application.thread_observe).ToArray();
    private static void Capture(int thread)
    {
        before[thread] = Read();
        for (var item = 0; item < 6; ++item) addresses[thread, item] = (nint)Application.thread_address(item);
    }
    private static void CheckAddresses(int thread)
    {
        for (var item = 0; item < 6; ++item)
            Require(addresses[thread, item] == (nint)Application.thread_address(item), "pinned thread owner stability");
    }
    private static void CheckPhysical(int thread, int salt)
    {
        Require(*(int*)addresses[thread, 0] == salt && *(int*)addresses[thread, 1] == 7 + salt &&
            ((int*)addresses[thread, 2])[0] == 11 && ((int*)addresses[thread, 2])[1] == 13 + salt &&
            ((int*)addresses[thread, 2])[2] == 17 && *(int*)addresses[thread, 3] == 23 + salt &&
            *(int*)addresses[thread, 4] == 31 + salt, "cross-thread physical storage observations");
    }
    private static void Collect() { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
    private static void Wait()
    {
        if (!barrier.SignalAndWait(TimeSpan.FromSeconds(30))) throw new TimeoutException("Thread owner barrier timed out.");
    }
    private static void Require(bool condition, string contract)
    {
        if (!condition) throw new InvalidOperationException("Thread storage check failed: " + contract);
    }
}
