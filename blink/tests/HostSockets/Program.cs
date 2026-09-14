using System.Net;
using System.Net.Sockets;
using System.Text;
using Managed.Emulation.Host;

static T Ok<T>(HostResult<T> result)
{
    if (!result.Succeeded) throw new Exception("Unexpected error " + result.Error);
    return result.Value;
}
static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
static void Error<T>(GuestError expected, HostResult<T> result) => Check(result.Error == expected, $"Expected {expected}, got {result.Error}");
static int Listen(VirtualTcpNetwork host, ushort port)
{
    int listener = Ok(host.Create());
    Ok(host.SetOption(listener, TcpHostOption.ReuseAddress, 1));
    var local = Ok(host.Bind(listener, new(GuestEndpoint.Loopback, port)));
    Check(local.Port == port, "Guest port changed");
    Ok(host.Listen(listener, 8));
    return listener;
}
static async Task SendAll(VirtualTcpNetwork host, int handle, ReadOnlyMemory<byte> bytes, CancellationToken token = default)
{
    int offset = 0;
    while (offset < bytes.Length)
    {
        int count = Ok(await host.SendAsync(handle, bytes[offset..], token));
        Check(count > 0, "No send progress"); offset += count;
    }
}
static async Task Serve(VirtualTcpNetwork host, int listener, byte[] response)
{
    var accepted = Ok(await host.AcceptAsync(listener));
    Check(accepted.Remote.Address == GuestEndpoint.Loopback && accepted.Remote.Port != 0, "Missing virtual peer metadata");
    byte[] request = new byte[9]; int offset = 0;
    while (offset < request.Length)
    {
        Check(Ok(await host.WaitReadableAsync(accepted.Handle, TimeSpan.FromSeconds(2))), "No readable request");
        int count = Ok(await host.ReceiveAsync(accepted.Handle, request.AsMemory(offset, Math.Min(2, request.Length - offset))));
        Check(count > 0, "Early request EOF"); offset += count;
    }
    Check(Encoding.ASCII.GetString(request) == "request!!", "Request bytes changed");
    await SendAll(host, accepted.Handle, response);
    Ok(host.Shutdown(accepted.Handle, SocketShutdown.Send));
    Ok(host.Close(accepted.Handle));
}
static async Task Client(IPEndPoint endpoint, byte[] expected)
{
    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    await socket.ConnectAsync(endpoint);
    foreach (string fragment in new[] { "req", "ue", "st!!" })
    {
        byte[] bytes = Encoding.ASCII.GetBytes(fragment); int sent = 0;
        while (sent < bytes.Length) sent += await socket.SendAsync(bytes.AsMemory(sent), SocketFlags.None);
    }
    using var received = new MemoryStream(); byte[] buffer = new byte[997];
    for (;;)
    {
        int count = await socket.ReceiveAsync(buffer, SocketFlags.None);
        if (count == 0) break;
        received.Write(buffer, 0, count);
        Check(received.Length <= expected.Length, "Excess output");
    }
    Check(received.ToArray().AsSpan().SequenceEqual(expected), "Response bytes changed");
}

await using var first = new VirtualTcpNetwork(16);
await using var second = new VirtualTcpNetwork(16);
int l1 = Listen(first, 8080), l2 = Listen(second, 8080);
var p1 = Ok(first.Publish(l1)); var p2 = Ok(second.Publish(l2));
Check(p1.Port != p2.Port, "Instances share published endpoint");
Check(Ok(first.LocalEndpoint(l1)).Port == 8080, "Host endpoint leaked into guest metadata");
int duplicate = Ok(first.Create());
Error(GuestError.AddressInUse, first.Bind(duplicate, new(GuestEndpoint.Loopback, 8080)));
Error(GuestError.Access, await first.ConnectAsync(duplicate, new(0x08080808, 53)));
Error(GuestError.ConnectionRefused, await first.ConnectAsync(duplicate, new(GuestEndpoint.Loopback, 9999)));
Ok(first.Close(duplicate));
Check(!Ok(await first.WaitReadableAsync(l1, TimeSpan.FromMilliseconds(15))), "Idle listener reported ready");
byte[] alpha = Enumerable.Range(0, 131072).Select(i => (byte)('a' + i % 26)).ToArray();
byte[] bravo = Encoding.ASCII.GetBytes("second-instance\n");
await Task.WhenAll(Serve(first, l1, alpha), Serve(second, l2, bravo), Client(p1, alpha), Client(p2, bravo)).WaitAsync(TimeSpan.FromSeconds(10));
Console.WriteLine("private port reuse, publication, exact fragmented/large traffic: PASS");

using (var canceled = new CancellationTokenSource(30)) Error(GuestError.Canceled, await first.AcceptAsync(l1, canceled.Token));
int ownClient = Ok(first.Create());
var accepting = first.AcceptAsync(l1);
Ok(await first.ConnectAsync(ownClient, new(GuestEndpoint.Loopback, 8080)));
var ownServer = Ok(await accepting);
Error(GuestError.AlreadyConnected, await first.ConnectAsync(ownClient, new(GuestEndpoint.Loopback, 8080)));
Check(ownServer.Remote == Ok(first.LocalEndpoint(ownClient)), "Private virtual peer metadata mismatch");
Check(!Ok(await first.WaitReadableAsync(ownServer.Handle, TimeSpan.FromMilliseconds(10))), "Empty stream reported readable");
var blocked = first.ReceiveAsync(ownServer.Handle, new byte[1]);
Ok(first.Close(ownServer.Handle));
var closed = await blocked.WaitAsync(TimeSpan.FromSeconds(2));
Check(!closed.Succeeded && closed.Error is GuestError.BadDescriptor or GuestError.Canceled, "Close did not cancel blocked receive");
Ok(first.Close(ownClient));
Console.WriteLine("private connect, accept cancellation/reuse, blocked receive closure: PASS");

using (var stalledClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
{
    stalledClient.ReceiveBufferSize = 1024;
    var accept = first.AcceptAsync(l1);
    await stalledClient.ConnectAsync(p1);
    int server = Ok(await accept).Handle;
    Ok(first.SetOption(server, TcpHostOption.SendBufferSize, 1024));
    using var deadline = new CancellationTokenSource(100);
    byte[] payload = new byte[8 << 20]; int sent = 0; bool canceled = false;
    while (sent < payload.Length)
    {
        var result = await first.SendAsync(server, payload.AsMemory(sent), deadline.Token);
        if (!result.Succeeded) { Check(result.Error == GuestError.Canceled, "Backpressure cancellation failed"); canceled = true; break; }
        Check(result.Value > 0, "Send made no progress"); sent += result.Value;
    }
    Check(canceled && sent < payload.Length, "Expected bounded cancellation under backpressure");
    Ok(first.Close(server));
}
Console.WriteLine("backpressure and send cancellation: PASS");

var pendingAccept = first.AcceptAsync(l1);
await first.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(2));
Error(GuestError.Canceled, await pendingAccept);
Check(first.OpenDescriptors == 0 && first.PendingOperations == 0, "Shutdown leaked sockets/operations");
await first.DisposeAsync();
Error(GuestError.BadDescriptor, first.Create());
await Task.WhenAll(Serve(second, l2, bravo), Client(p2, bravo)).WaitAsync(TimeSpan.FromSeconds(5));
await using var limited = new VirtualTcpNetwork(1);
int only = Ok(limited.Create()); Error(GuestError.TooManyFiles, limited.Create()); Ok(limited.Close(only));
Check(Ok(limited.Create()) == only, "Descriptor not reusable");
Console.WriteLine("bounded disposal drains operations, isolation and descriptor limits: PASS");
