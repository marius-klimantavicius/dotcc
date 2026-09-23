using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace Managed.Emulation;

internal sealed record MachineMount(string GuestPath, string HostPath, MountAccess Access, bool Replace);
internal sealed record MachineStorage(string Root, MachineMount[] Mounts);
internal sealed record MachineExecution(string Executable, string[] Arguments, string WorkingDirectory,
    Dictionary<string, string> Environment, ReadinessOptions Readiness, int BufferBytes, long OutputLimit,
    int CaptureBytes, TimeSpan DrainTimeout);
internal sealed record MachineStart(MachineOptions Options, MachineExecution Execution, MachineStorage Storage);
internal sealed record MachineFrame(string Kind, int Channel = 0, byte[]? Bytes = null,
    MachineStart? Start = null, MachineRunResult? Result = null, PublishedEndpoint[]? Endpoints = null);

/// <summary>Version 1 uses one outstanding 16 KiB frame per data channel.
/// Acknowledgements release credit only after the receiver accepts those bytes.
/// Control frames never wait for data-channel credit.</summary>
internal sealed class MachineConnection(Stream input, Stream output) : IAsyncDisposable
{
    internal const int Chunk = 16384;
    private readonly SemaphoreSlim writer = new(1, 1);
    private readonly SemaphoreSlim[] credits = [new(1, 1), new(1, 1), new(1, 1)];
    private readonly int[] receivedCredit = new int[3];
    private readonly Channel<MachineFrame>[] data = Enumerable.Range(0, 3).Select(_ =>
        System.Threading.Channels.Channel.CreateBounded<MachineFrame>(new BoundedChannelOptions(1)
        { SingleReader = true, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait })).ToArray();
    private readonly CancellationTokenSource lifetime = new();
    internal CancellationToken Token => lifetime.Token;
    internal Task SendAsync(MachineFrame frame, CancellationToken token = default) => WriteAsync(frame, token);
    private async Task WriteAsync(MachineFrame frame, CancellationToken token)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, lifetime.Token);
        await writer.WaitAsync(linked.Token).ConfigureAwait(false);
        try { await InstanceProtocol.WriteAsync(output, frame, MachineJson.Default.MachineFrame, linked.Token).ConfigureAwait(false); }
        finally { writer.Release(); }
    }
    internal async Task SendDataAsync(int channel, byte[]? bytes, CancellationToken token)
    {
        if (channel is < 0 or > 2 || bytes?.Length > Chunk) throw new InvalidDataException("Invalid console chunk.");
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, lifetime.Token);
        await credits[channel].WaitAsync(linked.Token).ConfigureAwait(false);
        // EOF is a protocol event, not a nullable byte-array payload. Some
        // generated JSON serialization paths encode null byte[] as empty base64.
        await WriteAsync(new(bytes == null ? "eof" : "data", channel, bytes), linked.Token).ConfigureAwait(false);
    }
    internal async Task ReceiveAsync(Func<MachineFrame, Task> control)
    {
        Exception? failure = null;
        try
        {
            while (await InstanceProtocol.ReadAsync(input, MachineJson.Default.MachineFrame, lifetime.Token).ConfigureAwait(false) is { } frame)
            {
                if (frame.Kind is "data" or "eof" or "credit")
                {
                    if (frame.Channel is < 0 or > 2) throw new InvalidDataException("Invalid data channel.");
                    if (frame.Kind == "credit") credits[frame.Channel].Release();
                    else if ((frame.Kind == "data" && frame.Bytes == null) || frame.Bytes?.Length > Chunk ||
                        (frame.Kind == "eof" && frame.Bytes?.Length > 0) ||
                        Interlocked.Exchange(ref receivedCredit[frame.Channel], 1) != 0 || !data[frame.Channel].Writer.TryWrite(frame))
                        throw new InvalidDataException("Console sender exceeded its bounded credit.");
                }
                else await control(frame).ConfigureAwait(false);
            }
        }
        catch (Exception error) { failure = error; throw; }
        finally { foreach (var channel in data) channel.Writer.TryComplete(failure); }
    }
    internal async Task PumpReceivedAsync(int channel, Func<byte[], CancellationToken, Task> accept,
        Action eof, CancellationToken token)
    {
        await foreach (MachineFrame frame in data[channel].Reader.ReadAllAsync(token).ConfigureAwait(false))
        {
            if (frame.Kind == "eof") { eof(); return; }
            await accept(frame.Bytes!, token).ConfigureAwait(false);
            Volatile.Write(ref receivedCredit[channel], 0);
            await WriteAsync(new("credit", channel), token).ConfigureAwait(false);
        }
    }
    internal async Task PumpSentAsync(int channel, Stream source, CancellationToken token)
    {
        byte[] buffer = new byte[Chunk];
        int count;
        while ((count = await source.ReadAsync(buffer, token).ConfigureAwait(false)) != 0)
            await SendDataAsync(channel, buffer.AsSpan(0, count).ToArray(), token).ConfigureAwait(false);
        await SendDataAsync(channel, null, token).ConfigureAwait(false);
    }
    public ValueTask DisposeAsync()
    {
        lifetime.Cancel(); foreach (var channel in data) channel.Writer.TryComplete();
        // Reader/writer calls can still unwind after cancellation; their
        // synchronization objects are deliberately not disposed underneath them.
        return ValueTask.CompletedTask;
    }
}

[JsonSerializable(typeof(MachineFrame))]
internal partial class MachineJson : JsonSerializerContext;
