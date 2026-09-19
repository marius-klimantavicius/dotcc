using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

static void Check(bool condition, string message) {
    if (!condition) throw new InvalidOperationException(message);
}
static async Task<int> Peer(IPEndPoint endpoint, TaskCompletionSource ready) {
    try {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(endpoint);
        int port = ((IPEndPoint)socket.LocalEndPoint!).Port;
        byte[] bytes = new byte[263];
        for (int i = 0; i < 257; ++i) bytes[i] = (byte)(i * 17 + 3);
        int total = 0;
        while (total < 257) {
            int count = await socket.SendAsync(bytes.AsMemory(total, 257 - total), SocketFlags.None);
            Check(count > 0 && count <= 257 - total, "peer positive send prefix"); total += count;
        }
        socket.Shutdown(SocketShutdown.Send); ready.SetResult();
        total = 0;
        while (total < 263) {
            int count = await socket.ReceiveAsync(bytes.AsMemory(total, 263 - total), SocketFlags.None);
            Check(count > 0 && count <= 263 - total, "peer positive receive prefix"); total += count;
        }
        for (int i = 0; i < 263; ++i) Check(bytes[i] == (byte)(i * 29 + 11), "peer exact response");
        Check(await socket.ReceiveAsync(bytes.AsMemory(0, 1), SocketFlags.None) == 0, "peer orderly EOF");
        return port;
    } catch (Exception error) { ready.TrySetException(error); throw; }
}
if (args.Length != 1) return 2;
using var variables = new HostVariables();
using var sleep = new HostSleep();
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
var directories = new HostDirectories(io);
try {
    Blink.BindHostIo(io); Blink.BindHostDirectories(directories);
    Blink.BindHostVariables(variables); Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment()); Blink.BindHostIdentity(new HostIdentity());
    GC.Collect(2, GCCollectionMode.Forced, true, true);
    int result = Blink.GuestTcpSetup(); if (result != 0) return result;
    var endpoint = io.Publish(Blink.GuestTcpListener());
    Check(endpoint.Succeeded && IPAddress.IsLoopback(endpoint.Value.Address), "explicit loopback publication");
    var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    Task<int> peer = Peer(endpoint.Value, ready);
    // Only the peer is asynchronous. All translated calls and bindings remain
    // on this original thread; no await continuation can move the guest owner.
    ready.Task.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
    GC.Collect(2, GCCollectionMode.Forced, true, true);
    result = Blink.GuestTcpExchange(); if (result != 0) return result;
    int physicalPeer = peer.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
    Check(io.OpenDescriptors == 3 && io.PendingSocketOperations == 0, "guest TCP handles and operations drained");
    File.WriteAllText(args[0], FormattableString.Invariant(
        $"{{\"kind\":\"managed\",\"guest_port\":{Blink.GuestTcpPort()},\"guest_peer_port\":{Blink.GuestTcpPeerPort()},\"physical_port\":{endpoint.Value.Port},\"physical_peer_port\":{physicalPeer},\"peer_response_bytes\":263,\"peer_exact\":true,\"peer_eof\":true,\"host_descriptors\":{io.OpenDescriptors},\"pending_socket_operations\":{io.PendingSocketOperations}}}\n"));
    Console.WriteLine("tcp peer response_bytes=263 exact=1 eof=1 joined=1");
    return 0;
} finally {
    try { Blink.GuestTcpDestroy(); }
    finally {
        try { Check(Blink.GuestTcpRelease() == 0, "guest memory owner cleanup"); }
        finally {
            Blink.UnbindHostIdentity(); Blink.UnbindHostEnvironment();
            Blink.UnbindHostDirectories(); directories.Dispose(); Blink.UnbindHostIo();
            Blink.UnbindHostVariables(); Blink.UnbindHostSleep();
            io.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
