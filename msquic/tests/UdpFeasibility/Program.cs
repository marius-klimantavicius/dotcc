using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

// Standalone feasibility tests for BCL Socket services, not a QUIC datapath.
var results = new List<ProbeCase>();
await Run("ipv4-packet-info", () => PacketInfo(AddressFamily.InterNetwork));
await Run("ipv6-packet-info", () => PacketInfo(AddressFamily.InterNetworkV6));
await Run("ipv6-link-local-scope", LinkLocalScope);
await Run("dual-stack-packet-info", DualStack);
await Run("bound-source-address", BoundSource);
await Run("ipv4-datagram-boundaries", () => Boundaries(AddressFamily.InterNetwork));
await Run("ipv6-datagram-boundaries", () => Boundaries(AddressFamily.InterNetworkV6));
await Run("ipv4-truncation", () => Truncation(AddressFamily.InterNetwork));
await Run("ipv6-truncation", () => Truncation(AddressFamily.InterNetworkV6));
await Run("pending-receive-forced-gc", PendingReceive);
await Run("queued-receive-completion", QueuedReceive);
await Run("cancel-dispose-races", CancelDisposeRaces);
await Run("ipv4-oversized-send", OversizedSend);
await Run("connected-unreachable", ConnectedUnreachable);
var receipt = new ProbeReceipt(
    DateTimeOffset.UtcNow,
    RuntimeInformation.FrameworkDescription,
    RuntimeInformation.OSDescription,
    RuntimeInformation.ProcessArchitecture.ToString(),
    RuntimeFeature.IsDynamicCodeSupported ? "jit" : "nativeaot",
    results.All(result => result.Passed),
    results,
    ["ECN receive bits are not exposed by SocketReceiveMessageFromResult.",
     "Wildcard source-address selection requires a separately bound socket or another design; SendToAsync has no per-packet source selector.",
     "Local-address tests do not qualify real-path PMTU discovery, remote link-local peers, or non-Linux runners.",
     "No translated MsQuic, TLS, worker scheduling, or QUIC packet processing is exercised here."]);
var serialized = JsonSerializer.Serialize(receipt, ProbeJsonContext.Default.ProbeReceipt);
if (args.Length == 1)
{
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0]))!);
    File.WriteAllText(args[0], serialized + Environment.NewLine);
}
Console.WriteLine(serialized);
return receipt.Passed ? 0 : 1;

async Task Run(string name, Func<Task<Dictionary<string, string>>> test)
{
    var start = Stopwatch.GetTimestamp();
    try
    {
        var observations = await test().WaitAsync(TimeSpan.FromSeconds(25));
        results.Add(new(name, true, Stopwatch.GetElapsedTime(start).TotalMilliseconds, observations, null));
        Console.Error.WriteLine($"PASS {name}");
    }
    catch (Exception error)
    {
        results.Add(new(name, false, Stopwatch.GetElapsedTime(start).TotalMilliseconds, [], error.ToString()));
        Console.Error.WriteLine($"FAIL {name}: {error.Message}");
    }
}

static Socket Listener(AddressFamily family, bool dual = false)
{
    var socket = new Socket(family, SocketType.Dgram, ProtocolType.Udp);
    if (family == AddressFamily.InterNetworkV6) socket.DualMode = dual;
    socket.SetSocketOption(family == AddressFamily.InterNetwork ? SocketOptionLevel.IP : SocketOptionLevel.IPv6,
                           SocketOptionName.PacketInformation, true);
    if (dual) socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
    socket.Bind(new IPEndPoint(family == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0));
    return socket;
}

static IPEndPoint Destination(Socket receiver, IPAddress? address = null) =>
    new(address ?? (receiver.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Loopback : IPAddress.IPv6Loopback),
        ((IPEndPoint)receiver.LocalEndPoint!).Port);

static async Task<SocketReceiveMessageFromResult> Receive(Socket socket, Memory<byte> memory, CancellationToken token = default)
{
    using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
    deadline.CancelAfter(TimeSpan.FromSeconds(3));
    EndPoint remote = new IPEndPoint(socket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
    return await socket.ReceiveMessageFromAsync(memory, SocketFlags.None, remote, deadline.Token);
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static IPAddress Normalize(IPAddress address) => address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

static async Task<Dictionary<string, string>> PacketInfo(AddressFamily family)
{
    using var receiver = Listener(family);
    using var sender = new Socket(family, SocketType.Dgram, ProtocolType.Udp);
    byte[] payload = [1, 2, 3, 4];
    var target = Destination(receiver);
    Require(await sender.SendToAsync(payload, SocketFlags.None, target) == payload.Length, "send length");
    byte[] buffer = new byte[64];
    var result = await Receive(receiver, buffer);
    Require(result.ReceivedBytes == payload.Length && buffer.AsSpan(0, payload.Length).SequenceEqual(payload), "payload");
    Require(Normalize(result.PacketInformation.Address).Equals(target.Address), "destination address metadata");
    Require(result.PacketInformation.Interface > 0, "missing interface index");
    Require(((IPEndPoint)result.RemoteEndPoint).Port == ((IPEndPoint)sender.LocalEndPoint!).Port, "remote port");
    var info = new Dictionary<string, string>
    {
        ["destination"] = result.PacketInformation.Address.ToString(),
        ["interface"] = result.PacketInformation.Interface.ToString(),
        ["remote"] = result.RemoteEndPoint.ToString()!,
        ["flags"] = result.SocketFlags.ToString()
    };
    if (family == AddressFamily.InterNetwork)
    {
        target = Destination(receiver, IPAddress.Parse("127.0.0.2"));
        await sender.SendToAsync(payload, SocketFlags.None, target);
        result = await Receive(receiver, buffer);
        Require(result.PacketInformation.Address.Equals(target.Address), "wildcard listener lost alternate destination");
        info["alternate_destination"] = result.PacketInformation.Address.ToString();
    }
    else
    {
        info["loopback_scope"] = ((IPEndPoint)result.RemoteEndPoint).Address.ScopeId.ToString();
        info["configured_link_local_addresses"] = string.Join(",", NetworkInterface.GetAllNetworkInterfaces()
            .SelectMany(n => n.GetIPProperties().UnicastAddresses)
            .Select(a => a.Address).Where(a => a.IsIPv6LinkLocal).Select(a => a.ToString()));
    }
    return info;
}

static async Task<Dictionary<string, string>> DualStack()
{
    using var receiver = Listener(AddressFamily.InterNetworkV6, dual: true);
    byte[] buffer = new byte[32];
    var result = new Dictionary<string, string>();
    foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
    {
        using var sender = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        await sender.SendToAsync(new byte[] { 17 }, SocketFlags.None, Destination(receiver, address));
        var received = await Receive(receiver, buffer);
        Require(received.ReceivedBytes == 1 && buffer[0] == 17, "dual-stack payload");
        Require(Normalize(received.PacketInformation.Address).Equals(address), "dual-stack destination");
        Require(received.PacketInformation.Interface > 0, "dual-stack interface");
        result[address.AddressFamily.ToString()] = $"destination={received.PacketInformation.Address}; remote={received.RemoteEndPoint}; interface={received.PacketInformation.Interface}";
    }
    return result;
}

static async Task<Dictionary<string, string>> BoundSource()
{
    using var receiver = Listener(AddressFamily.InterNetwork);
    using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    var source = IPAddress.Parse("127.0.0.2");
    sender.Bind(new IPEndPoint(source, 0));
    await sender.SendToAsync(new byte[] { 31 }, SocketFlags.None, Destination(receiver));
    var result = await Receive(receiver, new byte[32]);
    Require(((IPEndPoint)result.RemoteEndPoint).Address.Equals(source), "bound source address changed");
    return new() { ["source"] = result.RemoteEndPoint.ToString()! };
}

static async Task<Dictionary<string, string>> LinkLocalScope()
{
    IPAddress? address = NetworkInterface.GetAllNetworkInterfaces()
        .SelectMany(n => n.GetIPProperties().UnicastAddresses).Select(a => a.Address)
        .FirstOrDefault(a => a.IsIPv6LinkLocal && a.ScopeId > 0);
    Require(address != null, "No scoped link-local IPv6 address exists on this runner; case is unverified");
    using var receiver = Listener(AddressFamily.InterNetworkV6);
    using var sender = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
    sender.Bind(new IPEndPoint(address!, 0));
    await sender.SendToAsync(new byte[] { 0x35 }, SocketFlags.None, Destination(receiver, address));
    byte[] buffer = new byte[32];
    var result = await Receive(receiver, buffer);
    Require(result.ReceivedBytes == 1 && buffer[0] == 0x35, "scoped IPv6 payload");
    Require(result.PacketInformation.Address.GetAddressBytes().AsSpan().SequenceEqual(address!.GetAddressBytes()),
            "scoped IPv6 destination address");
    Require(result.PacketInformation.Interface == address.ScopeId, "scoped IPv6 interface");
    Require(((IPEndPoint)result.RemoteEndPoint).Address.ScopeId == address.ScopeId, "scoped IPv6 remote scope");
    return new() { ["destination"] = address.ToString(), ["interface"] = result.PacketInformation.Interface.ToString(),
                  ["remote"] = result.RemoteEndPoint.ToString()! };
}

static async Task<Dictionary<string, string>> Boundaries(AddressFamily family)
{
    using var receiver = Listener(family);
    using var sender = new Socket(family, SocketType.Dgram, ProtocolType.Udp);
    int[] sizes = [0, 1, 1200, 1472, 4000, 65507];
    byte[] buffer = new byte[65535];
    foreach (int size in sizes)
    {
        byte[] payload = Enumerable.Range(0, size).Select(i => (byte)(i % 251)).ToArray();
        Require(await sender.SendToAsync(payload, SocketFlags.None, Destination(receiver)) == size, "datagram send count");
        var result = await Receive(receiver, buffer);
        Require(result.ReceivedBytes == size, $"datagram boundary: {result.ReceivedBytes} != {size}");
        Require(buffer.AsSpan(0, size).SequenceEqual(payload), "datagram content");
        Require((result.SocketFlags & SocketFlags.Truncated) == 0, "unexpected truncation");
    }
    return new() { ["sizes"] = string.Join(",", sizes) };
}

static async Task<Dictionary<string, string>> Truncation(AddressFamily family)
{
    using var receiver = Listener(family);
    using var sender = new Socket(family, SocketType.Dgram, ProtocolType.Udp);
    await sender.SendToAsync(new byte[2048], SocketFlags.None, Destination(receiver));
    string indication;
    try
    {
        var result = await Receive(receiver, new byte[64]);
        Require(result.ReceivedBytes == 64 && (result.SocketFlags & SocketFlags.Truncated) != 0, "truncation not reported");
        indication = $"bytes={result.ReceivedBytes}; flags={result.SocketFlags}";
    }
    catch (SocketException error) when (error.SocketErrorCode == SocketError.MessageSize)
    {
        indication = error.SocketErrorCode.ToString();
    }
    await sender.SendToAsync(new byte[] { 0xa7 }, SocketFlags.None, Destination(receiver));
    byte[] next = new byte[64];
    var received = await Receive(receiver, next);
    Require(received.ReceivedBytes == 1 && next[0] == 0xa7, "truncated datagram remainder leaked into next receive");
    return new() { ["indication"] = indication, ["next_datagram"] = "intact" };
}

static async Task<Dictionary<string, string>> PendingReceive()
{
    using var receiver = Listener(AddressFamily.InterNetworkV6);
    using var sender = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
    using var owner = new NativeBuffer(2048);
    var pending = Receive(receiver, owner.Memory);
    Require(!pending.IsCompleted, "receive should initially be pending");
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    byte[] payload = Enumerable.Repeat((byte)0x5c, 1200).ToArray();
    await sender.SendToAsync(payload, SocketFlags.None, Destination(receiver));
    var result = await pending;
    Require(result.ReceivedBytes == payload.Length && owner.GetSpan()[..payload.Length].SequenceEqual(payload), "pending native buffer corrupted across GC");
    Require(owner.Pins == 0, "socket retained pin after receive completion");
    return new() { ["max_pins"] = owner.MaxPins.ToString(), ["final_pins"] = owner.Pins.ToString() };
}

static async Task<Dictionary<string, string>> QueuedReceive()
{
    using var receiver = Listener(AddressFamily.InterNetwork);
    using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    using var owner = new NativeBuffer(64);
    await sender.SendToAsync(new byte[] { 0x71 }, SocketFlags.None, Destination(receiver));
    Require(receiver.Poll(1_000_000, SelectMode.SelectRead), "queued receive never readable");
    using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(3));
    var pending = receiver.ReceiveMessageFromAsync(owner.Memory, SocketFlags.None,
        new IPEndPoint(IPAddress.Any, 0), cancellation.Token);
    bool synchronous = pending.IsCompletedSuccessfully;
    var result = await pending;
    Require(result.ReceivedBytes == 1 && owner.GetSpan()[0] == 0x71, "queued completion payload");
    Require(owner.Pins == 0, "queued completion retained pin");
    return new() { ["completed_synchronously"] = synchronous.ToString(), ["final_pins"] = owner.Pins.ToString() };
}

static async Task<Dictionary<string, string>> CancelDisposeRaces()
{
    const int iterations = 64;
    int completed = 0, canceled = 0, disposed = 0, maxPins = 0;
    for (int i = 0; i < iterations; ++i)
    {
        using var receiver = Listener(AddressFamily.InterNetwork);
        using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        using var owner = new NativeBuffer(128);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var destination = Destination(receiver);
        var pending = receiver.ReceiveMessageFromAsync(owner.Memory, SocketFlags.None,
            new IPEndPoint(IPAddress.Any, 0), cancellation.Token).AsTask();
        Require(!pending.IsCompleted, "race receive unexpectedly completed before trigger");
        Task<int>? send = i % 3 == 0 ? sender.SendToAsync(new byte[] { 0x61 }, SocketFlags.None, destination) : null;
        if (i % 2 == 0) cancellation.Cancel(); else receiver.Dispose();
        try
        {
            var result = await pending;
            Require(send != null && result.ReceivedBytes == 1 && owner.GetSpan()[0] == 0x61, "unexpected race completion");
            completed++;
        }
        catch (OperationCanceledException) { canceled++; }
        catch (ObjectDisposedException) { disposed++; }
        catch (SocketException error) when (error.SocketErrorCode is SocketError.OperationAborted or SocketError.Interrupted)
        { disposed++; }
        if (send != null) await send;
        receiver.Dispose();
        Require(owner.Pins == 0, "pin remains after pending operation drained");
        maxPins = Math.Max(maxPins, owner.MaxPins);
        owner.GetSpan().Fill(0xee);
        await Task.Delay(1);
        Require(owner.GetSpan().IndexOfAnyExcept((byte)0xee) < 0, "buffer modified after completion/drain");
    }
    Require(completed + canceled + disposed == iterations, "lost completion");
    return new() { ["iterations"] = iterations.ToString(), ["received"] = completed.ToString(),
        ["canceled"] = canceled.ToString(), ["closed"] = disposed.ToString(), ["max_pins"] = maxPins.ToString() };
}

static async Task<Dictionary<string, string>> OversizedSend()
{
    using var receiver = Listener(AddressFamily.InterNetwork);
    using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    sender.DontFragment = true;
    try
    {
        await sender.SendToAsync(new byte[65508], SocketFlags.None, Destination(receiver));
        throw new InvalidOperationException("Oversized IPv4 UDP payload unexpectedly sent");
    }
    catch (SocketException error) when (error.SocketErrorCode == SocketError.MessageSize)
    {
        return new() { ["error"] = error.SocketErrorCode.ToString(), ["dont_fragment"] = sender.DontFragment.ToString() };
    }
}

static async Task<Dictionary<string, string>> ConnectedUnreachable()
{
    // Reserve and release a loopback port, then observe the kernel's ICMP error.
    // There remains a tiny port-reuse race; this is evidence, not a unit test.
    var temporary = Listener(AddressFamily.InterNetwork);
    var destination = Destination(temporary);
    temporary.Dispose();
    using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    sender.Connect(destination);
    using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(2));
    try
    {
        await sender.SendAsync(new byte[] { 1 }, SocketFlags.None, cancellation.Token);
        await sender.ReceiveAsync(new byte[16], SocketFlags.None, cancellation.Token);
        throw new InvalidOperationException("Unexpected datagram at closed loopback port");
    }
    catch (SocketException error) when (error.SocketErrorCode == SocketError.ConnectionRefused)
    {
        return new() { ["error"] = error.SocketErrorCode.ToString() };
    }
}

sealed unsafe class NativeBuffer : MemoryManager<byte>
{
    private byte* buffer;
    private readonly int length;
    private int pins;
    public int Pins => Volatile.Read(ref pins);
    public int MaxPins { get; private set; }
    public NativeBuffer(int length)
    {
        this.length = length;
        buffer = (byte*)NativeMemory.AllocZeroed((nuint)length);
    }
    public override Span<byte> GetSpan()
    {
        ObjectDisposedException.ThrowIf(buffer == null, this);
        return new Span<byte>(buffer, length);
    }
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        ObjectDisposedException.ThrowIf(buffer == null, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)elementIndex, (uint)length);
        MaxPins = Math.Max(MaxPins, Interlocked.Increment(ref pins));
        return new MemoryHandle(buffer + elementIndex, default, this);
    }
    public override void Unpin() => Interlocked.Decrement(ref pins);
    protected override void Dispose(bool disposing)
    {
        if (Pins != 0) throw new InvalidOperationException("Attempted to free an in-flight socket buffer");
        NativeMemory.Free(buffer);
        buffer = null;
    }
}

record ProbeCase(string Name, bool Passed, double Milliseconds, Dictionary<string, string> Observations, string? Error);
record ProbeReceipt(DateTimeOffset Time, string Framework, string OS, string Architecture, string Runtime,
    bool Passed, List<ProbeCase> Cases, List<string> Limits);
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ProbeReceipt))]
partial class ProbeJsonContext : JsonSerializerContext;
