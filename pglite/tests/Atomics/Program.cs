using System.Runtime.InteropServices;
using System.Text.Json;
using PG = Managed.PGlite.Probes.Atomics;

internal static unsafe class Program
{
    private static void Require(bool condition, string operation)
    {
        if (!condition) throw new InvalidOperationException(operation);
    }

    public static int Main(string[] args)
    {
        try
        {
            Require(args.Length == 2, "Expected NATIVE_JSON MANAGED_JSON");
            using var native = JsonDocument.Parse(File.ReadAllText(args[0]));
            ulong[] layout = Enumerable.Range(0, 16).Select(i => PG.dpga_layout((uint)i)).ToArray();
            ulong[] expected = native.RootElement.GetProperty("layout").EnumerateArray().Select(x => x.GetUInt64()).ToArray();
            Require(layout.SequenceEqual(expected), "actual native PostgreSQL / emitted C11 port layout");
            // Measure generated CLR structs independently of the emitter's
            // offsetof metadata returned by dpga_layout.
            PG.pg_atomic_uint32 a = default;
            PG.pg_atomic_uint64 b = default;
            PG.pg_atomic_flag f = default;
            PG.dpga_state sample = default;
            var observed = new Dictionary<int, ulong>
            {
                [0] = (ulong)sizeof(PG.pg_atomic_uint32),
                [2] = (ulong)((byte*)&a.value - (byte*)&a),
                [3] = (ulong)sizeof(PG.pg_atomic_uint64),
                [5] = (ulong)((byte*)&b.value - (byte*)&b),
                [6] = (ulong)sizeof(PG.pg_atomic_flag),
                [8] = (ulong)((byte*)&f.value - (byte*)&f),
                [9] = (ulong)sizeof(PG.dpga_state),
                [11] = (ulong)((byte*)&sample.u32 - (byte*)&sample),
                [12] = (ulong)((byte*)&sample.u64 - (byte*)&sample),
                [13] = (ulong)((byte*)&sample.flag - (byte*)&sample),
                [14] = (ulong)((byte*)&sample.after - (byte*)&sample),
                [15] = (ulong)((byte*)&sample.protected_value - (byte*)&sample),
            };
            foreach (var field in observed)
                Require(field.Value == expected[field.Key], $"actual CLR layout field {field.Key}");
            int status = PG.dpga_check();
            Require(status == 0, $"emitted C behavior assertion at pg_atomics.c:{status}");
            void* storage = NativeMemory.AlignedAlloc((nuint)layout[9], (nuint)layout[10]);
            if (storage == null) throw new OutOfMemoryException();
            try
            {
                PG.dpga_init(storage);
                nint address = (nint)storage;
                var threads = new Thread[4];
                using var ready = new CountdownEvent(4);
                using var start = new ManualResetEventSlim();
                for (int i = 0; i < threads.Length; ++i)
                {
                    threads[i] = new Thread(() =>
                    {
                        ready.Signal();
                        start.Wait();
                        PG.dpga_worker((void*)address, 5000);
                    });
                    threads[i].Start();
                }
                ready.Wait();
                start.Set();
                foreach (var thread in threads) thread.Join();
                status = PG.dpga_finish(storage, 20000);
                Require(status == 0, $"emitted C concurrent assertion at pg_atomics.c:{status}");
                var publisher = new Thread(() => PG.dpga_publish((void*)address, 2000));
                publisher.Start();
                status = PG.dpga_consume(storage, 2000);
                publisher.Join();
                Require(status == 0, $"emitted C fence/payload assertion at pg_atomics.c:{status}");
            }
            finally { NativeMemory.AlignedFree(storage); }
            var report = new { layout, behavior = true, workers = 4, increments = 20000, messages = 2000 };
            File.WriteAllText(args[1], JsonSerializer.Serialize(report));
            Console.WriteLine("PASS actual emitted PostgreSQL wrappers: numeric/flag behavior, layout, four workers, 2000 fence handshakes");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
