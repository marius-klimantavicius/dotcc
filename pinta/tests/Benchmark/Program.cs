using System.Diagnostics;
using System.Globalization;
using Managed.Interpreters;

if (args.Length != 2) throw new ArgumentException("Expected receipt.pint path and iteration count.");
int count = int.Parse(args[1], CultureInfo.InvariantCulture);
if (count < 1 || count > 10000) throw new ArgumentOutOfRangeException(nameof(count));
var modules = new Dictionary<string, byte[]> { ["receipt.pint"] = File.ReadAllBytes(args[0]) };
byte[] expected = System.Text.Encoding.Unicode.GetBytes("Customer: Ada\nTotal: 37.5\n");
long[] elapsed = new long[6];
string[] phases = ["create", "load_and_globals", "execute", "collect_compact", "copy_output", "dispose"];
long allocationStart = 0;
int[] collections = new int[3];
// Warm up exactly the same workload before recording managed allocations/timings.
for (int iteration = -5; iteration < count; iteration++)
{
    if (iteration == 0)
    {
        Array.Clear(elapsed);
        allocationStart = GC.GetAllocatedBytesForCurrentThread();
        for (int generation = 0; generation < 3; generation++) collections[generation] = GC.CollectionCount(generation);
    }
    long start = Stopwatch.GetTimestamp();
    var engine = new PintaEngine(modules);
    long created = Stopwatch.GetTimestamp();
    try
    {
        var module = engine.LoadModule("receipt.pint");
        engine.SetString(module, "customer", "Ada");
        engine.SetInteger(module, "quantity", 3);
        engine.SetString(module, "unitPrice", "12.50");
        long loaded = Stopwatch.GetTimestamp();
        engine.Execute(module);
        long executed = Stopwatch.GetTimestamp();
        engine.Collect();
        long collected = Stopwatch.GetTimestamp();
        byte[] output = engine.CopyOutput();
        long copied = Stopwatch.GetTimestamp();
        if (!output.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Receipt bytes differ.");
        long disposeStart = Stopwatch.GetTimestamp();
        engine.Dispose();
        long disposed = Stopwatch.GetTimestamp();
        elapsed[0] += created - start;
        elapsed[1] += loaded - created;
        elapsed[2] += executed - loaded;
        elapsed[3] += collected - executed;
        elapsed[4] += copied - collected;
        elapsed[5] += disposed - disposeStart;
    }
    finally { engine.Dispose(); }
}
long allocated = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
Console.WriteLine($"iterations={count}");
for (int phase = 0; phase < phases.Length; phase++)
    Console.WriteLine($"{phases[phase]}_seconds={(elapsed[phase] / (double)Stopwatch.Frequency).ToString("R", CultureInfo.InvariantCulture)}");
Console.WriteLine($"managed_allocated_bytes={allocated}");
for (int generation = 0; generation < 3; generation++)
    Console.WriteLine($"clr_gc_gen{generation}={GC.CollectionCount(generation) - collections[generation]}");
Console.WriteLine($"peak_working_set_bytes={Process.GetCurrentProcess().PeakWorkingSet64}");
Console.WriteLine("arena_bytes=4194304");
Console.WriteLine("heap_bytes=1048576");
Console.WriteLine("stack_bytes=65536");
Console.WriteLine("output_bytes=52");
Console.WriteLine("validated=1");
