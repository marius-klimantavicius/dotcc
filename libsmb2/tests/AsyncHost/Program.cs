using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Managed.Smb;
using static Managed.Smb.LibSmb2;
using C = Managed.Smb.LibSmb2.Libc;

var stopwatch = System.Diagnostics.Stopwatch.StartNew();
long allocatedBefore = GC.GetTotalAllocatedBytes();
if (Marshal.SizeOf<t_socket>() != sizeof(int)) throw new Exception("socket ABI size");
t_socket invalid = -1;
if ((int)invalid != -1 || invalid >= 0) throw new Exception("socket sentinel/conversion");
await Task.WhenAll(Exercise(41001, AddressFamily.InterNetwork), Exercise(41002, AddressFamily.InterNetwork));
if (Socket.OSSupportsIPv6) await Exercise(41003, AddressFamily.InterNetworkV6);
var stats = HostSockets.Snapshot();
Assert(stats.RegisteredSockets == 0 && stats.RegisteredContexts == 0, "registry drained");
Assert(stats.PeakSocketReceiveBytes <= HostSockets.BufferCapacity && stats.PeakSocketSendBytes <= HostSockets.BufferCapacity, "buffer caps");
Console.WriteLine($"Elapsed={stopwatch.ElapsedMilliseconds}ms allocated={GC.GetTotalAllocatedBytes() - allocatedBefore} threadPoolThreads={ThreadPool.ThreadCount} metrics={stats}");
Console.WriteLine("PASS: async host IPv4/IPv6, independent contexts, bounded partial I/O, EOF, ownership, DNS, close/drain");

static async Task Exercise(nint owner, AddressFamily family)
{
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    var token = deadline.Token;
    var notifications = Channel.CreateUnbounded<t_socket>(new() { SingleReader = true });
    HostSockets.RegisterContext(owner, fd => notifications.Writer.TryWrite(fd));
    using var listener = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
    listener.Bind(new IPEndPoint(family == AddressFamily.InterNetwork ? IPAddress.Loopback : IPAddress.IPv6Loopback, 0));
    listener.Listen(1);
    var endpoint = (IPEndPoint)listener.LocalEndPoint!;
    t_socket handle = 0;
    byte[] sent = new byte[HostSockets.BufferCapacity * 4 + 137];
    for (int i = 0; i < sent.Length; i++) sent[i] = (byte)(i * 31 + 9);
    var accept = listener.AcceptAsync(token);
    try
    {
        using (HostSockets.EnterContext(owner))
        {
            handle = HostSockets.Create(family == AddressFamily.InterNetwork ? AF_INET : AF_INET6, SOCK_STREAM, 0);
            Assert(handle > 0, "create");
            VerifyDns(endpoint.Address, endpoint.Port);
            Assert(HostSockets.Connect(handle, endpoint) == -1 && C.errno == C.EINPROGRESS, "async connect contract");
            HostSockets.Add(owner, handle);
            // Connect must wake even if upstream suppresses a repeated interest callback.
        }
        int connectEvents = 0;
        while ((connectEvents & POLLOUT) == 0)
        {
            await notifications.Reader.ReadAsync(token);
            connectEvents = HostSockets.Events(owner, handle);
        }
        using (HostSockets.EnterContext(owner))
        {
            Assert(HostSockets.GetError(handle, out int error) == 0 && error == 0, "connect completion");
            HostSockets.SetEvents(owner, handle, POLLIN | POLLOUT);
        }
        using var peer = await accept;
        var echo = Echo(peer, sent.Length, token);
        var received = new byte[sent.Length];
        int sendOffset = 0, receiveOffset = 0;
        bool eof = false;
        while (!eof)
        {
            using (HostSockets.EnterContext(owner))
            {
                if (sendOffset < sent.Length)
                {
                    int accepted = HostSockets.Write(handle, sent.AsSpan(sendOffset));
                    Assert(accepted <= HostSockets.BufferCapacity, "bounded partial send");
                    if (accepted > 0) sendOffset += accepted;
                    else Assert(C.errno == C.EAGAIN, "write would-block");
                }
                // Small requests exercise short reads and buffered-data rearming.
                Span<byte> destination = receiveOffset < received.Length
                    ? received.AsSpan(receiveOffset, Math.Min(2503, received.Length - receiveOffset)) : new byte[1];
                int count = HostSockets.Read(handle, destination);
                if (count > 0) receiveOffset += count;
                else if (count == 0) eof = true;
                else Assert(C.errno == C.EAGAIN, "receive would-block");
                HostSockets.Rearm(owner, handle);
            }
            if (!eof)
            {
                await notifications.Reader.ReadAsync(token);
                HostSockets.Events(owner, handle);
            }
        }
        await echo;
        Assert(sendOffset == sent.Length && receiveOffset == received.Length, "complete byte counts");
        Assert(sent.AsSpan().SequenceEqual(received), "ordered exact bytes");
        // Valid registry membership is insufficient without the owning context scope.
        Assert(HostSockets.Write(handle, new byte[1]) == -1 && C.errno == C.EBADF, "foreign context rejected");
        using (HostSockets.EnterContext(owner))
        {
            Assert(HostSockets.Close(handle) == 0, "close");
            Assert(HostSockets.Read(handle, new byte[1]) == -1 && C.errno == C.EBADF, "retired token rejected");
            var newer = HostSockets.Create(family == AddressFamily.InterNetwork ? AF_INET : AF_INET6, SOCK_STREAM, 0);
            Assert(newer > handle, "tokens are not recycled");
            HostSockets.Close(newer);
        }
    }
    finally { await HostSockets.DrainContextAsync(owner); }
}
static async Task Echo(Socket peer, int length, CancellationToken token)
{
    byte[] buffer = new byte[8191];
    int total = 0;
    while (total < length)
    {
        int received = await peer.ReceiveAsync(buffer, SocketFlags.None, token);
        Assert(received > 0, "peer EOF after input");
        total += received;
        int offset = 0;
        while (offset < received) offset += await peer.SendAsync(buffer.AsMemory(offset, received - offset), SocketFlags.None, token);
    }
    peer.Shutdown(SocketShutdown.Send);
}
static unsafe void VerifyDns(IPAddress address, int port)
{
    byte[] name = "prepared.invalid\0"u8.ToArray();
    byte[] service = System.Text.Encoding.UTF8.GetBytes(port.ToString(System.Globalization.CultureInfo.InvariantCulture) + '\0');
    fixed (byte* node = name, value = service)
    {
        addrinfo* result = null;
        Assert(HostGetaddrinfo(node, value, null, &result) == EAI_FAIL && result == null, "unprepared DNS rejected");
        using var scope = HostSockets.PreparedAddresses("prepared.invalid", [address]);
        Assert(HostGetaddrinfo(node, value, null, &result) == 0 && result != null, "prepared DNS");
        Assert(result->ai_family == (address.AddressFamily == AddressFamily.InterNetwork ? AF_INET : AF_INET6), "DNS family");
        HostFreeaddrinfo(result);
    }
}
static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
