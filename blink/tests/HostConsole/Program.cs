using Managed.Emulation.Host;

static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
static int Value(HostResult<int> result) => result.Succeeded ? result.Value : throw new Exception("Console result: " + result.Error);
static async Task<byte[]> Drain(Stream stream)
{
    using var output = new MemoryStream(); await stream.CopyToAsync(output); return output.ToArray();
}
static async Task WriteAll(HostConsole console, bool error, byte[] bytes)
{
    int offset = 0;
    while (offset < bytes.Length)
    {
        int count = Value(await console.WriteOutputAsync(error, bytes.AsMemory(offset)));
        Check(count > 0, "output progress"); offset += count;
    }
}

// Input accepts arbitrary bytes and returns buffered data before EOF. Input and
// output queues are deliberately smaller than the payload to exercise wrapping.
using (var console = new HostConsole(7))
{
    byte[] payload = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
    Task produce = Task.Run(async () => { await console.StandardInput.WriteAsync(payload); console.CloseInput(); });
    using var result = new MemoryStream(); byte[] buffer = new byte[11];
    int count;
    while ((count = Value(await console.ReadInputAsync(buffer))) != 0) result.Write(buffer, 0, count);
    await produce; Check(payload.SequenceEqual(result.ToArray()), "binary input / EOF");
}
using (var console = new HostConsole(7, captureLimit: 512, outputMarker: [0xff, 0, 0x81]))
{
    byte[] stdout = [0x80, 0xff, 0, 0x81, 0, 0xfe, 42, 255, 128];
    byte[] stderr = [0, 255, 17, 129, 32, 0];
    Task<byte[]> outRead = Drain(console.StandardOutput), errRead = Drain(console.StandardError);
    await Task.WhenAll(WriteAll(console, false, stdout), WriteAll(console, true, stderr)); console.CompleteOutput();
    byte[] actualOutput = await outRead, actualError = await errRead;
    Check(stdout.SequenceEqual(actualOutput) && stderr.SequenceEqual(actualError), "binary output / EOF");
    Check(stdout.SequenceEqual(console.CapturedOutput.StandardOutput) && stderr.SequenceEqual(console.CapturedOutput.StandardError), "binary captures");
    Check(console.OutputMarkerSeen && !console.CaptureTruncated && console.OutputBytes == 15, "marker and byte accounting");
}
using (var console = new HostConsole(31, outputLimit: 10000, captureLimit: 10000))
{
    Task<byte[]> output = Drain(console.StandardOutput), error = Drain(console.StandardError);
    var writers = Enumerable.Range(0, 8).Select(index => Task.Run(async () =>
    {
        for (int i = 0; i < 100; ++i) await WriteAll(console, index >= 4, [(byte)index, (byte)i, 0, 255]);
    })).ToArray();
    await Task.WhenAll(writers); console.CompleteOutput();
    byte[] actualOutput = await output, actualError = await error;
    Check(actualOutput.Length == 1600 && actualError.Length == 1600, "concurrent writer totals");
    Check(actualOutput.SequenceEqual(console.CapturedOutput.StandardOutput), "stdout capture follows live ordering");
    Check(actualError.SequenceEqual(console.CapturedOutput.StandardError), "stderr capture follows live ordering");
    Check(console.OutputBytes == 3200 && !console.CaptureTruncated, "concurrent aggregate accounting");
}
using (var console = new HostConsole(4))
{
    Check((await console.ReadInputAsync(new byte[1], nonBlocking: true)).Error == GuestError.Again, "nonblocking input");
    Check(Value(await console.WriteOutputAsync(false, new byte[4])) == 4, "fill output");
    Check((await console.WriteOutputAsync(false, new byte[1], nonBlocking: true)).Error == GuestError.Again, "nonblocking output");
    var waiting = console.WriteOutputAsync(false, new byte[1]);
    Check(!waiting.IsCompleted, "bounded output applies backpressure");
    using var stop = new HostExecutionStop();
    var input = console.ReadInputAsync(new byte[1], stop.Token);
    stop.RequestStop(); console.Stop();
    Check((await waiting.WaitAsync(TimeSpan.FromSeconds(2))).Error == GuestError.Canceled, "stop releases blocked output");
    Check((await input.WaitAsync(TimeSpan.FromSeconds(2))).Error == GuestError.Canceled, "stop releases input");
}
using (var console = new HostConsole(4, outputLimit: 12, captureLimit: 3, captureOnly: true))
{
    Check(Value(await console.WriteOutputAsync(false, new byte[8])) == 8, "capture-only drains without consumer");
    Check(Value(await console.WriteOutputAsync(true, new byte[8])) == 4, "combined output limit partial write");
    Check((await console.WriteOutputAsync(false, new byte[1])).Error == GuestError.NoSpace && console.OutputLimitReached, "explicit output exhaustion");
    Check(console.CaptureTruncated && console.OutputBytes == 12 && console.CapturedOutput.StandardOutput.Length == 3, "capture truncation differs from output limit");
}
using (var console = new HostConsole(4))
{
    await using var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), console: console);
    // Borrowed console belongs to the run, not the descriptor container.
    await io.DisposeAsync();
    await console.StandardInput.WriteAsync(new byte[] { 1, 0, 255 }); console.CloseInput();
    byte[] result = new byte[3]; Check(Value(await console.ReadInputAsync(result)) == 3 && result.SequenceEqual(new byte[] { 1, 0, 255 }), "borrowed console remains owned by caller");
}
using (var console = new HostConsole(4))
{
    await using var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), console: console);
    Check(Value(await io.WriteAsync(1, new byte[4])) == 4, "fill borrowed output");
    Task<HostResult<int>> input = io.ReadAsync(0, new byte[1]);
    Task<HostResult<int>> output = io.WriteAsync(1, new byte[1]);
    Check(!input.IsCompleted && !output.IsCompleted && io.PendingConsoleOperations == 2, "console operations tracked");
    await io.DisposeAsync();
    Check((await input).Error == GuestError.Canceled && (await output).Error == GuestError.Canceled && io.PendingConsoleOperations == 0, "IO disposal drains console operations");
    await console.StandardInput.WriteAsync(new byte[] { 42 }); console.CloseInput();
    byte[] result = new byte[1]; Check(Value(await console.ReadInputAsync(result)) == 1 && result[0] == 42, "IO disposal preserves borrowed console");
}
Console.WriteLine("bounded binary console: PASS");
