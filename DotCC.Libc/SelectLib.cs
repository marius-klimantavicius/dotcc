#nullable enable
using System;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Linux LP64 fd_set is 1024 bits, independent of the number of open fds.
    public static void FD_ZERO(void* set)
    {
        if (set != null) new Span<byte>(set, 128).Clear();
    }
    public static void FD_SET(int fd, void* set)
    {
        if (set != null && (uint)fd < 1024) ((ulong*)set)[fd / 64] |= 1UL << (fd % 64);
    }
    public static void FD_CLR(int fd, void* set)
    {
        if (set != null && (uint)fd < 1024) ((ulong*)set)[fd / 64] &= ~(1UL << (fd % 64));
    }
    public static int FD_ISSET(int fd, void* set) =>
        set != null && (uint)fd < 1024 && (((ulong*)set)[fd / 64] & (1UL << (fd % 64))) != 0 ? 1 : 0;

    private struct SelectPollFd { internal int Fd; internal short Events, Revents; }

    /// <summary>Select managed file, socket and pipe descriptors. The result
    /// counts ready bits across all sets; timeout is the Linux LP64 timeval and
    /// is reduced by elapsed monotonic time. Submillisecond waits round upward
    /// to the runtime scheduler's millisecond granularity.</summary>
    public static int select(int nfds, void* readfds, void* writefds, void* errorfds, void* timeout)
    {
        if ((uint)nfds > 1024) return FcntlError(EINVAL);
        long microseconds = -1;
        long originalSeconds = 0, originalMicros = 0;
        if (timeout != null)
        {
            long seconds = ((long*)timeout)[0], micros = ((long*)timeout)[1];
            if (seconds < 0 || micros < 0 || micros >= 1000000) return FcntlError(EINVAL);
            originalSeconds = seconds; originalMicros = micros;
            microseconds = seconds > (long.MaxValue - micros) / 1000000
                ? long.MaxValue : seconds * 1000000 + micros;
        }
        SelectPollFd* checks = stackalloc SelectPollFd[nfds];
        int count = 0;
        for (int fd = 0; fd < nfds; fd++)
        {
            short events = (short)(FD_ISSET(fd, readfds) | (FD_ISSET(fd, writefds) << 2) |
                (FD_ISSET(fd, errorfds) << 1));
            if (events != 0) checks[count++] = new() { Fd = fd, Events = events, Revents = 0 };
        }
        var elapsed = global::System.Diagnostics.Stopwatch.StartNew();
        long Remaining() => microseconds < 0 ? -1 : Math.Max(0, microseconds - (long)elapsed.Elapsed.TotalMicroseconds);
        while (true)
        {
            long remaining = Remaining();
            int millis = remaining < 0 ? -1 : (int)Math.Min(int.MaxValue, remaining / 1000 + (remaining % 1000 != 0 ? 1 : 0));
            int polled = poll(checks, (ulong)count, millis);
            if (polled < 0) return -1;
            int ready = 0;
            for (int i = 0; i < count; i++)
            {
                if ((checks[i].Revents & 32) != 0) return FcntlError(EBADF);
                short result = checks[i].Revents, wanted = checks[i].Events;
                if ((wanted & 1) != 0 && (result & (1 | 8 | 16)) != 0) ready++;
                if ((wanted & 4) != 0 && (result & (4 | 8)) != 0) ready++;
                if ((wanted & 2) != 0 && (result & 2) != 0) ready++;
            }
            if (ready != 0 || Remaining() == 0)
            {
                FD_ZERO(readfds); FD_ZERO(writefds); FD_ZERO(errorfds);
                for (int i = 0; i < count; i++)
                {
                    int fd = checks[i].Fd;
                    short result = checks[i].Revents, wanted = checks[i].Events;
                    if ((wanted & 1) != 0 && (result & (1 | 8 | 16)) != 0) FD_SET(fd, readfds);
                    if ((wanted & 4) != 0 && (result & (4 | 8)) != 0) FD_SET(fd, writefds);
                    if ((wanted & 2) != 0 && (result & 2) != 0) FD_SET(fd, errorfds);
                }
                if (timeout != null)
                {
                    long used = (long)elapsed.Elapsed.TotalMicroseconds;
                    long seconds = originalSeconds - used / 1000000;
                    long micros = originalMicros - used % 1000000;
                    if (micros < 0) { seconds--; micros += 1000000; }
                    ((long*)timeout)[0] = seconds < 0 ? 0 : seconds;
                    ((long*)timeout)[1] = seconds < 0 ? 0 : micros;
                }
                return ready;
            }
            // HUP/ERR are unconditional poll results, but aren't exceptional
            // data for select. A permanently hung-up exception-only descriptor
            // cannot gain urgent data; ignore it while waiting on the remainder.
            for (int i = 0; i < count; i++)
                if ((checks[i].Revents & (8 | 16)) != 0) checks[i].Fd = -1;
        }
    }
}
