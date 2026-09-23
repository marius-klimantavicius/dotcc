using System.Diagnostics;

namespace Managed.Emulation.Host;

public readonly record struct EpollEvent(uint Events, ulong Data);

public sealed partial class InstanceIo
{
    // Each registration retains both the original descriptor number and its
    // open-description identity. Holding an interest is not a descriptor ref.
    private sealed class EpollInterest(int fd, Description target, uint events, ulong data)
    {
        internal readonly int Descriptor = fd;
        internal readonly Description Target = target;
        internal readonly uint Events = events;
        internal readonly ulong Data = data;
        internal ulong? DeliveredRead, DeliveredWrite;
        internal uint DeliveredTerminal;
        internal ulong DeliveredTerminalEpoch;
    }
    private sealed class EpollState
    {
        internal readonly TaskCompletionSource Closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly List<EpollInterest> Interests = [];
        internal int Cursor;
    }

    private int pendingEpollOperations;
    public int PendingEpollOperations { get { lock (sync) return pendingEpollOperations; } }

    public HostResult<int> CreateEpoll(int flags)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if ((flags & ~0x80000) != 0) return Fail<int>(GuestError.Invalid);
            int fd = Allocate();
            if (fd < 0) return Fail<int>(GuestError.TooManyFiles);
            try
            {
                descriptors.EnsureCapacity(descriptors.Count + 1);
                if (flags != 0) closeOnExec.EnsureCapacity(closeOnExec.Count + 1);
                descriptors.Add(fd, new Description(Kind.Epoll));
                if (flags != 0) closeOnExec.Add(fd);
                return HostResult<int>.Success(fd);
            }
            catch (OutOfMemoryException) { return Fail<int>(GuestError.NoMemory); }
        }
    }

    /// <summary>Socket ADD/DEL for IN/OUT and the finite drain-to-EAGAIN ET
    /// profile. MOD, one-shot, nested epoll and other object types remain explicit
    /// limitations. An interest is identified by (fd, open description).</summary>
    public HostResult<int> ControlEpoll(int epfd, int operation, int descriptor, uint events, ulong data)
    {
        lock (sync)
        {
            if (!Find(epfd, out var epoll) || !Find(descriptor, out var target))
                return Fail<int>(GuestError.BadDescriptor);
            if (epoll.Kind != Kind.Epoll || ReferenceEquals(epoll, target) || operation is < 1 or > 3)
                return Fail<int>(GuestError.Invalid);
            if (target.Kind != Kind.Socket || operation == 3) return Fail<int>(GuestError.Unsupported);
            var state = epoll.Epoll!;
            int index = state.Interests.FindIndex(i => i.Descriptor == descriptor && ReferenceEquals(i.Target, target));
            if (operation == 2)
            {
                if (index < 0) return Fail<int>(GuestError.NoEntry);
                state.Interests.RemoveAt(index);
                return HostResult<int>.Success(0);
            }
            if (index >= 0) return Fail<int>(GuestError.Exists);
            if ((events & ~(0x80000000u | 1u | 4u | 8u | 16u)) != 0) return Fail<int>(GuestError.Unsupported);
            if (state.Interests.Count >= descriptorLimit) return Fail<int>(GuestError.NoSpace);
            try { state.Interests.Add(new(descriptor, target, events, data)); }
            catch (OutOfMemoryException) { return Fail<int>(GuestError.NoMemory); }
            return HostResult<int>.Success(0);
        }
    }

    // Caller holds InstanceIo.sync, then takes network.sync. No network callback
    // takes the reverse lock order. EAGAIN updates and snapshots share network.sync.
    private uint AvailableEpollEvents(EpollInterest interest, out SocketEdgeReadiness ready)
    {
        ready = default;
        if (interest.Target.References == 0) return 0;
        var result = network.EdgeReadiness(interest.Target.Handle);
        if (!result.Succeeded) return 0;
        ready = result.Value;
        uint events = ready.Events & (interest.Events | 8u | 16u);
        if ((interest.Events & 0x80000000u) == 0) return events;
        // A HUP delivered before connect must not suppress a later connected
        // shutdown, even if no readiness snapshot observed the interim state.
        if (interest.DeliveredTerminalEpoch != ready.TerminalEpoch)
        {
            interest.DeliveredTerminal = 0;
            interest.DeliveredTerminalEpoch = ready.TerminalEpoch;
        }
        if (interest.DeliveredRead == ready.ReadEpoch) events &= ~1u;
        if (interest.DeliveredWrite == ready.WriteEpoch) events &= ~4u;
        return events & ~interest.DeliveredTerminal;
    }

    private EpollEvent[] CollectEpollEvents(EpollState state, int maximum)
    {
        var result = new List<EpollEvent>();
        int count = state.Interests.Count;
        if (count == 0) return [];
        int cursor = state.Cursor % count;
        for (int visited = 0; visited < count; ++visited)
        {
            int index = (cursor + visited) % count;
            var interest = state.Interests[index];
            uint events = AvailableEpollEvents(interest, out var ready);
            if (events == 0) continue;
            result.Add(new(events, interest.Data));
            if ((events & 1) != 0) interest.DeliveredRead = ready.ReadEpoch;
            if ((events & 4) != 0) interest.DeliveredWrite = ready.WriteEpoch;
            interest.DeliveredTerminal |= events & (8u | 16u);
            state.Cursor = (index + 1) % count;
            if (result.Count == maximum) break;
        }
        return result.ToArray();
    }

    private bool EpollReadable(EpollState state)
        => state.Interests.Any(interest => AvailableEpollEvents(interest, out _) != 0);

    private void RemoveEpollDescription(Description target)
    {
        foreach (var description in descriptors.Values)
            description.Epoll?.Interests.RemoveAll(i => ReferenceEquals(i.Target, target));
    }

    // Backward-compatible count API for existing empty-interest consumers.
    public async Task<HostResult<int>> WaitEpollAsync(int epfd, int maxEvents, int timeoutMilliseconds,
        CancellationToken cancellation = default)
    {
        var result = await WaitEpollEventsAsync(epfd, maxEvents, timeoutMilliseconds, cancellation).ConfigureAwait(false);
        return result.Succeeded ? HostResult<int>.Success(result.Value.Length) : Fail<int>(result.Error);
    }

    /// <summary>Bounded registered waits. ET emits real initial readiness and
    /// fresh readiness after the transport observes a drain to EAGAIN. It is not
    /// arbitrary Linux edge notification while unread data remains buffered.
    /// No independent observers or pending socket sends are created.</summary>
    public async Task<HostResult<EpollEvent[]>> WaitEpollEventsAsync(int epfd, int maxEvents, int timeoutMilliseconds,
        CancellationToken cancellation = default)
    {
        EpollState epoll;
        TaskCompletionSource completion;
        lock (sync)
        {
            if (!Find(epfd, out var description)) return Fail<EpollEvent[]>(GuestError.BadDescriptor);
            if (description.Kind != Kind.Epoll || maxEvents is < 1 or > 1024)
                return Fail<EpollEvent[]>(GuestError.Invalid);
            if (cancellation.IsCancellationRequested) return Fail<EpollEvent[]>(GuestError.Canceled);
            if (pendingEpollOperations >= descriptorLimit) return Fail<EpollEvent[]>(GuestError.Again);
            epoll = description.Epoll!;
            try
            {
                completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                pending.Add(completion.Task);
            }
            catch (OutOfMemoryException) { return Fail<EpollEvent[]>(GuestError.NoMemory); }
            ++pendingEpollOperations;
        }
        try
        {
            long start = Stopwatch.GetTimestamp();
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();
                lock (sync)
                {
                    if (disposed || epoll.Closed.Task.IsCompleted) return Fail<EpollEvent[]>(GuestError.Canceled);
                    EpollEvent[] events = CollectEpollEvents(epoll, maxEvents);
                    if (events.Length != 0) return HostResult<EpollEvent[]>.Success(events);
                }
                long elapsed = (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                if (timeoutMilliseconds >= 0 && elapsed >= timeoutMilliseconds)
                {
                    lock (sync)
                        return disposed || epoll.Closed.Task.IsCompleted || cancellation.IsCancellationRequested
                            ? Fail<EpollEvent[]>(GuestError.Canceled) : HostResult<EpollEvent[]>.Success([]);
                }
                int delay = timeoutMilliseconds < 0 ? 5 : (int)Math.Min(5, timeoutMilliseconds - elapsed);
                // WaitAsync observes final epoll close as well as caller cancellation.
                try { await epoll.Closed.Task.WaitAsync(TimeSpan.FromMilliseconds(delay), cancellation).ConfigureAwait(false); }
                catch (TimeoutException) { }
            }
        }
        catch (OperationCanceledException) { return Fail<EpollEvent[]>(GuestError.Canceled); }
        catch (OutOfMemoryException) { return Fail<EpollEvent[]>(GuestError.NoMemory); }
        finally
        {
            lock (sync) --pendingEpollOperations;
            completion.SetResult();
            lock (sync) pending.Remove(completion.Task);
        }
    }

    private static HostResult<int> CloseEpoll(EpollState epoll)
    {
        epoll.Interests.Clear();
        epoll.Closed.TrySetResult();
        return HostResult<int>.Success(0);
    }
}
