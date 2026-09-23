using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Managed.Emulation.Host;

/// <summary>Private bounded HTTP/1.x adapter. One request per connection, no ASP.NET dependency.</summary>
internal sealed class ImdsV2Server : IAsyncDisposable
{
    internal static readonly GuestEndpoint GuestAddress = new(0xa9fea9fe, 80);
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource stop = new();
    private readonly SemaphoreSlim slots = new(32);
    private readonly object sync = new();
    private readonly HashSet<Task> pending = [];
    private readonly ImdsV2Service service;
    private readonly Task accepting;
    private Task? disposal;
    private Exception? failure;
    internal IPEndPoint Endpoint { get; }

    internal ImdsV2Server(ImdsV2Options options)
    {
        service = new(options);
        try { listener.Start(32); Endpoint = (IPEndPoint)listener.LocalEndpoint; }
        catch { listener.Stop(); stop.Dispose(); slots.Dispose(); throw; }
        accepting = AcceptAsync();
    }

    private async Task AcceptAsync()
    {
        try
        {
            while (true)
            {
                await slots.WaitAsync(stop.Token).ConfigureAwait(false);
                TcpClient client;
                try { client = await listener.AcceptTcpClientAsync(stop.Token).ConfigureAwait(false); }
                catch { slots.Release(); throw; }
                var complete = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                lock (sync) pending.Add(complete.Task);
                _ = HandleAsync(client, complete);
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        catch (SocketException) when (stop.IsCancellationRequested) { }
        catch (ObjectDisposedException) when (stop.IsCancellationRequested) { }
        catch (Exception error) { Interlocked.CompareExchange(ref failure, error, null); }
    }

    private async Task HandleAsync(TcpClient client, TaskCompletionSource complete)
    {
        using (client)
        using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop.Token))
        {
            deadline.CancelAfter(TimeSpan.FromSeconds(30));
            try
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[16384];
                int length = 0, end = -1;
                while (length < buffer.Length && end < 0)
                {
                    int count = await stream.ReadAsync(buffer.AsMemory(length), deadline.Token).ConfigureAwait(false);
                    if (count == 0) return;
                    int previous = length; length += count;
                    for (int i = Math.Max(0, previous - 3); i + 3 < length; ++i)
                        if (buffer[i] == 13 && buffer[i + 1] == 10 && buffer[i + 2] == 13 && buffer[i + 3] == 10)
                        { end = i; break; }
                }
                ImdsResponse response;
                string method = "";
                if (end < 0) response = Error(431);
                else if (!Parse(buffer.AsSpan(0, end), out method, out string path, out var headers)) response = Error(400);
                else response = service.Handle(method, path, headers);
                string reason = response.Status switch
                {
                    200 => "OK", 400 => "Bad Request", 401 => "Unauthorized", 403 => "Forbidden",
                    404 => "Not Found", 405 => "Method Not Allowed", 431 => "Request Header Fields Too Large", _ => "Error"
                };
                string extra = response.TokenTtl is { } ttl ? $"X-aws-ec2-metadata-token-ttl-seconds: {ttl}\r\n" : "";
                byte[] head = Encoding.ASCII.GetBytes($"HTTP/1.1 {response.Status} {reason}\r\nContent-Type: {response.ContentType}\r\nContent-Length: {response.Body.Length}\r\nConnection: close\r\n{extra}\r\n");
                await stream.WriteAsync(head, deadline.Token).ConfigureAwait(false);
                if (method != "HEAD") await stream.WriteAsync(response.Body, deadline.Token).ConfigureAwait(false);
                client.Client.Shutdown(SocketShutdown.Send);
                // Drain until peer EOF (bounded), so closing with unread bytes
                // cannot reset and discard an otherwise complete response.
                deadline.CancelAfter(TimeSpan.FromSeconds(2));
                while (await stream.ReadAsync(buffer, deadline.Token).ConfigureAwait(false) != 0) { }
            }
            catch (OperationCanceledException) { }
            catch (IOException) { }
            catch (SocketException) { }
            catch (Exception error) { Interlocked.CompareExchange(ref failure, error, null); }
            finally
            {
                client.Dispose();
                slots.Release();
                lock (sync) { pending.Remove(complete.Task); complete.SetResult(); }
            }
        }
    }

    private static ImdsResponse Error(int code) => new(code, []);
    private static bool Parse(ReadOnlySpan<byte> bytes, out string method, out string path, out Dictionary<string, string> headers)
    {
        method = path = ""; headers = new(StringComparer.OrdinalIgnoreCase);
        foreach (byte b in bytes) if (b > 126 || b < 32 && b is not (9 or 10 or 13)) return false;
        string[] lines = Encoding.ASCII.GetString(bytes).Split("\r\n", StringSplitOptions.None);
        if (lines.Length > 65 || lines[0].Length > 2048) return false;
        string[] request = lines[0].Split(' ');
        if (request.Length != 3 || request[2] is not ("HTTP/1.1" or "HTTP/1.0") || !request[1].StartsWith('/')) return false;
        method = request[0]; path = request[1];
        foreach (string line in lines.Skip(1))
        {
            int colon = line.IndexOf(':');
            if (colon <= 0 || line[..colon].Any(c => !char.IsAsciiLetterOrDigit(c) && !"!#$%&'*+-.^_`|~".Contains(c))) return false;
            string value = line[(colon + 1)..].Trim(' ', '\t');
            if (value.Any(c => c is '\r' or '\n') || !headers.TryAdd(line[..colon], value)) return false;
        }
        if (request[2] == "HTTP/1.1" && (!headers.TryGetValue("Host", out string? host) || host.Length == 0)) return false;
        // IMDS requests have no payload. Unsupported framing is rejected and
        // this connection is closed, never reinterpreted as another request.
        if (headers.ContainsKey("Transfer-Encoding") || headers.ContainsKey("Expect")) return false;
        return !headers.TryGetValue("Content-Length", out string? size) ||
            long.TryParse(size, NumberStyles.None, CultureInfo.InvariantCulture, out long count) && count == 0;
    }

    public ValueTask DisposeAsync()
    {
        lock (sync) return new(disposal ??= DisposeCoreAsync());
    }
    private async Task DisposeCoreAsync()
    {
        stop.Cancel(); listener.Stop();
        await accepting.ConfigureAwait(false);
        Task[] tasks; lock (sync) tasks = pending.ToArray();
        await Task.WhenAll(tasks).ConfigureAwait(false);
        slots.Dispose(); stop.Dispose();
        if (failure != null) throw new IOException("Private IMDS listener failed.", failure);
    }
}
