using System.Buffers.Binary;
using System.Diagnostics;
using Managed.Emulation;

if (args.Length == 2 && args[0] == "--fixture")
{
    if (args[1] == "hold") { await Task.Delay(10000); return 0; }
    Stream input = Console.OpenStandardInput(), output = Console.OpenStandardOutput();
    var start = await InstanceProtocol.ReadAsync(input, InstanceJson.Default.WorkerRequest);
    if (start?.Kind != "start" || start.Options is null) return 18;
    start.Options.Validate();
    string mode = args[1];
    if (mode == "crash") return 17;
    if (mode == "oversized")
    {
        byte[] length = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(length, InstanceProtocol.MaximumFrame + 1);
        await output.WriteAsync(length); await output.FlushAsync(); await Task.Delay(10000); return 19;
    }
    if (mode == "mutation") await Task.Delay(150);
    await InstanceProtocol.WriteAsync(output, new WorkerEvent { Kind = "ready", Endpoints = start.Options.PublishedPorts.Select(port => new PublishedEndpoint(port, 12345)).ToArray() }, InstanceJson.Default.WorkerEvent);
    if (mode == "inherited")
    {
        string executable = Environment.ProcessPath!;
        var childStart = new ProcessStartInfo(executable) { UseShellExecute = false };
        if (Path.GetFileNameWithoutExtension(executable) == "dotnet") childStart.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "InstanceLifecycle.dll"));
        childStart.ArgumentList.Add("--fixture"); childStart.ArgumentList.Add("hold");
        using var child = Process.Start(childStart)!;
        await InstanceProtocol.WriteAsync(output, new WorkerEvent { Kind = "final", Reason = "guest-exit", ExitStatus = child.Id }, InstanceJson.Default.WorkerEvent);
        return 0;
    }
    if (mode == "diagnostics") { Console.Error.Write(new string('x', 100000)); await Task.Delay(10000); return 20; }
    if (mode == "ignore") { Console.Error.Write("before-stop"); await Task.Delay(10000); return 21; }
    if (mode is not ("exit" or "mutation"))
    {
        var stop = await InstanceProtocol.ReadAsync(input, InstanceJson.Default.WorkerRequest);
        if (stop?.Kind != "stop") return 22;
    }
    await InstanceProtocol.WriteAsync(output, new WorkerEvent { Kind = "final", Reason = mode == "exit" ? "guest-exit" : "stopped", ExitStatus = mode == "exit" ? 42 : 0,
        StandardOutput = "bounded guest output"u8.ToArray() }, InstanceJson.Default.WorkerEvent);
    return 0;
}
static void Check(bool good, string text) { if (!good) throw new Exception(text); }
WorkerLaunch Launch(string mode)
{
    string executable = Environment.ProcessPath!;
    string[] prefix = Path.GetFileNameWithoutExtension(executable) == "dotnet" ? [Path.Combine(AppContext.BaseDirectory, "InstanceLifecycle.dll")] : [];
    return new(executable, [..prefix, "--fixture", mode]);
}
InstanceOptions Options(int deadline = 5000) => new()
{
    Executable = "/fixture", Image = [new("/fixture", [1], true)], WallClockMilliseconds = deadline
};
async Task Test(string mode, string expected, bool stop = false, int deadline = 5000)
{
    long began = Stopwatch.GetTimestamp();
    await using var instance = await BlinkInstance.StartAsync(Launch(mode), Options(deadline));
    if (stop) await instance.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    InstanceResult result = stop ? await instance.StopAsync(TimeSpan.FromMilliseconds(200)) : await instance.Completion.WaitAsync(TimeSpan.FromSeconds(7));
    Check(result.Reason == expected, mode + " reason " + result.Reason);
    if (mode == "exit") Check(result.ExitStatus == 42 && result.StandardOutput.AsSpan().SequenceEqual("bounded guest output"u8), "exit capture");
    if (mode == "crash") Check(result.WorkerExitCode == 17, "crash code");
    Check(result.WorkerDiagnostics.Length <= 65536, "diagnostic bound");
    if (mode == "diagnostics") Check(result.WorkerDiagnostics == new string('x',65536), "retained flood prefix");
    if (mode == "ignore") Check(result.WorkerDiagnostics == "before-stop", "retained stop diagnostics");
    if (stop || deadline == 200) Check(Stopwatch.GetElapsedTime(began) < TimeSpan.FromSeconds(3), "bounded stop/deadline");
    Console.WriteLine(mode + " " + result.Reason);
}
await Test("exit", "guest-exit");
await Test("normal", "stopped", true);
await Test("ignore", "stopped", true);
await Test("ignore", "deadline", deadline: 200);
await Test("crash", "worker-failure");
await Test("oversized", "worker-failure");
await Test("diagnostics", "worker-output-limit");
// An exited root's descendant keeps both pipes open. Controller completion must
// still be bounded; the test owns and explicitly reaps this deliberate helper.
long inheritedStart = Stopwatch.GetTimestamp();
await using (var inherited = await BlinkInstance.StartAsync(Launch("inherited"), Options()))
{
    var result = await inherited.Completion.WaitAsync(TimeSpan.FromSeconds(3));
    try
    {
        Check(result.Reason == "worker-failure" && result.ExitStatus > 0, "inherited pipe result");
        Check(Stopwatch.GetElapsedTime(inheritedStart) < TimeSpan.FromSeconds(2), "inherited drain bound");
    }
    finally { if (result.ExitStatus > 0) { using var child = Process.GetProcessById(result.ExitStatus); child.Kill(); await child.WaitForExitAsync(); } }
}
var mutable = Options() with { PublishedPorts = [8080] };
await using (var snapshot = await BlinkInstance.StartAsync(Launch("mutation"), mutable))
{
    mutable.PublishedPorts[0] = 9999;
    mutable.Image[0].Contents[0] = 9;
    var endpoints = await snapshot.Ready;
    Check(endpoints.Single().GuestPort == 8080, "frozen publication options");
    Check((await snapshot.Completion).Reason != "worker-failure", "frozen event validation");
}
// Structurally valid options that exceed the serialized frame budget are
// rejected before even attempting to launch this nonexistent executable.
var large = Options() with
{
    Image = [new("/fixture", new byte[1048576], true)],
    Arguments = Enumerable.Repeat(new string('a',4096),256).ToArray(),
    Environment = Enumerable.Repeat("A=" + new string('b',4094),256).ToArray()
};
large.Validate();
try { await BlinkInstance.StartAsync(new("/nonexistent-worker-preflight", []), large); throw new Exception("oversized start accepted"); }
catch (InvalidDataException) { }
Console.WriteLine("inherited drains, immutable configuration and frame preflight passed");
// Independent processes, simultaneous ownership and asynchronous disposal.
await using (var first = await BlinkInstance.StartAsync(Launch("normal"), Options()))
await using (var second = await BlinkInstance.StartAsync(Launch("normal"), Options()))
{
    await Task.WhenAll(first.Ready, second.Ready);
    Check(first.WorkerProcessId != second.WorkerProcessId, "one process per owner");
    await Task.WhenAll(first.StopAsync(TimeSpan.FromSeconds(1)), second.StopAsync(TimeSpan.FromSeconds(1)));
}
Console.WriteLine("instance lifecycle passed");
return 0;
