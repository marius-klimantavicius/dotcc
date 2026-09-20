using System.Diagnostics;

namespace Managed.Emulation.Host;

public readonly record struct PollRequest(int Descriptor, short Events);
public readonly record struct PollResult(int Count, short[] Events);

public sealed partial class InstanceIo
{
    /// <summary>Bounded poll snapshots. Five-millisecond sleeps avoid spinning;
    /// disposal drains even a poll with no descriptors and infinite timeout.</summary>
    public async Task<HostResult<PollResult>> PollAsync(PollRequest[] requests, int timeoutMilliseconds,
        CancellationToken cancellation = default)
    {
        if (requests.Length > 1024) return Fail<PollResult>(GuestError.Invalid);
        var copy = (PollRequest[])requests.Clone();
        foreach (var request in copy)
            if (request.Descriptor >= 0 && (request.Events & ~(1 | 4 | 64 | 256 | 8 | 16 | 32)) != 0)
                return Fail<PollResult>(GuestError.Unsupported);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (sync)
        {
            if (disposed) return Fail<PollResult>(GuestError.BadDescriptor);
            pending.Add(completion.Task);
        }
        try
        {
            long start = Stopwatch.GetTimestamp();
            short[] events = new short[copy.Length];
            while (true)
            {
                if (cancellation.IsCancellationRequested) return Fail<PollResult>(GuestError.Canceled);
                Array.Clear(events);
                int count = 0;
                lock (sync)
                {
                    if (disposed) return Fail<PollResult>(GuestError.Canceled);
                    for (int i = 0; i < copy.Length; ++i)
                    {
                        var request = copy[i];
                        if (request.Descriptor < 0) continue;
                        if (!Find(request.Descriptor, out var description)) events[i] = 32;
                        else if (description.Kind == Kind.Socket)
                        {
                            var ready = network.Readiness(description.Handle, request.Events);
                            events[i] = ready.Succeeded ? ready.Value : (short)(ready.Error == GuestError.BadDescriptor ? 32 : 8);
                        }
                        else if (description.Kind == Kind.Pipe)
                        {
                            var ready = pipes.Readiness(description.Handle, request.Events);
                            events[i] = ready.Succeeded ? ready.Value : (short)32;
                        }
                        else if (description.Kind == Kind.Epoll) events[i] = EpollReadable(description.Epoll!) ? (short)(request.Events & 1) : (short)0;
                        else events[i] = (short)(request.Events & (1 | 4 | 64 | 256));
                        if (events[i] != 0) ++count;
                    }
                }
                if (count != 0) return HostResult<PollResult>.Success(new(count, events));
                long elapsed = (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                if (timeoutMilliseconds >= 0 && elapsed >= timeoutMilliseconds)
                    return HostResult<PollResult>.Success(new(0, events));
                int delay = timeoutMilliseconds < 0 ? 5 : (int)Math.Min(5, timeoutMilliseconds - elapsed);
                await Task.Delay(delay, cancellation).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { return Fail<PollResult>(GuestError.Canceled); }
        catch (OutOfMemoryException) { return Fail<PollResult>(GuestError.NoMemory); }
        finally
        {
            lock (sync) pending.Remove(completion.Task);
            completion.SetResult();
        }
    }
}
