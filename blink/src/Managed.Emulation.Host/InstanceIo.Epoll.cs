namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    // Shared open-description identity: dup never copies an interest set or its
    // lifetime. No registrations are supported by this bounded empty-set profile.
    private sealed class EpollState
    {
        internal readonly TaskCompletionSource Closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
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
                var description = new Description(Kind.Epoll);
                descriptors.Add(fd, description);
                if (flags != 0) closeOnExec.Add(fd);
                return HostResult<int>.Success(fd);
            }
            catch (OutOfMemoryException) { return Fail<int>(GuestError.NoMemory); }
        }
    }

    /// <summary>Registrations are explicitly unsupported; this call never adds,
    /// modifies or removes an interest and never fabricates successful control.</summary>
    public HostResult<int> ControlEpoll(int epfd, int operation, int descriptor, uint events, ulong data)
    {
        lock (sync)
        {
            if (!Find(epfd, out var epoll) || !Find(descriptor, out var target))
                return Fail<int>(GuestError.BadDescriptor);
            if (epoll.Kind != Kind.Epoll || ReferenceEquals(epoll, target) || operation is < 1 or > 3)
                return Fail<int>(GuestError.Invalid);
            return Fail<int>(GuestError.Unsupported);
        }
    }

    /// <summary>Waits on a genuinely empty interest set. A timeout returns zero;
    /// caller cancellation, final description close or owner shutdown returns
    /// Canceled. Close wakeup is a private-owner lifecycle rule, not a claim about
    /// Linux concurrent close semantics. Negative timeouts wait indefinitely.
    /// This profile bounds maxEvents to 1024 and concurrent pending waits to the
    /// instance descriptor limit (Again on admission refusal). It never writes
    /// an event.</summary>
    public async Task<HostResult<int>> WaitEpollAsync(int epfd, int maxEvents, int timeoutMilliseconds,
        CancellationToken cancellation = default)
    {
        EpollState epoll;
        TaskCompletionSource completion;
        lock (sync)
        {
            if (!Find(epfd, out var description)) return Fail<int>(GuestError.BadDescriptor);
            if (description.Kind != Kind.Epoll || maxEvents is < 1 or > 1024)
                return Fail<int>(GuestError.Invalid);
            if (cancellation.IsCancellationRequested) return Fail<int>(GuestError.Canceled);
            if (timeoutMilliseconds == 0) return HostResult<int>.Success(0);
            if (pendingEpollOperations >= descriptorLimit) return Fail<int>(GuestError.Again);
            epoll = description.Epoll!;
            try
            {
                completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                pending.Add(completion.Task);
            }
            catch (OutOfMemoryException) { return Fail<int>(GuestError.NoMemory); }
            ++pendingEpollOperations;
        }
        try
        {
            var timeout = timeoutMilliseconds < 0 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeoutMilliseconds);
            await epoll.Closed.Task.WaitAsync(timeout, cancellation).ConfigureAwait(false);
            return Fail<int>(GuestError.Canceled);
        }
        catch (TimeoutException)
        {
            lock (sync)
                return disposed || epoll.Closed.Task.IsCompleted || cancellation.IsCancellationRequested
                    ? Fail<int>(GuestError.Canceled) : HostResult<int>.Success(0);
        }
        catch (OperationCanceledException) { return Fail<int>(GuestError.Canceled); }
        catch (OutOfMemoryException) { return Fail<int>(GuestError.NoMemory); }
        finally
        {
            lock (sync) --pendingEpollOperations;
            // Keep the drain task registered until it is complete. Owner
            // disposal can therefore never miss an unfinished drain signal.
            completion.SetResult();
            lock (sync) pending.Remove(completion.Task);
        }
    }

    private static HostResult<int> CloseEpoll(EpollState epoll)
    {
        epoll.Closed.TrySetResult();
        return HostResult<int>.Success(0);
    }
}
