using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

internal sealed record RespError(string Message);
internal sealed record RespMap(object?[] Pairs);

/// <summary>BCL-only wire client. Errors stay values so negative tests verify actual RESP errors.</summary>
internal sealed class RespConnection : IAsyncDisposable
{
    private readonly TcpClient socket;
    private readonly NetworkStream stream;
    private readonly CancellationToken cancellation;
    private readonly byte[] one = new byte[1];
    private RespConnection(TcpClient socket, CancellationToken cancellation)
    { this.socket = socket; stream = socket.GetStream(); this.cancellation = cancellation; }

    internal static async Task<RespConnection> Connect(IPEndPoint endpoint, CancellationToken cancellation, string? password = null)
    {
        var socket = new TcpClient(endpoint.AddressFamily);
        try
        {
            await socket.ConnectAsync(endpoint.Address, endpoint.Port, cancellation);
            var result = new RespConnection(socket, cancellation);
            if (password is not null) Check.Equal(await result.Command("AUTH", password), "OK");
            return result;
        }
        catch { socket.Dispose(); throw; }
    }
    internal Task<object?> Command(params string[] args) => Bytes(args.Select(Encoding.UTF8.GetBytes).ToArray());
    internal async Task<object?> Bytes(params byte[][] args)
    {
        await stream.WriteAsync(Encode(args), cancellation);
        return await Read(0);
    }
    internal async Task<object?> Fragmented(byte[][] args, int size)
    {
        byte[] bytes = Encode(args);
        for (int offset = 0; offset < bytes.Length; offset += size)
            await stream.WriteAsync(bytes.AsMemory(offset, Math.Min(size, bytes.Length - offset)), cancellation);
        return await Read(0);
    }
    internal async Task<object?[]> Pipeline(params string[][] commands)
    {
        using var payload = new MemoryStream();
        foreach (var command in commands) payload.Write(Encode(command.Select(Encoding.UTF8.GetBytes).ToArray()));
        await stream.WriteAsync(payload.GetBuffer().AsMemory(0, checked((int)payload.Length)), cancellation);
        var results = new object?[commands.Length];
        for (int i = 0; i < results.Length; i++) results[i] = await Read(0);
        return results;
    }
    private static byte[] Encode(byte[][] args)
    {
        using var buffer = new MemoryStream();
        void Append(string value) => buffer.Write(Encoding.ASCII.GetBytes(value));
        Append("*" + args.Length.ToString(CultureInfo.InvariantCulture) + "\r\n");
        foreach (var arg in args)
        {
            Append("$" + arg.Length.ToString(CultureInfo.InvariantCulture) + "\r\n");
            buffer.Write(arg); Append("\r\n");
        }
        return buffer.ToArray();
    }
    private async Task<byte> Byte()
    {
        await stream.ReadExactlyAsync(one, cancellation);
        return one[0];
    }
    private async Task<string> Line()
    {
        using var buffer = new MemoryStream();
        while (true)
        {
            byte value = await Byte();
            if (value == '\r')
            {
                if (await Byte() != '\n') throw new InvalidDataException("Invalid RESP line terminator.");
                return Encoding.UTF8.GetString(buffer.GetBuffer(), 0, checked((int)buffer.Length));
            }
            if (buffer.Length >= 65536) throw new InvalidDataException("RESP line too large.");
            buffer.WriteByte(value);
        }
    }
    private async Task<object?> Read(int depth)
    {
        if (depth > 64) throw new InvalidDataException("RESP nesting too deep.");
        byte kind = await Byte();
        string line = await Line();
        switch (kind)
        {
            case (byte)'+': return line;
            case (byte)'-': return new RespError(line);
            case (byte)':': return long.Parse(line, CultureInfo.InvariantCulture);
            case (byte)'_': return line.Length == 0 ? null : throw new InvalidDataException("Invalid RESP null.");
            case (byte)'#': return line switch { "t" => true, "f" => false, _ => throw new InvalidDataException("Invalid RESP boolean.") };
            case (byte)',': return double.Parse(line, CultureInfo.InvariantCulture);
            case (byte)'$':
            case (byte)'!':
            case (byte)'=':
                int length = int.Parse(line, CultureInfo.InvariantCulture);
                if (length == -1 && kind == '$') return null;
                if (length is < 0 or > 16777216) throw new InvalidDataException("Invalid bulk length.");
                byte[] bytes = new byte[length];
                await stream.ReadExactlyAsync(bytes, cancellation);
                if (await Byte() != '\r' || await Byte() != '\n') throw new InvalidDataException("Invalid bulk terminator.");
                return kind == '!' ? new RespError(Encoding.UTF8.GetString(bytes)) : bytes;
            case (byte)'*':
            case (byte)'~':
            case (byte)'%':
                int count = int.Parse(line, CultureInfo.InvariantCulture);
                if (count == -1 && kind == '*') return null;
                if (count is < 0 or > 65536) throw new InvalidDataException("Invalid collection length.");
                object?[] values = new object?[kind == '%' ? checked(count * 2) : count];
                for (int i = 0; i < values.Length; i++) values[i] = await Read(depth + 1);
                return kind == '%' ? new RespMap(values) : values;
            default: throw new InvalidDataException($"Unexpected RESP prefix {(char)kind}.");
        }
    }
    public ValueTask DisposeAsync() { socket.Dispose(); return ValueTask.CompletedTask; }
}

internal static class Check
{
    internal static string Text(object? value) => value is byte[] bytes ? Encoding.UTF8.GetString(bytes) : value as string ?? throw new InvalidOperationException($"Expected text, got {value}.");
    internal static void Equal(object? actual, object? expected)
    {
        if (actual is byte[] bytes && expected is string) actual = Encoding.UTF8.GetString(bytes);
        if (!Equals(actual, expected)) throw new InvalidOperationException($"Expected {expected ?? "null"}, got {actual ?? "null"}.");
    }
    internal static void True(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    internal static void Error(object? value, string? contains = null)
    {
        if (value is not RespError error || contains is not null && !error.Message.Contains(contains, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Expected RESP error containing '{contains}', got {value}.");
    }
    internal static void Binary(object? value, byte[] expected) => True(value is byte[] bytes && bytes.AsSpan().SequenceEqual(expected), "Binary reply differs.");
}
