using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation;

internal static class Program
{
    private sealed record Observation(string Name, int Pid, int Port, string Executable, InstanceResult Result);
    private sealed record Exchange(string Name, int Bytes, string Sha256, int RequestBytes, int[] WriteEndOffsets);
    private static void Check(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }

    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 5) return 80;
        string dotnet = args[0], worker = args[1], elf = args[2], oracle = args[3], output = args[4];
        Directory.CreateDirectory(output);
        byte[] image = File.ReadAllBytes(elf);
        string imageSha = Convert.ToHexString(SHA256.HashData(image)).ToLowerInvariant();
        WorkerLaunch launch = worker.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            ? new(dotnet, [worker]) : new(worker, []);
        var observations = new List<Observation>();
        var exchanges = new List<Exchange>();
        var owners = new List<BlinkInstance>();
        var configured = new List<(string Name, string Executable, string MarkerSha)>();
        bool passed = false;
        string? failure = null;
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(150));
        InstanceOptions Options(string name, int wallMilliseconds = 60000)
        {
            string path = "/instance-" + name + "/service";
            byte[] marker = Encoding.UTF8.GetBytes("private-image-" + name + "\n");
            configured.Add((name, path, Convert.ToHexString(SHA256.HashData(marker)).ToLowerInvariant()));
            return new InstanceOptions
            {
                Executable = path, Image = [new(path, image, true), new("/instance.txt", marker)],
                Arguments = ["dotnet-service", "8080"],
                Environment = ["LANG=C", "DOTNET_GCHeapHardLimit=1000000", "DOTNET_GCRegionRange=2000000", "DOTNET_GCRegionSize=100000"],
                MemoryLimit = 64 * 1024 * 1024, DescriptorLimit = 128, OutputLimit = 16384,
                InstructionLimit = 20_000_000, WallClockMilliseconds = wallMilliseconds, PublishedPorts = [8080]
            };
        }
        async Task<int> Ready(BlinkInstance instance)
        {
            var endpoints = await instance.Ready.WaitAsync(deadline.Token);
            Check(endpoints.Length == 1 && endpoints[0].GuestPort == 8080, "real endpoint publication");
            return endpoints[0].HostPort;
        }
        async Task Request(int port, string name, string kind)
        {
            using var requestDeadline = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token);
            requestDeadline.CancelAfter(TimeSpan.FromSeconds(10));
            using var client = new TcpClient(AddressFamily.InterNetwork);
            await client.ConnectAsync(IPAddress.Loopback, port, requestDeadline.Token);
            using var stream = client.GetStream();
            byte[] request = File.ReadAllBytes(Path.Combine(oracle, kind + ".request"));
            int[] ends = kind == "fragmented" ? [1, 17, 59, 571, 1595, 3572, 3573] : [request.Length];
            int position = 0;
            foreach (int end in ends)
            {
                await stream.WriteAsync(request.AsMemory(position, end - position), requestDeadline.Token);
                position = end;
            }
            Check(position == request.Length, "complete reviewed write schedule");
            using var response = new MemoryStream();
            byte[] buffer = new byte[4096];
            while (true)
            {
                int count = await stream.ReadAsync(buffer, requestDeadline.Token);
                if (count == 0) break;
                Check(response.Length + count <= 8192, "response bound");
                response.Write(buffer, 0, count);
            }
            byte[] bytes = response.ToArray();
            File.WriteAllBytes(Path.Combine(output, name + ".response"), bytes);
            exchanges.Add(new(name, bytes.Length, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), request.Length, ends));
            Check(bytes.AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(oracle, kind + ".response"))), name + " native response equality");
        }
        void Record(string name, BlinkInstance instance, int port, string executable, InstanceResult result, string reason)
        {
            observations.Add(new(name, instance.WorkerProcessId, port, executable, result));
            File.WriteAllBytes(Path.Combine(output, name + ".guest.stdout"), result.StandardOutput);
            File.WriteAllBytes(Path.Combine(output, name + ".guest.stderr"), result.StandardError);
            Check(result.WorkerExitCode == 0 && result.WorkerDiagnostics.Length == 0, "normal worker process completion");
            Check(result.Reason == reason && result.ExitStatus == 0 && result.Halt == 0 && result.Signal == 0,
                "actual worker termination reason");
            Check(result.Instructions is > 0 and <= 20_000_000, "actual instruction accounting");
            Check(result.StandardOutput.AsSpan().SequenceEqual(reason != "exited" ? "READY 8080\n"u8 : "READY 8080\nSTOPPED\n"u8)
                && result.StandardError.Length == 0, "isolated guest output bytes");
            using var detail = JsonDocument.Parse(result.Detail ?? throw new InvalidDataException("Actual final detail missing"));
            var d = detail.RootElement;
            foreach (string field in new[] { "joined", "quiescent", "ioDisposed", "memoryReleased" })
                Check(d.GetProperty(field).GetBoolean(), field);
            Check(!d.GetProperty("notificationFailure").GetBoolean() && d.GetProperty("error").ValueKind == JsonValueKind.Null, "cleanup failure absent");
            Check(d.GetProperty("stopReason").GetString() == (reason == "stopped" ? "Requested" : reason == "deadline" ? "Deadline" : "None"), "actual stop outcome independent of controller override");
            Check(d.GetProperty("maximumWorkers").GetInt32() == 16 && d.GetProperty("unrecordedThreadObservations").GetUInt64() == 0, "bounded actual worker trace summary");
            var threads = d.GetProperty("threads").EnumerateArray().ToArray();
            Check(threads.Length is >= 2 and <= 16 && threads.All(t => t[6].GetBoolean() && t[4].GetInt32() == 0 && t[5].GetInt32() == 0), "actual Machines released");
            Check(threads.Select(t => t[0].GetInt32()).Distinct().Count() == threads.Length, "distinct virtual thread identities");
            Check(d.GetProperty("syscallSummary").GetArrayLength() != 0, "real syscall observations");
        }
        try
        {
            var optionsA = Options("a"); var optionsB = Options("b");
            var first = await BlinkInstance.StartAsync(launch, optionsA, deadline.Token); owners.Add(first);
            var second = await BlinkInstance.StartAsync(launch, optionsB, deadline.Token); owners.Add(second);
            int[] ports = await Task.WhenAll(Ready(first), Ready(second));
            Check(first.WorkerProcessId != second.WorkerProcessId && ports[0] != ports[1], "simultaneous independent worker processes and host listeners");
            await Request(ports[0], "a-health", "health"); await Request(ports[1], "b-health", "health");
            await Request(ports[0], "a-large", "large");
            await Request(ports[0], "a-fragmented", "fragmented");
            await Request(ports[0], "a-missing", "missing");
            await Request(ports[0], "a-stop", "stop");
            Record("a", first, ports[0], optionsA.Executable, await first.Completion.WaitAsync(deadline.Token), "exited");
            await Request(ports[1], "b-after-a-health", "health");
            Record("b", second, ports[1], optionsB.Executable, await second.StopAsync(TimeSpan.FromSeconds(5), deadline.Token), "stopped");
            var restart = await BlinkInstance.StartAsync(launch, optionsA, deadline.Token); owners.Add(restart);
            int restartedPort = await Ready(restart);
            Check(restart.WorkerProcessId != first.WorkerProcessId && restart.WorkerProcessId != second.WorkerProcessId,
                "restart uses a fresh process");
            await Request(restartedPort, "restart-health", "health"); await Request(restartedPort, "restart-stop", "stop");
            Record("restart", restart, restartedPort, optionsA.Executable, await restart.Completion.WaitAsync(deadline.Token), "exited");
            var deadlineOptions = Options("deadline", 15000);
            var idle = await BlinkInstance.StartAsync(launch, deadlineOptions, deadline.Token); owners.Add(idle);
            int idlePort = await Ready(idle);
            await Request(idlePort, "deadline-health", "health");
            // No pending client: the ordinary accept waits for the worker's
            // own monotonic deadline. Parent hard termination is not a pass.
            Record("deadline", idle, idlePort, deadlineOptions.Executable, await idle.Completion.WaitAsync(deadline.Token), "deadline");
            Check(exchanges.Count == 10 && observations.Count == 4, "exact normal scenario coverage");
            passed = true;
        }
        catch (Exception error) { failure = error.ToString(); }
        finally
        {
            foreach (var owner in owners)
                try { await owner.DisposeAsync(); }
                catch (Exception error) { passed = false; failure ??= error.ToString(); }
            using var file = File.Create(Path.Combine(output, "result.json"));
            using var writer = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject(); writer.WriteBoolean("passed", passed); writer.WriteString("error", failure);
            writer.WriteString("image_sha256", imageSha);
            writer.WriteStartArray("images");
            foreach (var item in configured)
            {
                writer.WriteStartObject(); writer.WriteString("name", item.Name); writer.WriteString("executable", item.Executable);
                writer.WriteString("marker_sha256", item.MarkerSha); writer.WriteBoolean("marker_read_by_guest", false); writer.WriteEndObject();
            }
            writer.WriteEndArray(); writer.WriteStartArray("exchanges");
            foreach (var item in exchanges)
            {
                writer.WriteStartObject(); writer.WriteString("name", item.Name); writer.WriteNumber("bytes", item.Bytes); writer.WriteString("sha256", item.Sha256); writer.WriteNumber("request_bytes", item.RequestBytes);
                writer.WriteStartArray("write_end_offsets"); foreach (int end in item.WriteEndOffsets) writer.WriteNumberValue(end); writer.WriteEndArray(); writer.WriteEndObject();
            }
            writer.WriteEndArray(); writer.WriteStartArray("workers");
            foreach (var item in observations)
            {
                writer.WriteStartObject(); writer.WriteString("name", item.Name); writer.WriteNumber("pid", item.Pid); writer.WriteNumber("port", item.Port);
                writer.WriteString("executable", item.Executable); writer.WriteString("reason", item.Result.Reason); writer.WriteNumber("exit_status", item.Result.ExitStatus);
                writer.WriteNumber("worker_exit_code", item.Result.WorkerExitCode); writer.WriteNumber("instructions", item.Result.Instructions);
                writer.WriteString("stdout_hex", Convert.ToHexString(item.Result.StandardOutput).ToLowerInvariant()); writer.WriteString("stderr_hex", Convert.ToHexString(item.Result.StandardError).ToLowerInvariant());
                writer.WriteString("worker_diagnostics", item.Result.WorkerDiagnostics); writer.WriteString("detail", item.Result.Detail); writer.WriteEndObject();
            }
            writer.WriteEndArray(); writer.WriteEndObject();
        }
        if (!passed) { Console.Error.WriteLine(failure); return 1; }
        Console.WriteLine("actual workers: two simultaneous isolated images; ten native HTTP comparisons; cooperative stop; fresh-process restart; idle deadline");
        return 0;
    }
}
