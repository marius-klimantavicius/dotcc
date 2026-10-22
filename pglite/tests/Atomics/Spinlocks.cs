using System.Runtime.InteropServices;
using System.Text.Json;
using PG = Managed.PGlite.Probes.Spinlocks;

internal static unsafe class SpinlocksProgram
{
    private static ulong Width<T>(ref T value) where T : unmanaged => (ulong)sizeof(T);
    private static void Require(bool condition, string operation)
    {
        if (!condition) throw new InvalidOperationException(operation);
    }

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && args[0] == "--stuck") { PG.dpgs_stuck(); return 20; }
            Require(args.Length == 2, "Expected NATIVE_JSON MANAGED_JSON");
            using var native = JsonDocument.Parse(File.ReadAllText(args[0]));
            ulong[] layout = Enumerable.Range(0, 8).Select(i => PG.dpgs_layout((uint)i)).ToArray();
            ulong[] expected = native.RootElement.GetProperty("layout").EnumerateArray().Select(x => x.GetUInt64()).ToArray();
            Require(layout.SequenceEqual(expected), "actual native PostgreSQL / emitted C11 slock_t layout");
            PG.dpgs_state sample = default;
            var observed = new Dictionary<int, ulong>
            {
                [0] = Width(ref sample.@lock),
                [2] = (ulong)sizeof(PG.dpgs_state),
                [4] = (ulong)((byte*)&sample.@lock - (byte*)&sample),
                [5] = (ulong)((byte*)&sample.after - (byte*)&sample),
                [6] = (ulong)((byte*)&sample.value - (byte*)&sample),
                [7] = (ulong)((byte*)&sample.phase - (byte*)&sample),
            };
            foreach (var field in observed)
                Require(field.Value == expected[field.Key], $"actual CLR spinlock layout field {field.Key}");
            int status = PG.dpgs_check();
            Require(status == 0, $"emitted spinlock/delay C assertion at pg_spinlocks.c:{status}");
            uint[] backoff = Enumerable.Range(0, 5).Select(i => PG.dpgs_backoff_value((uint)i)).ToArray();
            Require(backoff.SequenceEqual(native.RootElement.GetProperty("backoff").EnumerateArray().Select(x => x.GetUInt32())),
                    "actual upstream seeded backoff agrees with both native ports");
            void* storage = NativeMemory.AlignedAlloc((nuint)layout[2], (nuint)layout[3]);
            if (storage == null) throw new OutOfMemoryException();
            try
            {
                PG.dpgs_init(storage);
                nint address = (nint)storage;
                using var ready = new CountdownEvent(4);
                using var start = new ManualResetEventSlim();
                var workers = Enumerable.Range(0, 4).Select(_ => new Thread(() =>
                {
                    ready.Signal();
                    start.Wait();
                    PG.dpgs_tas_worker((void*)address, 5000);
                })).ToArray();
                foreach (var worker in workers) worker.Start();
                ready.Wait();
                start.Set();
                foreach (var worker in workers) worker.Join();
                Require(PG.dpgs_finish(storage, 20000) == 0, "20,000 TAS-protected increments and byte canaries");
                PG.dpgs_init(storage);
                int waiterStatus = -1;
                var waiter = new Thread(() => waiterStatus = PG.dpgs_waiter((void*)address, 64));
                waiter.Start();
                for (uint i = 1; i <= 64; ++i)
                {
                    PG.dpgs_holder_begin(storage, i);
                    while (PG.dpgs_sleeping() == 0) Thread.Yield();
                    status = PG.dpgs_holder_end(storage, i);
                    Require(status == 0, $"real upstream S_LOCK holder assertion at pg_spinlocks.c:{status}");
                }
                waiter.Join();
                Require(waiterStatus == 0, $"real upstream S_LOCK waiter assertion at pg_spinlocks.c:{waiterStatus}");
                Require(PG.dpgs_finish(storage, 64) == 0, "64 forced upstream S_LOCK sleep/acquire/release rounds");
            }
            finally { NativeMemory.AlignedFree(storage); }
            File.WriteAllText(args[1], JsonSerializer.Serialize(new
                { layout, backoff, behavior = true, workers = 4, increments = 20000, delayed_rounds = 64 }));
            Console.WriteLine("\nPASS actual emitted PostgreSQL one-byte TAS and upstream S_LOCK: layout, 20,000 increments, seeded delay, 64 forced contentions");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
