using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

internal static class Program
{
    private const ulong Budget = 100_000_000;

    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 3) return 80;
        string imageDirectory = Path.GetFullPath(args[0]);
        string oracleDirectory = Path.GetFullPath(args[1]);
        string output = Path.GetFullPath(args[2]);
        Directory.CreateDirectory(output);
        using var configuration = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(imageDirectory, "configuration.json")));
        var configured = configuration.RootElement;
        string mode = configured.GetProperty("mode").GetString()!;
        bool allowInterpreter = configured.GetProperty("allow_interpreter").GetBoolean();
        string imagePath = configured.GetProperty("path").GetString()!;
        string[] arguments = configured.GetProperty("argv").EnumerateArray().Select(item => item.GetString()!).ToArray();
        string[] environment = configured.GetProperty("environment").EnumerateArray().Select(item => item.GetString()!).ToArray();
        string[] expectedEnvironment = ["LANG=C", "DOTNET_GCHeapHardLimit=1000000",
            "DOTNET_GCRegionRange=2000000", "DOTNET_GCRegionSize=100000",
            "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false", "DOTNET_EnableDiagnostics=0"];
        if (mode != "static" || allowInterpreter ||
            imagePath != "/bin/kestrel-service" || !arguments.SequenceEqual(new[] { "kestrel-service", "8080" }) ||
            !environment.SequenceEqual(expectedEnvironment))
            throw new InvalidOperationException("Unreviewed image configuration");
        var image = new Dictionary<string, ReadOnlyMemory<byte>>();
        using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(imageDirectory, "manifest.json"))))
            foreach (var item in manifest.RootElement.EnumerateArray())
            {
                byte[] bytes = File.ReadAllBytes(Path.Combine(imageDirectory, item.GetProperty("file").GetString()!));
                string digest = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                if (digest != item.GetProperty("sha256").GetString()) throw new InvalidOperationException("Image hash differs");
                image.Add(item.GetProperty("guest_path").GetString()!, bytes);
            }
        if (!image.ContainsKey(imagePath) || image.Count != 1)
            throw new InvalidOperationException("Image does not match configured mode");
        var io = new InstanceIo(image, executablePaths: image.Keys.ToHashSet(), outputLimit: 16384);
        var stop = new HostExecutionStop(TimeSpan.FromSeconds(60));
        var owner = new ThreadedGuestExecution(io, stop);
        ThreadedGuestExecutionResult? result = null;
        string? executionError = null, diagnosticError = null;
        bool ready = false, joined = false, passed = false;
        var cases = new List<(string Name, bool Passed, int Bytes, string Sha)>();
        var traceGate = new object();
        var syscallTraces = new Dictionary<int, List<ThreadedSyscallObservation>>();
        var syscallCounts = new Dictionary<int, ulong>();
        ulong unrecordedThreadObservations = 0;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                result = owner.Run(imagePath, arguments, environment, Budget,
                    syscallTrace: observation =>
                    {
                        lock (traceGate)
                        {
                            if (!syscallTraces.TryGetValue(observation.GuestThreadId, out var rows))
                            {
                                if (syscallTraces.Count == 16) { ++unrecordedThreadObservations; return; }
                                syscallTraces.Add(observation.GuestThreadId, rows = new());
                                syscallCounts.Add(observation.GuestThreadId, 0);
                            }
                            ++syscallCounts[observation.GuestThreadId];
                            if (rows.Count < 16384) rows.Add(observation);
                        }
                    });
            }
            catch (Exception error) { executionError = error.ToString(); }
            finally { completion.SetResult(); }
        }) { IsBackground = true, Name = "translated-threaded-kestrel-guest" };
        try
        {
            thread.Start();
            byte[] expectedReady = "READY 8080\n"u8.ToArray();
            while (!completion.Task.IsCompleted)
            {
                stop.Token.ThrowIfCancellationRequested();
                byte[] captured = io.CapturedOutput.StandardOutput;
                if (captured.AsSpan().SequenceEqual(expectedReady)) { ready = true; break; }
                if (!expectedReady.AsSpan().StartsWith(captured))
                    throw new InvalidOperationException("Unexpected readiness output");
                await Task.Delay(10, stop.Token);
            }
            if (!ready) throw new InvalidOperationException("Guest ended before HTTP readiness");
            IPEndPoint? endpoint = null;
            for (int fd = 0; fd < 128; ++fd)
            {
                var local = io.LocalEndpoint(fd);
                if (!local.Succeeded || local.Value.Port != 8080) continue;
                var published = io.Publish(fd);
                if (!published.Succeeded) continue;
                if (endpoint != null) throw new InvalidOperationException("Multiple matching listeners");
                endpoint = published.Value;
            }
            if (endpoint == null || !IPAddress.IsLoopback(endpoint.Address))
                throw new InvalidOperationException("No published loopback listener");
            foreach (string name in new[] { "health", "large", "fragmented", "missing", "stop" })
            {
                byte[] request = File.ReadAllBytes(Path.Combine(oracleDirectory, name + ".request"));
                byte[] expected = File.ReadAllBytes(Path.Combine(oracleDirectory, name + ".response"));
                using var client = new TcpClient(AddressFamily.InterNetwork);
                await client.ConnectAsync(endpoint.Address, endpoint.Port, stop.Token);
                using NetworkStream stream = client.GetStream();
                int[] ends = name == "fragmented" ? [1, 17, 59, 571, 1595, request.Length - 1, request.Length] : [request.Length];
                int offset = 0;
                foreach (int end in ends)
                {
                    await stream.WriteAsync(request.AsMemory(offset, end - offset), stop.Token);
                    offset = end;
                }
                using var response = new MemoryStream();
                byte[] buffer = new byte[4096];
                while (true)
                {
                    int count = await stream.ReadAsync(buffer, stop.Token);
                    if (count == 0) break;
                    if (response.Length + count > 8192) throw new InvalidOperationException("Response limit exceeded");
                    response.Write(buffer, 0, count);
                }
                byte[] actual = response.ToArray();
                File.WriteAllBytes(Path.Combine(output, name + ".response"), actual);
                bool exact = SemanticResponse(actual) == SemanticResponse(expected);
                cases.Add((name, exact, actual.Length, Convert.ToHexString(SHA256.HashData(actual)).ToLowerInvariant()));
                if (!exact) throw new InvalidOperationException(name + " HTTP semantics differ from native");
            }
            await completion.Task.WaitAsync(stop.Token);
            if (executionError != null || result == null || !result.Exited || result.ExitStatus != 0 ||
                result.StopReason != HostExecutionStopReason.None || !result.AllWorkersJoined ||
                !result.MemoryReleased || !owner.IsQuiescent ||
                result.Threads.Count == 0 || result.Threads.Any(worker => !worker.MachineReleased || worker.Signal != 0) ||
                stop.NotificationFailure != null)
                throw new InvalidOperationException("Guest failed ordinary shutdown");
            var final = io.CapturedOutput;
            if (!final.StandardOutput.AsSpan().SequenceEqual("READY 8080\nSTOPPED\n"u8) || final.StandardError.Length != 0)
                throw new InvalidOperationException("Guest final stdout/stderr differs");
            passed = true;
        }
        catch (Exception error) { diagnosticError = error.ToString(); }
        finally
        {
            if (!completion.Task.IsCompleted) stop.RequestStop();
            joined = (thread.ThreadState & ThreadState.Unstarted) != 0 || thread.Join(TimeSpan.FromSeconds(5));
            bool quiescent = joined && owner.IsQuiescent;
            if (!quiescent)
            {
                passed = false;
                diagnosticError += "\nExecution did not join and quiesce; leave IO/stop alive for process discard.";
            }
            if (stop.NotificationFailure != null) passed = false;
            var captured = io.CapturedOutput;
            File.WriteAllBytes(Path.Combine(output, "stdout.txt"), captured.StandardOutput);
            File.WriteAllBytes(Path.Combine(output, "stderr.txt"), captured.StandardError);
            bool ioDisposed = false;
            if (quiescent)
            {
                await io.DisposeAsync();
                stop.Dispose();
                ioDisposed = true;
            }
            using (var file = File.Create(Path.Combine(output, "result.json")))
            using (var writer = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                writer.WriteBoolean("guest_passed", passed);
                writer.WriteString("image_mode", mode);
                writer.WriteBoolean("ready", ready);
                writer.WriteBoolean("joined", joined);
                writer.WriteBoolean("is_quiescent", quiescent);
                writer.WriteBoolean("io_disposed", ioDisposed);
                writer.WriteString("diagnostic_error", diagnosticError);
                writer.WriteString("execution_error", joined ? executionError : "Owning thread still live; exception result unavailable.");
                writer.WriteString("notification_error", stop.NotificationFailure?.ToString());
                writer.WriteNumber("instructions_completed", owner.InstructionsCompleted);
                writer.WriteString("owner_stop_reason", stop.Reason.ToString());
                // The callback copies scalars only. A lock protects diagnostic snapshots
                // even when failed containment leaves guest workers running.
                writer.WriteBoolean("syscall_trace_available", true);
                writer.WriteBoolean("syscall_trace_complete_at_snapshot", quiescent);
                lock (traceGate)
                {
                    writer.WriteNumber("unrecorded_thread_observations", unrecordedThreadObservations);
                    writer.WriteStartArray("syscall_threads");
                    foreach (var pair in syscallTraces.OrderBy(item => item.Key))
                    {
                        writer.WriteStartObject();
                        writer.WriteNumber("guest_thread_id", pair.Key);
                        writer.WriteNumber("syscall_count", syscallCounts[pair.Key]);
                        writer.WriteBoolean("truncated", syscallCounts[pair.Key] > (ulong)pair.Value.Count);
                        writer.WriteStartArray("trace");
                        foreach (var observation in pair.Value)
                        {
                            writer.WriteStartObject();
                            writer.WriteNumber("ip", observation.InstructionPointer);
                            writer.WriteNumber("number", observation.Number);
                            writer.WriteNumber("argument1", observation.Argument1);
                            writer.WriteNumber("argument2", observation.Argument2);
                            writer.WriteNumber("argument3", observation.Argument3);
                            writer.WriteNumber("argument4", observation.Argument4);
                            writer.WriteNumber("argument5", observation.Argument5);
                            writer.WriteNumber("argument6", observation.Argument6);
                            writer.WritePropertyName("return_value");
                            if (observation.ReturnValue is { } returned) writer.WriteNumberValue(returned);
                            else writer.WriteNullValue();
                            writer.WriteEndObject();
                        }
                        writer.WriteEndArray();
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }
                writer.WritePropertyName("execution");
                if (!joined || result == null) writer.WriteNullValue();
                else
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("instructions", result.Instructions);
                    writer.WriteBoolean("exited", result.Exited);
                    writer.WriteNumber("exit_status", result.ExitStatus);
                    writer.WriteString("stop_reason", result.StopReason.ToString());
                    writer.WriteNumber("retained_bytes_before_release", result.RetainedBytesBeforeRelease);
                    writer.WriteNumber("retained_mappings_before_release", result.RetainedMappingsBeforeRelease);
                    writer.WriteBoolean("memory_released", result.MemoryReleased);
                    writer.WriteBoolean("all_workers_joined", result.AllWorkersJoined);
                    writer.WriteStartArray("threads");
                    foreach (var worker in result.Threads)
                    {
                        writer.WriteStartObject();
                        writer.WriteNumber("guest_thread_id", worker.GuestThreadId);
                        writer.WriteNumber("instructions", worker.Instructions);
                        writer.WriteNumber("ip", worker.InstructionPointer);
                        writer.WriteString("termination", worker.Termination);
                        writer.WriteNumber("status", worker.Status);
                        writer.WriteNumber("halt", worker.Halt);
                        writer.WriteNumber("signal", worker.Signal);
                        writer.WriteBoolean("machine_released", worker.MachineReleased);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                writer.WriteStartArray("cases");
                foreach (var row in cases)
                {
                    writer.WriteStartObject();
                    writer.WriteString("name", row.Name);
                    writer.WriteBoolean("passed", row.Passed);
                    writer.WriteNumber("bytes", row.Bytes);
                    writer.WriteString("response_sha256", row.Sha);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
        }
        return passed ? 0 : 1;
    }

    private static string SemanticResponse(byte[] response)
    {
        int boundary = response.AsSpan().IndexOf("\r\n\r\n"u8);
        if (boundary < 0) throw new InvalidOperationException("Incomplete HTTP response");
        string[] lines = Encoding.ASCII.GetString(response, 0, boundary).Split("\r\n");
        if (!lines[0].StartsWith("HTTP/1.1 ", StringComparison.Ordinal))
            throw new InvalidOperationException("HTTP version differs");
        var headers = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (string line in lines.Skip(1))
        {
            int colon = line.IndexOf(':');
            if (colon <= 0 || !headers.TryAdd(line[..colon].ToLowerInvariant(), line[(colon + 1)..].Trim()))
                throw new InvalidOperationException("Invalid or duplicate response header");
        }
        if (!headers.Remove("date", out string? date) ||
            !DateTimeOffset.TryParseExact(date, "r", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out _))
            throw new InvalidOperationException("Invalid RFC1123 Date");
        byte[] body = response[(boundary + 4)..];
        if (!headers.TryGetValue("content-length", out string? length) ||
            length != body.Length.ToString(System.Globalization.CultureInfo.InvariantCulture))
            throw new InvalidOperationException("HTTP body length differs");
        return lines[0] + "\n" + string.Join("\n", headers.Select(pair => pair.Key + ":" + pair.Value)) +
            "\n\n" + Convert.ToHexString(body);
    }
}
