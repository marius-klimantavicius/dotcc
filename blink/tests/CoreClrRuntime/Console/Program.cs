using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

if (args.Length == 1 && args[0] == "smoke")
{
    Console.WriteLine("CORECLR IL OK");
    return 0;
}
if (args.Length != 2 || args[0] != "workload") return 64;
if (RuntimeInformation.ProcessArchitecture != Architecture.X64) return 65;
string work = args[1];
Directory.CreateDirectory(work);
long checksum = 0;
for (int i = 0; i < 128; ++i) checksum += Work.JitAndAllocate(i);
GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
GC.WaitForPendingFinalizers();
if (checksum != 2_757_056) throw new InvalidOperationException("JIT/allocation checksum");

var gate = new object();
int arrived = 0, sum = 0;
bool release = false;
var threads = Enumerable.Range(1, 2).Select(value => new Thread(() =>
{
    lock (gate)
    {
        ++arrived;
        Monitor.PulseAll(gate);
        while (!release) Monitor.Wait(gate);
        sum += value;
    }
})).ToArray();
foreach (var thread in threads) thread.Start();
lock (gate)
{
    while (arrived != 2) Monitor.Wait(gate);
    release = true;
    Monitor.PulseAll(gate);
}
foreach (var thread in threads) thread.Join();
using var semaphore = new SemaphoreSlim(1, 1);
Task.WaitAll(Enumerable.Range(1, 4).Select(value => Task.Run(async () =>
{
    await semaphore.WaitAsync();
    try { sum += value; }
    finally { semaphore.Release(); }
})).ToArray());
if (sum != 13) throw new InvalidOperationException("Thread/pool synchronization");

string input = Path.Combine(work, "input.txt"), temporary = Path.Combine(work, "temporary.txt"),
    output = Path.Combine(work, "output.txt");
if (File.ReadAllText(input) != "guest-input\n") throw new InvalidOperationException("Guest input");
File.WriteAllText(temporary, "guest-output\n");
using (var stream = new FileStream(temporary, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
{
    stream.Seek(0, SeekOrigin.End);
    stream.Write("append\n"u8);
    stream.Flush(flushToDisk: true);
}
File.Move(temporary, output, overwrite: true);
if (File.ReadAllText(output) != "guest-output\nappend\n" ||
    !Directory.EnumerateFiles(work).Contains(output)) throw new InvalidOperationException("Guest file round trip");
File.WriteAllText(temporary, "delete");
File.Delete(temporary);
if (File.Exists(temporary)) throw new InvalidOperationException("Guest delete");
Console.WriteLine("CORECLR WORKLOAD OK jit=2757056 threads=13 file=20");
return 0;

internal static class Work
{
    // This authored method has IL only; the ordinary runtime must JIT it.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long JitAndAllocate(int seed)
    {
        var bytes = new byte[16 * 1024];
        long sum = 0;
        for (int i = 0; i < bytes.Length; i += 97) { bytes[i] = (byte)(seed + i); sum += bytes[i]; }
        GC.KeepAlive(bytes);
        return sum;
    }
}
