using System.Diagnostics;
using Managed.Emulation.Host;

static void Require(bool condition) { if (!condition) throw new Exception("Stop-state contract differs"); }
using (var cancellation = new CancellationTokenSource())
using (var stop = new HostExecutionStop(cancellation: cancellation.Token))
{
    Require(stop.Reason == HostExecutionStopReason.None && !stop.Token.IsCancellationRequested);
    using var readStarted = new ManualResetEventSlim();
    var reader = Task.Run(() =>
    {
        readStarted.Set();
        var deadline = Stopwatch.StartNew();
        while (stop.Reason == HostExecutionStopReason.None)
            if (deadline.Elapsed > TimeSpan.FromSeconds(5)) throw new TimeoutException("Cancellation not observed");
        Require(stop.Reason == HostExecutionStopReason.Requested);
    });
    Require(readStarted.Wait(TimeSpan.FromSeconds(5)));
    cancellation.Cancel();
    reader.GetAwaiter().GetResult();
    Require(stop.Token.IsCancellationRequested && !stop.RequestBudgetStop());
    Require(stop.Reason == HostExecutionStopReason.Requested && stop.NotificationFailure == null);
}
using (var stop = new HostExecutionStop())
{
    Require(stop.RequestBudgetStop() && !stop.RequestStop());
    Require(stop.Reason == HostExecutionStopReason.Budget && stop.Token.IsCancellationRequested);
}
using (var cancellation = new CancellationTokenSource())
{
    cancellation.Cancel();
    using var stop = new HostExecutionStop(cancellation: cancellation.Token);
    Require(stop.Reason == HostExecutionStopReason.Requested);
}
using (var stop = new HostExecutionStop(TimeSpan.Zero))
{
    Require(stop.Reason == HostExecutionStopReason.Deadline && !stop.RequestStop());
}
using (var stop = new HostExecutionStop(TimeSpan.FromMilliseconds(20)))
{
    Require(stop.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)));
    Require(stop.Reason == HostExecutionStopReason.Deadline && !stop.RequestBudgetStop());
}
Console.WriteLine("stop cancellation/budget/deadline/monotonicity=pass");
if (args is ["--measure"])
{
    using var stop = new HostExecutionStop();
    const int calls = 10_000_000;
    for (int trial = 0; trial < 7; ++trial)
    {
        using var process = Process.GetCurrentProcess();
        var cpu = process.TotalProcessorTime;
        var time = Stopwatch.StartNew();
        int sum = 0;
        for (int i = 0; i < calls; ++i) sum += (int)stop.Reason;
        time.Stop(); process.Refresh(); Require(sum == 0);
        Console.WriteLine(FormattableString.Invariant($"{trial},{calls},{time.Elapsed.TotalMilliseconds:F6},{(process.TotalProcessorTime - cpu).TotalMilliseconds:F6}"));
    }
}
