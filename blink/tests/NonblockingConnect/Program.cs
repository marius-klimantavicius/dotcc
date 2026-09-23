using System.Net;
using System.Net.Sockets;
using Managed.Emulation.Host;

using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
using var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start(128);
ushort port = (ushort)((IPEndPoint)listener.LocalEndpoint).Port;
GuestEndpoint destination = new(GuestEndpoint.Loopback, port);
var policy = new GuestNetworkPolicy(outbound: [new("127.0.0.1", port)]);
int pending = 0, immediate = 0;

// Vary registration timing, including observing an unconnected HUP before the
// transition. Completion must still deliver a fresh OUT edge exactly once.
for (int iteration = 0; iteration < 32; ++iteration)
{
    await using var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), networkPolicy: policy);
    int fd = Ok(io.Socket());
    int duplicate = Ok(io.Duplicate(fd));
    Ok(io.SetStatusFlags(duplicate, 2048));
    Check((Ok(io.GetStatusFlags(fd)) & 2048) != 0, "duplicate shares nonblocking status");
    int epoll = Ok(io.CreateEpoll(0));
    bool before = iteration % 2 == 0;
    if (before)
    {
        Ok(io.ControlEpoll(epoll, 1, duplicate, 0x80000004, 123));
        Ok(await io.WaitEpollEventsAsync(epoll, 1, 0, deadline.Token));
    }
    var connecting = await io.ConnectAsync(fd, destination, deadline.Token);
    if (connecting.Succeeded) ++immediate;
    else { Check(connecting.Error == GuestError.InProgress, $"connect: {connecting.Error}"); ++pending; }

    var repeated = await io.ConnectAsync(duplicate, destination, deadline.Token);
    Check(repeated.Error is GuestError.AlreadyInProgress or GuestError.AlreadyConnected, "pending/connected repeat contract");
    // A snapshot during the transition is either empty or successfully writable,
    // never a fabricated hangup. No minimum loopback duration is assumed.
    var snapshot = Ok(await io.PollAsync([new(duplicate, 4)], 0, deadline.Token));
    Check((snapshot.Events[0] & (8 | 16)) == 0, "pending connect has no ERR/HUP");
    Ok(io.Close(fd));
    using var peer = await listener.AcceptSocketAsync(deadline.Token);
    if (!before) Ok(io.ControlEpoll(epoll, 1, duplicate, 0x80000004, 123));
    var poll = Ok(await io.PollAsync([new(duplicate, 4)], 5000, deadline.Token));
    Check(poll.Count == 1 && (poll.Events[0] & 4) != 0 && (poll.Events[0] & (8 | 16)) == 0, "connect readiness has OUT without ERR/HUP");
    Check(Ok(io.GetSocketOption(duplicate, 1, 4)) == 0, "SO_ERROR success");
    Check(Ok(io.GetSocketOption(duplicate, 1, 4)) == 0, "SO_ERROR repeated success");
    var edges = Ok(await io.WaitEpollEventsAsync(epoll, 1, 5000, deadline.Token));
    Check(edges.Length == 1 && edges[0].Data == 123 && edges[0].Events == 4, "one completion edge");
    Check(Ok(await io.WaitEpollEventsAsync(epoll, 1, 0, deadline.Token)).Length == 0, "no repeated OUT edge");
    Check((await io.ConnectAsync(duplicate, destination)).Error == GuestError.AlreadyConnected, "connected repeat gives EISCONN");
    Check(Ok(await io.SendAsync(duplicate, new byte[] { 42 }, deadline.Token)) == 1, "send connected byte");
    byte[] received = new byte[1];
    Check(await peer.ReceiveAsync(received, SocketFlags.None, deadline.Token) == 1 && received[0] == 42, "peer received byte");
    Ok(io.Close(duplicate));
    Check(Ok(await io.WaitEpollEventsAsync(epoll, 1, 0, deadline.Token)).Length == 0, "final close removes interest");
    int replacement = Ok(io.Socket());
    Check(replacement == fd, "descriptor slot reused");
    Check(io.PeerEndpoint(replacement).Error == GuestError.NotConnected, "replacement has no prior connection");
}
Console.WriteLine($"PASS 32 connect/epoll/duplicate cycles (pending={pending}, immediate={immediate})");

// The canceled caller scope does not own an already initiated asynchronous
// connect. Normal close may race successful establishment; both are supported.
for (int iteration = 0; iteration < 32; ++iteration)
{
    await using var network = new VirtualTcpNetwork(policy: policy);
    int fd = Ok(network.Create());
    Ok(network.SetNonBlocking(fd, true));
    using var syscall = new CancellationTokenSource();
    var result = await network.ConnectAsync(fd, destination, syscall.Token);
    Check(result.Succeeded || result.Error == GuestError.InProgress, "start before close");
    syscall.Cancel();
    if (iteration % 2 == 0)
    {
        using var peer = await listener.AcceptSocketAsync(deadline.Token);
        // Wait for completion using only public state, independent of its timing.
        while (network.PeerEndpoint(fd).Error == GuestError.NotConnected)
            await Task.Delay(1, deadline.Token);
        Check(Ok(network.GetOption(fd, 1, 4)) == 0, "caller cancellation did not cancel connection");
    }
    Ok(network.Close(fd));
    Check(network.PendingOperations == 0, "last close drains connect");
    int replacement = Ok(network.Create());
    Check(replacement == fd && network.PeerEndpoint(replacement).Error == GuestError.NotConnected, "handle reuse isolated");
    await network.DisposeAsync();
    Check(network.PendingOperations == 0 && network.OpenDescriptors == 0, "machine disposal drains work");
    // Drain ordinary completed handshakes queued by immediately closed sockets.
    while (listener.Pending()) { using var accepted = await listener.AcceptSocketAsync(deadline.Token); }
}
Console.WriteLine("PASS 32 cancellation/close/dispose/reuse cycles");

// Dispose directly after initiation, without first closing the descriptor.
// Multiple owners start independently; each must drain only its own operation.
await Task.WhenAll(Enumerable.Range(0, 16).Select(async _ =>
{
    var network = new VirtualTcpNetwork(policy: policy);
    int fd = Ok(network.Create());
    Ok(network.SetNonBlocking(fd, true));
    var result = await network.ConnectAsync(fd, destination, deadline.Token);
    Check(result.Succeeded || result.Error == GuestError.InProgress, "start before dispose");
    await network.DisposeAsync();
    Check(network.PendingOperations == 0 && network.OpenDescriptors == 0, "direct disposal drains connection");
}));
while (listener.Pending()) { using var accepted = await listener.AcceptSocketAsync(deadline.Token); }
Console.WriteLine("PASS 16 concurrent owners dispose after initiation");

// Observe pre-connect HUP, then do not inspect epoll again until after graceful
// full shutdown. No intermediate snapshot may be needed to rearm terminal edges.
await using (var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), networkPolicy: policy))
{
    int fd = Ok(io.Socket());
    int epoll = Ok(io.CreateEpoll(0));
    Ok(io.ControlEpoll(epoll, 1, fd, 0x80000005, 321));
    var initial = Ok(await io.WaitEpollEventsAsync(epoll, 1, 0, deadline.Token));
    Check(initial.Length == 1 && (initial[0].Events & 16) != 0, "unconnected HUP consumed");
    Ok(await io.ConnectAsync(fd, destination, deadline.Token));
    using var peer = await listener.AcceptSocketAsync(deadline.Token);
    Ok(io.Shutdown(fd, SocketShutdown.Both));
    var shutdown = Ok(await io.WaitEpollEventsAsync(epoll, 1, 5000, deadline.Token));
    Check(shutdown.Length == 1 && shutdown[0].Data == 321 && (shutdown[0].Events & 16) != 0, "fresh HUP after connect and shutdown");
    Check(Ok(await io.WaitEpollEventsAsync(epoll, 1, 0, deadline.Token)).Length == 0, "shutdown terminal edge delivered once");
}
Console.WriteLine("PASS terminal edge rearmed across connection lifecycle");

// Blocking callers share the same establishment and may set SO_SNDTIMEO.
await using (var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), networkPolicy: policy))
{
    int fd = Ok(io.Socket());
    Ok(io.SetSocketTimeout(fd, false, new(1, 0)));
    var result = await io.ConnectAsync(fd, destination, deadline.Token);
    Check(result.Succeeded || result.Error == GuestError.InProgress, "blocking connect with timeout");
    using var peer = await listener.AcceptSocketAsync(deadline.Token);
    var poll = Ok(await io.PollAsync([new(fd, 4)], 5000, deadline.Token));
    Check(poll.Count == 1 && poll.Events[0] == 4 && Ok(io.GetSocketOption(fd, 1, 4)) == 0, "blocking/timed completion");
}
Console.WriteLine("PASS blocking/timed connect");

static T Ok<T>(HostResult<T> result)
{
    if (!result.Succeeded) throw new Exception($"Host operation: {result.Error}");
    return result.Value;
}
static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
