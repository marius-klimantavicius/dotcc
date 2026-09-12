using System;
using System.Collections.Concurrent;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Managed.Transport;

namespace Managed.Transport.Hosting;

public sealed unsafe partial class MsQuicHost
{
    private sealed class DatagramPath : IDisposable
    {
        internal readonly MsQuicHost Host;
        internal readonly CXPLAT_UDP_DATAPATH_CALLBACKS Callbacks;
        internal readonly CXPLAT_WORKER_POOL* Workers;
        internal readonly int ClientLength;
        private readonly object gate = new();
        private readonly Stack<DatagramReceive> pool = new();
        private int sockets, closingSockets, rented;
        private bool closing, disposed;
        internal DatagramPath(MsQuicHost host, CXPLAT_UDP_DATAPATH_CALLBACKS callbacks, CXPLAT_WORKER_POOL* workers, int clientLength)
        {
            Host = host; Callbacks = callbacks; Workers = workers; ClientLength = clientLength;
            if (MsQuic.CxPlatWorkerPoolAddRef(workers, CXPLAT_WORKER_POOL_REF.CXPLAT_WORKER_POOL_REF_EXTERNAL) == 0)
                throw new ObjectDisposedException("worker pool");
        }
        internal void AddSocket() { lock (gate) { if (closing) throw new ObjectDisposedException("datapath"); sockets++; } }
        internal void BeginSocketClose() { lock (gate) closingSockets++; }
        internal void RemoveSocket() { lock (gate) { closingSockets--; if (--sockets < 0) FatalInvariant("Negative socket ownership count."); Monitor.PulseAll(gate); } }
        internal DatagramReceive Rent()
        {
            lock (gate)
            {
                if (closing) throw new ObjectDisposedException("datapath");
                var item = pool.Count != 0 ? pool.Pop() : new DatagramReceive(this);
                rented++;
                new Span<byte>(item.Data, sizeof(CXPLAT_RECV_DATA) + ClientLength).Clear();
                *item.Route = default;
                item.Data->Route = item.Route; item.Data->Buffer = item.Payload; item.Data->Allocated = 1; item.Data->DatapathType = 1;
                return item;
            }
        }
        internal void Return(DatagramReceive packet)
        {
            lock (gate)
            {
                packet.Data->Allocated = 0;
                if (closing || pool.Count >= 64) packet.Free(); else pool.Push(packet);
                if (--rented < 0) FatalInvariant("Receive buffer returned twice.");
                Monitor.PulseAll(gate);
            }
        }
        public void Dispose()
        {
            lock (gate)
            {
                if (disposed) return;
                if (sockets != closingSockets) FatalInvariant("Datapath destroyed with live sockets.");
                closing = true;
                while (sockets != 0 || rented != 0) Monitor.Wait(gate);
                while (pool.Count != 0) pool.Pop().Free();
                disposed = true;
            }
            MsQuic.CxPlatWorkerPoolRelease(Workers, CXPLAT_WORKER_POOL_REF.CXPLAT_WORKER_POOL_REF_EXTERNAL);
        }
    }

    private sealed class DatagramReceive
    {
        internal readonly DatagramPath Path;
        internal readonly CXPLAT_RECV_DATA* Data;
        internal readonly CXPLAT_ROUTE* Route;
        internal readonly byte* Payload;
        internal readonly DatagramMemory Memory;
        internal DatagramReceive(DatagramPath path)
        {
            Path = path;
            int routeOffset = checked((sizeof(CXPLAT_RECV_DATA) + path.ClientLength + 15) & ~15);
            int bytes = checked(routeOffset + sizeof(CXPLAT_ROUTE) + DatagramCapacity);
            Data = (CXPLAT_RECV_DATA*)path.Host.AllocatePlatformMemory((ulong)bytes, DatagramAllocationTag);
            if (Data == null) throw new OutOfMemoryException();
            Route = (CXPLAT_ROUTE*)((byte*)Data + routeOffset);
            Payload = (byte*)Route + sizeof(CXPLAT_ROUTE);
            try { Memory = new DatagramMemory((nint)Payload, DatagramCapacity); }
            catch { path.Host.FreePlatformMemory(Data, DatagramAllocationTag); throw; }
        }
        internal void Free() => Path.Host.FreePlatformMemory(Data, DatagramAllocationTag);
    }

    private struct DatagramNotification
    {
        internal CXPLAT_SQE Sqe;
        internal void* Context;
        internal void* Socket;
    }
    private sealed class DatagramPhysical
    {
        internal readonly Socket Socket;
        internal readonly Thread Thread;
        internal bool Started;
        internal DatagramPhysical(DatagramSocket owner, Socket socket)
        {
            Socket = socket;
            Thread = new Thread(() => owner.Receive(this)) { IsBackground = true, Name = "MsQuic UDP receive" };
        }
    }
    private sealed class DatagramSocket : IDisposable
    {
        internal readonly DatagramPath Path;
        internal readonly IPEndPoint Local;
        internal readonly IPEndPoint? Remote;
        internal readonly ushort Partition;
        internal readonly uint InterfaceIndex;
        internal readonly void* CallbackContext;
        internal void* Token;
        internal readonly object gate = new();
        internal readonly List<DatagramPhysical> physical = new();
        private readonly Queue<DatagramReceive> received = new(256);
        private readonly Queue<IPEndPoint> unreachable = new(64);
        private readonly CXPLAT_EVENTQ* queue;
        private DatagramNotification* notification;
        internal CXPLAT_SQE* NotificationSqe => &notification->Sqe;
        private int activeSends, callbackThread;
        private bool closing, disposed, counted, registered;

        internal DatagramSocket(DatagramPath path, CXPLAT_UDP_CONFIG config)
        {
            Path = path; Partition = config.PartitionIndex; InterfaceIndex = config.InterfaceIndex; CallbackContext = config.CallbackContext;
            if (Partition >= MsQuic.CxPlatWorkerPoolGetCount(path.Workers)) throw new ArgumentException("Invalid worker partition.");
            Remote = config.RemoteAddress == null ? null : DatagramEndpoint(config.RemoteAddress);
            if (Remote != null && IsWildcard(Remote.Address)) throw new ArgumentException("Remote endpoint must not be wildcard.");
            var local = config.LocalAddress == null || config.LocalAddress->Ip.sa_family == 0
                ? new IPEndPoint(Remote?.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0)
                : DatagramEndpoint(config.LocalAddress);
            queue = MsQuic.CxPlatWorkerPoolGetEventQ(path.Workers, Partition);
            Socket? socket = null;
            try
            {
                socket = CreateSocket(local);
                if (Remote != null) socket.Connect(MapEndpoint(Remote, socket));
                Local = Normalize((IPEndPoint)socket.LocalEndPoint!);
                physical.Add(new DatagramPhysical(this, socket));
                path.AddSocket(); counted = true;
            }
            catch { socket?.Dispose(); throw; }
        }
        internal void Start()
        {
            notification = (DatagramNotification*)Path.Host.AllocatePlatformMemory((ulong)sizeof(DatagramNotification), DatagramAllocationTag);
            if (notification == null) throw new OutOfMemoryException();
            notification->Context = Path.Host.ContextPointer; notification->Socket = Token;
            if (PlatformSqeInitialize(Path.Host.ContextPointer, queue, &DatagramDispatch, &notification->Sqe) == 0) throw new OutOfMemoryException();
            registered = true;
            foreach (var entry in physical.ToArray()) { entry.Thread.Start(); entry.Started = true; }
        }
        private Socket CreateSocket(IPEndPoint local)
        {
            var socket = new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                if (socket.AddressFamily == AddressFamily.InterNetworkV6) socket.DualMode = local.Address.Equals(IPAddress.IPv6Any);
                socket.SetSocketOption(socket.AddressFamily == AddressFamily.InterNetwork ? SocketOptionLevel.IP : SocketOptionLevel.IPv6, SocketOptionName.PacketInformation, true);
                if (socket.AddressFamily == AddressFamily.InterNetworkV6 && socket.DualMode) socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                if (socket.AddressFamily == AddressFamily.InterNetwork) socket.DontFragment = true;
                ApplyDatagramInterface(socket, InterfaceIndex);
                socket.Bind(local);
                return socket;
            }
            catch { socket.Dispose(); throw; }
        }
        internal Socket SelectSocket(IPEndPoint local)
        {
            lock (gate)
            {
                if (closing) throw new ObjectDisposedException("UDP socket");
                if (local.Port != 0 && local.Port != Local.Port) throw new ArgumentException("Route changed binding port.");
                if (!IsWildcard(Local.Address))
                {
                    if (!IsWildcard(local.Address) && !local.Address.Equals(Local.Address)) throw new ArgumentException("Route changed explicitly bound source.");
                    return physical[0].Socket;
                }
                if (IsWildcard(local.Address)) throw new ArgumentException("An outbound route needs a concrete source address.");
                foreach (var item in physical)
                    if (Normalize((IPEndPoint)item.Socket.LocalEndPoint!).Address.Equals(local.Address)) return item.Socket;
                var socket = CreateSocket(new IPEndPoint(local.Address, Local.Port));
                try
                {
                    var entry = new DatagramPhysical(this, socket);
                    physical.Add(entry);
                    entry.Thread.Start(); entry.Started = true;
                    return socket;
                }
                catch { physical.RemoveAll(item => ReferenceEquals(item.Socket, socket)); socket.Dispose(); throw; }
            }
        }
        private static IPEndPoint Normalize(IPEndPoint endpoint) => endpoint.Address.IsIPv4MappedToIPv6 ? new(endpoint.Address.MapToIPv4(), endpoint.Port) : endpoint;
        internal static IPEndPoint MapEndpoint(IPEndPoint endpoint, Socket socket) => socket.AddressFamily == AddressFamily.InterNetworkV6 && endpoint.AddressFamily == AddressFamily.InterNetwork ? new(endpoint.Address.MapToIPv6(), endpoint.Port) : endpoint;
        internal void Receive(DatagramPhysical entry)
        {
            try
            {
                while (true)
                {
                    lock (gate) { if (closing) return; }
                    DatagramReceive? packet = null;
                    try
                    {
                        packet = Path.Rent();
                        EndPoint peer = new IPEndPoint(entry.Socket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                        var result = entry.Socket.ReceiveMessageFromAsync(packet.Memory.Memory, SocketFlags.None, peer).AsTask().GetAwaiter().GetResult();
                        int length = result.ReceivedBytes;
                        var flags = result.SocketFlags;
                        var info = result.PacketInformation;
                        peer = result.RemoteEndPoint;
                        if ((flags & SocketFlags.Truncated) != 0)
                        { Interlocked.Increment(ref Path.Host.datagramTruncations); Path.Return(packet); packet = null; continue; }
                        var remote = Normalize((IPEndPoint)peer);
                        var address = info.Address.IsIPv4MappedToIPv6 ? info.Address.MapToIPv4() : info.Address;
                        if (info.Interface <= 0 || IsWildcard(address)) { Interlocked.Increment(ref Path.Host.datagramReceiveErrors); Path.Return(packet); packet = null; continue; }
                        if (address.AddressFamily == AddressFamily.InterNetworkV6 && address.IsIPv6LinkLocal && address.ScopeId == 0)
                            address = new IPAddress(address.GetAddressBytes(), info.Interface);
                        DatagramAddress(remote, &packet.Route->RemoteAddress);
                        DatagramAddress(new IPEndPoint(address, Local.Port), &packet.Route->LocalAddress);
                        packet.Route->LocalAddress.Ipv6.sin6_scope_id = checked((uint)info.Interface);
                        packet.Route->State = CXPLAT_ROUTE_STATE.RouteResolved; packet.Route->DatapathType = 1;
                        packet.Data->BufferLength = checked((ushort)length); packet.Data->PartitionIndex = Partition;
                        lock (gate)
                        {
                            if (closing) { Path.Return(packet); packet = null; return; }
                            if (received.Count == 256) { Interlocked.Increment(ref Path.Host.datagramReceiveErrors); Path.Return(packet); packet = null; continue; }
                            if (!Path.Host.datagramReceives.TryAdd((nint)packet.Data, packet)) FatalInvariant("Receive pointer already leased.");
                            received.Enqueue(packet); packet = null;
                            if (PlatformQueueEnqueue(Path.Host.ContextPointer, queue, &notification->Sqe) == 0) FatalInvariant("Live datapath notification rejected.");
                        }
                    }
                    catch (SocketException error)
                    {
                        if (packet != null) Path.Return(packet);
                        lock (gate) { if (closing) return; }
                        if (error.SocketErrorCode == SocketError.MessageSize) Interlocked.Increment(ref Path.Host.datagramTruncations);
                        else if (error.SocketErrorCode is SocketError.ConnectionRefused or SocketError.HostUnreachable or SocketError.NetworkUnreachable)
                        { if (Remote != null) NotifyUnreachable(Remote); }
                        else { Interlocked.Increment(ref Path.Host.datagramReceiveErrors); Thread.Sleep(1); }
                    }
                    catch (ObjectDisposedException)
                    { if (packet != null) Path.Return(packet); lock (gate) { if (closing) return; } throw; }
                    catch (OutOfMemoryException)
                    { if (packet != null) FatalInvariant("Unable to retain an asynchronous receive operation after submission."); Interlocked.Increment(ref Path.Host.datagramReceiveErrors); Thread.Sleep(1); }
                }
            }
            catch (Exception error) { FatalInvariant("UDP receive pump failed: " + error); }
        }
        internal void NotifyUnreachable(IPEndPoint remote)
        {
            lock (gate)
            {
                if (closing) return;
                if (unreachable.Count == 64) return; // Repeated network-error hints may coalesce; packets never do.
                unreachable.Enqueue(remote);
                if (PlatformQueueEnqueue(Path.Host.ContextPointer, queue, &notification->Sqe) == 0) FatalInvariant("Unreachable notification rejected.");
            }
        }
        internal void Dispatch()
        {
            while (true)
            {
                DatagramReceive? packet = null; IPEndPoint? remote = null;
                lock (gate)
                {
                    if (closing) return;
                    if (received.Count != 0) packet = received.Dequeue();
                    else if (unreachable.Count != 0) remote = unreachable.Dequeue();
                    else return;
                    callbackThread = Environment.CurrentManagedThreadId;
                }
                try
                {
                    if (packet != null) Path.Callbacks.Receive((CXPLAT_SOCKET*)Token, CallbackContext, packet.Data);
                    else { QUIC_ADDR address; DatagramAddress(remote!, &address); Path.Callbacks.Unreachable((CXPLAT_SOCKET*)Token, CallbackContext, &address); }
                }
                finally { lock (gate) { callbackThread = 0; Monitor.PulseAll(gate); } }
            }
        }
        internal void BeginSend() { lock (gate) { if (closing) throw new ObjectDisposedException("socket"); activeSends++; } }
        internal void EndSend() { lock (gate) { if (--activeSends < 0) FatalInvariant("Negative pending send count."); Monitor.PulseAll(gate); } }
        public void Dispose() => Close(false);
        internal void Close(bool releaseToken)
        {
            DatagramPhysical[] entries;
            lock (gate)
            {
                if (disposed) return;
                if (callbackThread == Environment.CurrentManagedThreadId) FatalInvariant("Socket deletion must be deferred outside its callback.");
                if (closing) { while (!disposed) Monitor.Wait(gate); return; }
                closing = true; entries = physical.ToArray();
            }
            if (counted) Path.BeginSocketClose();
            foreach (var entry in entries) entry.Socket.Dispose();
            foreach (var entry in entries) if (entry.Started) entry.Thread.Join();
            lock (gate) { while (activeSends != 0 || callbackThread != 0) Monitor.Wait(gate); }
            lock (gate)
            {
                while (received.Count != 0) ReturnDatagram(Path.Host, received.Dequeue().Data);
                unreachable.Clear();
            }
            if (registered) PlatformSqeCleanupDeferred(Path.Host.ContextPointer, queue, &notification->Sqe, () => FinishClose(releaseToken));
            else FinishClose(releaseToken);
        }
        private void FinishClose(bool releaseToken)
        {
            if (notification != null) Path.Host.FreePlatformMemory(notification, DatagramAllocationTag);
            if (counted) Path.RemoveSocket();
            lock (gate) { disposed = true; Monitor.PulseAll(gate); }
            if (releaseToken) Path.Host.ReleaseResource<DatagramSocket>(Token);
        }
    }

    private static void DatagramDispatch(CXPLAT_CQE* completion)
    {
        try
        {
            var notification = (DatagramNotification*)completion->Sqe;
            var host = FromContext(notification->Context);
            host.Resource<DatagramSocket>(notification->Socket).Dispatch();
        }
        catch (Exception error) { FatalInvariant("UDP worker completion failed: " + error); }
    }
    private static void ReturnDatagram(MsQuicHost host, CXPLAT_RECV_DATA* packet)
    {
        if (!host.datagramReceives.TryRemove((nint)packet, out var owner)) FatalInvariant("Unknown or already returned receive buffer.");
        owner.Path.Return(owner);
    }
    private static void DatapathReceiveReturn(void* context, CXPLAT_RECV_DATA* chain)
    {
        try
        {
            var host = FromContext(context);
            while (chain != null) { var next = chain->Next; ReturnDatagram(host, chain); chain = next; }
        }
        catch (Exception error) { FatalInvariant(error.ToString()); }
    }
}

// Memory is backed by the platform's pinned allocation registry. The receive
// owner stays rented until the Socket operation completes, including cancellation.
internal sealed unsafe class DatagramMemory(nint address, int length) : MemoryManager<byte>
{
    public override Span<byte> GetSpan() => new((void*)address, length);
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if ((uint)elementIndex > (uint)length) throw new ArgumentOutOfRangeException(nameof(elementIndex));
        return new MemoryHandle((byte*)address + elementIndex);
    }
    public override void Unpin() { }
    protected override void Dispose(bool disposing) { }
}
