using System.Net;
using System.Net.Sockets;
using Managed.Emulation.Host;

static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static T Good<T>(HostResult<T> result) { if (!result.Succeeded) throw new InvalidOperationException(result.Error.ToString()); return result.Value; }
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
var token = timeout.Token;
await using (var isolated = new VirtualTcpNetwork(policy: GuestNetworkPolicy.Isolated))
{
    Check(isolated.Create().Error == GuestError.Access && isolated.OpenDescriptors == 0, "isolated create");
}
var incoming = new[] { new GuestPortGrant(8080) };
var policy = new GuestNetworkPolicy(incoming);
incoming[0] = new(9000);
await using (var network = new VirtualTcpNetwork(policy: policy))
{
    int listener = Good(network.Create());
    Check(network.Bind(listener, new(GuestEndpoint.Loopback, 9000)).Error == GuestError.Access, "publication deny");
    Check(Good(network.Bind(listener, new(GuestEndpoint.Loopback, 8080))).Port == 8080, "guest port");
    Good(network.Listen(listener, 4));
    var endpoint = Good(network.Publish(listener));
    Check(endpoint.Address.Equals(IPAddress.Loopback) && endpoint.Port > 0, "actual publication");
    using var peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    await peer.ConnectAsync(endpoint, token);
    int accepted = Good(await network.AcceptAsync(listener, token)).Handle;
    await peer.SendAsync(new byte[] { 17, 29, 41 }, SocketFlags.None, token);
    var bytes = new byte[3];
    Check(Good(await network.ReceiveAsync(accepted, bytes, token)) == 3 && bytes.SequenceEqual(new byte[] { 17, 29, 41 }), "inbound data");
    Good(await network.SendAsync(accepted, new byte[] { 59 }, token));
    Check(await peer.ReceiveAsync(bytes, SocketFlags.None, token) == 1 && bytes[0] == 59, "outbound reply");
    Good(network.Close(accepted)); Good(network.Close(listener));
    Check(network.OpenDescriptors == 0 && network.PendingOperations == 0, "publication cleanup");
}
using var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Loopback, 0)); server.Listen(4);
ushort granted = checked((ushort)((IPEndPoint)server.LocalEndPoint!).Port);
await using (var network = new VirtualTcpNetwork(policy: new(outbound: new[] { new GuestOutboundGrant("127.0.0.1", granted) })))
{
    int socket = Good(network.Create());
    ushort denied = granted == 65535 ? (ushort)65534 : (ushort)(granted + 1);
    Check((await network.ConnectAsync(socket, new(GuestEndpoint.Loopback, denied), token)).Error == GuestError.Access, "outbound deny");
    Check(network.Bind(socket, new(GuestEndpoint.Loopback, 8080)).Error == GuestError.Access, "outbound is not publication");
    Good(await network.ConnectAsync(socket, new(GuestEndpoint.Loopback, granted), token));
    using var peer = await server.AcceptAsync(token);
    Good(await network.SendAsync(socket, new byte[] { 73 }, token));
    var bytes = new byte[1];
    Check(await peer.ReceiveAsync(bytes, SocketFlags.None, token) == 1 && bytes[0] == 73, "granted destination");
    Check(network.Listen(socket, 4).Error == GuestError.Access, "source not listener");
    Good(network.Close(socket));
    Check(network.OpenDescriptors == 0 && network.PendingOperations == 0, "outbound cleanup");
}
Console.WriteLine("network-grants isolated publication outbound bytes cleanup PASS");
