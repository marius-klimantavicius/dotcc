using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static Managed.Smb.LibSmb2;
using C = Managed.Smb.LibSmb2.Libc;

namespace Managed.Smb;

/// <summary>Completion-driven, bounded socket buffers for the translated callback client.</summary>
public static partial class HostSockets
{
    public const int BufferCapacity = 256 * 1024;
    private static readonly object RegistryGate = new();
    private static readonly Dictionary<int, Entry> Sockets = new();
    private static readonly Dictionary<nint, Context> Contexts = new();
    private static int _lastToken;
    private static long _completionCount, _notificationCount;
    private static int _peakReceiveBytes, _peakSendBytes;
    [ThreadStatic] private static Context? _current;
    [ThreadStatic] private static Prepared? _prepared;

    private sealed class Context(nint address, Action<t_socket> notify)
    {
        public readonly nint Address = address;
        public readonly Action<t_socket> Notify = notify;
        public readonly HashSet<Entry> Entries = new();
        public bool Closing;
    }
    private sealed class Entry(int token, Context owner, Socket socket)
    {
        public readonly object Gate = new();
        public readonly int Token = token;
        public readonly Context Owner = owner;
        public readonly Socket Socket = socket;
        public readonly byte[] Receive = new byte[BufferCapacity];
        public readonly Queue<byte[]> Send = new();
        public readonly CancellationTokenSource Stop = new();
        public int ReceiveOffset, ReceiveCount, SendBytes, Interest, Error, Flags;
        public bool Attached, Closed, Connected, Connecting, ConnectReady, WriteReady, Eof, Notified;
        public bool Receiving, Sending;
        public Task ConnectTask = Task.CompletedTask, ReceiveTask = Task.CompletedTask, SendTask = Task.CompletedTask;
    }
    private sealed record Prepared(string Hostname, IPAddress[] Addresses);
    private sealed class Scope(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;
        public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }

    public static void RegisterContext(nint context, Action<t_socket> notify)
    {
        ArgumentNullException.ThrowIfNull(notify);
        if (context == 0) throw new ArgumentOutOfRangeException(nameof(context));
        lock (RegistryGate) Contexts.Add(context, new(context, notify));
    }
    /// <summary>Scope must remain on one thread and must not cross an await.</summary>
    public static IDisposable EnterContext(nint context)
    {
        Context value;
        lock (RegistryGate)
        {
            if (!Contexts.TryGetValue(context, out value!) || value.Closing)
                throw new ObjectDisposedException(nameof(context));
        }
        var prior = _current;
        _current = value;
        return new Scope(() => _current = prior);
    }
    public static IDisposable PreparedAddresses(string hostname, IPAddress[] addresses)
    {
        ArgumentNullException.ThrowIfNull(hostname);
        ArgumentNullException.ThrowIfNull(addresses);
        var prior = _prepared;
        _prepared = new(hostname, addresses.ToArray());
        return new Scope(() => _prepared = prior);
    }
    private static Entry? Find(t_socket handle)
    {
        lock (RegistryGate)
        {
            if (Sockets.TryGetValue((int)handle, out var entry) && entry.Owner == _current && !entry.Owner.Closing)
                return entry;
        }
        C.errno = C.EBADF;
        return null;
    }
    private static Entry? Find(nint context, t_socket handle)
    {
        lock (RegistryGate)
            return Sockets.TryGetValue((int)handle, out var entry) && entry.Owner.Address == context && !entry.Owner.Closing ? entry : null;
    }
    public static void Add(nint context, t_socket handle)
    {
        var entry = Find(context, handle);
        if (entry is null) return;
        lock (entry.Gate) { entry.Attached = true; Notify(entry); }
    }
    public static void Remove(nint context, t_socket handle)
    {
        var entry = Find(context, handle);
        if (entry is null) return;
        lock (entry.Gate) { entry.Attached = false; entry.Interest = 0; }
    }
    public static void SetEvents(nint context, t_socket handle, int events)
    {
        var entry = Find(context, handle);
        if (entry is null) return;
        lock (entry.Gate)
        {
            if ((events & POLLOUT) != 0 && (entry.Interest & POLLOUT) == 0 && entry.SendBytes < BufferCapacity)
                entry.WriteReady = true;
            entry.Interest = events;
            Notify(entry);
        }
    }
    public static int Events(nint context, t_socket handle)
    {
        var entry = Find(context, handle);
        if (entry is null) return 0;
        lock (entry.Gate)
        {
            int result = Ready(entry);
            if (entry.ConnectReady) entry.ConnectReady = false;
            if ((result & POLLOUT) != 0) entry.WriteReady = false;
            return result;
        }
    }
    public static void Rearm(nint context, t_socket handle)
    {
        var entry = Find(context, handle);
        if (entry is null) return;
        lock (entry.Gate) Notify(entry);
    }
    private static int Ready(Entry entry)
    {
        if (entry.Closed || !entry.Attached) return 0;
        if (entry.ConnectReady) return POLLOUT;
        int result = 0;
        if ((entry.Interest & POLLIN) != 0 && (entry.ReceiveCount > 0 || entry.Eof)) result |= POLLIN;
        // Deliver accepted receive data before exposing an error from a later completion.
        if (entry.Error != 0 && entry.ReceiveCount == 0) result |= POLLERR;
        if (entry.Connected && entry.WriteReady && (entry.Interest & POLLOUT) != 0 && entry.SendBytes < BufferCapacity)
            result |= POLLOUT;
        return result;
    }
    private static void Notify(Entry entry)
    {
        if (entry.Notified || Ready(entry) == 0) return;
        entry.Notified = true;
        Interlocked.Increment(ref _notificationCount);
        ThreadPool.UnsafeQueueUserWorkItem(static state =>
        {
            var e = (Entry)state!;
            bool deliver;
            lock (e.Gate) { e.Notified = false; deliver = Ready(e) != 0; }
            lock (RegistryGate) deliver &= !e.Owner.Closing;
            if (deliver) e.Owner.Notify(e.Token);
        }, entry);
    }
    public static t_socket Create(int family, int type, int protocol)
    {
        Context? owner = _current;
        if (owner is null) { C.errno = C.EBADF; return -1; }
        if (type != SOCK_STREAM || (protocol != 0 && protocol != IPPROTO_TCP)) { C.errno = C.EPROTONOSUPPORT; return -1; }
        var af = family == AF_INET ? AddressFamily.InterNetwork : family == AF_INET6 ? AddressFamily.InterNetworkV6 : AddressFamily.Unspecified;
        if (af == AddressFamily.Unspecified) { C.errno = C.EAFNOSUPPORT; return -1; }
        Socket? socket = null;
        try
        {
            socket = new Socket(af, SocketType.Stream, ProtocolType.Tcp);
            lock (RegistryGate)
            {
                if (owner.Closing || _lastToken == int.MaxValue) { socket.Dispose(); C.errno = C.EMFILE; return -1; }
                var entry = new Entry(++_lastToken, owner, socket);
                Sockets.Add(entry.Token, entry);
                owner.Entries.Add(entry);
                return entry.Token;
            }
        }
        catch (Exception ex) when (ex is SocketException or OutOfMemoryException or ArgumentException)
        { socket?.Dispose(); C.errno = ErrorCode(ex); return -1; }
    }
    public static int Connect(t_socket handle, IPEndPoint endpoint)
    {
        var entry = Find(handle);
        if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Closed) { C.errno = C.EBADF; return -1; }
            if (entry.Connecting || entry.Connected) { C.errno = entry.Connected ? C.EISCONN : C.EINPROGRESS; return -1; }
            entry.Connecting = true;
            entry.ConnectTask = ConnectCore(entry, endpoint);
        }
        C.errno = C.EINPROGRESS;
        return -1;
    }
    private static async Task ConnectCore(Entry entry, IPEndPoint endpoint)
    {
        int error = 0;
        try { await entry.Socket.ConnectAsync(endpoint, entry.Stop.Token).ConfigureAwait(false); }
        catch (Exception ex) when (IsIoException(ex)) { error = ErrorCode(ex); }
        lock (entry.Gate)
        {
            if (entry.Closed) return;
            Interlocked.Increment(ref _completionCount);
            entry.Connecting = false;
            entry.Connected = error == 0;
            entry.Error = error;
            entry.ConnectReady = true;
            if (entry.Connected) StartReceive(entry);
            Notify(entry);
        }
    }
    private static void StartReceive(Entry entry)
    {
        if (!entry.Receiving && !entry.Closed && entry.Connected && !entry.Eof && entry.Error == 0 && entry.ReceiveCount < BufferCapacity)
        { entry.Receiving = true; entry.ReceiveTask = ReceiveCore(entry); }
    }
    private static async Task ReceiveCore(Entry entry)
    {
        var scratch = new byte[64 * 1024];
        while (true)
        {
            int capacity;
            lock (entry.Gate)
            {
                capacity = Math.Min(scratch.Length, BufferCapacity - entry.ReceiveCount);
                if (entry.Closed || capacity == 0) { entry.Receiving = false; return; }
            }
            int count = 0, error = 0;
            try { count = await entry.Socket.ReceiveAsync(scratch.AsMemory(0, capacity), SocketFlags.None, entry.Stop.Token).ConfigureAwait(false); }
            catch (Exception ex) when (IsIoException(ex)) { error = ErrorCode(ex); }
            lock (entry.Gate)
            {
                if (entry.Closed) { entry.Receiving = false; return; }
                Interlocked.Increment(ref _completionCount);
                if (error != 0) entry.Error = error;
                else if (count == 0) entry.Eof = true;
                else
                {
                    if (entry.ReceiveOffset + entry.ReceiveCount + count > BufferCapacity)
                    { Buffer.BlockCopy(entry.Receive, entry.ReceiveOffset, entry.Receive, 0, entry.ReceiveCount); entry.ReceiveOffset = 0; }
                    Buffer.BlockCopy(scratch, 0, entry.Receive, entry.ReceiveOffset + entry.ReceiveCount, count);
                    entry.ReceiveCount += count;
                    RecordPeak(ref _peakReceiveBytes, entry.ReceiveCount);
                }
                Notify(entry);
                if (entry.Eof || error != 0) { entry.Receiving = false; return; }
            }
        }
    }
    public static int Read(t_socket handle, Span<byte> destination)
    {
        var entry = Find(handle);
        if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Closed) { C.errno = C.EBADF; return -1; }
            if (destination.Length == 0) return 0;
            int count = Math.Min(destination.Length, entry.ReceiveCount);
            if (count > 0)
            {
                entry.Receive.AsSpan(entry.ReceiveOffset, count).CopyTo(destination);
                entry.ReceiveOffset += count; entry.ReceiveCount -= count;
                if (entry.ReceiveCount == 0) entry.ReceiveOffset = 0;
                StartReceive(entry);
                return count;
            }
            if (entry.Error != 0) { C.errno = entry.Error; return -1; }
            if (entry.Eof) return 0;
            C.errno = C.EAGAIN; return -1;
        }
    }
    public static int Write(t_socket handle, ReadOnlySpan<byte> source)
    {
        var entry = Find(handle);
        if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Closed) { C.errno = C.EBADF; return -1; }
            if (source.Length == 0) return 0;
            if (entry.Error != 0) { C.errno = entry.Error; return -1; }
            if (!entry.Connected) { C.errno = C.ENOTCONN; return -1; }
            int count = Math.Min(source.Length, BufferCapacity - entry.SendBytes);
            if (count == 0) { C.errno = C.EAGAIN; return -1; }
            entry.Send.Enqueue(source[..count].ToArray());
            entry.SendBytes += count;
            RecordPeak(ref _peakSendBytes, entry.SendBytes);
            if (!entry.Sending) { entry.Sending = true; entry.SendTask = SendCore(entry); }
            return count;
        }
    }
    private static async Task SendCore(Entry entry)
    {
        while (true)
        {
            byte[] bytes;
            lock (entry.Gate)
            {
                if (entry.Closed || entry.Send.Count == 0) { entry.Sending = false; return; }
                bytes = entry.Send.Peek();
            }
            int error = 0;
            try
            {
                int offset = 0;
                while (offset < bytes.Length)
                {
                    int count = await entry.Socket.SendAsync(bytes.AsMemory(offset), SocketFlags.None, entry.Stop.Token).ConfigureAwait(false);
                    Interlocked.Increment(ref _completionCount);
                    if (count == 0) { error = C.EPIPE; break; }
                    offset += count;
                }
            }
            catch (Exception ex) when (IsIoException(ex)) { error = ErrorCode(ex); }
            lock (entry.Gate)
            {
                if (entry.Closed) { entry.Sending = false; return; }
                bool wasFull = entry.SendBytes == BufferCapacity;
                entry.Send.Dequeue(); entry.SendBytes -= bytes.Length;
                if (error != 0) { entry.Error = error; entry.Sending = false; Notify(entry); return; }
                if (wasFull) entry.WriteReady = true;
                Notify(entry);
            }
        }
    }
    public static int Close(t_socket handle)
    {
        var entry = Find(handle);
        if (entry is null) return -1;
        Close(entry);
        return 0;
    }
    private static void Close(Entry entry)
    {
        lock (entry.Gate)
        {
            if (entry.Closed) return;
            entry.Closed = true; entry.Attached = false;
            entry.Stop.Cancel(); entry.Socket.Dispose();
            entry.Send.Clear(); entry.SendBytes = 0; entry.ReceiveCount = 0;
        }
        lock (RegistryGate) Sockets.Remove(entry.Token);
    }
    public static async Task DrainContextAsync(nint context)
    {
        Context owner;
        Entry[] entries;
        lock (RegistryGate)
        {
            if (!Contexts.TryGetValue(context, out owner!)) return;
            owner.Closing = true;
            Contexts.Remove(context);
            entries = owner.Entries.ToArray();
        }
        foreach (var entry in entries) Close(entry);
        foreach (var entry in entries)
        {
            Task[] tasks;
            lock (entry.Gate) tasks = [entry.ConnectTask, entry.ReceiveTask, entry.SendTask];
            await Task.WhenAll(tasks).ConfigureAwait(false);
            entry.Stop.Dispose();
        }
        lock (RegistryGate)
        {
            owner.Entries.Clear();
            if (Contexts.TryGetValue(context, out var registered) && registered == owner) Contexts.Remove(context);
        }
    }
    /// <summary>Cumulative counters are observational; reading them never schedules I/O.</summary>
    public readonly record struct Diagnostics(int RegisteredContexts, int RegisteredSockets,
        long BufferedReceiveBytes, long BufferedSendBytes, long IoCompletions, long ServiceNotifications,
        int PeakSocketReceiveBytes, int PeakSocketSendBytes);
    public static Diagnostics Snapshot()
    {
        Entry[] entries;
        int contexts;
        lock (RegistryGate) { entries = Sockets.Values.ToArray(); contexts = Contexts.Count; }
        long receive = 0, send = 0;
        foreach (var entry in entries)
            lock (entry.Gate) { receive += entry.ReceiveCount; send += entry.SendBytes; }
        return new(contexts, entries.Length, receive, send, Interlocked.Read(ref _completionCount),
            Interlocked.Read(ref _notificationCount), Volatile.Read(ref _peakReceiveBytes), Volatile.Read(ref _peakSendBytes));
    }
    private static void RecordPeak(ref int peak, int value)
    {
        int old;
        do { old = Volatile.Read(ref peak); if (value <= old) return; }
        while (Interlocked.CompareExchange(ref peak, value, old) != old);
    }
    private static bool IsIoException(Exception ex) => ex is SocketException or OperationCanceledException or ObjectDisposedException or IOException;
    private static int ErrorCode(Exception ex) => ex switch
    {
        OperationCanceledException => C.ECANCELED, ObjectDisposedException => C.EBADF,
        OutOfMemoryException => C.ENOMEM, ArgumentException => C.EINVAL,
        SocketException s => s.SocketErrorCode switch
        {
            SocketError.ConnectionRefused => C.ECONNREFUSED, SocketError.ConnectionReset => C.ECONNRESET,
            SocketError.ConnectionAborted => C.ECONNABORTED, SocketError.TimedOut => C.ETIMEDOUT,
            SocketError.NetworkUnreachable => C.ENETUNREACH, SocketError.HostUnreachable => C.EHOSTUNREACH,
            SocketError.AddressAlreadyInUse => C.EADDRINUSE, SocketError.AddressNotAvailable => C.EADDRNOTAVAIL,
            SocketError.NotConnected => C.ENOTCONN, SocketError.NoBufferSpaceAvailable => C.ENOBUFS,
            SocketError.AccessDenied => C.EACCES, SocketError.OperationAborted => C.ECANCELED,
            _ => C.EIO
        }, _ => C.EIO
    };
}
