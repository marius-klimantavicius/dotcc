using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Managed.Emulation;

public sealed record ImageFile(string Path, byte[] Contents, bool Executable = false);
public sealed record InstanceOptions
{
    public required string Executable { get; init; }
    public required ImageFile[] Image { get; init; }
    public string[] Arguments { get; init; } = [];
    public string[] Environment { get; init; } = [];
    public string WorkingDirectory { get; init; } = "/";
    public int MemoryLimit { get; init; } = 64 * 1024 * 1024;
    public int DescriptorLimit { get; init; } = 128;
    public int OutputLimit { get; init; } = 65536;
    public long InstructionLimit { get; init; } = 100_000_000;
    public int WallClockMilliseconds { get; init; } = 60_000;
    public ushort[] PublishedPorts { get; init; } = [];

    public InstanceOptions Snapshot()
    {
        Validate();
        var copy = this with
        {
            Image = Image.Select(file => file with { Contents = file.Contents.ToArray() }).ToArray(),
            Arguments = Arguments.ToArray(), Environment = Environment.ToArray(),
            PublishedPorts = PublishedPorts.ToArray()
        };
        copy.Validate();
        return copy;
    }
    public void Validate()
    {
        if (MemoryLimit is < 4096 or > 268435456 || DescriptorLimit is < 3 or > 4096 ||
            OutputLimit is < 0 or > 1048576 || InstructionLimit < 1 ||
            WallClockMilliseconds is < 1 or > 3600000)
            throw new ArgumentException("Invalid instance limit.");
        if (Image is null || Image.Length is < 1 or > 1024 ||
            Arguments is null || Environment is null || PublishedPorts is null ||
            Arguments.Length > 256 || Environment.Length > 256 || PublishedPorts.Length > 32)
            throw new ArgumentException("Invalid instance input count.");
        static bool Text(string value) => value is not null && value.Length <= 4096 && !value.Contains('\0');
        static bool Path(string value) => Text(value) && value.StartsWith('/') &&
            !value.Split('/').Any(part => part is "." or "..");
        if (!Path(Executable) || !Path(WorkingDirectory) ||
            Arguments.Any(value => !Text(value)) || Environment.Any(value => !Text(value) || !value.Contains('=')))
            throw new ArgumentException("Invalid guest path or argument/environment string.");
        long bytes = 0;
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in Image)
        {
            if (file is null || !Path(file.Path) || file.Contents is null || !paths.Add(file.Path))
                throw new ArgumentException("Invalid or duplicate image file.");
            bytes += file.Contents.Length;
        }
        if (bytes > 16 * 1024 * 1024 || !Image.Any(file => file.Path == Executable && file.Executable) ||
            PublishedPorts.Any(port => port == 0) || PublishedPorts.Distinct().Count() != PublishedPorts.Length)
            throw new ArgumentException("Image or publication exceeds the initial profile.");
    }
}
public sealed record WorkerRequest(string Kind, InstanceOptions? Options = null);
public sealed record PublishedEndpoint(ushort GuestPort, int HostPort, string HostAddress = "127.0.0.1");
public sealed record WorkerEvent
{
    public required string Kind { get; init; }
    public PublishedEndpoint[] Endpoints { get; init; } = [];
    public string? Reason { get; init; }
    public int ExitStatus { get; init; }
    public int Halt { get; init; }
    public int Signal { get; init; }
    public long Instructions { get; init; }
    public byte[] StandardOutput { get; init; } = [];
    public byte[] StandardError { get; init; } = [];
    public string? Detail { get; init; }
}

/// <summary>Length-bounded UTF8 JSON frames, separate from guest descriptors and
/// diagnostics. The worker must reserve its raw standard streams for this IPC.</summary>
public static class InstanceProtocol
{
    public const int MaximumFrame = 24 * 1024 * 1024;
    private sealed class FrameBuffer : MemoryStream
    {
        private void Check(int count)
        {
            if (count < 0 || Position + count > MaximumFrame)
                throw new InvalidDataException("Control frame exceeds its bound.");
        }
        public override void Write(byte[] buffer, int offset, int count)
        { Check(count); base.Write(buffer, offset, count); }
        public override void Write(ReadOnlySpan<byte> buffer)
        { Check(buffer.Length); base.Write(buffer); }
        public override void WriteByte(byte value) { Check(1); base.WriteByte(value); }
    }
    public static byte[] Encode<T>(T value, JsonTypeInfo<T> type)
    {
        using var buffer = new FrameBuffer();
        JsonSerializer.Serialize(buffer, value, type);
        return buffer.ToArray();
    }
    public static Task WriteAsync<T>(Stream stream, T value, JsonTypeInfo<T> type, CancellationToken token = default)
        => WriteEncodedAsync(stream, Encode(value, type), token);
    public static async Task WriteEncodedAsync(Stream stream, ReadOnlyMemory<byte> body, CancellationToken token = default)
    {
        if (body.Length is < 1 or > MaximumFrame) throw new InvalidDataException("Control frame exceeds its bound.");
        byte[] header = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(header, body.Length);
        await stream.WriteAsync(header, token).ConfigureAwait(false);
        await stream.WriteAsync(body, token).ConfigureAwait(false);
        await stream.FlushAsync(token).ConfigureAwait(false);
    }
    public static async Task<T?> ReadAsync<T>(Stream stream, JsonTypeInfo<T> type, CancellationToken token = default) where T : class
    {
        byte[] header = new byte[4];
        if (await stream.ReadAsync(header.AsMemory(0, 1), token).ConfigureAwait(false) == 0) return null;
        await stream.ReadExactlyAsync(header.AsMemory(1), token).ConfigureAwait(false);
        int length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length is < 1 or > MaximumFrame) throw new InvalidDataException("Invalid control frame length.");
        byte[] body = new byte[length];
        await stream.ReadExactlyAsync(body, token).ConfigureAwait(false);
        return JsonSerializer.Deserialize(body, type) ?? throw new InvalidDataException("Null control frame.");
    }
}
[JsonSerializable(typeof(WorkerRequest))]
[JsonSerializable(typeof(WorkerEvent))]
public partial class InstanceJson : JsonSerializerContext;
