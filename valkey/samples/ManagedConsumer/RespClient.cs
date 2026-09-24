using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

internal sealed class RespClient : IAsyncDisposable
{
    private readonly TcpClient socket;
    private readonly NetworkStream stream;
    private readonly CancellationToken cancellation;
    private readonly byte[] one = new byte[1];

    private RespClient(TcpClient socket, CancellationToken cancellation)
    { this.socket = socket; stream = socket.GetStream(); this.cancellation = cancellation; }

    internal static async Task<RespClient> ConnectAsync(IPEndPoint endpoint, string? password, CancellationToken cancellation)
    {
        var socket = new TcpClient(endpoint.AddressFamily);
        try
        {
            await socket.ConnectAsync(endpoint.Address, endpoint.Port, cancellation);
            var client = new RespClient(socket, cancellation);
            if (password != null) await client.CommandAsync("AUTH", password);
            return client;
        }
        catch { socket.Dispose(); throw; }
    }

    internal Task<object?> CommandAsync(params string[] arguments) =>
        CommandBytesAsync(arguments.Select(Encoding.UTF8.GetBytes).ToArray());

    internal async Task<object?> CommandBytesAsync(params byte[][] arguments)
    {
        using var payload = new MemoryStream();
        void Append(string text) => payload.Write(Encoding.ASCII.GetBytes(text));
        Append("*" + arguments.Length.ToString(CultureInfo.InvariantCulture) + "\r\n");
        foreach (byte[] argument in arguments)
        {
            Append("$" + argument.Length.ToString(CultureInfo.InvariantCulture) + "\r\n");
            payload.Write(argument);
            Append("\r\n");
        }
        await stream.WriteAsync(payload.GetBuffer().AsMemory(0, checked((int)payload.Length)), cancellation);
        return await ReadAsync(0);
    }

    private async Task<byte> ByteAsync()
    {
        await stream.ReadExactlyAsync(one, cancellation);
        return one[0];
    }

    private async Task<string> LineAsync()
    {
        using var bytes = new MemoryStream();
        while (true)
        {
            byte value = await ByteAsync();
            if (value == '\r')
            {
                if (await ByteAsync() != '\n') throw new InvalidDataException("Malformed RESP line.");
                return Encoding.UTF8.GetString(bytes.GetBuffer(), 0, (int)bytes.Length);
            }
            if (bytes.Length >= 65536) throw new InvalidDataException("RESP line exceeds sample limit.");
            bytes.WriteByte(value);
        }
    }

    private async Task<object?> ReadAsync(int depth)
    {
        if (depth > 32) throw new InvalidDataException("RESP nesting exceeds sample limit.");
        byte kind = await ByteAsync();
        string line = await LineAsync();
        switch (kind)
        {
            case (byte)'+': return line;
            case (byte)'-': throw new InvalidOperationException("Valkey error: " + line);
            case (byte)':': return long.Parse(line, CultureInfo.InvariantCulture);
            case (byte)'$':
                int length = int.Parse(line, CultureInfo.InvariantCulture);
                if (length == -1) return null;
                if (length is < 0 or > 16777216) throw new InvalidDataException("Invalid RESP bulk size.");
                byte[] data = new byte[length];
                await stream.ReadExactlyAsync(data, cancellation);
                if (await ByteAsync() != '\r' || await ByteAsync() != '\n') throw new InvalidDataException("Malformed RESP bulk terminator.");
                return data;
            case (byte)'*':
                int count = int.Parse(line, CultureInfo.InvariantCulture);
                if (count == -1) return null;
                if (count is < 0 or > 65536) throw new InvalidDataException("Invalid RESP array size.");
                var array = new object?[count];
                for (int i = 0; i < count; ++i) array[i] = await ReadAsync(depth + 1);
                return array;
            default: throw new InvalidDataException("Unsupported RESP reply prefix.");
        }
    }

    public ValueTask DisposeAsync() { socket.Dispose(); return ValueTask.CompletedTask; }
}
