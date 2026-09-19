using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation.Host;
using Managed.Emulation.Execution;

internal static class Program
{
    private const ulong InstructionLimit = 100_000_000;
    private static readonly string[] Cases = { "health", "file", "fragmented", "large", "missing", "stop" };
    private sealed record Execution(int Result, ulong Instructions, ulong Ip, int Halt,
        int Signal, int SignalCode, int Exited, int ExitStatus, int StopReason,
        ulong RetainedBytesBeforeRelease, ulong RetainedMappingsBeforeRelease, bool MemoryReleased, string? Error);

    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 4) return 80;
        string output = Path.GetFullPath(args[3]);
        Directory.CreateDirectory(output);
        var report = new Dictionary<string, object?> { ["passed"] = false, ["cases"] = new List<object>() };
        var completedCases = (List<object>)report["cases"]!;
        InstanceIo? io = null;
        Thread? thread = null;
        Task<Execution>? execution = null;
        var stop = new HostExecutionStop(TimeSpan.FromSeconds(120));
        bool executionJoined = false;
        try
        {
            byte[] service = File.ReadAllBytes(args[0]);
            if (Hash(service) != "916eeadfde4092c55bd2d05d5cacbe20482f4cabda8c0436fcfb68d690f2a9b8")
                throw new InvalidOperationException("Pinned service hash differs");
            byte[] fixture = File.ReadAllBytes(args[1]);
            io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> {
                ["/bin/service"] = service, ["/data/fixture"] = fixture },
                executablePaths: new HashSet<string> { "/bin/service" }, outputLimit: 4096);
            var done = new TaskCompletionSource<Execution>(TaskCreationOptions.RunContinuationsAsynchronously);
            execution = done.Task;
            InstanceIo ownedIo = io;
            thread = new Thread(() => done.SetResult(Execute(ownedIo, stop))) { IsBackground = true, Name = "translated-service" };
            thread.Start();
            byte[] ready = Encoding.ASCII.GetBytes("READY 8080\n");
            while (true)
            {
                stop.Token.ThrowIfCancellationRequested();
                var captured = io.CapturedOutput;
                if (captured.StandardOutput.Length != 0)
                {
                    if (!ready.AsSpan().StartsWith(captured.StandardOutput) && !captured.StandardOutput.AsSpan().SequenceEqual(ready))
                        throw new InvalidOperationException("Unexpected readiness bytes");
                    if (captured.StandardOutput.AsSpan().SequenceEqual(ready)) break;
                }
                if (execution.IsCompleted) throw new InvalidOperationException("Guest completed before readiness: " + JsonSerializer.Serialize(await execution));
                await Task.Delay(10, stop.Token);
            }
            IPEndPoint? endpoint = null;
            int listener = -1;
            for (int fd = 0; fd < 128; ++fd)
            {
                var local = io.LocalEndpoint(fd);
                if (!local.Succeeded || local.Value.Port != 8080) continue;
                var published = io.Publish(fd);
                if (!published.Succeeded) continue;
                if (endpoint != null) throw new InvalidOperationException("Multiple listening descriptors for the selected guest port");
                endpoint = published.Value; listener = fd;
            }
            if (endpoint == null || !IPAddress.IsLoopback(endpoint.Address) || endpoint.Port < 1)
                throw new InvalidOperationException("No actual loopback listener to publish");
            report["endpoint"] = new { GuestPort = 8080, HostAddress = endpoint.Address.ToString(), HostPort = endpoint.Port, Listener = listener };

            foreach (string name in Cases)
            {
                byte[] request = File.ReadAllBytes(Path.Combine(args[2], name + ".request"));
                byte[] expected = File.ReadAllBytes(Path.Combine(args[2], name + ".response"));
                using var client = new TcpClient(AddressFamily.InterNetwork);
                await client.ConnectAsync(endpoint.Address, endpoint.Port, stop.Token);
                using NetworkStream stream = client.GetStream();
                if (name == "fragmented")
                {
                    for (int offset = 0; offset < request.Length; offset += 3)
                    {
                        await stream.WriteAsync(request.AsMemory(offset, Math.Min(3, request.Length - offset)), stop.Token);
                        await Task.Delay(2, stop.Token);
                    }
                }
                else await stream.WriteAsync(request, stop.Token);
                using var response = new MemoryStream();
                byte[] buffer = new byte[4096];
                while (true)
                {
                    int count = await stream.ReadAsync(buffer, stop.Token);
                    if (count == 0) break;
                    if (response.Length + count > 262144) throw new InvalidOperationException("Response limit exceeded");
                    response.Write(buffer, 0, count);
                }
                byte[] actual = response.ToArray();
                File.WriteAllBytes(Path.Combine(output, name + ".request"), request);
                File.WriteAllBytes(Path.Combine(output, name + ".response"), actual);
                bool passed = actual.AsSpan().SequenceEqual(expected);
                completedCases.Add(new { Name = name, Passed = passed, RequestSha256 = Hash(request), ResponseSha256 = Hash(actual), ResponseBytes = actual.Length });
                if (!passed) throw new InvalidOperationException(name + ": exact native wire response mismatch");
            }
            Execution result = await execution.WaitAsync(stop.Token);
            report["execution"] = result;
            if (result.Error != null || result.Result != 0 || result.Exited != 1 || result.ExitStatus != 0 ||
                result.Signal != 0 || result.Halt != -10 || result.StopReason != 0 || result.Instructions > InstructionLimit ||
                result.RetainedBytesBeforeRelease > 64 * 1024 * 1024 || !result.MemoryReleased || stop.NotificationFailure != null)
                throw new InvalidOperationException("Guest did not finish with a clean ordinary exit");
            var finalOutput = io.CapturedOutput;
            if (!finalOutput.StandardOutput.AsSpan().SequenceEqual(Encoding.ASCII.GetBytes("READY 8080\nSTOPPED\n")) || finalOutput.StandardError.Length != 0)
                throw new InvalidOperationException("Guest stdout/stderr differs");
            report["passed"] = true;
        }
        catch (Exception error) { report["error"] = error.ToString(); }
        finally
        {
            if (execution != null && !execution.IsCompleted) stop.RequestStop();
            if (thread != null && !thread.Join(TimeSpan.FromSeconds(5)))
            {
                // A stuck worker is discarded by process exit. Never race C
                // pointers or tear down host owners while translated code runs.
                report["passed"] = false;
                report["cleanup_error"] = "Owning execution thread failed to stop within five seconds; process discard required";
            }
            else
            {
                executionJoined = true;
                if (execution != null && execution.IsCompletedSuccessfully) report["execution"] = execution.Result;
                if (io != null) await io.DisposeAsync();
            }
            try
            {
                if (io != null)
                {
                    var captured = io.CapturedOutput;
                    File.WriteAllBytes(Path.Combine(output, "stdout.txt"), captured.StandardOutput);
                    File.WriteAllBytes(Path.Combine(output, "stderr.txt"), captured.StandardError);
                }
                report["owner_stop_reason"] = stop.Reason.ToString();
                if (stop.NotificationFailure != null)
                {
                    report["passed"] = false;
                    report["stop_notification_error"] = stop.NotificationFailure.ToString();
                }
                File.WriteAllText(Path.Combine(output, "result.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            }
            finally
            {
                // A stuck interpreter still borrows this owner. Preserve it
                // together with IO until the failed process is discarded.
                if (executionJoined) stop.Dispose();
            }
        }
        return report["passed"] is true ? 0 : 1;
    }

    private static Execution Execute(InstanceIo io, HostExecutionStop stop)
    {
        try
        {
            var owner = new GuestExecution(io, stop);
            var value = owner.Run("/bin/service", new[] { "service", "8080", "/data/fixture" },
                new[] { "LANG=C" }, InstructionLimit);
            return new(0, value.Instructions, value.InstructionPointer, value.Halt,
                value.Signal, value.SignalCode, value.Exited ? 1 : 0, value.ExitStatus,
                (int)value.StopReason, value.RetainedBytesBeforeRelease, value.RetainedMappingsBeforeRelease, value.MemoryReleased, null);
        }
        catch (Exception error) { return new(-1, 0, 0, 0, 0, 0, 0, 0, (int)stop.Reason, ulong.MaxValue, ulong.MaxValue, false, error.ToString()); }
    }
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
