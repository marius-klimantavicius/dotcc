using System.Text;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

if (args.Length != 1) return 80;
byte[] elf = File.ReadAllBytes(args[0]);
for (int iteration = 0; iteration < 64; ++iteration)
{
    var image = new Dictionary<string, ReadOnlyMemory<byte>> { ["/probe"] = elf };
    using var files = new VirtualFileSystem(image, executablePaths: new HashSet<string> { "/probe" });
    await using var io = new InstanceIo(files);
    using var stop = new HostExecutionStop(TimeSpan.FromSeconds(10));
    var owner = new ThreadedGuestExecution(io, stop, 64UL << 20);
    ThreadedGuestExecutionResult? result = null;
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        try { result = owner.Run("/probe", ["/probe"], [], 100_000); }
        catch (Exception error) { failure = error; }
    });
    thread.Start(); thread.Join();
    if (failure != null) throw failure;
    if (result is not { Exited: true, ExitStatus: 0, StopReason: HostExecutionStopReason.None,
        AllWorkersJoined: true, MemoryReleased: true } || !owner.IsQuiescent || result.Threads.Count != 2 ||
        result.Threads.Any(worker => !worker.MachineReleased) ||
        Encoding.ASCII.GetString(io.CapturedOutput.StandardOutput) != "clone-publication PASS\n" ||
        io.CapturedOutput.StandardError.Length != 0)
        throw new InvalidOperationException($"Clone publication/clear-TID iteration {iteration} failed: {result}");
}
Console.WriteLine("PASS 64 clone publications, immediate child exits, clear-TID and cleanup");
return 0;
