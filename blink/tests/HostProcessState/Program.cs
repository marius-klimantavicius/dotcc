using System;
using System.Threading;
using Managed.Emulation;

static void Check(int result) {
    if (result != 0) throw new InvalidOperationException($"Process-state check failed: {result}");
}
Check(Blink.ProcessStateBegin());
int firstResult = -1, secondResult = -1;
Exception? failure = null;
var first = new Thread(() => {
    try { firstResult = Blink.ProcessStateWorker(0); }
    catch (Exception error) { Interlocked.CompareExchange(ref failure, error, null); }
}) { IsBackground = true };
var second = new Thread(() => {
    try { secondResult = Blink.ProcessStateWorker(1); }
    catch (Exception error) { Interlocked.CompareExchange(ref failure, error, null); }
}) { IsBackground = true };
first.Start(); second.Start();
if (!first.Join(TimeSpan.FromSeconds(10)) || !second.Join(TimeSpan.FromSeconds(10)))
    throw new TimeoutException("Process-state workers did not join; preserve registry until process discard.");
if (failure != null) throw failure;
Check(firstResult); Check(secondResult);
GC.Collect(2, GCCollectionMode.Forced, true, true);
Check(Blink.ProcessStateFinish());
Console.WriteLine("shared disposition; callbacks BCA once; clean end");
