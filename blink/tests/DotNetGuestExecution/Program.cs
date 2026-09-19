using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

internal static class Program
{
    private const ulong Budget = 20_000_000;

    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 3) return 80;
        string imageDirectory = Path.GetFullPath(args[0]);
        string oracleDirectory = Path.GetFullPath(args[1]);
        string output = Path.GetFullPath(args[2]);
        Directory.CreateDirectory(output);
        var image = new Dictionary<string, ReadOnlyMemory<byte>>();
        using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(imageDirectory, "manifest.json"))))
            foreach (var item in manifest.RootElement.EnumerateArray())
            {
                byte[] bytes = File.ReadAllBytes(Path.Combine(imageDirectory, item.GetProperty("file").GetString()!));
                string digest = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                if (digest != item.GetProperty("sha256").GetString()) throw new InvalidOperationException("Image hash differs");
                image.Add(item.GetProperty("guest_path").GetString()!, bytes);
            }
        var io = new InstanceIo(image, executablePaths: image.Keys.ToHashSet(), outputLimit: 16384);
        var stop = new HostExecutionStop(TimeSpan.FromSeconds(30));
        var owner = new GuestExecution(io, stop);
        GuestExecutionResult? result = null;
        string? executionError = null, diagnosticError = null;
        bool ready = false, joined = false, passed = false;
        var cases = new List<(string Name, bool Passed, int Bytes, string Sha)>();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                result = owner.Run("/bin/dotnet-service", new[] { "dotnet-service", "8080" },
                    new[] { "LANG=C", "LD_LIBRARY_PATH=/lib/x86_64-linux-gnu:/lib64" }, Budget, allowInterpreter: true);
            }
            catch (Exception error) { executionError = error.ToString(); }
            finally { completion.SetResult(); }
        }) { IsBackground = true, Name = "translated-dotnet-guest" };
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
            foreach (string name in new[] { "health", "stop" })
            {
                byte[] request = File.ReadAllBytes(Path.Combine(oracleDirectory, name + ".request"));
                byte[] expected = File.ReadAllBytes(Path.Combine(oracleDirectory, name + ".response"));
                using var client = new TcpClient(AddressFamily.InterNetwork);
                await client.ConnectAsync(endpoint.Address, endpoint.Port, stop.Token);
                using NetworkStream stream = client.GetStream();
                await stream.WriteAsync(request, stop.Token);
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
                bool exact = actual.AsSpan().SequenceEqual(expected);
                cases.Add((name, exact, actual.Length, Convert.ToHexString(SHA256.HashData(actual)).ToLowerInvariant()));
                if (!exact) throw new InvalidOperationException(name + " wire response differs from native");
            }
            await completion.Task.WaitAsync(stop.Token);
            if (executionError != null || result == null || !result.Exited || result.ExitStatus != 0 ||
                result.Signal != 0 || result.Halt != -10 || result.StopReason != HostExecutionStopReason.None ||
                !result.MemoryReleased || stop.NotificationFailure != null)
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
            if (!joined)
            {
                passed = false;
                diagnosticError += "\nOwning thread did not stop; leave owners alive for process discard.";
            }
            if (stop.NotificationFailure != null) passed = false;
            var captured = io.CapturedOutput;
            File.WriteAllBytes(Path.Combine(output, "stdout.txt"), captured.StandardOutput);
            File.WriteAllBytes(Path.Combine(output, "stderr.txt"), captured.StandardError);
            using (var file = File.Create(Path.Combine(output, "result.json")))
            using (var writer = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                writer.WriteBoolean("guest_passed", passed);
                writer.WriteBoolean("ready", ready);
                writer.WriteBoolean("joined", joined);
                writer.WriteString("diagnostic_error", diagnosticError);
                writer.WriteString("execution_error", executionError);
                writer.WriteString("notification_error", stop.NotificationFailure?.ToString());
                writer.WriteNumber("instructions_completed", owner.InstructionsCompleted);
                writer.WriteString("owner_stop_reason", stop.Reason.ToString());
                writer.WritePropertyName("failure_state");
                if (owner.LastFailureState is not { } failure) writer.WriteNullValue();
                else
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("ip", failure.InstructionPointer);
                    writer.WriteNumber("accumulator", failure.Accumulator);
                    writer.WriteNumber("argument1", failure.Argument1);
                    writer.WriteNumber("argument2", failure.Argument2);
                    writer.WriteNumber("argument3", failure.Argument3);
                    writer.WriteNumber("argument4", failure.Argument4);
                    writer.WriteNumber("argument5", failure.Argument5);
                    writer.WriteNumber("argument6", failure.Argument6);
                    writer.WriteNumber("host_error", failure.HostError);
                    writer.WriteEndObject();
                }
                writer.WritePropertyName("execution");
                if (result == null) writer.WriteNullValue();
                else
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("instructions", result.Instructions);
                    writer.WriteNumber("ip", result.InstructionPointer);
                    writer.WriteNumber("halt", result.Halt);
                    writer.WriteNumber("signal", result.Signal);
                    writer.WriteNumber("signal_code", result.SignalCode);
                    writer.WriteBoolean("exited", result.Exited);
                    writer.WriteNumber("exit_status", result.ExitStatus);
                    writer.WriteString("stop_reason", result.StopReason.ToString());
                    writer.WriteNumber("retained_bytes_before_release", result.RetainedBytesBeforeRelease);
                    writer.WriteNumber("retained_mappings_before_release", result.RetainedMappingsBeforeRelease);
                    writer.WriteBoolean("memory_released", result.MemoryReleased);
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
            if (joined)
            {
                await io.DisposeAsync();
                stop.Dispose();
            }
        }
        return passed ? 0 : 1;
    }
}
