using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static int blink_host_epoll_create1(int flags)
    {
        try { return io == null ? IoError(19) : (int)IoResult(io.CreateEpoll(flags)); }
        catch (Exception error) { return IoException(error); }
    }

    public static unsafe int blink_host_epoll_ctl(int epfd, int operation, int descriptor,
        blink_host_epoll_event* value)
    {
        try
        {
            if (io == null) return IoError(19);
            if (operation is 1 or 3 && value == null) return IoError(14);
            uint events = operation is 1 or 3 ? value->events : 0;
            ulong data = operation is 1 or 3 ? value->data.u64 : 0;
            return (int)IoResult(io.ControlEpoll(epfd, operation, descriptor, events, data));
        }
        catch (Exception error) { return IoException(error); }
    }

    public static unsafe int blink_host_epoll_pwait(int epfd, blink_host_epoll_event* events,
        int maxEvents, int timeoutMilliseconds, blink_host_sigset* mask)
    {
        if (io == null) return IoError(19);
        if (maxEvents is < 1 or > 1024) return IoError(22);
        if (events == null) return IoError(14);
        blink_host_sigset previous = default;
        bool restore = false;
        try
        {
            // C TLS must be changed on this calling worker, not an async continuation.
            if (mask != null)
            {
                if (blink_host_sigprocmask(2, mask, &previous) != 0) return -1;
                restore = true;
            }
            using var wake = BeginIoOperation();
            var result = io.WaitEpollEventsAsync(epfd, maxEvents, timeoutMilliseconds, wake?.Token ?? ioCancellation)
                .GetAwaiter().GetResult();
            if (!result.Succeeded)
                // Preserve epoll's established EINTR contract for either cancellation source.
                return IoError(result.Error == GuestError.Canceled ? 4 : (int)result.Error);
            if (result.Value.Length > maxEvents)
                throw new InvalidOperationException("Host returned too many epoll events.");
            for (int index = 0; index < result.Value.Length; ++index)
            {
                events[index].events = result.Value[index].Events;
                events[index].data.u64 = result.Value[index].Data;
            }
            // Upstream converts this private 16-byte ABI to guest 12-byte records.
            return result.Value.Length;
        }
        catch (OperationCanceledException) { return IoError(4); }
        catch (Exception error) { return IoException(error); }
        finally
        {
            int error = Libc.errno;
            if (restore) blink_host_sigprocmask(2, &previous, null);
            Libc.errno = error;
        }
    }
}
